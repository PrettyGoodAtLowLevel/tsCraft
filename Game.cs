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
        public Game(NativeWindowSettings settings): base(GameWindowSettings.Default, settings)
        {
            terrainGenThreads = new(threadCount:8);
            lightingThread = new(threadCount:1);

            TextureRegistry.InitTextures();
            BlockRegistry.InitBlocks();
            WorldGenerator.SetGlobalBlocks();
            SurfaceFeatureRegistry.InitSurfaceFeatures();
            BiomeData.Init();
            EntityManager.InitPlayer();
            Time.Reset();
            
            world = new ChunkManager(renderDistance:5, ref terrainGenThreads, ref lightingThread);          
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
               
            world.Update();
            Time.Increment(args);

            if (KeyboardState.IsKeyPressed(Keys.D5)) DebugProfiling();
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
            Console.WriteLine("Average Physics Update Time: " + (float)Profiler.GetProfileEntry("Fixed Update").AverageMs + " ms");
            Console.WriteLine("Average Entity Update Time: " + (float)Profiler.GetProfileEntry("Update").AverageMs + " ms");

            Console.WriteLine();
            Console.WriteLine("--------Rendering--------");
            Console.WriteLine("Average Chunk Render Time: " + (float)Profiler.GetProfileEntry("Chunk Rendering").AverageMs + " ms");
            Console.WriteLine("Average Entity Render Time: " + (float)Profiler.GetProfileEntry("Entity Rendering").AverageMs + " ms");
        }
    }
}