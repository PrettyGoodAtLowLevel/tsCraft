using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Graphics.ChunkRendering;
using OurCraft.Utility;
using OurCraft.World.WorldData;
using OurCraft.World.WorldGeneration.Mesh_Building;

namespace OurCraft.World.WorldGeneration
{
    //helps build mesh data of chunk/subchunks
    public static class ChunkBuilder
    {
        const int HEIGHT_IN_SUBCHUNKS = WorldConstants.CHUNK_HEIGHT_IN_SUBCHUNKS;

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
                SubChunkMesher.MeshSubChunk(subChunk, neighbors);
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
            SubChunkMesher.MarkSubChunksDirty(chunk);

            ChunkSectionNeighbors neighbors = new(chunk)
            {
                leftC = leftC, rightC = rightC,
                frontC = frontC, backC = backC,

                c1 = c1, c2 = c2,
                c3 = c3, c4 = c4,
            };

            //remesh dirty subchunks only once and reupload mesh
            for (int y = 0; y < HEIGHT_IN_SUBCHUNKS; y++)
            {

                if (!chunk.DirtySubChunks[y]) continue;
                SubChunk sub = chunk.SubChunks[y];
                SubChunkMesher.RemeshSubChunk(sub, neighbors);
                chunk.DirtySubChunks[y] = false;

            }

            chunk.changes.Clear();
            ChunkRenderer.GLUploadChunk(chunk);
        }
    }
}