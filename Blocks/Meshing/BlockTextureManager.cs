using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace OurCraft.Blocks.Meshing
{
    //allows for easy managing and loading of block textures
    public static class BlockTextureManager
    {
        //name -> index used in chunk meshes
        private static readonly Dictionary<string, ushort> textureIndices = new();

        //keeps GL texture IDs alive
        private static readonly List<int> glTextures = new();

        //bindless handles uploaded to GPU
        private static readonly List<ulong> bindlessHandles = new();

        //ssbo containing all bindless handles
        public static int TextureHandleSSBO { get; private set; }
        public static int TextureCount => bindlessHandles.Count;

        //loads all found texture pictures in a root folder and sends them to gpu
        public static void LoadAllTextures(string rootFolder, bool debug = false)
        {
            textureIndices.Clear();
            glTextures.Clear();
            bindlessHandles.Clear();

            string[] files = Directory.GetFiles(rootFolder, "*.png", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                LoadTexture(file, debug);
            }

            UploadHandlesToGPU();
            if (debug) Console.WriteLine($"Loaded {TextureCount} textures.");
        }

        //helper for finding texture indices used in block meshing
        public static ushort GetTextureIndex(string handle)
        {
            if (!textureIndices.TryGetValue(handle, out ushort index))
            {
                throw new Exception($"Texture '{handle}' not found.");
            }

            return index;
        }

        //loads a texture into gpu memory
        private static void LoadTexture(string filePath, bool debug = false)
        {
            string handle = Path.GetFileNameWithoutExtension(filePath);         
            if (textureIndices.ContainsKey(handle))
            {
                throw new Exception($"Duplicate texture name detected: {handle}\n{filePath}");
            }

            StbImage.stbi_set_flip_vertically_on_load(1);
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            using (Stream stream = File.OpenRead(filePath))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.SrgbAlpha, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte,image.Data);
            }
            GL.TextureParameter(texture, TextureParameterName.TextureLodBias,  -0.65f);
            GL.GenerateTextureMipmap(texture);

            //good defaults for voxel textures
            GL.TextureParameter(texture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TextureParameter(texture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TextureParameter(texture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TextureParameter(texture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            //optional anisotropic filtering
            float maxAniso = 16.0f;
            GL.TextureParameter(texture,(TextureParameterName)All.TextureMaxAnisotropyExt,  maxAniso);

            //create bindless handle
            ulong bindlessHandle = (ulong)GL.Arb.GetTextureHandle(texture);
            GL.Arb.MakeTextureHandleResident(bindlessHandle);
            ushort index = (ushort)bindlessHandles.Count;

            textureIndices.Add(handle, index);
            glTextures.Add(texture);
            bindlessHandles.Add(bindlessHandle);

            if (debug) Console.WriteLine($"Loaded {handle} -> {index}");
        }

        //sends all texture handles to ssbo on chunk fragment shader
        private static void UploadHandlesToGPU()
        {
            if (TextureHandleSSBO != 0)
            {
                GL.DeleteBuffer(TextureHandleSSBO);
            }

            TextureHandleSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, TextureHandleSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, bindlessHandles.Count * sizeof(ulong), bindlessHandles.ToArray(), BufferUsageHint.StaticDraw);

            //bind to binding point 0
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, TextureHandleSSBO);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        //gets rid of all loaded textures on gpu
        public static void Dispose()
        {
            foreach (ulong handle in bindlessHandles)
            {
                GL.Arb.MakeTextureHandleNonResident(handle);
            }

            foreach (int texture in glTextures)
            {
                GL.DeleteTexture(texture);
            }

            if (TextureHandleSSBO != 0)
            {
                GL.DeleteBuffer(TextureHandleSSBO);
            }

            textureIndices.Clear();
            glTextures.Clear();
            bindlessHandles.Clear();
        }
    }
}
