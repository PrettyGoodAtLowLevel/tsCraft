using OpenTK.Mathematics;

namespace OurCraft.Physics
{
    //provides helpers for collision detection, raycasting, etc
    public static class AABBMath
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
        public static AABB GetAABB(Vector3d pos, Vector3d boundsMin, Vector3d boundsMax)
        {
            return new AABB
            {
                min = pos + boundsMin,
                max = pos + boundsMax
            };
        }

        //avoids creating new aabbs, checks if local aabbs + world pos intersect
        public static bool IntersectsLocal(AABB a, AABB b, Vector3d posA, Vector3d posB)
        {
             return (a.min.X + posA.X < b.max.X + posB.X && a.max.X + posA.X > b.min.X + posB.X)
             && (a.min.Y + posA.Y < b.max.Y + posB.Y && a.max.Y + posA.Y > b.min.Y + posB.Y)
             && (a.min.Z + posA.Z < b.max.Z + posB.Z &&  a.max.Z + posA.Z > b.min.Z + posB.Z);
        }

        //avoids creating new aabbs, checks if local aabb + world pos intersect point
        public static bool PointIntersectsLocal(Vector3d point, AABB localBox, Vector3d boxWorldPos)
        {
            return (point.X >= localBox.min.X + boxWorldPos.X && point.X <= localBox.max.X + boxWorldPos.X)
            && (point.Y >= localBox.min.Y + boxWorldPos.Y && point.Y <= localBox.max.Y + boxWorldPos.Y)
            && (point.Z >= localBox.min.Z + boxWorldPos.Z && point.Z <= localBox.max.Z + boxWorldPos.Z);
        }

        //finds how much an aabb can move BEFORE hitting another aabb
        public static double ClipAxisLocal(AABB rbBox, AABB localBlock, Vector3d blockPos, double move, CollisionAxis axis, double currentAllowed)
        {
            if (move > 0) currentAllowed = ClipAxisPosLocal(rbBox, localBlock, blockPos, axis, currentAllowed);
            else if (move < 0) currentAllowed = ClipAxisNegLocal(rbBox, localBlock, blockPos, axis, currentAllowed);

            return currentAllowed;
        }

        //finds distance between 2 aabbs in positive move direction
        static double ClipAxisPosLocal(AABB rbBox, AABB localBlock, Vector3d blockPos, CollisionAxis axis, double currentAllowed)
        {
            const double eps = 1e-5;

            double minX = localBlock.min.X + blockPos.X;
            double maxX = localBlock.max.X + blockPos.X;

            double minY = localBlock.min.Y + blockPos.Y;
            double maxY = localBlock.max.Y + blockPos.Y;

            double minZ = localBlock.min.Z + blockPos.Z;
            double maxZ = localBlock.max.Z + blockPos.Z;

            if (axis == CollisionAxis.X && rbBox.max.X <= minX + eps)
            {
                if (rbBox.max.Y > minY + eps && rbBox.min.Y < maxY - eps &&
                    rbBox.max.Z > minZ + eps && rbBox.min.Z < maxZ - eps)
                {
                    double dist = minX - rbBox.max.X;
                    if (dist < currentAllowed) currentAllowed = dist;
                }
            }

            if (axis == CollisionAxis.Y && rbBox.max.Y <= minY + eps)
            {
                if (rbBox.max.X > minX + eps && rbBox.min.X < maxX - eps &&
                    rbBox.max.Z > minZ + eps && rbBox.min.Z < maxZ - eps)
                {
                    double dist = minY - rbBox.max.Y;
                    if (dist < currentAllowed) currentAllowed = dist;
                }
            }

            if (axis == CollisionAxis.Z && rbBox.max.Z <= minZ + eps)
            {
                if (rbBox.max.X > minX + eps && rbBox.min.X < maxX - eps &&
                    rbBox.max.Y > minY + eps && rbBox.min.Y < maxY - eps)
                {
                    double dist = minZ - rbBox.max.Z;
                    if (dist < currentAllowed) currentAllowed = dist;
                }
            }

            return currentAllowed;
        }

        //finds distance between 2 aabbs in negative move direction
        static double ClipAxisNegLocal(AABB rbBox, AABB localBlock, Vector3d blockPos, CollisionAxis axis, double currentAllowed)
        {
            const double eps = 1e-5;

            double minX = localBlock.min.X + blockPos.X;
            double maxX = localBlock.max.X + blockPos.X;

            double minY = localBlock.min.Y + blockPos.Y;
            double maxY = localBlock.max.Y + blockPos.Y;

            double minZ = localBlock.min.Z + blockPos.Z;
            double maxZ = localBlock.max.Z + blockPos.Z;

            if (axis == CollisionAxis.X && rbBox.min.X >= maxX - eps)
            {
                if (rbBox.max.Z > minZ + eps && rbBox.min.Z < maxZ - eps &&
                    rbBox.max.Y > minY + eps && rbBox.min.Y < maxY - eps)
                {
                    double dist = maxX - rbBox.min.X;
                    if (dist > currentAllowed) currentAllowed = dist;
                }
            }

            if (axis == CollisionAxis.Y && rbBox.min.Y >= maxY - eps)
            {
                if (rbBox.max.X > minX + eps && rbBox.min.X < maxX - eps &&
                    rbBox.max.Z > minZ + eps && rbBox.min.Z < maxZ - eps)
                {
                    double dist = maxY - rbBox.min.Y;
                    if (dist > currentAllowed) currentAllowed = dist;
                }
            }

            if (axis == CollisionAxis.Z && rbBox.min.Z >= maxZ - eps)
            {
                if (rbBox.max.X > minX + eps && rbBox.min.X < maxX - eps &&
                    rbBox.max.Y > minY + eps && rbBox.min.Y < maxY - eps)
                {
                    double dist = maxZ - rbBox.min.Z;
                    if (dist > currentAllowed) currentAllowed = dist;
                }
            }

            return currentAllowed;
        }
    }
}