using OpenTK.Graphics.OpenGL4;

namespace OurCraft.openGL_objects
{
    //render texture used for post processing
    public class FBO
    {
        //gl properties
        public int ID { get; private set; }
        public int ColorTexture { get; private set; }
        public int DepthRBO { get; private set; }

        private readonly int width, height;

        //creates a new fbo object
        public FBO(int width, int height, bool withDepth = true)
        {
            this.width = width;
            this.height = height;

            //gen framebuffer and bind
            ID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            //create framebuffer texture
            ColorTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            //have texture bounds set correctly
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //send framebuffer to openGL
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                    TextureTarget.Texture2D, ColorTexture, 0);

            //enable depth writes for the frame buffer to the render buffer
            if (withDepth)
            {
                DepthRBO = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRBO);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                                           RenderbufferTarget.Renderbuffer, DepthRBO);
            }

            //error checking
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete: " + status);

            //unbind
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        //set as active framebuffer object
        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
            GL.Viewport(0, 0, width, height);
        }

        //deactivate current frame buffer object
        public void Unbind(int screenWidth, int screenHeight)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, screenWidth, screenHeight);
        }

        //delete frame buffer when out of scope
        public void Delete()
        {
            GL.DeleteFramebuffer(ID);
            GL.DeleteTexture(ColorTexture);
            if (DepthRBO != 0) GL.DeleteRenderbuffer(DepthRBO);
        }
    }
}
