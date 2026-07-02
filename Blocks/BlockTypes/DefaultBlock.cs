using OpenTK.Mathematics;
using OurCraft.Blocks.Meshing;
using OurCraft.Physics;
using OurCraft.World;

namespace OurCraft.Blocks.Block_Implementations
{
    //one state, full cuboid block 1m^3 size
    public class DefaultBlock : Block
    {
        public bool detectsCollision = false;
        public bool physicsSolid = false;
        public bool isFluid = false;

        public Vector3i blockLightLevel = Vector3i.Zero;
        public int skyLightAttenuation = 0;

        public bool isLightSource = false;
        public bool blocksLight = false;

        public DefaultBlock(string name, BlockShape shape) : base(name, shape) { }

        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, ChunkManager world)
        {
            world.SetBlockState(globalPos + hitNormal, DefaultState);
        }

        public override bool IsLightPassable(BlockState state)
        {
            return !blocksLight;
        }

        public override bool IsLightSource(BlockState state)
        {
            return isLightSource;
        }

        public override Vector3i GetLightSourceLevel(BlockState state)
        {
            return blockLightLevel;
        }

        public override int GetSkyLightAttenuation(BlockState state)
        {
            return skyLightAttenuation;
        }

        public override CollisionShape GetCollisionShape(BlockState state)
        {
            return CollisionShapeData.FullBlock;
        }

        public override BlockPhysics GetBlockPhysics(BlockState state)
        {
            return new BlockPhysics()
            {
                friction = this.friction,
                wallFriction = this.wallFriction,
                bounce = this.bounce,
                isFluid = this.isFluid,
            };
        }

        public override bool IsFluid(BlockState state)
        {
            return isFluid;
        }

        public override bool DetectsCollision(BlockState state)
        {
            return detectsCollision;
        }

        public override bool IsPhysicsSolid(BlockState state)
        {
            return physicsSolid;
        }

        public override bool AOSolid(BlockState state)
        {
            return true;
        }
    }
}
