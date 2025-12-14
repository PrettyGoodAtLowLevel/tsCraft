using OpenTK.Mathematics;
using OurCraft.Graphics;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //like full block but translucent
    public class WaterBlock : FullBlock
    {
        public WaterBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        {
            IsSolid = false;
        }

        //light can pass through semi transparent water
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
