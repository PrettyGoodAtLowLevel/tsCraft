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
        {
            IsSolid = false;
        }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            world.SetBlock(globalPos + hitNormal, new BlockState((byte)id));
        }

        public override void UpdateBlockState(Vector3 globalPos, BlockState thisBlock, Chunkmanager world)
        {
            if (world.GetBlockState(globalPos + new Vector3i(0, -1, 0)).BlockID == BlockIDs.AIR_BLOCK)
            {
                Console.WriteLine("deleting");
                world.SetBlock(globalPos, new BlockState(BlockIDs.AIR_BLOCK));
            }
        }
    }
}
