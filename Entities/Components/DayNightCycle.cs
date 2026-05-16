using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Graphics;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //testing day night cycle system
    public class DayNightCycle : Component
    {
        //clock for the full day cycle
        public float cycleTime = 0f;

        //how fast the cycle moves
        public float cycleSpeed = 0.001f;

        //timeline proportions, adds to 1
        public float dayHold = 0.36f;
        public float sunsetDuration = 0.14f;
        public float nightHold = 0.36f;
        public float sunriseDuration = 0.14f;

        //higher = sharper transitions, lower = softer transitions
        public float transitionSharpness = 0.6f;

        public Vector3 dayColor = new Vector3(0.6f, 0.65f, 0.725f);
        public Vector3 nightColor = new Vector3(0.0f, 0.0f, 0.001f);

        internal override void Register()
        {
            BaseSystem<DayNightCycle>.Register(this);
        }

        internal override void Unregister() 
        {
            BaseSystem<DayNightCycle>.Unregister(this);
        }

        //update sky color based on time
        public override void OnUpdate(ChunkManager world, KeyboardState kb, MouseState ms)
        {
            cycleTime += (float)(Time.DeltaTime * cycleSpeed);
            cycleTime -= MathF.Floor(cycleTime);

            float skyBlend = GetSkyBlend(cycleTime);

            Renderer.skyColor = Vector3.Lerp(dayColor, nightColor, skyBlend);
            Renderer.UpdateSkyShader();
        }

        //find blending amount of sky colors based on time of day (fancy math i found online)
        private float GetSkyBlend(float t)
        {
            //normalize the timeline parts so they always form a valid cycle
            float total = dayHold + sunsetDuration + nightHold + sunriseDuration;
            if (total <= 0.0001f) return 0f;

            float day = dayHold / total;
            float sunset = sunsetDuration / total;
            float night = nightHold / total;
            float sunrise = sunriseDuration / total;

            float dayEnd = day;
            float sunsetEnd = dayEnd + sunset;
            float nightEnd = sunsetEnd + night;
            float sunriseEnd = nightEnd + sunrise;

            //full day
            if (t < dayEnd) return 0f;

            //day to Night
            if (t < sunsetEnd)
            {
                float x = (t - dayEnd) / MathF.Max(sunset, 0.0001f);
                return ApplyTransitionShape(x);
            }

            //full night
            if (t < nightEnd) return 1f;

            //night to Day
            if (t < sunriseEnd)
            {
                float x = (t - nightEnd) / MathF.Max(sunrise, 0.0001f);
                return 1f - ApplyTransitionShape(x);
            }

            return 0f;
        }

        //control sharpness of transition
        private float ApplyTransitionShape(float x)
        {
            x = Math.Clamp(x, 0f, 1f);
            float eased = x * x * (3f - 2f * x);

            //extra shaping control
            if (transitionSharpness != 1f) eased = MathF.Pow(eased, transitionSharpness);
            return eased;
        }
    }
}