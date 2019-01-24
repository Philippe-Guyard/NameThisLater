using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using FirstGameEngine.Animations;
using FirstGameEngine.Entities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace FirstGameEngine.Levels
{
    public class Level
    {
        private LevelInfo _info;

        public LevelAnimation Animation { get; set; }
        public LevelInfo Info
        {
            get => _info;
            set
            {
                _info = value;
                if (_info.SoundTrack == null)
                {
                    if (SoundtrackInstance != null)
                        SoundtrackInstance.Dispose();

                    SoundtrackInstance = null;
                }
                else
                {
                    SoundtrackInstance = _info.SoundTrack.CreateInstance();
                    SoundtrackInstance.IsLooped = true;
                }
            }
        }

        public Tiled.TiledMap Map;

        private bool AnimationPlayed { get; set; }
        private bool LocationShown { get; set; }
        
        private bool EnterStarted { get; set; }
        
        private TextAnimator TextDrawer { get; set; }

        public SoundEffectInstance SoundtrackInstance { get; private set; }

        public bool EnteredLevel => AnimationPlayed && LocationShown;

        public Level()
        {
            AnimationPlayed = LocationShown = EnterStarted = false;

            TextDrawer = new TextAnimator();
        }

        public void BeginLevelEnter()
        {
            AnimationPlayed = Animation == null;
            LocationShown = Info.RealWorldLocation == string.Empty;
            
            if (!LocationShown)
                TextDrawer.BeginAnimation(Info.RealWorldLocation);
            if (!AnimationPlayed)
                Animation.Begin();

            EnterStarted = true;
        }
        
        public void DrawLevelScreen(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            SpriteFont locatinFont, double elapsed)
        {
            if (!EnterStarted)
                throw new InvalidOperationException("BeginLevelEnter has not been called");

            if (EnteredLevel)
                throw new InvalidOperationException("Level already finished entering");

            graphicsDevice.Clear(Color.Black);
            if (!AnimationPlayed)
            {                
                Animation.Update(elapsed);
                Animation.Draw(spriteBatch);

                AnimationPlayed = Animation.AnimationFinished;
            }
            if (!LocationShown && AnimationPlayed)
            {                
               
                TextDrawer.TextFont = locatinFont;
                TextDrawer.DrawText(spriteBatch, elapsed);

                LocationShown = TextDrawer.TextDrawn;
             }            
        }

        public void Skip()
        {
            if (!AnimationPlayed)
                AnimationPlayed = true;
            else if (!LocationShown)
                LocationShown = true;
        }
    }
}
