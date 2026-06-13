using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.World;
using OurCraft.Terrain_Generation.Noise;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //large 2x2 trunk, jungle tree from minecraft
    public class JungleTree : Tree
    {
        const int JUNGLE_WIDTH = 5;
        const int MAX_JUNGLE_HEIGHT = 26;

        public JungleTree(BlockState placeOn, BlockState logBlock, BlockState leavesBlock) : base(placeOn, logBlock, leavesBlock)
        {
            this.localMin = new Vector3i(-JUNGLE_WIDTH, 0, -JUNGLE_WIDTH);
            this.localMax = new Vector3i(JUNGLE_WIDTH, MAX_JUNGLE_HEIGHT + 3, JUNGLE_WIDTH);
        }

        //check if trunk has room
        public override bool CanPlaceFeature(Vector3i startPos, Chunk target)
        {
            int[] offsets = [0, 1];
            foreach (int ox in offsets)
            {
                foreach (int oz in offsets)
                {
                    int localX = VoxelMath.ModPow2(startPos.X + ox, Chunk.CHUNK_WIDTH);
                    int localZ = VoxelMath.ModPow2(startPos.Z + oz, Chunk.CHUNK_WIDTH);
                    if (!Chunk.PosValid(localX, startPos.Y + MAX_JUNGLE_HEIGHT, localZ)) return false;

                    for (int i = 0; i < MAX_JUNGLE_HEIGHT; i++)
                    {
                        if (target.GetBlockUnsafe(localX, startPos.Y + i, localZ) != Block.AIR) return false;
                    }
                }
            }
                
            return true;
        }

        //place 2x2 trunk, then leaves
        public override void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world)
        {
            int height = 12 + NoiseRouter.GetVariation(startPos.X, startPos.Y, startPos.Z, NoiseRouter.seed, salt: 30, max: 8);

            //build chunk
            int[] offsets = { 0, 1 };
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

            int top = height - 1;
            (int yOff, int radius, bool keepCorners)[] layers =
            [
                ( 2, 2, false),
                ( 1, 3, false),
                ( 0, 3, true),
                (-1, 4 , false),
                (-2, 4 , true),
            ];

            foreach (var (yOff, radius, keepCorners) in layers)
            {
                int layerY = top + yOff;
                if (layerY < 0) continue;
                PlaceCenteredLayer(startPos, radius, layerY, target, keepCorners);
            }
        }

        //place 2x2 centered leaves layer
        private void PlaceCenteredLayer(Vector3i startPos, int radius, int offsetY, Chunk chunk, bool keepCorners = false)
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
                    TrySetBlock(new Vector3i(x, startPos.Y + offsetY, z), leavesBlock, chunk, replaceBlock: false);
                }
            }     
        }
    }
}