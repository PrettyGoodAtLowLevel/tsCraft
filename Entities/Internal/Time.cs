using OpenTK.Windowing.Common;
using OurCraft.Utility;

namespace OurCraft.Entities.Internal
{
    //contains all time, global state stuff ik but wtv
    public static class Time
    {
        public static double TotalTime { get; private set; } = 0.0;
        public static double DeltaTime { get; private set; } = 0.0;
        public static double UnscaledTotalTime { get; private set; } = 0.0;
        public static double UnscaledDeltaTime { get; private set; } = 0.0;
        public static double TimeScale { get; set; } = 1.0;
        public static double FixedDeltaTime { get; private set; } = PhysicsConstants.PHYSICS_TICK;

        //reset all values back to default
        public static void Reset()
        {
            TotalTime = 0.0;
            DeltaTime = 0.0;

            UnscaledTotalTime = 0.0;
            UnscaledDeltaTime = 0.0;

            TimeScale = 1.0;
            FixedDeltaTime = PhysicsConstants.PHYSICS_TICK;
        }

        //update time fields respectively
        public static void Increment(FrameEventArgs args)
        {
            TotalTime += args.Time * TimeScale;
            DeltaTime = args.Time * TimeScale;

            UnscaledTotalTime += args.Time;
            UnscaledDeltaTime = args.Time;
        }
    }
}
