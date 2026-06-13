using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Terrain_Generation.Noise;
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
        public Transform? orientation;

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

        public override void OnUpdate(ChunkManager world, KeyboardState kb, MouseState ms)
        {
            if (orientation == null) return;

            ScrollBlocks(ms);
            HandleBlockInteractions(world, ms, orientation);
            DebugInteractions(world, kb, orientation);
        }

        void HandleBlockInteractions(ChunkManager world, MouseState ms, Transform orientation)
        {
            bool hitBlock = AABBMath.RaycastVoxel(orientation.WorldPosition, orientation.Forward, reach,
            (x, y, z) => world.GetBlockState(new Vector3(x, y, z)) != Block.AIR && world.GetBlockState(new Vector3(x, y, z)) != waterBlock,
            out VoxelRaycastHit hit);

            //gameplay
            if (ms.IsButtonPressed(MouseButton.Left))
            {
                if (hitBlock) world.SetBlock(hit.blockPos, Block.AIR);              
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

                    CollisionShape predictedShape = BlockRegistry.GetBlock(currentBlockID).GetPredictedCollisionShape(hit.blockPos, hit.faceNormal,
                    bottom, top, front, back, right, left,
                    world.GetBlockState(hit.blockPos), world);

                    foreach(var aabb in predictedShape.aabbs)
                    if (AABBMath.IntersectsLocal(aabb, AABBMath.GetAABB(rb.position, rb.boundsMin, rb.boundsMax), hit.blockPos + hit.faceNormal, Vector3d.Zero)) return;

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
                    Console.WriteLine(state.ToString());
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
        void DebugInteractions(ChunkManager world, KeyboardState ks, Transform orientation)
        {
            DebugChunkState(world, ks);

            if (ks.IsKeyPressed(Keys.D5)) DebugProfiling();

            if (ks.IsKeyPressed(Keys.R))
            {
                Console.Clear();
                NoiseRouter.DebugPrint((int)Transform.WorldPosition.X, (int)Transform.WorldPosition.Z);
            }

            if (ks.IsKeyPressed(Keys.T)) Console.WriteLine(EntityManager.EntityCount);

            //debug, will remove this later
            SpawnTestEntity(ks, orientation);
            DebugRigidBody(ks);
            ManageTime(ks);
        }

        static void SpawnTestEntity(KeyboardState ks, Transform orientation)
        {
            if (ks.IsKeyPressed(Keys.Y))
            {
                int rand = RandomNumberGenerator.GetInt32(1000);

                Entity entCenter = EntityManager.AddEntity("phys " + rand);
                entCenter.Transform.localPosition = orientation.WorldPosition;
                /*
                EntityRender model = entCenter.AddComponent<EntityRender>();

                if (rand % 12 == 0) model.LoadModel("horse.json", "Textures/Mc Skins/horseBlack.png");
                else if (rand % 12 == 1) model.LoadModel("cloveModel.json", "Textures/Mc Skins/Clove val mc skin.png");
                else if (rand % 12 == 2) model.LoadModel("creeper.json", "Textures/Mc Skins/creeper.png");
                else if (rand % 12 == 3) model.LoadModel("ravager.json", "Textures/Mc Skins/ravager.png");
                else if (rand % 12 == 4) model.LoadModel("enderMan.json", "Textures/Mc Skins/enderman.png");
                else if (rand % 12 == 5) model.LoadModel("witch.json", "Textures/Mc Skins/witch.png");
                else if (rand % 12 == 6) model.LoadModel("wolf.json", "Textures/Mc Skins/wolf.png");
                else if (rand % 12 == 7) model.LoadModel("hoglin.json", "Textures/Mc Skins/hoglin.png");
                else if (rand % 12 == 8) model.LoadModel("panda.json", "Textures/Mc Skins/panda.png");
                else if (rand % 12 == 9) model.LoadModel("evoker.json", "Textures/Mc Skins/evoker.png");
                else if (rand % 12 == 10) model.LoadModel("warden.json", "Textures/Mc Skins/warden.png");
                else model.LoadModel("spider.json", "Textures/Mc Skins/spider.png");

                model.model.root.localScale *= (Vector3.One * (1.8f / 2));
                model.model.root.localPosition += Vector3d.UnitY * -(1.8f / 2);
                */

                //physics
                PhysicsObj rb = entCenter.AddComponent<PhysicsObj>();

                Vector3d boundsMin = new Vector3d(-0.3, -0.9, -0.3);
                Vector3d boundsMax = new Vector3d(0.3, 0.5, 0.3);

                rb.boundsMin = boundsMin;
                rb.boundsMax = boundsMax;
                rb.AddImpulse(orientation.Forward * 25);

                DebugRenderBox aabbbox = entCenter.AddComponent<DebugRenderBox>();
                aabbbox.min = (Vector3)boundsMin;
                aabbbox.max = (Vector3)boundsMax;
                aabbbox.SetUpRenderBox(orientation.Forward);
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
                Console.WriteLine("Rigid Body Sneaking: " + rb.sneaking);
                Console.WriteLine("Rigid Body In Noclip: " + rb.noClip);
            }
        }

        void ManageTime(KeyboardState ks)
        {
            if (ks.IsKeyPressed(Keys.X))
            {
                slowTime = !slowTime;
                if (slowTime) Time.TimeScale = 0.05;
                else Time.TimeScale = 1.0;
            }
        }

        static void DebugChunkState(ChunkManager world, KeyboardState ks)
        {
            if (ks.IsKeyPressed(Keys.G))
            {
                Console.Clear();
                ChunkCoord coord = world.GetPlayerChunk();
                Chunk? chunk = world.GetChunk(coord);

                if (chunk != null) Console.WriteLine("Current Player Chunk State: " + chunk.GetState());                
                world.Debug();
            }
        }

        //log a message on how much on average each "expensive" task takes
        static void DebugProfiling()
        {
            Console.Clear();
            Console.WriteLine("--------Chunk Generation--------");
            Console.WriteLine("Average Terrain Gen Time: " + (int)Profiler.GetProfileEntry("Terrain Gen").AverageMs + " ms");
            Console.WriteLine("Average Structure Gen Time: " + (int)Profiler.GetProfileEntry("Structure Gen").AverageMs + " ms");
            Console.WriteLine("Average Lighting Time: " + (int)Profiler.GetProfileEntry("Lighting").AverageMs + " ms");
            Console.WriteLine("Average Mesh Gen Time: " + (int)Profiler.GetProfileEntry("Mesh Gen").AverageMs + " ms");
            Console.WriteLine("Average GL Upload Time: " + (float)Profiler.GetProfileEntry("GL Upload").AverageMs + " ms");

            Console.WriteLine();
            Console.WriteLine("--------Game Logic--------");
            Console.WriteLine("Average World Update Time: " + (float)Profiler.GetProfileEntry("World Update").AverageMs + " ms");
            Console.WriteLine("Average Physics Update Time: " + (float)Profiler.GetProfileEntry("Fixed Update").AverageMs + " ms");
            Console.WriteLine("Average Entity Update Time: " + (float)Profiler.GetProfileEntry("Update").AverageMs + " ms");

            Console.WriteLine();
            Console.WriteLine("--------Rendering--------");
            Console.WriteLine("Average Chunk Render Time: " + (float)Profiler.GetProfileEntry("Chunk Rendering").AverageMs + " ms");
            Console.WriteLine("Average Entity Render Time: " + (float)Profiler.GetProfileEntry("Entity Rendering").AverageMs + " ms");
        }
    }
}