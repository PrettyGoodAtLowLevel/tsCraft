using OurCraft.Blocks;
using OurCraft.Utility;
using System.Text.Json;
using OurCraft.Terrain_Generation.Registries;
using OpenTK.Mathematics;

namespace OurCraft.Terrain_Generation
{
    //these enums are soley used for a lookup chart and dont effect the behavior of the biome, only the placement
    //measures overall temperature
    public enum TemperatureIndex
    {
        FREEZING,   //tundras, cold oceans
        COLD,       //taigas
        TEMPERATE,  //gravelly hills
        WARM,       //plains, forests
        HOT,        //savannas, deserts
    }

    //measures rainfall level
    public enum HumidityIndex
    {
        ARID,       //deserts, icelands
        DRY,        //savanna
        NORMAL,     //plains, forests
        HUMID,      //jungle
        WET         //swamps, mangroves
    }

    //measures plant count
    public enum VegetationIndex
    {
        BARREN,     //desert, plains
        SPARSE,     //sparse forest, gravelly hills
        DENSE       //forest
    }

    //represents a section of the worlds vegetation and temperature
    public class Biome
    {
        //the name of the biome in data
        public string Name { get; set; } = "new biome";
        public Vector3 Color = Vector3.Zero;
        public BiomeSkyConfig Sky { get; set; } = new();

        //the temperature and humidity of the biome for the biome noise
        public int TempIndex { get; set; } = 0;
        public int HumidIndex { get; set; } = 0;
        public int VegetationIndex { get; set; } = 0;

        //height data
        public int RegularHeight { get; set; } = 130;
        public int ShoreHeight { get; set; } = 127;
        public int PeakHeight { get; set; } = 200;
        public int OceanHeight { get; set; } = 110;

        //block data
        public BlockState WaterBlock { get; set; }
        public BlockState WaterSurfaceBlock { get; set; }
        public BlockState SurfaceBlock { get; set; }
        public BlockState SubSurfaceBlock { get; set; }

        public BlockState PeakSurfaceBlock { get; set; }
        public BlockState PeakSubSurfaceBlock { get; set; }

        public BlockState ShoreSurfaceBlock { get; set; }
        public BlockState ShoreSubSurfaceBlock { get; set; }

        public BlockState OceanSurfaceBlock { get; set; }
        public BlockState OceanSubSurfaceBlock { get; set; }

        public List<BiomeSurfaceFeature> features = [];
        public List<Deposit> deposits = [];
    }

    public class BiomeSkyConfig
    {
        public Vector3 skyLightColor = Vector3.Zero;
        public Vector3 zenithColor = Vector3.Zero;
        public Vector3 midSkyColor = Vector3.Zero;
        public Vector3 horizonColor = Vector3.Zero;
        public Vector3 horizonHazeColor = Vector3.Zero;
    }

    //json representation of biome
    public class BiomeJson
    {
        //general info
        public string Name { get; set; } = "NewBiome";
        public string Temperature { get; set; } = "Temperate";
        public string Humidity { get; set; } = "Normal";
        public string Vegetation { get; set; } = "Normal";

        public BiomeJsonSkyConfig SkyConfig { get; set; } = new();

        //height configuration
        public BiomeHeightConfig Heights { get; set; } = new();

        //block configuration
        public BiomeBlockConfig Blocks { get; set; } = new();

        //features like trees, flowers, grass
        public List<BiomeFeatureConfig> SurfaceFeatures { get; set; } = new();

        //deposits, like ores and gravel patches
        public List<string> Deposits { get; set; } = new();
    }

    //what block height is configured to what blocks
    public class BiomeHeightConfig
    {
        public int Regular { get; set; } = 130;
        public int Ocean { get; set; } = 110;
        public int Shore { get; set; } = 127;
        public int Peak { get; set; } = 200;
    }

    //the surface blocks of a biome
    public class BiomeBlockConfig
    {
        public string Water { get; set; } = "Water";
        public string WaterSurface { get; set; } = "Water";
        public string Surface { get; set; } = "Stone";
        public string SubSurface { get; set; } = "Stone";
        public string PeakSurface { get; set; } = "Stone";
        public string PeakSubSurface { get; set; } = "Stone";
        public string ShoreSurface { get; set; } = "Stone";
        public string ShoreSubSurface { get; set; } = "Stone";
        public string OceanSurface { get; set; } = "Stone";
        public string OceanSubSurface { get; set; } = "Stone";
    }

    //name of feature to find in surface feature map + chance to spawn
    public class BiomeFeatureConfig
    {
        public string Name { get; set; } = "Unknown Feature";
        public int Chance { get; set; } = 1000;
    }

    //colors of the sky for a biome
    public class BiomeJsonSkyConfig
    {
        public BiomeJsonColorConfig SkyLightColor { get; set; } = new();
        public BiomeJsonColorConfig ZenithColor { get; set; } = new();
        public BiomeJsonColorConfig MidSkyColor { get; set; } = new();
        public BiomeJsonColorConfig HorizonColor { get; set; } = new();
        public BiomeJsonColorConfig HorizonHazeColor { get; set; } = new();
    }

    //vector 3 json representation
    public class BiomeJsonColorConfig
    {
        public float R { get; set; } = 0.0f;
        public float G { get; set; } = 0.0f;
        public float B { get; set; } = 0.0f;
    }

    //helper for loading biome json and converting it to runtime biome data
    public static class BiomeLoader
    {
        private readonly static string biomePath = FileConstants.WORLD_GEN_DATA_PATH + "Biomes/";

        //loads a json config in c# off a file from json file
        public static BiomeJson LoadBiomeConfig(string fileName)
        {
            string filePath = biomePath + fileName;
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<BiomeJson>(json)!;
        }

        //converts json c# implementation of biome to cached blockstate info and structure + deposit info
        public static Biome ToRuntimeBiome(BiomeJson jsonConfig)
        {
            var biome = new Biome
            {
                Name = jsonConfig.Name,
                TempIndex = TemperatureIndexFromName(jsonConfig.Temperature),
                HumidIndex = HumidityIndexFromName(jsonConfig.Humidity),
                VegetationIndex = VegetationIndexFromName(jsonConfig.Vegetation),

                RegularHeight = jsonConfig.Heights.Regular,
                OceanHeight = jsonConfig.Heights.Ocean,
                ShoreHeight = jsonConfig.Heights.Shore,
                PeakHeight = jsonConfig.Heights.Peak,

                WaterBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.Water),
                WaterSurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.WaterSurface),
                SurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.Surface),
                SubSurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.SubSurface),
                PeakSurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.PeakSurface),
                PeakSubSurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.PeakSubSurface),
                ShoreSurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.ShoreSurface),
                ShoreSubSurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.ShoreSubSurface),
                OceanSurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.OceanSurface),
                OceanSubSurfaceBlock = BlockRegistry.GetDefaultBlockState(jsonConfig.Blocks.OceanSubSurface),
            };

            foreach (var surface in jsonConfig.SurfaceFeatures)
            {
                SurfaceFeature feature = SurfaceFeatureRegistry.GetFeature(surface.Name);
                biome.features.Add(new BiomeSurfaceFeature(feature, surface.Chance));
            }

            foreach(var deposit in jsonConfig.Deposits)
            {
                Deposit dep = DepositRegistry.GetDeposit(deposit);
                biome.deposits.Add(dep);
            }

            biome.Sky = new BiomeSkyConfig
            { 
                skyLightColor = new Vector3(jsonConfig.SkyConfig.SkyLightColor.R,
                jsonConfig.SkyConfig.SkyLightColor.G, jsonConfig.SkyConfig.SkyLightColor.B),

                midSkyColor = new Vector3(jsonConfig.SkyConfig.MidSkyColor.R,
                jsonConfig.SkyConfig.MidSkyColor.G, jsonConfig.SkyConfig.MidSkyColor.B),

                zenithColor = new Vector3(jsonConfig.SkyConfig.ZenithColor.R,
                jsonConfig.SkyConfig.ZenithColor.G, jsonConfig.SkyConfig.ZenithColor.B),

                horizonColor = new Vector3(jsonConfig.SkyConfig.HorizonColor.R,
                jsonConfig.SkyConfig.HorizonColor.G, jsonConfig.SkyConfig.HorizonColor.B),

                horizonHazeColor = new Vector3(jsonConfig.SkyConfig.HorizonHazeColor.R,
                jsonConfig.SkyConfig.HorizonHazeColor.G, jsonConfig.SkyConfig.HorizonHazeColor.B),
            };

            return biome;

        }

        //parsing biome data enums
        static int TemperatureIndexFromName(string temp)
        {
            return (int)((TemperatureIndex)Enum.Parse(typeof(TemperatureIndex), temp.ToUpper()));
        }

        static int HumidityIndexFromName(string humid)
        {
            return (int)((HumidityIndex)Enum.Parse(typeof(HumidityIndex), humid.ToUpper()));
        }

        static int VegetationIndexFromName(string humid)
        {
            return (int)((VegetationIndex)Enum.Parse(typeof(VegetationIndex), humid.ToUpper()));
        }
    }
}
