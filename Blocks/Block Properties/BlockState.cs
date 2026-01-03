using OpenTK.Mathematics;

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

        //getters, and block functions
        public Block GetBlock => BlockData.GetBlock(BlockID);
        public BlockShape BlockShape => GetBlock.blockShape;
        public string Name => GetBlock.GetBlockName();
        public bool IsLightSource => GetBlock.IsLightSource(this);
        public bool LightPassable => GetBlock.IsLightPassable(this);
        public Vector3i LightLevel => GetBlock.GetLightSourceLevel(this);
        public int SkyLightAttenuation => GetBlock.GetSkyLightAttenuation(this);
        public void DebugState()
        {
            GetBlock.DebugState(this);
        }

        //get meta data property
        public T GetProperty<T>(IBlockProperty<T> property)
        {
            return property.Decode(MetaData);
        }

        //hashing
        public override bool Equals(object? obj)
        {
            return obj is BlockState other && BlockID == other.BlockID && MetaData == other.MetaData;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BlockID, MetaData);
        }

        //simple checks if block states are the same
        public static bool operator ==(BlockState left, BlockState right)
        {
            return left.BlockID == right.BlockID && left.MetaData == right.MetaData;
        }

        public static bool operator !=(BlockState left, BlockState right)
        {
            return !(left == right);
        }       
    }

    //interface for decoding and encoding block property values into ushort metadata
    // ---------- non-generic property marker ----------
    public interface IBlockProperty 
    { 
        // Number of bits this property consumes
        int BitCount { get; } 
        // Decode the property value from raw metadata (returns boxed value)
        object Decode(ushort data); 
        // Encode a boxed value into metadata given existing metadata
        ushort Encode(ushort oldData, object value); 
    }

    //generic property interface extends marker
    public interface IBlockProperty<T> : IBlockProperty
    { 
        new T Decode(ushort data); 
        ushort Encode(ushort oldData, T newValue);
    }

    //allows to stack properties ontop of eachother without worrying about order and bit count
    // ----------property layout builder and property implementations----------
    public class PropertyLayoutBuilder 
    { 
        private int currentOffset = 0;
        public int BitsUsed => currentOffset;
        //add an enum property and return the EnumProperty with assigned offset
        public EnumProperty<T> AddEnum<T>() where T : Enum
        { 
            int count = Enum.GetValues(typeof(T)).Length; 
            int bitCount = Math.Max(1, (int)Math.Ceiling(Math.Log(count, 2)));
            var prop = new EnumProperty<T>(currentOffset, bitCount); 
            currentOffset += bitCount; return prop; 
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
            currentOffset += 8; return prop;
        }        
    }

    //contains the default block state, state lookup, and all possible state combinations
    public class BlockStateContainer
    { 
        //all possible states (ordered by metadata)
        public BlockState[] States = []; 
        
        //default state (metadata == 0)
        public BlockState DefaultState;

        //lookup by metadata value (0 .. MetaSize-1)
        public BlockState[] MetaLookup = []; 

        //optional dictionary for quick existence checks or other uses
        public Dictionary<ushort, BlockState> StateLookup = []; 
    }

    //contains helper methods for easily using blockstates
    public static class BlockStateExtensions
    { 
        //fluent wrapper that uses the block's container to return a cached state
        public static BlockState With<T>(this BlockState state, IBlockProperty<T> property, T value)
        {
            //compute new metadata using the property's Encode
            ushort newMeta = property.Encode(state.MetaData, value); 
            //get the block instance and its container
            var block = state.GetBlock; 
            var container = block.StateContainer;

            if (container == null)  throw new InvalidOperationException("BlockStateContainer not initialized for block " + block.GetBlockName());
            //return cached state from MetaLookup
            if (newMeta < container.MetaLookup.Length) return container.MetaLookup[newMeta]; 

            //fallback: return default state
            return container.DefaultState; 
        } 

        //generate all states for a block and return a container
        public static BlockStateContainer GenerateStates(Block block)
        {
            //compute total bits used by summing property bit counts
            int bitsUsed = block.Properties.Sum(p => p.BitCount);
            int metaSize = 1 << bitsUsed; 
            //number of metadata combinations
            var allStates = new BlockState[metaSize]; 

            //create every metadata combination
            for (int meta = 0; meta < metaSize; meta++)
            {                
                allStates[meta] = new BlockState(block.GetID(), (ushort)meta);
            } 
            var container = new BlockStateContainer
            { 
                States = allStates, MetaLookup = allStates,
                StateLookup = new Dictionary<ushort, BlockState>(metaSize) 
            }; 
            //fill dictionary and default state
            for (ushort i = 0; i < allStates.Length; i++)
            { 
                container.StateLookup[i] = allStates[i];
            } 
            container.DefaultState = container.States[0];
            return container;
        }
    }

    //enum property
    public class EnumProperty<T> : IBlockProperty<T> where T : Enum 
    { 
        private readonly int bitOffset; 
        private readonly int bitCount; private readonly T[] values;
        public int BitCount => bitCount;

        public EnumProperty(int offset, int bitCount)
        { 
            values = Enum.GetValues(typeof(T)) as T[] ?? throw new Exception("Enum type invalid.");
            bitOffset = offset; this.bitCount = bitCount; 
        } 
        
        public T Decode(ushort data)
        { 
            int mask = (1 << bitCount) - 1; 
            int index = (data >> bitOffset) & mask; 
            if (index < 0 || index >= values.Length) 
                index = 0; return values[index];
        } 

        object IBlockProperty.Decode(ushort data) => Decode(data); 
        public ushort Encode(ushort oldData, T newValue)
        { 
            int mask = (1 << bitCount) - 1; 
            int index = Array.IndexOf(values, newValue);
            if (index < 0 || index > mask) throw new Exception($"Invalid value {newValue} for property."); 
            
            //clear the existing bits
            oldData = (ushort)(oldData & ~(mask << bitOffset)); 
            
            //set the new value
            return (ushort)(oldData | (index << bitOffset)); 
        } 
        ushort IBlockProperty<T>.Encode(ushort oldData, T newValue) => Encode(oldData, newValue); 

        //boxed encode for non-generic interface
        ushort IBlockProperty.Encode(ushort oldData, object value)
        {
            return Encode(oldData, (T)value); 
        }
    } 
    
    //bool property
    public class BoolProperty : IBlockProperty<bool> 
    { 
        private readonly int bitOffset; 
        private readonly int bitCount = 1;
        public int BitCount => bitCount;
        public BoolProperty(int offset)
        {
            bitOffset = offset;
        } 
        
        public bool Decode(ushort data) 
        { 
            return ((data >> bitOffset) & 1) != 0;
        } 

        object IBlockProperty.Decode(ushort data) => Decode(data);

        public ushort Encode(ushort oldData, bool newValue)
        {
            ushort cleared = (ushort)(oldData & ~(1 << bitOffset)); 
            return (ushort)(cleared | ((newValue ? 1 : 0) << bitOffset)); 
        } 

        ushort IBlockProperty<bool>.Encode(ushort oldData, bool newValue) => Encode(oldData, newValue); 
        ushort IBlockProperty.Encode(ushort oldData, object value) => Encode(oldData, (bool)value);
    }

    //byte property
    public class ByteProperty : IBlockProperty<byte>
    {        
        private readonly int bitOffset;
        private readonly int bitCount = 8;
        public ByteProperty(int offset)
        { 
            bitOffset = offset; 
        } 
        public int BitCount => bitCount; 
        public byte Decode(ushort data)
        {
            return (byte)((data >> bitOffset) & 0xFF);
        } 
        object IBlockProperty.Decode(ushort data) => Decode(data);
        public ushort Encode(ushort oldData, byte newValue)
        { 
            ushort cleared = (ushort)(oldData & ~(0xFF << bitOffset)); 
            return (ushort)(cleared | (newValue << bitOffset)); 
        } 
        ushort IBlockProperty<byte>.Encode(ushort oldData, byte newValue) => Encode(oldData, newValue);
        ushort IBlockProperty.Encode(ushort oldData, object value) => Encode(oldData, (byte)value); 
    }
}