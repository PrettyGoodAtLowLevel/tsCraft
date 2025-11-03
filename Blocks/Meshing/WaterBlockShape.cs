using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
using OpenTK.Mathematics;

namespace OurCraft.Blocks.Meshing
{
    public class WaterBlockShape : BlockShape
    {

        //getting the face for the water block, pretty basic
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            if (faceSide == CubeFaces.TOP) return FaceType.WATER_TOP;
            else if (faceSide == CubeFaces.BOTTOM) return FaceType.WATER_BOTTOM;

            return FaceType.WATER;
        }

        //basic water mesh
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, VoxelAOData aOData)
        {
            BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, thisState,
            BottomFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, RightFaceTex, LeftFaceTex, mesh, aOData);
        }
    }
}
