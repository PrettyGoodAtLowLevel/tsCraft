using OpenTK.Mathematics;
using OurCraft.Entities;
using OurCraft.Entities.Components;
using OurCraft.Utility;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

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
    }

    //represents a basic bounding box in the world
    public struct AABB
    {
        public Vector3d min = Vector3d.Zero;
        public Vector3d max = Vector3d.Zero;

        public AABB() { }
    }

    //represents a basic physics info of a block
    public struct BlockPhysics
    {
        public float wallFriction = 0.0f;
        public float friction = 0.0f;
        public float bounce = 0.0f;
        public float gravityModifier = 1.0f;

        public BlockPhysics() { }
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

    //system class for physics
    public class PhysicsSystem : BaseSystem<RigidBody>
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

                //apply gravity, air drag, and friction
                if (body.useGravity) body.acceleration.Y -= Gravity;
                ResolveAirDrag(body);
                ResolveGroundDrag(body);

                //add acceleration and velocity
                body.velocity += body.acceleration * FixedTimestep;

                //clamp vel and do collision detections
                if (body.clampVel) ClampVelocity(body);
                ResolveCollisions(world, body);

                //reset acceleration, you need to keep applying force to continue to accelerate
                body.acceleration = Vector3d.Zero;
            }
        }

        //adds drag you skibidi
        public static void ResolveAirDrag(RigidBody body)
        {
            body.acceleration.X -= body.velocity.X * body.dragX;
            body.acceleration.Y -= body.velocity.Y * body.dragY;
            body.acceleration.Z -= body.velocity.Z * body.dragZ;
        }

        //adds extra drag when grounded you skibid
        private static void ResolveGroundDrag(RigidBody body)
        {
            if (!body.grounded) return;

            body.acceleration.X -= body.velocity.X * body.groundDragX;
            body.acceleration.Z -= body.velocity.Z * body.groundDragZ;
        }

        //solves for position on each axis for a rigid body after applying forces
        public static void ResolveCollisions(ChunkManager world, RigidBody body)
        {
            double dt = FixedTimestep;
            Vector3d pos = body.position;
            Vector3d vel = body.velocity;          
            Vector3d move = vel * dt;

            //compute how far on X axis we can move
            move.X = ResolvePosition(world, body.bounds, pos, new Vector3d(move.X, 0, 0));
            pos.X += move.X;

            //same for Y
            move.Y = ResolvePosition(world, body.bounds, pos, new Vector3d(0, move.Y, 0));
            pos.Y += move.Y;

            //and Z
            move.Z = ResolvePosition(world, body.bounds, pos, new Vector3d(0, 0, move.Z));
            pos.Z += move.Z;

            //grounded if colliding, and moving downwards
            body.grounded = (vel.Y < 0 && move.Y != vel.Y * dt);

            //zero velocity if blocked
            if (move.X != vel.X * dt) vel.X = 0;
            if (move.Y != vel.Y * dt) vel.Y = 0;
            if (move.Z != vel.Z * dt) vel.Z = 0;

            body.position = pos;
            body.velocity = vel;
        }

        //computes how much in one axis we can move based on movement line and future collisions
        static double ResolvePosition(ChunkManager world, Vector3d bounds, Vector3d pos, Vector3d move)
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
                        if (!block.IsPhysicsSolid) continue;

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

        //helper to not let velocity get out of hand
        public static void ClampVelocity(RigidBody body)
        {
            body.velocity.X = Math.Clamp(body.velocity.X, -PhysicsConstants.MAX_VEL_XZ, PhysicsConstants.MAX_VEL_XZ);
            body.velocity.Y = Math.Clamp(body.velocity.Y, -PhysicsConstants.MAX_VEL_Y, PhysicsConstants.MAX_VEL_Y);
            body.velocity.Z = Math.Clamp(body.velocity.Z, -PhysicsConstants.MAX_VEL_XZ, PhysicsConstants.MAX_VEL_XZ);
        }
    }
}