using OpenTK.Graphics.OpenGL4;

namespace OurCraft.Graphics.DebugRendering
{
    //works as a wrapper for openGL vaos for debug vertices
    public class DebugVAO
    {
        private int ID = 0;
        public DebugVAO() { ID = 0; }

        public void Create() => ID = GL.GenVertexArray();

        //float attribute vec2, vec3
        public void LinkAttrib(DebugVBO vbo, int layout, int numComponents, VertexAttribPointerType type, bool normalize, int stride, IntPtr offset)
        {
            vbo.Bind();
            GL.VertexAttribPointer(layout, numComponents, type, normalize, stride, offset);
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
