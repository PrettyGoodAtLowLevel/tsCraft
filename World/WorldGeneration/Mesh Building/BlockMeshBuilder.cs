using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Meshing;
using OurCraft.Graphics.ChunkRendering;
using OurCraft.World.WorldData;
using OurCraft.World.WorldGeneration.Voxel_Lighting;

namespace OurCraft.World.WorldGeneration.Mesh_Building
{
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
                bool sway = cuboid.sway;
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
                    byte flags = sway == true ? (byte)AnimationType.Leaves : (byte)AnimationType.None;
                    ushort texID = (ushort)face.TextureID;
                    //add the face: this will compute v0->v3 positions per-face and call the AddQuadUV overload.
                    AddModelFace(pos, cuboid, faceDir, face.UV.X, face.UV.Y, face.UV.Z, face.UV.W, mesh, sampleInner, nc, flags, texID);
                }
            }
        }

        //adds a model face based on cuboid from/to and atlas uv rectangle (u0,v0,u1,v1).
        //uses the same AO corner ordering as blockmeshbuilder AddFullFace/AddSlab methods.        
        private static void AddModelFace(Vector3 blockPos, CachedCuboid cuboid, CubeFaces face, float u0, float v0, float u1, float v1,
        ChunkMeshData mesh, bool sampleInner, ChunkSectionNeighbors nc, byte flags, ushort texID)
        {
            CachedCuboid c = cuboid;

            switch (face)
            {
                case CubeFaces.BOTTOM:
                    AddQuadUV(blockPos, c.bv0p, c.bv1p, c.bv2p, c.bv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc, flags, texID);
                    break;

                case CubeFaces.TOP:
                    AddQuadUV(blockPos, c.tv0p, c.tv1p, c.tv2p, c.tv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc, flags, texID);
                    break;

                case CubeFaces.FRONT:
                    AddQuadUV(blockPos, c.fv0p, c.fv1p, c.fv2p, c.fv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc, flags, texID);
                    break;

                case CubeFaces.BACK:
                    AddQuadUV(blockPos, c.bcv0p, c.bcv1p, c.bcv2p, c.bcv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc, flags, texID);
                    break;

                case CubeFaces.RIGHT:
                    AddQuadUV(blockPos, c.rv0p, c.rv1p, c.rv2p, c.rv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc, flags, texID);
                    break;

                case CubeFaces.LEFT:
                    AddQuadUV(blockPos, c.lv0p, c.lv1p, c.lv2p, c.lv3p, u0, v0, u1, v1, mesh, sampleInner, face, nc, flags, texID);
                    break;
            }
        }

        //addQuad that uses explicit atlas-normalized u/v rectangle (u0,v0,u1,v1).
        private static void AddQuadUV(Vector3 pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        float u0, float v0, float u1, float v1, ChunkMeshData mesh, bool sampleInner, CubeFaces face, ChunkSectionNeighbors nc, byte flags, ushort texID)
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

            bool useDiagonal02 = v0Ao + v2Ao + v0Light + v2Light > v1Ao + v3Ao + v1Light + v3Light;

            //add chunk mesh data
            if (!useDiagonal02)
            {
                mesh.AddChunkMeshData(new BlockVertex(pos + p0, new Vector2(u0, v0), v0Light, v0Ao, flags, (byte)face, texID));
                mesh.AddChunkMeshData(new BlockVertex(pos + p1, new Vector2(u1, v0), v1Light, v1Ao, flags, (byte)face, texID));
                mesh.AddChunkMeshData(new BlockVertex(pos + p2, new Vector2(u1, v1), v2Light, v2Ao, flags, (byte)face, texID));
                mesh.AddChunkMeshData(new BlockVertex(pos + p3, new Vector2(u0, v1), v3Light, v3Ao, flags, (byte)face, texID));
            }
            else
            {
                mesh.AddChunkMeshData(new BlockVertex(pos + p1, new Vector2(u1, v0), v1Light, v1Ao, flags, (byte)face, texID));
                mesh.AddChunkMeshData(new BlockVertex(pos + p2, new Vector2(u1, v1), v2Light, v2Ao, flags, (byte)face, texID));
                mesh.AddChunkMeshData(new BlockVertex(pos + p3, new Vector2(u0, v1), v3Light, v3Ao, flags, (byte)face, texID));
                mesh.AddChunkMeshData(new BlockVertex(pos + p0, new Vector2(u0, v0), v0Light, v0Ao, flags, (byte)face, texID));
            }
        }

        //x shaped blocks are hard to represent in cuboids so this is a sole helper method for adding them
        public static void BuildXGrass(Vector3 pos, ushort texID, ChunkMeshData mesh, ChunkSectionNeighbors nc)
        {
            float eps = 0.001f; //small inset to avoid z-fighting
            ushort light = ChunkNeighborHelpers.GetLightSafe((int)pos.X, (int)pos.Y, (int)pos.Z, 0, 0, 0, nc);

            //first diagonal (\)
            AddGrassQuad(pos, new Vector3(0.0f + eps, 0.0f, 0.0f + eps), new Vector3(1.0f - eps, 0.0f, 1.0f - eps),
            new Vector3(1.0f - eps, 1.0f, 1.0f - eps), new Vector3(0.0f + eps, 1.0f, 0.0f + eps),
            texID, mesh, light);

            AddGrassQuad(pos, new Vector3(1.0f - eps, 0.0f, 1.0f - eps), new Vector3(0.0f + eps, 0.0f, 0.0f + eps),
            new Vector3(0.0f + eps, 1.0f, 0.0f + eps), new Vector3(1.0f - eps, 1.0f, 1.0f - eps),
            texID, mesh, light);

            //second diagonal (/)
            AddGrassQuad(pos, new Vector3(1.0f - eps, 0.0f, 0.0f + eps), new Vector3(0.0f + eps, 0.0f, 1.0f - eps),
            new Vector3(0.0f + eps, 1.0f, 1.0f - eps), new Vector3(1.0f - eps, 1.0f, 0.0f + eps),
            texID, mesh, light);

            AddGrassQuad(pos, new Vector3(0.0f + eps, 0.0f, 1.0f - eps), new Vector3(1.0f - eps, 0.0f, 0.0f + eps),
            new Vector3(1.0f - eps, 1.0f, 0.0f + eps), new Vector3(0.0f + eps, 1.0f, 1.0f - eps),
            texID, mesh, light);
        }

        //add quad from raw vertices
        private static void AddGrassQuad(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, ushort texID, ChunkMeshData mesh, ushort lightValue)
        {
            byte f0 = v0.Y > 0.5f ? (byte)AnimationType.Leaves : (byte)AnimationType.None;
            byte f1 = v1.Y > 0.5f ? (byte)AnimationType.Leaves : (byte)AnimationType.None;
            byte f2 = v2.Y > 0.5f ? (byte)AnimationType.Leaves : (byte)AnimationType.None;
            byte f3 = v3.Y > 0.5f ? (byte)AnimationType.Leaves : (byte)AnimationType.None;

            mesh.AddChunkMeshData(new BlockVertex(pos + v0,
            new Vector2(0, 0), lightValue, ao: 3, f0, (byte)CubeFaces.FRONT, texID));

            mesh.AddChunkMeshData(new BlockVertex(pos + v1,
            new Vector2(1, 0), lightValue, ao: 3, f1, (byte)CubeFaces.FRONT, texID));

            mesh.AddChunkMeshData(new BlockVertex(pos + v2,
            new Vector2(1, 1), lightValue, ao: 3, f2, (byte)CubeFaces.FRONT, texID));

            mesh.AddChunkMeshData(new BlockVertex(pos + v3,
            new Vector2(0, 1), lightValue, ao: 3, f3, (byte)CubeFaces.FRONT, texID));
        }
    }
}