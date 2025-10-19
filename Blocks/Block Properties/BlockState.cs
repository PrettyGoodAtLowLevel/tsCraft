namespace OurCraft.Blocks.Block_Properties
{
    //block states use a property system to encode their values
    //values can be stuff like "facing:north", "powered:true"
    //these values can then be encoded to metadata so it is compact and memory effiecent
    //we can then decode this meta data to a mesh builder to find the current blockstate we are using for the mesh

    //represents a block ids current state in a subchunk
    public readonly struct BlockState
    {
        public readonly ushort BlockID; // the id in the global block array
        public readonly ushort MetaData; //up to 16 bits of custom data

        //default constructor
        public BlockState(ushort id, ushort metadata = 0)
        {
            BlockID = id;
            MetaData = metadata;
        }

        //getters
        public Block GetBlock => BlockData.GetBlock(BlockID);

        //get meta data property
        public T GetProperty<T>(IBlockProperty<T> property)
        {
            return property.Decode(MetaData);
        }
        
        //create block state with certain properties
        public BlockState WithProperty<T>(IBlockProperty<T> property, T value)
        {
            return new BlockState(BlockID, property.Encode(MetaData, value));
        }

        //hashing
        public override bool Equals(object? obj)
        {
            return obj is BlockState other &&
                   BlockID == other.BlockID &&
                   MetaData == other.MetaData;
        }

        public static bool operator ==(BlockState left, BlockState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlockState left, BlockState right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BlockID, MetaData);
        }
    }

    //interface for decoding and encoding block property values into ushort metadata
    public interface IBlockProperty<T>
    {
        T Decode(ushort data);
        ushort Encode(ushort oldData, T newValue);
    }

    //allows to stack properties ontop of eachother without worrying about order and bit count
    public class PropertyLayoutBuilder
    {
        private int currentOffset = 0;

        public EnumProperty<T> AddEnum<T>() where T : Enum
        {
            int count = Enum.GetValues(typeof(T)).Length;
            int bitCount = (int)Math.Ceiling(Math.Log2(count));
            var prop = new EnumProperty<T>(currentOffset);
            currentOffset += bitCount;
            return prop;
        }

        public BoolProperty AddBool()
        {
            var prop = new BoolProperty(currentOffset);
            currentOffset += 1;
            return prop;
        }

        public ByteProperty AddByte()
        {
            var prop = new ByteProperty(currentOffset);
            currentOffset += 8;
            return prop;
        }

        public int BitsUsed => currentOffset;
    }

    //implements the decoding and incoding interfaces to convert different types of primitives to metadata
    //converts enum values to metadata for block states
    public class EnumProperty<T> : IBlockProperty<T> where T : Enum
    {
        private readonly int bitOffset;
        private readonly int bitCount;
        private readonly T[] values;

        public EnumProperty(int offset)
        {
            values = Enum.GetValues(typeof(T)) as T[] ?? throw new Exception("Enum type invalid.");
            bitOffset = offset;
            bitCount = (int)Math.Ceiling(Math.Log2(values.Length));
        }

        //get actual property value from the enum
        public T Decode(ushort data)
        {
            int mask = (1 << bitCount) - 1;
            int index = (data >> bitOffset) & mask;
            return values[index];
        }

        //put property value into bytecode
        public ushort Encode(ushort oldData, T newValue)
        {
            int mask = (1 << bitCount) - 1;
            int index = Array.IndexOf(values, newValue);

            if (index < 0 || index > mask)
                throw new Exception($"Invalid value {newValue} for property.");

            //clear the existing bits
            oldData &= (ushort)~(mask << bitOffset);

            //set the new value
            return (ushort)(oldData | (index << bitOffset));
        }
    }

    //converts boolean values into metadata for block states
    public class BoolProperty : IBlockProperty<bool>
    {
        private readonly int bitOffset;

        public BoolProperty(int offset)
        {
            bitOffset = offset;
        }

        public bool Decode(ushort data)
        {
            return ((data >> bitOffset) & 1) != 0;
        }

        public ushort Encode(ushort oldData, bool newValue)
        {
            ushort cleared = (ushort)(oldData & ~(1 << bitOffset));
            return (ushort)(cleared | ((newValue ? 1 : 0) << bitOffset));
        }
    }

    //converts bytes 0-255 to metadata for block states
    public class ByteProperty : IBlockProperty<byte>
    {
        private readonly int bitOffset;

        public ByteProperty(int offset)
        {
            bitOffset = offset;
        }

        public byte Decode(ushort data)
        {
            return (byte)((data >> bitOffset) & 0xFF);
        }

        public ushort Encode(ushort oldData, byte newValue)
        {
            ushort cleared = (ushort)(oldData & ~(0xFF << bitOffset));
            return (ushort)(cleared | (newValue << bitOffset));
        }
    }
}
