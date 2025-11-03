using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
using OpenTK.Mathematics;

namespace OurCraft.Blocks.Meshing
{
    //half block shape, a little more complex
    public class SlabBlockShape : BlockShape
    {
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, VoxelAOData aOData)
        {
            SlabType type = thisState.GetProperty(SlabBlock.SLAB_TYPE);

            if (type == SlabType.Double) BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, thisState,
            BottomFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, RightFaceTex, LeftFaceTex, mesh, aOData);

            else if (type == SlabType.Top) BlockMeshBuilder.BuildSlab(pos, bottom, top, front, back, right, left, thisState,
            BottomFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, RightFaceTex, LeftFaceTex, mesh, false, aOData);

            else BlockMeshBuilder.BuildSlab(pos, bottom, top, front, back, right, left, thisState,
            BottomFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, RightFaceTex, LeftFaceTex, mesh, true, aOData);
        }

        //getting the face for the slab based on block state
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            SlabType type = state.GetProperty(SlabBlock.SLAB_TYPE);

            //double slabs have the same shape as a normal block
            if (type == SlabType.Double)
                return FaceType.FULL;

            //slab on bottom half of block
            else if (type == SlabType.Bottom)
            {
                if (faceSide == CubeFaces.BOTTOM) return FaceType.FULL; //has a full face at the bottom
                if (faceSide == CubeFaces.TOP) return FaceType.INDENTED; //doesnt have an adjacent face on top
                //other cases this is a bottom slab block
                return FaceType.BOTTOM_SLAB;
            }
            else //topslab
            {
                if (faceSide == CubeFaces.BOTTOM) return FaceType.INDENTED; //bottom face of top slab doesnt touch anything
                if (faceSide == CubeFaces.TOP) return FaceType.FULL; //top slab face touches the bottom of the block above
                //other cases this is a bottom slab block
                return FaceType.TOP_SLAB;
            }
        }
    }
}
