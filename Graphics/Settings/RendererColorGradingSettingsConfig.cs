using OpenTK.Mathematics;

namespace OurCraft.Graphics.Settings
{
    public static class RendererColorGradingSettingsConfig
    {
        public static void SetSaturation(float value)
        {
            Renderer.Settings.ColorGrading.Saturation = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("saturation", value);
        }

        public static void SetContrast(float value)
        {
            Renderer.Settings.ColorGrading.Contrast = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("contrast", value);
        }

        public static void SetBrightness(float value)
        {
            Renderer.Settings.ColorGrading.Brightness = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("brightness", value);
        }

        public static void SetChromaticAberration(float value)
        {
            Renderer.Settings.ColorGrading.ChromaticAberration = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("caStrength", value);
        }

        public static void SetVignette(float value)
        {
            Renderer.Settings.ColorGrading.Vignette = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("vignetteStrength", value);
        }

        public static void SetTintColor(Vector3 color)
        {
            Renderer.Settings.ColorGrading.TintColor = color;

            Renderer.postShader.Activate();
            Renderer.postShader.SetVector3("tintColor", color);
        }

        public static void SetTintStrength(float value)
        {
            Renderer.Settings.ColorGrading.TintStrength = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("tintIntensity", value);
        }
    }
}
