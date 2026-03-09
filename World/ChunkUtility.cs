using System;
namespace OurCraft.World
{
    //represents a chunk position
    public struct ChunkCoord : IEquatable<ChunkCoord>
    {
        public int X { get; private set; }
        public int Z { get; private set; }

        public ChunkCoord(int x, int z)
        {
            X = x; Z = z;
        }

        public static bool operator ==(ChunkCoord one, ChunkCoord other)
        {
            return (one.X == other.X && one.Z == other.Z);
        }
        public static bool operator !=(ChunkCoord one, ChunkCoord other)
        {
            return !(one == other);
        }

        //hashing
        public readonly override bool Equals(object? obj)
        {
            return obj is ChunkCoord other && Equals(other);
        }

        public readonly bool Equals(ChunkCoord other)
        {
            return this == other;
        }

        public readonly override int GetHashCode()
        {
            unchecked //allows wraparound on overflow
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Z;
                return hash;
            }
        }
    }

    //represents a chunks generation state
    public enum ChunkState
    {
        Initialized,
        VoxelOnly,
        Meshed,
        Built,
        Deleted
    }
}
