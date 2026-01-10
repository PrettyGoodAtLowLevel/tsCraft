using OurCraft.Physics;

namespace OurCraft.Entities
{
    //entities are game objects that can contain refrences to different components
    //all entities contain is a list of components and a transform
    public class Entity
    {       
        protected List<Component> Components { get; set; } = []; //the refrence of the entity and its data 
        public Transform Transform { get; set; } //every entity exists in 3d space and has a transform

        public Entity() { Transform = new(); }

        //attempts to add a component to the entity
        public T AddComponent<T>() where T : Component, new()
        {
            //check if component exists
            foreach (var comp in Components)
                if (comp is T t)
                {
                    Console.WriteLine($"Component '{typeof(T)}' already exists, Returning current instance");
                    return t;
                }

            //create component internally and assign it its entity refrence
            T component = new()
            {
                GameObject = this,
                Transform = this.Transform
            };

            //tell the base system that uses these types of components to update
            Components.Add(component);
            component.Register();
            
            //give back the component
            return component;
        }

        //attempts to retrieve a component from the entity
        public T? GetComponent<T>() where T : Component
        {
            foreach (var component in Components)
                if (component is T t) return t;

            return null;
        }

        //attempts to remove the instance of a component from the entity
        public void RemoveComponent<T>() where T : Component
        {
            //removes the first case of the component in the components list
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is T t)
                {
                    Components[i].Unregister();
                    Components.RemoveAt(i);
                    return;
                }
            }
        }

        //unregisters all components
        public void Destroy()
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                Components[i].Unregister();
            }

            Components.Clear();
        }
    }
}
