using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;
using OurCraft.Physics;

namespace OurCraft.Blocks.Block_Implementations
{
    //full regular full cube solid block implementation
    public class FullBlock : Block
    {
        //csctr
        public FullBlock(string name, BlockShape shape): base(name, shape) { }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, ChunkManager world)
        {
            world.SetBlock(globalPos + hitNormal, DefaultState);
        }

        public override AABB GetAABB(Vector3d worldPos, BlockState state)
        {
            return new AABB()
            {
                min = worldPos,
                max = worldPos + Vector3d.One
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