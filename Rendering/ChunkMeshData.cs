namespace OurCraft.Rendering
{
    //holds only vertex data for a chunk, each subchunk has one of these
    public class ChunkMeshData
    {
        //mesh data
        public readonly List<BlockVertex> vertices = [];
        public uint VertexCount { get; private set; } = 0;

        //add a vertex safely to vertices
        public void AddChunkMeshData(BlockVertex v)
        {
            vertices.Add(v);
            VertexCount++;
        }

        //clear mesh and rebuild as empty
        public void ClearMesh()
        {
            vertices.Clear();
            if (vertices.Count == 0)
            {
                vertices.Capacity = 0;
            }
        }
    }
}
