using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Jump
{
    public class Shader
    {
        public int Handle;
        public Shader(string v, string f)
        {
            int vs = GL.CreateShader(ShaderType.VertexShader); GL.ShaderSource(vs, v); GL.CompileShader(vs);
            int fs = GL.CreateShader(ShaderType.FragmentShader); GL.ShaderSource(fs, f); GL.CompileShader(fs);
            Handle = GL.CreateProgram(); GL.AttachShader(Handle, vs); GL.AttachShader(Handle, fs); GL.LinkProgram(Handle);
        }
        public void Use() => GL.UseProgram(Handle);
        public void SetMatrix4(string n, Matrix4 d) => GL.UniformMatrix4(GL.GetUniformLocation(Handle, n), false, ref d);
        public void SetVector4(string n, Vector4 d) => GL.Uniform4(GL.GetUniformLocation(Handle, n), d);
        public void SetBool(string n, bool d)
        {
            int value;
            if (d)
                value = 1;
            else
                value = 0;

            GL.Uniform1(GL.GetUniformLocation(Handle, n), value);
        }
    }
}