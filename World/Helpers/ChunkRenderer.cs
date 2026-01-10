using OurCraft.Entities.Components;
using OurCraft.Graphics;

namespace OurCraft.World.Helpers
{
    //helps render and create openGL buffers of chunks
    public static class ChunkRenderer
    {
        public static void GLUploadChunk(Chunk chunk)
        {
            ChunkState state = chunk.GetState();
            if (state == ChunkState.Deleted || state == ChunkState.VoxelOnly || state == ChunkState.Initialized || chunk.IsMeshing())
                return;

            UploadBatchedMesh(chunk, chunk.batchedMesh, transparent: false);
            UploadBatchedMesh(chunk, chunk.transparentMesh, transparent: true);

            chunk.SetState(ChunkState.Built);
        }

        public static void DrawSolid(Chunk chunk, Shader shader, CameraRender cam)
        {
            chunk.batchedMesh.Draw(shader, chunk.WorldPos, cam.Transform.position);
        }

        public static void DrawTransparent(Chunk chunk, Shader shader, CameraRender cam)
        {
            chunk.transparentMesh.Draw(shader, chunk.WorldPos, cam.Transform.position);
        }

        //combines vertex data of subchunks into one big openGL mesh
        private static void UploadBatchedMesh(Chunk chunk, ChunkMesh mesh, bool transparent = false)
        {
            //compute size upfront
            int totalVertexCount = 0;

            foreach (var subChunk in chunk.SubChunks)
            {
                if (transparent)
                {
                    totalVertexCount += subChunk.TransparentGeo.vertices.Count;
                }
                else
                {
                    totalVertexCount += subChunk.SolidGeo.vertices.Count;
                }
            }

            chunk.batchedMesh.SetupIndices(totalVertexCount);
            chunk.transparentMesh.SetupIndices(totalVertexCount);

            //preallocate size
            var totalVertices = new List<BlockVertex>(totalVertexCount);

            foreach (var subChunk in chunk.SubChunks)
            {
                if (transparent)
                {
                    ChunkMeshData geo = subChunk.TransparentGeo;
                    if (geo.vertices.Count == 0) continue;

                    totalVertices.AddRange(geo.vertices);
                }
                else
                {
                    ChunkMeshData geo = subChunk.SolidGeo;
                    if (geo.vertices.Count == 0) continue;

                    totalVertices.AddRange(geo.vertices);
                }
            }

            //mesh upload
            mesh.SetupMesh(totalVertices);
        }
    }
}
