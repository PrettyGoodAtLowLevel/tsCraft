using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //full block with alpha test blending and different face culling
    public class LeavesBlock : FullBlock
    {
        public LeavesBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }

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
