using OpenTK.Mathematics;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Blocks.BlockShapeData;

namespace OurCraft.Blocks
{
    //we pass around block neighbors so much its better to keep them packed in a struct
    public struct NeighborBlocks
    {
        public BlockState thisState, bottom, top, front, back, right, left;
    }

    //this is the actual logic that we point to once we have a blockstate
    //eventually blocks will be stored as json files and we will read from them when initializing all the blocks
    public abstract class Block
    {
        //internal block data
        protected string name = "";       
        protected ushort id = 0;
        public BlockState DefaultState => StateContainer?.DefaultState ?? new BlockState(id, 0);
        public List<IBlockProperty> Properties = [];
        public BlockStateContainer StateContainer = new();

        //visuals
        public BlockShape blockShape;
        public bool IsSolid = true;

        //globals
        public static readonly BlockState AIR;
        public static readonly BlockState INVALID;

        static Block()
        {
            AIR = new BlockState(BlockIDs.AIR_BLOCK);
            INVALID = new BlockState(BlockIDs.INVALID_BLOCK);
        }

        //ctr
        public Block(string name, BlockShape shape, ushort id)
        {
            this.name = name;
            this.id = id;
            blockShape = shape;
        }

        public Block()
        {
            name = "Empty Block";
            id = ushort.MaxValue;
            blockShape = BlockShapesRegistry.AirBlockShape;
        }

        //set id of block
        public void SetID(ushort id)
        {
            this.id = id;
        }

        //get the id of the block
        public ushort GetID()
        {
            return id;
        }

        // determines each block's way of changing the chunk block state data when placed
        public virtual void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world) { } 

        //determines each block's way of updating
        public virtual void UpdateBlockState(Vector3 globalPos, BlockState thisBlock, Chunkmanager world) { }

        //allows to view current properties of a blockstate
        public virtual void DebugState(BlockState state)
        {
            Console.Write("\n" + DefaultState.Name);
        }
        
        //properties
        public virtual bool IsLightSource(BlockState state) => false; 
        public virtual bool IsLightPassable(BlockState state) => false;
        public virtual Vector3i GetLightSourceLevel(BlockState state) => Vector3i.Zero; 
        public virtual int GetSkyLightAttenuation(BlockState state) => 15; 
        public string GetBlockName() => name;
    }
}