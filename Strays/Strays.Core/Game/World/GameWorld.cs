using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Game.Entities;
using Strays.Core.Services;

namespace Strays.Core.Game.World;

/// <summary>
/// Manages the entire game world including all biomes, chunks, and world streaming.
/// </summary>
public class GameWorld
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly GameStateService _gameState;
    private readonly Dictionary<string, Chunk> _chunks = new();
    private readonly HashSet<string> _loadedChunkIds = new();

    // World streaming settings
    private const int LoadDistance = 1000;  // Distance to load chunks
    private const int UnloadDistance = 1500; // Distance to unload chunks

    // Camera
    private Vector2 _cameraPosition;
    private Vector2 _viewportSize;

    /// <summary>
    /// All chunks in the world.
    /// </summary>
    public IReadOnlyDictionary<string, Chunk> Chunks => _chunks;

    /// <summary>
    /// Currently loaded chunks.
    /// </summary>
    public IEnumerable<Chunk> LoadedChunks => _loadedChunkIds.Select(id => _chunks[id]);

    /// <summary>
    /// Current camera position (top-left of viewport).
    /// </summary>
    public Vector2 CameraPosition => _cameraPosition;

    /// <summary>
    /// Current viewport size.
    /// </summary>
    public Vector2 ViewportSize => _viewportSize;

    /// <summary>
    /// The base path for Tiled map files.
    /// </summary>
    public string TiledBasePath { get; set; } = "Tiled/biomes/";

    /// <summary>
    /// Creates a new game world.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device for loading.</param>
    /// <param name="gameState">Game state service.</param>
    public GameWorld(GraphicsDevice graphicsDevice, GameStateService gameState)
    {
        _graphicsDevice = graphicsDevice;
        _gameState = gameState;
        _viewportSize = new Vector2(800, 480);
    }

    /// <summary>
    /// Initializes the world with default chunks.
    /// </summary>
    public void Initialize()
    {
        // For Phase 1, create a single chunk for The Fringe
        CreateDefaultFringeChunk();
    }

    /// <summary>
    /// Creates the default starting chunk for The Fringe.
    /// </summary>
    private void CreateDefaultFringeChunk()
    {
        // Find the Tiled maps directory
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var tiledPath = Path.Combine(basePath, "..", "..", "..", "..", "Tiled", "biomes");

        if (!Directory.Exists(tiledPath))
        {
            // Try relative to project root
            tiledPath = Path.Combine(basePath, "..", "..", "..", "..", "..", "Tiled", "biomes");
        }

        // Create the main Fringe chunk
        var fringeMapPath = Path.Combine(tiledPath, "fringe.tmx");

        // If fringe.tmx doesn't exist, use suburb.tmx as placeholder
        if (!File.Exists(fringeMapPath))
        {
            fringeMapPath = Path.Combine(tiledPath, "suburb.tmx");
        }

        var fringeChunk = new Chunk(
            "fringe_main",
            BiomeType.Fringe,
            Vector2.Zero,
            fringeMapPath
        );

        // Set a reasonable default size for the chunk
        fringeChunk.Load(_graphicsDevice);

        // If map didn't load, set a default size
        if (fringeChunk.Map == null)
        {
            // Create a larger placeholder area
            var field = typeof(Chunk).GetProperty("Size");
            // Use reflection or just accept the default 800x600 for now
        }

        _chunks["fringe_main"] = fringeChunk;

        // Add some test encounters
        AddTestEncounters(fringeChunk);
    }

    /// <summary>
    /// Adds test encounters to a chunk for Phase 1 testing.
    /// </summary>
    private void AddTestEncounters(Chunk chunk)
    {
        var random = new Random(42); // Fixed seed for consistent placement

        // Add several encounters spread across the chunk
        for (int i = 0; i < 5; i++)
        {
            var position = new Vector2(
                150 + random.Next((int)chunk.Size.X - 300),
                150 + random.Next((int)chunk.Size.Y - 300)
            );

            var encounter = new Encounter(
                $"encounter_{chunk.Id}_{i}",
                chunk.WorldPosition + position,
                chunk.Biome
            )
            {
                EnemyCount = 1 + random.Next(3),
                LevelRange = (1, 5)
            };

            // Add some placeholder Stray types
            encounter.PossibleStrays.Add("echo_pup");
            encounter.PossibleStrays.Add("circuit_crow");
            encounter.PossibleStrays.Add("relay_rodent");

            chunk.AddEncounter(encounter);
        }
    }

    /// <summary>
    /// Updates the world, handling chunk streaming based on protagonist position.
    /// </summary>
    /// <param name="gameTime">Current game time.</param>
    /// <param name="protagonistPosition">Position of the protagonist.</param>
    public void Update(GameTime gameTime, Vector2 protagonistPosition)
    {
        // Update camera to follow protagonist (centered)
        _cameraPosition = protagonistPosition - _viewportSize / 2;

        // Clamp camera to world bounds (if we have bounds)
        var currentChunk = GetChunkAt(protagonistPosition);
        if (currentChunk != null)
        {
            _cameraPosition.X = Math.Max(0, Math.Min(_cameraPosition.X, currentChunk.Size.X - _viewportSize.X));
            _cameraPosition.Y = Math.Max(0, Math.Min(_cameraPosition.Y, currentChunk.Size.Y - _viewportSize.Y));
        }

        // Handle chunk streaming (for future multi-chunk worlds)
        UpdateChunkStreaming(protagonistPosition);
    }

    /// <summary>
    /// Handles loading/unloading chunks based on distance from protagonist.
    /// </summary>
    private void UpdateChunkStreaming(Vector2 protagonistPosition)
    {
        foreach (var chunk in _chunks.Values)
        {
            var chunkCenter = chunk.WorldPosition + chunk.Size / 2;
            var distance = Vector2.Distance(protagonistPosition, chunkCenter);

            if (distance < LoadDistance && !chunk.IsLoaded)
            {
                chunk.Load(_graphicsDevice);
                _loadedChunkIds.Add(chunk.Id);
            }
            else if (distance > UnloadDistance && chunk.IsLoaded)
            {
                chunk.Unload();
                _loadedChunkIds.Remove(chunk.Id);
            }
        }
    }

    /// <summary>
    /// Gets the chunk at a world position.
    /// </summary>
    public Chunk? GetChunkAt(Vector2 worldPosition)
    {
        foreach (var chunk in _chunks.Values)
        {
            if (chunk.Contains(worldPosition))
                return chunk;
        }
        return null;
    }

    /// <summary>
    /// Checks if a world position is blocked by collision.
    /// </summary>
    public bool IsBlocked(Vector2 worldPosition)
    {
        var chunk = GetChunkAt(worldPosition);
        if (chunk == null)
            return true; // Outside all chunks is blocked

        return chunk.IsBlocked(worldPosition);
    }

    /// <summary>
    /// Gets all encounters that collide with a bounding box.
    /// </summary>
    public IEnumerable<Encounter> GetCollidingEncounters(Rectangle bounds)
    {
        foreach (var chunk in LoadedChunks)
        {
            foreach (var encounter in chunk.Encounters)
            {
                if (encounter.CheckCollision(bounds))
                    yield return encounter;
            }
        }
    }

    /// <summary>
    /// Marks an encounter as cleared.
    /// </summary>
    public void ClearEncounter(string encounterId)
    {
        foreach (var chunk in _chunks.Values)
        {
            var encounter = chunk.Encounters.FirstOrDefault(e => e.Id == encounterId);
            if (encounter != null)
            {
                encounter.Clear();
                _gameState.ClearEncounter(encounterId);
                return;
            }
        }
    }

    /// <summary>
    /// Sets the viewport size for camera calculations.
    /// </summary>
    public void SetViewportSize(Vector2 size)
    {
        _viewportSize = size;
    }

    /// <summary>
    /// Draws all visible chunks and their contents.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture for placeholders.</param>
    /// <param name="font">Font for text rendering.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font)
    {
        // Draw loaded chunks
        foreach (var chunk in LoadedChunks)
        {
            if (chunk.Map != null)
            {
                chunk.Draw(spriteBatch, _cameraPosition, _viewportSize);
            }
            else
            {
                // Draw placeholder if no map loaded
                chunk.DrawPlaceholder(spriteBatch, pixelTexture, _cameraPosition);
            }

            // Draw encounters
            foreach (var encounter in chunk.Encounters)
            {
                encounter.Draw(spriteBatch, pixelTexture, font, _cameraPosition);
            }
        }
    }

    /// <summary>
    /// Adds a chunk to the world.
    /// </summary>
    public void AddChunk(Chunk chunk)
    {
        _chunks[chunk.Id] = chunk;
    }

    /// <summary>
    /// Removes a chunk from the world.
    /// </summary>
    public void RemoveChunk(string chunkId)
    {
        if (_chunks.TryGetValue(chunkId, out var chunk))
        {
            chunk.Unload();
            _chunks.Remove(chunkId);
            _loadedChunkIds.Remove(chunkId);
        }
    }
}
