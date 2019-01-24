using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace FirstGameEngine.Animations
{
    public class AnimationFrame
    {
        public Texture2D Picture { get; set; }
        public Rectangle? SourceRectangle { get; set; }

        public float dX { get; set; }
        public float dY { get; set; }
        public float Duration { get; set; }
        public float TimeUntillNextFrame { get; set; }

        public AnimationFrame(float _x = 0, float _y = 0,
            float _dur = Constants.DefaultAnimationFrameDuration)
        {
            dX = _x;
            dY = _y;

            Duration = _dur;

            SourceRectangle = null;
        }

        public void LoadPicture(GraphicsDevice graphicsDevice, string path)
        {
            using (var stream = TitleContainer.OpenStream(path))
            {
                Picture = Texture2D.FromStream(graphicsDevice, stream);
            }
        }

        public AnimationFrame Copy()
        {
            return new AnimationFrame
            {
                dX = this.dX, dY = this.dY, Duration = this.Duration,
                Picture = this.Picture, SourceRectangle = this.SourceRectangle,
                TimeUntillNextFrame = this.TimeUntillNextFrame
            };
        }
    }
}
