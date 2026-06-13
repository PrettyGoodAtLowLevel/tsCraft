namespace OurCraft.Terrain_Generation.Registries
{
    //the biome data class holds all of the biomes and initializes them
    //it also provides a fast lookup table for searching through and finding biomes
    //the lookup table uses the temperature, humidity, and vegetation, to index into an array
    //this array provides as a quick way to search for biomes with o(1) time, and being clean,
    //rather than use a million -if statements
    public static class BiomeRegistry
    {
        //for superflat worlds
        public static Biome EmptyBiome { get; private set; } = new Biome();

        public readonly static List<Biome> worldGenBiomes = [];
        private static Biome[,,] biomeMap = new Biome[0, 0, 0];

        //loads up all the biomes 
        public static void Init()
        {
            EmptyBiome = LoadBiomeQuick("EmptyBiome.json", addToWorldGen:false);

            LoadBiomeQuick("Forest.json");
            LoadBiomeQuick("Tundra.json");
            LoadBiomeQuick("FrozenPeaks.json");
            LoadBiomeQuick("Taiga.json");        
            LoadBiomeQuick("WeirdForest.json");
            LoadBiomeQuick("Plains.json");
            LoadBiomeQuick("Desert.json");

            LoadBiomeTable();
        }

        //get the biome from lookup table
        public static Biome GetBiome(int temp, int humid, int veg)
        {
            return biomeMap[temp, humid, veg];
        } 

        //creates lookup table
        private static void LoadBiomeTable()
        {
            biomeMap = new Biome[(int)TemperatureIndex.HOT + 1, (int)HumidityIndex.WET + 1, (int)VegetationIndex.DENSE + 1];

            for(int x = 0; x <= (int)TemperatureIndex.HOT; x++)
            for (int y = 0; y <= (int)HumidityIndex.WET; y++)
            for (int z = 0; z <= (int)VegetationIndex.DENSE; z++)
            {
                biomeMap[x, y, z] = FindBiome(x, y, z);
            }                                         
        }

        //finds a biome based on distance lookup table
        private static Biome FindBiome(int temp, int humid, int veg)
        {
            Biome closest = EmptyBiome;
            float minDist = float.PositiveInfinity;

            foreach (Biome biome in worldGenBiomes)
            {
                float dist = BiomeDistance(temp, humid, veg, biome);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = biome;
                }
            }
            return closest;
        }

        //computes the distance between temperature values of the world and a biome
        private static float BiomeDistance(int t, int h, int v, Biome biome)
        {          
            const float tempWeight = 1.75f;  //temp is the most powerful biome factor
            const float humidWeight = 0.8f;  //humidity is a strong biome factor
            const float vegWeight = 0.5f;    //vegetation to make small differences, (forest vs plains or snowy forest vs snowy plain)

            float dt = t - biome.TempIndex;
            float dh = h - biome.HumidIndex;
            float dv = v - biome.VegetationIndex;

            //squared distance — no sqrt needed, just for comparison
            return (dt * dt * tempWeight) + (dh * dh * humidWeight) + (dv * dv * vegWeight);
        }

        //helper method for quickly loading in biomes
        public static Biome LoadBiomeQuick(string json, bool addToWorldGen = true)
        {
            Biome biome = BiomeLoader.ToRuntimeBiome(BiomeLoader.LoadBiomeConfig(json));
            if (addToWorldGen) worldGenBiomes.Add(biome);
            return biome;
        }
    }
}
