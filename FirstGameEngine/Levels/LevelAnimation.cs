using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace FirstGameEngine.Levels
{
    public class LevelAnimation
    {
        private enum AnimationStatus
        {
            Started,
            BlackIn,
            ShowingFrame,
            BlackOut,
            BlackScreen,
            Finished,
        }

        private const int TimeToShowBlackScreen = 2000;
        private const int TimeBetweenFrames = 3000;

        private int _cFrame;
        private float _alpha;

        public SoundEffect AttachedSoundtrack { get; set; }

        public int Duration { get; set; }

        private int TimeInto;

        private List<Texture2D> Frames { get; set; }
        private AnimationStatus Status { get; set; }

        private float FrameDuration => Frames == null ? 0 : Duration / Frames.Count;
        private Texture2D CurrentFrame => Frames == null ? null : Frames[_cFrame];

        public bool AnimationStarted => Status != AnimationStatus.Finished;
        public bool AnimationFinished => Status == AnimationStatus.Finished;

        public LevelAnimation()
        {
            _cFrame = 0;
            TimeInto = 0;

            Frames = new List<Texture2D>();

            Status = AnimationStatus.Finished;
        }

        public void LoadFromFolder(ContentManager contentManager, int framesCount, string path = "")
        {
            for(int i = 1; i <= framesCount; i++)
            {
                string imPath = path + "/" + i.ToString();
                Frames.Add(contentManager.Load<Texture2D>(imPath));
            }           
        }

        public void Begin()
        {
            Status = AnimationStatus.BlackIn;
            TimeInto = 0;
            _cFrame = 0;
        }

        public void End()
        {
            Status = AnimationStatus.Finished;
        }

        public void Update(double elapsed)
        {
            if (Status == AnimationStatus.Finished)
            {
                throw new InvalidOperationException("Animation.Begin() has not yet been called");
            }

            TimeInto += (int)elapsed;
            if (Status == AnimationStatus.BlackScreen)
            {
                if (TimeInto > TimeToShowBlackScreen)
                    this.End();
            }
            else if (Status == AnimationStatus.BlackIn)
            {
                _alpha = Math.Min(1f, (float)TimeInto / (float)TimeBetweenFrames);
                if (TimeInto > TimeBetweenFrames)
                {
                    Status = AnimationStatus.ShowingFrame;
                    TimeInto = 0;
                }
            }
            else if (Status == AnimationStatus.BlackOut)
            {
                _alpha = Math.Max(1 - ((float)TimeInto / TimeBetweenFrames), 0f);
                if (TimeInto > TimeBetweenFrames)
                {
                    Status = AnimationStatus.BlackIn;
                    TimeInto = 0;
                    ++_cFrame;
                }

                if (_cFrame == Frames.Count)
                {
                    Status = AnimationStatus.BlackScreen;
                    _cFrame = 0;
                    TimeInto = 0;
                }
            }
            else
            {
                if (TimeInto > FrameDuration)
                {
                    TimeInto = 0;
                    Status = AnimationStatus.BlackOut;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Color colorMask = Color.White;
            if (Status == AnimationStatus.BlackScreen)
                colorMask = Color.Black;
            if (Status == AnimationStatus.BlackIn)
                colorMask = Color.White * _alpha;
            if (Status == AnimationStatus.BlackOut)
                colorMask = Color.White * _alpha;
            spriteBatch.Draw
                (
                    texture: CurrentFrame,
                    destinationRectangle: new Rectangle(0, 0, Constants.SampleWindowWidth, Constants.SampleWindowHeight),
                    color: colorMask,
                    layerDepth: 0f
                );
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
