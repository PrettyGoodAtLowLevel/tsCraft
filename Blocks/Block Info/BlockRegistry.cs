using OurCraft.Blocks.Block_Info;

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

        //loads all blocks, break this up into smaller functions for better readability if you wish to override this
        public static void InitBlocks()
        {
            BlockLoader.RegisterAirBlock("Air");
            BlockLoader.RegisterDefaultBlock("Natural/GrassBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/DirtBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/StoneBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/SandBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/CactusBlock.json");

            BlockLoader.RegisterDefaultBlock("Natural/SnowBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/SnowyGrassBlock.json");           
            BlockLoader.RegisterDefaultBlock("Natural/IceBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/GravelBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/SlimeBlock.json");

            BlockLoader.RegisterDefaultBlock("Natural/WaterBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/WhiteWaterBlock.json");
            BlockLoader.RegisterDefaultBlock("Natural/PurpleWaterBlock.json");

            BlockLoader.RegisterCrossBlock("Natural/Rose.json");
            BlockLoader.RegisterCrossBlock("Natural/Grass.json");
            BlockLoader.RegisterCrossBlock("Natural/DeadBush.json");

            BlockLoader.RegisterDefaultBlock("Natural/OakLeaves.json");
            BlockLoader.RegisterDefaultBlock("Natural/SpruceLeaves.json");
            BlockLoader.RegisterDefaultBlock("Natural/BirchLeaves.json");
            BlockLoader.RegisterDefaultBlock("Natural/JungleLeaves.json");
            BlockLoader.RegisterDefaultBlock("Natural/AutumnLeaves.json");
            BlockLoader.RegisterDefaultBlock("Natural/CrimsonLeaves.json");
            BlockLoader.RegisterDefaultBlock("Natural/CherryLeaves.json");

            BlockLoader.RegisterLogBlock("Logs/OakLog.json");
            BlockLoader.RegisterLogBlock("Logs/SpruceLog.json");
            BlockLoader.RegisterLogBlock("Logs/BirchLog.json");
            BlockLoader.RegisterLogBlock("Logs/JungleLog.json");

            BlockLoader.RegisterDefaultBlock("Building/GlassBlock.json");
            BlockLoader.RegisterDefaultBlock("Planks/OakPlanks.json");
            BlockLoader.RegisterDefaultBlock("Planks/SprucePlanks.json");
            BlockLoader.RegisterDefaultBlock("Planks/BirchPlanks.json");
            BlockLoader.RegisterDefaultBlock("Planks/JunglePlanks.json");

            BlockLoader.RegisterSlabBlock("Natural/StoneSlab.json");
            BlockLoader.RegisterSlabBlock("Planks/OakSlab.json");
            BlockLoader.RegisterSlabBlock("Planks/SpruceSlab.json");
            BlockLoader.RegisterSlabBlock("Planks/BirchSlab.json");
            BlockLoader.RegisterSlabBlock("Planks/JungleSlab.json");

            BlockLoader.RegisterDefaultBlock("Building/RedStoneBlock.json");
            BlockLoader.RegisterDefaultBlock("Building/EmeraldBlock.json");
            BlockLoader.RegisterDefaultBlock("Building/LapizBlock.json");

            BlockLoader.RegisterChestBlock("Building/ChestBlock.json");
        }
    }
}
