using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Physics;
using OurCraft.Utility;
using OurCraft.World;

namespace OurCraft.Entities.Components
{
    //does a bobbing effect when player is moving
    public class ViewBobbing : Component
    {
        public float intensity = 0.0035f;
        public float smooth = 10.0f;
        public float effectSpeed = 10.0f;
        
        public PhysicsObj? rb;
        public Vector3d originalOffset = Vector3d.Zero;

        internal override void Register()
        {
            BaseSystem<ViewBobbing>.Register(this);            
        }

        internal override void Unregister()
        {
            BaseSystem<ViewBobbing>.Unregister(this);
        }
        
        //save original position of game object
        public override void OnStart()
        {
            originalOffset = Transform.localPosition;
            rb = EntityManager.GetEntity("Player")?.GetComponent<PhysicsObj>();
        }   

        //update head bobbing motion
        public override void OnUpdate(ChunkManager world, KeyboardState kb, MouseState ms)
        {

            if (rb == null) return;

            CheckViewBobTrigger(rb, Time.DeltaTime);
            StopViewBob(Time.DeltaTime);
        }   

        //check if player grounded and moving
        public void CheckViewBobTrigger(PhysicsObj rb, double time)
        {
            Vector2d inputMag = rb.velocity.Xz;
            if (inputMag.LengthFast > 0.1 && rb.grounded) StartViewBob(time);
        }

        //make view model follow si
        public Vector3d StartViewBob(double dt)
        {
            Vector3d pos = Vector3d.Zero;

            pos.Y += VoxelMath.Lerp(pos.Y, Math.Sin(Time.TotalTime * effectSpeed) * intensity * 1.4f, smooth * dt);
            pos.X += VoxelMath.Lerp(pos.X, Math.Cos(Time.TotalTime * effectSpeed / 2f) * intensity * 1.6f, smooth * dt);
            Transform.localPosition += pos;
            return pos;
        }

        //return view model to original position
        public void StopViewBob(double dt)
        {
            if (Transform.localPosition == originalOffset) return;
            Transform.localPosition = Vector3d.Lerp(Transform.localPosition, originalOffset, 1 * dt);
        }
    }
}
