using OpenTK.Mathematics;

namespace OurCraft.Graphics.Settings
{
    //controls all lighting and graphics settings
    public class GraphicsSettings
    {
        //master toggles
        public bool UseExposure = true;
        public bool UseBloom = true;
        public bool UseGodRays = true;

        //quality
        public GraphicsQuality Quality = GraphicsQuality.HIGH;

        //sub-settings
        public ExposureSettings Exposure = new();
        public BloomSettings Bloom = new();
        public GodRaySettings GodRays = new();
        public ColorGradingSettings ColorGrading = new();
        public SkySettings Sky = new();
    }

    //auto adjusts some graphics settings 
    public enum GraphicsQuality
    {
        PERFORMANCE,
        MEDIUM,
        HIGH,
        ULTRA,
    }

    //fog and sky color
    public class SkySettings
    {
        public Vector3 SkyLightColor = Vector3.Zero;

        //main gradient colors
        public Vector3 ZenithColor = new Vector3(0.10f, 0.25f, 0.75f);         //straight up
        public Vector3 MidSkyColor = new Vector3(0.30f, 0.55f, 0.95f);         //upper sky
        public Vector3 HorizonColor = new Vector3(0.85f, 0.92f, 1.00f);        //near horizon

        //horizon haze
        public Vector3 HorizonHazeColor = new Vector3(1.0f, 0.9f, 0.75f);
        public float HorizonHazeStrength = 0.6f;

        //sun
        public Vector3 SunColor = new Vector3(0.62f, 0.90f, 0.80f);
        public float SunIntensity = 25.0f;

        //sun disc size
        public float SunAngularSize = 0.995f;

        //large halo around sun
        public float SunGlowIntensity = 1.3f;
        public float SunGlowFalloff = 16.0f;

        //atmospheric scattering around sun
        public Vector3 SunScatterColor = new Vector3(0.95f, 0.915f, 0.91f);
        public float SunScatterIntensity = 0.4f;
        public float SunScatterFalloff = 4.0f;
    }

    //HDR & exposure
    public class ExposureSettings
    {
        public float MinExposure = 0.8f;
        public float MaxExposure = 2.8f;
        public float AdaptationSpeed = 2.0f;

        public float Gamma = 2.2f;
    }

    //bloom
    public class BloomSettings
    {
        public float Intensity = 1.0f;

        //brightness threshold before bloom starts
        public float Threshold = 1.0f;

        //soft transition into bloom
        public float SoftKnee = 1.0f;

        //1 = full res, 2 = half res, etc.
        public int DownsampleFactor = 2;
        public int BlurIterations = 6;
    }

    //god rays
    public class GodRaySettings
    {
        public float Intensity = 0.15f;
        public float AdaptationSpeed = 5.0f;

        public float Density = 0.8f;
        public float Decay = 0.95f;
        public float Weight = 0.5f;
        public float Exposure = 0.6f;

        public int Samples = 64;
        public Vector3 Color = new Vector3(0.8f, 0.7f, 0.6f);
    }

    //post color grading
    public class ColorGradingSettings
    {
        public float Saturation = 1.1f;
        public float Contrast = 1.0f;
        public float Brightness = 1.0f;

        public float ChromaticAberration = 0.015f;
        public float Vignette = 0.5f;

        public Vector3 TintColor = Vector3.Zero;
        public float TintStrength = 0.0f;
    }
}