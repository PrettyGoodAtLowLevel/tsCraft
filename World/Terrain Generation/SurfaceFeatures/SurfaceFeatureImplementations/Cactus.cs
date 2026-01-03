using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.World.Terrain_Generation.SurfaceFeatures.SurfaceFeatureImplementations
{
    public class Cactus : SurfaceFeature
    {
        public BlockState CactusBlock { get; set; }

        //the placing itself is dynamic to the chunk
        public override bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            var below = chunk.GetBlockUnsafe(startPos.X, startPos.Y - 1, startPos.Z);

            if (below != PlaceOn && below != AltPlaceOn) return false;

            for (int i = 0; i < 6; i++)
            {
                int wx = startPos.X;
                int wy = startPos.Y + i;
                int wz = startPos.Z;

                //stop if outside of chunk
                if (!Chunk.PosValid(wx, wy, wz))
                    return false;

                //get current and below blocks
                var current = chunk.GetBlockUnsafe(wx, wy, wz);
                

                //stop placing if space is not valid
                if (current != Block.AIR)
                    return false;
            }

            return true;
        }

        //place a random facing log procedurally across the world
        public override void PlaceFeature(Vector3i startPos, Chunk chunk)
        {
            int count = 2 + NoiseRouter.GetVariation(startPos.X + chunk.ChunkPos.X * Chunk.CHUNK_WIDTH, startPos.Y, startPos.Z + chunk.ChunkPos.Z * Chunk.CHUNK_WIDTH, 5, NoiseRouter.seed, 3);

            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X;
                int wy = startPos.Y + i;
                int wz = startPos.Z;

                chunk.SetBlockUnsafe(wx, wy, wz, CactusBlock);
            }
        }
    }
}
