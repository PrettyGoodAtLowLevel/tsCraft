using OpenTK.Mathematics;

namespace OurCraft.Physics
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

        public static bool Intersects(AABB a, AABB b)
        {           
            return (a.min.X < b.max.X && a.max.X > b.min.X) 
            && (a.min.Y < b.max.Y && a.max.Y > b.min.Y)
            && (a.min.Z < b.max.Z && a.max.Z > b.min.Z);      
        }

        public static bool PointIntersects(Vector3d point, AABB box)
        {
            return (point.X >= box.min.X && point.X <= box.max.X)
            && (point.Y >= box.min.Y && point.Y <= box.max.Y)
            && (point.Z >= box.min.Z && point.Z <= box.max.Z);
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
        public float bounce = 0.0f;

        public BlockPhysics() { }

        public readonly override string ToString()
        {
            string str = "";

            str += $"IsFluid: {isFluid}, Friction: {friction}, Bounce: {bounce}";
            return str;
        }
    }

    //provides helpers for collision detection, raycasting, etc
    public static class VoxelPhysics
    {
        //uses dda algorithm to check if we are hitting a block or not
        public static bool RaycastVoxel(Vector3d origin, Vector3 dir, float maxDistance, Func<int, int, int, bool> isSolidBlock, out VoxelRaycastHit hit)
        {
            hit = default;
            
            int x = (int)Math.Floor(origin.X);
            int y = (int)Math.Floor(origin.Y);
            int z = (int)Math.Floor(origin.Z);

            Vector3 direction = Vector3.Normalize(dir);
            int stepX = Math.Sign(direction.X);
            int stepY = Math.Sign(direction.Y);
            int stepZ = Math.Sign(direction.Z);

            double dx = (direction.X == 0) ? float.MaxValue : Math.Abs(1f / direction.X);
            double dy = (direction.Y == 0) ? float.MaxValue : Math.Abs(1f / direction.Y);
            double dz = (direction.Z == 0) ? float.MaxValue : Math.Abs(1f / direction.Z);

            double nextVoxelBoundaryX = (stepX > 0) ? (Math.Floor(origin.X) + 1) : Math.Floor(origin.X);
            double nextVoxelBoundaryY = (stepY > 0) ? (Math.Floor(origin.Y) + 1) : Math.Floor(origin.Y);
            double nextVoxelBoundaryZ = (stepZ > 0) ? (Math.Floor(origin.Z) + 1) : Math.Floor(origin.Z);

            double tMaxX = (direction.X == 0) ? double.MaxValue : (nextVoxelBoundaryX - origin.X) / direction.X;
            double tMaxY = (direction.Y == 0) ? double.MaxValue : (nextVoxelBoundaryY - origin.Y) / direction.Y;
            double tMaxZ = (direction.Z == 0) ? double.MaxValue : (nextVoxelBoundaryZ - origin.Z) / direction.Z;

            double distanceTraveled = 0f;
            while (distanceTraveled <= maxDistance)
            {
                if (isSolidBlock(x, y, z))
                {
                    hit.blockPos = new Vector3i(x, y, z);
                    hit.distance = distanceTraveled;
                    return true;
                }

                //decide which direction to step
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x += stepX;
                        distanceTraveled = tMaxX;
                        tMaxX += dx;
                        hit.faceNormal = new Vector3i(-stepX, 0, 0);
                    }
                    else
                    {
                        z += stepZ;
                        distanceTraveled = tMaxZ;
                        tMaxZ += dz;
                        hit.faceNormal = new Vector3i(0, 0, -stepZ);
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        y += stepY;
                        distanceTraveled = tMaxY;
                        tMaxY += dy;
                        hit.faceNormal = new Vector3i(0, -stepY, 0);
                    }
                    else
                    {
                        z += stepZ;
                        distanceTraveled = tMaxZ;
                        tMaxZ += dz;
                        hit.faceNormal = new Vector3i(0, 0, -stepZ);
                    }
                }
            }

            return false;
        }

        //helper for getting local bounds aabb to world bounds aabb
        public static AABB GetAABB(Vector3d pos, Vector3d bounds)
        {
            return new AABB
            {
                min = pos - bounds / 2,
                max = pos + bounds / 2
            };
        }

        //finds the distance between two aabbs if colliding
        public static double ClipAxis(AABB box, AABB block, double move, CollisionAxis axis, double currentAllowed)
        {
            if (move > 0) currentAllowed = ClipAxisPos(box, block, axis, currentAllowed);
            else if (move < 0) currentAllowed = ClipAxisNeg(box, block, axis, currentAllowed);

            return currentAllowed;
        }

        //finds the distance between aabbs if colliding in positive movement direction
        static double ClipAxisPos(AABB box, AABB block, CollisionAxis axis, double currentAllowed)
        {
            const double eps = 1e-6;

            if (axis == CollisionAxis.X && box.max.X <= block.min.X + eps)
            {
                if (box.max.Y > block.min.Y + eps && box.min.Y < block.max.Y - eps &&
                    box.max.Z > block.min.Z + eps && box.min.Z < block.max.Z - eps)
                {
                    double dist = block.min.X - box.max.X;
                    if (dist < currentAllowed) currentAllowed = dist;
                }
            }
            if (axis == CollisionAxis.Y && box.max.Y <= block.min.Y + eps)
            {
                if (box.max.X > block.min.X + eps && box.min.X < block.max.X - eps &&
                    box.max.Z > block.min.Z + eps && box.min.Z < block.max.Z - eps)
                {
                    double dist = block.min.Y - box.max.Y;
                    if (dist < currentAllowed) currentAllowed = dist;
                }
            }
            if (axis == CollisionAxis.Z && box.max.Z <= block.min.Z + eps)
            {
                if (box.max.Y > block.min.Y + eps && box.min.Y < block.max.Y - eps &&
                    box.max.X > block.min.X + eps && box.min.X < block.max.X - eps)
                {
                    double dist = block.min.Z - box.max.Z;
                    if (dist < currentAllowed) currentAllowed = dist;
                }
            }

            return currentAllowed;
        }

        //finds the distance between aabbs if colliding in negative movement direction
        static double ClipAxisNeg(AABB box, AABB block, CollisionAxis axis, double currentAllowed)
        {
            const double eps = 1e-6;

            if (axis == CollisionAxis.X && box.min.X >= block.max.X - eps)
            {
                if (box.max.Z > block.min.Z + eps && box.min.Z < block.max.Z - eps &&
                    box.max.Y > block.min.Y + eps && box.min.Y < block.max.Y - eps)
                {
                    double dist = block.max.X - box.min.X;
                    if (dist > currentAllowed) currentAllowed = dist;
                }
            }
            if (axis == CollisionAxis.Y && box.min.Y >= block.max.Y - eps)
            {
                if (box.max.X > block.min.X + eps && box.min.X < block.max.X - eps &&
                    box.max.Z > block.min.Z + eps && box.min.Z < block.max.Z - eps)
                {
                    double dist = block.max.Y - box.min.Y;
                    if (dist > currentAllowed) currentAllowed = dist;
                }
            }
            if (axis == CollisionAxis.Z && box.min.Z >= block.max.Z - eps)
            {
                if (box.max.X > block.min.X + eps && box.min.X < block.max.X - eps &&
                    box.max.Y > block.min.Y + eps && box.min.Y < block.max.Y - eps)
                {
                    double dist = block.max.Z - box.min.Z;
                    if (dist > currentAllowed) currentAllowed = dist;
                }
            }

            return currentAllowed;
        }
    }
}