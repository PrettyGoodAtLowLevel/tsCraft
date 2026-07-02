using OurCraft.Blocks;
using OurCraft.Terrain_Generation;
using OurCraft.Utility;

namespace OurCraft.World.ChunkGeneration.WorldGen
{
    //contains helpers for placing deposits
    public static class DepositPlacer
    {
        //creates minecraft like vein shapes for a deposit
        public static void PlaceVein(Chunk chunk, DepositInstance instance, int chunkMinX, int chunkMaxX, int chunkMinZ, int chunkMaxZ)
        {
            Random rng = new(instance.Seed);
            int steps = Math.Max(4, instance.Size * 2);

            float cx = instance.WorldX;
            float cy = instance.WorldY;
            float cz = instance.WorldZ;

            for (int i = 0; i < steps; i++)
            {
                int radius = Math.Max(1, instance.Size / 4);
                int height = Math.Max(1, instance.Size / 5);

                PlaceEllipsoid(chunk, instance, (int)MathF.Round(cx), (int)MathF.Round(cy), (int)MathF.Round(cz),
                radius, height, radius, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);

                cx += rng.Next(-1, 2);
                cy += rng.Next(-1, 2);
                cz += rng.Next(-1, 2);

                //keep the walk from drifting too far.
                cx = Math.Clamp(cx, instance.WorldX - instance.Size, instance.WorldX + instance.Size);
                cy = Math.Clamp(cy, instance.Deposit.minY, instance.Deposit.maxY);
                cz = Math.Clamp(cz, instance.WorldZ - instance.Size, instance.WorldZ + instance.Size);
            }
        }

        //creates smoothish circle of patch for a deposit
        public static void PlacePatch(Chunk chunk, DepositInstance instance, int chunkMinX, int chunkMaxX, int chunkMinZ, int chunkMaxZ)
        {
            int rx = Math.Max(1, instance.Size);
            int ry = Math.Max(1, instance.Size / 2);
            int rz = Math.Max(1, instance.Size);

            PlaceEllipsoid(chunk, instance, instance.WorldX, instance.WorldY, instance.WorldZ,
            rx, ry, rz, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);
        }

        //creates a smoothish square-like layer for a deposit
        public static void PlaceLayer(Chunk chunk, DepositInstance instance, int chunkMinX, int chunkMaxX, int chunkMinZ, int chunkMaxZ)
        {
            int rx = Math.Max(1, instance.Size);
            int ry = 1;
            int rz = Math.Max(1, instance.Size);

            //tiny deterministic wobble.
            int wobble = (VoxelMath.HashToUnitFloat(instance.Seed, instance.WorldX, instance.WorldY, instance.WorldZ) < 0.5f) ? -1 : 1;
            int y = Math.Clamp(instance.WorldY + wobble, instance.Deposit.minY, instance.Deposit.maxY);

            PlaceEllipsoid(chunk, instance, instance.WorldX, y, instance.WorldZ,
            rx, ry, rz, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);
        }

        //creates a smoothish disc layer for a deposit
        public static void PlaceDisc(Chunk chunk, DepositInstance instance, int chunkMinX, int chunkMaxX, int chunkMinZ, int chunkMaxZ)
        {
            int radius = Math.Max(1, instance.Size);
            int thickness = Math.Max(1, instance.Size / 4);

            PlaceEllipsoid(chunk, instance, instance.WorldX, instance.WorldY, instance.WorldZ,
            radius, thickness, radius, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);
        }

        //places an ellipsoid shape, aware of chunk bounds and aware of which blocks to replace based on deposit
        private static void PlaceEllipsoid(Chunk chunk, DepositInstance instance, int centerX, int centerY, int centerZ,
        int radiusX, int radiusY, int radiusZ, int chunkMinX, int chunkMaxX, int chunkMinZ, int chunkMaxZ)
        {
            radiusX = Math.Max(1, radiusX);
            radiusY = Math.Max(1, radiusY);
            radiusZ = Math.Max(1, radiusZ);

            int minX = Math.Max(chunkMinX, centerX - radiusX);
            int maxX = Math.Min(chunkMaxX, centerX + radiusX);

            int minY = Math.Max(instance.Deposit.minY, centerY - radiusY);
            int maxY = Math.Min(instance.Deposit.maxY, centerY + radiusY);

            int minZ = Math.Max(chunkMinZ, centerZ - radiusZ);
            int maxZ = Math.Min(chunkMaxZ, centerZ + radiusZ);

            if (minX > maxX || minY > maxY || minZ > maxZ) return;

            float invRx = 1.0f / radiusX;
            float invRy = 1.0f / radiusY;
            float invRz = 1.0f / radiusZ;

            for (int worldX = minX; worldX <= maxX; worldX++)
            {
                float dx = (worldX - centerX) * invRx;
                float dx2 = dx * dx;

                for (int worldY = minY; worldY <= maxY; worldY++)
                {
                    float dy = (worldY - centerY) * invRy;
                    float dy2 = dy * dy;

                    for (int worldZ = minZ; worldZ <= maxZ; worldZ++)
                    {
                        float dz = (worldZ - centerZ) * invRz;
                        float d2 = dx2 + dy2 + dz * dz;

                        if (d2 > 1.0f) continue;

                        //deterministic edge roughness per world block.
                        float noiseCutoff = 0.82f + VoxelMath.HashToUnitFloat(instance.Seed, worldX, worldY, worldZ) * 0.18f;
                        if (d2 > noiseCutoff) continue;

                        int localX = worldX - chunkMinX;
                        int localZ = worldZ - chunkMinZ;

                        if (!CanReplace(chunk, localX, worldY, localZ, instance.Deposit)) continue;

                        chunk.SetBlockStateUnsafe(localX, worldY, localZ, instance.Deposit.block);
                    }
                }
            }
        }

        //checks if a xyz position's block is replaceable by the deposit
        private static bool CanReplace(Chunk chunk, int x, int y, int z, Deposit deposit)
        {
            BlockState current = chunk.GetBlockStateUnsafe(x, y, z);

            if (deposit.replacementBlocks != null && deposit.replacementBlocks.Count > 0)
            {
                for (int i = 0; i < deposit.replacementBlocks.Count; i++)
                {
                    if (current.Equals(deposit.replacementBlocks[i])) return true;
                }

                return false;
            }

            return true;
        }
    }
}