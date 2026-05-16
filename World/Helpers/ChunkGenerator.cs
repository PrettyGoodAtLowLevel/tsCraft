using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Terrain_Generation;
using OurCraft.Utility;
using System.Diagnostics;

namespace OurCraft.World.Helpers
{
    //helps generate block data of chunks and subchunks using the world generator
    public static class ChunkGenerator
    {
        const int HEIGHT_IN_SUBCHUNKS = WorldConstants.CHUNK_HEIGHT_IN_SUBCHUNKS;
        const int WIDTH_IN_SUBCHUNKS = WorldConstants.CHUNK_WIDTH_IN_SUBCHUNKS;
        const int CHUNK_HEIGHT = WorldConstants.CHUNK_HEIGHT;
        const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;
        const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE;

        public const int INTERP_STEP = 2; //sample every 2 blocks → 9 points per axis (0,4,8,12,16)
        public const int INTERP_GRID = (SUBCHUNK_SIZE / INTERP_STEP) + 1; //9

        //fills in all subchunks with block state data
        public static void CreateTerrain(Chunk chunk)
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
            foreach (var subChunk in chunk.SubChunks) CreateDensityMap(subChunk, noiseRegions);
            foreach (var subChunk in chunk.SubChunks) SurfacePaint(subChunk, noiseRegions);           
            StructureStart(chunk, noiseRegions);

            chunk.SetState(ChunkState.StructureReady);
            chunk.generating = false;
        }

        //sorts all intersected structures within a chunk, and places the intersected blocks
        public static void PlaceStructures(Chunk target, ChunkManager world, int radius = 1)
        {
            var featuresToPlace = CollectStructures(target, world, radius);
            SortStructures(featuresToPlace);

            //place all features
            foreach (var pf in featuresToPlace) pf.feature.PlaceFeature(pf.startPos, target, world);

            //init lightmap
            target.MaxSolidY = GetChunkMaxSolidY(target) + 2;
            InitLightMap(target, target.MaxSolidY);

            target.SetState(ChunkState.StructuresPlaced);
        }

        //create base density map - stone vs air
        static void CreateDensityMap(SubChunk subChunk, NoiseRegion[,] noiseRegions)
        {
            int cxp = subChunk.ChunkXPos * SUBCHUNK_SIZE;
            int cyp = subChunk.ChunkYPos * SUBCHUNK_SIZE;
            int czp = subChunk.ChunkZPos * SUBCHUNK_SIZE;

            int ccx = CHUNK_WIDTH * subChunk.parent.ChunkPos.X;
            int ccz = CHUNK_WIDTH * subChunk.parent.ChunkPos.Z;

            float[] densityGrid = CreateDensityGrid(noiseRegions, cxp, cyp, czp, ccx, ccz);
            InterpolateCells(subChunk, densityGrid, noiseRegions, cxp, cyp, czp);
        }

        //paints the surface blocks with biome surface blocks
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
                        BlockState above = (y + 1 < SUBCHUNK_SIZE) ? subChunk.GetBlockState(x, y + 1, z) :
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

        //plants structure starts in a chunk
        static void StructureStart(Chunk chunk, NoiseRegion[,] noiseRegions)
        {
            int ccx = CHUNK_WIDTH * chunk.ChunkPos.X;
            int ccz = CHUNK_WIDTH * chunk.ChunkPos.Z;
            int seed = NoiseRouter.seed;

            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                for (int x = 0; x < CHUNK_WIDTH; x++)
                {
                    Biome biome = noiseRegions[x, z].biome;

                    for (int y = CHUNK_HEIGHT - 1; y >= 0; y--)
                    {
                        BlockState current = chunk.GetBlockUnsafe(x, y, z);
                        BlockState above = (y + 1 < CHUNK_HEIGHT) ? chunk.GetBlockUnsafe(x, y + 1, z) : Block.AIR;

                        if (current != WorldGenerator.GetSurfaceBlock(biome, y) || above != Block.AIR) continue;

                        Vector3i worldPos = new(x + ccx, y, z + ccz);
                        foreach (BiomeSurfaceFeature feature in biome.features)
                        {
                            int rand = NoiseRouter.GetStructureRandomness(worldPos.X, worldPos.Y, worldPos.Z, seed, feature.chance);

                            if (rand == 1 && feature.feature.CanPlaceFeature(worldPos + Vector3i.UnitY, chunk))
                            {
                                chunk.placedFeatures.Add(new PlacedFeature(feature.feature, worldPos + Vector3i.UnitY));
                                break;
                            }
                        }
                    }
                }
            }
        }

        //creates a coarse grid using density noise sampling
        static float[] CreateDensityGrid(NoiseRegion[,] noiseRegions, int cxp, int cyp, int czp, int ccx, int ccz)
        {
            const int GRID = INTERP_GRID;
            float[] densityGrid = new float[GRID * GRID * GRID];

            //build coarse density grid
            for (int gz = 0; gz < GRID; gz++)
            {
                int sampleZ = czp + gz * INTERP_STEP;

                for (int gx = 0; gx < GRID; gx++)
                {
                    int sampleX = cxp + gx * INTERP_STEP;

                    NoiseRegion nr = noiseRegions[Math.Min(sampleX, noiseRegions.GetLength(0) - 1), Math.Min(sampleZ, noiseRegions.GetLength(1) - 1)];

                    int globalX = sampleX + ccx;
                    int globalZ = sampleZ + ccz;

                    for (int gy = 0; gy < GRID; gy++)
                    {
                        int globalY = cyp + gy * INTERP_STEP;
                        float density;

                        if (globalY > WorldGenerator.MAX_HEIGHT) density = -1f;
                        else if (globalY < WorldGenerator.MIN_HEIGHT) density = 1f;
                        else
                        {
                            float surfaceDist = nr.heightOffset - globalY;

                            if (surfaceDist < -nr.maxDepth) density = -1f;
                            else if (surfaceDist > nr.maxDepth) density = 1f;
                            else density = WorldGenerator.GetDensity(globalX, globalY, globalZ, nr);
                        }

                        densityGrid[VoxelMath.GridIndex(gx, gy, gz)] = density;
                    }
                }
            }

            return densityGrid;
        }

        //interpolates the generated density grid and sets blocks
        static void InterpolateCells(SubChunk subChunk, float[] grid, NoiseRegion[,] noiseRegions, int cxp, int cyp, int czp)
        {
            const int GRID = INTERP_GRID;
            int step = INTERP_STEP;

            int solidIndex = subChunk.palette.GetOrAddIndex(WorldGenerator.WorldBlock);
            int airIndex = subChunk.palette.GetOrAddIndex(WorldGenerator.EmptyBlock);

            for (int gz = 0; gz < GRID - 1; gz++)
            {
                for (int gx = 0; gx < GRID - 1; gx++)
                {
                    for (int gy = 0; gy < GRID - 1; gy++)
                    {
                        //corner densities
                        float c000 = grid[VoxelMath.GridIndex(gx, gy, gz)];
                        float c100 = grid[VoxelMath.GridIndex(gx + 1, gy, gz)];
                        float c010 = grid[VoxelMath.GridIndex(gx, gy + 1, gz)];
                        float c110 = grid[VoxelMath.GridIndex(gx + 1, gy + 1, gz)];

                        float c001 = grid[VoxelMath.GridIndex(gx, gy, gz + 1)];
                        float c101 = grid[VoxelMath.GridIndex(gx + 1, gy, gz + 1)];
                        float c011 = grid[VoxelMath.GridIndex(gx, gy + 1, gz + 1)];
                        float c111 = grid[VoxelMath.GridIndex(gx + 1, gy + 1, gz + 1)];

                        //fill the interpolation cell
                        for (int z = 0; z < step; z++)
                        {
                            float tz = z / (float)step;

                            float z00 = VoxelMath.Lerp(c000, c001, tz);
                            float z10 = VoxelMath.Lerp(c100, c101, tz);
                            float z01 = VoxelMath.Lerp(c010, c011, tz);
                            float z11 = VoxelMath.Lerp(c110, c111, tz);

                            for (int y = 0; y < step; y++)
                            {
                                float ty = y / (float)step;

                                float y0 = VoxelMath.Lerp(z00, z01, ty);
                                float y1 = VoxelMath.Lerp(z10, z11, ty);

                                float density = y0;
                                float densityStep = (y1 - y0) / step;

                                int localY = gy * step + y;
                                int localZ = gz * step + z;

                                for (int x = 0; x < step; x++)
                                {
                                    int localX = gx * step + x;

                                    int state = density > 0 ? solidIndex : airIndex;
                                    subChunk.isAllAir = state != solidIndex && subChunk.isAllAir;
                                    subChunk.SetBlockFast(localX, localY, localZ, state);

                                    if (state == airIndex)
                                    {
                                        int globalY = cyp + localY;

                                        if (globalY <= WorldGenerator.SEA_LEVEL)
                                        {
                                            NoiseRegion nr = noiseRegions[cxp + localX, czp + localZ];
                                            int waterIndex = subChunk.palette.GetOrAddIndex(nr.biome.WaterBlock);
                                            int waterSurfaceIndex = subChunk.palette.GetOrAddIndex(nr.biome.WaterSurfaceBlock);

                                            int water = globalY == WorldGenerator.SEA_LEVEL ? waterSurfaceIndex : waterIndex;
                                            subChunk.SetBlockFast(localX, localY, localZ, water);
                                            subChunk.isAllAir = false;
                                        }
                                    }

                                    density += densityStep;
                                }
                            }
                        }
                    }
                }
            }
        }

        //initializes all subchunks in a chunk
        public static void InitSubChunks(Chunk chunk)
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

        //finds all structures that intersect with the target chunk, grid of chunks
        static List<PlacedFeature> CollectStructures(Chunk target, ChunkManager world, int radius)
        {
            var allFeatures = new List<PlacedFeature>();

            //add intersected surface features
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    Chunk? neighbor = world.GetChunk(new ChunkCoord(target.ChunkPos.X + dx, target.ChunkPos.Z + dz));
                    if (neighbor == null || !neighbor.HasVoxelData()) continue;

                    foreach (var sf in neighbor.placedFeatures)
                    {
                        if (sf.feature.notCrossChunk)
                        {
                            if (neighbor.ChunkPos == target.ChunkPos) allFeatures.Add(sf);
                            continue;
                        }
                        if (SurfaceFeature.IntersectsChunk(target, sf.startPos, sf.feature))
                        {
                            allFeatures.Add(sf);
                        }
                    }
                }
            }

            return allFeatures;
        }

        //determinsitically sorts structures (closest to origin first, then furthest last)
        static void SortStructures(List<PlacedFeature> features)
        {
            features.Sort((a, b) =>
            {
                int distA = a.startPos.X * a.startPos.X + a.startPos.Y * a.startPos.Y + a.startPos.Z * a.startPos.Z;
                int distB = b.startPos.X * b.startPos.X + b.startPos.Y * b.startPos.Y + b.startPos.Z * b.startPos.Z;
                int cd = distA.CompareTo(distB);
                if (cd != 0) return cd;
                int cx = a.startPos.X.CompareTo(b.startPos.X);
                if (cx != 0) return cx;
                int cz = a.startPos.Z.CompareTo(b.startPos.Z);
                if (cz != 0) return cz;
                return a.startPos.Y.CompareTo(b.startPos.Y);
            });
        }

        //scans chunk from top down to find the first y layer with blocks
        static int GetChunkMaxSolidY(Chunk chunk)
        {
            for (int sy = HEIGHT_IN_SUBCHUNKS - 1; sy >= 0; sy--)
            {
                bool layerHasBlocks = false;

                //first check if layer contains any blocks
                for (int sx = 0; sx < WIDTH_IN_SUBCHUNKS; sx++)
                {
                    for (int sz = 0; sz < WIDTH_IN_SUBCHUNKS; sz++)
                    {
                        if (!chunk.SubChunks[sx, sy, sz].IsAllAir())
                        {
                            layerHasBlocks = true;
                            break;
                        }
                    }
                    if (layerHasBlocks) break;
                }

                if (!layerHasBlocks) continue;

                //scan the entire subchunk y-layer for the highest block
                int maxY = -1;

                for (int sx = 0; sx < WIDTH_IN_SUBCHUNKS; sx++)
                {
                    for (int sz = 0; sz < WIDTH_IN_SUBCHUNKS; sz++)
                    {
                        SubChunk subChunk = chunk.SubChunks[sx, sy, sz];
                        if (subChunk.IsAllAir()) continue;

                        int y = ScanSubChunkLayer(subChunk);
                        if (y > maxY) maxY = y;
                    }
                }

                return maxY + 1;
            }

            return CHUNK_HEIGHT - 1;
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
