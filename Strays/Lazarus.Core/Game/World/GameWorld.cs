using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Services;

namespace Lazarus.Core.Game.World;

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
    private readonly List<BiomePortal> _portals = new();
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
    /// All portals in the world.
    /// </summary>
    public IReadOnlyList<BiomePortal> Portals => _portals;

    /// <summary>
    /// Event fired when biome changes.
    /// </summary>
    public event Action<BiomeType, BiomeType>? BiomeChanged;

    /// <summary>
    /// Event fired when weather changes.
    /// </summary>
    public event Action<WeatherType>? WeatherChanged;

    /// <summary>
    /// Event fired when player enters a portal.
    /// </summary>
    public event Action<BiomePortal>? PortalEntered;

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

        // Create portals between connected biomes
        CreateBiomePortals();

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
    /// Creates portals at the edges of biome chunks for transitions.
    /// </summary>
    private void CreateBiomePortals()
    {
        // Get chunk positions
        var fringe = _chunks.GetValueOrDefault("fringe_main");
        var rust = _chunks.GetValueOrDefault("rust_main");
        var green = _chunks.GetValueOrDefault("green_main");
        var quiet = _chunks.GetValueOrDefault("quiet_main");
        var teeth = _chunks.GetValueOrDefault("teeth_main");
        var glow = _chunks.GetValueOrDefault("glow_main");
        var archive = _chunks.GetValueOrDefault("archive_main");

        if (fringe == null) return;

        // Fringe <-> Rust (West edge of Fringe, East edge of Rust)
        if (rust != null)
        {
            var fringeToRust = new BiomePortal
            {
                Id = "portal_fringe_rust",
                FromBiome = BiomeType.Fringe,
                ToBiome = BiomeType.Rust,
                Direction = PortalDirection.West,
                Position = new Vector2(fringe.WorldPosition.X + 50, fringe.WorldPosition.Y + fringe.Size.Y / 2),
                Size = new Vector2(80, 150),
                TargetChunkId = "rust_main",
                TargetSpawnOffset = new Vector2(rust.Size.X - 100, rust.Size.Y / 2),
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Rust).Min
            };
            _portals.Add(fringeToRust);

            var rustToFringe = new BiomePortal
            {
                Id = "portal_rust_fringe",
                FromBiome = BiomeType.Rust,
                ToBiome = BiomeType.Fringe,
                Direction = PortalDirection.East,
                Position = new Vector2(rust.WorldPosition.X + rust.Size.X - 50, rust.WorldPosition.Y + rust.Size.Y / 2),
                Size = new Vector2(80, 150),
                TargetChunkId = "fringe_main",
                TargetSpawnOffset = new Vector2(100, fringe.Size.Y / 2),
                RecommendedLevel = 1
            };
            _portals.Add(rustToFringe);
        }

        // Fringe <-> Green (East edge of Fringe, West edge of Green)
        if (green != null)
        {
            var fringeToGreen = new BiomePortal
            {
                Id = "portal_fringe_green",
                FromBiome = BiomeType.Fringe,
                ToBiome = BiomeType.Green,
                Direction = PortalDirection.East,
                Position = new Vector2(fringe.WorldPosition.X + fringe.Size.X - 50, fringe.WorldPosition.Y + fringe.Size.Y / 2),
                Size = new Vector2(80, 150),
                TargetChunkId = "green_main",
                TargetSpawnOffset = new Vector2(100, green.Size.Y / 2),
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Green).Min
            };
            _portals.Add(fringeToGreen);

            var greenToFringe = new BiomePortal
            {
                Id = "portal_green_fringe",
                FromBiome = BiomeType.Green,
                ToBiome = BiomeType.Fringe,
                Direction = PortalDirection.West,
                Position = new Vector2(green.WorldPosition.X + 50, green.WorldPosition.Y + green.Size.Y / 2),
                Size = new Vector2(80, 150),
                TargetChunkId = "fringe_main",
                TargetSpawnOffset = new Vector2(fringe.Size.X - 100, fringe.Size.Y / 2),
                RecommendedLevel = 1
            };
            _portals.Add(greenToFringe);
        }

        // Fringe <-> Quiet (South edge of Fringe, North edge of Quiet)
        if (quiet != null)
        {
            var fringeToQuiet = new BiomePortal
            {
                Id = "portal_fringe_quiet",
                FromBiome = BiomeType.Fringe,
                ToBiome = BiomeType.Quiet,
                Direction = PortalDirection.South,
                Position = new Vector2(fringe.WorldPosition.X + fringe.Size.X / 2, fringe.WorldPosition.Y + fringe.Size.Y - 50),
                Size = new Vector2(150, 80),
                TargetChunkId = "quiet_main",
                TargetSpawnOffset = new Vector2(quiet.Size.X / 2, 100),
                RequiresFlag = "reached_quiet", // Unlock after story progression
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Quiet).Min
            };
            _portals.Add(fringeToQuiet);

            var quietToFringe = new BiomePortal
            {
                Id = "portal_quiet_fringe",
                FromBiome = BiomeType.Quiet,
                ToBiome = BiomeType.Fringe,
                Direction = PortalDirection.North,
                Position = new Vector2(quiet.WorldPosition.X + quiet.Size.X / 2, quiet.WorldPosition.Y + 50),
                Size = new Vector2(150, 80),
                TargetChunkId = "fringe_main",
                TargetSpawnOffset = new Vector2(fringe.Size.X / 2, fringe.Size.Y - 100),
                RecommendedLevel = 1
            };
            _portals.Add(quietToFringe);
        }

        // Rust <-> Teeth (South edge of Rust, North edge of Teeth)
        if (rust != null && teeth != null)
        {
            var rustToTeeth = new BiomePortal
            {
                Id = "portal_rust_teeth",
                FromBiome = BiomeType.Rust,
                ToBiome = BiomeType.Teeth,
                Direction = PortalDirection.South,
                Position = new Vector2(rust.WorldPosition.X + rust.Size.X / 2, rust.WorldPosition.Y + rust.Size.Y - 50),
                Size = new Vector2(150, 80),
                TargetChunkId = "teeth_main",
                TargetSpawnOffset = new Vector2(teeth.Size.X / 2, 100),
                RequiresFlag = "reached_teeth",
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Teeth).Min
            };
            _portals.Add(rustToTeeth);

            var teethToRust = new BiomePortal
            {
                Id = "portal_teeth_rust",
                FromBiome = BiomeType.Teeth,
                ToBiome = BiomeType.Rust,
                Direction = PortalDirection.North,
                Position = new Vector2(teeth.WorldPosition.X + teeth.Size.X / 2, teeth.WorldPosition.Y + 50),
                Size = new Vector2(150, 80),
                TargetChunkId = "rust_main",
                TargetSpawnOffset = new Vector2(rust.Size.X / 2, rust.Size.Y - 100),
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Rust).Min
            };
            _portals.Add(teethToRust);
        }

        // Teeth <-> Glow (South edge of Teeth, North edge of Glow)
        if (teeth != null && glow != null)
        {
            var teethToGlow = new BiomePortal
            {
                Id = "portal_teeth_glow",
                FromBiome = BiomeType.Teeth,
                ToBiome = BiomeType.Glow,
                Direction = PortalDirection.South,
                Position = new Vector2(teeth.WorldPosition.X + teeth.Size.X / 2, teeth.WorldPosition.Y + teeth.Size.Y - 50),
                Size = new Vector2(150, 80),
                TargetChunkId = "glow_main",
                TargetSpawnOffset = new Vector2(glow.Size.X / 2, 100),
                RequiresFlag = "reached_glow",
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Glow).Min
            };
            _portals.Add(teethToGlow);

            var glowToTeeth = new BiomePortal
            {
                Id = "portal_glow_teeth",
                FromBiome = BiomeType.Glow,
                ToBiome = BiomeType.Teeth,
                Direction = PortalDirection.North,
                Position = new Vector2(glow.WorldPosition.X + glow.Size.X / 2, glow.WorldPosition.Y + 50),
                Size = new Vector2(150, 80),
                TargetChunkId = "teeth_main",
                TargetSpawnOffset = new Vector2(teeth.Size.X / 2, teeth.Size.Y - 100),
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Teeth).Min
            };
            _portals.Add(glowToTeeth);
        }

        // Green <-> Archive Scar (North edge of Green, South edge of Archive)
        if (green != null && archive != null)
        {
            var greenToArchive = new BiomePortal
            {
                Id = "portal_green_archive",
                FromBiome = BiomeType.Green,
                ToBiome = BiomeType.ArchiveScar,
                Direction = PortalDirection.North,
                Position = new Vector2(green.WorldPosition.X + green.Size.X / 2, green.WorldPosition.Y + 50),
                Size = new Vector2(150, 80),
                TargetChunkId = "archive_main",
                TargetSpawnOffset = new Vector2(archive.Size.X / 2, archive.Size.Y - 100),
                RequiresFlag = "found_archive_scar", // Secret area
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.ArchiveScar).Min
            };
            _portals.Add(greenToArchive);

            var archiveToGreen = new BiomePortal
            {
                Id = "portal_archive_green",
                FromBiome = BiomeType.ArchiveScar,
                ToBiome = BiomeType.Green,
                Direction = PortalDirection.South,
                Position = new Vector2(archive.WorldPosition.X + archive.Size.X / 2, archive.WorldPosition.Y + archive.Size.Y - 50),
                Size = new Vector2(150, 80),
                TargetChunkId = "green_main",
                TargetSpawnOffset = new Vector2(green.Size.X / 2, 100),
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Green).Min
            };
            _portals.Add(archiveToGreen);
        }

        // Quiet <-> Green (East edge of Quiet, additional connection)
        if (quiet != null && green != null)
        {
            var quietToGreen = new BiomePortal
            {
                Id = "portal_quiet_green",
                FromBiome = BiomeType.Quiet,
                ToBiome = BiomeType.Green,
                Direction = PortalDirection.East,
                Position = new Vector2(quiet.WorldPosition.X + quiet.Size.X - 50, quiet.WorldPosition.Y + quiet.Size.Y / 3),
                Size = new Vector2(80, 150),
                TargetChunkId = "green_main",
                TargetSpawnOffset = new Vector2(100, green.Size.Y * 2 / 3),
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Green).Min
            };
            _portals.Add(quietToGreen);

            var greenToQuiet = new BiomePortal
            {
                Id = "portal_green_quiet",
                FromBiome = BiomeType.Green,
                ToBiome = BiomeType.Quiet,
                Direction = PortalDirection.West,
                Position = new Vector2(green.WorldPosition.X + 50, green.WorldPosition.Y + green.Size.Y * 2 / 3),
                Size = new Vector2(80, 150),
                TargetChunkId = "quiet_main",
                TargetSpawnOffset = new Vector2(quiet.Size.X - 100, quiet.Size.Y / 3),
                RequiresFlag = "reached_quiet",
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Quiet).Min
            };
            _portals.Add(greenToQuiet);
        }

        // Quiet <-> Teeth (West edge of Quiet to connect the route)
        if (quiet != null && teeth != null)
        {
            var quietToTeeth = new BiomePortal
            {
                Id = "portal_quiet_teeth",
                FromBiome = BiomeType.Quiet,
                ToBiome = BiomeType.Teeth,
                Direction = PortalDirection.West,
                Position = new Vector2(quiet.WorldPosition.X + 50, quiet.WorldPosition.Y + quiet.Size.Y * 2 / 3),
                Size = new Vector2(80, 150),
                TargetChunkId = "teeth_main",
                TargetSpawnOffset = new Vector2(teeth.Size.X - 100, teeth.Size.Y / 3),
                RequiresFlag = "reached_teeth",
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Teeth).Min
            };
            _portals.Add(quietToTeeth);

            var teethToQuiet = new BiomePortal
            {
                Id = "portal_teeth_quiet",
                FromBiome = BiomeType.Teeth,
                ToBiome = BiomeType.Quiet,
                Direction = PortalDirection.East,
                Position = new Vector2(teeth.WorldPosition.X + teeth.Size.X - 50, teeth.WorldPosition.Y + teeth.Size.Y / 3),
                Size = new Vector2(80, 150),
                TargetChunkId = "quiet_main",
                TargetSpawnOffset = new Vector2(100, quiet.Size.Y * 2 / 3),
                RequiresFlag = "reached_quiet",
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Quiet).Min
            };
            _portals.Add(teethToQuiet);
        }

        // Rust <-> Quiet (additional diagonal connection)
        if (rust != null && quiet != null)
        {
            var rustToQuiet = new BiomePortal
            {
                Id = "portal_rust_quiet",
                FromBiome = BiomeType.Rust,
                ToBiome = BiomeType.Quiet,
                Direction = PortalDirection.East,
                Position = new Vector2(rust.WorldPosition.X + rust.Size.X - 50, rust.WorldPosition.Y + rust.Size.Y * 2 / 3),
                Size = new Vector2(80, 150),
                TargetChunkId = "quiet_main",
                TargetSpawnOffset = new Vector2(100, quiet.Size.Y / 3),
                RequiresFlag = "reached_quiet",
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Quiet).Min
            };
            _portals.Add(rustToQuiet);

            var quietToRust = new BiomePortal
            {
                Id = "portal_quiet_rust",
                FromBiome = BiomeType.Quiet,
                ToBiome = BiomeType.Rust,
                Direction = PortalDirection.West,
                Position = new Vector2(quiet.WorldPosition.X + 50, quiet.WorldPosition.Y + quiet.Size.Y / 3),
                Size = new Vector2(80, 150),
                TargetChunkId = "rust_main",
                TargetSpawnOffset = new Vector2(rust.Size.X - 100, rust.Size.Y * 2 / 3),
                RecommendedLevel = BiomeData.GetLevelRange(BiomeType.Rust).Min
            };
            _portals.Add(quietToRust);
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

    #region Portal Methods

    /// <summary>
    /// Gets the portal at a specific world position.
    /// </summary>
    public BiomePortal? GetPortalAt(Vector2 worldPosition)
    {
        foreach (var portal in _portals)
        {
            if (portal.Contains(worldPosition))
                return portal;
        }
        return null;
    }

    /// <summary>
    /// Gets all portals that collide with a bounding rectangle.
    /// </summary>
    public IEnumerable<BiomePortal> GetCollidingPortals(Rectangle bounds)
    {
        foreach (var portal in _portals)
        {
            if (portal.CheckCollision(bounds))
                yield return portal;
        }
    }

    /// <summary>
    /// Gets all portals in the current biome.
    /// </summary>
    public IEnumerable<BiomePortal> GetPortalsInCurrentBiome()
    {
        return _portals.Where(p => p.FromBiome == _currentBiome);
    }

    /// <summary>
    /// Gets all available (unlocked) portals from the current biome.
    /// </summary>
    public IEnumerable<BiomePortal> GetAvailablePortals()
    {
        return _portals.Where(p => p.FromBiome == _currentBiome && IsPortalUnlocked(p));
    }

    /// <summary>
    /// Checks if a portal is unlocked based on story flags.
    /// </summary>
    public bool IsPortalUnlocked(BiomePortal portal)
    {
        if (string.IsNullOrEmpty(portal.RequiresFlag))
            return true;

        return _gameState.HasFlag(portal.RequiresFlag);
    }

    /// <summary>
    /// Updates portal states (player in range detection).
    /// </summary>
    public void UpdatePortals(GameTime gameTime, Rectangle playerBounds)
    {
        foreach (var portal in _portals)
        {
            portal.Update(gameTime);
            portal.IsPlayerInRange = portal.IsActive &&
                                     IsPortalUnlocked(portal) &&
                                     portal.CheckCollision(playerBounds);
        }
    }

    /// <summary>
    /// Attempts to use a portal at the player's position.
    /// </summary>
    /// <param name="playerBounds">Player's collision bounds.</param>
    /// <returns>The target position if successful, null if no valid portal.</returns>
    public Vector2? TryUsePortal(Rectangle playerBounds)
    {
        var portal = GetCollidingPortals(playerBounds)
            .FirstOrDefault(p => IsPortalUnlocked(p) && p.IsActive);

        if (portal == null)
            return null;

        return UsePortal(portal);
    }

    /// <summary>
    /// Uses a specific portal and returns the target spawn position.
    /// </summary>
    /// <param name="portal">The portal to use.</param>
    /// <returns>The spawn position in the target biome.</returns>
    public Vector2 UsePortal(BiomePortal portal)
    {
        // Find target chunk
        var targetChunk = _chunks.GetValueOrDefault(portal.TargetChunkId);
        if (targetChunk == null)
        {
            // Fallback to biome spawn point
            return GetBiomeSpawnPoint(portal.ToBiome);
        }

        // Ensure target chunk is loaded
        if (!targetChunk.IsLoaded)
        {
            targetChunk.Load(_graphicsDevice);
            _loadedChunkIds.Add(targetChunk.Id);
            _chunkTiers[targetChunk.Id] = ChunkSimulationTier.Active;
        }

        // Calculate spawn position
        var spawnPosition = targetChunk.WorldPosition + portal.TargetSpawnOffset;

        // Fire portal entered event
        PortalEntered?.Invoke(portal);

        // Update current chunk and biome
        var oldBiome = _currentBiome;
        _currentChunk = targetChunk;
        _currentBiome = targetChunk.Biome;

        // Immediately update camera to new position
        SetCameraPosition(spawnPosition);

        if (oldBiome != _currentBiome)
        {
            BiomeChanged?.Invoke(oldBiome, _currentBiome);
            UpdateWeatherForBiome(_currentBiome);
        }

        return spawnPosition;
    }

    /// <summary>
    /// Sets the camera position to center on a specific world position.
    /// </summary>
    public void SetCameraPosition(Vector2 worldPosition)
    {
        _cameraPosition = worldPosition - _viewportSize / 2;

        // Clamp to current chunk bounds if we have one
        if (_currentChunk != null)
        {
            var chunkBounds = _currentChunk.Bounds;
            _cameraPosition.X = Math.Max(chunkBounds.X, Math.Min(_cameraPosition.X, chunkBounds.Right - _viewportSize.X));
            _cameraPosition.Y = Math.Max(chunkBounds.Y, Math.Min(_cameraPosition.Y, chunkBounds.Bottom - _viewportSize.Y));
        }
    }

    /// <summary>
    /// Teleports to a position, updating camera and current chunk.
    /// </summary>
    public void TeleportTo(Vector2 worldPosition)
    {
        // Find and set the chunk at the position
        var chunk = GetChunkAt(worldPosition);
        if (chunk != null)
        {
            // Ensure chunk is loaded
            if (!chunk.IsLoaded)
            {
                chunk.Load(_graphicsDevice);
                _loadedChunkIds.Add(chunk.Id);
            }
            _chunkTiers[chunk.Id] = ChunkSimulationTier.Active;

            var oldBiome = _currentBiome;
            _currentChunk = chunk;
            _currentBiome = chunk.Biome;

            if (oldBiome != _currentBiome)
            {
                BiomeChanged?.Invoke(oldBiome, _currentBiome);
                UpdateWeatherForBiome(_currentBiome);
            }
        }

        // Update camera
        SetCameraPosition(worldPosition);
    }

    /// <summary>
    /// Gets the portal leading to a specific biome from the current biome.
    /// </summary>
    public BiomePortal? GetPortalTo(BiomeType targetBiome)
    {
        return _portals.FirstOrDefault(p =>
            p.FromBiome == _currentBiome &&
            p.ToBiome == targetBiome &&
            IsPortalUnlocked(p));
    }

    /// <summary>
    /// Draws all visible portals.
    /// </summary>
    public void DrawPortals(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font)
    {
        foreach (var portal in _portals)
        {
            // Only draw portals in currently loaded chunks
            var chunk = _chunks.GetValueOrDefault(GetChunkIdForPortal(portal));
            if (chunk == null || !chunk.IsLoaded)
                continue;

            // Only draw active/unlocked portals
            if (!portal.IsActive)
                continue;

            // Dim locked portals
            if (!IsPortalUnlocked(portal))
            {
                // Draw locked portal indicator
                DrawLockedPortal(spriteBatch, pixelTexture, font, portal);
            }
            else
            {
                portal.Draw(spriteBatch, pixelTexture, font, _cameraPosition);
            }
        }
    }

    /// <summary>
    /// Draws a locked portal indicator.
    /// </summary>
    private void DrawLockedPortal(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, BiomePortal portal)
    {
        var screenPos = portal.Position - _cameraPosition;
        var bounds = new Rectangle(
            (int)(screenPos.X - portal.Size.X / 2),
            (int)(screenPos.Y - portal.Size.Y / 2),
            (int)portal.Size.X,
            (int)portal.Size.Y
        );

        // Draw dimmed portal
        var lockedColor = Color.DarkGray * 0.5f;
        spriteBatch.Draw(pixelTexture, bounds, lockedColor);

        // Draw lock icon (X pattern)
        var borderColor = Color.Red * 0.7f;
        int borderWidth = 2;

        // Border
        spriteBatch.Draw(pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderWidth), borderColor);
        spriteBatch.Draw(pixelTexture, new Rectangle(bounds.X, bounds.Bottom - borderWidth, bounds.Width, borderWidth), borderColor);
        spriteBatch.Draw(pixelTexture, new Rectangle(bounds.X, bounds.Y, borderWidth, bounds.Height), borderColor);
        spriteBatch.Draw(pixelTexture, new Rectangle(bounds.Right - borderWidth, bounds.Y, borderWidth, bounds.Height), borderColor);

        // X pattern
        for (int i = 0; i < Math.Min(bounds.Width, bounds.Height) / 2; i++)
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(bounds.Center.X - i, bounds.Center.Y - i, 2, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(bounds.Center.X + i, bounds.Center.Y - i, 2, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(bounds.Center.X - i, bounds.Center.Y + i, 2, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(bounds.Center.X + i, bounds.Center.Y + i, 2, 2), borderColor);
        }

        // Draw locked label
        if (font != null)
        {
            var label = "LOCKED";
            var labelSize = font.MeasureString(label);
            var labelPos = new Vector2(screenPos.X - labelSize.X / 2, bounds.Y - labelSize.Y - 5);
            spriteBatch.DrawString(font, label, labelPos, Color.Red);

            // Show required flag hint
            if (!string.IsNullOrEmpty(portal.RequiresFlag))
            {
                var hint = $"Requires: {portal.RequiresFlag}";
                var hintSize = font.MeasureString(hint);
                var hintPos = new Vector2(screenPos.X - hintSize.X / 2, bounds.Bottom + 5);
                spriteBatch.DrawString(font, hint, hintPos, Color.Gray);
            }
        }
    }

    /// <summary>
    /// Gets the chunk ID that contains a portal (based on its FromBiome).
    /// </summary>
    private string GetChunkIdForPortal(BiomePortal portal)
    {
        // Map biome to main chunk ID
        return portal.FromBiome switch
        {
            BiomeType.Fringe => "fringe_main",
            BiomeType.Rust => "rust_main",
            BiomeType.Green => "green_main",
            BiomeType.Quiet => "quiet_main",
            BiomeType.Teeth => "teeth_main",
            BiomeType.Glow => "glow_main",
            BiomeType.ArchiveScar => "archive_main",
            _ => "fringe_main"
        };
    }

    /// <summary>
    /// Unlocks a portal by setting its required story flag.
    /// </summary>
    public void UnlockPortal(string portalId)
    {
        var portal = _portals.FirstOrDefault(p => p.Id == portalId);
        if (portal != null && !string.IsNullOrEmpty(portal.RequiresFlag))
        {
            _gameState.SetFlag(portal.RequiresFlag);
        }
    }

    /// <summary>
    /// Gets all biomes that are currently accessible from the current biome.
    /// </summary>
    public IEnumerable<BiomeType> GetAccessibleBiomes()
    {
        return GetAvailablePortals().Select(p => p.ToBiome).Distinct();
    }

    /// <summary>
    /// Gets the complete travel path between two biomes.
    /// </summary>
    public List<BiomeType>? GetTravelPath(BiomeType from, BiomeType to)
    {
        if (from == to)
            return new List<BiomeType> { from };

        // Simple BFS pathfinding
        var visited = new HashSet<BiomeType>();
        var queue = new Queue<(BiomeType biome, List<BiomeType> path)>();
        queue.Enqueue((from, new List<BiomeType> { from }));

        while (queue.Count > 0)
        {
            var (current, path) = queue.Dequeue();

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            // Get accessible biomes from current
            var accessibleFromCurrent = _portals
                .Where(p => p.FromBiome == current && IsPortalUnlocked(p))
                .Select(p => p.ToBiome)
                .Distinct();

            foreach (var next in accessibleFromCurrent)
            {
                if (next == to)
                {
                    var finalPath = new List<BiomeType>(path) { next };
                    return finalPath;
                }

                if (!visited.Contains(next))
                {
                    var newPath = new List<BiomeType>(path) { next };
                    queue.Enqueue((next, newPath));
                }
            }
        }

        return null; // No path found
    }

    #endregion
}
