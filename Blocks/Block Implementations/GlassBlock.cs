using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;

namespace OurCraft.Blocks.Block_Implementations
{
    //fully transparent glass block, no blending
    public class GlassBlock : FullBlock
    {
        public GlassBlock(string name, BlockShape shape): base(name, shape) { }

        //light can pass through glass
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        //sky light fully passes through glass
        public override int GetSkyLightAttenuation(BlockState state)
        {
            return LightConstants.NO_ATTENUATION;
        }

        public override bool AOSolid(BlockState state)
        {
            return false;
        }
    }
}
