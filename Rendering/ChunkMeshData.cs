namespace OurCraft.Rendering
{
    //holds only vertex data for a chunk, each subchunk has one of these
    public class ChunkMeshData
    {
        //mesh data
        public readonly List<Vertex> vertices = [];
        public readonly List<uint> indices = [];
        private uint indexSize = 0;

        //add a vertex safely to vertices
        public void AddChunkMeshData(Vertex v)
        {
            vertices.Add(v);
        }

        //adds all the indices for a cube face
        public void AddQuadIndices()
        {
            indices.Add(indexSize + 0);
            indices.Add(indexSize + 1);
            indices.Add(indexSize + 2);
            indices.Add(indexSize + 2);
            indices.Add(indexSize + 3);
            indices.Add(indexSize + 0);

            indexSize = (uint)vertices.Count;
        }

        //clear mesh and rebuild as empty
        public void ClearMesh()
        {
            vertices.Clear();
            indices.Clear();
            if (vertices.Count == 0 && indices.Count == 0)
            {
                vertices.Capacity = 0;
                indices.Capacity = 0;
            }
            indexSize = 0;
        }
    }
}
