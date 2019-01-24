using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using FirstGameEngine;
using MonoGame.Extended;
//using Xamarin.Forms;

namespace IOS_First_Game
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : AdvancedGame
    {
        const int BtnWidth = 45;
        const int BtnHeight = 45;

        Button JumpButton, RightButton, LeftButton, FireButton, AxeButton;
        List<Button> ActionButtons;

        Button PauseButton;

        TouchCollection touches;

        BatteryInfo Battery;

        Texture2D[] Charges;

        int CurrentChargeIndex;

        private Camera2D CurrentCam => Engines[CurrentLevelIndex].AttachedCamera; 

        public Game1() : base()
        {
            Xamarin.Forms.Forms.Init();

            Xamarin.Forms.DependencyService.Register<IBattery>();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            
            int w = BtnWidth;
            int h = BtnHeight;

            int screenW = (int)(BtnWidth * ScaleFactor);
            int screenH = (int)(BtnHeight * ScaleFactor);

            var bounds = new Rectangle(0, 0, (int)Width, (int)Height);

            float x = bounds.Right - screenW - 4 * (float)ScaleFactor;
            float y = bounds.Bottom - screenH - 10 * (float)ScaleFactor;
            JumpButton = new Button(x, y, w, h);
            JumpButton.Tag = ActionTypes.Jump;
            JumpButton.ScreenH = screenH;
            JumpButton.ScreenW = screenW;

            x = JumpButton.X - screenH - 10 * (float)ScaleFactor;
            y = JumpButton.Y;
            AxeButton = new Button(x, y, w, h);
            AxeButton.Tag = ActionTypes.AxeHit;
            AxeButton.ScreenH = screenH;
            AxeButton.ScreenW = screenW;

            x = JumpButton.X;
            y = JumpButton.Y - screenH - 10 * (float)ScaleFactor;
            FireButton = new Button(x, y, w, h);
            FireButton.Tag = ActionTypes.Fire;
            FireButton.ScreenH = screenH;
            FireButton.ScreenW = screenW;
            
            x = bounds.Left + 4 * (float)ScaleFactor;
            y = bounds.Bottom - screenH - 10 * (float)ScaleFactor;
            LeftButton = new Button(x, y, w, h);
            LeftButton.Tag = ActionTypes.GoLeft;
            LeftButton.ScreenH = screenH;
            LeftButton.ScreenW = screenW;

            x = LeftButton.X + screenW + 10 * (float)ScaleFactor;
            y = bounds.Bottom - screenH - 10 * (float)ScaleFactor;
            RightButton = new Button(x, y, w, h);
            RightButton.Tag = ActionTypes.GoRight;
            RightButton.ScreenH = screenH;
            RightButton.ScreenW = screenW;

            x = bounds.Right - screenW - 4 * (float)ScaleFactor;
            PauseButton = new Button(x, 4 * (float)ScaleFactor, w, (int)(h / 1.5f));
            PauseButton.Tag = null;
            PauseButton.ScreenH = screenH / 1.5f;
            PauseButton.ScreenW = screenW;

            ActionButtons = new List<Button>();

            JumpButton.LoadImage(Content, "buttons/jump");
            LeftButton.LoadImage(Content, "buttons/left");
            RightButton.LoadImage(Content, "buttons/right");
            FireButton.LoadImage(Content, "buttons/fire");
            PauseButton.LoadImage(Content, "buttons/pause");
            AxeButton.LoadImage(Content, "buttons/axe");

            ActionButtons.Add(RightButton);
            ActionButtons.Add(LeftButton);
            ActionButtons.Add(JumpButton);
            ActionButtons.Add(FireButton);
            ActionButtons.Add(AxeButton);

            Charges = new Texture2D[4];
            Charges[0] = Content.Load<Texture2D>("misc/Charge1");
            Charges[1] = Content.Load<Texture2D>("misc/Charge2");
            Charges[2] = Content.Load<Texture2D>("misc/Charge3");
            Charges[3] = Content.Load<Texture2D>("misc/Charge4");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed.
            // Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
#endif

            Battery = Xamarin.Forms.DependencyService.Get<BatteryInfo>();
            if (Battery.RemainingChargePercent >= 75)
            {
                CurrentChargeIndex = 0;
            }
            else if (Battery.RemainingChargePercent >= 50)
            {
                CurrentChargeIndex = 1;
            }
            else if (Battery.RemainingChargePercent >= 25)
            {
                CurrentChargeIndex = 2;
            }
            else
                CurrentChargeIndex = 3;

            foreach (var button in ActionButtons)
            {
                UpdateButton(button);
            }

            UpdateButton(PauseButton);

           base.Update(gameTime);
        }

        protected override void UpdateGame(GameTime gameTime, List<ActionTypes> actions)
        {
            base.UpdateGame(gameTime, actions);
            
            foreach (var button in ActionButtons)
            {
                UpdateButton(button);
            }

            if (PauseButton.IsPressed())
            {
                gameState = GameState.StartMenu;
            }
            
            UpdateButton(PauseButton);
        }

        protected override List<ActionTypes> GetActions()
        {
            List<ActionTypes> actions = new List<ActionTypes>();

            touches = TouchPanel.GetState();

            foreach (var button in ActionButtons)
            {
                if (button.IsPressed())
                    actions.Add((ActionTypes)button.Tag);
            }

            return actions;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        protected override void DrawStartMenu()
        {
            base.DrawStartMenu();       
        }

        protected override void DrawGameMenu()
        {
            base.DrawGameMenu();

            if (CurrentLevel.EnteredLevel)
            {
                foreach (var button in ActionButtons)
                {
                    button.Draw(spriteBatch);
                }

                foreach(var touch in TouchPanel.GetState())
                {
                    Vector2 posi = CurrentCam.ScreenToWorld(touch.Position.X, touch.Position.Y);

                    CircleF circle = new CircleF(posi, 20);
                    spriteBatch.DrawCircle(circle, 100, Color.Red);
                }

                PauseButton.Draw(spriteBatch);

                int width = (int)(Charges[CurrentChargeIndex].Width * 1.7f);
                int height = (int)(Charges[CurrentChargeIndex].Height * 1.7f);

                Vector2 pos = new Vector2(PauseButton.X + 6, PauseButton.Y + 3);
                Rectangle dest = new Rectangle((int)pos.X, (int)pos.Y, width, height);
                spriteBatch.Draw(Charges[CurrentChargeIndex], destinationRectangle: dest, layerDepth: 1f);
            }
        }

        protected void UpdateButton(Button btn)
        {
            Vector2 newPos = CurrentCam.ScreenToWorld(btn.ScreenX, btn.ScreenY);
            
            btn.X = (int)newPos.X;
            btn.Y = (int)newPos.Y;
        }
    }
}

