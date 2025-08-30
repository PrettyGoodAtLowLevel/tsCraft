using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Physics;
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

        //send mesh data to gpu
        public void SetupMesh(List<Vertex> vertices, List<uint> indices)
        {           
            if (vertices.Count == 0 || meshcount != 0) return; //dont draw if empty
            //delete gl objects if already created
            vao.Delete();
            vbo.Delete();
            ebo.Delete();

            vao.Create();
            vao.Bind();

            vbo.Create(vertices);
            ebo.Create(indices);
           
            int stride = Vertex.GetSize();
            IntPtr posOffset = Marshal.OffsetOf<Vertex>(nameof(Vertex.position));
            IntPtr uvOffset = Marshal.OffsetOf<Vertex>(nameof(Vertex.texUV));
            IntPtr normalOffset = Marshal.OffsetOf<Vertex>(nameof(Vertex.normal));
            IntPtr aoOffset = Marshal.OffsetOf<Vertex>(nameof(Vertex.ao));

            //position (vec3)
            vao.LinkAttrib(vbo, 0, 3, VertexAttribPointerType.Float, stride, posOffset);

            //texUV (vec2)
            vao.LinkAttrib(vbo, 1, 2, VertexAttribPointerType.Float, stride, uvOffset);

            //normal (unsigned byte, integer attribute)
            vao.LinkAttribInt(vbo, 2, 1, VertexAttribIntegerType.UnsignedByte, stride, normalOffset);

            //ao (unsigned byte, integer attribute)
            vao.LinkAttribInt(vbo, 3, 1, VertexAttribIntegerType.UnsignedByte, stride, aoOffset);

            vao.Unbind();
            vbo.Unbind();
            ebo.Unbind();
            indexSize = (uint)indices.Count;
            vertexCount = (uint)vertices.Count;
            meshcount = 1;     
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
            //bind vertex data and textures
            vao.Bind();

            //draw
            GL.DrawElements(PrimitiveType.Triangles, (int)indexSize, DrawElementsType.UnsignedInt, IntPtr.Zero);
            vao.Unbind();
        }
    }
}