using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using FirstGameEngine.Animations;

namespace FirstGameEngine.Entities.Monsters
{
    //Here all the Monster Textures (Memory Optimization and code readability)
    //Also, here all the animation settings (code readability)

    public static class TextureManager
    {
        private static bool Initialized = false;
        private static string NotInitErrorMsg = "TextureManager.Initiliaze() was not yet called";

        public static string HPFolder = "monster/HP";

        public static Texture2D CoinAtlas { get; private set; }
        public static Texture2D AlertAtlas { get; private set; }

        public static void Initialize(ContentManager contentManager, string farmerFolder, string guardFolder)
        {
            if (Initialized)
                return;
            
            FarmerTextureManager.Initialize(contentManager, farmerFolder);
            GuardTextureManager.Initialize(contentManager, guardFolder);

            CoinAtlas = contentManager.Load<Texture2D>("monster/Coin");
            AlertAtlas = contentManager.Load<Texture2D>("monster/alert");

            Initialized = true;
        }

        public static Texture2D GetImage(MonsterEntity source, string key = "Main")
        {
            if (!Initialized)
                throw new InvalidOperationException(NotInitErrorMsg);

            switch (source.Type)
            {
                case MonsterType.Farmer:
                    return FarmerTextureManager.MainTexture;
                case MonsterType.Guard:
                    return GuardTextureManager.MainTexture;
                default:
                    throw new NotImplementedException();
            }
        }
    
        public static Animation GetAnimation(MonsterEntity source, string key)
        {
            switch (source.Type)
            {
                case MonsterType.Farmer:
                    return FarmerTextureManager.GetAnimation(source, key);
                case MonsterType.Guard:
                    return GuardTextureManager.GetAnimation(source, key);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Animation GetAlertAnimation(AdvancedEntity source)
        {
            return Animation.CreateFromPhoto(AlertAtlas, source, true, true, 4, 14, 13, 2000, 13, true);
        }

        public static Animation GetCoinAnimation(AdvancedEntity source)
        {
            return Animation.CreateFromPhoto(CoinAtlas, source, true, true, 15, 16, 7, 800, 7, true);
        }

        private class AnimationInfo
        {
            public bool Loaded;
            public string AtlasPath;
            public Texture2D Atlas;

            public float FrameDuration;
            public int Count;

            public bool Flat;
            //0 if not change, 1 if positive, -1 if negative
            public int DeltaX; 
            public int DeltaY;

            public AnimationInfo(string path, float duration, int count, int deltaX, int deltaY, bool flat)
            {
                AtlasPath = path;
                FrameDuration = duration;
                Count = count;

                Loaded = false;
                Atlas = null;

                Flat = flat;
                DeltaX = deltaX;
                DeltaY = deltaY;
            }

            public void Load(ContentManager contentManager)
            {
                Atlas = contentManager.Load<Texture2D>(AtlasPath);
            }

            public Animation Get(MonsterEntity source, int width, int height)
            {
                bool changeX = DeltaX != 0;
                bool negative = changeX ? DeltaX < 0 : DeltaY < 0;

                return Animation.CreateFromPhoto
                    (
                        Atlas, source,
                        changeX, negative,
                        width, height,
                        Count, FrameDuration * Count, Count,
                        Flat
                    );
            }
        }

        private static class FarmerTextureManager
        {
            private static bool Initialized = false;

            private const string NotInitializedErrorMsg = "Initialize() was not yet called";

            //Just the farmer stanging still
            public static Texture2D MainTexture { get; private set; }

            //See Initialize() method
            private static Dictionary<string, AnimationInfo> Info;
            
            private static float Width => MainTexture.Width;
            private static float Height => MainTexture.Height;

            public static void Initialize(ContentManager contentManager, string folder)
            {
                if (Initialized)
                    return;

                if (!folder.EndsWith("/") && folder != String.Empty)
                    folder += "/";

                MainTexture = contentManager.Load<Texture2D>(folder + "Farmer");

                //Walking and running are set to 'Go Right' by default
                Info = new Dictionary<string, AnimationInfo>
                {
                    ["Attack"] = new AnimationInfo(folder + "Attack", 100, 12, 0, 0, true),
                    ["Prepare"] = new AnimationInfo(folder + "Prepare", 100, 6, 0, 0, true),
                    ["Death"] = new AnimationInfo(folder + "Death", 60, 19, 0, 0, true),
                    ["Pain"] = new AnimationInfo(folder + "Pain", 25, 16, 0, 0, true),
                    ["Run"] = new AnimationInfo(folder + "Run", 75, 10, 1, 0, false),
                    ["Angry"] = new AnimationInfo(folder + "Angry", 200, 8, 0, 0, true),
                    ["Walk"] = new AnimationInfo(folder + "Walk", 110, 10, 1, 0, false),
                };

                foreach (KeyValuePair<string, AnimationInfo> entry in Info)
                {
                    entry.Value.Load(contentManager);
                }
                
                Initialized = true;
            }  

            public static Animation GetAnimation(MonsterEntity source, string key)
            {
                if (!Initialized)
                    throw new InvalidOperationException(NotInitializedErrorMsg);
                if (source == null)
                    throw new ArgumentException("Cannot create animation from null");

                if (Info.ContainsKey(key))
                {
                    return Info[key].Get(source, (int)Width, (int)Height);
                }
                else
                    throw new ArgumentOutOfRangeException($"No such key {key}");
            }           
        }

        private static class GuardTextureManager
        {
            private static bool Initialized = false;

            private const string NotInitializedErrorMsg = "Initialize() was not yet called";

            public static Texture2D MainTexture;
            public static Texture2D ReadyToShootTexture;

            private static Dictionary<string, AnimationInfo> Info;

            private static float Width => MainTexture.Width;
            private static float Height => MainTexture.Height;

            public static void Initialize(ContentManager contentManager, string folder)
            {
                if (Initialized)
                    return;

                if (!folder.EndsWith("/") && folder.Length > 0)
                    folder += "/";

                MainTexture = contentManager.Load<Texture2D>(folder + "Guard");

                Info = new Dictionary<string, AnimationInfo>
                {
                    ["Walk"] = new AnimationInfo(folder + "Walk", 80, 12, 1, 0, false),
                    ["Attack"] = new AnimationInfo(folder + "Attack", 15, 22, 0, 0, true),
                    ["Death"] = new AnimationInfo(folder + "Death", 60, 16, 0, 0, true),
                    ["Pain"] = new AnimationInfo(folder + "Pain", 300, 3, 0, 0, true),
                    ["Recoil"] = new AnimationInfo(folder + "recoil", 240, 4, 0, 0, true),
                };

                foreach(KeyValuePair<string, AnimationInfo> entry in Info)
                {
                    Info[entry.Key].Load(contentManager);
                }

                Initialized = true;
            }

            public static Animation GetAnimation(MonsterEntity source, string key)
            {
                if (!Initialized)
                    throw new InvalidOperationException(NotInitErrorMsg);

                if (Info.ContainsKey(key))
                {
                    return Info[key].Get(source, (int)Width, (int)Height);
                }
                else
                    throw new KeyNotFoundException($"Couldn't find key: {key}");
            }
        }
    }
}
