using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.World;
using System.Xml.Linq;

namespace OurCraft.Blocks.Block_Implementations
{
    //full regular full cube solid block implementation
    public class FullBlock : Block
    {
        //csctr
        public FullBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            world.SetBlock(globalPos + hitNormal, new BlockState((byte)id));
        }

        public override bool IsLightPassable(BlockState state)
        {
            return false;
        }

        public override bool IsLightSource(BlockState state)
        {
            return false;
        }
    }
}
