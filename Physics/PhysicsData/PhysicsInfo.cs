using OpenTK.Mathematics;

//contains helpful physics data
namespace OurCraft.Physics.PhysicsData
{
    //which axis of collision are we resolving
    public enum CollisionAxis { X, Y, Z }

    //information about a block hit of a raycast
    public struct VoxelRaycastHit
    {
        public Vector3i blockPos;
        public Vector3i faceNormal;
        public double distance;

        public readonly override string ToString()
        {
            string str = "";

            str += $"BlockPos: {blockPos}, FaceNormal {faceNormal}, Distance {distance}";
            return str;
        }
    }

    //represents a basic bounding box in the world
    public struct AABB
    {
        public Vector3d min = Vector3d.Zero;
        public Vector3d max = Vector3d.Zero;

        public AABB() { }

        public AABB(Vector3d min, Vector3d max)
        {
            this.min = min;
            this.max = max;
        }  

        public readonly AABB Expand(Vector3d move)
        {
            Vector3d newMin = new Vector3d(
            move.X > 0 ? min.X : min.X + move.X,
            move.Y > 0 ? min.Y : min.Y + move.Y,
            move.Z > 0 ? min.Z : min.Z + move.Z);

            Vector3d newMax = new Vector3d(
            move.X < 0 ? max.X : max.X + move.X,
            move.Y < 0 ? max.Y : max.Y + move.Y,
            move.Z < 0 ? max.Z : max.Z + move.Z);

            return new AABB(newMin, newMax);
        }

        public readonly AABB Offset(Vector3d offset)
        {
            return new AABB(min + offset, max + offset);
        }

        public readonly override string ToString()
        {
            string str = "";

            str += $"Min: {min}, Max: {max}";
            return str;
        }
    }

    //represents basic block physics
    public struct BlockPhysics
    {
        public bool isFluid = false;
        public float friction = 10.0f;
        public float wallFriction = 0.0f;
        public float bounce = 0.0f;

        public BlockPhysics() { }

        public readonly override string ToString()
        {
            string str = "";

            str += $"IsFluid: {isFluid}, Friction: {friction}, Bounce: {bounce}";
            return str;
        }
    }

    //rigid body physics material
    public struct PhysicsMaterial
    {
        public bool useFriction = true;
        public bool useWallFriction = true;
        public bool useBounce = true;

        public float frictionCombine = 0.0f;
        public float wallFrictionCombine = 0.0f;
        public float bounceCombine = 0.0f;
 
        public PhysicsMaterial() { }

        public readonly override string ToString()
        {
            string str = "";

            str += $"Use Friction: {useFriction}, Use Bounce: {useBounce} \n";
            str += $"Friction: {frictionCombine}, Bounce: {bounceCombine}";
            return str;
        }
    }
}