using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Utility;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics.ChunkRendering
{
    //base vertex settings for blocks
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlockVertex
    {
        //assumes in local chunk coordinates
        public BlockVertex(Vector3 pos, Vector2 texUV, ushort lighting = 0, byte ao = 0, byte flags = 0, byte normal = 0, ushort texID = 0)
        {
            x = EncodeToShortXZ(pos.X);
            y = EncodeToShortY(pos.Y);
            z = EncodeToShortXZ(pos.Z);

            this.texUV = new Vector2h(texUV);
            this.texID = texID;
            this.lighting = lighting;

            byte packed = (byte)(normal << 2 | ao & 0x3);

            this.ao = packed;
            this.flags = flags;
        }

        //byte location = 0
        public ushort x; //since only 0-31 coords, short works fine
        public ushort y; //since 0-383 coords, float would be better
        public ushort z;

        //byte location = 6    
        public Vector2h texUV; //since only 0-1 coords, half precision float works fine

        //byte location = 10
        public ushort lighting = 0;

        //byte location = 12
        public byte ao = 0;

        //byte location = 13
        public byte flags = 0;

        //byte location = 14
        public ushort texID = 0;

        //always updates when adding new vertex properties
        public static int GetSize()
        {
            return Marshal.SizeOf<BlockVertex>();
        }

        //converts float values from 0-chunksize to shorts, which are then converted again in the shader
        static ushort EncodeToShortXZ(float value, float chunkSize = WorldConstants.CHUNK_WIDTH)
        {
            //normalize [0, chunkSize) -> [0, 1)
            float normalized = value / chunkSize;
            //scale to short range
            return (ushort)(normalized * ushort.MaxValue);
        }

        //converts float values from 0-chunksize to shorts, which are then converted again in the shader, but for y
        static ushort EncodeToShortY(float value, float chunkHeight = WorldConstants.CHUNK_HEIGHT)
        {
            float normalized = value / chunkHeight;
            return (ushort)(normalized * ushort.MaxValue);
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