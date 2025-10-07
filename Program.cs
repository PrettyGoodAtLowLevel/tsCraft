using OurCraft;

//entry point (actually durr)
static class Program
{
    private static void Main()
    {
        Console.WriteLine("start");
        using var game = new Game();
        game.Run();
    }
}