using OpenTK.Graphics.OpenGL4;

namespace OurCraft
{
    //refrences vertex data and memory layout of vertex data
    public class VAO
    {
        private int ID = 0;
        public VAO() { ID = 0; }

        public void Create() => ID = GL.GenVertexArray();

        //float attribute vec2, vec3
        public void LinkAttrib(VBO vbo, int layout, int numComponents, VertexAttribPointerType type, bool normalize, int stride, IntPtr offset)
        {
            vbo.Bind();
            //use the IntPtr overload so offset is interpreted as a byte offset, not a pointer value accidentally.
            GL.VertexAttribPointer(layout, numComponents, type, normalize, stride, offset);
            GL.EnableVertexAttribArray(layout);
            vbo.Unbind();
        }

        //integer attribute, int, byte, short
        public void LinkAttribInt(VBO vbo, int layout, int numComponents, VertexAttribIntegerType type, int stride, IntPtr offset)
        {
            vbo.Bind();
            GL.VertexAttribIPointer(layout, numComponents, type, stride, offset);
            GL.EnableVertexAttribArray(layout);
            vbo.Unbind();
        }

        public void Bind() => GL.BindVertexArray(ID);
        public void Unbind() => GL.BindVertexArray(0);

        public void Delete()
        {
            if (ID != 0)
            {
                GL.DeleteVertexArray(ID);
                ID = 0;
            }
        }
    }
}