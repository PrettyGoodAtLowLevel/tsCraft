using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.Terrain_Generation.Noise;
using OurCraft.World.WorldData;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //fallen trunk of a tree
    public class FallenLog : SurfaceFeature
    {
        public BlockState logBlock;

        const int MAX_LENGTH = 6;
        const int HEIGHT = 1;

        public FallenLog(BlockState logBlock)
        {
            this.logBlock = logBlock;
            this.localMin = new Vector3i(-MAX_LENGTH / 2, HEIGHT, -MAX_LENGTH / 2);
            this.localMax = new Vector3i(MAX_LENGTH / 2, HEIGHT, MAX_LENGTH / 2);
        }

        //logs adapt to environment
        public override bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            return true;
        }

        //dynamically place log
        public override void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world)
        {
            int axis = NoiseRouter.GetVariation(startPos.X, startPos.Y, startPos.Z, NoiseRouter.seed, salt:4, max:2);
            int count = 2 + NoiseRouter.GetVariation(startPos.X, startPos.Y, startPos.Z, NoiseRouter.seed, salt: 4, max: 3);

            switch(axis)
            {
                case 0:
                    PlaceXLog(startPos, target, count);
                    break;
                case 1:
                    PlaceZLog(startPos, target, count);
                    break;
                default:
                    break;
            }
        }

        //place log on x axis
        public void PlaceXLog(Vector3i startPos, Chunk target, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X + i;
                int wy = startPos.Y;
                int wz = startPos.Z;

                int lx = VoxelMath.ModPow2(wx, Chunk.CHUNK_WIDTH);
                int lz = VoxelMath.ModPow2(wz, Chunk.CHUNK_WIDTH);

                //stop if outside of chunk
                if (!Chunk.PosValid(lx, wy, lz)) break;

                //get current and below blocks
                var current = target.GetBlockStateUnsafe(lx, wy, lz);
                var below = target.GetBlockStateUnsafe(lx, wy - 1, lz);

                //stop placing if space is not valid
                if (current != Block.AIR || below == Block.AIR) break;

                //place the log
                BlockState state = logBlock.With(BlockLog.AXIS, Axis.X);
                TrySetBlock(new Vector3i(wx, wy, wz), state, target);
            }
        }

        //place log on z axis
        public void PlaceZLog(Vector3i startPos, Chunk target, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X;
                int wy = startPos.Y;
                int wz = startPos.Z + i;

                int lx = VoxelMath.ModPow2(wx, Chunk.CHUNK_WIDTH);
                int lz = VoxelMath.ModPow2(wz, Chunk.CHUNK_WIDTH);

                //stop if outside of chunk
                if (!Chunk.PosValid(lx, wy, lz)) break;

                //get current and below blocks
                var current = target.GetBlockStateUnsafe(lx, wy, lz);
                var below = target.GetBlockStateUnsafe(lx, wy - 1, lz);

                //stop placing if space is not valid
                if (current != Block.AIR || below == Block.AIR) break;

                //place the log
                BlockState state = logBlock.With(BlockLog.AXIS, Axis.Z);
                TrySetBlock(new Vector3i(wx, wy, wz), state, target);
            }
        }
    }
}
