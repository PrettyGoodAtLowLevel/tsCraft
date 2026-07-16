using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Utility;
using OurCraft.Terrain_Generation.Noise;
using OurCraft.World.WorldData;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //simple cactus block seen in mc
    public class Cactus : SurfaceFeature
    {
        public BlockState placeOn;
        public BlockState cactusBlock;

        public Cactus(BlockState placeOn, BlockState cactusBlock)
        {
            this.placeOn = placeOn;
            this.cactusBlock = cactusBlock;
            notCrossChunk = true; //cactus are 1x1 on xz, so cant cross chunks
        }

        //check if root has enough room on y axis
        public override bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            int localX = VoxelMath.ModPow2(startPos.X, Chunk.CHUNK_WIDTH);
            int localZ = VoxelMath.ModPow2(startPos.Z, Chunk.CHUNK_WIDTH);

            var below = chunk.GetBlockStateUnsafe(localX, startPos.Y - 1, localZ);
            if (below != placeOn) return false;

            for (int i = 0; i < 6; i++)
            {
                int wy = startPos.Y + i;

                //stop if outside of chunk
                if (!Chunk.PosValid(localX, wy, localZ)) return false;

                //get current and below blocks stop placing if space is not valid
                var current = chunk.GetBlockStateUnsafe(localX, wy, localZ);
                if (current != Block.AIR) return false;
            }

            return true;
        }

        //place pillar
        public override void PlaceFeature(Vector3i startPos, Chunk chunk, ChunkManager world)
        {
            int count = 2 + NoiseRouter.GetVariation(startPos.X, startPos.Y, startPos.Z, NoiseRouter.seed, salt:3, max:3);

            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X;
                int wy = startPos.Y + i;
                int wz = startPos.Z;

                TrySetBlock(new Vector3i(wx, wy, wz), cactusBlock, chunk);
            }
        }
    }
}