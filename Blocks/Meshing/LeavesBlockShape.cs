using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
using OpenTK.Mathematics;

namespace OurCraft.Blocks.Meshing
{
    public class LeavesBlockShape : BlockShape
    {
        //just build a full block, nothing crazy
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, VoxelAOData aOData)
        {
            //already has some helper methods in block mesh builder
            BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, thisState,
            BottomFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, RightFaceTex, LeftFaceTex, mesh, aOData);
        }

        //same thing as full block, just different face type
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.LEAVES;
        }
    }
}
