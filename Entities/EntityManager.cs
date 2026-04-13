using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Entities.Components;
using OurCraft.Physics;
using OurCraft.Utility;
using OurCraft.World;

namespace OurCraft.Entities
{
    //provides helpers for managing entities & updating them efficiently
    public class EntityManager
    {
        static readonly double fixedTimestep = PhysicsConstants.PHYSICS_TICK;
        static readonly double maxAccum = PhysicsConstants.MAX_ACCUM;
        public static double TimeScale = PhysicsConstants.DEFAULT_TIME_SCALE;

        static double accum = 0.0;
        public static double Alpha { get; private set; } = 0.0;

        //entity tracking
        readonly static Dictionary<string, Entity> Entities = [];
        public static string PlayerEntityName { get; private set; } = "Player";
        public static int EntityCount => Entities.Count;

        public static Entity? GetEntity(string name)
        {
            if (Entities.TryGetValue(name, out Entity? value)) return value;
            return null;
        }

        public static Entity AddEntity(string name)
        {
            if (!Entities.TryGetValue(name, out Entity? entity))
            {
                entity = new Entity();
                Entities.Add(name, entity);
            }
            else
            {
                Console.WriteLine($"Entity '{name}' already exists, returning existing instance.");
            }

            return entity;
        }

        public static void RemoveEntity(string name)
        {
            if (name.Equals(PlayerEntityName))
            {
                Console.WriteLine($"Cannot remove entity '{name}' since that is the current Player Entity");
                return;
            }

            if (Entities.TryGetValue(name, out Entity? value))
            {
                value.Destroy();
                Entities.Remove(name);
            }           
        }

        public static void SetPlayerEntity(string name)
        {
            PlayerEntityName = name;
        }                 

        //creates the player entity
        public static void Init()
        {
            Entity player = AddEntity("Player");
            player.Transform.position = new Vector3d(0.5, 160, 0.5);

            PlayerController c = player.AddComponent<PlayerController>();
            RigidBody rb = player.AddComponent<RigidBody>();
            player.AddComponent<CameraRender>();
            player.AddComponent<PlayerInteractions>();
            player.AddComponent<DayNightCycle>();

            rb.bounds = new Vector3d(0.6, 0.8, 0.6);
            c.rb = rb;
            rb.useGravity = false;
            rb.dragY = 3.0;
            rb.dragX = 3.0;
            rb.dragZ = 3.0;

            SetPlayerEntity("Player");
        }

        public static void Update(ChunkManager world, double dt, KeyboardState kb, MouseState ms)
        {
            double scaledTime = dt * TimeScale;
            PlayerControllerSystem.Update(world, scaledTime, kb, ms);
            PlayerInteractionSystem.Update(world, scaledTime, kb, ms);
            ConstantMovementSystem.Update(world, scaledTime, kb, ms);
            PhysicsSystem.Update(world, scaledTime, kb, ms);
            DayNightCycleSystem.Update(world, scaledTime, kb, ms);
        }

        //update physics every fixed timestep using accumlation
        public static void FixedUpdate(ChunkManager world, double dt)
        {
            double scaledTime = dt * TimeScale;
            accum += scaledTime;
            if (accum > maxAccum) accum = maxAccum;

            while (accum >= fixedTimestep)
            {
                PlayerControllerSystem.FixedUpdate(world);
                PhysicsSystem.StepPhysics(world);
                accum -= fixedTimestep;
            }
            Alpha = accum / fixedTimestep;
        }
    }  

    public class DebugRenderSystem : BaseSystem<DebugRenderBox>
    {
        public static List<DebugRenderBox> AllRenderBoxes => Components;
    }

    public class CameraRenderSystem : BaseSystem<CameraRender>
    {
        public static CameraRender? Current { get; private set; }
        public static void SetActive(CameraRender camRender) => Current = camRender;
    }

    public class PlayerControllerSystem : BaseSystem<PlayerController> { }

    public class PlayerInteractionSystem : BaseSystem<PlayerInteractions> { }

    public class ConstantMovementSystem : BaseSystem<ConstantMovement> { }

    public class DayNightCycleSystem : BaseSystem<DayNightCycle> { }
}