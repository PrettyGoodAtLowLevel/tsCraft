using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    public class FullLightBlock : FullBlock
    {
        public FullLightBlock(string name, BlockShape shape, ushort id, Vector3i light) :
        base(name, shape, id)
        { 
            lightValue = light;
        }
        public Vector3i lightValue;

        public override Vector3i GetLightSourceLevel(BlockState state)
        {
            return lightValue;
        }

        public override bool IsLightPassable(BlockState state)
        {
            return false;
        }

        public override bool IsLightSource(BlockState state)
        {
            return true;
        }
    }
}
