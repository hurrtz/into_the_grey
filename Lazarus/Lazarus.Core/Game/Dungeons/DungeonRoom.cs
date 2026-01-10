using System.Collections.Generic;

namespace Lazarus.Core.Game.Dungeons;

/// <summary>
/// The type of room in a dungeon.
/// </summary>
public enum RoomType
{
    /// <summary>
    /// Starting room - no combat, briefing.
    /// </summary>
    Entrance,

    /// <summary>
    /// Regular combat room.
    /// </summary>
    Combat,

    /// <summary>
    /// Mid-boss encounter.
    /// </summary>
    MidBoss,

    /// <summary>
    /// Final boss encounter.
    /// </summary>
    FinalBoss,

    /// <summary>
    /// Rest room - heal between fights.
    /// </summary>
    Rest,

    /// <summary>
    /// Treasure room - bonus loot.
    /// </summary>
    Treasure
}

/// <summary>
/// The state of a dungeon room.
/// </summary>
public enum RoomState
{
    /// <summary>
    /// Room not yet reached.
    /// </summary>
    Locked,

    /// <summary>
    /// Room is the current room.
    /// </summary>
    Current,

    /// <summary>
    /// Combat in progress.
    /// </summary>
    InCombat,

    /// <summary>
    /// Room cleared successfully.
    /// </summary>
    Cleared,

    /// <summary>
    /// Player was defeated in this room.
    /// </summary>
    Failed
}

/// <summary>
/// Represents a single room in a dungeon instance.
/// </summary>
public class DungeonRoom
{
    /// <summary>
    /// Room index (0-based).
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// The type of this room.
    /// </summary>
    public RoomType Type { get; init; } = RoomType.Combat;

    /// <summary>
    /// Current state of the room.
    /// </summary>
    public RoomState State { get; set; } = RoomState.Locked;

    /// <summary>
    /// Display name for this room.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Description or flavor text.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Enemy Stray IDs for this room's encounter.
    /// </summary>
    public List<string> EnemyIds { get; init; } = new();

    /// <summary>
    /// Levels for each enemy (parallel to EnemyIds).
    /// </summary>
    public List<int> EnemyLevels { get; init; } = new();

    /// <summary>
    /// Whether enemies in this room are boosted (boss stats).
    /// </summary>
    public bool IsBoosted { get; init; } = false;

    /// <summary>
    /// HP multiplier applied to enemies in this room.
    /// </summary>
    public float HpMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Damage multiplier applied to enemies in this room.
    /// </summary>
    public float DamageMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Rewards dropped when clearing this room.
    /// </summary>
    public DungeonReward? RoomReward { get; set; }

    /// <summary>
    /// Experience gained from clearing this room.
    /// </summary>
    public int ExpReward { get; set; }

    /// <summary>
    /// Whether this room has been looted.
    /// </summary>
    public bool Looted { get; set; } = false;

    /// <summary>
    /// Creates an entrance room.
    /// </summary>
    public static DungeonRoom CreateEntrance(string dungeonName)
    {
        return new DungeonRoom
        {
            Index = -1,
            Type = RoomType.Entrance,
            State = RoomState.Cleared,
            Name = "Entrance",
            Description = $"You stand at the entrance to {dungeonName}. Danger awaits within."
        };
    }

    /// <summary>
    /// Creates a combat room.
    /// </summary>
    public static DungeonRoom CreateCombat(int index, List<string> enemyIds, List<int> enemyLevels,
        float hpMult, float dmgMult, int expReward)
    {
        return new DungeonRoom
        {
            Index = index,
            Type = RoomType.Combat,
            State = RoomState.Locked,
            Name = $"Room {index + 1}",
            Description = "Enemies block your path.",
            EnemyIds = enemyIds,
            EnemyLevels = enemyLevels,
            HpMultiplier = hpMult,
            DamageMultiplier = dmgMult,
            ExpReward = expReward
        };
    }

    /// <summary>
    /// Creates a mid-boss room.
    /// </summary>
    public static DungeonRoom CreateMidBoss(int index, string bossId, string bossName, int bossLevel,
        float hpMult, float dmgMult, int expReward)
    {
        return new DungeonRoom
        {
            Index = index,
            Type = RoomType.MidBoss,
            State = RoomState.Locked,
            Name = "Guardian's Chamber",
            Description = $"{bossName} guards the path forward.",
            EnemyIds = new List<string> { bossId },
            EnemyLevels = new List<int> { bossLevel },
            IsBoosted = true,
            HpMultiplier = hpMult * 1.5f, // Bosses get extra HP
            DamageMultiplier = dmgMult * 1.2f,
            ExpReward = expReward * 3
        };
    }

    /// <summary>
    /// Creates a final boss room.
    /// </summary>
    public static DungeonRoom CreateFinalBoss(int index, string bossId, string bossName, int bossLevel,
        float hpMult, float dmgMult, int expReward)
    {
        return new DungeonRoom
        {
            Index = index,
            Type = RoomType.FinalBoss,
            State = RoomState.Locked,
            Name = "Boss Lair",
            Description = $"{bossName} awaits. This is the final challenge.",
            EnemyIds = new List<string> { bossId },
            EnemyLevels = new List<int> { bossLevel },
            IsBoosted = true,
            HpMultiplier = hpMult * 2.0f, // Final boss gets even more HP
            DamageMultiplier = dmgMult * 1.5f,
            ExpReward = expReward * 5
        };
    }

    /// <summary>
    /// Creates a rest room (healing opportunity).
    /// </summary>
    public static DungeonRoom CreateRest(int index)
    {
        return new DungeonRoom
        {
            Index = index,
            Type = RoomType.Rest,
            State = RoomState.Locked,
            Name = "Safe Haven",
            Description = "A moment of respite. Your party recovers some health."
        };
    }

    /// <summary>
    /// Gets a display string for the room.
    /// </summary>
    public string GetDisplayName()
    {
        string prefix = State switch
        {
            RoomState.Cleared => "[CLEAR] ",
            RoomState.Current => "[>>>] ",
            RoomState.InCombat => "[FIGHT] ",
            RoomState.Failed => "[FAILED] ",
            _ => ""
        };

        string suffix = Type switch
        {
            RoomType.MidBoss => " (Mini-Boss)",
            RoomType.FinalBoss => " (BOSS)",
            RoomType.Rest => " (Rest)",
            RoomType.Treasure => " (Treasure)",
            _ => ""
        };

        return $"{prefix}{Name}{suffix}";
    }
}
