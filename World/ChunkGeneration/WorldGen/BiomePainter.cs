using OurCraft.Blocks;
using OurCraft.Terrain_Generation;
using OurCraft.Utility;
using OurCraft.Terrain_Generation.Noise;

namespace OurCraft.World.ChunkGeneration.WorldGen
{
    //contains helpers for dealing with biomes and applying them to terrain
    public static class BiomePainter
    {
        const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS;

        //paints the surface blocks with biome surface blocks
        public static void SurfacePaint(SubChunk subChunk, NoiseRegion[,] noiseRegions)
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
                        BlockState above = y + 1 < SUBCHUNK_SIZE ? subChunk.GetBlockState(x, y + 1, z) :
                        subChunk.parent.GetBlockStateUnsafe(cxp + x, y + cyp + 1, czp + z);

                        bool currentEligible = current != Block.AIR && current != biome.WaterBlock && current != biome.WaterSurfaceBlock;
                        bool aboveEligible = above == Block.AIR || above == biome.WaterBlock || above == biome.WaterSurfaceBlock;

                        //only consider blocks with air above
                        if (currentEligible && aboveEligible)
                        {
                            for (int d = 0; d < 5 && y - d >= 0; d++)
                            {
                                int targetY = y - d;

                                //customize depth levels for the biomes
                                if (d == 0) subChunk.SetBlock(x, targetY, z, OverworldGenerator.GetSurfaceBlock(biome, targetY + cyp));
                                else if (d <= 2) subChunk.SetBlock(x, targetY, z, OverworldGenerator.GetSubSurfaceBlock(biome, targetY + cyp));
                            }
                        }
                    }
                }
            }
        }
    }
}
