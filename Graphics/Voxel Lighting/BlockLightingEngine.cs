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
        const int CHUNK_SIZE = Chunk.CHUNK_WIDTH; //32
        const int SUBCHUNK_SIZE = SubChunk.SUBCHUNK_SIZE;
        const int MAX_HEIGHT = Chunk.CHUNK_HEIGHT;

        const int MAX_LIGHT = 15;
        const int MIN_LIGHT = 0;
        const int LOW_LIGHT = 1;

        //seeds all the block lights in a chunk
        public static void SeedBlockLights(ChunkManager world, Chunk chunk, ConcurrentQueue<LightNode> blockLights)
        {
            ChunkCoord pos = chunk.ChunkPos;
            SeedCenterChunk(chunk, blockLights);

            SeedNeighborX(world.GetChunk(new ChunkCoord(pos.X + 1, pos.Z)), blockLights, posX:true);
            SeedNeighborX(world.GetChunk(new ChunkCoord(pos.X - 1, pos.Z)), blockLights, posX:false);
            SeedNeighborZ(world.GetChunk(new ChunkCoord(pos.X, pos.Z + 1)), blockLights, posZ:true);
            SeedNeighborZ(world.GetChunk(new ChunkCoord(pos.X, pos.Z - 1)), blockLights, posZ:false);

            SeedCornerChunk(world.GetChunk(new ChunkCoord(pos.X + 1, pos.Z + 1)), blockLights, posX:true, posZ:true);
            SeedCornerChunk(world.GetChunk(new ChunkCoord(pos.X + 1, pos.Z - 1)), blockLights, posX:true, posZ:false);
            SeedCornerChunk(world.GetChunk(new ChunkCoord(pos.X - 1, pos.Z + 1)), blockLights, posX:false, posZ:true);
            SeedCornerChunk(world.GetChunk(new ChunkCoord(pos.X - 1, pos.Z - 1)), blockLights, posX:false, posZ:false);
        }

        //finds all of the light sources in the chunk and seeds them
        public static void SeedCenterChunk(Chunk chunk, ConcurrentQueue<LightNode> blockLights)
        {
            for (int x = 0; x < Chunk.WIDTH_IN_SUBCHUNKS; x++)
            {
                for (int y = 0; y < Chunk.HEIGHT_IN_SUBCHUNKS; y++)
                {
                    for (int z = 0; z < Chunk.WIDTH_IN_SUBCHUNKS; z++)
                    {
                        SubChunk subChunk = chunk.SubChunks[x, y, z];
                        foreach (var lightSource in subChunk.lightSources)
                        {
                            ushort lx = 0, ly = 0, lz = 0;
                            VoxelMath.UnpackPos32(lightSource, ref lx, ref ly, ref lz);
                            BlockState state = subChunk.GetBlockState(lx, ly, lz);
                            Vector3i lightValue = state.LightLevel;
                            
                            int globalX = (subChunk.ChunkXPos * SUBCHUNK_SIZE) + lx + (CHUNK_SIZE * chunk.ChunkPos.X);
                            int globalY = ly + (SUBCHUNK_SIZE * subChunk.ChunkYPos);
                            int globalZ = (subChunk.ChunkZPos * SUBCHUNK_SIZE) + lz + (CHUNK_SIZE * chunk.ChunkPos.Z);

                            chunk.SetBlockLight((subChunk.ChunkXPos * SUBCHUNK_SIZE) + lx, globalY, (subChunk.ChunkZPos * SUBCHUNK_SIZE) + lz, lightValue);
                            blockLights.Enqueue(new LightNode(globalX, globalY, globalZ, lightValue));
                        }
                    }
                }
            }
        }

        //finds any unpropogated lights in the corner of chunks
        public static void SeedCornerChunk(Chunk? chunk, ConcurrentQueue<LightNode> blockLights, bool posX, bool posZ)
        {
            if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                return;

            int x = posX ? 0 : CHUNK_SIZE - 1;
            int z = posZ ? 0 : CHUNK_SIZE - 1;

            //go over one xz column and find the unpropagated lights, then seed them
            for(int globalY = MAX_HEIGHT - 1; globalY >= 0; globalY--)
            {
                ushort packed = chunk.GetLight(x, globalY, z);
                Vector3i existing = VoxelMath.UnpackLight16Block(packed);

                if (existing.X <= LOW_LIGHT && existing.Y <= LOW_LIGHT && existing.Z <= LOW_LIGHT)
                    continue;

                int globalX = (chunk.ChunkPos.X * CHUNK_SIZE) + x;
                int globalZ = (chunk.ChunkPos.Z * CHUNK_SIZE) + z;
                blockLights.Enqueue(new LightNode(globalX, globalY, globalZ, existing));
            }
        }

        //scans the bordering lights on the x neighbors of a chunk
        public static void SeedNeighborX(Chunk? chunk, ConcurrentQueue<LightNode> blockLights, bool posX)
        {
            if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                return;

            int x = posX ? 0 : CHUNK_SIZE - 1;

            //go over one x slice of the chunk and seed unpropagated light
            for (int globalY = MAX_HEIGHT - 1; globalY >= 0; globalY--)
            {
                for(int z = 0; z < CHUNK_SIZE - 1; z++)
                {
                    ushort packed = chunk.GetLight(x, globalY, z);
                    Vector3i existing = VoxelMath.UnpackLight16Block(packed);

                    if (existing.X <= LOW_LIGHT && existing.Y <= LOW_LIGHT && existing.Z <= LOW_LIGHT)
                        continue;

                    int globalX = (chunk.ChunkPos.X * CHUNK_SIZE) + x;
                    int globalZ = (chunk.ChunkPos.Z * CHUNK_SIZE) + z;
                    blockLights.Enqueue(new LightNode(globalX, globalY, globalZ, existing));
                }
            }
        }

        //scans border lights on z neighbors of a chunk
        public static void SeedNeighborZ(Chunk? chunk, ConcurrentQueue<LightNode> blockLights, bool posZ)
        {
            if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                return;

            int z = posZ ? 0 : CHUNK_SIZE - 1;

            //go over one z slice of the chunk and seed unpropagated light
            for (int globalY = MAX_HEIGHT - 1; globalY >= 0; globalY--)
            {
                for (int x = 0; x < CHUNK_SIZE - 1; x++)
                {
                    ushort packed = chunk.GetLight(x, globalY, z);
                    Vector3i existing = VoxelMath.UnpackLight16Block(packed);

                    if (existing.X <= LOW_LIGHT && existing.Y <= LOW_LIGHT && existing.Z <= LOW_LIGHT)
                        continue;

                    int globalX = (chunk.ChunkPos.X * CHUNK_SIZE) + x;
                    int globalZ = (chunk.ChunkPos.Z * CHUNK_SIZE) + z;
                    blockLights.Enqueue(new LightNode(globalX, globalY, globalZ, existing));
                }
            }
        }

        //flood fill bfs for all block lights in chunks
        public static void PropagateBlockLights(ChunkManager world, ConcurrentQueue<LightNode> blockLights, bool dirty = false)
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
                    int cx = VoxelMath.FloorDivPow2(wx, CHUNK_SIZE);
                    int cz = VoxelMath.FloorDivPow2(wz, CHUNK_SIZE);

                    //get current chunk
                    Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                    if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted())
                        continue; //dont propagate if chunk doesnt have block data                

                    //convert to chunk-local coordinates
                    int lx = VoxelMath.ModPow2(wx, CHUNK_SIZE);
                    int lz = VoxelMath.ModPow2(wz, CHUNK_SIZE);

                    BlockState state = chunk.GetBlockSafe(lx, wy, lz);
                    if (!state.LightPassable) continue; //dont propagate through solid blocks

                    //decrease light
                    Vector3i newLight = new Vector3i(Math.Max(MIN_LIGHT, light.X - 1),
                    Math.Max(MIN_LIGHT, light.Y - 1), Math.Max(MIN_LIGHT, light.Z - 1));
                    if (newLight.X <= MIN_LIGHT && newLight.Y <= MIN_LIGHT && newLight.Z <= MIN_LIGHT)
                        continue; //dont propagate if new light is too weak

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
        public static void PropagateBlockLightRemoval(ChunkManager world, ConcurrentQueue<RemoveLightNode> removeQueue, ConcurrentQueue<LightNode> reAddQueue)
        {
            Span<(int dx, int dy, int dz)> dirs =
            [
                ( 1, 0, 0), (-1, 0, 0), ( 0, 1, 0),//right left up
                ( 0,-1, 0), ( 0, 0, 1), ( 0, 0,-1) //down front back
            ];

            while (removeQueue.TryDequeue(out var node))
            {
                Vector3i light = node.light;

                foreach (var (dx, dy, dz) in dirs)
                {
                    int wx = node.x + dx;
                    int wy = node.y + dy;
                    int wz = node.z + dz;

                    //get chunk coords, dont propagate if chunk doesnt have block data
                    if (wy < 0 || wy >= MAX_HEIGHT) continue;
                    int cx = VoxelMath.FloorDivPow2(wx, CHUNK_SIZE);
                    int cz = VoxelMath.FloorDivPow2(wz, CHUNK_SIZE);
                    Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                    if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted()) continue;

                    //get chunk local coords
                    int lx = VoxelMath.ModPow2(wx, CHUNK_SIZE);
                    int lz = VoxelMath.ModPow2(wz, CHUNK_SIZE);

                    //get current light of chunk
                    ushort packed = chunk.GetLight(lx, wy, lz);
                    Vector3i existing = VoxelMath.UnpackLight16Block(packed);
                    if (existing == Vector3i.Zero) continue; //dont propagate darkness if already dark

                    //per rgb channel darkness propagation
                    Vector3i survivors = existing; //what remains after removal
                    Vector3i nextRemoval = Vector3i.Zero; //what to propagate outward
                    bool anyRemoved = false;
                    bool anySurvive = false;

                    if (existing.X < light.X)
                    {
                        anyRemoved = true;
                        nextRemoval.X = Math.Max(MIN_LIGHT, existing.X);
                        survivors.X = MIN_LIGHT;
                    }
                    else if (existing.X > MIN_LIGHT) anySurvive = true;


                    if (existing.Y < light.Y)
                    {
                        anyRemoved = true;
                        nextRemoval.Y = Math.Max(MIN_LIGHT, existing.Y);
                        survivors.Y = MIN_LIGHT;
                    }
                    else if (existing.Y > MIN_LIGHT) anySurvive = true;


                    if (existing.Z < light.Z)
                    {
                        anyRemoved = true;
                        nextRemoval.Z = Math.Max(MIN_LIGHT, existing.Z);
                        survivors.Z = MIN_LIGHT;
                    }
                    else if (existing.Z > MIN_LIGHT) anySurvive = true;

                    //if nothing was removed, this neighbor is independent and re-add its full existing light.
                    if (!anyRemoved)
                    {
                        reAddQueue.Enqueue(new LightNode(wx, wy, wz, existing));
                        continue;
                    }

                    //write back survivors
                    chunk.SetBlockLight(lx, wy, lz, survivors);
                    world.MarkPosDirty(new Vector3i(wx, wy, wz), chunk);

                    //if any channel survived, re-add that partial light to restore propagation paths.
                    if (anySurvive) reAddQueue.Enqueue(new LightNode(wx, wy, wz, survivors));

                    //propagate the per-channel removal outward
                    if (nextRemoval != Vector3i.Zero) removeQueue.Enqueue(new RemoveLightNode(wx, wy, wz, nextRemoval));
                }
            }
        }
    }
}