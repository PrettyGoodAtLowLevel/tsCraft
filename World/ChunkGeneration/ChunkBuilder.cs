using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Graphics;
using OurCraft.Utility;
using System.Diagnostics;

namespace OurCraft.World.ChunkGeneration
{
    //helps build mesh data of chunk/subchunks
    public static class ChunkBuilder
    {
        const int HEIGHT_IN_SUBCHUNKS = WorldConstants.CHUNK_HEIGHT_IN_SUBCHUNKS;
        const int WIDTH_IN_SUBCHUNKS = WorldConstants.CHUNK_WIDTH_IN_SUBCHUNKS;
        const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;
        const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS;

        //creates the cpu side mesh of each subchunk in a chunk
        public static void CreateChunkMesh(Chunk chunk, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
        Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            if (!chunk.IsLit()) return;
            chunk.generating = true;

            ChunkSectionNeighbors neighbors = new(chunk)
            {
                leftC = leftC, rightC = rightC,
                frontC = frontC, backC = backC,

                c1 = c1, c2 = c2,
                c3 = c3, c4 = c4,
            };

            foreach (var subChunk in chunk.SubChunks)
            {
                subChunk.ClearMesh();
            }

            foreach (var subChunk in chunk.SubChunks)
            {
                MeshSubChunk(subChunk, neighbors);
            }

            foreach (var subChunk in chunk.SubChunks)
            {
                subChunk.SolidGeo.vertices.TrimExcess();
                subChunk.TransparentGeo.vertices.TrimExcess();
            }

            if (chunk.GetState() == ChunkState.Lit) chunk.SetState(ChunkState.Mesh_Built);
            chunk.generating = false;       
        }

        //clears the mesh of a chunk if mesh data exists
        public static void ClearChunkMesh(Chunk chunk)
        {
            if (!chunk.IsLit()) return;
            foreach (var subChunk in chunk.SubChunks)
            {
                subChunk.ClearMesh();
            }

            chunk.batchedMesh.Delete();
            chunk.transparentMesh.Delete();
        }

        //remesh all effected subchunks and reupload mesh to openGL
        public static void RemeshChunk(Chunk chunk, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            MarkSubChunksDirty(chunk);

            ChunkSectionNeighbors neighbors = new(chunk)
            {
                leftC = leftC, rightC = rightC,
                frontC = frontC, backC = backC,

                c1 = c1, c2 = c2,
                c3 = c3, c4 = c4,
            };

            //remesh dirty subchunks only once and reupload mesh
            for (int x = 0; x < WIDTH_IN_SUBCHUNKS; x++)
            {
                for (int y = 0; y < HEIGHT_IN_SUBCHUNKS; y++)
                {
                    for (int z = 0; z < WIDTH_IN_SUBCHUNKS; z++)
                    {
                        if (!chunk.DirtySubChunks[x, y, z]) continue;
                        SubChunk sub = chunk.SubChunks[x, y, z];
                        RemeshSubChunk(sub, neighbors);
                        chunk.DirtySubChunks[x, y, z] = false;
                    }
                }
            }

            chunk.changes.Clear();
            ChunkRenderer.GLUploadChunk(chunk);
        }

        //based on the amount of changes in the chunk, find all the subchunks that were effected
        static void MarkSubChunksDirty(Chunk chunk)
        {
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            for (int i = 0; i < chunk.changes.Count; i++)
            {
                var p = chunk.changes[i];

                int subX = p.X / SubChunk.SUBCHUNK_SIZE;
                int subY = p.Y / SubChunk.SUBCHUNK_SIZE;
                int subZ = p.Z / SubChunk.SUBCHUNK_SIZE;

                int localX = p.X & sb; int localY = p.Y & sb; int localZ = p.Z & sb;

                //mark owning subchunk
                if ((uint)subX < WIDTH_IN_SUBCHUNKS && (uint)subY < HEIGHT_IN_SUBCHUNKS && (uint)subZ < WIDTH_IN_SUBCHUNKS) chunk.DirtySubChunks[subX, subY, subZ] = true;

                //X neighbors
                if (localX == sb && subX + 1 < WIDTH_IN_SUBCHUNKS) chunk.DirtySubChunks[subX + 1, subY, subZ] = true;
                if (localX == 0 && subX > 0) chunk.DirtySubChunks[subX - 1, subY, subZ] = true;

                //Y neighbors
                if (localY == sb && subY + 1 < HEIGHT_IN_SUBCHUNKS) chunk.DirtySubChunks[subX, subY + 1, subZ] = true;
                if (localY == 0 && subY > 0) chunk.DirtySubChunks[subX, subY - 1, subZ] = true;

                //Z neighbors
                if (localZ == sb && subZ + 1 < WIDTH_IN_SUBCHUNKS) chunk.DirtySubChunks[subX, subY, subZ + 1] = true;
                if (localZ == 0 && subZ > 0) chunk.DirtySubChunks[subX, subY, subZ - 1] = true;
            }
        }

        //subchunk meshing
        //creates cpu side subchunk mesh data
        static void MeshSubChunk(SubChunk sub, ChunkSectionNeighbors nc)
        {
            if (sub.IsAllAir()) return; //dont create mesh if chunk is completely air

            int cxp = sub.ChunkXPos * SUBCHUNK_SIZE;
            int cyp = sub.ChunkYPos * SUBCHUNK_SIZE;
            int czp = sub.ChunkZPos * SUBCHUNK_SIZE;

            for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--)
            {
                for (int x = SUBCHUNK_SIZE - 1; x >= 0; x--)
                {
                    for (int z = SUBCHUNK_SIZE - 1; z >= 0; z--)
                    {
                        AddMeshDataToChunk(sub, pos:new Vector3i(x, y, z), meshPos:new Vector3(cxp + x, cyp + y, czp + z), nc);
                    }
                }
            }
        }

        //clears subchunk cpu side mesh and recreates it
        static void RemeshSubChunk(SubChunk sub, ChunkSectionNeighbors nc)
        {
            sub.ClearMesh();
            MeshSubChunk(sub, nc);
        }

        //tries to add face data to a chunk mesh based on a bitmask of the adjacent blocks also samples lighting values for blocks exposed to light
        static void AddMeshDataToChunk(SubChunk sub, Vector3i pos, Vector3 meshPos, ChunkSectionNeighbors nc)
        {
            BlockState state = sub.GetBlockState(pos.X, pos.Y, pos.Z);
            if (state == Block.AIR || state.BlockEntityRenderType == BlockEntityRenderType.SeparateRenderer) return;
            Block block = BlockRegistry.GetBlock(state.BlockID);

            //get neighbor blocks
            NeighborBlocks nb = GetNeighborsSafe(sub, pos, nc);
            nb.thisState = state;

            //if surrounding blocks are all full solid cubes, and the current block is also full solid, then skip meshing entirely
            if (nb.top.BlockShape.IsFullOpaqueBlock && nb.bottom.BlockShape.IsFullOpaqueBlock
            && nb.front.BlockShape.IsFullOpaqueBlock && nb.back.BlockShape.IsFullOpaqueBlock &&
            nb.left.BlockShape.IsFullOpaqueBlock && nb.right.BlockShape.IsFullOpaqueBlock
            && block.blockShape.IsFullOpaqueBlock) return;

            ChunkMeshData meshRef = block.blockShape.IsTranslucent ? sub.TransparentGeo : sub.SolidGeo;
            block.blockShape.AddBlockMesh(meshPos, nb, meshRef, nc);
        }

        //get all neighbor blocks in a safe fashion
        static NeighborBlocks GetNeighborsSafe(SubChunk sub, Vector3i pos, ChunkSectionNeighbors nc)
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

            int nx = (sub.ChunkXPos * SUBCHUNK_SIZE) + x + offsetX;
            int ny = (sub.ChunkYPos * SUBCHUNK_SIZE) + y + offsetY; //global Y
            int nz = (sub.ChunkZPos * SUBCHUNK_SIZE) + z + offsetZ;

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