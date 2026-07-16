using OurCraft.Blocks;
using OurCraft.Utility;
using OurCraft.World.WorldData;

namespace OurCraft.World.WorldGeneration.Terrain_Gen
{
    //contains helpers for initalizing chunk voxel and lighting data
    public static class ChunkInitializer
    {
        const int HEIGHT_IN_SUBCHUNKS = WorldConstants.CHUNK_HEIGHT_IN_SUBCHUNKS;
        const int WIDTH_IN_SUBCHUNKS = WorldConstants.CHUNK_WIDTH_IN_SUBCHUNKS;
        const int CHUNK_HEIGHT = WorldConstants.CHUNK_HEIGHT;
        const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;

        //initializes all subchunks in a chunk
        public static void InitSubChunks(Chunk chunk)
        {
            chunk.DirtySubChunks = new bool[HEIGHT_IN_SUBCHUNKS];
            chunk.SubChunks = new SubChunk[HEIGHT_IN_SUBCHUNKS];

            for (int y = 0; y < HEIGHT_IN_SUBCHUNKS; y++)
            {
                chunk.SubChunks[y] = new SubChunk(chunk, y);
            }
        }

        //scans every single y layer from top to bottom
        public static int GetChunkMaxSolidY(Chunk chunk)
        {        
            int chunkHeight = HEIGHT_IN_SUBCHUNKS * SubChunk.SUBCHUNK_SIZE;

            for (int y = chunkHeight - 1; y >= 0; y--)
            {
                int sy = y / SubChunk.SUBCHUNK_SIZE;
                int ly = y % SubChunk.SUBCHUNK_SIZE;

                SubChunk sc = chunk.SubChunks[sy];

                for (int x = 0; x < SubChunk.SUBCHUNK_SIZE; x++)
                    for (int z = 0; z < SubChunk.SUBCHUNK_SIZE; z++)
                        if (sc.GetBlockState(x, ly, z) != Block.AIR) return y + 1;
            }

            return 0;
        }

        //fills all blocks above maxy with light 15, and everything else with 0
        public static void InitLightMap(Chunk chunk, int maxY)
        {
            chunk.lightMap = new ushort[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

            const int sky0 = 0;
            const ushort sky15 = 0 & 0xF | (0 & 0xF) << 4 | (0 & 0xF) << 8 | (15 & 0xF) << 12;

            for (int x = 0; x < CHUNK_WIDTH; x++)
            for (int z = 0; z < CHUNK_WIDTH; z++)
            for (int y = 0; y < CHUNK_HEIGHT; y++)
            {
                if (y < maxY) chunk.lightMap[x, y, z] = sky0;
                //if above all solid blocks, set to max sky instead
                else chunk.lightMap[x, y, z] = sky15;
            }             
        }
    }
}
