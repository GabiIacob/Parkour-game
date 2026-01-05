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
        // Stare joc
        private bool isPlaying = false;
        private bool isMusicOn = true;

        // Sunet
        private SoundPlayer _musicPlayer;

        // Randare
        private Camera _camera;
        private Shader _shader;
        private Texture _titleTexture;
        private Texture _lavaTexture;

        // OpenGL
        private int _vao;

        // Input
        private Vector2 _lastMousePosition;

        public Game(GameWindowSettings gws, NativeWindowSettings nws)
            : base(gws, nws)
        {
        }

        // ===================== LOAD =====================
        protected override void OnLoad()
        {
            base.OnLoad();

            // Setări OpenGL
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(
                BlendingFactor.SrcAlpha,
                BlendingFactor.OneMinusSrcAlpha
            );

            // Cameră - poziționată mai sus și mai în spate pentru a vedea scena
            _camera = new Camera(new Vector3(0, 2, 8));

            // Muzică
            try
            {
                _musicPlayer = new SoundPlayer("music.wav");
                _musicPlayer.PlayLooping();
            }
            catch { }

            // Shader code
            string vertexShaderCode = @"#version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec2 aTex;

                out vec2 texCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main()
                {
                    gl_Position = projection * view * model * vec4(aPos, 1.0);
                    texCoord = aTex;
                }";

            string fragmentShaderCode = @"#version 330 core
                in vec2 texCoord;
                out vec4 FragColor;

                uniform sampler2D tex0;
                uniform vec4 color;
                uniform bool useTex;

                void main()
                {
                    if (useTex)
                    {
                        vec4 texColor = texture(tex0, texCoord);
                        FragColor = texColor;
                    }
                    else
                    {
                        FragColor = color;
                    }
                }";

            _shader = new Shader(vertexShaderCode, fragmentShaderCode);
            _shader.Use();

            // Setăm sampler-ul la texture unit 0
            int texLocation = GL.GetUniformLocation(_shader.Handle, "tex0");
            GL.Uniform1(texLocation, 0);

            SetupScene();
        }

        // ===================== SCENA =====================
        private void SetupScene()
        {
            // Datele vertecșilor (UI + podea)
            float[] vertices =
            {
                // --- TITLU (0–5)
                -0.6f, 0.4f, 0,  0,0,
                 0.6f, 0.4f, 0,  1,0,
                 0.6f, 0.8f, 0,  1,1,

                 0.6f, 0.8f, 0,  1,1,
                -0.6f, 0.8f, 0,  0,1,
                -0.6f, 0.4f, 0,  0,0,

                // --- BUTON VERDE (6–11)
                -0.2f, 0.1f, 0,  0,0,
                 0.2f, 0.1f, 0,  0,0,
                 0.2f, 0.3f, 0,  0,0,

                 0.2f, 0.3f, 0,  0,0,
                -0.2f, 0.3f, 0,  0,0,
                -0.2f, 0.1f, 0,  0,0,

                // --- BUTON ALBASTRU (12–17)
                -0.2f,-0.15f,0, 0,0,
                 0.2f,-0.15f,0, 0,0,
                 0.2f, 0.05f,0, 0,0,

                 0.2f, 0.05f,0, 0,0,
                -0.2f, 0.05f,0, 0,0,
                -0.2f,-0.15f,0, 0,0,

                // --- BUTON ROȘU (18–23)
                -0.2f,-0.4f,0, 0,0,
                 0.2f,-0.4f,0, 0,0,
                 0.2f,-0.2f,0, 0,0,

                 0.2f,-0.2f,0, 0,0,
                -0.2f,-0.2f,0, 0,0,
                -0.2f,-0.4f,0, 0,0,

                // --- PODEA LAVA (24–29)
                -20, -2, -20,  0, 0,
                 20, -2, -20,  5, 0,
                 20, -2,  20,  5, 5,

                 20, -2,  20,  5, 5,
                -20, -2,  20,  0, 5,
                -20, -2, -20,  0, 0,
            };

            _vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float),
                vertices,
                BufferUsageHint.StaticDraw
            );

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            try
            {
                _titleTexture = Texture.LoadFromFile("game_title.png");
                System.Console.WriteLine("Title texture loaded");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Failed to load title: {ex.Message}");
            }

            try
            {
                _lavaTexture = Texture.LoadFromFile("lava.png");
                System.Console.WriteLine("Lava texture loaded successfully");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Failed to load lava: {ex.Message}");
            }
        }

        // ===================== UPDATE =====================
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!isPlaying)
            {
                HandleMenuInput();
            }
            else
            {
                HandleGameInput(e);
            }
        }

        private void HandleMenuInput()
        {
            if (!MouseState.IsButtonPressed(MouseButton.Left))
                return;

            Vector2 mouse = MouseState.Position;
            float cx = Size.X / 2f;
            float cy = Size.Y / 2f;

            // Start
            if (mouse.Y >= cy - 200 && mouse.Y <= cy - 50)
            {
                isPlaying = true;
                CursorState = CursorState.Grabbed;
                _lastMousePosition = mouse;
            }

            // Music
            else if (mouse.Y >= cy - 50 && mouse.Y <= cy + 50)
            {
                if (isMusicOn) _musicPlayer?.Stop();
                else _musicPlayer?.PlayLooping();

                isMusicOn = !isMusicOn;
            }

            // Exit
            else if (mouse.Y >= cy + 100 && mouse.Y <= cy + 250)
            {
                Close();
            }
        }

        private void HandleGameInput(FrameEventArgs e)
        {
            float speed = 4f * (float)e.Time;

            if (KeyboardState.IsKeyDown(Keys.W))
                _camera.Position += _camera.Front * speed;

            if (KeyboardState.IsKeyDown(Keys.S))
                _camera.Position -= _camera.Front * speed;

            if (KeyboardState.IsKeyDown(Keys.A))
                _camera.Position -= _camera.Right * speed;

            if (KeyboardState.IsKeyDown(Keys.D))
                _camera.Position += _camera.Right * speed;

            if (KeyboardState.IsKeyDown(Keys.Space))
                _camera.Position += Vector3.UnitY * speed;

            if (KeyboardState.IsKeyDown(Keys.LeftShift))
                _camera.Position -= Vector3.UnitY * speed;

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                isPlaying = false;
                CursorState = CursorState.Normal;
            }

            Vector2 mouse = MouseState.Position;

            _camera.Yaw += (mouse.X - _lastMousePosition.X) * 0.1f;
            _camera.Pitch -= (mouse.Y - _lastMousePosition.Y) * 0.1f;

            _lastMousePosition = mouse;
            _camera.UpdateVectors();
        }

        // ===================== RENDER =====================
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();

            if (isPlaying)
            {
                RenderGame();
            }
            else
            {
                RenderMenu();
            }

            SwapBuffers();
        }

        private void RenderGame()
        {
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1f);

            _shader.Use();
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4(
                "projection",
                Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(45f),
                    Size.X / (float)Size.Y,
                    0.1f,
                    100f
                )
            );

            GL.BindVertexArray(_vao);

            // Render lava floor - cu textura
            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetBool("useTex", true);
            _lavaTexture.Use();
            GL.DrawArrays(PrimitiveType.Triangles, 24, 6); // doar podeaua
        }

        private void RenderMenu()
        {
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);

            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetMatrix4("view", Matrix4.Identity);
            _shader.SetMatrix4("projection", Matrix4.Identity);

            GL.BindVertexArray(_vao);

            // Titlu
            if (_titleTexture != null)
            {
                _shader.SetBool("useTex", true);
                _titleTexture.Use();
            }
            else
            {
                _shader.SetBool("useTex", false);
                _shader.SetVector4("color", Vector4.One);
            }

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // Butoane
            _shader.SetBool("useTex", false);

            _shader.SetVector4("color", new Vector4(0, 0.8f, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 6, 6);

            _shader.SetVector4(
                "color",
                isMusicOn ? new Vector4(0, 0.4f, 0.8f, 1) : new Vector4(0.4f, 0.4f, 0.4f, 1)
            );
            GL.DrawArrays(PrimitiveType.Triangles, 12, 6);

            _shader.SetVector4("color", new Vector4(0.8f, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 18, 6);
        }
    }
}
