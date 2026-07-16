using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Graphics.OpenGL_Objects;
using OurCraft.Physics.PhysicsData;

namespace OurCraft.Graphics.EntityRendering.ModelLoading
{
    //contains entity rendering data imported from block bench
    public class EntityModel
    {
        public Transform root = new();
        public List<EntityMeshTransform> meshes = [];
        public List<Transform> bones = [];
        public Texture texture = new();

        public void Draw(Shader shader, Vector3d camPos)
        {
            int location = GL.GetUniformLocation(shader.ID, "tex0");
            Bindless.UniformHandleui64(location, texture.Handle);

            foreach (var mesh in meshes) mesh.part.Draw(shader, mesh.transform, camPos);
        }
    }
}