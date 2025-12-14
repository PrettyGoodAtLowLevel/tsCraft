using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;

namespace OurCraft.Blocks.Block_Implementations
{
    //empty block
    public class AirBlock : Block
    {
        public AirBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        {
            IsSolid = false;
        }

        //light can pass through air, durr
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
            return 0;
        }
    }
}
