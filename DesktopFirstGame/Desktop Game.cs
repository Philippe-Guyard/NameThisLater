using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

using System.Xml.Linq;

using System.Collections.Generic;

using FirstGameEngine;

using MonoGame.Extended;

namespace Desktop_First_Game
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class DesktopGame : AdvancedGame
    {       
        bool Up = false;
               
        public DesktopGame() : base()
        {
            this.IsMouseVisible = true;

            graphics.PreferredBackBufferHeight = 750;
            graphics.PreferredBackBufferWidth = 1334;
            graphics.ApplyChanges();
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                GoUpOneLevel();

            base.Update(gameTime);
        }

        protected override List<ActionTypes> GetActions()
        {
            var keys = Keyboard.GetState().GetPressedKeys();
            var actions = new List<ActionTypes>();
            foreach (Keys key in keys)
            {
                if (key.Equals(Keys.W))
                    actions.Add(ActionTypes.Jump);
                if (key.Equals(Keys.A))
                    actions.Add(ActionTypes.GoLeft);
                if (key.Equals(Keys.D))
                    actions.Add(ActionTypes.GoRight);
                if (key.Equals(Keys.S))
                    actions.Add(ActionTypes.GoDownLader);
                if (key.Equals(Keys.Space))
                    actions.Add(ActionTypes.Fire);
                if (key.Equals(Keys.U))
                {
                    actions.Add(ActionTypes.ClimbLader);
                    Up = true;
                }
                if (key.Equals(Keys.I))
                {
                    actions.Add(ActionTypes.GoDownLader);
                    Up = false;
                }
                if (key.Equals(Keys.Z))
                {
                    actions.Add(ActionTypes.AxeHit);
                }
            }

            if (Up)
            {
                if (Keyboard.GetState().IsKeyUp(Keys.U))
                {
                    actions.Add(ActionTypes.StopClimb);
                }
            }
            else
            {
                if (Keyboard.GetState().IsKeyUp(Keys.I))
                {
                    actions.Add(ActionTypes.StopClimb);
                }
            }

            return actions;
        }

        /// <summary> 
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);
            
            base.Draw(gameTime);
        }
    }
}
