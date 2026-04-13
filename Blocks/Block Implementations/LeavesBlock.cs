using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;

namespace OurCraft.Blocks.Block_Implementations
{
    //full block with alpha test blending and different face culling
    public class LeavesBlock : FullBlock
    {
        public LeavesBlock(string name, BlockShape shape): base(name, shape) { }

        //light can pass through leaves, even if the model looks opaque
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        //skylight can mostly pass through leaves
        public override int GetSkyLightAttenuation(BlockState state)
        {
            return LightConstants.LOW_ATTENUATION;
        }
    }
}
