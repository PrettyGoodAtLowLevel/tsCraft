using OurCraft.Entities.Internal;
using OurCraft.Graphics.EntityRendering.ModelLoading;

namespace OurCraft.Entities.Components.Rendering
{
    public class EntityRender : Component
    {
        public EntityModel model = new();

        internal override void Register()
        {
            BaseSystem<EntityRender>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<EntityRender>.Unregister(this);
        }

        public void LoadModel(string modelPath, string texturePath)
        {
            EntityModel? res = BlockBenchModelLoader.Load(modelPath, texturePath);
            if (res != null)
            {
                model = res;
                model.root.parent = Transform;
            }
        }
    }
}
