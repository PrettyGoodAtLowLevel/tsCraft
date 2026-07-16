using OpenTK.Mathematics;
using OurCraft.Physics.PhysicsData;

namespace OurCraft.Physics.System
{
    //provides helpers for collision detection, raycasting, etc
    public static class AABBHelpers
    {
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
             return a.min.X + posA.X < b.max.X + posB.X && a.max.X + posA.X > b.min.X + posB.X
             && a.min.Y + posA.Y < b.max.Y + posB.Y && a.max.Y + posA.Y > b.min.Y + posB.Y
             && a.min.Z + posA.Z < b.max.Z + posB.Z &&  a.max.Z + posA.Z > b.min.Z + posB.Z;
        }

        //avoids creating new aabbs, checks if local aabb + world pos intersect point
        public static bool PointIntersectsLocal(Vector3d point, AABB localBox, Vector3d boxWorldPos)
        {
            return point.X >= localBox.min.X + boxWorldPos.X && point.X <= localBox.max.X + boxWorldPos.X
            && point.Y >= localBox.min.Y + boxWorldPos.Y && point.Y <= localBox.max.Y + boxWorldPos.Y
            && point.Z >= localBox.min.Z + boxWorldPos.Z && point.Z <= localBox.max.Z + boxWorldPos.Z;
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