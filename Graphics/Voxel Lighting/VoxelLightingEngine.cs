using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;
using System.Collections.Concurrent;
using OurCraft.utility;

namespace OurCraft.Graphics.Voxel_Lighting
{
    //has helper methods to calculate block and sky lighting for the world
    public static class VoxelLightingEngine
    {
        const int CHUNK_SIZE = Chunk.CHUNK_WIDTH; //32
        const int MAX_HEIGHT = Chunk.CHUNK_HEIGHT;

        //const vars
        public const int MAX_LIGHT = 15;
        public const int MIN_LIGHT = 0;

        //seeds the flood fill lighting from a chunk and computes it
        public static void LightChunk(Chunk chunk, ChunkManager world)
        {
            ConcurrentQueue<LightNode> blockLights = new ConcurrentQueue<LightNode>();
            ConcurrentQueue<SkyLightNode> skyLights = new ConcurrentQueue<SkyLightNode>();

            //do sky lights
            SkyLightingEngine.SeedSkyLights(world, chunk, skyLights);
            SkyLightingEngine.PropagateSkyLights(world, skyLights);

            //do block lights
            BlockLightingEngine.SeedBlockLights(world, chunk, blockLights);
            BlockLightingEngine.PropagateBlockLights(world, blockLights);
        }

        //adds a block light into the world after inital load
        public static void AddBlockLight(ChunkManager world, Chunk chunk, BlockState state, Vector3i globalPos)
        {
            //get current light
            Vector3i lightLevel = state.LightLevel;
            if (lightLevel == Vector3i.Zero) return;

            //convert to chunk-local coordinates and set light
            int lx = VoxelMath.ModPow2(globalPos.X, CHUNK_SIZE);
            int lz = VoxelMath.ModPow2(globalPos.Z, CHUNK_SIZE);
            chunk.SetBlockLight(lx, globalPos.Y, lz, lightLevel);            

            //add to lights
            ConcurrentQueue<LightNode> lights = new();
            lights.Enqueue(new LightNode(globalPos.X, globalPos.Y, globalPos.Z, lightLevel));
            BlockLightingEngine.PropagateBlockLights(world, lights, dirty:true);
        }

        //removes a block light from the world
        public static void RemoveBlockLight(ChunkManager world, Chunk chunk, Vector3i globalPos)
        {           
            int lx = VoxelMath.ModPow2(globalPos.X, CHUNK_SIZE);
            int lz = VoxelMath.ModPow2(globalPos.Z, CHUNK_SIZE);

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
        public static void RemoveSkyLight(ChunkManager world, Chunk chunk, Vector3i globalPos)
        {           
            int lx = VoxelMath.ModPow2(globalPos.X, CHUNK_SIZE);
            int lz = VoxelMath.ModPow2(globalPos.Z, CHUNK_SIZE);

            ushort packed = chunk.GetLight(lx, globalPos.Y, lz);
            byte oldSky = VoxelMath.UnpackLight16Sky(packed);

            if (oldSky == MIN_LIGHT) return;

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
        public static void RemoveLightBlocker(ChunkManager world, Vector3i globalPos)
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

                int cx = VoxelMath.FloorDivPow2(wx, CHUNK_SIZE);
                int cz = VoxelMath.FloorDivPow2(wz, CHUNK_SIZE);
                Chunk? chunk = world.GetChunk(new ChunkCoord(cx, cz));
                if (chunk == null || !chunk.HasVoxelData() || chunk.Deleted()) continue;

                int lx = VoxelMath.ModPow2(wx, CHUNK_SIZE);
                int lz = VoxelMath.ModPow2(wz, CHUNK_SIZE);
                   
                ushort packed = chunk.GetLight(lx, wy, lz);
                Vector3i existing = VoxelMath.UnpackLight16Block(packed);
                byte existingSky = VoxelMath.UnpackLight16Sky(packed);

                if (existing != Vector3i.Zero) lights.Enqueue(new LightNode(wx, wy, wz, existing));
                if (existingSky != MIN_LIGHT) skyLights.Enqueue(new SkyLightNode(wx, wy, wz, existingSky));
            }

            SkyLightingEngine.PropagateSkyLights(world, skyLights, dirty:true);
            BlockLightingEngine.PropagateBlockLights(world, lights, dirty:true);
        }
    }
}