using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OurCraft.Utility;
using System.Text;

namespace OurCraft.Graphics.OpenGL_Objects
{
    //compiles .vert and .frag files into shader source code for openGL
    //also allows to set the shader source for the current rendering path, and allows to manipulate currently bound shader uniforms
    public class Shader
    {
        private readonly static string shadersFilePath = FileConstants.SHADERS_PATH;

        public int ID {get; private set;}

        //initialize id
        public Shader() { ID = 0; }

        //delete
        ~Shader()
        {
            Console.WriteLine($"Deleted shader at location {ID}");
            Delete();
        }

        //tries to load in shader source code from sepcified file paths
        //tries to load in shader source code from specified file paths
        public void Create(string vertexFile, string fragmentFile, bool debug = false)
        {
            string shaderFilePath = shadersFilePath;
            string vertexCode = File.ReadAllText(shaderFilePath + vertexFile);
            string fragmentCode = File.ReadAllText(shaderFilePath + fragmentFile);

            if (debug)
            {
                Console.WriteLine(vertexCode);
                Console.WriteLine(fragmentCode);
                Console.WriteLine($"Vertex: {vertexCode.Length} chars, {Encoding.UTF8.GetByteCount(vertexCode)} bytes");
                Console.WriteLine($"Fragment: {fragmentCode.Length} chars, {Encoding.UTF8.GetByteCount(fragmentCode)} bytes");
            }

            //load vshader
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            SetShaderSource(vertexShader, vertexCode);
            GL.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader, "VERTEX");

            //load fshader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            SetShaderSource(fragmentShader, fragmentCode);
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

        //sets shader source using an explicit UTF-8 byte length, since GL.ShaderSource(int, string)
        //uses string.Length (char count) instead of the actual UTF-8 byte count, which truncates
        //the source if it contains any non-ASCII characters (e.g. smart quotes, en-dashes, etc.)
        private static void SetShaderSource(int shader, string source)
        {
            int byteCount = Encoding.UTF8.GetByteCount(source);
            GL.ShaderSource(shader, 1, new[] { source }, new[] { byteCount });
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

        //activate shader
        public void Activate()
        {
            GL.UseProgram(ID);
        }

        //changes a uniform bool value
        public void SetBool(string name, bool value)
        { 
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

        //sets a shader matrix value
        public void SetMatrix4(string name, ref Matrix4 value)
        {
            int loc = GL.GetUniformLocation(ID, name);
            GL.UniformMatrix4(loc, false, ref value);
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
        private static void CheckShaderCompile(int shader, string type)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{infoLog}");
            }
        }

        //check shader errors
        private static void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"ERROR::PROGRAM_LINKING_ERROR\n{infoLog}");
            }
        }

        public override string ToString()
        {
            return $"ID: {ID}";
        }
    }
}