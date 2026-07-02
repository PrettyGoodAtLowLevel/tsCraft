namespace OurCraft.Graphics.Settings
{
    public static class RendererQualitySettingsConfig
    {
        public static void SetQuality(GraphicsQuality quality)
        {
            Renderer.Settings.Quality = quality;

            switch (quality)
            {
                case GraphicsQuality.PERFORMANCE:
                    RendererBloomSettingsConfig.SetDownsampleFactor(4);
                    RendererBloomSettingsConfig.SetBlurIterations(2);
                    RendererGodRaysSettingsConfig.SetSamples(32);
                    break;

                case GraphicsQuality.MEDIUM:
                    RendererBloomSettingsConfig.SetDownsampleFactor(2);
                    RendererBloomSettingsConfig.SetBlurIterations(4);
                    RendererGodRaysSettingsConfig.SetSamples(48);
                    break;

                case GraphicsQuality.HIGH:
                    RendererBloomSettingsConfig.SetDownsampleFactor(2);
                    RendererBloomSettingsConfig.SetBlurIterations(6);
                    RendererGodRaysSettingsConfig.SetSamples(64);
                    break;

                case GraphicsQuality.ULTRA:
                    RendererBloomSettingsConfig.SetDownsampleFactor(1);
                    RendererBloomSettingsConfig.SetBlurIterations(8);
                    RendererGodRaysSettingsConfig.SetSamples(128);
                    break;
            }
        }
    }
}
