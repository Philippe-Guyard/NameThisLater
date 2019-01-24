using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FirstGameEngine.Entities
{
    public class TriangularLader : Entity
    {
        const float offset = 0;

        public float Alpha { get; set; }
        public float Beta { get { return 90 - this.Alpha; } }
        
        public TriangularLader(float x = 0, float y = 0, float _layerDepth = 1f) : base(x, y, _layerDepth)
        {

        }

        public double GetHeight(float x)
        {
            return Math.Tan(this.Alpha) * (x - this.X) + 25;
        }

        public override void LoadImage(ContentManager contentManager, string pic)
        {
            base.LoadImage(contentManager, pic);

            this.Alpha = (float)(Math.Atan(this.GetRealHeight() / this.GetRealWidth()));
         //   offset = this.GetRealWidth() / 2;      
        }

        public Vector2 ToLocal(float x, float y)
        {
            float nx, ny;
            if (x < this.X || x > this.X + this.GetRealWidth())
                nx = float.NaN;
            else
                nx = x - this.X;

            if (y < this.Y - offset || y > this.GetRealWidth() + this.Y + offset)
                ny = float.NaN;
            else
                ny = y - this.Y;

            return new Vector2(nx, ny);
        }

        public CollisionType CheckCollision(Entity entity)
        {
            var vct = ToLocal(entity.GetRealX(), entity.GetRealY());
            float x0 = vct.X + entity.GetRealWidth();
            float y0 = vct.Y + entity.GetRealHeight() - 5;

            float x1 = -5;           
            float y1 = GetRealHeight() + 5;
            float x2, x3;
            x2 = x3 = this.GetRealWidth();
            float y2 = 0;
            float y3 = this.GetRealHeight();

            float a = (x1 - x0) * (y2 - y1) - (x2 - x1) * (y1 - y0);
            float b = (x2 - x0) * (y3 - y2) - (x3 - x2) * (y2 - y0);
            float c = (x3 - x0) * (y1 - y3) - (x1 - x3) * (y3 - y0);

            if ( (a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0) )
            {
                return CollisionType.Top;
            }

            return CollisionType.NoCollision;
        }
    }
}
