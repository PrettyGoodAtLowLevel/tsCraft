namespace OurCraft.Rendering
{
    //holds only vertex data for a chunk, each subchunk has one of these
    public class ChunkMeshData
    {
        //mesh data
        public readonly List<BlockVertex> vertices = [];
        public readonly List<uint> indices = [];
        public uint IndexCount { get; private set; } = 0;
        public uint VertexCount { get; private set; } = 0;

        //add a vertex safely to vertices
        public void AddChunkMeshData(BlockVertex v)
        {
            vertices.Add(v);
            VertexCount++;
        }

        //adds all the indices for a cube face
        public void AddQuadIndices()
        {
            indices.Add(IndexCount + 0);
            indices.Add(IndexCount + 1);
            indices.Add(IndexCount + 2);
            indices.Add(IndexCount + 2);
            indices.Add(IndexCount + 3);
            indices.Add(IndexCount + 0);

            IndexCount = (uint)vertices.Count;
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
            IndexCount = 0;
        }
    }
}
