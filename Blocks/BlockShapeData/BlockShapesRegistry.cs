using OpenTK.Graphics.ES11;
using OurCraft.Blocks.Meshing;

namespace OurCraft.Blocks.BlockShapeRegistry
{
    //holds all of the block shapes
    public static class BlockShapesRegistry
    {
        //------------------------------------------------
        //Empty
        public static EmptyBlockShape AirBlockShape { get; } = new()
        {
            TopFaceTex = 0, BottomFaceTex = 0, FrontFaceTex = 0, BackFaceTex = 0, RightFaceTex = 0, LeftFaceTex = 0,
        };

        //------------------------------------------------
        //Natural full blocks
        public static FullBlockShape GrassBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.grassTopTex, BottomFaceTex = TextureIDs.dirtTex,
            FrontFaceTex = TextureIDs.grassSideTex, BackFaceTex = TextureIDs.grassSideTex,
            RightFaceTex = TextureIDs.grassSideTex, LeftFaceTex = TextureIDs.grassSideTex,
        };

        public static FullBlockShape DirtBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.dirtTex, BottomFaceTex = TextureIDs.dirtTex,
            FrontFaceTex = TextureIDs.dirtTex, BackFaceTex = TextureIDs.dirtTex,
            RightFaceTex = TextureIDs.dirtTex, LeftFaceTex = TextureIDs.dirtTex,
        };

        public static FullBlockShape SandBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.sandTex, BottomFaceTex = TextureIDs.sandTex,
            FrontFaceTex = TextureIDs.sandTex, BackFaceTex = TextureIDs.sandTex,
            RightFaceTex = TextureIDs.sandTex, LeftFaceTex = TextureIDs.sandTex,
        };

        public static FullBlockShape SnowBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.snowTex, BottomFaceTex = TextureIDs.snowTex,
            FrontFaceTex = TextureIDs.snowTex, BackFaceTex = TextureIDs.snowTex,
            RightFaceTex = TextureIDs.snowTex, LeftFaceTex = TextureIDs.snowTex,
        };

        public static FullBlockShape SnowyGrassBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.snowTex, BottomFaceTex = TextureIDs.dirtTex,
            FrontFaceTex = TextureIDs.snowGrassSideTex, BackFaceTex = TextureIDs.snowGrassSideTex,
            RightFaceTex = TextureIDs.snowGrassSideTex, LeftFaceTex = TextureIDs.snowGrassSideTex,
        };

        public static FullBlockShape StoneBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.stoneTex, BottomFaceTex = TextureIDs.stoneTex,
            FrontFaceTex = TextureIDs.stoneTex, BackFaceTex = TextureIDs.stoneTex,
            RightFaceTex = TextureIDs.stoneTex, LeftFaceTex = TextureIDs.stoneTex,
        };

        public static WaterBlockShape WaterBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.blueWoolTex, BottomFaceTex = TextureIDs.blueWoolTex,
            FrontFaceTex = TextureIDs.blueWoolTex, BackFaceTex = TextureIDs.blueWoolTex,
            RightFaceTex = TextureIDs.blueWoolTex, LeftFaceTex = TextureIDs.blueWoolTex,
        };

        public static FullBlockShape IceBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.iceTex, BottomFaceTex = TextureIDs.iceTex,
            FrontFaceTex = TextureIDs.iceTex, BackFaceTex = TextureIDs.iceTex,
            RightFaceTex = TextureIDs.iceTex, LeftFaceTex = TextureIDs.iceTex,
        };

        public static FullBlockShape GravelBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.gravelTex, BottomFaceTex = TextureIDs.gravelTex,
            FrontFaceTex = TextureIDs.gravelTex, BackFaceTex = TextureIDs.gravelTex,
            RightFaceTex = TextureIDs.gravelTex, LeftFaceTex = TextureIDs.gravelTex,
        };

        public static FullBlockShape CactusBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.cactusTopTex, BottomFaceTex = TextureIDs.cactusBottomTex,
            FrontFaceTex = TextureIDs.cactusSideTex, BackFaceTex = TextureIDs.cactusSideTex,
            RightFaceTex = TextureIDs.cactusSideTex, LeftFaceTex = TextureIDs.cactusSideTex,
        };

        //------------------------------------------------
        //Cross-shaped natural blocks
        public static CrossQuadBlockShape CrossRoseShape { get; } = new()
        {
            TopFaceTex = TextureIDs.roseTex, BottomFaceTex = TextureIDs.roseTex,
            FrontFaceTex = TextureIDs.roseTex, BackFaceTex = TextureIDs.roseTex,
            RightFaceTex = TextureIDs.roseTex, LeftFaceTex = TextureIDs.roseTex,
        };

        public static CrossQuadBlockShape CrossGrassShape { get; } = new()
        {
            TopFaceTex = TextureIDs.xGrassTex, BottomFaceTex = TextureIDs.xGrassTex,
            FrontFaceTex = TextureIDs.xGrassTex, BackFaceTex = TextureIDs.xGrassTex,
            RightFaceTex = TextureIDs.xGrassTex, LeftFaceTex = TextureIDs.xGrassTex,
        };

        public static CrossQuadBlockShape DeadBushCrossShape { get; } = new()
        {
            TopFaceTex = TextureIDs.deadBushTex, BottomFaceTex = TextureIDs.deadBushTex,
            FrontFaceTex = TextureIDs.deadBushTex, BackFaceTex = TextureIDs.deadBushTex,
            RightFaceTex = TextureIDs.deadBushTex, LeftFaceTex = TextureIDs.deadBushTex,
        };

        //------------------------------------------------
        //Building blocks
        public static SlabBlockShape StoneSlabShape { get; } = new()
        {
            TopFaceTex = TextureIDs.stoneTex, BottomFaceTex = TextureIDs.stoneTex,
            FrontFaceTex = TextureIDs.stoneTex, BackFaceTex = TextureIDs.stoneTex,
            RightFaceTex = TextureIDs.stoneTex, LeftFaceTex = TextureIDs.stoneTex,
        };

        public static GlassBlockShape GlassBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.glassTex, BottomFaceTex = TextureIDs.glassTex,
            FrontFaceTex = TextureIDs.glassTex, BackFaceTex = TextureIDs.glassTex,
            RightFaceTex = TextureIDs.glassTex, LeftFaceTex = TextureIDs.glassTex,
        };

        //------------------------------------------------
        // Oak wood set
        public static FullBlockShape OakPlanksBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.oakPlanksTex, BottomFaceTex = TextureIDs.oakPlanksTex,
            FrontFaceTex = TextureIDs.oakPlanksTex, BackFaceTex = TextureIDs.oakPlanksTex,
            RightFaceTex = TextureIDs.oakPlanksTex, LeftFaceTex = TextureIDs.oakPlanksTex,
        };

        public static BlockLogShape OakLogBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.oakLogTopTex, BottomFaceTex = TextureIDs.oakLogTopTex,
            FrontFaceTex = TextureIDs.oakLogSideTex, BackFaceTex = TextureIDs.oakLogSideTex,
            RightFaceTex = TextureIDs.oakLogSideTex, LeftFaceTex = TextureIDs.oakLogSideTex,
        };

        public static SlabBlockShape OakSlabShape { get; } = new()
        {
            TopFaceTex = TextureIDs.oakPlanksTex, BottomFaceTex = TextureIDs.oakPlanksTex,
            FrontFaceTex = TextureIDs.oakPlanksTex, BackFaceTex = TextureIDs.oakPlanksTex,
            RightFaceTex = TextureIDs.oakPlanksTex, LeftFaceTex = TextureIDs.oakPlanksTex,
        };

        public static LeavesBlockShape OakLeavesBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.oakLeavesTex, BottomFaceTex = TextureIDs.oakLeavesTex,
            FrontFaceTex = TextureIDs.oakLeavesTex, BackFaceTex = TextureIDs.oakLeavesTex,
            RightFaceTex = TextureIDs.oakLeavesTex, LeftFaceTex = TextureIDs.oakLeavesTex,
        };

        //------------------------------------------------
        //Spruce wood set
        public static FullBlockShape SprucePlanksBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.sprucePlanksTex, BottomFaceTex = TextureIDs.sprucePlanksTex,
            FrontFaceTex = TextureIDs.sprucePlanksTex, BackFaceTex = TextureIDs.sprucePlanksTex,
            RightFaceTex = TextureIDs.sprucePlanksTex, LeftFaceTex = TextureIDs.sprucePlanksTex,
        };

        public static BlockLogShape SpruceLogBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.spruceLogTopTex, BottomFaceTex = TextureIDs.spruceLogTopTex,
            FrontFaceTex = TextureIDs.spruceLogSideTex, BackFaceTex = TextureIDs.spruceLogSideTex,
            RightFaceTex = TextureIDs.spruceLogSideTex, LeftFaceTex = TextureIDs.spruceLogSideTex,
        };

        public static SlabBlockShape SpruceSlabShape { get; } = new()
        {
            TopFaceTex = TextureIDs.sprucePlanksTex, BottomFaceTex = TextureIDs.sprucePlanksTex,
            FrontFaceTex = TextureIDs.sprucePlanksTex, BackFaceTex = TextureIDs.sprucePlanksTex,
            RightFaceTex = TextureIDs.sprucePlanksTex, LeftFaceTex = TextureIDs.sprucePlanksTex,
        };

        public static LeavesBlockShape SpruceLeavesBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.spruceLeavesTex, BottomFaceTex = TextureIDs.spruceLeavesTex,
            FrontFaceTex = TextureIDs.spruceLeavesTex, BackFaceTex = TextureIDs.spruceLeavesTex,
            RightFaceTex = TextureIDs.spruceLeavesTex, LeftFaceTex = TextureIDs.spruceLeavesTex,
        };

        //------------------------------------------------
        //Birch wood set
        public static FullBlockShape BirchPlanksBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.birchPlanksTex, BottomFaceTex = TextureIDs.birchPlanksTex,
            FrontFaceTex = TextureIDs.birchPlanksTex, BackFaceTex = TextureIDs.birchPlanksTex,
            RightFaceTex = TextureIDs.birchPlanksTex, LeftFaceTex = TextureIDs.birchPlanksTex,
        };

        public static BlockLogShape BirchLogBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.birchLogTopTex, BottomFaceTex = TextureIDs.birchLogTopTex,
            FrontFaceTex = TextureIDs.birchLogSideTex, BackFaceTex = TextureIDs.birchLogSideTex,
            RightFaceTex = TextureIDs.birchLogSideTex, LeftFaceTex = TextureIDs.birchLogSideTex,
        };

        public static SlabBlockShape BirchSlabShape { get; } = new()
        {
            TopFaceTex = TextureIDs.birchPlanksTex, BottomFaceTex = TextureIDs.birchPlanksTex,
            FrontFaceTex = TextureIDs.birchPlanksTex, BackFaceTex = TextureIDs.birchPlanksTex,
            RightFaceTex = TextureIDs.birchPlanksTex, LeftFaceTex = TextureIDs.birchPlanksTex,
        };

        public static LeavesBlockShape BirchLeavesBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.birchLeavesTex, BottomFaceTex = TextureIDs.birchLeavesTex,
            FrontFaceTex = TextureIDs.birchLeavesTex, BackFaceTex = TextureIDs.birchLeavesTex,
            RightFaceTex = TextureIDs.birchLeavesTex, LeftFaceTex = TextureIDs.birchLeavesTex,
        };

        //------------------------------------------------
        //Jungle wood set
        public static FullBlockShape JunglePlanksBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.junglePlanksTex, BottomFaceTex = TextureIDs.junglePlanksTex,
            FrontFaceTex = TextureIDs.junglePlanksTex, BackFaceTex = TextureIDs.junglePlanksTex,
            RightFaceTex = TextureIDs.junglePlanksTex, LeftFaceTex = TextureIDs.junglePlanksTex,
        };

        public static BlockLogShape JungleLogBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.jungleLogTopTex, BottomFaceTex = TextureIDs.jungleLogTopTex,
            FrontFaceTex = TextureIDs.jungleLogSideTex, BackFaceTex = TextureIDs.jungleLogSideTex,
            RightFaceTex = TextureIDs.jungleLogSideTex, LeftFaceTex = TextureIDs.jungleLogSideTex,
        };

        public static SlabBlockShape JungleSlabShape { get; } = new()
        {
            TopFaceTex = TextureIDs.junglePlanksTex, BottomFaceTex = TextureIDs.junglePlanksTex,
            FrontFaceTex = TextureIDs.junglePlanksTex, BackFaceTex = TextureIDs.junglePlanksTex,
            RightFaceTex = TextureIDs.junglePlanksTex, LeftFaceTex = TextureIDs.junglePlanksTex,
        };

        public static LeavesBlockShape JungleLeavesBlockShape { get; } = new()
        {
            TopFaceTex = TextureIDs.jungleLeavesTex, BottomFaceTex = TextureIDs.jungleLeavesTex,
            FrontFaceTex = TextureIDs.jungleLeavesTex, BackFaceTex = TextureIDs.jungleLeavesTex,
            RightFaceTex = TextureIDs.jungleLeavesTex, LeftFaceTex = TextureIDs.jungleLeavesTex,
        };
    }
}
