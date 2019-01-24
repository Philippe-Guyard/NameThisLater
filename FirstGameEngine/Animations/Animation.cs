using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using FirstGameEngine.Entities;

namespace FirstGameEngine.Animations
{
    public class Animation
    {
        public List<AnimationFrame> Frames;
        public Entity Source { get; set; }

        public bool IsAtlasing { get; set; }
        
        public Texture2D ImageSource { get; set; }

        public float Duration { get; private set; }
        public float Priority { get; set; }

        private int __curentFrame;
        private long __lastFrameCall, __lastMovementCall;

        public AnimationFrame CurrentFrame => Frames[__curentFrame];

        public Animation(Entity _source, bool atlasing = false)
        {
            Frames = new List<AnimationFrame>();
            
            Source = _source;

            __curentFrame = 0;
            __lastFrameCall = 0;
            __lastMovementCall = 0;

            Duration = 0;

            IsAtlasing = atlasing;

            ImageSource = null;
        }

        public Vector2 GetNextPosition(out Texture2D nextTexture, out Rectangle? nextRectangle,
            Rectangle? currentRectangle, GameTime time, out bool fullCycle)
        {
            if (Frames.Count == 0)
                throw new Exception();
            if (time.TotalGameTime.TotalMilliseconds > __lastFrameCall + 
                Math.Max(100, 3 * CurrentFrame.TimeUntillNextFrame))
                this.ResetFrames(time);
            if (time.TotalGameTime.TotalMilliseconds > __lastMovementCall +
                Math.Max(100, 3 * CurrentFrame.Duration))
                this.ResetMovement(time);

            fullCycle = false;

            bool flag = false;
            double change = time.TotalGameTime.TotalMilliseconds - __lastFrameCall;
            if (change <= CurrentFrame.TimeUntillNextFrame)
            {
                if (this.IsAtlasing)
                {
                    nextTexture = ImageSource;
                    nextRectangle = currentRectangle;
                }
                else
                {
                    nextTexture = Source.Image;
                    nextRectangle = null;
                }
            }
            else
            {
                flag = true;
                if (this.IsAtlasing)
                {
                    nextTexture = null;
                    nextRectangle = CurrentFrame.SourceRectangle;
                }
                else
                {
                    nextTexture = CurrentFrame.Picture;
                    nextRectangle = null;
                }

                __lastFrameCall = (long)time.TotalGameTime.TotalMilliseconds;
            }

            change = time.TotalGameTime.TotalMilliseconds - __lastMovementCall;      
            change /= Constants.DefaultAnimationFrameDuration;       

            Vector2 res = new Vector2(
                Source.X + (float)(CurrentFrame.dX * change),
                Source.Y + (float)(CurrentFrame.dY * change)
            );
            
            if (flag)
            {
                if (__curentFrame + 1 == Frames.Count)
                {
                    __curentFrame = 0;
                    fullCycle = true;
                }
                else 
                    ++__curentFrame;
            }

            __lastMovementCall = (long)time.TotalGameTime.TotalMilliseconds;

            return res;
        }

        #region Frames

        public void AddFrame(GraphicsDevice graphicsDevice, string pic, 
            float _x = 0, float _y = 0, float _dur = Constants.DefaultAnimationFrameDuration)
        {
            AnimationFrame nextFrame = new AnimationFrame(_x, _y, _dur);
            
            nextFrame.LoadPicture(graphicsDevice, pic);

            Duration += nextFrame.Duration;


            Frames.Add(nextFrame);
        }

        public void AddFrame(Texture2D pic, 
            float _x = 0, float _y = 0, float _dur = Constants.DefaultAnimationFrameDuration)
        {
            AnimationFrame nextFrame = new AnimationFrame(_x, _y, _dur);

            nextFrame.Picture = pic;

            Duration += nextFrame.Duration;


            Frames.Add(nextFrame);
        }

        public void AddFrame(Rectangle rectangle, float time, float x = 0, 
            float y = 0, float dur = Constants.DefaultAnimationFrameDuration)
        {
            AnimationFrame nextFrame = new AnimationFrame(x, y, dur);

            nextFrame.TimeUntillNextFrame = time;

            nextFrame.Picture = null;

            nextFrame.SourceRectangle = rectangle;

            Frames.Add(nextFrame);
        }

        public void AddFrame(AnimationFrame frame)
        {
            Frames.Add(frame.Copy());
        }

        #endregion

        public void ChangeSpeed(float dX, float dY)
        {
            foreach (var frame in Frames)
            {
                frame.dX += dX;
                frame.dY += dY;
            }
        }

        public void ChangeDuration(float newValue)
        {
            float frameDuration = newValue / Frames.Count;
            foreach (var frame in Frames)
            {
                frame.Duration = frameDuration;
            }
        }
        
        public bool IsResetable(GameTime gameTime)
        {
            return gameTime.TotalGameTime.TotalMilliseconds >
                __lastFrameCall + Math.Max(100, 3 * CurrentFrame.Duration);
        }

        public void ForceReset(GameTime gameTime)
        {
            ResetFrames(gameTime);
            ResetMovement(gameTime);
        }

        public Animation Reverse()
        {
            Animation res = new Animation(this.Source, this.IsAtlasing);
            res.ImageSource = this.ImageSource;

            for(int i = Frames.Count - 1; i >= 0; i--)
            {
                res.AddFrame(Frames[i]);
            }

            res.ChangeSpeed(CurrentFrame.dX * -2, CurrentFrame.dY * -2);

            return res;
        }

        #region private members

        private void ResetFrames(GameTime gameTime)
        {
            __curentFrame = 0;
            __lastFrameCall = (long)gameTime.TotalGameTime.TotalMilliseconds;
        }
        
        private void ResetMovement(GameTime gameTime)
        {
            __lastMovementCall = (long)gameTime.TotalGameTime.TotalMilliseconds;
        }

        #endregion

        public static Animation Mix(Animation _xAnim, Animation _yAnim, bool chooseXPics = false)
        {
            if (!_xAnim.Source.Equals(_yAnim.Source))
                throw new InvalidOperationException("Animations should have same source");


            Animation res = new Animation(_xAnim.Source);

            int maxCommonCount = Math.Min(_xAnim.Frames.Count, _yAnim.Frames.Count);
            for(int i = 0; i < maxCommonCount; i++)
            {
                if (chooseXPics)
                    res.AddFrame(_xAnim.Frames[i].Picture, _xAnim.Frames[i].dX,
                        _yAnim.Frames[i].dY, _xAnim.Frames[i].Duration);
                else
                    res.AddFrame(_yAnim.Frames[i].Picture, _xAnim.Frames[i].dX,
                        _yAnim.Frames[i].dY, _yAnim.Frames[i].Duration);
            }

            return res;
        }

        public static Animation CreateFromFolder(
            GraphicsDevice graphicsDevice, string path, Entity source, 
            bool changeX, bool negative, bool same = false)
        {
            var files = System.IO.Directory.GetFiles(path)
                .Where(x => System.IO.Path.GetExtension(x) == ".png")
                .OrderBy(s => s);

            float dX, dY;
            if (changeX)
            {
                if (negative)
                    dX = -source.Speed;
                else
                    dX = source.Speed;
                dY = 0;
            }
            else
            {
                if (negative)
                    dY = -source.Jump;
                else
                    dY = source.Jump;

                dX = 0;
            }

            Animation res = new Animation(source);

            Texture2D temp = null;
            foreach(var fileName in files)
            {
                if (same)
                {
                    if (temp == null)
                    {
                        using (var stream = TitleContainer.OpenStream(fileName))
                        {
                            temp = Texture2D.FromStream(graphicsDevice, stream);
                        }
                    }

                    res.AddFrame(temp, dX, dY);
                }
                else
                { 
                    res.AddFrame(graphicsDevice, fileName, dX, dY);
                }
            }

            return res;
        }

        public static Animation CreateFromPhoto(Texture2D imgSource,
            Entity source, bool changeX, bool negative,
            float width, float height, int count, float duration, int perLine,
            bool flatten = false, int offset = 0)
        {
            if (imgSource == null)
                throw new InvalidOperationException("Cannot create animation from null");

            float dX, dY;
            if (flatten)
                dX = dY = 0;
            else
            {
                if (changeX)
                {
                    if (negative)
                        dX = -source.Speed;
                    else
                        dX = source.Speed;

                    dY = 0;
                }
                else
                {
                    if (negative)
                        dY = -source.Jump;
                    else
                        dY = source.Jump;

                    dX = 0;
                }
            }

            //width = imgSource.Width / count;

            float tNext = duration / (count - offset);
            float frameDuration = Constants.DefaultAnimationFrameDuration;

            Rectangle rect = new Rectangle(0, 0, (int)width, (int)height);

            Animation res = new Animation(source, true);
            for (int i = 0; i < count; i++)
            {
                if (i % perLine == 0 && i > 0)
                {
                    rect.Y += (int)height;
                    rect.X = 0;
                }
                if (i >= offset)
                {
                    res.AddFrame(rect, tNext, dX, dY, frameDuration);
                }
                rect.X += (int)width;
            }
            res.ImageSource = imgSource;

            return res;
        }

        public static Animation CreateFromTextureMap(Entity source, Texture2D imgSource, List<Rectangle> rectangles,
            bool changeX, bool negative, float duration)
        {
            float dX, dY;
            if (changeX)
            {
                if (negative)
                    dX = -source.Speed;
                else
                    dX = source.Speed;
                dY = 0;
            }
            else
            {
                if (negative)
                    dY = -source.Jump;
                else
                    dY = source.Jump;

                dX = 0;
            }

            float frameDuration = duration / (float)rectangles.Count;

            Animation res = new Animation(source, true);

            foreach(var rect in rectangles)
            {
                res.AddFrame(rect, dX, dY, frameDuration);   
            }

            return res;
        }
    }
}
