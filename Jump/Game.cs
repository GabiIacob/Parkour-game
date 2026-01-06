using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Media;
using System.Collections.Generic;

namespace Jump
{
    class Game : GameWindow
    {
        // ===================== STARE JOC =====================
        private bool isPlaying = false;
        private bool isMusicOn = true;

        // Sunet
        private SoundPlayer _musicPlayer;
        private SoundPlayer _jumpSound;
        private SoundPlayer _landSound;

        // Randare
        private Camera _camera;
        private Shader _shader;
        private Texture _titleTexture;
        private Texture _lavaTexture;

        // OpenGL
        private int _vao;

        // Lista de modele 3D
        private List<Model> _blocks = new List<Model>();
        private List<Vector3> _blockPositions = new List<Vector3>();
        private List<Block> physicalBlocks = new List<Block>();

        //rock
        private List<Model> rocks = new List<Model>();
        private List<Vector3> rockPositions = new List<Vector3>();

        // Input
        private Vector2 _lastMousePosition;

        private float verticalSpeed = 0f;
        private float gravity = -9.8f;
        private bool isOnGround = false;
        private bool wasOnGround = false;

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

            // Cameră
            _camera = new Camera(new Vector3(0, 0, 8));

            // Muzică
            try
            {
                _musicPlayer = new SoundPlayer("music.wav");
                _musicPlayer.PlayLooping();
            }
            catch { }

            // Sunete
            try { _jumpSound = new SoundPlayer("jump.wav"); } catch { }
            try { _landSound = new SoundPlayer("land.wav"); } catch { }

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

            int texLocation = GL.GetUniformLocation(_shader.Handle, "tex0");
            GL.Uniform1(texLocation, 0);

            SetupScene();

            // ===================== ÎNCĂRCARE MODEL STONE =====================
            try
            {
                Model stoneBlock = new Model("models/STONE.dae");

                for (int i = 0; i < 15; i++)
                    _blocks.Add(stoneBlock);

                _blockPositions.Add(new Vector3(0f, -2f, -2f));
                _blockPositions.Add(new Vector3(1f, -2f, -2f));
                _blockPositions.Add(new Vector3(-1f, -2f, -2f));
                _blockPositions.Add(new Vector3(0f, -2f, -1f));
                _blockPositions.Add(new Vector3(1f, -2f, -1f));
                _blockPositions.Add(new Vector3(-1f, -2f, -1f));
                _blockPositions.Add(new Vector3(0f, -2f, 0f));
                _blockPositions.Add(new Vector3(1f, -2f, 0f));
                _blockPositions.Add(new Vector3(-1f, -2f, 0f));
                _blockPositions.Add(new Vector3(2f, -1f, 2f));
                _blockPositions.Add(new Vector3(4f, 0f, 4f));
                _blockPositions.Add(new Vector3(6f, -1f, 6f));
                _blockPositions.Add(new Vector3(8f, 0f, 8f));
                _blockPositions.Add(new Vector3(10f, 1f, 10f));
                _blockPositions.Add(new Vector3(12f, 2f, 12f));

                physicalBlocks.Clear();
                physicalBlocks.Add(new Block(new Vector3(-20f, -2f, -20f), new Vector3(40f, 1f, 40f)));

                Vector3 blockSize = new Vector3(1f, 1f, 1f);
                foreach (var pos in _blockPositions)
                    physicalBlocks.Add(new Block(pos, blockSize));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Failed to load STONE.dae: {ex.Message}");
            }

            // ===================== ÎNCĂRCARE MODEL ROCK =====================
            try
            {
                Model rockModel = new Model("models/rock.dae");

                for (int i = 0; i < 2; i++)
                    rocks.Add(rockModel);

                rockPositions.Add(new Vector3(-14f, 3f, 14f));
                rockPositions.Add(new Vector3(14f, 3f, 14f));

                float rockScale = 6f;
                Vector3 rockSize = new Vector3(rockScale, rockScale, rockScale);

                foreach (var pos in rockPositions)
                {
                    Vector3 collisionPos = pos - new Vector3(rockScale / 2, 0, rockScale / 2);
                    physicalBlocks.Add(new Block(collisionPos, rockSize));
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Failed to load rock.dae: {ex.Message}");
            }
        }

        // ===================== SCENA UI =====================
        private void SetupScene()
        {
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

            try { _titleTexture = Texture.LoadFromFile("game_title.png"); } catch { }
            try { _lavaTexture = Texture.LoadFromFile("lava.png"); } catch { }
        }

        // ===================== UPDATE =====================
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!isPlaying) HandleMenuInput();
            else HandleGameInput(e);
        }

        private void HandleMenuInput()
        {
            if (!MouseState.IsButtonPressed(MouseButton.Left)) return;

            Vector2 mouse = MouseState.Position;
            float cx = Size.X / 2f;
            float cy = Size.Y / 2f;

            if (mouse.Y >= cy - 200 && mouse.Y <= cy - 50)
            {
                isPlaying = true;
                CursorState = CursorState.Grabbed;
                _lastMousePosition = mouse;
            }
            else if (mouse.Y >= cy - 50 && mouse.Y <= cy + 50)
            {
                if (isMusicOn) _musicPlayer?.Stop();
                else _musicPlayer?.PlayLooping();
                isMusicOn = !isMusicOn;
            }
            else if (mouse.Y >= cy + 100 && mouse.Y <= cy + 250)
            {
                Close();
            }
        }

        private void HandleGameInput(FrameEventArgs e)
        {
            Vector3 playerSize = new Vector3(0.5f, 1.8f, 0.5f);
            float eyeHeight = 1.6f;
            float moveSpeed = 4f * (float)e.Time;

            // Jump cu sunet
            if (isOnGround && KeyboardState.IsKeyDown(Keys.Space))
            {
                verticalSpeed = 5f;
                isOnGround = false;
                _jumpSound?.Play();
            }

            verticalSpeed += gravity * (float)e.Time;

            Vector3 forward = Vector3.Normalize(new Vector3(_camera.Front.X, 0, _camera.Front.Z));
            Vector3 right = Vector3.Normalize(new Vector3(_camera.Right.X, 0, _camera.Right.Z));

            Vector3 move = Vector3.Zero;
            if (KeyboardState.IsKeyDown(Keys.W)) move += forward;
            if (KeyboardState.IsKeyDown(Keys.S)) move -= forward;
            if (KeyboardState.IsKeyDown(Keys.A)) move -= right;
            if (KeyboardState.IsKeyDown(Keys.D)) move += right;

            if (move.LengthSquared > 0)
                move = Vector3.Normalize(move) * moveSpeed;

            Vector3 testPosX = _camera.Position + new Vector3(move.X, 0, 0);
            if (!IsCollidingWithPlayer(testPosX, playerSize, eyeHeight))
                _camera.Position.X = testPosX.X;

            Vector3 testPosZ = _camera.Position + new Vector3(0, 0, move.Z);
            if (!IsCollidingWithPlayer(testPosZ, playerSize, eyeHeight))
                _camera.Position.Z = testPosZ.Z;

            Vector3 testPosY = _camera.Position + new Vector3(0, verticalSpeed * (float)e.Time, 0);
            Block collidedBlock = GetCollidingBlockWithPlayer(testPosY, playerSize, eyeHeight);

            if (collidedBlock == null)
            {
                _camera.Position.Y = testPosY.Y;
                wasOnGround = isOnGround;
                isOnGround = false;
            }
            else
            {
                if (verticalSpeed < 0)
                {
                    _camera.Position.Y = collidedBlock.Position.Y + collidedBlock.Size.Y + eyeHeight;

                    // Land sound - doar dacă erai în aer
                    if (!wasOnGround)
                        _landSound?.Play();

                    wasOnGround = isOnGround;
                    isOnGround = true;
                }
                else
                {
                    _camera.Position.Y = collidedBlock.Position.Y - (playerSize.Y - eyeHeight);
                    wasOnGround = isOnGround;
                }
                verticalSpeed = 0f;
            }

            var lava = physicalBlocks[0];
            if (_camera.Position.Y - eyeHeight < lava.Position.Y + lava.Size.Y)
            {
                _camera.Position = new Vector3(0f, eyeHeight, 0f);
                verticalSpeed = 0f;
                isOnGround = true;
            }

            Vector2 mouse = MouseState.Position;
            _camera.Yaw += (mouse.X - _lastMousePosition.X) * 0.1f;
            _camera.Pitch -= (mouse.Y - _lastMousePosition.Y) * 0.1f;
            _lastMousePosition = mouse;
            _camera.UpdateVectors();

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                isPlaying = false;
                CursorState = CursorState.Normal;
            }
        }

        private bool IsCollidingWithPlayer(Vector3 cameraPos, Vector3 playerSize, float eyeHeight)
        {
            float feetY = cameraPos.Y - eyeHeight;
            float headY = cameraPos.Y + (playerSize.Y - eyeHeight);
            float tolerance = 0.01f;

            foreach (var block in physicalBlocks)
            {
                bool collideX = cameraPos.X + playerSize.X / 2 >= block.Position.X &&
                                cameraPos.X - playerSize.X / 2 <= block.Position.X + block.Size.X;

                bool collideY = headY >= block.Position.Y + tolerance &&
                                feetY <= block.Position.Y + block.Size.Y - tolerance;

                bool collideZ = cameraPos.Z + playerSize.Z / 2 >= block.Position.Z &&
                                cameraPos.Z - playerSize.Z / 2 <= block.Position.Z + block.Size.Z;

                if (collideX && collideY && collideZ)
                {
                    return true;
                }
            }

            return false;
        }

        private Block GetCollidingBlockWithPlayer(Vector3 cameraPos, Vector3 playerSize, float eyeHeight)
        {
            float feetY = cameraPos.Y - eyeHeight;
            float headY = cameraPos.Y + (playerSize.Y - eyeHeight);

            foreach (var block in physicalBlocks)
            {
                bool collideX = cameraPos.X + playerSize.X / 2 >= block.Position.X &&
                                cameraPos.X - playerSize.X / 2 <= block.Position.X + block.Size.X;
                bool collideY = headY >= block.Position.Y &&
                                feetY <= block.Position.Y + block.Size.Y;
                bool collideZ = cameraPos.Z + playerSize.Z / 2 >= block.Position.Z &&
                                cameraPos.Z - playerSize.Z / 2 <= block.Position.Z + block.Size.Z;

                if (collideX && collideY && collideZ)
                    return block;
            }

            return null;
        }

        // ===================== RENDER =====================
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();

            if (isPlaying) RenderGame();
            else RenderMenu();

            SwapBuffers();
        }

        private void RenderGame()
        {
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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

            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetBool("useTex", true);
            _lavaTexture?.Use();
            GL.DrawArrays(PrimitiveType.Triangles, 24, 6);

            if (_blocks.Count > 0)
            {
                for (int i = 0; i < _blocks.Count; i++)
                {
                    var block = _blocks[i];
                    var pos = _blockPositions[i];

                    Matrix4 modelMatrix =
                        Matrix4.CreateScale(0.01f) *
                        Matrix4.CreateTranslation(pos);

                    _shader.SetMatrix4("model", modelMatrix);
                    _shader.SetBool("useTex", false);
                    _shader.SetVector4("color", new Vector4(0.5f, 0.5f, 0.5f, 1f));

                    GL.BindVertexArray(block.VAO);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, block.Vertices.Length / 5);
                }
            }

            if (rocks.Count > 0)
            {
                for (int i = 0; i < rocks.Count; i++)
                {
                    var rock = rocks[i];
                    var pos = rockPositions[i];

                    Matrix4 modelMatrix =
                        Matrix4.CreateScale(6f) *
                        Matrix4.CreateTranslation(pos);

                    _shader.SetMatrix4("model", modelMatrix);
                    _shader.SetBool("useTex", true);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    _lavaTexture?.Use();

                    GL.BindVertexArray(rock.VAO);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, rock.Vertices.Length / 5);
                }
            }
        }

        private void RenderMenu()
        {
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);

            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetMatrix4("view", Matrix4.Identity);
            _shader.SetMatrix4("projection", Matrix4.Identity);

            GL.BindVertexArray(_vao);

            if (_titleTexture != null)
            {
                _shader.SetBool("useTex", true);
                _titleTexture.Use();
            }
            else _shader.SetBool("useTex", false);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            _shader.SetBool("useTex", false);
            _shader.SetVector4("color", new Vector4(0, 0.8f, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 6, 6);

            _shader.SetVector4("color", isMusicOn ? new Vector4(0, 0.4f, 0.8f, 1) : new Vector4(0.4f, 0.4f, 0.4f, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 12, 6);

            _shader.SetVector4("color", new Vector4(0.8f, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 18, 6);
        }
    }
}