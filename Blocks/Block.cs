using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Blocks.Meshing;
using OurCraft.Physics.PhysicsData;
using OurCraft.World.WorldData;

namespace OurCraft.Blocks
{
    //tightly packed neighbor block state data
    public struct NeighborBlocks
    {
        public BlockState thisState, bottom, top, front, back, right, left;
    }

    public enum BlockEntityRenderType
    {
        ChunkMesh, SeparateRenderer, Dynamic
    }

    //this is the actual logic that we point to once we have a blockstate
    public abstract class Block
    {
        //internal block data
        protected string name = "";       
        protected ushort id = 0;

        //properties
        public BlockState DefaultState => StateContainer?.DefaultState ?? new BlockState(id, 0); //block id with no meta
        public List<IBlockProperty> Properties = [];
        public Dictionary<IBlockProperty, byte> PropertyLookup = [];
        public BlockStateContainer StateContainer = new();

        //physics
        public float friction = 10.0f;
        public float wallFriction = 0.0f;
        public float bounce = 0.0f;

        //visuals
        public BlockShape blockShape;
        public BlockEntityRenderType blockEntityRenderType;
        public bool IsRenderSolid = true;

        //globals
        public static readonly BlockState AIR;
        static Block() { AIR = new BlockState(0); }

        //ctr
        public Block(string name, BlockShape shape)
        {
            this.name = name;
            blockShape = shape;
        }

        //required data
        public void SetID(ushort id) { this.id = id; }
        public ushort GetID() { return id; }
        public virtual bool RequiresScheduledTicks => false;
        public virtual bool RequiresRandomTicks => false;
        public virtual bool HasBlockEntity => false;
        public virtual BlockEntity CreateBlockEntity(BlockState state, Vector3i globalPosition) { throw new InvalidOperationException(); }

        //world and player interactions
        public virtual void OnInteract(Vector3i pos, ChunkManager world, BlockState state) { }
        public virtual void RandomTick(Vector3i pos, ChunkManager world, BlockState state) { }
        public virtual void ScheduledTick(Vector3i pos, ChunkManager world, BlockState state) { }
        public virtual void OnNeighborChanged(Vector3i pos, ChunkManager world, BlockState state) { }
        public virtual void OnPlaced(Vector3i pos, ChunkManager world, BlockState state) { }
        public virtual void OnRemoved(Vector3i pos, ChunkManager world, BlockState state) { }

        //determines each block's way of changing the chunk block state data when placed
        public virtual void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState thisBlock, ChunkManager world) { }
        //determines which block state will be placed in the world if block was placed by player
        public virtual CollisionShape GetPredictedCollisionShape(Vector3 globalPos, Vector3 hitNormal, BlockState thisBlock, ChunkManager world) 
        { return DefaultState.GetCollisionShape(); }

        //br br patabim
        //block state
        public virtual void DebugState(BlockState state) { Console.Write("\n" + DefaultState.Name); }
        public string GetBlockName() => name;
        public bool HasProperty(IBlockProperty prop) => PropertyLookup.ContainsKey(prop);

        //lighting
        public virtual bool IsLightSource(BlockState state) => false; 
        public virtual bool IsLightPassable(BlockState state) => false;
        public virtual Vector3i GetLightSourceLevel(BlockState state) => Vector3i.Zero;
        public virtual int GetSkyLightAttenuation(BlockState state) => 15;
        public virtual bool AOSolid(BlockState state) => false;

        //physics
        public virtual bool DetectsCollision(BlockState state) => false; //detects collisions
        public virtual bool IsPhysicsSolid(BlockState state) => false;   //cant walk past
        public virtual bool IsFluid(BlockState state) => false;
        public virtual CollisionShape GetCollisionShape(BlockState state) => CollisionShapeData.Empty;
        public virtual BlockPhysics GetBlockPhysics(BlockState state)
        {
            BlockPhysics physics = new()
            { friction = this.friction, bounce = this.bounce, wallFriction = this.wallFriction };
            return physics;
        }
    }
}