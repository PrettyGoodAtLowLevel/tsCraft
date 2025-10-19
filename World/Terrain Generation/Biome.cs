using OurCraft.Blocks;

namespace OurCraft.World.Terrain_Generation
{
    //represents a section of the worlds vegetation and temperature
    public class Biome
    {
        //the name of the biome in data
        public string Name { get; set; } = "new biome";

        //the temperature and humidity of the biome for the biome noise
        public int TempIndex { get; set; } = 0;
        public int HumidIndex { get; set; } = 0;
        public int VegetationIndex { get; set; } = 0;

        //height data
        public int RegularHeight { get; set; } = 130;
        public int ShoreHeight { get; set; } = 127;
        public int PeakHeight { get; set; } = 200;
        public int OceanHeight { get; set; } = 110;

        //block data
        public ushort WaterBlock { get; set; } = BlockIDs.WATER_BLOCK;
        public ushort WaterSurfaceBlock { get; set; } = BlockIDs.WATER_BLOCK;
        public ushort SurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
        public ushort SubSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;

        public ushort PeakSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
        public ushort PeakSubSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;

        public ushort ShoreSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
        public ushort ShoreSubSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;

        public ushort OceanSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
        public ushort OceanSubSurfaceBlock { get; set; } = BlockIDs.STONE_BLOCK;
    }
}
