using OurCraft.Blocks.Meshing;

namespace OurCraft.Blocks.BlockShapeData
{
    //holds all of the block shapes
    //eventually we will refrence block definitions to refrence the actual json models, in a json file itself
    public static class BlockShapesRegistry
    {
        //Empty
        public static EmptyBlockShape AirBlockShape { get; } = new()
        { };

        //Natural full blocks
        public static FullBlockModelShape GrassBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/GrassBlock.json"))
        };

        public static FullBlockModelShape DirtBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/DirtBlock.json"))
        };

        public static FullBlockModelShape SandBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/SandBlock.json"))
        };

        public static FullBlockModelShape SnowBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/SnowBlock.json"))
        };

        public static FullBlockModelShape SnowyGrassBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/SnowyGrassBlock.json"))
        };

        public static FullBlockModelShape StoneBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/StoneBlock.json"))
        };

        public static FullBlockModelShape WaterBlockShape { get; } = new()
        {
            IsTranslucent = true,
            IsFullOpaqueBlock = false,
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/WaterBlock.json"))
        };

        public static FullBlockModelShape IceBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/IceBlock.json"))
        };

        public static FullBlockModelShape GravelBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/GravelBlock.json"))
        };

        public static FullBlockModelShape CactusBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/CactusBlock.json")),
            IsFullOpaqueBlock = false,
        };

        //Cross-shaped natural blocks
        public static CrossQuadBlockShape CrossRoseShape { get; } = new()
        {
            Tex = TextureIDs.roseTex,
        };

        public static CrossQuadBlockShape CrossGrassShape { get; } = new()
        {
            Tex = TextureIDs.xGrassTex,
        };

        public static CrossQuadBlockShape DeadBushCrossShape { get; } = new()
        {
            Tex = TextureIDs.deadBushTex,
        };

        //Building blocks
        public static SlabBlockModelShape StoneSlabShape { get; } = new()
        {
            cachedModelDouble = CachedBlockModel.BakeBlockModel(BlockModel.Load("Natural/StoneBlock.json")),
            cachedModelBottom = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/StoneSlabBottom.json")),
            cachedModelTop = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/StoneSlabTop.json"))
        };

        public static FullBlockModelShape GlassBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Building/GlassBlock.json")),
            IsFullOpaqueBlock = false,
        };

        // Oak wood set
        public static FullBlockModelShape OakPlanksBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Planks/OakPlanks.json"))
        };

        public static BlockLogModelShape OakLogBlockShape { get; } = new()
        {
            cachedModelX = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/OakLogX.json")),
            cachedModelY = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/OakLogY.json")),
            cachedModelZ = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/OakLogZ.json")),
        };

        public static SlabBlockModelShape OakSlabShape { get; } = new()
        {
            cachedModelDouble = CachedBlockModel.BakeBlockModel(BlockModel.Load("Planks/OakPlanks.json")),
            cachedModelBottom = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/OakSlabBottom.json")),
            cachedModelTop = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/OakSlabTop.json"))
        };

        public static FullBlockModelShape OakLeavesBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Leaves/OakLeaves.json"))
        };

        //Spruce wood set
        public static FullBlockModelShape SprucePlanksBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Planks/SprucePlanks.json"))
        };

        public static BlockLogModelShape SpruceLogBlockShape { get; } = new()
        {
            cachedModelX = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/SpruceLogX.json")),
            cachedModelY = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/SpruceLogY.json")),
            cachedModelZ = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/SpruceLogZ.json")),
        };

        public static SlabBlockModelShape SpruceSlabShape { get; } = new()
        {
            cachedModelDouble = CachedBlockModel.BakeBlockModel(BlockModel.Load("Planks/SprucePlanks.json")),
            cachedModelBottom = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/SpruceSlabBottom.json")),
            cachedModelTop = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/SpruceSlabTop.json"))
        };

        public static FullBlockModelShape SpruceLeavesBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Leaves/SpruceLeaves.json"))
        };

        //Birch wood set
        public static FullBlockModelShape BirchPlanksBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Planks/BirchPlanks.json"))
        };

        public static BlockLogModelShape BirchLogBlockShape { get; } = new()
        {
            cachedModelX = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/BirchLogX.json")),
            cachedModelY = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/BirchLogY.json")),
            cachedModelZ = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/BirchLogZ.json")),
        };

        public static SlabBlockModelShape BirchSlabShape { get; } = new()
        {
            cachedModelDouble = CachedBlockModel.BakeBlockModel(BlockModel.Load("Planks/BirchPlanks.json")),
            cachedModelBottom = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/BirchSlabBottom.json")),
            cachedModelTop = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/BirchSlabTop.json"))
        };

        public static FullBlockModelShape BirchLeavesBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Leaves/BirchLeaves.json"))
        };

        //Jungle wood set
        public static FullBlockModelShape JunglePlanksBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Planks/JunglePlanks.json"))
        };

        public static BlockLogModelShape JungleLogBlockShape { get; } = new()
        {
            cachedModelX = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/JungleLogX.json")),
            cachedModelY = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/JungleLogY.json")),
            cachedModelZ = CachedBlockModel.BakeBlockModel(BlockModel.Load("Logs/JungleLogZ.json")),
        };

        public static SlabBlockModelShape JungleSlabShape { get; } = new()
        {
            cachedModelDouble = CachedBlockModel.BakeBlockModel(BlockModel.Load("Planks/JunglePlanks.json")),
            cachedModelBottom = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/JungleSlabBottom.json")),
            cachedModelTop = CachedBlockModel.BakeBlockModel(BlockModel.Load("Slabs/JungleSlabTop.json"))
        };

        public static FullBlockModelShape JungleLeavesBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Leaves/JungleLeaves.json"))
        };

        //some light sources
        public static FullBlockModelShape RedstoneBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Building/RedstoneBlock.json"))
        };

        public static FullBlockModelShape EmeraldBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Building/EmeraldBlock.json"))
        };

        public static FullBlockModelShape LapizBlockShape { get; } = new()
        {
            cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load("Building/LapizBlock.json"))
        };
    }
}