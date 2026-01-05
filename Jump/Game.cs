using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Media;

namespace Jump
{
    class Game : GameWindow
    {
        bool isPlaying = false, isMusicOn = true;
        SoundPlayer _player;
        private Camera _camera;
        private Shader _shader;
        private int _vao;
        private Vector2 _lastMousePos;
        private Texture _tTitle;

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _camera = new Camera(new Vector3(0, 0, 3));
            try { _player = new SoundPlayer("music.wav"); _player.PlayLooping(); } catch { }

            string vCode = @"#version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec2 aTex;
                out vec2 texCoord;
                uniform mat4 view; uniform mat4 projection;
                void main() { gl_Position = vec4(aPos, 1.0) * view * projection; texCoord = aTex; }";

            string fCode = @"#version 330 core
                out vec4 FragColor;
                in vec2 texCoord;
                uniform sampler2D tex0;
                uniform vec4 color;
                uniform bool useTex;
                void main() { 
                    if(useTex) { FragColor = texture(tex0, texCoord); if(FragColor.a < 0.1) discard; }
                    else FragColor = color; 
                }";

            _shader = new Shader(vCode, fCode);
            SetupScene();
        }

        void SetupScene()
        {
            float[] v = {
                // X, Y, Z,          U, V
                // Titlu Imagine (0-5)
                -0.6f, 0.4f, 0,  0,0,  0.6f, 0.4f, 0,  1,0,  0.6f, 0.8f, 0,  1,1,
                 0.6f, 0.8f, 0,  1,1, -0.6f, 0.8f, 0,  0,1, -0.6f, 0.4f, 0,  0,0,
                // Buton Verde (6-11)
                -0.2f, 0.1f, 0,  0,0,  0.2f, 0.1f, 0,  0,0,  0.2f, 0.3f, 0,  0,0,
                 0.2f, 0.3f, 0,  0,0, -0.2f, 0.3f, 0,  0,0, -0.2f, 0.1f, 0,  0,0,
                // Buton Albastru (12-17)
                -0.2f,-0.15f,0,  0,0,  0.2f,-0.15f,0,  0,0,  0.2f, 0.05f,0,  0,0,
                 0.2f, 0.05f,0,  0,0, -0.2f, 0.05f,0,  0,0, -0.2f,-0.15f,0,  0,0,
                // Buton Rosu (18-23)
                -0.2f,-0.4f, 0,  0,0,  0.2f,-0.4f, 0,  0,0,  0.2f,-0.2f, 0,  0,0,
                 0.2f,-0.2f, 0,  0,0, -0.2f,-0.2f, 0,  0,0, -0.2f,-0.4f, 0,  0,0,
                // Camera 3D (24-59)
                -5,-5,-5,0,0, 5,-5,-5,0,0, 5,5,-5,0,0, 5,5,-5,0,0, -5,5,-5,0,0, -5,-5,-5,0,0,
                -5,-5, 5,0,0, 5,-5, 5,0,0, 5,5, 5,0,0, 5,5, 5,0,0, -5,5, 5,0,0, -5,-5, 5,0,0,
                -5, 5, 5,0,0, -5, 5,-5,0,0, -5,-5,-5,0,0, -5,-5,-5,0,0, -5,-5, 5,0,0, -5, 5, 5,0,0,
                 5, 5, 5,0,0,  5, 5,-5,0,0,  5,-5,-5,0,0,  5,-5,-5,0,0,  5,-5, 5,0,0,  5, 5, 5,0,0,
                -5,-5,-5,0,0, 5,-5,-5,0,0, 5,-5, 5,0,0, 5,-5, 5,0,0, -5,-5, 5,0,0, -5,-5,-5,0,0,
                -5, 5,-5,0,0, 5, 5,-5,0,0, 5, 5, 5,0,0, 5, 5, 5,0,0, -5, 5, 5,0,0, -5, 5,-5,0,0
            };

            _vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, v.Length * sizeof(float), v, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _tTitle = Texture.LoadFromFile("game_title.png");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!isPlaying)
            {
                if (MouseState.IsButtonPressed(MouseButton.Left))
                {
                    float cx = Size.X / 2f, cy = Size.Y / 2f; Vector2 m = MouseState.Position;
                    if (m.X >= cx - 150 && m.X <= cx + 150 && m.Y >= cy - 200 && m.Y <= cy - 50) { isPlaying = true; CursorState = CursorState.Grabbed; _lastMousePos = m; }
                    if (m.X >= cx - 150 && m.X <= cx + 150 && m.Y >= cy - 50 && m.Y <= cy + 50) { if (isMusicOn) _player?.Stop(); else _player?.PlayLooping(); isMusicOn = !isMusicOn; }
                    if (m.X >= cx - 150 && m.X <= cx + 150 && m.Y >= cy + 100 && m.Y <= cy + 250) Close();
                }
            }
            else
            {
                float s = 4f * (float)e.Time;
                if (KeyboardState.IsKeyDown(Keys.W)) _camera.Position += _camera.Front * s;
                if (KeyboardState.IsKeyDown(Keys.S)) _camera.Position -= _camera.Front * s;
                if (KeyboardState.IsKeyDown(Keys.Escape)) { isPlaying = false; CursorState = CursorState.Normal; }
                _camera.Yaw += (MouseState.Position.X - _lastMousePos.X) * 0.1f;
                _camera.Pitch -= (MouseState.Position.Y - _lastMousePos.Y) * 0.1f;
                _lastMousePos = MouseState.Position; _camera.UpdateVectors();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();
            if (isPlaying)
            {
                GL.ClearColor(0.1f, 0.1f, 0.2f, 1);
                _shader.SetMatrix4("view", _camera.GetViewMatrix());
                _shader.SetMatrix4("projection", Matrix4.CreatePerspectiveFieldOfView(1.0f, Size.X / (float)Size.Y, 0.1f, 100f));
                _shader.SetBool("useTex", false); _shader.SetVector4("color", new Vector4(0.7f, 0.7f, 0.7f, 1));
                GL.BindVertexArray(_vao); GL.DrawArrays(PrimitiveType.Triangles, 24, 36);
            }
            else
            {
                GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
                _shader.SetMatrix4("view", Matrix4.Identity); _shader.SetMatrix4("projection", Matrix4.Identity);
                GL.BindVertexArray(_vao);

                // Titlu cu imagine
                if (_tTitle != null) { _shader.SetBool("useTex", true); _tTitle.Use(); }
                else { _shader.SetBool("useTex", false); _shader.SetVector4("color", new Vector4(1, 1, 1, 1)); }
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                // Butoane fara imagine (doar culori)
                _shader.SetBool("useTex", false);
                _shader.SetVector4("color", new Vector4(0, 0.8f, 0, 1)); GL.DrawArrays(PrimitiveType.Triangles, 6, 6); // Verde
                _shader.SetVector4("color", isMusicOn ? new Vector4(0, 0.4f, 0.8f, 1) : new Vector4(0.4f, 0.4f, 0.4f, 1)); GL.DrawArrays(PrimitiveType.Triangles, 12, 6); // Albastru
                _shader.SetVector4("color", new Vector4(0.8f, 0, 0, 1)); GL.DrawArrays(PrimitiveType.Triangles, 18, 6); // Rosu
            }
            SwapBuffers();
        }
    }
}