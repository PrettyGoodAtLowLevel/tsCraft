using OpenTK.Mathematics;
using OurCraft.Rendering;
using OurCraft.World;

namespace OurCraft.Blocks.Block_Implementations
{
    //full regular full cube solid block implementation
    public class FullBlock : Block
    {
        //csctr
        public FullBlock(string name, int bm, int t, int f, int b, int r, int l, int id): 
        base(name, bm, t, f,  b, r,  l, id)
        { }

        public FullBlock(string name, int t, int id) :
        base(name, t, t, t, t, t, t, id)
        { }

        public FullBlock(string name, int b, int t, int s, int id) :
        base(name, b, t, s, s, s, s, id)
        { }

        //getting the face for the full block, pretty basic
        //every full cube block face, is well... a full face
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.FULL;
        }

        //mesh implementation for a full block
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state)
        {
            BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, state, bottomFaceTex, topFaceTex, frontFaceTex, backFaceTex, rightFaceTex, leftFaceTex, mesh);
        }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            world.SetBlock(globalPos + hitNormal, new BlockState((byte)id));
        }
    }
}
