using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;

namespace OurCraft.Blocks.Block_Implementations
{
    //like full block but translucent
    public class WaterBlock : FullBlock
    {
        public WaterBlock(string name, BlockShape shape): base(name, shape)
        {
            IsSolid = false;
        }

        //light can pass through semi transparent water
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        //skylight mostly passes through water, but deep oceans are dark
        public override int GetSkyLightAttenuation(BlockState state)
        {
            return LightConstants.LOW_ATTENUATION;
        }

        public override bool IsPhysicsSolid(BlockState state)
        {
            return false;
        }

        public override bool IsFluid(BlockState state)
        {
            return true;
        }
    }
}
