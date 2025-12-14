using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.World.Terrain_Generation.SurfaceFeatures.SurfaceFeatureImplementations
{
    //one block surface feature
    public class Plant : SurfaceFeature
    {
        public ushort BlockID { get; set; }
        public Plant()
        {
        }

        //just check if the bottom block is eligible
        public override bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            BlockState below = chunk.GetBlockUnsafe(startPos.X, startPos.Y - 1, startPos.Z);
            return below.BlockID == PlaceOn || below.BlockID == AltPlaceOn;
        }

        //simply replace starting block with plant block
        public override void PlaceFeature(Vector3i startPos, Chunk chunk)
        {
            int count = NoiseRouter.GetVariation(startPos.X + chunk.Pos.X * SubChunk.SUBCHUNK_SIZE, startPos.Y, startPos.Z + chunk.Pos.Z * SubChunk.SUBCHUNK_SIZE, 5, NoiseRouter.seed, 100);          
            chunk.SetBlockUnsafe(startPos.X, startPos.Y, startPos.Z, new BlockState(BlockID));
        }
    }
}
