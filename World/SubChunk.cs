using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.Terrain_Generation;
using OurCraft.Utility;

namespace OurCraft.World
{
    //SubChunks are small parts of chunks that contain block and mesh data
    //subchunks are utilized to split up meshing jobs and compress block state data
    public class SubChunk
    {
        public const int SUBCHUNK_SIZE = WorldConstants.SUBCHUNK_SIZE_IN_BLOCKS;
        public const int CHUNK_WIDTH = WorldConstants.CHUNK_WIDTH;
        public bool isAllAir = true;

        //world data
        public int ChunkXPos { get; private set; } = 0;
        public int ChunkYPos { get; private set; } = 0;
        public int ChunkZPos { get; private set; } = 0;
        public readonly Chunk parent;

        //block storage
        private IBlockIndexStorage blockIndices;
        public readonly BlockPalette palette;
        public List<ushort> lightSources = [];

        //mesh data
        public ChunkMeshData SolidGeo { get; private set; }
        public ChunkMeshData TransparentGeo { get; private set; }

        public SubChunk(Chunk parent, int xPos, int yPos, int zPos)
        {
            this.parent = parent;
            ChunkXPos = xPos;
            ChunkYPos = yPos;
            ChunkZPos = zPos;

            palette = new BlockPalette();
            blockIndices = new ByteBlockStorage(SUBCHUNK_SIZE * SUBCHUNK_SIZE * SUBCHUNK_SIZE);
            SolidGeo = new ChunkMeshData();
            TransparentGeo = new ChunkMeshData(); 
        }       

        public void ClearMesh()
        {
            SolidGeo.ClearMesh();
            TransparentGeo.ClearMesh();
        }

        public BlockState GetBlockState(int x, int y, int z)
        {
            int index = Flatten(x, y, z);
            int paletteIndex = blockIndices.Get(index);
            return palette.GetState(paletteIndex);
        }

        public void SetBlock(int x, int y, int z, BlockState state)
        {
            int index = Flatten(x, y, z);
            int paletteIndex = palette.GetOrAddIndex(state);

            //update light sources if light block is reset
            BlockState oldState = GetBlockState(x, y, z);
            if (oldState.GetBlock.IsLightSource(oldState)) lightSources.Remove(VoxelMath.PackPos32(x, y, z));

            //if block we are adding is light source, then add to light sources
            if (state.GetBlock.IsLightSource(state)) lightSources.Add(VoxelMath.PackPos32(x, y, z));

            //auto-upgrade to ushort
            if (paletteIndex > byte.MaxValue && blockIndices is ByteBlockStorage) UpgradeStorage();

            if (state != OverworldGenerator.EmptyBlock) isAllAir = false;

            blockIndices.Set(index, paletteIndex);
        }

        //really quick setblock, (like during density generation)
        public void SetBlockFast(int x, int y, int z, int paletteIndex)
        {
            int index = VoxelMath.SubChunkFlatten(x, y, z);        
            blockIndices.Set(index, paletteIndex);
        }

        public bool IsAllAir()
        {
            return isAllAir;
        }

        private static int Flatten(int x, int y, int z)
        {
            return x + SUBCHUNK_SIZE * (y + SUBCHUNK_SIZE * z);
        }

        //change index data to support more block states
        private void UpgradeStorage()
        {
            var oldStorage = (ByteBlockStorage)blockIndices;
            var newStorage = new UShortBlockStorage(oldStorage.Length);

            for (int i = 0; i < oldStorage.Length; i++) newStorage.Set(i, oldStorage.Get(i));            
            blockIndices = newStorage;
        }

        public override string ToString()
        {
            string str = "";

            str += $"SubChunk in chunk pos: ({ChunkXPos}, {ChunkYPos}, {ChunkZPos}), ";
            str += $"Light Sources: {lightSources.Count}";

            return str;
        }
    }
}
