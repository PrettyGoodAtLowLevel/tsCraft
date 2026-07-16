using OpenTK.Mathematics;

namespace OurCraft.Physics.PhysicsData
{
    //represents position, orientation, and scale in the world
    public class Transform
    {
        //moves the transform
        public Transform? parent = null;

        //vec3d for precise positional rendering
        public Vector3d localPosition = Vector3d.Zero;

        //quat allows for complex rotations without gimbal lock
        public Quaternion localRotation = Quaternion.Identity;

        //regular vec3 is all that is needed for scale
        public Vector3 localScale = Vector3.One;

        //transform the rotation of the entity by the global forward vector
        public Vector3 Forward => Vector3.Normalize(Vector3.Transform(-Vector3.UnitZ, WorldRotation));

        //returns the cross product of the forward with global y
        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));

        //returns the cross product of the right and forward vectors
        public Vector3 Up => Vector3.Cross(Right, Forward);

        //combine local position relative to parents rotation + position
        public Vector3d WorldPosition
        {
            get
            {
                if (parent == null) return localPosition;

                //scale, rotate, then translate
                Vector3 scaledLocal = (Vector3)(localPosition * parent.WorldScale);
                Vector3 rotated = Vector3.Transform(scaledLocal, parent.WorldRotation);

                return parent.WorldPosition + rotated;
            }
        }

        //simply multiply the local rotation * parent rotation
        public Quaternion WorldRotation
        {
            get
            {
                if (parent == null) return localRotation;

                return parent.WorldRotation * localRotation;
            }
        }

        //simply multiply the components of the scale vectors
        public Vector3 WorldScale
        {
            get
            {
                if (parent == null) return localScale;

                return parent.WorldScale * localScale; //component-wise
            }
        }

        public Transform()
        {
            localPosition = Vector3d.Zero;
            localRotation = Quaternion.Identity;
            localScale = Vector3.One;
            parent = null;
        }

        public override string ToString()
        {
            string str = "";

            str += $"LocalPosition: {localPosition}, ";
            str += $"LocalRotation: {localRotation}, ";
            str += $"LocalScale: {localScale} \n";

            str += $"WorldPosition: {WorldPosition}, ";
            str += $"WorldRotation: {WorldRotation}, ";
            str += $"WorldScale: {WorldScale}";

            return str;
        }
    }
}