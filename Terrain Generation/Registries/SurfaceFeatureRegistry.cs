using OurCraft.Blocks;
using OurCraft.Terrain_Generation.SurfaceFeatures;

namespace OurCraft.Terrain_Generation.Registries
{
    //contains all surface features in a feature map
    public static class SurfaceFeatureRegistry
    {
        static readonly Dictionary<string, SurfaceFeature> featureMap = [];      

        //loads all surface features, put surface features you want in game here
        public static void InitSurfaceFeatures()
        {
            OakTreeJson.LoadJsonConfig("OakTree.json");
            OakTreeJson.LoadJsonConfig("JungleOakTree.json");
            OakTreeJson.LoadJsonConfig("BirchOakTree.json");
            OakTreeJson.LoadJsonConfig("SpruceOakTree.json");
            OakTreeJson.LoadJsonConfig("AutumnOakTree.json");
            OakTreeJson.LoadJsonConfig("CrimsonOakTree.json");

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

        //tries to get a feature, if not found returns empty feature
        public static SurfaceFeature GetFeature(string name)
        {
            if (featureMap.TryGetValue(name, out var feature)) return feature;

            Console.WriteLine($"[SurfaceFeatureRegistry] Warning: Feature '{name}' not found!");
            return new Plant(Block.AIR, Block.AIR, Block.AIR); //empty surface feature
        }

        //adds a new feature to the feature map
        public static void AddFeature(SurfaceFeature feature, string name)
        {
            featureMap.TryAdd(name, feature);
        }
    }
}