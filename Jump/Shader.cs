using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Jump
{
    public class Shader
    {
        public int Handle;

        public Shader(string vertexSource, string fragmentSource)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.LinkProgram(Handle);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void SetMatrix4(string name, Matrix4 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, false, ref value);
        }

        public void SetVector4(string name, Vector4 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform4(location, value);
        }

        public void SetBool(string name, bool value)
        {
            int location = GL.GetUniformLocation(Handle, name);

            int intValue;

            if (value == true)
            {
                intValue = 1;
            }
            else
            {
                intValue = 0;
            }
            GL.Uniform1(location, intValue);
        }
        public void SetFloat(string name, float value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }
    }
}
