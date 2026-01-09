using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Game.Data;
using Strays.Core.Game.Entities;
using Strays.Core.Services;

namespace Strays.Core.Game.World;

/// <summary>
/// Types of interior spaces.
/// </summary>
public enum InteriorType
{
    /// <summary>Generic building interior.</summary>
    Generic,

    /// <summary>Shop/trading post.</summary>
    Shop,

    /// <summary>Residential house.</summary>
    House,

    /// <summary>Tavern/inn with rest services.</summary>
    Tavern,

    /// <summary>Workshop/crafting facility.</summary>
    Workshop,

    /// <summary>Medical/healing facility.</summary>
    Clinic,

    /// <summary>Quest hub/mission board.</summary>
    QuestHub,

    /// <summary>Storage facility.</summary>
    Warehouse,

    /// <summary>Natural cave.</summary>
    Cave,

    /// <summary>Ruins/abandoned structure.</summary>
    Ruins,

    /// <summary>Underground bunker.</summary>
    Bunker,

    /// <summary>Industrial facility.</summary>
    Industrial
}

/// <summary>
/// Defines an exit point within an interior.
/// </summary>
public class InteriorExit
{
    /// <summary>
    /// Unique ID for this exit.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Position of the exit trigger within the interior.
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// Size of the exit trigger area.
    /// </summary>
    public Vector2 Size { get; init; } = new Vector2(32, 32);

    /// <summary>
    /// Where the player spawns in the world after using this exit.
    /// If null, uses the building portal's world position.
    /// </summary>
    public Vector2? WorldSpawnPosition { get; init; }

    /// <summary>
    /// Display name for the exit (e.g., "Front Door", "Back Alley").
    /// </summary>
    public string Name { get; init; } = "Exit";

    /// <summary>
    /// Whether this exit leads to a different interior (for connected buildings).
    /// </summary>
    public string? LeadsToInteriorId { get; init; }

    /// <summary>
    /// Gets the bounding rectangle of this exit.
    /// </summary>
    public Rectangle Bounds => new Rectangle(
        (int)Position.X,
        (int)Position.Y,
        (int)Size.X,
        (int)Size.Y
    );
}

/// <summary>
/// Defines an NPC spawn point within an interior.
/// </summary>
public class InteriorNpcSpawn
{
    /// <summary>
    /// The NPC definition ID to spawn.
    /// </summary>
    public string NpcId { get; init; } = "";

    /// <summary>
    /// Position within the interior.
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// Direction the NPC faces.
    /// </summary>
    public Direction FacingDirection { get; init; } = Direction.South;

    /// <summary>
    /// Whether the NPC can move around or is stationary.
    /// </summary>
    public bool IsStationary { get; init; } = true;

    /// <summary>
    /// Optional patrol path for non-stationary NPCs.
    /// </summary>
    public List<Vector2>? PatrolPath { get; init; }
}

/// <summary>
/// Definition of an interior space (shop, house, dungeon room, etc.).
/// </summary>
public class InteriorDefinition
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Type of interior.
    /// </summary>
    public InteriorType Type { get; init; } = InteriorType.Generic;

    /// <summary>
    /// The biome this interior is associated with.
    /// </summary>
    public BiomeType Biome { get; init; } = BiomeType.Fringe;

    /// <summary>
    /// Path to the Tiled map file (.tmx) for this interior.
    /// Relative to Content folder.
    /// </summary>
    public string TiledMapPath { get; init; } = "";

    /// <summary>
    /// Where the player spawns when entering the interior.
    /// This is relative to the interior map origin.
    /// </summary>
    public Vector2 EntrySpawnPosition { get; init; } = new Vector2(100, 150);

    /// <summary>
    /// Direction player faces when spawning.
    /// </summary>
    public Direction EntryFacingDirection { get; init; } = Direction.North;

    /// <summary>
    /// Size of the interior in pixels.
    /// </summary>
    public Vector2 Size { get; init; } = new Vector2(320, 240);

    /// <summary>
    /// Exit points in this interior.
    /// </summary>
    public List<InteriorExit> Exits { get; init; } = new();

    /// <summary>
    /// NPCs that spawn in this interior.
    /// </summary>
    public List<InteriorNpcSpawn> NpcSpawns { get; init; } = new();

    /// <summary>
    /// Story flag required to access this interior.
    /// </summary>
    public string? RequiresFlag { get; init; }

    /// <summary>
    /// Whether encounters can occur in this interior.
    /// </summary>
    public bool IsSafeZone { get; init; } = true;

    /// <summary>
    /// Ambient lighting color tint.
    /// </summary>
    public Color AmbientColor { get; init; } = Color.White;

    /// <summary>
    /// Placeholder color for debug rendering.
    /// </summary>
    public Color PlaceholderColor { get; init; } = Color.SaddleBrown;

    /// <summary>
    /// Whether this interior spans multiple floors/levels.
    /// </summary>
    public bool IsMultiLevel { get; init; } = false;

    /// <summary>
    /// For multi-level interiors, connections to other interior IDs.
    /// Key = trigger ID, Value = target interior ID.
    /// </summary>
    public Dictionary<string, string> LevelConnections { get; init; } = new();

    /// <summary>
    /// Music track to play in this interior (null = continue world music).
    /// </summary>
    public string? MusicTrack { get; init; }

    /// <summary>
    /// Ambient sound loop to play (null = none).
    /// </summary>
    public string? AmbientSound { get; init; }
}

/// <summary>
/// Runtime instance of an interior space.
/// </summary>
public class InteriorInstance
{
    private readonly InteriorDefinition _definition;
    private readonly GameStateService _gameState;
    private readonly List<NPC> _npcs = new();
    private TiledMap? _map;
    private bool _isLoaded = false;

    /// <summary>
    /// The interior definition.
    /// </summary>
    public InteriorDefinition Definition => _definition;

    /// <summary>
    /// The loaded Tiled map for this interior.
    /// </summary>
    public TiledMap? Map => _map;

    /// <summary>
    /// Whether the interior has been loaded.
    /// </summary>
    public bool IsLoaded => _isLoaded;

    /// <summary>
    /// Active NPCs in this interior.
    /// </summary>
    public IReadOnlyList<NPC> NPCs => _npcs;

    /// <summary>
    /// Current player position within the interior.
    /// </summary>
    public Vector2 PlayerPosition { get; set; }

    /// <summary>
    /// Direction player is facing.
    /// </summary>
    public Direction PlayerFacing { get; set; }

    /// <summary>
    /// Whether the player has visited this interior before.
    /// </summary>
    public bool HasBeenVisited { get; private set; }

    /// <summary>
    /// The building portal that was used to enter this interior.
    /// Used to determine exit destination.
    /// </summary>
    public BuildingPortal? EntryPortal { get; set; }

    /// <summary>
    /// Event raised when player triggers an exit.
    /// </summary>
    public event Action<InteriorExit>? ExitTriggered;

    /// <summary>
    /// Event raised when player interacts with an NPC.
    /// </summary>
    public event Action<NPC>? NpcInteraction;

    public InteriorInstance(InteriorDefinition definition, GameStateService gameState)
    {
        _definition = definition;
        _gameState = gameState;
        PlayerPosition = definition.EntrySpawnPosition;
        PlayerFacing = definition.EntryFacingDirection;
    }

    /// <summary>
    /// Loads the interior map and spawns NPCs.
    /// </summary>
    public void Load(GraphicsDevice graphicsDevice)
    {
        if (_isLoaded) return;

        // Load the Tiled map if path is specified
        if (!string.IsNullOrEmpty(_definition.TiledMapPath))
        {
            try
            {
                _map = new TiledMap(graphicsDevice);
                _map.Load(_definition.TiledMapPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load interior map: {ex.Message}");
                // Continue without map - will use placeholder rendering
            }
        }

        // Spawn NPCs
        SpawnNpcs();

        _isLoaded = true;
    }

    /// <summary>
    /// Unloads the interior resources.
    /// </summary>
    public void Unload()
    {
        _map = null;
        _npcs.Clear();
        _isLoaded = false;
    }

    /// <summary>
    /// Called when player enters this interior.
    /// </summary>
    public void OnEnter()
    {
        HasBeenVisited = true;
        PlayerPosition = _definition.EntrySpawnPosition;
        PlayerFacing = _definition.EntryFacingDirection;

        // Set story flag for first visit if needed
        if (!string.IsNullOrEmpty(_definition.Id))
        {
            _gameState.SetFlag($"visited_{_definition.Id}");
        }
    }

    /// <summary>
    /// Updates the interior state.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // Update NPCs
        foreach (var npc in _npcs)
        {
            npc.Update(gameTime);
        }
    }

    /// <summary>
    /// Checks if the player is colliding with any exit.
    /// </summary>
    public InteriorExit? GetCollidingExit(Rectangle playerBounds)
    {
        foreach (var exit in _definition.Exits)
        {
            if (exit.Bounds.Intersects(playerBounds))
            {
                return exit;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the NPC the player can interact with (if any).
    /// </summary>
    public NPC? GetInteractableNpc(Vector2 playerPosition, Direction facingDirection, float interactDistance = 40f)
    {
        // Calculate interaction point in front of player
        Vector2 dirVector = GetDirectionVector(facingDirection);
        Vector2 interactPoint = playerPosition + dirVector * interactDistance;

        foreach (var npc in _npcs)
        {
            float distance = Vector2.Distance(interactPoint, npc.Position);
            if (distance < 30f) // NPC interaction radius
            {
                return npc;
            }
        }

        return null;
    }

    /// <summary>
    /// Triggers an exit event.
    /// </summary>
    public void TriggerExit(InteriorExit exit)
    {
        ExitTriggered?.Invoke(exit);
    }

    /// <summary>
    /// Draws the interior.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, Vector2 cameraOffset, Vector2 viewportSize)
    {
        // Draw the Tiled map if loaded
        if (_map != null)
        {
            _map.Draw(spriteBatch, cameraOffset, viewportSize);
        }
        else
        {
            // Placeholder rendering
            DrawPlaceholder(spriteBatch, pixelTexture, cameraOffset);
        }

        // Draw NPCs
        foreach (var npc in _npcs)
        {
            npc.Draw(spriteBatch, pixelTexture, font, cameraOffset);
        }

        // Draw exit indicators (debug)
        #if DEBUG
        DrawExitIndicators(spriteBatch, pixelTexture, cameraOffset);
        #endif
    }

    private void DrawPlaceholder(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 cameraOffset)
    {
        // Simple placeholder floor
        var floorRect = new Rectangle(
            (int)-cameraOffset.X,
            (int)-cameraOffset.Y,
            (int)_definition.Size.X,
            (int)_definition.Size.Y
        );

        spriteBatch.Draw(pixelTexture, floorRect, _definition.PlaceholderColor * 0.3f);
    }

    private void DrawExitIndicators(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 cameraOffset)
    {
        // Debug rendering for exits
        foreach (var exit in _definition.Exits)
        {
            var exitRect = new Rectangle(
                (int)(exit.Position.X - cameraOffset.X),
                (int)(exit.Position.Y - cameraOffset.Y),
                (int)exit.Size.X,
                (int)exit.Size.Y
            );
            spriteBatch.Draw(pixelTexture, exitRect, Color.Green * 0.3f);
        }
    }

    private void SpawnNpcs()
    {
        _npcs.Clear();

        foreach (var spawn in _definition.NpcSpawns)
        {
            var npcDef = NPCDefinitions.Get(spawn.NpcId);
            if (npcDef != null)
            {
                var npc = new NPC(npcDef, _gameState)
                {
                    Position = spawn.Position
                };
                _npcs.Add(npc);
            }
        }
    }

    /// <summary>
    /// Checks if this interior is accessible based on story flags.
    /// </summary>
    public bool IsAccessible()
    {
        if (string.IsNullOrEmpty(_definition.RequiresFlag))
            return true;

        return _gameState.HasFlag(_definition.RequiresFlag);
    }

    /// <summary>
    /// Gets the world spawn position for a given exit.
    /// </summary>
    public Vector2 GetWorldExitPosition(InteriorExit exit)
    {
        // If exit has explicit world spawn, use it
        if (exit.WorldSpawnPosition.HasValue)
            return exit.WorldSpawnPosition.Value;

        // Otherwise use the entry portal's position
        if (EntryPortal != null)
            return EntryPortal.ExitSpawnPosition;

        // Fallback
        return Vector2.Zero;
    }

    /// <summary>
    /// Gets a unit vector for a direction.
    /// </summary>
    private static Vector2 GetDirectionVector(Direction direction)
    {
        return direction switch
        {
            Direction.North => new Vector2(0, -1),
            Direction.NorthEast => Vector2.Normalize(new Vector2(1, -1)),
            Direction.East => new Vector2(1, 0),
            Direction.SouthEast => Vector2.Normalize(new Vector2(1, 1)),
            Direction.South => new Vector2(0, 1),
            Direction.SouthWest => Vector2.Normalize(new Vector2(-1, 1)),
            Direction.West => new Vector2(-1, 0),
            Direction.NorthWest => Vector2.Normalize(new Vector2(-1, -1)),
            _ => Vector2.Zero
        };
    }
}

/// <summary>
/// Static registry of all interior definitions.
/// </summary>
public static class InteriorDefinitions
{
    private static readonly Dictionary<string, InteriorDefinition> _interiors = new();
    private static bool _initialized = false;

    /// <summary>
    /// Gets an interior definition by ID.
    /// </summary>
    public static InteriorDefinition? Get(string id) =>
        _interiors.TryGetValue(id, out var interior) ? interior : null;

    /// <summary>
    /// Gets all interior definitions.
    /// </summary>
    public static IEnumerable<InteriorDefinition> All => _interiors.Values;

    /// <summary>
    /// Gets all interiors of a specific type.
    /// </summary>
    public static IEnumerable<InteriorDefinition> GetByType(InteriorType type) =>
        _interiors.Values.Where(i => i.Type == type);

    /// <summary>
    /// Gets all interiors in a specific biome.
    /// </summary>
    public static IEnumerable<InteriorDefinition> GetByBiome(BiomeType biome) =>
        _interiors.Values.Where(i => i.Biome == biome);

    /// <summary>
    /// Registers an interior definition.
    /// </summary>
    public static void Register(InteriorDefinition interior)
    {
        _interiors[interior.Id] = interior;
    }

    /// <summary>
    /// Initializes all interior definitions.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        RegisterFringeInteriors();
        RegisterRustInteriors();
    }

    private static void RegisterFringeInteriors()
    {
        // Salvager's Shop - The Fringe
        Register(new InteriorDefinition
        {
            Id = "salvagers_shop",
            Name = "Salvager's Trading Post",
            Description = "A cluttered shop filled with salvaged tech and oddities.",
            Type = InteriorType.Shop,
            Biome = BiomeType.Fringe,
            TiledMapPath = "Tiled/interiors/fringe_shop.tmx",
            Size = new Vector2(256, 192),
            EntrySpawnPosition = new Vector2(128, 160),
            EntryFacingDirection = Direction.North,
            AmbientColor = new Color(200, 180, 150), // Warm, dusty lighting
            PlaceholderColor = Color.SandyBrown,
            IsSafeZone = true,
            Exits = new List<InteriorExit>
            {
                new InteriorExit
                {
                    Id = "main_door",
                    Name = "Exit",
                    Position = new Vector2(112, 176),
                    Size = new Vector2(32, 16)
                }
            },
            NpcSpawns = new List<InteriorNpcSpawn>
            {
                new InteriorNpcSpawn
                {
                    NpcId = "trader_rust",  // Rust the salvager trader
                    Position = new Vector2(128, 48),
                    FacingDirection = Direction.South,
                    IsStationary = true
                }
            }
        });

        // Rest House - The Fringe
        Register(new InteriorDefinition
        {
            Id = "fringe_resthouse",
            Name = "Waypoint Rest House",
            Description = "A simple shelter for weary travelers.",
            Type = InteriorType.Tavern,
            Biome = BiomeType.Fringe,
            TiledMapPath = "Tiled/interiors/fringe_resthouse.tmx",
            Size = new Vector2(224, 160),
            EntrySpawnPosition = new Vector2(112, 140),
            EntryFacingDirection = Direction.North,
            AmbientColor = new Color(220, 200, 180),
            PlaceholderColor = Color.Sienna,
            IsSafeZone = true,
            Exits = new List<InteriorExit>
            {
                new InteriorExit
                {
                    Id = "main_door",
                    Name = "Exit",
                    Position = new Vector2(96, 144),
                    Size = new Vector2(32, 16)
                }
            },
            NpcSpawns = new List<InteriorNpcSpawn>
            {
                new InteriorNpcSpawn
                {
                    NpcId = "healer_patch",  // Patch the healer
                    Position = new Vector2(64, 48),
                    FacingDirection = Direction.East,
                    IsStationary = true
                }
            }
        });
    }

    private static void RegisterRustInteriors()
    {
        // Rust Haven Workshop
        Register(new InteriorDefinition
        {
            Id = "rust_workshop",
            Name = "Rust Haven Workshop",
            Description = "A smoky workshop where augmentations are crafted and repaired.",
            Type = InteriorType.Workshop,
            Biome = BiomeType.Rust,
            TiledMapPath = "Tiled/interiors/rust_workshop.tmx",
            Size = new Vector2(288, 224),
            EntrySpawnPosition = new Vector2(144, 200),
            EntryFacingDirection = Direction.North,
            AmbientColor = new Color(255, 180, 120), // Orange industrial lighting
            PlaceholderColor = Color.OrangeRed,
            IsSafeZone = true,
            Exits = new List<InteriorExit>
            {
                new InteriorExit
                {
                    Id = "main_door",
                    Name = "Exit",
                    Position = new Vector2(128, 208),
                    Size = new Vector2(32, 16)
                }
            },
            NpcSpawns = new List<InteriorNpcSpawn>
            {
                new InteriorNpcSpawn
                {
                    NpcId = "machinist_volt",  // Volt the machinist trader
                    Position = new Vector2(200, 80),
                    FacingDirection = Direction.West,
                    IsStationary = true
                }
            }
        });

        // Rust Market Stall (smaller interior)
        Register(new InteriorDefinition
        {
            Id = "rust_market",
            Name = "Scrap Market",
            Description = "A busy trading hub for spare parts and supplies.",
            Type = InteriorType.Shop,
            Biome = BiomeType.Rust,
            TiledMapPath = "Tiled/interiors/rust_market.tmx",
            Size = new Vector2(320, 256),
            EntrySpawnPosition = new Vector2(160, 230),
            EntryFacingDirection = Direction.North,
            AmbientColor = new Color(230, 200, 170),
            PlaceholderColor = Color.Peru,
            IsSafeZone = true,
            Exits = new List<InteriorExit>
            {
                new InteriorExit
                {
                    Id = "main_entrance",
                    Name = "Main Exit",
                    Position = new Vector2(144, 240),
                    Size = new Vector2(32, 16)
                },
                new InteriorExit
                {
                    Id = "back_door",
                    Name = "Back Alley",
                    Position = new Vector2(304, 128),
                    Size = new Vector2(16, 32)
                }
            },
            NpcSpawns = new List<InteriorNpcSpawn>
            {
                new InteriorNpcSpawn
                {
                    NpcId = "market_parts_vendor",  // Parts vendor
                    Position = new Vector2(80, 64),
                    FacingDirection = Direction.South,
                    IsStationary = true
                },
                new InteriorNpcSpawn
                {
                    NpcId = "market_chips_vendor",  // Chips vendor
                    Position = new Vector2(240, 64),
                    FacingDirection = Direction.South,
                    IsStationary = true
                }
            }
        });
    }
}
