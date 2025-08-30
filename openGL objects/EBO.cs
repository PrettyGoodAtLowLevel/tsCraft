using OpenTK.Graphics.OpenGL4;

namespace OurCraft
{
    //specifies order of verticies to be drawn in
    internal class EBO
    {
        private int ID = 0;

        //methods
        public EBO() { ID = 0; }

        //create ebo with list of vertex order
        public void Create(List<uint> indices)
        {
            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);

            // Convert the list to an array for GL.BufferData
            uint[] indexArray = indices.ToArray();

            GL.BufferData(BufferTarget.ElementArrayBuffer, indexArray.Length * sizeof(uint), indexArray, BufferUsageHint.StaticDraw);
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