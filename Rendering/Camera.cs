using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OurCraft.Rendering
{
    //manipulates the vertex shader to simulate 3d
    public class Camera //also could be player currently
    {
        public struct FrustumPlane
        {
            public Vector3 Normal;
            public float Distance;

            public float GetSignedDistanceToPoint(Vector3 point)
            {
                return Vector3.Dot(Normal, point) + Distance;
            }
        }

        public static FrustumPlane[] ExtractFrustumPlanes(Matrix4 matrix)
        {
            FrustumPlane[] planes = new FrustumPlane[6];

            //left
            planes[0].Normal.X = matrix.M14 + matrix.M11;
            planes[0].Normal.Y = matrix.M24 + matrix.M21;
            planes[0].Normal.Z = matrix.M34 + matrix.M31;
            planes[0].Distance = matrix.M44 + matrix.M41;

            //right
            planes[1].Normal.X = matrix.M14 - matrix.M11;
            planes[1].Normal.Y = matrix.M24 - matrix.M21;
            planes[1].Normal.Z = matrix.M34 - matrix.M31;
            planes[1].Distance = matrix.M44 - matrix.M41;

            //bottom
            planes[2].Normal.X = matrix.M14 + matrix.M12;
            planes[2].Normal.Y = matrix.M24 + matrix.M22;
            planes[2].Normal.Z = matrix.M34 + matrix.M32;
            planes[2].Distance = matrix.M44 + matrix.M42;

            //top
            planes[3].Normal.X = matrix.M14 - matrix.M12;
            planes[3].Normal.Y = matrix.M24 - matrix.M22;
            planes[3].Normal.Z = matrix.M34 - matrix.M32;
            planes[3].Distance = matrix.M44 - matrix.M42;

            //near
            planes[4].Normal.X = matrix.M14 + matrix.M13;
            planes[4].Normal.Y = matrix.M24 + matrix.M23;
            planes[4].Normal.Z = matrix.M34 + matrix.M33;
            planes[4].Distance = matrix.M44 + matrix.M43;

            //far
            planes[5].Normal.X = matrix.M14 - matrix.M13;
            planes[5].Normal.Y = matrix.M24 - matrix.M23;
            planes[5].Normal.Z = matrix.M34 - matrix.M33;
            planes[5].Distance = matrix.M44 - matrix.M43;

            //normalize all planes
            for (int i = 0; i < 6; i++)
            {
                float length = planes[i].Normal.Length;
                planes[i].Normal /= length;
                planes[i].Distance /= length;
            }

            return planes;
        }

        //members
        //screen settings
        private int width;
        private int height;
        //3d space
        public Vector3 Position = Vector3.Zero;
        public Vector3 Orientation = -Vector3.UnitZ;
        private Vector3 Up = Vector3.UnitY;

        //handles transformations in the shader
        private Matrix4 cameraMatrix;

        public float Speed = 10.0f;
        private float Sensitivity = 100.0f;

        //methods
        public Camera(int width, int height, Vector3 position, float speed = 10f, float sensitivity = 100f)
        {
            this.width = width;
            this.height = height;
            Position = position;
            Speed = speed;
            Sensitivity = sensitivity;
        }

        //updates the camera matrix with a view and perspective function
        public void UpdateMatrix(float fovDeg, float nearPlane, float farPlane)
        {
            var view = Matrix4.LookAt(Position, Position + Orientation, Up);
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovDeg), (float)width / height, nearPlane, farPlane);
            cameraMatrix = view * projection;
        }

        public FrustumPlane[] GetFrustum() { return ExtractFrustumPlanes(cameraMatrix); }
        //updates the uniform matrix value in the shader
        public void SendToShader(Shader shader, string uniformName)
        {
            shader.Activate();
            int uniformLocation = GL.GetUniformLocation(shader.ID, uniformName);
            GL.UniformMatrix4(uniformLocation, false, ref cameraMatrix);
        }

        //does all the movement for the camera
        public void HandleInput(KeyboardState keyboard, MouseState mouse, GameWindow window, float deltaTime)
        {
            float velocity = Speed * deltaTime;
            Vector3 forward = Orientation;
            forward.Y = 0; //ignore vertical component for horizontal movement
            forward = Vector3.Normalize(forward);

            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Up));

            if (keyboard.IsKeyDown(Keys.W))
                Position += forward * velocity;

            if (keyboard.IsKeyDown(Keys.S))
                Position -= forward * velocity;

            if (keyboard.IsKeyDown(Keys.A))
                Position -= right * velocity;

            if (keyboard.IsKeyDown(Keys.D))
                Position += right * velocity;

            if (keyboard.IsKeyDown(Keys.Space))
                Position += Up * velocity;

            if (keyboard.IsKeyDown(Keys.LeftShift))
                Position -= Up * velocity;

            if (keyboard.IsKeyPressed(Keys.F))
                Position.Y += 100;

            if (keyboard.IsKeyDown(Keys.Escape))
                window.Close();

            Vector2 mouseDelta = mouse.Delta;

            float rotX = Sensitivity * mouseDelta.Y / height;
            float rotY = Sensitivity * mouseDelta.X / width;

            // Vertical rotation (pitch)
            var right2 = Vector3.Normalize(Vector3.Cross(Orientation, Up));
            var newOrientation = Vector3.Transform(Orientation, Quaternion.FromAxisAngle(right2, MathHelper.DegreesToRadians(-rotX)));

            float angle = MathHelper.RadiansToDegrees(MathF.Acos(Vector3.Dot(newOrientation, Up)));
            if (angle > 1f && angle < 179f)
                Orientation = newOrientation;
            if (Speed < 0)
                Speed = 0;

            // Horizontal rotation (yaw)
            Orientation = Vector3.Transform(Orientation, Quaternion.FromAxisAngle(Up, MathHelper.DegreesToRadians(-rotY)));
        }
    }
}