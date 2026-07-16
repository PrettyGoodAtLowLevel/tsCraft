using OurCraft.Terrain_Generation;
using OurCraft.Terrain_Generation.Noise;
using OurCraft.Utility;
using OurCraft.World.WorldData;
using OurCraft.World.WorldGeneration.Terrain_Gen;

namespace OurCraft.World.WorldGeneration
{
    //helps generate block data of chunks and subchunks using the world generator
    public static class ChunkGenerator
    {
        const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;
        const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS;

        public const int INTERP_STEP_CAVE = 4; //sample every 2 blocks → 9 points per axis (0,4,8,12,16)
        public const int INTERP_STEP_DENSITY = 2;
        public const int INTERP_GRID_CAVE = SUBCHUNK_SIZE / INTERP_STEP_CAVE + 1; //9
        public const int INTERP_GRID_DENSITY = SUBCHUNK_SIZE / INTERP_STEP_DENSITY + 1; //9

        //fills in all subchunks with block state data
        public static void CreateTerrain(Chunk chunk)
        {
            if (chunk.GetState() != ChunkState.Initialized) return;
            ChunkInitializer.InitSubChunks(chunk);

            //create noise regions
            NoiseRegion[,] noiseRegions = new NoiseRegion[CHUNK_WIDTH, CHUNK_WIDTH];

            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                for (int x = 0; x < CHUNK_WIDTH; x++)
                {
                    int globalX = chunk.ChunkPos.X * CHUNK_WIDTH + x;
                    int globalZ = chunk.ChunkPos.Z * CHUNK_WIDTH + z;
                    NoiseRegion noiseRegion = OverworldGenerator.GetTerrainRegion(globalX, globalZ);
                    noiseRegions[x, z] = noiseRegion;
                }
            }

            //use noise regions & create blocks
            foreach (var subChunk in chunk.SubChunks) DensityGenerator.CreateDensityMap(subChunk, noiseRegions);
            foreach (var subChunk in chunk.SubChunks) BiomePainter.SurfacePaint(subChunk, noiseRegions);
            foreach (var subChunk in chunk.SubChunks) CaveGenerator.CarveCaves(subChunk, noiseRegions);

            DepositGenerator.GenerateDeposits(chunk);
            FeatureGenerator.FeatureSeed(chunk, noiseRegions);
            //StructureGenerator.StructureStart(chunk, noiseRegions);

            chunk.SetState(ChunkState.Terrain_Set);
        }

        //sorts all intersected structures within a chunk, and places the intersected blocks
        public static void PlaceFeatures(Chunk target, ChunkManager world, int radius = 1)
        {
            var featuresToPlace = FeatureGenerator.CollectFeatures(target, world, radius);
            FeatureGenerator.SortFeatures(featuresToPlace);

            //place all features
            foreach (var pf in featuresToPlace) pf.feature.PlaceFeature(pf.startPos, target, world);

            //init lightmap
            target.MaxSolidY = ChunkInitializer.GetChunkMaxSolidY(target) + 2;
            ChunkInitializer.InitLightMap(target, target.MaxSolidY);

            target.SetState(ChunkState.Structures_Placed);
        }
    }
}