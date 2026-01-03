using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.openGL_objects;
using OurCraft.World;

namespace OurCraft.Graphics
{
    //does all the drawing
    //draws chunks, ui, entities, particle systems, etc
    public class Renderer
    {        
        //chunk drawing
        private readonly Chunkmanager chunks;
        private readonly Shader shader = new Shader();
        private readonly Camera sceneCamera;
        public float fov = 90;

        //post processing
        private readonly FBO postFBO; 
        private readonly Shader postShader = new Shader(); 
        private readonly FullscreenQuad postProcessingQuad;
        private int screenWidth, screenHeight;

        //atmosphere
        public Vector3 skyColor = Vector3.Zero;

        public Renderer(ref Chunkmanager chunks, ref Camera cam, int width, int height)
        {
            //assign values create shaders
            this.chunks = chunks;
            sceneCamera = cam;
            screenWidth = width;
            screenHeight = height;

            //configure openGL and post processing
            ConfigureOpenGL(width, height);
            postFBO = new FBO(width, height, true);
            postProcessingQuad = new FullscreenQuad();
          
            //load textures get fog working, set shaders   
            ChunkMesh.LoadChunkTextures();
            InitShaders();
        }

        //draws a frame in a current world
        public void RenderSceneFrame(float time, float deltaTime)
        {
            //render all chunks to postFBO
            postFBO.Bind();
            ClearScene();
            DrawRawChunks(time);
            postFBO.Unbind(screenWidth, screenHeight);

            //apply post processing effects
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            postShader.Activate();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, postFBO.ColorTexture);
            postProcessingQuad.Draw();           
        }

        //draws chunks without any post processing
        private void DrawRawChunks(float time)
        {
            //setup
            UpdateCamera();
            ClearScene();
            ChunkMesh.globalBlockTexture.Bind();

            //get visible chunks
            var visibleChunks = GetVisibleChunks();

            //solids
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);

            foreach (var chunk in visibleChunks)
            {
                chunk.Draw(shader, sceneCamera);
            }

            //translucent pass
            //weird depth managing but we only have water so its fine
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);

            foreach (var chunk in visibleChunks)
            {
                chunk.DrawTransparent(shader, sceneCamera);
            }

            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
        }

        //clear color scene and reset depth buffer
        private void ClearScene()
        {
            GL.ClearColor(skyColor.X, skyColor.Y, skyColor.Z, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        //makes sure that fbos work properly on resize
        public void ResizeScene(int width, int height)
        {
            screenHeight = height;
            screenWidth = width;

            postShader.Activate();
            postShader.SetVector2("uResolution", new Vector2(width, height));
        }

        //update camera matrix for shader
        private void UpdateCamera()
        {
            sceneCamera.UpdateMatrix(fov, 0.01f, 3000.0f);
            sceneCamera.SendToShader(shader, "camMatrix");
        }

        //make shaders work properly
        private void InitShaders()
        {
            //create all shaders
            shader.Create("default.vert", "default.frag");
            postShader.Create("Post Processing/fullscreen.vert", "Post Processing/chromatic_ab.frag");
            skyColor = new Vector3(0.5f, 0.6f, 0.7f);

            //-----set up block shaders-----
            shader.Activate();
            shader.SetVector3("skyColor", skyColor);
            shader.SetFloat("fogStart", chunks.RenderDistance * Chunk.CHUNK_WIDTH - 20);
            shader.SetFloat("fogEnd", chunks.RenderDistance * Chunk.CHUNK_WIDTH);
            shader.SetFloat("fogDensity", 0.5f);

            //-----post processing----
            //tweak for weird screen effects
            postShader.Activate();
            postShader.SetInt("sceneTex", 0);
            postShader.SetFloat("caStrength", 0.0f);
            postShader.SetFloat("vignetteStrength", 0.0f);
            postShader.SetFloat("saturation", 1.0f);
            postShader.SetVector3("tintColor", new Vector3(0.0f, 0.0f, 0.0f)); 
            postShader.SetFloat("tintIntensity", 0.0f);
            postShader.SetVector2("uResolution", new Vector2(screenWidth, screenHeight));
            postShader.SetFloat("aaStrength", 0.0f);
            
        }

        //makes the sky dark
        public void ToggleNight()
        {
            skyColor = new Vector3(0.001f, 0.001f, 0.001f);
            shader.Activate();
            shader.SetVector3("skyColor", skyColor);

        }

        //makes the sky bright
        public void ToggleDay()
        {
            skyColor = new Vector3(0.5f, 0.6f, 0.7f);
            shader.Activate();
            shader.SetVector3("skyColor", skyColor);
        }

        //configure openGL properly
        private static void ConfigureOpenGL(int width, int height)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
            GL.Enable(EnableCap.FramebufferSrgb);
            GL.Viewport(0, 0, width, height); 
        }

        //gets all the visible chunks
        private List<Chunk> GetVisibleChunks()
        {
            Vector3 camPos = (Vector3)sceneCamera.Position;

            return chunks.ChunkMap.Values.Where(c =>
            {
                if (c.GetState() != ChunkState.Built) return false;

                //shift chunk bounds into camera-relative space
                Vector3 min = (Vector3)c.ChunkMin - camPos;
                Vector3 max = (Vector3)c.ChunkMax - camPos;

                return Chunk.IsBoxInFrustum(sceneCamera.GetFrustum(), min, max);
            }).ToList();
        }
    }
}
