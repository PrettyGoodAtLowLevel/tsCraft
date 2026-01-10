using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Terrain_Generation.SurfaceFeatures;
using System.Text.Json;

namespace OurCraft.Terrain_Generation
{
    //represents a section of the worlds vegetation and temperature
    public class Biome
    {
        //the name of the biome in data
        public string Name { get; set; } = "new biome";

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

        //surface features list
        public List<BiomeSurfaceFeature> SurfaceFeatures = [];
    }

    //json representation of biome
    public class BiomeJson
    {
        //general info
        public string Name { get; set; } = "NewBiome";
        public string Temperature { get; set; } = "Temperate";
        public string Humidity { get; set; } = "Normal";
        public string Vegetation { get; set; } = "Normal";

        //height configuration
        public BiomeHeightConfig Heights { get; set; } = new();

        //block configuration
        public BiomeBlockConfig Blocks { get; set; } = new();

        //features like trees, flowers, grass
        public List<BiomeFeatureConfig> SurfaceFeatures { get; set; } = new();
    }

    public class BiomeHeightConfig
    {
        public int Regular { get; set; } = 130;
        public int Ocean { get; set; } = 110;
        public int Shore { get; set; } = 127;
        public int Peak { get; set; } = 200;
    }

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

    public class BiomeFeatureConfig
    {
        public string Name { get; set; } = "UnknownFeature";
        public int Chance { get; set; } = 1000;
    }

    //helper for loading biome json and converting it to runtime biome data
    public static class BiomeLoader
    {
        //loads a json config in c# off a file from json file
        public static BiomeJson LoadBiomeConfig(string path)
        {
            string filePath = "C:/Users/alial/OneDrive/Desktop/OurCraft/Resources/Data/WorldGen/Biomes/"+path;
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<BiomeJson>(json)!;
        }

        //converts json c# implementation of biome to cached blockstate info
        public static Biome ToRuntimeBiome(BiomeJson config)
        {
            var biome = new Biome
            {
                Name = config.Name,
                TempIndex = TemperatureIndexFromName(config.Temperature),
                HumidIndex = HumidityIndexFromName(config.Humidity),
                VegetationIndex = VegetationIndexFromName(config.Vegetation),

                RegularHeight = config.Heights.Regular,
                OceanHeight = config.Heights.Ocean,
                ShoreHeight = config.Heights.Shore,
                PeakHeight = config.Heights.Peak,

                WaterBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.Water),
                WaterSurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.WaterSurface),
                SurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.Surface),
                SubSurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.SubSurface),
                PeakSurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.PeakSurface),
                PeakSubSurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.PeakSubSurface),
                ShoreSurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.ShoreSurface),
                ShoreSubSurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.ShoreSubSurface),
                OceanSurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.OceanSurface),
                OceanSubSurfaceBlock = BlockRegistry.GetDefaultBlockState(config.Blocks.OceanSubSurface),
            };

            foreach (var feature in config.SurfaceFeatures)
            {
                biome.SurfaceFeatures.Add(new BiomeSurfaceFeature(SurfaceFeatureRegistry.GetFeature(feature.Name), feature.Chance));
            }

            return biome;
        }

        //parsing biome data enums
        public static int TemperatureIndexFromName(string temp)
        {
            return (int)((TemperatureIndex)Enum.Parse(typeof(TemperatureIndex), temp.ToUpper()));
        }

        public static int HumidityIndexFromName(string humid)
        {
            return (int)((HumidityIndex)Enum.Parse(typeof(HumidityIndex), humid.ToUpper()));
        }

        public static int VegetationIndexFromName(string humid)
        {
            return (int)((VegetationIndex)Enum.Parse(typeof(VegetationIndex), humid.ToUpper()));
        }
    }
}
