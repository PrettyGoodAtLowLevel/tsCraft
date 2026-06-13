using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Terrain_Generation;
using OurCraft.Utility;
using OurCraft.Terrain_Generation.Noise;

namespace OurCraft.World.ChunkGeneration.WorldGen
{
    //contains helpers for placing surface features in chunks
    public static class FeatureGenerator
    {
        const int CHUNK_HEIGHT = WorldConstants.CHUNK_HEIGHT;
        const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;

        //plants structure starts in a chunk
        public static void FeatureSeed(Chunk chunk, NoiseRegion[,] noiseRegions)
        {
            int ccx = CHUNK_WIDTH * chunk.ChunkPos.X;
            int ccz = CHUNK_WIDTH * chunk.ChunkPos.Z;
            int seed = NoiseRouter.seed;

            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                for (int x = 0; x < CHUNK_WIDTH; x++)
                {
                    Biome biome = noiseRegions[x, z].biome;

                    for (int y = CHUNK_HEIGHT - 1; y >= 0; y--)
                    {
                        BlockState current = chunk.GetBlockUnsafe(x, y, z);
                        BlockState above = y + 1 < CHUNK_HEIGHT ? chunk.GetBlockUnsafe(x, y + 1, z) : Block.AIR;

                        if (current != OverworldGenerator.GetSurfaceBlock(biome, y) || above != Block.AIR) continue;

                        Vector3i worldPos = new(x + ccx, y, z + ccz);
                        foreach (BiomeSurfaceFeature feature in biome.features)
                        {
                            int rand = NoiseRouter.GetStructureRandomness(worldPos.X, worldPos.Y, worldPos.Z, seed, feature.chance);

                            if (rand == 1 && feature.feature.CanPlaceFeature(worldPos + Vector3i.UnitY, chunk))
                            {
                                chunk.placedFeatures.Add(new PlacedFeature(feature.feature, worldPos + Vector3i.UnitY));
                                break;
                            }
                        }
                    }
                }
            }
        }

        //finds all structures that intersect with the target chunk, grid of chunks
        public static List<PlacedFeature> CollectFeatures(Chunk target, ChunkManager world, int radius)
        {
            var allFeatures = new List<PlacedFeature>();

            //add intersected surface features
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    Chunk? neighbor = world.GetChunk(new ChunkCoord(target.ChunkPos.X + dx, target.ChunkPos.Z + dz));
                    if (neighbor == null || !neighbor.HasBlocks()) continue;

                    foreach (var sf in neighbor.placedFeatures)
                    {
                        if (sf.feature.notCrossChunk)
                        {
                            if (neighbor.ChunkPos == target.ChunkPos) allFeatures.Add(sf);
                            continue;
                        }
                        if (SurfaceFeature.IntersectsChunk(target, sf.startPos, sf.feature))
                        {
                            allFeatures.Add(sf);
                        }
                    }
                }
            }

            return allFeatures;
        }

        //determinsitically sorts structures (closest to origin first, then furthest last)
        public static void SortFeatures(List<PlacedFeature> features)
        {
            features.Sort((a, b) =>
            {
                int distA = a.startPos.X * a.startPos.X + a.startPos.Y * a.startPos.Y + a.startPos.Z * a.startPos.Z;
                int distB = b.startPos.X * b.startPos.X + b.startPos.Y * b.startPos.Y + b.startPos.Z * b.startPos.Z;
                int cd = distA.CompareTo(distB);
                if (cd != 0) return cd;
                int cx = a.startPos.X.CompareTo(b.startPos.X);
                if (cx != 0) return cx;
                int cz = a.startPos.Z.CompareTo(b.startPos.Z);
                if (cz != 0) return cz;
                return a.startPos.Y.CompareTo(b.startPos.Y);
            });
        }
    }
}
