using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;
using OpenTK.Mathematics;
using OpenTK.Graphics.ES11;

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
        FULLBLOCK,           //regular cube face
        CUTOUT,         //things like stairs
        BOTTOMSLAB,    //botom part of slab
        TOPSLAB,       //top slab
        INDENTED,       //faces that are not on the edge of a cube 
        WATER,          //water (duh)
        WATERTOP,
        WATERBOTTOM,
        GLASS,          //duh
        LEAVES,
    }

    //represents a shape of a block so that the meshing code is more abstract and clean
    public abstract class BlockShape
    {
        public BlockShape()
        { }

        public bool IsFullOpaqueBlock { get; set; } = false;
        public bool IsTranslucent { get; set; } = false;

        //how does this block shape get added to the world based on state
        public virtual void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top,
        BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, VoxelAOData aoData,
        ushort topLight, ushort bottomLight, ushort frontLight, ushort backLight, ushort rightLight, ushort leftLight)
        { }

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

        public static int GetTextureID(string name)
        {
            return TextureRegistry.GetTextureID(name);
        }
    }
}
