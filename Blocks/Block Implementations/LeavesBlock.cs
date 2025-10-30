using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //full block with alpha test blending and different face culling
    public class LeavesBlock : FullBlock
    {
        public LeavesBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }
    }
}
