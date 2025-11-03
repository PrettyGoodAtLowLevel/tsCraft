using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
using OpenTK.Mathematics;

namespace OurCraft.Blocks.Meshing
{
    public class GlassBlockShape : BlockShape
    {
        //alpha cutout glass face
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.GLASS;
        }

        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state, VoxelAOData aOData)
        {
            BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, state,
            BottomFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, RightFaceTex, LeftFaceTex, mesh, aOData);
        }
    }
}
