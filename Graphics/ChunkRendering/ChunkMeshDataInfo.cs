namespace OurCraft.Graphics.ChunkRendering
{
    //helper to track chunk mesh data offset GPU side
    public struct ChunkMeshDataInfo
    {
        public int oldVertexOffset = 0;
        public int oldVertexCount = 0;

        public int vertexOffset = 0;
        public int vertexCount = 0;

        public ChunkMeshDataInfo() { }
    }
}
