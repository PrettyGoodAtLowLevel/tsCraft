using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Utility;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics
{
    //base vertex settings for blocks
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlockVertex
    {
        //assumes in local chunk coordinates
        public BlockVertex(Vector3 pos, Vector2 texUV, ushort lighting = 0, byte normal = 0, byte ao = 0)
        {
            x = EncodeToShort(pos.X);
            z = EncodeToShort(pos.Z);
            y = pos.Y;
            
            this.texUV = new Vector2h(texUV);
            this.normal = normal;

            this.lighting = lighting; 
            this.ao = ao;
        }

        //byte location = 0
        public short x; //since only 0-31 coords, short works fine
        public float y; //since 0-383 coords, float would be better
        public short z;

        //byte location = 8
        public Vector2h texUV; //since only 0-1 coords, half precision float works fine

        //byte location = 12
        public ushort lighting = 0;

        //byte location = 14
        public byte normal = 0;

        //byte location = 15
        public byte ao = 0;

        //always updates when adding new vertex properties
        public static int GetSize()
        {
            return Marshal.SizeOf<BlockVertex>();
        }

        //converts float values from 0-chunksize to shorts, which are then converted again in the shader
        static short EncodeToShort(float value, float chunkSize = WorldConstants.CHUNK_WIDTH)
        {
            //normalize [0, chunkSize) -> [0, 1)
            float normalized = value / chunkSize;
            //scale to short range
            return (short)(normalized * short.MaxValue);
        }
    }

    //holds vertex data for openGL
    public class BlockVBO
    {
        public int ID { get; private set; }

        public BlockVBO() { ID = 0; }

        //uploads vertex data
        public void Create()
        {
            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        }

        public void Delete()
        {
            if (ID != 0)
            {
                GL.DeleteBuffer(ID);
                ID = 0;
            }
        }

        public void BufferData(BlockVertex[] data)
        {
            int sizeInBytes = Marshal.SizeOf<BlockVertex>() * data.Length;

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeInBytes, data, BufferUsageHint.StaticDraw);
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public override string ToString()
        {
            return $"ID: {ID}";
        }
    }
}