using OurCraft.Blocks;

namespace OurCraft.World.Terrain_Generation
{
    //determines surface blocks and surface features of a world region
    public class Biome
    {
        //empty constructor
        public Biome()
        {

        }

        public string name = string.Empty;
        public float temperatureThreshold = 0;

        //water block
        public ushort waterBlock = BlockIDs.WATER_BLOCK;

        //surface and sub surface blocks

        //around sea level
        public ushort shoreSurface = BlockIDs.STONE_BLOCK;
        public ushort shoreSubsurface = BlockIDs.STONE_BLOCK;

        //regular height
        public ushort landSurface = BlockIDs.STONE_BLOCK;
        public ushort landSubsurface = BlockIDs.STONE_BLOCK;

        //high zones
        public ushort peakSurface = BlockIDs.STONE_BLOCK;
        public ushort peakSubsurface = BlockIDs.STONE_BLOCK;

        //which block to grow plants and deco on
        public ushort applyDecoOn = BlockIDs.GRASS_BLOCK;

        //one block decorations on mid land
        public ushort decoBlock1 = BlockIDs.X_GRASS_BLOCK;
        public ushort decoBlock2 = BlockIDs.ROSE_BLOCK;
        public ushort decoBlock3 = BlockIDs.SPRUCE_LOG_BLOCK;
        public ushort decoBlock4 = BlockIDs.OAK_LEAVES_BLOCK;

        //out of 2000, what to spawn each deco block
        public int decoBlock1Threshold = 0;
        public int decoBlock2Threshold = 0;
        public int decoBlock3Threshold = 0;
        public int decoBlock4Threshold = 0;
    }
}
