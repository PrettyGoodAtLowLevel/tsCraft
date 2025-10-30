using OurCraft.Rendering;
using OpenTK.Mathematics;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //half block
    //contains one property - which type of slab we are using
    public class SlabBlock : Block
    {
        public static readonly EnumProperty<SlabType> SLAB_TYPE;

        //initializes the bits for a slab state
        static SlabBlock()
        {
            var layout = new PropertyLayoutBuilder();
            SLAB_TYPE = layout.AddEnum<SlabType>();
        }

        //default constructor
        public SlabBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }

        //determines how we place a slab in the world based on the slab we hit
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            SlabType thisBlockState = thisBlock.GetProperty(SLAB_TYPE);
            if (hitNormal.Y == 1 && thisBlock.BlockID == id && thisBlockState == SlabType.Bottom)
            {
                world.SetBlock(globalPos, new BlockState(id).WithProperty(SLAB_TYPE, SlabType.Double));
                return;
            }
            else if (hitNormal.Y == -1 && thisBlock.BlockID == id && thisBlockState == SlabType.Top)
            {
                world.SetBlock(globalPos, new BlockState(id).WithProperty(SLAB_TYPE, SlabType.Double));
                return;
            }
            else if (hitNormal.Y == 1)
            {
                world.SetBlock(globalPos + hitNormal, new BlockState(id).WithProperty(SLAB_TYPE, SlabType.Bottom));
                return;
            }
            else if (hitNormal.Y == -1)
            {
                world.SetBlock(globalPos + hitNormal, new BlockState(id).WithProperty(SLAB_TYPE, SlabType.Top));
                return;
            }
            else
            {
                world.SetBlock(globalPos + hitNormal, new BlockState(id).WithProperty(SLAB_TYPE, SlabType.Bottom));
                return;
            }
        }
    }
}
