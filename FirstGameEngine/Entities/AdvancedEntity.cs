using System;
using System.Timers;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using FirstGameEngine.Animations;
using FirstGameEngine.Entities.Monsters;

#pragma warning disable CS0618 // Type or member is obsolete

namespace FirstGameEngine.Entities
{
    //Base class for all monsters
    //TODO: Implement alert animation here
    public abstract class AdvancedEntity : Entity
    {
        const int CoinHeight = 16;
        const int CointWidth = 15;
        
        private float _healthPts;
        
        public float HealthPoints
        {
            get { return _healthPts; }
            set
            {
                if (InHit) return;
                if (_healthPts < value && value <= MaxHP)
                    _healthPts = value;
                else if (_healthPts != value)
                {
                    Hit(_healthPts - value);
                    _healthPts = value;
                }
            }
        }

        public float MaxHP { get; set; }

        public override float Jump
        {
            get => base.Jump;
            set
            {
                if (!BaseJump.HasValue)
                    BaseJump = value;

                base.Jump = value;
            }
        }

        public override float Speed
        {
            get => base.Speed;
            set
            {
                if (!BaseSpeed.HasValue)
                    BaseSpeed = value;

                base.Speed = value;
            }
        }

        private float? BaseJump;
        private float? BaseSpeed;
        
        protected SoundEffect HitEffect { get; set; }
        protected SoundEffect DieEffect { get; set; }

        protected Animation AnimationHit { get; set; }
        protected Animation AnimationDie { get; set; }
        protected Animation AnimationCoin { get; set; }

        protected SpriteEffects Effects { get; set; }

        public Entity Floor { get; set; }

        protected BarManager HpManager { get; set; }
        protected AlertManager AlertManager { get; set; }

        public bool Destroyed { get; protected set; }
        public bool InHit { get; protected set; }
        public bool CanDraw { get; protected set; }

        public bool InMarsh { private get; set; }

        public bool Active => HealthPoints > 0;
        
        public AdvancedEntity(float x = 0, float y = 0, float? _layerDepth = Constants.MonsterLayerDepth)
            : base(x, y, _layerDepth)
        {
            Destroyed = InHit = false;
            CanDraw = true;
            
            HpManager = new BarManager(this);

            InMarsh = false;
        }
        
        public virtual void LoadHP(ContentManager contentManager, string folder)
        {
            HpManager.LoadContent(contentManager, folder);
        }

        protected virtual void Alert(GameTime gameTime)
        {
            if (AlertManager == null)
                AlertManager = new AlertManager(this);

            AlertManager.StartAlert(gameTime);
        }
        
        public virtual void Update(GameTime gameTime, out bool canMove)
        {
            if (LastAnimation != null && LastAnimation.IsResetable(gameTime))
                LastAnimation = null;
            if (AlertManager != null && AlertManager.AlertStarted)
                AlertManager.Update(gameTime);

            if (InMarsh)
            {
                Speed = BaseSpeed.Value * 0.6f;
            }

            Vector2 vector;
            bool flag;
            canMove = true;
            if (!this.Active)
            {
                if (!Destroyed)
                {
                    base.PlayAnimation(AnimationDie, gameTime, out vector, out flag);
                    Destroyed = flag;
                    if (flag)
                    {
                        this.X += (int)this.GetRealWidth() / 2;
                        CanDraw = false;
                        base.RealWidth = CointWidth;
                        base.RealHeight = CoinHeight;
                    }
                }
                else
                {
                    if (this.Y + CoinHeight < this.FloorY)
                    {
                        this.Y += 1;
                    }
                    base.PlayAnimation(AnimationCoin, gameTime, out vector, out flag);
                }
                canMove = false;
            }
            else if (InHit)
            {
                base.PlayAnimation(AnimationHit, gameTime, out vector, out flag);
                InHit = !flag;
                canMove = false;
            }
        }

        protected virtual void Hit(float deltaHp)
        {
            InHit = true;
            if (HitEffect != null && Settings.SoundFX)
                HitEffect.Play();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle? destinationRectangle = null,
            bool beginDraw = false, bool endDraw = false, SpriteEffects effects = SpriteEffects.None)
        {
            if (AlertManager != null && AlertManager.AlertStarted)
                AlertManager.Draw(spriteBatch);

            if (Destroyed)
            {
                spriteBatch.Draw
                    (
                        texture: TextureManager.CoinAtlas,
                        position: new Vector2(X, Y),
                        sourceRectangle: AnimationCoin.CurrentFrame.SourceRectangle,
                        layerDepth: 0.99f
                    );
            }
            else
            {
                HpManager.Draw(spriteBatch);

                base.Draw(spriteBatch, destinationRectangle, beginDraw, endDraw, effects);
            }
        }


        public class BarManager
        {
            const float defaultHeight = 4;
            const float defaultYOff = -4;
            const float depth = 0.9f;

            AdvancedEntity Source;

            public float Width { get; set; }
            public float Height { get; set; }
            public float YOffset { get; set; }

            Texture2D Left, Right;
            Texture2D Back, Front;

            public BarManager(AdvancedEntity _source, float _height = defaultHeight)
            {
                Source = _source;
                Width = 30;//_source.GetRealWidth() / 5 * 3;
                Height = _height;

                YOffset = defaultYOff;
            }

            public void LoadContent(ContentManager contentManager, string folder)
            {
                Right = contentManager.Load<Texture2D>(folder + "/right");
                Left = contentManager.Load<Texture2D>(folder + "/left");
                Front = contentManager.Load<Texture2D>(folder + "/front");
                Back = contentManager.Load<Texture2D>(folder + "/back");

                Height = Front.Height;
                Width = Front.Width;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                int w = (int)Width;
                int h = (int)Height;

                int redW = (int)(Source.HealthPoints / Source.MaxHP * Width);
          
                Rectangle source = new Rectangle(0, 0, redW, Front.Height);
                
                float x = Source.GetRealX() + Source.GetRealWidth() / 2f - w / 2;
                if (Source.Effects == SpriteEffects.FlipHorizontally)
                    x = 2 * Source.GetRealX() + Source.GetRealWidth() - x - w / 2;
                Vector2 coor = new Vector2(x, Source.GetRealY() + YOffset);

#pragma warning disable CS0618 // Type or member is obsolete

                spriteBatch.Draw(Right, position: new Vector2(coor.X - Right.Width, coor.Y), layerDepth: depth);

                if (Source.HealthPoints == Source.MaxHP)
                {
                    spriteBatch.Draw(Left, position: new Vector2(coor.X + this.Width, coor.Y), layerDepth: depth);
                }

                spriteBatch.Draw(Back, position: new Vector2(coor.X - Right.Width - 1, coor.Y - 1), layerDepth: depth - 0.01f);

                spriteBatch.Draw(Front, position: coor, sourceRectangle: source, layerDepth: depth);

#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

    }

    public class AlertManager
    {
        private const int Height = 16;

        Animation AlertAnimation;
        AdvancedEntity Source;
        Rectangle SourceRectangle;

        public bool AlertStarted { get; private set; }

        public AlertManager(AdvancedEntity _source)
        {
            Source = _source;
            AlertAnimation = TextureManager.GetAlertAnimation(Source);
        }

        public void StartAlert(GameTime gameTime)
        {
            AlertAnimation.ForceReset(gameTime);

            SourceRectangle = new Rectangle(0, 0, 15, 16);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (AlertStarted)
            {
                Vector2 pos = new Vector2(Source.GetRealX() + Source.GetRealWidth() / 2, Source.GetRealY() - Height - 5);
                spriteBatch.Draw(TextureManager.AlertAtlas, 
                    sourceRectangle: SourceRectangle, position: pos, color: Color.White, layerDepth: 0.99f);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (AlertStarted)
            {
                bool flag;
                Texture2D next;
                Rectangle? rect;

                AlertAnimation.GetNextPosition(out next, out rect, SourceRectangle, gameTime, out flag);

                if (rect.HasValue)
                    SourceRectangle = rect.Value;
               
                if (flag)
                    AlertStarted = false;
            }
        }
    }
}
