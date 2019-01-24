using FirstGameEngine.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using TiledSharp;

namespace FirstGameEngine.Tiled
{
    public class TiledMap
    {
        private int _x, _y;

        private TmxMap BaseMap;
        private List<Texture2D> TilesetsAsTextures;

        private List<MarshEntity> Marshes;
        private List<Entity> Pikes;

        private Texture2D Front, Back;

        private List<ActiveLayer> ActiveLayers;
        protected AnimatedTiles Animated;

        public int X
        {
            get => _x;
            set
            {
                if(_x != value)
                {
                    foreach(var layer in ActiveLayers)
                    {
                        layer.Move(value - _x, 0);
                    }

                    _x = value;
                }
            }
        }
        public int Y
        {
            get => _y;
            set
            {
                if(_y != value)
                {
                    foreach(var layer in ActiveLayers)
                    {
                        layer.Move(0, value - _y);
                    }

                    _y = value;
                }
            }
        }

        private int WTileCount(TmxTileset set) => set.Image.Width.Value / set.TileWidth;
        private int HTileCount(TmxTileset set) => set.Image.Height.Value / set.TileHeight;

        private int TileSetIndex(TmxLayerTile tile) => BaseMap.Tilesets.IndexOf(BaseMap.Tilesets.Last(x => x.FirstGid <= tile.Gid));
        private int TileSetIndex(TmxTilesetTile tile) => BaseMap.Tilesets.IndexOf(BaseMap.Tilesets.Last(x => x.FirstGid <= tile.Id));

        private TmxTilesetTile FromLayerTile(TmxLayerTile tile) => BaseMap.Tilesets[TileSetIndex(tile)].Tiles.FirstOrDefault(x => x.Id == tile.Gid - 1);

        private int GetTileX(TmxLayerTile tile) => this.X + TileWidth * tile.X;
        private int GetTileY(TmxLayerTile tile) => this.Y + TileHeight * tile.Y;

        private TmxTilesetTile BaseTile(TmxTilesetTile frame) => BaseMap.Tilesets[TileSetIndex(frame)].Tiles
            .FirstOrDefault(x => x.Id >= frame.Id && x.Id - x.AnimationFrames.Count <= frame.Id);

        public int TileWidth => BaseMap.TileWidth;
        public int TileHeight => BaseMap.TileHeight;

        public int Width => BaseMap.Width * TileWidth;
        public int Height => BaseMap.Height * TileHeight;

        public List<MarshEntity> GetMarshes() => Marshes;
        public List<Entity> GetPikes() => Pikes;

        private double TotalDuration { get; set; }

        public TiledMap(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Load(GraphicsDevice graphicsDevice, string path)
        {
            BaseMap = new TmxMap(path);

            TilesetsAsTextures = new List<Texture2D>();
            Texture2D newSet;
            foreach(var set in BaseMap.Tilesets)
            {
                using(var stream = TitleContainer.OpenStream(set.Image.Source))
                {
                    newSet = Texture2D.FromStream(graphicsDevice, stream);
                }

                TilesetsAsTextures.Add(newSet);
            }

            ActiveLayers = new List<ActiveLayer>();

            ActiveLayers.Add(new ActiveLayer(this, "Floor", new CollisionType[] { CollisionType.Top }));
            ActiveLayers.Add(new ActiveLayer(this, "Walls", new CollisionType[] { CollisionType.Left, CollisionType.Right }));
            ActiveLayers.Add(new ActiveLayer(this, "Ceiling", new CollisionType[] { CollisionType.Bottom }));

            Animated = new AnimatedTiles(this);

            var possible = new string[] { "Marshes", "Pikes" };
            foreach(string layerName in possible)
            {
                var layer = BaseMap.Layers[layerName];
                foreach(var tile in layer.Tiles)
                {
                    if(tile.Gid > 0)
                    {
                        int idx = TileSetIndex(tile);
                        int frame = tile.Gid - 1;
                        var tmp = BaseMap.Tilesets[idx].Tiles.FirstOrDefault(x => x.Id >= frame && x.Id - x.AnimationFrames.Count <= frame);
                        if(tmp != null)
                        {
                            Animated.Add(tile);
                        }
                    }
                }
            }

            Marshes = new List<MarshEntity>();

            MarshBuilder temp = new MarshBuilder(this);
            MarshEntity marsh;
            foreach(var tile in BaseMap.Layers["Marshes"].Tiles)
            {
                if(tile.Gid > 0)
                {
                    if(temp.Add(tile, out marsh))
                    {
                        Marshes.Add(marsh);
                    }
                }
            }

            if(temp.TryGet(out marsh))
            {
                Marshes.Add(marsh);
            }

            Pikes = new List<Entity>();
            foreach(var tile in BaseMap.Layers["Pikes"].Tiles)
            {
                if (tile.Gid > 0)
                {
                    Entity e = new Entity(GetTileX(tile), GetTileY(tile), 0.95f);
                    e.RealWidth = TileWidth;
                    e.RealHeight = TileHeight;
                    e.Tag = "Pike";
                    
                    Pikes.Add(e);                   
                }
            }
        }

        public virtual void LoadFront(Microsoft.Xna.Framework.Content.ContentManager contentManager, GraphicsDevice graphicsDevice,  string front, string back)
        {
            Front = contentManager.Load<Texture2D>(front);
            Back = contentManager.Load<Texture2D>(back);
        }

        public void Update(GameTime gameTime)
        {
            TotalDuration += gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle destination)
        {
            if(Front != null)
            {
#pragma warning disable CS0618 // Type or member is obsolete

                int x = Math.Min(this.Width - 1, Math.Max(destination.X, this.X));
                int y = Math.Min(this.Height - 1, Math.Max(this.Y, destination.Y));

                Rectangle source = new Rectangle(x, y, Math.Min(this.Width - x, destination.Width), Math.Min(this.Height - y, destination.Height));
                destination = new Rectangle(destination.X, destination.Y, source.Width + 2, source.Height + 2);

                spriteBatch.Draw
                    (
                        texture: Front,
                        destinationRectangle: destination,
                        sourceRectangle: source,
                        color: Color.White,
                        layerDepth: Constants.MonsterLayerDepth - 0.01f
                    );

                spriteBatch.Draw
                    (
                        texture: Back,
                        destinationRectangle: destination,
                        sourceRectangle: source,
                        color: Color.White,
                        layerDepth: Constants.MonsterLayerDepth - 0.02f
                    );

                Animated.Draw(spriteBatch);
                return;
            }

            /*
            var offset = Tuple.Create(this.X, this.Y);

            foreach(TmxLayer layer in BaseMap.Layers)
            {
                foreach(TmxLayerTile tile in layer.Tiles)
                {
                    if(tile.Gid > 0)
                    {
                        float depth = Constants.MonsterLayerDepth - 0.01f;
                        if(layer.Name.Contains("BG"))
                        {
                            depth /= 2f;
                        }

                        DrawTile(spriteBatch, tile, TileSetIndex(tile), depth, offset);
                    }
                }
            }
            */
        }

        public bool CheckCollision(Entity entity, out List<Entity> possibleCollisions)
        {
            possibleCollisions = new List<Entity>();
            foreach(var layer in ActiveLayers)
            {
                possibleCollisions.AddRange(layer.Check(entity));
            }

            return possibleCollisions.Count > 0;
        }

        private void DrawTile(SpriteBatch spriteBatch, TmxLayerTile tile, int setIndex, float layerDepth, Tuple<int, int> offset)
        {
#pragma warning disable CS0618 // Type or member is obsolete

            if(tile.Gid == 0)
            {
                return; //Empty tile
            }

            int tileFrame = tile.Gid - 1;
            TmxTileset set = BaseMap.Tilesets[setIndex];
            TmxTilesetTile setTile = set.Tiles.FirstOrDefault(x => x.Id >= tileFrame && x.Id - x.AnimationFrames.Count <= tileFrame);
            if(setTile != null && BaseTile(setTile) != null)
            {
                int duration = setTile.AnimationFrames.Sum(x => x.Duration);
                int tempDuration = (int)TotalDuration % duration;

                foreach(TmxAnimationFrame frame in setTile.AnimationFrames)
                {
                    if(tempDuration <= frame.Duration)
                    {
                        tileFrame = frame.Id - 1;
                        break;
                    }
                    else
                    {
                        tempDuration -= frame.Duration;
                    }
                }

                tileFrame = tileFrame + 1;
            }

            int setWCount = WTileCount(set);
            int column = tileFrame % setWCount;
            int row = (int)Math.Floor(tileFrame * 1f / setWCount);

            Rectangle sourceRectangle = new Rectangle(column * TileWidth, row * TileHeight, TileWidth, TileHeight);

            //tile.X and tile.Y are relative 
            Rectangle destinationRectangle = new Rectangle(
                offset.Item1 + tile.X * TileWidth,
                offset.Item2 + tile.Y * TileHeight,
                TileWidth,
                TileHeight);

            SpriteEffects effects = tile.HorizontalFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            effects = tile.VerticalFlip ? SpriteEffects.FlipVertically : SpriteEffects.None;

            spriteBatch.Draw
                (
                    texture: TilesetsAsTextures[setIndex],
                    destinationRectangle: destinationRectangle,
                    sourceRectangle: sourceRectangle,
                    color: Color.White,
                    effects: effects,
                    layerDepth: layerDepth
                );
        }

        private class ActiveLayer
        {
            readonly TiledMap Source;
            readonly CollisionType[] Collisions;

            public string SourceName;
            private List<Entity> Entities;

            public ActiveLayer(TiledMap source, string name, IEnumerable<CollisionType> collisions)
            {
                this.SourceName = name;
                var BaseMap = source.BaseMap;
                Collisions = collisions.ToArray();

                Entities = new List<Entity>();
                foreach(TmxLayerTile tile in BaseMap.Layers[SourceName].Tiles)
                {
                    if(tile.Gid == 0) //Tile does not exist
                    {
                        continue;
                    }

                    Entity tileEntity = new Entity();
                    tileEntity.RealWidth = source.TileWidth;
                    tileEntity.RealHeight = source.TileHeight;
                    tileEntity.RealX = tileEntity.RealY = 0;

                    tileEntity.X = source.GetTileX(tile);
                    tileEntity.Y = source.GetTileY(tile);

                    Entities.Add(tileEntity);
                }
            }

            public List<Entity> Check(Entity entity)
            {
                List<Entity> possibleCollisions = new List<Entity>();

                foreach(var posCol in Entities)
                {
                    CollisionType colType = GameEngine.CheckCollision(entity, posCol);
                    if(Collisions.Contains(colType))
                    {
                        possibleCollisions.Add(posCol);
                    }
                }

                return possibleCollisions;

            }

            public void Move(int deltaX, int deltaY)
            {
                foreach(var entity in Entities)
                {
                    entity.X += deltaX;
                    entity.Y += deltaY;
                }
            }
        }

        protected class AnimatedTiles
        {
            private List<TmxLayerTile> Tiles;
            public TiledMap Source;

            public AnimatedTiles(TiledMap _source)
            {
                Source = _source;

                Tiles = new List<TmxLayerTile>();
            }

            public void Add(TmxLayerTile tile)
            {
                Tiles.Add(tile);
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                Tuple<int, int> offset = Tuple.Create(Source.X, Source.Y);
                foreach(var tile in Tiles)
                {
                    Source.DrawTile(spriteBatch, tile, Source.TileSetIndex(tile), 0.97f, offset);
                }
            }
        }

        private class MarshBuilder
        {
            private Rectangle? CurrentMarsh;
            readonly TiledMap Source;

            public MarshBuilder(TiledMap src)
            {
                CurrentMarsh = null;

                Source = src;
            }

            public bool Add(TmxLayerTile tile, out MarshEntity marsh)
            {
                marsh = null;
                if(CurrentMarsh.HasValue)
                {
                    if(Source.GetTileX(tile) == CurrentMarsh.Value.X + CurrentMarsh.Value.Width
                     && Source.GetTileY(tile) >= CurrentMarsh.Value.Y
                     && Source.GetTileY(tile) <= CurrentMarsh.Value.Y + CurrentMarsh.Value.Height)
                    {
                        CurrentMarsh = new Rectangle
                            (
                                CurrentMarsh.Value.X,
                                CurrentMarsh.Value.Y,
                                CurrentMarsh.Value.Width + Source.TileWidth,
                                CurrentMarsh.Value.Height
                            );

                        return false;
                    }
                    else
                    {
                        TryGet(out marsh); //Has to return true, since CurrentMarsh != null

                        CurrentMarsh = new Rectangle(Source.GetTileX(tile), Source.GetTileY(tile), Source.TileWidth, Source.TileHeight);

                        return true;
                    }
                }
                else
                {
                    CurrentMarsh = new Rectangle(Source.GetTileX(tile), Source.GetTileY(tile), Source.TileWidth, Source.TileHeight);
                    return false;
                }
            }

            public bool TryGet(out MarshEntity marsh)
            {
                marsh = null;
                if(CurrentMarsh.HasValue)
                {
                    marsh = new MarshEntity(CurrentMarsh.Value.X, CurrentMarsh.Value.Y, CurrentMarsh.Value.Width, CurrentMarsh.Value.Height);
                    return true;
                }

                return false;
            }
        }
    }
    
    public class IosTiledMap : TiledMap
    {
        private List<Texture2D> FrontTextures, BackTextures;
        private int TextureWidth; //Textures are not split by height

        public IosTiledMap(int x, int y) : base(x, y)
        {

        }

        public override void LoadFront(ContentManager contentManager, GraphicsDevice graphicsDevice, string front, string back)
        {
            var frontFiles = Directory.EnumerateFiles(contentManager.RootDirectory + "/" + front)
                .Select(x => front + "/" + Path.GetFileNameWithoutExtension(x));

            var backFiles = Directory.EnumerateFiles(contentManager.RootDirectory + "/" + back)
                .Select(x => back + "/" + Path.GetFileNameWithoutExtension(x));

            FrontTextures = new List<Texture2D>();
            foreach(var name in frontFiles)
                FrontTextures.Add(contentManager.Load<Texture2D>(name));

            BackTextures = new List<Texture2D>();
            foreach(var name in backFiles)
                BackTextures.Add(contentManager.Load<Texture2D>(name));

            TextureWidth = FrontTextures[0].Width;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle destination)
        {
            base.Animated.Draw(spriteBatch);

            //Strip source and destination to picture boundaries
            int x = Math.Min(this.Width - 1, Math.Max(destination.X, this.X));
            int y = Math.Min(this.Height - 1, Math.Max(this.Y, destination.Y));

            Rectangle source = new Rectangle(x, y, Math.Min(this.Width - x, destination.Width), Math.Min(this.Height - y, destination.Height));
            destination = new Rectangle(destination.X, destination.Y, source.Width, source.Height);

            int idx = destination.X / TextureWidth;
            int width = destination.Width;
            int rightBorder = ( idx + 1 ) * TextureWidth;
            if(destination.X + width > rightBorder)
                width = rightBorder - destination.X;


            Rectangle source1 = new Rectangle(destination.X % TextureWidth, destination.Y, width, destination.Height);
            Rectangle destination1 = new Rectangle(destination.X, destination.Y, width, destination.Height);

            Rectangle? source2 = null, destination2 = null;
            if (width < destination.Width)
            {
                source2 = new Rectangle(0, destination.Y, destination.Width - width, destination.Height);
                destination2 = new Rectangle(destination1.X + destination1.Width, destination.Y, source2.Value.Width, destination.Height);
            }


            spriteBatch.Draw
                (
                    texture: FrontTextures[idx],
                    destinationRectangle: destination1,
                    sourceRectangle: source1,
                    color: Color.White,
                    layerDepth: Constants.MonsterLayerDepth - 0.01f
                );

            spriteBatch.Draw
            (
                texture: BackTextures[idx],
                destinationRectangle: destination1,
                sourceRectangle: source1,
                color: Color.White,
                layerDepth: Constants.MonsterLayerDepth - 0.02f
            );


            if(source2.HasValue)
            {
                idx++;
                if(idx == FrontTextures.Count)
                    return;
                spriteBatch.Draw
                (
                    texture: FrontTextures[idx],
                    destinationRectangle: destination2,
                    sourceRectangle: source2,
                    color: Color.White,
                    layerDepth: Constants.MonsterLayerDepth - 0.01f
                );

                spriteBatch.Draw
                (
                    texture: BackTextures[idx],
                    destinationRectangle: destination2,
                    sourceRectangle: source2,
                    color: Color.White,
                    layerDepth: Constants.MonsterLayerDepth - 0.02f
                );
            }
        }
    }
}
