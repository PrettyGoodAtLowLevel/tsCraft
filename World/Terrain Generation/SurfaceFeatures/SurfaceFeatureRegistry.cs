using OurCraft.Blocks;
using OurCraft.World.Terrain_Generation.SurfaceFeatures.SurfaceFeatureImplementations;

namespace OurCraft.World.Terrain_Generation.SurfaceFeatures
{
    //holds all surface features at the start so we only pass around refrences to
    //surface features - fast
    public static class SurfaceFeatureRegistry
    {
        public static Dictionary<string, SurfaceFeature> featureMap = new Dictionary<string, SurfaceFeature>();

        //plants
        public static Plant Grass { get; private set; } = new();
        public static Plant Rose { get; private set; } = new();
        public static Plant DeadBush { get; private set; } = new();
        public static Plant OakLeaves { get; private set; } = new();
        public static Plant SpruceLeaves { get; private set; } = new();
        public static Plant JungleLeaves { get; private set; } = new();

        //log like
        public static FallenLog OakLog { get; private set; } = new();
        public static FallenLog SpruceLog { get; private set; } = new();
        public static FallenLog JungleLog { get; private set; } = new();
        public static Cactus Cactus { get; private set; } = new();
        public static Cactus IceCactus { get; private set; } = new();

        //trees
        public static Tree OakTree { get; private set; } = new();
        public static Tree BirchTree { get; private set; } = new();
        public static Tree JungleTree { get; private set; } = new();
        public static SpruceTree SpruceTree { get; private set; } = new();

        //weird trees
        public static Tree SpruceOakTree { get; private set; } = new();
        public static SpruceTree OakSpruceTree { get; private set; } = new();
        public static SpruceTree FrozenTree { get; private set; } = new();

        //tall trees
        public static TallOakTree TallOakTree { get; private set; } = new();
        public static TallSpruceTree TallSpruceTree { get; private set; } = new();
        public static TallOakTree TallOakJungleTree { get; private set; } = new();
        public static TallSpruceTree TallSpruceJungleTree { get; private set; } = new();

        //bushes
        public static Bush OakBush { get; private set; } = new();
        public static Bush SpruceBush { get; private set; } = new();
        public static Bush JungleBush { get; private set; } = new();

        public static void InitializeFeatures()
        {
            //one block plants
            Grass = new()
            {
                Name = "Grass Plant",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                PlantBlock = BlockRegistry.GetDefaultBlockState("Grass")
            };

            Rose = new()
            {
                Name = "Rose Plant",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                PlantBlock = BlockRegistry.GetDefaultBlockState("Rose")
            };

            DeadBush = new()
            {
                Name = "Dead Bush Plant",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Sand"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snow"),
                PlantBlock = BlockRegistry.GetDefaultBlockState("Dead Bush")
            };

            OakLeaves = new()
            {
                Name = "Oak Leaves Plant",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                PlantBlock = BlockRegistry.GetDefaultBlockState("Oak Leaves")
            };

            SpruceLeaves = new()
            {
                Name = "Spruce Leaves Plant",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                PlantBlock = BlockRegistry.GetDefaultBlockState("Spruce Leaves")
            };

            JungleLeaves = new()
            {
                Name = "Jungle Leaves Plant",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                PlantBlock = BlockRegistry.GetDefaultBlockState("Jungle Leaves")
            };

            //logs and cacti, tube like
            OakLog = new()
            {
                Name = "Fallen Oak Log",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Oak Log"),
            };

            SpruceLog = new()
            {
                Name = "Fallen Spruce Log",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Spruce Log"),
            };

            JungleLog = new()
            {
                Name = "Fallen Jungle Log",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Jungle Log"),
            };

            Cactus = new()
            {
                Name = "Cactus",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Sand"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                CactusBlock = BlockRegistry.GetDefaultBlockState("Cactus Block")
            };

            IceCactus = new()
            {
                Name = "Icy Cactus",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Snow"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                CactusBlock = BlockRegistry.GetDefaultBlockState("Ice Block")
            };


            //trees
            OakTree = new()
            {
                Name = "Oak Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Oak Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Oak Log"),
            };

            BirchTree = new()
            {
                Name = "Birch Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Birch Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Birch Log"),
            };

            JungleTree = new()
            {
                Name = "Jungle Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Jungle Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Jungle Log"),
            };

            SpruceTree = new()
            {
                Name = "Spruce Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Spruce Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Spruce Log"),
            };

            //weird trees
            SpruceOakTree = new()
            {
                Name = "Spruce Oak Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Spruce Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Spruce Log"),
            };

            OakSpruceTree = new()
            {
                Name = "Oak Spruce Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Oak Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Oak Log"),
            };

            FrozenTree = new()
            {
                Name = "Frozen Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Snow"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Ice Block"),
            };

            //tall trees
            TallOakTree = new()
            {
                Name = "Tall Oak Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Oak Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Oak Log"),
            };

            TallSpruceTree = new()
            {
                Name = "Tall Spruce Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Spruce Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Spruce Log"),
            };

            TallOakJungleTree = new()
            {
                Name = "Tall Oak Jungle Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Jungle Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Jungle Log"),
            };

            TallSpruceJungleTree = new()
            {
                Name = "Tall Spruce Jungle Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Jungle Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Jungle Log"),
            };

            //bushes
            OakBush = new()
            {
                Name = "Oak Bush",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Oak Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Oak Log"),
            };

            SpruceBush = new()
            {
                Name = "Tall Spruce Tree",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Spruce Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Spruce Log"),
            };

            JungleBush = new()
            {
                Name = "Jungle Bush",
                PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block"),
                AltPlaceOn = BlockRegistry.GetDefaultBlockState("Snowy Grass Block"),
                LeavesBlock = BlockRegistry.GetDefaultBlockState("Jungle Leaves"),
                LogBlock = BlockRegistry.GetDefaultBlockState("Jungle Log"),
            };
            RegisterFeatures();
        }

        public static void RegisterFeatures()
        {
            //register all surface features in the featureMap
            featureMap = new Dictionary<string, SurfaceFeature>
            {
                //plants
                { "Grass", Grass },
                { "Rose", Rose },
                { "DeadBush", DeadBush },
                { "OakLeaves", OakLeaves },
                { "SpruceLeaves", SpruceLeaves },
                { "JungleLeaves", JungleLeaves },

                //logs / cacti
                { "OakLog", OakLog },
                { "SpruceLog", SpruceLog },
                { "JungleLog", JungleLog },
                { "Cactus", Cactus },
                { "IceCactus", IceCactus },

                //trees
                { "OakTree", OakTree },
                { "BirchTree", BirchTree },
                { "JungleTree", JungleTree },
                { "SpruceTree", SpruceTree },

                //weird trees
                { "SpruceOakTree", SpruceOakTree },
                { "OakSpruceTree", OakSpruceTree },
                { "FrozenTree", FrozenTree },

                //tall trees
                { "TallOakTree", TallOakTree },
                { "TallSpruceTree", TallSpruceTree },
                { "TallOakJungleTree", TallOakJungleTree },
                { "TallSpruceJungleTree", TallSpruceJungleTree },

                //bushes
                { "OakBush", OakBush },
                { "SpruceBush", SpruceBush },
                { "JungleBush", JungleBush }
            };
        }

        public static SurfaceFeature GetFeature(string name)
        {
            if (featureMap.TryGetValue(name, out var feature))
                return feature;

            Console.WriteLine($"[SurfaceFeatureRegistry] Warning: Feature '{name}' not found!");
            return Grass;
        }

    }
}
