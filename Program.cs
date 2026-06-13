using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OurCraft;

static class Program
{
    //entry point, nah thats crazy you passed the first lesson of a c++ tutorial, fuck c++ btw, c# Terras
    private static void Main()
    {
        //create modern OpenGL context
        var nativeSettings = new NativeWindowSettings()
        {
            //default size
            ClientSize = new OpenTK.Mathematics.Vector2i(1920, 1080),
            Title = "OurCraft",

            //newest version, supports bindless textures
            API = ContextAPI.OpenGL,
            APIVersion = new Version(4, 6),
            
            //default settings
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible,
        };

        using var game = new Game(nativeSettings);
        game.Run();
    }
}