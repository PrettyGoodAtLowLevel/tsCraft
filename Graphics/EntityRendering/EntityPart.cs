using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Physics;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics.EntityRendering
{
    //simple mesh of a body part of an entity
    public class EntityPart
    {
        public EBO ebo = new();
        public EntityVBO vbo = new();
        public EntityVAO vao = new();
        int indexCount = 0;

        //creates vertices and indices based on face uvs, then upload mesh part to OGL
        public void SetUpMesh(Vector3 to, Vector3 from, Vector2[,] faceUvs)
        {
            List<Vertex> vertices = SetUpVertices(to, from, faceUvs);
            List<uint> indices = SetupIndices(vertices.Count);

            vao.Delete();
            vbo.Delete();
            ebo.Delete();

            vao.Create();
            vao.Bind();

            vbo.Create();
            vbo.BufferData(vertices.ToArray());
            ebo.Create();
            ebo.BufferData(indices.ToArray());

            int stride = Vertex.GetSize();
            IntPtr posOffset = Marshal.OffsetOf<Vertex>(nameof(Vertex.position));
            IntPtr colOffset = Marshal.OffsetOf<Vertex>(nameof(Vertex.uv));

            vao.LinkAttrib(vbo, 0, 3, VertexAttribPointerType.Float, false, stride, posOffset);
            vao.LinkAttrib(vbo, 1, 2, VertexAttribPointerType.Float, false, stride, colOffset);

            EntityVAO.Unbind();
            vbo.Unbind();
            ebo.Unbind();

            indexCount = indices.Count;
        }

        //creates indices based on vertex count
        public static List<uint> SetupIndices(int vertexCount)
        {
            int quadCount = vertexCount / 4;
            List<uint> indices = new(quadCount * 6);

            for (int i = 0; i < quadCount; i++)
            {
                int vi = i * 4;

                //triangle 1
                indices.Add((uint)(vi + 0));
                indices.Add((uint)(vi + 1));
                indices.Add((uint)(vi + 2));

                //triangle 2
                indices.Add((uint)(vi + 0));
                indices.Add((uint)(vi + 2));
                indices.Add((uint)(vi + 3));
            }

            return indices;
        }

        //builds cube vertices from min to max, then include a list of face uvs
        public static List<Vertex> SetUpVertices(Vector3 to, Vector3 from, Vector2[,] faceUVs)
        {
            List<Vertex> vertices =
            [
                //bottom
                new Vertex(new Vector3(from.X, from.Y, from.Z), faceUVs[0,0]),
                new Vertex(new Vector3(to.X,   from.Y, from.Z), faceUVs[0,1]),
                new Vertex(new Vector3(to.X,   from.Y, to.Z),   faceUVs[0,2]),
                new Vertex(new Vector3(from.X, from.Y, to.Z),   faceUVs[0,3]),

                //top
                new Vertex(new Vector3(from.X, to.Y, to.Z),     faceUVs[1,0]),
                new Vertex(new Vector3(to.X,   to.Y, to.Z),     faceUVs[1,1]),
                new Vertex(new Vector3(to.X,   to.Y, from.Z),   faceUVs[1,2]),
                new Vertex(new Vector3(from.X, to.Y, from.Z),   faceUVs[1,3]),

                //front
                new Vertex(new Vector3(from.X, from.Y, to.Z),   faceUVs[2,0]),
                new Vertex(new Vector3(to.X,   from.Y, to.Z),   faceUVs[2,1]),
                new Vertex(new Vector3(to.X,   to.Y, to.Z),     faceUVs[2,2]),
                new Vertex(new Vector3(from.X, to.Y, to.Z),     faceUVs[2,3]),

                //back
                new Vertex(new Vector3(to.X,   from.Y, from.Z), faceUVs[3,0]),
                new Vertex(new Vector3(from.X, from.Y, from.Z), faceUVs[3,1]),
                new Vertex(new Vector3(from.X, to.Y, from.Z),   faceUVs[3,2]),
                new Vertex(new Vector3(to.X,   to.Y, from.Z),   faceUVs[3,3]),

                //right
                new Vertex(new Vector3(to.X, from.Y, to.Z),     faceUVs[4,0]),
                new Vertex(new Vector3(to.X, from.Y, from.Z),   faceUVs[4,1]),
                new Vertex(new Vector3(to.X, to.Y, from.Z),     faceUVs[4,2]),
                new Vertex(new Vector3(to.X, to.Y, to.Z),       faceUVs[4,3]),

                //left
                new Vertex(new Vector3(from.X, from.Y, from.Z), faceUVs[5,0]),
                new Vertex(new Vector3(from.X, from.Y, to.Z),   faceUVs[5,1]),
                new Vertex(new Vector3(from.X, to.Y, to.Z),     faceUVs[5,2]),
                new Vertex(new Vector3(from.X, to.Y, from.Z),   faceUVs[5,3]),
            ];

            return vertices;
        }

        //draw, shader must be active
        public void Draw(Shader shader, Transform transform, Vector3d camPos)
        {
            //create & set uniform model matrix for vshader
            Matrix4 scale = Matrix4.CreateScale(transform.WorldScale);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(transform.WorldRotation);
            Matrix4 position = Matrix4.CreateTranslation((Vector3)(transform.WorldPosition - camPos));
            Matrix4 model = scale * rotation * position;

            shader.SetMatrix4("model", ref model);

            vao.Bind();
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            EntityVAO.Unbind();
        }
    }
}