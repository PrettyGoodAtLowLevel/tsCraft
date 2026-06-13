using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.World;

namespace OurCraft.Physics
{
    //contains tons of helpers for voxel physics engine
    public static class PhysicsHelpers
    {
        //clamps a body's velocity based on the max vel
        public static void ClampVelocity(PhysicsObj body)
        {
            float xzSpeedSq = (float)(body.velocity.X * body.velocity.X + body.velocity.Z * body.velocity.Z);
            float maxSq = body.maxVelXZ * body.maxVelXZ;
            if (xzSpeedSq > maxSq)
            {
                float scale = VoxelMath.InvSqrt(xzSpeedSq) * body.maxVelXZ;
                body.velocity.X *= scale;
                body.velocity.Z *= scale;
            }

            body.velocity.Y = Math.Clamp(body.velocity.Y, -body.maxVelY, body.maxVelY);
        }

        //checks if an aabb collides with the world at all
        public static bool BoxCollidesWorld(ChunkManager world, AABB box)
        {
            int minX = (int)Math.Floor(box.min.X);
            int maxX = (int)Math.Floor(box.max.X);

            int minY = (int)Math.Floor(box.min.Y);
            int maxY = (int)Math.Floor(box.max.Y);

            int minZ = (int)Math.Floor(box.min.Z);
            int maxZ = (int)Math.Floor(box.max.Z);

            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
            {
                BlockState block = world.GetBlockState(new Vector3(x, y, z));

                if (!block.DetectsCollision) continue;
                if (!block.IsPhysicsSolid) continue;

                CollisionShape shape = block.GetCollisionShape();

                foreach (var aabb in shape.aabbs)
                {
                    if (AABBMath.IntersectsLocal(aabb, box, new Vector3d(x, y, z), Vector3d.Zero)) return true; //collision found
                }
            }

            return false; //no collisions
        }

        //returns true if at least one corner of the AABB has solid ground below it
        public static bool AABBHasGroundBelow(ChunkManager world, AABB box, float dropCheckDist = 0.5625f)
        {
            const double e = 0.001;
            double y = box.min.Y - dropCheckDist;

            return PointHasGroundBelow(world, new(box.min.X + e, y, box.min.Z + e), dropCheckDist)
            || PointHasGroundBelow(world, new(box.max.X - e, y, box.min.Z + e), dropCheckDist)
            || PointHasGroundBelow(world, new(box.min.X + e, y, box.max.Z - e), dropCheckDist)
            || PointHasGroundBelow(world, new(box.max.X - e, y, box.max.Z - e), dropCheckDist);
        }

        static bool PointHasGroundBelow(ChunkManager world, Vector3d point, float dist)
        {
            int x = (int)Math.Floor(point.X);
            int z = (int)Math.Floor(point.Z);
            int yTop = (int)Math.Floor(point.Y + dist);
            int yBot = (int)Math.Floor(point.Y);

            for (int y = yBot; y <= yTop; y++)
            {
                BlockState block = world.GetBlockState(new Vector3(x, y, z));
                if (block.DetectsCollision && block.IsPhysicsSolid) return true;
            }
            return false;
        }

        //finds all the bottom blocks relative to an AABB and averages their bounce
        public static double GetGroundBounciness(ChunkManager world, PhysicsObj body, Vector3d pos)
        {
            if (!body.physicsMaterial.useBounce) return 0;
            AABB rbBox = AABBMath.GetAABB(pos, body.boundsMin, body.boundsMax);

            //expand aabb slightly for friction sampling only 
            const double frictionExpand = 0.05;
            double sampleMinX = rbBox.min.X - frictionExpand;
            double sampleMaxX = rbBox.max.X + frictionExpand;
            double sampleMinZ = rbBox.min.Z - frictionExpand;
            double sampleMaxZ = rbBox.max.Z + frictionExpand;

            int minX = (int)Math.Floor(sampleMinX);
            int maxX = (int)Math.Floor(sampleMaxX - 0.001);

            int minZ = (int)Math.Floor(sampleMinZ);
            int maxZ = (int)Math.Floor(sampleMaxZ - 0.001);

            //check the two block rows below the body's feet to catch slabs and 
            int minY = (int)Math.Floor(rbBox.min.Y - 1.01);
            int maxY = (int)Math.Floor(rbBox.min.Y);

            float totalWeight = 0f;
            float weightedBounciness = 0f;

            for (int x = minX; x <= maxX; x++)           
            for (int z = minZ; z <= maxZ; z++)             
            for (int y = minY; y <= maxY; y++)
            {
                 Vector3d blockPos = new Vector3(x, y, z);
                 BlockState block = world.GetBlockState(blockPos);

                 if (!block.IsPhysicsSolid) continue;

                 CollisionShape blockShape = block.GetCollisionShape();

                 foreach (var blockBox in blockShape.aabbs)
                 {
                    if (blockBox.max.Y + blockPos.Y < rbBox.min.Y - 0.01) continue;

                    double overlapX = Math.Min(rbBox.max.X, blockBox.max.X + blockPos.X) - Math.Max(rbBox.min.X, blockBox.min.X + blockPos.X);
                    double overlapZ = Math.Min(rbBox.max.Z, blockBox.max.Z + blockPos.Z) - Math.Max(rbBox.min.Z, blockBox.min.Z + blockPos.Z);
                    if (overlapX <= 0 || overlapZ <= 0) continue;

                    float weight = (float)(overlapX * overlapZ);
                    weightedBounciness += block.GetBlockPhysics().bounce * weight;
                    totalWeight += weight;
                }
            }
                
            

            return totalWeight == 0f ? 0 : (weightedBounciness / totalWeight) * body.physicsMaterial.bounceCombine;
        }

        //finds all the bottom blocks relative to an AABB and averages their friction
        public static double GetGroundFriction(ChunkManager world, PhysicsObj body)
        {
            AABB rbBox = AABBMath.GetAABB(body.position, body.boundsMin, body.boundsMax);

            //expand aabb slightly for friction sampling only 
            const double frictionExpand = 0.05;
            double sampleMinX = rbBox.min.X - frictionExpand;
            double sampleMaxX = rbBox.max.X + frictionExpand;
            double sampleMinZ = rbBox.min.Z - frictionExpand;
            double sampleMaxZ = rbBox.max.Z + frictionExpand;

            int minX = (int)Math.Floor(sampleMinX);
            int maxX = (int)Math.Floor(sampleMaxX - 0.001);

            int minZ = (int)Math.Floor(sampleMinZ);
            int maxZ = (int)Math.Floor(sampleMaxZ - 0.001);

            //check the two block rows below the body's feet to catch slabs and 
            int minY = (int)Math.Floor(rbBox.min.Y - 1.01);
            int maxY = (int)Math.Floor(rbBox.min.Y);

            float totalWeight = 0f;
            float weightedFriction = 0f;

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        Vector3d blockPos = new Vector3(x, y, z);
                        BlockState block = world.GetBlockState(blockPos);
                        if (!block.IsPhysicsSolid) continue;

                        CollisionShape blockShape = block.GetCollisionShape();

                        foreach (var blockBox in blockShape.aabbs)
                        {
                            if (blockBox.max.Y + blockPos.Y < rbBox.min.Y - 0.01) continue;

                            double overlapX = Math.Min(rbBox.max.X, blockBox.max.X + blockPos.X) - Math.Max(rbBox.min.X, blockBox.min.X + blockPos.X);
                            double overlapZ = Math.Min(rbBox.max.Z, blockBox.max.Z + blockPos.Z) - Math.Max(rbBox.min.Z, blockBox.min.Z + blockPos.Z);
                            if (overlapX <= 0 || overlapZ <= 0) continue;

                            float weight = (float)(overlapX * overlapZ);
                            weightedFriction += block.GetBlockPhysics().friction * weight;
                            totalWeight += weight;
                        }
                    }
                }
            }

            return totalWeight == 0f ? 0 : (weightedFriction / totalWeight) * body.physicsMaterial.frictionCombine;
        }

        //finds all wall blocks relative to an AABB and averages their friction
        public static double GetWallFriction(ChunkManager world, PhysicsObj body)
        {
            AABB rbBox = AABBMath.GetAABB(body.position, body.boundsMin, body.boundsMax);

            int minY = (int)Math.Floor(rbBox.min.Y);
            int maxY = (int)Math.Floor(rbBox.max.Y - 0.001);

            int minX = (int)Math.Floor(rbBox.min.X);
            int maxX = (int)Math.Floor(rbBox.max.X - 0.001);

            int minZ = (int)Math.Floor(rbBox.min.Z);
            int maxZ = (int)Math.Floor(rbBox.max.Z - 0.001);

            float totalWeight = 0f;
            float weightedFriction = 0f;

            void SampleFace(int bx, int by, int bz, bool checkX, bool posDir)
            {
                BlockState block = world.GetBlockState(new Vector3(bx, by, bz));
                if (!block.DetectsCollision || !block.IsPhysicsSolid) return;

                foreach (var blockBox in block.GetCollisionShape().aabbs)
                {
                    if (checkX)
                    {
                        if (posDir ? (blockBox.min.X + bx > rbBox.max.X + 0.01) : (blockBox.max.X + bx < rbBox.min.X - 0.01)) continue;

                        double oy = Math.Min(rbBox.max.Y, blockBox.max.Y + by) - Math.Max(rbBox.min.Y, blockBox.min.Y + by);
                        double oz = Math.Min(rbBox.max.Z, blockBox.max.Z + bz) - Math.Max(rbBox.min.Z, blockBox.min.Z + bz);

                        if (oy <= 0 || oz <= 0) return;

                        float w = (float)(oy * oz);
                        weightedFriction += block.GetBlockPhysics().wallFriction * w;
                        totalWeight += w;
                    }
                    else
                    {
                        if (posDir ? (blockBox.min.Z + bz > rbBox.max.Z + 0.01) : (blockBox.max.Z + bz < rbBox.min.Z - 0.01)) continue;

                        double oy = Math.Min(rbBox.max.Y, blockBox.max.Y + by) - Math.Max(rbBox.min.Y, blockBox.min.Y + by);
                        double ox = Math.Min(rbBox.max.X, blockBox.max.X + bx) - Math.Max(rbBox.min.X, blockBox.min.X + bx);

                        if (oy <= 0 || ox <= 0) return;

                        float w = (float)(oy * ox);
                        weightedFriction += block.GetBlockPhysics().wallFriction * w;
                        totalWeight += w;
                    }
                }
            }

            int xNeg = (int)Math.Floor(rbBox.min.X - 0.001);
            int xPos = (int)Math.Floor(rbBox.max.X + 0.01);

            int zNeg = (int)Math.Floor(rbBox.min.Z - 0.001);
            int zPos = (int)Math.Floor(rbBox.max.Z + 0.01);

            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    SampleFace(xNeg, y, z, checkX: true, posDir: false);
                    SampleFace(xPos, y, z, checkX: true, posDir: true);
                }
            }


            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    SampleFace(x, y, zNeg, checkX: false, posDir: false);
                    SampleFace(x, y, zPos, checkX: false, posDir: true);
                }
            }

            return totalWeight == 0f ? 0.0 : (weightedFriction / totalWeight) * body.physicsMaterial.wallFrictionCombine;
        }
    }
}