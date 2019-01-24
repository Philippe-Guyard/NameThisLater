using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended;

using FirstGameEngine.Entities;

#pragma warning disable CS0618 // Type or member is obsolete

namespace FirstGameEngine
{
    //Deprecated
    public class DynamicBackground
    {
        public const float LvlWeight = 0.08f;

        private List<Texture2D>[] TextureLevels;
        private List<StaticObject> StaticObjects;

        private List<Entity>[] Entities;
   
        private Random RNG;

        public float Height { get; set; }

        public DynamicBackground()
        {
            StaticObjects = new List<StaticObject>();

            RNG = new Random();
        }

        public void Load(ContentManager contentManager, string folder, int count)
        {
            TextureLevels = new List<Texture2D>[count];
            
            var names = System.IO.Directory.EnumerateFiles(contentManager.RootDirectory + "/" +  folder);

            foreach (string fullPath in names)
            {
                string path = System.IO.Path.GetFileNameWithoutExtension(fullPath);
                int level;
                string strLevel;
                bool isStatic = path[0] == '_';
                if (isStatic)
                    strLevel = path[1].ToString() + path[2].ToString();
                else
                    strLevel = path[0].ToString() + path[1].ToString();

                if (!GetLevel(strLevel, out level))
                    throw new InvalidOperationException();

                level--;

                Texture2D texture = contentManager.Load<Texture2D>(folder + path);
                if (isStatic)
                    StaticObjects.Add(new StaticObject(texture, level));
                else
                {
                    if (TextureLevels[level] == null)
                        TextureLevels[level] = new List<Texture2D>();

                    TextureLevels[level].Add(texture);
                }
            }            
        }

        public void Draw(SpriteBatch spriteBatch, RectangleF bounds)
        {
            foreach(var obj in StaticObjects)
            {
                spriteBatch.Draw(obj.texture, position: bounds.TopLeft, layerDepth: 0.1f - LvlWeight * obj.level);
            }

            for(int i = 0; i < Entities.Length; i++)
            {
                if (Entities[i] == null)
                    continue;
                for(int j = 0; j < Entities[i].Count; j++)
                {
                    Entities[i][j].Draw(spriteBatch);
                }
            }
        }
        
        private bool GetLevel(string s, out int level)
        {
            level = 0;
            if (int.TryParse(s[0].ToString() + s[1].ToString(), out level))
            {
                return true;
            }

            return int.TryParse(s[0].ToString(), out level);
        }

        private void GenerateEntities(RectangleF bounds)
        {
            Entities = new List<Entity>[TextureLevels.Length];

            for(int lvl = 0; lvl < TextureLevels.Length; lvl++)
            {
                if (TextureLevels[lvl] == null)
                    continue;

                Entities[lvl] = new List<Entity>();
                
                for(int ind = 0; ind < TextureLevels[lvl].Count; ind++)
                { 
                    int count = RNG.Next(2, 5);
                    for(int i = 0; i < count; i++)
                    {
                        Vector2 pos = GenerateRandomCoordinates(lvl, bounds);
                        Entities[lvl].Add(new Entity(TextureLevels[lvl][ind], pos.X, pos.Y, 0.1f - 0.01f * lvl));
                    }
                }
            }
        }

        private Vector2 GenerateRandomCoordinates(int lvl, RectangleF bounds)
        {
            bool flag = true;
            float x = 0, y = 0;

            while (flag)
            {
                x = bounds.Left + RNG.Next(1, (int)bounds.Width);
                y = bounds.Top + RNG.Next(1, (int)Height);

                flag = false;
                for (int j = 0; j < Entities[lvl].Count; j++)
                {
                    if (Entities[lvl][j].BoundingRectangle.Contains(x, y))
                    {
                        flag = true;
                        break;
                    }
                }
            }

            return new Vector2(x, y);
        }

        struct StaticObject
        {
            public int level;
            public Texture2D texture;

            public StaticObject(Texture2D _pic, int _level)
            {
                level = _level;
                texture = _pic;
            }
        }
    }

    public class StaticBackground
    {
        const float LevelWeight = 0.03f;
        const float DepthPerLayer = 0.001f;
        const int StaticCount = 2;

        StaticObject[] TextureLevels;

		public float LastX = 0;

        public int Levels => TextureLevels.Length;

        public StaticBackground()
        {

        }

        public void Load(ContentManager contentManager, string folder, int count)
        {
            TextureLevels = new StaticObject[count];

            var names = System.IO.Directory.EnumerateFiles(contentManager.RootDirectory + "/" + folder);

            foreach (string fullPath in names)
            {
                int level;
                string path = System.IO.Path.GetFileNameWithoutExtension(fullPath);

                if (!GetLevel(path, out level))
                    continue;//throw new InvalidOperationException(); 

                level--;

                TextureLevels[level] = new StaticObject(contentManager.Load<Texture2D>(folder + path));                
            }
        }

        private bool GetLevel(string s, out int level)
        {
            level = 0;

			if(s.Length > 1)
			{
				if(int.TryParse(s[0].ToString() + s[1].ToString(), out level))
				{
					return true;
				}
			}

            return int.TryParse(s[0].ToString(), out level);
        }

        public void Draw(SpriteBatch spriteBatch, RectangleF bounds)
        {
            for(int lvl = 0; lvl < Levels; lvl++)
            {
                TextureLevels[lvl].Draw(spriteBatch, bounds, lvl);
            }
            for(int lvl = 1; lvl <= StaticCount; lvl++)
            {
                TextureLevels[Levels - lvl].Draw(spriteBatch, bounds, 50);
            }
        }

        public void Update(RectangleF bounds, float deltaMove)
        {                  
            for(int lvl = 0; lvl < Levels; lvl++)
            {
                float percent = 0.9f - (lvl / 2) * LevelWeight;
				if(percent < 0)
					percent = 0;

                float realMove = deltaMove * percent;
                if (TextureLevels[lvl].ScreenPos.X + realMove <= -TextureLevels[lvl].texture.Width)
                {
                    TextureLevels[lvl].ScreenPos.X = 0;
                }
                if (TextureLevels[lvl].ScreenPos.X + realMove >= TextureLevels[lvl].texture.Width)
                {
                    TextureLevels[lvl].ScreenPos.X = 0;
                }

                TextureLevels[lvl].ScreenPos.X += realMove;
            }
        }

		public void MoveTo(RectangleF bounds, float targetX)
		{
			float delta = targetX - LastX;
			delta *= 0.1f;
			LastX = targetX;

			//Update(bounds, delta);
		}


        struct StaticObject
        {
            public Vector2 ScreenPos;
            public Texture2D texture;

            public StaticObject(Texture2D _texture, Vector2? pos = null)
            {
                texture = _texture;
                ScreenPos = pos.HasValue ? pos.Value : Vector2.Zero;
            }

            public void Draw(SpriteBatch spriteBatch, RectangleF bounds, int lvl)
            {

//				float ld = 0.1f - DepthPerLayer * lvl;
//				spriteBatch.Draw(texture, destinationRectangle: dest, sourceRectangle: source, layerDepth: ld);

				
                int w = Constants.SampleWindowWidth;
                int x = (int)Math.Round(ScreenPos.X);
                int y = (int)Math.Round(ScreenPos.Y);
                int h = texture.Height;

				int l = (int)Math.Round(bounds.Left);
				x += l - 5 * 32;
				x -= ( 20 - lvl ) * 10;
				x = x % texture.Width;
				
                Rectangle mainSource, mainDest;
                Rectangle? offSource = null, offDest = null;
                if (x < 0)
                {
                    int realW = Math.Min(-x, w);
                    mainSource = new Rectangle(texture.Width + x, y, realW, h);
                    mainDest = new Rectangle(l, y, realW, h);

                    if (x > -w)
                    {
                        offSource = new Rectangle(0, y, w + x, h);
                        offDest = new Rectangle(l - x, y, w + x, h);
                    }
                }
                else if (x + w > texture.Width)
                {             
                    int sepX = texture.Width - x;

                    mainSource = new Rectangle(x, y, sepX, h);
                    mainDest = new Rectangle(l, y, sepX, h);

                  //  Console.WriteLine($"MS: {mainSource.X}, {mainSource.Width}, {mainSource.Height}");
                  //  Console.WriteLine($"MD: {mainDest.X - l}, {mainDest.Width}, {mainDest.Height}");

                    offSource = new Rectangle(0, y, w - sepX, h);
                    offDest = new Rectangle(l + sepX, y, w - sepX, h);

                   // Console.WriteLine($"OS: {offSource.Value.X}, {offSource.Value.Width}, {offSource.Value.Height}");
                   // Console.WriteLine($"OD: {offDest.Value.X - l}, {offDest.Value.Width}, {offDest.Value.Height}");


                   // Console.WriteLine("===============================");
                }
                else
                {
                    mainSource = new Rectangle(x, y, w, h);
                    mainDest = new Rectangle(l, y, w, h);
                }

                float ld =  0.1f - DepthPerLayer * lvl;

                mainDest.Y += (int)Math.Round(bounds.Top) - 100;
                spriteBatch.Draw(texture, destinationRectangle: mainDest, sourceRectangle: mainSource, layerDepth: ld);
                if (offSource.HasValue && offDest.HasValue)
                {
                    offDest = new Rectangle(offDest.Value.X, offDest.Value.Y + (int)Math.Round(bounds.Top) - 100, offDest.Value.Width, offDest.Value.Height);             
                    spriteBatch.Draw(texture, destinationRectangle: offDest, sourceRectangle: offSource, layerDepth: ld);
                }
				
			}
        }
    }
}

#pragma warning restore CS0618 // Type or member is obsolete