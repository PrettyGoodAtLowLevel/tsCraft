using OurCraft.Blocks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace OurCraft.World.Terrain_Generation
{
    //provides methods to get noise values for terrain generation
    //allows to customize the noise map values to your desire 
    //also lets you debug the world gen if you want to see why you are in a certain terrain
    //loads json noise files and maps their values to fastnoiselite objects
    public static class NoiseRouter
    {
        //terrain shaping noises
        static readonly FastNoiseLite regionalNoise;
        static readonly FastNoiseLite erosionNoise;
        static readonly FastNoiseLite riverNoise;
        static readonly FastNoiseLite weirdnessNoise;
        static readonly FastNoiseLite fractureNoise;

        //creates the detailed terrain shape
        static readonly FastNoiseLite detailNoise;

        //biome noise
        static readonly FastNoiseLite temperatureNoise;
        static readonly FastNoiseLite humidityNoise;
        static readonly FastNoiseLite vegetationNoise;

        //better randomness
        public static readonly int seed = 0;
        static readonly int offsetX = 0;
        static readonly int offsetZ = 0;

        //set up all of the noises
        static NoiseRouter()
        {
            //make seed gen
            seed = RandomNumberGenerator.GetInt32(int.MaxValue);
            Random rand = new(seed);
            offsetX = rand.Next(10000);
            offsetZ = rand.Next(10000);

            //create seeds
            int regionalSeed = rand.Next();
            int erosionSeed = rand.Next();
            int riverSeed = rand.Next();
            int weirdSeed = rand.Next();
            int fractureSeed = rand.Next();
            int temperatureSeed = rand.Next();
            int humiditySeed = rand.Next();
            int vegetationSeed = rand.Next();

            //load the noise settings
            regionalNoise = NoiseJson.JsonToFastNoise("RegionalNoise.json", regionalSeed);
            erosionNoise = NoiseJson.JsonToFastNoise("ErosionNoise.json", erosionSeed);
            riverNoise = NoiseJson.JsonToFastNoise("RiverNoise.json", riverSeed);
            weirdnessNoise = NoiseJson.JsonToFastNoise("WeirdnessNoise.json", weirdSeed);
            fractureNoise = NoiseJson.JsonToFastNoise("FractureNoise.json", fractureSeed);
            detailNoise = NoiseJson.JsonToFastNoise("DetailNoise.json", seed);
            temperatureNoise = NoiseJson.JsonToFastNoise("TemperatureNoise.Json", temperatureSeed);
            humidityNoise = NoiseJson.JsonToFastNoise("HumidityNoise.json", humiditySeed);
            vegetationNoise = NoiseJson.JsonToFastNoise("VegetationNoise.json", vegetationSeed);
        }

        //--noise functions--

        //get the regional noise (2d heightmap)
        public static float GetRegionalNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            regionalNoise.DomainWarp(ref wx, ref wz);
            return regionalNoise.GetNoise(wx, wz);
        }

        //get erosion level (2d heightmap)
        public static float GetErosionNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            erosionNoise.DomainWarp(ref wx, ref wz);
            return erosionNoise.GetNoise(wx, wz);
        }

        //get river cutout (2d heightmap)
        public static float GetRiverNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            riverNoise.DomainWarp(ref wx, ref wz);
            return riverNoise.GetNoise(wx, wz);
        }

        //get weirdness value (2d heightmap)
        public static float GetWeirdnessNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            weirdnessNoise.DomainWarp(ref wx, ref wz);
            return weirdnessNoise.GetNoise(wx, wz);
        }

        //get fracture value (extra 2d heightmap layered on weirdness)
        public static float GetFractureNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            fractureNoise.DomainWarp(ref wx, ref wz);
            return fractureNoise.GetNoise(wx, wz);
        }

        //get detail nose (3d density map)
        public static float GetDetailNoise(int x, int y, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            return detailNoise.GetNoise(wx, y * 1.25f, wz);
        }

        //get heat of world
        public static float GetTemperatureNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            temperatureNoise.DomainWarp(ref wx, ref wz);
            return temperatureNoise.GetNoise(wx, wz);
        }

        //get rainfall amount
        public static float GetHumidityNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            humidityNoise.DomainWarp(ref wx, ref wz);
            return humidityNoise.GetNoise(wx, wz);
        }

        //get forest density
        public static float GetVegetationNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
           vegetationNoise.DomainWarp(ref wx, ref wz);
            return vegetationNoise.GetNoise(wx, wz);
        }

        //get structure randomness 
        public static int GetStructureRandomness(int x, int y, int z, int worldSeed, int max)
        {
            //a fast 64-bit coordinate hash (based on x, y, z, and world seed)
            long hash = worldSeed;
            hash ^= (x * 374761393 + y * 668265263 + z * 2147483647);
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash = (hash ^ (hash >> 16));

            //return a positive 0–9999 range
            return (int)(Math.Abs(hash) % max + 1);
        }

        //weird structure variety, things like tree trunk height, found this code online
        public static int GetVariation(int x, int y, int z, int worldSeed, int salt = 0, int max = 10000)
        {
            unchecked
            {
                long hash = worldSeed * (long)0x9E3779B97F4A7C15L; //golden ratio prime
                hash ^= x * (long)0xBF58476D1CE4E5B9L;
                hash ^= y * (long)0x94D049BB133111EBL;
                hash ^= z * 0xDEADBEEFL;
                hash ^= salt * 0x123456789ABCL;

                //final avalanche (SplitMix64 style)
                hash ^= (hash >> 30);
                hash *= (long)0xBF58476D1CE4E5B9L;
                hash ^= (hash >> 27);
                hash *= (long)0x94D049BB133111EBL;
                hash ^= (hash >> 31);

                return (int)(Math.Abs(hash % max));
            }
        }

        //shows all of the noisemap values at a certain point
        public static void DebugPrint(int x, int z)
        {
            //find terrain shaper noise values
            float reg = GetRegionalNoise(x, z);
            float ero = GetErosionNoise(x, z);
            float riv = GetRiverNoise(x, z);
            float w = GetWeirdnessNoise(x, z);
            float fracture = GetFractureNoise(x, z);

            //find biome noise values
            float temp = GetTemperatureNoise(x, z);
            float humid = GetHumidityNoise(x, z);
            float veg = GetVegetationNoise(x, z);

            //convert into a less messy float format
            string formattedReg = DebugRegionalNoise(reg);
            string formattedEro = DebugErosionNoise(ero);
            string formattedRiv = DebugRiverNoise(riv);

            //find the current biome based on noisemap values
            float ft = TerrainSplines.temperatureSpline.Evaluate(temp);
            float fh = TerrainSplines.humiditySpline.Evaluate(humid);
            float fv = TerrainSplines.vegetationSpline.Evaluate(veg);
            Biome biome = WorldGenerator.GetBiome((int)ft, (int)fh, (int)fv);

            string formattedAmplification =
            (TerrainSplines.weirdnessSpline.Evaluate(w) + 
            TerrainSplines.fractureSpline.Evaluate(fracture)).ToString("F3");

            //print
            Console.WriteLine("========Terrain Builder========");
            Console.WriteLine("regional: " + formattedReg);
            Console.WriteLine("erosion: " + formattedEro);
            Console.WriteLine("river: " + formattedRiv);
            Console.WriteLine("amplification: " + formattedAmplification + '\n');

            Console.WriteLine("========Biome Builder========");
            Console.WriteLine("Temperature: " + (TemperatureIndex)ft);
            Console.WriteLine("Humidity: " + (HumidityIndex)fh);
            Console.WriteLine("Vegetation: " + (VegetationIndex)fv);
            Console.WriteLine("Biome: " + biome.Name);
        }

        public static string DebugRegionalNoise(float r)
        {
            if (r <= -0.75f)
                return "Far From Mainland";
            else if (r <= -0.4)
                return "Off Mainland";
            else if (r <= -0.25f)
                return "Coastal";
            else if (r <= 0.5)
                return "In Mainland";
            else
                return "Far Mainland";
        }

        public static string DebugErosionNoise(float e)
        {
            if (e <= -0.35f)
                return "Highland";
            else if  (e >= 0.2f)
                return "Highland";
            return "Lowland";
        }

        public static string DebugRiverNoise(float r)
        {
            if (r <= 0.2f && r >= 0)
                return "River";
            return "None";
        }
    }    

    //represents a section of noise in the world
    public readonly struct NoiseRegion
    {
        //height offset = base height of terrain
        public readonly float heightOffset;

        //amplification = how much the terrain can vary from base height
        public readonly float amplification;

        //how much this can displace terrain hard cap
        public readonly int maxDepth;

        //what region of the world you are in
        public readonly Biome biome;

        //dflt constructor
        public NoiseRegion(float heightOffset, float amplification, Biome biome, int maxDepth) : this()
        {
            this.heightOffset = heightOffset;
            this.amplification = amplification;
            this.biome = biome;
            this.maxDepth = maxDepth;
        }
    }

    //noise file represented in json
    public class NoiseJson
    {
        public string NoiseType { get; set; } = "";
        public string FractalType { get; set; } = "";
        public string DomainWarpType { get; set; } = "";
        public float Frequency { get; set; } = 0;
        public int Octaves { get; set; } = 0;
        public float Warp { get; set; } = 0;

        public static NoiseJson Load(string fileName, bool debug = false)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Resources/Data/WorldGen/Noises/{fileName}";
            string json = File.ReadAllText(path);

            //allow case-insensitive JSON property matching
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<NoiseJson>(json, options);

            if (debug)
            {
                if (result == null)
                    Console.WriteLine("Deserialization failed!");
                else
                    Console.WriteLine("Loaded noise config successfully!");
            }

            //if thing is null return default noise json fast noise lite
            if (result == null)
            {
                NoiseJson temp = new NoiseJson();
                temp.NoiseType = "OpenSimplex2";
                temp.FractalType = "FBm";
                temp.DomainWarpType = "OpenSimplex2";
                temp.Frequency = 0.01f;
                temp.Octaves = 0;
                temp.Warp = 0;
                return temp;
            }

            return result;
        }

        //grab json file and cast it to regular fast noise lite
        public static FastNoiseLite JsonToFastNoise(string fileName, int seed)
        {
            NoiseJson json = Load(fileName);
            FastNoiseLite noise = new();
            noise.SetSeed(seed);
            noise.SetNoiseType(NoiseTypeFromString(json.NoiseType));
            noise.SetFractalType(FractalTypeFromString(json.FractalType));
            noise.SetDomainWarpType(DomainWarpTypeFromString(json.DomainWarpType));
            noise.SetFrequency(json.Frequency);
            noise.SetFractalOctaves(json.Octaves);
            noise.SetDomainWarpAmp(json.Warp);
            return noise;
        }

        //get the current fractal type from json string value
        public static FastNoiseLite.FractalType FractalTypeFromString(string name)
        {
            return (FastNoiseLite.FractalType)Enum.Parse(typeof(FastNoiseLite.FractalType), name);
        }

        //same thing but for the regular noise type
        public static FastNoiseLite.NoiseType NoiseTypeFromString(string name)
        {
            return (FastNoiseLite.NoiseType)Enum.Parse(typeof(FastNoiseLite.NoiseType), name);
        }

        //same thing but for domain warp type
        public static FastNoiseLite.DomainWarpType DomainWarpTypeFromString(string name)
        {
            return (FastNoiseLite.DomainWarpType)Enum.Parse(typeof(FastNoiseLite.DomainWarpType), name);
        }
    }
}
