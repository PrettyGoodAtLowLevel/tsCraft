using OpenTK.Mathematics;
using OurCraft.Rendering;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //fully transparent glass block, no blending
    public class GlassBlock : FullBlock
    {
        public GlassBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }
    }
}
