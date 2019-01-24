using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FirstGameEngine.Animations;

namespace FirstGameEngine.Entities.Monsters
{
    public class FiringMonsterEntity : MonsterEntity
    {
        public WeaponEntity Weapon { get; protected set; }
        
        public bool IsShooting { get; protected set; } //Guard prepares to shoot
        public bool IsRecoiling { get; protected set; } //Recharge
        public bool IsFiring { get; protected set; } //Fire animation occuring now 
        
        protected Animation ShootingAnimation { get; set; }
        protected Animation RecoilAnimation { get; set; }
        
        public FiringMonsterEntity(MonsterType type, float x, float y) : base(type, x, y)
        {
            this.Weapon = new WeaponEntity(x, y);
            this.Weapon.NeedsRecharge = false;
            this.Weapon.BulletDamage = 1;
            this.Weapon.TimeBetweenBullets = 700;
            this.Weapon.BulletLifespan = 60;
            this.Weapon.BulletSpeed = new Vector2(3 * Constants.EntitySpeed, 0);
            this.Distance = 40;

            IsShooting = false;
            IsRecoiling = false;
        }
        
        protected override void Hit(float deltaHp)
        {
            this.IsShooting = false;
            base.Hit(deltaHp);
        }
 
        public override void LoadContent(ContentManager contentManager)
        {  
            Weapon.LoadImage(contentManager, "gun");
            Weapon.BulletsImage = contentManager.Load<Texture2D>("bullet");
            
            ShootingAnimation = TextureManager.GetAnimation(this, "Attack");
            RecoilAnimation = TextureManager.GetAnimation(this, "Recoil");

            ShootingAnimation.Priority = 1f;
            RecoilAnimation.Priority = 0.99f;
            
            base.LoadContent(contentManager);
        }

        public override void Update(GameTime gameTime, HeroEntity hero)
        {
            if (Weapon.BulletSpeed.X < 0 && this.Effects == SpriteEffects.None
                || Weapon.BulletSpeed.X > 0 && this.Effects == SpriteEffects.FlipHorizontally)
                Weapon.BulletSpeed *= -1;

            base.Update(gameTime, hero);

            if (Direction == MovementDirection.Right)
                Weapon.X = this.X + (int)this.GetRealWidth() - 25;
            else
                Weapon.X = this.X;

            Weapon.Y = this.Y + (int)this.GetRealHeight() / 2 + 3;

            Weapon.Update(gameTime);
        }

        public override void Attack(GameTime gameTime, HeroEntity hero)
        {
            Vector2 v;
            if (!IsFiring)
            {
                IsShooting = true;
                bool flag;
                base.PlayAnimation(ShootingAnimation, gameTime, out v, out flag);
                if (flag)
                {
                    IsShooting = false;
                    IsFiring = true;
                    Weapon.Fire(gameTime, out flag);
                }
            }
            
            if (IsFiring)
            {
                bool flag;
                if (IsRecoiling)
                {
                    base.PlayAnimation(RecoilAnimation, gameTime, out v, out flag);
                    if (flag)
                    {
                        IsRecoiling = false;
                    }
                    else
                        return;
                }
                
                Weapon.Fire(gameTime, out flag);
                IsRecoiling = true;
            }
        }

        public override void Move(GameTime gameTime, HeroEntity heroEntity)
        {
            base.Move(gameTime, heroEntity);

            IsFiring = false;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle? destinationRectangle = null, bool beginDraw = false, bool endDraw = false, SpriteEffects effects = SpriteEffects.None)
        {
            Weapon.DrawBullets(spriteBatch);

            base.Draw(spriteBatch, destinationRectangle, beginDraw, endDraw, effects);
        }
    }
}

