using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.utility;
using OurCraft.World;
using OurCraft.World.Terrain_Generation;
using OurCraft.World.Terrain_Generation.SurfaceFeatures;
using static OurCraft.Physics.VoxelPhysics;

namespace OurCraft
{
    public class Game : GameWindow
    {
        static readonly int screenWidth = 1920;
        static readonly int screenHeight = 1080;

        #pragma warning disable 
        public Game() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(screenWidth, screenHeight),
            Title = "OURcraft",
            Flags = ContextFlags.ForwardCompatible,
            WindowBorder = WindowBorder.Resizable
        }){  }
        #pragma warning enable

        Chunkmanager world;
        ThreadPoolSystem worldGenThreads = new ThreadPoolSystem(8); //threads for initial chunk generation
        ThreadPoolSystem lightingThread = new ThreadPoolSystem(1); //worker thread for lighting

        Camera cam = new Camera(screenWidth, screenHeight, new Vector3(0.5f, 145, 0.5f), 7.5f, 25);           
        Renderer renderer;
        ushort currentBlock;
        double timer = 0;
        double rawTime = 0;
   
        //first load
        protected override void OnLoad()
        {           
            base.OnLoad();
            CursorState = CursorState.Grabbed;

            //initialize all the blocks and world gen settings
            BlockData.InitBlocks();
            WorldGenerator.SetGlobalBlocks();
            SurfaceFeatureRegistry.InitializeFeatures();
            BiomeData.Init();

            //create chunk manager + renderer and generate the world
            world = new Chunkmanager(Program.renderDistance, ref cam, ref worldGenThreads, ref lightingThread);
            renderer = new Renderer(ref world, ref cam, screenWidth, screenHeight);
            world.Generate();
            currentBlock = BlockRegistry.GetBlockID("Grass Block");
        }

        //when drawing things
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            //render your stuff here
            rawTime += args.Time;
            float shaderTime = (float)(rawTime % 1000.0);
            renderer.RenderSceneFrame(shaderTime, (float)args.Time);    
            
            SwapBuffers();
        }

        //update
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            cam.HandleInput(KeyboardState, MouseState, this, (float)args.Time);

            //handle gameplay
            HandleBlockInteractions();
            ScrollBlocks();
            Zoom();
            DebugInteractions();
            ToggleDayNight();

            //update world and log fps
            world.Update((float)args.Time, (float)rawTime);
            timer += args.Time;

            if (timer >= 1)
            {
                UpdateTitle(args);
            }
        }

        //when game is done running
        protected override void OnUnload()
        {
            base.OnUnload();
            worldGenThreads.Dispose();
            world.Delete();
        }

        //when window changes size
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            renderer.ResizeScene(Size.X, Size.Y);
        }


        //game function logic, eventually will be moved to seperate files when working on actual gameplay
        void TryCurrentBlockIncrease()
        {
            if (currentBlock < BlockData.MAXBLOCKID) currentBlock++;
            Console.Clear();
            Console.WriteLine("currentBlock: " + BlockData.GetBlock(currentBlock).GetBlockName());
        }

        void TryCurrentBlockDecrease()
        {
            if (currentBlock > 1) currentBlock--;
            Console.Clear();
            Console.WriteLine("currentBlock: " + BlockData.GetBlock(currentBlock).GetBlockName());
        }     
        
        void HandleBlockInteractions()
        {
            bool hitBlock = RaycastVoxel(cam.Position, cam.Orientation, 4.0f, (x, y, z) => world.GetBlockState(new Vector3(x, y, z)) != Block.AIR, out VoxelRaycastHit hit);

            //gameplay
            if (MouseState.IsButtonPressed(MouseButton.Left))
            {
                if (hitBlock)
                {
                    world.SetBlock(hit.blockPos, Block.AIR);
                }
            }
            if (MouseState.IsButtonPressed(MouseButton.Right))
            {
                if (hitBlock)
                {
                    //get blocks
                    BlockState bottom, top, front, back, left, right;

                    bottom = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, -1, 0));
                    top = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 1, 0));
                    front = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 0, 1));
                    back = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 0, -1));
                    right = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(1, 0, 0));
                    left = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(-1, 0, 0));

                    //try to add block
                    BlockData.GetBlock(currentBlock).PlaceBlockState(hit.blockPos, hit.faceNormal,
                    bottom, top, front, back, right, left,
                    world.GetBlockState(hit.blockPos), world);
                }
            }
            //debug block and light value
            if (MouseState.IsButtonPressed(MouseButton.Middle))
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

        void ScrollBlocks()
        {
            //switch blocks
            if (MouseState.ScrollDelta.Y < 0) TryCurrentBlockDecrease();
            if (MouseState.ScrollDelta.Y > 0) TryCurrentBlockIncrease();
        }

        void Zoom()
        {
            if (KeyboardState.IsKeyDown(Keys.Z)) renderer.fov = 20;
            else renderer.fov = 90;
        }

        void DebugInteractions()
        {
            //debugging
            if (KeyboardState.IsKeyPressed(Keys.G))
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

            if (KeyboardState.IsKeyPressed(Keys.R))
            {
                Console.Clear();
                NoiseRouter.DebugPrint((int)cam.Position.X, (int)cam.Position.Y);
            }
        }

        void ToggleDayNight()
        {
            if (KeyboardState.IsKeyPressed(Keys.D1)) renderer.ToggleDay();
            else if (KeyboardState.IsKeyPressed(Keys.D2)) renderer.ToggleNight();
        }

        void UpdateTitle(FrameEventArgs args)
        {
            long totalMem = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
            Title = "TsCraft, fps: " + (int)(1 / args.Time) + ", camera position (" + (int)cam.Position.X + ", " + ((int)cam.Position.Y) + ", " + (int)cam.Position.Z + ")";
            timer = 0;
        }
    }
}