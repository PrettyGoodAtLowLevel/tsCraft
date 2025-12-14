using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.World;
using System.Runtime.InteropServices;

namespace OurCraft
{
    //base vertex settings for blocks
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlockVertex
    {
        //assumes in local chunk coordinates
        public BlockVertex(Vector3 pos, Vector2 uv, ushort lightValue = 0, byte normal = 0)
        {
            x = EncodeToShort(pos.X);
            y = pos.Y;
            z = EncodeToShort(pos.Z);
            texUV = new Vector2h(uv);
            lighting = lightValue;
            this.normal = normal;
        }

        //byte location = 0
        public short x; //since only 0-31 coords, short works fine
        public float y; //but since 0-383 coords, float would be better, short still works but less presicion
        public short z;

        //byte location = 8
        public Vector2h texUV; //since only 0-1 coords, half precision float works fine

        //byte location = 12
        public ushort lighting = 0;

        //byte location = 15
        public byte normal = 0;

        //always updates when adding new vertex properties
        public static int GetSize()
        {
            return Marshal.SizeOf<BlockVertex>();
        }

        //converts float values from 0-chunksize to shorts, which are then converted again in the shader
        static short EncodeToShort(float value, float chunkSize = (float)SubChunk.SUBCHUNK_SIZE)
        {
            //normalize [0, chunkSize) -> [0, 1)
            float normalized = value / chunkSize;
            //scale to short range
            return (short)(normalized * short.MaxValue);
        }

        public static float DecodeFromShort(short encoded, float chunkSize = (float)SubChunk.SUBCHUNK_SIZE)
        {
            //convert short back to normalized [0, 1) range
            float normalized = encoded / (float)short.MaxValue;
            //scale back to [0, chunkSize) range
            return normalized * chunkSize;
        }
    }

    //holds vertex data for openGL
    public class VBO
    {
        public int ID { get; private set; }
        public int Capacity { get; private set; }

        public VBO() { ID = 0; }

        //uploads vertex data
        public void CreateEmpty(int sizeInBytes, BufferUsageHint usage = BufferUsageHint.DynamicDraw)
        {
            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeInBytes, IntPtr.Zero, usage);
            Capacity = sizeInBytes;
        }

        public void SubData<T>(int offsetInBytes, T[] data) where T : struct
        {
            int sizeInBytes = Marshal.SizeOf<T>() * data.Length;
            if (offsetInBytes + sizeInBytes > Capacity)
                throw new InvalidOperationException("Data upload exceeds VBO capacity!");

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, sizeInBytes, data);
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public void Delete()
        {
            if (ID != 0)
            {
                GL.DeleteBuffer(ID);
                ID = 0;
            }
        }
    }
}