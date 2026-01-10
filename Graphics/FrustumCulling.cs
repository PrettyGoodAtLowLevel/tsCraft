using OpenTK.Mathematics;

namespace OurCraft.Graphics
{
    //provides helpers for frustum culling
    public static class FrustumCulling
    {
        //for frustum culling
        public struct FrustumPlane
        {
            public Vector3 Normal;
            public float Distance;

            public float GetSignedDistanceToPoint(Vector3 point)
            {
                return Vector3.Dot(Normal, point) + Distance;
            }
        }

        //find the frustum view of the camera
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

        //checks if aabb is in camera view
        public static bool IsBoxInFrustum(FrustumPlane[] planes, Vector3 min, Vector3 max)
        {
            foreach (var plane in planes)
            {
                Vector3 positiveVertex = new Vector3(
                    plane.Normal.X >= 0 ? max.X : min.X,
                    plane.Normal.Y >= 0 ? max.Y : min.Y,
                    plane.Normal.Z >= 0 ? max.Z : min.Z);

                if (plane.GetSignedDistanceToPoint(positiveVertex) < 0)
                    return false;
            }

            return true;
        }
    }
}
