using OurCraft.Blocks;
using OurCraft.World.Terrain_Generation.SurfaceFeatures;
using System.Text.Json;
using System.Xml.Linq;

namespace OurCraft.World.Terrain_Generation
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
        public ushort WaterBlock { get; set; } = BlockIDs.WATER_BLOCK;
        public ushort WaterSurfaceBlock { get; set; } = BlockIDs.WATER_BLOCK;
        public ushort SurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
        public ushort SubSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;

        public ushort PeakSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
        public ushort PeakSubSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;

        public ushort ShoreSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
        public ushort ShoreSubSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;

        public ushort OceanSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
        public ushort OceanSubSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;

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
        public static BiomeJson LoadBiomeConfig(string path)
        {
            string filePath = "C:/Users/alial/OneDrive/Desktop/OurCraft/Resources/Data/WorldGen/Biomes/"+path;
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<BiomeJson>(json)!;
        }

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

                WaterBlock = BlockRegistry.GetBlock(config.Blocks.Water),
                WaterSurfaceBlock = BlockRegistry.GetBlock(config.Blocks.WaterSurface),
                SurfaceBlock = BlockRegistry.GetBlock(config.Blocks.Surface),
                SubSurfaceBlock = BlockRegistry.GetBlock(config.Blocks.SubSurface),
                PeakSurfaceBlock = BlockRegistry.GetBlock(config.Blocks.PeakSurface),
                PeakSubSurfaceBlock = BlockRegistry.GetBlock(config.Blocks.PeakSubSurface),
                ShoreSurfaceBlock = BlockRegistry.GetBlock(config.Blocks.ShoreSurface),
                ShoreSubSurfaceBlock = BlockRegistry.GetBlock(config.Blocks.ShoreSubSurface),
                OceanSurfaceBlock = BlockRegistry.GetBlock(config.Blocks.OceanSurface),
                OceanSubSurfaceBlock = BlockRegistry.GetBlock(config.Blocks.OceanSubSurface),
            };

            foreach (var feature in config.SurfaceFeatures)
            {
                biome.SurfaceFeatures.Add(
                    new BiomeSurfaceFeature(
                        SurfaceFeatureRegistry.GetFeature(feature.Name),
                        feature.Chance
                    )
                );
            }

            return biome;
        }

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
