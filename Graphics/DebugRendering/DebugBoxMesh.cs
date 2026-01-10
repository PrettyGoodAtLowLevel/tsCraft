using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Physics;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics.DebugRendering
{
    //quick debug box renderer
    public class DebugBoxMesh
    {
        public EBO ebo = new();
        public DebugVBO vbo = new();
        public DebugVAO vao = new();
        int indexCount = 0;

        //uploads an aabb to openGL
        public void SetUpMesh(Vector3 to, Vector3 from, Vector3 color)
        {
            List<DebugVertex> vertices = SetUpVertices(to, from, color);
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

            int stride = DebugVertex.GetSize();
            IntPtr posOffset = Marshal.OffsetOf<DebugVertex>(nameof(DebugVertex.position));
            IntPtr colOffset = Marshal.OffsetOf<DebugVertex>(nameof(DebugVertex.color));

            vao.LinkAttrib(vbo, 0, 3, VertexAttribPointerType.Float, false, stride, posOffset);
            vao.LinkAttrib(vbo, 1, 3, VertexAttribPointerType.Float, false, stride, colOffset);

            vao.Unbind();
            vbo.Unbind();
            ebo.Unbind();

            indexCount = indices.Count;
        }

        //build the indices for the aabb render
        public static List<uint> SetupIndices(int vertexCount)
        {
            int quadCount = vertexCount / 4; List<uint> indices = new(quadCount * 8);
            for (int i = 0; i < quadCount; i++)
            {
                int vi = i * 4; 
                //edges: 0-1, 1-2, 2-3, 3-0
                indices.Add((uint)(vi + 0)); 
                indices.Add((uint)(vi + 1));
                indices.Add((uint)(vi + 1)); 
                indices.Add((uint)(vi + 2)); 
                indices.Add((uint)(vi + 2)); 
                indices.Add((uint)(vi + 3)); 
                indices.Add((uint)(vi + 3)); 
                indices.Add((uint)(vi + 0)); 
            } 
            return indices;
        }

        //sets up the vertices for the aabb render
        public static List<DebugVertex> SetUpVertices(Vector3 to, Vector3 from, Vector3 color)
        {
            List<DebugVertex> vertices =
            [
                //bottom
                new DebugVertex(new Vector3(from.X, from.Y, from.Z), color),
                new DebugVertex(new Vector3(to.X, from.Y, from.Z), color),
                new DebugVertex(new Vector3(to.X, from.Y, to.Z), color),
                new DebugVertex(new Vector3(from.X, from.Y, to.Z), color),
                //top
                new DebugVertex(new Vector3(from.X, to.Y, to.Z), color),
                new DebugVertex(new Vector3(to.X, to.Y, to.Z), color),
                new DebugVertex(new Vector3(to.X, to.Y, from.Z), color),
                new DebugVertex(new Vector3(from.X, to.Y, from.Z), color),
                //front
                new DebugVertex(new Vector3(from.X, from.Y, to.Z), color),
                new DebugVertex(new Vector3(to.X, from.Y, to.Z), color),
                new DebugVertex(new Vector3(to.X, to.Y, to.Z), color),
                new DebugVertex(new Vector3(from.X, to.Y, to.Z), color),
                //back
                new DebugVertex(new Vector3(to.X, from.Y, from.Z), color),
                new DebugVertex(new Vector3(from.X, from.Y, from.Z), color),
                new DebugVertex(new Vector3(from.X, to.Y, from.Z), color),
                new DebugVertex(new Vector3(to.X, to.Y, from.Z), color),
                //right
                new DebugVertex(new Vector3(to.X, from.Y, to.Z), color),
                new DebugVertex(new Vector3(to.X, from.Y, from.Z), color),
                new DebugVertex(new Vector3(to.X, to.Y, from.Z), color),
                new DebugVertex(new Vector3(to.X, to.Y, to.Z), color),
                //left
                new DebugVertex(new Vector3(from.X, from.Y, from.Z), color),
                new DebugVertex(new Vector3(from.X, from.Y, to.Z), color),
                new DebugVertex(new Vector3(from.X, to.Y, to.Z), color),
                new DebugVertex(new Vector3(from.X, to.Y, from.Z), color),
            ]; 
            
            return vertices;
        }

        //draw, shader must be active
        public void Draw(Shader shader, Transform transform, Vector3d camPos)
        {
            //create & set uniform model matrix for vshader
            Matrix4 model =
            Matrix4.CreateScale(transform.scale) * //scale obj in local space
            Matrix4.CreateFromQuaternion(transform.rotation) * //rotate obj in local space
            Matrix4.CreateTranslation((Vector3)(transform.position - camPos)); //move to global space relative to camera

            shader.SetMatrix4("model", ref model);

            vao.Bind();
            GL.DrawElements(PrimitiveType.Lines, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            vao.Unbind();
        }
    }
}
