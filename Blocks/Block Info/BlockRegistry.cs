using OurCraft.Blocks.Block_Info;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks
{
    //to add a block,
    //1. create a json file config for the block, see json block config for documentation
    //2. use block loader's "Register Block" 'BlockLoader.RegisterBlockType(path))'
    //optionally you can cache the block by assigning it to the respective type
    public static class BlockRegistry //all blocks stored here
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
            BlockLoader.RegisterFullBlock("Natural/GrassBlock.json");
            BlockLoader.RegisterFullBlock("Natural/DirtBlock.json");
            BlockLoader.RegisterFullBlock("Natural/StoneBlock.json");
            BlockLoader.RegisterFullBlock("Natural/SandBlock.json");
            BlockLoader.RegisterFullBlock("Natural/CactusBlock.json");

            BlockLoader.RegisterFullBlock("Natural/SnowBlock.json");
            BlockLoader.RegisterFullBlock("Natural/SnowyGrassBlock.json");           
            BlockLoader.RegisterFullBlock("Natural/IceBlock.json");
            BlockLoader.RegisterFullBlock("Natural/GravelBlock.json");
            BlockLoader.RegisterFullBlock("Natural/SlimeBlock.json");

            BlockLoader.RegisterWaterBlock("Natural/WaterBlock.json");
            BlockLoader.RegisterCrossBlock("Natural/Rose.json");
            BlockLoader.RegisterCrossBlock("Natural/Grass.json");
            BlockLoader.RegisterCrossBlock("Natural/DeadBush.json");

            BlockLoader.RegisterLeavesBlock("Natural/OakLeaves.json");
            BlockLoader.RegisterLeavesBlock("Natural/SpruceLeaves.json");
            BlockLoader.RegisterLeavesBlock("Natural/BirchLeaves.json");
            BlockLoader.RegisterLeavesBlock("Natural/JungleLeaves.json");
            BlockLoader.RegisterLeavesBlock("Natural/AutumnLeaves.json");
            BlockLoader.RegisterLeavesBlock("Natural/CrimsonLeaves.json");
            BlockLoader.RegisterLeavesBlock("Natural/CherryLeaves.json");

            BlockLoader.RegisterLogBlock("Logs/OakLog.json");
            BlockLoader.RegisterLogBlock("Logs/SpruceLog.json");
            BlockLoader.RegisterLogBlock("Logs/BirchLog.json");
            BlockLoader.RegisterLogBlock("Logs/JungleLog.json");

            BlockLoader.RegisterGlassBlock("Building/GlassBlock.json");
            BlockLoader.RegisterFullBlock("Planks/OakPlanks.json");
            BlockLoader.RegisterFullBlock("Planks/SprucePlanks.json");
            BlockLoader.RegisterFullBlock("Planks/BirchPlanks.json");
            BlockLoader.RegisterFullBlock("Planks/JunglePlanks.json");

            BlockLoader.RegisterSlabBlock("Natural/StoneSlab.json");
            BlockLoader.RegisterSlabBlock("Planks/OakSlab.json");
            BlockLoader.RegisterSlabBlock("Planks/SpruceSlab.json");
            BlockLoader.RegisterSlabBlock("Planks/BirchSlab.json");
            BlockLoader.RegisterSlabBlock("Planks/JungleSlab.json");

            BlockLoader.RegisterFullLightBlock("Building/RedStoneBlock.json");
            BlockLoader.RegisterFullLightBlock("Building/EmeraldBlock.json");
            BlockLoader.RegisterFullLightBlock("Building/LapizBlock.json");
        }
    }
}
