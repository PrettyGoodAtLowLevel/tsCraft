using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;

namespace OurCraft.Terrain_Generation.SurfaceFeatures.SurfaceFeatureImplementations
{
    //coniforous tree surface feature
    public class Tree : SurfaceFeature
    {
        public BlockState LogBlock { get; set; }
        public BlockState LeavesBlock { get; set; }

        readonly int maxHeight = 11;

        //checks if the log and edge of leaves fit
        public override bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            //check log + leaves height vertically
            for (int i = 0; i < maxHeight; i++)
            {
                int lx = startPos.X;
                int ly = startPos.Y + i;
                int lz = startPos.Z;

                if (!Chunk.PosValid(lx, ly, lz)) return false;

                BlockState above = chunk.GetBlockUnsafe(lx, ly, lz);
                if (above != Block.AIR) return false;
            }

            int wx = startPos.X;
            int wy = startPos.Y + 5;
            int wz = startPos.Z;

            if (!Chunk.PosValid(wx + 2, wy, wz) || !Chunk.PosValid(wx - 2, wy, wz) 
            || !Chunk.PosValid(wx, wy, wz + 2) || !Chunk.PosValid(wx, wy, wz - 2))
                return false;

            BlockState check1 = chunk.GetBlockUnsafe(wx + 2, wy, wz);
            BlockState check2 = chunk.GetBlockUnsafe(wx - 2, wy, wz); 
            BlockState check3 = chunk.GetBlockUnsafe(wx, wy, wz + 2); 
            BlockState check4 = chunk.GetBlockUnsafe(wx, wy, wz - 2);

            if (check1 != Block.AIR || check2 != Block.AIR ||
            check3 != Block.AIR || check4 != Block.AIR)
                return false;

            return true;
        }

        //place a tree
        public override void PlaceFeature(Vector3i startPos, Chunk chunk)
        {
            int count = 5 + NoiseRouter.GetVariation(startPos.X + chunk.ChunkPos.X * Chunk.CHUNK_WIDTH, startPos.Y, startPos.Z + chunk.ChunkPos.Z * Chunk.CHUNK_WIDTH, 5, NoiseRouter.seed, 3);

            //place log
            int top = 0;
            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X;
                int wy = startPos.Y + i;
                int wz = startPos.Z;
                top++;
                //place the log
                chunk.SetBlockUnsafe(wx, wy, wz, LogBlock.With(BlockLog.AXIS, Axis.Y));
            }

            //place first leaves
            int radius = 2;
            int firstLayerY = top - 2;
            PlaceSquare(startPos, radius, firstLayerY, chunk);

            //place second leaves, no corner
            int secondLayerY = top - 1;
            PlaceRing(startPos, radius, secondLayerY, chunk);

            //place smaller square of leaves
            int thirdLayerY = top;
            PlaceSquare(startPos, radius - 1, thirdLayerY, chunk);

            //place smaller leaves, no corner
            int fourthLayerY = top + 1;
            PlaceRing(startPos, radius - 1, fourthLayerY, chunk);
        }

        //place a ring of leaves with a set position and radius
        public void PlaceRing(Vector3i startPos, int radius, int offsetY, Chunk chunk)
        {
            for (int x = startPos.X - radius; x <= startPos.X + radius; x++)
            {
                for (int z = startPos.Z - radius; z <= startPos.Z + radius; z++)
                {
                    int dx = x - startPos.X;
                    int dz = z - startPos.Z;

                    //skip corners only (dx and dz both at extreme ends)
                    if (Math.Abs(dx) == radius && Math.Abs(dz) == radius)
                        continue;

                    if (chunk.GetBlockUnsafe(x, startPos.Y + offsetY, z) != LogBlock)
                    {
                        chunk.SetBlockUnsafe(x, startPos.Y + offsetY, z, LeavesBlock);
                    }
                }
            }
        }

        //place a sqaure shape of leaves with a set position and radius
        public void PlaceSquare(Vector3i startPos, int radius, int offsetY, Chunk chunk)
        {
            for (int x = startPos.X - radius; x <= startPos.X + radius; x++)
            {
                for (int z = startPos.Z - radius; z <= startPos.Z + radius; z++)
                {
                    if (chunk.GetBlockUnsafe(x, startPos.Y + offsetY, z) != LogBlock)
                    {
                        chunk.SetBlockUnsafe(x, startPos.Y + offsetY, z, LeavesBlock);
                    }
                }
            }
        }
    }
}