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

        //just build a full block, nothing crazy
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, LightingData lightData, ushort thisLight)
        {
            BlockModelMeshBuilder.BuildFromCachedModel(cachedModel, pos, bottom, top, front, back, right, left, thisState, mesh, lightData);
        }

        //full block face getting is simple
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return cachedModel.FaceCull[(byte)faceSide];
        }
    }
}
