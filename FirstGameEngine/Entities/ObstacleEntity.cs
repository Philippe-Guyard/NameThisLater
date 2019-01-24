using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

//using Foundation;
using Microsoft.Xna.Framework.Graphics;
//using UIKit;

namespace FirstGameEngine.Entities
{
    public class ObstacleEntity : Entity
    {
        public ObstacleType Type { get; set; }

        public ObstacleEntity(Texture2D _image, 
            float _x = 0, float _y = 0, float _layerDepth = Constants.ObstaclesLayerDepth)
            : base(_image, _x, _y, _layerDepth)
        {

        }

        public ObstacleEntity(GraphicsDevice graphicsDevice, string _pic,
            float _x = 0, float _y = 0, float _layerDepth = Constants.ObstaclesLayerDepth)
            : base(graphicsDevice, _pic, _x, _y, _layerDepth)
        {

        }

        public ObstacleEntity(ObstacleType obstacleType ,float x = 0, float y = 0) 
            : base(x, y, Constants.ObstaclesLayerDepth)
        {
            this.Type = obstacleType;
        }

        public override void LoadImage(ContentManager contentManager, string folder = "")
        {
            switch (this.Type)
            {
                case ObstacleType.EnhancedChest:
                    if (Constants.EnhancedChestTexture == null)
                    {
                        base.LoadImage(contentManager, folder + "cool_chest");
                        Constants.EnhancedChestTexture = this.Image;
                    }
                    else
                        this.Image = Constants.EnhancedChestTexture;
                    
                    break;
                case ObstacleType.Chest:
                    if (Constants.ChestTexture == null)
                    {
                        base.LoadImage(contentManager, folder + "chest");
                        Constants.ChestTexture = this.Image;
                    }
                    else
                        this.Image = Constants.ChestTexture;

                    break;
                case ObstacleType.Lader:
                    if (Constants.LaderTexture == null)
                    {
                        base.LoadImage(contentManager, folder + "lader");
                        Constants.LaderTexture = this.Image;
                    }
                    else
                        this.Image = Constants.LaderTexture;

                    break;
                case ObstacleType.Tramway:
                    break;
                case ObstacleType.Floor:
                    this.Image = contentManager.Load<Texture2D>(folder + "Floor");
                    break;
                case ObstacleType.Haystack:
                    if (Constants.ChestTexture == null)
                    {
                        base.LoadImage(contentManager, folder + "haystack");
                        Constants.ChestTexture = this.Image;
                    }
                    else
                        this.Image = Constants.ChestTexture;

                    break;
                default:
                    throw new InvalidOperationException("Obstacle Type not defined");
            }
        }

        public virtual CollisionType CheckCollision(Entity entity)
        {
            return GameEngine.CheckCollision(entity, this);
        }
    }
}