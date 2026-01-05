using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Strays.Core
{
    /// <summary>
    /// Represents a Tiled map (.tmx) with support for embedded tilesets.
    /// </summary>
    public class TiledMap
    {
        private int mapWidth;
        private int mapHeight;
        private int tileWidth;
        private int tileHeight;
        private List<TiledLayer> layers;
        private Dictionary<int, TiledTileset> tilesets;
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// Width of the map in tiles.
        /// </summary>
        public int MapWidth => mapWidth;

        /// <summary>
        /// Height of the map in tiles.
        /// </summary>
        public int MapHeight => mapHeight;

        /// <summary>
        /// Width of each tile in pixels.
        /// </summary>
        public int TileWidth => tileWidth;

        /// <summary>
        /// Height of each tile in pixels.
        /// </summary>
        public int TileHeight => tileHeight;

        /// <summary>
        /// Width of the map in pixels.
        /// </summary>
        public int PixelWidth => mapWidth * tileWidth;

        /// <summary>
        /// Height of the map in pixels.
        /// </summary>
        public int PixelHeight => mapHeight * tileHeight;

        /// <summary>
        /// Creates an empty TiledMap.
        /// </summary>
        public TiledMap(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.layers = new List<TiledLayer>();
            this.tilesets = new Dictionary<int, TiledTileset>();
        }

        /// <summary>
        /// Loads a Tiled map from a .tmx file.
        /// </summary>
        public void Load(string tmxPath)
        {
            if (!File.Exists(tmxPath))
            {
                throw new FileNotFoundException($"Map file not found: {tmxPath}");
            }

            var doc = XDocument.Load(tmxPath);
            var mapElement = doc.Root;
            string basePath = Path.GetDirectoryName(tmxPath);

            // Parse map attributes
            mapWidth = int.Parse(mapElement.Attribute("width").Value);
            mapHeight = int.Parse(mapElement.Attribute("height").Value);
            tileWidth = int.Parse(mapElement.Attribute("tilewidth").Value);
            tileHeight = int.Parse(mapElement.Attribute("tileheight").Value);

            // Parse tilesets
            foreach (var tilesetElement in mapElement.Elements("tileset"))
            {
                LoadTileset(tilesetElement, basePath);
            }

            // Parse layers
            foreach (var layerElement in mapElement.Elements("layer"))
            {
                LoadLayer(layerElement);
            }
        }

        private void LoadTileset(XElement tilesetElement, string basePath)
        {
            int firstGid = int.Parse(tilesetElement.Attribute("firstgid").Value);

            // Check if it's an external tileset reference
            var sourceAttr = tilesetElement.Attribute("source");
            if (sourceAttr != null)
            {
                // Load external .tsx file
                string tsxPath = Path.Combine(basePath, sourceAttr.Value);
                if (File.Exists(tsxPath))
                {
                    var tsxDoc = XDocument.Load(tsxPath);
                    tilesetElement = tsxDoc.Root;
                    basePath = Path.GetDirectoryName(tsxPath);
                }
            }

            string name = tilesetElement.Attribute("name")?.Value ?? "unknown";
            int tileW = int.Parse(tilesetElement.Attribute("tilewidth")?.Value ?? tileWidth.ToString());
            int tileH = int.Parse(tilesetElement.Attribute("tileheight")?.Value ?? tileHeight.ToString());
            int tileCount = int.Parse(tilesetElement.Attribute("tilecount")?.Value ?? "0");
            int columns = int.Parse(tilesetElement.Attribute("columns")?.Value ?? "1");

            // Get image element
            var imageElement = tilesetElement.Element("image");
            Texture2D texture = null;

            if (imageElement != null)
            {
                string imageSource = imageElement.Attribute("source").Value;
                string imagePath = Path.Combine(basePath, imageSource);

                // Try to load the texture
                if (File.Exists(imagePath))
                {
                    using (var stream = File.OpenRead(imagePath))
                    {
                        texture = Texture2D.FromStream(graphicsDevice, stream);
                    }
                }
            }

            var tileset = new TiledTileset(firstGid, name, tileW, tileH, tileCount, columns, texture);
            tilesets[firstGid] = tileset;
        }

        private void LoadLayer(XElement layerElement)
        {
            string name = layerElement.Attribute("name")?.Value ?? "Layer";
            int width = int.Parse(layerElement.Attribute("width").Value);
            int height = int.Parse(layerElement.Attribute("height").Value);

            var dataElement = layerElement.Element("data");
            if (dataElement == null)
                return;

            string encoding = dataElement.Attribute("encoding")?.Value ?? "csv";
            int[] tileData = new int[width * height];

            if (encoding == "csv")
            {
                string csvData = dataElement.Value.Trim();
                string[] values = csvData.Split(',');
                for (int i = 0; i < Math.Min(values.Length, tileData.Length); i++)
                {
                    tileData[i] = int.Parse(values[i].Trim());
                }
            }

            layers.Add(new TiledLayer(name, width, height, tileData));
        }

        /// <summary>
        /// Gets the tileset for a given global tile ID.
        /// </summary>
        private TiledTileset GetTilesetForGid(int gid)
        {
            TiledTileset result = null;
            int highestFirstGid = 0;

            foreach (var kvp in tilesets)
            {
                if (kvp.Key <= gid && kvp.Key > highestFirstGid)
                {
                    highestFirstGid = kvp.Key;
                    result = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Draws the map.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition, Vector2 viewportSize)
        {
            // Calculate visible tile range
            int startX = Math.Max(0, (int)(cameraPosition.X / tileWidth) - 1);
            int startY = Math.Max(0, (int)(cameraPosition.Y / tileHeight) - 1);
            int endX = Math.Min(mapWidth, (int)((cameraPosition.X + viewportSize.X) / tileWidth) + 2);
            int endY = Math.Min(mapHeight, (int)((cameraPosition.Y + viewportSize.Y) / tileHeight) + 2);

            foreach (var layer in layers)
            {
                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        int gid = layer.GetTile(x, y);
                        if (gid == 0)
                            continue; // Empty tile

                        var tileset = GetTilesetForGid(gid);
                        if (tileset?.Texture == null)
                            continue;

                        int localId = gid - tileset.FirstGid;
                        Rectangle sourceRect = tileset.GetSourceRectangle(localId);

                        Vector2 position = new Vector2(
                            x * tileWidth - cameraPosition.X,
                            y * tileHeight - cameraPosition.Y);

                        spriteBatch.Draw(
                            tileset.Texture,
                            position,
                            sourceRect,
                            Color.White);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a world position is blocked (has a tile).
        /// </summary>
        public bool IsBlocked(float worldX, float worldY)
        {
            int tileX = (int)(worldX / tileWidth);
            int tileY = (int)(worldY / tileHeight);

            if (tileX < 0 || tileX >= mapWidth || tileY < 0 || tileY >= mapHeight)
                return true; // Outside map bounds is blocked

            // Check all layers for collision
            foreach (var layer in layers)
            {
                int gid = layer.GetTile(tileX, tileY);
                if (gid != 0)
                    return false; // For now, tiles are walkable (can add collision properties later)
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a tileset in a Tiled map.
    /// </summary>
    public class TiledTileset
    {
        public int FirstGid { get; }
        public string Name { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public int TileCount { get; }
        public int Columns { get; }
        public Texture2D Texture { get; }

        public TiledTileset(int firstGid, string name, int tileWidth, int tileHeight,
            int tileCount, int columns, Texture2D texture)
        {
            FirstGid = firstGid;
            Name = name;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            TileCount = tileCount;
            Columns = columns;
            Texture = texture;
        }

        public Rectangle GetSourceRectangle(int localId)
        {
            if (Columns == 0)
                return Rectangle.Empty;

            int x = (localId % Columns) * TileWidth;
            int y = (localId / Columns) * TileHeight;
            return new Rectangle(x, y, TileWidth, TileHeight);
        }
    }

    /// <summary>
    /// Represents a layer in a Tiled map.
    /// </summary>
    public class TiledLayer
    {
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        private readonly int[] tileData;

        public TiledLayer(string name, int width, int height, int[] tileData)
        {
            Name = name;
            Width = width;
            Height = height;
            this.tileData = tileData;
        }

        public int GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return 0;
            return tileData[y * Width + x];
        }
    }
}
