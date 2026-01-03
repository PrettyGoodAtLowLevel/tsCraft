using OpenTK.Mathematics;

namespace OurCraft.Physics
{
    //allows to express objects in 3d space
    public struct Transform
    {
        //members
        public Vector3 position;
        public Vector3 rotation; //uses euler angles
        public Vector3 scale;

        //methods
        public Transform()
        {
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector3.One;
        }

        //returns all of the vectors into one matrix used for the vertex shader
        public Matrix4 Matrix()
        {
            Matrix4 m = Matrix4.Identity;

            //apply linear transformations to matrix
            m *= Matrix4.CreateTranslation(position);
            m *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X));
            m *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y));
            m *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
            m *= Matrix4.CreateScale(scale);

            return m;
        }

        //move by specified amount
        public void Translate(Vector3 delta)
        {
            position += delta;
        }

        //add to rotation
        public void Rotate(Vector3 deltaDegrees)
        {
            rotation += deltaDegrees;
        }

        //set const position
        public void SetPosition(Vector3 pos)
        {
            position = pos;
        }

        //set rotation
        public void SetRotation(Vector3 rotDeg)
        {
            rotation = rotDeg;
        }

        //change size 
        public void SetScale(Vector3 scale)
        {
            this.scale = scale;
        }
    }
}