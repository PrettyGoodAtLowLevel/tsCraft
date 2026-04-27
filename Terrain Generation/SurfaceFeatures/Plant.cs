using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Utility;
using OurCraft.World;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //one block surface feature : flower, grass
    public class Plant : SurfaceFeature
    {
        public readonly BlockState placeOn = Block.AIR;
        public readonly BlockState plantBlock = Block.AIR;
        public readonly BlockState altPlaceOn = Block.AIR;

        public Plant(BlockState placeOn, BlockState plantBlock, BlockState altPlaceOn)
        {
            this.placeOn = placeOn;
            this.plantBlock = plantBlock;           
            this.altPlaceOn = altPlaceOn;
            notCrossChunk = true; //since its only one block on xz, cant cross chunks
        }

        //check if bottom block = place on blocks
        public override bool CanPlaceFeature(Vector3i startPos, Chunk target)
        {
            int localX = VoxelMath.ModPow2(startPos.X, Chunk.CHUNK_WIDTH);
            int localZ = VoxelMath.ModPow2(startPos.Z, Chunk.CHUNK_WIDTH);

            if (!Chunk.PosValid(localX, startPos.Y - 1, localZ)) return false;
            BlockState below = target.GetBlockUnsafe(localX, startPos.Y - 1, localZ);
            return below == placeOn || below == altPlaceOn;
        }

        //simply place block
        public override void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world)
        {
            TrySetBlock(startPos, plantBlock, target);
        }
    }
}