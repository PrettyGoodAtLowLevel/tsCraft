using OurCraft.Blocks;
using System.Runtime.CompilerServices;

namespace OurCraft.World.Terrain_Generation
{
    //contains all the biomes
    public static class BiomeData
    {
        public static readonly Biome plains = new Biome();

        //creates all the biomes
        public static void InitBiomes()
        {
            //create plains biome
            plains.name = "plains";
            plains.waterBlock = BlockIDs.WATER_BLOCK;

            plains.shoreSurface = BlockIDs.SAND_BLOCK;
            plains.shoreSubsurface = BlockIDs.SAND_BLOCK;

            plains.landSurface = BlockIDs.GRASS_BLOCK;
            plains.landSubsurface = BlockIDs.DIRT_BLOCK;

            plains.peakSurface = BlockIDs.SNOW_BLOCK;
            plains.peakSubsurface = BlockIDs.STONE_BLOCK;

            plains.decoBlock1 = BlockIDs.OAK_LEAVES_BLOCK;
            plains.decoBlock2 = BlockIDs.OAK_LEAVES_BLOCK;
            plains.decoBlock3 = BlockIDs.ROSE_BLOCK;
            plains.decoBlock4 = BlockIDs.X_GRASS_BLOCK;

            plains.decoBlock1Threshold = 1998;
            plains.decoBlock2Threshold = 1995;
            plains.decoBlock3Threshold = 1985;
            plains.decoBlock4Threshold = 1900;
            plains.applyDecoOn = BlockIDs.GRASS_BLOCK;
        }
    }
}
