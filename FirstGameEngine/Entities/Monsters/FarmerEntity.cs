using System;
using System.Collections.Generic;
using System.Text;

using FirstGameEngine.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FirstGameEngine.Entities.Monsters
{
    public class FarmerEntity : MonsterEntity
    {
        const int AttackCooldown = 950;
		public const int WaitInterval = 1000;

        private Animation RunRightAnimation { get; set; }
        private Animation RunLeftAnimation { get; set; }
        private Animation AngryAnimation { get; set; }
        private Animation PrepareAnimation { get; set; }
        private Animation AttackAnimation { get; set; }
        
        private bool AngerPlayed;
        private bool Preparing;

        private double LastAttackTime = 0;
		public double CurrentWaitInterval = 0;

        public bool DidHitHero { get; set; }
        public Rectangle? DamageCollisionMask { get; private set; } //Rectangle for collision when atacking

        public bool Attacking => DamageCollisionMask != null;

        public FarmerEntity(float x = 0, float y = 0, float layer_depth = 0.71F) 
            : base(MonsterType.Farmer, x, y, layer_depth)
        {
            this.RealX = 18;
            this.RealY = 9;
            this.RealWidth = 26;
            this.RealHeight = 53;

            this.BehaviorType = MonsterBehaviorType.Pursue;

            AngerPlayed = false;
            Preparing = false;
            DidHitHero = false;
        }

        public override void LoadContent(ContentManager contentManager)
        {
            base.LoadContent(contentManager);

            RunRightAnimation = TextureManager.GetAnimation(this, "Run");
            AngryAnimation = TextureManager.GetAnimation(this, "Angry");
            PrepareAnimation = TextureManager.GetAnimation(this, "Prepare");
            AttackAnimation = TextureManager.GetAnimation(this, "Attack");

            RunLeftAnimation = RunRightAnimation.Reverse();

            AngryAnimation.Priority = 0.9f;
            AttackAnimation.Priority = 1f;
            PrepareAnimation.Priority = 1f;

            RunRightAnimation.Priority = RunLeftAnimation.Priority = 0.7f;
        }

        public override void Move(GameTime gameTime, HeroEntity heroEntity)
        {
            Vector2 vct;
            bool flag;
            if (this.BehaviorType == MonsterBehaviorType.Pursue && HeroInPursueRange(heroEntity))
            {
                if (Direction == MovementDirection.Right)
                    PlayAnimation(RunRightAnimation, gameTime, out vct, out flag);
                else
                {
                    PlayAnimation(RunLeftAnimation, gameTime, out vct, out flag);
                }
            }
            else
                base.Move(gameTime, heroEntity);

            AngerPlayed = false;
        }

        public override void WaitForHero(GameTime gameTime)
        {
            if (!AngerPlayed)
            {
                Vector2 vct;
                bool flag;
                base.PlayAnimation(AngryAnimation, gameTime, out vct, out flag);
                if (flag)
                    AngerPlayed = true;
            }
            else
                base.WaitForHero(gameTime);
        }

        public override void Attack(GameTime gameTime, HeroEntity hero)
        {
            if(gameTime.TotalGameTime.TotalMilliseconds - LastAttackTime < AttackCooldown && DamageCollisionMask == null)
            {
                LastAnimation = null;
                return;
            }
            if (gameTime.TotalGameTime.TotalMilliseconds - LastAttackTime >= 70)
            {			
                DidHitHero = false;
                Preparing = false;
                DamageCollisionMask = null;
            }

            if (!Attacking && !Preparing)
            {
                if (hero.IsInvincible)
                    return;
				//The movement has just been started
				CurrentWaitInterval = 0;
                Preparing = true;
            }

			LastAttackTime = gameTime.TotalGameTime.TotalMilliseconds;
			CurrentWaitInterval += gameTime.ElapsedGameTime.TotalMilliseconds;
			if(Preparing)
            {
				/*
                //Get in position to attack
                Vector2 vct;
                bool flag; //Was the animation finished

                base.PlayAnimation(AttackAnimation, gameTime, out vct, out flag);
*/		
                if (true) /* if (flag) */
                {
                    Preparing = false;
                    //set the damage collision mask, which makes Attacking = true
                    if (this.Effects == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally)
                    {
                        DamageCollisionMask = new Rectangle(
                            (int)X,
                            (int)Center.Y - 10,
                            60, 24);
                    }
                    else
                    {
                        DamageCollisionMask = new Rectangle(
                            (int)X + Image.Width - 48,
                            (int)Center.Y - 10,
                            60, 20);
                    }

                    AttackAnimation.ForceReset(gameTime);
                }
            }
            
            //Do the attacking movement
            bool flag;
            Vector2 vct;

            base.PlayAnimation(AttackAnimation, gameTime, out vct, out flag);

            if (flag) //Reset the hole movement
            {
                DidHitHero = false;
                Preparing = false;
                DamageCollisionMask = null;
            }
        }
    }
}
