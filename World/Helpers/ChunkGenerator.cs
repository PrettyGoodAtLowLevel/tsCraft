using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Terrain_Generation;
using OurCraft.Terrain_Generation.SurfaceFeatures;

namespace OurCraft.World.Helpers
{
    //helps generate block data of chunks and subchunks using the world generator
    public static class ChunkGenerator
    {
        const int HEIGHT_IN_SUBCHUNKS = 24;
        const int WIDTH_IN_SUBCHUNKS = 2;
        const int CHUNK_HEIGHT = SUBCHUNK_SIZE * HEIGHT_IN_SUBCHUNKS;
        const int CHUNK_WIDTH = SUBCHUNK_SIZE * WIDTH_IN_SUBCHUNKS;
        const int SUBCHUNK_SIZE = 16;

        //fills in all subchunks with block state data
        public static void GenerateBlocks(Chunk chunk)
        {
            if (chunk.GetState() != ChunkState.Initialized) return;
            InitSubChunks(chunk);

            //create noise regions
            NoiseRegion[,] noiseRegions = new NoiseRegion[CHUNK_WIDTH, CHUNK_WIDTH];
            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                for (int x = 0; x < CHUNK_WIDTH; x++)
                {
                    int globalX = chunk.ChunkPos.X * CHUNK_WIDTH + x;
                    int globalZ = chunk.ChunkPos.Z * CHUNK_WIDTH + z;
                    NoiseRegion noiseRegion = WorldGenerator.GetTerrainRegion(globalX, globalZ);
                    noiseRegions[x, z] = noiseRegion;
                }
            }

            //use noise regions & create blocks
            foreach (var subChunk in chunk.SubChunks)
            {
                CreateDensityMap(subChunk, noiseRegions);
            }

            foreach (var subChunk in chunk.SubChunks)
            {
                SurfacePaint(subChunk, noiseRegions);
            }

            foreach (var subChunk in chunk.SubChunks)
            {
                PlaceSurfaceFeatures(subChunk, noiseRegions);
            }

            //do pre lighting stage lighting calculations
            chunk.MaxSolidY = GetChunkMaxSolidY(chunk) + 1;
            InitLightMap(chunk, chunk.MaxSolidY);

            chunk.SetState(ChunkState.VoxelOnly);
        }

        //create base density map - stone vs air
        static void CreateDensityMap(SubChunk subChunk, NoiseRegion[,] noiseRegions)
        {
            //subchunk inside chunk xyz
            int cxp = subChunk.ChunkXPos * SUBCHUNK_SIZE;
            int cyp = subChunk.ChunkYPos * SUBCHUNK_SIZE;
            int czp = subChunk.ChunkZPos * SUBCHUNK_SIZE;

            //chunk coord xz
            int ccx = CHUNK_WIDTH * subChunk.parent.ChunkPos.X;
            int ccz = CHUNK_WIDTH * subChunk.parent.ChunkPos.Z;

            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion noiseRegion = noiseRegions[cxp + x, czp + z];
                    for (int y = 0; y < SUBCHUNK_SIZE; y++)
                    {
                        //create stone-air map
                        int globalX = cxp + x + ccx;
                        int globalY = cyp + y;
                        int globalZ = czp + z + ccz;
                        float density = WorldGenerator.GetDensity(globalX, globalY, globalZ, noiseRegion);

                        BlockState state = WorldGenerator.EmptyBlock;
                        if (density > 0) state = WorldGenerator.WorldBlock;
                        subChunk.SetBlock(x, y, z, state);

                        //fill air blocks below sea level with water
                        if (subChunk.GetBlockState(x, y, z) == Block.AIR && globalY <= WorldGenerator.SEA_LEVEL)
                        {
                            BlockState state2 =
                                globalY == WorldGenerator.SEA_LEVEL ? noiseRegion.biome.WaterSurfaceBlock :
                                noiseRegion.biome.WaterBlock;

                            subChunk.SetBlock(x, y, z, state2);
                        }
                    }
                }
            }
        }

        //this code is atrocious
        static void SurfacePaint(SubChunk subChunk, NoiseRegion[,] noiseRegions)
        {
            //subchunk inside chunk xyz
            int cxp = subChunk.ChunkXPos * SUBCHUNK_SIZE;
            int cyp = subChunk.ChunkYPos * SUBCHUNK_SIZE;
            int czp = subChunk.ChunkZPos * SUBCHUNK_SIZE;

            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion noiseRegion = noiseRegions[cxp + x, czp + z];
                    Biome biome = noiseRegion.biome;
                    for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--) //top-down
                    {
                        BlockState current = subChunk.GetBlockState(x, y, z);

                        BlockState above = (y + 1 < SUBCHUNK_SIZE) ? subChunk.GetBlockState(x, y + 1, z):
                        subChunk.parent.GetBlockUnsafe(cxp + x, y + cyp + 1, czp + z);

                        bool currentEligible = current != Block.AIR && current != biome.WaterBlock && current != biome.WaterSurfaceBlock;
                        bool aboveEligible = above == Block.AIR || above == biome.WaterBlock || above == biome.WaterSurfaceBlock;

                        //only consider blocks with air above
                        if (currentEligible && aboveEligible)
                        {
                            for (int d = 0; d < 5 && (y - d) >= 0; d++)
                            {
                                int targetY = y - d;

                                //customize depth levels for the biomes
                                if (d == 0) subChunk.SetBlock(x, targetY, z, WorldGenerator.GetSurfaceBlock(biome, targetY + cyp));
                                else if (d <= 2) subChunk.SetBlock(x, targetY, z, WorldGenerator.GetSubSurfaceBlock(biome, targetY + cyp));
                            }
                        }
                    }
                }
            }
        }

        //this code is atrocious
        static void PlaceSurfaceFeatures(SubChunk subChunk, NoiseRegion[,] noiseRegions)
        {
            //subchunk inside chunk xyz
            int cxp = subChunk.ChunkXPos * SUBCHUNK_SIZE;
            int cyp = subChunk.ChunkYPos * SUBCHUNK_SIZE;
            int czp = subChunk.ChunkZPos * SUBCHUNK_SIZE;

            //chunk coord xz
            int ccx = CHUNK_WIDTH * subChunk.parent.ChunkPos.X;
            int ccz = CHUNK_WIDTH * subChunk.parent.ChunkPos.Z;
            int seed = NoiseRouter.seed;

            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion noiseRegion = noiseRegions[cxp + x, czp + z];
                    Biome biome = noiseRegion.biome;
                    for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--) // top-down
                    {
                        BlockState current = subChunk.GetBlockState(x, y, z);
                        BlockState above = (y + 1 < SUBCHUNK_SIZE) ? subChunk.GetBlockState(x, y + 1, z):
                        subChunk.parent.GetBlockUnsafe(cxp + x, cyp + y + 1, czp + z);

                        //only consider blocks with air above (i.e., surface or overhang)
                        bool currentEligible = current == WorldGenerator.GetSurfaceBlock(biome, y + cyp);    
                        if (currentEligible)
                        {
                            foreach (BiomeSurfaceFeature feature in biome.SurfaceFeatures)
                            {
                                Vector3i globalCoords = new(cxp + x + ccx, y + cyp, czp + z + ccz);

                                //generate random number with deterministic coordinate hash function (super weird but thread safe)
                                int rand = NoiseRouter.GetStructureRandomness(globalCoords.X, globalCoords.Y, globalCoords.Z, seed, feature.chance);

                                if (rand == 1 && feature.feature.CanPlaceFeature(new Vector3i(cxp + x, y + cyp + 1, czp + z), subChunk.parent))
                                {
                                    feature.feature.PlaceFeature(new Vector3i(cxp + x, y + cyp + 1, czp + z), subChunk.parent);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        static void InitSubChunks(Chunk chunk)
        {
            chunk.DirtySubChunks = new bool[WIDTH_IN_SUBCHUNKS, HEIGHT_IN_SUBCHUNKS, WIDTH_IN_SUBCHUNKS];
            chunk.SubChunks = new SubChunk[WIDTH_IN_SUBCHUNKS, HEIGHT_IN_SUBCHUNKS, WIDTH_IN_SUBCHUNKS];

            for (int x = 0; x < WIDTH_IN_SUBCHUNKS; x++)
            {
                for (int y = 0; y < HEIGHT_IN_SUBCHUNKS; y++)
                {
                    for (int z = 0; z < WIDTH_IN_SUBCHUNKS; z++)
                    {
                        chunk.SubChunks[x, y, z] = new SubChunk(chunk, x, y, z);
                    }
                }
            }
        }

        //scans chunk from top down to find the first y layer with blocks
        static int GetChunkMaxSolidY(Chunk chunk)
        {
            const int max = CHUNK_HEIGHT - 1;

            for (int sy = HEIGHT_IN_SUBCHUNKS - 1; sy >= 0; sy--)
            {
                for (int sx = 0; sx < WIDTH_IN_SUBCHUNKS; sx++)
                {
                    for (int sz = 0; sz < WIDTH_IN_SUBCHUNKS; sz++)
                    {
                        SubChunk subChunk = chunk.SubChunks[sx, sy, sz];
                        if (subChunk.IsAllAir()) continue;
                        return ScanSubChunkLayer(subChunk) + 1;
                    }
                }
            }

            return max;
        }

        //helper for getting max solid y
        static int ScanSubChunkLayer(SubChunk subChunk)
        {
            for (int y = SubChunk.SUBCHUNK_SIZE - 1; y >= 0; y--)
            {
                for (int x = 0; x < SubChunk.SUBCHUNK_SIZE; x++)
                {
                    for (int z = 0; z < SubChunk.SUBCHUNK_SIZE; z++)
                    {
                        BlockState state = subChunk.GetBlockState(x, y, z);
                        if (state != Block.AIR)
                        {
                            return y + subChunk.ChunkYPos * SubChunk.SUBCHUNK_SIZE;
                        }
                    }
                }
            }
            return -1;
        }

        static void InitLightMap(Chunk chunk, int maxY)
        {
            chunk.lightMap = new ushort[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

            const int sky0 = 0;
            const ushort sky15 = 0 & 0xF | (0 & 0xF) << 4 | (0 & 0xF) << 8 | (15 & 0xF) << 12;

            for (int x = 0; x < CHUNK_WIDTH; x++)
            {
                for (int z = 0; z < CHUNK_WIDTH; z++)
                {
                    for (int y = 0; y < CHUNK_HEIGHT; y++)
                    {
                        if (y < maxY) chunk.lightMap[x, y, z] = sky0;
                        //if above all solid blocks, set to max sky instead
                        else chunk.lightMap[x, y, z] = sky15;
                    }
                }
            }
        }
    }
}
