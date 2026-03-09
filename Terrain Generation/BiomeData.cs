namespace OurCraft.Terrain_Generation
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

        //for superflat worlds
        public static Biome EmptyBiome { get; private set; }

        public readonly static List<Biome> biomes = [];

        //create all the biomes
        static BiomeData()
        {
            //initialize the biome table and biomes
            Tundra = new Biome();
            EmptyBiome = new Biome();        
        }

        //loads up all the biomes
        public static void Init()
        {
            EmptyBiome = LoadBiomeQuick("EmptyBiome.json");
            Tundra = LoadBiomeQuick("Tundra.json");
        }

        //get the biome
        public static Biome FindBiome()
        {
            return Tundra;
        } 

        //helper method for quickly loading in biomes
        public static Biome LoadBiomeQuick(string json)
        {
            Biome biome = BiomeLoader.ToRuntimeBiome(BiomeLoader.LoadBiomeConfig(json));
            return biome;
        }
    }
}
