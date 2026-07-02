using OpenTK.Mathematics;
using OurCraft.Graphics;
using OurCraft.World;

namespace OurCraft.Blocks.Meshing
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
        FULLBLOCK,      //regular cube face
        CUTOUT,         //things like stairs
        BOTTOMSLAB,     //botom part of slab
        TOPSLAB,        //top slab
        INDENTED,       //faces that are not on the edge of a cube 
        WATER,          //water (duh)
        WATERTOP,
        WATERBOTTOM,
        GLASS,          //duh
        LEAVES,         //never culled
    }

    //different animation types for different blocks
    enum AnimationType
    {
        None = 0,
        Leaves = 1,
        Grass = 2,
        Crop = 3,
        Water = 4,
        Vine = 5
    }

    //represents a shape of a block so that the meshing code is more abstract and clean
    public abstract class BlockShape
    {       
        public bool IsFullOpaqueBlock { get; set; } = false;
        public bool IsTranslucent { get; set; } = false;

        //how does this block shape get added to the world based on state
        public virtual void AddBlockMesh(Vector3 pos, NeighborBlocks nb, ChunkMeshData mesh, ChunkSectionNeighbors nc) { }

        //gets the face type from a specified block state
        public virtual FaceType GetBlockFace(CubeFaces faceSide, BlockState state) { return FaceType.FULLBLOCK; }

        //helpers for getting faces
        public static CubeFaces FaceNameToCubeFace(string name)
        {
            switch (name)
            {
                case "Bottom":
                    return CubeFaces.BOTTOM;
                case "Top":
                    return CubeFaces.TOP;
                case "Front":
                    return CubeFaces.FRONT;
                case "Back":
                    return CubeFaces.BACK;
                case "Right":
                    return CubeFaces.RIGHT;
                case "Left":
                    return CubeFaces.LEFT;
                default:
                    return CubeFaces.BOTTOM;
            }
        }

        //get the current face type from json string value
        public static FaceType FaceTypeFromString(string name)
        {
            return (FaceType)Enum.Parse(typeof(FaceType), name.ToUpper());
        }

        public override string ToString()
        {
            string str = "";

            str += $"Occludes Everything: {IsFullOpaqueBlock}, ";
            str += $"Is Translucent: {IsTranslucent}";

            return str;
        }
    }
}
