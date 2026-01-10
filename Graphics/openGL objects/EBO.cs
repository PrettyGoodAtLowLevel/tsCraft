using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace OurCraft
{
    //specifies order of verticies to be drawn in
    public class EBO
    {
        public int ID { get; private set; }

        //methods
        public EBO() { ID = 0; }

        //create ebo with list of vertex order
        public void Create()
        {
            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
        }

        public void BufferData(uint[] data)
        {
            int sizeInBytes = Marshal.SizeOf<uint>() * data.Length;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeInBytes, data, BufferUsageHint.StaticDraw);
        }

        //activate current ebo
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
        }

        //deactivate ebo
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        //free up vram
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