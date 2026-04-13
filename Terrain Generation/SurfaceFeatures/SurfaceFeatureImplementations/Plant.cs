using OpenTK.Mathematics;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Terrain_Generation.SurfaceFeatures.SurfaceFeatureImplementations
{
    //one block surface feature
    public class Plant : SurfaceFeature
    {
        public BlockState PlantBlock { get; set; }
        public Plant() { }

        //just check if the bottom block is eligible
        public override bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            BlockState below = chunk.GetBlockUnsafe(startPos.X, startPos.Y - 1, startPos.Z);
            return below == PlaceOn || below == AltPlaceOn;
        }

        //simply replace starting block with plant block
        public override void PlaceFeature(Vector3i startPos, Chunk chunk)
        {
            chunk.SetBlockUnsafe(startPos.X, startPos.Y, startPos.Z, PlantBlock);
        }
    }
}