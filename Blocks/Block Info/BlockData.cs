using OurCraft.Blocks.Block_Implementations;

//contains all the indexes and data for each different block type
namespace OurCraft.Blocks
{
    //list of all block data in the game
    //includes actual block data
    public static class BlockData
    {
        //all blocks
        static readonly Block[] blocks =
        {
           //natural blocks
           new AirBlock("air", 0, 0, 0, 0, 0, 0, BlockIDs.AIR_BLOCK),
           new FullBlock("grass_block", TextureIDs.dirtTex, TextureIDs.grassTopTex, TextureIDs.grassSideTex, BlockIDs.GRASS_BLOCK),
           new FullBlock("dirt", TextureIDs.dirtTex, BlockIDs.DIRT_BLOCK),
           
           //stones
           new FullBlock("stone", TextureIDs.stoneTex, BlockIDs.STONE_BLOCK),
           new SlabBlock("stone_slab", TextureIDs.stoneTex, BlockIDs.STONE_SLAB_BLOCK),

           //see-through
           new WaterBlock("water", TextureIDs.blueWoolTex, BlockIDs.WATER_BLOCK),
           new GlassBlock("glass", TextureIDs.glassTex, BlockIDs.GLASS_BLOCK),

           //x shaped blocks
           new CrossQuadBlock("rose", TextureIDs.roseTex, BlockIDs.ROSE_BLOCK),
           new CrossQuadBlock("grass", TextureIDs.xGrassTex, BlockIDs.X_GRASS_BLOCK),

           //wood
           //oak wood
           new FullBlock("oak_planks", TextureIDs.oakPlanksTex, BlockIDs.OAK_PLANKS_BLOCK),
           new BlockLog("oak_log", TextureIDs.oakLogTopTex, TextureIDs.oakLogSideTex, BlockIDs.OAK_LOG_BLOCK),
           new SlabBlock("oak_slab", TextureIDs.oakPlanksTex, BlockIDs.OAK_SLAB_BLOCK),
           new LeavesBlock("oak_leaves", TextureIDs.oakLeavesTex, BlockIDs.OAK_LEAVES_BLOCK),

           //spruce wood
           new FullBlock("spruce_planks", TextureIDs.sprucePlanksTex, BlockIDs.SPRUCE_PLANKS_BLOCK),
           new BlockLog("spruce_log", TextureIDs.spruceLogTopTex, TextureIDs.spruceLogSideTex, BlockIDs.SPRUCE_LOG_BLOCK),
           new SlabBlock("spruce_slab", TextureIDs.sprucePlanksTex, BlockIDs.SPRUCE_SLAB_BLOCK),
           new LeavesBlock("spruce_leaves", TextureIDs.spruceLeavesTex, BlockIDs.SPRUCE_LEAVES_BLOCK),

           //birch wood
           new FullBlock("birch_planks", TextureIDs.birchPlanksTex, BlockIDs.BIRCH_PLANKS_BLOCK),
           new BlockLog("birch_log", TextureIDs.birchLogTopTex, TextureIDs.birchLogSideTex, BlockIDs.BIRCH_LOG_BLOCK),
           new SlabBlock("birch_slab", TextureIDs.birchPlanksTex, BlockIDs.BIRCH_SLAB_BLOCK),
           new LeavesBlock("birch_leaves", TextureIDs.birchLeavesTex, BlockIDs.BIRCH_LEAVES_BLOCK),

           //sand and snow
           new FullBlock("sand", TextureIDs.sandTex, BlockIDs.SAND_BLOCK),
           new FullBlock("snow", TextureIDs.snowTex, BlockIDs.SNOW_BLOCK),
           new FullBlock("snowy_grass_block", TextureIDs.dirtTex, TextureIDs.snowTex, TextureIDs.snowGrassSideTex, BlockIDs.SNOWY_GRASS_BLOCK),
        };
        
        //get block from id
        public static Block GetBlock(int ID)
        {
            if (ID == BlockIDs.INVALID_BLOCK || ID > MAXBLOCKID || ID < 0) return blocks[0];
            return blocks[ID];
        }

        public static readonly int MAXBLOCKID = blocks.Length - 1;
    }

    //contains all block ids for using in the block list
    public static class BlockIDs
    {
        public static readonly byte INVALID_BLOCK = 255;

        public static readonly byte AIR_BLOCK = 0;
        public static readonly byte GRASS_BLOCK = 1;
        public static readonly byte DIRT_BLOCK = 2;
       
        public static readonly byte STONE_BLOCK = 3;       
        public static readonly byte STONE_SLAB_BLOCK = 4;

        public static readonly byte WATER_BLOCK = 5;
        public static readonly byte GLASS_BLOCK = 6;
        
        public static readonly byte ROSE_BLOCK = 7;
        public static readonly byte X_GRASS_BLOCK = 8;

        //all wood types
        public static readonly byte OAK_PLANKS_BLOCK = 9;
        public static readonly byte OAK_LOG_BLOCK = 10;
        public static readonly byte OAK_SLAB_BLOCK = 11;
        public static readonly byte OAK_LEAVES_BLOCK = 12;

        public static readonly byte SPRUCE_PLANKS_BLOCK = 13;
        public static readonly byte SPRUCE_LOG_BLOCK = 14;
        public static readonly byte SPRUCE_SLAB_BLOCK = 15;
        public static readonly byte SPRUCE_LEAVES_BLOCK = 16;
        
        public static readonly byte BIRCH_PLANKS_BLOCK = 17;
        public static readonly byte BIRCH_LOG_BLOCK = 18;
        public static readonly byte BIRCH_SLAB_BLOCK = 19;
        public static readonly byte BIRCH_LEAVES_BLOCK = 20;


        //sand and snow
        public static readonly byte SAND_BLOCK = 21;
        public static readonly byte SNOW_BLOCK = 22;
        public static readonly byte SNOWY_GRASS_BLOCK = 23;
    }
}
