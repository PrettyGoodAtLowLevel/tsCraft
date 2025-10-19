using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //empty block
    public class AirBlock : Block
    {
        public AirBlock(string name, int bm, int t, int f, int b, int r, int l, ushort id) :
        base(name, bm, t, f, b, r, l, id)
        { }

        //get air side
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.AIR;
        }
    }
}
