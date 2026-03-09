using OurCraft.Blocks.Block_Info;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks
{
    //to add a block,
    //1. create a json file config for the block, see json block config for documentation
    //2. use block loader's "Register Block" 'BlockLoader.RegisterBlockType(path))'
    //optionally you can cache the block by assigning it to the respective type
    public static class BlockRegistry
    {
        //all blocks
        static readonly List<Block> blocks = [];
        //map of string to block ids
        static readonly Dictionary<string, ushort> blockRegistry = [];
        
        public static int BlockCount => blocks.Count;
        public static int MaxBlockID => blocks.Count - 1;

        public static Block GetBlock(int ID) => blocks[ID];
        public static void AddBlockList(Block block) => blocks.Add(block);
        public static void AddBlockRegistry(string name, ushort id) => blockRegistry.TryAdd(name, id);
        public static ushort GetBlockID(string name) => blockRegistry[name];
        public static BlockState GetDefaultBlockState(string name) => GetBlock(GetBlockID(name)).DefaultState;

        public static void InitBlocks()
        {
            BlockLoader.RegisterAirBlock("Air");
            BlockLoader.RegisterFullBlock("GrassBlock.json");
            BlockLoader.RegisterFullBlock("DirtBlock.json");
            BlockLoader.RegisterFullBlock("StoneBlock.json");
            BlockLoader.RegisterFullBlock("SandBlock.json");

            BlockLoader.RegisterFullBlock("SnowBlock.json");
            BlockLoader.RegisterFullBlock("SnowyGrassBlock.json");           
            BlockLoader.RegisterFullBlock("IceBlock.json");
            BlockLoader.RegisterFullBlock("GravelBlock.json");

            BlockLoader.RegisterWaterBlock("WaterBlock.json");
            BlockLoader.RegisterCrossBlock("Rose.json");
            BlockLoader.RegisterCrossBlock("Grass.json");
            BlockLoader.RegisterCrossBlock("DeadBush.json");

            BlockLoader.RegisterLeavesBlock("OakLeaves.json");
            BlockLoader.RegisterLeavesBlock("SpruceLeaves.json");
            BlockLoader.RegisterLeavesBlock("BirchLeaves.json");
            BlockLoader.RegisterLeavesBlock("JungleLeaves.json");

            BlockLoader.RegisterLogBlock("OakLog.json");
            BlockLoader.RegisterLogBlock("SpruceLog.json");
            BlockLoader.RegisterLogBlock("BirchLog.json");
            BlockLoader.RegisterLogBlock("JungleLog.json");

            BlockLoader.RegisterGlassBlock("GlassBlock.json");
            BlockLoader.RegisterFullBlock("OakPlanks.json");
            BlockLoader.RegisterFullBlock("SprucePlanks.json");
            BlockLoader.RegisterFullBlock("BirchPlanks.json");
            BlockLoader.RegisterFullBlock("JunglePlanks.json");

            BlockLoader.RegisterSlabBlock("StoneSlab.json");
            BlockLoader.RegisterSlabBlock("OakSlab.json");
            BlockLoader.RegisterSlabBlock("SpruceSlab.json");
            BlockLoader.RegisterSlabBlock("BirchSlab.json");
            BlockLoader.RegisterSlabBlock("JungleSlab.json");

            BlockLoader.RegisterFullLightBlock("RedStoneBlock.json");
            BlockLoader.RegisterFullLightBlock("EmeraldBlock.json");
            BlockLoader.RegisterFullLightBlock("LapizBlock.json");
        }
    }
}
