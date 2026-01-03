using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.Graphics.Voxel_Lighting;

namespace OurCraft.Blocks.Meshing
{
    public class FullBlockModelShape : BlockShape
    {
        public FullBlockModelShape()
        {
            IsFullOpaqueBlock = true;
        }
        public CachedBlockModel cachedModel = new();

        //just build a full cube block, nothing crazy
        public override void AddBlockMesh(Vector3 pos, NeighborBlocks nb, ChunkMeshData mesh, LightingData lightData)
        {           
            BlockMeshBuilder.BuildFromCachedModel(cachedModel, pos, nb, mesh, lightData);
        }

        //full block face getting is simple
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return cachedModel.FaceCull[(byte)faceSide];
        }
    }
}
