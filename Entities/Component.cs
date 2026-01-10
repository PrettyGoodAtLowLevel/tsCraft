using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Physics;
using OurCraft.World;
#pragma warning disable

namespace OurCraft.Entities
{
    //components serve as logic + data for entities to hold
    public abstract class Component
    {       
        public Entity GameObject { get; init; } //pointer back to the current entity owning this component
        public Transform Transform { get; init; } //pointer to the current entity transform

        //must implement these for proper memory management once entities are deleted
        internal abstract void Register();
        internal abstract void Unregister();

        //On Creation is called when a component is first initialized, (usually at the same time as register)
        public virtual void OnCreation() { }

        //On Start is called if the component exists on scene load
        public virtual void OnStart() { }

        //On Update is called once per frame
        public virtual void OnUpdate(ChunkManager world, double time, KeyboardState kb, MouseState ms) { }

        //On Destroy is called once the component is removed (usually at the same time as unregister)
        public virtual void OnDestroy() { }
    }
}
