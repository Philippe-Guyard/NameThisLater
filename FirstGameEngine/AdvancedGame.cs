#pragma warning disable CS0618 // Type or member is obsolete


using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using MonoGame.Extended;

/*
First see if those bugs work on IOS

//TODO: Fix double axe hit bug
//TODO: Fix double shooting bug
//TODO: Fix settings buttons multi-click bug

*/

//Add multiple floors in game engine

using FirstGameEngine.Levels;


namespace FirstGameEngine
{
    public class AdvancedGame : Game
    {
        public const int Levels = 1;

        protected GameState gameState;

        protected GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;

        protected List<GameEngine> Engines;

        protected SpriteFont defaultFont;
        protected SpriteFont RussianFont;

        protected SoundEffect MainSong;
        protected SoundEffectInstance MainInstance;

        protected Button StartButton;
        protected Button SettingsButton;

        protected List<Button> LevelButtons;

        protected SettingsUI CurrentSettingsUI;

        protected Texture2D StartBG;

        protected double FpsFactor = 60.000;
        protected long FrameCount = 0;

        protected int CurrentLevelIndex;

        public double Width { get; private set; }
        public double Height { get; private set; }

        protected double WidthScale, HeightScale;

        public Matrix2D Scale;

        public double ScaleFactor => (WidthScale + HeightScale) / 2.0;

        protected GameEngine gameEngine => Engines[CurrentLevelIndex];

        protected Level CurrentLevel => LevelsManager.Get(CurrentLevelIndex);

        public AdvancedGame()
        {
            Settings.SoundFX = true;
            Settings.Music = false;

            graphics = new GraphicsDeviceManager(this);
            
            Content.RootDirectory = "Content";

            gameState = GameState.StartMenu;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Engines = new List<GameEngine>();
            
            Width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            Height = GraphicsDevice.PresentationParameters.BackBufferHeight;

            WidthScale = Width / Constants.SampleWindowWidth;
            HeightScale = Height / Constants.SampleWindowHeight;

           // Scale = Matrix2D.CreateScale((float)WidthScale, (float)HeightScale);
            Scale = Matrix2D.CreateScale((float)ScaleFactor);

            int h = 64;
            int w = 64;
            int x = Constants.SampleWindowWidth / 2 - w / 2;

            StartButton = new Button(x, 10, w, h);
            SettingsButton = new Button(x, 90, w, h);

            SettingsButton.GraphicsDevice = StartButton.GraphicsDevice = this.GraphicsDevice;

            LevelButtons = new List<Button>();
            w = Constants.SampleWindowWidth / Levels - 15;

            for(int i = 0; i < Levels; i++)
            {
                LevelButtons.Add(new Button(15 + i * (w + 5), Constants.SampleWindowHeight / 2 - 15, w, 30));
                LevelButtons[i].GraphicsDevice = GraphicsDevice;
            }

            CurrentSettingsUI = new SettingsUI();

            FontManager.Initialize(Content);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
       
            defaultFont = Content.Load<SpriteFont>("Default");
            RussianFont = Content.Load<SpriteFont>("Russian");

            MainSong = Content.Load<SoundEffect>("sounds/soundtrack");
            MainInstance = MainSong.CreateInstance();
            MainInstance.IsLooped = true;
            if (Settings.Music)
            {
                MainInstance.Play();
            }

            LevelsManager.Initialize(GraphicsDevice, Content);
            for (int i = 0; i < Levels; i++)
            {
                Engines.Add(new GameEngine(GraphicsDevice));
                Engines[i].CreateCamera(GraphicsDevice, (float)ScaleFactor);

                Engines[i].LoadContent(Content, LevelsManager.Get(i));
            }


            StartButton.LoadImage(Content, "misc/PlayButton");
            SettingsButton.LoadImage(Content, "misc/OptionsButton");

            CurrentSettingsUI.LoadContent(Content, "misc/OptionsButtonsAtlas");

            StartBG = Content.Load<Texture2D>("misc/BG");

            base.LoadContent();

            graphics.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Two;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            switch (gameState)
            {
                case GameState.InLevel:
                    UpdateGame(gameTime, GetActions());
                    break;
                case GameState.StartMenu:
                    if (Settings.Music)
                    {
                        MainInstance.Play();
                    }
                    UpdateStartMenu();
                    break;

                case GameState.Settings:
                    UpdateSettings();
                    break;
                case GameState.Died:
                    break;
                case GameState.Levels:
                    UpdateLevelsScreen();
                    break;
            }

            base.Update(gameTime);
        }

        #region Screen Updates

        protected virtual void UpdateSettings()
        {
            CurrentSettingsUI.Update(Scale);

            if (CurrentSettingsUI.BackRequested)
            {
                this.GoUpOneLevel();
            }
        }

        protected virtual void UpdateStartMenu()
        {
            if (SettingsButton.IsPressed(Scale))
            {
                gameState = GameState.Settings;
                MainInstance.Stop();
            }
            else if (StartButton.IsPressed(Scale))
            {
                //gameState = GameState.Levels;
                gameState = GameState.InLevel;
                CurrentLevelIndex = 0;

                CurrentLevel.BeginLevelEnter();
                MainInstance.Stop();
            }
        }

        protected virtual void UpdateGame(GameTime gameTime, List<ActionTypes> actions)
        {            
            if (Settings.Music)
            {
                if (CurrentLevel.EnteredLevel && CurrentLevel.SoundtrackInstance != null)
                {
                    CurrentLevel.SoundtrackInstance.Play();
                }
            }
            if (!CurrentLevel.EnteredLevel)
                return;
            
            gameEngine.Update(gameTime, actions);

            if (FrameCount % 10 == 0)
            {
                if (gameTime.ElapsedGameTime.TotalSeconds > 0.001)
                    FpsFactor = 1f / gameTime.ElapsedGameTime.TotalSeconds;
            }

            ++FrameCount;

            if (gameEngine.BackRequested)
            {
                this.GoUpOneLevel();
                gameEngine.Reset();
            }
        }

        protected virtual void UpdateLevelsScreen()
        {
            for(int i = 0; i < Levels; i++)
            {
                if (LevelButtons[i].IsPressed(Scale))
                {
                    gameState = GameState.InLevel;
                    CurrentLevelIndex = i;

                    CurrentLevel.BeginLevelEnter();
                }
            }
        }
        
        #endregion

        protected void GoUpOneLevel()
        {
            switch (gameState)
            {
                case GameState.Settings:
                    gameState = GameState.StartMenu;
                    if (Settings.Music) 
                        MainInstance.Play();

                    break;
                case GameState.InLevel:
                    gameState = GameState.StartMenu;
                    if (Settings.Music && CurrentLevel.SoundtrackInstance != null)
                        CurrentLevel.SoundtrackInstance.Stop();
                    break;
                case GameState.Died:
                    gameState = GameState.StartMenu;
                    break;
                case GameState.StartMenu:
                    break;
                case GameState.Levels:
                    gameState = GameState.StartMenu;
                    if (Settings.Music)
                        MainInstance.Play();
                    break;
            }
        }

        protected virtual void DrawBackGround()
        {
            Texture2D bg = null;

            switch(gameState)
            {
                case GameState.StartMenu:
                    bg = StartBG;
                    break;
                case GameState.Settings:
                    bg = StartBG;
                    break;
                case GameState.Levels:
                    bg = StartBG;
                    break;
                case GameState.Died:
                    bg = StartBG;
                    break;
            }

            if (bg != null)
            {
                spriteBatch.Draw
                    (
                        texture: bg,
                        destinationRectangle:
                            new Rectangle(0, 0, Constants.SampleWindowWidth, Constants.SampleWindowHeight),
                        color: Color.White,
                        layerDepth: 0f
                    );
            }
        }

        protected virtual List<ActionTypes> GetActions()
        {
            return new List<ActionTypes>();
        }

        /// <summary> 
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            switch (gameState)
            {
                case GameState.InLevel:
                    DrawGameMenu();
                    break;
                case GameState.StartMenu:
                    StandardBeginDraw();

                    DrawStartMenu();
                    break;
                case GameState.Settings:
                    StandardBeginDraw();

                    DrawSettings();
                    break;

                case GameState.Died:
                    StandardBeginDraw();

                    DrawDieScreen();
                    break;

                case GameState.Levels:
                    StandardBeginDraw();

                    DrawLevels();
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected virtual void StandardBeginDraw()
        {
            spriteBatch.Begin
                (
                    blendState: BlendState.AlphaBlend,
                    sortMode: SpriteSortMode.FrontToBack,
                    transformMatrix: Scale,
                    samplerState: SamplerState.PointClamp
                );

            DrawBackGround();
        }

        #region DrawScreens

        protected virtual void DrawStartMenu()
        {
            StartButton.Draw(spriteBatch);
            
            SettingsButton.Draw(spriteBatch);            
        }

        protected virtual void DrawGameMenu()
        {
            if (CurrentLevel.EnteredLevel)
            {
                gameEngine.DrawEverything(spriteBatch);
            }
            else
            {
                spriteBatch.Begin(transformMatrix: Scale);

                CurrentLevel.DrawLevelScreen(GraphicsDevice, spriteBatch, defaultFont, TargetElapsedTime.TotalMilliseconds);
            }
        }

        protected virtual void DrawSettings(bool beginDraw = false)
        {
            CurrentSettingsUI.Draw(spriteBatch);
        }

        protected virtual void DrawDieScreen()
        {
            spriteBatch.DrawString(defaultFont, "You Lost!", new Vector2(100, 100), Color.White);
        }

        protected virtual void DrawLevels()
        {
            for(int i = 0; i < Levels; i++)
            {
                LevelButtons[i].Draw(spriteBatch);
            }
        }

        #endregion

        protected class ControlsEditor
        {
            public AdvancedGame Source { get; set; }

            private List<Button> Controls { get; set; }
            private Button Clicked;

            public ControlsEditor()
            {

            }

            public void AddControl(Button Control)
            {
                Controls.Add(new Button(Control.BoundingRectangle)
                {
                    ScreenH = Control.ScreenH, ScreenW = Control.ScreenW,
                    ScreenX = Control.ScreenX, ScreenY = Control.ScreenY,
                    Texture = Control.Texture
                });
            }

            public void Update()
            {
                bool click = false;
                foreach(var button in Controls)
                {
                    if (button.IsPressed())
                    {
                        Clicked = button;
                        click = true;
                        break;
                    }
                }

                if (click || Clicked == null)
                    return;

#if __IOS__
                UpdateIosControl();
#endif

            }

            public void Reset()
            {

            }

            private void UpdateIosControl()
            {
                var touches = TouchPanel.GetState();
                var firstTouch = touches.First(x => x.State != TouchLocationState.Invalid);

                Clicked.X = (int)firstTouch.Position.X;
                Clicked.Y = (int)firstTouch.Position.Y;
            }
        }

        protected class SettingsUI
        {
            public bool BackRequested { get; private set; }

            private Button SoundFxButton { get; set; }
            private Button MusicButton { get; set; }
            private Button BackButton { get; set; }

            private Rectangle SoundFxOn;
            private Rectangle SoundFxOff;

            private Rectangle MusicOn;
            private Rectangle MusicOff;

            public SettingsUI()
            {
                
            }
            
            public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager, string atlasPath)
            {
                const int w = 32;
                const int h = 32;

                MusicOn = new Rectangle(0, 0, w, h);
                SoundFxOn = new Rectangle(w, 0, w, h);
                MusicOff = new Rectangle(0, h, w, h);
                SoundFxOff = new Rectangle(w, h, w, h);

                Texture2D atlas = contentManager.Load<Texture2D>(atlasPath);

                const int y = Constants.SampleWindowHeight - h - 10;

                MusicButton = new Button(200, y, w, h);
                MusicButton.OnlyTriggerOnRelease = true;
                MusicButton.SourceRectangle = Settings.Music ? MusicOn : MusicOff;
                
                SoundFxButton = new Button(220, y, w, h);
                SoundFxButton.OnlyTriggerOnRelease = true;
                SoundFxButton.SourceRectangle = Settings.SoundFX ? SoundFxOn : SoundFxOff;

                BackButton = new Button(20, y, w, h);
                BackButton.SourceRectangle = new Rectangle(3 * w, 0, w, h);

                MusicButton.Texture = BackButton.Texture = SoundFxButton.Texture = atlas;
            }

            public void Update(Matrix2D? Scale = null)
            {
                BackRequested = false;

                if (SoundFxButton.IsPressed(Scale))
                {
                    Settings.SoundFX = !Settings.SoundFX;
                }
                if (MusicButton.IsPressed(Scale))
                {
             //       Settings.Music = !Settings.Music;
                }
                if (BackButton.IsPressed(Scale))
                {
                    BackRequested = true;
                }
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                SoundFxButton.SourceRectangle = Settings.SoundFX ? SoundFxOn : SoundFxOff;
                MusicButton.SourceRectangle = Settings.Music ? MusicOn : MusicOff;

                SoundFxButton.Draw(spriteBatch);
               // MusicButton.Draw(spriteBatch);
                BackButton.Draw(spriteBatch);
            }
        }
    }
}
