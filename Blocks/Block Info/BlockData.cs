using OurCraft.Blocks.Block_Implementations;

//contains all the indexes and data for each different block type
namespace OurCraft.Blocks
{
    //list of all block data in the game
    //includes actual block data
    //to add a block, create a block id for that block and then use "Register Block"
    public static class BlockData
    {
        //all blocks
        static readonly List<Block> blocks = new();
        public static int MAXBLOCKID;

        //register all of the blocks
        public static void InitBlocks()
        {
            //natural blocks
            BlockIDs.AIR_BLOCK = RegisterBlock(new AirBlock("Air", 0, 0, 0, 0, 0, 0, BlockIDs.AIR_BLOCK));
            BlockIDs.GRASS_BLOCK = RegisterBlock(new FullBlock("Grass Block", TextureIDs.dirtTex, TextureIDs.grassTopTex, TextureIDs.grassSideTex, BlockIDs.GRASS_BLOCK));
            BlockIDs.DIRT_BLOCK = RegisterBlock(new FullBlock("Dirt", TextureIDs.dirtTex, BlockIDs.DIRT_BLOCK));
            BlockIDs.SAND_BLOCK = RegisterBlock(new FullBlock("Sand", TextureIDs.sandTex, BlockIDs.SAND_BLOCK));
            BlockIDs.SNOW_BLOCK = RegisterBlock(new FullBlock("Snow", TextureIDs.snowTex, BlockIDs.SNOW_BLOCK));
            BlockIDs.SNOWY_GRASS_BLOCK = RegisterBlock(new FullBlock("Snowy Grass Block", TextureIDs.dirtTex, TextureIDs.snowTex, TextureIDs.snowGrassSideTex, BlockIDs.SNOWY_GRASS_BLOCK));
            BlockIDs.STONE_BLOCK = RegisterBlock(new FullBlock("Stone", TextureIDs.stoneTex, BlockIDs.STONE_BLOCK));
            BlockIDs.WATER_BLOCK = RegisterBlock(new WaterBlock("Water", TextureIDs.blueWoolTex, BlockIDs.WATER_BLOCK));

            //x shaped natural blocks
            BlockIDs.ROSE_BLOCK = RegisterBlock(new CrossQuadBlock("Rose", TextureIDs.roseTex, BlockIDs.ROSE_BLOCK));
            BlockIDs.X_GRASS_BLOCK = RegisterBlock(new CrossQuadBlock("Grass", TextureIDs.xGrassTex, BlockIDs.X_GRASS_BLOCK));
            BlockIDs.DEAD_BUSH_BLOCK = RegisterBlock(new CrossQuadBlock("Dead Bush", TextureIDs.deadBushTex, BlockIDs.DEAD_BUSH_BLOCK));

            //building blocks
            BlockIDs.STONE_SLAB_BLOCK = RegisterBlock(new SlabBlock("Stone Slab", TextureIDs.stoneTex, BlockIDs.STONE_SLAB_BLOCK));  
            BlockIDs.GLASS_BLOCK = RegisterBlock(new GlassBlock("Glass", TextureIDs.glassTex, BlockIDs.GLASS_BLOCK));

            //wood
            BlockIDs.OAK_PLANKS_BLOCK = RegisterBlock(new FullBlock("Oak Planks", TextureIDs.oakPlanksTex, BlockIDs.OAK_PLANKS_BLOCK));
            BlockIDs.OAK_LOG_BLOCK = RegisterBlock(new BlockLog("Oak Log", TextureIDs.oakLogTopTex, TextureIDs.oakLogSideTex, BlockIDs.OAK_LOG_BLOCK));
            BlockIDs.OAK_SLAB_BLOCK = RegisterBlock(new SlabBlock("Oak Slab", TextureIDs.oakPlanksTex, BlockIDs.OAK_SLAB_BLOCK));
            BlockIDs.OAK_LEAVES_BLOCK = RegisterBlock(new LeavesBlock("Oak Leaves", TextureIDs.oakLeavesTex, BlockIDs.OAK_LEAVES_BLOCK));

            BlockIDs.SPRUCE_PLANKS_BLOCK = RegisterBlock(new FullBlock("Spruce Planks", TextureIDs.sprucePlanksTex, BlockIDs.SPRUCE_PLANKS_BLOCK));
            BlockIDs.SPRUCE_LOG_BLOCK = RegisterBlock(new BlockLog("Spruce Log", TextureIDs.spruceLogTopTex, TextureIDs.spruceLogSideTex, BlockIDs.SPRUCE_LOG_BLOCK));
            BlockIDs.SPRUCE_SLAB_BLOCK = RegisterBlock(new SlabBlock("Spruce Slab", TextureIDs.sprucePlanksTex, BlockIDs.SPRUCE_SLAB_BLOCK));
            BlockIDs.SPRUCE_LEAVES_BLOCK = RegisterBlock(new LeavesBlock("Spruce Leaves", TextureIDs.spruceLeavesTex, BlockIDs.SPRUCE_LEAVES_BLOCK));

            BlockIDs.BIRCH_PLANKS_BLOCK = RegisterBlock(new FullBlock("Birch Planks", TextureIDs.birchPlanksTex, BlockIDs.BIRCH_PLANKS_BLOCK));
            BlockIDs.BIRCH_LOG_BLOCK = RegisterBlock(new BlockLog("Birch Log", TextureIDs.birchLogTopTex, TextureIDs.birchLogSideTex, BlockIDs.BIRCH_LOG_BLOCK));
            BlockIDs.BIRCH_SLAB_BLOCK = RegisterBlock(new SlabBlock("Birch Slab", TextureIDs.birchPlanksTex, BlockIDs.BIRCH_SLAB_BLOCK));
            BlockIDs.BIRCH_LEAVES_BLOCK = RegisterBlock(new LeavesBlock("Birch Leaves", TextureIDs.birchLeavesTex, BlockIDs.BIRCH_LEAVES_BLOCK));

            BlockIDs.JUNGLE_PLANKS_BLOCK = RegisterBlock(new FullBlock("Jungle Planks", TextureIDs.junglePlanksTex, BlockIDs.JUNGLE_PLANKS_BLOCK));
            BlockIDs.JUNGLE_LOG_BLOCK = RegisterBlock(new BlockLog("Jungle Log", TextureIDs.jungleLogTopTex, TextureIDs.jungleLogSideTex, BlockIDs.JUNGLE_LOG_BLOCK));
            BlockIDs.JUNGLE_SLAB_BLOCK = RegisterBlock(new SlabBlock("Jungle Slab", TextureIDs.junglePlanksTex, BlockIDs.JUNGLE_SLAB_BLOCK));
            BlockIDs.JUNGLE_LEAVES_BLOCK = RegisterBlock(new LeavesBlock("Jungle Leaves", TextureIDs.jungleLeavesTex, BlockIDs.JUNGLE_LEAVES_BLOCK));
            
            BlockIDs.ICE_BLOCK = RegisterBlock(new FullBlock("Ice Block", TextureIDs.iceTex, BlockIDs.ICE_BLOCK));
            BlockIDs.GRAVEL_BLOCK = RegisterBlock(new FullBlock("Gravel Block", TextureIDs.gravelTex, BlockIDs.GRAVEL_BLOCK));
            BlockIDs.CACTUS_BLOCK = RegisterBlock(new FullBlock("Cactus Block", TextureIDs.cactusBottomTex, TextureIDs.cactusTopTex, TextureIDs.cactusSideTex, BlockIDs.CACTUS_BLOCK));
        }

        //get block from id
        public static Block GetBlock(int ID)
        {
            if (ID == BlockIDs.INVALID_BLOCK || ID > MAXBLOCKID || ID < 0)
            {
                return blocks[0];
            }
            return blocks[ID];
        }

        //create new block
        public static ushort RegisterBlock(Block block)
        {
            if (blocks.Any(b => b.GetBlockName() == block.GetBlockName()))
                throw new Exception($"Block '{block.GetBlockName()}' already registered.");
            ushort id = (ushort)blocks.Count;
            blocks.Add(block);
            block.SetID(id);
            MAXBLOCKID = id;
            BlockRegistry.AddBlock(block.GetBlockName(), id);
            return id;
        }
    }

    //contains all block ids for using in the block list
    public static class BlockIDs
    {
        public static ushort INVALID_BLOCK = 255;
        public static ushort SNOWY_GRASS_BLOCK = 0;
        public static ushort AIR_BLOCK = 0;
        public static ushort GRASS_BLOCK = 0;
        public static ushort DIRT_BLOCK = 0;     
        public static ushort STONE_BLOCK = 0;       
        public static ushort STONE_SLAB_BLOCK = 0;
        public static ushort WATER_BLOCK = 0;
        public static ushort GLASS_BLOCK = 0;        
        public static ushort ROSE_BLOCK = 0;
        public static ushort X_GRASS_BLOCK = 0;
        public static ushort OAK_PLANKS_BLOCK = 0;
        public static ushort OAK_LOG_BLOCK = 0;
        public static ushort OAK_SLAB_BLOCK = 0;
        public static ushort OAK_LEAVES_BLOCK = 0;
        public static ushort SPRUCE_PLANKS_BLOCK = 0;
        public static ushort SPRUCE_LOG_BLOCK = 0;
        public static ushort SPRUCE_SLAB_BLOCK = 0;
        public static ushort SPRUCE_LEAVES_BLOCK = 0;     
        public static ushort BIRCH_PLANKS_BLOCK = 0;
        public static ushort BIRCH_LOG_BLOCK = 0;
        public static ushort BIRCH_SLAB_BLOCK = 0;
        public static ushort BIRCH_LEAVES_BLOCK = 0;
        public static ushort JUNGLE_PLANKS_BLOCK = 0;
        public static ushort JUNGLE_LOG_BLOCK = 0;
        public static ushort JUNGLE_SLAB_BLOCK = 0;
        public static ushort JUNGLE_LEAVES_BLOCK = 0;
        public static ushort SAND_BLOCK = 0;
        public static ushort SNOW_BLOCK = 0;
        public static ushort ICE_BLOCK = 0;
        public static ushort GRAVEL_BLOCK = 0;
        public static ushort DEAD_BUSH_BLOCK = 0;
        public static ushort CACTUS_BLOCK = 0;
    }

    //dictionary for mapping ids to strings
    public static class BlockRegistry
    {
        static readonly Dictionary<string, ushort> blockRegistry = [];

        public static void AddBlock(string name, ushort id)
        {
            blockRegistry.Add(name, id);
        }

        public static ushort GetBlock(string name)
        {
            return blockRegistry[name];
        }
    };
}
