using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;
using OurCraft.Physics;
using OurCraft.Utility;

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
        public SlabBlock(string name, BlockShape shape): base(name, shape)
        { 
            Properties.Add(SLAB_TYPE);
            PropertyLookup.Add(SLAB_TYPE, 0);
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

            if (thisState == SlabType.Double) return LightConstants.MAX_ATTENUATION;
            return LightConstants.NO_ATTENUATION;
        }

        //finds the slab state
        public override void DebugState(BlockState thisBlock)
        {
            base.DebugState(thisBlock);
            SlabType slabType = thisBlock.GetProperty(SLAB_TYPE);            
            Console.WriteLine(", Slab Type: " + slabType.ToString());
        }

        public override AABB GetAABB(Vector3d worldPos, BlockState state)
        {           
            SlabType thisState = state.GetProperty(SLAB_TYPE);

            if (thisState == SlabType.Double)
                return new AABB { min = worldPos, max = worldPos + Vector3d.One };

            if (thisState == SlabType.Bottom)
                return new AABB { min = worldPos, max = worldPos + new Vector3d(1.0, 0.5, 1.0) };
            
            return new AABB
            {
                min = worldPos + new Vector3d(0.0, 0.5, 0.0),
                max = worldPos + new Vector3d(1.0, 1.0, 1.0)
            };
        }

        public override bool DetectsCollision(BlockState state)
        {
            return true;
        }

        public override bool IsPhysicsSolid(BlockState state)
        {
            return true;
        }
    }
}