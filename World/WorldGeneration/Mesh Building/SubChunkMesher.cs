using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Graphics.ChunkRendering;
using OurCraft.Utility;
using OurCraft.World.WorldData;

namespace OurCraft.World.WorldGeneration.Mesh_Building
{
    //helper for creating subchunk meshes and updating them
    public static class SubChunkMesher
    {
        const int HEIGHT_IN_SUBCHUNKS = WorldConstants.CHUNK_HEIGHT_IN_SUBCHUNKS;
        const int WIDTH_IN_SUBCHUNKS = WorldConstants.CHUNK_WIDTH_IN_SUBCHUNKS;
        const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS;

        //based on the amount of changes in the chunk, find all the subchunks that were effected
        public static void MarkSubChunksDirty(Chunk chunk)
        {
            const int sb = SubChunk.SUBCHUNK_SIZE - 1;
            for (int i = 0; i < chunk.changes.Count; i++)
            {
                var chunkYPosChange = chunk.changes[i];
                int subY = chunkYPosChange / SubChunk.SUBCHUNK_SIZE;
                int localY = chunkYPosChange & sb;

                //mark owning subchunk
                if ((uint)subY < HEIGHT_IN_SUBCHUNKS) chunk.DirtySubChunks[subY] = true;

                //Y neighbors
                if (localY == sb && subY + 1 < HEIGHT_IN_SUBCHUNKS) chunk.DirtySubChunks[subY + 1] = true;
                if (localY == 0 && subY > 0) chunk.DirtySubChunks[subY - 1] = true;
            }
        }

        //subchunk meshing
        //creates cpu side subchunk mesh data
        public static void MeshSubChunk(SubChunk sub, ChunkSectionNeighbors nc)
        {
            if (sub.IsAllAir()) return; //dont create mesh if chunk is completely air

            int cyp = sub.ChunkYPos * SUBCHUNK_SIZE;
            for (int y = SUBCHUNK_SIZE - 1; y >= 0; y--)
            {
                for (int x = SUBCHUNK_SIZE - 1; x >= 0; x--)
                {
                    for (int z = SUBCHUNK_SIZE - 1; z >= 0; z--)
                    {
                        AddMeshDataToChunk(sub, pos: new Vector3i(x, y, z), meshPos: new Vector3(x, cyp + y, z), nc);
                    }
                }
            }
        }

        //clears subchunk cpu side mesh and recreates it
        public static void RemeshSubChunk(SubChunk sub, ChunkSectionNeighbors nc)
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
            NeighborBlocks nb = ChunkNeighborHelpers.GetNeighborsSafe(sub, pos, nc);
            nb.thisState = state;

            //if surrounding blocks are all full solid cubes, and the current block is also full solid, then skip meshing entirely
            if (nb.top.BlockShape.IsFullOpaqueBlock && nb.bottom.BlockShape.IsFullOpaqueBlock
            && nb.front.BlockShape.IsFullOpaqueBlock && nb.back.BlockShape.IsFullOpaqueBlock &&
            nb.left.BlockShape.IsFullOpaqueBlock && nb.right.BlockShape.IsFullOpaqueBlock
            && block.blockShape.IsFullOpaqueBlock) return;

            ChunkMeshData meshRef = block.blockShape.IsTranslucent ? sub.TransparentGeo : sub.SolidGeo;
            block.blockShape.AddBlockMesh(meshPos, nb, meshRef, nc);
        }
    }
}
