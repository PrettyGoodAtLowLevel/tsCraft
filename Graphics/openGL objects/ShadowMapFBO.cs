using OpenTK.Graphics.OpenGL4;

namespace OurCraft.Graphics.OpenGL_Objects
{
    //fbo specifically for rendering shadows
    public class ShadowMapFBO
    {
        public int ID { get; private set; }
        public int DepthTexture { get; private set; }

        //shadow map quality
        public readonly int Width;
        public readonly int Height;

        public ShadowMapFBO(int width, int height)
        {
            Width = width;
            Height = height;

            //create framebuffer
            ID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            //create depth texture
            DepthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            //shadow maps almost always use nearest filtering initially
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            //prevent shadow borders from sampling garbage
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            float[] borderColor = { 1f, 1f, 1f, 1f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            //attach as depth attachment only
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthTexture, 0);

            //no color output at all
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            //verify
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete) throw new Exception($"Shadow framebuffer incomplete: {status}");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        //sets active framebuffer
        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
            GL.Viewport(0, 0, Width, Height);
        }

        //sets active framebuffer to default one
        public void Unbind(int screenWidth, int screenHeight)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, screenWidth, screenHeight);
        }
    }
}
