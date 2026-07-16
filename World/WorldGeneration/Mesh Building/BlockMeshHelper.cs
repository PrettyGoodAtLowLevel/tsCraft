using OurCraft.Blocks.Meshing;
using OurCraft.Blocks;

namespace OurCraft.World.WorldGeneration.Mesh_Building
{
    //contains tons of helpers for building block meshes in a chunk
    public static class BlockMeshHelper
    {
        //gets the opposite of a specified face direction
        public static CubeFaces Opposite(CubeFaces face)
        {
            return face switch
            {
                CubeFaces.BOTTOM => CubeFaces.TOP,
                CubeFaces.TOP => CubeFaces.BOTTOM,
                CubeFaces.FRONT => CubeFaces.BACK,
                CubeFaces.BACK => CubeFaces.FRONT,
                CubeFaces.RIGHT => CubeFaces.LEFT,
                CubeFaces.LEFT => CubeFaces.RIGHT,
                _ => CubeFaces.BOTTOM,
            };
        }

        //gets the state of each block from face type, then checks if the faces should be visible
        public static bool IsFaceVisible(BlockState thisState, BlockState neighborState, CubeFaces face)
        {
            FaceType thisFace = thisState.BlockShape.GetBlockFace(face, thisState);
            FaceType neighborFace = neighborState.BlockShape.GetBlockFace(Opposite(face), neighborState);

            return ShouldRenderFace(thisFace, neighborFace);
        }

        //compares the face types against each other
        private static bool ShouldRenderFace(FaceType current, FaceType neighbor)
        {
            //air has no geometry
            if (current == FaceType.AIR) return false;

            //leaves never cull
            if (current == FaceType.LEAVES || neighbor == FaceType.LEAVES) return true;

            //render against air
            if (neighbor == FaceType.AIR)  return true;

            //water special cases
            if (current == FaceType.WATERTOP && neighbor == FaceType.WATER || current == FaceType.WATERTOP
            && neighbor == FaceType.WATERBOTTOM || current == FaceType.WATERBOTTOM && neighbor == FaceType.WATERTOP)
            return false;

            //full blocks hide neighboring full blocks
            if (current == FaceType.FULLBLOCK && neighbor == FaceType.FULLBLOCK) return false;

            //same partial faces cull each other
            if (current == neighbor && current != FaceType.INDENTED && current != FaceType.WATERTOP)  return false;

            //full blocks hide most partials
            if (current != FaceType.INDENTED && current != FaceType.WATERTOP && neighbor == FaceType.FULLBLOCK) return false;

            return true;
        }
    }
}