using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.BlockEntities;
using OurCraft.Blocks.Meshing;
using OurCraft.World.WorldData;

namespace OurCraft.Blocks.BlockTypes
{
    public class ChestBlock : DefaultBlock
    {
        //adds the properties to THIS instance of the block
        public ChestBlock(string name, BlockShape shape) : base(name, shape) { }

        public override BlockEntity CreateBlockEntity(BlockState state, Vector3i globalPosition)
        {
            return new ChestBlockEntity
            {
                GlobalPosition = globalPosition,
            };
        }

        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState thisBlock, ChunkManager world)
        {
            world.SetBlock(globalPos + hitNormal, DefaultState);
        }

        public override void OnPlaced(Vector3i pos, ChunkManager world, BlockState state)
        {
            BlockEntity? ent = world.TryGetBlockEntity(pos);
            if (ent == null) return;
        }

        public override bool HasBlockEntity => true;
    }
}
