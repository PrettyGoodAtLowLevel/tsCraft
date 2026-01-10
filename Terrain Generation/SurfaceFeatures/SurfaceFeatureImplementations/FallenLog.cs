using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;

namespace OurCraft.Terrain_Generation.SurfaceFeatures.SurfaceFeatureImplementations
{
    public class FallenLog : SurfaceFeature
    {
        public BlockState LogBlock { get; set; }

        public FallenLog()
        {
            Name = "Fallen Log";
            PlaceOn = BlockRegistry.GetDefaultBlockState("Grass Block");
        }

        //the placing itself is dynamic to the chunk
        public override bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            return true;
        }

        //place a random facing log procedurally across the world
        public override void PlaceFeature(Vector3i startPos, Chunk chunk)
        {
            int axis = NoiseRouter.GetVariation(startPos.X + chunk.ChunkPos.X * Chunk.CHUNK_WIDTH, startPos.Y, startPos.Z + chunk.ChunkPos.Z * Chunk.CHUNK_WIDTH, 12, NoiseRouter.seed, 4);
            int count = 3 + NoiseRouter.GetVariation(startPos.X + chunk.ChunkPos.X * Chunk.CHUNK_WIDTH, startPos.Y, startPos.Z + chunk.ChunkPos.Z * Chunk.CHUNK_WIDTH, 5, NoiseRouter.seed, 3);         
            switch(axis)
            {
                case 0:
                    PlaceXLog(startPos, chunk, count);
                    break;
                case 1:
                    PlaceZLog(startPos, chunk, count);
                    break;
                case 2:
                    PlaceXLogReverse(startPos, chunk, count);
                    break;
                case 3:
                    PlaceZLogReverse(startPos, chunk, count);
                    break;
                default:
                    PlaceXLog(startPos, chunk, count);
                    break;
            }              
        }

        //procedurally place a log in a world on the x axis
        public void PlaceXLog(Vector3i startPos, Chunk chunk, int count)
        {
            
            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X + i;
                int wy = startPos.Y;
                int wz = startPos.Z;

                //stop if outside of chunk
                if (!Chunk.PosValid(wx, wy, wz))
                    break;

                //get current and below blocks
                var current = chunk.GetBlockUnsafe(wx, wy, wz);
                var below = chunk.GetBlockUnsafe(wx, wy - 1, wz);

                //stop placing if space is not valid
                if (current != Block.AIR || below == Block.AIR)
                    break;

                //place the log
                BlockState state = LogBlock.With(BlockLog.AXIS, Axis.X);
                chunk.SetBlockUnsafe(wx, wy, wz, state);
            }
        }

        //procedurally place a log in a world on the z axis
        public void PlaceZLog(Vector3i startPos, Chunk chunk, int count)
        {
           
            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X;
                int wy = startPos.Y;
                int wz = startPos.Z + i;

                //stop if outside of chunk
                if (!Chunk.PosValid(wx, wy, wz))
                    break;

                //get current and below blocks
                var current = chunk.GetBlockUnsafe(wx, wy, wz);
                var below = chunk.GetBlockUnsafe(wx, wy - 1, wz);

                //stop placing if space is not valid
                if (current != Block.AIR || below == Block.AIR)
                    break;

                //place log
                BlockState state = LogBlock.With(BlockLog.AXIS, Axis.Z);
                chunk.SetBlockUnsafe(wx, wy, wz, state);
            }
        }

        //procedurally place a log in a world on the x axis in the negative direction
        public void PlaceXLogReverse(Vector3i startPos, Chunk chunk, int count)
        {

            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X - i;
                int wy = startPos.Y;
                int wz = startPos.Z;

                //stop if outside of chunk
                if (!Chunk.PosValid(wx, wy, wz))
                    break;

                //get current and below blocks
                var current = chunk.GetBlockUnsafe(wx, wy, wz);
                var below = chunk.GetBlockUnsafe(wx, wy - 1, wz);

                //stop placing if space is not valid
                if (current != Block.AIR || below == Block.AIR)
                    break;

                //place log
                BlockState state = LogBlock.With(BlockLog.AXIS, Axis.X);
                chunk.SetBlockUnsafe(wx, wy, wz, state);
            }
        }

        //procedurally place a log in a world on the z axis in the negative direction
        public void PlaceZLogReverse(Vector3i startPos, Chunk chunk, int count)
        {

            for (int i = 0; i < count; i++)
            {
                int wx = startPos.X;
                int wy = startPos.Y;
                int wz = startPos.Z - i;

                //stop if outside of chunk
                if (!Chunk.PosValid(wx, wy, wz))
                    break;

                //get current and below blocks
                var current = chunk.GetBlockUnsafe(wx, wy, wz);
                var below = chunk.GetBlockUnsafe(wx, wy - 1, wz);

                //stop placing if space is not valid
                if (current != Block.AIR || below == Block.AIR)
                    break;

                //place log
                BlockState state = LogBlock.With(BlockLog.AXIS, Axis.Z);
                chunk.SetBlockUnsafe(wx, wy, wz, state);
            }
        }
    }
}
