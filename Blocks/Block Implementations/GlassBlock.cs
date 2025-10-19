using OpenTK.Mathematics;
using OurCraft.Rendering;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //fully transparent glass block, no blending
    public class GlassBlock : FullBlock
    {
        public GlassBlock(string name, int bm, int t, int f, int b, int r, int l, ushort id) :
        base(name, bm, t, f, b, r, l, id)
        { }

        public GlassBlock(string name, int t, ushort id) :
        base(name, t, t, t, t, t, t, id)
        { }

        //alpha cutout glass face
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.GLASS;
        }

        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state)
        {
            BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, state, bottomFaceTex, topFaceTex, frontFaceTex, backFaceTex, rightFaceTex, leftFaceTex, mesh);
        }
    }
}
