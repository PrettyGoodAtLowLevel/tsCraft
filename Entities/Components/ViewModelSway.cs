using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Physics;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //does view model look sway
    public class ViewModelSway : Component
    {
        public float smooth = 2;
        public float swayMultiplier = 0.005f;
        float maxStepDistance = 12.5f;

        internal override void Register()
        {
            BaseSystem<ViewModelSway>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<ViewModelSway>.Unregister(this);
        }

        public override void OnUpdate(ChunkManager world, KeyboardState kb, MouseState ms)
        {
            UpdateRotation(ms, (float)Time.DeltaTime);
        }

        //does swaying based on mouse movement
        void UpdateRotation(MouseState mouse, float dt)
        {
            //get mouse movement
            float mouseDeltaX = mouse.Delta.X / dt;
            float mouseDeltaY = mouse.Delta.Y / dt;

            float y = Math.Clamp(-(mouseDeltaX * swayMultiplier), -maxStepDistance, maxStepDistance);
            float x = Math.Clamp(-(mouseDeltaY * swayMultiplier), -maxStepDistance, maxStepDistance);

            //create look rotation
            Quaternion pitch = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(x));
            Quaternion yaw = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(y));
            Quaternion targetRotation = Quaternion.Normalize(yaw * pitch);

            //spherically interpolate current rotation to look rotation
            Transform.localRotation = Quaternion.Slerp(Transform.localRotation, targetRotation, smooth * dt);
        }
    }
}