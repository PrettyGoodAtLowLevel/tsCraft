namespace OurCraft.Graphics.Settings
{
    public static class RendererExposureSettingsConfig
    {
        public static void SetMinExposure(float value)
        {
            Renderer.Settings.Exposure.MinExposure = value;
        }

        public static void SetMaxExposure(float value)
        {
            Renderer.Settings.Exposure.MaxExposure = value;
        }

        public static void SetExposureAdaptationSpeed(float value)
        {
            Renderer.Settings.Exposure.AdaptationSpeed = value;
        }

        public static void SetGamma(float value)
        {
            Renderer.Settings.Exposure.Gamma = value;

            Renderer.postShader.Activate();
            Renderer.postShader.SetFloat("gamma", value);
        }
    }
}
