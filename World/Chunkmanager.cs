using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Graphics.Voxel_Lighting;
using OurCraft.utility;
using OurCraft.Blocks.Block_Properties;
using System.Collections.Concurrent;
using OurCraft.World.Helpers;
using OurCraft.Physics;
using OurCraft.Entities;

namespace OurCraft.World
{
    //manages chunk generation, block getting, and block modifiying
    //uses multithreading queues to manage which chunks to generate
    public class ChunkManager
    {
        //tracking chunks
        public ConcurrentDictionary<ChunkCoord, Chunk> ChunkMap { get; private set; } = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> genQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> meshQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> lightingQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> deletionQueuedChunks = new();
        readonly ConcurrentDictionary<Chunk, byte> modifiedChunks = new(); 
        readonly ConcurrentDictionary<Chunk, byte> uploadedChunks = new();

        readonly ConcurrentQueue<ChunkCoord> chunkGenQueue = new();
        readonly ConcurrentQueue<ChunkCoord> lightingQueue = new();
        readonly ConcurrentQueue<ChunkCoord> chunkMeshQueue = new();
        readonly ConcurrentQueue<Chunk> chunkUploadQueue = new();
        readonly ConcurrentQueue<ChunkCoord> deletionQueue = new();
        readonly ConcurrentQueue<Chunk> modifiedQueue = new(); //player modifications
        readonly ConcurrentQueue<ChunkCoord> remeshQueue = new();

        //rendering
        public int RenderDistance { get; private set; } = 0;
        public float MaxChunkBuildPerFrame { get; private set; } = 0.01f;
        float chunkUploadTimer = 0;

        //player refrences
        ChunkCoord lastPlayerChunk;
        readonly Transform playerTracking = new();
        
        //threading
        private readonly ThreadPoolSystem threadPool;
        private readonly ThreadPoolSystem lightThread;

        public ChunkManager(RenderDistances renderDistance, ref ThreadPoolSystem tp, ref ThreadPoolSystem lighting)
        { 
            threadPool = tp;
            lightThread = lighting;

            Entity? playerEnt = EntityManager.GetEntity(EntityManager.PlayerEntityName);
            if (playerEnt != null) playerTracking = playerEnt.Transform;
            
            int worldSize = renderDistance.GetHashCode() + 2;
            RenderDistance = worldSize;               
            lastPlayerChunk = new ChunkCoord(0, 0);        
        }

        public void Debug()
        {
            Console.WriteLine("total tracked chunks: " + ChunkMap.Count);
            Console.WriteLine("gen queued chunks: " + genQueuedChunks.Count);
            Console.WriteLine("lighting queued chunks: " + lightingQueuedChunks.Count);
            Console.WriteLine("mesh queued chunks: " + meshQueuedChunks.Count);
            Console.WriteLine("upload queued chunks: " + uploadedChunks.Count);
            Console.WriteLine("deletion queued chunks: " + deletionQueuedChunks.Count);           
        }

        public void Generate()
        {
            ChunkCoord playerChunk = GetPlayerChunk();

            List<ChunkCoord> toGenerate = [];

            for (int x = playerChunk.X - RenderDistance; x <= playerChunk.X + RenderDistance; x++)
            {
                for (int z = playerChunk.Z - RenderDistance; z <= playerChunk.Z + RenderDistance; z++)
                {
                    toGenerate.Add(new ChunkCoord(x, z));
                }
            }

            //sort by distance from player chunk
            toGenerate.Sort((a, b) =>
            {
                int da = Math.Abs(a.X - playerChunk.X) + Math.Abs(a.Z - playerChunk.Z);
                int db = Math.Abs(b.X - playerChunk.X) + Math.Abs(b.Z - playerChunk.Z);
                return da.CompareTo(db);
            });

            //generate in correct order
            foreach (var coord in toGenerate)
            {
                if (!ChunkMap.ContainsKey(coord))
                {
                    ChunkMap.TryAdd(coord, new Chunk(coord));
                    GenerationEnqueue(coord);
                }
            }
        }

        private void UnloadFarChunks()
        {
            ChunkCoord playerChunk = GetPlayerChunk();
            foreach (var pair in ChunkMap)
            {
                //full deletion
                if (ChunkOutOfRenderDistance(pair.Key, playerChunk))
                {
                    if (pair.Value.Deleted()) return;
                    
                    pair.Value.MarkForDeletion();                                            
                    DeletionEnqueue(pair.Key);                 
                }
            }
        }

        //build voxel data for chunks on seperate threads
        private void ProcessChunkGenQueue()
        {
            if (chunkGenQueue.TryDequeue(out ChunkCoord coord))
            {
                genQueuedChunks.TryRemove(coord, out byte thing);

                Chunk? chunk = GetChunk(coord);
                if (chunk == null) return;

                threadPool.Submit(() =>
                {
                    ChunkGenerator.GenerateBlocks(chunk);
                    LightEnqueue(chunk.ChunkPos);
                });
            }
        }

        //lights chunks on a seperate thread
        private void ProcessLightQueue()
        {
            if (lightingQueue.IsEmpty) return;

            ChunkCoord playerChunk = GetPlayerChunk();

            if (lightingQueue.TryDequeue(out ChunkCoord coord))
            {           
                Chunk? chunk = GetChunk(coord);               
                if (chunk == null || chunk.Deleted())
                {
                    lightingQueuedChunks.TryRemove(coord, out byte thing);
                    return;
                }

                if (ChunkWorkable(coord))
                {
                    //light chunk, enqueue current for meshing, update neighbors
                    lightThread.Submit(() =>
                    {
                        VoxelLightingEngine.LightChunk(chunk, this);
                        MeshEnqueue(coord);
                        
                        //get chunks and update them if already built
                        Chunk? left, right, front, back, c1, c2, c3, c4;
                        left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
                        right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
                        front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
                        back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));
                        c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1));
                        c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1));
                        c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1));
                        c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1));
                        Chunk?[] chunks = [left, right, front, back, c1, c2, c3, c4];

                        foreach (var chunkd in chunks)
                            if (chunkd != null && CanBeRemeshed(chunkd))
                                RemeshEnqueue(chunkd.ChunkPos);

                        lightingQueuedChunks.TryRemove(coord, out byte thing);
                    });
                }
                else if (!ChunkOutOfRenderDistance(coord, playerChunk) && chunk != null && !chunk.Deleted())
                {
                    lightingQueuedChunks.TryRemove(coord, out byte thing);
                    LightEnqueue(coord);
                }               
            }
        }

        //generates mesh for chunks on seperate threads
        private void ProcessMeshQueue()
        {
            if (chunkMeshQueue.IsEmpty) return;

            ChunkCoord playerChunk = GetPlayerChunk();

            if (chunkMeshQueue.TryDequeue(out ChunkCoord coord))
            {            
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted())
                {
                    meshQueuedChunks.TryRemove(coord, out byte thing);
                    return;
                }

                //check if chunk ready, then generate mesh data on seperate thread
                if (ChunkWorkable(coord))
                {
                    Chunk? left, right, front, back, c1, c2, c3, c4;
                    left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
                    right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
                    front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
                    back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));
                    c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1));
                    c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1));
                    c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1));
                    c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1));

                    threadPool.Submit(() =>
                    {
                        ChunkBuilder.CreateChunkMesh(chunk, left, right, front, back, c1, c2, c3, c4);
                        UploadEnqueue(chunk);
                        meshQueuedChunks.TryRemove(coord, out byte thing);
                    });
                }
                else if (!ChunkOutOfRenderDistance(coord, playerChunk) && chunk != null && !chunk.Deleted())
                {
                    meshQueuedChunks.TryRemove(coord, out byte thing);
                    MeshEnqueue(coord);
                }                 
            }
        }

        //tries to add dirty chunks back to the mesh queue
        private void ProcessRemeshQueue()
        {
            if (remeshQueue.IsEmpty) return;

            ChunkCoord playerChunk = GetPlayerChunk();
            
            if (remeshQueue.TryDequeue(out ChunkCoord coord))
            {
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted()) return;

                bool res = MeshEnqueue(coord);

                //if cant add to meshing queue again, retry later
                if (!res && !ChunkOutOfRenderDistance(coord, playerChunk))
                {
                    RemeshEnqueue(coord);
                }
            }            
        }

        //send mesh data to gpu on main thread
        private void ProcessChunkUploadQueue()
        {        
            //try to pop from upload queue
            if (chunkUploadQueue.TryDequeue(out Chunk? chunk))
            {
                uploadedChunks.TryRemove(chunk, out byte thing);
                if (chunk != null && chunk.HasVoxelData() && chunkUploadTimer > MaxChunkBuildPerFrame)
                {
                    ChunkRenderer.GLUploadChunk(chunk);                
                    chunkUploadTimer = 0;
                }
                else if (chunk != null && chunk.HasVoxelData())
                {
                    UploadEnqueue(chunk);
                }
            }
        }

        //deletes chunks out of bounds
        private void ProcessDeletedChunks()
        {
            while (!deletionQueue.IsEmpty)
            {
                if (deletionQueue.TryDequeue(out ChunkCoord coord))
                {
                    Chunk? chunk = GetChunk(coord);
                    //if chunk is null, then its already deleted and we can skip
                    if (chunk == null) continue;

                    if (ChunkMap.Remove(coord, out chunk))
                    {
                        //remove from queues, and delete vram data
                        deletionQueuedChunks.Remove(coord, out byte thing);
                        lightingQueuedChunks.Remove(coord, out thing);
                        meshQueuedChunks.Remove(coord, out thing);
                        chunk.Delete();
                    }
                    else
                    {
                        DeletionEnqueue(coord);
                    }
                }
            }         
        }

        //remeshes all dirty chunks on the next frame
        private void ProcessModifiedChunks()
        {
            if (modifiedChunks.IsEmpty) return;

            while(!modifiedQueue.IsEmpty)
            {
                if(modifiedQueue.TryDequeue(out Chunk? chunk))
                {
                    if (chunk != null)
                    {
                        RemeshChunk(chunk);
                        modifiedChunks.TryRemove(chunk, out byte thing);
                    }                                                       
                }
            }
        }

        //load and unload chunks, and do multithreading stuff
        public void Update(float time)
        {        
            ChunkCoord currentPlayerChunk = GetPlayerChunk();
            //moved between chunks
            if (currentPlayerChunk.X != lastPlayerChunk.X || currentPlayerChunk.Z != lastPlayerChunk.Z)
            {
                UnloadFarChunks();
                Generate();
                lastPlayerChunk = currentPlayerChunk;
            }

            //manage chunks
            ProcessChunkGenQueue();
            ProcessLightQueue();
            ProcessMeshQueue();
            ProcessRemeshQueue();
            ProcessChunkUploadQueue();

            ProcessModifiedChunks();
            ProcessDeletedChunks();

            chunkUploadTimer += time;
        }

        //safely allows a chunk to be queued for voxel generation
        private void GenerationEnqueue(ChunkCoord coord)
        {
            if (!genQueuedChunks.ContainsKey(coord))
            {
                genQueuedChunks.TryAdd(coord, 0);
                chunkGenQueue.Enqueue(coord);
            }               
        }

        //safely allows a chunk to be queued for trying mesh generation
        private void LightEnqueue(ChunkCoord coord)
        {
            if (!lightingQueuedChunks.ContainsKey(coord))
            {
                lightingQueuedChunks.TryAdd(coord, 0);
                lightingQueue.Enqueue(coord);
            }              
        }

        //safely allows a chunk to be queued for mesh generation
        public bool MeshEnqueue(ChunkCoord coord)
        {
            if (!meshQueuedChunks.ContainsKey(coord))
            {
                meshQueuedChunks.TryAdd(coord, 0);
                chunkMeshQueue.Enqueue(coord);
                return true;
            }
            return false;
        }

        //safely allows a chunk to be queued for remeshing
        public void RemeshEnqueue(ChunkCoord coord)
        {
            remeshQueue.Enqueue(coord);
        }

        //safely allows a chunk to be queued for gpu upload
        private void UploadEnqueue(Chunk? chunk)
        {
            if (chunk == null) return;

            if (!uploadedChunks.ContainsKey(chunk))
            {
                uploadedChunks.TryAdd(chunk, 0);
                chunkUploadQueue.Enqueue(chunk);
            }             
        }

        //safely allows a chunk to be queued for deletion
        private void DeletionEnqueue(ChunkCoord coord)
        {
            if (!deletionQueuedChunks.ContainsKey(coord))
            {
                deletionQueuedChunks.TryAdd(coord, 0);
                deletionQueue.Enqueue(coord);
            }                        
        }

        //makes a chunk ready for remeshing
        private void ModifyEnqueue(Chunk? chunk)
        {
            if (chunk == null) return;

            if (!modifiedChunks.ContainsKey(chunk))
            {
                modifiedChunks.TryAdd(chunk, 0);
                modifiedQueue.Enqueue(chunk);
            }           
        }

        //safely fetch a chunk from chunk coordinates
        public Chunk? GetChunk(ChunkCoord coord)
        {
            if (ChunkMap.TryGetValue(coord, out Chunk? value)) return value;            
            return null;
        }

        //gets the chunk the player is in
        public ChunkCoord GetPlayerChunk()
        {
            int pX = (int)Math.Floor(playerTracking.position.X);
            int pZ = (int)Math.Floor(playerTracking.position.Z);

            //get chunk position from world position
            int chunkX = pX / Chunk.CHUNK_WIDTH;
            int chunkZ = pZ / Chunk.CHUNK_WIDTH;

            //handle negatives properly
            if (pX < 0) chunkX -= 1;
            if (pZ < 0) chunkZ -= 1;

            return new ChunkCoord(chunkX, chunkZ);
        }

        //checks if a chunk is too far away from player and should be unrendered
        private bool ChunkOutOfRenderDistance(ChunkCoord coord, ChunkCoord playerChunk)
        {
            return MathF.Abs(coord.Z - playerChunk.Z) > RenderDistance || MathF.Abs(coord.X - playerChunk.X) > RenderDistance;
        }

        //checks if a chunk can light or mesh
        public bool ChunkWorkable(ChunkCoord coord)
        {
            Chunk? thisChunk = GetChunk(coord);

            if (thisChunk == null) return false;
            if (!thisChunk.HasVoxelData()) return false;

            return true;
        }

        //checks if a chunk can be remeshed by the remesh queue, not by the player
        static bool CanBeRemeshed(Chunk chunk)
        {
            return chunk.GetState() == ChunkState.Meshed || chunk.GetState() == ChunkState.Built;
        }

        //get block state
        public BlockState GetBlockState(Vector3 pos)
        {
            //world to chunk coord
            int chunkX = (int)MathF.Floor(pos.X / Chunk.CHUNK_WIDTH);
            int chunkZ = (int)MathF.Floor(pos.Z / Chunk.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            if (chunk == null) return Block.INVALID;

            //get local coords
            int lx = ModPow2((int)MathF.Floor(pos.X), Chunk.CHUNK_WIDTH);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = ModPow2((int)MathF.Floor(pos.Z), Chunk.CHUNK_WIDTH);

            return chunk.GetBlockSafe(lx, ly, lz);
        }

        //get the light in a chunk
        public ushort GetLight(Vector3 pos)
        {
            //world to chunk coord
            int chunkX = (int)MathF.Floor(pos.X / Chunk.CHUNK_WIDTH);
            int chunkZ = (int)MathF.Floor(pos.Z / Chunk.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            if (chunk == null) return 0;

            //get local coords
            int lx = ModPow2((int)MathF.Floor(pos.X), Chunk.CHUNK_WIDTH);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = ModPow2((int)MathF.Floor(pos.Z), Chunk.CHUNK_WIDTH);

            return chunk.GetLight(lx, ly, lz);
        }

        //try set block in a chunk
        public void SetBlock(Vector3 pos, BlockState state)
        {
            if (pos.Y < 0 || pos.Y >= Chunk.CHUNK_HEIGHT) return;
            //world to chunk coord
            int chunkX = (int)MathF.Floor(pos.X / Chunk.CHUNK_WIDTH);
            int chunkZ = (int)MathF.Floor(pos.Z / Chunk.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            //get local chunk positions
            int lx = ModPow2((int)MathF.Floor(pos.X), Chunk.CHUNK_WIDTH);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = ModPow2((int)MathF.Floor(pos.Z), Chunk.CHUNK_WIDTH);
            if (chunk == null || chunk.GetState() != ChunkState.Built) return;

            //update light
            BlockState prev = chunk.GetBlockSafe(lx, ly, lz);
            chunk.SetBlock(lx, ly, lz, state);
            
            if (prev.IsLightSource) VoxelLightingEngine.RemoveBlockLight(this, chunk, (Vector3i)pos);

            if (!state.LightPassable) VoxelLightingEngine.RemoveBlockLight(this, chunk, (Vector3i)pos);
                  
            if (state.SkyLightAttenuation != VoxelLightingEngine.MIN_LIGHT)
                VoxelLightingEngine.RemoveSkyLight(this, chunk, (Vector3i)pos);

            else if (!prev.LightPassable || prev.SkyLightAttenuation != VoxelLightingEngine.MAX_LIGHT)
                VoxelLightingEngine.RemoveLightBlocker(this, (Vector3i)pos);

            if (state.IsLightSource) VoxelLightingEngine.AddBlockLight(this, chunk, state, (Vector3i)pos);

            //chunks need to be updated
            MarkPosDirty((Vector3i)pos, chunk);
            ModifyEnqueue(chunk);
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX, chunkZ + 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX, chunkZ - 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ + 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ - 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ + 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ - 1)));
        }

        //marks a block position as dirty and marks all chunks touching that block position as dirty
        public void MarkPosDirty(Vector3i pos, Chunk chunk)
        {
            const int c = Chunk.CHUNK_WIDTH - 1;

            int gx = (int)MathF.Floor(pos.X);
            int gy = (int)MathF.Floor(pos.Y);
            int gz = (int)MathF.Floor(pos.Z);
            
            int lx = gx & c;
            int lz = gz & c;
            int ly = gy; //y is already chunk-local
            chunk.changes.Add(new Vector3i(lx, ly, lz));
            ChunkCoord cc = chunk.ChunkPos;

            Chunk? right = GetChunk(new ChunkCoord(cc.X + 1, cc.Z));
            Chunk? left = GetChunk(new ChunkCoord(cc.X - 1, cc.Z));
            Chunk? front = GetChunk(new ChunkCoord(cc.X, cc.Z + 1));
            Chunk? back = GetChunk(new ChunkCoord(cc.X, cc.Z - 1));

            Chunk? rightFront = GetChunk(new ChunkCoord(cc.X + 1, cc.Z + 1));
            Chunk? rightBack = GetChunk(new ChunkCoord(cc.X + 1, cc.Z - 1));
            Chunk? leftFront = GetChunk(new ChunkCoord(cc.X - 1, cc.Z + 1));
            Chunk? leftBack = GetChunk(new ChunkCoord(cc.X - 1, cc.Z - 1));

            //x neighbors
            if (lx == 0 && left != null) left.changes.Add(new Vector3i(c, ly, lz));
            if (lx == c && right != null) right.changes.Add(new Vector3i(0, ly, lz));

            //z neighbors
            if (lz == 0 && back != null) back.changes.Add(new Vector3i(lx, ly, c));
            if (lz == c && front != null) front.changes.Add(new Vector3i(lx, ly, 0));

            //corners
            if (lx == 0 && lz == 0 && leftBack != null) leftBack.changes.Add(new Vector3i(c, ly, c));
            if (lx == 0 && lz == c && leftFront != null) leftFront.changes.Add(new Vector3i(c, ly, 0));
            if (lx == c && lz == 0 && rightBack != null) rightBack.changes.Add(new Vector3i(0, ly, c));
            if (lx == c && lz == c && rightFront != null) rightFront.changes.Add(new Vector3i(0, ly, 0));
        }

        //tries to remesh a chunk
        private void RemeshChunk(Chunk? chunk)
        {
            Chunk? left, right, back, front, c1, c2, c3, c4;
            ChunkCoord coord;

            if (chunk != null) coord = chunk.ChunkPos;
            else return;
            if (chunk.changes.Count == 0) return;
            int chunkX = coord.X;
            int chunkZ = coord.Z;

            left = GetChunk(new ChunkCoord(chunkX - 1, chunkZ));
            right = GetChunk(new ChunkCoord(chunkX + 1, chunkZ));
            front = GetChunk(new ChunkCoord(chunkX, chunkZ + 1));
            back = GetChunk(new ChunkCoord(chunkX, chunkZ - 1));

            c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1));
            c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1));
            c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1));
            c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1));

            if (left == null || right == null || front == null || back == null) return;
            if (c1 == null || c2 == null || c3 == null || c4 == null) return;

            if (!chunk.Modifiyable() || !left.Modifiyable() || !right.Modifiyable() || !front.Modifiyable() || !back.Modifiyable()) return;   
            if (!c1.Modifiyable() || !c2.Modifiyable() || !c3.Modifiyable() || !c4.Modifiyable()) return;

            ChunkBuilder.RemeshChunk(chunk, left, right, front, back, c1, c2, c3, c4);
        }

        //quick mod function only with powers of 2
        public static int ModPow2(int a, int b)
        {
            return ((a & (b-1)) + b) & (b-1);
        }
    }
}  