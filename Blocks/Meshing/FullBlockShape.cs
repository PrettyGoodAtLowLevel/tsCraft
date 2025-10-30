using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;

namespace OurCraft.Blocks.Meshing
{
    public class FullBlockShape : BlockShape
    {
        //just build a full block, nothing crazy
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState)
        {
            //already has some helper methods in block mesh builder
            BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, thisState,
            BottomFaceTex, TopFaceTex, FrontFaceTex, BackFaceTex, RightFaceTex, LeftFaceTex, mesh);
        }

        //full block face getting is simple
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.FULL;
        }
    }
}