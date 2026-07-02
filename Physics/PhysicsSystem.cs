using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Entities;
using OurCraft.Utility;
using OurCraft.World;

namespace OurCraft.Physics
{
    //system class for physics
    public class PhysicsSystem : BaseSystem<PhysicsObj>
    {
        static readonly double FixedTimestep = PhysicsConstants.PHYSICS_TICK;
        static readonly double BounceThreshold = -0.5f;
        static readonly Vector3d Gravity = PhysicsConstants.GRAVITY;

        //go through each body and run a physics frame
        public static void StepPhysics(ChunkManager world)
        {
            foreach (var body in Components)
            {
                //get previous pos for interpolation
                body.previousPosition = body.position;

                //air drag, and friction               
                ResolveAirDrag(body);
                if (!body.noClip) //cant have friction if not colliding
                {
                    ResolveGroundDrag(body, world);
                    ResolveWallDrag(body, world);
                }               

                //add acceleration and velocity, then reset accel
                body.velocity += body.acceleration * FixedTimestep;
                body.acceleration = Vector3d.Zero;

                //clamp vel and do collision detections
                PhysicsHelpers.ClampVelocity(body);
                ResolveCollisions(world, body);

                body.acceleration -= Gravity * body.gravityModifer;
            }
        }

        //adds air friction you skibidi
        public static void ResolveAirDrag(PhysicsObj body)
        {
            body.acceleration.X -= body.velocity.X * body.dragX;
            body.acceleration.Y -= body.velocity.Y * body.dragY;
            body.acceleration.Z -= body.velocity.Z * body.dragZ;
        }

        //adds extra drag when grounded and not in water you skibidi
        private static void ResolveGroundDrag(PhysicsObj body, ChunkManager world)
        {
            if (!body.grounded || body.inFluid || !body.physicsMaterial.useFriction) return;

            double friction = PhysicsHelpers.GetGroundFriction(world, body);
            if (friction <= 0.01) return;
            body.acceleration.X -= body.velocity.X * friction;
            body.acceleration.Z -= body.velocity.Z * friction;
        }

        //adds extra drag when coasting along walls, and not in water you skibidi
        private static void ResolveWallDrag(PhysicsObj body, ChunkManager world)
        {
            if (!body.physicsMaterial.useWallFriction || body.inFluid) return;

            double friction = PhysicsHelpers.GetWallFriction(world, body);
            if (friction <= 0.01) return;
            body.acceleration.Y -= body.velocity.Y * friction;
            body.acceleration.X -= body.velocity.X * friction;
            body.acceleration.Z -= body.velocity.Z * friction;
        }

        //solves for position on each axis for a rigid body after applying forces
        public static void ResolveCollisions(ChunkManager world, PhysicsObj body)
        {
            double dt = FixedTimestep;

            Vector3d pos = body.position;
            Vector3d vel = body.velocity;
            Vector3d move = vel * dt;

            body.inFluid = false;
            body.underWater = false;
            if (body.noClip) { body.position += move;  body.velocity = vel;  return; }

            //y axis
            double origMoveY = move.Y;
            move.Y = ResolveAxis(world, body.boundsMin, body.boundsMax, pos, new Vector3d(0, move.Y, 0), body);
            pos.Y += move.Y;

            //set grounded state before horizontal so step-up can read it
            bool collidedY = move.Y != origMoveY;
            body.grounded = false;  
            if (collidedY)
            {
                if (vel.Y < 0)
                {
                    double b = PhysicsHelpers.GetGroundBounciness(world, body, pos);
                    bool bounced = b > 0.1 && vel.Y < BounceThreshold;
                    if (bounced) vel.Y = -vel.Y * b;
                    else { vel.Y = 0; body.grounded = true; }
                }
                else if (vel.Y > 0) vel.Y = 0;
            }

            //xz axis, sorted by collision depth, step aware
            double ax = Math.Abs(move.X), az = Math.Abs(move.Z);

            if (ax <= az)
            {
                ResolveAxisWithStep(world, body, ref pos, ref vel, ref move, isX: true);
                ResolveAxisWithStep(world, body, ref pos, ref vel, ref move, isX: false);
            }
            else
            {
                ResolveAxisWithStep(world, body, ref pos, ref vel, ref move, isX: false);
                ResolveAxisWithStep(world, body, ref pos, ref vel, ref move, isX: true);
            }

            body.position = pos;
            body.velocity = vel;
        }

        //computes how much in one axis we can move based on movement line and future collisions
        static double ResolveAxis(ChunkManager world, Vector3d boundsMin, Vector3d boundsMax, Vector3d pos, Vector3d move, PhysicsObj body)
        {
            AABB rbBox = AABBMath.GetAABB(pos, boundsMin, boundsMax);
            AABB expanded = rbBox.Expand(move);
            double allowed = move.X + move.Y + move.Z;

            //sweep aabb collision bounds based on speed
            int minX = (int)Math.Floor(expanded.min.X);
            int maxX = (int)Math.Floor(expanded.max.X);

            int minY = (int)Math.Floor(expanded.min.Y);
            int maxY = (int)Math.Floor(expanded.max.Y);

            int minZ = (int)Math.Floor(expanded.min.Z);
            int maxZ = (int)Math.Floor(expanded.max.Z);

            //go through each box and find contact distance
            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
            {
                 BlockState block = world.GetBlockState(new Vector3(x, y, z));
                 CollisionShape colShape = block.GetCollisionShape();

                 foreach (var aabb in colShape.aabbs)
                 {
                    if (!block.DetectsCollision) continue;

                    if (!block.IsPhysicsSolid)
                    {
                        BlockPhysics blockPhysics = block.GetBlockPhysics();

                        if (blockPhysics.isFluid && AABBMath.IntersectsLocal(aabb, rbBox, new Vector3d(x, y, z), Vector3d.Zero))
                        if (blockPhysics.isFluid && AABBMath.IntersectsLocal(aabb, rbBox, new Vector3d(x, y, z), Vector3d.Zero))
                        {
                            if (AABBMath.PointIntersectsLocal(body.HeadPosition, aabb, new Vector3d(x, y, z))) body.underWater = true;
                            body.inFluid = true;
                            continue;
                        }
                        continue;
                    }

                    //find axis and calculate max move
                    allowed = AABBMath.ClipAxisLocal(rbBox, aabb, new Vector3d(x, y, z), move.X, CollisionAxis.X, allowed);
                    allowed = AABBMath.ClipAxisLocal(rbBox, aabb, new Vector3d(x, y, z), move.Y, CollisionAxis.Y, allowed);
                    allowed = AABBMath.ClipAxisLocal(rbBox, aabb, new Vector3d(x, y, z), move.Z, CollisionAxis.Z, allowed);
                 }
            }
                
            return allowed;
        }

        //resolves position, but with ability to step
        static void ResolveAxisWithStep(ChunkManager world, PhysicsObj body, ref Vector3d pos, ref Vector3d vel, ref Vector3d move, bool isX)
        {
            //get axis direction from move vector
            double desired = isX ? move.X : move.Z;
            Vector3d axisVec = isX ? new Vector3d(desired, 0, 0) : new Vector3d(0, 0, desired);

            //check if colliding
            double resolved = ResolveAxis(world, body.boundsMin, body.boundsMax, pos, axisVec, body);
            bool blocked = resolved != desired;

            double stepHeight = body.grounded ? body.groundStepHeight : body.airStepHeight;
            double yVelThreshold = body.grounded ? 0.1 : -0.1;

            //check if body is on ground, not in fluid, colliding with something, and not falling up
            if (blocked && !body.inFluid && stepHeight > 0.005 && vel.Y < yVelThreshold)
            {
                //get how far up can we actually lift based on step height
                double stepUp = ResolveAxis(world, body.boundsMin, body.boundsMax, pos, new Vector3d(0, stepHeight, 0), body);
                
                if (stepUp > 1e-6)  //slight epsilon
                {
                    //retry horizontal movement from lifted position
                    Vector3d lifted = new(pos.X, pos.Y + stepUp, pos.Z);
                    double resolvedLifted = ResolveAxis(world, body.boundsMin, body.boundsMax,  lifted, axisVec, body);

                    if (resolvedLifted == desired) //obstacle fully cleared
                    {
                        //apply horizontal move at lifted position
                        if (isX) lifted.X += resolvedLifted;
                        else lifted.Z += resolvedLifted;

                        //snap back down and land on top of stepped block
                        double snapDown = ResolveAxis(world, body.boundsMin, body.boundsMax, lifted, new Vector3d(0, -stepUp, 0), body);
                        lifted.Y += snapDown;                    

                        pos = lifted;
                        if (isX) move.X = resolvedLifted;
                        else move.Z = resolvedLifted;
                        return;
                    }
                }
            }

            if (body.sneaking && body.grounded && !body.inFluid && resolved != 0)
            {
                Vector3d testPos = pos;
                if (isX) testPos.X += resolved;
                else testPos.Z += resolved;

                AABB testBox = AABBMath.GetAABB(testPos, body.boundsMin, body.boundsMax);
                if (!PhysicsHelpers.AABBHasGroundBelow(world, testBox))
                {
                    resolved = 0;
                    blocked = true;
                }
            }

            //no step, apply and zero velocity if blocked
            if (isX) { move.X = resolved; pos.X += move.X; if (blocked) vel.X = 0; }
            else { move.Z = resolved; pos.Z += move.Z; if (blocked) vel.Z = 0; }
        }
    }
}