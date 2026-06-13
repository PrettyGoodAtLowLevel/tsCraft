using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Terrain_Generation.Registries;
using OurCraft.Utility;
using System.Text.Json;

namespace OurCraft.Terrain_Generation
{
    //represents how a deposit is placed in the world and what shape it takes on
    public enum DepositShape
    {
        VEIN,   //ore veins
        PATCH,  //gravel patches, sand patches
        LAYER,  //squarish layer
        DISC    //circlish layer
    }

    //different distribution of minerals in the game
    public class Deposit
    {
        public BlockState block;

        public int minY;
        public int maxY;

        public int spawnAttempts;      //higher = rarer
        public int spawnChance;

        public int minSize;
        public int maxSize;

        public List<BlockState> replacementBlocks = [];
        public DepositShape placementType;

        public Deposit() { }

        public static void RegisterDeposit(string fileName)
        {
            DepositJson depositJson = DepositJson.LoadDepositConfig(fileName);
            Deposit deposit = Deposit.LoadDeposit(depositJson);

            DepositRegistry.AddDeposit(deposit, depositJson.Name);
        }

        public static Deposit LoadDeposit(DepositJson json)
        {
            Deposit deposit = new()
            {
                block = BlockRegistry.GetDefaultBlockState(json.Block),

                minY = json.MinY,
                maxY = json.MaxY,

                spawnAttempts = json.SpawnAttempts,
                spawnChance = json.SpawnChance,

                minSize = json.MinSize,
                maxSize = json.MaxSize,

                placementType = (DepositShape)Enum.Parse(typeof(DepositShape), json.PlacementType),
                replacementBlocks = []
            };

            foreach(var block in json.ReplacementBlocks)
            {
                deposit.replacementBlocks.Add(BlockRegistry.GetDefaultBlockState(block));
            }

            return deposit;
        }
    }

    //used for loading ores from json files
    public class DepositJson
    {
        public string Name { get; set; } = "";
        public string Block { get; set; } = "";

        public int MinY { get; set; }
        public int MaxY { get; set; }

        public int SpawnAttempts { get; set; }
        public int SpawnChance { get; set; }

        public int MinSize { get; set; }
        public int MaxSize { get; set; }

        public string PlacementType { get; set; } = "";

        public string[] ReplacementBlocks { get; set; } = [];

        public static DepositJson LoadDepositConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "Deposits/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<DepositJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"Deposit '{fileName}' not found!");
                return new();
            }

            return result;
        }
    }

    //represents placed deposit
    public readonly struct DepositInstance
    {
        public readonly Deposit Deposit;

        public readonly int WorldX;
        public readonly int WorldY;
        public readonly int WorldZ;

        public readonly int Size;
        public readonly int Seed;

        public DepositInstance(Deposit deposit, int worldX, int worldY, int worldZ, int size, int seed)
        {
            Deposit = deposit;

            WorldX = worldX;
            WorldY = worldY;
            WorldZ = worldZ;

            Size = size;
            Seed = seed;
        }
    }

}