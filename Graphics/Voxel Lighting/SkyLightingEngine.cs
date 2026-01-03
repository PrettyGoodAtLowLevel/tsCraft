using OpenTK.Graphics.ES20;
using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.utility;
using OurCraft.World;
using System.Collections.Concurrent;

namespace OurCraft.Graphics.Voxel_Lighting
{
    //does all of the skylight calculations for us
    public static class SkyLightingEngine
    {
        const int CHUNK_SIZE = Chunk.CHUNK_WIDTH; //32
        const int MAX_HEIGHT = Chunk.CHUNK_HEIGHT;
        const int MAX_SKY = 15;
        const int MIN_SKY = 0;
        const int LOW_LIGHT = 1;

        //seeds the top layer of sky lights in a chunk
        public static void SeedSkyLights(Chunkmanager world, Chunk chunk, ConcurrentQueue<SkyLightNode> skyLights)
        {
            ChunkCoord pos = chunk.ChunkPos;
            SeedCenterChunk(chunk, skyLights);

            SeedNeighborX(world.GetChunk(new ChunkCoord(pos.X + 1, pos.Z)), skyLights, posX: true);
            SeedNeighborX(world.GetChunk(new ChunkCoord(pos.X - 1, pos.Z)), skyLights, posX: false);
            SeedNeighborZ(world.GetChunk(new ChunkCoord(pos.X, pos.Z + 1)), skyLights, posZ: true);
            SeedNeighborZ(world.GetChunk(new ChunkCoord(pos.X, pos.Z - 1)), skyLights, posZ: false);

            SeedCornerChunk(world.GetChunk(new ChunkCoord(pos.X + 1, pos.Z + 1)), skyLights, posX: true, posZ: true);
            SeedCornerChunk(world.GetChunk(new ChunkCoord(pos.X + 1, pos.Z - 1)), skyLights, posX: true, posZ: false);
            SeedCornerChunk(world.GetChunk(new ChunkCoord(pos.X - 1, pos.Z + 1)), skyLights, posX: false, posZ: true);
            SeedCornerChunk(world.GetChunk(new ChunkCoord(pos.X - 1, pos.Z - 1)), skyLights, posX: false, posZ: false);
        }

        //uses the top of the chunk as the start for skylight
        public static void SeedCenterChunk(Chunk chunk, ConcurrentQueue<SkyLightNode> skyLights)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    BlockState state = chunk.GetBlockSafe(x, chunk.MaxSolidY, z);
                    bool transparent = state.LightPassable;
                    if (!transparent) continue;

                    int globalX = x + (CHUNK_SIZE * chunk.ChunkPos.X);
                    int globalY = chunk.MaxSolidY;
                    int globalZ = z + (CHUNK_SIZE * chunk.ChunkPos.Z);

                    int lightValue = MAX_SKY - state.SkyLightAttenuation;
                    lightValue = Math.Clamp(lightValue, MIN_SKY, MAX_SKY);
                    skyLights.Enqueue(new SkyLightNode(globalX, globalY, globalZ, (byte)lightValue));
                }
            }
        }

        //finds any unpropogated lights in the corner of chunks
        public static void SeedCornerChunk(Chunk? chunk, ConcurrentQueue<SkyLightNode> blockLights, bool posX, bool posZ)
        {
            if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                return;

            int x = posX ? 0 : CHUNK_SIZE - 1;
            int z = posZ ? 0 : CHUNK_SIZE - 1;

            //seed one xz column on the corner of a chunk
            for (int globalY = MAX_HEIGHT - 1; globalY >= 0; globalY--)
            {
                ushort packed = chunk.GetLight(x, globalY, z);
                byte existing = VoxelMath.UnpackLight16Sky(packed);

                if (existing <= LOW_LIGHT) continue;

                int globalX = (chunk.ChunkPos.X * CHUNK_SIZE) + x;
                int globalZ = (chunk.ChunkPos.Z * CHUNK_SIZE) + z;
                blockLights.Enqueue(new SkyLightNode(globalX, globalY, globalZ, existing));
            }
        }

        //scans the bordering lights on the x neighbors of a chunk
        public static void SeedNeighborX(Chunk? chunk, ConcurrentQueue<SkyLightNode> blockLights, bool posX)
        {
            if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                return;

            int x = posX ? 0 : CHUNK_SIZE - 1;

            //seed one x slice of a chunk
            for (int globalY = MAX_HEIGHT - 1; globalY >= 0; globalY--)
            {
                for (int z = 0; z < CHUNK_SIZE - 1; z++)
                {
                    ushort packed = chunk.GetLight(x, globalY, z);
                    byte existing = VoxelMath.UnpackLight16Sky(packed);

                    if (existing <= LOW_LIGHT) continue;

                    int globalX = (chunk.ChunkPos.X * CHUNK_SIZE) + x;
                    int globalZ = (chunk.ChunkPos.Z * CHUNK_SIZE) + z;
                    blockLights.Enqueue(new SkyLightNode(globalX, globalY, globalZ, existing));
                }
            }
        }

        //scans border lights on z neighbors of a chunk
        public static void SeedNeighborZ(Chunk? chunk, ConcurrentQueue<SkyLightNode> blockLights, bool posZ)
        {
            if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                return;

            int z = posZ ? 0 : CHUNK_SIZE - 1;

            //seed one z slice of a chunk
            for (int globalY = MAX_HEIGHT - 1; globalY >= 0; globalY--)
            {
                for (int x = 0; x < CHUNK_SIZE - 1; x++)
                {
                    ushort packed = chunk.GetLight(x, globalY, z);
                    byte existing = VoxelMath.UnpackLight16Sky(packed);

                    if (existing <= LOW_LIGHT) continue;

                    int globalX = (chunk.ChunkPos.X * CHUNK_SIZE) + x;
                    int globalZ = (chunk.ChunkPos.Z * CHUNK_SIZE) + z;
                    blockLights.Enqueue(new SkyLightNode(globalX, globalY, globalZ, existing));
                }
            }
        }

        //flood fill bfs for all sky lights in chunks
        public static void PropagateSkyLights(Chunkmanager world, ConcurrentQueue<SkyLightNode> skyLights, bool dirty = false)
        {
            //directions
            Span<(int dx, int dy, int dz)> dirs =
            [
                ( 1, 0, 0), (-1, 0, 0), //right left
                ( 0, 1, 0), ( 0,-1, 0), //up down
                ( 0, 0, 1), ( 0, 0,-1)  //front back
            ];

            //bfs algorithm
            while (skyLights.TryDequeue(out var node))
            {
                //get current light
                int light = node.light;

                //spread across each direction
                foreach (var (dx, dy, dz) in dirs)
                {
                    int wx = node.x + dx;
                    int wy = node.y + dy;
                    int wz = node.z + dz;

                    //height clamp
                    if (wy < 0 || wy >= MAX_HEIGHT) continue;

                    //determine chunk position coordinates
                    int cx = VoxelMath.FloorDivPow2(wx, CHUNK_SIZE);
                    int cz = VoxelMath.FloorDivPow2(wz, CHUNK_SIZE);

                    //get current chunk
                    Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                    if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                        continue;
                    
                    //convert to chunk-local coordinates
                    int lx = VoxelMath.ModPow2(wx, CHUNK_SIZE);
                    int lz = VoxelMath.ModPow2(wz, CHUNK_SIZE);

                    //check if light can pass through block
                    BlockState state = chunk.GetBlockSafe(lx, wy, lz);
                    if (!state.LightPassable) continue;

                    //decrease light
                    int attenuatedSide = light - 1;
                    int attenuatedDown = light - state.SkyLightAttenuation;
                    int newLight = dy == -1 ? attenuatedDown : attenuatedSide;

                    //check if new light can change anything
                    if (newLight <= MIN_SKY) continue;

                    //current light at new block
                    ushort existingPacked = chunk.GetLight(lx, wy, lz);
                    int existing = VoxelMath.UnpackLight16Sky(existingPacked);

                    //only propagate stronger light
                    if (existing >= newLight) continue;

                    //use brighter of the two
                    int finalLight = Math.Max(existing, newLight);

                    //modify the chunk & enqueue world coordinates for BFS on newly set light
                    chunk.SetSkyLight(lx, wy, lz, finalLight);
                    if (dirty) world.MarkPosDirty(new Vector3i(wx, wy, wz), chunk);
                    skyLights.Enqueue(new SkyLightNode(wx, wy, wz, (byte)finalLight));
                }
            }
        }

        //propagates shade with skylights and requeues skylights that werent effected by darkness
        public static void PropagateSkyLightRemoval(Chunkmanager world, ConcurrentQueue<RemoveSkyNode> removeQueue, ConcurrentQueue<SkyLightNode> reAddQueue)
        {
            Span<(int dx, int dy, int dz)> dirs =
            [
                ( 1, 0, 0), (-1, 0, 0),
                ( 0, 0, 1), ( 0, 0,-1),
                ( 0,-1, 0), (0, 1, 0)
            ];

            while (removeQueue.TryDequeue(out var node))
            {
                byte light = node.light;

                foreach (var (dx, dy, dz) in dirs)
                {
                    int wx = node.x + dx;
                    int wy = node.y + dy;
                    int wz = node.z + dz;

                    if (wy < 0 || wy >= MAX_HEIGHT) continue;
                    int cx = VoxelMath.FloorDivPow2(wx, CHUNK_SIZE);
                    int cz = VoxelMath.FloorDivPow2(wz, CHUNK_SIZE);
                    Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                    if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted()) continue;

                    int lx = VoxelMath.ModPow2(wx, CHUNK_SIZE);
                    int lz = VoxelMath.ModPow2(wz, CHUNK_SIZE);

                    ushort packed = chunk.GetLight(lx, wy, lz);
                    byte existing = VoxelMath.UnpackLight16Sky(packed);
                    if (existing == MIN_SKY) continue;

                    //check if new light is equal when going down or less when going other directions
                    if (existing <= light && dy < 0 || existing < light)
                    {
                        chunk.SetSkyLight(lx, wy, lz, MIN_SKY);
                        world.MarkPosDirty(new Vector3i(wx, wy, wz), chunk);
                        removeQueue.Enqueue(new RemoveSkyNode(wx, wy, wz, existing));
                    }
                    else
                    {
                        reAddQueue.Enqueue(new SkyLightNode(wx, wy, wz, existing));
                    }
                }
            }
        }
    }
}
