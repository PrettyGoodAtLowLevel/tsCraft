using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Blocks;
using OurCraft.Graphics;
using OurCraft.utility;
using OurCraft.World;
using OurCraft.Terrain_Generation;
using OurCraft.Terrain_Generation.SurfaceFeatures;
using OurCraft.Entities;

namespace OurCraft
{
    //base game class, holds all the game data
    public class Game : GameWindow
    {
        const int SCREEN_WIDTH = 1920;
        const int SCREEN_HEIGHT = 1080;

        readonly ThreadPoolSystem worldGenThreads;
        readonly ThreadPoolSystem lightingThread;
        readonly ChunkManager world;        
        readonly Renderer renderer;
        double timer = 0;

        //creates threads, block data, entity data, then chunk manager + renderer
        public Game() : base(GameWindowSettings.Default, new NativeWindowSettings(){ClientSize = new Vector2i(SCREEN_WIDTH, SCREEN_HEIGHT)})
        {
            worldGenThreads = new(threadCount:8);
            lightingThread = new(threadCount:1);

            BlockData.InitBlocks();
            WorldGenerator.SetGlobalBlocks();
            SurfaceFeatureRegistry.InitializeFeatures();
            BiomeData.Init();
            EntityManager.Init();

            world = new ChunkManager(RenderDistances.SIX_CHUNKS, ref worldGenThreads, ref lightingThread);          
            renderer = new Renderer(ref world, SCREEN_WIDTH, SCREEN_HEIGHT);          
        }

        //first load, create resources here
        protected override void OnLoad()
        {           
            base.OnLoad();
            CursorState = CursorState.Grabbed;    
            world.Generate();
        }

        //rendering one frame, called after update, render things here
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            renderer.RenderSceneFrame();               
            SwapBuffers();
        }

        //update, things like physics gameplay
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            EntityManager.Update(world, args.Time, KeyboardState, MouseState);
            world.Update((float)args.Time);

            timer += args.Time;
            if (timer >= 1) UpdateTitle(args);        

            if (KeyboardState.IsKeyPressed(Keys.Escape)) Close();
        }

        //when closing the screen or done playing, manage resources here
        protected override void OnUnload()
        {
            base.OnUnload();
            worldGenThreads.Dispose();
        }

        //any logic when rezising the screen
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            renderer.ResizeScene(Size.X, Size.Y);
        }

        //logs the fps to the screen
        void UpdateTitle(FrameEventArgs args)
        {
            Title = "OURCraft, fps: " + (int)(1 / args.Time);
            timer = 0;
        }
    }
}