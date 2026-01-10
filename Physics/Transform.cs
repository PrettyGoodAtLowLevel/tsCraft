using OpenTK.Mathematics;

namespace OurCraft.Physics
{
    //represents position, orientation, and scale in the world
    public class Transform
    {
        //vec3d for precise positional rendering
        public Vector3d position = Vector3d.Zero;

        //quat allows for complex rotations without gimbal lock
        public Quaternion rotation = Quaternion.Identity;

        //regular vec3 is all that is needed for scale
        public Vector3 scale = Vector3.One;

        //transform the rotation of the entity by the global forward vector
        public Vector3 Forward =>
        Vector3.Normalize(Vector3.Transform(-Vector3.UnitZ, rotation));

        //returns the cross product of the forward with global y
        public Vector3 Right =>
        Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));

        //returns the cross product of the right and forward vectors
        public Vector3 Up =>
        Vector3.Cross(Right, Forward);
    }
}
