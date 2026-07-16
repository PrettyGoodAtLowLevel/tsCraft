using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Utility;
using OurCraft.World.WorldData;

namespace OurCraft.World.WorldUpdates
{
    public static class RandomBlockTick
    {
        //random tick chunk
        public static void TickChunk(ChunkManager world, Chunk chunk)
        {
            foreach(var sub in chunk.SubChunks)
            {
                TickSubChunk(sub, world);
            }
        }

        //random tick sub chunk
        private static void TickSubChunk(SubChunk subChunk, ChunkManager world)
        {
            if (subChunk.hasRandomTick == false) return;

            Chunk chunk = subChunk.parent;
            Random rng = Random.Shared;

            for (int i = 0; i < WorldConstants.DEFAULT_RANDOM_TICK; i++)
            {
                int localX = rng.Next(SubChunk.SUBCHUNK_SIZE);
                int localY = rng.Next(SubChunk.SUBCHUNK_SIZE);
                int localZ = rng.Next(SubChunk.SUBCHUNK_SIZE);

                BlockState state = subChunk.GetBlockState(localX, localY, localZ);

                //optimization
                if (!state.GetBlock.RequiresRandomTicks) continue;

                //convert to world coordinates
                int wx = (chunk.ChunkPos.X * WorldConstants.CHUNK_WIDTH) + localX;
                int wy = (subChunk.ChunkYPos * WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS) + localY;
                int wz = (chunk.ChunkPos.Z * WorldConstants.CHUNK_WIDTH) + localZ;

                state.RandomTick(new Vector3i(wx, wy, wz), world);
            }
        }
    }
}
