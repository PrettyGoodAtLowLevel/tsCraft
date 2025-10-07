using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Rendering;
using OurCraft.utility;
using System.Collections.Concurrent;

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
        readonly ConcurrentDictionary<ChunkCoord, byte> meshTriedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> deletionQueuedChunks = new();
        readonly ConcurrentDictionary<ChunkCoord, byte> meshDeletionQueuedChunks = new();

        //chunk queues (actual queues)
        readonly ConcurrentQueue<ChunkCoord> chunkGenQueue = new();
        readonly ConcurrentQueue<ChunkCoord> tryMeshQueue = new();
        readonly ConcurrentQueue<ChunkCoord> chunkMeshQueue = new();
        readonly ConcurrentQueue<Chunk> chunkUploadQueue = new();
        readonly ConcurrentQueue<ChunkCoord> meshDeletionQueue = new();
        readonly ConcurrentQueue<ChunkCoord> deletionQueue = new();

        //world settings
        public int RenderDistance { get; private set; } = 0; //how far chunks draw
        public int WorldDistance { get; private set; } = 0; //how far chunks generate
        public float MaxChunkBuildPerFrame { get; private set; } = 0.0001f;
        float chunkUploadTimer = 0;

        //refrences
        Camera player;
        ChunkCoord lastPlayerChunk;
        private readonly ThreadPoolSystem threadPool;

        //methods

        //basic constructor
        public Chunkmanager(RenderDistances renderDistance, ref Camera player, ref ThreadPoolSystem tp)
        { 
            this.player = player;
            threadPool = tp;
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
                    if (ChunkMap.TryAdd(coord, new Chunk(coord, player)))
                    {
                        chunkGenQueue.Enqueue(coord);

                        //try to add to mesh queue
                        TryMeshEnqueue(coord);
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
                        DeletionEnqueue(pair.Key);
                }

                //mesh deletion
                else if (ChunkOutOfRenderDistance(pair.Key, playerChunk))
                {
                    if (pair.Value.GetState() == ChunkState.Meshed || pair.Value.GetState() == ChunkState.Built)
                        MeshDeletionEnqueue(pair.Key);
                }

                //update chunks that arent meshed but should be
                else
                {
                    if (pair.Value.GetState() == ChunkState.VoxelOnly)
                    {
                        TryMeshEnqueue(pair.Key);
                    }
                }
            }
        }

        //tries to add a chunk to mesh queue if it has neighbors
        private void UpdateMeshQueue()
        {
            if (tryMeshQueue.IsEmpty) return;

            ChunkCoord playerChunk = GetPlayerChunk();

            //try to dequeue mesh and get value
            if (tryMeshQueue.TryDequeue(out ChunkCoord coord))
            {
                //get chunk
                meshTriedChunks.TryRemove(coord, out byte thing);
                Chunk? chunk = GetChunk(coord);

                //if chunk has neighbors then its ready for meshing
                if (ChunkHasNeighbors(coord))
                {
                    MeshEnqueue(coord);                 
                }
                //if not, make sure chunk exist, is not an edge chunk, and has voxel data
                else if (!ChunkOutOfRenderDistance(coord, playerChunk) && chunk != null && chunk.GetState() != ChunkState.Meshed && chunk.GetState() != ChunkState.Built && chunk.GetState() != ChunkState.Deleted)
                {
                    TryMeshEnqueue(coord);
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

                //check once again for neighbors, if neighbors deleted, then back out
                if (!ChunkHasNeighbors(coord)) return;

                //create mesh and upload on a seperate thread
                threadPool.Submit(() =>
                {
                    Chunk? left, right, front, back;
                    left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z));
                    right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z));
                    front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1));
                    back = GetChunk(new ChunkCoord(coord.X, coord.Z - 1));

                    if (left == null || right == null || front == null || back == null) return;
                    if (!left.HasVoxelData() || !right.HasVoxelData() || !front.HasVoxelData() || !back.HasVoxelData()) return;

                    chunk.CreateChunkMesh(left, right, front, back);
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
                if (chunk != null && chunk.GetState() == ChunkState.Meshed && chunkUploadTimer > MaxChunkBuildPerFrame)
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
            UpdateMeshQueue();
            ProcessMeshQueue();
            ProcessChunkUploadQueue();

            //delete stuff
            ProcessDeletedMeshes();
            ProcessDeletedChunks();

            chunkUploadTimer += time;

            if (globalTime > 20)
            {
                if (RenderDistance > 12) MaxChunkBuildPerFrame = 0.005f;
                else MaxChunkBuildPerFrame = 0.001f;
            }
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
        private void TryMeshEnqueue(ChunkCoord coord)
        {
            if (meshTriedChunks.TryAdd(coord, 0))
            {
                tryMeshQueue.Enqueue(coord);
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

        //safely fetch a chunk from chunk coordinates
        private Chunk? GetChunk(ChunkCoord coord)
        {
            if (ChunkMap.ContainsKey(coord))
            {
                return ChunkMap[coord];
            }
            return null;
        }

        //gets the chunk the player is in
        private ChunkCoord GetPlayerChunk()
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

        //checks if a chunk is fully out of generation range
        private bool ChunkOutOfBounds(ChunkCoord coord, ChunkCoord playerChunk)
        {

            return MathF.Abs(coord.Z - playerChunk.Z) > WorldDistance || MathF.Abs(coord.X - playerChunk.X) > WorldDistance;
        }

        //checks if a chunk has neighbors with voxel data around them
        private bool ChunkHasNeighbors(ChunkCoord coord)
        {
            //get this chunk and adjacent chunks
            Chunk? thisChunk, left, right, back, front;
            thisChunk = GetChunk(coord);
            left = GetChunk(new ChunkCoord(coord.X - 1, coord.Z ));
            right = GetChunk(new ChunkCoord(coord.X + 1, coord.Z ));
            front = GetChunk(new ChunkCoord(coord.X, coord.Z + 1 ));
            back = GetChunk(new ChunkCoord( coord.X, coord.Z - 1 ));

            //if chunks dont exist or dont have block data, then back out
            if (thisChunk == null || left == null || right == null || front == null || back == null) return false;
            if (!thisChunk.HasVoxelData() || !left.HasVoxelData() || !right.HasVoxelData() || !front.HasVoxelData() || !back.HasVoxelData()) return false;

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
            int lx = Mod((int)MathF.Floor(pos.X), SubChunk.SUBCHUNK_SIZE);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = Mod((int)MathF.Floor(pos.Z), SubChunk.SUBCHUNK_SIZE);

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
            int lx = Mod((int)MathF.Floor(pos.X), SubChunk.SUBCHUNK_SIZE);
            int ly = (int)MathF.Floor(pos.Y);
            int lz = Mod((int)MathF.Floor(pos.Z), SubChunk.SUBCHUNK_SIZE);

            if (chunk == null || chunk.GetState() != ChunkState.Built) return;

            chunk.SetBlock(lx, ly, lz, state);
            RemeshChunk(chunk, ly); //chunk needs its mesh updated

            //remesh neighbor chunks
            if (lx == SubChunk.SUBCHUNK_SIZE - 1) RemeshChunk(GetChunk(new ChunkCoord(chunkX + 1, chunkZ)), ly);
            if (lx == 0) RemeshChunk(GetChunk(new ChunkCoord(chunkX - 1, chunkZ)), ly);

            if (lz == SubChunk.SUBCHUNK_SIZE - 1) RemeshChunk(GetChunk(new ChunkCoord(chunkX, chunkZ + 1)), ly);
            if (lz == 0) RemeshChunk(GetChunk(new ChunkCoord(chunkX, chunkZ - 1)), ly);
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
        private void RemeshChunk(Chunk? chunk, int globalY)
        {
            Chunk? left, right, back, front;
            ChunkCoord coord;

            if (chunk != null) coord = chunk.Pos;
            else return;

            int chunkX = coord.X;
            int chunkZ = coord.Z;

            left = GetChunk(new ChunkCoord(chunkX - 1, chunkZ));
            right = GetChunk(new ChunkCoord(chunkX + 1, chunkZ));
            front = GetChunk(new ChunkCoord(chunkX, chunkZ + 1));
            back = GetChunk(new ChunkCoord(chunkX, chunkZ - 1));

            if (chunk == null || left == null || right == null || front == null || back == null) return;
            if (!chunk.Remeshable() || !left.Remeshable() || !right.Remeshable() || !front.Remeshable() || !back.Remeshable()) return;

            chunk.Remesh(left, right, front, back, globalY);
        }

        //math helpers
        private static int Mod(int a, int b)
        {
            return (a % b + b) % b;
        }
    }
}  