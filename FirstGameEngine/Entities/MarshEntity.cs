using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using FirstGameEngine.Animations;

namespace FirstGameEngine.Entities
{
    public class MarshEntity : Entity
    {
#pragma warning disable CS0618 // Type or member is obsolete
        
        private Dictionary<string, EarthBlock> EarthBlocks;
        private Dictionary<string, SwampBlock> SwampBlocks;

        private int CentralWidth => (int)GetRealWidth() - 4 * EarthBlock.Width; 
        private int CentralCount => CentralWidth / EarthBlock.Width;

        public MarshEntity(float x, float y, float width, float height) : base(x, y, 1f)
        {            
            RealWidth = width;
            RealHeight = height;           
        }

        public void LoadContent(ContentManager contentManager, string earth)
        {
            //InitEarth(contentManager);

            //InitSwamp(contentManager);
        }


        #region Earth

        private void InitEarth(ContentManager contentManager)
        {
            EarthBlocks = new Dictionary<string, EarthBlock>();

            InitEarthForeground(contentManager);

            InitEarthBackground(contentManager);
        }

        private void InitEarthForeground(ContentManager contentManager)
        {
            EarthBlock val;
            val.SourceRectangle = new Rectangle(0, 0, EarthBlock.Width, EarthBlock.Height);
            val.LayerDepth = 0.95f;
            const int w = EarthBlock.Width;
            const int h = EarthBlock.Height;

            //First 10 blocks - latteral sides
            for (int i = 0; i < 10; i++)
            {
                string key = (i + 1).ToString();

                val.Atlas = contentManager.Load<Texture2D>("marsh/earth/" + key);

                val.Effects = SpriteEffects.None;
                int posX = (i % 2) * w;
                int posY = (i / 2) * h;
                int sourceX = (i + 1) * w;
                int sourceY = 0;
                //val.SourceRectangle = new Rectangle(sourceX, sourceY, w, h);
                val.Position = new Vector2(posX, posY);

                EarthBlocks.Add(key, val);

                //reversed
                val.Effects = SpriteEffects.FlipHorizontally;
                val.Position = new Vector2(GetRealWidth() - posX - w, posY);

                EarthBlocks.Add(key + "R", val);
            }

            val.Effects = SpriteEffects.None;
            val.Position = Vector2.Zero;
            for (int i = 11; i <= 14; i++)
            {
                //                val.SourceRectangle = new Rectangle(i * w, 0, w, h);
                val.Atlas = contentManager.Load<Texture2D>("marsh/earth/" + i.ToString());
                
                EarthBlocks.Add(i.ToString(), val);
            }

            val.Atlas = contentManager.Load<Texture2D>("marsh/earth/0");
            EarthBlocks.Add("0", val);
        }

        private void InitEarthBackground(ContentManager contentManager)
        {
            const int w = EarthBlock.Width;
            const int h = EarthBlock.Height;

            EarthBlock val;
            val.SourceRectangle = new Rectangle(0, 0, EarthBlock.Width, EarthBlock.Height);
            val.LayerDepth = 0.9f;
            val.Effects = SpriteEffects.None;
            val.Position = Vector2.Zero;
            for(int i = 3; i <= 8; i++)
            {
                int sourceX = (i + 14) * w;
                //val.SourceRectangle = new Rectangle(sourceX, 0, w, h);
                val.Atlas = contentManager.Load<Texture2D>("marsh/earth/" + i.ToString() + "B");

                EarthBlocks.Add(i.ToString() + "B", val);
            }
        }

        private void DrawEarth(SpriteBatch spriteBatch)
        {
            //Draw latteral
            for(int i = 1; i <= 10; i++)
            {
                EarthBlocks[i.ToString()].Draw(spriteBatch);
                EarthBlocks[i.ToString() + "R"].Draw(spriteBatch);
            }

            //Draw central
            //Block 8 starts the 12,13 chain
            //Block 9 starts the 11, 14 chain

            int w = EarthBlock.Width;

            int beginX = (int)EarthBlocks["8"].Position.X;
            int beginY = (int)EarthBlocks["8"].Position.Y;
            
            EarthBlock block;
            for(int i = 0; i < CentralCount; i++)
            {
                string key = (12 + (i % 2)).ToString();
                block = EarthBlocks[key];
                block.Position = new Vector2(beginX + (i + 1) * w, beginY);

                EarthBlock.Draw(spriteBatch, block);
            }

            beginX = (int)EarthBlocks["10"].Position.X;
            beginY = (int)EarthBlocks["10"].Position.Y;
            for (int i = 0; i < CentralCount; i++)
            {
                string key = (i % 2 == 0 ? 11 : 14).ToString();
                block = EarthBlocks[key];
                block.Position = new Vector2(beginX + (i + 1) * w, beginY);

                EarthBlock.Draw(spriteBatch, block);
            }
            //Draw last

            for (int i = 0; i < GetRealWidth() / w; i++)
            {
                block = EarthBlocks["0"];
                block.Position = new Vector2(i * w, GetRealHeight() - EarthBlock.Height);

                EarthBlock.Draw(spriteBatch, block);
            }

            DrawEarthBackground(spriteBatch);
        }

        private void DrawEarthBackground(SpriteBatch spriteBatch)
        {
            EarthBlock block;
            /*
            //Block 1
            block = EarthBlocks["1B"];
            block.Position = Vector2.Zero;

            EarthBlock.Draw(spriteBatch, block);


            block.Position = new Vector2(GetRealWidth() - EarthBlock.Width, 0);

            EarthBlock.Draw(spriteBatch, block);

            //Block 2
            block = EarthBlocks["2B"];
            block.Position = new Vector2(EarthBlock.Width, 0);

            EarthBlock.Draw(spriteBatch, block);


            block.Position = new Vector2(GetRealWidth() - 2 * EarthBlock.Width, 0);

            EarthBlock.Draw(spriteBatch, block);

    */

            //Chains (3-4 chain starts at block 4, 5-6 chain starts at block 6)
            int startX = (int)EarthBlocks["3"].Position.X;
            int startY = (int)EarthBlocks["3"].Position.Y;
            
            for(int i = 0; i < CentralCount + 2; i++) //One for each latteral side
            {
                string key = (3 + (i % 2)).ToString() + "B";
                block = EarthBlocks[key];
                block.Position = new Vector2(startX + (i + 1) * EarthBlock.Width, startY);

                EarthBlock.Draw(spriteBatch, block);
            }


            startX = (int)EarthBlocks["5"].Position.X;
            startY = (int)EarthBlocks["5"].Position.Y;

            for (int i = 0; i < CentralCount + 2; i++) //One for each latteral side
            {
                string key = (5 + (i % 2)).ToString() + "B";
                block = EarthBlocks[key];
                block.Position = new Vector2(startX + (i + 1) * EarthBlock.Width, startY);

                EarthBlock.Draw(spriteBatch, block);
            }

            startX = (int)EarthBlocks["8"].Position.X;
            startY = (int)EarthBlocks["8"].Position.Y;

            for (int i = 0; i < CentralCount; i++)
            {
                string key = (7 + (i % 2)).ToString() + "B";
                block = EarthBlocks[key];
                block.Position = new Vector2(startX + (i + 1) * EarthBlock.Width, startY);

                EarthBlock.Draw(spriteBatch, block);
            }
        }

        #endregion

        #region Swamp

        private void InitSwamp(ContentManager contentManager)
        {
            SwampBlocks = new Dictionary<string, SwampBlock>();

            SwampBlock block;

            //First, load animated blocks
            block.SourceRectangle = new Rectangle(0, 0, 
                (CentralWidth <= 256 ? CentralWidth: 256), EarthBlock.Height);

            string basePath = "marsh/swamp/";
            string[] BGkeys = { "SwampBGFlow", "SwampFarBGFlow", "SwampFarBGUnderflowStatic"};
            string[] FGkeys = { "SwampFGFlow", "SwampFGUnderflow" };

            block.LayerDepth = 0.9f;
            for(int i = 0; i < BGkeys.Length; i++)
            {
                block.Texture = contentManager.Load<Texture2D>(basePath + BGkeys[i] + "1");
                block.Texture2 = contentManager.Load<Texture2D>(basePath + BGkeys[i] + "2");

                SwampBlocks.Add(BGkeys[i], block);
            }

            block.LayerDepth = 0.996f;
            for(int i = 0; i < FGkeys.Length; i++)
            {
                block.Texture = contentManager.Load<Texture2D>(basePath + FGkeys[i] + "1");
                block.Texture2 = contentManager.Load<Texture2D>(basePath + FGkeys[i] + "2");

                SwampBlocks.Add(FGkeys[i], block);
            }
        }

        private void DrawSwamp(SpriteBatch spriteBatch)
        {
            if (CentralWidth <= 256)
            {
                SwampBlocks["SwampFarBGFlow"].Draw(spriteBatch, new Vector2(2 * EarthBlock.Width, 0));
                SwampBlocks["SwampFGFlow"].Draw(spriteBatch, new Vector2(2 * EarthBlock.Width, 0));
                SwampBlocks["SwampBGFlow"].Draw(spriteBatch, new Vector2(2 * EarthBlock.Width, 0));
            }
        }

        #endregion

        public override void Draw(SpriteBatch spriteBatch, Rectangle? destinationRectangle = null, bool beginDraw = false, bool endDraw = false, SpriteEffects effects = SpriteEffects.None)
        {
//            DrawEarth(spriteBatch);

//            DrawSwamp(spriteBatch);
        }

        private struct EarthBlock
        {
            public const int Width = 16;
            public const int Height = 16;

            public float LayerDepth;

            public Texture2D Atlas;
            public static MarshEntity Source;

            public Rectangle SourceRectangle;
            public Vector2 Position;

            public SpriteEffects Effects;

            public void Draw(SpriteBatch spriteBatch)
            {
                Position = new Vector2(Source.GetRealX() + Position.X, Source.GetRealY() + Position.Y);

                spriteBatch.Draw(Atlas, sourceRectangle: SourceRectangle, position: Position, effects: Effects, layerDepth: LayerDepth);
            }

            public static void Draw(SpriteBatch spriteBatch, EarthBlock block)
            {
                Vector2 Position = new Vector2(Source.GetRealX() + block.Position.X, Source.GetRealY() + block.Position.Y);

                spriteBatch.Draw(block.Atlas, sourceRectangle: block.SourceRectangle, position: Position, effects: block.Effects, layerDepth: block.LayerDepth);
            }
        }

        private struct SwampBlock
        {
            const int TimeBetweenFrames = 50;
            const int Width = EarthBlock.Width;
            const int Height = EarthBlock.Height;

            public static MarshEntity Source;

            public float LayerDepth;

            public Texture2D Texture, Texture2;
            public Rectangle SourceRectangle;

            public bool Animated => Texture2 != null;

            public void Draw(SpriteBatch spriteBatch, Vector2 position)
            {
                position = new Vector2(Source.GetRealX() + position.X, Source.GetRealY() + position.Y);

                spriteBatch.Draw(Texture, position: position, sourceRectangle: SourceRectangle, layerDepth: LayerDepth);
            }

            public void Update(GameTime gameTime)
            {
                if (this.Animated)
                {

                }
            }

            public static void Draw(SwampBlock block, SpriteBatch spriteBatch, Vector2 position)
            {
                spriteBatch.Draw
                    (
                        texture: block.Texture,
                        position: position,
                        sourceRectangle: block.SourceRectangle,
                        layerDepth: block.LayerDepth
                    );
            }
        }
    }
}
