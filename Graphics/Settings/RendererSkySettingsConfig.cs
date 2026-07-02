using OpenTK.Mathematics;

namespace OurCraft.Graphics.Settings
{
    public static class RendererSkySettingsConfig
    {
        public static void SetSkyLightColor(Vector3 color)
        {
            Renderer.Settings.Sky.SkyLightColor = color;

            Renderer.blockShader.Activate();
            Renderer.blockShader.SetVector3("skyColor", color);

            Renderer.transparentBlockShader.Activate();
            Renderer.transparentBlockShader.SetVector3("skyColor", color);
        }

        public static void SetZenithColor(Vector3 color)
        {
            Renderer.Settings.Sky.ZenithColor = color;
            Renderer.skyShader.Activate();
            Renderer.skyShader.SetVector3("zenithColor", color);
        }

        public static void SetMidSkyColor(Vector3 color)
        {
            Renderer.Settings.Sky.MidSkyColor = color;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetVector3("midSkyColor", color);
        }

        public static void SetHorizonColor(Vector3 color)
        {
            Renderer.Settings.Sky.HorizonColor = color;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetVector3("horizonColor", color);
        }

        //horizon haze
        public static void SetHorizonHazeColor(Vector3 color)
        {
            Renderer.Settings.Sky.HorizonHazeColor = color;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetVector3("horizonHazeColor", color);
        }

        public static void SetHorizonHazeStrength(float strength)
        {
            Renderer.Settings.Sky.HorizonHazeStrength = strength;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetFloat("horizonHazeStrength", strength);
        }

        //sun
        public static void SetSunColor(Vector3 color)
        {
            Renderer.Settings.Sky.SunColor = color;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetVector3("sunColor", color);
        }

        public static void SetSunIntensity(float intensity)
        {
            Renderer.Settings.Sky.SunIntensity = intensity;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetFloat("sunIntensity", intensity);
        }

        //sun disc
        public static void SetSunAngularSize(float size)
        {
            Renderer.Settings.Sky.SunAngularSize = size;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetFloat("sunAngularSize", size);
        }

        //sun glow
        public static void SetSunGlowIntensity(float intensity)
        {
            Renderer.Settings.Sky.SunGlowIntensity = intensity;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetFloat("sunGlowIntensity", intensity);
        }

        public static void SetSunGlowFalloff(float falloff)
        {
            Renderer.Settings.Sky.SunGlowFalloff = falloff;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetFloat("sunGlowFalloff", falloff);
        }

        //atmospheric scattering
        public static void SetSunScatterColor(Vector3 color)
        {
            Renderer.Settings.Sky.SunScatterColor = color;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetVector3("sunScatterColor", color);
        }

        public static void SetSunScatterIntensity(float intensity)
        {
            Renderer.Settings.Sky.SunScatterIntensity = intensity;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetFloat("sunScatterIntensity", intensity);
        }

        public static void SetSunScatterFalloff(float falloff)
        {
            Renderer.Settings.Sky.SunScatterFalloff = falloff;

            Renderer.skyShader.Activate();
            Renderer.skyShader.SetFloat("sunScatterFalloff", falloff);
        }
    }
}
