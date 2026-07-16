using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.World.WorldData;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //simply an oak tree, but short
    public class Bush : Tree
    {
        const int BUSH_WIDTH = 2;
        const int MAX_BUSH_HEIGHT = 5;

        public Bush(BlockState placeOn, BlockState logBlock, BlockState leavesBlock) : base(placeOn, logBlock, leavesBlock)
        {
            this.localMin = new Vector3i(-BUSH_WIDTH, 0, -BUSH_WIDTH);
            this.localMax = new Vector3i(BUSH_WIDTH, MAX_BUSH_HEIGHT, BUSH_WIDTH);
        }

        //check if trunk can fit
        public override bool CanPlaceFeature(Vector3i startPos, Chunk target)
        {
            int localX = VoxelMath.ModPow2(startPos.X, Chunk.CHUNK_WIDTH);
            int localZ = VoxelMath.ModPow2(startPos.Z, Chunk.CHUNK_WIDTH);

            if (!Chunk.PosValid(localX, startPos.Y + MAX_BUSH_HEIGHT, localZ)) return false;

            for (int i = 0; i < startPos.Y + MAX_BUSH_HEIGHT; i++)
            {
                BlockState state = target.GetBlockStateUnsafe(localX, startPos.Y + i, localZ);
                if (state != Block.AIR) return false;
            }
            return true;
        }

        //place oak tree, but with constant short height
        public override void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world)
        {
            //height the same
            int height = 2;

            for (int i = 0; i < height; i++) TrySetBlock(new Vector3i(startPos.X, startPos.Y + i, startPos.Z), logBlock.With(BlockLog.AXIS, Axis.Y), target);

            //place first leaves
            int radius = 2;
            int firstLayerY = height - 2;
            PlaceSquare(startPos, radius, firstLayerY, target);

            //place second leaves, no corner
            int secondLayerY = height - 1;
            PlaceRing(startPos, radius, secondLayerY, target);

            //place smaller square of leaves
            int thirdLayerY = height;
            PlaceSquare(startPos, radius - 1, thirdLayerY, target, ignoreCenter: false);

            //place smaller leaves, no corner
            int fourthLayerY = height + 1;
            PlaceRing(startPos, radius - 1, fourthLayerY, target, ignoreCenter: false);
        }
    }
}
