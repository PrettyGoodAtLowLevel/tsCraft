using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    public class FullLightBlock : FullBlock
    {
        //assigns the light value to the block
        public FullLightBlock(string name, BlockShape shape, ushort id, Vector3i light) :
        base(name, shape, id)
        { 
            if (light.X > 15) light.X = 15;
            if (light.Y > 15)light.Y = 15;
            if (light.Z > 15) light.Z = 15;
            if (light.X < 0) light.X = 0;
            if (light.Y < 0) light.Y = 0;
            if (light.Z < 0) light.Z = 0;
            lightValue = light;
        }
        public Vector3i lightValue;

        //is light
        public override Vector3i GetLightSourceLevel(BlockState state)
        {
            return lightValue;
        }

        //since the model is opaque, light cant actually pass through the block
        public override bool IsLightPassable(BlockState state)
        {
            return false;
        }

        //is light
        public override bool IsLightSource(BlockState state)
        {
            return true;
        }
    }
}
