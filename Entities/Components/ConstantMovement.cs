using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.World;
using OurCraft.Physics;

namespace OurCraft.Entities.Components
{
    //moves an entity in the direction they are facing
    public class ConstantMovement : Component
    {
        public int speed = 1;

        internal override void Register()
        {
            BaseSystem<ConstantMovement>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<ConstantMovement>.Unregister(this);
        }

        public override void OnUpdate(ChunkManager world, double time, KeyboardState kb, MouseState ms)
        {
            Transform.position += speed * Transform.Forward * (float)time;
        }
    }
}
