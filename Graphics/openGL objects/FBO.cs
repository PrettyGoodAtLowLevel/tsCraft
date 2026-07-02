using OpenTK.Graphics.OpenGL4;

namespace OurCraft.openGL_objects
{
    //a texture you can "draw into" and then use post processing on
    public class FBO
    {
        public int ID { get; private set; }
        public int ColorTexture { get; private set; }
        public int DepthTexture { get; private set; }
        public readonly int width, height;

        //creates a framebuffer, optionally with tracking depth of each pixel as well
        public FBO(int width, int height, bool withDepth = true)
        {
            //set proper height
            this.width = width;
            this.height = height;

            //create framebuffer and set as active framebuffer
            ID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            //create color texture
            ColorTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            //setup color texture to be proper and clamp to screen
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //attach color texture to framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorTexture, 0);

            //optional render to depth buffer as well
            if (withDepth)
            {
                //create depth specific texture
                DepthTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

                //setup depth texture to be proper and clamp to screen
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                //attach depth texture to current framebuffer
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthTexture, 0);
            }

            //specify to draw to color buffer
            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });

            //check if framebuffer works
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete) throw new Exception("Framebuffer not complete: " + status);

            //unbind framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        //set active framebuffer to current id
        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
            GL.Viewport(0, 0, width, height);
        }

        //set active framebuffer to none
        public void Unbind(int screenWidth, int screenHeight)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, screenWidth, screenHeight);
        }
    }
}
