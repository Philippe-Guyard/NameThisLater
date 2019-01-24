using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace FirstGameEngine.Entities
{
    public class WeaponEntity : Entity
    {
        public const string BulletPath = "BULLET";

        private bool PlayRechargeSound = true; //Wheter or not the recharge sound should be played

        public List<BulletEntity> Bullets { get; set; }
        public Texture2D BulletsImage { get; set; }

        public float FireRate { get; set; }
        public float RechargeTime { get; set; } 
        public float BulletDamage { get; set; }
    
        public bool NeedsRecharge { get; set; }
        public bool Preferred { get; set; }

        public int BulletLifespan { get; set; }

        public float TimeBetweenBullets { get; set; }
        public Vector2 BulletSpeed { get; set; }

        public Vector2? BulletStartPos { get; set; }

        public MySoundEffect RechargeEffect { get; set; }
        
        public MySoundEffect ShootingEffect { get; set; }

        private float _lastCall;
        private int _currFireRate;

        public float UsedFireRate => (float)_currFireRate / FireRate;

        public WeaponEntity(float x = 0, float y = 0, 
            float _layerDepth = Constants.ObstaclesLayerDepth + 0.01f) : base(x, y, _layerDepth)
        {
            this.Bullets = new List<BulletEntity>();

            _lastCall = _currFireRate = 0;
        }

        public void Fire(GameTime gameTime, out bool canFire)
        {
            canFire = false;
            float elapsed = (float)gameTime.TotalGameTime.TotalMilliseconds - _lastCall;
            if (elapsed >= RechargeTime && this.NeedsRecharge && this.UsedFireRate == 1f)
                this.Recharge();

            if ( (this._currFireRate < FireRate || !this.NeedsRecharge) && elapsed > TimeBetweenBullets)
            {
                Vector2 bpos = BulletStartPos.HasValue ? BulletStartPos.Value : 
                    new Vector2(this.X + this.GetRealWidth(), this.Y + this.GetRealHeight() / 2);

                BulletEntity b = new BulletEntity(BulletSpeed, bpos.X, bpos.Y);
                b.Y -= this.BulletsImage.Height / 2;

                b.Jump = BulletSpeed.Y;
                b.Speed = BulletSpeed.X;
                
                b.Image = this.BulletsImage;

                b.Damage = this.BulletDamage;

                b.MaxLifeSpan = this.BulletLifespan;

                Bullets.Add(b);

                canFire = true;
                this._currFireRate++;
                this._lastCall = (float)gameTime.TotalGameTime.TotalMilliseconds;

                if (Settings.SoundFX)
                {
                    if (ShootingEffect != null)
                    {
                        ShootingEffect.Play();
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (this.NeedsRecharge && this.UsedFireRate == 1f)
            {
                if (Settings.SoundFX)
                {
                    if (PlayRechargeSound)
                    {
                        RechargeEffect.Play();
                        PlayRechargeSound = false;
                    }
                }
            }

            float elapsed = (float)gameTime.TotalGameTime.TotalMilliseconds - _lastCall;
            if (elapsed >= RechargeTime && this.NeedsRecharge && this.UsedFireRate == 1f)
                this.Recharge();

            Bullets = Bullets.Where(x => x.Active).ToList();

            foreach (var bullet in Bullets)
                bullet.Update();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle? destinationRectangle = null, 
            bool beginDraw = false, bool endDraw = false, SpriteEffects effects = SpriteEffects.None)
        {
            DrawBullets(spriteBatch);

            if (this.Image != null)
                base.Draw(spriteBatch, destinationRectangle, beginDraw, endDraw, effects);
        }

        public void Recharge()
        {
            this._currFireRate = 0;
            this._lastCall = 0;

            PlayRechargeSound = true;
        }

        public void DrawBullets(SpriteBatch spriteBatch)
        {
            foreach (var bullet in Bullets)
                bullet.Draw(spriteBatch);
        }
    }

    public class BulletEntity : Entity
    {
        public float Damage { get; set; }
        public bool Active { get; set; }
        public int MaxLifeSpan { get; set; }

        private int CurrentLifeSpan { get; set; }

        readonly Vector2 Direction;

        public BulletEntity(Vector2 dir, float x = 0, float y = 0, 
            float _layerDepth = Constants.BulletsLayerDepth) : base(x, y, _layerDepth)
        {
            this.Direction = Vector2.Normalize(dir);
            this.Active = true;

            CurrentLifeSpan = 0;
        }

        public void Update()
        {
            var move = Math.Abs(this.Direction.X * this.Speed);
            if (Direction.X < 0) move *= -1;
            this.X += (int)move;
            this.Y += (int)(this.Direction.Y * this.Jump);
            
            CurrentLifeSpan++;
            if (CurrentLifeSpan > MaxLifeSpan)
                this.Active = false;
     //       Console.WriteLine("{0} {1}", this.X, this.Y);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle? destinationRectangle = null, 
            bool beginDraw = false, bool endDraw = false, SpriteEffects effects = SpriteEffects.None)
        {
            if (this.Active)
                base.Draw(spriteBatch, destinationRectangle, beginDraw, endDraw, effects);
        }
    }
}
