using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Entities;
using OurCraft.Entities.Components;
using OurCraft.openGL_objects;
using OurCraft.World;
using OurCraft.World.Helpers;

namespace OurCraft.Graphics
{
    //does all the drawing
    //draws chunks, ui, entities, particle systems, etc
    public class Renderer
    {        
        //chunk drawing
        private readonly ChunkManager chunks;
        private readonly Shader chunkShader = new Shader();
        private readonly Shader debugShader = new Shader();
        private readonly CameraRender? sceneCamera;

        //post processing
        private readonly FBO postFBO; 
        private readonly Shader postShader = new Shader(); 
        private readonly FullscreenQuad postProcessingQuad;
        private int screenWidth, screenHeight;

        //atmosphere
        private Vector3 skyColor = Vector3.Zero;

        public Renderer(ref ChunkManager chunks,  int width, int height)
        {
            //assign values create shaders
            this.chunks = chunks;
            sceneCamera = CameraRenderSystem.Current;
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
        public void RenderSceneFrame()
        {
            if (sceneCamera == null) return;

            //render all chunks to postFBO
            postFBO.Bind();
            ClearScene();
            UpdateCamera(sceneCamera);
            DrawDebugBoxes(sceneCamera);
            DrawRawChunks(sceneCamera);   
            postFBO.Unbind(screenWidth, screenHeight);

            //apply post processing effects
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            postShader.Activate();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, postFBO.ColorTexture);
            postProcessingQuad.Draw();           
        }

        //get all render boxes and draw them
        private void DrawDebugBoxes(CameraRender sceneCamera)
        {
            var boxes = DebugRenderSystem.AllRenderBoxes;
            debugShader.Activate();
            foreach (var box in boxes)
            {
                box.mesh.Draw(debugShader, box.Transform, sceneCamera.Transform.position);
            }
        }

        //draws chunks without any post processing
        private void DrawRawChunks(CameraRender sceneCamera)
        {
            //setup           
            ChunkMesh.globalBlockTexture.Bind();

            //get visible chunks
            chunkShader.Activate();
            var visibleChunks = GetChunks(sceneCamera);

            //solids
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);

            foreach (var chunk in visibleChunks)
            {
                ChunkRenderer.DrawSolid(chunk, chunkShader, sceneCamera);
            }

            //translucent pass
            //weird depth managing but we only have water so its fine
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);

            foreach (var chunk in visibleChunks)
            {
                ChunkRenderer.DrawTransparent(chunk, chunkShader, sceneCamera);
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

            if (sceneCamera != null)
            {
                sceneCamera.width = width;
                sceneCamera.height = height;
            }
        }

        //update camera matrix for shader
        private void UpdateCamera(CameraRender sceneCamera)
        {
            sceneCamera.UpdateMatrix();
            sceneCamera.SendToShader(chunkShader, "camMatrix");
            sceneCamera.SendToShader(debugShader, "camMatrix");
        }

        //make shaders work properly
        private void InitShaders()
        {
            //create all shaders
            chunkShader.Create("default.vert", "default.frag");
            debugShader.Create("DebugDrawing/Debug.vert", "DebugDrawing/Debug.frag");
            postShader.Create("Post Processing/fullscreen.vert", "Post Processing/chromatic_ab.frag");
            skyColor = new Vector3(0.5f, 0.6f, 0.7f);

            //-----set up block shaders-----
            chunkShader.Activate();
            chunkShader.SetVector3("skyColor", skyColor);
            chunkShader.SetFloat("fogStart", chunks.RenderDistance * Chunk.CHUNK_WIDTH - 20);
            chunkShader.SetFloat("fogEnd", chunks.RenderDistance * Chunk.CHUNK_WIDTH);
            chunkShader.SetFloat("fogDensity", 0.5f);

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
        private List<Chunk> GetChunks(CameraRender sceneCamera)
        {
            Vector3 camPos = (Vector3)sceneCamera.Transform.position;

            return chunks.ChunkMap.Values.Where(c =>
            {
                if (c.GetState() != ChunkState.Built) return false;

                //shift chunk bounds into camera-relative space
                Vector3 min = (Vector3)c.ChunkMin - camPos;
                Vector3 max = (Vector3)c.ChunkMax - camPos;

                return FrustumCulling.IsBoxInFrustum(sceneCamera.GetFrustum(), min, max);
            }).ToList();
        }
    }
}
