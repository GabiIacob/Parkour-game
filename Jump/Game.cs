using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Media;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jump
{
    class Game : GameWindow
    {
        //stari joc
        private bool isPlaying = false;
        private bool isMusicOn = true;


        //muzica
        private SoundPlayer _musicPlayer;
        private SoundPlayer _jumpSound;
        private SoundPlayer _landSound;
        private SoundPlayer _damageSound;

        //jucator
        private Player _player;
        private float _lavaTimer = 0f;
        private float _fallDamageThreshold = 10f;
        private float _lastFallSpeed = 0f;

        //camera si texturi
        private Camera _camera;
        private Shader _shader;
        private Texture _titleTexture;
        private Texture _lavaTexture;
        private Texture _stoneTexture;
        private Texture _lavamoonTexture;
        private Texture _coalTexture;
        private Texture _controlsTexture;

        private int _vao;

        //modele pozitii si coliziuni
        private List<Model> _blocks = new List<Model>();
        private List<Vector3> _blockPositions = new List<Vector3>();
        private List<Block> physicalBlocks = new List<Block>();

        private List<Model> rocks = new List<Model>();
        private List<Vector3> rockPositions = new List<Vector3>();

        private List<Model> _coal = new List<Model>();
        private List<Vector3> _coalPositions = new List<Vector3>();
        private Model lavamoon;
        private List<Vector3> lavamoonPositions = new List<Vector3>();


        private bool isGameOver = false;
        private float gameOverTimer = 0f;
        private float gameOverDuration = 4f;
        private class FireParticle
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float Life;
            public float Size;
        }
        private List<FireParticle> fireParticles = new List<FireParticle>();
        private Random random = new Random();

        private Vector2 _lastMousePosition;

        private float verticalSpeed = 0f;
        private float gravity = -9.8f;
        private bool isOnGround = false;
        private bool wasOnGround = false;


        private float _lavamoonRotation = 0f;


        private float _healthRegenTimer = 0f;
        private float _healthRegenInterval = 10f;
        private int _healthRegenAmount = 10;



        private float coalSpawnTimer = 0f;
        private float coalSpawnInterval = 10f;

        public Game(GameWindowSettings gws, NativeWindowSettings nws)
            : base(gws, nws)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(
                BlendingFactor.SrcAlpha,
                BlendingFactor.OneMinusSrcAlpha
            );

            _camera = new Camera(new Vector3(0, 0, 0));

            _player = new Player(100);

            try
            {
                _musicPlayer = new SoundPlayer("music.wav");
                _musicPlayer.PlayLooping();
            }
            catch { }

            try
            {
                _jumpSound = new SoundPlayer("jump.wav");
            }
            catch { }
            try
            {
                _landSound = new SoundPlayer("land.wav");
            }
            catch { }
            try
            {
                _damageSound = new SoundPlayer("damage.wav");
            }
            catch { }
            try
            {
                _lavamoonTexture = Texture.LoadFromFile("lavamoon.png");
            }
            catch { }
            try
            {
                _controlsTexture = Texture.LoadFromFile("controls.png");
            }
            catch
            {
                Console.WriteLine("Nu am putut încărca controls.png");
            }

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

            try
            {
                Model stoneBlock = new Model("models/STONE.dae");

                for (int i = 0; i < 43; i++)
                    _blocks.Add(stoneBlock);
                //baza
                _blockPositions.Add(new Vector3(0f, -2f, -2f));
                _blockPositions.Add(new Vector3(1f, -2f, -2f));
                _blockPositions.Add(new Vector3(-1f, -2f, -2f));
                _blockPositions.Add(new Vector3(0f, -2f, -1f));
                _blockPositions.Add(new Vector3(1f, -2f, -1f));
                _blockPositions.Add(new Vector3(-1f, -2f, -1f));
                _blockPositions.Add(new Vector3(0f, -2f, 0f));
                _blockPositions.Add(new Vector3(1f, -2f, 0f));
                _blockPositions.Add(new Vector3(-1f, -2f, 0f));


                //parkour
                _blockPositions.Add(new Vector3(2f, -1f, 2f));
                _blockPositions.Add(new Vector3(4f, 0f, 4f));
                _blockPositions.Add(new Vector3(6f, 1f, 6f));
                _blockPositions.Add(new Vector3(8f, 2f, 8f));
                _blockPositions.Add(new Vector3(10f, 3f, 10f));
                _blockPositions.Add(new Vector3(12f, 4f, 8f));



                _blockPositions.Add(new Vector3(11f, 5f, 5f));

                _blockPositions.Add(new Vector3(8f, 2f, 0f));

                _blockPositions.Add(new Vector3(10f, 3f, -3f));

                _blockPositions.Add(new Vector3(6f, 4f, -2f));

                _blockPositions.Add(new Vector3(6f, 5f, -4f));

                _blockPositions.Add(new Vector3(10f, 6f, -4f));

                _blockPositions.Add(new Vector3(9f, 7f, -7f));
                _blockPositions.Add(new Vector3(13f, 7f, -10));
                _blockPositions.Add(new Vector3(10f, 8f, -13f));

                _blockPositions.Add(new Vector3(10f, 9f, -16f));


                _blockPositions.Add(new Vector3(6f, 9f, -16f));

                _blockPositions.Add(new Vector3(2f, 9f, -16f));
                _blockPositions.Add(new Vector3(-3f, 9f, -16f));
                _blockPositions.Add(new Vector3(-3f, 10f, -13f));

                _blockPositions.Add(new Vector3(-5f, 11f, -11f));

                _blockPositions.Add(new Vector3(-7f, 11f, -8f));
                _blockPositions.Add(new Vector3(-9f, 11f, -5f));
                _blockPositions.Add(new Vector3(-12f, 11f, -2f));
                _blockPositions.Add(new Vector3(-12f, 11f, 2f));
                _blockPositions.Add(new Vector3(-12f, 11f, 6f));


                _blockPositions.Add(new Vector3(-11f, 11f, 6f));
                _blockPositions.Add(new Vector3(-13f, 11f, 6f));
                _blockPositions.Add(new Vector3(-12f, 11f, 7f));
                _blockPositions.Add(new Vector3(-11f, 11f, 7f));
                _blockPositions.Add(new Vector3(-13f, 11f, 7f));
                _blockPositions.Add(new Vector3(-12f, 11f, 8f));
                _blockPositions.Add(new Vector3(-11f, 11f, 8f));
                _blockPositions.Add(new Vector3(-13f, 11f, 8f));




























                physicalBlocks.Clear();
                physicalBlocks.Add(new Block(new Vector3(-20f, -2f, -20f), new Vector3(40f, 1f, 40f)));

                Vector3 blockSize = new Vector3(1f, 1f, 1f);
                for (int i = 0; i < _blockPositions.Count; i++)
                {
                    Vector3 size = new Vector3(1f, 1f, 1f);


                    physicalBlocks.Add(new Block(_blockPositions[i], size));
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Failed to load STONE.dae: {ex.Message}");
            }

            try
            {
                Model lavamooon = new Model("models/lavamoon.dae");
                lavamoon = lavamooon;
                lavamoonPositions.Add(new Vector3(0f, 12f, -32f));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Failed to load lavamoon.dae: {ex.Message}");
            }

            try
            {
                Model coalModel = new Model("models/coal.dae");
                _coal.Add(coalModel);

                Vector3 coalPos = new Vector3(-10f, -0.5f, -10f);
                _coalPositions.Add(coalPos);

                Vector3 coalSize = new Vector3(1.5f, 1.5f, 1.5f);

                Vector3 collisionPos = coalPos - new Vector3(coalSize.X / 2, 0, coalSize.Z / 2);

                // Adaugă blocul fizic pentru coliziune
                physicalBlocks.Add(new Block(collisionPos, coalSize));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Failed to load coal.dae: {ex.Message}");
            }
            try
            {
                Model rockModel = new Model("models/rock.dae");

                for (int i = 0; i < 4; i++)
                    rocks.Add(rockModel);

                rockPositions.Add(new Vector3(-14f, 3f, 14f));
                rockPositions.Add(new Vector3(14f, 3f, 14f));
                rockPositions.Add(new Vector3(14f, 3f, -14f));
                rockPositions.Add(new Vector3(-14f, 3f, -14f));





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

        private void SetupScene()
        {
            float[] vertices =
            {
                // titlu
                -0.6f, 0.4f, 0,  0,0,
                 0.6f, 0.4f, 0,  1,0,
                 0.6f, 0.8f, 0,  1,1,
                 0.6f, 0.8f, 0,  1,1,
                -0.6f, 0.8f, 0,  0,1,
                -0.6f, 0.4f, 0,  0,0,

                // verde
                -0.2f, 0.1f, 0,  0,0,
                 0.2f, 0.1f, 0,  0,0,
                 0.2f, 0.3f, 0,  0,0,
                 0.2f, 0.3f, 0,  0,0,
                -0.2f, 0.3f, 0,  0,0,
                -0.2f, 0.1f, 0,  0,0,

                //albastru
                -0.2f,-0.15f,0, 0,0,
                 0.2f,-0.15f,0, 0,0,
                 0.2f, 0.05f,0, 0,0,
                 0.2f, 0.05f,0, 0,0,
                -0.2f, 0.05f,0, 0,0,
                -0.2f,-0.15f,0, 0,0,

                // rosu
                -0.2f,-0.4f,0, 0,0,
                 0.2f,-0.4f,0, 0,0,
                 0.2f,-0.2f,0, 0,0,
                 0.2f,-0.2f,0, 0,0,
                -0.2f,-0.2f,0, 0,0,
                -0.2f,-0.4f,0, 0,0,

                // podea
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
            }
            catch { }
            try
            {
                _lavaTexture = Texture.LoadFromFile("lava.png");
            }
            catch { }
            try
            {
                _stoneTexture = Texture.LoadFromFile("stone.png");
            }
            catch { }
            try
            {
                _coalTexture = Texture.LoadFromFile("coal.png");
            }
            catch { }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!isPlaying)
                HandleMenuInput();
            else
            {
                HandleGameInput(e);
                UpdateFireParticles(e);
                _lavamoonRotation += (float)e.Time;
                UpdateCoalParticles(e);

            }
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
                if (isMusicOn)
                    _musicPlayer?.Stop();
                else
                    _musicPlayer?.PlayLooping();
                isMusicOn = !isMusicOn;
            }
            else if (mouse.Y >= cy + 100 && mouse.Y <= cy + 250)
            {
                Close();
            }
        }

        private void HandleGameInput(FrameEventArgs e)
        {
            if (isPlaying && !isGameOver)
            {
                coalSpawnTimer += (float)e.Time;
                if (coalSpawnTimer >= coalSpawnInterval)
                {
                    SpawnRandomCoal();
                    coalSpawnTimer = 0f;
                }
            }

            // HP-regen
            if (!isGameOver && _player.health > 0 && _player.health < 100)
            {
                _healthRegenTimer += (float)e.Time;

                if (_healthRegenTimer >= _healthRegenInterval)
                {
                    _player.health += _healthRegenAmount;

                    if (_player.health > 100)
                        _player.health = 100;

                    _healthRegenTimer = 0f;
                }
            }

            if (isGameOver)
            {
                gameOverTimer += (float)e.Time;
                if (gameOverTimer >= gameOverDuration)
                {
                    isPlaying = false;
                    isGameOver = false;
                    gameOverTimer = 0f;
                    CursorState = CursorState.Normal;
                    _healthRegenTimer = 0f;

                    _camera.Position = new Vector3(0f, 1.6f, 0f);
                    verticalSpeed = 0f;
                    isOnGround = true;
                    _player = new Player(100);
                    _lavaTimer = 0f;

                }
                return;
            }
            Vector3 playerSize = new Vector3(0.5f, 1.8f, 0.5f);
            float eyeHeight = 1.6f;
            float moveSpeed = 4f * (float)e.Time;

            var lavaBlock = physicalBlocks[0];

            bool isInLava = _camera.Position.Y - eyeHeight < lavaBlock.Position.Y + lavaBlock.Size.Y + 0.5f;

            if (isInLava)
            {
                _lavaTimer += (float)e.Time;
                if (_lavaTimer >= 2.5f)
                {
                    _player.TakeDmg(25);
                    _healthRegenTimer = 0f;

                    _damageSound?.Play();
                    float shakeAmount = 20f;
                    _camera.Yaw += (float)((random.NextDouble() - 0.5) * shakeAmount);
                    _camera.Pitch += (float)((random.NextDouble() - 0.5) * shakeAmount);
                    _camera.UpdateVectors();

                    System.Console.WriteLine($"Lava damage! Health: {_player.health}");
                    _lavaTimer = 0f;

                    if (_player.health <= 0)
                    {
                        isGameOver = true;
                        gameOverTimer = 0f;
                        _damageSound?.Play();
                        System.Console.WriteLine("GAME OVER!");
                        return;
                    }
                }
            }
            else
            {
                _lavaTimer = 0f;
            }

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



            if (KeyboardState.IsKeyDown(Keys.S)) 
                move -= forward;
            if (KeyboardState.IsKeyDown(Keys.A)) 
                move -= right;
            if (KeyboardState.IsKeyDown(Keys.D)) 
                move += right;
            if (KeyboardState.IsKeyPressed(Keys.E))
            {
                TryBreakCoal();
            }
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
                _lastFallSpeed = verticalSpeed;
            }
            else
            {
                if (verticalSpeed < 0)
                {
                    _camera.Position.Y = collidedBlock.Position.Y + collidedBlock.Size.Y + eyeHeight;

                    if (_lastFallSpeed < -_fallDamageThreshold && !wasOnGround)
                    {
                        int fallDamage = (int)((-_lastFallSpeed - _fallDamageThreshold) * 2);
                        _player.TakeDmg(fallDamage);
                        _landSound?.Play();
                        _healthRegenTimer = 0f;

                        _damageSound?.Play();
                        float shakeAmount = 20f;
                        _camera.Yaw += (float)((random.NextDouble() - 0.5) * shakeAmount);
                        _camera.Pitch += (float)((random.NextDouble() - 0.5) * shakeAmount);
                        _camera.UpdateVectors();
                        System.Console.WriteLine($"Fall damage: {fallDamage}! Health: {_player.health}");

                        if (_player.health <= 0)
                        {
                            isGameOver = true;
                            gameOverTimer = 0f;
                            _damageSound?.Play();
                            System.Console.WriteLine("GAME OVER!");
                            return;
                        }
                    }


                    wasOnGround = isOnGround;
                    isOnGround = true;
                }
                else
                {
                    _camera.Position.Y = collidedBlock.Position.Y - (playerSize.Y - eyeHeight);
                    wasOnGround = isOnGround;
                }
                verticalSpeed = 0f;
                _lastFallSpeed = 0f;
            }

            if (_camera.Position.Y - eyeHeight < lavaBlock.Position.Y + lavaBlock.Size.Y)
            {
                _camera.Position = new Vector3(0f, eyeHeight, 0f);
                verticalSpeed = 0f;
                isOnGround = true;
                _lavaTimer = 0f;
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

        private void UpdateFireParticles(FrameEventArgs e)
        {
            float deltaTime = (float)e.Time;

            for (int i = 0; i < 50; i++)
            {
                if (random.NextDouble() < deltaTime)
                {
                    fireParticles.Add(new FireParticle
                    {
                        Position = new Vector3(
                            (float)(random.NextDouble() * 40 - 20), -1.9f, (float)(random.NextDouble() * 40 - 20)),
                        Velocity = new Vector3(
                            (float)(random.NextDouble() * 0.5 - 0.25),
                            (float)(random.NextDouble() * 2 + 1),
                            (float)(random.NextDouble() * 0.5 - 0.25)
                        ),
                        Life = 1f,
                        Size = (float)(random.NextDouble() * 0.3 + 0.2)
                    });
                }
            }

            for (int i = fireParticles.Count - 1; i >= 0; i--)
            {
                var p = fireParticles[i];

                p.Position += p.Velocity * deltaTime;
                p.Velocity.Y -= 0.5f * deltaTime;
                p.Life -= deltaTime;

                if (p.Life <= 0)
                    fireParticles.RemoveAt(i);
            }
        }

        private void RenderFireParticles()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

            foreach (var p in fireParticles)
            {
                float alpha = p.Life;
                Vector4 color;




                if (p.Life > 0.7f)
                    color = new Vector4(1f, 1f, 0.3f, alpha);
                else if (p.Life > 0.3f)
                    color = new Vector4(1f, 0.5f, 0f, alpha);
                else
                    color = new Vector4(1f, 0f, 0f, alpha);

                Matrix4 modelMatrix =
                    Matrix4.CreateScale(p.Size) *
                    Matrix4.CreateTranslation(p.Position);

                _shader.SetMatrix4("model", modelMatrix);
                _shader.SetBool("useTex", false);
                _shader.SetVector4("color", color);

                GL.BindVertexArray(_vao);
                GL.DrawArrays(PrimitiveType.Triangles, 6, 6);
            }

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();

            if (isPlaying)
            {
                if (isGameOver)
                    RenderGameOver();
                else
                    RenderGame();
            }
            else
                RenderMenu();



            SwapBuffers();
        }
        private void RenderGameOver()
        {
            RenderGame();

            GL.Disable(EnableCap.DepthTest);
            _shader.Use();
            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetMatrix4("view", Matrix4.Identity);
            _shader.SetMatrix4("projection", Matrix4.Identity);
            _shader.SetBool("useTex", false);

            float[] overlay = {
                -1f, -1f, 0, 0, 0,
                 1f, -1f, 0, 0, 0,
                 1f,  1f, 0, 0, 0,
                 1f,  1f, 0, 0, 0,
                 -1f,  1f, 0, 0, 0,
                 -1f, -1f, 0, 0, 0};

            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, overlay.Length * sizeof(float), overlay, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _shader.SetVector4("color", new Vector4(0.5f, 0.5f, 0.5f, 0.6f));
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.Enable(EnableCap.DepthTest);
        }
        private void RenderGame()
        {
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // NU apela RenderCoalParticles() aici!
            // RenderCoalParticles(); // ȘTERGE ACEASTĂ LINIE

            _shader.Use();

            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4(
                "projection",
                Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(60f),
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

                    if (_stoneTexture != null)
                    {
                        _shader.SetBool("useTex", true);
                        GL.ActiveTexture(TextureUnit.Texture0);
                        _stoneTexture.Use();
                    }
                    else
                    {
                        _shader.SetBool("useTex", false);
                        _shader.SetVector4("color", new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    }

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

            if (lavamoon != null)
            {
                for (int i = 0; i < lavamoonPositions.Count; i++)
                {
                    var pos = lavamoonPositions[i];

                    Matrix4 modelMatrix =
                        Matrix4.CreateScale(1f) *
                        Matrix4.CreateRotationY(_lavamoonRotation) *
                        Matrix4.CreateTranslation(pos);

                    _shader.SetMatrix4("model", modelMatrix);
                    _shader.SetBool("useTex", true);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    _lavamoonTexture?.Use();

                    GL.BindVertexArray(lavamoon.VAO);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, lavamoon.Vertices.Length / 5);
                }
            }

            // Randează coal blocks
            if (_coal.Count > 0)
            {
                float playerReach = 3f;
                Vector3 playerPos = _camera.Position - new Vector3(0, 1.6f, 0);

                for (int i = 0; i < _coal.Count; i++)
                {
                    var coal = _coal[i];
                    var pos = _coalPositions[i];

                    bool isNear = (pos - playerPos).Length < playerReach;
                    float scale = isNear ? 1.6f : 1.5f;

                    Matrix4 modelMatrix = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(pos);
                    _shader.SetMatrix4("model", modelMatrix);
                    _shader.SetBool("useTex", true);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    _coalTexture?.Use();
                    GL.BindVertexArray(coal.VAO);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, coal.Vertices.Length / 5);
                }
            }

            RenderCoalParticles(); 
            RenderFireParticles();
            RenderHealthBar();
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
            if (_controlsTexture != null)
            {
                _shader.SetBool("useTex", true);
                _controlsTexture.Use();

                float scaleX = 0.8f; 
                float scaleY = 1.5f; 
                float posX = -0.6f; 
                float posY = -1f; 

                Matrix4 modelMatrix =
                    Matrix4.CreateScale(scaleX, scaleY, 1f) *
                    Matrix4.CreateTranslation(posX, posY, 0f);

                _shader.SetMatrix4("model", modelMatrix);

                GL.BindVertexArray(_vao);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            }
        }
        private void RenderHealthBar()
        {
            GL.Disable(EnableCap.DepthTest);

            _shader.Use();
            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetMatrix4("view", Matrix4.Identity);
            _shader.SetMatrix4("projection", Matrix4.Identity);
            _shader.SetBool("useTex", false);

            float[] healthBarBg = {
                -0.95f, 0.85f, 0,  0, 0,
                -0.35f, 0.85f, 0,  0, 0,
                -0.35f, 0.95f, 0,  0, 0,
                -0.35f, 0.95f, 0,  0, 0,
                -0.95f, 0.95f, 0,  0, 0,
                -0.95f, 0.85f, 0,  0, 0
    };

            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, healthBarBg.Length * sizeof(float),
                          healthBarBg, BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _shader.SetVector4("color", new Vector4(0.3f, 0, 0, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            float healthPercent = _player.health / 100f;
            float barWidth = 0.6f * healthPercent;

            float[] healthBarFill = {
                -0.95f, 0.85f, 0,  0, 0,
                -0.95f + barWidth, 0.85f, 0,  0, 0,
                -0.95f + barWidth, 0.95f, 0,  0, 0,
                -0.95f + barWidth, 0.95f, 0,  0, 0,
                -0.95f, 0.95f, 0,  0, 0,
                -0.95f, 0.85f, 0,  0, 0
    };

            GL.BufferData(BufferTarget.ArrayBuffer, healthBarFill.Length * sizeof(float),
                          healthBarFill, BufferUsageHint.DynamicDraw);

            Vector4 healthColor;
            if (healthPercent > 0.6f)
                healthColor = new Vector4(0, 0.8f, 0, 1);
            else if (healthPercent > 0.3f)
                healthColor = new Vector4(0.9f, 0.9f, 0, 1);
            else
                healthColor = new Vector4(0.9f, 0, 0, 1);

            _shader.SetVector4("color", healthColor);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            float[] healthBarOutline = {
                -0.95f, 0.85f, 0,  0, 0,
                 -0.35f, 0.85f, 0,  0, 0,
                  -0.35f, 0.95f, 0,  0, 0,
                -0.95f, 0.95f, 0,  0, 0
    };

            GL.BufferData(BufferTarget.ArrayBuffer, healthBarOutline.Length * sizeof(float),
                          healthBarOutline, BufferUsageHint.DynamicDraw);

            _shader.SetVector4("color", new Vector4(1, 1, 1, 1));
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);

            GL.Enable(EnableCap.DepthTest);
        }
        private void SpawnRandomCoal()
        {
            try
            {
                Model coalModel;
                if (_coal.Count > 0)
                    coalModel = _coal[0];
                else
                {
                    coalModel = new Model("models/coal.dae");
                    _coal.Add(coalModel);
                }

                float x = (float)(random.NextDouble() * 40 - 20);
                float z = (float)(random.NextDouble() * 40 - 20);
                float y = -0.5f;

                Vector3 pos = new Vector3(x, y, z);

                _coal.Add(coalModel);
                _coalPositions.Add(pos);

                Vector3 coalSize = new Vector3(1.5f, 1.5f, 1.5f);
                Vector3 collisionPos = pos - new Vector3(coalSize.X / 2, 0, coalSize.Z / 2);
                physicalBlocks.Add(new Block(collisionPos, coalSize));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Failed to spawn coal: {ex.Message}");
            }
        }

        private class CoalParticle
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float Life;
            public float Size;
        }

        private List<CoalParticle> coalParticles = new List<CoalParticle>();

        private void TryBreakCoal()
        {
            float playerReach = 3f;
            Vector3 playerPos = _camera.Position - new Vector3(0, 1.6f, 0);

            for (int i = _coalPositions.Count - 1; i >= 0; i--)
            {
                Vector3 coalPos = _coalPositions[i];
                if ((coalPos - playerPos).Length < playerReach)
                {
                    _coalPositions.RemoveAt(i);
                    _coal.RemoveAt(i);

                    Vector3 coalSize = new Vector3(1.5f, 1.5f, 1.5f);
                    Vector3 collisionPos = coalPos - new Vector3(coalSize.X / 2, 0, coalSize.Z / 2);
                    Block toRemove = physicalBlocks.FirstOrDefault(b =>(b.Position - collisionPos).Length < 1f);

                    if (toRemove != null)
                        physicalBlocks.Remove(toRemove);

                    SpawnCoalParticles(coalPos);
                    _player.health = Math.Min(_player.health + 20, 100);
                    _damageSound?.Play();
                    break;
                }
            }
        }

        private void SpawnCoalParticles(Vector3 position)
        {
            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                coalParticles.Add(new CoalParticle
                {
                    Position = position,
                    Velocity = new Vector3(
                        (float)(rand.NextDouble() * 2 - 1),
                        (float)(rand.NextDouble() * 2),
                        (float)(rand.NextDouble() * 2 - 1)
                    ),
                    Life = 1f,
                    Size = (float)(rand.NextDouble() * 0.3 + 0.2)
                });
            }
        }

        private void UpdateCoalParticles(FrameEventArgs e)
        {
            float deltaTime = (float)e.Time;
            for (int i = coalParticles.Count - 1; i >= 0; i--)
            {
                var p = coalParticles[i];
                p.Position += p.Velocity * deltaTime;
                p.Velocity.Y -= 0.5f * deltaTime;
                p.Life -= deltaTime;
                if (p.Life <= 0)
                    coalParticles.RemoveAt(i);
            }
        }

        private void RenderCoalParticles()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

            foreach (var p in coalParticles)
            {
                float alpha = p.Life;
                Vector4 color = new Vector4(0.2f, 0.2f, 0.2f, alpha);

                Matrix4 modelMatrix = Matrix4.CreateScale(p.Size) *
                                      Matrix4.CreateTranslation(p.Position);

                _shader.SetMatrix4("model", modelMatrix);
                _shader.SetBool("useTex", false);
                _shader.SetVector4("color", color);

                GL.BindVertexArray(_vao);
                GL.DrawArrays(PrimitiveType.Triangles, 6, 6);
            }

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }




    }

}

