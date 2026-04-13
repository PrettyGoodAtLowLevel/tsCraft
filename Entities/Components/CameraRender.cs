using OpenTK.Mathematics;
using OurCraft.Utility;
using static OurCraft.Graphics.FrustumCulling;

namespace OurCraft.Entities.Components
{
    //handles matrix transformations in the shader
    public class CameraRender : Component
    {
        public int width = RenderingConstants.SCREEN_WIDTH;
        public int height = RenderingConstants.SCREEN_HEIGHT;
      
        public int FOV = RenderingConstants.DEFAULT_FOV;
        readonly float nearPlane = RenderingConstants.DEFAULT_NEAR_PLANE;
        readonly float farPlane = RenderingConstants.DEFAULT_FAR_PLANE;

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

        public FrustumPlane[] GetFrustum()
        {
            return ExtractFrustumPlanes(cameraMatrix);
        }

        //updates the camera matrix with a view and perspective function
        public void UpdateMatrix()
        {
            var view = Matrix4.LookAt(Vector3.Zero, Transform.Forward, Vector3.UnitY);           
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), (float)width / height, nearPlane, farPlane); 
            cameraMatrix = view * projection;
        }

        //updates the uniform matrix value in the shader
        public void SendToShader(Shader shader, string uniformName)
        {
            shader.Activate();
            shader.SetMatrix4(uniformName, ref cameraMatrix);
        }
    }
}
