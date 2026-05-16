using OurCraft.Blocks;
using OurCraft.Terrain_Generation.SurfaceFeatures;
using OurCraft.Utility;
using System.Text.Json;

//contains all json configs of surface features
namespace OurCraft.Terrain_Generation
{
    //loads json config of oak tree to game state
    public class OakTreeJson
    {
        public string Name { get; set; } = "";
        public string LogBlock { get; set; } = "";
        public string LeavesBlock { get; set; } = "";
        public string PlaceOn { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<OakTreeJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            Tree tree = new Tree(BlockRegistry.GetDefaultBlockState(result.PlaceOn),
            BlockRegistry.GetDefaultBlockState(result.LogBlock), BlockRegistry.GetDefaultBlockState(result.LeavesBlock));

            SurfaceFeatureRegistry.AddFeature(tree, result.Name);
        }
    }

    //loads json config of spruce tree to game state
    public class SpruceTreeJson
    {
        public string Name { get; set; } = "";
        public string LogBlock { get; set; } = "";
        public string LeavesBlock { get; set; } = "";
        public string PlaceOn { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<SpruceTreeJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            SpruceTree tree = new SpruceTree(BlockRegistry.GetDefaultBlockState(result.PlaceOn),
            BlockRegistry.GetDefaultBlockState(result.LogBlock), BlockRegistry.GetDefaultBlockState(result.LeavesBlock));

            SurfaceFeatureRegistry.AddFeature(tree, result.Name);
        }
    }

    //loads json config of fallen log to game state
    public class FallenLogJson
    {
        public string Name { get; set; } = "";
        public string LogBlock { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<FallenLogJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            FallenLog fallenLog = new FallenLog(BlockRegistry.GetDefaultBlockState(result.LogBlock));
            SurfaceFeatureRegistry.AddFeature(fallenLog, result.Name);
        }
    }

    //loads json config of a jungle tree to game state
    public class JungleTreeJson
    {
        public string Name { get; set; } = "";
        public string LogBlock { get; set; } = "";
        public string LeavesBlock { get; set; } = "";
        public string PlaceOn { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<JungleTreeJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            JungleTree tree = new JungleTree(BlockRegistry.GetDefaultBlockState(result.PlaceOn),
            BlockRegistry.GetDefaultBlockState(result.LogBlock), BlockRegistry.GetDefaultBlockState(result.LeavesBlock));

            SurfaceFeatureRegistry.AddFeature(tree, result.Name);
        }
    }

    //loads json config of a mega spruce tree to game state
    public class MegaSpruceTreeJson
    {
        public string Name { get; set; } = "";
        public string LogBlock { get; set; } = "";
        public string LeavesBlock { get; set; } = "";
        public string PlaceOn { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<MegaSpruceTreeJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            MegaSpruceTree tree = new MegaSpruceTree(BlockRegistry.GetDefaultBlockState(result.PlaceOn),
            BlockRegistry.GetDefaultBlockState(result.LogBlock), BlockRegistry.GetDefaultBlockState(result.LeavesBlock));

            SurfaceFeatureRegistry.AddFeature(tree, result.Name);
        }
    }

    //loads json config of a plant to game state
    public class PlantJson
    {
        public string Name { get; set; } = "";
        public string PlaceOn { get; set; } = "";
        public string AltPlaceOn { get; set; } = "";
        public string PlantBlock { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<PlantJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            Plant plant = new Plant(BlockRegistry.GetDefaultBlockState(result.PlaceOn),
            BlockRegistry.GetDefaultBlockState(result.PlantBlock), BlockRegistry.GetDefaultBlockState(result.AltPlaceOn));

            SurfaceFeatureRegistry.AddFeature(plant, result.Name);
        }
    }

    //loads json config of a cactus to game state
    public class CactusJson
    {
        public string Name { get; set; } = "";
        public string PlaceOn { get; set; } = "";
        public string CactusBlock { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<CactusJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            Cactus cactus = new Cactus(BlockRegistry.GetDefaultBlockState(result.PlaceOn),
            BlockRegistry.GetDefaultBlockState(result.CactusBlock));

            SurfaceFeatureRegistry.AddFeature(cactus, result.Name);
        }
    }

    //loads json config of bush to game state
    public class BushJson
    {
        public string Name { get; set; } = "";
        public string LogBlock { get; set; } = "";
        public string LeavesBlock { get; set; } = "";
        public string PlaceOn { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<BushJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            Bush bush = new Bush(BlockRegistry.GetDefaultBlockState(result.PlaceOn),
            BlockRegistry.GetDefaultBlockState(result.LogBlock), BlockRegistry.GetDefaultBlockState(result.LeavesBlock));

            SurfaceFeatureRegistry.AddFeature(bush, result.Name);
        }
    }

    //loads json config of boulder into game state
    public class BoulderJson
    {
        public string Name { get; set; } = "";
        public string RockBlock { get; set; } = "";

        public static void LoadJsonConfig(string fileName)
        {
            string filePath = FileConstants.WORLD_GEN_DATA_PATH + "SurfaceFeatures/";
            string path = filePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<BoulderJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"[SurfaceFeatureRegistry] Surface feature '{fileName}' not found!");
                return;
            }

            Boulder boulder = new Boulder(BlockRegistry.GetDefaultBlockState(result.RockBlock));
            SurfaceFeatureRegistry.AddFeature(boulder, result.Name);
        }
    }
}