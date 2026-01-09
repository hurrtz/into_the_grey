using Microsoft.Xna.Framework;
using Strays.Core.Game.World;

namespace Strays.Core.Game.Dungeons;

/// <summary>
/// Represents an entrance to a dungeon instance in the world.
/// </summary>
public class DungeonPortal
{
    /// <summary>
    /// The dungeon this portal leads to.
    /// </summary>
    public string DungeonId { get; init; } = "";

    /// <summary>
    /// World position of the portal.
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// Collision bounds for the portal.
    /// </summary>
    public Rectangle Bounds => new Rectangle(
        (int)Position.X - 24,
        (int)Position.Y - 24,
        48,
        48
    );

    /// <summary>
    /// Which biome this portal is located in.
    /// </summary>
    public BiomeType Biome { get; init; }

    /// <summary>
    /// Whether this portal is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Story flag required to use this portal (optional).
    /// </summary>
    public string? RequiredFlag { get; init; }

    /// <summary>
    /// Display name shown when near the portal.
    /// </summary>
    public string DisplayName => Dungeons.Get(DungeonId)?.Name ?? "Unknown Dungeon";

    /// <summary>
    /// Gets the dungeon definition.
    /// </summary>
    public DungeonDefinition? GetDungeon() => Dungeons.Get(DungeonId);

    /// <summary>
    /// Creates a portal for a dungeon.
    /// </summary>
    public static DungeonPortal Create(string dungeonId, BiomeType biome, Vector2 position, string? requiredFlag = null)
    {
        return new DungeonPortal
        {
            DungeonId = dungeonId,
            Biome = biome,
            Position = position,
            RequiredFlag = requiredFlag
        };
    }
}
