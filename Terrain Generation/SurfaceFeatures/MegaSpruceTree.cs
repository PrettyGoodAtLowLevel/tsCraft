using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.Terrain_Generation.Noise;
using OurCraft.World.WorldData;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //large spruce tree like in minecraft taiga forest
    public class MegaSpruceTree : Tree
    {
        const int MEGA_SPRUCE_WIDTH = 6;
        const int MAX_SPRUCE_HEIGHT = 28;

        public MegaSpruceTree(BlockState placeOn, BlockState logBlock, BlockState leavesBlock) : base(placeOn, logBlock, leavesBlock)
        {
            this.localMin = new Vector3i(-MEGA_SPRUCE_WIDTH, 0, -MEGA_SPRUCE_WIDTH);
            this.localMax = new Vector3i(MEGA_SPRUCE_WIDTH, MAX_SPRUCE_HEIGHT + 3, MEGA_SPRUCE_WIDTH);
        }

        //check if trunk can be placed
        public override bool CanPlaceFeature(Vector3i startPos, Chunk target)
        {
            int[] offsets = [0, 1];
            foreach (int ox in offsets)
            {
                foreach (int oz in offsets)
                {
                    int localX = VoxelMath.ModPow2(startPos.X + ox, Chunk.CHUNK_WIDTH);
                    int localZ = VoxelMath.ModPow2(startPos.Z + oz, Chunk.CHUNK_WIDTH);
                    if (!Chunk.PosValid(localX, startPos.Y + MAX_SPRUCE_HEIGHT, localZ)) return false;
                    for (int i = 0; i < MAX_SPRUCE_HEIGHT; i++)
                    {
                        if (target.GetBlockStateUnsafe(localX, startPos.Y + i, localZ) != Block.AIR) return false;
                    }
                }
            }
                
            return true;
        }

        //build trunk + leaves
        public override void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world)
        {
            int height = 15 + NoiseRouter.GetVariation(startPos.X, startPos.Y, startPos.Z, NoiseRouter.seed, salt: 30, max: 10);

            // 2x2 trunk
            int[] offsets = [0, 1];
            for (int i = 0; i < height; i++)
            {
                foreach (int ox in offsets)
                {
                    foreach (int oz in offsets)
                    {
                        TrySetBlock(new Vector3i(startPos.X + ox, startPos.Y + i, startPos.Z + oz), logBlock.With(BlockLog.AXIS, Axis.Y), target);
                    }
                }
            }
               
            int firstY = height - 9;

            //tier -1
            PlaceCenteredLayer(startPos, 3, firstY - 4, target);
            PlaceCenteredLayer(startPos, 3, firstY - 3, target, keepCorners: true);
            //tier 0 - widest teir
            PlaceCenteredLayer(startPos, 4, firstY - 2, target, keepCorners: true);
            PlaceCenteredLayer(startPos, 5, firstY - 1, target);
            //tier 1
            PlaceCenteredLayer(startPos, 4, firstY, target);
            PlaceCenteredLayer(startPos, 3, firstY + 1, target);
            //tier 2
            PlaceCenteredLayer(startPos, 3, firstY + 2, target);
            PlaceCenteredLayer(startPos, 2, firstY + 3, target);
            //tier 3
            PlaceCenteredLayer(startPos, 2, firstY + 4, target);
            PlaceCenteredLayer(startPos, 1, firstY + 5, target);
            //top cap (gap of 1, then tip — mirrors the taiga pattern)
            PlaceCenteredLayer(startPos, 2, firstY + 7, target, keepCorners: false, replaceBlock: true);
            PlaceCenteredLayer(startPos, 1, firstY + 8, target, keepCorners: true, replaceBlock: true);
        }

        //place 2x2 centered ring of leaves
        private void PlaceCenteredLayer(Vector3i startPos, int radius, int offsetY, Chunk chunk, bool keepCorners = false, bool replaceBlock = false)
        {
            int xMin = startPos.X - (radius - 1);
            int xMax = startPos.X + radius;
            int zMin = startPos.Z - (radius - 1);
            int zMax = startPos.Z + radius;

            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    bool isCorner = (x == xMin || x == xMax) && (z == zMin || z == zMax);
                    if (!keepCorners && isCorner) continue;
                    TrySetBlock(new Vector3i(x, startPos.Y + offsetY, z), leavesBlock, chunk, replaceBlock);
                }
            }     
        }
    }
}
