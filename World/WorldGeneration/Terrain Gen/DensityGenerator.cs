using OurCraft.Terrain_Generation;
using OurCraft.Utility;
using OurCraft.Terrain_Generation.Noise;
using OurCraft.World.WorldData;

namespace OurCraft.World.WorldGeneration.Terrain_Gen
{
    //contains helper methods for doing density generation for chunks
    public static class DensityGenerator
    {
        const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;
        const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS;

        public const int INTERP_STEP = ChunkGenerator.INTERP_STEP_DENSITY; //sample every 2 blocks → 9 points per axis (0,4,8,12,16)
        public const int INTERP_GRID = SUBCHUNK_SIZE / INTERP_STEP + 1; //9

        //create stone vs air vs water in chunk
        public static void CreateDensityMap(SubChunk subChunk, NoiseRegion[,] noiseRegions)
        {
            int cyp = subChunk.ChunkYPos * SUBCHUNK_SIZE;
            int ccx = CHUNK_WIDTH * subChunk.parent.ChunkPos.X;
            int ccz = CHUNK_WIDTH * subChunk.parent.ChunkPos.Z;

            if (cyp >= WorldGenConstants.DEFAULT_MAX_HEIGHT)
            {
                FillSubChunkWithAir(subChunk, noiseRegions);
                return;
            }

            if (cyp < WorldGenConstants.DEFAULT_MIN_HEIGHT)
            {
                FillSubChunkWithStone(subChunk);
                return;
            }

            //find min/max possible terrain influence for this chunk
            float lowestPossibleTerrainY = float.MaxValue;
            float highestPossibleTerrainY = float.MinValue;

            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {
                    NoiseRegion nr = noiseRegions[x, z];

                    float minY = nr.heightOffset - nr.maxDepth;
                    float maxY = nr.heightOffset + nr.maxDepth;

                    if (minY < lowestPossibleTerrainY) lowestPossibleTerrainY = minY;
                    if (maxY > highestPossibleTerrainY) highestPossibleTerrainY = maxY;
                }
            }

            //entire subchunk is above possible terrain
            if (cyp > highestPossibleTerrainY)
            {
                FillSubChunkWithAir(subChunk, noiseRegions);
                return;
            }

            //entire subchunk is below possible terrain
            if ((cyp + SUBCHUNK_SIZE - 1) < lowestPossibleTerrainY)
            {
                FillSubChunkWithStone(subChunk);
                return;
            }

            float[] densityGrid = CreateDensityGrid(noiseRegions, cyp, ccx, ccz);
            InterpolateCells(subChunk, densityGrid, noiseRegions, cyp);
        }

        //quickly fills a subchunk with only air
        static void FillSubChunkWithAir(SubChunk subChunk, NoiseRegion[,] noiseRegions)
        {
            int airIndex = subChunk.palette.GetOrAddIndex(OverworldGenerator.EmptyBlock);
            int cyp = subChunk.ChunkYPos * SUBCHUNK_SIZE;

            subChunk.isAllAir = true;
            for (int y = 0; y < SUBCHUNK_SIZE; y++)
            {
                int globalY = cyp + y;
                for (int x = 0; x < SUBCHUNK_SIZE; x++)
                {               
                    for (int z = 0; z < SUBCHUNK_SIZE; z++)
                    {
                        if (globalY <= OverworldGenerator.SEA_LEVEL)
                        {
                            NoiseRegion nr = noiseRegions[x, z];
                            int waterIndex = subChunk.palette.GetOrAddIndex(nr.biome.WaterBlock);
                            int waterSurfaceIndex = subChunk.palette.GetOrAddIndex(nr.biome.WaterSurfaceBlock);

                            int water = globalY == OverworldGenerator.SEA_LEVEL ? waterSurfaceIndex : waterIndex;
                            subChunk.SetBlockFast(x, y, z, water);
                            subChunk.isAllAir = false;
                        }
                        else subChunk.SetBlockFast(x, y, z, airIndex);
                    }
                }
            } 
        }

        //quickly fills a subchunk with only stone
        static void FillSubChunkWithStone(SubChunk subChunk)
        {
            int solidIndex = subChunk.palette.GetOrAddIndex(OverworldGenerator.WorldBlock);

            for (int x = 0; x < SUBCHUNK_SIZE; x++)
            for (int y = 0; y < SUBCHUNK_SIZE; y++)
            for (int z = 0; z < SUBCHUNK_SIZE; z++)
            {
                subChunk.SetBlockFast(x, y, z, solidIndex);
            }
                                
            subChunk.isAllAir = false;
        }

        //creates a coarse grid using density noise sampling
        static float[] CreateDensityGrid(NoiseRegion[,] noiseRegions, int cyp, int ccx, int ccz)
        {
            const int GRID = INTERP_GRID;
            float[] densityGrid = new float[GRID * GRID * GRID];

            //build coarse density grid
            for (int gz = 0; gz < GRID; gz++)
            {
                int sampleZ = gz * INTERP_STEP;

                for (int gx = 0; gx < GRID; gx++)
                {
                    int sampleX = gx * INTERP_STEP;

                    NoiseRegion nr = noiseRegions[Math.Min(sampleX, noiseRegions.GetLength(0) - 1), Math.Min(sampleZ, noiseRegions.GetLength(1) - 1)];

                    int globalX = sampleX + ccx;
                    int globalZ = sampleZ + ccz;

                    for (int gy = 0; gy < GRID; gy++)
                    {
                        int globalY = cyp + gy * INTERP_STEP;
                        float density;

                        if (globalY > OverworldGenerator.MAX_HEIGHT) density = -1f;
                        else if (globalY < OverworldGenerator.MIN_HEIGHT) density = 1f;
                        else
                        {
                            float surfaceDist = nr.heightOffset - globalY;
                            if (surfaceDist < -nr.maxDepth) density = -1f;
                            else if (surfaceDist > nr.maxDepth) density = 1f;
                            else density = OverworldGenerator.GetDensity(globalX, globalY, globalZ, nr.heightOffset, nr.amplification);
                        }

                        densityGrid[VoxelMath.GridIndex(gx, gy, gz)] = density;
                    }
                }
            }

            return densityGrid;
        }

        //interpolates the generated density grid and sets blocks
        static void InterpolateCells(SubChunk subChunk, float[] grid, NoiseRegion[,] noiseRegions, int cyp)
        {
            const int GRID = INTERP_GRID;
            int step = INTERP_STEP;

            int solidIndex = subChunk.palette.GetOrAddIndex(OverworldGenerator.WorldBlock);
            int airIndex = subChunk.palette.GetOrAddIndex(OverworldGenerator.EmptyBlock);

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

                                    int state = density > 0.0f ? solidIndex : airIndex;
                                    subChunk.isAllAir = state != solidIndex && subChunk.isAllAir;
                                    subChunk.SetBlockFast(localX, localY, localZ, state);

                                    if (state == airIndex)
                                    {
                                        int globalY = cyp + localY;

                                        if (globalY <= OverworldGenerator.SEA_LEVEL)
                                        {
                                            NoiseRegion nr = noiseRegions[localX, localZ];
                                            int waterIndex = subChunk.palette.GetOrAddIndex(nr.biome.WaterBlock);
                                            int waterSurfaceIndex = subChunk.palette.GetOrAddIndex(nr.biome.WaterSurfaceBlock);

                                            int water = globalY == OverworldGenerator.SEA_LEVEL ? waterSurfaceIndex : waterIndex;
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
    }
}