using OpenTK.Mathematics;

namespace OurCraft.Physics
{
    public class VoxelPhysics
    {
        public struct VoxelRaycastHit
        {
            public Vector3i blockPos; //grid position of the hit block
            public Vector3i faceNormal; //the face that was hit (e.g., (1,0,0) for +X)
            public double distance; //distance from ray origin to hit
        }

        //uses dda algorithm to check if we are hitting a block or not
        //Func<int, int, int, bool> = A callback to check if a block is solid 
        public static bool RaycastVoxel(Vector3d origin, Vector3 direction, float maxDistance, Func<int, int, int, bool> isSolidBlock, out VoxelRaycastHit hit)
        {
            hit = default;

            int x = (int)Math.Floor(origin.X);
            int y = (int)Math.Floor(origin.Y);
            int z = (int)Math.Floor(origin.Z);

            direction = Vector3.Normalize(direction);

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
    }
}
