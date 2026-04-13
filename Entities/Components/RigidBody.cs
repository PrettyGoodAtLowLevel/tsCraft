using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //represents a physics object in our world
    public class RigidBody : Component
    {
        //forces
        public Vector3d previousPosition;
        public Vector3d position;
        public Vector3d bounds;
        public Vector3d velocity;
        public Vector3d acceleration;

        //settings
        public bool clampVel = true;
        public bool useGravity = true;       

        //air drag coefficients
        public double dragX = 1.5;
        public double dragZ = 1.5;
        public double dragY = 0.1;

        //ground drag coefficients
        public double groundDragX = 10.0;
        public double groundDragZ = 10.0;

        public bool grounded = false;

        internal override void Register()
        {
            BaseSystem<RigidBody>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<RigidBody>.Unregister(this);
        }

        public override void OnCreation()
        {
            previousPosition = Vector3d.Zero;
            velocity = Vector3d.Zero;
            acceleration = Vector3d.Zero;
            position = Transform.position;
            bounds = Vector3d.One;
        }

        public override void OnDestroy()
        {
            velocity = Vector3d.Zero;
            acceleration = Vector3d.Zero;
        }

        public override void OnUpdate(ChunkManager world, double time, KeyboardState kb, MouseState ms)
        {
            Transform.position = GetRenderPos();
        }

        //instantly changes the velocity to a rigid body
        public void AddImpulse(Vector3d force)
        {
            velocity += force;
        }

        //adds a force over time to a rigid body
        public void AddForce(Vector3d force)
        {
            acceleration += force;
        }

        //gets the interpolated rendering pos for our rigidbody
        public Vector3d GetRenderPos()
        {
            return Vector3d.Lerp(previousPosition, position, EntityManager.Alpha);
        }
    }
}
