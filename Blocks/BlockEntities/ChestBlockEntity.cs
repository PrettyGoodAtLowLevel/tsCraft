using OpenTK.Mathematics;
using OurCraft.World.WorldData;

namespace OurCraft.Blocks.BlockEntities
{
    public class ChestBlockEntity : BlockEntity
    {
        public string CustomName = "Chest Entity";
        public List<int> storage = [];

        public override void OnInteract(ChunkManager world, BlockState state)
        {
            Console.Clear();
            Console.WriteLine("---chest entity interaction----");
            Console.WriteLine(CustomName);
            storage.Add(2);
            Console.WriteLine("Storage Count: " + storage.Count);          
            world.SetBlock(GlobalPosition + Vector3i.UnitY, Block.AIR);
            Console.WriteLine(state);
            Console.WriteLine("-------------------------------");
        }
    }
}
