using OpenTK.Mathematics;
using OurCraft.Physics.PhysicsData;

namespace OurCraft.Graphics.EntityRendering
{
    public class EntityMeshTransform
    {
        public EntityPart part;
        public Transform transform;
        public Vector3 min;
        public Vector3 max;

        public void CreateMesh(Vector2[,] uvs)
        {
            part.SetUpMesh(max, min, uvs);
        }

        public EntityMeshTransform()
        {
            part = new();
            transform = new();
        }
    }
}
