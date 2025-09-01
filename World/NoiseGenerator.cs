using OpenTK.Graphics.ES11;
using OurCraft.utility;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

//not really done anything here yet, next update is big improvements to this
namespace OurCraft.World
{
    //represents a region of the world
    public readonly struct NoiseRegion
    {
        public readonly float baseHeight, erosion, crazy;

        public NoiseRegion(float baseHeight, float erosion, float crazy) : this()
        {
            this.baseHeight = baseHeight;
            this.erosion = erosion;
            this.crazy = crazy;
        }
    }

    //contains all the perlin noise values and tools for world generation
    public static class NoiseGenerator
    {
        //terrain height values
        public const int SEA_LEVEL = 128;
        public const int MIN_HEIGHT = 99;
        public const int MAX_HEIGHT = 320;

        //random worlds
        private static readonly int seed;
        private static readonly int offsetX;
        private static readonly int offsetZ;

        //noise maps
        private static readonly FastNoiseLite continentalNoise;

        //---noisemap settings----
        private static readonly float conNoiseFreq = 0.00075f;

        //initialise all the noises and height layers
        static NoiseGenerator()
        {
            seed = RandomNumberGenerator.GetInt32(int.MaxValue);
            Console.WriteLine("world seed: " + seed);

            //random offsets based on seed
            offsetX = RandomNumberGenerator.GetInt32(-10000, 10000);
            offsetZ = RandomNumberGenerator.GetInt32(-10000, 10000);

            //------2d base noisemaps------

            //base overall height of terrain
            continentalNoise = new FastNoiseLite(seed);
            continentalNoise.SetFrequency(conNoiseFreq); //large continents and oceans
        }

        //determines base terrain height
        private static readonly List<SplinePoint> conSpline = 
        [
            new SplinePoint(-1.0f, 110),
            new SplinePoint(-0.5f, 128),
            new SplinePoint(0.3f, 132),
            new SplinePoint(1.0f, 140f),
        ];

        //determines base regions of terrain
        public static NoiseRegion GetTerrainRegion(int x, int z)
        {
            float rawCon = continentalNoise.GetNoise(x + offsetX, z + offsetZ);
            float baseHeight = VoxelMath.EvaluateSpline(conSpline, rawCon, true);

            return new NoiseRegion(baseHeight, 0, 0);
        }

        //creates actual 3d shape of terrain based on 2d terrain region height map
        public static float GetDensity(int x, int y, int z, NoiseRegion control)
        {
            float baseDensity = control.baseHeight - y;

            return Math.Clamp(baseDensity, -1, 1);
        }
    }
}