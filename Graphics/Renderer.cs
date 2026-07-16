using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Entities.Components.Rendering;
using OurCraft.Entities.Internal;
using OurCraft.Graphics.ChunkRendering;
using OurCraft.Graphics.OpenGL_Objects;
using OurCraft.Graphics.Settings;
using OurCraft.Graphics.SkyRendering;
using OurCraft.Physics.System;
using OurCraft.Utility;
using OurCraft.World.WorldData;
using OurCraft.World.WorldGeneration;

namespace OurCraft.Graphics
{
    //does all rendering for game in one spot
    public class Renderer
    {
        //render objects
        private readonly CameraRender? sceneCamera;                  //camera for view-projection matrix
        private readonly IcoSphere skySphere;                        //skybox
        public readonly ChunkManager world;                          //all chunks to draw
        public readonly FullscreenQuad postProcessingQuad;           //post processing image layered on everything
        public static readonly GraphicsSettings Settings = new();    //not technically a render object, but has all render settings
        
        //3d shaders
        public static readonly Shader blockShader = new();           //regular chunk shader
        public static readonly Shader transparentBlockShader = new();//transparent chunk shader
        public static readonly Shader debugAABBShader = new();       //renders debug aabbs
        public static readonly Shader entityShader = new();          //renders entities
        public static readonly Shader shadowShader = new();          //(WIP) shadow mapper
        public static readonly Shader skyShader = new();             //draws skybox

        //post processing shaders
        public static readonly Shader oitResolveShader = new();      //resolves transparency texture buffers
        public static readonly Shader postShader = new();            //all post processing effects done here
        public static readonly Shader brightPassShader = new();      //bloom bright pass texture
        public static readonly Shader blurShader = new();            //bloom blur texture, to layer on post texture
        public static readonly Shader godRaysShader = new();         //god rays drawer

        //texture buffers
        public readonly ShadowMapFBO shadowFBO;                      //will be revamped to shadow cascades
        public readonly OitFBO oitFBO;                               //transparency buffer
        public readonly FBO postFBO;                                 //final post effects
        public readonly FBO resolvedSceneFBO;                        //transparency filtered scene
        public readonly FBO godRaysFBO;                              //god ray texture buffer
        public readonly FBO brightPassFBO;                           //bright bloom texture buffer
        public readonly FBO bloomPingFBO;                            //bloom blur buffer 1
        public readonly FBO bloomPongFBO;                            //bloom blur buffer 2

        //state tracking
        public static int screenWidth, screenHeight;
        private float shaderTime = 0.0f;

        //atmosphere
        float currentExposure;
        private float sunVisibility = 0f;    
        public static Vector3 sunDirection = Vector3.Normalize(new Vector3(0.4f, -0.8f, 0.2f));
        private Vector2 sunScreenPos; 

        //loads all openGL objects, get object refrences
        public Renderer(ref ChunkManager world,  int width, int height)
        {
            //assign values create shaders
            this.world = world;
            sceneCamera = CameraRenderSystem.Current;
            screenWidth = width;
            screenHeight = height;

            //configure openGL and post processing
            ConfigureOpenGL(width, height);

            postFBO = new FBO(width, height, true);                   //opaque scene
            resolvedSceneFBO = new FBO(width, height, true);          //resolved scene
            oitFBO = new OitFBO(width, height, postFBO.DepthTexture); //shares scene depth

            skySphere = new IcoSphere(1000f, 4);
            skySphere.Upload();

            brightPassFBO = new FBO(width, height, false);
            bloomPingFBO = new FBO(width, height, false);
            bloomPongFBO = new FBO(width, height, false);
            godRaysFBO = new FBO(width, height, true);
            shadowFBO = new ShadowMapFBO(4096, 4096);

            postProcessingQuad = new FullscreenQuad();
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

        //draws a frame in a current world
        public void RenderSceneFrame()
        {
            if (sceneCamera == null) return;

            UpdateSwayTime();
            UpdateExposure(sceneCamera);
            UpdateGodRays(sceneCamera);

            DrawWorld(sceneCamera);
            ResolveTransparency();

            if (Settings.UseGodRays)BuildGodRays();
            if (Settings.UseBloom)  BuildBloom();
            DrawPostProcesing();
        }

        //updates time values in vertex shaders, for things like wind sway
        public void UpdateSwayTime()
        {
            shaderTime += (float)Time.DeltaTime;

            transparentBlockShader.Activate();
            transparentBlockShader.SetFloat("uTime", shaderTime);

            blockShader.Activate();
            blockShader.SetFloat("uTime", shaderTime);

            shadowShader.Activate();
            shadowShader.SetFloat("uTime", shaderTime);
        }

        //updates exposure value of shader based on player pos skylight
        public void UpdateExposure(CameraRender sceneCamera)
        {
            postShader.Activate();
            if (!Settings.UseExposure)
            {
                postShader.SetFloat("exposure", 1.0f);
                return;
            }

            float skylight = world.GetSkyLight(sceneCamera.Transform.WorldPosition);
            float targetExposure = MathHelper.Lerp(Settings.Exposure.MaxExposure, Settings.Exposure.MinExposure, (skylight / 15f));

            currentExposure = MathHelper.Lerp(currentExposure, targetExposure, (float)Time.DeltaTime * Settings.Exposure.AdaptationSpeed);
            postShader.SetFloat("exposure", currentExposure);
        }

        //updates god rays based on what player is looking at and if sun is blocked
        public void UpdateGodRays(CameraRender sceneCamera)
        {
            sunScreenPos = ComputeSunScreenPosition(sceneCamera, out float screenVisibility);

            bool blocked = PhysicsHelpers.RaycastVoxel(sceneCamera.Transform.WorldPosition, dir: -sunDirection, maxDistance: 1000.0f,
            (x, y, z) => world.GetBlockState(new Vector3(x, y, z)).BlockShape.IsFullOpaqueBlock, out _);
            float occlusionVisibility = blocked ? 0.0f : 1.0f;

            //smooth only the occlusion term
            sunVisibility = MathHelper.Lerp(sunVisibility, occlusionVisibility, (float)Time.DeltaTime * Settings.GodRays.AdaptationSpeed);

            //final visibility
            float finalVisibility = screenVisibility * sunVisibility;
            godRaysShader.Activate();
            godRaysShader.SetFloat("sunVisibility", finalVisibility);
        }

        //draws all chunks, entities, and debug boxes
        public void DrawWorld(CameraRender sceneCamera)
        {
            UpdateCamera(sceneCamera);
            List<Chunk> chunks = GetChunks(sceneCamera);

            //opaque scene
            postFBO.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DrawSky(sceneCamera);
            DrawDebugBoxes(sceneCamera);
            using (Profiler.Scope("Entity Rendering")) DrawEntityMeshes(sceneCamera);
            using (Profiler.Scope("Chunk Rendering Opaque")) DrawRawChunksOpaque(sceneCamera, chunks, blockShader);
            
            //transparency accumulation
            oitFBO.Bind();
            oitFBO.Clear(); //accum = 0, reveal = 1
            using (Profiler.Scope("Chunk Rendering Transparent")) DrawRawChunksTransparent(sceneCamera, chunks);
            oitFBO.Unbind(screenWidth, screenHeight);
        }

        //draws the sky sphere around the camera
        private void DrawSky(CameraRender camera)
        {
            skyShader.Activate();
            skyShader.SetVector3("sunDirection", Vector3.Normalize(sunDirection));

            GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Lequal);

            //draw inside of sphere
            GL.CullFace(TriangleFace.Front);
            skySphere.Draw();
            GL.CullFace(TriangleFace.Back);

            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Less);
        }

        //resolve transparency over opaque scene
        public void ResolveTransparency()
        {
            resolvedSceneFBO.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            oitResolveShader.Activate();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, postFBO.ColorTexture);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, oitFBO.AccumTexture);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, oitFBO.RevealTexture);

            postProcessingQuad.Draw();
            resolvedSceneFBO.Unbind(screenWidth, screenHeight);
        }

        //creates the god rays texture to layer on top of post processing
        private void BuildGodRays()
        {
            //avoid weird stretching, dont draw god rays if sun not visible
            if (sunVisibility <= 0.001f)
            {
                godRaysFBO.Bind();
                GL.ClearColor(0f, 0f, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                godRaysFBO.Unbind(screenWidth, screenHeight);
                return;
            }

            godRaysFBO.Bind();

            GL.Disable(EnableCap.DepthTest);
            godRaysShader.Activate();
            godRaysShader.SetVector2("lightScreenPos", sunScreenPos);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, resolvedSceneFBO.ColorTexture);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, postFBO.DepthTexture);

            postProcessingQuad.Draw();

            GL.Enable(EnableCap.DepthTest);
            godRaysFBO.Unbind(screenWidth, screenHeight);
        }

        //gets the bloom texture ready for post processing
        public void BuildBloom()
        {
            ExtractBrightAreas();
            BlurBloom();
        }

        //draw bloom, chromatic abberation, vignette, etc ontop of resolved scene texture
        public void DrawPostProcesing()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            postShader.Activate();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, resolvedSceneFBO.ColorTexture);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, bloomPongFBO.ColorTexture);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, godRaysFBO.ColorTexture);

            postProcessingQuad.Draw();
        }

        //creates a texture of all the bright spots in our scene
        private void ExtractBrightAreas()
        {
            brightPassFBO.Bind();
            brightPassShader.Activate();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, resolvedSceneFBO.ColorTexture);

            postProcessingQuad.Draw();
            brightPassFBO.Unbind(screenWidth, screenHeight);
        }

        //blurs all of the bright spots found in the brightness pass
        private void BlurBloom()
        {
            bool horizontal = true;
            bool firstPass = true;
            int blurIterations = Settings.Bloom.BlurIterations;

            //two pass blur
            for (int i = 0; i < blurIterations; i++)
            {
                var targetFbo = horizontal ? bloomPingFBO : bloomPongFBO;
                var sourceTex = firstPass ? brightPassFBO.ColorTexture : (horizontal ? bloomPongFBO.ColorTexture : bloomPingFBO.ColorTexture);

                targetFbo.Bind();
                blurShader.Activate();
                blurShader.SetBool("horizontal", horizontal);

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, sourceTex);

                postProcessingQuad.Draw();
                targetFbo.Unbind(screenWidth, screenHeight);

                horizontal = !horizontal;
                if (firstPass) firstPass = false;
            }
        }

        //get all render boxes and draw them
        private static void DrawDebugBoxes(CameraRender sceneCamera)
        {
            var boxes = DebugRenderSystem.AllRenderBoxes;
            debugAABBShader.Activate();
            foreach (var box in boxes) box.mesh.Draw(debugAABBShader, box.Transform, sceneCamera.Transform.WorldPosition + sceneCamera.offset);            
        }

        //get all entity meshes and draw them
        private static void DrawEntityMeshes(CameraRender sceneCamera)
        {
            var models = EntityRenderSystem.AllModels;
            entityShader.Activate();
            foreach (var mod in models) mod.model.Draw(entityShader, sceneCamera.Transform.WorldPosition + sceneCamera.offset);          
        }

        //draw all solid chunks
        private static void DrawRawChunksOpaque(CameraRender sceneCamera, List<Chunk> visibleChunks, Shader shader)
        {
            shader.Activate();

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);

            foreach (var chunk in visibleChunks) ChunkRenderer.DrawSolid(chunk, shader, sceneCamera);
        }

        //draws all the transparent chunks with blending enabled
        private static void DrawRawChunksTransparent(CameraRender sceneCamera, List<Chunk> visibleChunks)
        {
            transparentBlockShader.Activate();

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.Enable(EnableCap.Blend);

            //accumulation buffer: additive
            GL.BlendEquation(0, BlendEquationMode.FuncAdd);
            GL.BlendFunc(0, BlendingFactorSrc.One, BlendingFactorDest.One);

            //revealage buffer: multiply by (1 - alpha)
            GL.BlendEquation(1, BlendEquationMode.FuncAdd);
            GL.BlendFunc(1, BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcColor);

            foreach (var chunk in visibleChunks) ChunkRenderer.DrawTransparent(chunk, transparentBlockShader, sceneCamera);

            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
        }

        //helper to clamp values to edge of screen smoothly
        private static float EdgeFade(float x, float edge)
        {
            if (x < 0f || x > 1f) return 0f;
            if (x < edge) return x / edge;
            if (x > 1f - edge) return (1f - x) / edge;
            return 1f;
        }

        //calculates where the sun is on the screen relative to the camera (uses fancy math OwO)
        private static Vector2 ComputeSunScreenPosition(CameraRender cam, out float visibility)
        {
            Vector3 localSunPos = -Vector3.Normalize(sunDirection) * 1000f;

            Matrix4 view = cam.GetViewMatrix();
            Matrix4 proj = cam.GetProjectionMatrix();
            Vector4 clip = new Vector4(localSunPos, 1.0f);

            clip = Vector4.TransformRow(clip, view);
            clip = Vector4.TransformRow(clip, proj);

            if (clip.W <= 0.0f)
            {
                visibility = 0.0f;
                return new Vector2(-1f, -1f);
            }

            float invW = 1.0f / clip.W;
            Vector2 ndc = new Vector2( clip.X * invW, clip.Y * invW);

            Vector2 uv = ndc * 0.5f + new Vector2(0.5f);
            visibility = EdgeFade(uv.X, 0.15f) * EdgeFade(uv.Y, 0.15f);

            return uv;
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
        private static void UpdateCamera(CameraRender sceneCamera)
        {
            sceneCamera.UpdateMatrix();

            sceneCamera.SendViewProjection(skyShader);
            sceneCamera.SendToShader(blockShader, "camMatrix");
            sceneCamera.SendToShader(transparentBlockShader, "camMatrix");
            sceneCamera.SendToShader(debugAABBShader, "camMatrix");
            sceneCamera.SendToShader(entityShader, "camMatrix");
        }

        //gets all the visible chunks
        private List<Chunk> GetChunks(CameraRender sceneCamera)
        {
            Vector3 camPos = (Vector3)sceneCamera.Transform.WorldPosition;

            return world.ChunkMap.Values.Where(c =>
            {
                if (c.GetState() != ChunkState.Render_Ready) return false;
                if (world.ChunkOutOfRenderDistance(c.ChunkPos)) return false;

                //shift chunk bounds into camera-relative space
                Vector3 min = (Vector3)c.ChunkMin - camPos;
                Vector3 max = (Vector3)c.ChunkMax - camPos;

                return FrustumCulling.IsBoxInFrustum(sceneCamera.GetFrustum(), min, max);
            }).ToList();
        }
    }
}