using OurCraft.Terrain_Generation;
using OurCraft.Terrain_Generation.Registries;
using OurCraft.Utility;
using OurCraft.World.WorldData;

namespace OurCraft.World.WorldGeneration.Terrain_Gen
{
    //contains many helpers for generating different deposits reliably in the world
    public static class DepositGenerator
    {
        private const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;

        //bigger than a chunk so veins/patches can cross chunk edges cleanly.
        //64 is a good default if max deposit radius is 32.
        private const int FEATURE_CELL_SIZE = 64;
        private const int MAX_DEPOSIT_RADIUS = 32;

        //gets all deposits in world that intersect with chunk and places corresponding blocks
        public static void GenerateDeposits(Chunk chunk)
        {
            List<Deposit> allDeposits = new(DepositRegistry.GetAllDeposits());
            if (allDeposits.Count == 0) return;

            int chunkMinX = chunk.ChunkPos.X * CHUNK_WIDTH;
            int chunkMinZ = chunk.ChunkPos.Z * CHUNK_WIDTH;
            int chunkMaxX = chunkMinX + CHUNK_WIDTH - 1;
            int chunkMaxZ = chunkMinZ + CHUNK_WIDTH - 1;

            //expand by max deposit radius so we catch features that start outside the chunk
            int searchMinX = chunkMinX - MAX_DEPOSIT_RADIUS;
            int searchMaxX = chunkMaxX + MAX_DEPOSIT_RADIUS;
            int searchMinZ = chunkMinZ - MAX_DEPOSIT_RADIUS;
            int searchMaxZ = chunkMaxZ + MAX_DEPOSIT_RADIUS;

            int minCellX = VoxelMath.FloorDiv(searchMinX, FEATURE_CELL_SIZE);
            int maxCellX = VoxelMath.FloorDiv(searchMaxX, FEATURE_CELL_SIZE);
            int minCellZ = VoxelMath.FloorDiv(searchMinZ, FEATURE_CELL_SIZE);
            int maxCellZ = VoxelMath.FloorDiv(searchMaxZ, FEATURE_CELL_SIZE);

            //put deposits into chunk
            for (int cellX = minCellX; cellX <= maxCellX; cellX++)
            {
                for (int cellZ = minCellZ; cellZ <= maxCellZ; cellZ++)
                {
                    DepositInstance[] instances = BuildCellInstances(cellX, cellZ, allDeposits);
                    StampInstancesIntoChunk(chunk, instances, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);
                }
            }
        }

        //finds deposits that intersect with current chunk cell or neighbor chunk cells
        private static DepositInstance[] BuildCellInstances(int cellX, int cellZ, List<Deposit> deposits)
        {
            List<DepositInstance> instances = [];

            int cellSeed = VoxelMath.HashCellCoords(cellX, cellZ);
            Random rng = new(cellSeed);

            int cellMinX = cellX * FEATURE_CELL_SIZE;
            int cellMinZ = cellZ * FEATURE_CELL_SIZE;

            for (int i = 0; i < deposits.Count; i++)
            {
                //get deposit and check if valid
                Deposit deposit = deposits[i];

                if (deposit.spawnAttempts <= 0) continue;
                if (deposit.spawnChance <= 0) continue;
                if (deposit.minSize <= 0 || deposit.maxSize <= 0) continue;

                //get max and min y section of current deposit and skip if out of range
                int minY = Math.Min(deposit.minY, deposit.maxY);
                int maxY = Math.Max(deposit.minY, deposit.maxY);

                if (minY > maxY) continue;

                //get deposit attempt count relative to deposit world density
                int attempts = GetScaledAttemptCount(deposit);

                //try to generate deposit "attempts" amount of times
                for (int attempt = 0; attempt < attempts; attempt++)
                {
                    //run rng to see if deposit can exist
                    if (rng.Next(deposit.spawnChance) != 0) continue;

                    //run rng to check size of deposit
                    int size = rng.Next(deposit.minSize, deposit.maxSize + 1);
                    if (size <= 0) continue;

                    //run rng to get world origin of deposit
                    int worldX = cellMinX + rng.Next(FEATURE_CELL_SIZE);
                    int worldZ = cellMinZ + rng.Next(FEATURE_CELL_SIZE);
                    int worldY = (minY == maxY) ? minY : rng.Next(minY, maxY + 1);
                    
                    //check if origin is in proper biome or not
                    Biome biome = OverworldGenerator.GetBiome(worldX, worldZ);
                    if (!OverworldGenerator.ContainsDeposit(biome, deposit)) continue;

                    //add deposit to found deposits
                    int seed = HashInstanceSeed(cellX, cellZ, deposit, worldX, worldY, worldZ, size);
                    instances.Add(new DepositInstance(deposit, worldX, worldY, worldZ, size, seed));
                }
            }

            return instances.ToArray();
        }

        //gets scaled attempts for a deposit relative to world density (per chunk)
        private static int GetScaledAttemptCount(Deposit deposit)
        {
            //scales attempts to preserve roughly ore per chunk density
            const double CHUNK_AREA = (double)CHUNK_WIDTH * CHUNK_WIDTH;
            const double CELL_AREA = (double)FEATURE_CELL_SIZE * FEATURE_CELL_SIZE;

            int scaled = (int)Math.Ceiling(deposit.spawnAttempts * (CELL_AREA / CHUNK_AREA));
            return Math.Max(1, scaled);
        }

        //gets deposit instances in a chunk, and places all properly
        private static void StampInstancesIntoChunk(Chunk chunk, DepositInstance[] instances, int chunkMinX, int chunkMaxX, int chunkMinZ, int chunkMaxZ)
        {
            for (int i = 0; i < instances.Length; i++)
            {
                DepositInstance instance = instances[i];

                switch (instance.Deposit.placementType)
                {
                    case DepositShape.VEIN:
                        DepositPlacer.PlaceVein(chunk, instance, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);
                        break;

                    case DepositShape.PATCH:
                        DepositPlacer.PlacePatch(chunk, instance, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);
                        break;

                    case DepositShape.LAYER:
                        DepositPlacer.PlaceLayer(chunk, instance, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);
                        break;

                    case DepositShape.DISC:
                        DepositPlacer.PlaceDisc(chunk, instance, chunkMinX, chunkMaxX, chunkMinZ, chunkMaxZ);
                        break;
                }
            }
        }

        //create hash from deposit origin
        private static int HashInstanceSeed(int cellX, int cellZ, Deposit deposit, int worldX, int worldY, int worldZ, int size)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + cellX;
                hash = hash * 31 + cellZ;
                hash = hash * 31 + size;
                hash = hash * 31 + worldX;
                hash = hash * 31 + worldY;
                hash = hash * 31 + worldZ;
                hash = hash * 31 + deposit.minY;
                hash = hash * 31 + deposit.maxY;
                hash = hash * 31 + deposit.spawnAttempts;
                hash = hash * 31 + deposit.spawnChance;
                hash = hash * 31 + deposit.minSize;
                hash = hash * 31 + deposit.maxSize;
                hash = hash * 31 + (int)deposit.placementType;
                hash = hash * 31 + (deposit.block.GetHashCode());

                if (deposit.replacementBlocks != null)
                {
                    for (int i = 0; i < deposit.replacementBlocks.Count; i++) hash = hash * 31 + (deposit.replacementBlocks[i].GetHashCode());
                }

                hash ^= (hash << 13);
                hash ^= (hash >> 17);
                hash ^= (hash << 5);

                return hash;
            }
        }
    }
}