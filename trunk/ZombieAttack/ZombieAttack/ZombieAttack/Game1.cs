using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

/* Stuff to do 
 * zombies to go towards camera/player position /update player position - jake
 * zombies need to rotate in direction they're heading - nick
 * gun in view - bebe
 * bullet collision detection - nick
 * gun animation - nick
 * fix collision detection for zombies/trees/player- jake
 * put health packs and ammo on ground **not necessarily for alpha
 * make bigger/better level - Jake  
 * skybox - bebe
 * win/lose/start screens -bebe
 * fix rotation and movement code for the player-nick
 * *************************************************
 * For final
 * music - nick
 * muzzle flash
 * blood effects?
 * collision detection
 * more guns?
 * Intermission screens for betweens levels
 * maybe high score
*/

namespace ZombieAttack
{
    //the various states the game can be in
    public enum GameState { Loading, Pause, Running, Won, Lost, Start, newLevel }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //new variables
        KeyboardState lastKeyboardState = new KeyboardState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        GamePadState currentGamePadState = new GamePadState();
        int zombiesKilled;
        int killTotal = 10;
        int level = 2;
        TimeSpan startTime, roundTimer, roundTime;
        Random random;
        SpriteBatch spriteBatch;
        SpriteFont statsFont;
        GameState currentGameState = GameState.Loading;

        GameObject ground;
        Camera gameCamera;
        
        int GAME_SCORE;

        Player player;
        Zombie[] zombies;
        Tree[] trees;
        Guns guns;
        List<Ray> rayList;
        ParticleSystem explosionParticles;


        //old variables
        Matrix view;
        Matrix proj;

        Model zombie;
        Model skyboxModel;
        GameObject brick_wall;
        Texture2D startGame;
        Texture2D gameOver;
        Texture2D winState;
        Texture2D NEWLEVEL;
        Texture2D pause_screen;
        Texture2D health_bar;
        Matrix[] skyboxTransforms;
        GameObject handgun;
        GameObject shotgun;

        double lastShotTime;
        
        //Matrix[] trees;

        SoundEffectInstance background_music;
        SoundEffectInstance gun_sound;

        Texture2D blackTexture;
        Texture2D grassTexture;
        Texture2D brickTexture;

        // Set the avatar position and rotation variables.
        Vector3 avatarPosition = new Vector3(0, 2, -50);
        Vector3 gunPosition = new Vector3(0, 0, -450);
        Vector3 cameraLookat;
        float avatarYaw;
        float avatarPitch;
        int oldMouseY = Mouse.GetState().Y;
        int oldMouseX = Mouse.GetState().X;
        float mouseSensitivity = 0.01F;

     
        // Set the direction the camera points without rotation.
        Vector3 cameraReference = new Vector3(0, 0, 1);

        // Set rates in world units per 1/60th second (the default fixed-step interval).
        float rotationSpeed = 1f / 60f;
        float forwardSpeed = 100f / 60f;

        // Set field of view of the camera in radians (pi/4 is 45 degrees).
        static float viewAngle = MathHelper.PiOver4;

        // Set distance from the camera of the near and far clipping planes.
        static float nearClip = 1.0f;
        static float farClip = 2000.0f;

        GraphicsDeviceManager graphics;
        GraphicsDevice device;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            roundTime = GlobalState.ROUND_TIME;
            random = new Random();

            this.graphics.PreferredBackBufferWidth = 800;
            this.graphics.PreferredBackBufferHeight = 600;

            this.graphics.IsFullScreen = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            int centerX = Window.ClientBounds.Width / 2;
            int centerY = Window.ClientBounds.Height / 2;
            //
            Mouse.SetPosition(centerX, centerY);

            lastShotTime = 0f;
            GAME_SCORE = 0;
           

            ground = new GameObject();
            //ground.position = new Vector3(ground.position.X, -25, ground.position.Z);
            guns = new Guns();
            shotgun = new GameObject();
            handgun = new GameObject();
            brick_wall = new GameObject();
            gameCamera = new Camera();
            currentGameState = GameState.Start;
            rayList = new List<Ray>();
            explosionParticles = new ExplosionParticleSystem(this, Content);
            Components.Add(explosionParticles);
            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            device = graphics.GraphicsDevice;
            zombie = Content.Load<Model>("Models/Zombie");
            shotgun.model = Content.Load<Model>("Models/Shotgun");
            handgun.model = Content.Load<Model>("Models/box");

            //Guns
            guns.LoadContent(Content, "Models/Rifle2", "Models/box", "Textures/muzzleflash");
            //guns.LoadContent(Content, "Textures/muzzleflash");
            //tree = Content.Load<Model>("Models/firtree1");
            grassTexture = Content.Load<Texture2D>("Textures/grass");
            blackTexture = Content.Load<Texture2D>("Textures/grass");
            brickTexture = Content.Load<Texture2D>("Textures/brick_wall_tex");
            health_bar = Content.Load<Texture2D>("Textures/health_bar");
            //Game Screens
            startGame = Content.Load<Texture2D>("Models/StartGame");
            gameOver = Content.Load<Texture2D>("Models/GameOver");
            winState = Content.Load<Texture2D>("Models/winState");
            NEWLEVEL = Content.Load<Texture2D>("Models/NEWLEVEL");
            pause_screen = Content.Load<Texture2D>("Models/Pause_Screen");
            //Skybox
            skyboxModel = Content.Load<Model>("Textures/skybox2");
            skyboxTransforms = new Matrix[skyboxModel.Bones.Count];
            //new code
            ground.model = Content.Load<Model>("Models/box");
            brick_wall.model = Content.Load<Model>("Models/box");
            statsFont = Content.Load<SpriteFont>("Fonts/StatsFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Initialize zombies
            zombies = new Zombie[killTotal];
            for (int index = 0; index < zombies.Length; index++)
            {
                zombies[index] = new Zombie();
                zombies[index].LoadContent(Content, "Models/Zombie");
            }

            //initialize trees
            trees = new Tree[GlobalState.NUM_TREES];

            for (int index = 0; index < trees.Length; index++)
            {
                trees[index] = new Tree();
                trees[index].LoadContent(Content, "Models/firtree1");

            }

            

            //initialize player
            player = new Player();
            player.LoadContent(Content, "Models/box");

            //Function for placing trees and zombies
            PlaceTreesAndZombies();
            // TODO: use this.Content to load your game content here

            SoundEffect soundEffect;
            
            soundEffect = Content.Load<SoundEffect>("audio/backgroundmusic");
            background_music = soundEffect.CreateInstance();
            soundEffect = Content.Load<SoundEffect>("audio/gunsound");
            gun_sound = soundEffect.CreateInstance();
            gun_sound.Volume = GlobalState.GUN_SOUND_VOLUME;
            background_music.IsLooped = true;
            background_music.Volume = GlobalState.BACKGROUND_MUSIC_VOLUME;
            
        }

        private void PlaceTreesAndZombies()
        {
            int min = GlobalState.MIN_DISTANCE;
            int max = GlobalState.MAX_DISTANCE;

            //place zombies
            foreach (Zombie zombie in zombies)
            {
                zombie.position = GenerateRandomPosition(min, max);
                zombie.Killed = false;
            }

            //place trees
            foreach (Tree tree in trees)
            {
                tree.position = GenerateRandomPosition(min, max);
            }
        }

        //generates a random position to be used for trees and zombies
        private Vector3 GenerateRandomPosition(int min, int max)
        {
            int xValue, zValue;
            do
            {
                xValue = random.Next(min, max);
                zValue = random.Next(min, max);
                if (random.Next(100) % 2 == 0)
                    xValue *= -1;
                if (random.Next(100) % 2 == 0)
                    zValue *= -1;

            } while (IsOccupied(xValue, zValue));

            return new Vector3(xValue, -1, zValue);
        }

        //checks during random generation to see if position is occupied
        private bool IsOccupied(int xValue, int zValue)
        {
            foreach (GameObject currentObj in trees)
            {
                if (((int)(MathHelper.Distance(
                    xValue, currentObj.position.X)) < 10) &&
                    ((int)(MathHelper.Distance(
                    zValue, currentObj.position.Z)) < 10))
                    return true;
            }

            foreach (GameObject currentObj in zombies)
            {
                if (((int)(MathHelper.Distance(
                    xValue, currentObj.position.X)) < 15) &&
                    ((int)(MathHelper.Distance(
                    zValue, currentObj.position.Z)) < 15))
                    return true;
            }
            return false;
        }

        private void checkBulletHit()
        {
            SortedList<float,Zombie> hitList = new SortedList<float,Zombie>();
            Random random = new Random();

            Ray bulletPath = new Ray(player.position, Vector3.Transform(new Vector3(0,0,1.0f),Matrix.CreateRotationY(player.forward_direction)));
            rayList.Add(bulletPath);
            foreach (Zombie zombie in zombies)
            {
                Nullable<float> distance = bulletPath.Intersects(new BoundingSphere(zombie.position, 5.0f));
                if(distance != null && !zombie.Killed)
                   hitList.Add(distance.Value + (float)random.NextDouble()/100.0f ,zombie);
                
            }

            if (hitList.Count > 0)
            {
                Vector3 pos = hitList.First().Value.position;
                hitList.First().Value.Killed = true;
                for (int i = 0; i < GlobalState.NUM_PARTICLES; i++)
                    explosionParticles.AddParticle(pos, new Vector3(1.0f, 1.0f, 1.0f));
                GAME_SCORE += hitList.Count * 10 * player.health;
            }

        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //UpdateAvatarPosition();
            //UpdateCamera();
            // TODO: Add your update logic here
            //NEW CODE
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();
            lastGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            double currentTime = gameTime.TotalGameTime.TotalSeconds;
            double elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
            explosionParticles.SetCamera(gameCamera.view_matrix, gameCamera.projection_matrix);

            // Allows the game to exit
            if ((currentKeyboardState.IsKeyDown(Keys.Escape)) ||
                (currentGamePadState.Buttons.Back == ButtonState.Pressed))
                this.Exit();
            //game is currently loading
            if (currentGameState == GameState.Loading)
            {
                if ((lastKeyboardState.IsKeyDown(Keys.Enter) &&
                    (currentKeyboardState.IsKeyUp(Keys.Enter))) ||
                    currentGamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    roundTimer = roundTime;
                    currentGameState = GameState.Running;
                }
            }

            if (currentGameState == GameState.Start)
            {
                if ((lastKeyboardState.IsKeyDown(Keys.Enter) &&
                    (currentKeyboardState.IsKeyUp(Keys.Enter))) ||
                    currentGamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    background_music.Play();
                    currentGameState = GameState.Running;
                }
            }

            if (currentGameState == GameState.Pause)
            {
                if ((lastKeyboardState.IsKeyDown(Keys.Enter) && currentKeyboardState.IsKeyUp(Keys.Enter)))
                    currentGameState = GameState.Running;
            }

            //game is currently running
            if ((currentGameState == GameState.Running))
            {
                player.Update(currentGamePadState, currentKeyboardState, trees, zombies, elapsedTime);
                gameCamera.Update(player.forward_direction, player.position, aspectRatio);
                background_music.Play();
                if (currentMouseState.LeftButton == ButtonState.Pressed  && (currentTime > lastShotTime + 0.5))
                {
                    gun_sound.Play();
                    guns.shoot();
                    checkBulletHit();
                    lastShotTime = currentTime;

                }
                zombiesKilled = 0;
                foreach (Zombie zombie in zombies)
                {
                    if (zombie.Killed)
                        zombiesKilled++;
                    else zombie.Update(player.position);
                    //check if player has reached a new level

                    if (zombiesKilled == killTotal)
                    {
                        currentGameState = GameState.newLevel;
                        background_music.Stop();
                    }
                    
                }
                //check if game is won
                if (killTotal == GlobalState.WIN_NUM_ZOMBIES)
                {
                    currentGameState = GameState.Won;
                    background_music.Stop();
                }
                roundTimer -= gameTime.ElapsedGameTime;

                //check if game is lost
                if (player.health == 0)
                {
                    currentGameState = GameState.Lost;
                    background_music.Stop();
                }

                else if (currentKeyboardState.IsKeyDown(Keys.Space))
                    currentGameState = GameState.Pause;
            }

            if ((currentGameState == GameState.Won) ||
                (currentGameState == GameState.Lost))
            {
                // Reset the world for a new game
                if ((lastKeyboardState.IsKeyDown(Keys.Enter) &&
                    (currentKeyboardState.IsKeyUp(Keys.Enter))) ||
                    currentGamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    killTotal = 10;
                    GAME_SCORE = 0;
                    zombies = new Zombie[killTotal];
                    for (int index = 0; index < zombies.Length; index++)
                    {
                        zombies[index] = new Zombie();
                        zombies[index].LoadContent(Content, "Models/Zombie");
                    }
                    level = 2;
                    ResetGame(gameTime, aspectRatio);

                }
            }

            if (currentGameState == GameState.newLevel)
            {
                if ((lastKeyboardState.IsKeyDown(Keys.Enter) &&
                   (currentKeyboardState.IsKeyUp(Keys.Enter))) ||
                   currentGamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    killTotal += 10;
                    zombies = new Zombie[killTotal];
                    for (int index = 0; index < zombies.Length; index++)
                    {
                        zombies[index] = new Zombie();
                        zombies[index].LoadContent(Content, "Models/Zombie");
                    }
                    ResetGame(gameTime, aspectRatio);
                    level = 3;
                    
                }
            }
            base.Update(gameTime);
        }
         

        private void ResetGame(GameTime gameTime, float aspectRatio)
        {
            player.Reset();
            gameCamera.Update(player.forward_direction,
                player.position, aspectRatio);
            PlaceTreesAndZombies();

            zombiesKilled = 0;
            startTime = gameTime.TotalGameTime;
            roundTimer = roundTime;
            currentGameState = GameState.Running;
        }

        /// <summary>
        /// Updates the position and direction of the avatar.
        /// </summary>
        void UpdateAvatarPosition()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();
            if (keyboardState.IsKeyDown(Keys.Left) || (currentState.DPad.Left == ButtonState.Pressed))
            {
                // Rotate left.
                avatarYaw += rotationSpeed;
            }
            if (keyboardState.IsKeyDown(Keys.Right) || (currentState.DPad.Right == ButtonState.Pressed))
            {
                // Rotate right.
                avatarYaw -= rotationSpeed;
            }
            if (keyboardState.IsKeyDown(Keys.Up) || (currentState.DPad.Up == ButtonState.Pressed) || (keyboardState.IsKeyDown(Keys.W)))
            {
                Matrix forwardMovement = Matrix.CreateRotationY(avatarYaw);
                Vector3 v = new Vector3(0, 0, forwardSpeed);
                v = Vector3.Transform(v, forwardMovement);
                avatarPosition.Z += v.Z;
                avatarPosition.X += v.X;
            }
            if (keyboardState.IsKeyDown(Keys.Down) || (currentState.DPad.Down == ButtonState.Pressed) || (keyboardState.IsKeyDown(Keys.S)))
            {
                Matrix forwardMovement = Matrix.CreateRotationY(avatarYaw);
                Vector3 v = new Vector3(0, 0, -forwardSpeed);
                v = Vector3.Transform(v, forwardMovement);
                avatarPosition.Z += v.Z;
                avatarPosition.X += v.X;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                Matrix movement = Matrix.CreateRotationY(avatarYaw);
                Vector3 v = new Vector3(forwardSpeed, 0, 0);
                v = Vector3.Transform(v, movement);
                avatarPosition.Z += v.Z;
                avatarPosition.X += v.X;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                Matrix movement = Matrix.CreateRotationY(avatarYaw);
                Vector3 v = new Vector3(-forwardSpeed, 0, 0);
                v = Vector3.Transform(v, movement);
                avatarPosition.Z += v.Z;
                avatarPosition.X += v.X;
            }

            
            //update view angle from mouse state
            {

                int centerX = Window.ClientBounds.Width / 2;
                int centerY = Window.ClientBounds.Height / 2;

                avatarYaw -= (mouseState.X - centerX) * mouseSensitivity;
                
                avatarPitch -= (mouseState.Y - centerY) * mouseSensitivity;
                              
                Mouse.SetPosition(centerX, centerY);

            }
        }

        /// <summary>
        /// Updates the position and direction of the camera relative to the avatar.
        /// </summary>
        void UpdateCamera()
        {
            // Calculate the camera's current position.
            Vector3 cameraPosition = avatarPosition;

            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(avatarYaw, -avatarPitch, 0);
            //Matrix rotationMatrix = Matrix.CreateRotationY(avatarYaw);

            // Create a vector pointing the direction the camera is facing.
            Vector3 transformedReference = Vector3.Transform(cameraReference, rotationMatrix);

            // Calculate the position the camera is looking at.
            cameraLookat = cameraPosition + transformedReference;

            
            // Set up the view matrix and projection matrix.
            view = Matrix.CreateLookAt(cameraPosition, cameraLookat, new Vector3(0.0f, 1.0f, 0.0f));

            Viewport viewport = graphics.GraphicsDevice.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            proj = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearClip, farClip);
        }

        /// <summary>
        /// Draws the box model; a reference point for the avatar.
        /// </summary>
        void DrawModel(Model model, Matrix world, Texture2D texture)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.Projection = proj;
                    be.View = view;
                    be.World = world * Matrix.CreateTranslation(new Vector3(0, -60, 0)) * Matrix.CreateScale(100.0f, 1.0f, 100.0f);
                    be.Texture = texture;
                    be.TextureEnabled = true;
                }
                mesh.Draw();
            }
        }
        //If you don't have to draw on a texture for the model, use this.
        void SimpleDrawModel(Model model, Matrix world)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.Projection = proj;
                    be.View = view;
                    be.World = world;
                }
                mesh.Draw();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.SteelBlue);

            //DrawModel(handgun, Matrix.Identity, blackTexture);

            //DrawModel(handgun, Matrix.Identity, blackTexture);
            //SimpleDrawModel(zombie, Matrix.Identity);
           
            switch (currentGameState)
            {
                case GameState.Loading:
                    break;
                case GameState.Running:
                    DrawGameplayScreen();
                    break;
                case GameState.Pause:
                    DrawPauseScreen();
                    break;
                case GameState.Won:
                    DrawWinScreen(GlobalState.STR_WIN);
                    break;
                case GameState.Lost:
                    DrawLossScreen(GlobalState.STR_LOSE);
                    break;
                case GameState.Start:
                    DrawStartScreen();
                    break;
                 case GameState.newLevel:
                    DrawNewLevelScreen(level);   
                    break; 
            };

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void DrawTerrain(Model model, Texture2D texture)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    Matrix[] transforms = new Matrix[model.Bones.Count];
                    model.CopyAbsoluteBoneTransformsTo(transforms);
                    Matrix world_matrix = Matrix.Identity;


                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(60.0f, 0.1f, 60.0f)
                        * world_matrix * Matrix.CreateTranslation(0.0f, -5.0f, 0.0f);

                    //apply texture to the ground
                    effect.Texture = texture;
                    effect.TextureEnabled = true;

                    // Use the matrices provided by the game camera
                    effect.View = gameCamera.view_matrix;
                    effect.Projection = gameCamera.projection_matrix;
                }
                mesh.Draw();
            }
        }

        private void DrawPauseScreen()
        {
            float xOffsetText, yOffsetText;
            Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Vector2 strCenter;


            xOffsetText = yOffsetText = 0;
            Vector2 strResult = statsFont.MeasureString("You're Paused");
            Vector2 strPlayAgainSize =
                statsFont.MeasureString(GlobalState.STR_PAUSE);
            Vector2 strPosition;
            strCenter = new Vector2(strResult.X / 2, strResult.Y / 2);

            yOffsetText = (viewportSize.Y * 0.85f - strCenter.Y);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            spriteBatch.Begin();
            spriteBatch.Draw(pause_screen, Vector2.Zero, null, Color.White);
            //spriteBatch.DrawString(statsFont, gameResult,strPosition, Color.Red);

            strCenter =
                new Vector2(strPlayAgainSize.X / 2, strPlayAgainSize.Y / 2);
            yOffsetText = (viewportSize.Y * 0.9f - strCenter.Y) +
                (float)statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);
            spriteBatch.DrawString(statsFont, GlobalState.STR_PAUSE, strPosition, Color.Red);

            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement

            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;

            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.AlphaTestEnable = false;

            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }

        private void DrawWalls(Model model, Texture2D texture)
        {
            //Draw one wall
            
            /*float original_size = 2;
            float scalingFactor = GlobalState.WALL_LENGTH / original_size;
            model.Root.Transform = model.Root.Transform * Matrix.CreateScale(scalingFactor);
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            */
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            for (int i = 0; i < 4; i++)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                        
                        Matrix world_matrix = Matrix.Identity;


                        effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(GlobalState.WALL_SCALEX, 0.7f, 0.1f)
                            * world_matrix * Matrix.CreateTranslation(-GlobalState.WALL_SHIFT + i * GlobalState.WALL_LENGTH, 0.0f, 405.0f);

                        //apply texture to the wall
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                        // Use the matrices provided by the game camera
                        effect.View = gameCamera.view_matrix;
                        effect.Projection = gameCamera.projection_matrix;

                    }
                    mesh.Draw();
                }
            }
            //draw the second wall
            for (int i = 0; i < 4; i++)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;

                        Matrix world_matrix = Matrix.Identity;


                        effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(GlobalState.WALL_SCALEX, 0.7f, 0.1f)
                            * world_matrix * Matrix.CreateTranslation(-GlobalState.WALL_SHIFT + i * GlobalState.WALL_LENGTH, 0.0f, -405.0f);

                        //apply texture to the wall
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                        // Use the matrices provided by the game camera
                        effect.View = gameCamera.view_matrix;
                        effect.Projection = gameCamera.projection_matrix;

                    }
                    mesh.Draw();
                }
            }
            //the third and fourth walls must be rotated
            for (int i = 0; i < 4; i++)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;

                        Matrix world_matrix = Matrix.Identity;
                        Matrix rotation_matrix = Matrix.CreateRotationY(MathHelper.ToRadians(90));

                        effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(GlobalState.WALL_SCALEX, 0.7f, 0.1f)
                            * world_matrix * rotation_matrix * Matrix.CreateTranslation(-405.0f, 0.0f, -GlobalState.WALL_SHIFT + i * GlobalState.WALL_LENGTH);

                        //apply texture to the wall
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                        // Use the matrices provided by the game camera
                        effect.View = gameCamera.view_matrix;
                        effect.Projection = gameCamera.projection_matrix;

                    }
                    mesh.Draw();
                }
            }
            //draw the 4th wall
            for (int i = 0; i < 4; i++)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;

                        Matrix world_matrix = Matrix.Identity;
                        Matrix rotation_matrix = Matrix.CreateRotationY(MathHelper.ToRadians(90));

                        effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(GlobalState.WALL_SCALEX, 0.7f, 0.1f)
                            * world_matrix * rotation_matrix * Matrix.CreateTranslation(405.0f, 0.0f, -GlobalState.WALL_SHIFT + i * GlobalState.WALL_LENGTH);

                        //apply texture to the wall
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                        // Use the matrices provided by the game camera
                        effect.View = gameCamera.view_matrix;
                        effect.Projection = gameCamera.projection_matrix;

                    }
                    mesh.Draw();
                }
            }
            
        }

        private void DrawStartScreen()
        {
            float xOffsetText, yOffsetText;
            Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Vector2 strCenter;
            string start_str = "Press Enter to start the game!";
            xOffsetText = yOffsetText = 0;
            Vector2 strResult = statsFont.MeasureString(start_str);
            Vector2 strPlayAgainSize =
                statsFont.MeasureString(GlobalState.STR_RESTART);
            Vector2 strPosition;
            strCenter = new Vector2(strResult.X / 2, strResult.Y / 2);

            yOffsetText = (viewportSize.Y *0.83f  - strCenter.Y);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            spriteBatch.Begin();

            spriteBatch.Draw(startGame,Vector2.Zero, null, Color.White); 
            spriteBatch.DrawString(statsFont, start_str,
                strPosition, Color.Red);

            strCenter =
                new Vector2(strPlayAgainSize.X / 2, strPlayAgainSize.Y / 2);
            yOffsetText = (viewportSize.Y * 0.85f - strCenter.Y) +
                (float)statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            //spriteBatch.DrawString(statsFont, GlobalState.STR_RESTART,strPosition, Color.Red);

            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement

            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;

            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.AlphaTestEnable = false;

            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }
        private void DrawNewLevelScreen(int level)
        {
            string current_level = "LEVEL " + level.ToString();
            float xOffsetText, yOffsetText;
            Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Vector2 strCenter;

            xOffsetText = yOffsetText = 0;
            Vector2 strResult = statsFont.MeasureString(current_level);
            Vector2 strPlayAgainSize =
                statsFont.MeasureString(GlobalState.STR_CONTINUE);
            Vector2 strPosition;
            strCenter = new Vector2(strResult.X / 2, strResult.Y / 2);

            yOffsetText = (viewportSize.Y / 2 - strCenter.Y);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            spriteBatch.Begin();
            spriteBatch.Draw(NEWLEVEL, Vector2.Zero, null, Color.White);
            spriteBatch.DrawString(statsFont, current_level, strPosition, Color.Red);

            strCenter =
                new Vector2(strPlayAgainSize.X / 2, strPlayAgainSize.Y / 2);
            yOffsetText = (viewportSize.Y / 2 - strCenter.Y) +
                (float)statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);
            spriteBatch.DrawString(statsFont, GlobalState.STR_CONTINUE, strPosition, Color.AntiqueWhite);

            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement

            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;
        }

        private void DrawWinScreen(string gameResult)
        {
            float xOffsetText, yOffsetText;
            Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Vector2 strCenter;

            xOffsetText = yOffsetText = 0;
            Vector2 strResult = statsFont.MeasureString(gameResult);
            Vector2 strPlayAgainSize =
                statsFont.MeasureString(GlobalState.STR_RESTART);
            Vector2 strPosition;
            strCenter = new Vector2(strResult.X / 2, strResult.Y / 2);

            yOffsetText = (viewportSize.Y / 2 - strCenter.Y);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            spriteBatch.Begin();
            spriteBatch.Draw(winState, Vector2.Zero, null, Color.White); 
            spriteBatch.DrawString(statsFont, gameResult, strPosition, Color.Red);

            strCenter =
                new Vector2(strPlayAgainSize.X / 2, strPlayAgainSize.Y / 2);
            yOffsetText = (viewportSize.Y / 2 - strCenter.Y) +
                (float)statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);
            spriteBatch.DrawString(statsFont, GlobalState.STR_RESTART, strPosition, Color.AntiqueWhite);

            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement

            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;

            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.AlphaTestEnable = false;

            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }


        private void DrawLossScreen(string gameResult)
        {
            float xOffsetText, yOffsetText;
            Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Vector2 strCenter;

            xOffsetText = yOffsetText = 0;
            Vector2 strResult = statsFont.MeasureString(gameResult);
            Vector2 strPlayAgainSize =
                statsFont.MeasureString(GlobalState.STR_RESTART);
            Vector2 strPosition;
            strCenter = new Vector2(strResult.X / 2, strResult.Y / 2);

            yOffsetText = (viewportSize.Y *0.85f - strCenter.Y);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            spriteBatch.Begin();
            spriteBatch.Draw(gameOver, Vector2.Zero, null, Color.White); 
            //spriteBatch.DrawString(statsFont, gameResult,strPosition, Color.Red);

            strCenter =
                new Vector2(strPlayAgainSize.X / 2, strPlayAgainSize.Y / 2);
            yOffsetText = (viewportSize.Y*0.9f - strCenter.Y) +
                (float)statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);
            spriteBatch.DrawString(statsFont, GlobalState.STR_RESTART, strPosition, Color.AntiqueWhite);

            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement

            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;

            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.AlphaTestEnable = false;

            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }

        private void DrawGameplayScreen()
        {
            BoundingFrustum view_frustum = new BoundingFrustum(gameCamera.view_matrix * gameCamera.projection_matrix);
            //device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
            skyboxModel.CopyAbsoluteBoneTransformsTo(skyboxTransforms);
            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = skyboxTransforms[mesh.ParentBone.Index] * Matrix.CreateScale(20.0f);
                    effect.View = gameCamera.view_matrix;
                    effect.Projection = gameCamera.projection_matrix;
                }
                mesh.Draw();
            }

            DrawTerrain(ground.model, grassTexture);
            DrawWalls(brick_wall.model, brickTexture);
            foreach (Zombie zombie in zombies)
            {
                BoundingSphere object_sphere = new BoundingSphere(zombie.position, 5.0f);
                if (!zombie.Killed && view_frustum.Intersects(object_sphere))
                {
                    zombie.Draw(gameCamera.view_matrix,
                        gameCamera.projection_matrix);
                }
            }
            foreach (Tree tree in trees)
            {
                BoundingSphere object_sphere = new BoundingSphere(tree.position, 8.0f);
                if (view_frustum.Intersects(object_sphere))
                tree.Draw(gameCamera.view_matrix, gameCamera.projection_matrix);
            }
            DepthStencilState dss = new DepthStencilState();
            DepthStencilState dss2 = new DepthStencilState();
            dss.DepthBufferEnable = false;
            dss2.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;
            guns.Draw(player.position, player.forward_direction, gameCamera.view_matrix, gameCamera.projection_matrix);
            GraphicsDevice.DepthStencilState = dss2;

            /*foreach (Ray ray in rayList)
            {
               RayRenderer.Render(ray,100.0f, GraphicsDevice, gameCamera.view_matrix,gameCamera.projection_matrix, Color.Red);
            }
            */
            //player.Draw(gameCamera.view_matrix, gameCamera.projection_matrix);
            DrawStats();
              

        }

        private void DrawStats()
        {
            float xOffsetText, yOffsetText;
            string str1 = String.Format("Score: {0}", GAME_SCORE);
            string str2 =
                GlobalState.STR_KILL_COUNT + zombiesKilled.ToString() +
                " of " + killTotal.ToString(); 
            Rectangle rectSafeArea;


            //Calculate str1 position
            rectSafeArea = GraphicsDevice.Viewport.TitleSafeArea;

            xOffsetText = rectSafeArea.X;
            yOffsetText = rectSafeArea.Y;

            Vector2 strSize = statsFont.MeasureString(str1);
            Vector2 strPosition =
                new Vector2((int)xOffsetText + 10, (int)yOffsetText);
            Vector2 strPosition2 = strPosition;
            strPosition2.X = GraphicsDevice.Viewport.Width - 150;

            string debug_str1 = String.Format("Player position = X: {0}, Y: {1}, Z: {2}", player.position.X,
                player.position.Y, player.position.Z);

            spriteBatch.Begin();
            //spriteBatch.DrawString(statsFont, str1, strPosition, Color.White);
            //strPosition.Y += strSize.Y;

            //draw health bar
            int h_b_height = 20;
            Rectangle h_b_rect = new Rectangle((int)strPosition.X, (int)strPosition.Y, player.health, h_b_height);
            spriteBatch.DrawString(statsFont, str1, strPosition2, Color.White);
            spriteBatch.Draw(health_bar, h_b_rect, Color.Red);
            strPosition.Y += h_b_height;
            spriteBatch.DrawString(statsFont, str2, strPosition, Color.White);
            strPosition.Y += strSize.Y;
            //spriteBatch.DrawString(statsFont, debug_str1, strPosition, Color.White);
            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement

            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;

            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.AlphaTestEnable = false;

            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }

    }
}
