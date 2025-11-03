using OpenTK.Mathematics;
using OurCraft.Rendering;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks
{
    //contains tons of helpers for building block meshes in a chunk
    public static class BlockMeshBuilder
    {
        //const data
        private const int textureSizeInBlocksX = 32; //512 / 16
        private const int textureSizeInBlocksY = 16; //256 / 16

        public static void BuildFullBlock(Vector3 pos,
        BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisState,
        int bottomTex, int topTex, int frontTex, int backTex, int rightTex, int leftTex,
        ChunkMeshData mesh, VoxelAOData aoData)
        {
            //bottom (-Y)
            if (IsFaceVisible(thisState, bottom, CubeFaces.BOTTOM))
                AddFullFace(pos, CubeFaces.BOTTOM, bottomTex, mesh, aoData);

            //top (+Y)
            if (IsFaceVisible(thisState, top, CubeFaces.TOP))
                AddFullFace(pos, CubeFaces.TOP, topTex, mesh, aoData);

            // front (+Z)
            if (IsFaceVisible(thisState, front, CubeFaces.FRONT))
                AddFullFace(pos, CubeFaces.FRONT, frontTex, mesh, aoData);

            // back (-Z)
            if (IsFaceVisible(thisState, back, CubeFaces.BACK))
                AddFullFace(pos, CubeFaces.BACK, backTex, mesh, aoData);

            // right (+X)
            if (IsFaceVisible(thisState, right, CubeFaces.RIGHT))
                AddFullFace(pos, CubeFaces.RIGHT, rightTex, mesh, aoData);

            // left (-X)
            if (IsFaceVisible(thisState, left, CubeFaces.LEFT))
                AddFullFace(pos, CubeFaces.LEFT, leftTex, mesh, aoData);
        }

        public static void BuildSlab(Vector3 pos,
        BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisState,
        int bottomTex, int topTex, int frontTex, int backTex, int rightTex, int leftTex,
        ChunkMeshData mesh, bool bottomSlab, VoxelAOData aoData)
        {
            //get the y positions for the slab
            float yMin = bottomSlab ? 0 : 0.5f;
            float yMax = bottomSlab ? 0.5f : 1.0f;

            //bottom (-Y)
            if (IsFaceVisible(thisState, bottom, CubeFaces.BOTTOM))
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.BottomFaceFromCube(aoData));
                AddQuad(pos, new Vector3(1.0f, yMin, 1.0f), new Vector3(0.0f, yMin, 1.0f), new Vector3(0.0f, yMin, 0.0f), new Vector3(1.0f, yMin, 0.0f),
                bottomTex, (byte)CubeFaces.BOTTOM, mesh, aoBytes[3], aoBytes[2], aoBytes[0], aoBytes[1]);
            }

            //top (+Y)
            if (IsFaceVisible(thisState, top, CubeFaces.TOP))
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.TopFaceFromCube(aoData));
                AddQuad(pos, new Vector3(1.0f, yMax, 0.0f), new Vector3(0.0f, yMax, 0.0f), new Vector3(0.0f, yMax, 1.0f), new Vector3(1.0f, yMax, 1.0f),
                topTex, (byte)CubeFaces.TOP, mesh, aoBytes[1], aoBytes[0], aoBytes[2], aoBytes[3]);
            }

            //front (+Z)
            if (IsFaceVisible(thisState, front, CubeFaces.FRONT))
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.FrontFaceFromCube(aoData));
                AddSlabSide(pos, new Vector3(0.0f, yMin, 1.0f), new Vector3(1.0f, yMin, 1.0f), new Vector3(1.0f, yMax, 1.0f), new Vector3(0.0f, yMax, 1.0f),
                frontTex, (byte)CubeFaces.FRONT, mesh, aoBytes[0], aoBytes[1], aoBytes[3], aoBytes[2]);
            }

            //back (-Z)
            if (IsFaceVisible(thisState, back, CubeFaces.BACK))
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.BackFaceFromCube(aoData));
                AddSlabSide(pos, new Vector3(1.0f, yMin, 0.0f), new Vector3(0.0f, yMin, 0.0f), new Vector3(0.0f, yMax, 0.0f), new Vector3(1.0f, yMax, 0.0f),
                backTex, (byte)CubeFaces.BACK, mesh, aoBytes[1], aoBytes[0], aoBytes[2], aoBytes[3]);
            }

            //right (+X)
            if (IsFaceVisible(thisState, right, CubeFaces.RIGHT))
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.RightFaceFromCube(aoData));
                AddSlabSide(pos, new Vector3(1.0f, yMin, 1.0f), new Vector3(1.0f, yMin, 0.0f), new Vector3(1.0f, yMax, 0.0f), new Vector3(1.0f, yMax, 1.0f),
                rightTex, (byte)CubeFaces.RIGHT, mesh, aoBytes[0], aoBytes[1], aoBytes[3], aoBytes[2]);
            }

            //left (-X)
            if (IsFaceVisible(thisState, left, CubeFaces.LEFT))
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.LeftFaceFromCube(aoData));
                AddSlabSide(pos, new Vector3(0.0f, yMin, 0.0f), new Vector3(0.0f, yMin, 1.0f), new Vector3(0.0f, yMax, 1.0f), new Vector3(0.0f, yMax, 0.0f),
                leftTex, (byte)CubeFaces.LEFT, mesh, aoBytes[1], aoBytes[0], aoBytes[2], aoBytes[3]);
            }
        }

        public static void BuildXShapeBlock(Vector3 pos, int texID, ChunkMeshData mesh)
        {
            float eps = 0.001f; //small inset to avoid z-fighting

            //first diagonal (\)
            AddQuad(pos,
                new Vector3(0.0f + eps, 0.0f, 0.0f + eps),
                new Vector3(1.0f - eps, 0.0f, 1.0f - eps),
                new Vector3(1.0f - eps, 1.0f, 1.0f - eps),
                new Vector3(0.0f + eps, 1.0f, 0.0f + eps),
                texID, (byte)CubeFaces.FRONT.GetHashCode(), mesh);

            AddQuad(pos,
                new Vector3(1.0f - eps, 0.0f, 1.0f - eps),
                new Vector3(0.0f + eps, 0.0f, 0.0f + eps),
                new Vector3(0.0f + eps, 1.0f, 0.0f + eps),
                new Vector3(1.0f - eps, 1.0f, 1.0f - eps),
                texID, (byte)CubeFaces.BACK.GetHashCode(), mesh);

            //second diagonal (/)
            AddQuad(pos,
                new Vector3(1.0f - eps, 0.0f, 0.0f + eps),
                new Vector3(0.0f + eps, 0.0f, 1.0f - eps),
                new Vector3(0.0f + eps, 1.0f, 1.0f - eps),
                new Vector3(1.0f - eps, 1.0f, 0.0f + eps),
                texID, (byte)CubeFaces.RIGHT.GetHashCode(), mesh);

            AddQuad(pos,
                new Vector3(0.0f + eps, 0.0f, 1.0f - eps),
                new Vector3(1.0f - eps, 0.0f, 0.0f + eps),
                new Vector3(1.0f - eps, 1.0f, 0.0f + eps),
                new Vector3(0.0f + eps, 1.0f, 1.0f - eps),
                texID, (byte)CubeFaces.LEFT.GetHashCode(), mesh);
        }
        
        private static void AddFullFace(Vector3 pos, CubeFaces faceType, int textureID, ChunkMeshData mesh, VoxelAOData aoData)
        {
            //bottom (-Y)
            if (faceType == CubeFaces.BOTTOM)
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.BottomFaceFromCube(aoData));
                AddQuad(pos, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 1.0f),
                textureID, (byte)CubeFaces.BOTTOM, mesh, aoBytes[0], aoBytes[1], aoBytes[3], aoBytes[2]);
            }

            //top (+Y)
            else if (faceType == CubeFaces.TOP)
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.TopFaceFromCube(aoData));
                AddQuad(pos, new Vector3(0.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f),
                textureID, (byte)CubeFaces.TOP, mesh, aoBytes[2], aoBytes[3], aoBytes[1], aoBytes[0]);
            }

            //front (+Z)
            else if (faceType == CubeFaces.FRONT)
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.FrontFaceFromCube(aoData));
                AddQuad(pos, new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f),  new Vector3(1.0f, 1.0f, 1.0f),  new Vector3(0.0f, 1.0f, 1.0f),
                textureID, (byte)CubeFaces.FRONT, mesh, aoBytes[0], aoBytes[1], aoBytes[3], aoBytes[2]);
            }

            //back (-Z)
            else if (faceType == CubeFaces.BACK)
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.BackFaceFromCube(aoData));
                AddQuad(pos, new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f),
                textureID, (byte)CubeFaces.BACK, mesh, aoBytes[1], aoBytes[0], aoBytes[2], aoBytes[3]);
            }

            //right (+X)
            else if (faceType == CubeFaces.RIGHT)
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.RightFaceFromCube(aoData));
                AddQuad(pos, new Vector3(1.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f),
                textureID, (byte)CubeFaces.RIGHT, mesh, aoBytes[0], aoBytes[1], aoBytes[3], aoBytes[2]);
            }

            //left (-X)
            else if (faceType == CubeFaces.LEFT)
            {
                byte[] aoBytes = VoxelAOHelper.GetAoBytes(VoxelAOHelper.LeftFaceFromCube(aoData));
                AddQuad(pos, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f),
                textureID, (byte)CubeFaces.LEFT, mesh, aoBytes[1], aoBytes[0], aoBytes[2], aoBytes[3]);
            }
        }

        //specifies exact vertex coordinates for adding a quad
        private static void AddQuad(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int texID, byte normal, ChunkMeshData mesh, byte av0, byte av1, byte av2, byte av3)
        {
            mesh.AddChunkMeshData(new BlockVertex(pos + v0,
            new Vector2(GetTextureX(texID), GetTextureY(texID)), normal, av0));

            mesh.AddChunkMeshData(new BlockVertex(pos + v1,
            new Vector2(GetTextureX(texID) + NormalizedBlockTextureX(), GetTextureY(texID)), normal, av1));

            mesh.AddChunkMeshData(new BlockVertex(pos + v2,
            new Vector2(GetTextureX(texID) + NormalizedBlockTextureX(), GetTextureY(texID) + NormalizedBlockTextureY()), normal, av2));

            mesh.AddChunkMeshData(new BlockVertex(pos + v3,
            new Vector2(GetTextureX(texID), GetTextureY(texID) + NormalizedBlockTextureY()), normal, av3));
        }

        private static void AddQuad(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int texID, byte normal, ChunkMeshData mesh)
        {
            mesh.AddChunkMeshData(new BlockVertex(pos + v0,
            new Vector2(GetTextureX(texID), GetTextureY(texID)), normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v1,
            new Vector2(GetTextureX(texID) + NormalizedBlockTextureX(), GetTextureY(texID)), normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v2,
            new Vector2(GetTextureX(texID) + NormalizedBlockTextureX(), GetTextureY(texID) + NormalizedBlockTextureY()), normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v3,
            new Vector2(GetTextureX(texID), GetTextureY(texID) + NormalizedBlockTextureY()), normal));
        }

        private static void AddSlabSide(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int texID, byte normal, ChunkMeshData mesh, byte av0, byte av1, byte av2, byte av3)
        {
            mesh.AddChunkMeshData(new BlockVertex(pos + v0,
            new Vector2(GetTextureX(texID), GetTextureY(texID)), normal, av0));

            mesh.AddChunkMeshData(new BlockVertex(pos + v1,
            new Vector2(GetTextureX(texID) + NormalizedBlockTextureX(), GetTextureY(texID)), normal, av1));

            mesh.AddChunkMeshData(new BlockVertex(pos + v2,
            new Vector2(GetTextureX(texID) + NormalizedBlockTextureX(), GetTextureY(texID) + NormalizedBlockTextureY() / 2), normal, av2));

            mesh.AddChunkMeshData(new BlockVertex(pos + v3,
            new Vector2(GetTextureX(texID), GetTextureY(texID) + NormalizedBlockTextureY() / 2), normal, av3));
        }

        //face culling things
        private static CubeFaces Opposite(CubeFaces face)
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

        private static bool IsFaceVisible(BlockState thisState, BlockState neighborState, CubeFaces face)
        {
            FaceType thisFace = BlockData.GetBlock(thisState.BlockID).blockShape.GetBlockFace(face, thisState);
            FaceType neighborFace = neighborState.GetBlock.blockShape.GetBlockFace(Opposite(face), neighborState);

            return ShouldRenderFace(thisFace, neighborFace);
        }

        private static bool ShouldRenderFace(FaceType current, FaceType neighbor)
        {
            //air means no geometry to render
            if (current == FaceType.AIR) return false;

            //if neighbor is air, always show face
            if (neighbor == FaceType.AIR) return true;

            //full block adjacent always hides any partial or full face
            if (current != FaceType.INDENTED && current != FaceType.WATER_TOP && neighbor == FaceType.FULL) return false;

            //if top water is touching bottom water, then dont show face
            if (current == FaceType.WATER_TOP && neighbor == FaceType.WATER ||
            current == FaceType.WATER_TOP && neighbor == FaceType.WATER_BOTTOM ||
            current == FaceType.WATER_BOTTOM && neighbor == FaceType.WATER_TOP) return false;

            //same-shaped partial faces (e.g. two bottom slabs) cull each other 
            if (current == neighbor &&
            //(unless they dont take up full cube space or are leaves or are water top)
            current != FaceType.INDENTED && neighbor != FaceType.INDENTED
            && current != FaceType.LEAVES && neighbor != FaceType.LEAVES) return false;

            //otherwise, show face
            return true;
        }

        //-------texture getters----------
        private static float GetTextureX(int textureID)
        {
            int x = textureID % textureSizeInBlocksX;
            return x * NormalizedBlockTextureX();
        }

        private static float GetTextureY(int textureID)
        {
            int y = textureID / textureSizeInBlocksX;
            float texY = 1.0f - (y + 1) * NormalizedBlockTextureY();
            return texY;
        }

        private static float NormalizedBlockTextureX()
        {
            return 1.0f / textureSizeInBlocksX;
        }

        private static float NormalizedBlockTextureY()
        {
            return 1.0f / textureSizeInBlocksY;
        }
        //-------------------------------
    }
}
