using OpenTK.Mathematics;

namespace OurCraft.Graphics.Voxel_Lighting
{
    //represents a block light position in the world, kept as struct for better performance
    public struct LightNode
    {
        public int x, y, z;
        public Vector3i light;

        public LightNode(int x, int y, int z, Vector3i light)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.light = light;
        }
    }

    //represents a sky light position in the world, kept as struct for better performance
    public struct SkyLightNode
    {
        public int x, y, z;
        public byte light;

        public SkyLightNode(int x, int y, int z, byte light)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.light = light;
        }
    }

    //represents a block light to be removed from the world
    public struct RemoveLightNode
    {
        public int x, y, z;
        public Vector3i light;

        public RemoveLightNode(int x, int y, int z, Vector3i light)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.light = light;
        }
    }

    //represents a sky light to be removed from the world
    public struct RemoveSkyNode
    {
        public int x, y, z;
        public byte light;

        public RemoveSkyNode(int x, int y, int z, byte light)
        {
            this.x = x; this.y = y; this.z = z; this.light = light;
        }
    }
}