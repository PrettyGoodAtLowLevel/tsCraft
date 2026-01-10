using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace OurCraft
{

    //picture data mapped onto mesh vertices
    public class Texture
    {
        //members
        private int ID = 0;
        public string path = string.Empty;

        //methods

        //initialize id
        public Texture() { ID = 0; }

        //tries to load in a texture file with stb sharp
        public bool Load(string filename, TextureUnit unit = TextureUnit.Texture0)
        {
            //get correct orientation 
            string fullPath = "C:/Users/alial/OneDrive/Desktop/OurCraft/Resources/" + filename; 
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

            //set filtering and wrapping
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest); 
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); 
            return true;
        }

        //bind texture
        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }

        //unbind texture
        public void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        //set uniform texture in frag shader
        public void Use(Shader shader, string uniformName, int unitIndex)
        {
            shader.Activate();
            int location = GL.GetUniformLocation(shader.ID, uniformName);
            GL.Uniform1(location, unitIndex);
        }

        //free up vram
        public void Delete()
        {
            if (ID != 0)
            {
                GL.DeleteTexture(ID);
                ID = 0;
            }
        }
    }
}