using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //empty block
    public class AirBlock : Block
    {
        public AirBlock(string name, BlockShape shape): base(name, shape)
        {
            IsSolid = false;
        }

        //light can pass through air, durr
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        //air is not a light source
        public override bool IsLightSource(BlockState state)
        {
            return false;
        }

        //air lets skylight fully pass
        public override int GetSkyLightAttenuation(BlockState state)
        {
            return 0;
        }
    }
}
