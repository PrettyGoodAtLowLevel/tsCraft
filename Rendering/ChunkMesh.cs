using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Physics;
using OurCraft.World;
using System.Runtime.InteropServices;

namespace OurCraft.Rendering
{ 

    //holds mesh data and gl objects in one place for a chunk
    public class ChunkMesh
    {
        //---explanation---

        //acts as a regular openGL mesh, but with helpers for adding voxel data
        //only loads in block textures to optimize mesh building since this mesh is only for blocks
        //each chunk has one of these that combines all subchunk mesh data for one combined mesh

        //positioning
        private Matrix4 modelMat = Matrix4.Identity;
        public Transform transform = new();

        //gl objects
        private readonly VAO vao;
        private readonly VBO vbo;
        private readonly EBO ebo;
        public static Texture globalBlockTexture = new();

        //mesh stats
        private List<uint> indices = new List<uint>();
        private uint indexSize = 0;
        private uint vertexCount = 0;
        private int meshcount = 0;

        //initialize everything
        public ChunkMesh()
        {
            vao = new VAO();
            vbo = new VBO();
            ebo = new EBO();
        }

        //since this is a chunk/blocks, only load block textures 
        public static void LoadChunkTextures()
        {
            globalBlockTexture.Load("Textures/dingledong.png");                    
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
            if (vertices.Count == 0 || meshcount != 0) return;

            vao.Delete();
            vbo.Delete();
            ebo.Delete();

            vao.Create();
            vao.Bind();

            int vertCount = vertices.Count;

            //upload vertices
            vbo.CreateEmpty(vertCount * BlockVertex.GetSize());
            vbo.SubData(0, vertices.ToArray());
            ebo.CreateEmpty(indices.Count * sizeof(uint));
            ebo.SubData(0, indices.ToArray());

            //link vertex attributes
            int stride = BlockVertex.GetSize();
            IntPtr xPosOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.x));
            IntPtr yPosOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.y));
            IntPtr zPosOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.z));
            IntPtr uvOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.texUV));
            IntPtr normalOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.normal));
            IntPtr aoOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.ao));
            IntPtr lightingOffset = Marshal.OffsetOf<BlockVertex>(nameof(BlockVertex.lighting));

            vao.LinkAttrib(vbo, 0, 1, VertexAttribPointerType.Short, false, stride, xPosOffset);
            vao.LinkAttrib(vbo, 1, 1, VertexAttribPointerType.Float, false, stride, yPosOffset);
            vao.LinkAttrib(vbo, 2, 1, VertexAttribPointerType.Short, false, stride, zPosOffset);
            vao.LinkAttrib(vbo, 3, 2, VertexAttribPointerType.HalfFloat, false, stride, uvOffset);
            vao.LinkAttribInt(vbo, 4, 1, VertexAttribIntegerType.UnsignedByte, stride, normalOffset);
            vao.LinkAttribInt(vbo, 5, 1, VertexAttribIntegerType.UnsignedByte, stride, aoOffset);
            vao.LinkAttribInt(vbo, 6, 1, VertexAttribIntegerType.UnsignedShort, stride, lightingOffset);

            vao.Unbind();
            vbo.Unbind();
            ebo.Unbind();

            indexSize = (uint)indices.Count;
            indices.Clear();
            indices.Capacity = 0;
            vertexCount = (uint)vertCount;
            meshcount = 1;
        }
        
        public void SetDataCount(List<BlockVertex> vertices, List<uint> indices)
        {
            indexSize = (uint)indices.Count;
            vertexCount = (uint)vertices.Count;
        }

        //clear mesh and rebuild as empty
        public void ClearMesh()
        {
            indexSize = 0;
            vertexCount = 0;
            meshcount = 0;
        }

        //clear gl objects
        public void Delete()
        {
            vao.Delete();
            vbo.Delete();
            ebo.Delete();
        }

        //get if has mesh
        public bool HasMesh()
        {
            return vertexCount > 0;
        }

        //draw with shader
        public void Draw(Shader shader, Camera camera)
        {
            if (!HasMesh()) return;
            shader.Activate();

            //set uniform model matrix for vshader
            modelMat = transform.Matrix();
            GL.UniformMatrix4(GL.GetUniformLocation(shader.ID, "model"), false, ref modelMat);

            //bind vertex data and textures, then draw
            vao.Bind();
            GL.DrawElements(PrimitiveType.Triangles, (int)indexSize, DrawElementsType.UnsignedInt, IntPtr.Zero);
            vao.Unbind();
        }
    }
}