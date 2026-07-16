using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Utility;
using OurCraft.World.WorldData;

namespace OurCraft.World.WorldGeneration.Mesh_Building
{
    //contains helpers for dealing with getting data across chunks and dealing with chunk boundaries
    public static class ChunkNeighborHelpers
    {
        public const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;
        public const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS;

        //get all neighbor blocks in a safe fashion
        public static NeighborBlocks GetNeighborsSafe(SubChunk sub, Vector3i pos, ChunkSectionNeighbors nc)
        {
            NeighborBlocks nb = new();
            int x = pos.X;
            int y = pos.Y;
            int z = pos.Z;

            //helper to get neighbor block safely
            BlockState N(int ox, int oy, int oz) => GetNeighborBlockSafe(sub, x, y, z, ox, oy, oz, nc);

            //top face + top corners
            nb.top = N(0, +1, 0);
            nb.bottom = N(0, -1, 0);
            nb.front = N(0, 0, +1);
            nb.back = N(0, 0, -1);
            nb.right = N(1, 0, 0);
            nb.left = N(-1, 0, 0);
            return nb;
        }

        //helper to fetch neighbor types safely
        static BlockState GetNeighborBlockSafe(SubChunk sub, int x, int y, int z, int offsetX, int offsetY, int offsetZ,
        ChunkSectionNeighbors nc)
        {
            const int cs = CHUNK_WIDTH - 1;

            int nx = x + offsetX;
            int ny = (sub.ChunkYPos * SUBCHUNK_SIZE) + y + offsetY; //global Y
            int nz = z + offsetZ;

            //out of world bounds (vertical)
            if (ny < 0 || ny >= Chunk.CHUNK_HEIGHT) return Block.AIR;
            if ((uint)nx < CHUNK_WIDTH && (uint)nz < CHUNK_WIDTH) return sub.parent.GetBlockStateUnsafe(nx, ny, nz);

            //start with this chunk as default
            Chunk? targetChunk = sub.parent;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= CHUNK_WIDTH;
            bool nzBack = nz < 0;
            bool nzFront = nz >= CHUNK_WIDTH;

            //if diagonal: prefer corner chunks
            if (nxLeft && nzBack) targetChunk = nc.c1; //back-left           
            else if (nxRight && nzBack) targetChunk = nc.c2; //back-right           
            else if (nxLeft && nzFront) targetChunk = nc.c3; //front-right        
            else if (nxRight && nzFront) targetChunk = nc.c4; //front-left           
            else
            {
                //non-diagonal: choose along X or Z
                if (nxLeft) targetChunk = nc.leftC;
                else if (nxRight) targetChunk = nc.rightC;

                if (nzBack) targetChunk = nc.backC;
                else if (nzFront) targetChunk = nc.frontC;
            }

            if (targetChunk == null || !targetChunk.HasAllBlocks()) return Block.AIR;

            //convert to local coordinates inside target chunk
            int localX = ((nx & cs) + CHUNK_WIDTH) & cs;
            int localZ = ((nz & cs) + CHUNK_WIDTH) & cs;

            return targetChunk.GetBlockStateUnsafe(localX, ny, localZ);
        }

        //gets the lighting value of a block safely
        public static ushort GetLightSafe(int x, int y, int z, int offsetX, int offsetY, int offsetZ, ChunkSectionNeighbors nc)
        {
            const ushort defaultLight = ((0 & 0xF) | ((0 & 0xF) << 4) | ((0 & 0xF) << 8) | ((15 & 0xF) << 12));
            const int cs = CHUNK_WIDTH - 1;

            int nx = x + offsetX;
            int ny = y + offsetY;
            int nz = z + offsetZ;

            //vertical world bounds or inside center chunk
            if (ny < 0 || ny >= Chunk.CHUNK_HEIGHT) return defaultLight;
            if ((uint)nx < CHUNK_WIDTH && (uint)nz < CHUNK_WIDTH) return nc.center.GetLight(nx, ny, nz);

            //start with center
            Chunk? targetChunk = nc.center;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= CHUNK_WIDTH;
            bool nzBack = nz < 0;
            bool nzFront = nz >= CHUNK_WIDTH;

            //diagonal neighbors
            if (nxLeft && nzBack) targetChunk = nc.c1;
            else if (nxRight && nzBack) targetChunk = nc.c2;
            else if (nxLeft && nzFront) targetChunk = nc.c3;
            else if (nxRight && nzFront) targetChunk = nc.c4;
            else
            {
                //straight neighbors
                if (nxLeft) targetChunk = nc.leftC;
                else if (nxRight) targetChunk = nc.rightC;

                if (nzBack) targetChunk = nc.backC;
                else if (nzFront) targetChunk = nc.frontC;
            }

            if (targetChunk == null || !targetChunk.HasAllBlocks()) return defaultLight;

            //wrap coordinates into neighbor chunk local space
            int localX = ((nx & cs) + CHUNK_WIDTH) & cs;
            int localZ = ((nz & cs) + CHUNK_WIDTH) & cs;

            return targetChunk.GetLight(localX, ny, localZ);
        }

        //helper to check if a neighboring block is solid safely
        public static bool IsNeighborSolidSafe(int x, int y, int z, int offsetX, int offsetY, int offsetZ, ChunkSectionNeighbors nc)
        {
            const int cs = CHUNK_WIDTH - 1;

            int nx = x + offsetX;
            int ny = y + offsetY;
            int nz = z + offsetZ;

            //vertical world bounds
            if (ny < 0 || ny >= Chunk.CHUNK_HEIGHT) return false;

            //inside center chunk
            if ((uint)nx < CHUNK_WIDTH && (uint)nz < CHUNK_WIDTH)
            {
                BlockState bstate = nc.center.GetBlockStateUnsafe(nx, ny, nz);
                return bstate.AOSolid;
            }

            //choose neighbor chunk
            Chunk? targetChunk = nc.center;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= CHUNK_WIDTH;

            bool nzBack = nz < 0;
            bool nzFront = nz >= CHUNK_WIDTH;

            //diagonal neighbors
            if (nxLeft && nzBack) targetChunk = nc.c1;
            else if (nxRight && nzBack) targetChunk = nc.c2;
            else if (nxLeft && nzFront) targetChunk = nc.c3;
            else if (nxRight && nzFront) targetChunk = nc.c4;
            else
            {
                //straight neighbors
                if (nxLeft) targetChunk = nc.leftC;
                else if (nxRight) targetChunk = nc.rightC;

                if (nzBack) targetChunk = nc.backC;
                else if (nzFront) targetChunk = nc.frontC;
            }

            if (targetChunk == null || !targetChunk.HasAllBlocks()) return false;

            //wrap coordinates into neighbor chunk local space
            int localX = ((nx & cs) + CHUNK_WIDTH) & cs;
            int localZ = ((nz & cs) + CHUNK_WIDTH) & cs;
            BlockState state = targetChunk.GetBlockStateUnsafe(localX, ny, localZ);
            return state.AOSolid;
        }
    }
}