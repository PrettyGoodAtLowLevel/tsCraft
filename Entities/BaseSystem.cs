using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.World;

namespace OurCraft.Entities
{
    //allows to efficiently update components without needing to jump around in memory much
    public class BaseSystem<T> where T : Component
    {
        protected static List<T> Components = [];

        //adds component
        public static void Register(T component)
        {
            component.OnCreation();
            Components.Add(component);
        }

        //removes component
        public static void Unregister(T component)
        {
            component.OnDestroy();
            Components.Remove(component);
        }

        //initialize all components
        public static void Start()
        {
            foreach (T component in Components)
            {
                component.OnStart();
            }
        }

        //update all components
        public static void Update(ChunkManager world, KeyboardState kb, MouseState ms)
        {
            foreach (T component in Components)
            {
                component.OnUpdate(world, kb, ms);
            }
        }

        //update all components each physics frame
        public static void FixedUpdate(ChunkManager world)
        {
            foreach (T component in Components)
            {
                component.OnFixedUpdate(world);
            }
        }

        //remove all components
        public static void Clear()
        {
            foreach (var component in Components)
                component.OnDestroy();

            Components.Clear();
        }
    }
}
