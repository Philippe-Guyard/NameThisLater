using System;
using System.Linq;
using System.Timers;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

using FirstGameEngine.Animations;
using FirstGameEngine.Entities.Monsters;

using MonoGame.Extended;

namespace FirstGameEngine.Entities
{
    public class HeroEntity : Entity
    {
        public const string TexturePath = "HeroTexture";
		public const int CameraDelayFrames = 9;

        static Random RNG = new Random();

		private int _hpTicks = 0;
		private float _hp;

        public float HealthPoints
        {
            get { return _hp; }
            set
            {
                if (_hp == 0) //First set
                {
                    _hp = value;
                    return;
                }
                if (value != _hp && !IsInvincible)
                {
                    HeartManager.Hit(Math.Abs(_hp - value));
                    _hp = value;

                    if (Settings.SoundFX)
                    {
                        if (DamageEffects != null && DamageEffects.Count > 0)
                        {
                            DamageEffects[RNG.Next(DamageEffects.Count)].Play();
                        }
                    }

                    lastHit = 0;
                    this.ReceivingDamage = true;
                    IsInvincible = true;
                }
            }
        }

		private Queue<Vector2> LastPositions;

		public override int X
		{
			get => base.X;
			set
			{
				base.X = value;
			}
		}

		public override int Y
		{
			get => base.Y;
			set
			{
				base.Y = value;
			}
		}	

		public float Power { get; set; }
        public Entity Floor { get; set; }
        public Entity Lader { get; set; }
        public WeaponEntity CurrentWeapon { get; private set; }

        private HeartAnimator HeartManager { get; set; }
        private BulletManager BulletAnimator { get; set; }
		
        private int AxeHitPause = 0;

        private bool Falling { get; set; }
        private bool Jumping { get; set; }
        private bool Suspended { get; set; }
        private bool Climbing { get; set; }
        private bool Descending { get; set; }

        private bool PreparingShot { get; set; }
        private bool ReadyToShoot { get; set; }
        private bool Shooting { get; set; }

        private bool ReceivingDamage { get; set; }

        private float BaseJump;
        private float BaseSpeed;
        private float JumpMultiplier { get; set; }
        private float SpeedMultiplier { get; set; }

        private int WalkCount = -1;

        public float ClimbSpeed { get; set; }

        public bool OnTrilad { get; set; }
        public bool HasMoved { get; set; }
        public bool IsInAxeHit { get; set; }
        public bool InMarsh { get; private set; }
        public bool FinishedDying { get; private set; }
        public bool IsInvincible { get; private set; }

        public float Invicibility { get; set; }

        public float AxeDamage { get; set; }

        private float CurrentJumpHeight { get; set; }
        private float YClimbingDestination { get; set; }
        
        private List<ActionTypes> Unhandled { get; set; }

        private TriangularLader TriLader;
        private Entity Sun;
        private Entity Clouds;
        private StaticBackground Background;

        private SpriteEffects Effects;

        private Texture2D HeartTexture { get; set; }
        private Texture2D WithGunTexture { get; set; }

        private Texture2D IdleAtlas;
        private Texture2D RightAtlas;
        private Texture2D JumpAtlas;
        private Texture2D ShootAtlas;
        private Texture2D ToWeaponAtlas;
        private Texture2D AxeHitAtlas;
        private Texture2D DamageAxeAtlas;
        private Texture2D DamageShootingAtlas;
        private Texture2D DieAtlas;

        private Animation IdleAnimation;
        private Animation ShootingAnimation;
        private Animation ToWeaponAnimation;
        private Animation AxeHitAnimation;
        private Animation DamageAxeAnimation;
        private Animation DamageShootingAnimation;
        private Animation DieAnimation;

        private MySoundEffect JumpEffect;
        private List<MySoundEffect> DamageEffects;
        private MySoundEffect PrepairingToShootSound;
        private MySoundEffect AxeHitEffect;
        private MySoundEffect WalkingEffect;

        private double lastHit = 0;
        
        public Rectangle AxeHitMask => 
            this.Effects == SpriteEffects.FlipHorizontally ? 
            new Rectangle
            (
                (int)this.GetRealX() - 28,
                (int)this.Y + 10,
                23,
                60
            ) 
            :
            new Rectangle
            (
                (int)(GetRealX() + GetRealWidth()) + 5,
                (int)this.Y + 10,
                23,
                60
            );

        public bool Active => HealthPoints > 0;
        public float floorY => OnTrilad ?
            TriLader.Y + TriLader.GetRealHeight() - (float)TriLader.GetHeight(this.GetRealX())
            :
            ( Floor == null ? float.MaxValue - 1 : Floor.Y );

        #region Initialization

        public HeroEntity(GraphicsDevice graphicsDevice, float _x = 0, float _y = 0)
            : base(graphicsDevice, ToRealPath(TexturePath), _x, _y, Constants.HeroLayerDepth)
        {
            DefaultInitialize();
        }

        public HeroEntity(float x = 0, float y = 0)
            : base(x: x, y: y, _layerDepth: Constants.HeroLayerDepth)
        {
            DefaultInitialize();
        }

        private void DefaultInitialize()
        {
			LastPositions = new Queue<Vector2>();

            base.Jump = Constants.HeroJump;
            base.Speed = Constants.HeroSpeed;

            this.BaseJump = base.Jump;
            this.BaseSpeed = base.Speed;

            this.JumpMultiplier = this.SpeedMultiplier = 1f;
            this.Invicibility = 1000f;

            Unhandled = new List<ActionTypes>();

            HeartManager = new HeartAnimator(this);
            BulletAnimator = new BulletManager(this);

            Sun = null;

            this.OnTrilad = false;
            this.Climbing = this.Suspended = this.Descending = this.Jumping = this.Falling = false;
            this.PreparingShot = this.Shooting = this.ReadyToShoot = false;
            this.IsInAxeHit = false;
            this.ReceivingDamage = false;
            this.FinishedDying = false;
            this.IsInvincible = false;

            DamageEffects = new List<MySoundEffect>();
        }
        
        public void LoadContent(ContentManager contentManager, string folder = "")
        {
            RightAtlas = contentManager.Load<Texture2D>(folder + "Run");
            IdleAtlas = contentManager.Load<Texture2D>(folder + "Idle");
            JumpAtlas = contentManager.Load<Texture2D>(folder + "Jump");
            ShootAtlas = contentManager.Load<Texture2D>(folder + "Shot");
            ToWeaponAtlas = contentManager.Load<Texture2D>(folder + "toWeapon");
            AxeHitAtlas = contentManager.Load<Texture2D>(folder + "Attack");
            DamageAxeAtlas = contentManager.Load<Texture2D>(folder + "DMGaxe");
            DamageShootingAtlas = contentManager.Load<Texture2D>(folder + "DMGgun");
            DieAtlas = contentManager.Load<Texture2D>(folder + "die");

            WithGunTexture = contentManager.Load<Texture2D>(folder + "withGun");

            this.CurrentWeapon.LoadImage(contentManager, "Weapon");
            this.CurrentWeapon.BulletsImage = contentManager.Load<Texture2D>(WeaponEntity.BulletPath);
            
            this.HeartTexture = contentManager.Load<Texture2D>(folder + "Heart");

            this.JumpEffect = new MySoundEffect(contentManager, "sounds/jump");

            this.DamageEffects.Add(new MySoundEffect(contentManager, "sounds/heroHit1"));
            this.DamageEffects.Add(new MySoundEffect(contentManager, "sounds/heroHit2"));

            this.WalkingEffect = new MySoundEffect(contentManager, "sounds/walk");

            PrepairingToShootSound = new MySoundEffect(contentManager, "sounds/changeWeapon");

            AxeHitEffect = new MySoundEffect(contentManager, "sounds/axehit");

            HeartManager.HeartTexture = this.HeartTexture;
            BulletAnimator.Texture = contentManager.Load<Texture2D>(folder + "coolBullet");
        }

        public void InitializeAnimationsAsDefault()
        {
            base.InitializeAnimationsAsDefault(false, false, true, true);

            const int w = 72;
            const int h = 73;

            this.AnimationRight = Animation.CreateFromPhoto(RightAtlas, this, true, false, w, h, 12, 600, 12);
            AnimationRight.Priority = 0.7f;

            this.AnimationLeft = AnimationRight.Reverse();
            AnimationLeft.Priority = 0.7f;

            this.IdleAnimation = Animation.CreateFromPhoto(IdleAtlas, this, true, true, w, h, 10, 750, 10, true);
            this.IdleAnimation.Priority = 0.3f;

            this.AnimationJump = Animation.CreateFromPhoto(JumpAtlas, this, false, true, w, h, 3, 210, 6);
            this.AnimationJump.Priority = 0.95f;

            this.AnimationFall = Animation.CreateFromPhoto(JumpAtlas, this, false, false, w, h, 14, 770, 14, offset: 3);
            this.AnimationFall.Priority = 0.94f;

            this.ShootingAnimation = Animation.CreateFromPhoto(ShootAtlas, this, false, false, w, h, 7, 500, 7, true);
            this.ShootingAnimation.Priority = 1f;

            this.ToWeaponAnimation = Animation.CreateFromPhoto(ToWeaponAtlas, this, false, false, w, h, 12, 350, 12, true);
            ToWeaponAnimation.Priority = 1f;

            this.AxeHitAnimation = Animation.CreateFromPhoto(AxeHitAtlas, this, false, false, w, h, 10, 200, 10, true);
            this.AxeHitAnimation.Priority = 1f;

            this.DamageShootingAnimation = Animation.CreateFromPhoto(DamageShootingAtlas, this, false, false, w, h, 3, 400, 3, true);
            this.DamageShootingAnimation.Priority = 1f;

            this.DamageAxeAnimation = Animation.CreateFromPhoto(DamageAxeAtlas, this, false, false, w, h, 4, 150, 4, true);
            this.DamageAxeAnimation.Priority = 1f;

            this.DieAnimation = Animation.CreateFromPhoto(DieAtlas, this, false, false, w, h, 17, 1500, 17, true);
            this.DieAnimation.Priority = 1f;
        }

        #endregion
        
        public void AttachSun(Entity Sun)
        {
            this.Sun = Sun;
        }

        public void AttachClouds(Entity Clouds)
        {
            this.Clouds = Clouds;
        }

        public void AttachBackground(StaticBackground background)
        {
            Background = background;
			Background.LastX = this.X;
        }

        public void SwitchTo(WeaponEntity weapon)
        {
            this.CurrentWeapon = weapon;

            this.CurrentWeapon.X = this.GetRealX() + (int)this.GetRealWidth() / 2;
            this.CurrentWeapon.Y = this.GetRealY() + (int)this.GetRealHeight() / 2 - (int)CurrentWeapon.GetRealHeight() / 2;
        }

        public void EnterInMarsh()
        {
            if (!this.InMarsh)
            {
                this.JumpMultiplier *= 0.8f;
                this.SpeedMultiplier *= 0.6f;

                this.Speed = BaseSpeed * SpeedMultiplier;
                
                this.ToWeaponAnimation.ChangeDuration(ToWeaponAnimation.Duration * 0.6f);

                this.InMarsh = true;
            }
        }

        public void EscapeFromMarsh()
        {
            if (this.InMarsh)
            {
                this.JumpMultiplier /= 0.8f;
                this.SpeedMultiplier /= 0.6f;

                this.Speed = BaseSpeed * SpeedMultiplier;

                this.ToWeaponAnimation.ChangeDuration(ToWeaponAnimation.Duration / 0.6f);
                this.InMarsh = false;
            }
        }

        #region Continuous actions

        private void StartTotalJump(GameTime gameTime)
        {
            if (this.GetRealY() + this.GetRealHeight() >= floorY)
            {
                if (this.Jumping)
                    return;

                AnimationJump.ForceReset(gameTime);

                JumpEffect.Play();

                this.Jumping = true;
                this.CurrentJumpHeight = 0;
                this.Falling = false;

                this.Jump = this.BaseJump;
                
                OnTrilad = false;
            }
        }

        private void StartLaderClimb(Entity lader)
        {
            if (!this.Climbing && !this.Descending)
            {
                this.Climbing = true;
                this.YClimbingDestination = lader.Y;
                if (Math.Abs(this.GetRealY() + this.GetRealHeight() - this.YClimbingDestination) <= 7)
                {
                    this.Climbing = false;
                    return;
                }
                this.Lader = lader;
                this.Suspended = false;
            }
        }

        private void StopClimb()
        {
            if ( (this.Climbing || this.Descending) && !this.Suspended)
            {
                this.Climbing = false;
                this.Descending = false;
                this.Suspended = true;
            }
        }

        private void StartLaderDescent(Entity lader)
        {
            if (!this.Climbing && !this.Descending)
            {
                this.Descending = true;
                this.YClimbingDestination = lader.Y + lader.GetRealHeight();
                if (Math.Abs(this.GetRealY() + this.GetRealHeight() - this.YClimbingDestination) <= 7)
                {
                    this.Descending = false;
                    return;
                }
                this.Lader = lader;
                this.Suspended = false;
            }
        }

        private void PrepareShot()
        {
            if (!this.PreparingShot)
            {
                this.PreparingShot = true;

                PrepairingToShootSound.Play();
            }
        }

		public void AttachToCamera(GraphicsDevice graphicsDevice, Camera2D camera)
		{
			camera.LookAt(new Vector2(this.GetRealX() - 7, this.GetRealY()));
			this.AttachedToCamera = true;
			this.Camera = camera;	
		}

		private void StartShot(GameTime gameTime)
        {
            this.Shooting = true;
            this.ReadyToShoot = false;

            bool flag;
            this.CurrentWeapon.Fire(gameTime, out flag);
        }

        #endregion

        public override int GetRealX()
        {
            int realX = RealX.HasValue ? RealX.Value : 0;
            if (Effects == SpriteEffects.FlipHorizontally)
            {
                int leftX = Convert.ToInt32(Image.Width - (realX + GetRealWidth()));

                return X + leftX;
            }

            return base.GetRealX();
        }


        #region Update and related members

        public void Update(GameTime gameTime, 
            List<ActionTypes> actions = null, List<Entity> possibleCollisions = null,
            Entity Lader = null, MonsterEntity monster = null, TriangularLader trilad = null)
        {
			if(LastPositions.Count > CameraDelayFrames)
				LastPositions.Dequeue();

			LastPositions.Enqueue(this.Center);

            if (LastAnimation != null && LastAnimation.IsResetable(gameTime))
                LastAnimation = null;
            if (lastHit == 0 && IsInvincible)
                lastHit = gameTime.TotalGameTime.TotalMilliseconds;
            if (IsInvincible)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds > lastHit + (double)Invicibility)
                {
                    IsInvincible = false;
                    lastHit = 0;
                }
            }

            if (!this.Active)
            {
                if (!this.FinishedDying)
                {
                    Vector2 v;
                    bool f;
                    base.PlayAnimation(DieAnimation, gameTime, out v, out f);

                    FinishedDying = f;
                }
            }

			if(this.AttachedToCamera)
			{
				float deltaX = 0, deltaY = 0;
				if(LastPositions.Count > CameraDelayFrames)
				{
					Vector2 next = LastPositions.First();
					Vector2 current = Camera.BoundingRectangle.Center;

					deltaX = next.X - current.X;
					deltaX *= 0.1f;

					next = LastPositions.Last();
					deltaY = next.Y - current.Y;
					//deltaY *= 0.8f;

					if(!( deltaX == 0 && deltaY == 0 ))
					{
						Camera.Move(new Vector2(deltaX, deltaY));

						Camera.Position = new Vector2((int)Math.Round(Camera.Position.X), (int)Math.Round(Camera.Position.Y));
					}
				}
			}


			if(this.Effects == SpriteEffects.FlipHorizontally)
            {
                if (this.CurrentWeapon.BulletSpeed.X > 0)
                    this.CurrentWeapon.BulletSpeed *= new Vector2(-1, 1);
            }
            else if (this.Effects == SpriteEffects.None)
            {
                if (this.CurrentWeapon.BulletSpeed.X < 0)
                    this.CurrentWeapon.BulletSpeed *= new Vector2(-1, 1);
            }

            if(WalkCount >= 0)
                WalkCount++;

            if(WalkCount > 2 || Jumping || Falling)
                WalkingEffect.Stop();

            HasMoved = false;

            if (this.OnTrilad)
                this.Speed = this.BaseSpeed / 1.5f;
            else if (!this.InMarsh)
            {
                this.Speed = this.BaseSpeed;
            }

            TotalChanges = Vector2.Zero;

            bool flag;
            Vector2 vector = new Vector2(0, 0);

            #region Ladder Climbing

            if (this.Climbing)
            {
                this.Jump = this.ClimbSpeed;
                if (actions.Contains(ActionTypes.StopClimb))
                {
                    this.StopClimb();
                }
                
                bool cycle;
                if (this.GetRealY() + this.GetRealHeight() <= this.YClimbingDestination)
                {
                    this.Suspended = false;
                    this.Climbing = false;
                    this.Descending = false;
                    this.Falling = false;
                    this.Floor = Lader;
                }
                else
                    this.Move(MovementDirection.Up, gameTime, out vector, out cycle);

                this.SwitchTo(CurrentWeapon);

                return;
            }


            if (this.Descending)
            {
                this.Jump = ClimbSpeed;
                if (actions.Contains(ActionTypes.StopClimb))
                    this.StopClimb();

                bool cycle;
                if (this.GetRealY() + this.GetRealHeight() >= this.YClimbingDestination)
                {
                    this.Suspended = false;
                    this.Descending = false;
                    //          this.Floor = Lader;
                }
                else
                {
                    this.Move(MovementDirection.Down, gameTime, out vector, out cycle);                   
                }

                this.SwitchTo(CurrentWeapon);

                return;
            }
            
            if (Lader != null && actions.Contains(ActionTypes.ClimbLader))
            {
                var col = GameEngine.CheckCollision(this, Lader);
                if (col == CollisionType.Right || col == CollisionType.Left || col == CollisionType.Bottom)
                {
                    if (this.GetRealX() > Lader.X - 5 && this.X < Lader.X + Lader.GetRealWidth() - Lader.GetRealWidth() / 2)
                    {
                        StartLaderClimb(Lader);
                    }
                }
                
                else if (col == CollisionType.Top)
                {
                    if (!(this.GetRealY() + this.GetRealHeight() <= Lader.Y))
                    {
                        StartLaderClimb(Lader);
                    }
                }
                
            }

            if (Lader != null && actions.Contains(ActionTypes.GoDownLader))
            {
                var col = GameEngine.CheckCollision(this, Lader);
                if (col == CollisionType.Right || col == CollisionType.Left || col == CollisionType.Top)
                {
                    if (this.GetRealX() > Lader.X - 5 && this.GetRealX() < Lader.X + Lader.GetRealWidth() - Lader.GetRealWidth() / 2)
                        StartLaderDescent(Lader);
                }
            }

            #endregion

            HandleMonsterCollision(monster, gameTime);

            bool canUpdate = true;
            if (this.ReceivingDamage)
            {
             //   Animation CurrDamageAnimation = ReadyToShoot ? DamageShootingAnimation : DamageAxeAnimation;

              //  base.PlayAnimation(CurrDamageAnimation, gameTime, out vector, out flag);

                canUpdate = true;
				flag = !IsInvincible;
                if (flag)
                {
                    ReceivingDamage = false;
					canUpdate = true;
                }
            }
            else if (this.PreparingShot)
            {
                base.PlayAnimation(ToWeaponAnimation, gameTime, out vector, out flag);
                canUpdate = false;
                if (flag)
                {
                    PreparingShot = false;
                    ReadyToShoot = true;
                    canUpdate = true;
                    this.Image = WithGunTexture;
                }              
            }
            else if (this.Shooting)
            {
                base.PlayAnimation(ShootingAnimation, gameTime, out vector, out flag);
                //canUpdate = false;
                if (flag)
                {
                    this.Shooting = false;
                    this.ReadyToShoot = true;
                    canUpdate = true;
                }
            }
            else if (this.IsInAxeHit)
            {
                base.PlayAnimation(AxeHitAnimation, gameTime, out vector, out flag);
                canUpdate = false;
                if(flag)
                {
                    IsInAxeHit = false;
                    AxeHitPause = 5;
					canUpdate = true;
                }
            }
            AxeHitPause--;
            

            if (actions != null)
                Unhandled.AddRange(actions);

            FilterActions();

            if (possibleCollisions != null)
            {
                foreach (var possibleCollision in possibleCollisions)
                {
                    var collision = GameEngine.CheckCollision(this, possibleCollision);

                    if(possibleCollision.Tag == "Pike")
                        collision = CollisionType.Top;
                    
                    switch (collision)
                    {
                        case CollisionType.Bottom:                            
                            this.Falling = true;
                            this.Jumping = false;                            
                            break;
                        case CollisionType.Left:                           
                            break;
                        case CollisionType.Right:                         
                            break;
                        case CollisionType.Top:
                            this.Falling = false;
                            this.Floor = possibleCollision;
                            break;
                    }
                }               
            }
            
            if (canUpdate)
                HandleAll(possibleCollisions, gameTime);
            
            if (!this.Suspended)
                UpdateJumpInfo(gameTime);

#region Trilad

            if (trilad != null)
            {
                TriLader = trilad;
            }
            
            float _x = this.GetRealX();
            this.OnTrilad = TriLader != null && (_x < TriLader.GetRealX() + TriLader.GetRealWidth() && _x > TriLader.GetRealX());
            this.OnTrilad = OnTrilad && !this.Jumping;
            if (this.OnTrilad && !this.Falling)
            {
                float nextY = TriLader.Y + TriLader.GetRealHeight() - (float)TriLader.GetHeight(this.GetRealX()) - this.GetRealHeight();
                
                this.Y = (int)nextY;
            }

#endregion

#region UpdateAttached
            
            if (Background != null)
            {
				if(LastPositions.Count > CameraDelayFrames)
				{
					Background.MoveTo(Camera.BoundingRectangle, LastPositions.First().X);
				}
            }

            this.CurrentWeapon.Update(gameTime);

#endregion
            
            if (HasMoved || IsInAxeHit)
            {
                ReadyToShoot = false;
                PreparingShot = false;
                Shooting = false;
            }

            if (!ReadyToShoot && !Shooting)
                base.PlayAnimation(IdleAnimation, gameTime, out vector, out flag);

            this.CurrentWeapon.X = this.X + (int)(this.GetRealWidth() / 2);
            this.CurrentWeapon.Y = this.Y + (int)(this.GetRealHeight() / 2 - CurrentWeapon.GetRealHeight() / 2);

            TotalChanges = Vector2.Zero;
        }

        private void UpdateJumpInfo(GameTime gameTime)
        {
            if (this.Floor != null && !Jumping)
                this.Y = Floor.Y - Image.Height;
            
            Vector2 vector;
            bool flag;

            if (Jump > 2 * Constants.HeroJump)
                Jump = Constants.HeroJump;
            
            if (this.CurrentJumpHeight >= Constants.MaxJumpHeight || Jump < 0)
            {
                this.Jumping = false;
                this.Jump = BaseJump;
            }


            if (Floor == null && !Jumping)
                this.Falling = true;
            if (this.GetRealY() + this.GetRealHeight() < floorY && !Jumping)
            {
                this.Falling = true;
            }
            else if (!Jumping)
            {
                this.Falling = false;
            }           

            if (this.Falling || this.Jumping)
            {
                float mult = 0f;
                if (this.Jumping)
                {
                    mult = -1f * JumpMultiplier;
                    this.Move(MovementDirection.Up, gameTime, out vector, out flag);
                    this.CurrentJumpHeight += vector.Y;
                }
                if (this.Falling)
                {
                    mult = 0.25f * JumpMultiplier;
                    if (Jump > 2 * Constants.HeroJump)
                        mult = 0;
                    this.Move(MovementDirection.Down, gameTime, out vector, out flag);
                }

                Jump = Jump + mult * 0.09f * 1.5f;
            }
			else
			{
				this.Jump = BaseJump;
			}
        }

        private void HandleMonsterCollision(MonsterEntity monster, GameTime gameTime)
        {
            //No mechanics here
        }

        private void HandleAll(List<Entity> possibleCollisions, GameTime gameTime)
        {
            Vector2 vector;
            bool flag;
            if (Unhandled != null && Unhandled.Count > 0)
            {
                List<CollisionType> collisions = new List<CollisionType>();
                if (possibleCollisions != null)
                {
                    for (int i = 0; i < possibleCollisions.Count; i++)
                    {
                        collisions.Add(GameEngine.CheckCollision(this, possibleCollisions[i]));
                        if(possibleCollisions[i].Tag == "Pike")
                            collisions[i] = CollisionType.Top;
                    }                  
                }
                foreach (var action in Unhandled)
                {
                    if (possibleCollisions != null)
                    {
                        HandleAction(action, gameTime, out vector, out flag, collisions);
                    }
                    else
                        HandleAction(action, gameTime, out vector, out flag);
                }

                Unhandled.Clear();
            }
        }

        private void HandleAction(ActionTypes action, GameTime gameTime, 
            out Vector2 vector, out bool flag,
            List<CollisionType> collisions = null)
        {
            vector = new Vector2(0, 0);
            flag = false;
            switch (action)
            {
                case ActionTypes.GoDownLader:
                    if ((collisions != null && !collisions.Contains(CollisionType.Top))
                        || collisions == null && (this.Descending || this.Suspended))
                    {
                        if (!Falling && !Jumping && !Shooting && !PreparingShot)
                        {
                            this.Move(MovementDirection.Down, gameTime, out vector, out flag);
                            HasMoved = true;
                        }
                    }
                    break;
                case ActionTypes.GoLeft:
                    if(( collisions != null && !collisions.Contains(CollisionType.Right) )
                        || collisions == null)
                    {
                        if(!Shooting && !PreparingShot)
                        {
                            this.Move(MovementDirection.Left, gameTime, out vector, out flag);
                            Effects = SpriteEffects.FlipHorizontally;
                            HasMoved = true;
                            if(!this.Jumping && !this.Falling)
                            {
                                WalkingEffect.Play();
                                WalkCount = 0;
                            }
                        }
                    }
                    break;
                case ActionTypes.GoRight:
                    if ((collisions != null && !collisions.Contains(CollisionType.Left))
                        || collisions == null)
                    {
                        if (!Shooting && !PreparingShot)
                        {
                            this.Move(MovementDirection.Right, gameTime, out vector, out flag);
                            Effects = SpriteEffects.None;
                            HasMoved = true;
                            if(!this.Jumping && !this.Falling)
                            {
                                WalkingEffect.Play();
                                WalkCount = 0;
                            }
                        }
                    }
                    break;
                case ActionTypes.Jump:
                    if ((collisions != null && !collisions.Contains(CollisionType.Bottom))
                        || collisions == null)
                    {
                        if (!Shooting && !PreparingShot)
                        {
                            this.StartTotalJump(gameTime);
                            HasMoved = true;
                        }
                    }
                    break;
                case ActionTypes.Fire:
                    if (this.CurrentWeapon.UsedFireRate < 1f && !this.IsInvincible && !HasMoved)
                    {
                        if (this.ReadyToShoot)
                            this.StartShot(gameTime);
                        else
                            PrepareShot();
                    }

                    //this.CurrentWeapon.Fire(gameTime, out flag);
                    break;
                case ActionTypes.AxeHit:
                    if (!IsInAxeHit && !this.IsInvincible && AxeHitPause <= 0)
                    {
                        IsInAxeHit = true;
                        AxeHitEffect.Play();
                        AxeHitPause = 0;
                    }
                    break;
            }

            if (vector != Vector2.Zero)
                this.Suspended = false;
        }


        private void FilterActions()
        {
            var newU = new List<ActionTypes>();

            if (Unhandled.Contains(ActionTypes.Jump))
            {
                newU.Add(ActionTypes.Jump);
            }

            if (Unhandled.Contains(ActionTypes.GoRight))
            {
                newU.Add(ActionTypes.GoRight);
            }
            else if (Unhandled.Contains(ActionTypes.GoLeft))
            {
                newU.Add(ActionTypes.GoLeft);
            }

            if (Unhandled.Contains(ActionTypes.Fire))
            {
                newU.Add(ActionTypes.Fire);
            }
            else if (Unhandled.Contains(ActionTypes.AxeHit))
            {
                if (!IsInAxeHit)
                {
                    newU.Add(ActionTypes.AxeHit);
                }
            }

            if (Unhandled.Contains(ActionTypes.ClimbLader))
            {
                newU.Add(ActionTypes.ClimbLader);
            }
            else if (Unhandled.Contains(ActionTypes.GoDownLader))
            {
                newU.Add(ActionTypes.GoDownLader);
            }

            Unhandled = newU;
        }

#endregion

        public override void Draw(SpriteBatch spriteBatch, Rectangle? destinationRectangle = null, 
            bool beginDraw = false, bool endDraw = false, SpriteEffects effects = SpriteEffects.None)
        {
            this.CurrentWeapon.DrawBullets(spriteBatch);
            this.HeartManager.Draw(this.HealthPoints, this, spriteBatch);
            this.BulletAnimator.Draw(spriteBatch);

			if(this.IsInvincible)
			{
				_hpTicks++;
				if(_hpTicks % 20 == 0)
				{
					if(base.ColorMask == Color.White)
						base.ColorMask = Color.Red;
					else
						base.ColorMask = Color.White;
				}
			}
			else
				base.ColorMask = Color.White;

            base.Draw(spriteBatch, destinationRectangle, beginDraw, endDraw, Effects);
        }

        public static string ToRealPath(string path)
        {
            return "Content/" + path + ".png"; 
        }

        private class HeartAnimator
        {
            public HeroEntity Source { get; set; }

            public float Interval { get; set; }
            public float TotalTime { get { return Source.Invicibility; } }
            public float DeltaHp { get; private set; }
            public Texture2D HeartTexture { get; set; }
            
            private bool NeedDraw { get; set; }

            private Timer HeartTimer { get; set; }
            private int count;
	
            public HeartAnimator(HeroEntity _source)
            {
                this.Source = _source;

                Interval = 200;

                Reset();
            }

            public void Hit(float deltaHp)
            {
                if (HeartTimer != null)
                    return;
                this.DeltaHp = deltaHp;
                
                HeartTimer = new Timer();

                HeartTimer.AutoReset = true;
                HeartTimer.Interval = this.Interval;
                HeartTimer.Elapsed += HeartTimer_Elapsed;

                HeartTimer.Start();
            }

            private void HeartTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                this.NeedDraw = !this.NeedDraw;
                count++;
                if (count * Interval > TotalTime)
                {
                    HeartTimer.Dispose();
                    HeartTimer = null;

                    Reset();
                }
            }

            private void Reset()
            {
                NeedDraw = false;
                DeltaHp = 0;
                count = 0;
            }

            public void Draw(float healthPoints, HeroEntity source, SpriteBatch spriteBatch)
            {
                const float coef = 1f;
                int newWidth = (int)(coef * HeartTexture.Width);
                int newHeight = (int)(coef * HeartTexture.Height);

                int XOffset = 7;
                int YOffset = 5;

                Rectangle rectangle = new Rectangle(XOffset, YOffset, newWidth, newHeight);
                if (source.AttachedToCamera)
                {
                    rectangle.X += (int)source.Camera.BoundingRectangle.TopLeft.X;
                    rectangle.Y += (int)source.Camera.BoundingRectangle.TopLeft.Y;
                }
				
                for (int i = 0; i < healthPoints; i++)
                {
#pragma warning disable CS0618 // Type or member is obsolete

                    spriteBatch.Draw(HeartTexture, destinationRectangle: rectangle, layerDepth: source.LayerDepth);

                    rectangle.X += newWidth + XOffset;
                }
                if (NeedDraw)
                {
                    for (int i = 0; i < DeltaHp; i++)
                    {
                        spriteBatch.Draw(HeartTexture, destinationRectangle: rectangle, layerDepth: source.LayerDepth);
                        rectangle.X += newWidth + XOffset;

#pragma warning restore CS0618 // Type or member is obsolete
                    }
                }
            }
        }

        private class BulletManager
        {
            public HeroEntity Source { get; set; }
            public Texture2D Texture { get; set; }

            public BulletManager(HeroEntity _source)
            {
                Source = _source;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                float cnt = (1 - Source.CurrentWeapon.UsedFireRate) * Source.CurrentWeapon.FireRate;

                Rectangle destRect = new Rectangle(5, 20, Texture.Width, Texture.Height);
                if (Source.AttachedToCamera)
                {
                    destRect.X += (int)Source.Camera.BoundingRectangle.TopLeft.X;
                    destRect.Y += (int)Source.Camera.BoundingRectangle.TopLeft.Y;
                }

                for(int i = 0; i < cnt; i++)
                {
                    spriteBatch.Draw(Texture, destinationRectangle: destRect, layerDepth: 1f);
                    destRect.X += destRect.Width + 3;
                }
            }
        }
    }
}