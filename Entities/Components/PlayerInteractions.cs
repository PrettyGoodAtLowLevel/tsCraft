using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Terrain_Generation;
using OurCraft.Utility;
using OurCraft.World;
using System.Security.Cryptography;
using OurCraft.Physics;

namespace OurCraft.Entities.Components
{
    //allows for player to interact with the world, this will eventually get nuked
    public class PlayerInteractions : Component
    {
        int currentBlockID = 0;
        readonly float reach = 4.0f;
        bool slowTime = false;
        public Vector3 camOffset = Vector3.UnitY * RenderingConstants.CAM_HEIGHT_OFFSET;
        public BlockState waterBlock;

        internal override void Register()
        {
            BaseSystem<PlayerInteractions>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<PlayerInteractions>.Unregister(this);
        }

        public override void OnCreation()
        {
            currentBlockID = BlockRegistry.GetBlockID("Grass Block");
            waterBlock = BlockRegistry.GetDefaultBlockState("Water");
        }

        public override void OnUpdate(ChunkManager world, double time, KeyboardState kb, MouseState ms)
        {
            ScrollBlocks(ms);
            HandleBlockInteractions(world, ms);
            DebugInteractions(world, kb);
        }

        void HandleBlockInteractions(ChunkManager world, MouseState ms)
        {
            bool hitBlock = VoxelPhysics.RaycastVoxel(Transform.position + camOffset, Transform.Forward, reach,
            (x, y, z) => world.GetBlockState(new Vector3(x, y, z)) != Block.AIR && world.GetBlockState(new Vector3(x, y, z)) != waterBlock,
            out VoxelRaycastHit hit);

            //gameplay
            if (ms.IsButtonPressed(MouseButton.Left))
            {
                if (hitBlock)
                {
                    world.SetBlock(hit.blockPos, Block.AIR);
                }
            }
            if (ms.IsButtonPressed(MouseButton.Right))
            {
                if (hitBlock)
                {
                    PhysicsObj? rb = GameObject.GetComponent<PhysicsObj>();
                    if (rb == null) return;

                    //get blocks
                    BlockState bottom, top, front, back, left, right;

                    bottom = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, -1, 0));
                    top = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 1, 0));
                    front = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 0, 1));
                    back = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 0, -1));
                    right = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(1, 0, 0));
                    left = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(-1, 0, 0));

                    AABB predictedAABB = BlockRegistry.GetBlock(currentBlockID).GetPredictedAABB(hit.blockPos, hit.faceNormal,
                    bottom, top, front, back, right, left,
                    world.GetBlockState(hit.blockPos), world);

                    if (AABB.Intersects(predictedAABB, VoxelPhysics.GetAABB(rb.position, rb.bounds))) return;

                    //try to add block
                    BlockRegistry.GetBlock(currentBlockID).PlaceBlockState(hit.blockPos, hit.faceNormal,
                    bottom, top, front, back, right, left,
                    world.GetBlockState(hit.blockPos), world);
                }
            }
            //debug block and light value
            if (ms.IsButtonPressed(MouseButton.Middle))
            {
                if (hitBlock)
                {
                    Console.Clear();
                    Vector3i hitBlockPos = hit.blockPos + hit.faceNormal;
                    ushort light = world.GetLight(hitBlockPos);
                    Vector3i blockLight = VoxelMath.UnpackLight16Block(light);
                    byte skyLight = VoxelMath.UnpackLight16Sky(light);
                    Console.WriteLine("Sampling light at pos: (" + hitBlockPos.X + ", " + hitBlockPos.Y + ", " + hitBlockPos.Z + ")");
                    Console.WriteLine("Light R: " + blockLight.X + " Light G: " + blockLight.Y + " Light B: " + blockLight.Z);
                    Console.WriteLine("SkyLight: " + skyLight);
                    Console.WriteLine("\nDebugging BlockState at: (" + hit.blockPos.X + ", " + hit.blockPos.Y + ", " + hit.blockPos.Z + ")");
                    BlockState state = world.GetBlockState(hit.blockPos);
                    state.DebugState();
                }
            }
        }

        void ScrollBlocks(MouseState ms)
        {            
            if (ms.ScrollDelta.Y < 0) TryCurrentBlockDecrease();
            if (ms.ScrollDelta.Y > 0) TryCurrentBlockIncrease();
        }

        void TryCurrentBlockIncrease()
        {
            if (currentBlockID < BlockRegistry.MaxBlockID) currentBlockID++;
            Console.Clear();
            Console.WriteLine("currentBlock: " + BlockRegistry.GetBlock(currentBlockID).GetBlockName());
        }

        void TryCurrentBlockDecrease()
        {
            if (currentBlockID > 1) currentBlockID--;
            Console.Clear();
            Console.WriteLine("currentBlock: " + BlockRegistry.GetBlock(currentBlockID).GetBlockName());
        }

        //debug, will remove this later
        void DebugInteractions(ChunkManager world, KeyboardState ks)
        {
            DebugChunkState(world, ks);

            if (ks.IsKeyPressed(Keys.R))
            {
                Console.Clear();
                NoiseRouter.DebugPrint((int)Transform.position.X, (int)Transform.position.Z);
            }

            if (ks.IsKeyPressed(Keys.T)) Console.WriteLine(EntityManager.EntityCount);

            //debug, will remove this later
            SpawnRigidBody(ks);
            DebugRigidBody(ks);
            ManageTime(ks);
        }
        
        void SpawnRigidBody(KeyboardState ks)
        {
            
            if (ks.IsKeyPressed(Keys.Y))
            {
                int rand = RandomNumberGenerator.GetInt32(1000);
                Entity ent = EntityManager.AddEntity("phys " + rand);
                ent.Transform.position = Transform.position;

                DebugRenderBox box = ent.AddComponent<DebugRenderBox>();
                box.max = new Vector3(0.3f, 0.9f, 0.3f);
                box.min = new Vector3(-0.3f, -0.9f, -0.3f);
                box.SetUpRenderBox(Transform.Forward.Normalized());

                PhysicsObj rb = ent.AddComponent<PhysicsObj>();
                rb.bounds = new Vector3d(0.6, 1.8, 0.6);
                rb.AddImpulse(Transform.Forward * 25);
            }           
        }

        void DebugRigidBody(KeyboardState ks)
        {
            if (ks.IsKeyPressed(Keys.B))
            {
                PhysicsObj? rb = GameObject.GetComponent<PhysicsObj>();
                if (rb == null) return;

                Console.Clear();
                Console.WriteLine("Rigid Body Position: (" + rb.position.X + ", " + rb.position.Y + ", " + rb.position.Z + ")");
                Console.WriteLine("Rigid Body Velocity: (" + rb.velocity.X + ", " + rb.velocity.Y + ", " + rb.velocity.Z + ")");
                Console.WriteLine("Rigid Body Acceleration: (" + rb.acceleration.X + ", " + rb.acceleration.Y + ", " + rb.acceleration.Z + ")");
                Console.WriteLine("Rigid Body Grounded: " + rb.grounded);
                Console.WriteLine("Rigid Body in fluid: " + rb.inFluid);
                Console.WriteLine("Rigid Body Underwater: " + rb.underWater);
            }
        }

        void ManageTime(KeyboardState ks)
        {
            if (ks.IsKeyPressed(Keys.X))
            {
                slowTime = !slowTime;
                if (slowTime) EntityManager.TimeScale = 0.25;
                else EntityManager.TimeScale = 1.0;
            }
        }

        static void DebugChunkState(ChunkManager world, KeyboardState ks)
        {
            if (ks.IsKeyPressed(Keys.G))
            {
                Console.Clear();
                ChunkCoord coord = world.GetPlayerChunk();
                Chunk? chunk = world.GetChunk(coord);
                if (chunk != null)
                {
                    Console.WriteLine(chunk.GetState());
                }
                world.Debug();
            }
        }
    }
}