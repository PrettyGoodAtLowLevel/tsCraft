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

        readonly static Dictionary<string, Entity> Entities = [];
        public static string PlayerEntityName { get; private set; } = "Player";
        public static int EntityCount => Entities.Count;

        //tries to get an entity, gives null if not found
        public static Entity? GetEntity(string name)
        {
            if (Entities.TryGetValue(name, out Entity? value)) return value;
            return null;
        }

        //adds an entity if non existent and returns it
        //if entity already exists, returns existing entity
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

        //removes entity if not player entity
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

        //sets the player entity
        public static void SetPlayerEntity(string name)
        {
            PlayerEntityName = name;
        }                 

        //creates the player entity
        public static void InitPlayer()
        {
            Entity player = AddEntity("Player");
            player.Transform.position = new Vector3d(0.5, 400, 0.5);

            PlayerController c = player.AddComponent<PlayerController>();
            DebugRenderBox render = player.AddComponent<DebugRenderBox>();
            player.AddComponent<PhysicsObj>();           
            player.AddComponent<CameraRender>();
            player.AddComponent<PlayerInteractions>();
            player.AddComponent<DayNightCycle>();
          
            c.OnStart();
            render.min = new Vector3(-0.3f, -0.9f, -0.3f);
            render.max = new Vector3(0.3f, 0.9f, 0.3f);
            render.SetUpRenderBox(Vector3.Zero);

            SetPlayerEntity("Player");
        }

        //updates all systems
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

    //list of different systems
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