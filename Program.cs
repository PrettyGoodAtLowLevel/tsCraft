using OurCraft;
using OurCraft.World;
using OurCraft.World.Terrain_Generation;

//entry point (actually durr)
static class Program
{
    public static RenderDistances renderDistance;
    private static void Main()
    {
        Console.WriteLine("Enter Render distance 2-16: ");
        string? input = Console.ReadLine();
        int renderDist;
        try
        {
            renderDist = int.Parse(input);
        }
        catch
        { 
            renderDist = 0;
        }
        if (renderDist > 16 || renderDist < 2)
        {
            Console.WriteLine("wrong formatt dumbass");
            return;
        }
        Console.WriteLine("Flat or normal world? true or false");
        bool flat;
        string? input2 = Console.ReadLine();

        try
        {
            flat = bool.Parse(input2);
        }
        catch
        {
            Console.WriteLine("Freaking idiot");
            flat = false;
            return;
        }
        renderDistance = (RenderDistances)renderDist - 2;
        Console.WriteLine(renderDistance.ToString());
        using var game = new Game();
        WorldGenerator.FlatWorld = flat;
        game.Run();
    }
}