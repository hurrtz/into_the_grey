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
/// Simulation tier for chunks - determines update frequency and detail level.
/// </summary>
public enum ChunkSimulationTier
{
    /// <summary>
    /// Fully active - updates every frame, full rendering.
    /// </summary>
    Active,

    /// <summary>
    /// Adjacent chunks - updates less frequently, simplified simulation.
    /// </summary>
    Adjacent,

    /// <summary>
    /// Distant chunks - minimal updates, may be unloaded.
    /// </summary>
    Distant,

    /// <summary>
    /// Unloaded - not in memory.
    /// </summary>
    Unloaded
}

/// <summary>
/// Manages the entire game world including all biomes, chunks, and world streaming.
/// </summary>
public class GameWorld
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly GameStateService _gameState;
    private readonly Dictionary<string, Chunk> _chunks = new();
    private readonly HashSet<string> _loadedChunkIds = new();
    private readonly Dictionary<string, ChunkSimulationTier> _chunkTiers = new();
    private readonly Random _random = new();

    // World streaming settings (in pixels)
    private const int ActiveDistance = 800;    // Fully simulated
    private const int AdjacentDistance = 1600; // Partially simulated
    private const int LoadDistance = 2400;     // Loaded but minimal updates
    private const int UnloadDistance = 3200;   // Unload threshold

    // Weather system
    private WeatherType _currentWeather = WeatherType.None;
    private float _weatherTimer = 0f;
    private float _weatherDuration = 0f;
    private float _weatherTransition = 0f;

    // Camera
    private Vector2 _cameraPosition;
    private Vector2 _viewportSize;

    // Current biome tracking
    private BiomeType _currentBiome = BiomeType.Fringe;
    private Chunk? _currentChunk;

    /// <summary>
    /// All chunks in the world.
    /// </summary>
    public IReadOnlyDictionary<string, Chunk> Chunks => _chunks;

    /// <summary>
    /// Currently loaded chunks.
    /// </summary>
    public IEnumerable<Chunk> LoadedChunks => _loadedChunkIds.Select(id => _chunks[id]);

    /// <summary>
    /// Active chunks (fully simulated).
    /// </summary>
    public IEnumerable<Chunk> ActiveChunks =>
        _chunkTiers.Where(kvp => kvp.Value == ChunkSimulationTier.Active)
                   .Select(kvp => _chunks[kvp.Key]);

    /// <summary>
    /// Current camera position (top-left of viewport).
    /// </summary>
    public Vector2 CameraPosition => _cameraPosition;

    /// <summary>
    /// Current viewport size.
    /// </summary>
    public Vector2 ViewportSize => _viewportSize;

    /// <summary>
    /// The current biome the protagonist is in.
    /// </summary>
    public BiomeType CurrentBiome => _currentBiome;

    /// <summary>
    /// The current chunk the protagonist is in.
    /// </summary>
    public Chunk? CurrentChunk => _currentChunk;

    /// <summary>
    /// Current weather effect.
    /// </summary>
    public WeatherType CurrentWeather => _currentWeather;

    /// <summary>
    /// Weather transition progress (0-1).
    /// </summary>
    public float WeatherTransition => _weatherTransition;

    /// <summary>
    /// The base path for Tiled map files.
    /// </summary>
    public string TiledBasePath { get; set; } = "Tiled/biomes/";

    /// <summary>
    /// Event fired when biome changes.
    /// </summary>
    public event Action<BiomeType, BiomeType>? BiomeChanged;

    /// <summary>
    /// Event fired when weather changes.
    /// </summary>
    public event Action<WeatherType>? WeatherChanged;

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
    /// Initializes the world with all biome chunks arranged in the world.
    /// </summary>
    public void Initialize()
    {
        // Create the full world with all biomes
        CreateWorldLayout();

        // Initialize weather for starting biome
        UpdateWeatherForBiome(_currentBiome);
    }

    /// <summary>
    /// Creates the world layout with all biomes positioned correctly.
    /// The layout follows the game's progression path and biome connectivity.
    /// </summary>
    private void CreateWorldLayout()
    {
        var tiledPath = FindTiledPath();

        // World layout (approximate positions based on connectivity):
        //
        //                    [Archive Scar]
        //                         |
        //    [Rust] --- [Fringe] --- [Green]
        //      |           |           |
        //   [Teeth] --- [Quiet] -------+
        //      |
        //    [Glow]
        //

        // Get chunk sizes from biome definitions
        var fringeSize = BiomeData.GetChunkSize(BiomeType.Fringe);
        var rustSize = BiomeData.GetChunkSize(BiomeType.Rust);
        var greenSize = BiomeData.GetChunkSize(BiomeType.Green);
        var quietSize = BiomeData.GetChunkSize(BiomeType.Quiet);
        var teethSize = BiomeData.GetChunkSize(BiomeType.Teeth);
        var glowSize = BiomeData.GetChunkSize(BiomeType.Glow);
        var archiveSize = BiomeData.GetChunkSize(BiomeType.ArchiveScar);

        // Center position for Fringe (starting biome)
        var fringePos = Vector2.Zero;

        // Calculate positions based on connectivity
        var rustPos = new Vector2(fringePos.X - rustSize.Width, fringePos.Y);
        var greenPos = new Vector2(fringePos.X + fringeSize.Width, fringePos.Y);
        var quietPos = new Vector2(fringePos.X, fringePos.Y + fringeSize.Height);
        var teethPos = new Vector2(rustPos.X, rustPos.Y + rustSize.Height);
        var glowPos = new Vector2(teethPos.X, teethPos.Y + teethSize.Height);
        var archivePos = new Vector2(greenPos.X, greenPos.Y - archiveSize.Height);

        // Create chunks for each biome
        CreateBiomeChunk("fringe_main", BiomeType.Fringe, fringePos, tiledPath);
        CreateBiomeChunk("rust_main", BiomeType.Rust, rustPos, tiledPath);
        CreateBiomeChunk("green_main", BiomeType.Green, greenPos, tiledPath);
        CreateBiomeChunk("quiet_main", BiomeType.Quiet, quietPos, tiledPath);
        CreateBiomeChunk("teeth_main", BiomeType.Teeth, teethPos, tiledPath);
        CreateBiomeChunk("glow_main", BiomeType.Glow, glowPos, tiledPath);
        CreateBiomeChunk("archive_main", BiomeType.ArchiveScar, archivePos, tiledPath);

        // Initialize all chunks as unloaded
        foreach (var chunk in _chunks.Values)
        {
            _chunkTiers[chunk.Id] = ChunkSimulationTier.Unloaded;
        }

        // Load the starting chunk (Fringe)
        if (_chunks.TryGetValue("fringe_main", out var startChunk))
        {
            startChunk.Load(_graphicsDevice);
            _loadedChunkIds.Add(startChunk.Id);
            _chunkTiers[startChunk.Id] = ChunkSimulationTier.Active;
            _currentChunk = startChunk;
        }
    }

    /// <summary>
    /// Finds the Tiled maps directory.
    /// </summary>
    private string FindTiledPath()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var tiledPath = Path.Combine(basePath, "..", "..", "..", "..", "Tiled", "biomes");

        if (!Directory.Exists(tiledPath))
        {
            tiledPath = Path.Combine(basePath, "..", "..", "..", "..", "..", "Tiled", "biomes");
        }

        return tiledPath;
    }

    /// <summary>
    /// Creates a chunk for a specific biome.
    /// </summary>
    private void CreateBiomeChunk(string id, BiomeType biome, Vector2 position, string tiledPath)
    {
        var biomeDef = BiomeData.GetDefinition(biome);
        var mapPath = Path.Combine(tiledPath, biomeDef.MapFile);

        // If specific map doesn't exist, use suburb.tmx as placeholder
        if (!File.Exists(mapPath))
        {
            var suburbPath = Path.Combine(tiledPath, "suburb.tmx");
            if (File.Exists(suburbPath))
            {
                mapPath = suburbPath;
            }
        }

        var chunk = new Chunk(id, biome, position, mapPath);

        // Set size from biome definition
        SetChunkSize(chunk, biomeDef.ChunkWidth, biomeDef.ChunkHeight);

        _chunks[id] = chunk;

        // Add biome-appropriate encounters
        AddBiomeEncounters(chunk);
    }

    /// <summary>
    /// Sets the chunk size from biome definition.
    /// </summary>
    private void SetChunkSize(Chunk chunk, int width, int height)
    {
        chunk.Size = new Vector2(width, height);
    }

    /// <summary>
    /// Adds biome-appropriate encounters to a chunk.
    /// </summary>
    private void AddBiomeEncounters(Chunk chunk)
    {
        var biomeDef = BiomeData.GetDefinition(chunk.Biome);
        var encounterCount = (int)(5 * biomeDef.EncounterRate);

        for (int i = 0; i < encounterCount; i++)
        {
            var position = new Vector2(
                150 + _random.Next((int)chunk.Size.X - 300),
                150 + _random.Next((int)chunk.Size.Y - 300)
            );

            var levelRange = biomeDef.LevelRange;
            var encounter = new Encounter(
                $"encounter_{chunk.Id}_{i}",
                chunk.WorldPosition + position,
                chunk.Biome
            )
            {
                EnemyCount = 1 + _random.Next(3),
                LevelRange = levelRange
            };

            // Add native Strays from biome
            foreach (var stray in biomeDef.NativeStrays)
            {
                encounter.PossibleStrays.Add(stray);
            }

            // Small chance to add rare Strays
            if (_random.NextDouble() < 0.1)
            {
                foreach (var rareStray in biomeDef.RareStrays)
                {
                    encounter.PossibleStrays.Add(rareStray);
                }
            }

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
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update camera to follow protagonist (centered)
        _cameraPosition = protagonistPosition - _viewportSize / 2;

        // Track current chunk and biome changes
        var newChunk = GetChunkAt(protagonistPosition);
        if (newChunk != null && newChunk != _currentChunk)
        {
            var oldBiome = _currentBiome;
            _currentChunk = newChunk;
            _currentBiome = newChunk.Biome;

            if (oldBiome != _currentBiome)
            {
                BiomeChanged?.Invoke(oldBiome, _currentBiome);
                UpdateWeatherForBiome(_currentBiome);
            }
        }

        // Clamp camera to current chunk bounds
        if (_currentChunk != null)
        {
            var chunkBounds = _currentChunk.Bounds;
            _cameraPosition.X = Math.Max(chunkBounds.X, Math.Min(_cameraPosition.X, chunkBounds.Right - _viewportSize.X));
            _cameraPosition.Y = Math.Max(chunkBounds.Y, Math.Min(_cameraPosition.Y, chunkBounds.Bottom - _viewportSize.Y));
        }

        // Handle chunk streaming with tiered simulation
        UpdateChunkStreaming(protagonistPosition);

        // Update weather
        UpdateWeather(deltaTime);

        // Update active chunks
        UpdateActiveChunks(gameTime);
    }

    /// <summary>
    /// Handles loading/unloading chunks based on distance from protagonist.
    /// Uses tiered simulation for performance.
    /// </summary>
    private void UpdateChunkStreaming(Vector2 protagonistPosition)
    {
        foreach (var chunk in _chunks.Values)
        {
            var chunkCenter = chunk.WorldPosition + chunk.Size / 2;
            var distance = Vector2.Distance(protagonistPosition, chunkCenter);

            // Determine the appropriate tier
            ChunkSimulationTier newTier;
            if (distance < ActiveDistance)
            {
                newTier = ChunkSimulationTier.Active;
            }
            else if (distance < AdjacentDistance)
            {
                newTier = ChunkSimulationTier.Adjacent;
            }
            else if (distance < LoadDistance)
            {
                newTier = ChunkSimulationTier.Distant;
            }
            else
            {
                newTier = ChunkSimulationTier.Unloaded;
            }

            var currentTier = _chunkTiers.GetValueOrDefault(chunk.Id, ChunkSimulationTier.Unloaded);

            // Handle tier changes
            if (newTier != currentTier)
            {
                // Load chunk if entering loaded range
                if (newTier != ChunkSimulationTier.Unloaded && !chunk.IsLoaded)
                {
                    chunk.Load(_graphicsDevice);
                    _loadedChunkIds.Add(chunk.Id);
                }
                // Unload if leaving loaded range (with hysteresis)
                else if (newTier == ChunkSimulationTier.Unloaded && distance > UnloadDistance && chunk.IsLoaded)
                {
                    chunk.Unload();
                    _loadedChunkIds.Remove(chunk.Id);
                }

                _chunkTiers[chunk.Id] = newTier;
            }
        }
    }

    /// <summary>
    /// Updates chunks based on their simulation tier.
    /// </summary>
    private void UpdateActiveChunks(GameTime gameTime)
    {
        foreach (var kvp in _chunkTiers)
        {
            if (!_chunks.TryGetValue(kvp.Key, out var chunk))
                continue;

            switch (kvp.Value)
            {
                case ChunkSimulationTier.Active:
                    // Full update every frame - encounters respawn, full AI
                    break;

                case ChunkSimulationTier.Adjacent:
                    // Simplified update - less frequent
                    break;

                case ChunkSimulationTier.Distant:
                    // Minimal update - just track state
                    break;
            }
        }
    }

    /// <summary>
    /// Updates the weather for a new biome.
    /// </summary>
    private void UpdateWeatherForBiome(BiomeType biome)
    {
        var biomeDef = BiomeData.GetDefinition(biome);
        var possibleWeather = biomeDef.PossibleWeather;

        if (possibleWeather.Count == 0 || (possibleWeather.Count == 1 && possibleWeather[0] == WeatherType.None))
        {
            SetWeather(WeatherType.None);
            return;
        }

        // Random chance for weather
        if (_random.NextDouble() < 0.3)
        {
            var weatherIndex = _random.Next(possibleWeather.Count);
            SetWeather(possibleWeather[weatherIndex]);
            _weatherDuration = 30f + (float)_random.NextDouble() * 60f; // 30-90 seconds
        }
        else
        {
            SetWeather(WeatherType.None);
            _weatherDuration = 60f + (float)_random.NextDouble() * 120f; // 60-180 seconds until weather starts
        }
    }

    /// <summary>
    /// Sets the current weather with transition.
    /// </summary>
    private void SetWeather(WeatherType weather)
    {
        if (_currentWeather != weather)
        {
            _currentWeather = weather;
            _weatherTransition = 0f;
            WeatherChanged?.Invoke(weather);
        }
    }

    /// <summary>
    /// Updates weather timer and transitions.
    /// </summary>
    private void UpdateWeather(float deltaTime)
    {
        // Update transition
        if (_weatherTransition < 1f)
        {
            _weatherTransition = Math.Min(1f, _weatherTransition + deltaTime * 0.2f); // 5 second transition
        }

        // Update duration timer
        _weatherTimer += deltaTime;
        if (_weatherTimer >= _weatherDuration)
        {
            _weatherTimer = 0f;
            UpdateWeatherForBiome(_currentBiome);
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
            _chunkTiers.Remove(chunkId);
        }
    }

    /// <summary>
    /// Gets all chunks that belong to a specific biome.
    /// </summary>
    public IEnumerable<Chunk> GetChunksInBiome(BiomeType biome)
    {
        return _chunks.Values.Where(c => c.Biome == biome);
    }

    /// <summary>
    /// Gets chunks that connect to a given biome based on biome connectivity.
    /// </summary>
    public IEnumerable<Chunk> GetConnectedChunks(BiomeType fromBiome)
    {
        var connectedBiomes = BiomeData.GetConnectedBiomes(fromBiome);
        return _chunks.Values.Where(c => connectedBiomes.Contains(c.Biome));
    }

    /// <summary>
    /// Checks if a position is near a biome boundary.
    /// </summary>
    /// <param name="position">World position to check.</param>
    /// <param name="threshold">Distance from boundary (default 100 pixels).</param>
    /// <returns>True if near a boundary.</returns>
    public bool IsNearBiomeBoundary(Vector2 position, float threshold = 100f)
    {
        var currentChunk = GetChunkAt(position);
        if (currentChunk == null)
            return false;

        var bounds = currentChunk.Bounds;

        // Check distance to each edge
        return position.X - bounds.Left < threshold ||
               bounds.Right - position.X < threshold ||
               position.Y - bounds.Top < threshold ||
               bounds.Bottom - position.Y < threshold;
    }

    /// <summary>
    /// Gets the biome transition zone if the player is near a boundary.
    /// </summary>
    public (BiomeType From, BiomeType To)? GetBiomeTransitionZone(Vector2 position)
    {
        var currentChunk = GetChunkAt(position);
        if (currentChunk == null)
            return null;

        if (!IsNearBiomeBoundary(position, 150f))
            return null;

        // Find the nearest adjacent biome chunk
        var bounds = currentChunk.Bounds;
        Chunk? nearestOther = null;
        float nearestDistance = float.MaxValue;

        foreach (var chunk in _chunks.Values)
        {
            if (chunk == currentChunk)
                continue;

            // Only consider connected biomes
            if (!BiomeData.AreConnected(currentChunk.Biome, chunk.Biome))
                continue;

            var otherCenter = chunk.WorldPosition + chunk.Size / 2;
            var distance = Vector2.Distance(position, otherCenter);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestOther = chunk;
            }
        }

        if (nearestOther != null)
        {
            return (currentChunk.Biome, nearestOther.Biome);
        }

        return null;
    }

    /// <summary>
    /// Gets the simulation tier for a chunk.
    /// </summary>
    public ChunkSimulationTier GetChunkTier(string chunkId)
    {
        return _chunkTiers.GetValueOrDefault(chunkId, ChunkSimulationTier.Unloaded);
    }

    /// <summary>
    /// Gets environmental hazards at a position.
    /// </summary>
    public IEnumerable<EnvironmentalHazard> GetHazardsAt(Vector2 position)
    {
        var chunk = GetChunkAt(position);
        if (chunk == null)
            yield break;

        var biomeDef = BiomeData.GetDefinition(chunk.Biome);
        foreach (var hazard in biomeDef.Hazards)
        {
            yield return hazard;
        }
    }

    /// <summary>
    /// Checks if the protagonist can travel to a biome (based on connectivity).
    /// </summary>
    public bool CanTravelTo(BiomeType targetBiome)
    {
        return BiomeData.AreConnected(_currentBiome, targetBiome);
    }

    /// <summary>
    /// Gets the world bounds encompassing all chunks.
    /// </summary>
    public Rectangle GetWorldBounds()
    {
        if (_chunks.Count == 0)
            return Rectangle.Empty;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var chunk in _chunks.Values)
        {
            minX = Math.Min(minX, chunk.WorldPosition.X);
            minY = Math.Min(minY, chunk.WorldPosition.Y);
            maxX = Math.Max(maxX, chunk.WorldPosition.X + chunk.Size.X);
            maxY = Math.Max(maxY, chunk.WorldPosition.Y + chunk.Size.Y);
        }

        return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
    }

    /// <summary>
    /// Teleports to a specific biome (for fast travel or story events).
    /// </summary>
    public Vector2 GetBiomeSpawnPoint(BiomeType biome)
    {
        var chunk = _chunks.Values.FirstOrDefault(c => c.Biome == biome);
        if (chunk == null)
            return Vector2.Zero;

        // Return center of chunk as spawn point
        return chunk.WorldPosition + chunk.Size / 2;
    }

    /// <summary>
    /// Gets debug info about world state.
    /// </summary>
    public string GetDebugInfo()
    {
        var activeCount = _chunkTiers.Values.Count(t => t == ChunkSimulationTier.Active);
        var adjacentCount = _chunkTiers.Values.Count(t => t == ChunkSimulationTier.Adjacent);
        var loadedCount = _loadedChunkIds.Count;

        return $"Biome: {BiomeData.GetName(_currentBiome)} | " +
               $"Weather: {_currentWeather} | " +
               $"Chunks: {activeCount}A/{adjacentCount}Adj/{loadedCount}L";
    }
}
