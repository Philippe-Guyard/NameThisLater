using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FirstGameEngine
{
    public static class FontManager
    {
        public static void Initialize(ContentManager contentManager)
        {
            DigitFont.Initialize(contentManager);
        }

        public static class DigitFont
        {
            public const int DigitWidth = 11;
            public const int DigitHeight = 14;
            
            private static Texture2D DigitAtlas;
            private static Rectangle[] Digits;
            
            public static float Scale { get; set; }
            public static float SpaceBetweenDigits { get; set; }

            public static void Initialize(ContentManager contentManager)
            {
                DigitAtlas = contentManager.Load<Texture2D>("misc/numbers");

                Digits = new Rectangle[10];
                for(int i = 0; i < 10; i++)
                {
                    Digits[i] = new Rectangle(i * DigitWidth, 0, DigitWidth, DigitHeight);
                }

                Scale = 1f;
                SpaceBetweenDigits = 1.5f;
            }

            public static void DrawNumber(SpriteBatch spriteBatch, Vector2 position, int number, float depth = 1f)
            {
                string strRepresentation = number.ToString();

                float w = DigitWidth * Scale;

                Vector2 nextPos = position;
                float x = position.X;
                for(int i = 0; i < strRepresentation.Length; i++)
                {
                    int digit = Convert.ToInt32(strRepresentation[i].ToString());
                    nextPos = new Vector2(x + i * w, position.Y);

                    Rectangle destionation = new Rectangle((int)nextPos.X, (int)nextPos.Y, (int)w, (int)(DigitHeight * Scale));
                    spriteBatch.Draw(DigitAtlas, sourceRectangle: Digits[digit], destinationRectangle: destionation, layerDepth: depth);

                    x += SpaceBetweenDigits;
                }
            }

            public static float MeasureNumber(int number)
            {
                return number.ToString().Length * DigitWidth * Scale + number.ToString().Length * SpaceBetweenDigits;
            }            
        }
    }
}
