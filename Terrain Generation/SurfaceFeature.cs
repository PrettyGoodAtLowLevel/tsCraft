using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Utility;
using OurCraft.World.WorldData;

namespace OurCraft.Terrain_Generation
{
    //things like trees, rocks, and small structures
    public abstract class SurfaceFeature
    {
        public const int MAX_SIZE = Chunk.CHUNK_WIDTH;

        public Vector3i localMin = Vector3i.Zero;
        public Vector3i localMax = Vector3i.Zero;
        public bool notCrossChunk = false; //only if surface feature is 1 by 1 on xz

        public SurfaceFeature() { }

        public virtual bool CanPlaceFeature(Vector3i startPos, Chunk target)
        {
            return true;
        }

        public virtual void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world){ }

        //sets blocks only in a target chunk
        public static void TrySetBlock(Vector3i worldPos, BlockState block, Chunk target, bool replaceBlock = true)
        {
            int minX = target.ChunkPos.X * Chunk.CHUNK_WIDTH;
            int minZ = target.ChunkPos.Z * Chunk.CHUNK_WIDTH;

            int lx = worldPos.X - minX;
            int lz = worldPos.Z - minZ;
            int ly = worldPos.Y;

            if (lx < 0 || lx >= Chunk.CHUNK_WIDTH) return;            //outside chunk on X
            if (lz < 0 || lz >= Chunk.CHUNK_WIDTH) return;            //outside chunk on Z
            if (ly < 0 || ly >= WorldConstants.CHUNK_HEIGHT) return;  //out of world
            if (!replaceBlock && target.GetBlockStateUnsafe(lx, ly, lz) != Block.AIR) return; //cant replace block

            target.SetBlockStateUnsafe(lx, ly, lz, block);
        }

        //checks if the bounding box of a surface feature intersects with a chunk
        public static bool IntersectsChunk(Chunk target, Vector3i structureStartPos, SurfaceFeature feature)
        {
            int chunkMinX = target.ChunkPos.X * Chunk.CHUNK_WIDTH;
            int chunkMinZ = target.ChunkPos.Z * Chunk.CHUNK_WIDTH;

            int chunkMaxX = chunkMinX + Chunk.CHUNK_WIDTH;
            int chunkMaxZ = chunkMinZ + Chunk.CHUNK_WIDTH;

            int featureMinX = structureStartPos.X + feature.localMin.X;
            int featureMinZ = structureStartPos.Z + feature.localMin.Z;

            int featureMaxX = structureStartPos.X + feature.localMax.X;
            int featureMaxZ = structureStartPos.Z + feature.localMax.Z;

            return (chunkMinX <= featureMaxX && chunkMaxX >= featureMinX) //x overlap
            && (chunkMinZ <= featureMaxZ && chunkMaxZ >= featureMinZ);    //z overlap
        }
    }

    //structure + chance to spawn
    public readonly struct BiomeSurfaceFeature
    {
        public readonly int chance = 0;
        public readonly SurfaceFeature feature;

        public BiomeSurfaceFeature(SurfaceFeature feature, int chance)
        {
            this.feature = feature;
            this.chance = chance;
        }
    }

    //start position of a feature + what feature to place
    public readonly struct PlacedFeature
    {
        public readonly Vector3i startPos;
        public readonly SurfaceFeature feature;

        public PlacedFeature(SurfaceFeature feature, Vector3i startPos)
        {
            this.feature = feature;
            this.startPos = startPos;
        }
    }
}