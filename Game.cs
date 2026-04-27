using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Blocks;
using OurCraft.Graphics;
using OurCraft.Utility;
using OurCraft.World;
using OurCraft.Terrain_Generation;
using OurCraft.Entities;

namespace OurCraft
{
    //base game class, holds all the game data
    public class Game : GameWindow
    {
        readonly ThreadPoolSystem terrainGenThreads;
        readonly ThreadPoolSystem lightingThread;
        readonly ChunkManager world;        
        readonly Renderer renderer;

        //creates threads, block data, entity data, then chunk manager + renderer
        public Game(): base(GameWindowSettings.Default, new NativeWindowSettings()
        {ClientSize = new Vector2i(RenderingConstants.SCREEN_WIDTH, RenderingConstants.SCREEN_HEIGHT)})
        {
            Title = "Our Craft";
            terrainGenThreads = new(threadCount:8);
            lightingThread = new(threadCount:1);

            TextureRegistry.InitTextures();
            BlockRegistry.InitBlocks();
            WorldGenerator.SetGlobalBlocks();
            SurfaceFeatureRegistry.InitSurfaceFeatures();
            BiomeData.Init();
            EntityManager.InitPlayer();

            world = new ChunkManager(renderDistance:5, ref terrainGenThreads, ref lightingThread);          
            renderer = new Renderer(ref world, RenderingConstants.SCREEN_WIDTH, RenderingConstants.SCREEN_HEIGHT);          
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

            EntityManager.FixedUpdate(world, args.Time);
            EntityManager.Update(world, args.Time, KeyboardState, MouseState);
            world.Update((float)args.Time); 
            
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