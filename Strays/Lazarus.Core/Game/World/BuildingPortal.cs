using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lazarus.Core.Game.World;

/// <summary>
/// Visual style for building entrances.
/// </summary>
public enum BuildingEntranceStyle
{
    /// <summary>Standard door.</summary>
    Door,

    /// <summary>Open archway/entrance.</summary>
    Archway,

    /// <summary>Stairs leading down.</summary>
    StairsDown,

    /// <summary>Stairs leading up.</summary>
    StairsUp,

    /// <summary>Cave entrance.</summary>
    CaveEntrance,

    /// <summary>Hatch/trapdoor.</summary>
    Hatch,

    /// <summary>Industrial door/gate.</summary>
    IndustrialGate,

    /// <summary>Curtain/cloth entrance.</summary>
    Curtain
}

/// <summary>
/// A portal in the world that leads to an interior space.
/// Placed on the world map to represent building entrances, doors, etc.
/// </summary>
public class BuildingPortal
{
    private float _pulseTimer = 0f;
    private bool _playerOverlapping = false;

    /// <summary>
    /// Unique ID for this portal.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name shown to player (e.g., "Salvager's Shop").
    /// </summary>
    public string DisplayName { get; init; } = "";

    /// <summary>
    /// The interior this portal leads to.
    /// </summary>
    public string InteriorId { get; init; } = "";

    /// <summary>
    /// World position of the portal (top-left corner).
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Size of the portal trigger area.
    /// </summary>
    public Vector2 Size { get; set; } = new Vector2(48, 32);

    /// <summary>
    /// Visual style of the entrance.
    /// </summary>
    public BuildingEntranceStyle Style { get; init; } = BuildingEntranceStyle.Door;

    /// <summary>
    /// Where the player spawns when exiting from this building.
    /// </summary>
    public Vector2 ExitSpawnPosition { get; init; }

    /// <summary>
    /// Direction player faces when exiting.
    /// </summary>
    public Direction ExitFacingDirection { get; init; } = Direction.South;

    /// <summary>
    /// The biome this portal is in.
    /// </summary>
    public BiomeType Biome { get; init; } = BiomeType.Fringe;

    /// <summary>
    /// Story flag required to use this portal (null = always available).
    /// </summary>
    public string? RequiresFlag { get; init; }

    /// <summary>
    /// Whether this portal is currently active (usable).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the portal should be visible even when locked.
    /// </summary>
    public bool ShowWhenLocked { get; init; } = true;

    /// <summary>
    /// Message to show when portal is locked.
    /// </summary>
    public string LockedMessage { get; init; } = "This door is locked.";

    /// <summary>
    /// Placeholder color for debug rendering.
    /// </summary>
    public Color PlaceholderColor { get; init; } = Color.Brown;

    /// <summary>
    /// Whether the player is currently overlapping this portal.
    /// </summary>
    public bool IsPlayerOverlapping => _playerOverlapping;

    /// <summary>
    /// Gets the bounding rectangle of this portal.
    /// </summary>
    public Rectangle Bounds => new Rectangle(
        (int)Position.X,
        (int)Position.Y,
        (int)Size.X,
        (int)Size.Y
    );

    /// <summary>
    /// Updates the portal state.
    /// </summary>
    public void Update(GameTime gameTime, Rectangle playerBounds)
    {
        // Update pulse animation
        _pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_pulseTimer > MathF.PI * 2)
            _pulseTimer -= MathF.PI * 2;

        // Check player overlap
        _playerOverlapping = Bounds.Intersects(playerBounds);
    }

    /// <summary>
    /// Checks if the player can use this portal.
    /// </summary>
    public bool CanUse(Services.GameStateService gameState)
    {
        if (!IsActive)
            return false;

        if (!string.IsNullOrEmpty(RequiresFlag) && !gameState.HasFlag(RequiresFlag))
            return false;

        return true;
    }

    /// <summary>
    /// Gets the prompt text to show when player is near.
    /// </summary>
    public string GetPromptText(Services.GameStateService gameState)
    {
        if (!IsActive)
            return "";

        if (!string.IsNullOrEmpty(RequiresFlag) && !gameState.HasFlag(RequiresFlag))
            return LockedMessage;

        return $"Enter {DisplayName}";
    }

    /// <summary>
    /// Draws the portal.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 cameraOffset, SpriteFont? font = null)
    {
        var drawPos = Position - cameraOffset;
        var drawRect = new Rectangle(
            (int)drawPos.X,
            (int)drawPos.Y,
            (int)Size.X,
            (int)Size.Y
        );

        // Calculate pulse effect
        float pulse = 0.7f + MathF.Sin(_pulseTimer * 3f) * 0.15f;

        // Draw base portal
        Color baseColor = GetStyleColor() * pulse;
        spriteBatch.Draw(pixelTexture, drawRect, baseColor);

        // Draw border
        DrawBorder(spriteBatch, pixelTexture, drawRect, Color.White * 0.5f);

        // Draw entrance indicator based on style
        DrawStyleIndicator(spriteBatch, pixelTexture, drawRect);

        // Draw interaction prompt when player is near
        if (_playerOverlapping && font != null)
        {
            var promptPos = new Vector2(
                drawRect.Center.X,
                drawRect.Top - 20
            );

            string prompt = "[E] Enter";
            Vector2 textSize = font.MeasureString(prompt);
            promptPos.X -= textSize.X / 2;

            // Background
            var bgRect = new Rectangle(
                (int)promptPos.X - 4,
                (int)promptPos.Y - 2,
                (int)textSize.X + 8,
                (int)textSize.Y + 4
            );
            spriteBatch.Draw(pixelTexture, bgRect, Color.Black * 0.7f);

            // Text
            spriteBatch.DrawString(font, prompt, promptPos, Color.White);
        }
    }

    private Color GetStyleColor()
    {
        return Style switch
        {
            BuildingEntranceStyle.Door => new Color(139, 90, 43),         // Wood brown
            BuildingEntranceStyle.Archway => new Color(120, 120, 120),    // Stone gray
            BuildingEntranceStyle.StairsDown => new Color(80, 80, 100),   // Dark blue-gray
            BuildingEntranceStyle.StairsUp => new Color(100, 100, 80),    // Light gray
            BuildingEntranceStyle.CaveEntrance => new Color(60, 50, 40),  // Dark earth
            BuildingEntranceStyle.Hatch => new Color(100, 80, 60),        // Metal brown
            BuildingEntranceStyle.IndustrialGate => new Color(80, 80, 90),// Industrial gray
            BuildingEntranceStyle.Curtain => new Color(120, 60, 60),      // Red cloth
            _ => PlaceholderColor
        };
    }

    private void DrawStyleIndicator(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle rect)
    {
        Color indicatorColor = Color.Black * 0.4f;

        switch (Style)
        {
            case BuildingEntranceStyle.Door:
                // Draw door frame
                var handleRect = new Rectangle(
                    rect.Right - 8,
                    rect.Center.Y - 4,
                    4, 8
                );
                spriteBatch.Draw(pixelTexture, handleRect, Color.Gold * 0.8f);
                break;

            case BuildingEntranceStyle.StairsDown:
            case BuildingEntranceStyle.StairsUp:
                // Draw stair lines
                int stepCount = 3;
                int stepHeight = rect.Height / stepCount;
                for (int i = 0; i < stepCount; i++)
                {
                    var stepRect = new Rectangle(
                        rect.X + 4,
                        rect.Y + i * stepHeight,
                        rect.Width - 8,
                        2
                    );
                    spriteBatch.Draw(pixelTexture, stepRect, indicatorColor);
                }
                break;

            case BuildingEntranceStyle.CaveEntrance:
                // Draw irregular cave opening shape (simplified as darker center)
                var caveRect = new Rectangle(
                    rect.X + 8,
                    rect.Y + 4,
                    rect.Width - 16,
                    rect.Height - 8
                );
                spriteBatch.Draw(pixelTexture, caveRect, Color.Black * 0.5f);
                break;

            case BuildingEntranceStyle.Archway:
                // Draw arch top
                var archRect = new Rectangle(
                    rect.X + 4,
                    rect.Y,
                    rect.Width - 8,
                    8
                );
                spriteBatch.Draw(pixelTexture, archRect, indicatorColor);
                break;
        }
    }

    private void DrawBorder(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle rect, Color color)
    {
        int thickness = 2;

        // Top
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        // Left
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }
}

/// <summary>
/// Static registry of all building portals in the game.
/// </summary>
public static class BuildingPortals
{
    private static readonly Dictionary<string, BuildingPortal> _portals = new();
    private static bool _initialized = false;

    /// <summary>
    /// Gets a building portal by ID.
    /// </summary>
    public static BuildingPortal? Get(string id) =>
        _portals.TryGetValue(id, out var portal) ? portal : null;

    /// <summary>
    /// Gets all building portals.
    /// </summary>
    public static IEnumerable<BuildingPortal> All => _portals.Values;

    /// <summary>
    /// Gets all building portals in a specific biome.
    /// </summary>
    public static IEnumerable<BuildingPortal> GetByBiome(BiomeType biome) =>
        _portals.Values.Where(p => p.Biome == biome);

    /// <summary>
    /// Registers a building portal.
    /// </summary>
    public static void Register(BuildingPortal portal)
    {
        _portals[portal.Id] = portal;
    }

    /// <summary>
    /// Initializes all building portals.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        RegisterFringePortals();
        RegisterRustPortals();
    }

    private static void RegisterFringePortals()
    {
        // Salvager's Shop entrance
        Register(new BuildingPortal
        {
            Id = "fringe_shop_entrance",
            DisplayName = "Salvager's Trading Post",
            InteriorId = "salvagers_shop",
            Biome = BiomeType.Fringe,
            Position = new Vector2(400, 300),  // Position in world
            Size = new Vector2(48, 32),
            Style = BuildingEntranceStyle.Door,
            ExitSpawnPosition = new Vector2(400, 340),
            ExitFacingDirection = Direction.South,
            PlaceholderColor = Color.SandyBrown
        });

        // Rest House entrance
        Register(new BuildingPortal
        {
            Id = "fringe_resthouse_entrance",
            DisplayName = "Waypoint Rest House",
            InteriorId = "fringe_resthouse",
            Biome = BiomeType.Fringe,
            Position = new Vector2(600, 250),
            Size = new Vector2(48, 32),
            Style = BuildingEntranceStyle.Archway,
            ExitSpawnPosition = new Vector2(600, 290),
            ExitFacingDirection = Direction.South,
            PlaceholderColor = Color.Sienna
        });
    }

    private static void RegisterRustPortals()
    {
        // Workshop entrance
        Register(new BuildingPortal
        {
            Id = "rust_workshop_entrance",
            DisplayName = "Rust Haven Workshop",
            InteriorId = "rust_workshop",
            Biome = BiomeType.Rust,
            Position = new Vector2(350, 400),
            Size = new Vector2(56, 40),
            Style = BuildingEntranceStyle.IndustrialGate,
            ExitSpawnPosition = new Vector2(350, 450),
            ExitFacingDirection = Direction.South,
            PlaceholderColor = Color.OrangeRed
        });

        // Scrap Market entrance
        Register(new BuildingPortal
        {
            Id = "rust_market_entrance",
            DisplayName = "Scrap Market",
            InteriorId = "rust_market",
            Biome = BiomeType.Rust,
            Position = new Vector2(550, 350),
            Size = new Vector2(64, 40),
            Style = BuildingEntranceStyle.Archway,
            ExitSpawnPosition = new Vector2(550, 400),
            ExitFacingDirection = Direction.South,
            PlaceholderColor = Color.Peru
        });

        // Back alley exit for market (spawns at different location)
        Register(new BuildingPortal
        {
            Id = "rust_market_backalley",
            DisplayName = "Back Alley",
            InteriorId = "rust_market",
            Biome = BiomeType.Rust,
            Position = new Vector2(650, 320),
            Size = new Vector2(32, 48),
            Style = BuildingEntranceStyle.Archway,
            ExitSpawnPosition = new Vector2(660, 330),
            ExitFacingDirection = Direction.East,
            PlaceholderColor = Color.DarkGray
        });
    }
}
