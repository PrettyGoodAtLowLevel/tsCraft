using OurCraft.Blocks;
using OurCraft.Terrain_Generation;
using OurCraft.Utility;
using OurCraft.Terrain_Generation.Noise;
using OurCraft.Terrain_Generation.Registries;

namespace OurCraft.World.ChunkGeneration.WorldGen
{
    //contains helpers for generating caves density for chunks
    public static class CaveGenerator
    {
        const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;
        const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS;

        const int MIN_CAVE_HEIGHT = 5;
        const int MAX_CAVE_HEIGHT = WorldGenConstants.DEFAULT_MAX_CAVE_HEIGHT;
        const float CAVE_THRESHOLD = 2.0f;

        public const int INTERP_STEP = ChunkGenerator.INTERP_STEP_CAVE;
        public const int INTERP_GRID = SUBCHUNK_SIZE / INTERP_STEP + 1;

        //creates density grid for caves, interpolates to carve out caves quickly
        public static void CarveCaves(SubChunk subChunk, NoiseRegion[,] noiseRegions)
        {
            if (subChunk.isAllAir) return;

            int cxp = subChunk.ChunkXPos * SUBCHUNK_SIZE;
            int cyp = subChunk.ChunkYPos * SUBCHUNK_SIZE;
            int czp = subChunk.ChunkZPos * SUBCHUNK_SIZE;
            int ccx = CHUNK_WIDTH * subChunk.parent.ChunkPos.X;
            int ccz = CHUNK_WIDTH * subChunk.parent.ChunkPos.Z;

            //early out if subchunk is fully outside cave range
            if (cyp + SUBCHUNK_SIZE - 1 < MIN_CAVE_HEIGHT || cyp > MAX_CAVE_HEIGHT) return;

            float[] caveGrid = BuildCaveGrid(noiseRegions, cxp, cyp, czp, ccx, ccz);
            CarveCaveCells(subChunk, caveGrid, cyp);
        }

        //builds a small cave grid that we can upsample for optimization
        static float[] BuildCaveGrid(NoiseRegion[,] noiseRegions, int cxp, int cyp, int czp, int ccx, int ccz)
        {
            const int GRID = INTERP_GRID;
            float[] caveGrid = new float[GRID * GRID * GRID];

            for (int gz = 0; gz < GRID; gz++)
            {
                int sampleZ = czp + gz * INTERP_STEP;

                for (int gx = 0; gx < GRID; gx++)
                {
                    int sampleX = cxp + gx * INTERP_STEP;

                    NoiseRegion nr = noiseRegions[Math.Min(sampleX, noiseRegions.GetLength(0) - 1), Math.Min(sampleZ, noiseRegions.GetLength(1) - 1)];
                    float caveAmp = nr.caveAmp;

                    int globalX = sampleX + ccx;
                    int globalZ = sampleZ + ccz;

                    for (int gy = 0; gy < GRID; gy++)
                    {
                        int globalY = cyp + gy * INTERP_STEP;

                        //store 0 outside cave range so interpolation naturally
                        //fades carving to zero at the height boundaries
                        float cave = (globalY >= MIN_CAVE_HEIGHT && globalY <= MAX_CAVE_HEIGHT) ? OverworldGenerator.GetCaveDensity(globalX, globalY, globalZ, caveAmp) : 0f;
                        caveGrid[VoxelMath.GridIndexCave(gx, gy, gz)] = cave;
                    }
                }
            }

            return caveGrid;
        }

        //upsamples cave density grid and carves caves
        static void CarveCaveCells(SubChunk subChunk, float[] grid, int cyp)
        {
            const int GRID = INTERP_GRID;
            int step = INTERP_STEP;

            BlockState air = BlockRegistry.GetDefaultBlockState("Air");
            BlockState water = BlockRegistry.GetDefaultBlockState("Water");

            for (int gz = 0; gz < GRID - 1; gz++)
            {
                for (int gx = 0; gx < GRID - 1; gx++)
                {
                    for (int gy = 0; gy < GRID - 1; gy++)
                    {
                        float c000 = grid[VoxelMath.GridIndexCave(gx, gy, gz)];
                        float c100 = grid[VoxelMath.GridIndexCave(gx + 1, gy, gz)];
                        float c010 = grid[VoxelMath.GridIndexCave(gx, gy + 1, gz)];
                        float c110 = grid[VoxelMath.GridIndexCave(gx + 1, gy + 1, gz)];
                        float c001 = grid[VoxelMath.GridIndexCave(gx, gy, gz + 1)];
                        float c101 = grid[VoxelMath.GridIndexCave(gx + 1, gy, gz + 1)];
                        float c011 = grid[VoxelMath.GridIndexCave(gx, gy + 1, gz + 1)];
                        float c111 = grid[VoxelMath.GridIndexCave(gx + 1, gy + 1, gz + 1)];

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

                                int localY = gy * step + y;
                                int localZ = gz * step + z;
                                int globalY = cyp + localY;

                                //undergroundBoost depends only on Y
                                float undergroundBoost = SplineRegistry.caveOpenSpline.Evaluate(globalY);

                                float caveVal = y0;
                                float caveStep = (y1 - y0) / step;

                                for (int x = 0; x < step; x++)
                                {
                                    if (caveVal * undergroundBoost > CAVE_THRESHOLD)
                                    {
                                        int localX = gx * step + x;
                                        BlockState current = subChunk.GetBlockState(localX, localY, localZ);
                                        if (current != air && current != water) subChunk.SetBlock(localX, localY, localZ, air);
                                    }
                                    caveVal += caveStep;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}