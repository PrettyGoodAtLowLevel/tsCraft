using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Blocks;
using OurCraft.Rendering;
using OurCraft.utility;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;
using static OurCraft.Physics.VoxelPhysics;
using OurCraft.World.Terrain_Generation;
using OpenTK.Graphics.OpenGL4;
using System.Text.Json.Nodes;
using OurCraft.Blocks.Meshing;

namespace OurCraft
{
    public class Game : GameWindow
    {
        static readonly int screenWidth = 1920;
        static readonly int screenHeight = 1080;

        public Game() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(screenWidth, screenHeight),
            Title = "OURcraft",
            Flags = ContextFlags.ForwardCompatible,
            WindowBorder = WindowBorder.Resizable
        }) { }

        Chunkmanager world;
        Camera cam = new Camera(screenWidth, screenHeight, new Vector3(0.5f, 145, 0.5f), 7.5f, 25);   
        ThreadPoolSystem worldGenThreads = new ThreadPoolSystem(8); //threads for initial chunk generation
        Renderer renderer;
        ushort currentBlock;
        double timer = 0;
        double rawTime = 0;
   
        //first load
        protected override void OnLoad()
        {           
            base.OnLoad();
            CursorState = CursorState.Grabbed;
            BlockData.InitBlocks();
            WorldGenerator.FlatWorld = false;
            world = new Chunkmanager(Program.renderDistance, ref cam, ref worldGenThreads);
            renderer = new Renderer(ref world, ref cam, screenWidth, screenHeight);
            world.Generate();
            currentBlock = BlockRegistry.GetBlock("Grass Block");
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
            bool hitBlock = RaycastVoxel(cam.Position, cam.Orientation, 4.0f, (x, y, z) => world.GetBlockState(new Vector3(x, y, z)).BlockID != BlockIDs.AIR_BLOCK, out VoxelRaycastHit hit);

            if (MouseState.IsButtonPressed(MouseButton.Left))
            {
                if (hitBlock)
                {
                    world.SetBlock(hit.blockPos, new BlockState(BlockIDs.AIR_BLOCK));
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

            //switch blocks
            if (MouseState.ScrollDelta.Y < 0) TryCurrentBlockDecrease();
            if (MouseState.ScrollDelta.Y > 0) TryCurrentBlockIncrease();

            if (KeyboardState.IsKeyDown(Keys.Z)) renderer.fov = 20;
            else renderer.fov = 90;

            if (KeyboardState.IsKeyPressed(Keys.C)) renderer.ToggleAOOff();
            if (KeyboardState.IsKeyPressed(Keys.V)) renderer.ToggleAOOn();

            if (KeyboardState.IsKeyPressed(Keys.R))
            {
                Console.Clear();
                NoiseRouter.DebugPrint((int)cam.Position.X, (int)cam.Position.Z);
            }

            world.Update((float)args.Time, (float)rawTime);
            timer += args.Time;

            if (timer >= 1)
            {
                long totalMem = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
                Title = "TsCraft, fps: " + (int)(1 / args.Time) + ", camera position (" + (int)cam.Position.X + ", " + ((int)cam.Position.Y) + ", " + (int)cam.Position.Z + ")";
                timer = 0;
            }
        }

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
    }
}

//base weapon settings (every weapon has some configuration of these + their own attack functions)
//float attackSpeed = 1.6 (can hit 1.6 times per sec)
//int baseDmg = 10 (does 5 hearts of damage on full charge attack)
//float splashRange = 1 (one block of splash/sweep range)
//float splashDropoff = 0.75 (the other enemies hit only take 75% of the base dmg)
//float knockBack = 1 (does one block of knockback)
//float critMultiplier = 1.25 (critical hits do 25% more damage)
//float critChance = 1.0 (every attempt at a critcal hit works)
//float attackReach = 3.0 (3 blocks of reach)