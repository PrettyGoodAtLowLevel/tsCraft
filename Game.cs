using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Utility;
using OurCraft.Blocks;
using OurCraft.World;
using OurCraft.Graphics;
using OurCraft.Entities;
using OurCraft.Terrain_Generation.Registries;
using OurCraft.Terrain_Generation;

namespace OurCraft
{
    //base game class, holds all the game data
    public class Game : GameWindow
    {
        readonly ThreadPoolSystem terrainGenThreads;
        readonly ThreadPoolSystem lightingThread;
        readonly ChunkManager world;        
        readonly Renderer renderer;

        //creates block data, entity data, then chunk manager + renderer
        public Game(NativeWindowSettings settings): base(GameWindowSettings.Default, settings)
        {
            TextureRegistry.InitTextures();
            BlockRegistry.InitBlocks();

            OverworldGenerator.SetGlobalBlocks();
            SurfaceFeatureRegistry.InitSurfaceFeatures();
            DepositRegistry.InitDeposits();
            BiomeRegistry.Init();       

            EntityManager.InitPlayer();
            Time.Reset();

            terrainGenThreads = new(threadCount: 4);
            lightingThread = new(threadCount: 1);

            world = new ChunkManager(renderDistance:6, ref terrainGenThreads, ref lightingThread);          
            renderer = new Renderer(ref world, RenderingConstants.SCREEN_WIDTH, RenderingConstants.SCREEN_HEIGHT);
        }

        //first load, create resources here
        protected override void OnLoad()
        {           
            base.OnLoad();

            CursorState = CursorState.Grabbed;    
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

            using (Profiler.Scope("Fixed Update")) EntityManager.FixedUpdate(world);
            using (Profiler.Scope("Update")) EntityManager.Update(world, KeyboardState, MouseState);
            using (Profiler.Scope("World Update")) world.Update();

            Time.Increment(args);

            if (KeyboardState.IsKeyPressed(Keys.Escape)) Close();
        }

        //when closing the screen or done playing, manage resources here
        protected override void OnUnload()
        {
            base.OnUnload();
            terrainGenThreads.Dispose();
            lightingThread.Dispose();
        }

        //any logic when rezising the screen
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            renderer.ResizeScene(Size.X, Size.Y);
        }
    }
}