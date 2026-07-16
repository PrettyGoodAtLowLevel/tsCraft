using OpenTK.Graphics.OpenGL4;
using OurCraft.Utility;
using StbImageSharp;

namespace OurCraft.Graphics.OpenGL_Objects
{
    //picture data mapped onto mesh vertices
    public class Texture
    {
        private int ID = 0;
        public string path = string.Empty;
        public ulong Handle;

        //initialize id
        public Texture() { ID = 0; }

        //tries to load in a texture file with stb sharp
        public bool Load(string filename, TextureUnit unit = TextureUnit.Texture0)
        {
            //get correct orientation 
            string fullPath = FileConstants.ASSETS_PATH + filename; 
            StbImage.stbi_set_flip_vertically_on_load(1); 

            //load image using StbImageSharp
            ImageResult image; 
            try
            { 
                using var stream = File.OpenRead(fullPath); 
                image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha); 
            } 
            catch (Exception e)
            { 
                Console.WriteLine($"Failed to load texture: {filename}\n{e.Message}"); return false;
            }

            //if successful load, start openGL process
            path = filename; 
            ID = GL.GenTexture(); 
            GL.ActiveTexture(unit); 
            GL.BindTexture(TextureTarget.Texture2D, ID); 

            //upload to OpenGL
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.SrgbAlpha, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxAnisotropy,  1.0f);

            //set filtering and wrapping
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest); 
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //set up bindless texture handle
            Handle = Bindless.GetTextureHandle((uint)ID);
            Bindless.MakeTextureHandleResident(Handle);

            return true;
        }

        //free up vram
        public void Delete()
        {
            if (ID != 0)
            {
                Bindless.MakeTextureHandleNonResident(Handle);
                GL.DeleteTexture(ID);

                ID = 0;
            }
        }

        //bind texture, only for chunk shader
        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }

        public override string ToString()
        {
            return $"ID: {ID}, Path: {path}, Handle: {Handle}";
        }
    }
}