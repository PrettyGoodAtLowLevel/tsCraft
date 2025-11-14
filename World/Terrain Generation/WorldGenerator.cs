
using System;

namespace OurCraft.World.Terrain_Generation
{
    //does all of the math for the world generation
    public static class WorldGenerator
    {
        //terrain height values
        public const int SEA_LEVEL = 126;
        public const int MIN_HEIGHT = 90;
        public const int MAX_HEIGHT = 320;
        public static bool FlatWorld = true;

        //determines low fidelity shape of terrain
        public static NoiseRegion GetTerrainRegion(int x, int z)
        {
            if (FlatWorld) return new NoiseRegion(130, 0, BiomeData.EmptyBiome, 0);

            //get terrain shaping noises
            float continentalness = NoiseRouter.GetRegionalNoise(x, z);
            float erosion = NoiseRouter.GetErosionNoise(x, z);
            float river = NoiseRouter.GetRiverNoise(x, z);
            float weirdness = NoiseRouter.GetWeirdnessNoise(x, z);
            float fracture = NoiseRouter.GetFractureNoise(x, z);

            //get the biome map noises
            float rawTemperature = NoiseRouter.GetTemperatureNoise(x, z);
            float rawHumididity = NoiseRouter.GetHumidityNoise(x, z);
            float rawVegetation = NoiseRouter.GetVegetationNoise(x, z);

            //evaluate splines for terrain shape
            float conOffset = TerrainSplines.regionSpline.Evaluate(continentalness);
            float eroOffset = TerrainSplines.erosionSpline.Evaluate(erosion);
            float rivOffset = TerrainSplines.riverSpline.Evaluate(river);
            float rivFactor = TerrainSplines.riverFactorSpline.Evaluate(erosion);
            float amplification =
            TerrainSplines.weirdnessSpline.Evaluate(weirdness) + TerrainSplines.fractureSpline.Evaluate(fracture);

            //clamp raw biome noise to proper indexes
            int finalTemp = (int)TerrainSplines.temperatureSpline.Evaluate(rawTemperature);
            int finalHumid = (int)TerrainSplines.humiditySpline.Evaluate(rawHumididity);
            int finalVeg = (int)TerrainSplines.vegetationSpline.Evaluate(rawVegetation);

            int maxDepth = GetMaxDepth(amplification);
            //combine noise outputs
            float offset = conOffset + eroOffset + (rivOffset * rivFactor);      
            return new NoiseRegion(offset, amplification, GetBiome(finalTemp, finalHumid, finalVeg), maxDepth);
        }

        //indexes into the biome table with the current temp, humidity, and vegetation
        public static Biome GetBiome(int temp, int humid, int veg)
        {
            return BiomeData.FindBiome(temp,humid,veg);
        }

        //creates the detailed shape of terrain by adding 3d detail to the raw heightmap
        public static float GetDensity(int x, int y, int z, NoiseRegion control)
        {
            //hard clamp values, also boosts generation speed
            if (y > MAX_HEIGHT) return -1;
            else if (y < MIN_HEIGHT) return 1;

            //distance from surface (positive below surface, negative above)
            float surfaceDist = control.heightOffset - y;

            //early-out optimization beyond influence range
            if (surfaceDist < -control.maxDepth) return -1f; //far above terrain
            if (surfaceDist > control.maxDepth) return 1f;   //deep underground

            float rawDensity = (control.heightOffset - y) / 1.0f;
            float rawNoise = NoiseRouter.GetDetailNoise(x, y, z);
            float finalDensity = rawDensity + (rawNoise * control.amplification);
            return Math.Clamp(finalDensity, -1, 1);
        }

        //finds the surface block of the world based on biome and height
        public static ushort GetSurfaceBlock(Biome biome, int height)
        {
            if (height < biome.OceanHeight) return biome.OceanSurfaceBlock;
            else if (height < biome.ShoreHeight) return biome.ShoreSurfaceBlock;
            else if (height < biome.PeakHeight) return biome.SurfaceBlock;
            else return biome.PeakSurfaceBlock;
        }

        //finds the subsurface block based on biome and height
        public static ushort GetSubSurfaceBlock(Biome biome, int height)
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
            if (amp < 7.5f)
                return 10;
            else if (amp < 50.5f)
                return 60;
            else
                return 150;
        }
    }
}