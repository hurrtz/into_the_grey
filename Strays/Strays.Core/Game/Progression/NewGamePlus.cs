using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Data;
using Strays.Core.Game.Entities;
using Strays.Core.Services;

namespace Strays.Core.Game.Progression;

/// <summary>
/// New Game+ cycle levels.
/// </summary>
public enum NewGamePlusCycle
{
    /// <summary>
    /// First playthrough.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// First New Game+ (NG+1).
    /// </summary>
    Plus1 = 1,

    /// <summary>
    /// Second New Game+ (NG+2).
    /// </summary>
    Plus2 = 2,

    /// <summary>
    /// Third New Game+ (NG+3).
    /// </summary>
    Plus3 = 3,

    /// <summary>
    /// Fourth New Game+ (NG+4).
    /// </summary>
    Plus4 = 4,

    /// <summary>
    /// Fifth and max New Game+ (NG+5).
    /// </summary>
    Plus5 = 5
}

/// <summary>
/// What carries over in New Game+.
/// </summary>
[Flags]
public enum NewGamePlusCarryOver
{
    None = 0,
    Level = 1 << 0,
    Currency = 1 << 1,
    Inventory = 1 << 2,
    Microchips = 1 << 3,
    Augmentations = 1 << 4,
    Bestiary = 1 << 5,
    Achievements = 1 << 6,
    RecruitedStrays = 1 << 7,
    StrayLevels = 1 << 8,
    FactionReputation = 1 << 9,
    UnlockedAbilities = 1 << 10,
    PlayTime = 1 << 11,

    /// <summary>
    /// Standard carry-over (most common items).
    /// </summary>
    Standard = Level | Currency | Inventory | Microchips | Augmentations | Bestiary | Achievements | PlayTime,

    /// <summary>
    /// Full carry-over (everything possible).
    /// </summary>
    Full = Standard | RecruitedStrays | StrayLevels | FactionReputation | UnlockedAbilities
}

/// <summary>
/// Difficulty modifiers for New Game+ cycles.
/// </summary>
public class NewGamePlusDifficulty
{
    /// <summary>
    /// Multiplier for enemy HP.
    /// </summary>
    public float EnemyHpMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier for enemy attack.
    /// </summary>
    public float EnemyAttackMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier for enemy defense.
    /// </summary>
    public float EnemyDefenseMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier for enemy speed.
    /// </summary>
    public float EnemySpeedMultiplier { get; set; } = 1f;

    /// <summary>
    /// Level offset added to all enemies.
    /// </summary>
    public int EnemyLevelOffset { get; set; } = 0;

    /// <summary>
    /// Multiplier for experience gained.
    /// </summary>
    public float ExperienceMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier for currency dropped.
    /// </summary>
    public float CurrencyMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier for rare item drop chance.
    /// </summary>
    public float RareDropMultiplier { get; set; } = 1f;

    /// <summary>
    /// Whether elite enemies spawn more frequently.
    /// </summary>
    public bool IncreasedEliteSpawns { get; set; } = false;

    /// <summary>
    /// Whether bosses have additional phases.
    /// </summary>
    public bool ExtendedBossPhases { get; set; } = false;

    /// <summary>
    /// New enemy types unlocked this cycle.
    /// </summary>
    public List<string> UnlockedEnemies { get; set; } = new();

    /// <summary>
    /// New items available this cycle.
    /// </summary>
    public List<string> UnlockedItems { get; set; } = new();

    /// <summary>
    /// New quests available this cycle.
    /// </summary>
    public List<string> UnlockedQuests { get; set; } = new();
}

/// <summary>
/// Manages New Game+ functionality.
/// </summary>
public class NewGamePlusManager
{
    private static readonly Dictionary<NewGamePlusCycle, NewGamePlusDifficulty> _difficulties = new();

    /// <summary>
    /// Current NG+ cycle.
    /// </summary>
    public NewGamePlusCycle CurrentCycle { get; private set; } = NewGamePlusCycle.Normal;

    /// <summary>
    /// Total completions across all saves.
    /// </summary>
    public int TotalCompletions { get; private set; } = 0;

    /// <summary>
    /// What will carry over to the next cycle.
    /// </summary>
    public NewGamePlusCarryOver CarryOverSettings { get; set; } = NewGamePlusCarryOver.Standard;

    /// <summary>
    /// Whether NG+ is available (game completed at least once).
    /// </summary>
    public bool IsAvailable => TotalCompletions > 0 || CurrentCycle > NewGamePlusCycle.Normal;

    /// <summary>
    /// Special flags earned across all cycles.
    /// </summary>
    public HashSet<string> PermanentFlags { get; } = new();

    /// <summary>
    /// Event fired when starting a new NG+ cycle.
    /// </summary>
    public event EventHandler<NewGamePlusCycle>? CycleStarted;

    /// <summary>
    /// Event fired when a cycle is completed.
    /// </summary>
    public event EventHandler<NewGamePlusCycle>? CycleCompleted;

    static NewGamePlusManager()
    {
        InitializeDifficulties();
    }

    private static void InitializeDifficulties()
    {
        // Normal difficulty (first playthrough)
        _difficulties[NewGamePlusCycle.Normal] = new NewGamePlusDifficulty();

        // NG+1: Moderate increase
        _difficulties[NewGamePlusCycle.Plus1] = new NewGamePlusDifficulty
        {
            EnemyHpMultiplier = 1.25f,
            EnemyAttackMultiplier = 1.15f,
            EnemyDefenseMultiplier = 1.1f,
            EnemySpeedMultiplier = 1.05f,
            EnemyLevelOffset = 5,
            ExperienceMultiplier = 1.2f,
            CurrencyMultiplier = 1.3f,
            RareDropMultiplier = 1.25f,
            UnlockedItems = new List<string> { "ng_plus_emblem", "legacy_chip" },
            UnlockedQuests = new List<string> { "ng_plus_1_quest" }
        };

        // NG+2: Significant increase
        _difficulties[NewGamePlusCycle.Plus2] = new NewGamePlusDifficulty
        {
            EnemyHpMultiplier = 1.5f,
            EnemyAttackMultiplier = 1.3f,
            EnemyDefenseMultiplier = 1.2f,
            EnemySpeedMultiplier = 1.1f,
            EnemyLevelOffset = 10,
            ExperienceMultiplier = 1.4f,
            CurrencyMultiplier = 1.6f,
            RareDropMultiplier = 1.5f,
            IncreasedEliteSpawns = true,
            UnlockedEnemies = new List<string> { "shadow_variant_1" },
            UnlockedItems = new List<string> { "veteran_emblem", "shadow_chip" },
            UnlockedQuests = new List<string> { "ng_plus_2_quest", "shadow_hunt_1" }
        };

        // NG+3: Hard mode
        _difficulties[NewGamePlusCycle.Plus3] = new NewGamePlusDifficulty
        {
            EnemyHpMultiplier = 2.0f,
            EnemyAttackMultiplier = 1.5f,
            EnemyDefenseMultiplier = 1.4f,
            EnemySpeedMultiplier = 1.2f,
            EnemyLevelOffset = 15,
            ExperienceMultiplier = 1.6f,
            CurrencyMultiplier = 2.0f,
            RareDropMultiplier = 1.75f,
            IncreasedEliteSpawns = true,
            ExtendedBossPhases = true,
            UnlockedEnemies = new List<string> { "shadow_variant_1", "shadow_variant_2", "nightmare_spawn" },
            UnlockedItems = new List<string> { "champion_emblem", "nightmare_chip" },
            UnlockedQuests = new List<string> { "ng_plus_3_quest", "shadow_hunt_2", "nightmare_gauntlet" }
        };

        // NG+4: Very hard mode
        _difficulties[NewGamePlusCycle.Plus4] = new NewGamePlusDifficulty
        {
            EnemyHpMultiplier = 2.5f,
            EnemyAttackMultiplier = 1.75f,
            EnemyDefenseMultiplier = 1.6f,
            EnemySpeedMultiplier = 1.3f,
            EnemyLevelOffset = 20,
            ExperienceMultiplier = 1.8f,
            CurrencyMultiplier = 2.5f,
            RareDropMultiplier = 2.0f,
            IncreasedEliteSpawns = true,
            ExtendedBossPhases = true,
            UnlockedEnemies = new List<string> { "shadow_variant_1", "shadow_variant_2", "shadow_variant_3", "nightmare_spawn", "void_elite" },
            UnlockedItems = new List<string> { "legend_emblem", "void_chip" },
            UnlockedQuests = new List<string> { "ng_plus_4_quest", "shadow_hunt_3", "void_challenge" }
        };

        // NG+5: Maximum difficulty (cap)
        _difficulties[NewGamePlusCycle.Plus5] = new NewGamePlusDifficulty
        {
            EnemyHpMultiplier = 3.0f,
            EnemyAttackMultiplier = 2.0f,
            EnemyDefenseMultiplier = 1.8f,
            EnemySpeedMultiplier = 1.4f,
            EnemyLevelOffset = 25,
            ExperienceMultiplier = 2.0f,
            CurrencyMultiplier = 3.0f,
            RareDropMultiplier = 2.5f,
            IncreasedEliteSpawns = true,
            ExtendedBossPhases = true,
            UnlockedEnemies = new List<string> { "shadow_variant_1", "shadow_variant_2", "shadow_variant_3", "nightmare_spawn", "void_elite", "apex_predator" },
            UnlockedItems = new List<string> { "eternal_emblem", "apex_chip", "ultimate_core" },
            UnlockedQuests = new List<string> { "ng_plus_5_quest", "ultimate_hunt", "true_ending_quest" }
        };
    }

    /// <summary>
    /// Gets the difficulty settings for the current cycle.
    /// </summary>
    public NewGamePlusDifficulty GetCurrentDifficulty()
    {
        return _difficulties.TryGetValue(CurrentCycle, out var diff) ? diff : _difficulties[NewGamePlusCycle.Normal];
    }

    /// <summary>
    /// Gets the difficulty settings for a specific cycle.
    /// </summary>
    public static NewGamePlusDifficulty GetDifficulty(NewGamePlusCycle cycle)
    {
        return _difficulties.TryGetValue(cycle, out var diff) ? diff : _difficulties[NewGamePlusCycle.Normal];
    }

    /// <summary>
    /// Marks the current cycle as complete.
    /// </summary>
    public void CompleteCycle(string endingType)
    {
        TotalCompletions++;
        PermanentFlags.Add($"completed_cycle_{(int)CurrentCycle}");
        PermanentFlags.Add($"ending_{endingType}");

        // Special flags for multiple completions
        if (TotalCompletions >= 3)
        {
            PermanentFlags.Add("veteran_player");
        }

        if (TotalCompletions >= 5)
        {
            PermanentFlags.Add("dedicated_player");
        }

        if ((int)CurrentCycle >= 5)
        {
            PermanentFlags.Add("max_cycle_reached");
        }

        CycleCompleted?.Invoke(this, CurrentCycle);
    }

    /// <summary>
    /// Starts a new NG+ cycle.
    /// </summary>
    public NewGamePlusStartData StartNewCycle(GameStateService currentState)
    {
        // Determine next cycle
        var nextCycle = CurrentCycle < NewGamePlusCycle.Plus5
            ? (NewGamePlusCycle)((int)CurrentCycle + 1)
            : NewGamePlusCycle.Plus5;

        // Create carry-over data
        var startData = new NewGamePlusStartData
        {
            Cycle = nextCycle,
            CarryOver = CarryOverSettings
        };

        // Carry over based on settings
        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.Level))
        {
            startData.StartingLevel = Math.Max(1, currentState.GetProtagonistLevel());
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.Currency))
        {
            startData.Currency = currentState.Currency;
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.Inventory))
        {
            startData.InventoryItems = new Dictionary<string, int>(currentState.Inventory);
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.Microchips))
        {
            startData.Microchips = currentState.ExportMicrochips();
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.Augmentations))
        {
            startData.Augmentations = currentState.ExportAugmentations();
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.Bestiary))
        {
            startData.BestiaryUnlocks = currentState.ExportBestiaryUnlocks();
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.Achievements))
        {
            startData.Achievements = currentState.ExportAchievements();
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.RecruitedStrays))
        {
            startData.RecruitedStrayIds = currentState.ExportRecruitedStrays();
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.StrayLevels))
        {
            startData.StrayLevels = currentState.ExportStrayLevels();
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.FactionReputation))
        {
            // Carry over half of reputation
            startData.FactionReputation = currentState.ExportFactionReputation()
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value / 2);
        }

        if (CarryOverSettings.HasFlag(NewGamePlusCarryOver.PlayTime))
        {
            startData.PreviousPlayTime = currentState.TotalPlayTime;
        }

        // Always carry permanent flags
        startData.PermanentFlags = new HashSet<string>(PermanentFlags);

        CurrentCycle = nextCycle;
        CycleStarted?.Invoke(this, nextCycle);

        return startData;
    }

    /// <summary>
    /// Applies NG+ modifiers to enemy stats.
    /// </summary>
    public void ApplyEnemyModifiers(Stray enemy)
    {
        var diff = GetCurrentDifficulty();

        // Apply level offset
        int newLevel = enemy.Level + diff.EnemyLevelOffset;
        enemy.SetLevel(newLevel);

        // Note: The actual stat multipliers would need to be applied
        // during combat calculations, not directly to the Stray
    }

    /// <summary>
    /// Modifies combat stats based on current difficulty.
    /// </summary>
    public int ModifyEnemyStat(int baseStat, string statType)
    {
        var diff = GetCurrentDifficulty();

        float multiplier = statType.ToLower() switch
        {
            "hp" or "maxhp" => diff.EnemyHpMultiplier,
            "attack" or "atk" => diff.EnemyAttackMultiplier,
            "defense" or "def" => diff.EnemyDefenseMultiplier,
            "speed" or "spd" => diff.EnemySpeedMultiplier,
            _ => 1f
        };

        return (int)(baseStat * multiplier);
    }

    /// <summary>
    /// Modifies experience gain.
    /// </summary>
    public int ModifyExperience(int baseExp)
    {
        return (int)(baseExp * GetCurrentDifficulty().ExperienceMultiplier);
    }

    /// <summary>
    /// Modifies currency drops.
    /// </summary>
    public int ModifyCurrency(int baseCurrency)
    {
        return (int)(baseCurrency * GetCurrentDifficulty().CurrencyMultiplier);
    }

    /// <summary>
    /// Gets the drop rate multiplier for rare items.
    /// </summary>
    public float GetRareDropMultiplier()
    {
        return GetCurrentDifficulty().RareDropMultiplier;
    }

    /// <summary>
    /// Gets a display name for the current cycle.
    /// </summary>
    public string GetCycleDisplayName()
    {
        return CurrentCycle switch
        {
            NewGamePlusCycle.Normal => "Normal",
            NewGamePlusCycle.Plus1 => "New Game+",
            NewGamePlusCycle.Plus2 => "New Game++",
            NewGamePlusCycle.Plus3 => "New Game+++",
            NewGamePlusCycle.Plus4 => "New Game++++",
            NewGamePlusCycle.Plus5 => "New Game+++++ (MAX)",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a color for the current cycle (for UI display).
    /// </summary>
    public Color GetCycleColor()
    {
        return CurrentCycle switch
        {
            NewGamePlusCycle.Normal => Color.White,
            NewGamePlusCycle.Plus1 => Color.LightGreen,
            NewGamePlusCycle.Plus2 => Color.Cyan,
            NewGamePlusCycle.Plus3 => Color.Gold,
            NewGamePlusCycle.Plus4 => Color.Orange,
            NewGamePlusCycle.Plus5 => Color.Red,
            _ => Color.White
        };
    }

    /// <summary>
    /// Exports data for saving.
    /// </summary>
    public NewGamePlusSaveData Export()
    {
        return new NewGamePlusSaveData
        {
            CurrentCycle = (int)CurrentCycle,
            TotalCompletions = TotalCompletions,
            CarryOverSettings = (int)CarryOverSettings,
            PermanentFlags = PermanentFlags.ToList()
        };
    }

    /// <summary>
    /// Imports data from save.
    /// </summary>
    public void Import(NewGamePlusSaveData data)
    {
        CurrentCycle = (NewGamePlusCycle)data.CurrentCycle;
        TotalCompletions = data.TotalCompletions;
        CarryOverSettings = (NewGamePlusCarryOver)data.CarryOverSettings;

        PermanentFlags.Clear();
        foreach (var flag in data.PermanentFlags)
        {
            PermanentFlags.Add(flag);
        }
    }
}

/// <summary>
/// Data for starting a new NG+ cycle.
/// </summary>
public class NewGamePlusStartData
{
    public NewGamePlusCycle Cycle { get; set; }
    public NewGamePlusCarryOver CarryOver { get; set; }
    public int StartingLevel { get; set; } = 1;
    public int Currency { get; set; } = 0;
    public Dictionary<string, int> InventoryItems { get; set; } = new();
    public List<string> Microchips { get; set; } = new();
    public List<string> Augmentations { get; set; } = new();
    public HashSet<string> BestiaryUnlocks { get; set; } = new();
    public HashSet<string> Achievements { get; set; } = new();
    public List<string> RecruitedStrayIds { get; set; } = new();
    public Dictionary<string, int> StrayLevels { get; set; } = new();
    public Dictionary<string, int> FactionReputation { get; set; } = new();
    public HashSet<string> PermanentFlags { get; set; } = new();
    public TimeSpan PreviousPlayTime { get; set; } = TimeSpan.Zero;
}

/// <summary>
/// Serializable NG+ save data.
/// </summary>
public class NewGamePlusSaveData
{
    public int CurrentCycle { get; set; }
    public int TotalCompletions { get; set; }
    public int CarryOverSettings { get; set; }
    public List<string> PermanentFlags { get; set; } = new();
}
