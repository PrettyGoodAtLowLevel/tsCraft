using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.Graphics.Voxel_Lighting;

namespace OurCraft.World.Helpers
{
    //helps build mesh data of chunk/subchunks
    public static class ChunkBuilder
    {
        const int HEIGHT_IN_SUBCHUNKS = 24;
        const int WIDTH_IN_SUBCHUNKS = 2;
        const int CHUNK_HEIGHT = SUBCHUNK_SIZE * HEIGHT_IN_SUBCHUNKS;
        const int CHUNK_WIDTH = SUBCHUNK_SIZE * WIDTH_IN_SUBCHUNKS;
        const int SUBCHUNK_SIZE = 16;

        //creates the cpu side mesh of each subchunk in a chunk
        public static void CreateChunkMesh(Chunk chunk, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
        Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            if (!chunk.HasVoxelData()) return;
            chunk.meshing = true;
            foreach (var subChunk in chunk.SubChunks)
            {
                subChunk.ClearMesh();
            }

            foreach (var subChunk in chunk.SubChunks)
            {
                MeshSubChunk(subChunk, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            }

            foreach (var subChunk in chunk.SubChunks)
            {
                subChunk.SolidGeo.vertices.TrimExcess();
                subChunk.TransparentGeo.vertices.TrimExcess();
            }

            if (chunk.GetState() == ChunkState.VoxelOnly) chunk.SetState(ChunkState.Meshed);
            chunk.meshing = false;       
        }

        //remesh all effected subchunks and reupload mesh to openGL
        public static void RemeshChunk(Chunk chunk, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            MarkSubChunksDirty(chunk);

            //remesh dirty subchunks only once and reupload mesh
            for (int x = 0; x < WIDTH_IN_SUBCHUNKS; x++)
            {
                for (int y = 0; y < HEIGHT_IN_SUBCHUNKS; y++)
                {
                    for (int z = 0; z < WIDTH_IN_SUBCHUNKS; z++)
                    {
                        if (!chunk.DirtySubChunks[x, y, z]) continue;
                        SubChunk sub = chunk.SubChunks[x, y, z];
                        RemeshSubChunk(sub, leftC, rightC, frontC, backC, c1, c2, c3, c4);
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
        static void MeshSubChunk(SubChunk sub, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
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
                        AddMeshDataToChunk(sub, pos:new Vector3i(x, y, z), meshPos:new Vector3(cxp + x, cyp + y, czp + z),
                        leftC, rightC, frontC, backC, c1, c2, c3, c4);
                    }
                }
            }
        }

        //clears subchunk cpu side mesh and recreates it
        static void RemeshSubChunk(SubChunk sub, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            sub.ClearMesh();
            MeshSubChunk(sub, leftC, rightC, frontC, backC, c1, c2, c3, c4);
        }

        //tries to add face data to a chunk mesh based on a bitmask of the adjacent blocks also samples lighting values for blocks exposed to light
        static void AddMeshDataToChunk(SubChunk sub, Vector3i pos, Vector3 meshPos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            BlockState state = sub.GetBlockState(pos.X, pos.Y, pos.Z);
            if (state == Block.AIR) return;
            Block block = BlockData.GetBlock(state.BlockID);

            //get neighbor blocks
            NeighborBlocks nb = GetNeighborsSafe(sub, pos, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            nb.thisState = state;

            //if surrounding blocks are all full solid cubes, and the current block is also full solid, then skip meshing entirely
            if (nb.top.BlockShape.IsFullOpaqueBlock && nb.bottom.BlockShape.IsFullOpaqueBlock
            && nb.front.BlockShape.IsFullOpaqueBlock && nb.back.BlockShape.IsFullOpaqueBlock &&
            nb.left.BlockShape.IsFullOpaqueBlock && nb.right.BlockShape.IsFullOpaqueBlock
            && block.blockShape.IsFullOpaqueBlock) return;

            LightingData lightData = GetLightData(sub,pos, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            ChunkMeshData meshRef = block.blockShape.IsTranslucent ? sub.TransparentGeo : sub.SolidGeo;
            block.blockShape.AddBlockMesh(meshPos, nb, meshRef, lightData);
        }

        //get all neighbor blocks in a safe fashion
        static NeighborBlocks GetNeighborsSafe(SubChunk sub, Vector3i pos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
        Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            NeighborBlocks nb = new();
            int x = pos.X;
            int y = pos.Y;
            int z = pos.Z;

            //helper to get neighbor block safely
            BlockState N(int ox, int oy, int oz) => GetNeighborBlockSafe(sub, x, y, z, ox, oy, oz, leftC, rightC, frontC, backC, c1, c2, c3, c4);

            //top face + top corners
            nb.top = N(0, +1, 0);
            nb.bottom = N(0, -1, 0);
            nb.front = N(0, 0, +1);
            nb.back = N(0, 0, -1);
            nb.right = N(1, 0, 0);
            nb.left = N(-1, 0, 0);
            return nb;
        }

        //returns the values necessary for computing the light values for meshing
        static LightingData GetLightData(SubChunk sub, Vector3i pos, Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC,
        Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            LightingData ld = new();
            int x = pos.X;
            int y = pos.Y;
            int z = pos.Z;

            //helper to get neighbor block safely
            ushort L(int ox, int oy, int oz) => GetLightSafe(sub, x, y, z, ox, oy, oz, leftC, rightC, frontC, backC, c1, c2, c3, c4);
            ld.thisLight = L(0, 0, 0);

            //top face + top corners
            ld.topLight = L(0, +1, 0);
            ld.bottomLight = L(0, -1, 0);
            ld.frontLight = L(0, 0, +1);
            ld.backLight = L(0, 0, -1);
            ld.rightLight = L(1, 0, 0);
            ld.leftLight = L(-1, 0, 0);

            return ld;
        }

        //helper to fetch neighbor types safely
        static BlockState GetNeighborBlockSafe(SubChunk sub, int x, int y, int z, int offsetX, int offsetY, int offsetZ,
        Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            const int cs = CHUNK_WIDTH - 1;

            int nx = (sub.ChunkXPos * SUBCHUNK_SIZE) + x + offsetX;
            int ny = (sub.ChunkYPos * SUBCHUNK_SIZE) + y + offsetY; //global Y
            int nz = (sub.ChunkZPos * SUBCHUNK_SIZE) + z + offsetZ;

            //out of world bounds (vertical)
            if (ny < 0 || ny >= Chunk.CHUNK_HEIGHT) return Block.AIR;
            if ((uint)nx < CHUNK_WIDTH && (uint)nz < CHUNK_WIDTH) return sub.parent.GetBlockUnsafe(nx, ny, nz);

            //start with this chunk as default
            Chunk? targetChunk = sub.parent;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= CHUNK_WIDTH;
            bool nzBack = nz < 0;
            bool nzFront = nz >= CHUNK_WIDTH;

            //if diagonal: prefer corner chunks
            if (nxLeft && nzBack) targetChunk = c1; //back-left           
            else if (nxRight && nzBack) targetChunk = c2; //back-right           
            else if (nxLeft && nzFront) targetChunk = c3; //front-right        
            else if (nxRight && nzFront) targetChunk = c4; //front-left           
            else
            {
                //non-diagonal: choose along X or Z
                if (nxLeft) targetChunk = leftC;
                else if (nxRight) targetChunk = rightC;

                if (nzBack) targetChunk = backC;
                else if (nzFront) targetChunk = frontC;
            }

            if (targetChunk == null || !targetChunk.HasVoxelData()) return Block.AIR;

            //convert to local coordinates inside target chunk
            int localX = ((nx & cs) + CHUNK_WIDTH) & cs;
            int localZ = ((nz & cs) + CHUNK_WIDTH) & cs;

            return targetChunk.GetBlockUnsafe(localX, ny, localZ);
        }

        //gets the lighting value of a block safely
        static ushort GetLightSafe(SubChunk sub, int x, int y, int z, int offsetX, int offsetY, int offsetZ,
        Chunk? leftC, Chunk? rightC, Chunk? frontC, Chunk? backC, Chunk? c1, Chunk? c2, Chunk? c3, Chunk? c4)
        {
            const ushort defaultLight = ((0 & 0xF) | ((0 & 0xF) << 4) | ((0 & 0xF) << 8) | ((15 & 0xF) << 12));
            const int cs = CHUNK_WIDTH - 1;

            int nx = (sub.ChunkXPos * SUBCHUNK_SIZE) + x + offsetX;
            int ny = (sub.ChunkYPos * SUBCHUNK_SIZE) + y + offsetY; //global Y
            int nz = (sub.ChunkZPos * SUBCHUNK_SIZE) + z + offsetZ;

            //out of world bounds (vertical)
            if (ny < 0 || ny >= Chunk.CHUNK_HEIGHT) return defaultLight;
            if ((uint)nx < CHUNK_WIDTH && (uint)nz < CHUNK_WIDTH) return sub.parent.GetLight(nx, ny, nz);

            //start with this chunk as default
            Chunk? targetChunk = sub.parent;

            bool nxLeft = nx < 0;
            bool nxRight = nx >= CHUNK_WIDTH;
            bool nzBack = nz < 0;
            bool nzFront = nz >= CHUNK_WIDTH;

            //if diagonal: prefer corner chunks
            if (nxLeft && nzBack) targetChunk = c1; //back-left           
            else if (nxRight && nzBack) targetChunk = c2; //back-right           
            else if (nxLeft && nzFront) targetChunk = c3; //front-right        
            else if (nxRight && nzFront) targetChunk = c4; //front-left           
            else
            {
                //non-diagonal: choose along X or Z
                if (nxLeft) targetChunk = leftC;
                else if (nxRight) targetChunk = rightC;

                if (nzBack) targetChunk = backC;
                else if (nzFront) targetChunk = frontC;
            }

            if (targetChunk == null || !targetChunk.HasVoxelData()) return defaultLight;

            //convert to local coordinates inside target chunk
            int localX = ((nx & cs) + CHUNK_WIDTH) & cs;
            int localZ = ((nz & cs) + CHUNK_WIDTH) & cs;

            return targetChunk.GetLight(localX, ny, localZ);
        }
    }
}
