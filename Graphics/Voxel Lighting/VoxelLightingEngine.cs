using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;
using System.Collections.Concurrent;
using OurCraft.utility;
using System.Diagnostics;

namespace OurCraft.Graphics.Voxel_Lighting
{
    //has helper methods to calculate block and sky lighting for the world
    public static class VoxelLightingEngine
    {
        const int CHUNK_SIZE = SubChunk.SUBCHUNK_SIZE; //32
        const int MAX_HEIGHT = Chunk.CHUNK_HEIGHT;
        private readonly static ConcurrentDictionary<ChunkCoord, ConcurrentQueue<LightNode>> DefferedBlockLights = new();
        private readonly static ConcurrentDictionary<ChunkCoord, ConcurrentQueue<SkyLightNode>> DefferedSkyLights = new();

        //updates any deffered lighting that cant exist anymore since the chunk wont be loaded soon
        public static void UnloadDefferedLights(ChunkCoord playerChunk, int renderDistance)
        {
            foreach (var coord in DefferedBlockLights.Keys)
            {
                int dx = (int)MathF.Abs(coord.X - playerChunk.X);
                int dz = (int)MathF.Abs(coord.Z - playerChunk.Z);

                if (dx > renderDistance + 1 || dz > renderDistance + 1)
                {
                    DefferedBlockLights.TryRemove(coord, out var valueToDispose);
                }
            }
            foreach (var coord in DefferedSkyLights.Keys)
            {
                int dx = (int)MathF.Abs(coord.X - playerChunk.X);
                int dz = (int)MathF.Abs(coord.Z - playerChunk.Z);

                if (dx > renderDistance + 1 || dz > renderDistance + 1)
                {
                    DefferedSkyLights.TryRemove(coord, out var valueToDispose);
                }
            }
        }

        //seeds the flood fill lighting from a chunk and computes it
        public static void LightChunk(Chunk chunk, Chunkmanager world)
        {
            ConcurrentQueue<LightNode> blockLights = new ConcurrentQueue<LightNode>();
            ConcurrentQueue<SkyLightNode> skyLights = new ConcurrentQueue<SkyLightNode>();

            //seed block lights
            BlockLightingEngine.SeedBlockLights(chunk, blockLights);
            SkyLightingEngine.SeedSkyLights(chunk, skyLights);

            //seed deffered block lights if existing
            if (DefferedBlockLights.TryGetValue(chunk.Pos, out var defferedBlockLights))
            {
                if (defferedBlockLights == null) return;

                BlockLightingEngine.SeedDefferedLights(chunk, blockLights, defferedBlockLights);
                DefferedBlockLights.TryRemove(chunk.Pos, out var thing);
            }

            //seed deffered skylights if existing
            if (DefferedSkyLights.TryGetValue(chunk.Pos, out var defferedSkyLights))
            {
                if (defferedSkyLights == null) return;

                SkyLightingEngine.SeedDefferedSkyLights(chunk, skyLights, defferedSkyLights);
                DefferedSkyLights.TryRemove(chunk.Pos, out var thing);
            }

            SkyLightingEngine.PropagateSkyLights(world, skyLights);
            BlockLightingEngine.PropagateBlockLights(world, blockLights);
        }

        //enqueues lighting for a future chunk that may have not been loaded yet
        public static void DeferBlockLight(ChunkCoord coord, int wx, int wy, int wz, Vector3i light)
        {
            if (DefferedBlockLights.TryGetValue(coord, out var defferedLights))
            {
                Vector3i newLightn = new Vector3i(Math.Max(0, light.X - 1),
                Math.Max(0, light.Y - 1), Math.Max(0, light.Z - 1));

                if (newLightn.X <= 0 && newLightn.Y <= 0 && newLightn.Z <= 0) return;
                defferedLights.Enqueue(new LightNode(wx, wy, wz, newLightn));
            }
            else
            {
                ConcurrentQueue<LightNode> lights = new ConcurrentQueue<LightNode>();

                Vector3i newLightn = new Vector3i(Math.Max(0, light.X - 1),
                Math.Max(0, light.Y - 1), Math.Max(0, light.Z - 1));
                if (newLightn.X <= 0 && newLightn.Y <= 0 && newLightn.Z <= 0) return;

                lights.Enqueue(new LightNode(wx, wy, wz, newLightn));
                DefferedBlockLights.TryAdd(coord, lights);
            }
        }

        //enqueues skylighting for a future chunk that may have not been loaded yet
        public static void DeferSkyLight(ChunkCoord coord, int wx, int wy, int wz, byte light)
        {
            if (DefferedSkyLights.TryGetValue(coord, out var defferedSkyLights))
            {
                int newLightn = Math.Max(0, light - 1);
                if (newLightn <= 0) return;
                defferedSkyLights.Enqueue(new SkyLightNode(wx, wy, wz, (byte)newLightn));
            }
            else
            {
                ConcurrentQueue<SkyLightNode> skyLights = new ConcurrentQueue<SkyLightNode>();

                int newLightn = Math.Max(0, light - 1);
                if (newLightn <= 0) return;
                skyLights.Enqueue(new SkyLightNode(wx, wy, wz, (byte)newLightn));
                DefferedSkyLights.TryAdd(coord, skyLights);
            }
        }

        //adds a block light into the world after inital load
        public static void AddBlockLight(Chunkmanager world, Chunk chunk, BlockState state, Vector3i globalPos)
        {
            //get current light
            Vector3i lightLevel = state.GetBlock.GetLightSourceLevel(state);
            if (lightLevel == Vector3i.Zero) return;

            //convert to chunk-local coordinates and set light
            int lx = VoxelMath.Mod(globalPos.X, CHUNK_SIZE);
            int lz = VoxelMath.Mod(globalPos.Z, CHUNK_SIZE);
            chunk.SetBlockLight(lx, globalPos.Y, lz, lightLevel);            

            //add to lights
            ConcurrentQueue<LightNode> lights = new();
            lights.Enqueue(new LightNode(globalPos.X, globalPos.Y, globalPos.Z, lightLevel));
            BlockLightingEngine.PropagateBlockLights(world, lights, dirty:true);
        }

        //removes a block light from the world
        public static void RemoveBlockLight(Chunkmanager world, Chunk chunk, Vector3i globalPos)
        {
            int lx = VoxelMath.Mod(globalPos.X, CHUNK_SIZE);
            int lz = VoxelMath.Mod(globalPos.Z, CHUNK_SIZE);

            ushort packed = chunk.GetLight(lx, globalPos.Y, lz);
            Vector3i oldLight = VoxelMath.UnpackLight16Block(packed);
            if (oldLight == Vector3i.Zero) return;

            //remove the source
            chunk.SetBlockLight(lx, globalPos.Y, lz, Vector3i.Zero);

            ConcurrentQueue<RemoveLightNode> removeQueue = new();
            ConcurrentQueue<LightNode> reAddQueue = new();

            removeQueue.Enqueue(new RemoveLightNode(globalPos.X, globalPos.Y,
            globalPos.Z, oldLight));

            //propagate shadows and re-propagate surviving lights
            BlockLightingEngine.PropagateBlockLightRemoval(world, removeQueue, reAddQueue);
            BlockLightingEngine.PropagateBlockLights(world, reAddQueue, dirty: true);
        }

        //updates areas that are now under the sky
        public static void RemoveSkyLight(Chunkmanager world, Chunk chunk, Vector3i globalPos)
        {
            int lx = VoxelMath.Mod(globalPos.X, CHUNK_SIZE);
            int lz = VoxelMath.Mod(globalPos.Z, CHUNK_SIZE);

            ushort packed = chunk.GetLight(lx, globalPos.Y, lz);
            byte oldSky = VoxelMath.UnpackLight16Sky(packed);

            if (oldSky == 0) return;

            // Clear this voxel now that it blocks sky
            chunk.SetSkyLight(lx, globalPos.Y, lz, 0);

            var skyRemoveQueue = new ConcurrentQueue<RemoveSkyNode>();
            var reSkyQueue = new ConcurrentQueue<SkyLightNode>();

            //seed the removal with the old value that was at this pos
            skyRemoveQueue.Enqueue(new RemoveSkyNode(globalPos.X, globalPos.Y, globalPos.Z, oldSky));

            SkyLightingEngine.PropagateSkyLightRemoval(world, skyRemoveQueue, reSkyQueue);
            SkyLightingEngine.PropagateSkyLights(world, reSkyQueue, dirty: true);

        }

        //allows for light values to repropagate through old solid block
        public static void RemoveLightBlocker(Chunkmanager world, Vector3i globalPos)
        {
            ConcurrentQueue<LightNode> lights = new ConcurrentQueue<LightNode>();
            ConcurrentQueue<SkyLightNode> skyLights = new ConcurrentQueue<SkyLightNode>();
            Span<(int dx, int dy, int dz)> dirs =
            [
                ( 1, 0, 0), (-1, 0, 0), ( 0, 1, 0),
                ( 0,-1, 0), ( 0, 0, 1), ( 0, 0,-1)
            ];

            foreach (var (dx, dy, dz) in dirs)
            {
                int wx = globalPos.X + dx;
                int wy = globalPos.Y + dy;
                int wz = globalPos.Z + dz;
                if (wy < 0 || wy >= MAX_HEIGHT) continue;

                int cx = VoxelMath.FloorDiv(wx, CHUNK_SIZE);
                int cz = VoxelMath.FloorDiv(wz, CHUNK_SIZE);
                Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted()) continue;

                int lx = VoxelMath.Mod(wx, CHUNK_SIZE);
                int lz = VoxelMath.Mod(wz, CHUNK_SIZE);
                   
                ushort packed = chunk.GetLight(lx, wy, lz);
                Vector3i existing = VoxelMath.UnpackLight16Block(packed);
                byte existingSky = VoxelMath.UnpackLight16Sky(packed);

                if (existing != Vector3i.Zero) lights.Enqueue(new LightNode(wx, wy, wz, existing));
                if (existingSky != 0) skyLights.Enqueue(new SkyLightNode(wx, wy, wz, existingSky));
            }

            SkyLightingEngine.PropagateSkyLights(world, skyLights, dirty:true);
            BlockLightingEngine.PropagateBlockLights(world, lights, dirty:true);
        }
    }
}