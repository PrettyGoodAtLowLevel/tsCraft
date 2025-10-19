using OurCraft.Blocks;

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

    //holds all the biomes
    public static class BiomeData
    {
        public static Biome Plains { get; private set; }
        public static Biome Desert { get; private set; }
        public static Biome Tundra { get; private set; }
        public static Biome ColdDesert { get; private set; }
        public static Biome Taiga { get; private set; }

        public readonly static List<Biome> biomes = [];
        static readonly Biome[,,] biomeTable;

        static BiomeData()
        {
            //initialize the biome table and biomes
            Plains = new Biome();
            Desert = new Biome();
            Tundra = new Biome();
            ColdDesert = new Biome();
            Taiga = new Biome();

            biomeTable = new Biome[TemperatureIndex.HOT.GetHashCode() + 1,
            HumidityIndex.WET.GetHashCode() + 1,
            VegetationIndex.DENSE.GetHashCode() + 1];

            //set up biomes
            InitBiomes();
            InitializeBiomeTable();
            PropagateBiomeTable();

            //debug test for biome table propagation
            /*
            for (int t = 0; t < 5; t++)
            {
                for (int h = 0; h < 5; h++)
                {
                    for (int v = 0; v < 3; v++)
                    {
                        Console.WriteLine($"{(TemperatureIndex)t}, {(HumidityIndex)h}, {(VegetationIndex)v} = {biomeTable[t, h, v].Name}");
                    }
                }
            }
            */
        }

        //get the biome
        public static Biome FindBiome(int temp, int humid, int veg)
        {
            return biomeTable[temp, humid, veg];
        }

        public static void InitBiomes()
        {
            InitPlains();
            InitDesert();
            InitTundra();
            InitColdDesert();
            InitTaiga();
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

        public static void InitPlains()
        {
            Plains.Name = "Plains";
            Plains.TempIndex = TemperatureIndex.WARM.GetHashCode();
            Plains.HumidIndex = HumidityIndex.NORMAL.GetHashCode();
            Plains.VegetationIndex = VegetationIndex.SPARSE.GetHashCode();
            Plains.RegularHeight = 130;
            Plains.OceanHeight = 100;
            Plains.ShoreHeight = 127;
            Plains.PeakHeight = 230;
            Plains.WaterBlock = BlockRegistry.GetBlock("Water");
            Plains.SurfaceBlock = BlockRegistry.GetBlock("Grass Block");
            Plains.SubSurfaceBlock = BlockRegistry.GetBlock("Dirt");
            Plains.PeakSurfaceBlock = BlockRegistry.GetBlock("Stone");
            Plains.PeakSubSurfaceBlock = BlockRegistry.GetBlock("Stone");
            Plains.ShoreSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Plains.ShoreSubSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Plains.OceanSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Plains.OceanSubSurfaceBlock = BlockRegistry.GetBlock("Sand");
            biomes.Add(Plains);
        }

        public static void InitDesert()
        {
            Desert.Name = "Desert";
            Desert.TempIndex = TemperatureIndex.HOT.GetHashCode();
            Desert.HumidIndex = HumidityIndex.ARID.GetHashCode();
            Desert.VegetationIndex = VegetationIndex.SPARSE.GetHashCode();
            Desert.RegularHeight = 130;
            Desert.OceanHeight = 100;
            Desert.ShoreHeight = 127;
            Desert.PeakHeight = 230;
            Desert.WaterBlock = BlockRegistry.GetBlock("Water");
            Desert.SurfaceBlock = BlockRegistry.GetBlock("Sand");
            Desert.SubSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Desert.PeakSurfaceBlock = BlockRegistry.GetBlock("Stone");
            Desert.PeakSubSurfaceBlock = BlockRegistry.GetBlock("Stone");
            Desert.ShoreSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Desert.ShoreSubSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Desert.OceanSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Desert.OceanSubSurfaceBlock = BlockRegistry.GetBlock("Stone");
            biomes.Add(Desert);
        }

        public static void InitTundra()
        {
            Tundra.Name = "Tundra";
            Tundra.TempIndex = TemperatureIndex.COLD.GetHashCode();
            Tundra.HumidIndex = HumidityIndex.NORMAL.GetHashCode();
            Tundra.VegetationIndex = VegetationIndex.SPARSE.GetHashCode();
            Tundra.RegularHeight = 130;
            Tundra.OceanHeight = 100;
            Tundra.ShoreHeight = 127;
            Tundra.PeakHeight = 240;
            Tundra.WaterBlock = BlockRegistry.GetBlock("Water");
            Tundra.SurfaceBlock = BlockRegistry.GetBlock("Snowy Grass Block");
            Tundra.SubSurfaceBlock = BlockRegistry.GetBlock("Dirt");
            Tundra.PeakSurfaceBlock = BlockRegistry.GetBlock("Snow");
            Tundra.PeakSubSurfaceBlock = BlockRegistry.GetBlock("Stone");
            Tundra.ShoreSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Tundra.ShoreSubSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Tundra.OceanSurfaceBlock = BlockRegistry.GetBlock("Sand");
            Tundra.OceanSubSurfaceBlock = BlockRegistry.GetBlock("Dirt");
            biomes.Add(Tundra);
        }

        public static void InitColdDesert()
        {
            ColdDesert.Name = "ColdDesert";
            ColdDesert.TempIndex = TemperatureIndex.FREEZING.GetHashCode();
            ColdDesert.HumidIndex = HumidityIndex.ARID.GetHashCode();
            ColdDesert.VegetationIndex = VegetationIndex.BARREN.GetHashCode();
            ColdDesert.RegularHeight = 130;
            ColdDesert.OceanHeight = 100;
            ColdDesert.ShoreHeight = 127;
            ColdDesert.PeakHeight = 180;
            ColdDesert.WaterBlock = BlockRegistry.GetBlock("Water");
            ColdDesert.SurfaceBlock = BlockRegistry.GetBlock("Snow");
            ColdDesert.SubSurfaceBlock = BlockRegistry.GetBlock("Dirt");
            ColdDesert.PeakSurfaceBlock = BlockRegistry.GetBlock("Snow");
            ColdDesert.PeakSubSurfaceBlock = BlockRegistry.GetBlock("Stone");
            ColdDesert.ShoreSurfaceBlock = BlockRegistry.GetBlock("Snow");
            ColdDesert.ShoreSubSurfaceBlock = BlockRegistry.GetBlock("Snow");
            ColdDesert.OceanSurfaceBlock = BlockRegistry.GetBlock("Snow");
            ColdDesert.OceanSubSurfaceBlock = BlockRegistry.GetBlock("Stone");
            biomes.Add(ColdDesert);
        }

        public static void InitTaiga()
        {
            Taiga.Name = "Taiga";
            Taiga.TempIndex = TemperatureIndex.TEMPERATE.GetHashCode();
            Taiga.HumidIndex = HumidityIndex.HUMID.GetHashCode();
            Taiga.VegetationIndex = VegetationIndex.DENSE.GetHashCode();
            Taiga.RegularHeight = 130;
            Taiga.OceanHeight = 100;
            Taiga.ShoreHeight = 127;
            Taiga.PeakHeight = 235;
            Taiga.WaterBlock = BlockRegistry.GetBlock("Water");
            Taiga.SurfaceBlock = BlockRegistry.GetBlock("Grass Block");
            Taiga.SubSurfaceBlock = BlockRegistry.GetBlock("Dirt");
            Taiga.PeakSurfaceBlock = BlockRegistry.GetBlock("Snow");
            Taiga.PeakSubSurfaceBlock = BlockRegistry.GetBlock("Stone");
            Taiga.ShoreSurfaceBlock = BlockRegistry.GetBlock("Grass Block");
            Taiga.ShoreSubSurfaceBlock = BlockRegistry.GetBlock("Dirt");
            Taiga.OceanSurfaceBlock = BlockRegistry.GetBlock("Grass Block");
            Taiga.OceanSubSurfaceBlock = BlockRegistry.GetBlock("Dirt");
            biomes.Add(Taiga);
        }
    }
}
