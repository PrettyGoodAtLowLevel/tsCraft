using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //represents a physics object in our world
    public class PhysicsObj : Component
    {
        //forces
        public Vector3d previousPosition;
        public Vector3d position;
        public Vector3d headOffset;
        public Vector3d bounds;
        public Vector3d velocity;
        public Vector3d acceleration;
        public Vector3d HeadPosition { get => position + headOffset; }

        //settings
        public bool useFriction = true;
        public bool bounce = true;

        public float gravityModifer = 2.25f;
        public float maxVelXZ = 100.0f;
        public float maxVelY = 100.0f;

        //air drag coefficients
        public double dragX = 1.5;
        public double dragZ = 1.5;
        public double dragY = 0.1;

        public bool grounded = false;
        public bool inFluid = false;
        public bool underWater = false;

        internal override void Register()
        {
            BaseSystem<PhysicsObj>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<PhysicsObj>.Unregister(this);
        }

        //initialize all physical properties
        public override void OnCreation()
        {
            previousPosition = Vector3d.Zero;
            velocity = Vector3d.Zero;
            headOffset = Vector3d.Zero;
            acceleration = Vector3d.Zero;
            position = Transform.position;
            bounds = Vector3d.One;
        }

        //reset all forces
        public override void OnDestroy()
        {
            velocity = Vector3d.Zero;
            acceleration = Vector3d.Zero;
        }

        //set transform position to interpolated position
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
