using OpenTK.Mathematics;
using OurCraft.World;
using OurCraft.World.ChunkGeneration;
using System.Runtime.CompilerServices;

namespace OurCraft.Utility
{
    //contains custom helper math functions
    public static class VoxelMath
    {
        //math helpers
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothStep(float t)
        {
            //smooth interpolation: 3t^2 - 2t^3
            return t * t * (3f - 2f * t);
        }

        //does modulus, with the size needing to be a power of 2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ModPow2(int value, int size)
        {
            int mask = size - 1;
            return (value & mask);
        }

        //does floor division with the divisor needing to be a power of 2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorDivPow2(int a, int b)
        {
            int div = a / b;
            int rem = a % b;
            if ((rem != 0) && ((rem < 0) != (b < 0)))
                div--;
            return div;
        }

        //quick floor division
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorDiv(int value, int divisor)
        {
            int result = value / divisor;
            if ((value ^ divisor) < 0 && value % divisor != 0) result--;
            return result;
        }

        //pack subchunk local coords into a ushort
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort PackPos32(int x, int y, int z)
        {
            return (ushort)
            ((x & 0x1F) |       //5 bits
            ((y & 0x1F) << 5) | //next 5 bits
            ((z & 0x1F) << 10));//next 5 bits
        }

        //unpacks values from a ushort, mainly for lighting positions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackPos32(ushort packed, ref ushort x, ref ushort y, ref ushort z)
        {
            x = (ushort)(packed & 0x1F);
            y = (ushort)((packed >> 5) & 0x1F);
            z = (ushort)((packed >> 10) & 0x1F);
        }

        //packs a lighting value into a ushort
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort PackLight16(Vector3i light, ushort sky)
        {
            return (ushort)((light.X & 0xF) | ((light.Y & 0xF) << 4) |
            ((light.Z & 0xF) << 8) | ((sky & 0xF) << 12));
        }

        //unpacks a full lighting value
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackLight16(ushort light, ref byte r, ref byte g, ref byte b, ref byte s)
        {
            r = (byte)((light >> 0) & 0xF); g = (byte)((light >> 4) & 0xF);
            b = (byte)((light >> 8) & 0xF); s = (byte)((light >> 12) & 0xF);
        }

        //same thing but for vector3i and only for the block light
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i UnpackLight16Block(ushort light)
        {
            return new Vector3i((light >> 0) & 0xF, (light >> 4) & 0xF, (light >> 8) & 0xF);
        }

        //get skylight from packed light
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte UnpackLight16Sky(ushort light)
        {
            return (byte)((light >> 12) & 0xF);
        }

       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GridIndex(int x, int y, int z)
        {
            const int GRID = ChunkGenerator.INTERP_GRID_DENSITY;
            return x + y * GRID + z * GRID * GRID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GridIndexCave(int x, int y, int z)
        {
            const int GRID = ChunkGenerator.INTERP_GRID_CAVE;
            return x + y * GRID + z * GRID * GRID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SubChunkFlatten(int x, int y, int z)
        {
            return x + SubChunk.SUBCHUNK_SIZE * (y + SubChunk.SUBCHUNK_SIZE * z);
        }

        //allows to index into a chunks block entities with a vector3 coords
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToBlockEntityIndex(int localX, int localY, int localZ)
        {
            return localX + localZ * WorldConstants.CHUNK_WIDTH + localY * (WorldConstants.CHUNK_WIDTH * WorldConstants.CHUNK_WIDTH);
        }

        //allows to get back a vector3 from local chunk index
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i FromBlockEntityIndex(int index)
        {
            const int CHUNK_AREA = (WorldConstants.CHUNK_WIDTH * WorldConstants.CHUNK_WIDTH);
            const int CHUNK_WIDTH = (WorldConstants.CHUNK_WIDTH);

            int y = index / CHUNK_AREA;
            index -= y * CHUNK_AREA;

            int z = index / CHUNK_WIDTH;
            int x = index % CHUNK_WIDTH;

            return new Vector3i(x, y, z);
        }

        //not my code - this thing i found online idk
        public static unsafe float InvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = *(int*)&x;
            i = 0x5f3759df - (i >> 1);
            x = *(float*)&i;
            x = x * (1.5f - xhalf * x * x);
            return x;
        }

        //create hash from deposit cell coordinates
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int HashCellCoords(int cellX, int cellZ)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + cellX;
                hash = hash * 31 + cellZ;

                hash ^= (hash << 13);
                hash ^= (hash >> 17);
                hash ^= (hash << 5);

                return hash;
            }
        }

        //hashing i found online
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float HashToUnitFloat(int seed, int x, int y, int z)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)seed) * 16777619u;
                h = (h ^ (uint)x) * 16777619u;
                h = (h ^ (uint)y) * 16777619u;
                h = (h ^ (uint)z) * 16777619u;

                h ^= h >> 15;
                h *= 2246822519u;
                h ^= h >> 13;
                h *= 3266489917u;
                h ^= h >> 16;

                return (h & 0x00FFFFFFu) / 16777216f;
            }
        }
    }

    //collection of points used for remapping x values to y values
    //does not need to be a linear function
    public readonly struct SplineGraph
    {
        readonly List<SplinePoint> points = [];

        public SplineGraph(List<SplinePoint> points)
        {
            this.points = points;
        }

        //finds two points that x is inbetween and interpolates between points
        public float Evaluate(float x, bool smooth = false)
        {
            if (points == null || points.Count == 0) return 0f;

            //clamp x within spline domain
            if (x <= points[0].x) return points[0].y;
            if (x >= points[^1].x) return points[^1].y;

            //find the interval where x lies
            for (int i = 0; i < points.Count - 1; i++)
            {
                float x0 = points[i].x;
                float y0 = points[i].y;
                float x1 = points[i + 1].x;
                float y1 = points[i + 1].y;

                if (x >= x0 && x <= x1)
                {
                    float t = (x - x0) / (x1 - x0);
                    if (smooth) t = VoxelMath.SmoothStep(t);
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
        public readonly float x;
        public readonly float y;

        public SplinePoint(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
