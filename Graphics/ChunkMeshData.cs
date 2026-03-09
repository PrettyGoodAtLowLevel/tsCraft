namespace OurCraft.Graphics
{
    //holds only vertex data for a chunk, each subchunk has one of these
    public class ChunkMeshData
    {
        ~ChunkMeshData()
        {
            ClearMesh();
        }

        //clear mesh and rebuild as empty
        public void ClearMesh()
        {
            vertices.Clear();
        }

        //mesh data
        public readonly List<BlockVertex> vertices = [];

        //add a vertex safely to vertices
        public void AddChunkMeshData(BlockVertex v)
        {
            vertices.Add(v);
        }
    }
}
