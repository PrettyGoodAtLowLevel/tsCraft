using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //fps style player free cam
    public class CameraController : Component
    {
        private readonly double Speed = 10f;
        private readonly double Sensitivity = 0.02f;       
        Vector2 lookVector;

        internal override void Register()
        {
            BaseSystem<CameraController>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<CameraController>.Unregister(this);
        }

        public override void OnUpdate(ChunkManager world, double time, KeyboardState kb, MouseState ms)
        {
            UpdateRotation(ms);
            UpdateMovement(kb, time);
        }

        void UpdateRotation(MouseState mouse)
        {
            //mouse delta
            lookVector.Y -= (float)(mouse.Delta.X * Sensitivity);
            lookVector.X -= (float)(mouse.Delta.Y * Sensitivity);

            //clamp pitch not to look to far up, clamp yaw to avoid overflow
            lookVector.X = Math.Clamp(lookVector.X, -89f, 89f);
            lookVector.Y %= 360f;

            //create rotation
            Quaternion pitch =
            Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(lookVector.X));

            Quaternion yaw =
            Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(lookVector.Y));

            Transform.rotation = Quaternion.Normalize(yaw * pitch);
        }

        void UpdateMovement(KeyboardState kb, double dt)
        {
            double v = Speed * dt;

            Vector3d forward = Transform.Forward;
            Vector3d right = Transform.Right;

            //flatten for ground movement
            forward.Y = 0;
            forward.Normalize();

            if (kb.IsKeyDown(Keys.W)) Transform.position += forward * v;
            if (kb.IsKeyDown(Keys.S)) Transform.position -= forward * v;
            if (kb.IsKeyDown(Keys.A)) Transform.position -= right * v;
            if (kb.IsKeyDown(Keys.D)) Transform.position += right * v;

            if (kb.IsKeyDown(Keys.Space)) Transform.position += Vector3d.UnitY * v;
            if (kb.IsKeyDown(Keys.LeftShift)) Transform.position += -Vector3d.UnitY * v;
        }
    }
}
