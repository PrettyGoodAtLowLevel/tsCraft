using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace OurCraft
{
    //specifies order of verticies to be drawn in
    internal class EBO
    {
        public int ID { get; private set; }
        public int capacity { get; private set; }

        //methods
        public EBO() { ID = 0; }

        //create ebo with list of vertex order
        public void CreateEmpty(int sizeInBytes, BufferUsageHint usage = BufferUsageHint.DynamicDraw)
        {
            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeInBytes, IntPtr.Zero, usage);
            capacity = sizeInBytes;
        }

        public void SubData<T>(int offsetInBytes, T[] data) where T : struct
        {
            int sizeInBytes = Marshal.SizeOf<T>() * data.Length;
            if (offsetInBytes + sizeInBytes > capacity)
                throw new InvalidOperationException("Data upload exceeds VBO capacity!");

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, sizeInBytes, data);
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