using System.Security.Cryptography;

namespace OurCraft.World.Terrain_Generation
{
    //provides methods to get noise values for terrain generation
    //allows to customize the noise map values to your desire 
    //also lets you debug the world gen if you want to see why you are in a certain terrain
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
        static readonly int seed = 0;
        static readonly int offsetX = 0;
        static readonly int offsetZ = 0;

        //noise settings
        //continents
        static readonly float regionalNoiseFreq = 0.00025f;
        static readonly int regionalNoiseOctaves = 4;
        static readonly float regionalNoiseWarp = 300;

        //hills, highlands
        static readonly float erosionNoiseFreq = 0.00065f;
        static readonly int erosionNoiseOctaves = 3;
        static readonly float erosionNoiseWarp = 300;

        //creates rivers in flatlands
        static readonly float riverNoiseFreq = 0.001f;
        static readonly int riverNoiseOctaves = 2;
        static readonly float riverNoiseWarp = 300;

        //creates random large bumps onto the terrain
        static readonly float weirdnessNoiseFreq = 0.002f;
        static readonly int weirdnessNoiseOctaves = 3;
        static readonly float weirdnessNoiseWarp = 300;

        //rare crazy fantasy terrain modifieer
        static readonly float fractureNoiseFreq = 0.001f;
        static readonly int fractureNoiseOctaves = 2;
        static readonly float fractureNoiseWarp = 300;

        //small bumps
        static readonly float detailNoiseFreq = 0.005f;
        static readonly int detailNoiseOctaves = 4;

        //how hot the world is
        static readonly float temperatureNoiseFreq = 0.00025f;
        static readonly int temperatureNoiseOctaves = 2;
        static readonly float temperatureNoiseWarp = 20;

        //how much rainfall the world has
        static readonly float humidityNoiseFreq = 0.00025f;
        static readonly int humidityNoiseOctaves = 2;
        static readonly float humidityNoiseWarp = 20;

        //the amount of trees and plants in the world
        static readonly float vegetationNoiseFreq = 0.0004f;
        static readonly int vegetationNoiseOctaves = 2;
        static readonly float vegetationNoiseWarp = 20;

        //set up all of the noises
        static NoiseRouter()
        {
            //create random seeds for noisemaps
            seed = RandomNumberGenerator.GetInt32(int.MaxValue);
            Random rand = new(seed);
            offsetX = rand.Next(10000);
            offsetZ = rand.Next(10000);
            int regionalSeed = rand.Next();
            int erosionSeed = rand.Next();
            int riverSeed = rand.Next();
            int weirdSeed = rand.Next();
            int fractureSeed = rand.Next();
            int temperatureSeed = rand.Next();
            int humiditySeed = rand.Next();
            int vegetationSeed = rand.Next();

            //initialize noises
            regionalNoise = new FastNoiseLite(regionalSeed);
            erosionNoise = new FastNoiseLite(erosionSeed);
            riverNoise = new FastNoiseLite(riverSeed);
            weirdnessNoise = new FastNoiseLite(weirdSeed);
            fractureNoise = new FastNoiseLite(fractureSeed);
            detailNoise = new FastNoiseLite(seed);
            temperatureNoise = new FastNoiseLite(temperatureSeed);
            humidityNoise = new FastNoiseLite(humiditySeed);
            vegetationNoise = new FastNoiseLite(vegetationSeed);

            //actually set their noise settings to desired value
            ConfigureRegionNoise();
            ConfigureErosionNoise();
            ConfigureRiverNoise();
            ConfigureWierdnessNoise();
            ConfigureFractureNoise();
            ConfigureDetailNoise();
            ConfigureTemperatureNoise();
            ConfigureHumidityNoise();
            ConfigureVegetationNoise();
        }

        //noise configurations

        //creates large oceans and land masses
        static void ConfigureRegionNoise()
        {
            regionalNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            regionalNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            regionalNoise.SetFrequency(regionalNoiseFreq);
            regionalNoise.SetFractalOctaves(regionalNoiseOctaves);
            regionalNoise.SetDomainWarpAmp(regionalNoiseWarp);
        }

        //creates highlands or low lands
        static void ConfigureErosionNoise()
        {
            erosionNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            erosionNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            erosionNoise.SetFrequency(erosionNoiseFreq);
            erosionNoise.SetFractalOctaves(erosionNoiseOctaves);
            erosionNoise.SetDomainWarpAmp(erosionNoiseWarp);
        }

        //carves rivers into the world
        static void ConfigureRiverNoise()
        {
            riverNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            riverNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            riverNoise.SetFrequency(riverNoiseFreq);
            riverNoise.SetFractalOctaves(riverNoiseOctaves);
            riverNoise.SetDomainWarpAmp(riverNoiseWarp);
        }

        //creates weird terrain
        static void ConfigureWierdnessNoise()
        {
            weirdnessNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            weirdnessNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            weirdnessNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            weirdnessNoise.SetFrequency(weirdnessNoiseFreq);
            weirdnessNoise.SetFractalOctaves(weirdnessNoiseOctaves);
            weirdnessNoise.SetDomainWarpAmp(weirdnessNoiseWarp);
        }

        //creates crazy terrain
        static void ConfigureFractureNoise()
        {
            fractureNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            fractureNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            fractureNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            fractureNoise.SetFrequency(fractureNoiseFreq);
            fractureNoise.SetFractalOctaves(fractureNoiseOctaves);
            fractureNoise.SetDomainWarpAmp(fractureNoiseWarp);
        }

        //creates rugged terrain shape
        static void ConfigureDetailNoise()
        {
            detailNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            detailNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
            detailNoise.SetFrequency(detailNoiseFreq);
            detailNoise.SetFractalOctaves(detailNoiseOctaves);
        }

        //how hot is this area?
        static void ConfigureTemperatureNoise()
        {
            temperatureNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            temperatureNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            temperatureNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            temperatureNoise.SetFrequency(temperatureNoiseFreq);
            temperatureNoise.SetFractalOctaves(temperatureNoiseOctaves);
            temperatureNoise.SetDomainWarpAmp(temperatureNoiseWarp);
        }

        //how much rainfall in this area?
        static void ConfigureHumidityNoise()
        {
            humidityNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            humidityNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            humidityNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            humidityNoise.SetFrequency(humidityNoiseFreq);
            humidityNoise.SetFractalOctaves(humidityNoiseOctaves);
            humidityNoise.SetDomainWarpAmp(humidityNoiseWarp);
        }

        //how much plants are in this area?
        static void ConfigureVegetationNoise()
        {
            vegetationNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            vegetationNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            vegetationNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            vegetationNoise.SetFrequency(vegetationNoiseFreq);
            vegetationNoise.SetFractalOctaves(vegetationNoiseOctaves);
            vegetationNoise.SetDomainWarpAmp(vegetationNoiseWarp);
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
            string formattedReg = reg.ToString("F3");
            string formattedEro = ero.ToString("F3");
            string formattedRiv = riv.ToString("F3");

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
    }

    //represents a section of noise in the world
    public readonly struct NoiseRegion
    {
        //height offset = base height of terrain
        public readonly float heightOffset;

        //amplification = how much the terrain can vary from base height
        public readonly float amplification;

        //what region of the world you are in
        public readonly Biome biome;

        //dflt constructor
        public NoiseRegion(float heightOffset, float amplification, Biome biome) : this()
        {
            this.heightOffset = heightOffset;
            this.amplification = amplification;
            this.biome = biome;
        }
    }
}
