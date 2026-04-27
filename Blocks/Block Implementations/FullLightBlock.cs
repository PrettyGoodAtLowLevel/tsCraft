using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;

namespace OurCraft.Blocks.Block_Implementations
{
    //full block, but with lighting
    public class FullLightBlock : FullBlock
    {
        public Vector3i lightValue;

        //assigns the light value to the block
        public FullLightBlock(string name, BlockShape shape, Vector3i light): base(name, shape)
        { 
            if (light.X > LightConstants.MAX_LIGHT) light.X = LightConstants.MAX_LIGHT;
            if (light.Y > LightConstants.MAX_LIGHT) light.Y = LightConstants.MAX_LIGHT;
            if (light.Z > LightConstants.MAX_LIGHT) light.Z = LightConstants.MAX_LIGHT;
            if (light.X < LightConstants.MIN_LIGHT) light.X = LightConstants.MIN_LIGHT;
            if (light.Y < LightConstants.MIN_LIGHT) light.Y = LightConstants.MIN_LIGHT;
            if (light.Z < LightConstants.MIN_LIGHT) light.Z = LightConstants.MIN_LIGHT;
            lightValue = light;
        }
        
        //is light
        public override Vector3i GetLightSourceLevel(BlockState state)
        {
            return lightValue;
        }

        //is light
        public override bool IsLightSource(BlockState state)
        {
            return true;
        }
    }
}
