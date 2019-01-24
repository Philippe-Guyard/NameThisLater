using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using FirstGameEngine.Animations;

namespace FirstGameEngine.Entities.Monsters
{
    public class MonsterEntity : AdvancedEntity
    {
        private float _dist;

        public float Distance
        {
            get
            {               
                return _dist;
            }
            set
            {
                if (_dist != value)
                {
                    _dist = value;
                    Left = BaseX - _dist;
                    Right = BaseX + _dist;
                }
            }
        }
        
        public float Radius { get; set; }
        public float AttackRange { get; set; }

        public float Damage { get; set; }

        public float Left { get; protected set; }
        public float Right { get; protected set; }

        public MonsterType Type { get; set; }
        public MonsterBehaviorType BehaviorType { get; set; }
        
        protected bool ShouldDraw { get; set; }
        protected int _frameCount;

        private float CurrentDistance { get; set; }
        
        private float BaseX { get; set; }
        
        protected MovementDirection Direction { get; set; }
        
        protected bool HeroInPursueRange(HeroEntity h) => Math.Abs(h.GetRealX() - GetRealX()) <= Radius;
        protected bool HeroInAttackRange(HeroEntity h) => Math.Abs(h.GetRealX() - GetRealX()) <= AttackRange;

        public MonsterEntity(MonsterType type, float x = 0, float y = 0,
            float layer_depth = Constants.MonsterLayerDepth) : base(x, y, layer_depth)
        {
            this.Type = type;
            Direction = MovementDirection.Right;

            ShouldDraw = true;
            _frameCount = 0;

            BaseX = X;
        }

        public virtual void LoadContent(ContentManager contentManager)
        {
            base.LoadHP(contentManager, TextureManager.HPFolder);
            Image = TextureManager.GetImage(this);
            
            this.InvokeLoad();
        }
        
        public void InitializeAnimationsAsDefault()
        {
            AnimationHit = TextureManager.GetAnimation(this, "Pain");
            AnimationDie = TextureManager.GetAnimation(this, "Death");
            AnimationRight = TextureManager.GetAnimation(this, "Walk");
            AnimationCoin = TextureManager.GetCoinAnimation(this);

            AnimationLeft = AnimationRight.Reverse();

            AnimationHit.Priority = 1f;
            AnimationDie.Priority = 1f;
            AnimationRight.Priority = AnimationLeft.Priority = 0.7f;
        }
        
        public virtual void Update(GameTime gameTime, HeroEntity hero)
        {
            bool flag; 
            base.Update(gameTime, out flag);
            if (!flag || this.Destroyed || HealthPoints <= 0)
                return;

            Vector2 vector;
            switch (this.BehaviorType)
            { 
                case MonsterBehaviorType.Patrol:
                    if (HeroInAttackRange(hero))
                    {
                        if (hero.GetRealX() < GetRealX())
                            Effects = SpriteEffects.FlipHorizontally;
                        if (hero.GetRealX() > GetRealX())
                            Effects = SpriteEffects.None;

                        //if (!hero.IsInvincible)
                        Attack(gameTime, hero);

                        return;
                    }

                    var dir = Direction;
                    if (GetRealX() >= Right)
                        Direction = MovementDirection.Left;
                    if (GetRealX() <= Left)
                        Direction = MovementDirection.Right;

                    Move(gameTime, hero);

                    if (Direction != dir)
                    {
                        ShouldDraw = false;
                        _frameCount = 0;
                    }

                    break;
                case MonsterBehaviorType.Pursue:
                    if (HeroInAttackRange(hero))
                    {
                        if (hero.GetRealX() < GetRealX())
                            Effects = SpriteEffects.FlipHorizontally;
                        if (hero.GetRealX() > GetRealX())
                            Effects = SpriteEffects.None;

                        //if (!hero.IsInvincible)
                        Attack(gameTime, hero);

                        return;
                    }
                    else if (HeroInPursueRange(hero))
                    {
                        if (hero.GetRealX() < GetRealX())
                            Direction = MovementDirection.Left;
                        if (hero.GetRealX() > GetRealX())
                            Direction = MovementDirection.Right;
                    }
                    else
                    {
                        if (GetRealX() >= Right)
                            Direction = MovementDirection.Left;
                        if (GetRealX() <= Left)
                            Direction = MovementDirection.Right;
                    }

                    if (GetRealX() <= Right && Direction == MovementDirection.Right)
                    {
                        Move(gameTime, hero);
                    }
                    else if (GetRealX() >= Left && Direction == MovementDirection.Left)
                    {
                        Move(gameTime, hero);
                    }
                    else if 
                        (
                            (GetRealX() <= Left && Direction == MovementDirection.Left) //Can't go left but needs to
                                                ||
                            (GetRealX() >= Right && Direction == MovementDirection.Right) //Can't go right but needs to
                        )
                    {
                        WaitForHero(gameTime);
                    }

                    break;
            }

            if (Direction == MovementDirection.Left)
                Effects = SpriteEffects.FlipHorizontally;
            else
                Effects = SpriteEffects.None;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle? destinationRectangle = null,
            bool beginDraw = false, bool endDraw = false, SpriteEffects effects = SpriteEffects.None)
        {
            if (!CanDraw)
            {
                base.Draw(spriteBatch, destinationRectangle, beginDraw, endDraw, effects);
                return;
            }
            if (ShouldDraw)
            {
                effects = this.Effects;
                base.Draw(spriteBatch, destinationRectangle, beginDraw, endDraw, effects);
            }

            ++_frameCount;
            if (_frameCount > 0) ShouldDraw = true;
        }

        public virtual void Move(GameTime gameTime, HeroEntity heroEntity)
        {
            Vector2 v;
            bool flag;
            Move(Direction, gameTime, out v, out flag);
        }

        public virtual void WaitForHero(GameTime gameTime)
        {
            //No idle animations, so just stay draw the main texture
            SourceRectangle = null;
        }
        
        public virtual void Attack(GameTime gameTime, HeroEntity hero)
        {
            //Just do nothing, hero HP is handled elsewhere
        }
    }
}

