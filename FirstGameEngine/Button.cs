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
        const int offset = 5;
        private const int TargetFrameCount = 7;

        private float _blend = 1f;
        private int _frameCount;
		private bool pressed = false;

        public bool OnlyTriggerOnRelease { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int ScreenX { get; set; }
        public int ScreenY { get; set; }
        public float ScreenW { get; set; }
        public float ScreenH { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public object Tag { get; set; }

        public GraphicsDevice GraphicsDevice { get; set; }
        
        public Texture2D Texture { get; set; }
        public Rectangle? SourceRectangle { get; set; }

        public Color RectColor { get; set; }

        public Rectangle BoundingRectangle => new Rectangle((int)X, (int)Y, Width, Height);
        //public Rectangle TouchTriggerRect => new Rectangle((int)ScreenX + 25, (int)ScreenY + offset, Width + 90, Height + 90);
        public Rectangle TouchTriggerRect => new Rectangle
            (
                (int)ScreenX - offset, (int)ScreenY - offset,
                (int)ScreenW + 2 * offset, (int)ScreenH + 2 * offset
            );

        public Button(float _x, float _y, int _w, int _h)
        {
            X = (int)_x;
            Y = (int)_y;
            Width = _w;
            Height = _h;
        
            RectColor = Color.DimGray;

            ScreenX = X;
            ScreenY = Y;
            ScreenW = Width;
            ScreenH = Height;

            OnlyTriggerOnRelease = false;
        }

        public Button(Rectangle rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            RectColor = Color.DimGray;

            ScreenX = X;
            ScreenY = Y;
            ScreenW = Width;
            ScreenH = Height;

            OnlyTriggerOnRelease = false;
        }

        public void Reset()
        {
            this.X = ScreenX;
            this.Y = ScreenY;
        }

        public void LoadImage(ContentManager contentManager, string path)
        {
            Texture = contentManager.Load<Texture2D>(path);
        }
        
        public bool IsPressed(Matrix2D? scale = null, Matrix? transform3 = null)
        {
			if (pressed) //The button wasn't unpressed from last press
				return false;
#if __IOS__
            return IOS_IsPressed(scale, transform3);
#elif WINDOWS || LINUX
            bool res = Desktop_IsPressed(scale, transform3);
			if (res)
				this.Press();
			return res;
#endif

            return false;
        }

        private bool IOS_IsPressed(Matrix2D? scale, Matrix? transform3)
        {
            var touches = TouchPanel.GetState();
            for (int i = 0; i < touches.Count; i++)
            {
                if (OnlyTriggerOnRelease)
                {
                    if (touches[i].State != TouchLocationState.Released)
                        continue;
                }

                var coor = new Vector2(touches[i].Position.X, touches[i].Position.Y);
                if (scale.HasValue)
                {
                    coor = Vector2.Transform(coor, Matrix2D.Invert(scale.Value));
                }
				if (transform3.HasValue)
				{
					coor = Vector2.Transform(coor, Matrix.Invert(transform3.Value));
				}
                if (TouchTriggerRect.Contains(coor))
                {
                    this.Press();
                    return true;
                }
            }

            return false;
        }

        private bool Desktop_IsPressed(Matrix2D? scale, Matrix? transform3)
        {
            var click = Mouse.GetState();

            var coor = new Vector2(click.X, click.Y);
            if (scale.HasValue)
            {
                Matrix2D inputScale = Matrix2D.Invert(scale.Value);
                coor = Vector2.Transform(coor, inputScale);
            }

			if (transform3.HasValue)
			{
				coor = Vector2.Transform(coor, Matrix.Invert(transform3.Value));
				return BoundingRectangle.Contains(coor) && click.LeftButton == ButtonState.Pressed;
			}
			

            return TouchTriggerRect.Contains(coor)
                && click.LeftButton == ButtonState.Pressed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_frameCount > TargetFrameCount)
            {
                this.UnPress();
            }
			if (pressed)
				_frameCount++;

            if (this.Texture == null)
            {
                if (GraphicsDevice == null)
                    throw new InvalidOperationException();

                this.Texture = new Texture2D(GraphicsDevice, Width, Height);

                Color[] data = new Color[Width * Height];
                for (int i = 0; i < data.Length; i++) data[i] = RectColor;

                Texture.SetData(data);
            }

            if (SourceRectangle.HasValue)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                spriteBatch.Draw
                    (
                        Texture,
                        destinationRectangle: BoundingRectangle,
                        sourceRectangle: SourceRectangle.Value,
                        color: Color.White * _blend, layerDepth: 1f
                    );
            }
            else
            {
                spriteBatch.Draw
                    (
                        Texture,
                        destinationRectangle: BoundingRectangle,
                        color: Color.White * _blend, layerDepth: 1f
                    );
            }
        }

        private void Press()
        {
            _blend = 0.5f;
            _frameCount = 0;
			pressed = true;
        }

        private void UnPress()
        {
            _blend = 1f;
            _frameCount = 0;
			pressed = false;
        }
    }
}
