using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Utility;
using OurCraft.World;
using OurCraft.Terrain_Generation.Noise;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //big clump of rocks, eroded from the center
    public class Boulder : SurfaceFeature
    {
        public readonly BlockState stoneBlock;
        const int MAX_RADIUS = 5;

        public Boulder(BlockState stoneBlock)
        {
            this.stoneBlock = stoneBlock;
            this.localMin = new Vector3i(-MAX_RADIUS, 0, -MAX_RADIUS);
            this.localMax = new Vector3i(MAX_RADIUS, MAX_RADIUS * 2, MAX_RADIUS);
        }

        //check if center + radius in chunk
        public override bool CanPlaceFeature(Vector3i startPos, Chunk target)
        {
            int localX = VoxelMath.ModPow2(startPos.X, Chunk.CHUNK_WIDTH);
            int localZ = VoxelMath.ModPow2(startPos.Z, Chunk.CHUNK_WIDTH);
            if (!Chunk.PosValid(localX, startPos.Y + MAX_RADIUS * 2, localZ)) return false;

            //check the center column above is clear
            for (int i = 0; i < MAX_RADIUS * 2; i++)
            {
                BlockState state = target.GetBlockStateUnsafe(localX, startPos.Y + i, localZ);
                if (state != Block.AIR) return false;
            }
            return true;
        }

        //place rocks, slowly eroding from center
        public override void PlaceFeature(Vector3i startPos, Chunk target, ChunkManager world)
        {
            int radius = 4 + NoiseRouter.GetVariation(startPos.X, startPos.Y, startPos.Z, NoiseRouter.seed, salt: 3, max: 2);
            float squashY = 0.65f;
            float centerY = radius * squashY * 0.4f;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -1; y <= (int)(radius / squashY); y++) //start at -1 to anchor into ground
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        float dx = x / (float)radius;
                        float dy = (y - centerY) / (float)(radius * squashY);
                        float dz = z / (float)radius;

                        float dist = dx * dx + dy * dy + dz * dz;

                        if (dist <= 0.3f)
                        {
                            TrySetBlock(new Vector3i(startPos.X + x, startPos.Y + y, startPos.Z + z), stoneBlock, target);
                            continue;
                        }

                        if (dist > 1.0f) continue;

                        int wx = startPos.X + x;
                        int wy = startPos.Y + y;
                        int wz = startPos.Z + z;

                        float surfaceT = (dist - 0.3f) / 0.7f;
                        if (y < centerY) surfaceT *= 0.3f;

                        int threshold = (int)(surfaceT * 3f);
                        int noise = NoiseRouter.GetVariation(wx, wy, wz, NoiseRouter.seed, salt: 99, max: 4);
                        if (noise < threshold) continue;

                        TrySetBlock(new Vector3i(wx, wy, wz), stoneBlock, target);
                    }
                }
            }
        }
    }    
}
