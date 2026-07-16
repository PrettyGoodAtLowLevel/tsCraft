using OpenTK.Mathematics;
using OurCraft.Graphics.ChunkRendering;
using OurCraft.World.WorldData;
using OurCraft.World.WorldGeneration.Mesh_Building;

namespace OurCraft.Blocks.Meshing.ModelShapes
{
    //basic one model block shape
    public class FullBlockModelShape : BlockShape
    {
        public CachedBlockModel cachedModel = new();

        public FullBlockModelShape()
        {
            IsFullOpaqueBlock = true;
        }      

        //just build a full cube block, nothing crazy
        public override void AddBlockMesh(Vector3 pos, NeighborBlocks nb, ChunkMeshData mesh, ChunkSectionNeighbors nc)
        {           
            BlockMeshBuilder.BuildFromCachedModel(cachedModel, pos, nb, mesh, nc);
        }

        //full block face getting is simple
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return cachedModel.FaceCull[(byte)faceSide];
        }
    }
}
