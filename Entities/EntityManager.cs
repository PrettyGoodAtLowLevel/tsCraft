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
        static readonly double physTick = PhysicsConstants.PHYSICS_TICK;
        static readonly double maxAccum = PhysicsConstants.MAX_ACCUM;

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
            Entity camera = AddEntity("Camera");
            Entity viewModel = AddEntity("View Model");

            player.Transform.localPosition = new Vector3d(0.5, 400, 0.5);
            camera.Transform.parent = player.Transform;
            camera.Transform.localPosition += Vector3d.UnitY * RenderingConstants.CAM_HEIGHT_OFFSET;
            viewModel.Transform.parent = camera.Transform;

            PlayerController c = player.AddComponent<PlayerController>();
            PlayerInteractions i = player.AddComponent<PlayerInteractions>();
            player.AddComponent<PhysicsObj>();                       
            player.AddComponent<DayNightCycle>();

            camera.AddComponent<CameraRender>();
            viewModel.AddComponent<ViewModelSway>();
            EntityRender playerHand = viewModel.AddComponent<EntityRender>();          
            ViewBobbing viewBobbing = viewModel.AddComponent<ViewBobbing>();
                   
            i.orientation = camera.Transform;
            c.OnStart();

            playerHand.LoadModel("playerSkin.json", "Textures/Mc Skins/timeBoss.png");
            playerHand.model.root.localScale *= (Vector3.One * 0.65f);
            viewModel.Transform.localPosition = new Vector3d(0.125, -0.6, -0.115);
            viewBobbing.OnStart();

            SetPlayerEntity("Player");
        }

        //updates all systems
        public static void Update(ChunkManager world, KeyboardState kb, MouseState ms)
        {
            PlayerControllerSystem.Update(world, kb, ms);
            PlayerInteractionSystem.Update(world, kb, ms);

            ConstantMovementSystem.Update(world, kb, ms);
            PhysicsSystem.Update(world, kb, ms);

            DayNightCycleSystem.Update(world, kb, ms);

            ViewModelSwaySystem.Update(world, kb, ms);
            ViewBobbingSystem.Update(world, kb, ms);
        }

        //update physics every fixed timestep using accumlation
        public static void FixedUpdate(ChunkManager world)
        {
            accum += Time.DeltaTime;
            if (accum > maxAccum) accum = maxAccum;

            while (accum >= physTick)
            {
                PlayerControllerSystem.FixedUpdate(world);
                PhysicsSystem.StepPhysics(world);
                accum -= physTick;
            }
            Alpha = accum / physTick;
        }
    }  

    //list of different systems
    public class DebugRenderSystem : BaseSystem<DebugRenderBox>
    {
        public static List<DebugRenderBox> AllRenderBoxes => Components;
    }

    public class EntityRenderSystem : BaseSystem<EntityRender>
    {
        public static List<EntityRender> AllModels => Components;
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
    public class ViewModelSwaySystem : BaseSystem<ViewModelSway> { }
    public class ViewBobbingSystem : BaseSystem<ViewBobbing> { }
}