using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace OurCraft
{
    //base vertex settings
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public Vertex(Vector3 position, Vector2 texUV, byte normal, byte ao = 0)
        {
            this.position = position;
            this.texUV = texUV;
            this.normal = normal;
            this.ao = ao;
        }

        //byte location = 0
        public Vector3 position;

        //byte location = 12
        public Vector2 texUV;

        //byte location = 21
        public byte normal;

        //byte location = 22
        public byte ao;

        //always updates when adding new vertex properties
        public static int GetSize()
        {
            return Marshal.SizeOf<Vertex>();
        }
    }

    //holds vertex data for openGL
    internal class VBO
    {
        private int ID;

        public VBO() { ID = 0; }

        //uploads vertex data
        public void Create(List<Vertex> vertices)
        {
            if (vertices.Count == 0) return;

            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vertex.GetSize(), vertices.ToArray(), BufferUsageHint.StaticDraw);
        }

        public void Update(List<Vertex> vertices)
        {
            if (vertices.Count == 0) return;

            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vertex.GetSize(), vertices.ToArray(), BufferUsageHint.DynamicDraw);
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public void Delete()
        {
            if (ID != 0)
            {
                GL.DeleteBuffer(ID);
                ID = 0;
            }
        }
    }
}