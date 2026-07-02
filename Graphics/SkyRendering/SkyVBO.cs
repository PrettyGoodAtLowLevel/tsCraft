using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics.SkyRendering
{
    public class SkyVBO
    {
        public int ID { get; private set; }

        public SkyVBO() { ID = 0; }

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

        public void BufferData(Vector3[] data)
        {
            int sizeInBytes = Marshal.SizeOf<Vector3>() * data.Length;

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
