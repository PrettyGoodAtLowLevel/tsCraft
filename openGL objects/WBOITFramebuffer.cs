using OpenTK.Graphics.OpenGL4;
using OurCraft.Rendering;

namespace OurCraft.openGL_objects
{
    //fbo dedicated for transparency on gpu
    public class WBOITFBO
    {
        public int FBO { get; private set; }
        public int AccumColorTex { get; private set; }
        public int AccumAlphaTex { get; private set; }
        public int DepthRBO { get; private set; }

        private readonly int width;
        private readonly int height;

        public WBOITFBO(int width, int height)
        {
            this.width = width;
            this.height = height;
            InitFBO();
        }

        private void InitFBO()
        {
            //generate textures
            AccumColorTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, AccumColorTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f,
                width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            AccumAlphaTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, AccumAlphaTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R16f,
                width, height, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            //create depth renderbuffer
            DepthRBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);

            //create FBO
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, AccumColorTex, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, AccumAlphaTex, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthRBO);

            // Must tell OpenGL to draw to both attachments
            GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            //check completeness
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("WBOIT FBO incomplete: " + status);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        //clear accum textures before drawing transparent objects
        public void Clear()
        {
            //clear color accumulation (RGBA)
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });

            //clear alpha accumulation (R)
            GL.ClearBuffer(ClearBuffer.Color, 1, new float[] { 0f, 0f, 0f, 0f });

            //clear depth
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });

            Unbind(0, 0);
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.Viewport(0, 0, width, height);
        }

        public void Unbind(int screenWidth, int screenHeight)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, screenWidth, screenHeight);
        }

        //call this after drawing transparent objects: composite into final framebuffer
        public void Composite(Shader compositeShader, FullscreenQuad quad)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //default framebuffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            compositeShader.Activate();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, AccumColorTex);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, AccumAlphaTex);

            quad.Draw();
        }
    }
}