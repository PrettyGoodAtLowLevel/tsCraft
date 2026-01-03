using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //full block with alpha test blending and different face culling
    public class LeavesBlock : FullBlock
    {
        public LeavesBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }

        //light can pass through leaves, even if the model looks opaque
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        //the leaves themselves are not light sources
        public override bool IsLightSource(BlockState state)
        {
            return false;
        }

        //skylight can mostly pass through leaves
        public override int GetSkyLightAttenuation(BlockState state)
        {
            return 1;
        }
    }
}
