using OpenTK.Mathematics;
using OurCraft.Utility;
using OurCraft.Graphics.OpenGL_Objects;

namespace OurCraft.Graphics.Settings
{   
    //loads shaders, no really
    public static class ShaderLoader
    {
        //main function for loading all shaders, split this into whatever you want
        public static void LoadShaders()
        {
            CreateShaders();

            SetupChunkShader(Renderer.blockShader);
            SetupChunkShader(Renderer.transparentBlockShader);

            InitalizeShaders();
            ConfigurePostProcessShaders();
            SetUpSkyShader();
        }

        //create all shaders
        private static void CreateShaders()
        {
            Renderer.blockShader.Create("BlockRendering/block.vert", "BlockRendering/block.frag");
            Renderer.transparentBlockShader.Create("BlockRendering/block.vert", "BlockRendering/blockOIT.frag");

            Renderer.debugAABBShader.Create("DebugDrawing/DebugAABB.vert", "DebugDrawing/DebugAABB.frag");
            Renderer.entityShader.Create("EntityDrawing/Entity.vert", "EntityDrawing/Entity.frag");

            Renderer.oitResolveShader.Create("Post Processing/fullscreen.vert", "Post Processing/oit_resolve.frag");
            Renderer.postShader.Create("Post Processing/fullscreen.vert", "Post Processing/postFX.frag");

            Renderer.brightPassShader.Create("Post Processing/fullscreen.vert", "Post Processing/bloom.frag");
            Renderer.blurShader.Create("Post Processing/fullscreen.vert", "Post Processing/blur.frag");
            Renderer.godRaysShader.Create("Post Processing/fullscreen.vert", "Post Processing/god_rays.frag");

            Renderer.skyShader.Create("Sky/sky.vert", "Sky/sky.frag");
            Renderer.shadowShader.Create("Shadows/shadow.vert", "Shadows/shadow.frag");
        }

        //sets up textures and important variables in the shaders
        private static void InitalizeShaders()
        {
            //resolve pass
            Renderer.oitResolveShader.Activate();
            Renderer.oitResolveShader.SetInt("opaqueTex", 0);
            Renderer.oitResolveShader.SetInt("oitAccumTex", 1);
            Renderer.oitResolveShader.SetInt("oitRevealTex", 2);

            //post pass
            Renderer.postShader.Activate();
            Renderer.postShader.SetInt("sceneTex", 0);
            Renderer.postShader.SetInt("bloomTex", 1);
            Renderer.postShader.SetInt("godRaysTex", 2);

            Renderer.brightPassShader.Activate();
            Renderer.brightPassShader.SetInt("sceneTex", 0);

            Renderer.blurShader.Activate();
            Renderer.blurShader.SetInt("sceneTex", 1);

            //setup textures
            Renderer.godRaysShader.Activate();
            Renderer.godRaysShader.SetInt("sceneTex", 0);
            Renderer.godRaysShader.SetInt("depthTex", 1);
        }

        //sets up the shader uniforms for post processing
        private static void ConfigurePostProcessShaders()
        {
            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("saturation", Renderer.Settings.ColorGrading.Saturation);
            Renderer.postShader.SetFloat("caStrength", Renderer.Settings.ColorGrading.ChromaticAberration);
            Renderer.postShader.SetFloat("vignetteStrength", Renderer.Settings.ColorGrading.Vignette);
            Renderer.postShader.SetVector3("tintColor", Renderer.Settings.ColorGrading.TintColor);
            Renderer.postShader.SetFloat("tintIntensity", Renderer.Settings.ColorGrading.TintStrength);
            Renderer.postShader.SetFloat("godRaysIntensity", Renderer.Settings.GodRays.Intensity);
            Renderer.postShader.SetFloat("bloomIntensity", Renderer.Settings.Bloom.Intensity);

            Renderer.brightPassShader.Activate();
            Renderer.brightPassShader.SetFloat("threshold", Renderer.Settings.Bloom.Threshold);
            Renderer.brightPassShader.SetFloat("knee", Renderer.Settings.Bloom.SoftKnee);

            Renderer.blurShader.Activate();
            Renderer.blurShader.SetVector2("Resolution", new Vector2(Renderer.screenWidth / Renderer.Settings.Bloom.DownsampleFactor,
            Renderer.screenHeight / Renderer.Settings.Bloom.DownsampleFactor));

            Renderer.godRaysShader.Activate();
            Renderer.godRaysShader.SetFloat("density", Renderer.Settings.GodRays.Density);
            Renderer.godRaysShader.SetFloat("decay", Renderer.Settings.GodRays.Decay);
            Renderer.godRaysShader.SetFloat("exposure", Renderer.Settings.GodRays.Exposure);
            Renderer.godRaysShader.SetFloat("weight", Renderer.Settings.GodRays.Weight);
            Renderer.godRaysShader.SetInt("samples", Renderer.Settings.GodRays.Samples);
            Renderer.godRaysShader.SetVector3("rayColor", Renderer.Settings.GodRays.Color);
        }

        //quick setup for chunk shaders to be used for both solid and transparent shaders
        private static void SetupChunkShader(Shader shader)
        {
            shader.Activate();
            shader.SetVector3("skyColor", Renderer.Settings.Sky.SkyLightColor);
            shader.SetFloat("uChunkSize", WorldConstants.CHUNK_WIDTH);
            shader.SetFloat("uChunkHeight", WorldConstants.CHUNK_HEIGHT);
        }

        //sets up all sky shader uniforms
        private static void SetUpSkyShader()
        {
            Renderer.skyShader.Activate();
            Renderer.skyShader.SetVector3("zenithColor", Renderer.Settings.Sky.ZenithColor);
            Renderer.skyShader.SetVector3("midSkyColor", Renderer.Settings.Sky.MidSkyColor);
            Renderer.skyShader.SetVector3("horizonColor", Renderer.Settings.Sky.HorizonColor);
            Renderer.skyShader.SetVector3("horizonHazeColor", Renderer.Settings.Sky.HorizonHazeColor);
            Renderer.skyShader.SetFloat("horizonHazeStrength", Renderer.Settings.Sky.HorizonHazeStrength);

            Renderer.skyShader.SetVector3("sunDirection", Vector3.Normalize(Renderer.sunDirection));
            Renderer.skyShader.SetVector3("sunColor", Renderer.Settings.Sky.SunColor);
            Renderer.skyShader.SetFloat("sunIntensity", Renderer.Settings.Sky.SunIntensity);
            Renderer.skyShader.SetFloat("sunAngularSize", Renderer.Settings.Sky.SunAngularSize);
            Renderer.skyShader.SetFloat("sunGlowIntensity", Renderer.Settings.Sky.SunGlowIntensity);
            Renderer.skyShader.SetFloat("sunGlowFalloff", Renderer.Settings.Sky.SunGlowFalloff);

            Renderer.skyShader.SetVector3("sunScatterColor", Renderer.Settings.Sky.SunScatterColor);
            Renderer.skyShader.SetFloat("sunScatterIntensity", Renderer.Settings.Sky.SunScatterIntensity);
            Renderer.skyShader.SetFloat("sunScatterFalloff", Renderer.Settings.Sky.SunScatterFalloff);
        }
    }
}
