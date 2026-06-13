using OpenTK.Mathematics;
using OurCraft.Graphics;
using OurCraft.Graphics.Voxel_Lighting;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.World;
using OurCraft.World.ChunkGeneration;

namespace OurCraft.Blocks
{
    //contains tons of helpers for building block meshes in a chunk
    public static class BlockMeshHelper
    {
        private const int TEX_SIZE_IN_BLOCKS_X = RenderingConstants.BLOCK_TEXTURE_WIDTH; //512 / 16
        private const int TEX_SIZE_IN_BLOCKS_Y = RenderingConstants.BLOCK_TEXTURE_HEIGHT; //256 / 16

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
            current != FaceType.INDENTED && neighbor != FaceType.INDENTED) return false;

            //otherwise, show face
            return true;
        }

        //get x coord of texture based on texture id index
        public static float GetTextureX(int textureID)
        {
            int x = textureID % TEX_SIZE_IN_BLOCKS_X;
            return x * NormalizedBlockTextureX();
        }

        //get y coord of texture based on texture id index
        public static float GetTextureY(int textureID)
        {
            int y = textureID / TEX_SIZE_IN_BLOCKS_X;
            float texY = 1.0f - (y + 1) * NormalizedBlockTextureY();
            return texY;
        }

        //converts voxel texture to openGL texture on x axis
        public static float NormalizedBlockTextureX()
        {
            return 1.0f / TEX_SIZE_IN_BLOCKS_X;
        }

        //converts voxel texture to openGL texture on y axis
        public static float NormalizedBlockTextureY()
        {
            return 1.0f / TEX_SIZE_IN_BLOCKS_Y;
        }
    }

    //builds block shapes from cached block model data
    public static class BlockMeshBuilder
    {
        //builds geometry from cached block model uses ao helper methods and uv sampling helpers
        public static void BuildFromCachedModel(CachedBlockModel model, Vector3 pos, NeighborBlocks nb,
        ChunkMeshData mesh, ChunkSectionNeighbors nc)
        {
            //neighbors array indexed by CubeFaces order used across your code
            BlockState[] neighbors = [nb.bottom, nb.top, nb.front, nb.back, nb.right, nb.left];

            //iterate cuboids and faces
            foreach (var cuboid in model.Cuboids)
            {
                for (int f = 0; f < 6; f++)
                {
                    CubeFaces faceDir = (CubeFaces)f;
                    CachedFace face = cuboid.Faces[f];
                    
                    //if the face is cullable, check visibility rule
                    if (face.Cullable)
                    {
                        //check if block is visible or not
                        BlockState neighborState = neighbors[f];
                        if (!BlockMeshHelper.IsFaceVisible(nb.thisState, neighborState, faceDir)) continue;
                    }
                    bool sampleInner = !face.Cullable;                
                    //add the face: this will compute v0->v3 positions per-face and call the AddQuadUV overload.
                    AddModelFace(pos, cuboid, faceDir, face.UV.X, face.UV.Y, face.UV.Z, face.UV.W, mesh, sampleInner, nc);
                }
            }
        }
        
        //adds a model face based on cuboid from/to and atlas uv rectangle (u0,v0,u1,v1).
        //uses the same AO corner ordering as blockmeshbuilder AddFullFace/AddSlab methods.        
        private static void AddModelFace(Vector3 blockPos, CachedCuboid cuboid, CubeFaces face, float u0, float v0, float u1, float v1,
        ChunkMeshData mesh, bool sampleInner, ChunkSectionNeighbors nc)
        {
            CachedCuboid c = cuboid;

            switch (face)
            {
                case CubeFaces.BOTTOM:
                    AddQuadUV(blockPos, c.bv0p, c.bv1p, c.bv2p, c.bv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc);
                    break;

                case CubeFaces.TOP:
                    AddQuadUV(blockPos, c.tv0p, c.tv1p, c.tv2p, c.tv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc);
                    break;

                case CubeFaces.FRONT:
                    AddQuadUV(blockPos, c.fv0p, c.fv1p, c.fv2p, c.fv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc);
                    break;

                case CubeFaces.BACK:
                    AddQuadUV(blockPos, c.bcv0p, c.bcv1p, c.bcv2p, c.bcv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc);
                    break;

                case CubeFaces.RIGHT:
                    AddQuadUV(blockPos, c.rv0p, c.rv1p, c.rv2p, c.rv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc);
                    break;

                case CubeFaces.LEFT:
                    AddQuadUV(blockPos, c.lv0p, c.lv1p, c.lv2p, c.lv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc);
                    break;
            }       
        }

        //addQuad that uses explicit atlas-normalized u/v rectangle (u0,v0,u1,v1).
        private static void AddQuadUV(Vector3 pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        float u0, float v0, float u1, float v1, ChunkMeshData mesh, bool sampleInner, CubeFaces face, ChunkSectionNeighbors nc)
        {
            //get ao and smooth lighting offsets
            int oX = face == CubeFaces.LEFT ? -1 : face == CubeFaces.RIGHT ? 1 : 0;
            int oY = face == CubeFaces.BOTTOM ? -1 : face == CubeFaces.TOP ? 1 : 0;
            int oZ = face == CubeFaces.BACK ? -1 : face == CubeFaces.FRONT ? 1 : 0;

            //get vertex index order for smooth lighting and ao
            int vi0, vi1, vi2, vi3;
            if (face == CubeFaces.TOP || face == CubeFaces.BOTTOM) { vi0 = 3; vi1 = 2; vi2 = 1; vi3 = 0; }
            else { vi0 = 0; vi1 = 1; vi2 = 2; vi3 = 3; }

            //get smooth lighting
            ushort v0Light = SmoothLightingHelpers.SampleVertexLight((int)pos.X, (int)pos.Y, (int)pos.Z, oX, oY, oZ, face, vi0, nc, sampleInner);
            ushort v1Light = SmoothLightingHelpers.SampleVertexLight((int)pos.X, (int)pos.Y, (int)pos.Z, oX, oY, oZ, face, vi1, nc, sampleInner);
            ushort v2Light = SmoothLightingHelpers.SampleVertexLight((int)pos.X, (int)pos.Y, (int)pos.Z, oX, oY, oZ, face, vi2, nc, sampleInner);
            ushort v3Light = SmoothLightingHelpers.SampleVertexLight((int)pos.X, (int)pos.Y, (int)pos.Z, oX, oY, oZ, face, vi3, nc, sampleInner);

            //get ao
            byte v0Ao = SmoothLightingHelpers.SampleAO((int)pos.X, (int)pos.Y, (int)pos.Z, oX, oY, oZ, face, vi0, nc);
            byte v1Ao = SmoothLightingHelpers.SampleAO((int)pos.X, (int)pos.Y, (int)pos.Z, oX, oY, oZ, face, vi1, nc);
            byte v2Ao = SmoothLightingHelpers.SampleAO((int)pos.X, (int)pos.Y, (int)pos.Z, oX, oY, oZ, face, vi2, nc);
            byte v3Ao = SmoothLightingHelpers.SampleAO((int)pos.X, (int)pos.Y, (int)pos.Z, oX, oY, oZ, face, vi3, nc);

            bool useDiagonal02 = (v0Ao + v2Ao + v0Light + v2Light) > (v1Ao + v3Ao + v1Light + v3Light);

            //add chunk mesh data
            if (!useDiagonal02)
            {
                mesh.AddChunkMeshData(new BlockVertex(pos + p0, new Vector2(u0, v0), v0Light, (byte)face, v0Ao));
                mesh.AddChunkMeshData(new BlockVertex(pos + p1, new Vector2(u1, v0), v1Light, (byte)face, v1Ao));
                mesh.AddChunkMeshData(new BlockVertex(pos + p2, new Vector2(u1, v1), v2Light, (byte)face, v2Ao));
                mesh.AddChunkMeshData(new BlockVertex(pos + p3, new Vector2(u0, v1), v3Light, (byte)face, v3Ao));
            }
            else
            {            
                mesh.AddChunkMeshData(new BlockVertex(pos + p1, new Vector2(u1, v0), v1Light, (byte)face, v1Ao));
                mesh.AddChunkMeshData(new BlockVertex(pos + p2, new Vector2(u1, v1), v2Light, (byte)face, v2Ao));
                mesh.AddChunkMeshData(new BlockVertex(pos + p3, new Vector2(u0, v1), v3Light, (byte)face, v3Ao));
                mesh.AddChunkMeshData(new BlockVertex(pos + p0, new Vector2(u0, v0), v0Light, (byte)face, v0Ao));
            } 
        }

        //x shaped blocks are hard to represent in cuboids so this is a sole helper method for adding them
        public static void BuildXShapeBlock(Vector3 pos, int texID, ChunkMeshData mesh, ChunkSectionNeighbors nc)
        {
            float eps = 0.001f; //small inset to avoid z-fighting
            ushort light = ChunkBuilder.GetLightSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, 0, 0, nc);

            //first diagonal (\)
            AddQuad(pos, new Vector3(0.0f + eps, 0.0f, 0.0f + eps), new Vector3(1.0f - eps, 0.0f, 1.0f - eps),
            new Vector3(1.0f - eps, 1.0f, 1.0f - eps), new Vector3(0.0f + eps, 1.0f, 0.0f + eps),
            texID, mesh, light, (byte)CubeFaces.LEFT);

            AddQuad(pos, new Vector3(1.0f - eps, 0.0f, 1.0f - eps), new Vector3(0.0f + eps, 0.0f, 0.0f + eps),
            new Vector3(0.0f + eps, 1.0f, 0.0f + eps), new Vector3(1.0f - eps, 1.0f, 1.0f - eps),
            texID, mesh, light, (byte)CubeFaces.RIGHT);

            //second diagonal (/)
            AddQuad(pos, new Vector3(1.0f - eps, 0.0f, 0.0f + eps), new Vector3(0.0f + eps, 0.0f, 1.0f - eps),
            new Vector3(0.0f + eps, 1.0f, 1.0f - eps), new Vector3(1.0f - eps, 1.0f, 0.0f + eps),
            texID, mesh, light, (byte)CubeFaces.FRONT);

            AddQuad(pos, new Vector3(0.0f + eps, 0.0f, 1.0f - eps), new Vector3(1.0f - eps, 0.0f, 0.0f + eps),
            new Vector3(1.0f - eps, 1.0f, 0.0f + eps), new Vector3(0.0f + eps, 1.0f, 1.0f - eps),
            texID, mesh, light, (byte)CubeFaces.BACK);
        }

        //add quad from raw vertices
        private static void AddQuad(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int texID, ChunkMeshData mesh, ushort lightValue, byte normal)
        {
            mesh.AddChunkMeshData(new BlockVertex(pos + v0,
            new Vector2(BlockMeshHelper.GetTextureX(texID), BlockMeshHelper.GetTextureY(texID)),
            lightValue, normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v1, 
            new Vector2(BlockMeshHelper.GetTextureX(texID) + BlockMeshHelper.NormalizedBlockTextureX(),
            BlockMeshHelper.GetTextureY(texID)), lightValue, normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v2,
            new Vector2(BlockMeshHelper.GetTextureX(texID) + BlockMeshHelper.NormalizedBlockTextureX(),
            BlockMeshHelper.GetTextureY(texID) + BlockMeshHelper.NormalizedBlockTextureY()),
            lightValue, normal));

            mesh.AddChunkMeshData(new BlockVertex(pos + v3,
            new Vector2(BlockMeshHelper.GetTextureX(texID), BlockMeshHelper.GetTextureY(texID) + BlockMeshHelper.NormalizedBlockTextureY()),
            lightValue, normal));
        }
    }
}