using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Lazarus.Core.Game.Progression;

/// <summary>
/// Categories of achievements.
/// </summary>
public enum AchievementCategory
{
    /// <summary>
    /// Story progression achievements.
    /// </summary>
    Story,

    /// <summary>
    /// Combat-related achievements.
    /// </summary>
    Combat,

    /// <summary>
    /// Collection/completion achievements.
    /// </summary>
    Collection,

    /// <summary>
    /// Exploration achievements.
    /// </summary>
    Exploration,

    /// <summary>
    /// Secret/hidden achievements.
    /// </summary>
    Secret,

    /// <summary>
    /// Social/companion achievements.
    /// </summary>
    Social,

    /// <summary>
    /// Challenge achievements.
    /// </summary>
    Challenge
}

/// <summary>
/// Rarity tier of an achievement.
/// </summary>
public enum AchievementRarity
{
    /// <summary>
    /// Common achievements most players will get.
    /// </summary>
    Common,

    /// <summary>
    /// Uncommon achievements requiring some effort.
    /// </summary>
    Uncommon,

    /// <summary>
    /// Rare achievements requiring dedication.
    /// </summary>
    Rare,

    /// <summary>
    /// Very rare achievements for completionists.
    /// </summary>
    Epic,

    /// <summary>
    /// Legendary achievements for the most dedicated.
    /// </summary>
    Legendary
}

/// <summary>
/// Definition of an achievement.
/// </summary>
public class AchievementDefinition
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
    /// Description (shown when locked if not secret).
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Description shown when unlocked.
    /// </summary>
    public string UnlockedDescription { get; init; } = "";

    /// <summary>
    /// Category.
    /// </summary>
    public AchievementCategory Category { get; init; }

    /// <summary>
    /// Rarity tier.
    /// </summary>
    public AchievementRarity Rarity { get; init; } = AchievementRarity.Common;

    /// <summary>
    /// Whether this is a secret achievement (hidden until unlocked).
    /// </summary>
    public bool IsSecret { get; init; } = false;

    /// <summary>
    /// Points/score awarded.
    /// </summary>
    public int Points { get; init; } = 10;

    /// <summary>
    /// Icon color.
    /// </summary>
    public Color IconColor { get; init; } = Color.Gold;

    /// <summary>
    /// Counter target for progress achievements (0 = instant unlock).
    /// </summary>
    public int TargetCount { get; init; } = 0;

    /// <summary>
    /// Stat key to track for progress.
    /// </summary>
    public string? TrackedStat { get; init; }

    /// <summary>
    /// Flag that triggers this achievement.
    /// </summary>
    public string? TriggerFlag { get; init; }

    /// <summary>
    /// Reward item IDs.
    /// </summary>
    public List<string> RewardItems { get; init; } = new();

    /// <summary>
    /// Reward currency.
    /// </summary>
    public int RewardCurrency { get; init; } = 0;

    /// <summary>
    /// Unlocks for New Game+.
    /// </summary>
    public List<string> NewGamePlusUnlocks { get; init; } = new();
}

/// <summary>
/// Runtime state of an achievement.
/// </summary>
public class AchievementState
{
    /// <summary>
    /// Definition ID.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Whether unlocked.
    /// </summary>
    public bool IsUnlocked { get; set; } = false;

    /// <summary>
    /// Time when unlocked.
    /// </summary>
    public DateTime? UnlockedAt { get; set; }

    /// <summary>
    /// Current progress (for counter achievements).
    /// </summary>
    public int Progress { get; set; } = 0;

    /// <summary>
    /// Whether rewards have been claimed.
    /// </summary>
    public bool RewardsClaimed { get; set; } = false;
}

/// <summary>
/// Event args for achievement unlock.
/// </summary>
public class AchievementUnlockedEventArgs : EventArgs
{
    public AchievementDefinition Achievement { get; init; } = null!;
    public AchievementState State { get; init; } = null!;
}

/// <summary>
/// System for tracking and unlocking achievements.
/// </summary>
public class AchievementSystem
{
    private static readonly Dictionary<string, AchievementDefinition> _definitions = new();
    private readonly Dictionary<string, AchievementState> _states = new();
    private readonly Dictionary<string, int> _stats = new();

    /// <summary>
    /// Event fired when an achievement is unlocked.
    /// </summary>
    public event EventHandler<AchievementUnlockedEventArgs>? AchievementUnlocked;

    /// <summary>
    /// Event fired when progress is made.
    /// </summary>
    public event EventHandler<(string achievementId, int progress, int target)>? ProgressMade;

    /// <summary>
    /// Queue of recently unlocked achievements for display.
    /// </summary>
    public Queue<AchievementDefinition> UnlockQueue { get; } = new();

    static AchievementSystem()
    {
        RegisterAchievements();
    }

    /// <summary>
    /// Registers all achievements.
    /// </summary>
    private static void RegisterAchievements()
    {
        // Story Achievements
        Register(new AchievementDefinition
        {
            Id = "ach_first_steps",
            Name = "First Steps",
            Description = "Begin your journey in the wasteland",
            UnlockedDescription = "You awoke in the pod field and took your first steps.",
            Category = AchievementCategory.Story,
            Rarity = AchievementRarity.Common,
            Points = 10,
            TriggerFlag = "tutorial_complete",
            IconColor = Color.SkyBlue
        });

        Register(new AchievementDefinition
        {
            Id = "ach_meet_companion",
            Name = "Loyal Friend",
            Description = "Meet your companion",
            UnlockedDescription = "You found Bandit, your loyal companion.",
            Category = AchievementCategory.Story,
            Rarity = AchievementRarity.Common,
            Points = 15,
            TriggerFlag = "met_companion",
            IconColor = Color.Orange
        });

        Register(new AchievementDefinition
        {
            Id = "ach_act1_complete",
            Name = "Into The Grey",
            Description = "Complete Act 1",
            UnlockedDescription = "You've seen the truth of the wasteland. There's no going back.",
            Category = AchievementCategory.Story,
            Rarity = AchievementRarity.Uncommon,
            Points = 25,
            TriggerFlag = "act1_complete",
            IconColor = Color.Silver
        });

        Register(new AchievementDefinition
        {
            Id = "ach_act2_complete",
            Name = "The Weight of Responsibility",
            Description = "Complete Act 2",
            UnlockedDescription = "Bandit's condition worsens. The path ahead grows darker.",
            Category = AchievementCategory.Story,
            Rarity = AchievementRarity.Rare,
            Points = 50,
            TriggerFlag = "act2_complete",
            IconColor = Color.Gold
        });

        Register(new AchievementDefinition
        {
            Id = "ach_game_complete",
            Name = "The End... Or Is It?",
            Description = "Complete the game",
            UnlockedDescription = "You reached an ending. But was it the right one?",
            Category = AchievementCategory.Story,
            Rarity = AchievementRarity.Rare,
            Points = 100,
            TriggerFlag = "game_complete",
            IconColor = Color.Gold,
            RewardCurrency = 1000
        });

        // Combat Achievements
        Register(new AchievementDefinition
        {
            Id = "ach_first_victory",
            Name = "Trial by Fire",
            Description = "Win your first combat",
            UnlockedDescription = "You proved you can survive.",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Common,
            Points = 10,
            TrackedStat = "battles_won",
            TargetCount = 1,
            IconColor = Color.Red
        });

        Register(new AchievementDefinition
        {
            Id = "ach_veteran",
            Name = "Veteran",
            Description = "Win 50 battles",
            UnlockedDescription = "Battle-hardened and experienced.",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Uncommon,
            Points = 30,
            TrackedStat = "battles_won",
            TargetCount = 50,
            IconColor = Color.Crimson
        });

        Register(new AchievementDefinition
        {
            Id = "ach_warlord",
            Name = "Warlord",
            Description = "Win 200 battles",
            UnlockedDescription = "A legend of the wasteland.",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Epic,
            Points = 75,
            TrackedStat = "battles_won",
            TargetCount = 200,
            IconColor = Color.DarkRed
        });

        Register(new AchievementDefinition
        {
            Id = "ach_boss_slayer",
            Name = "Boss Slayer",
            Description = "Defeat 5 bosses",
            UnlockedDescription = "The mightiest fall before you.",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Rare,
            Points = 50,
            TrackedStat = "bosses_defeated",
            TargetCount = 5,
            IconColor = Color.Purple
        });

        Register(new AchievementDefinition
        {
            Id = "ach_pacifist_battle",
            Name = "Merciful Victor",
            Description = "Win a battle without dealing damage",
            UnlockedDescription = "Sometimes the best victory requires no violence.",
            Category = AchievementCategory.Combat,
            Rarity = AchievementRarity.Rare,
            Points = 40,
            TriggerFlag = "pacifist_victory",
            IconColor = Color.White,
            IsSecret = true
        });

        // Collection Achievements
        Register(new AchievementDefinition
        {
            Id = "ach_first_kyn",
            Name = "New Friend",
            Description = "Recruit your first Kyn",
            UnlockedDescription = "The first of many companions.",
            Category = AchievementCategory.Collection,
            Rarity = AchievementRarity.Common,
            Points = 15,
            TrackedStat = "kyns_recruited",
            TargetCount = 1,
            IconColor = Color.LimeGreen
        });

        Register(new AchievementDefinition
        {
            Id = "ach_collector",
            Name = "Collector",
            Description = "Recruit 20 different Kyns",
            UnlockedDescription = "Your roster grows impressive.",
            Category = AchievementCategory.Collection,
            Rarity = AchievementRarity.Uncommon,
            Points = 40,
            TrackedStat = "unique_kyns",
            TargetCount = 20,
            IconColor = Color.Green
        });

        Register(new AchievementDefinition
        {
            Id = "ach_completionist",
            Name = "Completionist",
            Description = "Recruit 50 different Kyns",
            UnlockedDescription = "Master of Kyns.",
            Category = AchievementCategory.Collection,
            Rarity = AchievementRarity.Epic,
            Points = 100,
            TrackedStat = "unique_kyns",
            TargetCount = 50,
            IconColor = Color.ForestGreen
        });

        Register(new AchievementDefinition
        {
            Id = "ach_chip_master",
            Name = "Chip Master",
            Description = "Max out a microchip's firmware level",
            UnlockedDescription = "Technology bends to your will.",
            Category = AchievementCategory.Collection,
            Rarity = AchievementRarity.Rare,
            Points = 35,
            TriggerFlag = "maxed_chip",
            IconColor = Color.Cyan
        });

        // Exploration Achievements
        Register(new AchievementDefinition
        {
            Id = "ach_explorer",
            Name = "Explorer",
            Description = "Visit all 7 biomes",
            UnlockedDescription = "You've seen every corner of the wasteland.",
            Category = AchievementCategory.Exploration,
            Rarity = AchievementRarity.Uncommon,
            Points = 40,
            TrackedStat = "biomes_visited",
            TargetCount = 7,
            IconColor = Color.DodgerBlue
        });

        Register(new AchievementDefinition
        {
            Id = "ach_archive_found",
            Name = "Lost and Found",
            Description = "Discover the Archive Scar",
            UnlockedDescription = "Some places are meant to stay forgotten.",
            Category = AchievementCategory.Exploration,
            Rarity = AchievementRarity.Rare,
            Points = 50,
            TriggerFlag = "discovered_archive_scar",
            IconColor = Color.MediumPurple,
            IsSecret = true
        });

        Register(new AchievementDefinition
        {
            Id = "ach_dungeon_master",
            Name = "Dungeon Master",
            Description = "Complete 10 dungeons",
            UnlockedDescription = "No dungeon can stop you.",
            Category = AchievementCategory.Exploration,
            Rarity = AchievementRarity.Uncommon,
            Points = 45,
            TrackedStat = "dungeons_completed",
            TargetCount = 10,
            IconColor = Color.DarkSlateGray
        });

        // Secret Achievements
        Register(new AchievementDefinition
        {
            Id = "ach_diadem_complete",
            Name = "The Crown",
            Description = "???",
            UnlockedDescription = "The Diadem is complete. You hold absolute power.",
            Category = AchievementCategory.Secret,
            Rarity = AchievementRarity.Legendary,
            Points = 150,
            TriggerFlag = "diadem_complete",
            IconColor = Color.Gold,
            IsSecret = true,
            NewGamePlusUnlocks = new List<string> { "diadem_item" }
        });

        Register(new AchievementDefinition
        {
            Id = "ach_all_endings",
            Name = "Every Possible Path",
            Description = "???",
            UnlockedDescription = "You've seen every ending. The Grey holds no more secrets.",
            Category = AchievementCategory.Secret,
            Rarity = AchievementRarity.Legendary,
            Points = 200,
            TrackedStat = "endings_seen",
            TargetCount = 7,
            IconColor = Color.Magenta,
            IsSecret = true,
            NewGamePlusUnlocks = new List<string> { "developer_commentary" }
        });

        Register(new AchievementDefinition
        {
            Id = "ach_true_ending",
            Name = "The Truth",
            Description = "???",
            UnlockedDescription = "You found the truth behind everything.",
            Category = AchievementCategory.Secret,
            Rarity = AchievementRarity.Legendary,
            Points = 175,
            TriggerFlag = "true_ending",
            IconColor = Color.White,
            IsSecret = true
        });

        // Social/Companion Achievements
        Register(new AchievementDefinition
        {
            Id = "ach_companion_max",
            Name = "Best Friends",
            Description = "Reach maximum bond with your companion",
            UnlockedDescription = "Bandit trusts you completely.",
            Category = AchievementCategory.Social,
            Rarity = AchievementRarity.Rare,
            Points = 60,
            TriggerFlag = "max_companion_bond",
            IconColor = Color.Pink
        });

        Register(new AchievementDefinition
        {
            Id = "ach_helped_npcs",
            Name = "Good Samaritan",
            Description = "Help 20 NPCs with their problems",
            UnlockedDescription = "The wasteland is a little brighter thanks to you.",
            Category = AchievementCategory.Social,
            Rarity = AchievementRarity.Uncommon,
            Points = 35,
            TrackedStat = "npcs_helped",
            TargetCount = 20,
            IconColor = Color.Yellow
        });

        // Challenge Achievements
        Register(new AchievementDefinition
        {
            Id = "ach_speedrun",
            Name = "Speedrunner",
            Description = "Complete the game in under 4 hours",
            UnlockedDescription = "Fast and efficient.",
            Category = AchievementCategory.Challenge,
            Rarity = AchievementRarity.Epic,
            Points = 100,
            TriggerFlag = "speedrun_complete",
            IconColor = Color.Turquoise,
            IsSecret = true
        });

        Register(new AchievementDefinition
        {
            Id = "ach_no_deaths",
            Name = "Untouchable",
            Description = "Complete the game without any party wipes",
            UnlockedDescription = "Flawless victory.",
            Category = AchievementCategory.Challenge,
            Rarity = AchievementRarity.Legendary,
            Points = 150,
            TriggerFlag = "no_deaths_run",
            IconColor = Color.Gold,
            IsSecret = true
        });
    }

    /// <summary>
    /// Registers an achievement definition.
    /// </summary>
    private static void Register(AchievementDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }

    /// <summary>
    /// Gets an achievement definition.
    /// </summary>
    public static AchievementDefinition? GetDefinition(string id)
    {
        return _definitions.TryGetValue(id, out var def) ? def : null;
    }

    /// <summary>
    /// Gets all achievement definitions.
    /// </summary>
    public static IEnumerable<AchievementDefinition> GetAllDefinitions()
    {
        return _definitions.Values;
    }

    /// <summary>
    /// Gets or creates state for an achievement.
    /// </summary>
    public AchievementState GetState(string id)
    {
        if (!_states.TryGetValue(id, out var state))
        {
            state = new AchievementState { Id = id };
            _states[id] = state;
        }

        return state;
    }

    /// <summary>
    /// Checks if an achievement is unlocked.
    /// </summary>
    public bool IsUnlocked(string id)
    {
        return GetState(id).IsUnlocked;
    }

    /// <summary>
    /// Gets progress for an achievement.
    /// </summary>
    public (int current, int target) GetProgress(string id)
    {
        var state = GetState(id);
        var def = GetDefinition(id);

        return (state.Progress, def?.TargetCount ?? 0);
    }

    /// <summary>
    /// Unlocks an achievement.
    /// </summary>
    public bool Unlock(string id)
    {
        var state = GetState(id);

        if (state.IsUnlocked)
        {
            return false;
        }

        var def = GetDefinition(id);
        if (def == null)
        {
            return false;
        }

        state.IsUnlocked = true;
        state.UnlockedAt = DateTime.Now;
        state.Progress = def.TargetCount;

        UnlockQueue.Enqueue(def);

        AchievementUnlocked?.Invoke(this, new AchievementUnlockedEventArgs
        {
            Achievement = def,
            State = state
        });

        return true;
    }

    /// <summary>
    /// Checks a flag and unlocks any associated achievements.
    /// </summary>
    public void CheckFlag(string flag)
    {
        foreach (var def in _definitions.Values)
        {
            if (def.TriggerFlag == flag && !IsUnlocked(def.Id))
            {
                Unlock(def.Id);
            }
        }
    }

    /// <summary>
    /// Increments a stat and checks for related achievements.
    /// </summary>
    public void IncrementStat(string statName, int amount = 1)
    {
        if (!_stats.ContainsKey(statName))
        {
            _stats[statName] = 0;
        }

        _stats[statName] += amount;

        // Check achievements that track this stat
        foreach (var def in _definitions.Values)
        {
            if (def.TrackedStat != statName || IsUnlocked(def.Id))
            {
                continue;
            }

            var state = GetState(def.Id);
            state.Progress = _stats[statName];

            ProgressMade?.Invoke(this, (def.Id, state.Progress, def.TargetCount));

            if (state.Progress >= def.TargetCount)
            {
                Unlock(def.Id);
            }
        }
    }

    /// <summary>
    /// Sets a stat value directly.
    /// </summary>
    public void SetStat(string statName, int value)
    {
        int current = _stats.GetValueOrDefault(statName, 0);
        int diff = value - current;

        if (diff > 0)
        {
            IncrementStat(statName, diff);
        }
        else
        {
            _stats[statName] = value;
        }
    }

    /// <summary>
    /// Gets a stat value.
    /// </summary>
    public int GetStat(string statName)
    {
        return _stats.GetValueOrDefault(statName, 0);
    }

    /// <summary>
    /// Gets total points earned.
    /// </summary>
    public int GetTotalPoints()
    {
        return _states.Values
            .Where(s => s.IsUnlocked)
            .Sum(s => GetDefinition(s.Id)?.Points ?? 0);
    }

    /// <summary>
    /// Gets unlock count.
    /// </summary>
    public (int unlocked, int total) GetUnlockCount()
    {
        int total = _definitions.Count;
        int unlocked = _states.Values.Count(s => s.IsUnlocked);
        return (unlocked, total);
    }

    /// <summary>
    /// Gets achievements by category.
    /// </summary>
    public IEnumerable<(AchievementDefinition def, AchievementState state)> GetByCategory(AchievementCategory category)
    {
        return _definitions.Values
            .Where(d => d.Category == category)
            .Select(d => (d, GetState(d.Id)))
            .OrderByDescending(x => x.Item2.IsUnlocked)
            .ThenBy(x => x.d.Points);
    }

    /// <summary>
    /// Gets recently unlocked achievements.
    /// </summary>
    public IEnumerable<(AchievementDefinition def, AchievementState state)> GetRecentUnlocks(int count = 5)
    {
        return _states.Values
            .Where(s => s.IsUnlocked && s.UnlockedAt.HasValue)
            .OrderByDescending(s => s.UnlockedAt)
            .Take(count)
            .Select(s => (GetDefinition(s.Id)!, s));
    }

    /// <summary>
    /// Exports state for saving.
    /// </summary>
    public Dictionary<string, object> ExportState()
    {
        return new Dictionary<string, object>
        {
            { "states", _states.ToDictionary(k => k.Key, v => new { v.Value.IsUnlocked, v.Value.UnlockedAt, v.Value.Progress, v.Value.RewardsClaimed }) },
            { "stats", _stats }
        };
    }

    /// <summary>
    /// Imports state from save data.
    /// </summary>
    public void ImportState(Dictionary<string, int> stats, Dictionary<string, (bool unlocked, DateTime? when, int progress, bool claimed)> states)
    {
        _stats.Clear();
        foreach (var kvp in stats)
        {
            _stats[kvp.Key] = kvp.Value;
        }

        _states.Clear();
        foreach (var kvp in states)
        {
            _states[kvp.Key] = new AchievementState
            {
                Id = kvp.Key,
                IsUnlocked = kvp.Value.unlocked,
                UnlockedAt = kvp.Value.when,
                Progress = kvp.Value.progress,
                RewardsClaimed = kvp.Value.claimed
            };
        }
    }

    /// <summary>
    /// Gets the color for an achievement rarity.
    /// </summary>
    public static Color GetRarityColor(AchievementRarity rarity)
    {
        return rarity switch
        {
            AchievementRarity.Common => Color.White,
            AchievementRarity.Uncommon => Color.LimeGreen,
            AchievementRarity.Rare => Color.DodgerBlue,
            AchievementRarity.Epic => Color.MediumPurple,
            AchievementRarity.Legendary => Color.Gold,
            _ => Color.Gray
        };
    }
}
