using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics
{ 

    //holds combined chunk mesh data in one draw call for optmization
    public class BatchedChunkMesh
    {
        //gl objects
        private readonly BlockVAO vao;
        private readonly BlockVBO vbo;
        private readonly EBO ebo;

        //mesh stats
        private List<uint> indices = new List<uint>();
        private uint indexCount = 0;
        private uint vertexCount = 0;

        //initialize everything
        public BatchedChunkMesh()
        {
            vao = new BlockVAO();
            vbo = new BlockVBO();
            ebo = new EBO();
        }

        ~BatchedChunkMesh()
        {
            vertexCount = 0;
            indexCount = 0;
        }

        //clear gl objects
        public void Delete()
        {
            indexCount = 0;
            vertexCount = 0;
            vao.Delete();
            vbo.Delete();
            ebo.Delete();
        }

        //build the indices for the chunk
        public void SetupIndices(int vertexCount)
        {
            indices.Clear();
            int quadCount = vertexCount / 4;
            int indexCount = quadCount * 6;
            indices = new List<uint>(indexCount);
            for (int i = 0; i < quadCount; i++)
            {
                int vi = i * 4;

                indices.Add((uint)(vi + 0));
                indices.Add((uint)(vi + 1));
                indices.Add((uint)(vi + 2));
                indices.Add((uint)(vi + 2));
                indices.Add((uint)(vi + 3));
                indices.Add((uint)(vi + 0));
            }
        }
        
        //send mesh data to gpu
        public void SetupMesh(List<BlockVertex> vertices)
        {            
            if (vertices.Count == 0) return;
            if (indices.Count == 0) return;

            vao.Delete();
            vbo.Delete();
            ebo.Delete();

            vao.Create();
            vao.Bind();

            int vertCount = vertices.Count;

            //upload vertices
            vbo.Create();
            vbo.BufferData(vertices.ToArray());
            ebo.Create();
            ebo.BufferData(indices.ToArray());

            //link vertex attributes
            int stride = BlockVertex.GetSize();
            IntPtr xPosOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.x));
            IntPtr yPosOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.y));
            IntPtr zPosOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.z));
            IntPtr uvOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.texUV));
            IntPtr lightingOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.lighting));
            IntPtr aoOffset = Marshal.OffsetOf <BlockVertex>(nameof(BlockVertex.ao));
            IntPtr flagsOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.flags));
            IntPtr texIDOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.texID));

            vao.LinkAttrib(vbo, 0, 1, VertexAttribPointerType.UnsignedShort, false, stride, xPosOffset);
            vao.LinkAttrib(vbo, 1, 1, VertexAttribPointerType.UnsignedShort, false, stride, yPosOffset);
            vao.LinkAttrib(vbo, 2, 1, VertexAttribPointerType.UnsignedShort, false, stride, zPosOffset);
            vao.LinkAttrib(vbo, 3, 2, VertexAttribPointerType.HalfFloat, false, stride, uvOffset);
            vao.LinkAttribInt(vbo, 4, 1, VertexAttribIntegerType.UnsignedShort, stride, lightingOffset); 
            vao.LinkAttribInt(vbo, 5, 1, VertexAttribIntegerType.UnsignedByte, stride, aoOffset);
            vao.LinkAttribInt(vbo, 6, 1, VertexAttribIntegerType.UnsignedByte, stride, flagsOffset);
            vao.LinkAttribInt(vbo, 7, 1, VertexAttribIntegerType.UnsignedShort, stride, texIDOffset);

            vao.Unbind();
            vbo.Unbind();
            ebo.Unbind();

            indexCount = (uint)indices.Count;
            indices.Clear();
            indices.Capacity = 0;
            vertexCount = (uint)vertCount;
        }     

        //get if has mesh
        public bool HasMesh()
        {
            return vertexCount > 0;
        }

        //draw with shader, shader must be active once drawing
        public void Draw(Shader shader, Vector3d chunkWorldPos, Vector3d camPos)
        {
            if (!HasMesh()) return;

            //set uniform model matrix for vshader
            Vector3 relPos = (Vector3)(chunkWorldPos - camPos);
            Matrix4 model = Matrix4.CreateTranslation(relPos);
            shader.SetMatrix4("model", ref model);
            shader.SetVector2("uChunkWorldPos", new Vector2((float)chunkWorldPos.X, (float)chunkWorldPos.Z));

            //bind vertex data and textures, then draw
            vao.Bind();
            GL.DrawElements(PrimitiveType.Triangles, (int)indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            vao.Unbind();
        }
    }
}