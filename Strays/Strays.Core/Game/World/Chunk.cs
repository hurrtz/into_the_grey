using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Strays.Core.Game.World;

/// <summary>
/// Represents a chunk of the game world - a section of a biome.
/// Chunks are the unit of loading/unloading for world streaming.
/// </summary>
public class Chunk
{
    /// <summary>
    /// Unique identifier for this chunk.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The biome this chunk belongs to.
    /// </summary>
    public BiomeType Biome { get; }

    /// <summary>
    /// World position of this chunk's top-left corner.
    /// </summary>
    public Vector2 WorldPosition { get; }

    /// <summary>
    /// Size of this chunk in pixels.
    /// </summary>
    public Vector2 Size { get; private set; }

    /// <summary>
    /// Whether this chunk is currently loaded.
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// The Tiled map for this chunk (null if not loaded).
    /// </summary>
    public TiledMap? Map { get; private set; }

    /// <summary>
    /// Encounters in this chunk.
    /// </summary>
    public List<Encounter> Encounters { get; } = new();

    /// <summary>
    /// Path to the .tmx file for this chunk.
    /// </summary>
    public string MapPath { get; }

    /// <summary>
    /// Bounding rectangle in world coordinates.
    /// </summary>
    public Rectangle Bounds => new Rectangle(
        (int)WorldPosition.X,
        (int)WorldPosition.Y,
        (int)Size.X,
        (int)Size.Y
    );

    /// <summary>
    /// Creates a new chunk.
    /// </summary>
    /// <param name="id">Unique identifier.</param>
    /// <param name="biome">The biome type.</param>
    /// <param name="worldPosition">Position in world coordinates.</param>
    /// <param name="mapPath">Path to the .tmx file.</param>
    public Chunk(string id, BiomeType biome, Vector2 worldPosition, string mapPath)
    {
        Id = id;
        Biome = biome;
        WorldPosition = worldPosition;
        MapPath = mapPath;
        Size = new Vector2(800, 600); // Default size, updated when map loads
    }

    /// <summary>
    /// Loads the chunk's map and data.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device for texture loading.</param>
    public void Load(GraphicsDevice graphicsDevice)
    {
        if (IsLoaded)
            return;

        try
        {
            if (File.Exists(MapPath))
            {
                Map = new TiledMap(graphicsDevice);
                Map.Load(MapPath);
                Size = new Vector2(Map.PixelWidth, Map.PixelHeight);
            }
            else
            {
                // No map file - create a default empty chunk
                System.Diagnostics.Debug.WriteLine($"Chunk map not found: {MapPath}");
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load chunk {Id}: {ex.Message}");
            IsLoaded = true; // Mark as loaded to prevent repeated attempts
        }
    }

    /// <summary>
    /// Unloads the chunk to free memory.
    /// </summary>
    public void Unload()
    {
        if (!IsLoaded)
            return;

        Map = null;
        IsLoaded = false;

        // Force garbage collection for textures
        GC.Collect();
    }

    /// <summary>
    /// Checks if a world position is within this chunk.
    /// </summary>
    public bool Contains(Vector2 worldPosition)
    {
        return Bounds.Contains((int)worldPosition.X, (int)worldPosition.Y);
    }

    /// <summary>
    /// Checks if a world position is blocked by collision.
    /// </summary>
    public bool IsBlocked(Vector2 worldPosition)
    {
        if (Map == null)
            return false;

        // Convert world position to chunk-local position
        var localPos = worldPosition - WorldPosition;
        return Map.IsBlocked(localPos.X, localPos.Y);
    }

    /// <summary>
    /// Draws the chunk.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="cameraPosition">Camera position in world coordinates.</param>
    /// <param name="viewportSize">Size of the viewport.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition, Vector2 viewportSize)
    {
        if (Map != null)
        {
            // Offset camera by chunk position
            var chunkCamera = cameraPosition - WorldPosition;
            Map.Draw(spriteBatch, chunkCamera, viewportSize);
        }
    }

    /// <summary>
    /// Draws placeholder background when no map is loaded.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture.</param>
    /// <param name="cameraPosition">Camera position in world coordinates.</param>
    public void DrawPlaceholder(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 cameraPosition)
    {
        var screenPos = WorldPosition - cameraPosition;
        var drawRect = new Rectangle(
            (int)screenPos.X,
            (int)screenPos.Y,
            (int)Size.X,
            (int)Size.Y
        );

        // Draw biome-colored background
        spriteBatch.Draw(pixelTexture, drawRect, BiomeData.GetBackgroundColor(Biome));

        // Draw grid lines to show chunk boundaries
        var borderColor = Color.White * 0.2f;
        var gridSize = 64;

        for (int x = 0; x < Size.X; x += gridSize)
        {
            var lineRect = new Rectangle((int)screenPos.X + x, (int)screenPos.Y, 1, (int)Size.Y);
            spriteBatch.Draw(pixelTexture, lineRect, borderColor);
        }

        for (int y = 0; y < Size.Y; y += gridSize)
        {
            var lineRect = new Rectangle((int)screenPos.X, (int)screenPos.Y + y, (int)Size.X, 1);
            spriteBatch.Draw(pixelTexture, lineRect, borderColor);
        }
    }

    /// <summary>
    /// Adds an encounter to this chunk.
    /// </summary>
    public void AddEncounter(Encounter encounter)
    {
        Encounters.Add(encounter);
    }

    /// <summary>
    /// Removes an encounter from this chunk.
    /// </summary>
    public void RemoveEncounter(Encounter encounter)
    {
        Encounters.Remove(encounter);
    }
}
