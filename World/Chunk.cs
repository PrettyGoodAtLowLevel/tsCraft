using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.Terrain_Generation;
using OurCraft.Utility;

namespace OurCraft.World
{
    //a Chunk is just a part of the world
    //chunks contain a lightmap, placed structures, list of subchunks (which hold block & mesh data), and a batched openGL mesh
    //the batched mesh allows for low draw calls and split subchunks allow for smaller remesh jobs & block storage
    public class Chunk
    {
        public const int HEIGHT_IN_SUBCHUNKS = WorldConstants.CHUNK_HEIGHT_IN_SUBCHUNKS;
        public const int WIDTH_IN_SUBCHUNKS = WorldConstants.CHUNK_WIDTH_IN_SUBCHUNKS;
        public const int CHUNK_HEIGHT = WorldConstants.CHUNK_HEIGHT;
        public const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;

        //rendering
        public readonly BatchedChunkMesh batchedMesh;
        public readonly BatchedChunkMesh transparentMesh;

        //positioning
        public ChunkCoord ChunkPos { get; private set; }
        public Vector3d ChunkMin { get; private set; }
        public Vector3d ChunkMax { get; private set; }
        public Vector3d RenderWorldPos { get; private set; }

        //block and generation data
        public SubChunk[,,] SubChunks { get; set; } = new SubChunk[0, 0, 0];
        public bool[,,] DirtySubChunks { get; set; } = new bool[0, 0, 0];
        public ushort[,,] lightMap = new ushort[0, 0, 0];
        public List<PlacedFeature> placedFeatures = []; //structure starts
        public int MaxSolidY { get; set; } = CHUNK_HEIGHT - 1; //highest y layer of all air in a chunk

        //state tracking
        volatile ChunkState state;
        public volatile bool generating = false;
        public List<Vector3i> changes = [];

        public Chunk(ChunkCoord coord)
        {
            ChunkPos = coord;
            state = ChunkState.Initialized;
            batchedMesh = new BatchedChunkMesh();
            transparentMesh = new BatchedChunkMesh();

            RenderWorldPos = new Vector3d(ChunkPos.X * CHUNK_WIDTH, 0, ChunkPos.Z * CHUNK_WIDTH);
            ChunkMin = new Vector3d(ChunkPos.X * CHUNK_WIDTH, 0, ChunkPos.Z * CHUNK_WIDTH);
            ChunkMax = ChunkMin + new Vector3d(CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH);
        }

        public void Delete()
        {
            batchedMesh.Delete();
            transparentMesh.Delete();
        }

        public void MarkForDeletion() => state = ChunkState.Deleted;
        public ChunkState GetState() => state;
        public void SetState(ChunkState state) => this.state = state;
        public bool HasBlocks() => state != ChunkState.Deleted && state != ChunkState.Initialized; //terrain is built
        public bool HasAllBlocks() => state != ChunkState.Deleted && state != ChunkState.Initialized && state != ChunkState.Terrain_Set; //structures are placed
        public bool IsMeshing() => generating;
        public bool IsLit() => state == ChunkState.Lit || state == ChunkState.Mesh_Built  || state == ChunkState.Render_Ready;
        public bool Modifiyable() => state == ChunkState.Render_Ready;
        public bool Deleted() => state == ChunkState.Deleted;       

        public BlockState GetBlockSafe(int x, int globalY, int z)
        {
            //fast modulus math
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            if (HasBlocks() == false || globalY < 0 || globalY >= CHUNK_HEIGHT) return Block.AIR;           

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY & sb;

            int subChunkX = x / SubChunk.SUBCHUNK_SIZE;
            int localX = x & sb;

            int subChunkZ = z / SubChunk.SUBCHUNK_SIZE;
            int localZ = z & sb;

            return SubChunks[subChunkX, subChunkY, subChunkZ].GetBlockState(localX, localY, localZ);
        }

        public BlockState GetBlockUnsafe(int x, int globalY, int z)
        {
            //fast modulus math
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            //doesnt check if has all blocks yet
            if (globalY < 0 || globalY >= CHUNK_HEIGHT) return Block.AIR;           

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY & sb;

            int subChunkX = x / SubChunk.SUBCHUNK_SIZE;
            int localX = x & sb;

            int subChunkZ = z / SubChunk.SUBCHUNK_SIZE;
            int localZ = z & sb;

            SubChunk sub = SubChunks[subChunkX, subChunkY, subChunkZ];
            return sub.GetBlockState(localX, localY, localZ);            
        }

        public void SetBlock(int x, int globalY, int z, BlockState state)
        {
            //fast modulus math
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            if (HasAllBlocks() == false || globalY < 0 || globalY > CHUNK_HEIGHT - 1) return;
            changes.Add(new Vector3i(x, globalY, z));

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY & sb;

            int subChunkX = x / SubChunk.SUBCHUNK_SIZE;
            int localX = x & sb;

            int subChunkZ = z / SubChunk.SUBCHUNK_SIZE;
            int localZ = z & sb;

            SubChunks[subChunkX, subChunkY, subChunkZ].SetBlock(localX, localY, localZ, state);
        }

        public void SetBlockUnsafe(int x, int globalY, int z, BlockState state)
        {
            //fast modulus math
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            if (globalY < 0 || globalY > CHUNK_HEIGHT - 1) return;

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY & sb;

            int subChunkX = x / SubChunk.SUBCHUNK_SIZE;
            int localX = x & sb;

            int subChunkZ = z / SubChunk.SUBCHUNK_SIZE;
            int localZ = z & sb;

            SubChunks[subChunkX, subChunkY, subChunkZ].SetBlock(localX, localY, localZ, state);
        }

        public ushort GetLight(int x, int y, int z)
        {            
            if (HasAllBlocks() == false || !PosValid(x, y, z)) return 0;       
            return lightMap[x, y, z];
        }

        public void SetBlockLight(int x, int y, int z, Vector3i value)
        {
            if (HasAllBlocks() == false || !PosValid(x, y, z)) return;

            //preserve the upper 4 bits for skylight
            ushort current = lightMap[x, y, z];    
            ushort preserved = (ushort)(current & 0xF000);

            //pack blocklight into the lower 12 bits
            ushort packed = (ushort)((value.X & 0xF) |
            ((value.Y & 0xF) << 4) | ((value.Z & 0xF) << 8));
            lightMap[x, y, z] = (ushort)(preserved | packed);
        }

        public void SetSkyLight(int x, int y, int z, int value)
        {
            if (HasAllBlocks() == false || !PosValid(x, y, z)) return;

            //read the current light value
            ushort current = lightMap[x, y, z];

            //rreserve the lower 12 bits of block lights
            ushort preservedBlockLight = (ushort)(current & 0x0FFF);
            ushort newSkyLight = (ushort)((value & 0xF) << 12);

            //combine preserved block light with new sky light
            lightMap[x, y, z] = (ushort)(preservedBlockLight | newSkyLight);
        }

        public static bool PosValid(int x, int y, int z)
        {
            return x >= 0 && x < CHUNK_WIDTH
            && z >= 0 && z < CHUNK_WIDTH
            && y >= 0 && y < CHUNK_HEIGHT;
        }
    }
}