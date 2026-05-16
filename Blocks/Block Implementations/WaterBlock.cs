using OurCraft.Blocks.Block_Properties;
using OurCraft.Physics;
using OurCraft.Utility;

namespace OurCraft.Blocks.Block_Implementations
{
    //similar to full block, but translucent and has fluid properties
    public class WaterBlock : FullBlock
    {
        public WaterBlock(string name, BlockShape shape): base(name, shape)
        {
            IsRenderSolid = false;
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

        public override BlockPhysics GetBlockPhysics(BlockState state)
        {
            BlockPhysics physics = new(){ isFluid = true };
            return physics;
        }
    }
}
