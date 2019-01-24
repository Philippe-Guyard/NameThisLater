using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace FirstGameEngine.Animations
{
    public class TextAnimator
    {
        public const int TimePerLetter = 50;
        public const int TimePerComma = 1000;
        public const int TimeForFinalGlimpse = 3500;

        
        private bool EnterStarted { get; set; }
        private bool HandlingComma { get; set; }
        private bool FinalGlimpse { get; set; }

        private double TimeInto { get; set; }

        private int SplitIndex { get; set; }

        private string Prev { get; set; }

        public bool TextDrawn { get; private set; }
        public string Text { get; private set; }

        public SpriteFont TextFont { get; set; }

        public TextAnimator()
        {

        }

        public void BeginAnimation(string text)
        {
            TimeInto = 0;
            SplitIndex = 0;
            HandlingComma = false;
            Prev = string.Empty;
            FinalGlimpse = false;

            Text = text;

            EnterStarted = true;
        }

        public void DrawText(SpriteBatch spriteBatch, double elapsed)
        {
            if (TextFont == null)
                throw new InvalidOperationException("No font has been specified");
            if (!EnterStarted)
                throw new InvalidOperationException("BeginAnimation was not yet called");

            TimeInto += elapsed;

            string location = Prev;

            if (FinalGlimpse)
            {

            }
            else
            {
                var split = Text.Split(',');
                int len = 0;
                if (SplitIndex < split.Length)
                {
                    len = (int)TimeInto / TimePerLetter;
                    bool flag = HandlingComma;
                    if (!HandlingComma)
                    {
                        HandlingComma = len == split[SplitIndex].Length;
                    }
                    if (flag != HandlingComma)
                    {
                        if (SplitIndex < split.Length)
                            Prev += split[SplitIndex];
                        TimeInto = 0;
                        SplitIndex++;
                        if (SplitIndex < split.Length)
                            Prev += ",";
                    }
                }
                if (HandlingComma)
                {
                    HandlingComma = TimeInto < TimePerComma;
                    if (!HandlingComma)
                        TimeInto = 0;
                }
                else
                {
                    len = Math.Min(len, split[SplitIndex].Length);
                    location += split[SplitIndex].Substring(0, len);
                }

                FinalGlimpse = SplitIndex == split.Length;
            }

            int w = (int)TextFont.MeasureString(location).Length() / 4;

            Vector2 screenCenter = new Vector2(Constants.SampleWindowWidth / 2 - w, Constants.SampleWindowHeight / 2);
            spriteBatch.DrawString
                (
                    spriteFont: TextFont,
                    text: location,
                    position: screenCenter,
                    color: Color.White,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    scale: 0.5f,
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );

            TextDrawn = location.Length >= Text.Length && TimeInto > TimeForFinalGlimpse;
        }
    }
}
