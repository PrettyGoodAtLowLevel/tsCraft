using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Entities;
using OurCraft.Entities.Components;
using OurCraft.Utility;
using OurCraft.World;

namespace OurCraft.Physics
{
    //system class for physics
    public class PhysicsSystem : BaseSystem<PhysicsObj>
    {
        static readonly double FixedTimestep = PhysicsConstants.PHYSICS_TICK;
        static readonly double Gravity = PhysicsConstants.GRAVITY;

        //go through each body and run a physics frame
        public static void StepPhysics(ChunkManager world)
        {
            foreach (var body in Components)
            {
                //get previous pos for interpolation
                body.previousPosition = body.position;

                //air drag, and friction               
                ResolveAirDrag(body);
                ResolveGroundDrag(body, world);

                //add acceleration and velocity, then reset accel
                body.velocity += body.acceleration * FixedTimestep;
                body.acceleration = Vector3d.Zero;

                //clamp vel and do collision detections
                ClampVelocity(body);
                ResolveCollisions(world, body);

                //apply gravity
                body.acceleration.Y -= Gravity * body.gravityModifer;
            }
        }

        //adds drag you skibidi
        public static void ResolveAirDrag(PhysicsObj body)
        {
            body.acceleration.X -= body.velocity.X * body.dragX;
            body.acceleration.Y -= body.velocity.Y * body.dragY;
            body.acceleration.Z -= body.velocity.Z * body.dragZ;
        }

        //adds extra drag when grounded and not in water you skibidi
        private static void ResolveGroundDrag(PhysicsObj body, ChunkManager world)
        {
            if (!body.grounded || body.inFluid || !body.useFriction) return;

            double friction = GetGroundFriction(world, body);
            body.acceleration.X -= body.velocity.X * friction;
            body.acceleration.Z -= body.velocity.Z * friction;
        }

        //solves for position on each axis for a rigid body after applying forces
        public static void ResolveCollisions(ChunkManager world, PhysicsObj body)
        {
            if (body.velocity == Vector3d.Zero) return;

            double dt = FixedTimestep;
            Vector3d pos = body.position;
            Vector3d vel = body.velocity;
            Vector3d move = vel * dt;
            body.inFluid = false;
            body.underWater = false;

            //inlined - no overhead
            void ResolveX() { move.X = ResolvePosition(world, body.bounds, pos, new Vector3d(move.X, 0, 0), body); pos.X += move.X; }
            void ResolveY() { move.Y = ResolvePosition(world, body.bounds, pos, new Vector3d(0, move.Y, 0), body); pos.Y += move.Y; }
            void ResolveZ() { move.Z = ResolvePosition(world, body.bounds, pos, new Vector3d(0, 0, move.Z), body); pos.Z += move.Z; }

            double ax = Math.Abs(move.X), ay = Math.Abs(move.Y), az = Math.Abs(move.Z);

            //resolve smallest axis first to eliminate corner bias
            if (ax <= ay && ax <= az) { ResolveX(); if (ay <= az) { ResolveY(); ResolveZ(); } else { ResolveZ(); ResolveY(); } }
            else if (ay <= ax && ay <= az) { ResolveY(); if (ax <= az) { ResolveX(); ResolveZ(); } else { ResolveZ(); ResolveX(); } }
            else { ResolveZ(); if (ax <= ay) { ResolveX(); ResolveY(); } else { ResolveY(); ResolveX(); } }

            //set body as grounded if y velocity was negative and colliding
            body.grounded = (vel.Y < 0 && move.Y != vel.Y * dt);

            if (move.X != vel.X * dt) vel.X = 0;
            if (move.Y != vel.Y * dt)
            {
                double bounciness = GetGroundBounciness(world, body, pos);
                if (bounciness > 0.1 && vel.Y < -0.5) vel.Y = -vel.Y * bounciness * (1.0 + body.dragY * FixedTimestep);
                else vel.Y = 0;
            }
            if (move.Z != vel.Z * dt) vel.Z = 0;

            body.position = pos;
            body.velocity = vel;
        }

        //computes how much in one axis we can move based on movement line and future collisions
        static double ResolvePosition(ChunkManager world, Vector3d bounds, Vector3d pos, Vector3d move, PhysicsObj body)
        {
            AABB box = VoxelPhysics.GetAABB(pos, bounds);
            double allowed = move.X + move.Y + move.Z;

            //sweep aabb collision bounds based on speed
            int minX = (int)Math.Floor(box.min.X + Math.Min(move.X, 0));
            int maxX = (int)Math.Floor(box.max.X + Math.Max(move.X, 0));

            int minY = (int)Math.Floor(box.min.Y + Math.Min(move.Y, 0));
            int maxY = (int)Math.Floor(box.max.Y + Math.Max(move.Y, 0));

            int minZ = (int)Math.Floor(box.min.Z + Math.Min(move.Z, 0));
            int maxZ = (int)Math.Floor(box.max.Z + Math.Max(move.Z, 0));

            //go through each box and find contact distance
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        BlockState block = world.GetBlockState(new Vector3(x, y, z));
                        if (!block.IsPhysicsSolid)
                        {
                            if (!block.DetectsCollision) continue;
                          
                            AABB bBox = block.GetAABB(new Vector3d(x, y, z));
                            BlockPhysics blockPhysics = block.GetBlockPhysics();

                            if (blockPhysics.isFluid && AABB.Intersects(bBox, box))
                            {
                                if (AABB.PointIntersects(body.HeadPosition, bBox)) body.underWater = true;
                                body.inFluid = true;
                                continue;
                            }
                            continue;
                        }
                        AABB blockBox = block.GetAABB(new Vector3d(x, y, z));

                        //find axis and calculate max move
                        if (move.X != 0) allowed = VoxelPhysics.ClipAxis(box, blockBox, move.X, CollisionAxis.X, allowed);
                        if (move.Y != 0) allowed = VoxelPhysics.ClipAxis(box, blockBox, move.Y, CollisionAxis.Y, allowed);
                        if (move.Z != 0) allowed = VoxelPhysics.ClipAxis(box, blockBox, move.Z, CollisionAxis.Z, allowed);
                    }
                }
            }

            return allowed;
        }

        //finds all the bottom blocks relative to an AABB and averages their friction
        public static double GetGroundFriction(ChunkManager world, PhysicsObj body)
        {
            AABB box = VoxelPhysics.GetAABB(body.position, body.bounds);

            int minX = (int)Math.Floor(box.min.X);
            int maxX = (int)Math.Floor(box.max.X - 0.001);
            int minZ = (int)Math.Floor(box.min.Z);
            int maxZ = (int)Math.Floor(box.max.Z - 0.001);

            //check the two block rows below the body's feet to catch slabs and other partial blocks
            int minY = (int)Math.Floor(box.min.Y - 1.01);
            int maxY = (int)Math.Floor(box.min.Y);

            float totalWeight = 0f;
            float weightedFriction = 0f;

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        BlockState block = world.GetBlockState(new Vector3(x, y, z));
                        if (!block.IsPhysicsSolid) continue;

                        AABB blockBox = block.GetAABB(new Vector3d(x, y, z));

                        //block must actually be touching the bottom of our AABB
                        if (blockBox.max.Y < box.min.Y - 0.01) continue;

                        double overlapX = Math.Min(box.max.X, blockBox.max.X) - Math.Max(box.min.X, blockBox.min.X);
                        double overlapZ = Math.Min(box.max.Z, blockBox.max.Z) - Math.Max(box.min.Z, blockBox.min.Z);

                        if (overlapX <= 0 || overlapZ <= 0) continue;

                        float weight = (float)(overlapX * overlapZ);
                        weightedFriction += block.GetBlockPhysics().friction * weight;
                        totalWeight += weight;
                    }
                }
            }

            return totalWeight == 0f ? 0 : weightedFriction / totalWeight;
        }

        //finds all the bottom blocks relative to an AABB and averages their bounce
        public static double GetGroundBounciness(ChunkManager world, PhysicsObj body, Vector3d pos)
        {
            if (!body.bounce) return 0;
            AABB box = VoxelPhysics.GetAABB(pos, body.bounds);

            int minX = (int)Math.Floor(box.min.X);
            int maxX = (int)Math.Floor(box.max.X - 0.001);
            int minZ = (int)Math.Floor(box.min.Z);
            int maxZ = (int)Math.Floor(box.max.Z - 0.001);

            int minY = (int)Math.Floor(box.min.Y - 1.01);
            int maxY = (int)Math.Floor(box.min.Y);

            float totalWeight = 0f;
            float weightedBounciness = 0f;

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        BlockState block = world.GetBlockState(new Vector3(x, y, z));
                        if (!block.IsPhysicsSolid) continue;

                        AABB blockBox = block.GetAABB(new Vector3d(x, y, z));
                        if (blockBox.max.Y < box.min.Y - 0.01) continue;

                        double overlapX = Math.Min(box.max.X, blockBox.max.X) - Math.Max(box.min.X, blockBox.min.X);
                        double overlapZ = Math.Min(box.max.Z, blockBox.max.Z) - Math.Max(box.min.Z, blockBox.min.Z);
                        if (overlapX <= 0 || overlapZ <= 0) continue;

                        float weight = (float)(overlapX * overlapZ);
                        weightedBounciness += block.GetBlockPhysics().bounce * weight;
                        totalWeight += weight;
                    }
                }
            }

            return totalWeight == 0f ? 0 : weightedBounciness / totalWeight;
        }

        //quake style clamp
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
    }
}