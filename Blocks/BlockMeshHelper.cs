using OpenTK.Mathematics;
using OurCraft.Rendering;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks
{
    //contains tons of helpers for building block meshes in a chunk
    public static class BlockMeshHelper
    {
        //const data
        private const int textureSizeInBlocksX = 32; //512 / 16
        private const int textureSizeInBlocksY = 16; //256 / 16

        //face culling things
        public static CubeFaces Opposite(CubeFaces face)
        {
            switch (face)
            {
                case CubeFaces.BOTTOM:
                    return CubeFaces.TOP;
                case CubeFaces.TOP:
                    return CubeFaces.BOTTOM;
                case CubeFaces.FRONT:
                    return CubeFaces.BACK;
                case CubeFaces.BACK:
                    return CubeFaces.FRONT;
                case CubeFaces.RIGHT:
                    return CubeFaces.LEFT;
                case CubeFaces.LEFT:
                    return CubeFaces.RIGHT;
                default:
                    return CubeFaces.BOTTOM;
            }
        }

        //gets the state of each block from face type, then checks if the faces should be visible
        public static bool IsFaceVisible(BlockState thisState, BlockState neighborState, CubeFaces face)
        {
            FaceType thisFace = BlockData.GetBlock(thisState.BlockID).blockShape.GetBlockFace(face, thisState);
            FaceType neighborFace = neighborState.GetBlock.blockShape.GetBlockFace(Opposite(face), neighborState);

            return ShouldRenderFace(thisFace, neighborFace);
        }

        //compares the face types against each other
        private static bool ShouldRenderFace(FaceType current, FaceType neighbor)
        {
            //air means no geometry to render
            if (current == FaceType.AIR) return false;

            //if neighbor is air, always show face
            if (neighbor == FaceType.AIR) return true;

            //full block adjacent always hides any partial or full face
            if (current != FaceType.INDENTED && current != FaceType.WATERTOP && neighbor == FaceType.FULLBLOCK) return false;

            //if top water is touching bottom water, then dont show face
            if (current == FaceType.WATERTOP && neighbor == FaceType.WATER ||
            current == FaceType.WATERTOP && neighbor == FaceType.WATERBOTTOM ||
            current == FaceType.WATERBOTTOM && neighbor == FaceType.WATERTOP) return false;

            //same-shaped partial faces (e.g. two bottom slabs) cull each other 
            if (current == neighbor &&
            //(unless they dont take up full cube space or are leaves or are water top)
            current != FaceType.INDENTED && neighbor != FaceType.INDENTED
            && current != FaceType.LEAVES && neighbor != FaceType.LEAVES) return false;

            //otherwise, show face
            return true;
        }

        //-------texture getters----------
        public static float GetTextureX(int textureID)
        {
            int x = textureID % textureSizeInBlocksX;
            return x * NormalizedBlockTextureX();
        }

        public static float GetTextureY(int textureID)
        {
            int y = textureID / textureSizeInBlocksX;
            float texY = 1.0f - (y + 1) * NormalizedBlockTextureY();
            return texY;
        }

        public static float NormalizedBlockTextureX()
        {
            return 1.0f / textureSizeInBlocksX;
        }

        public static float NormalizedBlockTextureY()
        {
            return 1.0f / textureSizeInBlocksY;
        }
    }

    //builds block shapes from cached block model data
    public static class BlockModelMeshBuilder
    {
        //builds geometry from cached block model uses ao helper methods and uv sampling helpers
        public static void BuildFromCachedModel(CachedBlockModel model, Vector3 pos,
        BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisState,
        ChunkMeshData mesh, VoxelAOData aoData)
        {
            //neighbors array indexed by CubeFaces order used across your code
            BlockState[] neighbors = [ bottom, top, front, back, right, left ];

            //iterate cuboids and faces
            foreach (var cuboid in model.Cuboids)
            {
                //cuboid.From/To are in 0 -> 1 (baked)
                Vector3 from = cuboid.From;
                Vector3 to = cuboid.To;

                for (int f = 0; f < 6; f++)
                {
                    CubeFaces faceDir = (CubeFaces)f;
                    CachedFace face = cuboid.Faces[f];
                    if (face == null) continue; //skip if no face

                    //if the face is cullable, check the same visibility rule you already use
                    if (face.Cullable)
                    {
                        //use the existing IsFaceVisible - it expects BlockState this & neighbor + face
                        BlockState neighborState = neighbors[f];
                        if (!BlockMeshHelper.IsFaceVisible(thisState, neighborState, faceDir))
                            continue; //fully culled -> skip
                    }

                    //compute atlas subrect for this cached face UV (cached UV is normalized relative to tile: 0->1)
                    //face.UV = (u0_norm, v0_norm, u1_norm, v1_norm) in tile space
                    float u0 = BlockMeshHelper.GetTextureX(face.TextureID) + face.UV.X * BlockMeshHelper.NormalizedBlockTextureX();
                    float v0 = BlockMeshHelper.GetTextureY(face.TextureID) + face.UV.Y * BlockMeshHelper.NormalizedBlockTextureY();
                    float u1 = BlockMeshHelper.GetTextureX(face.TextureID) + face.UV.Z * BlockMeshHelper.NormalizedBlockTextureX();
                    float v1 = BlockMeshHelper.GetTextureY(face.TextureID) + face.UV.W * BlockMeshHelper.NormalizedBlockTextureY();

                    //compute AO bytes for this face using existing helpers
                    byte[] aoBytes;
                    switch (faceDir)
                    {
                        case CubeFaces.BOTTOM: aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.BottomFaceFromCube(aoData)); break;
                        case CubeFaces.TOP: aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.TopFaceFromCube(aoData)); break;
                        case CubeFaces.FRONT: aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.FrontFaceFromCube(aoData)); break;
                        case CubeFaces.BACK: aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.BackFaceFromCube(aoData)); break;
                        case CubeFaces.RIGHT: aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.RightFaceFromCube(aoData)); break;
                        case CubeFaces.LEFT: aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.LeftFaceFromCube(aoData)); break;
                        default: aoBytes = [ 0, 0, 0, 0 ]; break;
                    }

                    //add the face: this will compute v0->v3 positions per-face and call the AddQuadUV overload.
                    AddModelFace(pos, from, to, faceDir, u0, v0, u1, v1, mesh, aoBytes);
                }
            }
        }

        
        //adds a model face based on cuboid from/to and atlas uv rectangle (u0,v0,u1,v1).
        //uses the same AO corner ordering as blockmeshbuilder AddFullFace/AddSlab methods.        
        private static void AddModelFace(Vector3 blockPos, Vector3 from, Vector3 to, CubeFaces face, float u0, float v0, float u1, float v1,
        ChunkMeshData mesh, byte[] aoBytes)
        {
            //compute per-face corner positions in the same orientation your other methods use
            Vector3 v0p, v1p, v2p, v3p;

            switch (face)
            {
                case CubeFaces.BOTTOM:
                    v0p = new Vector3(from.X, from.Y, from.Z);
                    v1p = new Vector3(to.X, from.Y, from.Z);
                    v2p = new Vector3(to.X, from.Y, to.Z);
                    v3p = new Vector3(from.X, from.Y, to.Z);
                    //addQuad expects ao order: ev[0], ev[1], ev[3], ev[2] in your bottom code — follow that mapping:
                    AddQuadUV(blockPos, v0p, v1p, v2p, v3p, u0, v0, u1, v1, (byte)CubeFaces.BOTTOM, mesh,
                        aoBytes[0], aoBytes[1], aoBytes[3], aoBytes[2]);
                    break;

                case CubeFaces.TOP:
                    v0p = new Vector3(from.X, to.Y, to.Z);
                    v1p = new Vector3(to.X, to.Y, to.Z);
                    v2p = new Vector3(to.X, to.Y, from.Z);
                    v3p = new Vector3(from.X, to.Y, from.Z);
                    AddQuadUV(blockPos, v0p, v1p, v2p, v3p, u0, v0, u1, v1, (byte)CubeFaces.TOP, mesh,
                        aoBytes[2], aoBytes[3], aoBytes[1], aoBytes[0]);
                    break;

                case CubeFaces.FRONT:
                    v0p = new Vector3(from.X, from.Y, to.Z);
                    v1p = new Vector3(to.X, from.Y, to.Z);
                    v2p = new Vector3(to.X, to.Y, to.Z);
                    v3p = new Vector3(from.X, to.Y, to.Z);
                    AddQuadUV(blockPos, v0p, v1p, v2p, v3p, u0, v0, u1, v1, (byte)CubeFaces.FRONT, mesh,
                        aoBytes[0], aoBytes[1], aoBytes[3], aoBytes[2]);
                    break;

                case CubeFaces.BACK:
                    v0p = new Vector3(to.X, from.Y, from.Z);
                    v1p = new Vector3(from.X, from.Y, from.Z);
                    v2p = new Vector3(from.X, to.Y, from.Z);
                    v3p = new Vector3(to.X, to.Y, from.Z);
                    AddQuadUV(blockPos, v0p, v1p, v2p, v3p, u0, v0, u1, v1, (byte)CubeFaces.BACK, mesh,
                        aoBytes[1], aoBytes[0], aoBytes[2], aoBytes[3]);
                    break;

                case CubeFaces.RIGHT:
                    v0p = new Vector3(to.X, from.Y, to.Z);
                    v1p = new Vector3(to.X, from.Y, from.Z);
                    v2p = new Vector3(to.X, to.Y, from.Z);
                    v3p = new Vector3(to.X, to.Y, to.Z);
                    AddQuadUV(blockPos, v0p, v1p, v2p, v3p, u0, v0, u1, v1, (byte)CubeFaces.RIGHT, mesh,
                        aoBytes[0], aoBytes[1], aoBytes[3], aoBytes[2]);
                    break;

                case CubeFaces.LEFT:
                    v0p = new Vector3(from.X, from.Y, from.Z);
                    v1p = new Vector3(from.X, from.Y, to.Z);
                    v2p = new Vector3(from.X, to.Y, to.Z);
                    v3p = new Vector3(from.X, to.Y, from.Z);
                    AddQuadUV(blockPos, v0p, v1p, v2p, v3p, u0, v0, u1, v1, (byte)CubeFaces.LEFT, mesh,
                        aoBytes[1], aoBytes[0], aoBytes[2], aoBytes[3]);
                    break;

                default:
                    return;
            }
        }

        //x shaped blocks are hard to represent in cuboids so this is a sole helper method for adding them
        public static void BuildXShapeBlock(Vector3 pos, int texID, ChunkMeshData mesh)
        {
            float eps = 0.001f; //small inset to avoid z-fighting

            //first diagonal (\)
            AddQuad(pos, new Vector3(0.0f + eps, 0.0f, 0.0f + eps), new Vector3(1.0f - eps, 0.0f, 1.0f - eps),
            new Vector3(1.0f - eps, 1.0f, 1.0f - eps), new Vector3(0.0f + eps, 1.0f, 0.0f + eps),
            texID, (byte)CubeFaces.FRONT.GetHashCode(), mesh);

            AddQuad(pos, new Vector3(1.0f - eps, 0.0f, 1.0f - eps), new Vector3(0.0f + eps, 0.0f, 0.0f + eps),
            new Vector3(0.0f + eps, 1.0f, 0.0f + eps), new Vector3(1.0f - eps, 1.0f, 1.0f - eps),
            texID, (byte)CubeFaces.BACK.GetHashCode(), mesh);

            //second diagonal (/)
            AddQuad(pos, new Vector3(1.0f - eps, 0.0f, 0.0f + eps), new Vector3(0.0f + eps, 0.0f, 1.0f - eps),
            new Vector3(0.0f + eps, 1.0f, 1.0f - eps), new Vector3(1.0f - eps, 1.0f, 0.0f + eps),
            texID, (byte)CubeFaces.RIGHT.GetHashCode(), mesh);

            AddQuad(pos, new Vector3(0.0f + eps, 0.0f, 1.0f - eps), new Vector3(1.0f - eps, 0.0f, 0.0f + eps),
            new Vector3(1.0f - eps, 1.0f, 0.0f + eps), new Vector3(0.0f + eps, 1.0f, 1.0f - eps),
            texID, (byte)CubeFaces.LEFT.GetHashCode(), mesh);
        }

        //add quad from raw vertices
        private static void AddQuad(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int texID, byte normal, ChunkMeshData mesh)
        {
            mesh.AddChunkMeshData(new BlockVertex(pos + v0,
            new Vector2(BlockMeshHelper.GetTextureX(texID), BlockMeshHelper.GetTextureY(texID)), normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v1,
            new Vector2(BlockMeshHelper.GetTextureX(texID) + BlockMeshHelper.NormalizedBlockTextureX(), BlockMeshHelper.GetTextureY(texID)), normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v2,
            new Vector2(BlockMeshHelper.GetTextureX(texID) + BlockMeshHelper.NormalizedBlockTextureX(), BlockMeshHelper.GetTextureY(texID) + BlockMeshHelper.NormalizedBlockTextureY()), normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v3,
            new Vector2(BlockMeshHelper.GetTextureX(texID), BlockMeshHelper.GetTextureY(texID) + BlockMeshHelper.NormalizedBlockTextureY()), normal));
        }

        //addQuad that uses explicit atlas-normalized u/v rectangle (u0,v0,u1,v1).
        //this mirrors existing AddQuad behavior but supports subrects (for partial-UV faces).
        private static void AddQuadUV(Vector3 pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        float u0, float v0, float u1, float v1, byte normal, ChunkMeshData mesh, byte av0, byte av1, byte av2, byte av3)
        {
            mesh.AddChunkMeshData(new BlockVertex(pos + p0, new Vector2(u0, v0), normal, av0));
            mesh.AddChunkMeshData(new BlockVertex(pos + p1, new Vector2(u1, v0), normal, av1));
            mesh.AddChunkMeshData(new BlockVertex(pos + p2, new Vector2(u1, v1), normal, av2));
            mesh.AddChunkMeshData(new BlockVertex(pos + p3, new Vector2(u0, v1), normal, av3));
        }

        //overload without AO params (keeps parity with blockmeshbuilder other AddQuad)
        private static void AddQuadUV(Vector3 pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
            float u0, float v0, float u1, float v1, byte normal, ChunkMeshData mesh)
        {
            mesh.AddChunkMeshData(new BlockVertex(pos + p0, new Vector2(u0, v0), normal));
            mesh.AddChunkMeshData(new BlockVertex(pos + p1, new Vector2(u1, v0), normal));
            mesh.AddChunkMeshData(new BlockVertex(pos + p2, new Vector2(u1, v1), normal));
            mesh.AddChunkMeshData(new BlockVertex(pos + p3, new Vector2(u0, v1), normal));
        }
    }
}
