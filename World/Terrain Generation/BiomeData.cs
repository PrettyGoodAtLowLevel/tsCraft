namespace OurCraft.World.Terrain_Generation
{
    //these enums are soley used for a lookup chart and dont effect the behavior of the biome, only the placement
    //measures overall temperature
    public enum TemperatureIndex
    { 
        FREEZING,   //tundras, cold oceans
        COLD,       //taigas
        TEMPERATE,  //gravelly hills
        WARM,       //plains, forests
        HOT,        //savannas, deserts
    }

    //measures rainfall level
    public enum HumidityIndex
    {
        ARID,       //deserts, icelands
        DRY,        //savanna
        NORMAL,     //plains, forests
        HUMID,      //jungle
        WET         //swamps, mangroves
    }

    //measures plant count
    public enum VegetationIndex
    {
        BARREN,     //desert, plains
        SPARSE,     //sparse forest, gravelly hills
        DENSE       //forest
    }

    //the biome data class holds all of the biomes and initializes them
    //it also provides a fast lookup table for searching through and finding biomes
    //the lookup table uses the temperature, humidity, and vegetation, to index into an array
    //this array provides as a quick way to search for biomes with o(1) time, and being clean,
    //rather than use a million -if statements
    public static class BiomeData
    {
        public static Biome Tundra { get; private set; }
        public static Biome ColdDesert { get; private set; }
        public static Biome FrozenPeaks { get; private set; }

        //for superflat worlds
        public static Biome EmptyBiome { get; private set; }

        public readonly static List<Biome> biomes = [];
        static readonly Biome[,,] biomeTable;

        //create all the biomes
        static BiomeData()
        {
            //initialize the biome table and biomes
            Tundra = new Biome();
            ColdDesert = new Biome();
            FrozenPeaks = new Biome();
            EmptyBiome = new Biome();

            biomeTable = new Biome[TemperatureIndex.HOT.GetHashCode() + 1,
            HumidityIndex.WET.GetHashCode() + 1,
            VegetationIndex.DENSE.GetHashCode() + 1];          
        }

        //loads up all the biomes
        public static void Init()
        {
            EmptyBiome = LoadBiomeQuick("EmptyBiome.json");
            InitBiomes();
            InitializeBiomeTable();
            PropagateBiomeTable();
        }

        //get the biome
        public static Biome FindBiome(int temp, int humid, int veg)
        {
            return biomeTable[temp, humid, veg];
        }

        public static void InitBiomes()
        {
            InitTundra();
            InitColdDesert();
            InitFrozenPeaks();
        }

        //create biomes
        public static void InitializeBiomeTable()
        {
            foreach (Biome biome in biomes)
            {
                int tempIndex = biome.TempIndex.GetHashCode();
                int humidIndex = biome.HumidIndex.GetHashCode();
                int vegIndex = biome.VegetationIndex.GetHashCode();

                biomeTable[tempIndex, humidIndex, vegIndex] = biome;
            }
        }

        //spread out biomes if not all 75 biomes exist
        public static void PropagateBiomeTable()
        {
            int tempCount = biomeTable.GetLength(0);
            int humidCount = biomeTable.GetLength(1);
            int vegCount = biomeTable.GetLength(2);

            //keep repeating until all cells are filled
            bool filledAny;
            do
            {
                filledAny = false;

                for (int t = 0; t < tempCount; t++)
                {
                    for (int h = 0; h < humidCount; h++)
                    {
                        for (int v = 0; v < vegCount; v++)
                        {
                            if (biomeTable[t, h, v] == null)
                            {
                                Biome? nearest = FindNearestBiome(t, h, v);
                                if (nearest != null)
                                {
                                    biomeTable[t, h, v] = nearest;
                                    filledAny = true;
                                }
                            }
                        }
                    }
                }

            } while (filledAny);
        }

        //get best biome to propagate for the biome lookup
        private static Biome? FindNearestBiome(int t, int h, int v)
        {
            int tempCount = biomeTable.GetLength(0);
            int humidCount = biomeTable.GetLength(1);
            int vegCount = biomeTable.GetLength(2);

            Biome? bestBiome = null;
            double bestScore = double.MaxValue;

            foreach (Biome biome in biomes)
            {
                int tt = biome.TempIndex.GetHashCode();
                int hh = biome.HumidIndex.GetHashCode();
                int vv = biome.VegetationIndex.GetHashCode();

                //normalized differences
                double tempDiff = (t - tt) / (double)(tempCount - 1);
                double humidDiff = (h - hh) / (double)(humidCount - 1);
                double vegDiff = (v - vv) / (double)(vegCount - 1);

                //base weighted distance
                double dist = Math.Sqrt(
                    Math.Pow(tempDiff * 3.5, 2) +
                    Math.Pow(humidDiff * 1.0, 2) +
                    Math.Pow(vegDiff * 0.35, 2)
                );

                //add soft penalties for extreme mismatches
                double penalty = 1.0;
                if (Math.Abs(t - tt) >= 3) penalty *= 2.2;
                if (Math.Abs(h - hh) >= 3) penalty *= 1.8;
                if (Math.Abs(v - vv) >= 2) penalty *= 1.4;

                double score = dist * penalty;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestBiome = biome;
                }
            }

            return bestBiome;
        }

        //----biome initializations----
        //will move this to another file later on

        //the cold biome
        public static void InitTundra()
        {
            Tundra = LoadBiomeQuick("Tundra.json");
            biomes.Add(Tundra);
        }

        //rare cold biome
        public static void InitColdDesert()
        {
            ColdDesert = LoadBiomeQuick("ColdDesert.json");
            biomes.Add(ColdDesert);
        }

        //the cold alternative
        public static void InitFrozenPeaks()
        {
            FrozenPeaks = LoadBiomeQuick("FrozenPeaks.json");
            biomes.Add(FrozenPeaks);
        }

        //helper method for quickly loading in biomes
        public static Biome LoadBiomeQuick(string json)
        {
            Biome biome = BiomeLoader.ToRuntimeBiome(BiomeLoader.LoadBiomeConfig(json));
            return biome;
        }
    }
}
