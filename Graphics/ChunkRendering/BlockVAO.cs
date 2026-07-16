using OpenTK.Graphics.OpenGL4;

namespace OurCraft.Graphics.ChunkRendering
{
    //refrences vertex data and memory layout of vertex data
    public class BlockVAO
    {
        private int ID = 0;
        public BlockVAO() { ID = 0; }

        public void Create() => ID = GL.GenVertexArray();

        public void Delete()
        {
            if (ID != 0)
            {
                GL.DeleteVertexArray(ID);
                ID = 0;
            }
        }

        //float attribute vec2, vec3
        public void LinkAttrib(BlockVBO vbo, int layout, int numComponents, VertexAttribPointerType type, bool normalize, int stride, nint offset)
        {
            vbo.Bind();
            //use the IntPtr overload so offset is interpreted as a byte offset, not a pointer value accidentally.
            GL.VertexAttribPointer(layout, numComponents, type, normalize, stride, offset);
            GL.EnableVertexAttribArray(layout);
            vbo.Unbind();
        }

        //integer attribute, int, byte, short
        public void LinkAttribInt(BlockVBO vbo, int layout, int numComponents, VertexAttribIntegerType type, int stride, nint offset)
        {
            vbo.Bind();
            GL.VertexAttribIPointer(layout, numComponents, type, stride, offset);
            GL.EnableVertexAttribArray(layout);
            vbo.Unbind();
        }

        public void Bind() => GL.BindVertexArray(ID);
        public void Unbind() => GL.BindVertexArray(0);

        public override string ToString()
        {
            return $"ID: {ID}";
        }
    }
}