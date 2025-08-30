using OurCraft.Rendering;
using OpenTK.Mathematics;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //half block
    //contains one property - which type of slab we are using
    public class SlabBlock : Block
    {
        public static readonly EnumProperty<SlabType> SLAB_TYPE;

        //initializes the bits for a slab state
        static SlabBlock()
        {
            var layout = new PropertyLayoutBuilder();
            SLAB_TYPE = layout.AddEnum<SlabType>();
        }

        //default constructor
        public SlabBlock(string name, int bm, int t, int f, int b, int r, int l, int id)
        : base(name, bm, t, f, b, r, l, id) { }

        public SlabBlock(string name, int t, int id) :
        base(name, t, t, t, t, t, t, id){ }

        //adds the correct block mesh of the slab to the world
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state)
        {
            SlabType type = state.GetProperty(SLAB_TYPE);

            if (type == SlabType.Double) BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, state, topFaceTex, topFaceTex, frontFaceTex, backFaceTex, rightFaceTex, leftFaceTex, mesh);
            else if (type == SlabType.Top) BlockMeshBuilder.BuildSlab(pos, bottom, top, front, back, right, left, state, topFaceTex, topFaceTex, frontFaceTex, backFaceTex, rightFaceTex, leftFaceTex, mesh, false);
            else BlockMeshBuilder.BuildSlab(pos, bottom, top, front, back, right, left, state, topFaceTex, topFaceTex, frontFaceTex, backFaceTex, rightFaceTex, leftFaceTex, mesh, true);
        }

        //determines how we place a slab in the world based on the slab we hit
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            SlabType thisBlockState = thisBlock.GetProperty(SLAB_TYPE);
            if (hitNormal.Y == 1 && thisBlock.BlockID == id && thisBlockState == SlabType.Bottom)
            {
                world.SetBlock(globalPos, new BlockState((byte)id).WithProperty(SLAB_TYPE, SlabType.Double));
                return;
            }
            else if (hitNormal.Y == -1 && thisBlock.BlockID == id && thisBlockState == SlabType.Top)
            {
                world.SetBlock(globalPos, new BlockState((byte)id).WithProperty(SLAB_TYPE, SlabType.Double));
                return;
            }
            else if (hitNormal.Y == 1)
            {
                world.SetBlock(globalPos + hitNormal, new BlockState((byte)id).WithProperty(SLAB_TYPE, SlabType.Bottom));
                return;
            }
            else if (hitNormal.Y == -1)
            {
                world.SetBlock(globalPos + hitNormal, new BlockState((byte)id).WithProperty(SLAB_TYPE, SlabType.Top));
                return;
            }
            else
            {
                world.SetBlock(globalPos + hitNormal, new BlockState((byte)id).WithProperty(SLAB_TYPE, SlabType.Bottom));
                return;
            }
        }

        //getting the face for the slab based on block state
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            SlabType type = state.GetProperty(SLAB_TYPE);

            //double slabs have the same shape as a normal block
            if (type == SlabType.Double)
                return FaceType.FULL;

            //slab on bottom half of block
            else if (type == SlabType.Bottom)
            {
                if (faceSide == CubeFaces.BOTTOM) return FaceType.FULL; //has a full face at the bottom
                if (faceSide == CubeFaces.TOP) return FaceType.INDENTED; // doesnt have an adjacent face on top
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
