using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;

namespace OurCraft.Blocks.Meshing
{
    public class BlockLogShape : BlockShape
    {
        //find axis and switch textures around full block shape
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState)
        {
            Axis axis = thisState.GetProperty(BlockLog.AXIS);

            //add mesh type based on axis
            if (axis == Axis.X) BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, thisState,
            RightFaceTex, LeftFaceTex, FrontFaceTex, BackFaceTex, TopFaceTex, TopFaceTex, mesh);

            else if (axis == Axis.Z) BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, thisState,
            RightFaceTex, LeftFaceTex, TopFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, mesh);

            else if (axis == Axis.Y) BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, thisState,
            TopFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, RightFaceTex, LeftFaceTex, mesh);
        }


        //full block face getting is simple
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state) 
        { 
            return FaceType.FULL; 
        }
    }
}