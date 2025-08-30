using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Rendering;
using static OurCraft.Rendering.Camera;

namespace OurCraft.World
{
    //represents a chunk position
    public struct ChunkCoord : IEquatable<ChunkCoord>
    {
        //members
        public int X { get; private set; }
        public int Z { get; private set; }

        //methods
        public ChunkCoord(int x, int z)
        {
            X = x;
            Z = z;
        }

        //set position later on
        public void SetPos(int x, int z)
        {
            X = x;
            Z = z;
        }

        //compares if they are the same 2 coords
        public static bool operator== (ChunkCoord one, ChunkCoord other)
        {
            return (one.X == other.X && one.Z == other.Z);
        }
        public static bool operator!= (ChunkCoord one, ChunkCoord other)
        {
            return !(one == other);
        }

        //hashing
        //override Equals
        public override bool Equals(object? obj)
        {
            return obj is ChunkCoord other && Equals(other);
        }

        public bool Equals(ChunkCoord other)
        {
            return this == other;
        }

        // override GetHashCode
        public override int GetHashCode()
        {
            unchecked //allows wraparound on overflow
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Z;
                return hash;
            }
        }
    }

    //represents a chunks generation state
    public enum ChunkState
    {
        Initialized,  //newly created, no data
        VoxelOnly,    //voxel data generated
        Meshed,       //mesh generated but not uploaded
        Built,        //mesh uploaded to GPU
        Deleted       //fully removed
    }

    //holds mesh and block data for a part of the world
    public class Chunk
    {
        //---explanation---

        //chunks are collections of subchunks (which hold the block and mesh data)
        //chunks are treated as sections of the world that group voxel and mesh data together
        //this is so that every block is not treated as an individiual (that would then mean many draw calls, which is bad for gpu)
        //the chunks are 16 by 16 subchunks, that are then stacked on top of each other 16 times, this creates the total chunk of 16 * 256 * 16

        //---data---
        //shader - drawing info for subchunks
        //Pos - global position of chunk for subchunks to use when building meshes
        //subchunk count and chunk height - already explained
        //subChunks - the actual collection of subchunks
        //state - chunks generation state (intialized, only block ids, ready to mesh, meshed, deleted) useful for synchronization

        //rendering
        readonly Camera cam;
        readonly ChunkMesh batchedMesh;

        //world data
        public ChunkCoord Pos { get; private set; }
        public const int SUBCHUNK_COUNT = 12;
        public const int CHUNK_HEIGHT = SubChunk.SUBCHUNK_SIZE * SUBCHUNK_COUNT;
        public SubChunk[] subChunks = new SubChunk[SUBCHUNK_COUNT];

        //generation data
        volatile ChunkState state;

        //frustum culling
        public Vector3 chunkMin;
        public Vector3 chunkMax;

        //basic constructor
        public Chunk(ChunkCoord coord, Camera cam)
        {
            this.cam = cam;
            Pos = coord;
            state = ChunkState.Initialized;
            batchedMesh = new ChunkMesh();

            //initialize subchunks
            for (int i = 0; i < SUBCHUNK_COUNT; i++)
            {
                subChunks[i] = new SubChunk(this, i);
            }
            chunkMin = new Vector3(Pos.X * SubChunk.SUBCHUNK_SIZE, 0, Pos.Z * SubChunk.SUBCHUNK_SIZE);
            chunkMax = chunkMin + new Vector3(SubChunk.SUBCHUNK_SIZE, SUBCHUNK_COUNT * SubChunk.SUBCHUNK_SIZE, SubChunk.SUBCHUNK_SIZE);
        }

        //fills in all subchunks with block id data
        public void CreateVoxelMap()
        {          
            if (state != ChunkState.Initialized) return;

            NoiseRegion[,] terrainHeights = new NoiseRegion[SubChunk.SUBCHUNK_SIZE, SubChunk.SUBCHUNK_SIZE];

            for (int z = 0; z < SubChunk.SUBCHUNK_SIZE; z++)
            {
                for (int x = 0; x < SubChunk.SUBCHUNK_SIZE; x++)
                {
                    int globalX =  Pos.X * SubChunk.SUBCHUNK_SIZE + x;
                    int globalZ = Pos.Z * SubChunk.SUBCHUNK_SIZE + z;
                    NoiseRegion terrainHeight = NoiseGenerator.GetTerrainRegion(globalX, globalZ);
                    terrainHeights[x, z] = terrainHeight;
                }
            }          

            foreach (var subChunk in subChunks)
            {
                subChunk.CreateVoxelMap(terrainHeights);
            }
            state = ChunkState.VoxelOnly;     
        }

        //creates all subchunk mesh data
        public void CreateChunkMesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC)
        {
            if (state != ChunkState.VoxelOnly) return;
            foreach(var subChunk in subChunks)
            {
                subChunk.CreateChunkMesh(leftC, rightC, frontC, backC);
            }
            state = ChunkState.Meshed;
        }

        //builds all subchunk meshes to openGL
        public void SendMeshToOpenGL()
        {
            if (state == ChunkState.Deleted || state == ChunkState.VoxelOnly || state == ChunkState.Initialized)
                return;
            UploadSolidMesh();
            state = ChunkState.Built;
        }

        //combines the vertex data into one big mesh for solid geo
        private void UploadSolidMesh()
        {
            //compute size upfront
            int totalVertexCount = 0;
            int totalIndexCount = 0;

            foreach (var subChunk in subChunks)
            {
                totalVertexCount += subChunk.SolidGeo.vertices.Count;
                totalIndexCount += subChunk.SolidGeo.indices.Count;
            }

            //preallocate size
            var totalVertices = new List<Vertex>(totalVertexCount);
            var totalIndices = new List<uint>(totalIndexCount);

            //append mesh data
            foreach (var subChunk in subChunks)
            {
                var geo = subChunk.SolidGeo;
                if (geo.vertices.Count == 0 || geo.indices.Count == 0)
                    continue;

                uint baseIndex = (uint)totalVertices.Count;

                //append vertices
                totalVertices.AddRange(geo.vertices);

                //fast index transform using a temporary array
                int indexCount = geo.indices.Count;
                var adjustedIndices = new uint[indexCount];
                for (int i = 0; i < indexCount; i++)
                {
                    adjustedIndices[i] = geo.indices[i] + baseIndex;
                }
                totalIndices.AddRange(adjustedIndices);
            }

            //mesh upload
            batchedMesh.SetupMesh(totalVertices, totalIndices);
            batchedMesh.transform.SetPosition(new Vector3(Pos.X * SubChunk.SUBCHUNK_SIZE, 0, Pos.Z * SubChunk.SUBCHUNK_SIZE));
        }

        //draws all the subchunk opaque meshes
        public void Draw(Shader shader, Camera camera)
        {
            batchedMesh.Draw(shader, camera);
        }

        //makes all subchunk meshes empty
        public void ClearChunkMesh()
        {
            foreach (var subChunk in subChunks)
            {
                subChunk.ClearMesh();
            }
            batchedMesh.ClearMesh();
            state = ChunkState.VoxelOnly;
        }

        //dispose all subchunk meshes properly
        public void Delete()
        {
            state = ChunkState.Deleted;
            foreach(var subChunk in subChunks)
            {
                subChunk.Delete();
            }
            batchedMesh.Delete();
        }

        //getters
        //get state
        public ChunkState GetState()
        {
            return state;
        }

        //get if chunk has block data
        public bool HasVoxelData()
        {
            return state != ChunkState.Deleted && state != ChunkState.Initialized;
        }

        //gets if a chunk can be modified
        public bool Remeshable()
        {
            return state == ChunkState.Built;
        }

        //checks if chunk is drawable
        public static bool IsBoxInFrustum(FrustumPlane[] planes, Vector3 min, Vector3 max)
        {
            foreach (var plane in planes)
            {
                Vector3 positiveVertex = new Vector3(
                    plane.Normal.X >= 0 ? max.X : min.X,
                    plane.Normal.Y >= 0 ? max.Y : min.Y,
                    plane.Normal.Z >= 0 ? max.Z : min.Z
                );

                if (plane.GetSignedDistanceToPoint(positiveVertex) < 0)
                    return false;
            }

            return true;
        }

        //block states

        //trys to get the block from a chunk
        public BlockState GetBlock(int x, int globalY, int z)
        {
            if (HasVoxelData() == false || globalY < 0 || globalY > CHUNK_HEIGHT - 1) return new BlockState(0);

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY % SubChunk.SUBCHUNK_SIZE;

            return subChunks[subChunkY].GetBlockState(x, localY, z);
        }

        //trys to set the block in a chunk safely
        public void SetBlock(int x, int y, int z, BlockState state)
        {
            if (HasVoxelData() == false || y < 0 || y > CHUNK_HEIGHT - 1) return;

            //get subchunk position
            int subChunkY = y / SubChunk.SUBCHUNK_SIZE;
            int localY = y % SubChunk.SUBCHUNK_SIZE;

            subChunks[subChunkY].SetBlock(x, localY, z, state);
        }

        //remesh subchunks that are effected by a block being broken or placed
        public void Remesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, int globalY)
        {
            //get subchunk position for remeshing
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY % SubChunk.SUBCHUNK_SIZE;

            //remesh this subchunk and adjacent subchunks if on a subchunk edge
            subChunks[subChunkY].Remesh(leftC, rightC, frontC, backC);
            if (localY == SubChunk.SUBCHUNK_SIZE - 1 && subChunks[subChunkY + 1] != null) subChunks[subChunkY + 1].Remesh(leftC, rightC, frontC, backC);
            if (localY == 0 && subChunkY - 1 >= 0) subChunks[subChunkY - 1].Remesh(leftC, rightC, frontC, backC);

            batchedMesh.ClearMesh();
            SendMeshToOpenGL();
        }
    }

    //small part of a chunk that contains block data and mesh data
    public class SubChunk
    {
        //---explanation---

        //subchunks contain all the actual block and mesh data for a chunk
        //we split things into subchunks so that block states, and block remeshing is easier
        //we only draw subchunks and meshes that have vertex data, so if a mesh has no faces or is all air, dont draw it
        //this makes things easier for remeshing while making initial generation still fast

        //---data---
        //subchunk size - how big a subchunk is in blocks^3
        //blocks - the actual refrence to the block states in the chunk
        //yPos, in which part of the parent chunk is this subchunk located in
        //parent - the chunk that contains this subchunk
        //shader - for drawing meshes
        //chunkmesh - for the geometry and calling openGL functions

        public const int SUBCHUNK_SIZE = 32;
        private IBlockIndexStorage blockIndices;
        private readonly BlockPalette palette;
        private bool isAllAir = true;
        public int YPos { get; private set; } = 0;
        Chunk parent;
        public ChunkMeshData SolidGeo { get; private set; }
        public Vector3 Center { get; private set; }

        //basic constructor
        public SubChunk(Chunk parent, int yPos)
        { 
            this.parent = parent;
            YPos = yPos;
            palette = new BlockPalette();
            blockIndices = new ByteBlockStorage(SUBCHUNK_SIZE * SUBCHUNK_SIZE * SUBCHUNK_SIZE);

            SolidGeo = new ChunkMeshData();

            Center = new Vector3(
                 parent.Pos.X * SUBCHUNK_SIZE + SUBCHUNK_SIZE / 2.0f,
                 yPos * SUBCHUNK_SIZE + SUBCHUNK_SIZE / 2.0f,
                 parent.Pos.Z * SUBCHUNK_SIZE + SUBCHUNK_SIZE / 2.0f
            );
        }

        //fills in the block id array of the chunk
        public void CreateVoxelMap(NoiseRegion[,] terrainHeights)
        {
            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {             
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion terrainHeight = terrainHeights[x, z];

                    for (int y = 0; y < SUBCHUNK_SIZE; y++)
                    {                                          
                        int globalX = parent.Pos.X * SUBCHUNK_SIZE + x;
                        int globalY = YPos * SUBCHUNK_SIZE + y;
                        int globalZ = parent.Pos.Z * SUBCHUNK_SIZE + z;
                        BlockState state = new BlockState(BlockIDs.AIR_BLOCK);
                        float density = NoiseGenerator.GetDensity(globalX, globalY, globalZ, terrainHeight);

                        if (density > 0.0) state = new BlockState(BlockIDs.STONE_BLOCK);
                        if ((int)terrainHeight.baseHeight == globalY) state = new BlockState(BlockIDs.GRASS_BLOCK);
                        SetBlock(x, y, z, state);

                        if (GetBlockState(x, y, z).BlockID == BlockIDs.AIR_BLOCK && globalY <= NoiseGenerator.SEA_LEVEL)
                        {
                            BlockState state2 = new BlockState(BlockIDs.WATER_BLOCK);
                            SetBlock(x, y, z, state2);
                        }
                    }
                }
            }
        }

        //tries to create the first mesh
        public void CreateChunkMesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC)
        {
            if (isAllAir) return; //dont create mesh if chunk is completely air

            for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--)
                for (int x = SUBCHUNK_SIZE - 1; x >= 0; x--)
                    for (int z = SUBCHUNK_SIZE - 1; z >= 0; z--)
                    {
                        AddVoxelDataToChunk(new Vector3(x, y, z), new Vector3(x, YPos * SUBCHUNK_SIZE + y, z), leftC, rightC, frontC, backC);
                    }
        }

        //tries to add face data to a chunk mesh based on a bitmask of the adjacent blocks
        public void AddVoxelDataToChunk(Vector3 pos, Vector3 meshPos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC)
        {            
            Block block = BlockData.GetBlock(GetBlockState((int)pos.X, (int)pos.Y, (int)pos.Z).BlockID);
            BlockState state = GetBlockState((int)pos.X, (int)pos.Y, (int)pos.Z);

            //get neighbor blocks
            BlockState top = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, 1, 0, leftC, rightC, frontC, backC);
            BlockState bottom = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, -1, 0, leftC, rightC, frontC, backC);
            BlockState front = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, 0, 1, leftC, rightC, frontC, backC);
            BlockState back = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, 0, -1, leftC, rightC, frontC, backC);
            BlockState left = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, -1, 0, 0, leftC, rightC, frontC, backC);
            BlockState right = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 1, 0, 0, leftC, rightC, frontC, backC);

            block.AddBlockMesh(meshPos, bottom, top, front, back, right, left, SolidGeo, state);
        }

        //rebuilds the subchunk mesh data when modifying a block
        public void Remesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC)
        {
            ClearMesh();
            CreateChunkMesh(leftC, rightC, frontC, backC);
        }

        //clears the mesh data
        public void ClearMesh()
        {
            SolidGeo.ClearMesh();
        }

        //clears all the chunk mesh data and disposes the openGL mesh
        public void Delete()
        {
            SolidGeo.ClearMesh();
        }

        //trys to get the block from a chunk
        public BlockState GetBlockState(int x, int y, int z)
        {
            int index = Flatten(x, y, z);
            int paletteIndex = blockIndices.Get(index);
            return palette.GetState(paletteIndex);
        }

        //trys to set the block in a chunk
        public void SetBlock(int x, int y, int z, BlockState state)
        {
            int index = Flatten(x, y, z);
            int paletteIndex = palette.GetOrAddIndex(state);
            if (state.BlockID != BlockIDs.AIR_BLOCK) isAllAir = false;

            //auto-upgrade to ushort
            if (paletteIndex > byte.MaxValue && blockIndices is ByteBlockStorage)
            {
                UpgradeToUShort();
            }

            blockIndices.Set(index, paletteIndex);
        }

        //helper to fetch neighbor types safely
        public BlockState GetNeighborBlockSafe(int x, int y, int z, int offsetX, int offsetY, int offsetZ, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC)
        {
            int nx = x + offsetX;
            int ny = YPos * SUBCHUNK_SIZE + y + offsetY; //global Y
            int nz = z + offsetZ;

            // out of world bounds
            if (ny < 0 || ny >= Chunk.CHUNK_HEIGHT)
                return new BlockState(BlockIDs.AIR_BLOCK);

            Chunk? targetChunk = parent;

            //X-axis neighbors
            if (nx < 0) targetChunk = leftC;
            else if (nx >= SUBCHUNK_SIZE) targetChunk = rightC;

            //Z-axis neighbors
            if (nz < 0) targetChunk = backC;
            else if (nz >= SUBCHUNK_SIZE) targetChunk = frontC;

            if (targetChunk == null || !targetChunk.HasVoxelData())
                return new BlockState(BlockIDs.AIR_BLOCK);

            //convert to local coordinates inside target chunk/subchunk
            int localX = (nx + SUBCHUNK_SIZE) % SUBCHUNK_SIZE;
            int localZ = (nz + SUBCHUNK_SIZE) % SUBCHUNK_SIZE;

            return targetChunk.GetBlock(localX, ny, localZ);
        }

        //math helpers
        private static int Flatten(int x, int y, int z)
        {
            return x + SUBCHUNK_SIZE * (y + SUBCHUNK_SIZE * z);
        }

        //change index data to ushort when many block states
        private void UpgradeToUShort()
        {
            var oldStorage = (ByteBlockStorage)blockIndices;
            var newStorage = new UShortBlockStorage(oldStorage.Length);

            for (int i = 0; i < oldStorage.Length; i++)
            {
                newStorage.Set(i, oldStorage.Get(i));
            }
            blockIndices = newStorage;
        }
    }
}