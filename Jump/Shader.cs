using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Jump
{
    public class Shader
    {
        // ID-ul programului shader din OpenGL
        public int Handle;

        // Constructor: creează shaderul din codul vertex + fragment
        public Shader(string vertexSource, string fragmentSource)
        {
            // 1. Creăm și compilăm vertex shaderul
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);

            // 2. Creăm și compilăm fragment shaderul
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);

            // 3. Creăm programul shader și atașăm shaderele
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // 4. Link (legăm) programul
            GL.LinkProgram(Handle);
        }

        // Activează shaderul
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // Trimite o matrice 4x4 către shader (uniform)
        public void SetMatrix4(string name, Matrix4 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, false, ref value);
        }

        // Trimite un Vector4 către shader (uniform)
        public void SetVector4(string name, Vector4 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform4(location, value);
        }

        // Trimite un bool către shader (OpenGL nu are bool real)
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
    }
}
