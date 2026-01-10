using OurCraft.Graphics;
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

        //initializes the bits for a slab state implementation
        static SlabBlock()
        {
            var layout = new PropertyLayoutBuilder();
            SLAB_TYPE = layout.AddEnum<SlabType>();
        }

        //default constructor, assigns the slab type cominations to THIS instance
        public SlabBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { 
            Properties.Add(SLAB_TYPE);
        }

        //determines how we place a slab in the world based on the block we hit
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, ChunkManager world)
        {
            SlabType thisBlockState = thisBlock.GetProperty(SLAB_TYPE);
            SlabType stateToPlace = SlabType.Bottom;

            if (hitNormal.Y == 1 && thisBlock.BlockID == id && thisBlockState == SlabType.Bottom) stateToPlace = SlabType.Double;
            else if (hitNormal.Y == -1 && thisBlock.BlockID == id && thisBlockState == SlabType.Top) stateToPlace = SlabType.Double;
            else if (hitNormal.Y == 1) stateToPlace = SlabType.Bottom;
            else if (hitNormal.Y == -1) stateToPlace = SlabType.Top;

            BlockState state = DefaultState.With(SLAB_TYPE, stateToPlace);
            if (stateToPlace == SlabType.Double) world.SetBlock(globalPos, state);
            else world.SetBlock(globalPos + hitNormal, state);
        }

        //if double slab, then is opaque, if single slab then light can pass through
        public override bool IsLightPassable(BlockState state)
        {
            SlabType thisState = state.GetProperty(SLAB_TYPE);

            if (thisState == SlabType.Double) return false;
            return true;
        }

        //if double slab, then is opaque, if single slab then light can pass through
        public override int GetSkyLightAttenuation(BlockState state)
        {
            SlabType thisState = state.GetProperty(SLAB_TYPE);

            if (thisState == SlabType.Double) return 15;
            return 0;
        }

        //finds the slab state
        public override void DebugState(BlockState thisBlock)
        {
            base.DebugState(thisBlock);
            SlabType slabType = thisBlock.GetProperty(SLAB_TYPE);            
            Console.WriteLine(", Slab Type: " + slabType.ToString());
        }

        //slab isnt a light source
        public override bool IsLightSource(BlockState state)
        {
            return false;
        }
    }
}