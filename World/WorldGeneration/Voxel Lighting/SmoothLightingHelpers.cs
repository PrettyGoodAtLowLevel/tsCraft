using OpenTK.Mathematics;
using OurCraft.Blocks.Meshing;
using OurCraft.World.WorldData;
using OurCraft.World.WorldGeneration.Mesh_Building;

namespace OurCraft.World.WorldGeneration.Voxel_Lighting
{
    //contains tons of helpers for voxel ambient occlusion and smooth lighting
    public static class SmoothLightingHelpers
    {
        //represents a smooth lighting/voxel ao face
        public readonly struct FaceBasis
        {
            public readonly Vector3 Normal;
            public readonly Vector3 U;
            public readonly Vector3 V;

            public FaceBasis(Vector3 normal, Vector3 u, Vector3 v)
            {
                Normal = normal;
                U = u;
                V = v;
            }
        }

        //vertex order
        //p0 = bottom-left
        //p1 = bottom-right
        //p2 = top-right
        //p3 = top-left
        public static FaceBasis GetFaceBasis(CubeFaces face) => face switch
        {
            CubeFaces.RIGHT => new FaceBasis(new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 1, 0)),
            CubeFaces.LEFT => new FaceBasis(new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0)),
            CubeFaces.TOP => new FaceBasis(new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1)),
            CubeFaces.BOTTOM => new FaceBasis(new Vector3(0, -1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1)),
            CubeFaces.FRONT => new FaceBasis(new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0)),
            CubeFaces.BACK => new FaceBasis(new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0)),
            _ => throw new ArgumentOutOfRangeException(nameof(face))
        };

        //for each vertex, returns the 4 2D sample offsets around that vertex on the face plane.
        //these are the 4 values you can sample and average for smooth lighting / AO.
        public static void GetVertexSampleOffsets(CubeFaces face, int vertexIndex, out Vector3 s0, out Vector3 s1, out Vector3 s2, out Vector3 s3)
        {
            var basis = GetFaceBasis(face);

            //vertex order:
            //0 = bottom-left
            //1 = bottom-right
            //2 = top-right
            //3 = top-left
            (int ax0, int ay0, int ax1, int ay1, int ax2, int ay2, int ax3, int ay3) = vertexIndex switch
            {
                0 => (0, 0, -1, 0, 0, -1, -1, -1),
                1 => (0, 0, 1, 0, 0, -1, 1, -1),
                2 => (0, 0, 1, 0, 0, 1, 1, 1),
                3 => (0, 0, -1, 0, 0, 1, -1, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(vertexIndex))
            };

            s0 = basis.U * ax0 + basis.V * ay0;
            s1 = basis.U * ax1 + basis.V * ay1;
            s2 = basis.U * ax2 + basis.V * ay2;
            s3 = basis.U * ax3 + basis.V * ay3;
        }

        //averages 4 packed 4-nibble light values nibble-by-nibble.
        public static ushort AveragePackedLight(ushort a, ushort b, ushort c, ushort d)
        {
            ushort result = 0;

            for (int nibble = 0; nibble < 4; nibble++)
            {
                int shift = nibble * 4;

                int va = (a >> shift) & 0xF;
                int vb = (b >> shift) & 0xF;
                int vc = (c >> shift) & 0xF;
                int vd = (d >> shift) & 0xF;

                int avg = (va + vb + vc + vd + 2) / 4; //+2 for rounding
                result |= (ushort)((avg & 0xF) << shift);
            }

            return result;
        }

        //only for 3 lights variation, not using corner
        public static ushort AveragePackedLight(ushort a, ushort b, ushort c)
        {
            ushort result = 0;

            for (int nibble = 0; nibble < 4; nibble++)
            {
                int shift = nibble * 4;

                int va = (a >> shift) & 0xF;
                int vb = (b >> shift) & 0xF;
                int vc = (c >> shift) & 0xF;

                int avg = (va + vb + vc + 1) / 3;
                result |= (ushort)((avg & 0xF) << shift);
            }

            return result;
        }

        //sample the 4 lights around one vertex and average them.
        public static ushort SampleVertexLight(int x, int y, int z, int offsetX, int offsetY, int offsetZ, CubeFaces face, int vertexIndex, ChunkSectionNeighbors nc, bool sampleInner)
        {
            GetVertexSampleOffsets(face, vertexIndex, out var s0, out var s1, out var s2, out var s3);

            if (!SmoothSidesSolid(x, y, z, offsetX, offsetY, offsetZ, face, vertexIndex, nc))
            {
                ushort l0 = sampleInner ? ChunkNeighborHelpers.GetLightSafe(x, y, z, 0, 0, 0, nc) //sample inner if face is inner
                : ChunkNeighborHelpers.GetLightSafe(x, y, z, offsetX + (int)s0.X, offsetY + (int)s0.Y, offsetZ + (int)s0.Z, nc);
                ushort l1 = ChunkNeighborHelpers.GetLightSafe(x, y, z, offsetX + (int)s1.X, offsetY + (int)s1.Y, offsetZ + (int)s1.Z, nc);
                ushort l2 = ChunkNeighborHelpers.GetLightSafe(x, y, z, offsetX + (int)s2.X, offsetY + (int)s2.Y, offsetZ + (int)s2.Z, nc);
                ushort l3 = ChunkNeighborHelpers.GetLightSafe(x, y, z, offsetX + (int)s3.X, offsetY + (int)s3.Y, offsetZ + (int)s3.Z, nc);

                return AveragePackedLight(l0, l1, l2, l3);
            }
            else
            {
                ushort l0 = sampleInner ? ChunkNeighborHelpers.GetLightSafe(x, y, z, 0, 0, 0, nc)
                : ChunkNeighborHelpers.GetLightSafe(x, y, z, offsetX + (int)s0.X, offsetY + (int)s0.Y, offsetZ + (int)s0.Z, nc);
                ushort l1 = ChunkNeighborHelpers.GetLightSafe(x, y, z, offsetX + (int)s1.X, offsetY + (int)s1.Y, offsetZ + (int)s1.Z, nc);
                ushort l2 = ChunkNeighborHelpers.GetLightSafe(x, y, z, offsetX + (int)s2.X, offsetY + (int)s2.Y, offsetZ + (int)s2.Z, nc);
                return AveragePackedLight(l0, l1, l2);
            }
        }

        //sample 2 sides and corner around a vertex for ambient occlusion
        public static byte SampleAO(int x, int y, int z, int offsetX, int offsetY, int offsetZ, CubeFaces face, int vertexIndex, ChunkSectionNeighbors nc)
        {
            byte solid = 0;
            GetVertexSampleOffsets(face, vertexIndex, out _, out var s1, out var s2, out var s3);

            if (ChunkNeighborHelpers.IsNeighborSolidSafe(x, y, z, offsetX + (int)s1.X, offsetY + (int)s1.Y, offsetZ + (int)s1.Z, nc)) solid++;
            if (ChunkNeighborHelpers.IsNeighborSolidSafe(x, y, z, offsetX + (int)s2.X, offsetY + (int)s2.Y, offsetZ + (int)s2.Z, nc)) solid++;
            if (ChunkNeighborHelpers.IsNeighborSolidSafe(x, y, z, offsetX + (int)s3.X, offsetY + (int)s3.Y, offsetZ + (int)s3.Z, nc)) solid++;

            return solid;
        }

        //checks if sides on vertex are solid for smooth lighting
        public static bool SmoothSidesSolid(int x, int y, int z, int offsetX, int offsetY, int offsetZ, CubeFaces face, int vertexIndex, ChunkSectionNeighbors nc)
        {
            GetVertexSampleOffsets(face, vertexIndex, out _, out var s1, out var s2, out _);

            int solid = 0;
            if (ChunkNeighborHelpers.IsNeighborSolidSafe(x, y, z, offsetX + (int)s1.X, offsetY + (int)s1.Y, offsetZ + (int)s1.Z, nc)) solid++;
            if (ChunkNeighborHelpers.IsNeighborSolidSafe(x, y, z, offsetX + (int)s2.X, offsetY + (int)s2.Y, offsetZ + (int)s2.Z, nc)) solid++;

            return solid >= 2;
        }
    }
}
