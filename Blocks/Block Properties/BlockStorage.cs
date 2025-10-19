//contains systems for managing voxel data memory usage
namespace OurCraft.Blocks.Block_Properties
{
    //holds only the unique block states in a subchunk
    public class BlockPalette
    {
        //actual data
        private readonly List<BlockState> entries = [];
        private readonly Dictionary<BlockState, int> entryMap = [];

        //helpers for adding to data
        public int GetOrAddIndex(BlockState state)
        {
            if (!entryMap.TryGetValue(state, out int index))
            {
                index = entries.Count;
                entries.Add(state);
                entryMap[state] = index;
            }
            return index;
        }

        //getters
        public BlockState GetState(int index) => entries[index];
        public int Count => entries.Count;
    }

    //allows to dynamically change the amount of data needed to represent block states
    public interface IBlockIndexStorage
    {
        int Get(int index);
        void Set(int index, int paletteIndex);
        int Length { get; }
    }

    //when chunk only has > 255 different block states
    public class ByteBlockStorage : IBlockIndexStorage
    {
        private byte[] data;
        public int Length => data.Length;

        public ByteBlockStorage(int size) => data = new byte[size];

        public int Get(int index) => data[index];
        public void Set(int index, int paletteIndex) => data[index] = (byte)paletteIndex;
    }

    //when chunk has more than 255 different block states
    public class UShortBlockStorage : IBlockIndexStorage
    {
        private ushort[] data;
        public int Length => data.Length;

        public UShortBlockStorage(int size) => data = new ushort[size];

        public int Get(int index) => data[index];
        public void Set(int index, int paletteIndex) => data[index] = (ushort)paletteIndex;
    }
}
