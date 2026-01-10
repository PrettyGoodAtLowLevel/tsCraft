using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics.DebugRendering
{
    //tightly packed vertex that only holds position and color data for debug rendering
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DebugVertex
    {
        public DebugVertex(Vector3 position, Vector3 color)
        {
            this.position = position; 
            this.color = color;
        }

        public Vector3 position;
        public Vector3 color;

        public static int GetSize()
        {
            return Marshal.SizeOf<DebugVertex>();
        }
    }

    //allows for a vbo to be used with debug vertices
    public class DebugVBO
    {
        public int ID { get; private set; }

        public DebugVBO() { ID = 0; }

        public void Create()
        {
            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        }

        public void BufferData(DebugVertex[] data)
        {
            int sizeInBytes = Marshal.SizeOf<DebugVertex>() * data.Length;

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeInBytes, data, BufferUsageHint.StaticDraw);
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public void Delete()
        {
            if (ID == 0) return;
            GL.DeleteBuffer(ID);
            ID = 0;          
        }
    }
}
