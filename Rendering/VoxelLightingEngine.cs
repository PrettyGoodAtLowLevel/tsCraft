using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;
using System.Collections.Concurrent;
using System.Diagnostics;
using OurCraft.utility;

namespace OurCraft.Rendering
{
    //represents a light position in the world, kept as struct for better performance
    public struct LightNode
    {
        public int x, y, z;
        public Vector3i light;

        public LightNode(int x, int y, int z, Vector3i light)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.light = light;
        }
    }

    //does all of the lighting calculations in chunks for us
    public static class VoxelLightingEngine
    {
        const int CHUNK_SIZE = SubChunk.SUBCHUNK_SIZE; //32
        const int MAX_HEIGHT = Chunk.CHUNK_HEIGHT;

        //seeds the flood fill lighting for the 3x3 grid then computes it
        public static void LightChunks(Chunk center, Chunk left, Chunk right, Chunk front, Chunk back,
        Chunk leftFront, Chunk leftBack, Chunk rightFront, Chunk rightBack, Chunkmanager world)
        {
            Stopwatch sw = Stopwatch.StartNew();
            ConcurrentQueue<LightNode> lights = new ConcurrentQueue<LightNode>();

            //seed all light sources for block light propagation that work in 3x3 grid
            //center chunk just seed all light sources
            SeedCenterChunk(center, lights);
            SeedAxialNeighborX(left, lights, false);
            SeedAxialNeighborX(right, lights, true);
            SeedAxialNeighborZ(front, lights, true);
            SeedAxialNeighborZ(back, lights, false);
            SeedCornerNeighbor(leftFront, lights, false, true);
            SeedCornerNeighbor(leftBack, lights, false, false);
            SeedCornerNeighbor(rightFront, lights, true, true);
            SeedCornerNeighbor(rightBack, lights, true, false);

            //propagate all of he block lights
            PropagateBlockLights(world, lights);
            sw.Stop();
            center.lightingTime += (int)sw.ElapsedMilliseconds;
        }

        //flood fill bfs for all block lights in chunks, only uses light sources from 3x3 chunk grid for memory safety
        public static void PropagateBlockLights(Chunkmanager world, ConcurrentQueue<LightNode> lights)
        {
            //directions
            Span<(int dx, int dy, int dz)> dirs =
            [
                ( 1, 0, 0),
                (-1, 0, 0),
                ( 0, 1, 0),
                ( 0,-1, 0),
                ( 0, 0, 1),
                ( 0, 0,-1)
            ];          

            //bfs algorithm
            while (lights.TryDequeue(out var node))
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
                    if (wy < 0 || wy >= MAX_HEIGHT)
                        continue;
                    
                    //determine chunk position coordinates
                    int cx = VoxelMath.FloorDiv(wx, CHUNK_SIZE);   //robust for negatives
                    int cz = VoxelMath.FloorDiv(wz, CHUNK_SIZE);

                    //get current chunk
                    Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                    if (chunk == null || !chunk.HasVoxelData())
                        continue;

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

                    //pack light and modify the chunk
                    ushort packed = VoxelMath.PackLight16(finalLight, 0);
                    chunk.SetLight(lx, wy, lz, packed);

                    //enqueue world coordinates for BFS on newly set light
                    lights.Enqueue(new LightNode(wx, wy, wz, finalLight));
                }
            }
        }


        //simply seeds all of the light sources in the center chunk
        public static void SeedCenterChunk(Chunk center, ConcurrentQueue<LightNode> lights)
        {
            foreach (var subChunk in center.subChunks)
            {
                if (subChunk.lightSources.Count == 0) continue;
                foreach (var light in subChunk.lightSources)
                {
                    ushort x = 0, y = 0, z = 0;
                    VoxelMath.UnpackPos32(light, ref x, ref y, ref z);
                    int worldY = y + (subChunk.YPos * SubChunk.SUBCHUNK_SIZE);

                    BlockState state = center.GetBlockSafe(x, worldY, z);
                    Vector3i lightLevel = state.GetBlock.GetLightSourceLevel(state);

                    //set the light in the center chunk's lightmap
                    ushort packed = VoxelMath.PackLight16(lightLevel, 0);
                    center.SetLight(x, worldY, z, packed);
                    int globalX = center.Pos.X * SubChunk.SUBCHUNK_SIZE + x;
                    int globalZ = center.Pos.Z * SubChunk.SUBCHUNK_SIZE + z;

                    //enqueue with world coordinates (0-31 for center chunk)
                    lights.Enqueue(new LightNode(globalX, (short)worldY, globalZ, lightLevel));
                }
            }
        }

        //seed x neighbors that can propagate into the center chunk
        public static void SeedAxialNeighborX(Chunk neighbor, ConcurrentQueue<LightNode> lights, bool posX)
        {
            if (neighbor == null) return;
            int threshold = 15;

            foreach (var subChunk in neighbor.subChunks)
            {
                if (subChunk.lightSources.Count == 0) continue;
                foreach (var light in subChunk.lightSources)
                {
                    ushort x = 0, y = 0, z = 0;
                    VoxelMath.UnpackPos32(light, ref x, ref y, ref z);

                    //only seed if the light can propagate into the center chunk
                    if (posX)
                    {
                        //only lights near neighbor's -X face (close to center)
                        if (x > threshold) continue;
                    }
                    else
                    {
                        //only lights near neighbor's +X face (close to center)
                        if (x < SubChunk.SUBCHUNK_SIZE - 1 - threshold) continue;
                    }

                    int worldY = y + (subChunk.YPos * SubChunk.SUBCHUNK_SIZE);
                    BlockState state = neighbor.GetBlockSafe(x, worldY, z);
                    Vector3i lightLevel = state.GetBlock.GetLightSourceLevel(state);

                    //set light in the neighbor's lightmap first
                    ushort packed = VoxelMath.PackLight16(lightLevel, 0);
                    neighbor.SetLight(x, worldY, z, packed);

                    int globalX = neighbor.Pos.X * SubChunk.SUBCHUNK_SIZE + x;
                    int globalZ = neighbor.Pos.Z * SubChunk.SUBCHUNK_SIZE + z;
                    //enqueue with world coordinates (0-31 for center chunk)
                    lights.Enqueue(new LightNode(globalX, (short)worldY, globalZ, lightLevel));
                }
            }
        }

        //seed z neighbors that can propagate into the center chunk
        public static void SeedAxialNeighborZ(Chunk neighbor, ConcurrentQueue<LightNode> lights, bool posZ)
        {
            if (neighbor == null) return;
            int threshold = 15;

            foreach (var subChunk in neighbor.subChunks)
            {
                if (subChunk.lightSources.Count == 0) continue;
                foreach (var light in subChunk.lightSources)
                {
                    ushort x = 0, y = 0, z = 0;
                    VoxelMath.UnpackPos32(light, ref x, ref y, ref z);

                    //only seed if able to reach the center chunk
                    if (posZ)
                    {
                        //+Z neighbor → only near the -Z face
                        if (z > threshold) continue;
                    }
                    else
                    {
                        //-Z neighbor → only near +Z face
                        if (z < SubChunk.SUBCHUNK_SIZE - 1 - threshold) continue;
                    }

                    int worldY = y + (subChunk.YPos * SubChunk.SUBCHUNK_SIZE);
                    BlockState state = neighbor.GetBlockSafe(x, worldY, z);                  
                    Vector3i lightLevel = state.GetBlock.GetLightSourceLevel(state);

                    //set light in neighbor's lightmap
                    ushort packed = VoxelMath.PackLight16(lightLevel, 0);
                    neighbor.SetLight(x, worldY, z, packed);

                    int globalX = neighbor.Pos.X * SubChunk.SUBCHUNK_SIZE + x;
                    int globalZ = neighbor.Pos.Z * SubChunk.SUBCHUNK_SIZE + z;
                    //enqueue with world coordinates (0-31 for center chunk)
                    lights.Enqueue(new LightNode(globalX, (short)worldY, globalZ, lightLevel));
                }
            }
        }

        //seed corner neighbors that can propagate into neighbor chunks and then the center chunk
        public static void SeedCornerNeighbor(Chunk neighbor, ConcurrentQueue<LightNode> lights, bool posX, bool posZ)
        {
            if (neighbor == null) return;
            int threshold = 8;

            foreach (var subChunk in neighbor.subChunks)
            {
                if (subChunk.lightSources.Count == 0) continue;
                foreach (var light in subChunk.lightSources)
                {
                    ushort x = 0, y = 0, z = 0;
                    VoxelMath.UnpackPos32(light, ref x, ref y, ref z);

                    //must be near BOTH borders for diagonal propagation

                    //X side check
                    if (posX)
                    {
                        //neighbor is +X → near -X face
                        if (x > threshold) continue;
                    }
                    else
                    {
                        //neighbor is -X → near +X face
                        if (x < SubChunk.SUBCHUNK_SIZE - 1 - threshold) continue;
                    }

                    //Z side check
                    if (posZ)
                    {
                        //neighbor is +Z → near -Z face
                        if (z > threshold) continue;
                    }
                    else
                    {
                        //neighbor is -Z → near +Z face
                        if (z < SubChunk.SUBCHUNK_SIZE - 1 - threshold) continue;
                    }

                    int worldY = y + (subChunk.YPos * SubChunk.SUBCHUNK_SIZE);
                    BlockState state = neighbor.GetBlockSafe(x, worldY, z);
                    Vector3i lightLevel = state.GetBlock.GetLightSourceLevel(state);

                    //set light in neighbor's lightmap
                    ushort packed = VoxelMath.PackLight16(lightLevel, 0);
                    neighbor.SetLight(x, worldY, z, packed);

                    //transform to world coordinates
                    int globalX = neighbor.Pos.X * SubChunk.SUBCHUNK_SIZE + x;
                    int globalZ = neighbor.Pos.Z * SubChunk.SUBCHUNK_SIZE + z;
                    //enqueue with world coordinates (0-31 for center chunk)
                    lights.Enqueue(new LightNode(globalX, (short)worldY, globalZ, lightLevel));
                }
            }
        }
    }
}