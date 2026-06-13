namespace OurCraft.Terrain_Generation.Noise
{
    //represents a section of noise in the world
    public readonly struct NoiseRegion
    {
        //height offset = base height of terrain
        public readonly float heightOffset;

        //amplification = how much the terrain can vary from base height with 3d noise
        public readonly float amplification;

        //how much 3d noise can displace terrain hard cap
        public readonly int maxDepth;

        //what region of the world you are in
        public readonly Biome biome;

        //how close are caves to surface, higher = more openings on surface
        public readonly float caveAmp;

        public NoiseRegion(float heightOffset, float amplification, Biome biome, int maxDepth, float caveAmp) : this()
        {
            this.heightOffset = heightOffset;
            this.amplification = amplification;
            this.biome = biome;
            this.maxDepth = maxDepth;
            this.caveAmp = caveAmp;
        }
    }
}