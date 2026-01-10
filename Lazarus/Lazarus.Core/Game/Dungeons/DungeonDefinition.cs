using System.Collections.Generic;
using Lazarus.Core.Game.World;

namespace Lazarus.Core.Game.Dungeons;

/// <summary>
/// Difficulty levels for dungeon instances.
/// </summary>
public enum DungeonDifficulty
{
    Easy,
    Normal,
    Hard,
    Brutal
}

/// <summary>
/// Defines a dungeon template that can be instanced.
/// </summary>
public class DungeonDefinition
{
    /// <summary>
    /// Unique identifier for this dungeon.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name of the dungeon.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Description shown to player before entering.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Which biome this dungeon is located in.
    /// </summary>
    public BiomeType Biome { get; init; } = BiomeType.Fringe;

    /// <summary>
    /// Minimum recommended level.
    /// </summary>
    public int MinLevel { get; init; } = 1;

    /// <summary>
    /// Maximum recommended level.
    /// </summary>
    public int MaxLevel { get; init; } = 10;

    /// <summary>
    /// Total number of rooms in the dungeon.
    /// </summary>
    public int RoomCount { get; init; } = 10;

    /// <summary>
    /// Which room contains the mid-boss (0-indexed).
    /// </summary>
    public int MidBossRoom { get; init; } = 4;

    /// <summary>
    /// Which room contains the final boss (0-indexed, usually last).
    /// </summary>
    public int FinalBossRoom { get; init; } = 9;

    /// <summary>
    /// Encounter IDs for regular rooms.
    /// </summary>
    public List<string> RegularEncounterIds { get; init; } = new();

    /// <summary>
    /// Kyn IDs that can appear in regular encounters.
    /// </summary>
    public List<string> RegularEnemyIds { get; init; } = new();

    /// <summary>
    /// Kyn ID for the mid-boss.
    /// </summary>
    public string MidBossId { get; init; } = "";

    /// <summary>
    /// Display name for the mid-boss.
    /// </summary>
    public string MidBossName { get; init; } = "Guardian";

    /// <summary>
    /// Kyn ID for the final boss.
    /// </summary>
    public string FinalBossId { get; init; } = "";

    /// <summary>
    /// Display name for the final boss.
    /// </summary>
    public string FinalBossName { get; init; } = "Dungeon Lord";

    /// <summary>
    /// The objective text shown to the player.
    /// </summary>
    public string ObjectiveText { get; init; } = "Defeat the dungeon boss";

    /// <summary>
    /// Item IDs that can drop as rewards.
    /// </summary>
    public List<string> RewardItemIds { get; init; } = new();

    /// <summary>
    /// Base experience reward for completion.
    /// </summary>
    public int BaseExpReward { get; init; } = 100;

    /// <summary>
    /// Base currency reward for completion.
    /// </summary>
    public int BaseCurrencyReward { get; init; } = 50;

    /// <summary>
    /// Ambient color tint for the dungeon.
    /// </summary>
    public string AmbientColor { get; init; } = "#1a1a2e";

    /// <summary>
    /// Whether this dungeon is unlocked by default.
    /// </summary>
    public bool UnlockedByDefault { get; init; } = false;

    /// <summary>
    /// Story flag required to unlock this dungeon.
    /// </summary>
    public string? RequiredFlag { get; init; }

    /// <summary>
    /// Gets the number of enemies per room based on difficulty.
    /// </summary>
    public int GetEnemyCount(DungeonDifficulty difficulty, bool isBossRoom)
    {
        if (isBossRoom) return 1; // Boss rooms have just the boss

        return difficulty switch
        {
            DungeonDifficulty.Easy => 2,
            DungeonDifficulty.Normal => 3,
            DungeonDifficulty.Hard => 4,
            DungeonDifficulty.Brutal => 5,
            _ => 3
        };
    }
}

/// <summary>
/// Difficulty modifiers applied to dungeon instances.
/// </summary>
public static class DifficultyModifiers
{
    /// <summary>
    /// Gets the HP multiplier for enemies.
    /// </summary>
    public static float GetHpMultiplier(DungeonDifficulty difficulty) => difficulty switch
    {
        DungeonDifficulty.Easy => 0.75f,
        DungeonDifficulty.Normal => 1.0f,
        DungeonDifficulty.Hard => 1.5f,
        DungeonDifficulty.Brutal => 2.0f,
        _ => 1.0f
    };

    /// <summary>
    /// Gets the damage multiplier for enemies.
    /// </summary>
    public static float GetDamageMultiplier(DungeonDifficulty difficulty) => difficulty switch
    {
        DungeonDifficulty.Easy => 0.75f,
        DungeonDifficulty.Normal => 1.0f,
        DungeonDifficulty.Hard => 1.25f,
        DungeonDifficulty.Brutal => 1.5f,
        _ => 1.0f
    };

    /// <summary>
    /// Gets the reward multiplier.
    /// </summary>
    public static float GetRewardMultiplier(DungeonDifficulty difficulty) => difficulty switch
    {
        DungeonDifficulty.Easy => 1.0f,
        DungeonDifficulty.Normal => 1.5f,
        DungeonDifficulty.Hard => 2.5f,
        DungeonDifficulty.Brutal => 4.0f,
        _ => 1.0f
    };

    /// <summary>
    /// Gets the experience multiplier.
    /// </summary>
    public static float GetExpMultiplier(DungeonDifficulty difficulty) => difficulty switch
    {
        DungeonDifficulty.Easy => 0.75f,
        DungeonDifficulty.Normal => 1.0f,
        DungeonDifficulty.Hard => 1.75f,
        DungeonDifficulty.Brutal => 2.5f,
        _ => 1.0f
    };

    /// <summary>
    /// Whether bosses have multiple phases on this difficulty.
    /// </summary>
    public static bool HasBossPhases(DungeonDifficulty difficulty) =>
        difficulty == DungeonDifficulty.Brutal;

    /// <summary>
    /// Whether enemies have extra abilities on this difficulty.
    /// </summary>
    public static bool HasExtraAbilities(DungeonDifficulty difficulty) =>
        difficulty >= DungeonDifficulty.Hard;

    /// <summary>
    /// Gets display name for difficulty.
    /// </summary>
    public static string GetDisplayName(DungeonDifficulty difficulty) => difficulty switch
    {
        DungeonDifficulty.Easy => "Easy",
        DungeonDifficulty.Normal => "Normal",
        DungeonDifficulty.Hard => "Hard",
        DungeonDifficulty.Brutal => "BRUTAL",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets description for difficulty.
    /// </summary>
    public static string GetDescription(DungeonDifficulty difficulty) => difficulty switch
    {
        DungeonDifficulty.Easy => "Reduced enemy strength. Standard rewards.",
        DungeonDifficulty.Normal => "Balanced challenge. 1.5x rewards.",
        DungeonDifficulty.Hard => "Tougher enemies with extra abilities. 2.5x rewards.",
        DungeonDifficulty.Brutal => "Maximum challenge. Boss phases. 4x rewards.",
        _ => ""
    };
}
