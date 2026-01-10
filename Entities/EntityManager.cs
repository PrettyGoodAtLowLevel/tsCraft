using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Entities.Components;
using OurCraft.World;

namespace OurCraft.Entities
{
    //provides helpers for managing entities & updating them efficiently
    public class EntityManager
    {
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
            player.AddComponent<CameraController>();
            player.AddComponent<CameraRender>();
            player.AddComponent<PlayerInteractions>();
            SetPlayerEntity("Player");
        }

        public static void Update(ChunkManager world, double time, KeyboardState kb, MouseState ms)
        {
            CameraControllerSystem.Update(world, time, kb, ms);
            PlayerInteractionSystem.Update(world, time, kb, ms);
            ConstantMovementSystem.Update(world, time, kb, ms);
        }
    }

    class DebugRenderSystem : BaseSystem<DebugRenderBox>
    {
        public static List<DebugRenderBox> AllRenderBoxes => Components;
    }

    class CameraRenderSystem : BaseSystem<CameraRender>
    {
        public static CameraRender? Current { get; private set; }
        public static void SetActive(CameraRender camRender) => Current = camRender;
    }

    class CameraControllerSystem : BaseSystem<CameraController> { }

    class PlayerInteractionSystem : BaseSystem<PlayerInteractions> { }

    class ConstantMovementSystem : BaseSystem<ConstantMovement> { }
}