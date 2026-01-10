using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Services;

namespace Lazarus.Core.Game.World;

/// <summary>
/// Services available at settlements.
/// </summary>
[Flags]
public enum SettlementServices
{
    None = 0,
    Healing = 1,
    Trading = 2,
    Crafting = 4,
    Storage = 8,
    Quests = 16,
    Rest = 32,
    All = Healing | Trading | Crafting | Storage | Quests | Rest
}

/// <summary>
/// Definition for a settlement.
/// </summary>
public class SettlementDefinition
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
    /// The biome this settlement is in.
    /// </summary>
    public BiomeType Biome { get; init; } = BiomeType.Fringe;

    /// <summary>
    /// Dominant faction.
    /// </summary>
    public Faction Faction { get; init; } = Faction.None;

    /// <summary>
    /// Available services.
    /// </summary>
    public SettlementServices Services { get; init; } = SettlementServices.None;

    /// <summary>
    /// IDs of NPCs that live here.
    /// </summary>
    public List<string> NpcIds { get; init; } = new();

    /// <summary>
    /// Size of the settlement area.
    /// </summary>
    public Vector2 Size { get; init; } = new Vector2(200, 200);

    /// <summary>
    /// Color for placeholder rendering.
    /// </summary>
    public Color PlaceholderColor { get; init; } = Color.Gray;

    /// <summary>
    /// Flag required for this settlement to be accessible.
    /// </summary>
    public string? RequiresFlag { get; init; }

    /// <summary>
    /// Whether this is a safe zone (no encounters).
    /// </summary>
    public bool IsSafeZone { get; init; } = true;
}

/// <summary>
/// A settlement in the game world.
/// </summary>
public class Settlement
{
    private readonly SettlementDefinition _definition;
    private readonly GameStateService _gameState;
    private readonly List<NPC> _npcs = new();

    /// <summary>
    /// The settlement definition.
    /// </summary>
    public SettlementDefinition Definition => _definition;

    /// <summary>
    /// Unique ID.
    /// </summary>
    public string Id => _definition.Id;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name => _definition.Name;

    /// <summary>
    /// World position (center).
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Size of the settlement area.
    /// </summary>
    public Vector2 Size => _definition.Size;

    /// <summary>
    /// Bounding rectangle of the settlement.
    /// </summary>
    public Rectangle Bounds => new Rectangle(
        (int)(Position.X - Size.X / 2),
        (int)(Position.Y - Size.Y / 2),
        (int)Size.X,
        (int)Size.Y
    );

    /// <summary>
    /// NPCs in this settlement.
    /// </summary>
    public IReadOnlyList<NPC> NPCs => _npcs;

    /// <summary>
    /// Available services.
    /// </summary>
    public SettlementServices Services => _definition.Services;

    /// <summary>
    /// Whether this settlement is currently accessible.
    /// </summary>
    public bool IsAccessible
    {
        get
        {
            if (string.IsNullOrEmpty(_definition.RequiresFlag))
                return true;

            return _gameState.HasFlag(_definition.RequiresFlag);
        }
    }

    /// <summary>
    /// Whether this is a safe zone.
    /// </summary>
    public bool IsSafeZone => _definition.IsSafeZone;

    /// <summary>
    /// Whether the player is currently in this settlement.
    /// </summary>
    public bool PlayerInside { get; set; }

    /// <summary>
    /// Event fired when player enters the settlement.
    /// </summary>
    public event EventHandler? PlayerEntered;

    /// <summary>
    /// Event fired when player exits the settlement.
    /// </summary>
    public event EventHandler? PlayerExited;

    public Settlement(SettlementDefinition definition, GameStateService gameState)
    {
        _definition = definition;
        _gameState = gameState;

        // Create NPCs
        foreach (var npcId in definition.NpcIds)
        {
            var npc = NPC.Create(npcId, gameState);
            if (npc != null)
            {
                _npcs.Add(npc);
            }
        }

        // Position NPCs within settlement
        PositionNPCs();
    }

    private void PositionNPCs()
    {
        if (_npcs.Count == 0)
            return;

        // Arrange NPCs in a grid pattern within the settlement
        int cols = (int)Math.Ceiling(Math.Sqrt(_npcs.Count));
        float spacing = Math.Min(Size.X, Size.Y) / (cols + 1);

        for (int i = 0; i < _npcs.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            float x = Position.X - Size.X / 2 + spacing * (col + 1);
            float y = Position.Y - Size.Y / 2 + spacing * (row + 1);

            _npcs[i].Position = new Vector2(x, y);
        }
    }

    /// <summary>
    /// Updates NPC positions when settlement position changes.
    /// </summary>
    public void UpdateNPCPositions()
    {
        PositionNPCs();
    }

    /// <summary>
    /// Checks if a point is inside the settlement.
    /// </summary>
    public bool Contains(Vector2 point)
    {
        return Bounds.Contains((int)point.X, (int)point.Y);
    }

    /// <summary>
    /// Checks if a rectangle intersects the settlement.
    /// </summary>
    public bool Intersects(Rectangle rect)
    {
        return Bounds.Intersects(rect);
    }

    /// <summary>
    /// Updates the settlement state.
    /// </summary>
    public void Update(GameTime gameTime, Vector2 playerPosition)
    {
        bool wasInside = PlayerInside;
        PlayerInside = Contains(playerPosition);

        if (PlayerInside && !wasInside)
        {
            PlayerEntered?.Invoke(this, EventArgs.Empty);
        }
        else if (!PlayerInside && wasInside)
        {
            PlayerExited?.Invoke(this, EventArgs.Empty);
        }

        // Update NPCs
        foreach (var npc in _npcs)
        {
            npc.Update(gameTime);
        }
    }

    /// <summary>
    /// Gets the nearest interactable NPC to a position.
    /// </summary>
    public NPC? GetNearestInteractableNPC(Vector2 position)
    {
        NPC? nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var npc in _npcs)
        {
            if (!npc.IsVisible || !npc.CanInteract)
                continue;

            float distance = Vector2.Distance(position, npc.Position);
            if (distance < nearestDistance && npc.IsInRange(position))
            {
                nearest = npc;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Checks if a specific service is available.
    /// </summary>
    public bool HasService(SettlementServices service)
    {
        return (Services & service) != 0;
    }

    /// <summary>
    /// Draws the settlement.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, Vector2 cameraPosition)
    {
        if (!IsAccessible)
            return;

        var screenPos = Position - cameraPosition;
        var rect = new Rectangle(
            (int)(screenPos.X - Size.X / 2),
            (int)(screenPos.Y - Size.Y / 2),
            (int)Size.X,
            (int)Size.Y
        );

        // Draw settlement background
        spriteBatch.Draw(pixelTexture, rect, _definition.PlaceholderColor * 0.3f);

        // Draw border
        DrawBorder(spriteBatch, pixelTexture, rect, _definition.PlaceholderColor);

        // Draw NPCs
        foreach (var npc in _npcs)
        {
            npc.Draw(spriteBatch, pixelTexture, font, cameraPosition);
        }

        // Draw settlement name
        if (font != null)
        {
            var nameSize = font.MeasureString(Name);
            var namePos = new Vector2(
                screenPos.X - nameSize.X / 2,
                rect.Y - nameSize.Y - 10
            );
            spriteBatch.DrawString(font, Name, namePos, Color.White);

            // Draw safe zone indicator
            if (IsSafeZone)
            {
                var safeText = "[Safe Zone]";
                var safeSize = font.MeasureString(safeText);
                var safePos = new Vector2(
                    screenPos.X - safeSize.X / 2,
                    rect.Y - nameSize.Y - safeSize.Y - 15
                );
                spriteBatch.DrawString(font, safeText, safePos, Color.LimeGreen);
            }
        }
    }

    private void DrawBorder(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle rect, Color color)
    {
        int thickness = 3;

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
/// Static registry of all settlement definitions.
/// </summary>
public static class SettlementDefinitions
{
    private static readonly Dictionary<string, SettlementDefinition> _definitions = new();

    /// <summary>
    /// All settlement definitions.
    /// </summary>
    public static IReadOnlyDictionary<string, SettlementDefinition> All => _definitions;

    /// <summary>
    /// Gets a settlement definition by ID.
    /// </summary>
    public static SettlementDefinition? Get(string id) =>
        _definitions.TryGetValue(id, out var def) ? def : null;

    static SettlementDefinitions()
    {
        // The Fringe settlements
        Register(new SettlementDefinition
        {
            Id = "fringe_camp",
            Name = "Salvager's Camp",
            Description = "A makeshift camp where survivors trade and rest.",
            Biome = BiomeType.Fringe,
            Faction = Faction.Salvagers,
            Services = SettlementServices.Healing | SettlementServices.Trading | SettlementServices.Rest,
            NpcIds = new List<string> { "trader_rust", "healer_sara" },
            Size = new Vector2(250, 200),
            PlaceholderColor = Color.SaddleBrown,
            IsSafeZone = true
        });

        Register(new SettlementDefinition
        {
            Id = "nimdok_terminal_loc",
            Name = "Lazarus Terminal",
            Description = "An interface point to communicate with Lazarus.",
            Biome = BiomeType.Fringe,
            Faction = Faction.Lazarus,
            Services = SettlementServices.Quests,
            NpcIds = new List<string> { "nimdok_terminal" },
            Size = new Vector2(100, 100),
            PlaceholderColor = Color.DarkMagenta,
            IsSafeZone = true
        });

        // Rust Belt settlements
        Register(new SettlementDefinition
        {
            Id = "rust_haven",
            Name = "Rust Haven",
            Description = "A fortified settlement built from scrap metal.",
            Biome = BiomeType.Rust,
            Faction = Faction.Machinists,
            Services = SettlementServices.All,
            NpcIds = new List<string> { "machinist_volt" },
            Size = new Vector2(300, 250),
            PlaceholderColor = Color.OrangeRed,
            RequiresFlag = "reached_rust_belt",
            IsSafeZone = true
        });

        // Green Zone settlements
        Register(new SettlementDefinition
        {
            Id = "green_sanctuary",
            Name = "Green Sanctuary",
            Description = "A haven for Kyns among the overgrown ruins.",
            Biome = BiomeType.Green,
            Faction = Faction.Shepherds,
            Services = SettlementServices.All,
            NpcIds = new List<string> { "shepherd_elder" },
            Size = new Vector2(350, 300),
            PlaceholderColor = Color.ForestGreen,
            RequiresFlag = "reached_green_zone",
            IsSafeZone = true
        });

        // Quiet Buffer settlements
        Register(new SettlementDefinition
        {
            Id = "quiet_refuge",
            Name = "The Quiet Refuge",
            Description = "A hidden shelter in the silent zone.",
            Biome = BiomeType.Quiet,
            Faction = Faction.None,
            Services = SettlementServices.Healing | SettlementServices.Rest,
            NpcIds = new List<string>(),
            Size = new Vector2(150, 150),
            PlaceholderColor = Color.SlateGray,
            RequiresFlag = "discovered_quiet_refuge",
            IsSafeZone = true
        });
    }

    private static void Register(SettlementDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }
}
