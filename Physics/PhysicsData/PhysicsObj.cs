using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Entities.Internal;
using OurCraft.World.WorldData;

namespace OurCraft.Physics.PhysicsData
{
    //represents a physics object in our world
    public class PhysicsObj : Component
    {
        //force data
        public Vector3d previousPosition;
        public Vector3d position;     
        public Vector3d velocity;
        public Vector3d acceleration;

        //local collision shape
        public Vector3d headOffset;
        public Vector3d boundsMin;
        public Vector3d boundsMax;      

        //step settings
        public double groundStepHeight = 0.5625; //step over slabs
        public double airStepHeight = 0.03125; //1/32th of a block

        //physics settings
        public PhysicsMaterial physicsMaterial;
        public float gravityModifer = 2.25f;

        public double dragX = 1.5;
        public double dragZ = 1.5;
        public double dragY = 0.1;

        //clamp vel to not go out of hand
        public float maxVelXZ = 100.0f;
        public float maxVelY = 100.0f;

        //debug
        public bool noClip = false;
        public bool grounded = false;
        public bool sneaking = false;
        public bool inFluid = false;
        public bool underWater = false;
        public Vector3d HeadPosition { get => position + headOffset; }

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
            previousPosition = Transform.WorldPosition;
            position = Transform.WorldPosition;

            velocity = Vector3d.Zero;          
            acceleration = Vector3d.Zero;
            
            boundsMin = Vector3d.Zero;
            boundsMax = Vector3d.One;
            headOffset = Vector3d.Zero;

            physicsMaterial = new()
            {
                bounceCombine = 1.0f,
                useBounce = true,

                frictionCombine = 1.0f,
                useFriction = true,

                wallFrictionCombine = 1.0f,
                useWallFriction = true
            };
        }

        //reset all forces
        public override void OnDestroy()
        {
            velocity = Vector3d.Zero;
            acceleration = Vector3d.Zero;
        }

        //set transform position to interpolated position
        public override void OnUpdate(ChunkManager world, KeyboardState kb, MouseState ms)
        {
            Transform.localPosition = GetRenderPos();
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
