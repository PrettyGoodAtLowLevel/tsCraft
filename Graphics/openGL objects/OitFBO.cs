using OpenTK.Graphics.OpenGL4;

namespace OurCraft.Graphics.OpenGL_Objects
{
    //framebuffer, but specifically used for order independent transparency
    public sealed class OitFBO
    {
        public int ID { get; private set; }
        public int AccumTexture { get; private set; }
        public int RevealTexture { get; private set; }

        private readonly int width, height;

        //creates oit fbo with shared texture
        public OitFBO(int width, int height, int sharedDepthTexture)
        {
            //setup size
            this.width = width;
            this.height = height;

            //create framebuffer
            ID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            //create accumlation texture with proper screen clamping
            AccumTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, AccumTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            
            //create revealage texture with proper screen clamping
            RevealTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, RevealTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R16f, width, height, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //attach all textures to current framebuffer, along with depth texture from other framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, AccumTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, RevealTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, sharedDepthTexture, 0);

            //specifies which color buffers to draw into, this time 2 so needs an array
            DrawBuffersEnum[] bufs = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };
            GL.DrawBuffers(bufs.Length, bufs);

            //check if framebuffer is working
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete) throw new Exception("OIT framebuffer not complete: " + status);

            //unbind current framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        //sets active framebuffer to current id
        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
            GL.Viewport(0, 0, width, height);
        }

        //sets the active framebuffer bound to none
        public void Unbind(int screenWidth, int screenHeight)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, screenWidth, screenHeight);
        }

        //resets the accumlation and revealage buffers
        public void Clear()
        {
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f }); //accum = 0
            GL.ClearBuffer(ClearBuffer.Color, 1, new float[] { 1f, 1f, 1f, 1f }); //reveal = 1
        }
    }
}
