using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Rendering;
using OurCraft.utility;
using OurCraft.Blocks.Block_Properties;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace OurCraft.World
{
    //contains all the types of render distances in the game that are possible
    public enum RenderDistances
    {
        TWO_CHUNKS,
        THREE_CHUNKS,
        FOUR_CHUNKS,
        FIVE_CHUNKS,
        SIX_CHUNKS,
        SEVEN_CHUNKS,
        EIGHT_CHUNKS,
        NINE_CHUNKS,
        TEN_CHUNKS,
        ELEVEN_CHUNKS,
        TWELVE_CHUNKS,
        THIRTEEN_CHUNKS,
        FOURTEEN_CHUNKS,
        FIFTEEN_CHUNKS,
        SIXTEEN_CHUNKS,
    }

    //manages chunk generation
    public class Chunkmanager
    {
        //---explanation---

        //the chunk manager loads chunks around the player in a render distance + 1 area square
        //chunks with all of their neighbors voxel data generated, start making their mesh
        //to ensure all chunks in render distance can make mesh data, we generate an extra ring chunks, that have no mesh, but voxel data for the chunks in render distance to mesh

        //---data---

        //all the chunks - chunkMap
        //chunks trying to make voxel data - chunkGenQueue
        //chunks trying to mesh - tryMeshQueue
        //chunks ready to mesh - meshQueue
        //chunks ready for openGL commands - uploadQueue
        //chunks to delete mesh - meshDeletionQueue
        //chunks to fully delete - deletedQueue
        //each queue has a set/dictionary corresponding to them so no copies happen

        //containers (no copies), the byte is reduntant in the other dictionaries
        public ConcurrentDictionary<ChunkCoord, Chunk> ChunkMap { get; private set; } = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> meshQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> lightingQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> deletionQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> meshDeletionQueuedChunks = new();
        readonly ConcurrentDictionary<Chunk, byte> remeshQueuedChunks = new();

        //chunk queues (actual queues)
        readonly ConcurrentQueue<ChunkCoord> chunkGenQueue = new();
        readonly ConcurrentQueue<ChunkCoord> lightingQueue = new();
        readonly ConcurrentQueue<ChunkCoord> chunkMeshQueue = new();
        readonly ConcurrentQueue<Chunk> chunkUploadQueue = new();
        readonly ConcurrentQueue<ChunkCoord> meshDeletionQueue = new();
        readonly ConcurrentQueue<ChunkCoord> deletionQueue = new();
        readonly ConcurrentQueue<Chunk> remeshQueue = new();

        //world settings
        public int RenderDistance { get; private set; } = 0; //how far chunks draw
        public int WorldDistance { get; private set; } = 0; //how far chunks generate
        public float MaxChunkBuildPerFrame { get; private set; } = 0.0001f;
        float chunkUploadTimer = 0;

        //refrences
        Camera player;
        ChunkCoord lastPlayerChunk;
        private readonly ThreadPoolSystem threadPool;
        private readonly ThreadPoolSystem lightThread;

        //basic constructor
        public Chunkmanager(RenderDistances renderDistance, ref Camera player, ref ThreadPoolSystem tp, ref ThreadPoolSystem lighting)
        { 
            this.player = player;
            threadPool = tp;
            lightThread = lighting;
            int worldSize = renderDistance.GetHashCode() + 2;
            RenderDistance = worldSize;
            WorldDistance = worldSize + 1;                 
            lastPlayerChunk = new ChunkCoord(0, 0);        
        }

        //generate chunks around the player
        public void Generate()
        {
            ChunkCoord playerChunk = GetPlayerChunk();

            for (int x = playerChunk.X - WorldDistance; x <= playerChunk.X + WorldDistance; x++)
            {
                for (int z = playerChunk.Z - WorldDistance; z <= playerChunk.Z + WorldDistance; z++)
                {
                    ChunkCoord coord = new(x, z);
                    if (ChunkMap.TryAdd(coord, new Chunk(coord)))
                    {
                        chunkGenQueue.Enqueue(coord);
                    }
                }
            }
        }

        //delete far away chunks
        private void UpdateFarChunks()
        {
            ChunkCoord playerChunk = GetPlayerChunk();
            foreach (var pair in ChunkMap)
            {
                //full deletion
                if (ChunkOutOfBounds(pair.Key, playerChunk))
                {
                    if (pair.Value.GetState() != ChunkState.Deleted)
                    {
                        pair.Value.MarkForDeletion();
                        DeletionEnqueue(pair.Key);
                    }
                }

                //mesh deletion
                else if (ChunkOutOfRenderDistance(pair.Key, playerChunk))
                {
                    if (pair.Value.GetState() == ChunkState.Meshed || pair.Value.GetState() == ChunkState.Built)
                    {
                        pair.Value.SetVoxelOnly();
                        MeshDeletionEnqueue(pair.Key);
                    }
                }

                //update chunks that arent meshed but should be
                else
                {
                    if (pair.Value.GetState() == ChunkState.VoxelOnly)
                    {
                        LightEnqueue(pair.Key);
                    }
                }
            }
        }

        //tries to add a chunk to mesh queue if it has neighbors
        private void ProcessLightQueue()
        {
            if (lightingQueue.IsEmpty) return;

            ChunkCoord playerChunk = GetPlayerChunk();

            //try to dequeue mesh and get value
            if (lightingQueue.TryDequeue(out ChunkCoord coord))
            {
                //get chunk
                lightingQueuedChunks.TryRemove(coord, out byte thing);
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted()) return;

                //if chunk has neighbors then its ready for meshing
                if (ChunkLightable(coord))
                {                 
                    //get this chunk and adjacent chunks
                    Chunk? left, right, back, front,
                    c1, c2, c3, c4;
                    
                    left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
                    right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
                    front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
                    back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));

                    c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1)); //back-left
                    c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1)); //back-right
                    c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1)); //front-left
                    c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1)); //front-right
                    if (left == null || right == null || front == null || back == null) return;
                    if (!left.HasVoxelData() || !right.HasVoxelData() || !front.HasVoxelData() || !back.HasVoxelData()) return;

                    if (c1 == null || c2 == null || c3 == null || c4 == null) return;
                    if (!c1.HasVoxelData() || !c2.HasVoxelData() || !c3.HasVoxelData() || !c4.HasVoxelData()) return;

                    chunk.lighting = true;
                    left.lighting = true;
                    right.lighting = true;
                    front.lighting = true;
                    back.lighting = true;
                    c1.lighting = true;
                    c2.lighting = true;
                    c3.lighting = true;
                    c4.lighting = true;                

                    lightThread.Submit(() =>
                    {
                        VoxelLightingEngine.LightChunks(chunk, left, right, front, back, c3, c1, c4, c2, this);
                        chunk.fullyLit = true;
                        chunk.lighting = false;
                        left.lighting = false;
                        right.lighting = false;
                        front.lighting = false;
                        back.lighting = false;
                        c1.lighting = false;
                        c2.lighting = false;
                        c3.lighting = false;
                        c4.lighting = false;
                        MeshEnqueue(coord);
                    });            
                }
                //if not, make sure chunk exist, is not an edge chunk, and has voxel data
                else if (!ChunkOutOfRenderDistance(coord, playerChunk) && chunk != null && chunk.GetState() != ChunkState.Meshed && chunk.GetState() != ChunkState.Built && chunk.GetState() != ChunkState.Deleted)
                {
                    LightEnqueue(coord);
                }
            }
        }

        //build voxel data for chunks on seperate threads
        private void ProcessChunkGenQueue()
        {
            if (chunkGenQueue.TryDequeue(out ChunkCoord coord))
            {
                //check if chunk exists
                Chunk? chunk = GetChunk(coord);
                if (chunk == null) return;             

                //create block data on seperate thread 
                threadPool.Submit(() =>
                {
                    chunk.CreateVoxelMap();
                    ChunkCoord playerChunk = GetPlayerChunk();
                    //now that we are generating this chunk, make sure the lighting queue knows about it
                    if (!ChunkOutOfRenderDistance(chunk.Pos, playerChunk))
                    {
                        LightEnqueue(chunk.Pos);
                    }
                });
            }
        }

        //generates meshes of chunks with all neighbors on seperate thread
        private void ProcessMeshQueue()
        {
            if (chunkMeshQueue.TryDequeue(out ChunkCoord coord))
            {
                //check if chunk exists
                Chunk? chunk = GetChunk(coord);
                if (chunk == null) return;
                meshQueuedChunks.TryRemove(coord, out byte thing);
                ChunkCoord playerChunk = GetPlayerChunk();
                //check once again for neighbors, if neighbors deleted, then back out
                if (!ChunkMeshable(coord) && !ChunkOutOfRenderDistance(coord, playerChunk) && chunk.GetState() != ChunkState.Meshed && chunk.GetState() != ChunkState.Built && chunk.GetState() != ChunkState.Deleted)
                {
                    MeshEnqueue(coord);
                    return; 
                }

                            
                //create mesh and upload on a seperate thread
                threadPool.Submit(() =>
                {
                    Chunk? left, right, front, back,
                    c1, c2, c3, c4;
                    left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
                    right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
                    front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
                    back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));

                    c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1)); //back-left
                    c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1)); //back-right
                    c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1)); //front-left
                    c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1)); //front-right

                    if (left == null || right == null || front == null || back == null) return;
                    if (!left.HasVoxelData() || !right.HasVoxelData() || !front.HasVoxelData() || !back.HasVoxelData()) return;

                    if (c1 == null || c2 == null || c3 == null || c4 == null) return;
                    if (!c1.HasVoxelData() || !c2.HasVoxelData() || !c3.HasVoxelData() || !c4.HasVoxelData()) return;

                    chunk.meshing = true;
                    chunk.CreateChunkMesh(left, right, front, back, c1, c2, c3, c4);
                    chunk.meshing = false;
                    chunkUploadQueue.Enqueue(chunk);
                });
            }
        }

        //send mesh data to gpu on main thread
        private void ProcessChunkUploadQueue()
        {        
            //try to pop from upload queue
            if (chunkUploadQueue.TryDequeue(out Chunk? chunk))
            {
                //if chunk is meshed but no voxel data, then build and stay popped
                if (chunk != null && chunk.GetState() == ChunkState.Meshed && !chunk.meshing && chunkUploadTimer > MaxChunkBuildPerFrame)
                {
                    chunk.SendMeshToOpenGL();
                    chunkUploadTimer = 0;
                }
                //if chunk isnt built, retry if not being deleted
                else if (chunk != null && chunk.GetState() != ChunkState.Deleted) chunkUploadQueue.Enqueue(chunk);
            }
        }

        //deletes chunks out of bounds
        private void ProcessDeletedChunks()
        {
            while (!deletionQueue.IsEmpty)
            {
                //try to pop from deletion queue
                if (deletionQueue.TryDequeue(out ChunkCoord coord))
                {
                    Chunk? chunk = GetChunk(coord);
                    //if chunk is null, then its already deleted and we can skip
                    if (chunk != null)
                    {
                        //check if we can remove chunk
                        if (ChunkMap.TryRemove(coord, out chunk))
                        {
                            //delete vram data if successful
                            deletionQueuedChunks.TryRemove(coord, out byte thing);
                            lightingQueuedChunks.TryRemove(coord, out thing);
                            meshQueuedChunks.TryRemove(coord, out thing);
                            chunk.Delete();
                        }
                        //retry if unsuccessful
                        else
                        {
                            deletionQueue.Enqueue(coord);
                        }
                    }
                }
            }         
        }

        //deletes meshes of chunks out of view
        private void ProcessDeletedMeshes()
        {
            while (!meshDeletionQueue.IsEmpty)
            {
                if (meshDeletionQueue.TryDequeue(out ChunkCoord coord))
                {
                    Chunk? chunk = GetChunk(coord);
                    //delete mesh if chunk exists
                    if (chunk != null)
                    {
                        chunk.ClearChunkMesh();
                        meshDeletionQueuedChunks.TryRemove(coord, out byte thing);
                        lightingQueuedChunks.TryRemove(coord, out thing);
                        meshQueuedChunks.TryRemove(coord, out thing);
                    }
                }
            }         
        }

        //remeshes all dirty chunks
        private void ProcessRemeshedChunks()
        {
            while(!remeshQueue.IsEmpty)
            {
                if(remeshQueue.TryDequeue(out Chunk? chunk))
                {
                    if (chunk != null)
                    {
                        chunk.meshing = true;
                        RemeshChunk(chunk);
                        chunk.meshing = false;
                        remeshQueuedChunks.TryRemove(chunk, out byte thing);
                    }
                }
            }
        }

        //load/unload world when moving between chunks
        public void Update(float time, float globalTime)
        {        
            ChunkCoord currentPlayerChunk = GetPlayerChunk();
            //moved between chunks
            if (currentPlayerChunk.X != lastPlayerChunk.X || currentPlayerChunk.Z != lastPlayerChunk.Z)
            {
                UpdateFarChunks();
                Generate();
                lastPlayerChunk = currentPlayerChunk;
            }

            //multithreading managers
            ProcessChunkGenQueue();
            ProcessLightQueue();
            ProcessMeshQueue();
            ProcessChunkUploadQueue();

            //update dirty chunks
            ProcessRemeshedChunks();

            //delete stuff
            ProcessDeletedMeshes();
            ProcessDeletedChunks();

            chunkUploadTimer += time;
        }

        //clears chunk map
        public void Delete()
        {
            foreach(var pair in ChunkMap)
            {
                //delete chunk data
                pair.Value.Delete();
            }
            ChunkMap.Clear();
        }

        //---utility---

        //safely allows a chunk to be queued for trying mesh generation
        private void LightEnqueue(ChunkCoord coord)
        {
            if (lightingQueuedChunks.TryAdd(coord, 0))
            {
                lightingQueue.Enqueue(coord);
            }
        }

        //safely allows a chunk to be queued for mesh generation
        private void MeshEnqueue(ChunkCoord coord)
        {
            if (meshQueuedChunks.TryAdd(coord, 0))
            {
                chunkMeshQueue.Enqueue(coord);
            }
        }

        //safely allows a chunk to be queued for mesh deletion
        private void MeshDeletionEnqueue(ChunkCoord coord)
        {
            if (meshDeletionQueuedChunks.TryAdd(coord, 0))
            {
                meshDeletionQueue.Enqueue(coord);
            }
        }

        //safely allows a chunk to be queued for deletion
        private void DeletionEnqueue(ChunkCoord coord)
        {
            if (deletionQueuedChunks.TryAdd(coord, 0))
            {
                deletionQueue.Enqueue(coord);
            }
        }

        //makes a chunk ready for remeshing
        private void RemeshEnqueue(Chunk? chunk, int globalY)
        {
            if (chunk == null) return;
            chunk.changes.Add(globalY);
            if (remeshQueuedChunks.TryAdd(chunk, 0))
            {               
                remeshQueue.Enqueue(chunk);
            }
        }

        //safely fetch a chunk from chunk coordinates
        public Chunk? GetChunk(ChunkCoord coord)
        {
            if (ChunkMap.ContainsKey(coord))
            {
                return ChunkMap[coord];
            }
            return null;
        }

        //gets the chunk the player is in
        public ChunkCoord GetPlayerChunk()
        {
            //get int player position
            int pX = (int)Math.Floor(player.Position.X);
            int pZ = (int)Math.Floor(player.Position.Z);

            //get chunk position from world position
            int chunkX = pX / SubChunk.SUBCHUNK_SIZE;
            int chunkZ = pZ / SubChunk.SUBCHUNK_SIZE;

            //handle negatives properly
            if (pX < 0) chunkX -= 1;
            if (pZ < 0) chunkZ -= 1;

            return new ChunkCoord(chunkX, chunkZ);
        }

        //get refrence to the chunk the player is in
        public Chunk? GetChunkPlayerIsIn()
        {
            Chunk? chunk = GetChunk(GetPlayerChunk());
            if (chunk == null) return null;
            return chunk;
        }

        //checks if a chunk is too far away from player and should be unrendered
        private bool ChunkOutOfRenderDistance(ChunkCoord coord, ChunkCoord playerChunk)
        {
            return MathF.Abs(coord.Z - playerChunk.Z) > RenderDistance || MathF.Abs(coord.X - playerChunk.X) > RenderDistance;
        }

        //checks if a chunk is fully out of generation range
        private bool ChunkOutOfBounds(ChunkCoord coord, ChunkCoord playerChunk)
        {
            return MathF.Abs(coord.Z - playerChunk.Z) > WorldDistance || MathF.Abs(coord.X - playerChunk.X) > WorldDistance;
        }

        //checks if a chunk has neighbors with voxel data around them
        public bool ChunkMeshable(ChunkCoord coord)
        {
            //get this chunk and adjacent chunks
            Chunk? thisChunk, left, right, back, front,
            c1, c2, c3, c4;

            thisChunk = GetChunk(coord);
            left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z ));
            right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z ));
            front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1 ));
            back = GetChunk(new ChunkCoord( coord.X, coord.Z - 1 ));

            c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1)); //back-left
            c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1)); //back-right
            c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1)); //front-left
            c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1)); //front-right

            //if chunks dont exist or dont have block data, then back out
            if (thisChunk == null || left == null || right == null || front == null || back == null) return false;
            if (!thisChunk.HasVoxelData() || !left.HasVoxelData() || !right.HasVoxelData() || !front.HasVoxelData() || !back.HasVoxelData()) return false;

            if (c1 == null || c2 == null || c3 == null || c4 == null) return false;
            if (!c1.HasVoxelData() || !c2.HasVoxelData() || !c3.HasVoxelData() || !c4.HasVoxelData()) return false;

            //if chunks are marked for deletion back out
            if (thisChunk.Deleted() || left.Deleted() || right.Deleted() || front.Deleted() || back.Deleted()) return false;
            if (c1.Deleted() || c2.Deleted() || c3.Deleted() || c4.Deleted()) return false;

            //if chunk isnt fully lit then back out
            if (thisChunk.fullyLit == false) return false;

            //if chunks are currently lighting back out
            if (thisChunk.lighting || left.lighting || right.lighting || front.lighting || back.lighting) return false;
            if (c1.lighting || c2.lighting || c3.lighting || c4.lighting) return false;

            return true;
        }

        //checks if a chunk can light based on neighbor info
        public bool ChunkLightable(ChunkCoord coord)
        {
            //get this chunk and adjacent chunks
            Chunk? thisChunk, left, right, back, front,
            c1, c2, c3, c4;

            thisChunk = GetChunk(coord);
            left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
            right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
            front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
            back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));

            c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1)); //back-left
            c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1)); //back-right
            c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1)); //front-left
            c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1)); //front-right

            //if chunks are null or dont have voxel data back out
            if (thisChunk == null || left == null || right == null || front == null || back == null) return false;
            if (!thisChunk.HasVoxelData() || !left.HasVoxelData() || !right.HasVoxelData() || !front.HasVoxelData() || !back.HasVoxelData()) return false;

            if (c1 == null || c2 == null || c3 == null || c4 == null) return false;
            if (!c1.HasVoxelData() || !c2.HasVoxelData() || !c3.HasVoxelData() || !c4.HasVoxelData()) return false;

            //if chunks are marked for deletion back out
            if (thisChunk.Deleted() || left.Deleted() || right.Deleted() || front.Deleted() || back.Deleted()) return false;
            if (c1.Deleted() || c2.Deleted() || c3.Deleted() || c4.Deleted()) return false;

            //if chunks are currently lighting back out
            if (thisChunk.lighting || left.lighting || right.lighting || front.lighting || back.lighting) return false;
            if (c1.lighting || c2.lighting || c3.lighting || c4.lighting) return false;

            //if chunks are currently meshing then back out
            if (thisChunk.meshing || left.meshing || right.meshing || front.meshing || back.meshing) return false;
            if (c1.meshing || c2.meshing || c3.meshing || c4.meshing) return false;          

            return true;
        }

        //get block state
        public BlockState GetBlockState(Vector3 pos)
        {
            //world to chunk coord
            int chunkX = (int)MathF.Floor(pos.X / SubChunk.SUBCHUNK_SIZE);
            int chunkZ = (int)MathF.Floor(pos.Z / SubChunk.SUBCHUNK_SIZE);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            if (chunk == null) return new BlockState(BlockIDs.INVALID_BLOCK);

            //get local coords
            int lx = ModGlobalToChunk((int)MathF.Floor(pos.X), SubChunk.SUBCHUNK_SIZE);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = ModGlobalToChunk((int)MathF.Floor(pos.Z), SubChunk.SUBCHUNK_SIZE);

            return chunk.GetBlockSafe(lx, ly, lz);
        }

        //try set block in a chunk
        public void SetBlock(Vector3 pos, BlockState state)
        {
            if (pos.Y < 0 || pos.Y >= SubChunk.SUBCHUNK_SIZE * Chunk.SUBCHUNK_COUNT) return;
            //world to chunk coord
            int chunkX = (int)MathF.Floor(pos.X / SubChunk.SUBCHUNK_SIZE);
            int chunkZ = (int)MathF.Floor(pos.Z / SubChunk.SUBCHUNK_SIZE);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            //get local chunk positions
            int lx = ModGlobalToChunk((int)MathF.Floor(pos.X), SubChunk.SUBCHUNK_SIZE);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = ModGlobalToChunk((int)MathF.Floor(pos.Z), SubChunk.SUBCHUNK_SIZE);

            if (chunk == null || chunk.GetState() != ChunkState.Built) return;

            chunk.SetBlock(lx, ly, lz, state);            
            RemeshEnqueue(chunk, ly); //chunk needs its mesh updated

            //remesh neighbor chunks
            if (lx == SubChunk.SUBCHUNK_SIZE - 1)
                RemeshEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ)), ly);
            if (lx == 0)
                RemeshEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ)), ly);

            if (lz == SubChunk.SUBCHUNK_SIZE - 1)
                RemeshEnqueue(GetChunk(new ChunkCoord(chunkX, chunkZ + 1)), ly);
            if (lz == 0)
                RemeshEnqueue(GetChunk(new ChunkCoord(chunkX, chunkZ - 1)), ly);
            
            //remesh corner chunks
            if (lx == SubChunk.SUBCHUNK_SIZE - 1 && lz == SubChunk.SUBCHUNK_SIZE - 1)
                RemeshEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ + 1)), ly);
            if (lx == 0 && lz == 0)
                RemeshEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ - 1)), ly);
            if (lx == 0 && lz == SubChunk.SUBCHUNK_SIZE - 1)
                RemeshEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ + 1)), ly);
            if (lx == SubChunk.SUBCHUNK_SIZE - 1 && lx == 0)
                RemeshEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ - 1)), ly);
        }

        public ushort GetBlockLight(Vector3 pos)
        {
            int chunkX = (int)MathF.Floor(pos.X / SubChunk.SUBCHUNK_SIZE);
            int chunkZ = (int)MathF.Floor(pos.Z / SubChunk.SUBCHUNK_SIZE);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            if (chunk == null) return 0;

            //get local coords
            int lx = ModGlobalToChunk((int)MathF.Floor(pos.X), SubChunk.SUBCHUNK_SIZE);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = ModGlobalToChunk((int)MathF.Floor(pos.Z), SubChunk.SUBCHUNK_SIZE);
            Console.WriteLine(lx + ", " + ly + ", " + lz);
            return chunk.lightMap[lx, ly, lz];
        }

        //updates surrounding blocks when a block is set, ONLY CALL WHEN ALL BLOCKS ARE READY
        public void UpdateNeighborBlocks(Vector3 globalPos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left)
        {
            BlockData.GetBlock(bottom.BlockID).UpdateBlockState(globalPos + new Vector3(0, -1, 0), bottom, this);
            BlockData.GetBlock(top.BlockID).UpdateBlockState(globalPos + new Vector3(0, 1, 0), top, this);
            BlockData.GetBlock(front.BlockID).UpdateBlockState(globalPos + new Vector3(0, 0, 1), front, this);
            BlockData.GetBlock(back.BlockID).UpdateBlockState(globalPos + new Vector3(0, 0, -1), back, this);
            BlockData.GetBlock(right.BlockID).UpdateBlockState(globalPos + new Vector3(1, 0, 0), right, this);
            BlockData.GetBlock(left.BlockID).UpdateBlockState(globalPos + new Vector3(-1, 0, 0), left, this);
        }

        //tries to remesh a chunk
        private void RemeshChunk(Chunk? chunk)
        {
            Chunk? left, right, back, front,
            c1, c2, c3, c4;
            ChunkCoord coord;

            if (chunk != null) coord = chunk.Pos;
            else return;

            int chunkX = coord.X;
            int chunkZ = coord.Z;

            left = GetChunk(new ChunkCoord(chunkX - 1, chunkZ));
            right = GetChunk(new ChunkCoord(chunkX + 1, chunkZ));
            front = GetChunk(new ChunkCoord(chunkX, chunkZ + 1));
            back = GetChunk(new ChunkCoord(chunkX, chunkZ - 1));

            c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1)); //back-left
            c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1)); //back-right
            c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1)); //front-left
            c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1)); //front-right

            if (chunk == null || left == null || right == null || front == null || back == null) return;
            if (!chunk.Remeshable() || !left.Remeshable() || !right.Remeshable() || !front.Remeshable() || !back.Remeshable()) return;

            if (c1 == null || c2 == null || c3 == null || c4 == null) return;
            if (!c1.Remeshable() || !c2.Remeshable() || !c3.Remeshable() || !c4.Remeshable()) return;

            chunk.Remesh(left, right, front, back, c1, c2, c3, c4);
        }

        //math helpers
        public static int ModGlobalToChunk(int a, int b)
        {
            return (a % b + b) % b;
        }
    }
}  