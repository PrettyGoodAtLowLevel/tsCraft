using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
using OpenTK.Mathematics;

namespace OurCraft.Blocks.Meshing
{
    //x shaped block, for flowers
    public class CrossQuadBlockShape : BlockShape
    {
        //we want all faces on a cross quad block to be visible
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.INDENTED;
        }

        //mesh implementation for a full block
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state, VoxelAOData aOData)
        {
            BlockMeshBuilder.BuildXShapeBlock(pos, BottomFaceTex, mesh);
        }
    }
}
