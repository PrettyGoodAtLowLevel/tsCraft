using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Graphics.Settings;
using OurCraft.Terrain_Generation;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //testing biome sky color interpolation
    public class SkyColorChanger : Component
    {
        public float interpSpeed = 2.0f;

        Vector3 currentSkyLightColor = Vector3.Zero;

        Vector3 currentZenithColor = Vector3.Zero;
        Vector3 currentMidSkyColor = Vector3.Zero;

        Vector3 currentHorizonColor = Vector3.Zero;
        Vector3 currentHorizonHazeColor = Vector3.Zero;

        internal override void Register()
        {
            BaseSystem<SkyColorChanger>.Register(this);
        }

        internal override void Unregister() 
        {
            BaseSystem<SkyColorChanger>.Unregister(this);
        }

        //update sky color based on time
        public override void OnUpdate(ChunkManager world, KeyboardState kb, MouseState ms)
        {
            Biome biome = OverworldGenerator.GetBiome((int)Transform.WorldPosition.X, (int)Transform.WorldPosition.Z);

            Vector3 targetSkylightColor = biome.Sky.skyLightColor;

            Vector3 targetHorizonColor = biome.Sky.horizonColor;
            Vector3 targetHazeColor = biome.Sky.horizonHazeColor;

            Vector3 targetMidSkyColor = biome.Sky.midSkyColor;
            Vector3 targetZenithColor = biome.Sky.zenithColor;
            
            currentSkyLightColor = Vector3.Lerp(currentSkyLightColor, targetSkylightColor, (float)Time.DeltaTime * interpSpeed);

            currentHorizonColor = Vector3.Lerp(currentHorizonColor, targetHorizonColor, (float)Time.DeltaTime * interpSpeed);
            currentHorizonHazeColor = Vector3.Lerp(currentHorizonHazeColor, targetHazeColor, (float)Time.DeltaTime * interpSpeed);

            currentZenithColor = Vector3.Lerp(currentZenithColor, targetZenithColor, (float)Time.DeltaTime * interpSpeed);
            currentMidSkyColor = Vector3.Lerp(currentMidSkyColor, targetMidSkyColor, (float)Time.DeltaTime * interpSpeed);          

            RendererSkySettingsConfig.SetSkyLightColor(currentSkyLightColor);

            RendererSkySettingsConfig.SetZenithColor(currentZenithColor);
            RendererSkySettingsConfig.SetMidSkyColor(currentMidSkyColor);

            RendererSkySettingsConfig.SetHorizonColor(currentHorizonColor);
            RendererSkySettingsConfig.SetHorizonHazeColor(currentHorizonHazeColor);
        }
    }
}