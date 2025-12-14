using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Graphics;
using OurCraft.Graphics.Voxel_Lighting;
using OurCraft.utility;
using OurCraft.Blocks.Block_Properties;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace OurCraft.World
{
    //contains all the types of render distances in the game that are possible
    public enum RenderDistances
    {
        TWO_CHUNKS, THREE_CHUNKS,
        FOUR_CHUNKS, FIVE_CHUNKS,
        SIX_CHUNKS, SEVEN_CHUNKS,
        EIGHT_CHUNKS, NINE_CHUNKS,
        TEN_CHUNKS, ELEVEN_CHUNKS,
        TWELVE_CHUNKS, THIRTEEN_CHUNKS,
        FOURTEEN_CHUNKS, FIFTEEN_CHUNKS,
        SIXTEEN_CHUNKS,
    }

    //manages chunk generation
    public class Chunkmanager
    {
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
        readonly ConcurrentQueue<Chunk> modifiedQueue = new();

        public int RenderDistance { get; private set; } = 0;
        public float MaxChunkBuildPerFrame { get; private set; } = 0.0001f;
        float chunkUploadTimer = 0;

        ChunkCoord lastPlayerChunk;
        readonly Camera player;
        
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
            lastPlayerChunk = new ChunkCoord(0, 0);        
        }

        //generate chunks around the player
        public void Generate()
        {
            ChunkCoord playerChunk = GetPlayerChunk();

            for (int x = playerChunk.X - RenderDistance; x <= playerChunk.X + RenderDistance; x++)
            {
                for (int z = playerChunk.Z - RenderDistance; z <= playerChunk.Z + RenderDistance; z++)
                {
                    ChunkCoord coord = new(x, z);
                    if (ChunkMap.TryAdd(coord, new Chunk(coord))) GenerationEnqueue(coord);
                }
            }
        }

        //delete far away chunks and update any deffered lighting that is too far away
        private void UnloadFarChunks()
        {
            ChunkCoord playerChunk = GetPlayerChunk();
            VoxelLightingEngine.UnloadDefferedLights(playerChunk, RenderDistance);
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
                //check if chunk exists
                Chunk? chunk = GetChunk(coord);
                if (chunk == null) return;

                //create block data on seperate thread 
                threadPool.Submit(() =>
                {
                    chunk.CreateVoxelMap();
                    LightEnqueue(chunk.Pos);
                });
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
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted()) return;
                if (ChunkWorkable(coord))
                {
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
                        c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1)); //back-left
                        c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1)); //back-right
                        c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1)); //front-left
                        c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1)); //front-right
                        Chunk?[] chunks = [left, right, front, back, c1, c2, c3, c4];
                        foreach (var chunkd in chunks)
                            if (chunkd != null && chunkd.GetState() == ChunkState.Built)
                                MeshEnqueue(chunkd.Pos);

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

        //generates meshes of chunks with all neighbors on seperate thread
        private void ProcessMeshQueue()
        {
            if (chunkMeshQueue.IsEmpty) return;

            ChunkCoord playerChunk = GetPlayerChunk();

            if (chunkMeshQueue.TryDequeue(out ChunkCoord coord))
            {
                //check if chunk exists              
                Chunk? chunk = GetChunk(coord);
                if (chunk == null || chunk.Deleted()) return;
                
                //check once again for neighbors, if neighbors deleted, then back out
                if (ChunkWorkable(coord))
                {
                    Chunk? left, right, front, back, c1, c2, c3, c4;
                    left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
                    right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
                    front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
                    back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));
                    c1 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z - 1)); //back-left
                    c2 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z - 1)); //back-right
                    c3 = GetChunk(new ChunkCoord(coord.X - 1, coord.Z + 1)); //front-left
                    c4 = GetChunk(new ChunkCoord(coord.X + 1, coord.Z + 1)); //front-right

                    //create mesh on a seperate thread
                    threadPool.Submit(() =>
                    {
                        chunk.CreateChunkMesh(left, right, front, back, c1, c2, c3, c4);
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

        //send mesh data to gpu on main thread
        private void ProcessChunkUploadQueue()
        {        
            //try to pop from upload queue
            if (chunkUploadQueue.TryDequeue(out Chunk? chunk))
            {
                //if chunk is meshed but no voxel data, then build and stay popped
                if (chunk != null && chunk.HasVoxelData() && chunkUploadTimer > MaxChunkBuildPerFrame)
                {
                    chunk.SendMeshToOpenGL();
                    uploadedChunks.TryRemove(chunk, out byte thing);
                    chunkUploadTimer = 0;
                }
            }
        }

        //deletes chunks out of bounds
        private void ProcessDeletedChunks()
        {
            ChunkCoord playerChunk = GetPlayerChunk();
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
                            DeletionEnqueue(coord);
                        }
                    }
                }
            }         
        }

        //remeshes all dirty chunks
        private void ProcessModifiedChunks()
        {
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

        //update chunks and do multithreading stuff
        public void Update(float time, float globalTime)
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
            ProcessChunkUploadQueue();
            ProcessModifiedChunks();
            ProcessDeletedChunks();

            chunkUploadTimer += time;
        }

        //clears chunk map
        public void Delete()
        {
            foreach(var pair in ChunkMap)
                pair.Value.Delete();
            ChunkMap.Clear();
        }

        //safely allows a chunk to be queued for voxel generation
        private void GenerationEnqueue(ChunkCoord coord)
        {
            if (genQueuedChunks.TryAdd(coord, 0))
                chunkGenQueue.Enqueue(coord);
        }

        //safely allows a chunk to be queued for trying mesh generation
        private void LightEnqueue(ChunkCoord coord)
        {
            if (lightingQueuedChunks.TryAdd(coord, 0))
                lightingQueue.Enqueue(coord);
        }

        //safely allows a chunk to be queued for mesh generation
        private void MeshEnqueue(ChunkCoord coord)
        {
            if (meshQueuedChunks.TryAdd(coord, 0))
                chunkMeshQueue.Enqueue(coord);
        }

        //safely allows a chunk to be queued for gpu upload
        private void UploadEnqueue(Chunk? chunk)
        {
            if (chunk == null) return;
            if (uploadedChunks.TryAdd(chunk, 0))
                chunkUploadQueue.Enqueue(chunk);
        }

        //safely allows a chunk to be queued for deletion
        private void DeletionEnqueue(ChunkCoord coord)
        {
            if (deletionQueuedChunks.TryAdd(coord, 0))
                deletionQueue.Enqueue(coord);           
        }

        //makes a chunk ready for remeshing
        private void ModifyEnqueue(Chunk? chunk)
        {
            if (chunk == null) return;
                
            if (modifiedChunks.TryAdd(chunk, 0))               
                modifiedQueue.Enqueue(chunk);
        }

        //safely fetch a chunk from chunk coordinates
        public Chunk? GetChunk(ChunkCoord coord)
        {
            if (ChunkMap.TryGetValue(coord, out Chunk? value))
                return value;
            
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

        //checks if a chunk is too far away from player and should be unrendered
        private bool ChunkOutOfRenderDistance(ChunkCoord coord, ChunkCoord playerChunk)
        {
            return MathF.Abs(coord.Z - playerChunk.Z) > RenderDistance || MathF.Abs(coord.X - playerChunk.X) > RenderDistance;
        }

        //checks if a chunk can light or mesh
        public bool ChunkWorkable(ChunkCoord coord)
        {
            //get this chunk and adjacent chunks
            Chunk? thisChunk = GetChunk(coord);

            if (thisChunk == null) return false;
            if (!thisChunk.HasVoxelData()) return false;

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
            if (pos.Y < 0 || pos.Y >= Chunk.CHUNK_HEIGHT) return;
            //world to chunk coord
            int chunkX = (int)MathF.Floor(pos.X / SubChunk.SUBCHUNK_SIZE);
            int chunkZ = (int)MathF.Floor(pos.Z / SubChunk.SUBCHUNK_SIZE);
            Chunk? chunk = GetChunk(new ChunkCoord(chunkX, chunkZ));

            //get local chunk positions
            int lx = ModGlobalToChunk((int)MathF.Floor(pos.X), SubChunk.SUBCHUNK_SIZE);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = ModGlobalToChunk((int)MathF.Floor(pos.Z), SubChunk.SUBCHUNK_SIZE);
            if (chunk == null || chunk.GetState() != ChunkState.Built) return;

            //remove light if old block is light source
            BlockState prev = chunk.GetBlockSafe(lx, ly, lz);
            chunk.SetBlock(lx, ly, lz, state);
            
            if (prev.GetBlock.IsLightSource(prev))
                VoxelLightingEngine.RemoveBlockLight(this, chunk, (Vector3i)pos);

            if (!state.GetBlock.IsLightPassable(state))
            {
                VoxelLightingEngine.RemoveSkyLight(this, chunk, (Vector3i)pos);
                VoxelLightingEngine.RemoveBlockLight(this, chunk, (Vector3i)pos);
            }           
            else if (!prev.GetBlock.IsLightPassable(prev))
                VoxelLightingEngine.RemoveLightBlocker(this, (Vector3i)pos);

            if (state.GetBlock.IsLightSource(state))
                VoxelLightingEngine.AddBlockLight(this, chunk, state, (Vector3i)pos);

            MarkPosDirty((Vector3i)pos, chunk);
            //get other chunks to remesh
            ModifyEnqueue(chunk); //chunk needs its mesh updated
            //these chunks may have been modified
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX, chunkZ + 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX, chunkZ - 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ + 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ - 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX - 1, chunkZ + 1)));
            ModifyEnqueue(GetChunk(new ChunkCoord(chunkX + 1, chunkZ - 1)));
        }

        //marks all surrounding chunks that they are dirty
        public void MarkPosDirty(Vector3i pos, Chunk chunk)
        {
            int lx = ModGlobalToChunk((int)MathF.Floor(pos.X), SubChunk.SUBCHUNK_SIZE);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = ModGlobalToChunk((int)MathF.Floor(pos.Z), SubChunk.SUBCHUNK_SIZE);
            chunk.changes.Add(ly);

            ChunkCoord cc = chunk.Pos;
            Chunk? right = GetChunk(new ChunkCoord(cc.X + 1, cc.Z));
            Chunk? left = GetChunk(new ChunkCoord(cc.X - 1, cc.Z));
            Chunk? front = GetChunk(new ChunkCoord(cc.X, cc.Z + 1));
            Chunk? back = GetChunk(new ChunkCoord(cc.X, cc.Z - 1));
            Chunk? rightFront = GetChunk(new ChunkCoord(cc.X + 1, cc.Z + 1));
            Chunk? rightBack = GetChunk(new ChunkCoord(cc.X + 1, cc.Z - 1));
            Chunk? leftFront = GetChunk(new ChunkCoord(cc.X - 1, cc.Z + 1));
            Chunk? leftBack = GetChunk(new ChunkCoord(cc.X - 1, cc.Z - 1));

            if (lx == 0 && left != null) left.changes.Add(ly);
            if (lx == SubChunk.SUBCHUNK_SIZE - 1 && right != null) right.changes.Add(ly);
            if (lz == 0 && back != null) back.changes.Add(ly);
            if (lz == SubChunk.SUBCHUNK_SIZE - 1 && front != null) front.changes.Add(ly);

            if (lx == 0 && lz == 0 && leftBack != null) leftBack.changes.Add(ly);
            if (lx == 0 && lz == SubChunk.SUBCHUNK_SIZE - 1 && leftFront != null) leftFront.changes.Add(ly);
            if (lx == SubChunk.SUBCHUNK_SIZE - 1 && lz == 0 && rightBack != null) rightBack.changes.Add(ly);
            if (lx == SubChunk.SUBCHUNK_SIZE - 1 && lz == SubChunk.SUBCHUNK_SIZE - 1 && rightFront != null) rightFront.changes.Add(ly);
        }

        //tries to remesh a chunk
        private void RemeshChunk(Chunk? chunk)
        {
            Chunk? left, right, back, front, c1, c2, c3, c4;
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

            if (left == null || right == null || front == null || back == null) return;
            if (c1 == null || c2 == null || c3 == null || c4 == null) return;

            if (!chunk.Remeshable() || !left.Remeshable() || !right.Remeshable() || !front.Remeshable() || !back.Remeshable()) return;   
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