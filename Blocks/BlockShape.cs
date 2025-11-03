using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
using OpenTK.Mathematics;

namespace OurCraft.Blocks
{
    //face data to index
    public enum CubeFaces
    {
        BOTTOM,
        TOP,
        FRONT,
        BACK,
        RIGHT,
        LEFT
    }

    //face culling data for blocks to use
    public enum FaceType
    {
        AIR,
        FULL,           //regular cube face
        CUTOUT,         //things like stairs
        BOTTOM_SLAB,    //botom part of slab
        TOP_SLAB,       //top slab
        INDENTED,       //faces that are not on the edge of a cube 
        WATER,          //water (duh)
        WATER_TOP,
        WATER_BOTTOM,
        GLASS,          //duh
        LEAVES,
    }

    //represents a shape of a block so that the meshing code is more abstract and clean
    public abstract class BlockShape
    {
        public BlockShape()
        {
          
        }

        public int BottomFaceTex { get; set; } = 0;
        public int TopFaceTex { get; set; } = 0;
        public int FrontFaceTex { get; set; } = 0;
        public int BackFaceTex { get; set; } = 0;
        public int RightFaceTex { get; set; } = 0;
        public int LeftFaceTex { get; set; } = 0;
        public bool IsFullBlock { get; set; } = false;

        //how does this block shape get added to the world based on state
        public virtual void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top,
        BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, VoxelAOData aoData)
        { }

        //gets the face type from a specified block state
        public virtual FaceType GetBlockFace(CubeFaces faceSide, BlockState state) { return FaceType.FULL; }
    }
}
