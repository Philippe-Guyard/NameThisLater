using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using FirstGameEngine.Entities;

namespace FirstGameEngine.Levels
{
    public class LevelInfo
    {
        public LevelGraphics Graphics { get; set; }
        public SoundEffect SoundTrack { get; set; }

        public StaticBackground Background { get; set; }


        public string RealWorldLocation { get; set; }

        public LevelInfo()
        {

        }
    }

    public class LevelGraphics
    {
        public string ObstaclesFolder { get; set; }
        public string DecorationsFolder { get; set; }
        public string BackgroundElementsFolder { get; set; }
        public string HeroFolder { get; set; }
        public string GuardFolder { get; set; }

        public string[] RequiredDecorations;
        
        public LevelGraphics()
        {

        }
    }
}
