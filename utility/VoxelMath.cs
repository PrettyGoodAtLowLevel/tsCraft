using OpenTK.Mathematics;
using OurCraft.World;

namespace OurCraft.utility
{
    //contains custom helper math functions
    //mainly for terrain gen
    public static class VoxelMath
    {
        //math helpers
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static float SmoothStep(float t)
        {
            //smooth interpolation: 3t^2 - 2t^3
            return t * t * (3f - 2f * t);
        }

        //interpolates spline points for a final height value
        public static float EvaluateSpline(List<SplinePoint> points, float noiseValue, bool smooth = false)
        {
            if (points == null || points.Count == 0)
                return 0f;

            //clamp noiseValue within spline domain
            if (noiseValue <= points[0].noiseValue)
                return points[0].height;

            if (noiseValue >= points[^1].noiseValue)
                return points[^1].height;

            // Find the interval where noiseValue lies
            for (int i = 0; i < points.Count - 1; i++)
            {
                float x0 = points[i].noiseValue;
                float y0 = points[i].height;
                float x1 = points[i + 1].noiseValue;
                float y1 = points[i + 1].height;

                if (noiseValue >= x0 && noiseValue <= x1)
                {
                    float t = (noiseValue - x0) / (x1 - x0);
                    if (smooth)
                        t = SmoothStep(t);
                    return Lerp(y0, y1, t);
                }
            }

            //fallback (should not hit if clamped above)
            return 0f;
        }
    }


    //represents points on a 2d graph
    public readonly struct SplinePoint
    {
        public readonly float noiseValue;
        public readonly float height;

        public SplinePoint(float noiseValue, float height)
        {
            this.noiseValue = noiseValue;
            this.height = height;
        }
    }
}
