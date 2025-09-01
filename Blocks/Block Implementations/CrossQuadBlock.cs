using OurCraft.Rendering;
using OurCraft.World;
using OpenTK.Mathematics;

namespace OurCraft.Blocks.Block_Implementations
{
    //x shaped blocks like flowers and tall grass
    public class CrossQuadBlock : Block
    {
        public CrossQuadBlock(string name, int bm, int t, int f, int b, int r, int l, int id) :
        base(name, bm, t, f, b, r, l, id)
        { }

        public CrossQuadBlock(string name, int t, int id) :
        base(name, t, t, t, t, t, t, id)
        { }

        //getting the face for the block, pretty basic
        //we want all faces on a cross quad block to be visible
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.INDENTED;
        }

        //mesh implementation for a full block
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state)
        {
            BlockMeshBuilder.BuildXShapeBlock(pos, bottomFaceTex, mesh);
        }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            world.SetBlock(globalPos + hitNormal, new BlockState((byte)id));
        }
    }
}
