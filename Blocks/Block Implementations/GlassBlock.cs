using OpenTK.Mathematics;
using OurCraft.Graphics;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //fully transparent glass block, no blending
    public class GlassBlock : FullBlock
    {
        public GlassBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }

        //light can pass through glass
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        public override bool IsLightSource(BlockState state)
        {
            return false;
        }

        public override int GetLightAttenuation(BlockState state)
        {
            return 1;
        }
    }
}
