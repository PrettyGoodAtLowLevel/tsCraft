using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Blocks.Meshing;
using OurCraft.Physics;
using OurCraft.World;

namespace OurCraft.Blocks
{
    //block states use a property system to encode their values
    //values can be stuff like "facing:north", "powered:true"
    //these values can then be encoded to metadata so it is compact and memory effiecent
    public readonly struct BlockState
    {
        public readonly ushort BlockID;  //the id in the global block array
        public readonly ushort MetaData; //up to 16 bits of custom data

        //default constructor
        public BlockState(ushort id, ushort metadata = 0)
        {
            BlockID = id;
            MetaData = metadata;
        }

        //internal block data
        public string Name => GetBlock.GetBlockName();
        public Block GetBlock => BlockRegistry.GetBlock(BlockID);
        public bool HasBlockEntity => GetBlock.HasBlockEntity;    
        public BlockShape BlockShape => GetBlock.blockShape;
        public BlockEntityRenderType BlockEntityRenderType => GetBlock.blockEntityRenderType;

        //random tick
        public void OnInteract(Vector3i pos, ChunkManager world) => GetBlock.OnInteract(pos, world, this);
        public void ScheduleTick(Vector3i pos, ChunkManager world) => GetBlock.ScheduledTick(pos, world, this);
        public void RandomTick(Vector3i pos, ChunkManager world) => GetBlock.RandomTick(pos, world, this);
        public void OnNeighborChanged(Vector3i pos, ChunkManager world) => GetBlock.OnNeighborChanged(pos, world, this);
        public void OnPlaced(Vector3i pos, ChunkManager world) => GetBlock.OnPlaced(pos, world, this);
        public void OnRemove(Vector3i pos, ChunkManager world) => GetBlock.OnRemoved(pos, world, this);

        //lighting
        public bool IsLightSource => GetBlock.IsLightSource(this);
        public bool LightPassable => GetBlock.IsLightPassable(this);
        public Vector3i LightLevel => GetBlock.GetLightSourceLevel(this);
        public int SkyLightAttenuation => GetBlock.GetSkyLightAttenuation(this);
        public bool AOSolid => GetBlock.AOSolid(this);
        
        //physics
        public bool DetectsCollision => GetBlock.DetectsCollision(this);
        public bool IsPhysicsSolid => GetBlock.IsPhysicsSolid(this);
        public bool IsFluid => GetBlock.IsFluid(this);
        public CollisionShape GetCollisionShape() => GetBlock.GetCollisionShape(this);
        public BlockPhysics GetBlockPhysics() => GetBlock.GetBlockPhysics(this);

        //state tracking
        public void DebugState() => GetBlock.DebugState(this);       
        public T GetProperty<T>(IBlockProperty<T> property) => property.Decode(MetaData);

        //debug & hashing
        public override string ToString()
        {
            string str = "";

            str += $"Block ID: {BlockID}, ";
            str += $"Debug MetaData: {MetaData}, ";
            str += $"Name: '{Name}', ";
            str += $"Has Block Entity: '{HasBlockEntity}'\n";

            str += $"Is Light Source: {IsLightSource}, ";
            str += $"Is Light Passable: {LightPassable}, ";
            str += $"Light Level RGB: '{LightLevel}', ";
            str += $"Sky Light Attenuation: {SkyLightAttenuation} \n";

            str += $"Detects Collision: {DetectsCollision}, ";
            str += $"Physics Solid: {IsPhysicsSolid}, ";
            str += $"Is Fluid: {IsFluid} \n";

            return str;
        }

        public override bool Equals(object? obj)
        {
            return obj is BlockState other && BlockID == other.BlockID && MetaData == other.MetaData;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BlockID, MetaData);
        }

        public static bool operator ==(BlockState left, BlockState right)
        {
            return left.BlockID == right.BlockID && left.MetaData == right.MetaData;
        }

        public static bool operator !=(BlockState left, BlockState right)
        {
            return !(left == right);
        }       
    }
}