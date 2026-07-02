using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;
using OurCraft.Physics;
using OurCraft.Utility;
using OurCraft.Blocks.Meshing;

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
            if (stateToPlace == SlabType.Double) world.SetBlockState(globalPos, state);
            else world.SetBlockState(globalPos + hitNormal, state);
        }

        //get the AABB if slab was placed in world spot
        public override CollisionShape GetPredictedCollisionShape(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, ChunkManager world)
        {
            SlabType thisBlockState = thisBlock.GetProperty(SLAB_TYPE);
            SlabType stateToPlace = SlabType.Bottom;

            if (hitNormal.Y == 1 && thisBlock.BlockID == id && thisBlockState == SlabType.Bottom) stateToPlace = SlabType.Double;
            else if (hitNormal.Y == -1 && thisBlock.BlockID == id && thisBlockState == SlabType.Top) stateToPlace = SlabType.Double;
            else if (hitNormal.Y == 1) stateToPlace = SlabType.Bottom;
            else if (hitNormal.Y == -1) stateToPlace = SlabType.Top;

            BlockState state = DefaultState.With(SLAB_TYPE, stateToPlace);

            //move down double slab by hit normal since double slab will be in old slab place, not offset by hitnormal
            if (stateToPlace == SlabType.Double) return new CollisionShape(boxes: [new AABB(Vector3d.Zero - hitNormal, Vector3d.One - hitNormal)]);
            //just give back regular old collision shape then
            else return state.GetCollisionShape();
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

        //only solid if full slab
        public override bool AOSolid(BlockState state)
        {
            SlabType thisState = state.GetProperty(SLAB_TYPE);

            if (thisState == SlabType.Double) return true;
            return false;
        }

        //finds the slab state
        public override void DebugState(BlockState thisBlock)
        {
            base.DebugState(thisBlock);
            SlabType slabType = thisBlock.GetProperty(SLAB_TYPE);            
            Console.WriteLine(", Slab Type: " + slabType.ToString());
        }

        //constructs an AABB based on state + world position
        public override CollisionShape GetCollisionShape(BlockState state)
        {           
            SlabType thisState = state.GetProperty(SLAB_TYPE);

            if (thisState == SlabType.Double) return CollisionShapeData.FullBlock;
            if (thisState == SlabType.Bottom) return CollisionShapeData.BottomHalfSlab;
            
            return CollisionShapeData.TopHalfSlab;
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