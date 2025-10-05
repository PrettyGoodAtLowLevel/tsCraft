using OurCraft.Blocks;
using OurCraft.utility;
using System.Data;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

//simple terrain generator for a more beta 1.7.3 mc feel, not very complex
//needs:
//one block surface features, grass flowers (done)
//2.biome noise
//3.better terrain generator (like the cascades mod from minecraft)
//4.optmized terrain, less 3d noise calls when able
//5.multi-block surface features that still fit in a chunk
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
        public const int SEA_LEVEL = 126;
        public const int MIN_HEIGHT = 90;
        public const int MAX_HEIGHT = 320;
        public const int LAND = SEA_LEVEL + 1;
        public const int PEAK = SEA_LEVEL + 120;

        //random worlds
        private static readonly int offsetX;
        private static readonly int offsetZ;

        //3d noise maps
        //creates the real shape of the terrain
        private static readonly FastNoiseLite baseNoise; //overhangs

        //2d noise maps
        //determines what type of shape to try to generate with 3d noise

        //initialise all the noises
        static WorldGenerator()
        {
            //create seed and biomes
            BiomeData.InitBiomes();
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
            //hard clamp for solid and air regions
            if (y < 130)
                return 1f; //fully solid
            if (y > 130)
                return -1f; //fully air
            return 0;
        }

        //maps a noise value to a specific biome
        public static Biome GetBiome()
        {
            return BiomeData.plains;
        }

        //get the surface block based on biome and terrain height
        public static BlockState GetSurface(Biome biome, int globalY)
        {
            if (globalY < LAND) return new BlockState(biome.shoreSurface);
            else if (globalY < PEAK) return new BlockState(biome.landSurface);
            else return new BlockState(biome.peakSurface);
        }

        //get sub surface block based on biome and terrain height
        public static BlockState GetSubsurface(Biome biome, int globalY)
        {
            if (globalY < LAND) return new BlockState(biome.shoreSubsurface);
            else if (globalY < PEAK) return new BlockState(biome.landSubsurface);
            else return new BlockState(biome.peakSubsurface);
        }

        //get the deco block for this position
        public static BlockState GetDecoBlock(Biome biome, int yPos)
        {
            int rand = RandomNumberGenerator.GetInt32(2000);
            BlockState state = new BlockState(BlockIDs.AIR_BLOCK);
            if (rand > biome.decoBlock1Threshold) state = new BlockState(biome.decoBlock1);
            else if (rand > biome.decoBlock2Threshold) state = new BlockState(biome.decoBlock2);
            else if (rand > biome.decoBlock3Threshold) state = new BlockState(biome.decoBlock3);
            else if (rand > biome.decoBlock4Threshold) state = new BlockState(biome.decoBlock4);
                return state;
        }

        //debug stuff
        public static void DebugValues(int x, int z)
        {
        }
    }
}