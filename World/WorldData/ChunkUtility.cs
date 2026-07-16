//contains a bunch of helpers dealing with chunks
namespace OurCraft.World.WorldData
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
            return one.X == other.X && one.Z == other.Z;
        }

        public static bool operator !=(ChunkCoord one, ChunkCoord other)
        {
            return !(one == other);
        }

        public override readonly string ToString()
        {
            return $"X: {X}, Z: {Z}";
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
        Terrain_Set,
        Structures_Placed,
        Lit,
        Mesh_Built,
        Render_Ready,
        Deleted
    }

    //handles world updates and remeshing
    public enum BlockUpdateFlags
    {
        INTERNAL,
        MESH_ONLY,
        FULL_REBUILD,
    }

    //holds chunk neighbors, initialize once, pass refrence around during whole mesh operation
    public class ChunkSectionNeighbors
    {
        public Chunk center;
        public Chunk? leftC, rightC, frontC, backC;
        public Chunk? c1, c2, c3, c4;

        public ChunkSectionNeighbors(Chunk center)
        {
            this.center = center;
        }
    }
}