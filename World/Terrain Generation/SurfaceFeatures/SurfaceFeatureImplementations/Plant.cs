using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.World.Terrain_Generation.SurfaceFeatures.SurfaceFeatureImplementations
{
    //one block surface feature
    public class Plant : SurfaceFeature
    {
        public BlockState PlantBlock { get; set; }
        public Plant()
        {
        }

        //just check if the bottom block is eligible
        public override bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            BlockState below = chunk.GetBlockUnsafe(startPos.X, startPos.Y - 1, startPos.Z);
            return below == PlaceOn || below == AltPlaceOn;
        }

        //simply replace starting block with plant block
        public override void PlaceFeature(Vector3i startPos, Chunk chunk)
        {
            //int count = NoiseRouter.GetVariation(startPos.X + chunk.ChunkPos.X * SubChunk.SUBCHUNK_SIZE, startPos.Y, startPos.Z + chunk.ChunkPos.Z * SubChunk.SUBCHUNK_SIZE, 5, NoiseRouter.seed, 100);
            //if (count > 100)
            chunk.SetBlockUnsafe(startPos.X, startPos.Y, startPos.Z, PlantBlock);
            /*
            else if (count > 70)
                chunk.SetBlockUnsafe(startPos.X, startPos.Y, startPos.Z, BlockData.GetBlock(BlockIDs.REDSTONE_BLOCK).DefaultState);
            else if (count > 30)
                chunk.SetBlockUnsafe(startPos.X, startPos.Y, startPos.Z, BlockData.GetBlock(BlockIDs.LAPIZ_BLOCK).DefaultState);
            else
                chunk.SetBlockUnsafe(startPos.X, startPos.Y, startPos.Z, BlockData.GetBlock(BlockIDs.EMERALD_BLOCK).DefaultState);
            */
        }
    }
}
