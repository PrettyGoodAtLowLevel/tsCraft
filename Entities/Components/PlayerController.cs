using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Graphics;
using OurCraft.Utility;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //physics based fps style controller
    public class PlayerController : Component
    {
        //collisions
        public float playerHeight = 1.8f;
        public float playerWidth = 0.6f;
        public Vector3 headOffset = Vector3.UnitY * RenderingConstants.CAM_HEIGHT_OFFSET;

        //basic controls
        public readonly float airAccel = 15f;
        public readonly float groundAccel = 100f;
        public readonly float jumpForce = 8.0f;
        public readonly double Sensitivity = 0.02f;

        //gravity
        public readonly float fluidGravity = 0.25f;
        public readonly float regularGravity = 2.25f;

        //speed control
        public readonly float maxSpeedXZ = 6.5f;
        public readonly float maxSpeedY = 100.0f;

        public readonly float fluidDragXZ = 2.5f;
        public readonly float regularDragXZ = 1.5f;

        public readonly float fluidDragY = 1.5f;
        public readonly float regularDragY = 0.1f;

        public readonly float fluidFriction = 0.0f;
        public readonly float regularFriction = 10.0f;

        //platforming settings
        public readonly float coyoteTime = 0.12f;
        public readonly float jumpBufferTime = 0.15f;

        //debug
        private bool wasGrounded = false;
        private float curSpeed = 0.0f;
        private float jumpBufferTimer = 0f;
        private float coyoteTimer = 0f; //for when being able to jump

        Vector2 lookVector;
        Vector3d moveDir;
        public PhysicsObj? rb;

        internal override void Register()
        {
            BaseSystem<PlayerController>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<PlayerController>.Unregister(this);
        }

        //initalize physics properties
        public override void OnStart()
        {
            rb = GameObject.GetComponent<PhysicsObj>();
            if (rb == null) return;

            rb.bounds = new(playerWidth, playerHeight, playerWidth);
            rb.headOffset = headOffset;
            rb.maxVelY = maxSpeedY;
            rb.maxVelXZ = maxSpeedXZ;
        }

        //manage speed, input, and look direction of player
        public override void OnUpdate(ChunkManager world, double time, KeyboardState kb, MouseState ms)
        {
            if (rb == null) return;

            UpdateRotation(ms);
            UpdateDir(kb);

            ManageCoyoteTime((float)time, rb);                    
            JumpInput(kb, (float)time, rb);
            ManageSpeed(rb);

            PostProcessEffects(rb);
        }

        //simply move player in look direction
        public override void OnFixedUpdate(ChunkManager world)
        {
            if (moveDir != Vector3d.Zero) rb?.AddForce(moveDir.Normalized() * curSpeed);
        }

        //update look rotation based on mouse input
        void UpdateRotation(MouseState mouse)
        {
            lookVector.Y -= (float)(mouse.Delta.X * Sensitivity);
            lookVector.X -= (float)(mouse.Delta.Y * Sensitivity);
            lookVector.X = Math.Clamp(lookVector.X, -89f, 89f);
            lookVector.Y %= 360f;

            Quaternion pitch = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(lookVector.X));
            Quaternion yaw = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(lookVector.Y));
            Transform.rotation = Quaternion.Normalize(yaw * pitch);
        }

        //update move direction based on input and look rotation
        void UpdateDir(KeyboardState kb)
        {
            Vector3d forward = Transform.Forward;
            Vector3d right = Transform.Right;

            forward.Y = 0;
            forward.Normalize();
            right.Y = 0;
            right.Normalize();
            moveDir = Vector3d.Zero;

            if (kb.IsKeyDown(Keys.W)) moveDir += forward;
            if (kb.IsKeyDown(Keys.S)) moveDir -= forward;
            if (kb.IsKeyDown(Keys.A)) moveDir -= right;
            if (kb.IsKeyDown(Keys.D)) moveDir += right;
        }

        //track when the player leaves the ground to start the coyote timer
        void ManageCoyoteTime(float deltaTime, PhysicsObj rb)
        {
            if (rb == null) return;
            
            if (wasGrounded && !rb.grounded && !rb.inFluid) coyoteTimer = coyoteTime;
            else if (rb.grounded || rb.inFluid) coyoteTimer = 0f;
            else coyoteTimer = Math.Max(0f, coyoteTimer - deltaTime);

            wasGrounded = rb.grounded;
        }

        //update player speed based on in air, grounded, or in fluid
        void ManageSpeed(PhysicsObj rb)
        {
            curSpeed = (rb.grounded && !rb.inFluid) ? groundAccel : airAccel;
            rb.gravityModifer = rb.inFluid ? fluidGravity : regularGravity;

            rb.dragX = rb.inFluid ? fluidDragXZ : regularDragXZ;
            rb.dragZ = rb.inFluid ? fluidDragXZ : regularDragXZ;
            rb.dragY = rb.inFluid ? fluidDragY : regularDragY;
        }

        //do post process fx when underwater
        static void PostProcessEffects(PhysicsObj rb)
        {
            if (rb.underWater)
            {
                Renderer.postShader.SetVector3("tintColor", new Vector3(0.01f, 0.0f, 0.5f));
                Renderer.postShader.SetFloat("tintIntensity", 0.8f);
            }
            else
            {
                Renderer.postShader.SetVector3("tintColor", new Vector3(0.0f, 0.0f, 1.0f));
                Renderer.postShader.SetFloat("tintIntensity", 0.0f);
            }
        }

        //jumping input, jump buffering, reg jump, or coyote time jump
        void JumpInput(KeyboardState kb, float deltaTime, PhysicsObj rb)
        {
            if (rb == null) return;
            if (kb.IsKeyPressed(Keys.D3)) rb.AddImpulse(Vector3d.UnitY * 120);
            if (rb.inFluid)
            {
                if (kb.IsKeyDown(Keys.Space)) moveDir += Vector3.UnitY;
                if (kb.IsKeyDown(Keys.LeftShift)) moveDir -= Vector3.UnitY;
                return;
            }

            //count down the buffer timer each frame, and reset it when Space is freshly pressed
            if (kb.IsKeyPressed(Keys.Space)) jumpBufferTimer = jumpBufferTime;
            else jumpBufferTimer = Math.Max(0f, jumpBufferTimer - deltaTime);

            //jump if able to and reset jump buffer + coyote time
            bool canJump = (rb.grounded || coyoteTimer > 0f) && rb.velocity.Y < 0.1f;
            if (jumpBufferTimer > 0f && canJump)
            {
                rb.AddImpulse(Vector3d.UnitY * jumpForce);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }
        }
    }
}