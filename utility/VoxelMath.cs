using OpenTK.Mathematics;

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

        //does modulus, with the size needing to be a power of 2
        public static int ModPow2(int value, int size)
        {
            int mask = size - 1;
            return (value & mask);
        }

        //does floor division with the divisor needing to be a power of 2
        public static int FloorDivPow2(int a, int b)
        {
            int div = a / b;
            int rem = a % b;
            if ((rem != 0) && ((rem < 0) != (b < 0)))
                div--;
            return div;
        }

        //pack subchunk local coords into a ushort
        public static ushort PackPos32(int x, int y, int z)
        {
            return (ushort)
            ((x & 0x1F) |       //5 bits
            ((y & 0x1F) << 5) | //next 5 bits
            ((z & 0x1F) << 10));//next 5 bits
        }

        //unpacks values from a ushort, mainly for lighting positions
        public static void UnpackPos32(ushort packed, ref ushort x, ref ushort y, ref ushort z)
        {
            x = (ushort)(packed & 0x1F);
            y = (ushort)((packed >> 5) & 0x1F);
            z = (ushort)((packed >> 10) & 0x1F);
        }

        //packs a lighting value into a ushort
        public static ushort PackLight16(Vector3i light, ushort sky)
        {
            return (ushort)((light.X & 0xF) | ((light.Y & 0xF) << 4) |
            ((light.Z & 0xF) << 8) | ((sky & 0xF) << 12));
        }

        //unpacks a full lighting value
        public static void UnpackLight16(ushort light, ref byte r, ref byte g, ref byte b, ref byte s)
        {
            r = (byte)((light >> 0) & 0xF); g = (byte)((light >> 4) & 0xF);
            b = (byte)((light >> 8) & 0xF); s = (byte)((light >> 12) & 0xF);
        }

        //same thing but for vector3i and only for the block light
        public static Vector3i UnpackLight16Block(ushort light)
        {
            return new Vector3i((light >> 0) & 0xF, (light >> 4) & 0xF, (light >> 8) & 0xF);
        }

        //get skylight from packed light
        public static byte UnpackLight16Sky(ushort light)
        {
            return (byte)((light >> 12) & 0xF);
        }
    }

    //represents a collection of spline points
    public readonly struct SplineGraph
    {
        readonly List<SplinePoint> points = [];

        public SplineGraph(List<SplinePoint> points)
        {
            this.points = points;
        }

        public float Evaluate(float noiseValue, bool smooth = false)
        {
            if (points == null || points.Count == 0)
                return 0f;

            //clamp noiseValue within spline domain
            if (noiseValue <= points[0].noiseValue)
                return points[0].height;

            if (noiseValue >= points[^1].noiseValue)
                return points[^1].height;

            //find the interval where noiseValue lies
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
                        t = VoxelMath.SmoothStep(t);
                    return VoxelMath.Lerp(y0, y1, t);
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
