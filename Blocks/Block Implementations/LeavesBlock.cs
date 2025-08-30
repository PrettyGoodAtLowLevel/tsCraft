namespace OurCraft.Blocks.Block_Implementations
{
    //full block with alpha test blending and different face culling
    public class LeavesBlock : FullBlock
    {
        public LeavesBlock(string name, int bm, int t, int f, int b, int r, int l, int id) :
        base(name, bm, t, f, b, r, l, id)
        { }

        public LeavesBlock(string name, int t, int id) :
        base(name, t, t, t, t, t, t, id)
        { }

        //same thing as full block, just different face type
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.LEAVES;
        }
    }
}
