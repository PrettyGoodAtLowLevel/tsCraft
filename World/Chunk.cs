using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Blocks.Meshing;
using OurCraft.Graphics;
using OurCraft.utility;
using OurCraft.World.Terrain_Generation;
using OurCraft.World.Terrain_Generation.SurfaceFeatures;
using static OurCraft.Graphics.Camera;
using OurCraft.Graphics.Voxel_Lighting;

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
        //rendering
        public readonly ChunkMesh batchedMesh;
        public readonly ChunkMesh transparentMesh;

        //world data
        public ChunkCoord Pos { get; private set; }
        public const int SUBCHUNK_COUNT = 12;
        public const int CHUNK_HEIGHT = SubChunk.SUBCHUNK_SIZE * SUBCHUNK_COUNT;

        public SubChunk[] subChunks = new SubChunk[SUBCHUNK_COUNT];
        public ushort[,,] lightMap = new ushort[0, 0, 0];
        public ushort[,] heightMap = new ushort[0, 0]; 

        //generation data
        volatile ChunkState state;
        public List<int> changes = [];             

        //frustum culling
        public Vector3 chunkMin;
        public Vector3 chunkMax;

        //basic constructor
        public Chunk(ChunkCoord coord)
        {         
            Pos = coord;
            state = ChunkState.Initialized;
            batchedMesh = new ChunkMesh();
            transparentMesh = new ChunkMesh();

            //initialize subchunks
            for (int i = 0; i < SUBCHUNK_COUNT; i++)
            {
                subChunks[i] = new SubChunk(this, i);
            }
            chunkMin = new Vector3(Pos.X * SubChunk.SUBCHUNK_SIZE, 0, Pos.Z * SubChunk.SUBCHUNK_SIZE);
            chunkMax = chunkMin + new Vector3(SubChunk.SUBCHUNK_SIZE, SUBCHUNK_COUNT * SubChunk.SUBCHUNK_SIZE, SubChunk.SUBCHUNK_SIZE);
        }

        //set all lightmap values to default
        public void InitLightMap()
        {
            lightMap = new ushort[SubChunk.SUBCHUNK_SIZE, SubChunk.SUBCHUNK_SIZE * SUBCHUNK_COUNT, SubChunk.SUBCHUNK_SIZE];
            heightMap = new ushort[SubChunk.SUBCHUNK_SIZE, SubChunk.SUBCHUNK_SIZE];

            const int sky0 = 0;
            ushort defaultLight = sky0;

            for (int x = 0; x < SubChunk.SUBCHUNK_SIZE; x++)
            {
                for (int z = 0; z < SubChunk.SUBCHUNK_SIZE; z++)
                {
                    heightMap[x, z] = CHUNK_HEIGHT - 1;
                    for (int y = 0; y < SubChunk.SUBCHUNK_SIZE * SUBCHUNK_COUNT; y++)
                    {                  
                         lightMap[x, y, z] = defaultLight;
                    }
                }
            }            
        }

        //fills in all subchunks with block id data
        public void CreateVoxelMap()
        {          
            if (state != ChunkState.Initialized) return;
            //prefill lightmap
            InitLightMap();

            //create noise regions
            NoiseRegion[,] noiseRegions = new NoiseRegion[SubChunk.SUBCHUNK_SIZE, SubChunk.SUBCHUNK_SIZE];
            for (int z = 0; z < SubChunk.SUBCHUNK_SIZE; z++)
            {
                for (int x = 0; x < SubChunk.SUBCHUNK_SIZE; x++)
                {
                    int globalX =  Pos.X * SubChunk.SUBCHUNK_SIZE + x;
                    int globalZ = Pos.Z * SubChunk.SUBCHUNK_SIZE + z;
                    NoiseRegion noiseRegion = WorldGenerator.GetTerrainRegion(globalX, globalZ);
                    noiseRegions[x, z] = noiseRegion;
                }
            }

            //create base density map
            foreach (var subChunk in subChunks)
            {
                subChunk.CreateDensityMap(noiseRegions);                
            }

            //surface paint the subchunks
            foreach (var subChunk in subChunks)
            {
                subChunk.SurfacePaint(noiseRegions);
            }

            //slap on surface feature
            foreach (var subChunk in subChunks)
            {              
                subChunk.PlaceSurfaceFeatures(noiseRegions);            
            }
            
            //update state
            state = ChunkState.VoxelOnly;
        }

        //creates all subchunk mesh data
        public void CreateChunkMesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
            Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            if (!HasVoxelData()) return;
            foreach(var subChunk in subChunks)
            {
                subChunk.ClearMesh();
            }

            foreach (var subChunk in subChunks)
            {
                subChunk.CreateChunkMesh(leftC, rightC, frontC, backC, c1, c2, c3, c4);
            }        
            
            int totalVertexCount = 0;
            foreach (var subChunk in subChunks)
            {
                totalVertexCount += subChunk.SolidGeo.vertices.Count;
            }
            batchedMesh.SetupIndices(totalVertexCount);
            transparentMesh.SetupIndices(totalVertexCount);

            if (state == ChunkState.VoxelOnly)
                state = ChunkState.Meshed;
        }

        //builds the combined subchunk mesh data
        public void SendMeshToOpenGL()
        {
            if (state == ChunkState.Deleted || state == ChunkState.VoxelOnly || state == ChunkState.Initialized)
                return;
            UploadSolidMesh();
            UploadTranparentMesh();
            state = ChunkState.Built;
        }
        
        //combines the vertex data into one big mesh for solid geometry
        private void UploadSolidMesh()
        {         
            //compute size upfront
            int totalVertexCount = 0;

            foreach (var subChunk in subChunks)
            {
                totalVertexCount += subChunk.SolidGeo.vertices.Count;
            }

            //preallocate size
            var totalVertices = new List<BlockVertex>(totalVertexCount);

            int vertexOffset = 0;

            //append mesh data
            foreach (var subChunk in subChunks)
            {
                ChunkMeshData geo = subChunk.SolidGeo;
                if (geo.vertices.Count == 0)
                    continue;

                //append vertices
                totalVertices.AddRange(geo.vertices);

                vertexOffset += geo.vertices.Count;
            }

            //mesh upload
            batchedMesh.SetupMesh(totalVertices);
            batchedMesh.transform.SetPosition(new Vector3(Pos.X * SubChunk.SUBCHUNK_SIZE, 0, Pos.Z * SubChunk.SUBCHUNK_SIZE));
        }

        //same thing as the regular mesh, but for transparent water geometry
        private void UploadTranparentMesh()
        {
            //compute size upfront
            int totalVertexCount = 0;

            foreach (var subChunk in subChunks)
            {
                totalVertexCount += subChunk.TransparentGeo.vertices.Count;
            }

            //preallocate size
            var totalVertices = new List<BlockVertex>(totalVertexCount);
            int vertexOffset = 0;

            //append mesh data
            foreach (var subChunk in subChunks)
            {
                ChunkMeshData geo = subChunk.TransparentGeo;
                if (geo.vertices.Count == 0)
                    continue;

                //append vertices
                totalVertices.AddRange(geo.vertices);

                vertexOffset += geo.vertices.Count;
            }

            //mesh upload
            transparentMesh.SetupMesh(totalVertices);
            transparentMesh.transform.SetPosition(new Vector3(Pos.X * SubChunk.SUBCHUNK_SIZE, 0, Pos.Z * SubChunk.SUBCHUNK_SIZE));
        }

        //draws all the subchunk opaque meshes
        public void Draw(Shader shader, Camera camera)
        {
            batchedMesh.Draw(shader, camera);       
        }

        //draws all of the transparent stuff in a subchunk
        public void DrawTransparent(Shader shader, Camera camera)
        {
            transparentMesh.Draw(shader, camera);
        }

        //dispose all subchunk meshes properly
        public void Delete()
        {
            state = ChunkState.Deleted;
            batchedMesh.Delete();
            transparentMesh.Delete();
        }

        //set chunk ready to delete
        public void MarkForDeletion()
        {
            state = ChunkState.Deleted;
            foreach(var subChunk in subChunks)
            {
                subChunk.isAllAir = true;
            }
        }

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

        //gets if a chunk is being deleted
        public bool Deleted()
        {
            return state == ChunkState.Deleted;
        }

        //block manipulation

        //trys to get the block from a chunk
        public BlockState GetBlockSafe(int x, int globalY, int z)
        {
            if (HasVoxelData() == false || globalY < 0 || globalY > CHUNK_HEIGHT - 1)
            {
                return new BlockState(0);
            }

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY % SubChunk.SUBCHUNK_SIZE;

            return subChunks[subChunkY].GetBlockState(x, localY, z);
        }

        //instantly gets a block from a chunk, may fail
        public BlockState GetBlockUnsafe(int x, int globalY, int z)
        {
            //doesnt check if has all blocks yet
            if (globalY < 0 || globalY > CHUNK_HEIGHT - 1)
            {
                return new BlockState(0);
            }

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY % SubChunk.SUBCHUNK_SIZE;

            return subChunks[subChunkY].GetBlockState(x, localY, z);
        }

        //sets block and sets chunk as dirty
        public void SetBlock(int x, int y, int z, BlockState state)
        {
            if (HasVoxelData() == false || y < 0 || y > CHUNK_HEIGHT - 1) return;
            changes.Add(y);
            //get subchunk position
            int subChunkY = y / SubChunk.SUBCHUNK_SIZE;
            int localY = y % SubChunk.SUBCHUNK_SIZE;
            subChunks[subChunkY].SetBlock(x, localY, z, state);
        }

        //gives back skylight and block light from local position
        public ushort GetLight(int x, int y, int z)
        {            
            if (HasVoxelData() == false || !PosValid(x, y, z))
            {
                return 0;
            }
            return lightMap[x, y, z];
        }

        //sets a whole light value position, dont use this unless needed
        public void SetLight(int x, int y, int z, ushort value)
        {
            if (HasVoxelData() == false || !PosValid(x, y, z))
            {
                return;
            }
            lightMap[x, y, z] = value;
        }

        //sets only the block rgb colors at a light value position
        public void SetBlockLight(int x, int y, int z, Vector3i value)
        {
            if (HasVoxelData() == false || !PosValid(x, y, z))
                return;

            //preserve the upper 4 bits for skylight
            ushort current = lightMap[x, y, z];    
            ushort preserved = (ushort)(current & 0xF000);

            //pack blocklight into the lower 12 bits
            ushort packed = (ushort)((value.X & 0xF) |
            ((value.Y & 0xF) << 4) | ((value.Z & 0xF) << 8));

            lightMap[x, y, z] = (ushort)(preserved | packed);
        }

        //sets a skylight value at a local position in the chunk
        public void SetSkyLight(int x, int y, int z, int value)
        {
            if (!PosValid(x, y, z)) return;

            //read the current light value
            ushort current = lightMap[x, y, z];

            //rreserve the lower 12 bits of block lights
            ushort preservedBlockLight = (ushort)(current & 0x0FFF);
            ushort newSkyLight = (ushort)((value & 0xF) << 12);

            //combine preserved block light with new sky light
            lightMap[x, y, z] = (ushort)(preservedBlockLight | newSkyLight);
        }

        public void SetHeightMap(int x, int z, int value)
        {
            if (!PosValid(x, z)) return;
            if (value >= CHUNK_HEIGHT) heightMap[x, z] = CHUNK_HEIGHT;
            heightMap[x, z] = (ushort)value;
        }

        //unsafe setblock
        public void SetBlockUnsafe(int x, int y, int z, BlockState state)
        {
            if (y < 0 || y > CHUNK_HEIGHT - 1) return;

            //get subchunk position
            int subChunkY = y / SubChunk.SUBCHUNK_SIZE;
            int localY = y % SubChunk.SUBCHUNK_SIZE;
            
            subChunks[subChunkY].SetBlock(x, localY, z, state);
        }

        //remesh subchunks that are effected by a block being broken or placed
        public void Remesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            //helper to safely add subchunk indices
            bool TryAddSubChunk(HashSet<int> set, int idx)
            {
                if (idx >= 0 && idx < subChunks.Length)
                {
                    set.Add(idx);
                    return true;
                }
                return false;
            }

            var toRemesh = new HashSet<int>();

            for (int i = 0; i < changes.Count; i++)
            {
                int globalY = changes[i];
                int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
                int localY = globalY % SubChunk.SUBCHUNK_SIZE;

                TryAddSubChunk(toRemesh, subChunkY);

                if (localY == SubChunk.SUBCHUNK_SIZE - 1)
                    TryAddSubChunk(toRemesh, subChunkY + 1);

                if (localY == 0)
                    TryAddSubChunk(toRemesh, subChunkY - 1);
            }

            //remesh each unique subchunk exactly once
            foreach (int idx in toRemesh)
            {
                subChunks[idx].Remesh(leftC, rightC, frontC, backC, c1, c2, c3, c4);
            }

            int totalVertexCount = 0;

            foreach (var subChunk in subChunks)
            {
                totalVertexCount += subChunk.SolidGeo.vertices.Count;
            }
            batchedMesh.SetupIndices(totalVertexCount);
            transparentMesh.SetupIndices(totalVertexCount);

            SendMeshToOpenGL();

            //now that everything is updated, clear it
            changes.Clear();
        }

        //debug and helpers

        //checks if a position fits in a chunk, for many axis
        public static bool PosValid(int x, int y, int z)
        {
            return x >= 0 && x < SubChunk.SUBCHUNK_SIZE && z >= 0 && z < SubChunk.SUBCHUNK_SIZE && y >= 0 && y < SubChunk.SUBCHUNK_SIZE * SUBCHUNK_COUNT;
        }

        //overload only for x and z layers of a chunk
        public static bool PosValid(int x, int z)
        {
            return x >= 0 && x < SubChunk.SUBCHUNK_SIZE && z >= 0 && z < SubChunk.SUBCHUNK_SIZE;
        }

        //checks if chunk is in camera view
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
    }

    //small part of a chunk that contains block data and mesh data
    public class SubChunk
    {
        public const int SUBCHUNK_SIZE = 32;
        private IBlockIndexStorage blockIndices;
        private readonly BlockPalette palette;
        public List<ushort> lightSources = new List<ushort>();
        public bool isAllAir = true;
        public int YPos { get; private set; } = 0;
        readonly Chunk parent;
        public ChunkMeshData SolidGeo { get; private set; }
        public ChunkMeshData TransparentGeo { get; private set; }

        //basic constructor
        public SubChunk(Chunk parent, int yPos)
        { 
            this.parent = parent;
            YPos = yPos;
            palette = new BlockPalette();
            blockIndices = new ByteBlockStorage(SUBCHUNK_SIZE * SUBCHUNK_SIZE * SUBCHUNK_SIZE);
            SolidGeo = new ChunkMeshData();
            TransparentGeo = new ChunkMeshData();
        }

        //create base density map - stone vs air
        public void CreateDensityMap(NoiseRegion[,] noiseRegions)
        {
            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {             
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion noiseRegion = noiseRegions[x, z];
                    for (int y = 0; y < SUBCHUNK_SIZE; y++)
                    {   
                        //create stone-air map
                        int globalX = parent.Pos.X * SUBCHUNK_SIZE + x;
                        int globalY = YPos * SUBCHUNK_SIZE + y;
                        int globalZ = parent.Pos.Z * SUBCHUNK_SIZE + z;
                        float density = WorldGenerator.GetDensity(globalX, globalY, globalZ, noiseRegion);

                        //we use raw block ids for extra performance
                        BlockState state = new(BlockIDs.AIR_BLOCK);                
                        if (density > 0) state = new(BlockIDs.STONE_BLOCK);
                        SetBlock(x, y, z, state);

                        //fill air blocks below sea level with water
                        if (GetBlockState(x, y, z).BlockID == BlockIDs.AIR_BLOCK && globalY <= WorldGenerator.SEA_LEVEL)
                        {
                            BlockState state2 = globalY == WorldGenerator.SEA_LEVEL ? new(noiseRegion.biome.WaterSurfaceBlock) : new(noiseRegion.biome.WaterBlock);
                            SetBlock(x, y, z, state2);
                        }
                    }
                }
            }
        }

        //paint the top layers with their respective biome surface block
        public void SurfacePaint(NoiseRegion[,] noiseRegions)
        { 
            for(int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                for(int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion noiseRegion = noiseRegions[x, z];

                    for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--) // top-down
                    {
                        BlockState current = GetBlockState(x, y, z);
                        BlockState above = (y + 1 < SUBCHUNK_SIZE) ? GetBlockState(x, y + 1, z) : parent.GetBlockUnsafe(x, (y + YPos * SUBCHUNK_SIZE) + 1, z);

                        bool currentEligible = current.BlockID != BlockIDs.AIR_BLOCK && current.BlockID != noiseRegion.biome.WaterBlock && current.BlockID != noiseRegion.biome.WaterSurfaceBlock;
                        bool aboveEligible = above.BlockID == BlockIDs.AIR_BLOCK || above.BlockID == noiseRegion.biome.WaterBlock || above.BlockID == noiseRegion.biome.WaterSurfaceBlock;

                        //only consider blocks with air above (i.e., surface or overhang)
                        if (currentEligible && aboveEligible)
                        {
                            for (int d = 0; d < 5 && (y - d) >= 0; d++) //the loop values are customizable
                            {
                                int targetY = y - d;

                                //customize depth levels for the biomes
                                if (d == 0)
                                    SetBlock(x, targetY, z, new(WorldGenerator.GetSurfaceBlock(noiseRegion.biome, targetY + YPos * SUBCHUNK_SIZE)));
                                else if (d <= 2)
                                    SetBlock(x, targetY, z, new(WorldGenerator.GetSubSurfaceBlock(noiseRegion.biome, targetY + YPos * SUBCHUNK_SIZE)));                               
                            }
                        }
                    }
                }
            }
        }

        //slap on trees, rocks, grass all on top of top layer of blocks
        public void PlaceSurfaceFeatures(NoiseRegion[,] noiseRegions)
        {
            int seed = NoiseRouter.seed;
            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion noiseRegion = noiseRegions[x, z];

                    for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--) // top-down
                    {
                        BlockState current = GetBlockState(x, y, z);
                        BlockState above = (y + 1 < SUBCHUNK_SIZE) ? GetBlockState(x, y + 1, z) : parent.GetBlockUnsafe(x, (y + YPos * SUBCHUNK_SIZE) + 1, z);

                        bool currentEligible = current.BlockID == WorldGenerator.GetSurfaceBlock(noiseRegion.biome, y + YPos * SUBCHUNK_SIZE);
                        bool aboveEligible = above.BlockID == BlockIDs.AIR_BLOCK;

                        //only consider blocks with air above (i.e., surface or overhang)
                        if (currentEligible && aboveEligible)
                        {
                            foreach (BiomeSurfaceFeature feature in noiseRegion.biome.SurfaceFeatures)
                            {
                                Vector3i globalCoords = new(x + parent.Pos.X * SUBCHUNK_SIZE, (y + YPos * SUBCHUNK_SIZE), z + parent.Pos.Z * SUBCHUNK_SIZE);

                                //generate random number with deterministic coordinate hash function (super weird but thread safe)
                                int rand = NoiseRouter.GetStructureRandomness(globalCoords.X, globalCoords.Y, globalCoords.Z, seed, feature.chance);

                                if (rand == 1 && feature.feature.CanPlaceFeature(new Vector3i(x, (y + YPos * SUBCHUNK_SIZE) + 1, z), parent))
                                {
                                    feature.feature.PlaceFeature(new Vector3i(x, (y + YPos * SUBCHUNK_SIZE) + 1, z), parent);
                                    break;
                                }                                  
                            }                            
                        }
                    }
                }
            }
        }

        //tries to create the mesh data
        public void CreateChunkMesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            if (isAllAir) return; //dont create mesh if chunk is completely air          
            for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--)
                for (int x = SUBCHUNK_SIZE - 1; x >= 0; x--)
                    for (int z = SUBCHUNK_SIZE - 1; z >= 0; z--)
                    {
                        AddMeshDataToChunk(new Vector3(x, y, z), new Vector3(x, YPos * SUBCHUNK_SIZE + y, z), leftC, rightC, frontC, backC, c1, c2, c3, c4);
                    }
            SolidGeo.vertices.TrimExcess();
            TransparentGeo.vertices.TrimExcess();
        }

        //tries to add face data to a chunk mesh based on a bitmask of the adjacent blocks
        //also samples lighting values for blocks exposed to light
        public void AddMeshDataToChunk(Vector3 pos, Vector3 meshPos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {            
            Block block = BlockData.GetBlock(GetBlockState((int)pos.X, (int)pos.Y, (int)pos.Z).BlockID);
            if (block.blockShape is EmptyBlockShape) return;

            BlockState state = GetBlockState((int)pos.X, (int)pos.Y, (int)pos.Z);

            //get neighbor blocks
            BlockState top = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, 1, 0, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            BlockState bottom = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, -1, 0, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            BlockState front = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, 0, 1, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            BlockState back = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, 0, -1, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            BlockState left = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, -1, 0, 0, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            BlockState right = GetNeighborBlockSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 1, 0, 0, leftC, rightC, frontC, backC, c1, c2, c3, c4);

            //if surrounding blocks are all full solid cubes, and the current block is also full solid, then skip meshing entirely
            if (top.GetBlock.blockShape.IsFullOpaqueBlock && bottom.GetBlock.blockShape.IsFullOpaqueBlock
            && front.GetBlock.blockShape.IsFullOpaqueBlock && back.GetBlock.blockShape.IsFullOpaqueBlock &&
            left.GetBlock.blockShape.IsFullOpaqueBlock && right.GetBlock.blockShape.IsFullOpaqueBlock
            && block.blockShape.IsFullOpaqueBlock)
                return;

            ushort thisLight = parent.GetLight((int)meshPos.X, (int)meshPos.Y, (int)meshPos.Z);

            LightingData lightData = GetLightData(pos, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            ChunkMeshData meshRef = block.blockShape.IsTranslucent ? TransparentGeo : SolidGeo;
            block.blockShape.AddBlockMesh(meshPos, bottom, top, front, back, right, left, meshRef, state, lightData, thisLight);
        }

        //returns the values necessary for computing the light values for meshing
        private LightingData GetLightData(Vector3 pos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
        Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            LightingData ld = new();
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int z = (int)pos.Z;

            //helper to get neighbor block safely
            ushort L(int ox, int oy, int oz) => GetLightSafe(x, y, z, ox, oy, oz, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            ld.thisLight = L(0, 0, 0);

            //top face + top corners
            ld.topLight = L(0, +1, 0);
            ld.bottomLight = L(0, -1, 0);
            ld.frontLight = L(0, 0, +1);
            ld.backLight = L(0, 0, -1);
            ld.rightLight = L(1, 0, 0);
            ld.leftLight = L(-1, 0, 0);

            return ld;
        }

        //rebuilds the subchunk mesh data when modifying a block
        public void Remesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {          
            ClearMesh();
            CreateChunkMesh(leftC, rightC, frontC, backC, c1, c2, c3, c4);
        }

        //clears the mesh data
        public void ClearMesh()
        {
            SolidGeo.ClearMesh();
            TransparentGeo.ClearMesh();
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
            if (state.BlockID != BlockIDs.AIR_BLOCK) isAllAir = false; //update if not all air anymore
            
            //update light sources if light block is reset
            BlockState oldState = GetBlockState(x, y, z);
            if (oldState.GetBlock.IsLightSource(oldState))
                lightSources.Remove(VoxelMath.PackPos32(x, y, z));

            //if block we are adding is light source, then add to light sources
            if (state.GetBlock.IsLightSource(state)) lightSources.Add(VoxelMath.PackPos32(x, y, z));

            //auto-upgrade to ushort
            if (paletteIndex > byte.MaxValue && blockIndices is ByteBlockStorage)
            {
                UpgradeToUShort();
            }

            blockIndices.Set(index, paletteIndex);
        }

        //gets the lighting value of a block safely
        public ushort GetLightSafe(int x, int y, int z, int offsetX, int offsetY, int offsetZ,
        Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            int nx = x + offsetX;
            int ny = YPos * SUBCHUNK_SIZE + y + offsetY; //global Y
            int nz = z + offsetZ;

            //out of world bounds (vertical)
            if (ny < 0 || ny >= 384)
                return ((0 & 0xF) | ((0 & 0xF) << 4) | ((0 & 0xF) << 8) | ((15 & 0xF) << 12));

            //start with this chunk as default
            Chunk? targetChunk = parent;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= SUBCHUNK_SIZE;
            bool nzBack = nz < 0;
            bool nzFront = nz >= SUBCHUNK_SIZE;

            //if diagonal: prefer corner chunks
            if (nxLeft && nzBack)
            {
                targetChunk = c1; //back-left
            }
            else if (nxRight && nzBack)
            {
                targetChunk = c2; //back-right
            }
            else if (nxLeft && nzFront)
            {
                targetChunk = c3; //front-right
            }
            else if (nxRight && nzFront)
            {
                targetChunk = c4; //front-left
            }
            else
            {
                //non-diagonal: choose along X or Z
                if (nxLeft) targetChunk = leftC;
                else if (nxRight) targetChunk = rightC;

                if (nzBack) targetChunk = backC;
                else if (nzFront) targetChunk = frontC;
            }

            if (targetChunk == null || !targetChunk.HasVoxelData())
                return ((0 & 0xF) | ((0 & 0xF) << 4) | ((0 & 0xF) << 8) | ((15 & 0xF) << 12));

            //convert to local coordinates inside target chunk/subchunk
            //this handles -1 -> SUBCHUNK_SIZE-1 mapping etc.
            int localX = ((nx % SUBCHUNK_SIZE) + SUBCHUNK_SIZE) % SUBCHUNK_SIZE;
            int localZ = ((nz % SUBCHUNK_SIZE) + SUBCHUNK_SIZE) % SUBCHUNK_SIZE;

            return targetChunk.GetLight(localX, ny, localZ);
        }

        //helper to fetch neighbor types safely — now supports corner chunks (c1..c4)
        public BlockState GetNeighborBlockSafe(int x, int y, int z, int offsetX, int offsetY, int offsetZ,
        Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            int nx = x + offsetX;
            int ny = YPos * SUBCHUNK_SIZE + y + offsetY; //global Y
            int nz = z + offsetZ;

            //out of world bounds (vertical)
            if (ny < 0 || ny >= 384) return new BlockState(BlockIDs.AIR_BLOCK);

            //start with this chunk as default
            Chunk? targetChunk = parent;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= SUBCHUNK_SIZE;
            bool nzBack = nz < 0;
            bool nzFront = nz >= SUBCHUNK_SIZE;

            //if diagonal: prefer corner chunks
            if (nxLeft && nzBack)
            {
                targetChunk = c1; //back-left
            }
            else if (nxRight && nzBack)
            {
                targetChunk = c2; //back-right
            }
            else if (nxLeft && nzFront)
            {
                targetChunk = c3; //front-right
            }
            else if (nxRight && nzFront)
            {
                targetChunk = c4; //front-left
            }
            else
            {
                //non-diagonal: choose along X or Z
                if (nxLeft) targetChunk = leftC;
                else if (nxRight) targetChunk = rightC;

                if (nzBack) targetChunk = backC;
                else if (nzFront) targetChunk = frontC;
            }

            if (targetChunk == null || !targetChunk.HasVoxelData())
                return new BlockState(BlockIDs.AIR_BLOCK);

            //convert to local coordinates inside target chunk/subchunk
            //this handles -1 -> SUBCHUNK_SIZE-1 mapping etc.
            int localX = ((nx % SUBCHUNK_SIZE) + SUBCHUNK_SIZE) % SUBCHUNK_SIZE;
            int localZ = ((nz % SUBCHUNK_SIZE) + SUBCHUNK_SIZE) % SUBCHUNK_SIZE;

            return targetChunk.GetBlockSafe(localX, ny, localZ);
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