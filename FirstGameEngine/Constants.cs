using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace FirstGameEngine
{
    public static class Constants
    {
        public const float HeroLayerDepth = 0.966f;
        public const float ObstaclesLayerDepth = 0.7f;
        public const float MonsterLayerDepth = 0.71f;

        public const float TextLayerDepth = 0.6f;

        public const float ForeGroundLayerDepth = 0.5f;
        public const float BackgroundLayerDepth = 0.1f;

        public const float BulletsLayerDepth = 0.69f;

        public const float DefaultAnimationFrameDuration = 16f;

        public const int BackgroundLayerIndex = 0;

        //X change in Entity position in DefaultAnimationFrameDuration milliseconds  
        public const float HeroSpeed = 2.5f;
        public const float EntitySpeed = 1.45f;

        //Y change in Entity position in DefaultAnimationFrameDuration milliseconds
        public const float HeroJump = 5f;
        public const float EntityJump = 1.45f;

        public const float MaxJumpHeight = 105f;
        
        public const float gravConst = 9.69f;
        public const float FloorMass = 45000f;
        public const float HeroMass = 75;

        public const int SampleWindowWidth = 480;
        public const int SampleWindowHeight = 270;

        public static Texture2D TramwayTexture = null;
        public static Texture2D EnhancedChestTexture = null;
        public static Texture2D ChestTexture = null;
        public static Texture2D HayStackTexture = null;
        public static Texture2D LaderTexture = null;
    }
}
