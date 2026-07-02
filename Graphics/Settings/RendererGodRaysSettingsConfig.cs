using OpenTK.Mathematics;

namespace OurCraft.Graphics.Settings
{
    public static class RendererGodRaysSettingsConfig
    {
        public static void SetEnabled(bool enabled)
        {
            Renderer.Settings.UseGodRays = enabled;
        }

        public static void SetIntensity(float value)
        {
            Renderer.Settings.GodRays.Intensity = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("godRaysIntensity", value);
        }

        public static void SetAdaptationSpeed(float value)
        {
            Renderer.Settings.GodRays.AdaptationSpeed = value;
        }

        public static void SetDensity(float value)
        {
            Renderer.Settings.GodRays.Density = value;

            Renderer.godRaysShader.Activate();
            Renderer.godRaysShader.SetFloat("density", value);
        }

        public static void SetDecay(float value)
        {
            Renderer.Settings.GodRays.Decay = value;

            Renderer.godRaysShader.Activate();
            Renderer.godRaysShader.SetFloat("decay", value);
        }

        public static void SetExposure(float value)
        {
            Renderer.Settings.GodRays.Exposure = value;

            Renderer.godRaysShader.Activate();
            Renderer.godRaysShader.SetFloat("exposure", value);
        }

        public static void SetWeight(float value)
        {
            Renderer.Settings.GodRays.Weight = value;

            Renderer.godRaysShader.Activate();
            Renderer.godRaysShader.SetFloat("weight", value);
        }

        public static void SetSamples(int samples)
        {
            Renderer.Settings.GodRays.Samples = samples;

            Renderer.godRaysShader.Activate();
            Renderer.godRaysShader.SetInt("samples", samples);
        }

        public static void SetColor(Vector3 color)
        {
            Renderer.Settings.GodRays.Color = color;

            Renderer.godRaysShader.Activate();
            Renderer.godRaysShader.SetVector3("rayColor", color);
        }
    }
}
