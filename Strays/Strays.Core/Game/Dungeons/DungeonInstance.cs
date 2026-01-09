using System;
using System.Collections.Generic;
using System.Linq;

namespace Strays.Core.Game.Dungeons;

/// <summary>
/// The overall state of a dungeon run.
/// </summary>
public enum DungeonState
{
    /// <summary>
    /// Selecting difficulty, not yet started.
    /// </summary>
    Preparing,

    /// <summary>
    /// Dungeon run in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Player defeated, run failed.
    /// </summary>
    Failed,

    /// <summary>
    /// All rooms cleared, run complete.
    /// </summary>
    Completed,

    /// <summary>
    /// Player retreated voluntarily.
    /// </summary>
    Retreated
}

/// <summary>
/// An active dungeon instance that tracks the player's progress.
/// </summary>
public class DungeonInstance
{
    private readonly Random _random = new();

    /// <summary>
    /// The definition this instance is based on.
    /// </summary>
    public DungeonDefinition Definition { get; }

    /// <summary>
    /// Selected difficulty level.
    /// </summary>
    public DungeonDifficulty Difficulty { get; private set; }

    /// <summary>
    /// Current state of the dungeon run.
    /// </summary>
    public DungeonState State { get; private set; } = DungeonState.Preparing;

    /// <summary>
    /// All rooms in this instance.
    /// </summary>
    public List<DungeonRoom> Rooms { get; } = new();

    /// <summary>
    /// Index of the current room.
    /// </summary>
    public int CurrentRoomIndex { get; private set; } = -1;

    /// <summary>
    /// The current room.
    /// </summary>
    public DungeonRoom? CurrentRoom => CurrentRoomIndex >= 0 && CurrentRoomIndex < Rooms.Count
        ? Rooms[CurrentRoomIndex]
        : null;

    /// <summary>
    /// Number of rooms cleared.
    /// </summary>
    public int RoomsCleared => Rooms.Count(r => r.State == RoomState.Cleared);

    /// <summary>
    /// Total rooms in dungeon.
    /// </summary>
    public int TotalRooms => Rooms.Count;

    /// <summary>
    /// Whether mid-boss has been defeated.
    /// </summary>
    public bool MidBossDefeated => Rooms.Any(r => r.Type == RoomType.MidBoss && r.State == RoomState.Cleared);

    /// <summary>
    /// Whether final boss has been defeated.
    /// </summary>
    public bool FinalBossDefeated => Rooms.Any(r => r.Type == RoomType.FinalBoss && r.State == RoomState.Cleared);

    /// <summary>
    /// Accumulated rewards from cleared rooms.
    /// </summary>
    public DungeonReward AccumulatedRewards { get; } = new();

    /// <summary>
    /// Time when the run started.
    /// </summary>
    public DateTime StartTime { get; private set; }

    /// <summary>
    /// Total time spent in dungeon.
    /// </summary>
    public TimeSpan ElapsedTime => DateTime.Now - StartTime;

    /// <summary>
    /// Number of party wipes (continues used).
    /// </summary>
    public int Deaths { get; private set; } = 0;

    /// <summary>
    /// Creates a new dungeon instance.
    /// </summary>
    public DungeonInstance(DungeonDefinition definition)
    {
        Definition = definition;
    }

    /// <summary>
    /// Starts the dungeon run with the selected difficulty.
    /// </summary>
    public void Start(DungeonDifficulty difficulty)
    {
        Difficulty = difficulty;
        State = DungeonState.InProgress;
        StartTime = DateTime.Now;

        GenerateRooms();

        // Start at first room
        CurrentRoomIndex = 0;
        if (Rooms.Count > 0)
        {
            Rooms[0].State = RoomState.Current;
        }
    }

    /// <summary>
    /// Generates all rooms for this instance.
    /// </summary>
    private void GenerateRooms()
    {
        Rooms.Clear();

        float hpMult = DifficultyModifiers.GetHpMultiplier(Difficulty);
        float dmgMult = DifficultyModifiers.GetDamageMultiplier(Difficulty);
        int baseExp = Definition.BaseExpReward / Definition.RoomCount;

        for (int i = 0; i < Definition.RoomCount; i++)
        {
            DungeonRoom room;

            if (i == Definition.MidBossRoom)
            {
                // Mid-boss room
                int bossLevel = Definition.MinLevel + (Definition.MaxLevel - Definition.MinLevel) / 2;
                room = DungeonRoom.CreateMidBoss(i, Definition.MidBossId, Definition.MidBossName,
                    bossLevel, hpMult, dmgMult, baseExp);
            }
            else if (i == Definition.FinalBossRoom)
            {
                // Final boss room
                room = DungeonRoom.CreateFinalBoss(i, Definition.FinalBossId, Definition.FinalBossName,
                    Definition.MaxLevel, hpMult, dmgMult, baseExp);
            }
            else
            {
                // Regular combat room
                var enemies = GenerateEnemiesForRoom(i);
                var levels = enemies.Select(_ => GetEnemyLevel(i)).ToList();
                room = DungeonRoom.CreateCombat(i, enemies, levels, hpMult, dmgMult, baseExp);
            }

            Rooms.Add(room);
        }
    }

    /// <summary>
    /// Generates enemy list for a regular combat room.
    /// </summary>
    private List<string> GenerateEnemiesForRoom(int roomIndex)
    {
        int enemyCount = Definition.GetEnemyCount(Difficulty, false);
        var enemies = new List<string>();

        // Later rooms get more varied/stronger enemies
        float progressRatio = (float)roomIndex / Definition.RoomCount;

        for (int i = 0; i < enemyCount; i++)
        {
            if (Definition.RegularEnemyIds.Count > 0)
            {
                // Pick from available enemies, favoring later enemies in later rooms
                int maxIndex = Math.Min(
                    Definition.RegularEnemyIds.Count - 1,
                    (int)(Definition.RegularEnemyIds.Count * (progressRatio + 0.5))
                );
                int enemyIndex = _random.Next(0, maxIndex + 1);
                enemies.Add(Definition.RegularEnemyIds[enemyIndex]);
            }
            else
            {
                // Fallback to generic enemy
                enemies.Add("wild_stray");
            }
        }

        return enemies;
    }

    /// <summary>
    /// Gets enemy level for a room based on progress.
    /// </summary>
    private int GetEnemyLevel(int roomIndex)
    {
        float progressRatio = (float)roomIndex / Definition.RoomCount;
        int levelRange = Definition.MaxLevel - Definition.MinLevel;
        return Definition.MinLevel + (int)(levelRange * progressRatio) + _random.Next(-1, 2);
    }

    /// <summary>
    /// Called when entering combat in the current room.
    /// </summary>
    public void EnterCombat()
    {
        if (CurrentRoom != null)
        {
            CurrentRoom.State = RoomState.InCombat;
        }
    }

    /// <summary>
    /// Called when combat in the current room is won.
    /// </summary>
    public void CompleteRoom()
    {
        if (CurrentRoom == null) return;

        CurrentRoom.State = RoomState.Cleared;

        // Generate and accumulate rewards
        var roomReward = DungeonRewardGenerator.GenerateRoomReward(Definition, CurrentRoom, Difficulty);
        CurrentRoom.RoomReward = roomReward;
        AccumulatedRewards.Add(roomReward);

        // Check for dungeon completion
        if (CurrentRoomIndex >= Rooms.Count - 1)
        {
            CompleteDungeon();
        }
    }

    /// <summary>
    /// Advances to the next room.
    /// </summary>
    public bool AdvanceToNextRoom()
    {
        if (State != DungeonState.InProgress) return false;
        if (CurrentRoom?.State != RoomState.Cleared) return false;
        if (CurrentRoomIndex >= Rooms.Count - 1) return false;

        CurrentRoomIndex++;
        Rooms[CurrentRoomIndex].State = RoomState.Current;
        return true;
    }

    /// <summary>
    /// Called when the player is defeated in combat.
    /// </summary>
    public void OnDefeat()
    {
        Deaths++;

        if (CurrentRoom != null)
        {
            CurrentRoom.State = RoomState.Failed;
        }

        // For now, defeat ends the run
        // Could implement retry/continue system here
        State = DungeonState.Failed;
    }

    /// <summary>
    /// Player voluntarily retreats from the dungeon.
    /// </summary>
    public void Retreat()
    {
        State = DungeonState.Retreated;

        // Player keeps rewards from cleared rooms
        var completionBonus = DungeonRewardGenerator.GenerateCompletionBonus(
            Definition, Difficulty, RoomsCleared, TotalRooms);
        AccumulatedRewards.Add(completionBonus);
    }

    /// <summary>
    /// Marks the dungeon as fully completed.
    /// </summary>
    private void CompleteDungeon()
    {
        State = DungeonState.Completed;

        // Add completion bonus
        var completionBonus = DungeonRewardGenerator.GenerateCompletionBonus(
            Definition, Difficulty, RoomsCleared, TotalRooms);
        AccumulatedRewards.Add(completionBonus);
    }

    /// <summary>
    /// Gets the final rewards (call after dungeon ends).
    /// </summary>
    public DungeonReward GetFinalRewards()
    {
        // Apply any final multipliers
        float deathPenalty = Math.Max(0.5f, 1.0f - Deaths * 0.1f);

        var finalReward = new DungeonReward
        {
            Experience = (int)(AccumulatedRewards.Experience * deathPenalty),
            Currency = (int)(AccumulatedRewards.Currency * deathPenalty),
            Items = new List<RewardItem>(AccumulatedRewards.Items),
            BonusItems = new List<RewardItem>(AccumulatedRewards.BonusItems)
        };

        return finalReward;
    }

    /// <summary>
    /// Gets progress as a percentage.
    /// </summary>
    public float GetProgressPercent() => TotalRooms > 0 ? (float)RoomsCleared / TotalRooms * 100f : 0f;

    /// <summary>
    /// Gets a status summary string.
    /// </summary>
    public string GetStatusSummary()
    {
        return State switch
        {
            DungeonState.Preparing => "Select difficulty to begin",
            DungeonState.InProgress => $"Room {CurrentRoomIndex + 1}/{TotalRooms} - {CurrentRoom?.Name ?? "Unknown"}",
            DungeonState.Completed => $"COMPLETE! Cleared all {TotalRooms} rooms",
            DungeonState.Failed => $"DEFEATED at Room {CurrentRoomIndex + 1}",
            DungeonState.Retreated => $"Retreated after {RoomsCleared} rooms",
            _ => "Unknown"
        };
    }
}
