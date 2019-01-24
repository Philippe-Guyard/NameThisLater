using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

using FirstGameEngine.Animations;
using FirstGameEngine.Entities;
using FirstGameEngine.Entities.Monsters;
using FirstGameEngine.Levels;
using FirstGameEngine.Tiled;

namespace FirstGameEngine
{
    public class GameEngine
    {
        private const int DeathY = 3000;

		public readonly GraphicsDevice GraphicsDevice;

        public bool BackRequested { get; set; }

        HeroEntity heroEntity;

        List<ObstacleEntity> obstacleEntities;
        
        List<Entity> Laders;
        List<Entity> Pikes;
        List<MarshEntity> Marshes;

        List<MonsterEntity> MonsterEntities;
        List<FarmerEntity> FarmerEntities;
        List<FiringMonsterEntity> FiringMonsterEntities;
        
        List<TriangularLader> TriangularLaders;

        List<WeaponEntity> HeroWeapons;

        ObstacleEntity Floor;

        TiledMap MainMap;

        Animation CoinAnimation;
        Texture2D CurrentCoinTexture;
        Rectangle? CurrentCoinSourceRect;

        StaticBackground Background;
        
        ContentManager LastContentManager;
        Level LastLevel;

        public Random RNG { get; private set; }

        public int HeroCoins { get; private set; }

        public Vector2 CameraPos { get { return heroEntity.Camera.Position; } }
        public Vector2 LastCameraMovement { get; private set; }

        public Camera2D AttachedCamera { get; private set; }
		
        private bool _firingHit, _monsterHit;

        public GameEngine(GraphicsDevice graphicsDevice)
        {
            BackRequested = false;
			GraphicsDevice = graphicsDevice;

            this.Initialize();
        }
        
        private void Initialize(bool loaded = false)
        {
            _firingHit = _monsterHit = false;
             
            HeroCoins = 100;

            heroEntity = new HeroEntity(11 * 32, 29 * 32);
            heroEntity.FloorY = 1500;
            heroEntity.HealthPoints = 30f;
            heroEntity.Invicibility = 3000f;
            heroEntity.ClimbSpeed = heroEntity.Jump / 5;
            heroEntity.RealWidth = 25;
            heroEntity.RealHeight = 57;
            heroEntity.RealX = 18;
            heroEntity.RealY = 18;
            heroEntity.AxeDamage = 1f;

            HeroWeapons = new List<WeaponEntity>();

            WeaponEntity weapon = new WeaponEntity();

            weapon.BulletDamage = 1;
            weapon.FireRate = 1;
            weapon.RechargeTime = 3000;
            weapon.TimeBetweenBullets = 430;
            weapon.ImageLoaded += Weapon_ImageLoaded;
            weapon.Preferred = true;
            weapon.NeedsRecharge = true;
            weapon.BulletLifespan = 450;
            weapon.RealHeight = weapon.RealWidth = 1;
            weapon.BulletSpeed = new Vector2(3 * Constants.EntitySpeed, 0);

            HeroWeapons.Add(weapon);
         //   Floor = new ObstacleEntity(ObstacleType.Floor, 0, 206);
            
            Laders = new List<Entity>();
            
            obstacleEntities = new List<ObstacleEntity>();
           // obstacleEntities.Add(Floor);
            
 //           CreateHaystack(250, 0);
         //   CreateLader(400, 0);
         //   CreateLader(550, 0);
         //   CreateLader(200, 0);

            MonsterEntities = new List<MonsterEntity>();
            FarmerEntities = new List<FarmerEntity>();            
            FiringMonsterEntities = new List<FiringMonsterEntity>();
            CreateFarmer(20 * 32, 43 * 32);
            CreateGuard(91 * 32, 37 * 32);
            CreateFarmer(101 * 32, 38 * 32);
            CreateGuard(110 * 32, 38 * 32);

            TriangularLaders = new List<TriangularLader>();
//            CreateTriangularLader(600, 0);

            Marshes = new List<MarshEntity>();
//            CreateMarsh(heroEntity.X - 150, 200, 8 * 16, 6 * 16);
 
            Background = new StaticBackground();
           // Background.Height = 120;

            this.RNG = new Random(228);

            heroEntity.SwitchTo(weapon);
            heroEntity.AttachBackground(Background);
        }

        private void InitFirst()
        {

        }

        private void ReInit()
        {

        }

        #region public members

        public void Update(GameTime gameTime, List<ActionTypes> actions = null)
        {       
            bool flag;
            Texture2D aa;
            CoinAnimation.GetNextPosition(out aa, out CurrentCoinSourceRect, CurrentCoinSourceRect, gameTime, out flag);

            MainMap.Update(gameTime);

            if (heroEntity.Y >= DeathY) heroEntity.HealthPoints = 0;

            if (!heroEntity.Active)
            {
                heroEntity.Update(gameTime);

                BackRequested = heroEntity.FinishedDying;

                return;
            }
            
            if (heroEntity.IsInAxeHit)
            {
                Entity hit = new Entity();
                hit.X = heroEntity.AxeHitMask.X;
                hit.Y = heroEntity.AxeHitMask.Y;
                hit.RealWidth = heroEntity.AxeHitMask.Width;
                hit.RealHeight = heroEntity.AxeHitMask.Height;

                var monster = CheckMonsters(hit);
                var fire = CheckFiring(hit);
                if (monster != null && !_monsterHit)
                {
                    monster.HealthPoints -= heroEntity.AxeDamage;
                    _monsterHit = true;
                }
                if (fire != null && !_firingHit)
                {
                    fire.HealthPoints -= heroEntity.AxeDamage;
                    _firingHit = true;
                }
            }
            else
            {
                _monsterHit = _firingHit = false;
            }

            foreach (var bullet in heroEntity.CurrentWeapon.Bullets)
            {
                var monster = CheckMonsters(bullet);
                var firing = CheckFiring(bullet);
                if (CheckObstacles(bullet).Count > 0)
                {
                    bullet.Active = false;
                }
                else if (monster != null)
                {
                    bullet.Active = false;
                    monster.HealthPoints -= bullet.Damage;
                }
                else if (firing != null)
                {
                    firing.HealthPoints -= bullet.Damage;

                    bullet.Active = false;
                }
            }

            List<MonsterEntity> newMonsters = new List<MonsterEntity>();
            foreach (var monster in MonsterEntities)
            {
                if (monster.Destroyed)
                {
                    if (CheckCollision(monster, heroEntity) == CollisionType.NoCollision)
                    {
                        newMonsters.Add(monster);
                    }
                    else
                        HeroCoins++;
                }
                else
                    newMonsters.Add(monster);
            }
            MonsterEntities = newMonsters;

            List<FiringMonsterEntity> newFiring = new List<FiringMonsterEntity>();
            foreach (var monster in FiringMonsterEntities)
            {
                foreach (var bullet in monster.Weapon.Bullets)
                {
                    if (CheckObstacles(bullet).Count > 0)
                    {
                        bullet.Active = false;
                    }
                }
                if (monster.Destroyed)
                {
                    if (CheckCollision(monster, heroEntity) == CollisionType.NoCollision)
                    {
                        newFiring.Add(monster);
                    }
                    else
                        HeroCoins++;
                }
                else
                    newFiring.Add(monster);
            }
            FiringMonsterEntities = newFiring;

            foreach (var monster in MonsterEntities)
            {
                monster.Update(gameTime, heroEntity);
            }

            foreach (var fire in FiringMonsterEntities)
            {
                fire.Update(gameTime, heroEntity);
                foreach (var bullet in fire.Weapon.Bullets)
                {
                    if (CheckCollision(heroEntity, bullet) != CollisionType.NoCollision)
                    {
                        heroEntity.HealthPoints -= bullet.Damage;
                        bullet.Active = false;
                    }
                }
            }

            if (!heroEntity.IsInAxeHit)
            {
                foreach (var farmer in FarmerEntities)
                {
                    if (farmer.Attacking && !farmer.DidHitHero)
                    {
						if(farmer.CurrentWaitInterval < FarmerEntity.WaitInterval)
							continue;

                        Entity e = new Entity(farmer.DamageCollisionMask.Value.X, farmer.DamageCollisionMask.Value.Y);
                        e.RealWidth = farmer.DamageCollisionMask.Value.Width;
                        e.RealHeight = farmer.DamageCollisionMask.Value.Height;
                        if (CheckCollision(e, heroEntity) != CollisionType.NoCollision)
                        {
                            heroEntity.HealthPoints -= farmer.Damage;
                            farmer.DidHitHero = true;
                        }                        
                    }
                    else if (!farmer.Attacking)
                        farmer.DidHitHero = false;
                }
            }

            var temp = CheckObstacles(heroEntity);
            var lad = CheckLaders(heroEntity);
            var trilad = CheckTriangularLaders(heroEntity);

            if (CheckMarshes(heroEntity) != null) 
                heroEntity.EnterInMarsh();
            else
            {
                if (heroEntity.InMarsh)
                    heroEntity.EscapeFromMarsh();
            }
            
            if (heroEntity.Floor != null &&
                CheckCollision(heroEntity, heroEntity.Floor) == CollisionType.NoCollision)
            {
                heroEntity.Floor = null;
            }
            
            heroEntity.Update(gameTime, actions, temp, lad, null, trilad);
        }

        public void DrawEverything(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin
                (
                    transformMatrix: AttachedCamera.GetViewMatrix(),
                    sortMode: SpriteSortMode.FrontToBack,
                    samplerState: SamplerState.PointClamp,
                    blendState: BlendState.AlphaBlend
                );

            Rectangle copy = new Rectangle
            (
                (int)AttachedCamera.BoundingRectangle.X, (int)AttachedCamera.BoundingRectangle.Y,
                (int)AttachedCamera.BoundingRectangle.Width, (int)AttachedCamera.BoundingRectangle.Height
            );
            MainMap.Draw(spriteBatch, copy);
            
            if (heroEntity.Visible)
                heroEntity.Draw(spriteBatch);

            foreach (var obstacle in obstacleEntities)
            {
             //   if (obstacle.Visible)
       //             obstacle.Draw(spriteBatch);
            }

            /*
            foreach (var lad in Laders)
            {
                if (lad.Visible)
                {
                    lad.Draw(spriteBatch);
                }
            }
            */

            foreach (var monster in MonsterEntities)
            {
                monster.Draw(spriteBatch);
                //Console.WriteLine($"Monster: {monster.X}, {monster.Y}. Hero: {heroEntity.X}, {heroEntity.Y}");
            }

            foreach(var fire in FiringMonsterEntities)
            {
                if (fire.Visible)
                {
                    fire.Draw(spriteBatch);
                }
            }

            Background.Draw(spriteBatch, heroEntity.Camera.BoundingRectangle);

            /*
            foreach(var trilad in TriangularLaders)
            {
                if (trilad.Visible)
                {
                    trilad.Draw(spriteBatch);
                }
            }
			d
            foreach (var marsh in Marshes)
            {
                if (marsh.Visible)
                {
                    marsh.Draw(spriteBatch);
                }
            }*/

            
            FontManager.DigitFont.Scale = 1f;

            Vector2 pos = new Vector2(AttachedCamera.BoundingRectangle.Right - 70 - 55, AttachedCamera.BoundingRectangle.Top + 5);
            FontManager.DigitFont.DrawNumber(spriteBatch, pos, HeroCoins);
            pos = new Vector2(pos.X + FontManager.DigitFont.MeasureNumber(HeroCoins) + 2, pos.Y - 2);
            spriteBatch.Draw
                (
                    texture: CurrentCoinTexture,
                    sourceRectangle: CurrentCoinSourceRect,
                    position: pos,
                    layerDepth: 1f
                );
                
        }
        
        public void LoadContent(ContentManager contentManager, Level level)
        {
            var info = level.Info;

            MainMap = level.Map;
            MainMap.X = 0;
            MainMap.Y = 0;
            heroEntity.MaxY = MainMap.Height;

            Marshes = MainMap.GetMarshes();
            Pikes = MainMap.GetPikes();
         //   Back = contentManager.Load<Texture2D>("moutains");

            TextureManager.Initialize(contentManager, "Farmer", "guard");

            foreach (var obstacle in obstacleEntities)
            {
                obstacle.LoadImage(contentManager, info.Graphics.ObstaclesFolder);
            }

            foreach (var lad in Laders)
            {
                if (Constants.LaderTexture == null)
                    Constants.LaderTexture = contentManager.Load<Texture2D>("lader");

                lad.Image = Constants.LaderTexture;
                lad.InvokeLoad();
                //lad.LoadImage(contentManager, "");
            }

            foreach(var monster in MonsterEntities)
            {
                monster.LoadContent(contentManager);

                monster.InitializeAnimationsAsDefault();

                if(CheckMarshes(monster) != null)
                    monster.InMarsh = true;
            }
     
            foreach(var fire in FiringMonsterEntities)
            {
                fire.LoadContent(contentManager);

                //fire.InitializeAnimationsAsDefault();
                if(CheckMarshes(fire) != null)
                    fire.InMarsh = true;
            }            

            foreach(var marsh in Marshes)
            {
                marsh.LoadContent(contentManager, "marsh/earth");
            }

            foreach(var trilad in TriangularLaders)
            {
                trilad.LoadImage(contentManager, "TriLader");
            }

            heroEntity.LoadImage(contentManager, info.Graphics.HeroFolder + "protagonist");
            
            Background.Load(contentManager, info.Graphics.BackgroundElementsFolder, 16);
            
           // PlaceOnTop(heroEntity, Floor);

            foreach(var weapon in HeroWeapons)
            {
                weapon.LoadImage(contentManager, "gun");
                weapon.BulletsImage = contentManager.Load<Texture2D>("bullet");
                weapon.RechargeEffect = new MySoundEffect(contentManager, "sounds/recharge");
                weapon.ShootingEffect = new MySoundEffect(contentManager, "sounds/shoot");
            }

            LastContentManager = contentManager;
            LastLevel = level;

            heroEntity.LoadContent(contentManager, "hero/");

            heroEntity.InitializeAnimationsAsDefault();

            CurrentCoinTexture = contentManager.Load<Texture2D>("monster/Coin");
            CurrentCoinSourceRect = new Rectangle(0, 0, 15, 16);
            CoinAnimation = Animation.CreateFromPhoto(CurrentCoinTexture, heroEntity, false, false, 15, 16, 7, 1000, 7, true);
        }

        public void AddCamera(GraphicsDevice graphicsDevice, Camera2D camera)
        {
            heroEntity.AttachToCamera(graphicsDevice, camera);
			
            this.AttachedCamera = camera;
        }

        public void CreateCamera(GraphicsDevice graphicsDevice, float scale)
        {
            AttachedCamera = new Camera2D(graphicsDevice);
            AttachedCamera.Zoom = scale;

            heroEntity.AttachToCamera(graphicsDevice, AttachedCamera);
        }

        public void Reset()
        {
            this.BackRequested = false;
            this.Initialize();
            this.LoadContent(LastContentManager, LastLevel);

            if (this.AttachedCamera != null)
                this.AddCamera(GraphicsDevice, this.AttachedCamera);
        }

        #endregion

        #region private members

        #region Check 

        private TriangularLader CheckTriangularLaders(Entity testEntity)
        {
            foreach(var lader in TriangularLaders)
            {
                if (lader.CheckCollision(testEntity) != CollisionType.NoCollision)
                    return lader;
            }

            return null;
        }

        private List<Entity> CheckObstacles(Entity testEntity)
        {
            List<Entity> res;
            MainMap.CheckCollision(testEntity, out res);

            foreach(var entity in Pikes)
            {
                if (CheckCollision(testEntity, entity) != CollisionType.NoCollision)
                {
                    if(testEntity is HeroEntity)
                        heroEntity.HealthPoints -= 1;

                    //Console.WriteLine(CheckCollision(testEntity, entity));

                    res.Add(entity);
                }
            }

            return res;
        }

        private Entity CheckLaders(Entity testEntity)
        {
            foreach(var entity in Laders)
            {
                if (CheckCollision(testEntity, entity) != CollisionType.NoCollision)
                    return entity;
            }

            return null;
        }

        private MonsterEntity CheckMonsters(Entity testEntity)
        {
            foreach(var monster in MonsterEntities)
            {
                if (CheckCollision(testEntity, monster) != CollisionType.NoCollision)
                {
                    return monster;
                }
            }

            return null;
        }

        private FiringMonsterEntity CheckFiring(Entity testEntity)
        {
            foreach (var monster in FiringMonsterEntities)
            {
                if (CheckCollision(testEntity, monster) != CollisionType.NoCollision)
                {
                    return monster;
                }
            }

            return null;
        }

        private Entity CheckMarshes(Entity testEntity)
        {
            foreach(var marsh in Marshes)
            {
                if (CheckCollision(testEntity, marsh) != CollisionType.NoCollision)
                {
                    return marsh;
                }
            }

            return null;
        }

        #endregion

        #region Create

        private void CreateEnhancedChest(float x, float y)
        {
            ObstacleEntity chest = new ObstacleEntity(ObstacleType.EnhancedChest, x: x, y: y);

            obstacleEntities.Add(chest);
        }

        private void CreateChest(float x, float y)
        {
            ObstacleEntity chest = new ObstacleEntity(ObstacleType.Chest, x, y);

            obstacleEntities.Add(chest);
        }
        
        private void CreateHaystack(float x, float y)
        {
            ObstacleEntity haystack = new ObstacleEntity(ObstacleType.Haystack, x, y);
            haystack.FloorY = Floor.Y;
            haystack.ImageLoaded += Entity_ImageLoaded;

            obstacleEntities.Add(haystack);
        }

        private void CreateLader(float x, float y)
        {
            Entity lader = new Entity(x, y, _layerDepth: Constants.ObstaclesLayerDepth);
            lader.FloorY = Floor.Y;
            lader.ImageLoaded += Entity_ImageLoaded;

            Laders.Add(lader);
        }

        private void CreateMonster(float x, float y)
        {
            MonsterEntity monster = new MonsterEntity(MonsterType.Farmer, x, y);
            monster.Radius = 200;
            monster.FloorY = Floor.Y;
            monster.ImageLoaded += Monster_ImageLoaded;
            monster.Damage = 1;
            monster.Distance = 100;
            monster.HealthPoints = 3;

            MonsterEntities.Add(monster);
        }

        private void CreateTriangularLader(float x, float y)
        {
            TriangularLader lader = new TriangularLader(x, y);
            lader.FloorY = Floor.Y;
            lader.ImageLoaded += TriLad_ImageLoaded;

            TriangularLaders.Add(lader);
        }
        
        private void CreateGuard(float x, float y)
        {
            FiringMonsterEntity firingMonster = new FiringMonsterEntity(MonsterType.Guard, x, y);
            firingMonster.FloorY = firingMonster.Y + 60;
            firingMonster.ImageLoaded += Monster_ImageLoaded;
            firingMonster.BehaviorType = MonsterBehaviorType.Patrol;

            firingMonster.Distance = 100;
            firingMonster.Radius = 250;
            firingMonster.HealthPoints = 5;
            firingMonster.MaxHP = 5;
            firingMonster.AttackRange = firingMonster.Radius;

            FiringMonsterEntities.Add(firingMonster);
        }
        
        private void CreateMarsh(float x, float y, float width, float height)
        {
            MarshEntity Marsh = new MarshEntity(x, y, width, height);
            
            Entity lader1 = new Entity(x, y);
            lader1.RealHeight = height;
            lader1.RealWidth = 10;
            lader1.Visible = false;

            Entity lader2 = new Entity(x + width - 20, y, 1f);
            lader2.RealHeight = height;
            lader2.RealWidth = 10;
            lader2.Visible = false;
            
            Laders.Add(lader1);
            Laders.Add(lader2);

            Marshes.Add(Marsh);
        }
        
        private void CreateFarmer(float x, float y)
        {
            FarmerEntity farmer = new FarmerEntity(x, y);
            farmer.Distance = 100;
            farmer.MaxHP = 5;
            farmer.HealthPoints = 5;
            farmer.Radius = 200; 
            farmer.ImageLoaded += Monster_ImageLoaded;
            farmer.AttackRange = 48;
            farmer.FloorY = farmer.Y + 62;
            farmer.Damage = 1f;

            FarmerEntities.Add(farmer);
            MonsterEntities.Add(farmer);
        }

        #endregion

        #region Handle Load
        
        private void Entity_ImageLoaded(Entity entity)
        {
            if (entity.FloorY.HasValue)
                PlaceOnTop(entity, Floor);
        }

        private void Monster_ImageLoaded(Entity entity)
        {
            MonsterEntity monster = (MonsterEntity)entity;

            monster.InitializeAnimationsAsDefault();
        }
        
        private void Weapon_ImageLoaded(Entity entity)
        {
            WeaponEntity weapon = (WeaponEntity)entity;

            if (weapon.Preferred)
                heroEntity.SwitchTo(weapon);
        }
        
        private void TriLad_ImageLoaded(Entity entity)
        {
            TriangularLader lader = (TriangularLader)entity;
            Entity_ImageLoaded(lader);
        }

        #endregion

        #endregion

        //TODO: Split this into CheckTopCollision and CheckBottomCollision and add a "Floor" and "Border" layers to map
        public static CollisionType CheckCollision(Entity firstEntity, Entity secondEntity)
        {
            const float yOffset = 7;

            
            float x1 = firstEntity.GetRealX();
            float x2 = secondEntity.GetRealX();

            float y1 = firstEntity.GetRealY();
            float y2 = secondEntity.GetRealY();


            float w1 = firstEntity.GetRealWidth();
            float w2 = secondEntity.GetRealWidth();

            float h1 = firstEntity.GetRealHeight();
            float h2 = secondEntity.GetRealHeight();
                                  
            if (x1 <= x2 && x1 + w1 >= x2)
            {
                if (Math.Abs(y1 + h1 - y2) <= yOffset)
                    return CollisionType.Top;
                else if (Math.Abs(y1 - h2 - y2) <= yOffset)
                    return CollisionType.Bottom;
                else if ((y1 <= y2 && y1 + h1 >= y2) || (y1 >= y2 && y1 <= y2 + h2))
                    return CollisionType.Left;
            }
            if (x2 <= x1 && x2 + w2 >= x1)
            {
                if (Math.Abs(y1 + h1 - y2) <= yOffset)
                    return CollisionType.Top;
                else if (Math.Abs(y1 - h2 - y2) <= yOffset)
                    return CollisionType.Bottom;
                else if ((y1 <= y2 && y1 + h1 >= y2) || (y1 >= y2 && y1 <= y2 + h2))
                    return CollisionType.Right;
            }
            
            /*
            if (l1 <= l2 && r1 >= l2)
            {
                if (Math.Abs(d1 - u2) <= yOffset)
                    return CollisionType.Top;
                else if (Math.Abs(u1 - d2) <= yOffset)
                    return CollisionType.Bottom;
                else if ((u1 <= u2 && d1 >= u2) || (u1 >= u2 && u1 <= d2))
                    return CollisionType.Left;
            }
            if (l2 <= l1 && r2 >= l1)
            {
                if (Math.Abs(d1 - u2) <= yOffset)
                    return CollisionType.Top;
                else if (Math.Abs(u1 - d2) <= yOffset)
                    return CollisionType.Bottom;
                else if ((u1 <= u2 && d1 >= u2) || (u1 >= u2 && u1 <= d2))
                    return CollisionType.Right;
            }*/

            return CollisionType.NoCollision;
        }
        
        public static void PlaceOnTop(Entity entity, Entity floor)
        {
            entity.Y = (int)entity.FloorY.Value - (int)entity.GetRealHeight();
        }
    }
}
