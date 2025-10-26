using OurCraft.Blocks;
using OurCraft.World.Terrain_Generation.SurfaceFeatures.SurfaceFeatureImplementations;

namespace OurCraft.World.Terrain_Generation.SurfaceFeatures
{
    //holds all surface features at the start so we only pass around refrences to
    //surface features - fast
    public static class SurfaceFeatureRegistry
    {
        //plants
        public static Plant Grass { get; private set; }
        public static Plant Rose { get; private set; }
        public static Plant DeadBush { get; private set; }
        public static Plant OakLeaves { get; private set; }
        public static Plant SpruceLeaves { get; private set; }
        public static Plant JungleLeaves { get; private set; }

        //log like
        public static FallenLog OakLog { get; private set; }
        public static FallenLog SpruceLog { get; private set; }
        public static FallenLog JungleLog { get; private set; }
        public static Cactus Cactus { get; private set; }
        public static Cactus IceCactus { get; private set; }

        //trees
        public static Tree OakTree { get; private set; }
        public static Tree BirchTree { get; private set; }      
        public static Tree JungleTree { get; private set; }
        public static SpruceTree SpruceTree { get; private set; }

        //weird trees
        public static Tree SpruceOakTree { get; private set; }
        public static SpruceTree OakSpruceTree { get; private set; }
        public static SpruceTree FrozenTree { get; private set; }

        //tall trees
        public static TallOakTree TallOakTree { get; private set; }
        public static TallSpruceTree TallSpruceTree { get; private set; }
        public static TallOakTree TallOakJungleTree { get; private set; }
        public static TallSpruceTree TallSpruceJungleTree { get; private set; }

        static SurfaceFeatureRegistry()
        {
            //one block plants
            Grass = new()
            {
                Name = "Grass Plant",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                BlockID = BlockRegistry.GetBlock("Grass")
            };

            Rose = new()
            {
                Name = "Rose Plant",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Grass Block"),
                BlockID = BlockRegistry.GetBlock("Rose")
            };

            DeadBush = new()
            {
                Name = "Dead Bush Plant",
                PlaceOn = BlockRegistry.GetBlock("Sand"),
                AltPlaceOn = BlockRegistry.GetBlock("Snow"),
                BlockID = BlockRegistry.GetBlock("Dead Bush")
            };

            OakLeaves = new()
            {
                Name = "Oak Leaves Plant",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                BlockID = BlockRegistry.GetBlock("Oak Leaves")
            };

            SpruceLeaves = new()
            {
                Name = "Spruce Leaves Plant",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                BlockID = BlockRegistry.GetBlock("Spruce Leaves")
            };

            JungleLeaves = new()
            {
                Name = "Jungle Leaves Plant",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                BlockID = BlockRegistry.GetBlock("Jungle Leaves")
            };



            //logs and cacti, tube like
            OakLog = new()
            {
                Name = "Fallen Oak Log",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                BlockID = BlockRegistry.GetBlock("Oak Log"),
            };

            SpruceLog = new()
            {
                Name = "Fallen Spruce Log",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                BlockID = BlockRegistry.GetBlock("Spruce Log"),
            };

            JungleLog = new()
            {
                Name = "Fallen Jungle Log",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                BlockID = BlockRegistry.GetBlock("Jungle Log"),
            };

            Cactus = new()
            {
                Name = "Cactus",
                PlaceOn = BlockRegistry.GetBlock("Sand"),
                AltPlaceOn = BlockRegistry.GetBlock("Grass Block"),
                BlockID = BlockRegistry.GetBlock("Cactus Block")
            };

            IceCactus = new()
            {
                Name = "Icy Cactus",
                PlaceOn = BlockRegistry.GetBlock("Snow"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                BlockID = BlockRegistry.GetBlock("Ice Block")
            };


            //trees
            OakTree = new()
            {
                Name = "Oak Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Oak Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Oak Log"),
            };

            BirchTree = new()
            {
                Name = "Birch Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Birch Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Birch Log"),
            };

            JungleTree = new()
            {
                Name = "Jungle Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Jungle Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Jungle Log"),
            };

            SpruceTree = new ()
            {
                Name = "Spruce Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Spruce Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Spruce Log"),
            };

            //weird trees
            SpruceOakTree = new()
            {
                Name = "Spruce Oak Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Spruce Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Spruce Log"),
            };

            OakSpruceTree = new()
            {
                Name = "Oak Spruce Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Oak Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Oak Log"),
            };

            FrozenTree = new()
            {
                Name = "Frozen Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Snow"),
                LogBlockID = BlockRegistry.GetBlock("Ice Block"),
            };

            //tall trees
            TallOakTree = new()
            {
                Name = "Tall Oak Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Oak Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Oak Log"),
            };

            TallSpruceTree = new()
            {
                Name = "Tall Spruce Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Spruce Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Spruce Log"),
            };

            TallOakJungleTree = new()
            {
                Name = "Tall Oak Jungle Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Jungle Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Jungle Log"),
            };

            TallSpruceJungleTree = new()
            {
                Name = "Tall Spruce Jungle Tree",
                PlaceOn = BlockRegistry.GetBlock("Grass Block"),
                AltPlaceOn = BlockRegistry.GetBlock("Snowy Grass Block"),
                LeavesBlockID = BlockRegistry.GetBlock("Jungle Leaves"),
                LogBlockID = BlockRegistry.GetBlock("Jungle Log"),
            };
        }
    }
}
