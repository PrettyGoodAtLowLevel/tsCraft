using OpenTK.Graphics.OpenGL4;

namespace OurCraft.Graphics
{
    //full screen render texture for post processing
    public class FullscreenQuad
    {
        private int quadVAO;

        //initalize screen texture
        public FullscreenQuad()
        {
            InitFullscreenQuad();
        }

        //bind vao and draw quad
        public void Draw()
        {
            GL.BindVertexArray(quadVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        //create basic full quad in screen space coordinates
        private void InitFullscreenQuad()
        {
            float[] quadVertices =
            {
                //positions    //texCoords
                -1f,  1f,      0f, 1f,
                -1f, -1f,      0f, 0f,
                 1f, -1f,      1f, 0f,

                -1f,  1f,      0f, 1f,
                 1f, -1f,      1f, 0f,
                 1f,  1f,      1f, 1f
            };

            quadVAO = GL.GenVertexArray();
            int VBO = GL.GenBuffer();
            GL.BindVertexArray(quadVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        }
    }
}
