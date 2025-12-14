using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.utility;
using OurCraft.World;
using System.Collections.Concurrent;

namespace OurCraft.Graphics.Voxel_Lighting
{
    //does all of the block light calculations for us
    public static class BlockLightingEngine
    {
        const int CHUNK_SIZE = SubChunk.SUBCHUNK_SIZE; //32
        const int MAX_HEIGHT = Chunk.CHUNK_HEIGHT;

        //seeds all the block lights in a chunk
        public static void SeedBlockLights(Chunk chunk, ConcurrentQueue<LightNode> blockLights)
        {
            foreach (var subChunk in chunk.subChunks)
            {
                foreach (var lightSource in subChunk.lightSources)
                {
                    ushort x = 0, y = 0, z = 0;
                    VoxelMath.UnpackPos32(lightSource, ref x, ref y, ref z);
                    BlockState state = subChunk.GetBlockState(x, y, z);
                    Vector3i lightValue = state.GetBlock.GetLightSourceLevel(state);

                    int globalX = x + (CHUNK_SIZE * chunk.Pos.X);
                    int globalY = y + (CHUNK_SIZE * subChunk.YPos);
                    int globalZ = z + (CHUNK_SIZE * chunk.Pos.Z);

                    chunk.SetBlockLight(x, globalY, z, lightValue);
                    blockLights.Enqueue(new LightNode(globalX, globalY, globalZ, lightValue));
                }
            }
        }

        //seeds any deffered light work that other chunks may have put into this chunk
        public static void SeedDefferedLights(Chunk chunk, ConcurrentQueue<LightNode> blockLights, ConcurrentQueue<LightNode> defferedLights)
        {
            while (defferedLights.TryDequeue(out LightNode light))
            {
                //get local chunk position for getting light
                int wx = light.x, wy = light.y, wz = light.z;
                int lx = VoxelMath.Mod(wx, CHUNK_SIZE);
                int lz = VoxelMath.Mod(wz, CHUNK_SIZE);

                BlockState state = chunk.GetBlockSafe(lx, wy, lz);
                if (state.GetBlock.IsLightPassable(state) == false) continue;

                //unpack light
                ushort existingPacked = chunk.GetLight(lx, wy, lz);
                Vector3i existing = VoxelMath.UnpackLight16Block(existingPacked);
                Vector3i newLight = light.light;

                //use brighter of the two
                if (existing.X >= newLight.X && existing.Y >= newLight.Y && existing.Z >= newLight.Z)
                    continue;

                Vector3i finalLight = new Vector3i(Math.Max(existing.X, newLight.X),
                Math.Max(existing.Y, newLight.Y), Math.Max(existing.Z, newLight.Z));

                //modify the chunk & enqueue world coordinates for BFS on newly set light
                chunk.SetBlockLight(lx, wy, lz, finalLight);
                blockLights.Enqueue(new LightNode(wx, wy, wz, finalLight));
            }
        }

        //flood fill bfs for all block lights in chunks
        public static void PropagateBlockLights(Chunkmanager world, ConcurrentQueue<LightNode> blockLights, bool dirty = false)
        {
            //directions
            Span<(int dx, int dy, int dz)> dirs =
            [
                ( 1, 0, 0), (-1, 0, 0), //right left
                ( 0, 1, 0), ( 0,-1, 0), //up down
                ( 0, 0, 1), ( 0, 0,-1)  //front back
            ];

            //bfs algorithm
            while (blockLights.TryDequeue(out var node))
            {
                //get current light
                Vector3i light = node.light;

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
                        VoxelLightingEngine.DeferBlockLight(coord, wx, wy, wz, light);
                        continue;
                    }

                    //convert to chunk-local coordinates
                    int lx = VoxelMath.Mod(wx, CHUNK_SIZE);
                    int lz = VoxelMath.Mod(wz, CHUNK_SIZE);

                    //check if light can pass through block
                    BlockState state = chunk.GetBlockSafe(lx, wy, lz);
                    if (!state.GetBlock.IsLightPassable(state))
                        continue;

                    //decrease light
                    Vector3i newLight = new Vector3i(Math.Max(0, light.X - 1),
                    Math.Max(0, light.Y - 1), Math.Max(0, light.Z - 1));

                    //check if new light can change anything
                    if (newLight.X <= 0 && newLight.Y <= 0 && newLight.Z <= 0)
                        continue;

                    //current light at new block
                    ushort existingPacked = chunk.GetLight(lx, wy, lz);
                    Vector3i existing = VoxelMath.UnpackLight16Block(existingPacked);

                    //only propagate stronger light
                    if (existing.X >= newLight.X && existing.Y >= newLight.Y && existing.Z >= newLight.Z)
                        continue;

                    //use brighter of the two
                    Vector3i finalLight = new Vector3i(Math.Max(existing.X, newLight.X),
                    Math.Max(existing.Y, newLight.Y), Math.Max(existing.Z, newLight.Z));

                    //modify the chunk & enqueue world coordinates for BFS on newly set light
                    chunk.SetBlockLight(lx, wy, lz, finalLight);
                    if (dirty) world.MarkPosDirty(new Vector3i(wx, wy, wz), chunk);  //mark chunk y pos as dirty if we are repropagating
                    blockLights.Enqueue(new LightNode(wx, wy, wz, finalLight));
                }
            }
        }

        //propagates darkness and requeues lights that werent effected by darkness
        public static void PropagateBlockLightRemoval(Chunkmanager world, ConcurrentQueue<RemoveLightNode> removeQueue, ConcurrentQueue<LightNode> reAddQueue)
        {
            Span<(int dx, int dy, int dz)> dirs =
            [
                ( 1, 0, 0), (-1, 0, 0), ( 0, 1, 0),
                ( 0,-1, 0), ( 0, 0, 1), ( 0, 0,-1)
            ];

            while (removeQueue.TryDequeue(out var node))
            {
                Vector3i light = node.light;

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
                    Vector3i existing = VoxelMath.UnpackLight16Block(packed);
                    if (existing == Vector3i.Zero) continue;

                    //per-channel darkness
                    Vector3i survivors = existing;              //what remains after removal
                    Vector3i nextRemoval = Vector3i.Zero; //what to propagate outward
                    bool anyRemoved = false;
                    bool anySurvive = false;

                    if (existing.X < light.X)
                    {
                        anyRemoved = true;
                        nextRemoval.X = Math.Max(0, existing.X);
                        survivors.X = 0;
                    }
                    else if (existing.X > 0) anySurvive = true;


                    if (existing.Y < light.Y)
                    {
                        anyRemoved = true;
                        nextRemoval.Y = Math.Max(0, existing.Y);
                        survivors.Y = 0;
                    }
                    else if (existing.Y > 0) anySurvive = true;


                    if (existing.Z < light.Z)
                    {
                        anyRemoved = true;
                        nextRemoval.Z = Math.Max(0, existing.Z);
                        survivors.Z = 0;
                    }
                    else if (existing.Z > 0) anySurvive = true;

                    //if nothing was removed, this neighbor is independent -> re-add its full existing light.
                    if (!anyRemoved)
                    {
                        reAddQueue.Enqueue(new LightNode(wx, wy, wz, existing));
                        continue;
                    }

                    //write back survivors
                    chunk.SetBlockLight(lx, wy, lz, survivors);
                    world.MarkPosDirty(new Vector3i(wx, wy, wz), chunk);

                    //if any channel survived, re-add that partial light to restore propagation paths.
                    if (anySurvive)
                        reAddQueue.Enqueue(new LightNode(wx, wy, wz, survivors));

                    //propagate the per-channel removal outward
                    if (nextRemoval != Vector3i.Zero)
                        removeQueue.Enqueue(new RemoveLightNode(wx, wy, wz, nextRemoval));
                }
            }
        }
    }
}