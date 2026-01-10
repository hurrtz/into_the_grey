using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Lazarus.Core.Game.Combat;
using Lazarus.Core.Game.Data;

namespace Lazarus.Core.Game.Entities;

/// <summary>
/// Triggers that can cause a Stray to evolve.
/// </summary>
public enum EvolutionTrigger
{
    /// <summary>
    /// Reaching a specific level.
    /// </summary>
    Level,

    /// <summary>
    /// High stress from combat or events.
    /// </summary>
    Stress,

    /// <summary>
    /// Having a specific augmentation equipped.
    /// </summary>
    Augmentation,

    /// <summary>
    /// Using a specific item.
    /// </summary>
    Item,

    /// <summary>
    /// High bond level with protagonist.
    /// </summary>
    Bond,

    /// <summary>
    /// Story event trigger.
    /// </summary>
    Story,

    /// <summary>
    /// Exposure to corruption.
    /// </summary>
    Corruption
}

/// <summary>
/// Defines the requirements and results of an evolution.
/// </summary>
public class EvolutionDefinition
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// ID of the base form Stray.
    /// </summary>
    public string FromStrayId { get; init; } = "";

    /// <summary>
    /// ID of the evolved form Stray.
    /// </summary>
    public string ToStrayId { get; init; } = "";

    /// <summary>
    /// Primary trigger type.
    /// </summary>
    public EvolutionTrigger Trigger { get; init; } = EvolutionTrigger.Level;

    /// <summary>
    /// Required level (for Level trigger).
    /// </summary>
    public int RequiredLevel { get; init; } = 20;

    /// <summary>
    /// Required stress amount (for Stress trigger).
    /// </summary>
    public int RequiredStress { get; init; } = 100;

    /// <summary>
    /// Required augmentation ID (for Augmentation trigger).
    /// </summary>
    public string? RequiredAugmentation { get; init; }

    /// <summary>
    /// Required item ID (for Item trigger).
    /// </summary>
    public string? RequiredItem { get; init; }

    /// <summary>
    /// Required bond level (for Bond trigger).
    /// </summary>
    public int RequiredBond { get; init; } = 50;

    /// <summary>
    /// Required story flag (for Story trigger).
    /// </summary>
    public string? RequiredFlag { get; init; }

    /// <summary>
    /// Stat multipliers applied on evolution.
    /// </summary>
    public Dictionary<string, float> StatMultipliers { get; init; } = new();

    /// <summary>
    /// Abilities unlocked on evolution.
    /// </summary>
    public List<string> UnlockedAbilities { get; init; } = new();

    /// <summary>
    /// Element change on evolution (null = keep original).
    /// </summary>
    public Element? NewElement { get; init; }

    /// <summary>
    /// Placeholder color shift on evolution.
    /// </summary>
    public Color? NewColor { get; init; }

    /// <summary>
    /// Description of the evolution.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Whether this is a corruption-based evolution (negative consequences).
    /// </summary>
    public bool IsCorruptedEvolution { get; init; } = false;
}

/// <summary>
/// Tracks a Stray's evolution state and stress.
/// </summary>
public class EvolutionState
{
    /// <summary>
    /// Current stress level (0-100).
    /// </summary>
    public int Stress { get; private set; } = 0;

    /// <summary>
    /// Maximum stress before forced evolution check.
    /// </summary>
    public const int MaxStress = 100;

    /// <summary>
    /// Whether this Stray has evolved.
    /// </summary>
    public bool HasEvolved { get; private set; } = false;

    /// <summary>
    /// ID of the evolution applied (if any).
    /// </summary>
    public string? AppliedEvolutionId { get; private set; }

    /// <summary>
    /// Number of times this Stray has evolved.
    /// </summary>
    public int EvolutionCount { get; private set; } = 0;

    /// <summary>
    /// Stat multipliers from evolution.
    /// </summary>
    public Dictionary<string, float> EvolutionMultipliers { get; } = new();

    /// <summary>
    /// Abilities unlocked through evolution.
    /// </summary>
    public List<string> EvolvedAbilities { get; } = new();

    /// <summary>
    /// Event fired when stress changes.
    /// </summary>
    public event EventHandler<int>? StressChanged;

    /// <summary>
    /// Event fired when evolution occurs.
    /// </summary>
    public event EventHandler<EvolutionDefinition>? Evolved;

    /// <summary>
    /// Adds stress to the Stray.
    /// </summary>
    /// <param name="amount">Amount of stress to add.</param>
    /// <returns>True if stress cap was reached.</returns>
    public bool AddStress(int amount)
    {
        int oldStress = Stress;
        Stress = Math.Clamp(Stress + amount, 0, MaxStress);

        if (Stress != oldStress)
        {
            StressChanged?.Invoke(this, Stress);
        }

        return Stress >= MaxStress;
    }

    /// <summary>
    /// Reduces stress.
    /// </summary>
    /// <param name="amount">Amount to reduce.</param>
    public void ReduceStress(int amount)
    {
        int oldStress = Stress;
        Stress = Math.Max(0, Stress - amount);

        if (Stress != oldStress)
        {
            StressChanged?.Invoke(this, Stress);
        }
    }

    /// <summary>
    /// Resets stress to zero.
    /// </summary>
    public void ResetStress()
    {
        if (Stress > 0)
        {
            Stress = 0;
            StressChanged?.Invoke(this, Stress);
        }
    }

    /// <summary>
    /// Applies an evolution to this Stray.
    /// </summary>
    public void ApplyEvolution(EvolutionDefinition evolution)
    {
        HasEvolved = true;
        AppliedEvolutionId = evolution.Id;
        EvolutionCount++;

        // Apply stat multipliers
        foreach (var kvp in evolution.StatMultipliers)
        {
            if (EvolutionMultipliers.ContainsKey(kvp.Key))
            {
                EvolutionMultipliers[kvp.Key] *= kvp.Value;
            }
            else
            {
                EvolutionMultipliers[kvp.Key] = kvp.Value;
            }
        }

        // Add unlocked abilities
        foreach (var ability in evolution.UnlockedAbilities)
        {
            if (!EvolvedAbilities.Contains(ability))
            {
                EvolvedAbilities.Add(ability);
            }
        }

        // Reset stress after evolution
        ResetStress();

        Evolved?.Invoke(this, evolution);
    }

    /// <summary>
    /// Gets the evolution multiplier for a stat.
    /// </summary>
    public float GetMultiplier(string stat)
    {
        return EvolutionMultipliers.TryGetValue(stat, out float mult) ? mult : 1f;
    }
}

/// <summary>
/// Manages evolution checks and triggers.
/// </summary>
public class EvolutionManager
{
    /// <summary>
    /// Checks if a Stray can evolve and returns available evolutions.
    /// </summary>
    public List<EvolutionDefinition> GetAvailableEvolutions(
        Stray stray,
        Func<string, bool>? hasFlag = null,
        Func<string, bool>? hasAugmentation = null)
    {
        var available = new List<EvolutionDefinition>();

        foreach (var evolution in Evolutions.GetByFromStray(stray.Definition.Id))
        {
            if (CanEvolve(stray, evolution, hasFlag, hasAugmentation))
            {
                available.Add(evolution);
            }
        }

        return available;
    }

    /// <summary>
    /// Checks if a specific evolution is possible.
    /// </summary>
    public bool CanEvolve(
        Stray stray,
        EvolutionDefinition evolution,
        Func<string, bool>? hasFlag = null,
        Func<string, bool>? hasAugmentation = null)
    {
        // Already evolved with this specific evolution
        if (stray.EvolutionState.AppliedEvolutionId == evolution.Id)
            return false;

        return evolution.Trigger switch
        {
            EvolutionTrigger.Level => stray.Level >= evolution.RequiredLevel,
            EvolutionTrigger.Stress => stray.EvolutionState.Stress >= evolution.RequiredStress,
            EvolutionTrigger.Augmentation => !string.IsNullOrEmpty(evolution.RequiredAugmentation) &&
                                              (hasAugmentation?.Invoke(evolution.RequiredAugmentation) ?? false),
            EvolutionTrigger.Bond => stray.BondLevel >= evolution.RequiredBond,
            EvolutionTrigger.Story => !string.IsNullOrEmpty(evolution.RequiredFlag) &&
                                       (hasFlag?.Invoke(evolution.RequiredFlag) ?? false),
            EvolutionTrigger.Corruption => stray.EvolutionState.Stress >= evolution.RequiredStress &&
                                            (hasAugmentation?.Invoke("nimdok_cortex") ?? false),
            _ => false
        };
    }

    /// <summary>
    /// Triggers evolution for a Stray.
    /// </summary>
    /// <returns>True if evolution occurred.</returns>
    public bool TriggerEvolution(Stray stray, EvolutionDefinition evolution)
    {
        if (stray.EvolutionState.AppliedEvolutionId == evolution.Id)
            return false;

        stray.EvolutionState.ApplyEvolution(evolution);
        return true;
    }

    /// <summary>
    /// Adds combat stress to a Stray based on events.
    /// </summary>
    public void AddCombatStress(Stray stray, int damagePercent, bool alliesDefeated, bool nearDeath)
    {
        int stress = 0;

        // Stress from taking damage
        stress += damagePercent / 10;

        // Stress from allies being defeated
        if (alliesDefeated)
            stress += 10;

        // Stress from being near death
        if (nearDeath)
            stress += 15;

        stray.EvolutionState.AddStress(stress);
    }
}

/// <summary>
/// Static registry of all evolutions in the game.
/// </summary>
public static class Evolutions
{
    private static readonly Dictionary<string, EvolutionDefinition> _evolutions = new();

    /// <summary>
    /// All registered evolutions.
    /// </summary>
    public static IReadOnlyDictionary<string, EvolutionDefinition> All => _evolutions;

    /// <summary>
    /// Gets an evolution by ID.
    /// </summary>
    public static EvolutionDefinition? Get(string id) =>
        _evolutions.TryGetValue(id, out var evo) ? evo : null;

    /// <summary>
    /// Gets all evolutions from a specific base Stray.
    /// </summary>
    public static IEnumerable<EvolutionDefinition> GetByFromStray(string strayId) =>
        _evolutions.Values.Where(e => e.FromStrayId == strayId);

    /// <summary>
    /// Registers an evolution.
    /// </summary>
    public static void Register(EvolutionDefinition evolution)
    {
        _evolutions[evolution.Id] = evolution;
    }

    static Evolutions()
    {
        RegisterStarterEvolutions();
        RegisterCompanionEvolutions();
        RegisterCorruptedEvolutions();
    }

    private static void RegisterStarterEvolutions()
    {
        // Echo Pup evolution chain
        Register(new EvolutionDefinition
        {
            Id = "echo_pup_evolve",
            FromStrayId = "echo_pup",
            ToStrayId = "echo_hound",
            Trigger = EvolutionTrigger.Level,
            RequiredLevel = 15,
            StatMultipliers = new() { { "MaxHp", 1.3f }, { "Attack", 1.25f }, { "Speed", 1.2f } },
            UnlockedAbilities = new() { "power_strike", "overclock" },
            Description = "Echo Pup has grown into Echo Hound!"
        });

        Register(new EvolutionDefinition
        {
            Id = "echo_hound_evolve",
            FromStrayId = "echo_hound",
            ToStrayId = "echo_alpha",
            Trigger = EvolutionTrigger.Level,
            RequiredLevel = 30,
            StatMultipliers = new() { { "MaxHp", 1.4f }, { "Attack", 1.35f }, { "Speed", 1.25f }, { "Special", 1.2f } },
            UnlockedAbilities = new() { "multi_strike", "thunderbolt" },
            NewColor = Color.DarkGoldenrod,
            Description = "Echo Hound has evolved into the mighty Echo Alpha!"
        });

        // Rust Rat evolution
        Register(new EvolutionDefinition
        {
            Id = "rust_rat_evolve",
            FromStrayId = "rust_rat",
            ToStrayId = "rust_ravager",
            Trigger = EvolutionTrigger.Level,
            RequiredLevel = 12,
            StatMultipliers = new() { { "Speed", 1.4f }, { "Attack", 1.2f } },
            UnlockedAbilities = new() { "toxic_spray" },
            Description = "Rust Rat evolved into Rust Ravager!"
        });

        // Circuit Cat evolution
        Register(new EvolutionDefinition
        {
            Id = "circuit_cat_evolve",
            FromStrayId = "circuit_cat",
            ToStrayId = "voltage_lynx",
            Trigger = EvolutionTrigger.Level,
            RequiredLevel = 18,
            StatMultipliers = new() { { "Special", 1.35f }, { "Speed", 1.25f } },
            UnlockedAbilities = new() { "chain_lightning" },
            NewElement = Element.Electric,
            Description = "Circuit Cat evolved into Voltage Lynx!"
        });
    }

    private static void RegisterCompanionEvolutions()
    {
        // Bandit's evolution through Gravitation stress
        Register(new EvolutionDefinition
        {
            Id = "bandit_stress_evolve",
            FromStrayId = "companion_dog",
            ToStrayId = "companion_dog_evolved",
            Trigger = EvolutionTrigger.Stress,
            RequiredStress = 80,
            StatMultipliers = new() { { "Attack", 1.5f }, { "Special", 1.5f }, { "Defense", 0.9f } },
            UnlockedAbilities = new() { "corrupted_strike" },
            NewColor = Color.DarkOrange,
            Description = "Bandit's power is growing... but at what cost?"
        });

        // Tinker's evolution
        Register(new EvolutionDefinition
        {
            Id = "tinker_stress_evolve",
            FromStrayId = "companion_cat",
            ToStrayId = "companion_cat_evolved",
            Trigger = EvolutionTrigger.Stress,
            RequiredStress = 80,
            StatMultipliers = new() { { "Special", 1.6f }, { "Speed", 1.3f } },
            UnlockedAbilities = new() { "mind_blast" },
            NewElement = Element.Psionic,
            Description = "Tinker's mind expands beyond normal limits..."
        });

        // Pirate's evolution
        Register(new EvolutionDefinition
        {
            Id = "pirate_stress_evolve",
            FromStrayId = "companion_rabbit",
            ToStrayId = "companion_rabbit_evolved",
            Trigger = EvolutionTrigger.Stress,
            RequiredStress = 80,
            StatMultipliers = new() { { "Speed", 1.7f }, { "Attack", 1.3f } },
            UnlockedAbilities = new() { "multi_strike" },
            Description = "Pirate becomes a blur of motion and fury..."
        });
    }

    private static void RegisterCorruptedEvolutions()
    {
        // Corrupted evolution - happens when Stray has Lazarus cortex and high stress
        Register(new EvolutionDefinition
        {
            Id = "corrupted_generic",
            FromStrayId = "*", // Special marker - applies to any Stray
            ToStrayId = "corrupted_form",
            Trigger = EvolutionTrigger.Corruption,
            RequiredStress = 90,
            RequiredAugmentation = "nimdok_cortex",
            StatMultipliers = new() { { "Attack", 1.6f }, { "Special", 1.6f }, { "MaxHp", 0.8f }, { "Defense", 0.85f } },
            UnlockedAbilities = new() { "corrupted_strike" },
            NewElement = Element.Corruption,
            NewColor = Color.DarkMagenta,
            IsCorruptedEvolution = true,
            Description = "Lazarus's corruption consumes the Stray..."
        });

        // Special: Bandit's final corrupted form (Act 3)
        Register(new EvolutionDefinition
        {
            Id = "bandit_hyper_evolved",
            FromStrayId = "companion_dog_evolved",
            ToStrayId = "bandit_corrupted",
            Trigger = EvolutionTrigger.Story,
            RequiredFlag = "gravitation_stage_4",
            StatMultipliers = new() { { "Attack", 2.0f }, { "Special", 2.0f }, { "Speed", 1.5f } },
            UnlockedAbilities = new() { "gravitation", "corrupted_strike", "sacrifice" },
            NewElement = Element.Corruption,
            NewColor = Color.Black,
            IsCorruptedEvolution = true,
            Description = "Bandit has become something terrifying... and sad."
        });
    }
}
