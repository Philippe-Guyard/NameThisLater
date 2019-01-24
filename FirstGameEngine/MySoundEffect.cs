using System;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace FirstGameEngine
{
    public class MySoundEffect
    {
        private SoundEffect BaseEffect;

        private SoundEffectInstance Instance;

        public MySoundEffect(ContentManager contentManager, string path)
        {
            BaseEffect = contentManager.Load<SoundEffect>(path);

            Instance = BaseEffect.CreateInstance();
        }

        public void Play()
        {
            if (Settings.SoundFX)
            {
                if (Instance.State == SoundState.Stopped)
                {
                    Instance.Play();
                }
            }
        }

        public void Stop()
        {
            if (Instance.State != SoundState.Stopped)
            {
                Instance.Stop();
            }
        }
    }
}
