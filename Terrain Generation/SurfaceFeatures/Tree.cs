using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.World;
using OurCraft.Terrain_Generation.Noise;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //default oak tree shape from minecraft
    public class Tree : SurfaceFeature
    {
        public readonly BlockState placeOn = Block.AIR;
        public readonly BlockState logBlock = Block.AIR;
        public readonly BlockState leavesBlock = Block.AIR;

        const int TREE_WIDTH = 2;
        const int MAX_TREE_HEIGHT = 10;

        public Tree(BlockState placeOn, BlockState logBlock, BlockState leavesBlock)
        {
            this.placeOn = placeOn;
            this.logBlock = logBlock;
            this.leavesBlock = leavesBlock;

            this.localMin = new Vector3i(-TREE_WIDTH, 0, -TREE_WIDTH);
            this.localMax = new Vector3i(TREE_WIDTH, MAX_TREE_HEIGHT, TREE_WIDTH);
        }

        //checks if trunk can be placed
        public override bool CanPlaceFeature(Vector3i startPos, Chunk target)
        {
            int localX = VoxelMath.ModPow2(startPos.X, Chunk.CHUNK_WIDTH);
            int localZ = VoxelMath.ModPow2(startPos.Z, Chunk.CHUNK_WIDTH);

            if (!Chunk.PosValid(localX, startPos.Y + MAX_TREE_HEIGHT, localZ)) return false;

            for(int i = 0; i < startPos.Y + MAX_TREE_HEIGHT; i++)
            {
                BlockState state = target.GetBlockStateUnsafe(localX, startPos.Y + i, localZ);
                if (state != Block.AIR) return false;
            }
            return true;
        }

        //places trunk + leaves rings
        public override void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world)
        {
            int count = 5 + NoiseRouter.GetVariation(startPos.X, startPos.Y, startPos.Z, NoiseRouter.seed, salt:0, max:2);

            //place log
            int top = 0;
            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X;
                int wy = startPos.Y + i;
                int wz = startPos.Z;
                top++;
                //place the log
                TrySetBlock(new Vector3i(wx, wy, wz), logBlock.With(BlockLog.AXIS, Axis.Y), target);
            }

            //place first leaves
            int radius = 2;
            int firstLayerY = top - 2;
            PlaceSquare(startPos, radius, firstLayerY, target);

            //place second leaves, no corner
            int secondLayerY = top - 1;
            PlaceRing(startPos, radius, secondLayerY, target);

            //place smaller square of leaves
            int thirdLayerY = top;
            PlaceSquare(startPos, radius - 1, thirdLayerY, target, ignoreCenter:false);

            //place smaller leaves, no corner
            int fourthLayerY = top + 1;
            PlaceRing(startPos, radius - 1, fourthLayerY, target, ignoreCenter:false);
        }

        //place a ring of leaves with a set position and radius
        public void PlaceRing(Vector3i startPos, int radius, int offsetY, Chunk chunk, bool ignoreCenter = true)
        {
            for (int x = startPos.X - radius; x <= startPos.X + radius; x++)
            {
                for (int z = startPos.Z - radius; z <= startPos.Z + radius; z++)
                {
                    int dx = x - startPos.X;
                    int dz = z - startPos.Z;

                    //skip corners only (dx and dz both at extreme ends)
                    if (Math.Abs(dx) == radius && Math.Abs(dz) == radius || dx == 0 && dz == 0 && ignoreCenter)  continue;                 
                    TrySetBlock(new Vector3i(x, startPos.Y + offsetY, z), leavesBlock, chunk, replaceBlock: false);                   
                }
            }
        }

        //place a sqaure shape of leaves with a set position and radius, keep corners
        public void PlaceSquare(Vector3i startPos, int radius, int offsetY, Chunk chunk, bool ignoreCenter = true)
        {
            for (int x = startPos.X - radius; x <= startPos.X + radius; x++)
            {
                for (int z = startPos.Z - radius; z <= startPos.Z + radius; z++)
                {
                    int dx = x - startPos.X;
                    int dz = z - startPos.Z;
                    if (dx == 0 && dz == 0 && ignoreCenter) continue;
                    TrySetBlock(new Vector3i(x, startPos.Y + offsetY, z), leavesBlock, chunk, replaceBlock: false);                   
                }
            }
        }
    }
}