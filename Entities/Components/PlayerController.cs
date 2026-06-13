using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Graphics;
using OurCraft.Physics;
using OurCraft.Utility;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //physics based fps style controller
    public class PlayerController : Component
    {
        //collisions
        public float playerHeight = 1.8f;
        public float playerCrouchHeight = 1.0f;
        public float playerWidth = 0.6f;
        public Vector3 headOffset = Vector3.UnitY * RenderingConstants.CAM_HEIGHT_OFFSET;

        //basic controls
        public readonly float jumpForce = 8.0f;
        public readonly double Sensitivity = 0.02f;

        //stepping
        public readonly float stepHeight = 0.6f;        //can step over slabs easily
        public readonly float airStepHeight = 0.03125f; //can run around corners, but not one block gaps

        //gravity
        public readonly float fluidGravity = 0.25f;
        public readonly float regularGravity = 2.25f;
        public readonly float flyingGravity = 0.0f;

        //speed control
        public readonly float flyingAccel = 110.0f;
        public readonly float sprintAccel = 70.0f;
        public readonly float walkAccel = 55.0f;
        public readonly float crouchAccel = 15f;
        public readonly float maxSpeedY = 100.0f;

        public readonly float flyingDragXZ = 4.0f;
        public readonly float fluidDragXZ = 2.5f;
        public readonly float regularDragXZ = 1.5f;

        public readonly float flyingDragY = 3.0f;
        public readonly float fluidDragY = 1.5f;
        public readonly float regularDragY = 0.1f;

        public readonly float fluidFriction = 0.0f;
        public readonly float regularFriction = 10.0f;

        //platforming settings
        public readonly float coyoteTime = 0.12f;
        public readonly float jumpBufferTime = 0.15f;

        //debug
        private bool flying = false;
        private bool crouching = false;
        private bool sprinting = false;
        private bool wasGrounded = false;
        private float curAccel = 0.0f;
        private float jumpBufferTimer = 0f;
        private float coyoteTimer = 0f; //for when being able to jump

        Vector2 lookVector;
        Vector3d moveDir;
        Vector3d orientationOrigin = Vector3d.Zero;
        PhysicsObj? rb;
        Transform? orientation;

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
            orientation = EntityManager.GetEntity("Camera")?.Transform;

            if (rb == null || orientation == null) return;
            if (orientation != null) orientationOrigin = orientation.localPosition;

            rb.boundsMin = new(-playerWidth / 2, -playerHeight / 2, -playerWidth / 2);
            rb.boundsMax = new(playerWidth / 2, playerHeight / 2, playerWidth / 2);

            rb.headOffset = headOffset;
            rb.maxVelY = maxSpeedY;     
            
            rb.airStepHeight = airStepHeight;
            rb.groundStepHeight = stepHeight;
        }

        //manage speed, input, and look direction of player
        public override void OnUpdate(ChunkManager world, KeyboardState kb, MouseState ms)
        {
            if (rb == null || orientation == null) return;

            UpdateRotation(ms, orientation);
            UpdateDir(kb, orientation);
            ManageSpeed(rb);
            ManageCoyoteTime((float)Time.DeltaTime, rb);

            FlyingInput(kb, rb);
            SprintInput(kb, rb);
            JumpInput(kb, (float)Time.DeltaTime, rb);
            CrouchInput(kb, rb, world);
            
            PostProcessEffects(rb);       
        }

        //simply move player in look direction
        public override void OnFixedUpdate(ChunkManager world)
        {
            if (moveDir != Vector3d.Zero) rb?.AddForce(moveDir.Normalized() * curAccel);
        }

        //update look rotation based on mouse input
        void UpdateRotation(MouseState mouse, Transform orientation)
        {
            lookVector.Y -= (float)(mouse.Delta.X * Sensitivity);
            lookVector.X -= (float)(mouse.Delta.Y * Sensitivity);
            lookVector.X = Math.Clamp(lookVector.X, -89f, 89f);
            lookVector.Y %= 360f;

            Quaternion pitch = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(lookVector.X));
            Quaternion yaw = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(lookVector.Y));
            orientation.localRotation = Quaternion.Normalize(yaw * pitch);
        }

        //update move direction based on input and look rotation
        void UpdateDir(KeyboardState kb, Transform orientation)
        {
            Vector3d forward = orientation.Forward;
            Vector3d right = orientation.Right;

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
            curAccel = flying ? flyingAccel : crouching ? crouchAccel : sprinting ? sprintAccel : walkAccel;
            float airModifier = flying ? 1f : rb.grounded && !rb.inFluid ? 1f : rb.inFluid ? 0.15f : 0.1f;
            curAccel *= airModifier;

            rb.gravityModifer = flying ? flyingGravity : rb.inFluid ? fluidGravity : regularGravity;

            rb.dragX = flying ? flyingDragXZ : rb.inFluid ? fluidDragXZ : regularDragXZ;
            rb.dragZ = flying ? flyingDragXZ : rb.inFluid ? fluidDragXZ : regularDragXZ;
            rb.dragY = flying ? flyingDragY : rb.inFluid ? fluidDragY : regularDragY;
        }

        //do post process fx when underwater
        static void PostProcessEffects(PhysicsObj rb)
        {
            if (rb.underWater)
            {
                Renderer.postShader.SetVector3("tintColor", new Vector3(0.01f, 0.0f, 0.5f));
                Renderer.postShader.SetFloat("tintIntensity", 0.5f);
            }
            else
            {
                Renderer.postShader.SetVector3("tintColor", new Vector3(0.0f, 0.0f, 1.0f));
                Renderer.postShader.SetFloat("tintIntensity", 0.0f);
            }
        }

        //allows player to sprint if pressing left shift and not in water
        void SprintInput(KeyboardState kb, PhysicsObj rb)
        {
            sprinting = kb.IsKeyDown(Keys.LeftControl) && !rb.inFluid;
        }

        //jumping input, jump buffering, reg jump, or coyote time jump
        void JumpInput(KeyboardState kb, float deltaTime, PhysicsObj rb)
        {
            if (rb == null) return;
            if (kb.IsKeyPressed(Keys.D3)) rb.AddImpulse(Vector3d.UnitY * 120);
            if (rb.inFluid || flying)
            {
                if (kb.IsKeyDown(Keys.Space)) moveDir += Vector3.UnitY;
                if (kb.IsKeyDown(Keys.LeftShift)) moveDir -= Vector3.UnitY;
                return;
            }

            //count down the buffer timer each frame, and reset it when Space is freshly pressed
            if (kb.IsKeyPressed(Keys.Space)) jumpBufferTimer = jumpBufferTime;
            else jumpBufferTimer = Math.Max(0f, jumpBufferTimer - deltaTime);

            //jump if able to and reset jump buffer + coyote time
            bool canJump = ((coyoteTimer > 0f) && rb.velocity.Y < 0.1f) || rb.grounded;
            if (jumpBufferTimer > 0f && canJump && !flying)
            {
                rb.velocity.Y = 0;
                rb.AddImpulse(Vector3d.UnitY * jumpForce);
                if (moveDir != Vector3d.Zero && sprinting) rb.AddImpulse(moveDir * 2);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }
        }

        //allows to toggle flying and noclip
        void FlyingInput(KeyboardState kb, PhysicsObj rb)
        {
            if (kb.IsKeyPressed(Keys.D7)) flying = !flying;
            if (kb.IsKeyPressed(Keys.D6)) rb.noClip = !rb.noClip;
        }

        //crouches and uncrouches the player based on shift key
        void CrouchInput(KeyboardState kb, PhysicsObj rb, ChunkManager world)
        {
            if (orientation == null) return;
            if (kb.IsKeyDown(Keys.LeftShift) && !crouching && !flying) Crouch(rb, orientation);        
            else if (!kb.IsKeyDown(Keys.LeftShift) && crouching || flying) TryUncrouch(rb, world, orientation);           
        }

        //changes aabb size of player
        void Crouch(PhysicsObj rb, Transform orientation)
        {
            rb.boundsMin = new(-playerWidth / 2, -playerHeight / 2, -playerWidth / 2);
            rb.boundsMax = new(playerWidth / 2, playerCrouchHeight / 2, playerWidth / 2);

            crouching = true;
            rb.sneaking = true;

            orientation.localPosition = new Vector3d(0, 0.4, 0);
            rb.headOffset = new Vector3d(0, 0.4, 0);        
        }

        //tries to unchange aabb size if room available
        void TryUncrouch(PhysicsObj rb, ChunkManager world, Transform orientation)
        {
            AABB newBox = new()
            {min = new(-playerWidth / 2, -playerHeight / 2, -playerWidth / 2),
            max = new(playerWidth / 2, playerHeight / 2, playerWidth / 2)};

            newBox.min += rb.position; 
            newBox.max += rb.position;

            if (PhysicsHelpers.BoxCollidesWorld(world, newBox)) return;

            rb.boundsMin = new(-playerWidth / 2, -playerHeight / 2, -playerWidth / 2);
            rb.boundsMax = new(playerWidth / 2, playerHeight / 2, playerWidth / 2);

            crouching = false;
            rb.sneaking = false;

            orientation.localPosition = orientationOrigin;
            rb.headOffset = new Vector3d(0, RenderingConstants.CAM_HEIGHT_OFFSET, 0);   
        }
    }
}