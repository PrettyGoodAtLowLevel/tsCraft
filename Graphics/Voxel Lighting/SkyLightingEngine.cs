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
        const int CHUNK_SIZE = SubChunk.SUBCHUNK_SIZE; //32
        const int MAX_HEIGHT = Chunk.CHUNK_HEIGHT;

        //seeds the top layer of sky lights in a chunk
        public static void SeedSkyLights(Chunk chunk, ConcurrentQueue<SkyLightNode> skyLights)
        {
            for (int x = 0; x < SubChunk.SUBCHUNK_SIZE; x++)
            {
                for (int z = 0; z < SubChunk.SUBCHUNK_SIZE; z++)
                {
                    BlockState state = chunk.GetBlockSafe(x, MAX_HEIGHT - 1, z);
                    bool transparent = state.GetBlock.IsLightPassable(state);
                    if (!transparent) continue;

                    int globalX = x + (CHUNK_SIZE * chunk.Pos.X);
                    int globalY = MAX_HEIGHT - 1;
                    int globalZ = z + (CHUNK_SIZE * chunk.Pos.Z);

                    int lightValue = 15 - state.GetBlock.GetLightAttenuation(state);
                    lightValue = Math.Clamp(lightValue, 0, 15);
                    skyLights.Enqueue(new SkyLightNode(globalX, globalY, globalZ, (byte)lightValue));
                }
            }
        }

        //same thing as above but for pre-set skylights
        public static void SeedDefferedSkyLights(Chunk chunk, ConcurrentQueue<SkyLightNode> skyLights, ConcurrentQueue<SkyLightNode> defferedSkyLights)
        {
            while (defferedSkyLights.TryDequeue(out SkyLightNode light))
            {
                //get local chunk position for getting light
                int wx = light.x, wy = light.y, wz = light.z;
                int lx = VoxelMath.Mod(wx, CHUNK_SIZE);
                int lz = VoxelMath.Mod(wz, CHUNK_SIZE);

                BlockState state = chunk.GetBlockSafe(lx, wy, lz);
                if (state.GetBlock.IsLightPassable(state) == false) continue;

                //unpack light
                ushort existingPacked = chunk.GetLight(lx, wy, lz);
                byte existing = VoxelMath.UnpackLight16Sky(existingPacked);
                byte newLight = light.light;

                //use brighter of the two
                if (existing >= newLight)
                    continue;

                byte finalLight = Math.Max(existing, newLight);

                //modify the chunk & enqueue world coordinates for BFS on newly set light
                chunk.SetSkyLight(lx, wy, lz, finalLight);
                skyLights.Enqueue(new SkyLightNode(wx, wy, wz, finalLight));
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
                    int cx = VoxelMath.FloorDiv(wx, CHUNK_SIZE);
                    int cz = VoxelMath.FloorDiv(wz, CHUNK_SIZE);

                    //get current chunk
                    Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                    if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                    {
                        ChunkCoord coord = new(cx, cz);
                        VoxelLightingEngine.DeferSkyLight(coord, wx, wy, wz, (byte)light);
                        continue;
                    }

                    //convert to chunk-local coordinates
                    int lx = VoxelMath.Mod(wx, CHUNK_SIZE);
                    int lz = VoxelMath.Mod(wz, CHUNK_SIZE);

                    //check if light can pass through block
                    BlockState state = chunk.GetBlockSafe(lx, wy, lz);
                    if (!state.GetBlock.IsLightPassable(state)) continue;

                    //decrease light
                    int attenuatedSide = light - 1;
                    int attenuatedDown = light - state.GetBlock.GetLightAttenuation(state);
                    int newLight = dy == -1 ? attenuatedDown : attenuatedSide;

                    //check if new light can change anything
                    if (newLight <= 0) continue;

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
                    int cx = VoxelMath.FloorDiv(wx, CHUNK_SIZE);
                    int cz = VoxelMath.FloorDiv(wz, CHUNK_SIZE);
                    Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                    if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted()) continue;

                    int lx = VoxelMath.Mod(wx, CHUNK_SIZE);
                    int lz = VoxelMath.Mod(wz, CHUNK_SIZE);

                    ushort packed = chunk.GetLight(lx, wy, lz);
                    byte existing = VoxelMath.UnpackLight16Sky(packed);
                    if (existing == 0) continue;

                    //check if new light is equal when going down or less when going other directions
                    if (existing <= light && dy < 0 || existing < light)
                    {
                        chunk.SetSkyLight(lx, wy, lz, 0);
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
