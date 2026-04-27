using OurCraft.Blocks;
using OurCraft.Terrain_Generation.SurfaceFeatures;

namespace OurCraft.Terrain_Generation
{
    //contains all surface features in a feature map
    public static class SurfaceFeatureRegistry
    {
        static readonly Dictionary<string, SurfaceFeature> featureMap = [];      

        public static void InitSurfaceFeatures()
        {
            OakTreeJson.LoadJsonConfig("OakTree.json");
            OakTreeJson.LoadJsonConfig("JungleOakTree.json");
            OakTreeJson.LoadJsonConfig("BirchOakTree.json");
            OakTreeJson.LoadJsonConfig("SpruceOakTree.json");
            OakTreeJson.LoadJsonConfig("AutumnOakTree.json");

            SpruceTreeJson.LoadJsonConfig("SpruceTree.json");
            SpruceTreeJson.LoadJsonConfig("FrozenSpruceTree.json");
            SpruceTreeJson.LoadJsonConfig("CherrySpruceTree.json");
            SpruceTreeJson.LoadJsonConfig("CrimsonSpruceTree.json");
            SpruceTreeJson.LoadJsonConfig("AutumnSpruceTree.json");

            MegaSpruceTreeJson.LoadJsonConfig("MegaSpruceTree.json");
            MegaSpruceTreeJson.LoadJsonConfig("AutumnMegaSpruceTree.json");
            MegaSpruceTreeJson.LoadJsonConfig("CrimsonMegaSpruceTree.json");

            JungleTreeJson.LoadJsonConfig("JungleTree.json");

            FallenLogJson.LoadJsonConfig("FallenOakLog.json");
            FallenLogJson.LoadJsonConfig("FallenJungleLog.json");

            PlantJson.LoadJsonConfig("GrassPlant.json");
            CactusJson.LoadJsonConfig("Cactus.json");

            BushJson.LoadJsonConfig("OakBush.json");
            BushJson.LoadJsonConfig("JungleBush.json");

            BoulderJson.LoadJsonConfig("StoneBoulder.json");
            BoulderJson.LoadJsonConfig("IceBoulder.json");
        }

        public static SurfaceFeature GetFeature(string name)
        {
            if (featureMap.TryGetValue(name, out var feature)) return feature;

            Console.WriteLine($"[SurfaceFeatureRegistry] Warning: Feature '{name}' not found!");
            return new Plant(Block.AIR, Block.AIR, Block.AIR); //empty surface feature
        }

        public static void AddFeature(SurfaceFeature feature, string name)
        {
            featureMap.TryAdd(name, feature);
        }
    }
}