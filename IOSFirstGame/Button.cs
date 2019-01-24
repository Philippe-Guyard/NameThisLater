using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using MonoGame.Extended;

namespace FirstGameEngine
{
    public class Button
    {
        readonly float BaseX, BaseY;
        
        public float X { get; set; }
        public float Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public GraphicsDevice GraphicsDevice { get; set; }
        
        public Texture2D Texture { get; set; }
        public Color RectColor { get; set; }

        public Rectangle BoundingRectangle => new Rectangle((int)X, (int)Y, Width, Height);

        public Button(float _x, float _y, int _w, int _h)
        {
            X = _x;
            Y = _y;
            Width = _w;
            Height = _h;

            RectColor = Color.DimGray;

            BaseX = X;
            BaseY = Y;
        }

        public Button(Rectangle rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            RectColor = Color.DimGray;

            BaseX = X;
            BaseY = Y;
        }

        public void LoadImage(ContentManager contentManager, string path)
        {
            Texture = contentManager.Load<Texture2D>(path);
        }
        
        public bool IsPressed()
        {
//#if __IOS__
            TouchCollection touches = TouchPanel.GetState();
            for(int i = 0; i < touches.Count; i++)
            {
                Point p = new Point((int)touches[i].Position.X, (int)touches[i].Position.Y);
                if (BoundingRectangle.Contains(p))
                {
                    return true;
                }
            }

            return false;
//#endif

            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (this.Texture == null)
            {
                if (GraphicsDevice == null)
                    throw new InvalidOperationException();

                this.Texture = new Texture2D(GraphicsDevice, Width, Height);

                Color[] data = new Color[Width * Height];
                for (int i = 0; i < data.Length; i++) data[i] = RectColor;

                Texture.SetData(data);
            }

            spriteBatch.Draw(Texture, BoundingRectangle, Color.White);
        }
    }
}
