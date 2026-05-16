using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics.EntityRendering
{
    //standard vertex for 3d entities in voxel game
    public struct Vertex
    {
        public Vector3 position = Vector3.Zero;
        public Vector2 uv = Vector2.Zero;

        public Vertex(Vector3 position, Vector2 uv)
        {
            this.position = position;
            this.uv = uv;
        }

        public static int GetSize()
        {
            return Marshal.SizeOf<Vertex>();
        }
    }

    //holds vertex data for openGL but for entity rendering
    public class EntityVBO
    {
        public int ID { get; private set; }

        public EntityVBO() { ID = 0; }

        //uploads vertex data
        public void Create()
        {
            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        }

        public void Delete()
        {
            if (ID != 0)
            {
                GL.DeleteBuffer(ID);
                ID = 0;
            }
        }

        public void BufferData(Vertex[] data)
        {
            int sizeInBytes = Marshal.SizeOf<Vertex>() * data.Length;

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeInBytes, data, BufferUsageHint.StaticDraw);
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public override string ToString()
        {
            return $"ID: {ID}";
        }
    }
}
