namespace OurCraft.Terrain_Generation.Registries
{
    //contains all ores in a ore map
    public static class DepositRegistry
    {
        static readonly Dictionary<string, Deposit> depositMap = [];

        //put all deposits you want to load here
        public static void InitDeposits()
        {
            Deposit.RegisterDeposit("GravelOre.json");
            Deposit.RegisterDeposit("DirtOre.json");

            Deposit.RegisterDeposit("LapizOre.json");
            Deposit.RegisterDeposit("EmeraldOre.json");
            Deposit.RegisterDeposit("RedstoneOre.json");

            Deposit.RegisterDeposit("SurfaceGravel.json");
            Deposit.RegisterDeposit("SurfaceDirt.json");

            Deposit.RegisterDeposit("IcePatch.json");
        }

        public static Deposit GetDeposit(string name)
        {
            if (depositMap.TryGetValue(name, out var deposit)) return deposit;

            Console.WriteLine($"[OreRegistry] Warning: Deposit '{name}' not found!");
            return new Deposit(); //empty deposit
        }

        public static void AddDeposit(Deposit deposit, string name)
        {
            depositMap.TryAdd(name, deposit);
        }

        public static IEnumerable<Deposit> GetAllDeposits()
        {
            return depositMap.Values;
        }
    }
}