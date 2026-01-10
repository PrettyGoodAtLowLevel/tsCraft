using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.World;

namespace OurCraft.Entities
{
    //allows to efficiently update components without needing to jump around in memory much
    public class BaseSystem<T> where T : Component
    {
        protected static List<T> Components = [];

        public static void Register(T component)
        {
            component.OnCreation();
            Components.Add(component);
        }

        public static void Unregister(T component)
        {
            component.OnDestroy();
            Components.Remove(component);
        }

        public static void Start()
        {
            foreach (T component in Components)
            {
                component.OnStart();
            }
        }

        public static void Update(ChunkManager world, double deltaTime, KeyboardState kb, MouseState ms)
        {
            foreach (T component in Components)
            {
                component.OnUpdate(world, deltaTime, kb, ms);
            }
        }

        public static void Clear()
        {
            foreach (var component in Components)
                component.OnDestroy();

            Components.Clear();
        }
    }
}
