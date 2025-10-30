using OpenTK.Mathematics;
using OurCraft.Rendering;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //like full block but translucent
    public class WaterBlock : FullBlock
    {
        public WaterBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }
    }
}
