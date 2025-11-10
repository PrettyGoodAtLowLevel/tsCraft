using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OurCraft
{
    //compiles txt files into shader source code for openGL
    //also allows to activate the set shader source code for the current rendering path
    public class Shader
    {
        //members
        public int ID {get; private set;}

        //methods
        //initialize id
        public Shader() { ID = 0; }

        //tries to load in shader source code from sepcified file paths
        public void Create(string vertexFile, string fragmentFile)
        {
            string shaderFilePath = "C:/Users/alial/OneDrive/Desktop/OurCraft/Shaders/";

            string vertexCode = File.ReadAllText(shaderFilePath + vertexFile);
            string fragmentCode = File.ReadAllText(shaderFilePath + fragmentFile);

            //load vshader
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexCode);
            GL.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader, "VERTEX");

            //load fshader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentCode);
            GL.CompileShader(fragmentShader);
            CheckShaderCompile(fragmentShader, "FRAGMENT");

            //attatch shader source code to gpu
            ID = GL.CreateProgram();
            GL.AttachShader(ID, vertexShader);
            GL.AttachShader(ID, fragmentShader);
            GL.LinkProgram(ID);
            CheckProgramLink(ID);
            
            //after fully compiled on gpu no reason to keep this extra data
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }
        
        //activate shader
        public void Activate()
        {
            GL.UseProgram(ID);
        }

        //free up vram at end of program
        public void Delete()
        {
            if (ID != 0)
            {
                GL.DeleteProgram(ID);
                ID = 0;
            }
        }

        //changes a uniform bool value
        public void SetBool(string name, bool value)
        {
            this.Activate();
            int loc = GL.GetUniformLocation(ID, name);
            int val = value == true ? 1 : 0;
            GL.Uniform1(loc, val);
        }

        //changes a uniform integer value
        public void SetInt(string name, int value)
        {
            int loc = GL.GetUniformLocation(ID, name);
            GL.Uniform1(loc, value);
        }

        //changes a uniform float value
        public void SetFloat(string name, float value)
        {
            int loc = GL.GetUniformLocation(ID, name);
            GL.Uniform1(loc, value);
        }

        //changes a uniform vector3 value
        public void SetVector3(string name, Vector3 value)
        {
            int loc = GL.GetUniformLocation(ID, name);
            GL.Uniform3(loc, value);
        }

        //changes a uniform vector2 value
        public void SetVector2(string name, Vector2 value)
        {
            int loc = GL.GetUniformLocation(ID, name);
            GL.Uniform2(loc, value);
        }

        //check shader errors
        private void CheckShaderCompile(int shader, string type)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{infoLog}");
            }
        }

        //check shader errors
        private void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"ERROR::PROGRAM_LINKING_ERROR\n{infoLog}");
            }
        }

        //delete
        ~Shader()
        {
            Console.WriteLine($"Deleted shader at location {ID}");
            Delete();
        }
    }
}