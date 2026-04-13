using OpenTK.Mathematics;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Blocks.Meshing;
using OurCraft.Physics;

namespace OurCraft.Blocks
{
    //we pass around block neighbors so much its better to keep them packed in a struct
    public struct NeighborBlocks
    {
        public BlockState thisState, bottom, top, front, back, right, left;
    }

    //this is the actual logic that we point to once we have a blockstate
    public abstract class Block
    {
        //internal block data
        protected string name = "";       
        protected ushort id = 0;

        //properties
        public BlockState DefaultState => StateContainer?.DefaultState ?? new BlockState(id, 0); //block with no meta
        public List<IBlockProperty> Properties = [];
        public Dictionary<IBlockProperty, byte> PropertyLookup = [];
        public BlockStateContainer StateContainer = new();

        //visuals
        public BlockShape blockShape;
        public bool IsSolid = true;

        //globals
        public static readonly BlockState AIR;

        static Block() { AIR = new BlockState(0); }

        //ctr
        public Block(string name, BlockShape shape)
        {
            this.name = name;
            blockShape = shape;
        }

        public Block()
        {
            name = "Empty Block";
            id = ushort.MaxValue;
            blockShape = new EmptyBlockShape();
        }

        //set id of block
        public void SetID(ushort id) { this.id = id; }

        //get the id of the block
        public ushort GetID() { return id; }

        //determines each block's way of changing the chunk block state data when placed
        public virtual void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, ChunkManager world) { } 

        //determines each block's way of updating
        public virtual void UpdateBlockState(Vector3 globalPos, BlockState thisBlock, ChunkManager world) { }

        //allows to view current properties of a blockstate
        public virtual void DebugState(BlockState state) { Console.Write("\n" + DefaultState.Name); }

        //properties
        public string GetBlockName() => name;
        public bool HasProperty(IBlockProperty prop) => PropertyLookup.ContainsKey(prop);

        public virtual bool IsLightSource(BlockState state) => false; 
        public virtual bool IsLightPassable(BlockState state) => false;
        public virtual Vector3i GetLightSourceLevel(BlockState state) => Vector3i.Zero;
        public virtual int GetSkyLightAttenuation(BlockState state) => 15;

        public virtual bool DetectsCollision(BlockState state) => false; //detects collisions
        public virtual bool IsPhysicsSolid(BlockState state) => false; //cant walk past
        public virtual bool IsFluid(BlockState state) => false;
        public virtual AABB GetAABB(Vector3d worldPos, BlockState state) => new AABB();
        public virtual BlockPhysics GetBlockPhysics(BlockState state) => new BlockPhysics();
    }
}