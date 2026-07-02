using OpenTK.Mathematics;
using OurCraft.World;

namespace OurCraft.Blocks
{
    //block, with extra complex behavior and meta data
    public abstract class BlockEntity
    {
        public Vector3i GlobalPosition { get; internal set; }       

        //empty ctor
        public BlockEntity() { }

        public virtual void OnInteract(ChunkManager world, BlockState state) { }

        public virtual void Tick(ChunkManager world) { }

        public virtual void Load(BinaryReader reader) { }

        public virtual void Save(BinaryWriter writer) { }
    }
}
