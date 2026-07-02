using OurCraft.Blocks;
using OurCraft.Utility;
using System.Runtime.CompilerServices;
using OurCraft.Terrain_Generation.Noise;
using OurCraft.Terrain_Generation.Registries;

namespace OurCraft.Terrain_Generation
{
    //creates the terrain of the world + where structures should start
    public static class OverworldGenerator
    {
        //terrain height values
        public const int SEA_LEVEL = WorldGenConstants.DEFAULT_SEA_LEVEL;
        public const int MIN_HEIGHT = WorldGenConstants.DEFAULT_MIN_HEIGHT;
        public const int MAX_HEIGHT = WorldGenConstants.DEFAULT_MAX_HEIGHT;
        public const float MAX_AMP = 128;

        static BlockState worldBlock;
        static BlockState emptyBlock;

        public static BlockState WorldBlock => worldBlock;
        public static BlockState EmptyBlock => emptyBlock;

        //global blocks of the world gen
        public static void SetGlobalBlocks()
        {
            worldBlock = BlockRegistry.GetDefaultBlockState("Stone");
            emptyBlock = BlockRegistry.GetDefaultBlockState("Air");
        }

        //determines low fidelity shape of terrain
        public static NoiseRegion GetTerrainRegion(int x, int z)
        {
            //get terrain shaping noises
            float continentalness = NoiseRouter.GetRegionalNoise(x, z);
            float erosion = NoiseRouter.GetErosionNoise(x, z);
            float river = NoiseRouter.GetRiverNoise(x, z);
            float weirdness = NoiseRouter.GetWeirdnessNoise(x, z);
            float fracture = NoiseRouter.GetFractureNoise(x, z);
            float caveSize = NoiseRouter.GetCaveSizeNoise(x, z);

            //evaluate splines for terrain offset
            float conOffset = SplineRegistry.regionSpline.Evaluate(continentalness);
            float eroOffset = SplineRegistry.erosionSpline.Evaluate(erosion);
            float rivOffset = SplineRegistry.riverSpline.Evaluate(river);
            float rivFactor = SplineRegistry.riverFactorSpline.Evaluate(erosion);

            //evaluate splines for terrain intensity
            float amplification =
            SplineRegistry.weirdnessSpline.Evaluate(weirdness) +         //small bumps
            SplineRegistry.fractureSpline.Evaluate(fracture) +           //rare fantasy terrain
            SplineRegistry.erosionAmplificationSpline.Evaluate(erosion); //mountain roughness bias
            float caveSizeFactor = SplineRegistry.caveSizeSpline.Evaluate(caveSize);

            //clamp amplification
            float finalAmp = Math.Min(amplification, MAX_AMP);
            int maxDepth = GetMaxDepth(amplification);

            //find biome
            float temp = NoiseRouter.GetTemperatureNoise(x, z);
            float humid = NoiseRouter.GetHumidityNoise(x, z);
            float veg = NoiseRouter.GetVegetationNoise(x, z);

            float ft = SplineRegistry.temperatureSpline.Evaluate(temp);
            float fh = SplineRegistry.humiditySpline.Evaluate(humid);
            float fv = SplineRegistry.vegetationSpline.Evaluate(veg);

            //combine noise outputs
            float offset = conOffset + eroOffset + (rivOffset * rivFactor);
            return new NoiseRegion(offset, finalAmp, GetBiome((int)ft, (int)fh, (int)fv), maxDepth, caveSizeFactor);
        }

        //indexes into the biome table with the current temp, humidity, and vegetation
        public static Biome GetBiome(int temp, int humid, int veg)
        {
            return BiomeRegistry.GetBiome(temp, humid, veg);
        }

        //get biome from xz
        public static Biome GetBiome(int x, int z)
        {
            float temp = NoiseRouter.GetTemperatureNoise(x, z);
            float humid = NoiseRouter.GetHumidityNoise(x, z);
            float veg = NoiseRouter.GetVegetationNoise(x, z);

            float ft = SplineRegistry.temperatureSpline.Evaluate(temp);
            float fh = SplineRegistry.humiditySpline.Evaluate(humid);
            float fv = SplineRegistry.vegetationSpline.Evaluate(veg);

            return BiomeRegistry.GetBiome((int)ft, (int)fh, (int)fv);
        }

        //checks if a biome has a deposit or not
        public static bool ContainsDeposit(Biome biome, Deposit deposit)
        {
            foreach(var dep in biome.deposits)
            {
                if (dep == deposit) return true;              
            }
            return false;
        }

        //creates the detailed shape of terrain by adding 3d detail to the raw heightmap
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDensity(int x, int y, int z, float heightOffset, float amplification)
        {
            float rawDensity = heightOffset - y;
            float rawNoise = NoiseRouter.GetDetailNoise(x, y, z);

            float density = rawDensity + rawNoise * amplification;

            if (density > 1f) return 1f;
            if (density < -1f) return -1f;

            return density;
        }

        //calculates base cave density for world
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetCaveDensity(int x, int y, int z, float caveSize)
        {
            float rawNoise = NoiseRouter.GetCaveNoise(x, y, z);
            float density = rawNoise * caveSize;
            return density;
        }

        //finds the surface block of the world based on biome and height
        public static BlockState GetSurfaceBlock(Biome biome, int height)
        {
            if (height < biome.OceanHeight) return biome.OceanSurfaceBlock;
            else if (height < biome.ShoreHeight) return biome.ShoreSurfaceBlock;
            else if (height < biome.PeakHeight) return biome.SurfaceBlock;
            else return biome.PeakSurfaceBlock;
        }

        //finds the subsurface block based on biome and height
        public static BlockState GetSubSurfaceBlock(Biome biome, int height)
        {
            if (height < biome.OceanHeight) return biome.OceanSubSurfaceBlock;
            else if (height < biome.ShoreHeight) return biome.ShoreSubSurfaceBlock;
            else if (height < biome.PeakHeight) return biome.SubSurfaceBlock;
            else return biome.PeakSubSurfaceBlock;
        }

        //get max depth and height of 3d noise changes based on amp factor
        //for optimization purposes
        public static int GetMaxDepth(float amp)
        {
            if (amp < 7.5f) return 10;
            else if (amp < 50.5f) return 60;
            else return 150;
        }
    }
}