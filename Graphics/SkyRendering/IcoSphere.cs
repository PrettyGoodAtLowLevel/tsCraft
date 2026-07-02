using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OurCraft.Physics;

namespace OurCraft.Graphics.SkyRendering
{
    //code i found online to generate an ico sphere, used for sky rendering
    public class IcoSphere
    {
        public Transform Transform { get; set; } = new();
        public List<Vector3> Vertices { get; private set; }
        public List<uint> Indices { get; private set; }
        private Dictionary<long, int> middlePointCache;

        public SkyVAO vao = new();
        public SkyVBO vbo = new();
        public EBO ebo = new();

        public IcoSphere(float radius = 1f, int subdivisions = 3)
        {
            Vertices = new List<Vector3>();
            Indices = new List<uint>();
            middlePointCache = new Dictionary<long, int>();

            Create(radius, subdivisions);
        }

        public void Upload()
        {
            if (Vertices.Count == 0) return;
            if (Indices.Count == 0) return;

            vao.Delete();
            vbo.Delete();
            ebo.Delete();

            vao.Create();
            vao.Bind();

            int vertCount = Vertices.Count;

            //upload vertices
            vbo.Create();
            vbo.BufferData(Vertices.ToArray());
            ebo.Create();
            ebo.BufferData(Indices.ToArray());

            int stride = Marshal.SizeOf<Vector3>();
            vao.LinkAttrib(vbo, 0, 3, VertexAttribPointerType.Float, false, stride, IntPtr.Zero);

            vao.Unbind();
            vbo.Unbind();
            ebo.Unbind();
        }

        //draw with shader, shader must be active once drawing
        public void Draw()
        {
            GL.Disable(EnableCap.CullFace);
            vao.Bind();

            GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);

            vao.Unbind();
            GL.Enable(EnableCap.CullFace);
        }

        private void Create(float radius, int subdivisions)
        {
            //create 12 vertices of an icosahedron
            float t = (1.0f + MathF.Sqrt(5.0f)) / 2.0f;

            AddVertex(new Vector3(-1, t, 0), radius);
            AddVertex(new Vector3(1, t, 0), radius);
            AddVertex(new Vector3(-1, -t, 0), radius);
            AddVertex(new Vector3(1, -t, 0), radius);

            AddVertex(new Vector3(0, -1, t), radius);
            AddVertex(new Vector3(0, 1, t), radius);
            AddVertex(new Vector3(0, -1, -t), radius);
            AddVertex(new Vector3(0, 1, -t), radius);

            AddVertex(new Vector3(t, 0, -1), radius);
            AddVertex(new Vector3(t, 0, 1), radius);
            AddVertex(new Vector3(-t, 0, -1), radius);
            AddVertex(new Vector3(-t, 0, 1), radius);

            List<Triangle> faces = new List<Triangle>
            {
                new Triangle(0,11,5),
                new Triangle(0,5,1),
                new Triangle(0,1,7),
                new Triangle(0,7,10),
                new Triangle(0,10,11),

                new Triangle(1,5,9),
                new Triangle(5,11,4),
                new Triangle(11,10,2),
                new Triangle(10,7,6),
                new Triangle(7,1,8),

                new Triangle(3,9,4),
                new Triangle(3,4,2),
                new Triangle(3,2,6),
                new Triangle(3,6,8),
                new Triangle(3,8,9),

                new Triangle(4,9,5),
                new Triangle(2,4,11),
                new Triangle(6,2,10),
                new Triangle(8,6,7),
                new Triangle(9,8,1)
            };

            // Subdivide
            for (int i = 0; i < subdivisions; i++)
            {
                List<Triangle> newFaces = new List<Triangle>();

                foreach (Triangle tri in faces)
                {
                    int a = GetMiddlePoint(tri.V1, tri.V2, radius);
                    int b = GetMiddlePoint(tri.V2, tri.V3, radius);
                    int c = GetMiddlePoint(tri.V3, tri.V1, radius);

                    newFaces.Add(new Triangle(tri.V1, a, c));
                    newFaces.Add(new Triangle(tri.V2, b, a));
                    newFaces.Add(new Triangle(tri.V3, c, b));
                    newFaces.Add(new Triangle(a, b, c));
                }

                faces = newFaces;
            }

            foreach (Triangle tri in faces)
            {
                Indices.Add((uint)tri.V1);
                Indices.Add((uint)tri.V2);
                Indices.Add((uint)tri.V3);
            }
        }

        private int AddVertex(Vector3 vertex, float radius)
        {
            vertex = Vector3.Normalize(vertex) * radius;

            int index = Vertices.Count;
            Vertices.Add(vertex);

            return index;
        }

        private int GetMiddlePoint(int p1, int p2, float radius)
        {
            long smaller = Math.Min(p1, p2);
            long greater = Math.Max(p1, p2);

            long key = (smaller << 32) + greater;

            if (middlePointCache.TryGetValue(key, out int value)) return value;

            Vector3 point1 = Vertices[p1];
            Vector3 point2 = Vertices[p2];
            Vector3 middle = (point1 + point2) * 0.5f;

            int index = AddVertex(middle, radius);

            middlePointCache.Add(key, index);

            return index;
        }

        private struct Triangle
        {
            public int V1;
            public int V2;
            public int V3;

            public Triangle(int v1, int v2, int v3)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
            }
        }
    }
}
