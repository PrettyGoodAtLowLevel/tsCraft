using OpenTK.Mathematics;
using OurCraft.Utility;

namespace OurCraft.Entities.Components
{
    //handles matrix transformations in the shader
    public class CameraRender : Component
    {
        public int width = RenderingConstants.SCREEN_WIDTH;
        public int height = RenderingConstants.SCREEN_HEIGHT;
      
        public int FOV = RenderingConstants.DEFAULT_FOV;
        public readonly float NearPlane = RenderingConstants.DEFAULT_NEAR_PLANE;
        public readonly float FarPlane = RenderingConstants.DEFAULT_FAR_PLANE;
        public Vector3 offset = Vector3.Zero;

        private Matrix4 cameraMatrix;

        internal override void Register()
        {
            BaseSystem<CameraRender>.Register(this);
            CameraRenderSystem.SetActive(this);
        }

        internal override void Unregister()
        {
            BaseSystem<CameraRender>.Unregister(this);
        }

        //get frustum of camera
        public Graphics.FrustumCulling.FrustumPlane[] GetFrustum()
        {
            return Graphics.FrustumCulling.ExtractFrustumPlanes(cameraMatrix);
        }

        //updates the camera matrix with a view and perspective function
        public void UpdateMatrix()
        {
            var view = Matrix4.LookAt(Vector3.Zero, Transform.Forward, Vector3.UnitY);           
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), (float)width / height, NearPlane, FarPlane); 
            cameraMatrix = view * projection;
        } 

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), (float)width / height, NearPlane, FarPlane);
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Vector3.Zero, Transform.Forward, Vector3.UnitY);
        }

        //updates the uniform matrix value in the shader
        public void SendToShader(Shader shader, string uniformName)
        {
            shader.Activate();
            shader.SetMatrix4(uniformName, ref cameraMatrix);
        }

        public void SendViewProjection(Shader shader)
        {
            shader.Activate();

            Matrix4 proj = GetProjectionMatrix();
            Matrix4 view = GetViewMatrix();

            shader.SetMatrix4("projection", ref proj);
            shader.SetMatrix4("view", ref view);
        }
    }
}
