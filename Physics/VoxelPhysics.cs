using OpenTK.Mathematics;

namespace OurCraft.Physics
{
    public class VoxelPhysics
    {
        public struct VoxelRaycastHit
        {
            public Vector3i blockPos; //grid position of the hit block
            public Vector3i faceNormal; //the face that was hit (e.g., (1,0,0) for +X)
            public float distance; //distance from ray origin to hit
        }

        //uses dda algorithm to check if we are hitting a block or not
        //Func<int, int, int, bool> = A callback to check if a block is solid 
        public static bool RaycastVoxel(Vector3 origin, Vector3 direction, float maxDistance, Func<int, int, int, bool> isSolidBlock, out VoxelRaycastHit hit)
        {
            hit = default;

            int x = (int)MathF.Floor(origin.X);
            int y = (int)MathF.Floor(origin.Y);
            int z = (int)MathF.Floor(origin.Z);

            direction = Vector3.Normalize(direction);

            int stepX = Math.Sign(direction.X);
            int stepY = Math.Sign(direction.Y);
            int stepZ = Math.Sign(direction.Z);

            float dx = (direction.X == 0) ? float.MaxValue : Math.Abs(1f / direction.X);
            float dy = (direction.Y == 0) ? float.MaxValue : Math.Abs(1f / direction.Y);
            float dz = (direction.Z == 0) ? float.MaxValue : Math.Abs(1f / direction.Z);

            float nextVoxelBoundaryX = (stepX > 0) ? (MathF.Floor(origin.X) + 1) : MathF.Floor(origin.X);
            float nextVoxelBoundaryY = (stepY > 0) ? (MathF.Floor(origin.Y) + 1) : MathF.Floor(origin.Y);
            float nextVoxelBoundaryZ = (stepZ > 0) ? (MathF.Floor(origin.Z) + 1) : MathF.Floor(origin.Z);

            float tMaxX = (direction.X == 0) ? float.MaxValue : (nextVoxelBoundaryX - origin.X) / direction.X;
            float tMaxY = (direction.Y == 0) ? float.MaxValue : (nextVoxelBoundaryY - origin.Y) / direction.Y;
            float tMaxZ = (direction.Z == 0) ? float.MaxValue : (nextVoxelBoundaryZ - origin.Z) / direction.Z;

            float distanceTraveled = 0f;

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
