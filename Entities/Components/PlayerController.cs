using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //fps style player free cam
    public class PlayerController : Component
    {
        private readonly float speed = 50f;
        private readonly double Sensitivity = 0.02f;

        Vector2 lookVector;
        Vector3d moveDir;
        public RigidBody? rb;

        internal override void Register()
        {
            BaseSystem<PlayerController>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<PlayerController>.Unregister(this);
        }

        public override void OnUpdate(ChunkManager world, double time, KeyboardState kb, MouseState ms)
        {
            UpdateRotation(ms);
            UpdateDir(kb);
        }

        public override void OnFixedUpdate(ChunkManager world)
        {
            if (moveDir != Vector3d.Zero) rb?.AddForce(moveDir.Normalized() * speed);
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

        void UpdateDir(KeyboardState kb)
        {
            Vector3d forward = Transform.Forward;
            Vector3d right = Transform.Right;
            //flatten for ground movement
            forward.Y = 0;
            forward.Normalize();
            right.Y = 0;
            right.Normalize();

            moveDir = Vector3d.Zero;
            if (kb.IsKeyDown(Keys.W)) moveDir += forward;
            if (kb.IsKeyDown(Keys.S)) moveDir -= forward;
            if (kb.IsKeyDown(Keys.A)) moveDir -= right;
            if (kb.IsKeyDown(Keys.D)) moveDir += right;
            if (kb.IsKeyDown(Keys.Space)) moveDir += Vector3d.UnitY;
            if (kb.IsKeyDown(Keys.LeftShift)) moveDir -= Vector3d.UnitY;
        }
    }
}
