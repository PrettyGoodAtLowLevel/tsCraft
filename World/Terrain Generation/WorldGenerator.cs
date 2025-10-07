using OurCraft.Blocks;
using System.Security.Cryptography;

//doesnt do anything (*for now*)
namespace OurCraft.World.Terrain_Generation
{
    //represents a region of the world
    public readonly struct NoiseRegion
    {
        public readonly float heightOffset, squashingFactor;

        public NoiseRegion(float heightOffset, float squashingFactor) : this()
        {
            this.heightOffset = heightOffset;
            this.squashingFactor = squashingFactor;
        }
    }

    //contains all the perlin noise values and tools for world generation
    public static class WorldGenerator
    {
        //terrain height values
        public const int SEA_LEVEL = 100;
        public const int MIN_HEIGHT = 90;
        public const int MAX_HEIGHT = 320;
        public const int LAND = SEA_LEVEL + 1;
        public const int PEAK = SEA_LEVEL + 120;
        private static readonly FastNoiseLite baseNoise;

        //initialise all the noises
        static WorldGenerator()
        {
            int seed = RandomNumberGenerator.GetInt32(int.MaxValue);
            Random rand = new(seed);
            int baseSeed = rand.Next(int.MaxValue);       

            baseNoise = new FastNoiseLite(baseSeed);
            baseNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            baseNoise.SetFractalOctaves(4);
            baseNoise.SetFrequency(0.002f);
            baseNoise.SetDomainWarpAmp(200);       
        }

        //determines base continents and rivers
        public static NoiseRegion GetTerrainRegion(int x, int z)
        {
            return new NoiseRegion(0,0);
        }

        //creates actual 3d shape of terrain based on 2d terrain region height map
        public static float GetDensity(int x, int y, int z, NoiseRegion control)
        {
            return y > 130 ? -1 : 1;
        }
    }
}