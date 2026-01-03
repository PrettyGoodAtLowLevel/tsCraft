using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
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
        public const int HEIGHT_IN_SUBCHUNKS = 24;
        public const int WIDTH_IN_SUBCHUNKS = 2;
        public const int CHUNK_HEIGHT = SubChunk.SUBCHUNK_SIZE * HEIGHT_IN_SUBCHUNKS;
        public const int CHUNK_WIDTH = SubChunk.SUBCHUNK_SIZE * WIDTH_IN_SUBCHUNKS;

        //rendering
        public readonly ChunkMesh batchedMesh;
        public readonly ChunkMesh transparentMesh;

        //positioning
        public ChunkCoord ChunkPos { get; private set; }
        public Vector3d ChunkMin { get; private set; }
        public Vector3d ChunkMax { get; private set; }
        public Vector3d WorldPos { get; private set; }

        //block and generation data
        public SubChunk[,,] subChunks = new SubChunk[0, 0, 0];
        public ushort[,,] lightMap = new ushort[0, 0, 0];
        public int MaxSolidY { get; private set; } = CHUNK_HEIGHT - 1; //highest y layer with only air blocks
        volatile ChunkState state;
        public volatile bool meshing = false;
        public List<Vector3i> changes = [];
        private bool[,,] subChunkDirty = new bool[0, 0, 0];       

        //basic constructor
        public Chunk(ChunkCoord coord)
        {         
            ChunkPos = coord;
            state = ChunkState.Initialized;
            batchedMesh = new ChunkMesh();
            transparentMesh = new ChunkMesh();
            ChunkMin = new Vector3d(ChunkPos.X * CHUNK_WIDTH, 0, ChunkPos.Z * CHUNK_WIDTH);
            ChunkMax = ChunkMin + new Vector3d(CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH);
        }

        //clears batched mesh gl object data
        public void Delete()
        {
            state = ChunkState.Deleted;
            batchedMesh.Delete();
            transparentMesh.Delete();
        }

        //scans chunk from top down to find the first y layer with blocks
        public int GetMaxSolidY()
        {
            const int max = CHUNK_HEIGHT - 1;

            for(int sy = HEIGHT_IN_SUBCHUNKS - 1; sy >= 0; sy--)
            {
                for(int sx = 0; sx < WIDTH_IN_SUBCHUNKS; sx++)
                {
                    for (int sz = 0; sz < WIDTH_IN_SUBCHUNKS; sz++)
                    {
                        SubChunk subChunk = subChunks[sx, sy, sz];
                        if (subChunk.IsAllAir()) continue;
                        return ScanSubChunkLayer(subChunk) + 1;
                    }
                }
            }

            return max;
        }

        //helper for getting max solid y
        public static int ScanSubChunkLayer(SubChunk subChunk)
        {
            for(int y = SubChunk.SUBCHUNK_SIZE - 1; y >= 0; y--)
            {
                for(int x = 0; x < SubChunk.SUBCHUNK_SIZE; x++)
                {
                    for(int z = 0; z < SubChunk.SUBCHUNK_SIZE; z++)
                    {
                        BlockState state = subChunk.GetBlockState(x, y, z);
                        if (state != Block.AIR)
                        {
                            return y + (subChunk.ChunkYPos * SubChunk.SUBCHUNK_SIZE);
                        }
                    }
                }
            }
            return -1;
        }

        //set all lightmap values to default
        public void InitLightMap()
        {
            lightMap = new ushort[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

            const int sky0 = 0;
            const ushort sky15 = ((0 & 0xF) | ((0 & 0xF) << 4) | ((0 & 0xF) << 8) | ((15 & 0xF) << 12));

            for (int x = 0; x < CHUNK_WIDTH; x++)
            {
                for (int z = 0; z < CHUNK_WIDTH; z++)
                {
                    for (int y = 0; y < CHUNK_HEIGHT; y++)
                    {
                        if (y < MaxSolidY) lightMap[x, y, z] = sky0;
                        else lightMap[x, y, z] = sky15;
                    }
                }
            }            
        }

        //fills in all subchunks with block state data
        public void CreateVoxelMap()
        {          
            if (state != ChunkState.Initialized) return;

            //initalize
            WorldPos = new Vector3d(ChunkPos.X * CHUNK_WIDTH, 0, ChunkPos.Z * CHUNK_WIDTH);

            //initialize subchunks
            subChunkDirty = new bool[WIDTH_IN_SUBCHUNKS, HEIGHT_IN_SUBCHUNKS, WIDTH_IN_SUBCHUNKS];
            subChunks = new SubChunk[WIDTH_IN_SUBCHUNKS, HEIGHT_IN_SUBCHUNKS, WIDTH_IN_SUBCHUNKS];
            for (int x = 0; x < WIDTH_IN_SUBCHUNKS; x++)
            {
                for (int y = 0; y < HEIGHT_IN_SUBCHUNKS; y++)
                {
                    for (int z = 0; z < WIDTH_IN_SUBCHUNKS; z++)
                    {
                        subChunks[x, y, z] = new SubChunk(this, x, y, z);
                    }
                }
            }

            //create noise regions
            NoiseRegion[,] noiseRegions = new NoiseRegion[CHUNK_WIDTH, CHUNK_WIDTH];
            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                for (int x = 0; x < CHUNK_WIDTH; x++)
                {
                    int globalX =  ChunkPos.X * CHUNK_WIDTH + x;
                    int globalZ = ChunkPos.Z * CHUNK_WIDTH + z;
                    NoiseRegion noiseRegion = WorldGenerator.GetTerrainRegion(globalX, globalZ);
                    noiseRegions[x, z] = noiseRegion;
                }
            }

            foreach(var subChunk in subChunks)
            {
                subChunk.CreateDensityMap(noiseRegions);
            }

            foreach(var subChunk in subChunks)
            {
                subChunk.SurfacePaint(noiseRegions);
            }

            foreach (var subChunk in subChunks)
            {
                subChunk.PlaceSurfaceFeatures(noiseRegions);
            }

            //do pre lighting stage lighting calculations
            MaxSolidY = GetMaxSolidY() + 1;
            InitLightMap();

            //update state
            state = ChunkState.VoxelOnly;
        }

        //creates all subchunk mesh data
        public void CreateChunkMesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
        Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            if (!HasVoxelData()) return;
            meshing = true;
            foreach (var subChunk in subChunks)
            {
                subChunk.ClearMesh();
            }

            foreach (var subChunk in subChunks)
            {
                subChunk.CreateMeshData(leftC, rightC, frontC, backC, c1, c2, c3, c4);
            }

            foreach (var subChunk in subChunks)
            {
                subChunk.SolidGeo.vertices.TrimExcess();
                subChunk.TransparentGeo.vertices.TrimExcess();
            }

            if (state == ChunkState.VoxelOnly) state = ChunkState.Meshed;
            meshing = false;
        }

        //builds the combined subchunk mesh data
        public void SendMeshToOpenGL()
        {
            if (state == ChunkState.Deleted || state == ChunkState.VoxelOnly || state == ChunkState.Initialized || meshing)
                return;

            UploadBatchedMesh(batchedMesh, transparent: false);
            UploadBatchedMesh(transparentMesh, transparent: true);

            state = ChunkState.Built;
        }

        //combines vertex data of subchunks into one big openGL mesh
        private void UploadBatchedMesh(ChunkMesh mesh, bool transparent = false)
        {
            //compute size upfront
            int totalVertexCount = 0;

            foreach(var subChunk in subChunks)
            {
                if (transparent)
                {
                    totalVertexCount += subChunk.TransparentGeo.vertices.Count;
                }
                else
                {
                    totalVertexCount += subChunk.SolidGeo.vertices.Count;
                }
            }

            batchedMesh.SetupIndices(totalVertexCount);
            transparentMesh.SetupIndices(totalVertexCount);

            //preallocate size
            var totalVertices = new List<BlockVertex>(totalVertexCount);

            foreach(var subChunk in subChunks)
            {
                if (transparent)
                {
                    ChunkMeshData geo = subChunk.TransparentGeo;
                    if (geo.vertices.Count == 0) continue;

                    //append vertices
                    totalVertices.AddRange(geo.vertices);
                }
                else
                {
                    ChunkMeshData geo = subChunk.SolidGeo;
                    if (geo.vertices.Count == 0) continue;

                    //append vertices
                    totalVertices.AddRange(geo.vertices);
                }
            }

            //mesh upload
            mesh.SetupMesh(totalVertices);
        }      

        //draws all the subchunk opaque meshes
        public void Draw(Shader shader, Camera cam)
        {
            batchedMesh.Draw(shader, WorldPos, cam);       
        }

        //draws all of the transparent stuff in a subchunk
        public void DrawTransparent(Shader shader, Camera cam)
        {
            transparentMesh.Draw(shader, WorldPos, cam);
        }

        //set chunk ready to delete
        public void MarkForDeletion()
        {
            state = ChunkState.Deleted;
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
        public bool Modifiyable()
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
            //fast modulus math
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            if (HasVoxelData() == false || globalY < 0 || globalY >= CHUNK_HEIGHT) return Block.AIR;           

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY & sb;

            int subChunkX = x / SubChunk.SUBCHUNK_SIZE;
            int localX = x & sb;

            int subChunkZ = z / SubChunk.SUBCHUNK_SIZE;
            int localZ = z & sb;

            return subChunks[subChunkX, subChunkY, subChunkZ].GetBlockState(localX, localY, localZ);
        }

        //instantly gets a block from a chunk, may fail
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

            return subChunks[subChunkX, subChunkY, subChunkZ].GetBlockState(localX, localY, localZ);
        }

        //sets block and sets chunk as dirty
        public void SetBlock(int x, int globalY, int z, BlockState state)
        {
            //fast modulus math
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            if (HasVoxelData() == false || globalY < 0 || globalY > CHUNK_HEIGHT - 1) return;
            changes.Add(new Vector3i(x, globalY, z));

            //get subchunk position
            int subChunkY = globalY / SubChunk.SUBCHUNK_SIZE;
            int localY = globalY & sb;

            int subChunkX = x / SubChunk.SUBCHUNK_SIZE;
            int localX = x & sb;

            int subChunkZ = z / SubChunk.SUBCHUNK_SIZE;
            int localZ = z & sb;

            subChunks[subChunkX, subChunkY, subChunkZ].SetBlock(localX, localY, localZ, state);
        }

        //unsafe setblock, no checks before hand, doesnt mark chunk pos as dirty
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

            subChunks[subChunkX, subChunkY, subChunkZ].SetBlock(localX, localY, localZ, state);
        }

        //gives back skylight and block light from local position
        public ushort GetLight(int x, int y, int z)
        {            
            if (HasVoxelData() == false || !PosValid(x, y, z)) return 0;       
            return lightMap[x, y, z];
        }

        //sets a whole light value position, dont use this unless needed
        public void SetLight(int x, int y, int z, ushort value)
        {
            if (HasVoxelData() == false || !PosValid(x, y, z)) return;          
            lightMap[x, y, z] = value;
        }

        //sets only the block rgb colors at a light value position
        public void SetBlockLight(int x, int y, int z, Vector3i value)
        {
            if (HasVoxelData() == false || !PosValid(x, y, z)) return;

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

        //remesh subchunks that are effected by a block being broken or placed
        public void Remesh(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            for (int i = 0; i < changes.Count; i++)
            {
                var p = changes[i];

                int subX = p.X / SubChunk.SUBCHUNK_SIZE;
                int subY = p.Y / SubChunk.SUBCHUNK_SIZE;
                int subZ = p.Z / SubChunk.SUBCHUNK_SIZE;

                int localX = p.X & sb; int localY = p.Y & sb; int localZ = p.Z & sb;

                //mark owning subchunk
                if ((uint)subX < WIDTH_IN_SUBCHUNKS && (uint)subY < HEIGHT_IN_SUBCHUNKS && (uint)subZ < WIDTH_IN_SUBCHUNKS) subChunkDirty[subX, subY, subZ] = true;

                //X neighbors
                if (localX == sb && subX + 1 < WIDTH_IN_SUBCHUNKS) subChunkDirty[subX + 1, subY, subZ] = true;
                if (localX == 0 && subX > 0) subChunkDirty[subX - 1, subY, subZ] = true;

                //Y neighbors
                if (localY == sb && subY + 1 < HEIGHT_IN_SUBCHUNKS) subChunkDirty[subX, subY + 1, subZ] = true;
                if (localY == 0 && subY > 0) subChunkDirty[subX, subY - 1, subZ] = true;

                //Z neighbors
                if (localZ == sb && subZ + 1 < WIDTH_IN_SUBCHUNKS) subChunkDirty[subX, subY, subZ + 1] = true;
                if (localZ == 0 && subZ > 0) subChunkDirty[subX, subY, subZ - 1] = true;
            }

            //remesh dirty subchunks only once and reupload mesh
            for (int x = 0; x < WIDTH_IN_SUBCHUNKS; x++)
            {
                for (int y = 0; y < HEIGHT_IN_SUBCHUNKS; y++)
                {
                    for (int z = 0; z < WIDTH_IN_SUBCHUNKS; z++)
                    {
                        if (!subChunkDirty[x, y, z]) continue;
                        subChunks[x, y, z].Remesh(leftC, rightC, frontC, backC, c1, c2, c3, c4);
                        subChunkDirty[x, y, z] = false;
                    }
                }
            }
            changes.Clear();
            SendMeshToOpenGL();
        }

        //debug and helpers
        //checks if a position fits in a chunk, for many axis
        public static bool PosValid(int x, int y, int z)
        {
            return x >= 0 && x < CHUNK_WIDTH && z >= 0 && z < CHUNK_WIDTH && y >= 0 && y < CHUNK_HEIGHT;
        }

        //overload only for x and z layers of a chunk
        public static bool PosValid(int x, int z)
        {
            return x >= 0 && x < CHUNK_WIDTH && z >= 0 && z < CHUNK_WIDTH;
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
        public const int SUBCHUNK_SIZE = 16;
        public const int CHUNK_WIDTH = SUBCHUNK_SIZE * Chunk.WIDTH_IN_SUBCHUNKS;

        //positioning and parent data
        public int ChunkXPos { get; private set; } = 0;
        public int ChunkYPos { get; private set; } = 0;
        public int ChunkZPos { get; private set; } = 0;
        readonly Chunk parent;

        //block storage
        private IBlockIndexStorage blockIndices;
        private readonly BlockPalette palette;
        public List<ushort> lightSources = [];             

        //mesh data
        public ChunkMeshData SolidGeo { get; private set; }
        public ChunkMeshData TransparentGeo { get; private set; }

        //basic constructor
        public SubChunk(Chunk parent, int xPos, int yPos, int zPos)
        { 
            this.parent = parent;
            ChunkXPos = xPos;
            ChunkYPos = yPos;           
            ChunkZPos = zPos;

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
                    NoiseRegion noiseRegion = noiseRegions[(ChunkXPos * SUBCHUNK_SIZE) + x, (ChunkZPos * SUBCHUNK_SIZE) + z];
                    for (int y = 0; y < SUBCHUNK_SIZE; y++)
                    {   
                        //create stone-air map
                        int globalX = (ChunkXPos * SUBCHUNK_SIZE) + x + (CHUNK_WIDTH * parent.ChunkPos.X);
                        int globalY = (ChunkYPos * SUBCHUNK_SIZE) + y;
                        int globalZ = (ChunkZPos * SUBCHUNK_SIZE) + z + (CHUNK_WIDTH * parent.ChunkPos.Z);
                        float density = WorldGenerator.GetDensity(globalX, globalY, globalZ, noiseRegion);

                        BlockState state = WorldGenerator.EmptyBlock;                
                        if (density > 0) state = WorldGenerator.WorldBlock;
                        SetBlock(x, y, z, state);

                        //fill air blocks below sea level with water
                        if (GetBlockState(x, y, z) == Block.AIR && globalY <= WorldGenerator.SEA_LEVEL)
                        {
                            BlockState state2 = 
                                globalY == WorldGenerator.SEA_LEVEL ? noiseRegion.biome.WaterSurfaceBlock:
                                noiseRegion.biome.WaterBlock;

                            SetBlock(x, y, z, state2);
                        }
                    }
                }
            }
        }

        //this code is atrocious
        public void SurfacePaint(NoiseRegion[,] noiseRegions)
        { 
            for(int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                for(int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion noiseRegion = noiseRegions[(ChunkXPos * SUBCHUNK_SIZE) + x, (ChunkZPos * SUBCHUNK_SIZE) + z];

                    for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--) // top-down
                    {
                        BlockState current = GetBlockState(x, y, z);
                        BlockState above = (y + 1 < SUBCHUNK_SIZE) ? GetBlockState(x, y + 1, z) : parent.GetBlockUnsafe((ChunkXPos * SUBCHUNK_SIZE) + x, (y + ChunkYPos * SUBCHUNK_SIZE) + 1, (ChunkZPos * SUBCHUNK_SIZE) + z);

                        bool currentEligible = current != Block.AIR && current != noiseRegion.biome.WaterBlock && current != noiseRegion.biome.WaterSurfaceBlock;
                        bool aboveEligible = above == Block.AIR || above == noiseRegion.biome.WaterBlock || above == noiseRegion.biome.WaterSurfaceBlock;

                        //only consider blocks with air above (i.e., surface or overhang)
                        if (currentEligible && aboveEligible)
                        {
                            for (int d = 0; d < 5 && (y - d) >= 0; d++) //the loop values are customizable
                            {
                                int targetY = y - d;

                                //customize depth levels for the biomes
                                if (d == 0) SetBlock(x, targetY, z, WorldGenerator.GetSurfaceBlock(noiseRegion.biome, targetY + ChunkYPos * SUBCHUNK_SIZE));
                                else if (d <= 2) SetBlock(x, targetY, z, WorldGenerator.GetSubSurfaceBlock(noiseRegion.biome, targetY + ChunkYPos * SUBCHUNK_SIZE));                               
                            }
                        }
                    }
                }
            }
        }

        //this code is atrocious
        public void PlaceSurfaceFeatures(NoiseRegion[,] noiseRegions)
        {
            int seed = NoiseRouter.seed;
            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion noiseRegion = noiseRegions[(ChunkXPos * SUBCHUNK_SIZE) + x, (ChunkZPos * SUBCHUNK_SIZE) + z];

                    for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--) // top-down
                    {
                        BlockState current = GetBlockState(x, y, z);
                        BlockState above = (y + 1 < SUBCHUNK_SIZE) ? GetBlockState(x, y + 1, z) : parent.GetBlockUnsafe((ChunkXPos * SUBCHUNK_SIZE) + x, (ChunkYPos * SUBCHUNK_SIZE) + y + 1, (ChunkZPos * SUBCHUNK_SIZE) + z);

                        bool currentEligible = current == WorldGenerator.GetSurfaceBlock(noiseRegion.biome, y + (ChunkYPos * SUBCHUNK_SIZE));
                        bool aboveEligible = above == Block.AIR;

                        //only consider blocks with air above (i.e., surface or overhang)
                        if (currentEligible && aboveEligible)
                        {
                            foreach (BiomeSurfaceFeature feature in noiseRegion.biome.SurfaceFeatures)
                            {
                                Vector3i globalCoords = new((ChunkXPos * SUBCHUNK_SIZE) + x+ (CHUNK_WIDTH * parent.ChunkPos.X),
                                (y + ChunkYPos * SUBCHUNK_SIZE),
                                (ChunkZPos * SUBCHUNK_SIZE) + z + (CHUNK_WIDTH * parent.ChunkPos.Z));

                                //generate random number with deterministic coordinate hash function (super weird but thread safe)
                                int rand = NoiseRouter.GetStructureRandomness(globalCoords.X, globalCoords.Y, globalCoords.Z, seed, feature.chance);

                                if (rand == 1 && feature.feature.CanPlaceFeature(new Vector3i((ChunkXPos * SUBCHUNK_SIZE) + x, y + (ChunkYPos * SUBCHUNK_SIZE) + 1, (ChunkZPos * SUBCHUNK_SIZE) + z), parent))
                                {
                                    feature.feature.PlaceFeature(new Vector3i((ChunkXPos * SUBCHUNK_SIZE) + x, (y + ChunkYPos * SUBCHUNK_SIZE) + 1, (ChunkZPos * SUBCHUNK_SIZE) + z), parent);
                                    break;
                                }                                  
                            }                            
                        }
                    }
                }
            }
        }

        //tries to create the mesh data
        public void CreateMeshData(Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            if (IsAllAir()) return; //dont create mesh if chunk is completely air

            for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--)
            {
                for (int x = SUBCHUNK_SIZE - 1; x >= 0; x--)
                {
                    for (int z = SUBCHUNK_SIZE - 1; z >= 0; z--)
                    {
                        AddMeshDataToChunk(new Vector3i(x, y, z), new Vector3((ChunkXPos * SUBCHUNK_SIZE) + x, (ChunkYPos * SUBCHUNK_SIZE) + y, (ChunkZPos * SUBCHUNK_SIZE) + z),
                        leftC, rightC, frontC, backC, c1, c2, c3, c4);
                    }
                }
            }
        }

        //tries to add face data to a chunk mesh based on a bitmask of the adjacent blocks also samples lighting values for blocks exposed to light
        public void AddMeshDataToChunk(Vector3i pos, Vector3 meshPos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            BlockState state = GetBlockState(pos.X, pos.Y, pos.Z);
            if (state == Block.AIR) return;
            Block block = BlockData.GetBlock(state.BlockID);

            //get neighbor blocks
            NeighborBlocks nb = GetNeighborsSafe(pos, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            nb.thisState = state;

            //if surrounding blocks are all full solid cubes, and the current block is also full solid, then skip meshing entirely
            if (nb.top.BlockShape.IsFullOpaqueBlock && nb.bottom.BlockShape.IsFullOpaqueBlock
            && nb.front.BlockShape.IsFullOpaqueBlock && nb.back.BlockShape.IsFullOpaqueBlock &&
            nb.left.BlockShape.IsFullOpaqueBlock && nb.right.BlockShape.IsFullOpaqueBlock
            && block.blockShape.IsFullOpaqueBlock) return;
            
            LightingData lightData = GetLightData(pos, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            ChunkMeshData meshRef = block.blockShape.IsTranslucent ? TransparentGeo : SolidGeo;
            block.blockShape.AddBlockMesh(meshPos, nb, meshRef, lightData);           
        }

        //get all neighbor blocks in a safe fashion
        private NeighborBlocks GetNeighborsSafe(Vector3i pos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
        Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            NeighborBlocks nb = new();
            int x = pos.X;
            int y = pos.Y;
            int z = pos.Z;

            //helper to get neighbor block safely
            BlockState N(int ox, int oy, int oz) => GetNeighborBlockSafe(x, y, z, ox, oy, oz, leftC, rightC, frontC, backC, c1, c2, c3, c4);

            //top face + top corners
            nb.top = N(0, +1, 0);
            nb.bottom = N(0, -1, 0);
            nb.front = N(0, 0, +1);
            nb.back = N(0, 0, -1);
            nb.right = N(1, 0, 0);
            nb.left = N(-1, 0, 0);
            return nb;
        }

        //returns the values necessary for computing the light values for meshing
        private LightingData GetLightData(Vector3i pos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
        Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            LightingData ld = new();
            int x = pos.X;
            int y = pos.Y;
            int z = pos.Z;

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
            CreateMeshData(leftC, rightC, frontC, backC, c1, c2, c3, c4);
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

        //gets if the subchunk is all air or not
        public bool IsAllAir()
        {
            if (palette.Count > 1) return false;

            if(palette.entryMap.TryGetValue(Block.AIR, out int value))
            {
                return true;
            }
            return false;
        }

        //gets the lighting value of a block safely
        public ushort GetLightSafe(int x, int y, int z, int offsetX, int offsetY, int offsetZ,
        Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            const ushort defaultLight = ((0 & 0xF) | ((0 & 0xF) << 4) | ((0 & 0xF) << 8) | ((15 & 0xF) << 12));
            const int cs = CHUNK_WIDTH - 1;

            int nx = (ChunkXPos * SUBCHUNK_SIZE) + x + offsetX;
            int ny = (ChunkYPos * SUBCHUNK_SIZE) + y + offsetY; //global Y
            int nz = (ChunkZPos * SUBCHUNK_SIZE) + z + offsetZ;
           
            //out of world bounds (vertical)
            if (ny < 0 || ny >= Chunk.CHUNK_HEIGHT) return defaultLight;
            if ((uint)nx < CHUNK_WIDTH && (uint)nz < CHUNK_WIDTH) return parent.GetLight(nx, ny, nz);

            //start with this chunk as default
            Chunk? targetChunk = parent;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= CHUNK_WIDTH;
            bool nzBack = nz < 0;
            bool nzFront = nz >= CHUNK_WIDTH;

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

            if (targetChunk == null || !targetChunk.HasVoxelData()) return defaultLight;

            //convert to local coordinates inside target chunk
            int localX = ((nx & cs) + CHUNK_WIDTH) & cs;
            int localZ = ((nz & cs) + CHUNK_WIDTH) & cs;

            return targetChunk.GetLight(localX, ny, localZ);
        }

        //helper to fetch neighbor types safely — now supports corner chunks (c1..c4)
        public BlockState GetNeighborBlockSafe(int x, int y, int z, int offsetX, int offsetY, int offsetZ,
        Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            const int cs = CHUNK_WIDTH - 1;

            int nx = (ChunkXPos * SUBCHUNK_SIZE) + x + offsetX;
            int ny = (ChunkYPos * SUBCHUNK_SIZE) + y + offsetY; //global Y
            int nz = (ChunkZPos * SUBCHUNK_SIZE) + z + offsetZ;

            //out of world bounds (vertical)
            if (ny < 0 || ny >= Chunk.CHUNK_HEIGHT) return Block.AIR;
            if ((uint)nx < CHUNK_WIDTH && (uint)nz < CHUNK_WIDTH) return parent.GetBlockUnsafe(nx, ny, nz);
            
            //start with this chunk as default
            Chunk? targetChunk = parent;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= CHUNK_WIDTH;
            bool nzBack = nz < 0;
            bool nzFront = nz >= CHUNK_WIDTH;

            //if diagonal: prefer corner chunks
            if (nxLeft && nzBack) targetChunk = c1; //back-left           
            else if (nxRight && nzBack) targetChunk = c2; //back-right           
            else if (nxLeft && nzFront) targetChunk = c3; //front-right        
            else if (nxRight && nzFront) targetChunk = c4; //front-left           
            else
            {
                //non-diagonal: choose along X or Z
                if (nxLeft) targetChunk = leftC;
                else if (nxRight) targetChunk = rightC;

                if (nzBack) targetChunk = backC;
                else if (nzFront) targetChunk = frontC;
            }

            if (targetChunk == null || !targetChunk.HasVoxelData()) return Block.AIR;

            //convert to local coordinates inside target chunk
            int localX = ((nx & cs) + CHUNK_WIDTH) & cs;
            int localZ = ((nz & cs) + CHUNK_WIDTH) & cs;

            return targetChunk.GetBlockUnsafe(localX, ny, localZ);
        }

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