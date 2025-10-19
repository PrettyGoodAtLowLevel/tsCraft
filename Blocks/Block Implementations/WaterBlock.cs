using OpenTK.Mathematics;
using OurCraft.Rendering;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //like full block but translucent
    public class WaterBlock : FullBlock
    {
        public WaterBlock(string name, int bm, int t, int f, int b, int r, int l, ushort id) :
        base(name, bm, t, f, b, r, l, id)
        { }

        public WaterBlock(string name, int t, ushort id) :
        base(name, t, t, t, t, t, t, id)
        { }

        //getting the face for the water block, pretty basic
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            if (faceSide == CubeFaces.TOP) return FaceType.WATER_TOP;
            else if (faceSide == CubeFaces.BOTTOM) return FaceType.WATER_BOTTOM;

            return FaceType.WATER;
        }

        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state)
        {
            BlockMeshBuilder.BuildFullWater(pos, bottom, top, front, back, right, left, state, bottomFaceTex, topFaceTex, frontFaceTex, backFaceTex, rightFaceTex, leftFaceTex, mesh);
        }
    }
}
