using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Blocks;
using OurCraft.Rendering;
using OurCraft.utility;
using OurCraft.World;
using static OurCraft.Physics.VoxelPhysics;
using OurCraft.World.Terrain_Generation;

namespace OurCraft
{
    public class Game : GameWindow
    {
        static int screenWidth = 1920;
        static int screenHeight = 1080;

        public Game() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(screenWidth, screenHeight),
            Title = "OURcraft",
            Flags = ContextFlags.ForwardCompatible,
            WindowBorder = WindowBorder.Resizable
        }) { }

        Chunkmanager world;
        Camera cam = new Camera(screenWidth, screenHeight, new Vector3(0.5f, 145, 0.5f), 70.5f, 25);   
        ThreadPoolSystem worldGenThreads = new ThreadPoolSystem(8); //threads for initial chunk generation
        Renderer renderer;
        byte currentBlock = 1;
        double timer = 0;
        double rawTime = 0;

        //first load
        protected override void OnLoad()
        {
            base.OnLoad();
            CursorState = CursorState.Grabbed;
            world = new Chunkmanager(RenderDistances.TEN_CHUNKS, ref cam, ref worldGenThreads);
            renderer = new Renderer(ref world, ref cam, screenWidth, screenHeight);
            world.Generate();          
        }

        //when drawing things
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            //render your stuff here
            rawTime += args.Time;
            float shaderTime = (float)(rawTime % 1000.0);
            renderer.RenderSceneFrame(shaderTime);           
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
                    //get blocks
                    BlockState bottom, top, front, back, left, right;

                    bottom = world.GetBlockState(hit.blockPos + new Vector3(0, -1, 0));
                    top = world.GetBlockState(hit.blockPos + new Vector3(0, 1, 0));
                    front = world.GetBlockState(hit.blockPos + new Vector3(0, 0, 1));
                    back = world.GetBlockState(hit.blockPos + new Vector3(0, 0, -1));
                    right = world.GetBlockState(hit.blockPos + new Vector3(1, 0, 0));
                    left = world.GetBlockState(hit.blockPos + new Vector3(-1, 0, 0));

                    if (bottom.BlockID == BlockIDs.INVALID_BLOCK || top.BlockID == BlockIDs.INVALID_BLOCK || front.BlockID == BlockIDs.INVALID_BLOCK
                    || back.BlockID == BlockIDs.INVALID_BLOCK || right.BlockID == BlockIDs.INVALID_BLOCK || left.BlockID == BlockIDs.INVALID_BLOCK) return;

                    world.SetBlock(hit.blockPos, new BlockState(BlockIDs.AIR_BLOCK));
                    world.UpdateNeighborBlocks(hit.blockPos, bottom, top, front, back, right, left);
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

                    if (bottom.BlockID == BlockIDs.INVALID_BLOCK || top.BlockID == BlockIDs.INVALID_BLOCK || front.BlockID == BlockIDs.INVALID_BLOCK
                    || back.BlockID == BlockIDs.INVALID_BLOCK || right.BlockID == BlockIDs.INVALID_BLOCK || left.BlockID == BlockIDs.INVALID_BLOCK) return;

                    //try to add block
                    BlockData.GetBlock(currentBlock).PlaceBlockState(hit.blockPos, hit.faceNormal,
                    bottom, top, front, back, right, left,
                    world.GetBlockState(hit.blockPos), world);

                    world.UpdateNeighborBlocks(hit.blockPos, bottom, top, front, back, right, left);
                }
            }

            //switch blocks
            if (MouseState.ScrollDelta.Y < 0) TryCurrentBlockDecrease();
            if (MouseState.ScrollDelta.Y > 0) TryCurrentBlockIncrease();


            if (KeyboardState.IsKeyPressed(Keys.R))
            {
                Console.Clear();
                WorldGenerator.DebugValues((int)cam.Position.X, (int)cam.Position.Z);
            }

            if (KeyboardState.IsKeyDown(Keys.Z)) renderer.fov = 20;
            else renderer.fov = 90;

            world.Update((float)args.Time, (float)rawTime);
            timer += args.Time;

            if (timer >= 1)
            {
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