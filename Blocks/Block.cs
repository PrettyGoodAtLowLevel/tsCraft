using OpenTK.Mathematics;
using OurCraft.Rendering;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

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

    //a refrence to mesh and texture data for a chunk to use when building their mesh
    //some blocks also have state properties that chunks hold onto for things like orientatation
    public abstract class Block
    {
        //block data
        protected string name = "";       
        protected ushort id = 0;

        //texture ids
        protected int bottomFaceTex = 0;
        protected int topFaceTex = 0;
        protected int frontFaceTex = 0;
        protected int backFaceTex = 0;
        protected int rightFaceTex = 0;
        protected int leftFaceTex = 0;

        //ctr
        public Block(string name, int bm, int t, int f, int b, int r, int l, ushort id)
        {
            this.name = name;
            this.id = id;
            bottomFaceTex = bm;
            topFaceTex = t;
            frontFaceTex = f;
            backFaceTex = b;
            rightFaceTex = r;
            leftFaceTex = l;          
        }

        //set id of block
        public void SetID(ushort id)
        {
            this.id = id;
        }

        //each block defines their own mesh based on block state and neighbor data
        public virtual void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top,
        BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state) { }

        //determines each blocks way of changing the chunk block state data when placed
        public virtual void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal,
        BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world) { }

        //determines each blocks way of changing the chunk block state data when placed
        public virtual void UpdateBlockState(Vector3 globalPos,
        BlockState thisBlock, Chunkmanager world) { }

        //gets the face type from a specified block state
        public virtual FaceType GetBlockFace(CubeFaces faceSide, BlockState state) { return FaceType.FULL; }

        //get other block info
        public string GetBlockName() { return name; }
    }
}