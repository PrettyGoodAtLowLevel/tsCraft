using OurCraft.Rendering;
using OurCraft.World;
using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //x shaped blocks like flowers and tall grass
    public class CrossQuadBlock : Block
    {
        public CrossQuadBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            if (bottom.BlockID != BlockRegistry.GetBlock("Grass Block") && bottom.BlockID != BlockRegistry.GetBlock("Dirt"))
            {
                return;
            }
            world.SetBlock(globalPos + hitNormal, new BlockState((byte)id));
        }
    }
}
