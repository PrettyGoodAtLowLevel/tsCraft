using FastNoiseLiteRef;
using OurCraft.Terrain_Generation.Registries;
using System.Security.Cryptography;

namespace OurCraft.Terrain_Generation.Noise
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

        //3d noise controller
        static readonly FastNoiseLite weirdnessNoise;
        static readonly FastNoiseLite fractureNoise;

        //cave noise controllers
        static readonly FastNoiseLite caveSizeNoise;

        //creates the detailed terrain shape & caves
        static readonly FastNoiseLite detailNoise;
        static readonly FastNoiseLite caveNoise;

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
            seed = RandomNumberGenerator.GetInt32(int.MaxValue - 100);
            Random rand = new(seed);
            offsetX = rand.Next(10000);
            offsetZ = rand.Next(10000);

            //create seeds
            int regionalSeed = rand.Next();
            int erosionSeed = rand.Next();
            int riverSeed = rand.Next();
            int weirdSeed = rand.Next();
            int fractureSeed = rand.Next();
            int caveSizeSeed = rand.Next();
            int temperatureSeed = rand.Next();
            int humiditySeed = rand.Next();
            int vegetationSeed = rand.Next();

            //load the noise settings
            regionalNoise = NoiseJson.JsonToFastNoise("RegionalNoise.json", regionalSeed);
            erosionNoise = NoiseJson.JsonToFastNoise("ErosionNoise.json", erosionSeed);
            riverNoise = NoiseJson.JsonToFastNoise("RiverNoise.json", riverSeed);
            caveSizeNoise = NoiseJson.JsonToFastNoise("CaveSizeNoise.json", caveSizeSeed);

            weirdnessNoise = NoiseJson.JsonToFastNoise("WeirdnessNoise.json", weirdSeed);
            fractureNoise = NoiseJson.JsonToFastNoise("FractureNoise.json", fractureSeed);

            detailNoise = NoiseJson.JsonToFastNoise("DetailNoise.json", seed);
            caveNoise = NoiseJson.JsonToFastNoise("CaveNoise.json", seed + 1);

            temperatureNoise = NoiseJson.JsonToFastNoise("TemperatureNoise.Json", temperatureSeed);
            humidityNoise = NoiseJson.JsonToFastNoise("HumidityNoise.json", humiditySeed);
            vegetationNoise = NoiseJson.JsonToFastNoise("VegetationNoise.json", vegetationSeed);
        }

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

        //gets multiplier for cave size
        public static float GetCaveSizeNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            caveSizeNoise.DomainWarp(ref wx, ref wz);
            return caveSizeNoise.GetNoise(wx, wz);
        }

        //get detail nose (3d density map)
        public static float GetDetailNoise(int x, int y, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            return detailNoise.GetNoise(wx, y * 1.25f, wz);
        }

        //get cave noise
        public static float GetCaveNoise(int x, int y, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            return caveNoise.GetNoise(wx, y * 1.75f, wz);
        }

        //get heat of world
        public static float GetTemperatureNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            return temperatureNoise.GetNoise(wx, wz);
        }

        //get rainfall amount
        public static float GetHumidityNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
            return humidityNoise.GetNoise(wx, wz);
        }

        //get forest density
        public static float GetVegetationNoise(int x, int z)
        {
            float wx = x + offsetX, wz = z + offsetZ;
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
            float ca = GetCaveSizeNoise(x, z);

            //find biome noise values
            float temp = GetTemperatureNoise(x, z);
            float humid = GetHumidityNoise(x, z);
            float veg = GetVegetationNoise(x, z);

            float amp =
            SplineRegistry.weirdnessSpline.Evaluate(w) +
            SplineRegistry.fractureSpline.Evaluate(fracture) +
            SplineRegistry.erosionAmplificationSpline.Evaluate(ero);
            float caveAmp = SplineRegistry.caveSizeSpline.Evaluate(ca);

            //convert into a less messy float format
            string formattedReg = DebugRegionalNoise(reg);
            string formattedEro = DebugErosionNoise(ero);
            string formattedRiv = DebugRiverNoise(riv);
            string formattedAmp = DebugAmplification(amp);
            string formattedCA = DebugCaveAmplification(caveAmp);

            //find the current biome based on noisemap values
            float ft = SplineRegistry.temperatureSpline.Evaluate(temp);
            float fh = SplineRegistry.humiditySpline.Evaluate(humid);
            float fv = SplineRegistry.vegetationSpline.Evaluate(veg);          
            Biome biome = OverworldGenerator.GetBiome((int)ft, (int)fh, (int)fv);        

            //print
            Console.WriteLine("========Terrain Builder========");
            Console.WriteLine("regional: " + formattedReg);
            Console.WriteLine("erosion: " + formattedEro);
            Console.WriteLine("river: " + formattedRiv);
            Console.WriteLine("amplification: " + formattedAmp + '\n');

            Console.WriteLine("========Biome Builder========");
            Console.WriteLine("Temperature: " + (TemperatureIndex)ft);
            Console.WriteLine("Humidity: " + (HumidityIndex)fh);
            Console.WriteLine("Vegetation: " + (VegetationIndex)fv);
            Console.WriteLine("Biome: " + biome.Name);

            Console.WriteLine("========Cave Builder========");
            Console.WriteLine("Cave Amplification: " + formattedCA);
        }

        public static string DebugRegionalNoise(float r)
        {
            if (r <= -0.4f)       return "Off Mainland";
            else if (r <= -0.25f) return "Coastal";
            else if (r <= 0.5)    return "In Mainland";
            else                  return "Far Mainland";
        }

        public static string DebugErosionNoise(float e)
        {
            if (e <= -0.35f)     return "Highland";
            else if  (e >= 0.2f) return "Meadowland";
            return "Lowland";
        }

        public static string DebugRiverNoise(float r)
        {
            if (r <= 0.2f && r >= 0) return "River";
            return "None";
        }

        public static string DebugAmplification(float a)
        {
            if (a <= 10.5f)   return "Flat";
            else if (a <= 25) return "Bumpy";
            else if (a <= 50) return "Amplified";
            else if (a <= 100)return "Crazy";
            else              return "Fractured";
        }

        public static string DebugCaveAmplification(float a)
        {
            if (a <= 0.25f) return "No Caves";
            else if (a <= 0.5) return "Small Tunnels";
            else if (a <= 1.5) return "Regular Caves";
            else if (a <= 2.5) return "Larger Caves";
            else return "Massive Caverns";
        }
    }    
}
