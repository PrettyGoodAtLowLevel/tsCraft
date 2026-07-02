using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.World;
using OurCraft.Terrain_Generation.Noise;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //minecrafts spruce tree example
    public class SpruceTree : Tree
    {
        const int SPRUCE_WIDTH = 3;
        const int MAX_SPRUCE_HEIGHT = 16;

        public SpruceTree(BlockState placeOn, BlockState logBlock, BlockState leavesBlock) : base(placeOn, logBlock, leavesBlock)
        {
            this.localMin = new Vector3i(-SPRUCE_WIDTH, 0, -SPRUCE_WIDTH);
            this.localMax = new Vector3i(SPRUCE_WIDTH, MAX_SPRUCE_HEIGHT + 1, SPRUCE_WIDTH);
        }

        //check if trunk can be placed
        public override bool CanPlaceFeature(Vector3i startPos, Chunk target)
        {
            int localX = VoxelMath.ModPow2(startPos.X, Chunk.CHUNK_WIDTH);
            int localZ = VoxelMath.ModPow2(startPos.Z, Chunk.CHUNK_WIDTH);
            if (!Chunk.PosValid(localX, startPos.Y + MAX_SPRUCE_HEIGHT, localZ)) return false;
            for (int i = 0; i < MAX_SPRUCE_HEIGHT; i++)
            {
                if (target.GetBlockStateUnsafe(localX, startPos.Y + i, localZ) != Block.AIR) return false;
            }
            return true;
        }

        //place trunk + leaves
        public override void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world)
        {
            //height varies: 8–11 blocks
            int height = 8 + NoiseRouter.GetVariation(startPos.X, startPos.Y, startPos.Z, NoiseRouter.seed, salt: 10, max: 3);

            for (int i = 0; i < height; i++) TrySetBlock(new Vector3i(startPos.X, startPos.Y + i, startPos.Z), logBlock.With(BlockLog.AXIS, Axis.Y), target);

            //place first leaves
            int radius = 3;

            int firstY = height - 7;
            PlaceRing(startPos, radius, firstY, target);

            int secondY = firstY + 1;
            PlaceRing(startPos, radius - 1, secondY, target);

            int thirdY = firstY + 2;
            PlaceRing(startPos, radius - 2, thirdY, target);

            int fourthY = firstY + 3;
            PlaceRing(startPos, radius - 1, fourthY, target);

            int fifthY = firstY + 4;
            PlaceRing(startPos, radius - 2, fifthY, target);

            int finalY = firstY + 6;
            PlaceRing(startPos, radius - 2, finalY, target, false);

            TrySetBlock(new Vector3i(startPos.X, startPos.Y + finalY, startPos.Z), leavesBlock, target);
        }
    }
}