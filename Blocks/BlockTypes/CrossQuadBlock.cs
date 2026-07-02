using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Blocks.Meshing;
using OurCraft.World;

namespace OurCraft.Blocks.Block_Implementations
{
    //x shaped blocks like flowers and tall grass
    public class CrossQuadBlock : Block
    {
        public CrossQuadBlock(string name, BlockShape shape): base(name, shape)
        {
            IsRenderSolid = false;
        }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, ChunkManager world)
        {
            world.SetBlockState(globalPos + hitNormal, DefaultState);
        }

        //light can pass through non full blocks
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        //light can pass through this block
        public override int GetSkyLightAttenuation(BlockState state)
        {
            return 0;
        }
        
        public override void RandomTick(Vector3i pos, ChunkManager world, BlockState state)
        {
            int rng = Random.Shared.Next(100);
            if (rng != 1) return;

            BlockState log = BlockRegistry.GetDefaultBlockState("Oak Log");
            BlockState leaves = BlockRegistry.GetDefaultBlockState("Oak Leaves");
            for(int y = pos.Y; y < pos.Y + 5; y++)
            {               
                world.SetBlock(new Vector3d(pos.X, y, pos.Z), log.With(BlockLog.AXIS, Axis.Y));
            }

            //place first leaves
            int radius = 2;
            int firstLayerY = 5 - 2;
            PlaceSquare(pos, radius, firstLayerY, leaves, world);

            //place second leaves, no corner
            int secondLayerY = 5 - 1;
            PlaceRing(pos, radius, secondLayerY, leaves, world);

            //place smaller square of leaves
            int thirdLayerY = 5;
            PlaceSquare(pos, radius - 1, thirdLayerY, leaves, world, ignoreCenter: false);

            //place smaller leaves, no corner
            int fourthLayerY = 5 + 1;
            PlaceRing(pos, radius - 1, fourthLayerY, leaves, world, ignoreCenter: false);
        }

        //place a ring of leaves with a set position and radius
        public void PlaceRing(Vector3i startPos, int radius, int offsetY, BlockState leavesBlock, ChunkManager world, bool ignoreCenter = true)
        {
            for (int x = startPos.X - radius; x <= startPos.X + radius; x++)
            {
                for (int z = startPos.Z - radius; z <= startPos.Z + radius; z++)
                {
                    int dx = x - startPos.X;
                    int dz = z - startPos.Z;

                    //skip corners only (dx and dz both at extreme ends)
                    if (Math.Abs(dx) == radius && Math.Abs(dz) == radius || dx == 0 && dz == 0 && ignoreCenter) continue;
                    world.SetBlock(new Vector3i(x, startPos.Y + offsetY, z), leavesBlock);
                }
            }
        }

        //place a sqaure shape of leaves with a set position and radius, keep corners
        public void PlaceSquare(Vector3i startPos, int radius, int offsetY, BlockState leavesBlock, ChunkManager world, bool ignoreCenter = true)
        {
            for (int x = startPos.X - radius; x <= startPos.X + radius; x++)
            {
                for (int z = startPos.Z - radius; z <= startPos.Z + radius; z++)
                {
                    int dx = x - startPos.X;
                    int dz = z - startPos.Z;
                    if (dx == 0 && dz == 0 && ignoreCenter) continue;
                    world.SetBlock(new Vector3i(x, startPos.Y + offsetY, z), leavesBlock);
                }
            }
        }

        public override bool RequiresRandomTicks => true;
    }
}
