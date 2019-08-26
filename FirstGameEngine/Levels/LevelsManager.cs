using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FirstGameEngine.Levels
{
    public static class LevelsManager
    {
        private static List<Level> Levels;
		private static Level Tutorial;

        //Loads some content
        public static void Initialize(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            Levels = new List<Level>();

            Level lvl;

			#region Tutorial

			Tutorial = new Level();
			LevelInfo info = new LevelInfo();
			info.Graphics = new LevelGraphics();
			info.Graphics.BackgroundElementsFolder = "background/";
			info.Graphics.ObstaclesFolder = "obstacles/";
			info.Graphics.HeroFolder = "hero/";
			info.Graphics.DecorationsFolder = "decorations/";
			info.Graphics.GuardFolder = "guard/";
			info.RealWorldLocation = "Somewhere in XIX century England";

			Tutorial.Info = info;

#if __IOS__
            Tutorial.Map = new Tiled.IosTiledMap(0, 0);
#else
			Tutorial.Map = new Tiled.TiledMap(0, 0);
#endif
			Tutorial.Map.Load(graphicsDevice, "Content/Test/LevelTest.tmx");
			Tutorial.Map.LoadFront(contentManager, graphicsDevice, "Levels/Level1/Front", "Levels/Level1/Back");

			#endregion

			#region Level 1

			lvl = new Level();            
            LevelInfo Info = new LevelInfo();
            Info.Graphics = new LevelGraphics();
            Info.Graphics.BackgroundElementsFolder = "background/";
            Info.Graphics.ObstaclesFolder = "obstacles/";
            Info.Graphics.HeroFolder = "hero/";
            Info.Graphics.DecorationsFolder = "decorations/";
            Info.Graphics.GuardFolder = "guard/";
            Info.RealWorldLocation = "Англия, Деревня Тибей.";
            //Info.SoundTrack = contentManager.Load<SoundEffect>("Levels/Level1/soundtrack");

            //Info.Graphics.RequiredDecorations = new[] { "grass", "home" };

            lvl.Info = Info;

            //lvl.Animation = new LevelAnimation();
            //lvl.Animation.LoadFromFolder(contentManager, 4, "Levels/Level1/CutScene");
            //lvl.Animation.Duration = 20000;

#if __IOS__
            lvl.Map = new Tiled.IosTiledMap(0, 0);
#else
            lvl.Map = new Tiled.TiledMap(0, 0);
#endif
            lvl.Map.Load(graphicsDevice, "Content/Test/LevelTest.tmx");
            lvl.Map.LoadFront(contentManager, graphicsDevice, "Levels/Level1/Front", "Levels/Level1/Back");

            Levels.Add(lvl);
#endregion

            /*
#region Level 2

            lvl = new Level();
            Info = new LevelInfo();
            Info.Graphics = new LevelGraphics();
            Info.Graphics.BackgroundElementsFolder = "background/";
            Info.Graphics.ObstaclesFolder = "obstacles/";
            Info.Graphics.HeroFolder = "hero/";
            Info.Graphics.DecorationsFolder = "decorations/";
            Info.Graphics.GuardFolder = "guard/";
            Info.Graphics.RequiredDecorations = new[] { "grass", "home" };
            Info.RealWorldLocation = "Город Честерфилд, пригород города Шефилд.";
            Info.SoundTrack = contentManager.Load<SoundEffect>("Levels/Level2/soundtrack");

            lvl.Info = Info;

            lvl.Map = new Tiled.TiledMap(0, 0);
            lvl.Map.Load(graphicsDevice, "Content/Test/Sanya.tmx");

            Levels.Add(lvl);

#endregion

#region Level 3

            lvl = new Level();
            Info = new LevelInfo();
            Info.Graphics = new LevelGraphics();
            Info.Graphics.BackgroundElementsFolder = "background/";
            Info.Graphics.ObstaclesFolder = "obstacles/";
            Info.Graphics.HeroFolder = "hero/";
            Info.Graphics.DecorationsFolder = "decorations/";
            Info.Graphics.GuardFolder = "guard/";
            Info.Graphics.RequiredDecorations = new[] { "grass", "home" };
            Info.RealWorldLocation = "Англия, город Лестер.";
            Info.SoundTrack = contentManager.Load<SoundEffect>("Levels/Level3/soundtrack");

            lvl.Info = Info;

            lvl.Map = new Tiled.TiledMap(0, 0);
            lvl.Map.Load(graphicsDevice, "Content/Test/Sanya.tmx");

            Levels.Add(lvl);

#endregion

#region Level 4

            lvl = new Level();
            Info = new LevelInfo();
            Info.Graphics = new LevelGraphics();
            Info.Graphics.BackgroundElementsFolder = "background/";
            Info.Graphics.ObstaclesFolder = "obstacles/";
            Info.Graphics.HeroFolder = "hero/";
            Info.Graphics.DecorationsFolder = "decorations/";
            Info.Graphics.GuardFolder = "guard/";
            Info.Graphics.RequiredDecorations = new[] { "grass", "home" };
            Info.RealWorldLocation = "Нортгемптонский замок.";
            Info.SoundTrack = contentManager.Load<SoundEffect>("Levels/Level4/soundtrack");

            lvl.Info = Info;

            lvl.Map = new Tiled.TiledMap(0, 0);
            lvl.Map.Load(graphicsDevice, "Content/Test/Sanya.tmx");

            Levels.Add(lvl);

#endregion

#region Level 5

            lvl = new Level();
            Info = new LevelInfo();
            Info.Graphics = new LevelGraphics();
            Info.Graphics.BackgroundElementsFolder = "background/";
            Info.Graphics.ObstaclesFolder = "obstacles/";
            Info.Graphics.HeroFolder = "hero/";
            Info.Graphics.DecorationsFolder = "decorations/";
            Info.Graphics.GuardFolder = "guard/";
            Info.Graphics.RequiredDecorations = new[] { "grass", "home" };
            Info.RealWorldLocation = "Букингемский дворец.";
            Info.SoundTrack = contentManager.Load<SoundEffect>("Levels/Level5/soundtrack");

            lvl.Info = Info;

            lvl.Map = new Tiled.TiledMap(0, 0);
            lvl.Map.Load(graphicsDevice, "Content/Test/LevelTest.tmx");

            Levels.Add(lvl);

#endregion

            */
        }

        public static Level Get(int index)
        {
			if (index == -1)
				return Tutorial;

            if (index >= Levels.Count)
                throw new InvalidOperationException("No such level");


            return Levels[index];
        }
    }
}
