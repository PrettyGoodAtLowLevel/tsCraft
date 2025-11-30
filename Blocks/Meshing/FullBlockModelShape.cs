using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
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
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, VoxelAOData aOData,
        ushort topLight, ushort bottomLight, ushort frontLight, ushort backLight, ushort rightLight, ushort leftLight)
        {
            BlockModelMeshBuilder.BuildFromCachedModel(cachedModel, pos, bottom, top, front, back, right, left, thisState, mesh, aOData,
            topLight, bottomLight, frontLight, backLight, rightLight, leftLight);
        }

        //full block face getting is simple
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return cachedModel.FaceCull[(byte)faceSide];
        }
    }
}
