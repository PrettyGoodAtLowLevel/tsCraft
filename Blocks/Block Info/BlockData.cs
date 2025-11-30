using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.BlockShapeData;

//contains all the indexes and data for each different block type
namespace OurCraft.Blocks
{
    //list of all block data in the game
    //includes actual block data
    //to add a block, create a block id for that block and then use "Register Block"
    public static class BlockData
    {
        //all blocks
        static readonly List<Block> blocks = [];
        public static int MAXBLOCKID { get; private set; }

        //register all of the blocks
        public static void InitBlocks()
        {
            //natural blocks
            BlockIDs.AIR_BLOCK = RegisterBlock(new AirBlock("Air", BlockShapesRegistry.AirBlockShape, BlockIDs.AIR_BLOCK));
            BlockIDs.GRASS_BLOCK = RegisterBlock(new FullBlock("Grass Block", BlockShapesRegistry.GrassBlockShape, BlockIDs.GRASS_BLOCK));
            BlockIDs.DIRT_BLOCK = RegisterBlock(new FullBlock("Dirt", BlockShapesRegistry.DirtBlockShape, BlockIDs.DIRT_BLOCK));
            BlockIDs.SAND_BLOCK = RegisterBlock(new FullBlock("Sand", BlockShapesRegistry.SandBlockShape, BlockIDs.SAND_BLOCK));
            BlockIDs.SNOW_BLOCK = RegisterBlock(new FullBlock("Snow", BlockShapesRegistry.SnowBlockShape, BlockIDs.SNOW_BLOCK));
            BlockIDs.SNOWY_GRASS_BLOCK = RegisterBlock(new FullBlock("Snowy Grass Block", BlockShapesRegistry.SnowyGrassBlockShape, BlockIDs.SNOWY_GRASS_BLOCK));
            BlockIDs.STONE_BLOCK = RegisterBlock(new FullBlock("Stone", BlockShapesRegistry.StoneBlockShape, BlockIDs.STONE_BLOCK));
            BlockIDs.WATER_BLOCK = RegisterBlock(new WaterBlock("Water", BlockShapesRegistry.WaterBlockShape, BlockIDs.WATER_BLOCK));

            //x shaped natural blocks
            BlockIDs.ROSE_BLOCK = RegisterBlock(new CrossQuadBlock("Rose", BlockShapesRegistry.CrossRoseShape, BlockIDs.ROSE_BLOCK));
            BlockIDs.X_GRASS_BLOCK = RegisterBlock(new CrossQuadBlock("Grass", BlockShapesRegistry.CrossGrassShape, BlockIDs.X_GRASS_BLOCK));
            BlockIDs.DEAD_BUSH_BLOCK = RegisterBlock(new CrossQuadBlock("Dead Bush", BlockShapesRegistry.DeadBushCrossShape, BlockIDs.DEAD_BUSH_BLOCK));

            //building blocks
            BlockIDs.STONE_SLAB_BLOCK = RegisterBlock(new SlabBlock("Stone Slab", BlockShapesRegistry.StoneSlabShape, BlockIDs.STONE_SLAB_BLOCK));  
            BlockIDs.GLASS_BLOCK = RegisterBlock(new GlassBlock("Glass", BlockShapesRegistry.GlassBlockShape, BlockIDs.GLASS_BLOCK));

            //wood
            BlockIDs.OAK_PLANKS_BLOCK = RegisterBlock(new FullBlock("Oak Planks", BlockShapesRegistry.OakPlanksBlockShape, BlockIDs.OAK_PLANKS_BLOCK));
            BlockIDs.OAK_LOG_BLOCK = RegisterBlock(new BlockLog("Oak Log", BlockShapesRegistry.OakLogBlockShape, BlockIDs.OAK_LOG_BLOCK));
            BlockIDs.OAK_SLAB_BLOCK = RegisterBlock(new SlabBlock("Oak Slab", BlockShapesRegistry.OakSlabShape, BlockIDs.OAK_SLAB_BLOCK));
            BlockIDs.OAK_LEAVES_BLOCK = RegisterBlock(new LeavesBlock("Oak Leaves", BlockShapesRegistry.OakLeavesBlockShape, BlockIDs.OAK_LEAVES_BLOCK));

            //
            BlockIDs.SPRUCE_PLANKS_BLOCK = RegisterBlock(new FullBlock("Spruce Planks", BlockShapesRegistry.SprucePlanksBlockShape, BlockIDs.SPRUCE_PLANKS_BLOCK));
            BlockIDs.SPRUCE_LOG_BLOCK = RegisterBlock(new BlockLog("Spruce Log", BlockShapesRegistry.SpruceLogBlockShape, BlockIDs.SPRUCE_LOG_BLOCK));
            BlockIDs.SPRUCE_SLAB_BLOCK = RegisterBlock(new SlabBlock("Spruce Slab", BlockShapesRegistry.SpruceSlabShape, BlockIDs.SPRUCE_SLAB_BLOCK));
            BlockIDs.SPRUCE_LEAVES_BLOCK = RegisterBlock(new LeavesBlock("Spruce Leaves", BlockShapesRegistry.SpruceLeavesBlockShape, BlockIDs.SPRUCE_LEAVES_BLOCK));

            //
            BlockIDs.BIRCH_PLANKS_BLOCK = RegisterBlock(new FullBlock("Birch Planks", BlockShapesRegistry.BirchPlanksBlockShape, BlockIDs.BIRCH_PLANKS_BLOCK));
            BlockIDs.BIRCH_LOG_BLOCK = RegisterBlock(new BlockLog("Birch Log", BlockShapesRegistry.BirchLogBlockShape, BlockIDs.BIRCH_LOG_BLOCK));
            BlockIDs.BIRCH_SLAB_BLOCK = RegisterBlock(new SlabBlock("Birch Slab", BlockShapesRegistry.BirchSlabShape, BlockIDs.BIRCH_SLAB_BLOCK));
            BlockIDs.BIRCH_LEAVES_BLOCK = RegisterBlock(new LeavesBlock("Birch Leaves", BlockShapesRegistry.BirchLeavesBlockShape, BlockIDs.BIRCH_LEAVES_BLOCK));

            //
            BlockIDs.JUNGLE_PLANKS_BLOCK = RegisterBlock(new FullBlock("Jungle Planks", BlockShapesRegistry.JunglePlanksBlockShape, BlockIDs.JUNGLE_PLANKS_BLOCK));
            BlockIDs.JUNGLE_LOG_BLOCK = RegisterBlock(new BlockLog("Jungle Log", BlockShapesRegistry.JungleLogBlockShape, BlockIDs.JUNGLE_LOG_BLOCK));
            BlockIDs.JUNGLE_SLAB_BLOCK = RegisterBlock(new SlabBlock("Jungle Slab", BlockShapesRegistry.JungleSlabShape, BlockIDs.JUNGLE_SLAB_BLOCK));
            BlockIDs.JUNGLE_LEAVES_BLOCK = RegisterBlock(new LeavesBlock("Jungle Leaves", BlockShapesRegistry.JungleLeavesBlockShape, BlockIDs.JUNGLE_LEAVES_BLOCK));
            
            //
            BlockIDs.ICE_BLOCK = RegisterBlock(new FullBlock("Ice Block", BlockShapesRegistry.IceBlockShape, BlockIDs.ICE_BLOCK));
            BlockIDs.GRAVEL_BLOCK = RegisterBlock(new FullBlock("Gravel Block", BlockShapesRegistry.GravelBlockShape, BlockIDs.GRAVEL_BLOCK));
            BlockIDs.CACTUS_BLOCK = RegisterBlock(new FullBlock("Cactus Block", BlockShapesRegistry.CactusBlockShape, BlockIDs.CACTUS_BLOCK));

            //
            BlockIDs.REDSTONE_BLOCK = RegisterBlock(new FullLightBlock("Redstone Block", BlockShapesRegistry.RedstoneBlockShape, BlockIDs.REDSTONE_BLOCK, new Vector3i(15, 0, 0)));
            BlockIDs.EMERALD_BLOCK = RegisterBlock(new FullLightBlock("Emerald Block", BlockShapesRegistry.EmeraldBlockShape, BlockIDs.EMERALD_BLOCK, new Vector3i(0, 15, 0)));
            BlockIDs.LAPIZ_BLOCK = RegisterBlock(new FullLightBlock("Lapiz Block", BlockShapesRegistry.LapizBlockShape, BlockIDs.LAPIZ_BLOCK, new Vector3i(0, 0, 15)));
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
        public static ushort REDSTONE_BLOCK = 0;
        public static ushort EMERALD_BLOCK = 0;
        public static ushort LAPIZ_BLOCK = 0;
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
