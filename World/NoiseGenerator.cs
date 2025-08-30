using OpenTK.Graphics.ES11;
using OurCraft.utility;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace OurCraft.World
{
    //regional values of a part in the world
    //con represents land vs water region
    //ero represents flatlands vs mountains
    //crazy represnents weird 3d noise terrain vs normal region terrain
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
        private static readonly FastNoiseLite erosionNoise;
        private static readonly FastNoiseLite crazyNoise;       

        private static readonly FastNoiseLite threeDNoise;
        private static readonly FastNoiseLite threeDDetailnoise;

        //---noisemap settings----
        private static readonly float conNoiseFreq = 0.00075f;
        private static readonly float eroNoiseFreq = 0.0005f;
        private static readonly float mountainNoiseFreq = 0.0005f;
        private static readonly float mountainNoiseWarpAmt = 300;

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

            //flattens terrain overall
            erosionNoise = new FastNoiseLite(seed + 1);
            erosionNoise.SetFrequency(eroNoiseFreq); //adds hills and lakes

            //adds large mountains
            crazyNoise = new FastNoiseLite(seed + 2);
            crazyNoise.SetFrequency(mountainNoiseFreq);
            crazyNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            crazyNoise.SetDomainWarpAmp(mountainNoiseWarpAmt);

            //--------3d detail noisemaps------
            threeDNoise = new FastNoiseLite(seed + 3);
            threeDNoise.SetFrequency(0.01f);
            threeDNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            threeDNoise.SetDomainWarpAmp(2000);

            threeDDetailnoise = new FastNoiseLite(seed + 4);
            threeDDetailnoise.SetFrequency(0.005f);
            threeDDetailnoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            threeDDetailnoise.SetFractalOctaves(4);
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

            return Math.Clamp(baseDensity, -1, 1); //*135f: multiplier here for more amplififed terrain and "3d"
        }
    }
}
