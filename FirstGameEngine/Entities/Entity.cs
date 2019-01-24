using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using FirstGameEngine.Animations;

namespace FirstGameEngine.Entities
{
    public class Entity
    {
        private float _speed, _jump;
        private int _x, _y;

        public delegate void ImageLoadStateHandler(Entity entity);

        public event ImageLoadStateHandler ImageLoaded;
        
        public float? FloorY { get; set; }

        public float LayerDepth { get; protected set; }

		protected Color ColorMask = Color.White;

        public virtual int X
        {
            get { return _x; }
            set
            {
                if (_x != value)
                {
                    TotalChanges += new Vector2(value - _x, 0);

                    PrevX = _x;
                    _x = value;
                }
            }
        }
        public virtual int Y
        {
            get { return _y; }
            set
            {
                if (_y != value)
                {
                    if(this.AttachedToCamera)
                    {
                        Camera.Move(new Vector2(0, (value - _y) ));
                    }

                    TotalChanges += new Vector2(0, value - _y);

                    PrevY = _y;
                    _y = value;
                }
            }
        }
        
        public float PrevY { get; set; }
        public float PrevX { get; set; }
        
        public virtual float Speed
        {
            get { return _speed; }
            set
            {
                if (value != _speed)
                {
                    ChangeSpeed(_speed, value);
                    _speed = value;
                }
            }
        }

        public virtual float Jump
        {
            get { return _jump; }
            set
            {
                if (value != _jump)
                {
                    ChangeJump(_jump, value);
                    _jump = value;
                }
            }
        }

        public float? RealHeight { protected get; set; }
        public float? RealWidth { protected get; set; }
        public int? RealX { protected get; set; }
        public int? RealY { protected get; set; }

        public Texture2D Image { get; set; }
        public virtual Rectangle? SourceRectangle { get; set; }

        public string Tag;

        public Camera2D Camera { get; set; }
        public bool AttachedToCamera { get; set; }
        public int MaxY { get; set; }

        public bool Visible { get; set; }

        protected virtual Animation AnimationRight { get; set; }
        protected virtual Animation AnimationLeft { get; set; }
        protected virtual Animation AnimationJump { get; set; }
        protected virtual Animation AnimationFall { get; set; }

        protected Vector2 TotalChanges { get; set; }

        public long LastAnimationCall { get; set; }

        Texture2D LastImageSource;
        protected Animation LastAnimation;

        public virtual float Left => GetRealX();
        public virtual float Right => GetRealX() + GetRealWidth();
        public virtual float Up => GetRealY();
        public virtual float Down => GetRealY() + GetRealHeight();

        public Vector2 Center => new Vector2(X + this.GetRealWidth() / 2, Y + this.GetRealHeight() / 2);

        public Rectangle CollisionMask => new Rectangle
            (
                (int)GetRealX(), (int)GetRealY(),
                (int)GetRealWidth(), (int)GetRealHeight()
            );

        public Rectangle BoundingRectangle => new Rectangle
            (
                (int)X, (int)Y, (int)GetRealWidth(), (int)GetRealHeight()
            );

        public Entity(Texture2D _image, float _x = 0, float _y = 0, float? _layerDepth = null)
        {
            Image = _image;

            DefaultInitialize(_x, _y, _layerDepth);
        }

        public Entity(GraphicsDevice graphicsDevice, string _pic,
            float _x = 0, float _y = 0, float? _layerDepth = null)
        {
            using (var stream = TitleContainer.OpenStream(_pic))
            {
                Image = Texture2D.FromStream(graphicsDevice, stream);
            }

            DefaultInitialize(_x, _y, _layerDepth);
        }

        public Entity(float x = 0, float y = 0, float? _layerDepth = null)
        {
            DefaultInitialize(x, y, _layerDepth);

            Image = null;
        }

        private void DefaultInitialize(float x, float y, float? _layerDepth)
        {
            this.X = (int)x;
            this.Y = (int)y;

            if (_layerDepth == null)
                LayerDepth = Constants.ForeGroundLayerDepth;
            else
                LayerDepth = (float)_layerDepth;

            Speed = Constants.EntitySpeed;
            Jump = Constants.EntityJump;

            LastAnimationCall = 0;

            LastImageSource = null;

            RealHeight = RealWidth = null;
            RealX = RealY = null;

            TotalChanges = Vector2.Zero;

            LastAnimation = null;

            Visible = true;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle? destinationRectangle = null,
            bool beginDraw = false, bool endDraw = false, SpriteEffects effects = SpriteEffects.None)
        {
            if (Image == null)
            {
                throw new InvalidOperationException("Image wasn't loaded");
            }

            if (beginDraw)
                spriteBatch.Begin();

            Rectangle destRect;
            if (!destinationRectangle.HasValue)
            {
                int width = this.Image.Width;
                int height = this.Image.Height;
                if (this.SourceRectangle.HasValue)
                {
                    width = this.SourceRectangle.Value.Width;
                    height = this.SourceRectangle.Value.Height;
                }

                destRect = new Rectangle((int)X, (int)Y, width, height);
            }
            else
            {
                destRect = destinationRectangle.Value;
            }

            if (SourceRectangle == null || LastImageSource == null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                spriteBatch.Draw(Image, destinationRectangle: destRect, color: ColorMask, layerDepth: LayerDepth, effects: effects);
            }
            else
            {
                spriteBatch.Draw(LastImageSource, destinationRectangle: destRect, color: ColorMask,
                  sourceRectangle: SourceRectangle, layerDepth: LayerDepth, effects: effects);

                //           LastAnimation.Draw(spriteBatch, (int)X, (int)Y, LayerDepth);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (endDraw)
                spriteBatch.End();
        }

        public float GetRealHeight()
        {
            if (this.RealHeight.HasValue)
                return this.RealHeight.Value;

            return this.SourceRectangle == null ? this.Image.Height : SourceRectangle.Value.Height;
        }

        public float GetRealWidth()
        {
            if (this.RealWidth.HasValue)
                return this.RealWidth.Value;

            return this.SourceRectangle == null ? this.Image.Width : SourceRectangle.Value.Width;
        }

        public virtual int GetRealX()
        {
            return RealX.HasValue ? this.X + this.RealX.Value : this.X;
        }

        public virtual int GetRealY()
        {
            return RealY.HasValue ? this.Y + this.RealY.Value : this.Y;
        }

        public void Move(MovementDirection movementDirection, GameTime gameTime,
            out Vector2 change, out bool fullCycle)
        {
            if (Image == null)
                throw new InvalidOperationException("Image wasn't loaded");

            switch (movementDirection)
            {
                case MovementDirection.Down:
                    Fall(gameTime, out change, out fullCycle);
                    break;
                case MovementDirection.Up:
                    MoveUp(gameTime, out change, out fullCycle);
                    break;
                case MovementDirection.Right:
                    MoveRight(gameTime, out change, out fullCycle);
                    break;
                case MovementDirection.Left:
                    MoveLeft(gameTime, out change, out fullCycle);
                    break;
                default:
                    change = new Vector2(0, 0);
                    fullCycle = false;
                    break;
            }
        }

        private void MoveRight(GameTime gameTime, out Vector2 change, out bool fullCycle)
        {
            PlayAnimation(AnimationRight, gameTime, out change, out fullCycle);
        }

        private void MoveLeft(GameTime gameTime, out Vector2 change, out bool fullCycle)
        {
            PlayAnimation(AnimationLeft, gameTime, out change, out fullCycle);
        }

        private void MoveUp(GameTime gameTime, out Vector2 change, out bool fullCycle)
        {
            PlayAnimation(AnimationJump, gameTime, out change, out fullCycle);
        }

        private void Fall(GameTime gameTime, out Vector2 change, out bool fullCycle)
        {
            PlayAnimation(AnimationFall, gameTime, out change, out fullCycle);
        }

        protected void PlayAnimation(Animation animation, GameTime gameTime, 
            out Vector2 change, out bool fullCycle)
        {

            Texture2D nextImage;
            Rectangle? nextRect;
            var nextPos = animation.GetNextPosition(out nextImage, out nextRect,
                SourceRectangle, gameTime, out fullCycle);

            float xChange = X - nextPos.X;
            float yChange = Y - nextPos.Y;
            change = new Vector2(xChange, yChange);

            //Update corrdinatinates
            X = (int)Math.Round(nextPos.X);
            Y = (int)Math.Round(nextPos.Y);
            
            //Only update the picture if the new priority if bigger than the old one
            float prior = LastAnimation == null ? -1 : LastAnimation.Priority;
            if (animation.Priority >= prior)
            {
                SourceRectangle = nextRect;
                LastImageSource = animation.ImageSource;
                LastAnimation = animation;
            }

            LastAnimationCall = (long)gameTime.TotalGameTime.TotalMilliseconds;
        }

        private void ChangeSpeed(float prevSpeed, float newSpeed)
        {
            if (AnimationRight != null)
                AnimationRight.ChangeSpeed(newSpeed - prevSpeed, 0);
            if (AnimationLeft != null)
                AnimationLeft.ChangeSpeed(prevSpeed - newSpeed, 0);
        }

        private void ChangeJump(float prevJump, float newJump)
        {
            if (AnimationJump != null)
                AnimationJump.ChangeSpeed(0, prevJump - newJump);
            if (AnimationFall != null)
                AnimationFall.ChangeSpeed(0, newJump - prevJump);
        }

        public virtual Entity Copy()
        {
            return (Entity)this.MemberwiseClone();
        }

        public virtual void InitializeAnimationsAsDefault(
            bool right = true, bool left = true, bool jump = true, bool fall = true)
        {
            if (Image == null)
                throw new InvalidOperationException("Image wasn't loaded");

            if (right)
            {
                AnimationRight = new Animation(this);
                AnimationRight.AddFrame(this.Image, this.Speed);
                AnimationRight.Priority = 0.3f;
            }

            if (left)
            {
                AnimationLeft = new Animation(this);
                AnimationLeft.AddFrame(this.Image, this.Speed * -1);
                AnimationLeft.Priority = 0.3f;
            }

            if (jump)
            {
                AnimationJump = new Animation(this);
                AnimationJump.AddFrame(this.Image, _y: this.Jump * -1);
                AnimationJump.Priority = 1f;
            }

            if (fall)
            {
                AnimationFall = new Animation(this);
                AnimationFall.AddFrame(this.Image, _y: this.Jump);
                AnimationFall.Priority = 1f;
            }
        }

        public virtual void LoadImage(Microsoft.Xna.Framework.Content.ContentManager contentManager,
            string pic)
        {
            if (this.Image == null)
                Image = contentManager.Load<Texture2D>(pic);

            ImageLoaded?.Invoke(this);
        }

        public virtual void InvokeLoad()
        {
            ImageLoaded?.Invoke(this);
        }

        public virtual void LoadImage(GraphicsDevice graphicsDevice, string pic)
        {
            using (var stream = TitleContainer.OpenStream(pic))
            {
                Image = Texture2D.FromStream(graphicsDevice, stream);
            }

            ImageLoaded?.Invoke(this);
        }
    }
}
