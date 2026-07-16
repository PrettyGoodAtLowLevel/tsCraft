using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Utility;
using System.Collections.Concurrent;
using OurCraft.World.WorldGeneration;
using OurCraft.World.WorldUpdates;
using OurCraft.Entities.Internal;
using OurCraft.Physics.PhysicsData;

namespace OurCraft.World.WorldData
{
    //manages chunk generation, block getting, and block modifiying
    //uses multithreading queues to manage which chunks to generate
    public class ChunkManager
    {
        //tracking chunks
        public ConcurrentDictionary<ChunkCoord, Chunk> ChunkMap { get; private set; } = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> genQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> structureQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> meshQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> lightingQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> deletionQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> meshDeletedChunks = new();
        readonly ConcurrentDictionary<Chunk, byte> modifiedChunks = new(); 
        readonly ConcurrentDictionary<Chunk, byte> uploadedChunks = new();

        readonly ConcurrentQueue<ChunkCoord> terrainGenQueue = new();
        readonly ConcurrentQueue<ChunkCoord> structureGenQueue = new();
        readonly ConcurrentQueue<ChunkCoord> lightingQueue = new();
        readonly ConcurrentQueue<ChunkCoord> chunkMeshQueue = new();
        readonly ConcurrentQueue<Chunk> chunkUploadQueue = new();
        readonly ConcurrentQueue<ChunkCoord> deletionQueue = new();
        readonly ConcurrentQueue<ChunkCoord> meshDeletionQueue = new();
        readonly ConcurrentQueue<Chunk> modifiedQueue = new();
        readonly ConcurrentQueue<ChunkCoord> remeshQueue = new();

        //rendering
        public int RenderDistance { get; private set; } = 0;
        public int WorldDistance { get => RenderDistance + 1; }
        public int SimulationDistance { get; private set; } = 0;

        //player refrences
        ChunkCoord lastPlayerChunk;
        readonly Transform playerTracking = new();
        
        //threading
        private readonly ThreadPoolSystem terrainGenThread;
        private readonly ThreadPoolSystem lightThread;

        //block updates
        private readonly double BlockUpdateTime = PhysicsConstants.BLOCK_TICK;
        private double chunkRandomTickTimer = 0.0f;

        //constructor that needs render dist, sim dist, and threads
        public ChunkManager(int renderDistance, int simDistance, ref ThreadPoolSystem terGen, ref ThreadPoolSystem lighting)
        { 
            terrainGenThread = terGen;        
            lightThread = lighting;

            Entity? playerEnt = EntityManager.GetEntity(EntityManager.PlayerEntityName);
            if (playerEnt != null) playerTracking = playerEnt.Transform;
            
            RenderDistance = renderDistance;
            SimulationDistance = simDistance;
            Generate();
        }

        //debugs information on the stats of chunks
        public void Debug()
        {
            Console.WriteLine("total tracked chunks: " + ChunkMap.Count);
            Console.WriteLine("gen queued chunks: " + genQueuedChunks.Count);
            Console.WriteLine("structure queued chunks: " + structureQueuedChunks.Count);
            Console.WriteLine("lighting queued chunks: " + lightingQueuedChunks.Count);
            Console.WriteLine("mesh queued chunks: " + meshQueuedChunks.Count);
            Console.WriteLine("upload queued chunks: " + uploadedChunks.Count);
            Console.WriteLine("deletion queued chunks: " + deletionQueuedChunks.Count);
        }

        //spiral chunk generation starting from player
        public void Generate()
        {
            ChunkCoord playerChunk = GetPlayerChunk();
            GenerateChunk(playerChunk);

            int x = 0;
            int z = 0;

            int dx = 0;
            int dz = -1;

            int max = WorldDistance * 2 + 1;
            int maxI = max * max;

            for (int i = 0; i < maxI; i++)
            {
                if (Math.Abs(x) <= WorldDistance && Math.Abs(z) <= WorldDistance)
                {
                    ChunkCoord coord = new(playerChunk.X + x, playerChunk.Z + z);
                    GenerateChunk(coord);
                }

                //spiral turning logic
                if (x == z || x < 0 && x == -z || x > 0 && x == 1 - z)
                {
                    (dx, dz) = (-dz, dx);
                }

                x += dx;
                z += dz;
            }
        }

        //queues a chunk for the next stage chunk generation
        public void GenerateChunk(ChunkCoord coord)
        {
            if (!ChunkMap.ContainsKey(coord))
            {
                ChunkMap.TryAdd(coord, new Chunk(coord));
                TerrainGenEnqueue(coord);
            }
            else
            {
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted() || ChunkOutOfRenderDistance(coord)) return;

                if (chunk.GetState() == ChunkState.Initialized)
                {
                    TerrainGenEnqueue(coord);
                    return;
                }
                if (chunk.GetState() == ChunkState.Terrain_Set)
                {
                    StructureEnqueue(coord);
                    return;
                }
                if (chunk.GetState() == ChunkState.Structures_Placed)
                {
                    LightEnqueue(coord);
                    return;
                }
                if (chunk.GetState() == ChunkState.Lit)
                {
                    MeshEnqueue(coord);
                    return;
                }
            }
        }

        //deletes all chunks out of world distance and unrenders all chunks out of view distance
        private void UnloadFarChunks()
        {
            foreach (var pair in ChunkMap)
            {
                //full deletion
                if (ChunkOutOfWorldDistance(pair.Key))
                {
                    if (pair.Value.Deleted()) continue;
                    
                    pair.Value.MarkForDeletion();                                            
                    DeletionEnqueue(pair.Key);                 
                }
                //mesh deletion
                else if (ChunkOutOfRenderDistance(pair.Key))
                {
                    if (pair.Value.Deleted()) continue;

                    MeshDeletionEnqueue(pair.Key);
                }
            }
        }

        //build voxel data for chunks on seperate threads
        private void ProcessTerrainGenQueue()
        {
            if (terrainGenQueue.TryDequeue(out ChunkCoord coord))
            {                
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted())
                {
                    genQueuedChunks.TryRemove(coord, out byte _);
                    return;
                }

                if (chunk.generating) return;
                chunk.generating = true;

                terrainGenThread.Submit(() =>
                {
                    using (Profiler.Scope("Terrain Gen")) ChunkGenerator.CreateTerrain(chunk);

                    if (!ChunkOutOfRenderDistance(coord)) StructureEnqueue(chunk.ChunkPos);
                    genQueuedChunks.TryRemove(coord, out byte _);
                    chunk.generating = false;
                });
            }
        }

        //places the blocks of structure starts in a chunk
        private void ProcessStructureQueue()
        {
            if (structureGenQueue.TryDequeue(out ChunkCoord coord))
            {
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted())
                {
                    structureQueuedChunks.TryRemove(coord, out byte _);
                    return;
                }              

                if (ChunkHasNeighbors(coord))
                {
                    if (chunk.generating) return;
                    chunk.generating = true;

                    terrainGenThread.Submit(() =>
                     {
                         using (Profiler.Scope("Structure Gen")) ChunkGenerator.PlaceFeatures(chunk, this);

                         LightEnqueue(chunk.ChunkPos);
                         structureQueuedChunks.TryRemove(coord, out byte _);
                         chunk.generating = false;
                     });
                }
                else if (!ChunkOutOfRenderDistance(coord) && chunk != null && !chunk.Deleted())
                {
                    structureQueuedChunks.TryRemove(coord, out byte _);
                    StructureEnqueue(coord);
                }
            }
        }

        //lights chunks on a seperate thread
        private void ProcessLightQueue()
        {
            if (lightingQueue.IsEmpty) return;

            if (lightingQueue.TryDequeue(out ChunkCoord coord))
            {           
                Chunk? chunk = GetChunk(coord);               
                if (chunk == null || chunk.Deleted())
                {
                    lightingQueuedChunks.TryRemove(coord, out byte _);
                    return;
                }

                if (ChunkWorkable(coord))
                {
                    if (chunk.generating) return;
                    chunk.generating = true;

                    //light chunk, enqueue current for meshing, update neighbors
                    lightThread.Submit(() =>
                    {
                        using (Profiler.Scope("Lighting")) VoxelLightingEngine.LightChunk(chunk, this);
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
                            if (chunkd != null && CanBeRemeshed(chunkd) && ChunkHasNeighbors(chunkd.ChunkPos))
                                RemeshEnqueue(chunkd.ChunkPos);

                        lightingQueuedChunks.TryRemove(coord, out byte _);
                        chunk.generating = false;
                    });
                }
                else if (!ChunkOutOfRenderDistance(coord) && chunk != null && !chunk.Deleted())
                {
                    lightingQueuedChunks.TryRemove(coord, out byte _);
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
                    meshQueuedChunks.TryRemove(coord, out byte _);
                    return;
                }

                //check if chunk ready, then generate mesh data on seperate thread
                if (ChunkWorkable(coord))
                {
                    if (chunk.generating) return;
                    chunk.generating = true;

                    Chunk? left, right, front, back, c1, c2, c3, c4;
                    left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
                    right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
                    front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
                    back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));
                    c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1));
                    c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1));
                    c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1));
                    c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1));

                    terrainGenThread.Submit(() =>
                    {
                        using (Profiler.Scope("Mesh Gen")) ChunkBuilder.CreateChunkMesh(chunk, left, right, front, back, c1, c2, c3, c4);
                        UploadEnqueue(chunk);
                        meshQueuedChunks.TryRemove(coord, out byte _);
                        chunk.generating = false;
                    });
                }
                else if (!ChunkOutOfRenderDistance(coord) && chunk != null && !chunk.Deleted())
                {
                    meshQueuedChunks.TryRemove(coord, out byte _);
                    MeshEnqueue(coord);
                }                 
            }
        }

        //tries to add dirty chunks back to the mesh queue
        private void ProcessRemeshQueue()
        {
            if (remeshQueue.IsEmpty) return;
            
            if (remeshQueue.TryDequeue(out ChunkCoord coord))
            {
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted()) return;

                bool res = MeshEnqueue(coord);

                //if cant add to meshing queue again, retry later
                if (!res && !ChunkOutOfRenderDistance(coord))
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
                uploadedChunks.TryRemove(chunk, out byte _);
                if (chunk != null && chunk.IsLit())
                {
                    using (Profiler.Scope("GL Upload")) ChunkRenderer.GLUploadChunk(chunk);
                }
                else if (chunk != null && chunk.HasAllBlocks())
                {
                    UploadEnqueue(chunk);
                }
            }
        }

        //deletes meshes out of render distance
        private void ProcessDeletedMeshes()
        {
            while(!meshDeletionQueue.IsEmpty)
            {
                if (meshDeletionQueue.TryDequeue(out ChunkCoord coord))
                {
                    DeleteChunkMesh(coord);
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
                    DeleteChunk(coord);
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
                        modifiedChunks.TryRemove(chunk, out byte _);
                    }                                                       
                }
            }
        }

        //random ticks all chunks close to player
        private void ProcessRandomTick()
        {
            ChunkCoord playerChunk = GetPlayerChunk();

            for (int x = playerChunk.X - SimulationDistance; x <= playerChunk.X + SimulationDistance; x++)
            {
                for (int z = playerChunk.Z - SimulationDistance; z <= playerChunk.Z + SimulationDistance; z++)
                {
                    ChunkCoord coord = new(x, z);
                    Chunk? chunk = GetChunk(coord);

                    if (chunk == null || !ChunkRandomTickable(coord)) continue;
                    RandomBlockTick.TickChunk(this, chunk);
                }
            }
        }

        //load and unload chunks, and do multithreading stuff
        public void Update()
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
            ProcessTerrainGenQueue();
            ProcessStructureQueue();
            ProcessLightQueue();

            ProcessMeshQueue();
            ProcessRemeshQueue();
            ProcessChunkUploadQueue();

            ProcessModifiedChunks();
            ProcessDeletedMeshes();
            ProcessDeletedChunks();

            chunkRandomTickTimer += Time.DeltaTime;
            while(chunkRandomTickTimer >= BlockUpdateTime)
            {
                ProcessRandomTick();
                chunkRandomTickTimer -= BlockUpdateTime;
            }
        }

        //fully deletes a chunk
        public void DeleteChunk(ChunkCoord coord)
        {
            Chunk? chunk = GetChunk(coord);
            if (chunk == null) return;

            if (ChunkOutOfWorldDistance(coord))
            {
                if (ChunkMap.Remove(coord, out chunk))
                {
                    lightingQueuedChunks.Remove(coord, out byte _);
                    meshQueuedChunks.Remove(coord, out _);
                    structureQueuedChunks.Remove(coord, out _);                   
                    
                    chunk.Delete();
                    genQueuedChunks.Remove(coord, out _);
                    meshDeletedChunks.Remove(coord, out _);
                    deletionQueuedChunks.Remove(coord, out _);
                }
                else if (chunk != null)
                {
                    DeletionEnqueue(coord);
                }
            }
        }

        //only deletes the mesh of a chunk
        public void DeleteChunkMesh(ChunkCoord coord)
        {
            Chunk? chunk = GetChunk(coord);
            if (chunk == null || chunk.Deleted()) return;

            if (ChunkOutOfRenderDistance(coord))
            {
                //remove from queues, and delete vram data
                lightingQueuedChunks.Remove(coord, out byte _);
                meshQueuedChunks.Remove(coord, out _);
                structureQueuedChunks.Remove(coord, out _);
                genQueuedChunks.Remove(coord, out _);
                
                ChunkBuilder.ClearChunkMesh(chunk);
                if (chunk.IsLit()) chunk.SetState(ChunkState.Lit);
                meshDeletedChunks.Remove(coord, out _);
            }
        }

        //safely allows a chunk to be queued for voxel generation
        private void TerrainGenEnqueue(ChunkCoord coord)
        {
            if (!genQueuedChunks.ContainsKey(coord))
            {
                genQueuedChunks.TryAdd(coord, 0);
                terrainGenQueue.Enqueue(coord);
            }               
        }

        //allows a chunk to be safely enqueued for structure placement
        private void StructureEnqueue(ChunkCoord coord)
        {
            if (!structureQueuedChunks.ContainsKey(coord))
            {
                structureQueuedChunks.TryAdd(coord, 0);
                structureGenQueue.Enqueue(coord);
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

        //safely allows a chunk to be queued for mesh deletion
        private void MeshDeletionEnqueue(ChunkCoord coord)
        {
            if (!meshDeletedChunks.ContainsKey(coord))
            {
                meshDeletedChunks.TryAdd(coord, 0);
                meshDeletionQueue.Enqueue(coord);
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
            int pX = (int)Math.Floor(playerTracking.WorldPosition.X);
            int pZ = (int)Math.Floor(playerTracking.WorldPosition.Z);

            //get chunk position from world position
            int chunkX = pX / WorldConstants.CHUNK_WIDTH;
            int chunkZ = pZ / WorldConstants.CHUNK_WIDTH;

            //handle negatives properly
            if (pX < 0) chunkX -= 1;
            if (pZ < 0) chunkZ -= 1;

            return new ChunkCoord(chunkX, chunkZ);
        }

        //checks if a chunk is too far away from player and should be unrendered
        public bool ChunkOutOfRenderDistance(ChunkCoord coord)
        {
            ChunkCoord playerChunk = GetPlayerChunk();
            return Math.Abs(coord.Z - playerChunk.Z) > RenderDistance || Math.Abs(coord.X - playerChunk.X) > RenderDistance;
        }

        //checks if a chunk is too far away from a player and should be deleted
        private bool ChunkOutOfWorldDistance(ChunkCoord coord)
        {
            ChunkCoord playerChunk = GetPlayerChunk();
            return Math.Abs(coord.Z - playerChunk.Z) > WorldDistance || Math.Abs(coord.X - playerChunk.X) > WorldDistance;
        }

        //checks if a chunk can structure place, light, or mesh
        public bool ChunkWorkable(ChunkCoord coord)
        {
            Chunk? thisChunk = GetChunk(coord);
            return thisChunk != null && thisChunk.HasBlocks();
        }

        //checks if a chunk has neighbors with voxel data around them
        private bool ChunkHasNeighbors(ChunkCoord coord)
        {
            //get this chunk and adjacent chunks
            Chunk? thisChunk, left, right, back, front;
            thisChunk = GetChunk(coord);
            left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
            right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
            front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
            back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));

            //if chunks dont exist or dont have block data, then back out
            if (thisChunk == null || left == null || right == null || front == null || back == null) return false;
            if (!thisChunk.HasBlocks() || !left.HasBlocks() || !right.HasBlocks() || !front.HasBlocks() || !back.HasBlocks()) return false;

            //get corners as well
            Chunk? backLeft, backRight, frontLeft, frontRight;
            backLeft = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1));
            backRight = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1));
            frontLeft = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1));
            frontRight = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1));

            if (backLeft == null || backRight == null || frontLeft == null || frontRight == null) return false;
            if (!backLeft.HasBlocks() || !backRight.HasBlocks() || !frontLeft.HasBlocks() || !frontRight.HasBlocks()) return false;

            return true;
        }

        //checks if a chunk can be remeshed by the remesh queue, not by the player
        static bool CanBeRemeshed(Chunk chunk)
        {
            return chunk.GetState() == ChunkState.Mesh_Built || chunk.GetState() == ChunkState.Render_Ready;
        }

        //checks if a chunk has neighbors and is in range of random tick
        private bool ChunkRandomTickable(ChunkCoord coord)
        {
            //get this chunk and adjacent chunks
            Chunk? thisChunk, left, right, back, front;
            thisChunk = GetChunk(coord);
            left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
            right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
            front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
            back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));

            //if chunks dont exist or dont have block data, then back out
            if (thisChunk == null || left == null || right == null || front == null || back == null) return false;
            if (!thisChunk.Modifiyable() || !left.Modifiyable() || !right.Modifiyable() || !front.Modifiyable() || !back.Modifiyable()) return false;

            //get corners as well
            Chunk? backLeft, backRight, frontLeft, frontRight;
            backLeft = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1));
            backRight = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1));
            frontLeft = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1));
            frontRight = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1));

            if (backLeft == null || backRight == null || frontLeft == null || frontRight == null) return false;
            if (!backLeft.Modifiyable() || !backRight.Modifiyable() || !frontLeft.Modifiyable() || !frontRight.Modifiyable()) return false;

            return true;
        }

        //gets block state from global position, invalid positions are treated as air
        public BlockState GetBlockState(Vector3d pos)
        {
            //world to chunk coord
            int chunkX = (int)Math.Floor(pos.X / WorldConstants.CHUNK_WIDTH);
            int chunkZ = (int)Math.Floor(pos.Z / WorldConstants.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            if (chunk == null) return Block.AIR;

            //get local coords
            int lx = ModPow2((int)Math.Floor(pos.X), WorldConstants.CHUNK_WIDTH);
            int ly = (int)Math.Floor(pos.Y);
            int lz = ModPow2((int)Math.Floor(pos.Z), WorldConstants.CHUNK_WIDTH);

            return chunk.GetBlockStateSafe(lx, ly, lz);
        }

        //helper to try to fetch block entities in a chunk
        public BlockEntity? TryGetBlockEntity(Vector3d pos)
        {
            int chunkX = (int)Math.Floor(pos.X / WorldConstants.CHUNK_WIDTH);
            int chunkZ = (int)Math.Floor(pos.Z / WorldConstants.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));
            if (chunk == null || chunk.GetState() != ChunkState.Render_Ready) return null;

            //get local coords
            int lx = ModPow2((int)Math.Floor(pos.X), WorldConstants.CHUNK_WIDTH);
            int ly = (int)Math.Floor(pos.Y);
            int lz = ModPow2((int)Math.Floor(pos.Z), WorldConstants.CHUNK_WIDTH);

            int localIndex = VoxelMath.ToBlockEntityIndex(lx, ly, lz);
            chunk.TryGetBlockEntity(localIndex, out BlockEntity? entity);
            return entity;
        }

        //get the light in a chunk
        public ushort GetLight(Vector3d pos)
        {
            //world to chunk coord
            int chunkX = (int)Math.Floor(pos.X / WorldConstants.CHUNK_WIDTH);
            int chunkZ = (int)Math.Floor(pos.Z / WorldConstants.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            if (chunk == null) return 0;

            //get local coords
            int lx = ModPow2((int)Math.Floor(pos.X), WorldConstants.CHUNK_WIDTH);
            int ly = (int)Math.Floor(pos.Y);
            int lz = ModPow2((int)Math.Floor(pos.Z), WorldConstants.CHUNK_WIDTH);

            return chunk.GetLight(lx, ly, lz);
        }

        //get skylight from global pos
        public byte GetSkyLight(Vector3d globalPos)
        {
            //world to chunk coord
            int chunkX = (int)Math.Floor(globalPos.X / WorldConstants.CHUNK_WIDTH);
            int chunkZ = (int)Math.Floor(globalPos.Z / WorldConstants.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            if (chunk == null || chunk.GetState() != ChunkState.Render_Ready) return 15;

            //get local coords
            int lx = ModPow2((int)Math.Floor(globalPos.X), WorldConstants.CHUNK_WIDTH);
            int ly = (int)Math.Floor(globalPos.Y);
            int lz = ModPow2((int)Math.Floor(globalPos.Z), WorldConstants.CHUNK_WIDTH);
            if (ly > WorldConstants.CHUNK_HEIGHT) return 15;

            return VoxelMath.UnpackLight16Sky(chunk.GetLight(lx, ly, lz));
        }

        //setblock that doesnt handle block entities, use carefully
        public void SetBlockState(Vector3d pos, BlockState state)
        {
            if (pos.Y < 0 || pos.Y >= WorldConstants.CHUNK_HEIGHT) return;
            //world to chunk coord
            int chunkX = (int)Math.Floor(pos.X / WorldConstants.CHUNK_WIDTH);
            int chunkZ = (int)Math.Floor(pos.Z / WorldConstants.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            //get local chunk positions
            int lx = ModPow2((int)Math.Floor(pos.X), WorldConstants.CHUNK_WIDTH);
            int ly = (int)Math.Floor(pos.Y);
            int lz = ModPow2((int)Math.Floor(pos.Z), WorldConstants.CHUNK_WIDTH);
            if (chunk == null || chunk.GetState() != ChunkState.Render_Ready) return;

            //update light
            BlockState prev = chunk.GetBlockStateSafe(lx, ly, lz);
            chunk.SetBlockState(lx, ly, lz, state);
            
            if (prev.IsLightSource) VoxelLightingEngine.RemoveBlockLight(this, chunk, (Vector3i)pos);
            if (!state.LightPassable) VoxelLightingEngine.RemoveBlockLight(this, chunk, (Vector3i)pos);              
            if (state.SkyLightAttenuation != VoxelLightingEngine.MIN_LIGHT) VoxelLightingEngine.RemoveSkyLight(this, chunk, (Vector3i)pos);
            else if (!prev.LightPassable || prev.SkyLightAttenuation != VoxelLightingEngine.MAX_LIGHT) VoxelLightingEngine.RemoveLightBlocker(this, (Vector3i)pos);

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

        //setblock with handling block entities
        public void SetBlock(Vector3d pos, BlockState state, BlockUpdateFlags updateFlags = BlockUpdateFlags.FULL_REBUILD)
        {   
            if (pos.Y < 0 || pos.Y >= WorldConstants.CHUNK_HEIGHT) return;
            //world to chunk coord
            int chunkX = (int)Math.Floor(pos.X / WorldConstants.CHUNK_WIDTH);
            int chunkZ = (int)Math.Floor(pos.Z / WorldConstants.CHUNK_WIDTH);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            //get local chunk positions
            int lx = ModPow2((int)Math.Floor(pos.X), WorldConstants.CHUNK_WIDTH);
            int ly = (int)Math.Floor(pos.Y);
            int lz = ModPow2((int)Math.Floor(pos.Z), WorldConstants.CHUNK_WIDTH);
            if (chunk == null || chunk.GetState() != ChunkState.Render_Ready) return;
        
            BlockState prev = chunk.GetBlockStateSafe(lx, ly, lz);
            chunk.SetBlock(new Vector3i(lx, ly, lz), state);
            if (updateFlags == BlockUpdateFlags.INTERNAL) return; //opt out if only internal state change, no model/shape change

            //update light if full rebuild
            if (updateFlags == BlockUpdateFlags.FULL_REBUILD)
            {
                if (prev.IsLightSource) VoxelLightingEngine.RemoveBlockLight(this, chunk, (Vector3i)pos);
                if (!state.LightPassable) VoxelLightingEngine.RemoveBlockLight(this, chunk, (Vector3i)pos);
                if (state.SkyLightAttenuation != VoxelLightingEngine.MIN_LIGHT) VoxelLightingEngine.RemoveSkyLight(this, chunk, (Vector3i)pos);
                else if (!prev.LightPassable || prev.SkyLightAttenuation != VoxelLightingEngine.MAX_LIGHT) VoxelLightingEngine.RemoveLightBlocker(this, (Vector3i)pos);
                if (state.IsLightSource) VoxelLightingEngine.AddBlockLight(this, chunk, state, (Vector3i)pos);
            }

            //if mesh only rebuild or full rebuild update mesh
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
            const int c = WorldConstants.CHUNK_WIDTH - 1;

            int gx = pos.X;
            int gy = pos.Y;
            int gz = pos.Z;
            
            int lx = gx & c;
            int lz = gz & c;
            int ly = gy; //y is already chunk-local
            chunk.changes.Add(ly);
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
            if (lx == 0 && left != null) left.changes.Add(ly);
            if (lx == c && right != null) right.changes.Add(ly);

            //z neighbors
            if (lz == 0 && back != null) back.changes.Add(ly);
            if (lz == c && front != null) front.changes.Add(ly);

            //corners
            if (lx == 0 && lz == 0 && leftBack != null) leftBack.changes.Add(ly);
            if (lx == 0 && lz == c && leftFront != null) leftFront.changes.Add(ly);
            if (lx == c && lz == 0 && rightBack != null) rightBack.changes.Add(ly);
            if (lx == c && lz == c && rightFront != null) rightFront.changes.Add(ly);
        }

        //tries to remesh a chunk
        private void RemeshChunk(Chunk? chunk)
        {
            Chunk? left, right, back, front, c1, c2, c3, c4;
            ChunkCoord coord;

            if (chunk != null) coord = chunk.ChunkPos;          
            else return;
            if (!chunk.Modifiyable()) return;
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

            ChunkBuilder.RemeshChunk(chunk, left, right, front, back, c1, c2, c3, c4);
        }

        //quick mod function only with powers of 2
        public static int ModPow2(int a, int b){ return (a & b-1) + b & b-1; }
    }
}  