using OurCraft.Graphics.DebugRendering;
using OpenTK.Mathematics;

namespace OurCraft.Entities.Components
{
    //represents a renderable debug aabb mesh component
    public class DebugRenderBox : Component
    {
        public Vector3 min = new(-0.4f, -0.9f, -0.4f);
        public Vector3 max = new(0.4f, 0.9f, 0.4f);
        public DebugBoxMesh mesh = new();

        internal override void Register()
        {
            BaseSystem<DebugRenderBox>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<DebugRenderBox>.Unregister(this);
        }

        public void SetUpRenderBox(Vector3 color)
        {
            mesh.SetUpMesh(max, min, color);
        }
    }
}
