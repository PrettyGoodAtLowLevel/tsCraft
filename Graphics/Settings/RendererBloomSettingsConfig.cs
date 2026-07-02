using OpenTK.Mathematics;

namespace OurCraft.Graphics.Settings
{
    public static class RendererBloomSettingsConfig
    {
        public static void SetEnabled(bool enabled)
        {
            Renderer.Settings.UseBloom = enabled;
        }

        public static void SetIntensity(float value)
        {
            Renderer.Settings.Bloom.Intensity = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("bloomIntensity", value);
        }

        public static void SetThreshold(float value)
        {
            Renderer.Settings.Bloom.Threshold = value;

            Renderer.brightPassShader.Activate();
            Renderer.brightPassShader.SetFloat("threshold", value);
        }

        public static void SetSoftKnee(float value)
        {
            Renderer.Settings.Bloom.SoftKnee = value;

            Renderer.brightPassShader.Activate();
            Renderer.brightPassShader.SetFloat("knee", value);
        }

        public static void SetDownsampleFactor(int value)
        {
            Renderer.Settings.Bloom.DownsampleFactor = value;

            Renderer.blurShader.Activate();
            Renderer.blurShader.SetVector2("Resolution", new Vector2( Renderer.screenWidth / value, Renderer.screenHeight / value));
        }

        public static void SetBlurIterations(int value)
        {
            Renderer.Settings.Bloom.BlurIterations = value;
        }
    }
}
