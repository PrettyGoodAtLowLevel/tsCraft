using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Physics;
using OurCraft.Entities.Internal;
using OurCraft.World.WorldData;

namespace OurCraft.Entities.Components.Gameplay
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

        //move entity in forward vector
        public override void OnUpdate(ChunkManager world,  KeyboardState kb, MouseState ms)
        {
            Transform.localPosition += speed * Transform.Forward * (float)Time.DeltaTime;
        }
    }
}