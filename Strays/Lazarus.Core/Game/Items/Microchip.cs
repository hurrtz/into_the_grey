using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Lazarus.Core.Game.Combat;
using Lazarus.Core.Game.Stats;

namespace Lazarus.Core.Game.Items;

/// <summary>
/// Categories of microchips based on their function.
/// </summary>
public enum MicrochipCategory
{
    /// <summary>
    /// Protocol chips grant new actions when ATB is full.
    /// </summary>
    Protocol,

    /// <summary>
    /// Element chips provide elemental damage and status effects.
    /// </summary>
    Element,

    /// <summary>
    /// Augment chips modify linked chips (FF7 Materia style).
    /// </summary>
    Augment,

    /// <summary>
    /// Driver chips provide passive stat bonuses.
    /// </summary>
    Driver,

    /// <summary>
    /// Daemon chips summon temporary entities (drones, turrets, zones).
    /// </summary>
    Daemon,

    /// <summary>
    /// Support chips provide healing, buffs, and ally enhancement.
    /// </summary>
    Support
}

/// <summary>
/// Subcategories for Protocol chips.
/// </summary>
public enum ProtocolSubcategory
{
    StanceGuard,
    Targeting,
    Mobility,
    Control,
    TeamUtility,
    Capture
}

/// <summary>
/// Element types for Element chips.
/// </summary>
public enum ChipElement
{
    None,
    Thermal,
    Cryo,
    Electric,
    EMP,
    Corrosive,
    Toxic,
    Sonic,
    Radiant,
    KineticImpulse,
    DataSignal
}

/// <summary>
/// Subcategories for Augment chips.
/// </summary>
public enum AugmentSubcategory
{
    TargetingCoverage,
    BehaviorModifier,
    Economy,
    StatusPayload,
    Sustain,
    Reaction,
    Utility
}

/// <summary>
/// Subcategories for Driver chips.
/// </summary>
public enum DriverSubcategory
{
    Resource,
    Tempo,
    Offense,
    Defense,
    Luck,
    RoleIdentity,
    SensorSuite
}

/// <summary>
/// Subcategories for Daemon chips.
/// </summary>
public enum DaemonSubcategory
{
    Drone,
    Turret,
    Swarm,
    Zone,
    Decoy
}

/// <summary>
/// Subcategories for Support chips.
/// </summary>
public enum SupportSubcategory
{
    Restoration,
    Barrier,
    Cleansing,
    Buff,
    TeamUtility
}

/// <summary>
/// Firmware/level of a microchip (Mk1 through Mk5).
/// </summary>
public enum FirmwareLevel
{
    Mk1 = 1,
    Mk2 = 2,
    Mk3 = 3,
    Mk4 = 4,
    Mk5 = 5
}

/// <summary>
/// Definition of a microchip type.
/// </summary>
public class MicrochipDefinition
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
    /// Chip category (Protocol, Element, Augment, Driver, Daemon, Support).
    /// </summary>
    public MicrochipCategory Category { get; init; } = MicrochipCategory.Driver;

    /// <summary>
    /// Rarity tier.
    /// </summary>
    public ItemRarity Rarity { get; init; } = ItemRarity.Common;

    /// <summary>
    /// Whether this is a unique chip (only one exists in the game).
    /// </summary>
    public bool IsUnique { get; init; } = false;

    /// <summary>
    /// Energy cost to use this chip's ability (for active chips).
    /// </summary>
    public int EnergyCost { get; init; } = 0;

    /// <summary>
    /// Heat generated when using this chip.
    /// </summary>
    public int HeatGenerated { get; init; } = 0;

    /// <summary>
    /// Maximum heat capacity for this chip before overheating.
    /// </summary>
    public int HeatMax { get; init; } = 100;

    /// <summary>
    /// Base heat dissipation rate (C₀ in the formula).
    /// </summary>
    public float HeatDissipationBase { get; init; } = 10f;

    /// <summary>
    /// Heat dissipation curve factor (α in the formula).
    /// </summary>
    public float HeatDissipationAlpha { get; init; } = 2f;

    /// <summary>
    /// TU thresholds for firmware levels [Mk1→Mk2, Mk2→Mk3, Mk3→Mk4, Mk4→Mk5].
    /// </summary>
    public int[] FirmwareTuThresholds { get; init; } = { 50, 125, 312, 781 };

    /// <summary>
    /// TU bonus per use of this chip.
    /// </summary>
    public int TuPerUse { get; init; } = 2;

    /// <summary>
    /// Ability ID granted by this chip (for ability-granting chips).
    /// </summary>
    public string? GrantsAbility { get; init; }

    /// <summary>
    /// Element type (for Element chips).
    /// </summary>
    public ChipElement Element { get; init; } = ChipElement.None;

    /// <summary>
    /// Stat bonuses provided (for Driver chips). Legacy - use StatModifiers for new code.
    /// </summary>
    public Dictionary<string, int> StatBonuses { get; init; } = new();

    /// <summary>
    /// Stat multipliers provided (for Driver chips). Legacy - use StatModifiers for new code.
    /// </summary>
    public Dictionary<string, float> StatMultipliers { get; init; } = new();

    /// <summary>
    /// Stat modifiers using the new comprehensive stat system.
    /// </summary>
    public List<StatModifier> StatModifiers { get; init; } = new();

    /// <summary>
    /// Whether this chip requires a linked socket to function (Augment chips).
    /// </summary>
    public bool RequiresLink { get; init; } = false;

    /// <summary>
    /// Augment effect type (for Augment chips).
    /// </summary>
    public string? AugmentEffect { get; init; }

    /// <summary>
    /// Minimum level to equip.
    /// </summary>
    public int MinLevel { get; init; } = 1;

    /// <summary>
    /// Placeholder color for UI.
    /// </summary>
    public Color PlaceholderColor { get; init; } = Color.Cyan;

    /// <summary>
    /// Gets the color associated with this rarity.
    /// </summary>
    public Color GetRarityColor() => Rarity switch
    {
        ItemRarity.Common => Color.White,
        ItemRarity.Uncommon => Color.LimeGreen,
        ItemRarity.Rare => Color.DodgerBlue,
        ItemRarity.Epic => Color.MediumPurple,
        ItemRarity.Legendary => Color.Gold,
        ItemRarity.Corrupted => Color.DarkMagenta,
        _ => Color.White
    };

    /// <summary>
    /// Gets TU required to reach a specific firmware level from Mk1.
    /// </summary>
    public int GetTuForLevel(FirmwareLevel level)
    {
        if (level == FirmwareLevel.Mk1) return 0;
        int total = 0;
        for (int i = 0; i < (int)level - 1 && i < FirmwareTuThresholds.Length; i++)
        {
            total += FirmwareTuThresholds[i];
        }
        return total;
    }

    /// <summary>
    /// Gets TU required to go from current level to next level.
    /// </summary>
    public int GetTuToNextLevel(FirmwareLevel currentLevel)
    {
        if (currentLevel >= FirmwareLevel.Mk5) return 0;
        int index = (int)currentLevel - 1;
        return index < FirmwareTuThresholds.Length ? FirmwareTuThresholds[index] : 0;
    }

    /// <summary>
    /// Gets the effect power scaling for a firmware level.
    /// </summary>
    public float GetFirmwareMultiplier(FirmwareLevel level) => level switch
    {
        FirmwareLevel.Mk1 => 1.0f,
        FirmwareLevel.Mk2 => 1.15f,
        FirmwareLevel.Mk3 => 1.35f,
        FirmwareLevel.Mk4 => 1.6f,
        FirmwareLevel.Mk5 => 2.0f,
        _ => 1.0f
    };
}

/// <summary>
/// An instance of a microchip with runtime state.
/// </summary>
public class Microchip
{
    private static int _nextInstanceId = 1;

    /// <summary>
    /// Unique instance ID.
    /// </summary>
    public string InstanceId { get; }

    /// <summary>
    /// The microchip definition.
    /// </summary>
    public MicrochipDefinition Definition { get; }

    /// <summary>
    /// Current firmware level.
    /// </summary>
    public FirmwareLevel FirmwareLevel { get; private set; } = FirmwareLevel.Mk1;

    /// <summary>
    /// Current TU progress toward next firmware level.
    /// </summary>
    public int CurrentTu { get; private set; } = 0;

    /// <summary>
    /// Current heat level (0 to HeatMax).
    /// </summary>
    public float CurrentHeat { get; private set; } = 0f;

    /// <summary>
    /// Whether this chip is overheated (cannot be used until heat drops below 100%).
    /// </summary>
    public bool IsOverheated => CurrentHeat >= Definition.HeatMax;

    /// <summary>
    /// Heat percentage (0-1).
    /// </summary>
    public float HeatPercent => Definition.HeatMax > 0 ? CurrentHeat / Definition.HeatMax : 0f;

    /// <summary>
    /// Whether this chip is equipped to a Stray.
    /// </summary>
    public bool IsEquipped { get; set; } = false;

    /// <summary>
    /// ID of the Stray this is equipped to (if any).
    /// </summary>
    public string? EquippedToStrayId { get; set; }

    /// <summary>
    /// Socket index this chip is in (-1 if not in socket).
    /// </summary>
    public int SocketIndex { get; set; } = -1;

    public Microchip(MicrochipDefinition definition)
    {
        InstanceId = $"chip_{_nextInstanceId++}";
        Definition = definition;
    }

    /// <summary>
    /// Adds heat when the chip is used.
    /// </summary>
    public void AddHeat()
    {
        CurrentHeat = Math.Min(CurrentHeat + Definition.HeatGenerated, Definition.HeatMax * 1.5f);
    }

    /// <summary>
    /// Dissipates heat based on the quadratic formula.
    /// Called per ATB tick.
    /// cool(H) = C₀ / (1 + α × (H / H_max)²)
    /// </summary>
    public void DissipateHeat()
    {
        if (CurrentHeat <= 0) return;

        float hMax = Definition.HeatMax;
        float c0 = Definition.HeatDissipationBase;
        float alpha = Definition.HeatDissipationAlpha;

        float hRatio = CurrentHeat / hMax;
        float coolAmount = c0 / (1f + alpha * hRatio * hRatio);

        CurrentHeat = Math.Max(0f, CurrentHeat - coolAmount);
    }

    /// <summary>
    /// Resets heat to zero (used for combat start).
    /// </summary>
    public void ResetHeat()
    {
        CurrentHeat = 0f;
    }

    /// <summary>
    /// Adds TU from using the chip.
    /// </summary>
    public bool AddUseTu()
    {
        return AddTu(Definition.TuPerUse);
    }

    /// <summary>
    /// Adds TU from battle rewards.
    /// </summary>
    /// <param name="amount">TU to add.</param>
    /// <returns>True if firmware level increased.</returns>
    public bool AddTu(int amount)
    {
        if (FirmwareLevel >= FirmwareLevel.Mk5) return false;

        CurrentTu += amount;

        int tuNeeded = Definition.GetTuToNextLevel(FirmwareLevel);
        if (tuNeeded > 0 && CurrentTu >= tuNeeded)
        {
            CurrentTu -= tuNeeded;
            FirmwareLevel = (FirmwareLevel)((int)FirmwareLevel + 1);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the current effect multiplier based on firmware level.
    /// </summary>
    public float GetEffectMultiplier()
    {
        return Definition.GetFirmwareMultiplier(FirmwareLevel);
    }

    /// <summary>
    /// Gets the stat bonus from this chip, scaled by firmware.
    /// </summary>
    public int GetStatBonus(string stat)
    {
        if (!Definition.StatBonuses.TryGetValue(stat, out int bonus))
            return 0;

        return (int)(bonus * GetEffectMultiplier());
    }

    /// <summary>
    /// Gets the stat multiplier from this chip.
    /// </summary>
    public float GetStatMultiplier(string stat)
    {
        return Definition.StatMultipliers.TryGetValue(stat, out float mult) ? mult : 1f;
    }

    /// <summary>
    /// Checks if this chip can be used (not overheated).
    /// </summary>
    public bool CanUse()
    {
        return !IsOverheated;
    }

    /// <summary>
    /// Checks if a Stray has enough energy to use this chip.
    /// </summary>
    public bool HasEnoughEnergy(int currentEnergy)
    {
        return currentEnergy >= Definition.EnergyCost;
    }

    /// <summary>
    /// Creates save data for this chip.
    /// </summary>
    public MicrochipSaveData ToSaveData()
    {
        return new MicrochipSaveData
        {
            InstanceId = InstanceId,
            DefinitionId = Definition.Id,
            FirmwareLevel = (int)FirmwareLevel,
            CurrentTu = CurrentTu,
            EquippedToStrayId = EquippedToStrayId,
            SocketIndex = SocketIndex
        };
    }

    /// <summary>
    /// Creates a chip from save data.
    /// </summary>
    public static Microchip? FromSaveData(MicrochipSaveData data)
    {
        var definition = Microchips.Get(data.DefinitionId);
        if (definition == null) return null;

        var chip = new Microchip(definition)
        {
            FirmwareLevel = (FirmwareLevel)data.FirmwareLevel,
            CurrentTu = data.CurrentTu,
            EquippedToStrayId = data.EquippedToStrayId,
            SocketIndex = data.SocketIndex,
            IsEquipped = data.EquippedToStrayId != null
        };

        return chip;
    }
}

/// <summary>
/// Save data for a microchip instance.
/// </summary>
public class MicrochipSaveData
{
    public string InstanceId { get; set; } = "";
    public string DefinitionId { get; set; } = "";
    public int FirmwareLevel { get; set; } = 1;
    public int CurrentTu { get; set; } = 0;
    public string? EquippedToStrayId { get; set; }
    public int SocketIndex { get; set; } = -1;
}

/// <summary>
/// Represents a microchip socket on a Stray.
/// </summary>
public class MicrochipSocket
{
    /// <summary>
    /// Index of this socket (0-based).
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Index of the linked socket (-1 if not linked).
    /// </summary>
    public int LinkedSocketIndex { get; init; } = -1;

    /// <summary>
    /// Whether this socket is linked to another.
    /// </summary>
    public bool IsLinked => LinkedSocketIndex >= 0;

    /// <summary>
    /// The chip currently in this socket (null if empty).
    /// </summary>
    public Microchip? EquippedChip { get; set; }

    /// <summary>
    /// Whether this socket is empty.
    /// </summary>
    public bool IsEmpty => EquippedChip == null;
}

/// <summary>
/// Configuration for microchip sockets at a specific evolution stage.
/// </summary>
public class SocketConfiguration
{
    /// <summary>
    /// Total number of sockets.
    /// </summary>
    public int SocketCount { get; init; } = 3;

    /// <summary>
    /// Pairs of linked socket indices.
    /// Each pair is [socketA, socketB].
    /// </summary>
    public List<int[]> LinkedPairs { get; init; } = new() { new[] { 0, 1 } };

    /// <summary>
    /// Creates the default starting configuration (3 slots, 2 linked).
    /// </summary>
    public static SocketConfiguration Default => new()
    {
        SocketCount = 3,
        LinkedPairs = new List<int[]> { new[] { 0, 1 } }
    };

    /// <summary>
    /// Creates sockets from this configuration.
    /// </summary>
    public MicrochipSocket[] CreateSockets()
    {
        var sockets = new MicrochipSocket[SocketCount];

        // Initialize all sockets
        for (int i = 0; i < SocketCount; i++)
        {
            sockets[i] = new MicrochipSocket { Index = i, LinkedSocketIndex = -1 };
        }

        // Set up links
        foreach (var pair in LinkedPairs)
        {
            if (pair.Length >= 2 && pair[0] < SocketCount && pair[1] < SocketCount)
            {
                sockets[pair[0]] = new MicrochipSocket { Index = pair[0], LinkedSocketIndex = pair[1] };
                sockets[pair[1]] = new MicrochipSocket { Index = pair[1], LinkedSocketIndex = pair[0] };
            }
        }

        return sockets;
    }
}

/// <summary>
/// Static registry of all microchip definitions.
/// </summary>
public static class Microchips
{
    private static readonly Dictionary<string, MicrochipDefinition> _chips = new();

    /// <summary>
    /// All registered microchips.
    /// </summary>
    public static IReadOnlyDictionary<string, MicrochipDefinition> All => _chips;

    /// <summary>
    /// Gets a microchip by ID.
    /// </summary>
    public static MicrochipDefinition? Get(string id) =>
        _chips.TryGetValue(id, out var chip) ? chip : null;

    /// <summary>
    /// Gets all microchips of a specific category.
    /// </summary>
    public static IEnumerable<MicrochipDefinition> GetByCategory(MicrochipCategory category) =>
        _chips.Values.Where(c => c.Category == category);

    /// <summary>
    /// Gets all microchips of a specific rarity.
    /// </summary>
    public static IEnumerable<MicrochipDefinition> GetByRarity(ItemRarity rarity) =>
        _chips.Values.Where(c => c.Rarity == rarity);

    /// <summary>
    /// Gets all microchips of a specific element.
    /// </summary>
    public static IEnumerable<MicrochipDefinition> GetByElement(ChipElement element) =>
        _chips.Values.Where(c => c.Element == element);

    /// <summary>
    /// Registers a microchip.
    /// </summary>
    public static void Register(MicrochipDefinition chip)
    {
        _chips[chip.Id] = chip;
    }

    static Microchips()
    {
        RegisterProtocolChips();
        RegisterElementChips();
        RegisterAugmentChips();
        RegisterDriverChips();
        RegisterDaemonChips();
        RegisterSupportChips();
        RegisterUniqueChips();
    }

    private static void RegisterProtocolChips()
    {
        // Stance/Guard
        Register(new MicrochipDefinition
        {
            Id = "proto_guard_stance",
            Name = "Guard Stance",
            Description = "Take a defensive stance, reducing incoming damage by 50% until next turn.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Common,
            EnergyCost = 15,
            HeatGenerated = 10,
            HeatMax = 80,
            GrantsAbility = "guard_stance",
            PlaceholderColor = Color.SteelBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "proto_brace",
            Name = "Brace",
            Description = "Brace for impact. Nullifies the next attack completely.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 25,
            HeatGenerated = 25,
            HeatMax = 100,
            GrantsAbility = "brace",
            PlaceholderColor = Color.Gray
        });

        Register(new MicrochipDefinition
        {
            Id = "proto_evasive_stance",
            Name = "Evasive Stance",
            Description = "Increase evasion rate significantly until next action.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Common,
            EnergyCost = 20,
            HeatGenerated = 15,
            HeatMax = 80,
            GrantsAbility = "evasive_stance",
            PlaceholderColor = Color.LightGray
        });

        // Targeting
        Register(new MicrochipDefinition
        {
            Id = "proto_mark_target",
            Name = "Mark Target",
            Description = "Mark an enemy, increasing all damage dealt to them by 25%.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Common,
            EnergyCost = 10,
            HeatGenerated = 8,
            HeatMax = 60,
            GrantsAbility = "mark_target",
            PlaceholderColor = Color.Red
        });

        Register(new MicrochipDefinition
        {
            Id = "proto_scan",
            Name = "Scan Weakness",
            Description = "Reveal enemy stats and elemental weaknesses.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Common,
            EnergyCost = 8,
            HeatGenerated = 5,
            HeatMax = 50,
            GrantsAbility = "scan_weakness",
            PlaceholderColor = Color.Cyan
        });

        Register(new MicrochipDefinition
        {
            Id = "proto_taunt",
            Name = "Threat Taunt",
            Description = "Force enemies to target you for 2 turns.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 20,
            HeatGenerated = 18,
            HeatMax = 80,
            GrantsAbility = "taunt",
            PlaceholderColor = Color.OrangeRed
        });

        // Mobility
        Register(new MicrochipDefinition
        {
            Id = "proto_lunge",
            Name = "Lunge",
            Description = "Rush forward with a quick strike. High priority.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Common,
            EnergyCost = 15,
            HeatGenerated = 12,
            HeatMax = 70,
            GrantsAbility = "lunge",
            PlaceholderColor = Color.LimeGreen
        });

        Register(new MicrochipDefinition
        {
            Id = "proto_reposition",
            Name = "Reposition Step",
            Description = "Move to avoid attacks and gain ATB.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 12,
            HeatGenerated = 10,
            HeatMax = 60,
            GrantsAbility = "reposition",
            PlaceholderColor = Color.Aqua
        });

        // Control
        Register(new MicrochipDefinition
        {
            Id = "proto_tripline",
            Name = "Tripline Toss",
            Description = "Throw a tripline to immobilize an enemy for 1 turn.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 22,
            HeatGenerated = 20,
            HeatMax = 90,
            GrantsAbility = "tripline",
            PlaceholderColor = Color.Brown
        });

        Register(new MicrochipDefinition
        {
            Id = "proto_knockback",
            Name = "Knockback Shove",
            Description = "Push an enemy back, delaying their ATB.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Common,
            EnergyCost = 18,
            HeatGenerated = 15,
            HeatMax = 80,
            GrantsAbility = "knockback",
            PlaceholderColor = Color.DarkOrange
        });

        // Team Utility
        Register(new MicrochipDefinition
        {
            Id = "proto_cover_ally",
            Name = "Cover Ally",
            Description = "Take damage in place of a targeted ally.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 15,
            HeatGenerated = 12,
            HeatMax = 70,
            GrantsAbility = "cover_ally",
            PlaceholderColor = Color.Blue
        });

        Register(new MicrochipDefinition
        {
            Id = "proto_battery_transfer",
            Name = "Battery Transfer",
            Description = "Transfer 30 energy to an ally.",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Rare,
            EnergyCost = 40,
            HeatGenerated = 15,
            HeatMax = 80,
            GrantsAbility = "battery_transfer",
            PlaceholderColor = Color.Yellow
        });
    }

    private static void RegisterElementChips()
    {
        // Thermal
        Register(new MicrochipDefinition
        {
            Id = "elem_ember",
            Name = "Ember Burst",
            Description = "Launch a burst of thermal energy. May cause Burn.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Common,
            Element = ChipElement.Thermal,
            EnergyCost = 18,
            HeatGenerated = 20,
            HeatMax = 100,
            GrantsAbility = "ember_burst",
            PlaceholderColor = Color.Orange
        });

        Register(new MicrochipDefinition
        {
            Id = "elem_inferno",
            Name = "Inferno",
            Description = "Engulf the target in flames. High damage and guaranteed Burn.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Rare,
            Element = ChipElement.Thermal,
            EnergyCost = 35,
            HeatGenerated = 40,
            HeatMax = 120,
            GrantsAbility = "inferno",
            MinLevel = 10,
            PlaceholderColor = Color.OrangeRed
        });

        // Cryo
        Register(new MicrochipDefinition
        {
            Id = "elem_frost",
            Name = "Frost Spike",
            Description = "Pierce with ice. May Slow the target.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Common,
            Element = ChipElement.Cryo,
            EnergyCost = 18,
            HeatGenerated = 18,
            HeatMax = 100,
            GrantsAbility = "frost_spike",
            PlaceholderColor = Color.LightBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "elem_flash_freeze",
            Name = "Flash Freeze",
            Description = "Instantly freeze the target. May cause Frozen status.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Rare,
            Element = ChipElement.Cryo,
            EnergyCost = 32,
            HeatGenerated = 35,
            HeatMax = 110,
            GrantsAbility = "flash_freeze",
            MinLevel = 10,
            PlaceholderColor = Color.CornflowerBlue
        });

        // Electric
        Register(new MicrochipDefinition
        {
            Id = "elem_shock",
            Name = "Shock Pulse",
            Description = "Release an electric pulse. May cause Shock.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Common,
            Element = ChipElement.Electric,
            EnergyCost = 16,
            HeatGenerated = 15,
            HeatMax = 90,
            GrantsAbility = "shock_pulse",
            PlaceholderColor = Color.Yellow
        });

        Register(new MicrochipDefinition
        {
            Id = "elem_thunderbolt",
            Name = "Thunderbolt",
            Description = "Call down lightning. High damage with chain potential.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Rare,
            Element = ChipElement.Electric,
            EnergyCost = 38,
            HeatGenerated = 42,
            HeatMax = 120,
            GrantsAbility = "thunderbolt",
            MinLevel = 12,
            PlaceholderColor = Color.Gold
        });

        // EMP
        Register(new MicrochipDefinition
        {
            Id = "elem_emp_pulse",
            Name = "EMP Pulse",
            Description = "Disable electronic systems. Locks random chips.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Uncommon,
            Element = ChipElement.EMP,
            EnergyCost = 25,
            HeatGenerated = 25,
            HeatMax = 100,
            GrantsAbility = "emp_pulse",
            PlaceholderColor = Color.MediumPurple
        });

        // Corrosive
        Register(new MicrochipDefinition
        {
            Id = "elem_acid_spray",
            Name = "Acid Spray",
            Description = "Spray corrosive acid. Reduces target's defense.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Common,
            Element = ChipElement.Corrosive,
            EnergyCost = 20,
            HeatGenerated = 18,
            HeatMax = 90,
            GrantsAbility = "acid_spray",
            PlaceholderColor = Color.GreenYellow
        });

        // Toxic
        Register(new MicrochipDefinition
        {
            Id = "elem_toxic_cloud",
            Name = "Toxic Cloud",
            Description = "Release a cloud of toxins. Applies Poison DoT.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Common,
            Element = ChipElement.Toxic,
            EnergyCost = 22,
            HeatGenerated = 20,
            HeatMax = 100,
            GrantsAbility = "toxic_cloud",
            PlaceholderColor = Color.Purple
        });

        // Sonic
        Register(new MicrochipDefinition
        {
            Id = "elem_sonic_blast",
            Name = "Sonic Blast",
            Description = "Emit a devastating sonic wave. May Stagger.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Uncommon,
            Element = ChipElement.Sonic,
            EnergyCost = 24,
            HeatGenerated = 22,
            HeatMax = 100,
            GrantsAbility = "sonic_blast",
            PlaceholderColor = Color.White
        });

        // Radiant
        Register(new MicrochipDefinition
        {
            Id = "elem_laser_beam",
            Name = "Laser Beam",
            Description = "Fire a precision laser. High accuracy, ignores evasion.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Uncommon,
            Element = ChipElement.Radiant,
            EnergyCost = 26,
            HeatGenerated = 28,
            HeatMax = 110,
            GrantsAbility = "laser_beam",
            PlaceholderColor = Color.Red
        });

        // Kinetic-Impulse
        Register(new MicrochipDefinition
        {
            Id = "elem_force_wave",
            Name = "Force Wave",
            Description = "Release a kinetic shockwave. Knockback effect.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Uncommon,
            Element = ChipElement.KineticImpulse,
            EnergyCost = 22,
            HeatGenerated = 20,
            HeatMax = 95,
            GrantsAbility = "force_wave",
            PlaceholderColor = Color.DarkGray
        });

        // Data/Signal
        Register(new MicrochipDefinition
        {
            Id = "elem_data_spike",
            Name = "Data Spike",
            Description = "Inject corrupted data. Hacking damage to machines.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Uncommon,
            Element = ChipElement.DataSignal,
            EnergyCost = 20,
            HeatGenerated = 18,
            HeatMax = 90,
            GrantsAbility = "data_spike",
            PlaceholderColor = Color.Cyan
        });

        Register(new MicrochipDefinition
        {
            Id = "elem_signal_jam",
            Name = "Signal Jam",
            Description = "Jam enemy communications. Disrupts coordination.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Rare,
            Element = ChipElement.DataSignal,
            EnergyCost = 28,
            HeatGenerated = 25,
            HeatMax = 100,
            GrantsAbility = "signal_jam",
            MinLevel = 8,
            PlaceholderColor = Color.DarkCyan
        });
    }

    private static void RegisterAugmentChips()
    {
        // Targeting & Coverage
        Register(new MicrochipDefinition
        {
            Id = "aug_multi_target",
            Name = "Multi-Target",
            Description = "When linked, allows abilities to hit all enemies.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Uncommon,
            RequiresLink = true,
            AugmentEffect = "multi_target",
            EnergyCost = 0, // Adds to linked chip's cost
            HeatGenerated = 15, // Extra heat
            HeatMax = 80,
            PlaceholderColor = Color.LightSalmon
        });

        Register(new MicrochipDefinition
        {
            Id = "aug_splash",
            Name = "Splash",
            Description = "When linked, abilities deal 50% damage to adjacent targets.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Common,
            RequiresLink = true,
            AugmentEffect = "splash",
            HeatGenerated = 10,
            HeatMax = 70,
            PlaceholderColor = Color.Coral
        });

        Register(new MicrochipDefinition
        {
            Id = "aug_chain",
            Name = "Chain",
            Description = "When linked, abilities bounce to 2 additional targets at reduced power.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Rare,
            RequiresLink = true,
            AugmentEffect = "chain",
            HeatGenerated = 18,
            HeatMax = 90,
            PlaceholderColor = Color.MediumVioletRed
        });

        // Behavior Modifiers
        Register(new MicrochipDefinition
        {
            Id = "aug_piercing",
            Name = "Piercing",
            Description = "When linked, abilities ignore 50% of target's defense.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Uncommon,
            RequiresLink = true,
            AugmentEffect = "piercing",
            HeatGenerated = 12,
            HeatMax = 80,
            PlaceholderColor = Color.Silver
        });

        Register(new MicrochipDefinition
        {
            Id = "aug_homing",
            Name = "Homing",
            Description = "When linked, abilities cannot miss.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Uncommon,
            RequiresLink = true,
            AugmentEffect = "homing",
            HeatGenerated = 8,
            HeatMax = 70,
            PlaceholderColor = Color.Lime
        });

        // Economy
        Register(new MicrochipDefinition
        {
            Id = "aug_en_efficiency",
            Name = "EN Efficiency",
            Description = "When linked, reduces energy cost of abilities by 25%.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Uncommon,
            RequiresLink = true,
            AugmentEffect = "en_efficiency",
            HeatGenerated = 5,
            HeatMax = 60,
            PlaceholderColor = Color.LightGreen
        });

        Register(new MicrochipDefinition
        {
            Id = "aug_heat_vent",
            Name = "Heat Vent",
            Description = "When linked, reduces heat generated by 30%.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Uncommon,
            RequiresLink = true,
            AugmentEffect = "heat_vent",
            HeatGenerated = 0,
            HeatMax = 50,
            PlaceholderColor = Color.SkyBlue
        });

        // Status Payload
        Register(new MicrochipDefinition
        {
            Id = "aug_burn_payload",
            Name = "Burn Payload",
            Description = "When linked, adds Burn chance to abilities.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Common,
            RequiresLink = true,
            AugmentEffect = "burn_payload",
            HeatGenerated = 10,
            HeatMax = 75,
            PlaceholderColor = Color.OrangeRed
        });

        Register(new MicrochipDefinition
        {
            Id = "aug_slow_payload",
            Name = "Slow Payload",
            Description = "When linked, adds Slow chance to abilities.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Common,
            RequiresLink = true,
            AugmentEffect = "slow_payload",
            HeatGenerated = 10,
            HeatMax = 75,
            PlaceholderColor = Color.LightBlue
        });

        // Sustain
        Register(new MicrochipDefinition
        {
            Id = "aug_drain",
            Name = "Drain",
            Description = "When linked, heal for 20% of damage dealt.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Rare,
            RequiresLink = true,
            AugmentEffect = "drain",
            HeatGenerated = 15,
            HeatMax = 90,
            PlaceholderColor = Color.DarkRed
        });

        Register(new MicrochipDefinition
        {
            Id = "aug_battery_leech",
            Name = "Battery Leech",
            Description = "When linked, restore 10 EN on successful hit.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Rare,
            RequiresLink = true,
            AugmentEffect = "battery_leech",
            HeatGenerated = 12,
            HeatMax = 85,
            PlaceholderColor = Color.Gold
        });

        // Reactions
        Register(new MicrochipDefinition
        {
            Id = "aug_counter",
            Name = "Counter",
            Description = "When linked, auto-cast the ability when hit.",
            Category = MicrochipCategory.Augment,
            Rarity = ItemRarity.Rare,
            RequiresLink = true,
            AugmentEffect = "counter",
            HeatGenerated = 20,
            HeatMax = 100,
            PlaceholderColor = Color.Crimson
        });
    }

    private static void RegisterDriverChips()
    {
        // Resource
        Register(new MicrochipDefinition
        {
            Id = "drv_en_max_1",
            Name = "Energy Capacitor I",
            Description = "Increases maximum energy by 30.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "MaxEnergy", 30 } },
            PlaceholderColor = Color.Yellow
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_en_max_2",
            Name = "Energy Capacitor II",
            Description = "Increases maximum energy by 60.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "MaxEnergy", 60 } },
            MinLevel = 8,
            PlaceholderColor = Color.Gold
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_en_regen_1",
            Name = "Solar Converter I",
            Description = "Increases energy regeneration by 3 per ATB tick.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "EnergyRegen", 3 } },
            PlaceholderColor = Color.LightYellow
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_en_regen_2",
            Name = "Solar Converter II",
            Description = "Increases energy regeneration by 6 per ATB tick.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "EnergyRegen", 6 } },
            MinLevel = 10,
            PlaceholderColor = Color.Orange
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_heat_dissipation",
            Name = "Coolant System",
            Description = "Increases base heat dissipation rate by 25%.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatMultipliers = new() { { "HeatDissipation", 1.25f } },
            PlaceholderColor = Color.SkyBlue
        });

        // Tempo
        Register(new MicrochipDefinition
        {
            Id = "drv_speed_1",
            Name = "Accelerator I",
            Description = "Increases Speed by 8.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Speed", 8 } },
            PlaceholderColor = Color.LightGreen
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_speed_2",
            Name = "Accelerator II",
            Description = "Increases Speed by 15.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "Speed", 15 } },
            MinLevel = 8,
            PlaceholderColor = Color.Green
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_first_strike",
            Name = "First Strike",
            Description = "Start combat with 50% ATB filled.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "InitialAtb", 50 } },
            PlaceholderColor = Color.Turquoise
        });

        // Offense
        Register(new MicrochipDefinition
        {
            Id = "drv_attack_1",
            Name = "Power Core I",
            Description = "Increases Attack by 8.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Attack", 8 } },
            PlaceholderColor = Color.Red
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_attack_2",
            Name = "Power Core II",
            Description = "Increases Attack by 15.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "Attack", 15 } },
            MinLevel = 8,
            PlaceholderColor = Color.DarkRed
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_special_1",
            Name = "Amplifier I",
            Description = "Increases Special by 8.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Special", 8 } },
            PlaceholderColor = Color.Purple
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_crit_chance",
            Name = "Precision Module",
            Description = "Increases critical hit chance by 10%.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "CritChance", 10 } },
            PlaceholderColor = Color.Crimson
        });

        // Defense
        Register(new MicrochipDefinition
        {
            Id = "drv_defense_1",
            Name = "Armor Plate I",
            Description = "Increases Defense by 8.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Defense", 8 } },
            PlaceholderColor = Color.SlateGray
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_defense_2",
            Name = "Armor Plate II",
            Description = "Increases Defense by 15.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "Defense", 15 } },
            MinLevel = 8,
            PlaceholderColor = Color.DarkSlateGray
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_hp_1",
            Name = "Vitality Core I",
            Description = "Increases maximum HP by 15.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "MaxHp", 15 } },
            PlaceholderColor = Color.Pink
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_hp_2",
            Name = "Vitality Core II",
            Description = "Increases maximum HP by 35.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "MaxHp", 35 } },
            MinLevel = 8,
            PlaceholderColor = Color.HotPink
        });

        // Elemental Resistance
        Register(new MicrochipDefinition
        {
            Id = "drv_thermal_resist",
            Name = "Heat Shield",
            Description = "Reduces Thermal damage by 25%.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "ThermalResist", 25 } },
            PlaceholderColor = Color.OrangeRed
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_cryo_resist",
            Name = "Antifreeze Module",
            Description = "Reduces Cryo damage by 25%.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "CryoResist", 25 } },
            PlaceholderColor = Color.LightBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "drv_electric_resist",
            Name = "Insulation Layer",
            Description = "Reduces Electric damage by 25%.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "ElectricResist", 25 } },
            PlaceholderColor = Color.Yellow
        });
    }

    private static void RegisterDaemonChips()
    {
        // Drones
        Register(new MicrochipDefinition
        {
            Id = "daemon_scout_drone",
            Name = "Scout Drone",
            Description = "Deploy a drone that reveals enemy stats and increases accuracy.",
            Category = MicrochipCategory.Daemon,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 30,
            HeatGenerated = 35,
            HeatMax = 120,
            GrantsAbility = "scout_drone",
            PlaceholderColor = Color.LightGray
        });

        Register(new MicrochipDefinition
        {
            Id = "daemon_shield_drone",
            Name = "Shield Drone",
            Description = "Deploy a drone that provides a protective barrier.",
            Category = MicrochipCategory.Daemon,
            Rarity = ItemRarity.Rare,
            EnergyCost = 40,
            HeatGenerated = 45,
            HeatMax = 130,
            GrantsAbility = "shield_drone",
            MinLevel = 10,
            PlaceholderColor = Color.SteelBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "daemon_attack_drone",
            Name = "Hound Drone",
            Description = "Deploy an aggressive drone that attacks each turn.",
            Category = MicrochipCategory.Daemon,
            Rarity = ItemRarity.Rare,
            EnergyCost = 45,
            HeatGenerated = 50,
            HeatMax = 140,
            GrantsAbility = "attack_drone",
            MinLevel = 12,
            PlaceholderColor = Color.DarkRed
        });

        // Turrets
        Register(new MicrochipDefinition
        {
            Id = "daemon_laser_turret",
            Name = "Laser Turret",
            Description = "Deploy a turret that fires radiant beams at enemies.",
            Category = MicrochipCategory.Daemon,
            Rarity = ItemRarity.Rare,
            EnergyCost = 50,
            HeatGenerated = 55,
            HeatMax = 150,
            GrantsAbility = "laser_turret",
            MinLevel = 14,
            PlaceholderColor = Color.Red
        });

        Register(new MicrochipDefinition
        {
            Id = "daemon_shock_turret",
            Name = "Shock Turret",
            Description = "Deploy a turret that chains electric damage between enemies.",
            Category = MicrochipCategory.Daemon,
            Rarity = ItemRarity.Rare,
            EnergyCost = 48,
            HeatGenerated = 52,
            HeatMax = 145,
            GrantsAbility = "shock_turret",
            MinLevel = 14,
            PlaceholderColor = Color.Yellow
        });

        // Zones
        Register(new MicrochipDefinition
        {
            Id = "daemon_healing_field",
            Name = "Repair Field",
            Description = "Deploy a zone that heals allies within it each turn.",
            Category = MicrochipCategory.Daemon,
            Rarity = ItemRarity.Rare,
            EnergyCost = 55,
            HeatGenerated = 45,
            HeatMax = 140,
            GrantsAbility = "healing_field",
            MinLevel = 12,
            PlaceholderColor = Color.LimeGreen
        });

        Register(new MicrochipDefinition
        {
            Id = "daemon_stun_field",
            Name = "Stun Field",
            Description = "Deploy a zone that may stun enemies who act within it.",
            Category = MicrochipCategory.Daemon,
            Rarity = ItemRarity.Epic,
            EnergyCost = 60,
            HeatGenerated = 60,
            HeatMax = 160,
            GrantsAbility = "stun_field",
            MinLevel = 16,
            PlaceholderColor = Color.MediumPurple
        });

        // Decoys
        Register(new MicrochipDefinition
        {
            Id = "daemon_holo_decoy",
            Name = "Holo-Decoy",
            Description = "Deploy a holographic decoy that draws enemy attacks.",
            Category = MicrochipCategory.Daemon,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 25,
            HeatGenerated = 30,
            HeatMax = 100,
            GrantsAbility = "holo_decoy",
            PlaceholderColor = Color.Cyan
        });
    }

    private static void RegisterSupportChips()
    {
        // Restoration
        Register(new MicrochipDefinition
        {
            Id = "sup_repair_pulse",
            Name = "Repair Pulse",
            Description = "Restore HP to one ally.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Common,
            EnergyCost = 20,
            HeatGenerated = 12,
            HeatMax = 80,
            GrantsAbility = "repair_pulse",
            PlaceholderColor = Color.LimeGreen
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_mass_repair",
            Name = "Repair Wave",
            Description = "Restore HP to all allies.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Rare,
            EnergyCost = 45,
            HeatGenerated = 35,
            HeatMax = 120,
            GrantsAbility = "repair_wave",
            MinLevel = 12,
            PlaceholderColor = Color.Green
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_regen_field",
            Name = "Regeneration Field",
            Description = "Apply regeneration to an ally for 3 turns.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 28,
            HeatGenerated = 20,
            HeatMax = 90,
            GrantsAbility = "regen_field",
            PlaceholderColor = Color.PaleGreen
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_emergency_reboot",
            Name = "Emergency Reboot",
            Description = "Revive a fallen ally with 30% HP.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Rare,
            EnergyCost = 60,
            HeatGenerated = 50,
            HeatMax = 150,
            GrantsAbility = "emergency_reboot",
            MinLevel = 10,
            PlaceholderColor = Color.Gold
        });

        // Barriers
        Register(new MicrochipDefinition
        {
            Id = "sup_shield_coat",
            Name = "Shield Coat",
            Description = "Apply a damage-absorbing shield to an ally.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Common,
            EnergyCost = 22,
            HeatGenerated = 15,
            HeatMax = 85,
            GrantsAbility = "shield_coat",
            PlaceholderColor = Color.SteelBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_signal_screen",
            Name = "Signal Screen",
            Description = "Block Data/EMP attacks for 2 turns.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 25,
            HeatGenerated = 18,
            HeatMax = 90,
            GrantsAbility = "signal_screen",
            PlaceholderColor = Color.DarkCyan
        });

        // Cleansing
        Register(new MicrochipDefinition
        {
            Id = "sup_purge_toxin",
            Name = "Purge Toxin",
            Description = "Remove poison and toxic status from an ally.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Common,
            EnergyCost = 15,
            HeatGenerated = 10,
            HeatMax = 70,
            GrantsAbility = "purge_toxin",
            PlaceholderColor = Color.MediumPurple
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_coolant_flush",
            Name = "Coolant Flush",
            Description = "Clear overheat status from an ally's chips.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 18,
            HeatGenerated = 5,
            HeatMax = 60,
            GrantsAbility = "coolant_flush",
            PlaceholderColor = Color.LightBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_system_wipe",
            Name = "System Wipe",
            Description = "Remove all debuffs from an ally.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Rare,
            EnergyCost = 35,
            HeatGenerated = 30,
            HeatMax = 110,
            GrantsAbility = "system_wipe",
            MinLevel = 8,
            PlaceholderColor = Color.White
        });

        // Buffs
        Register(new MicrochipDefinition
        {
            Id = "sup_overclock",
            Name = "Overclock",
            Description = "Boost an ally's ATB fill rate for 3 turns.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 25,
            HeatGenerated = 20,
            HeatMax = 90,
            GrantsAbility = "overclock",
            PlaceholderColor = Color.Orange
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_damage_amp",
            Name = "Damage Amplifier",
            Description = "Increase an ally's damage output for 3 turns.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 28,
            HeatGenerated = 22,
            HeatMax = 95,
            GrantsAbility = "damage_amp",
            PlaceholderColor = Color.Red
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_armor_harden",
            Name = "Armor Harden",
            Description = "Increase an ally's defense for 3 turns.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 24,
            HeatGenerated = 18,
            HeatMax = 85,
            GrantsAbility = "armor_harden",
            PlaceholderColor = Color.Gray
        });

        // Team Utility
        Register(new MicrochipDefinition
        {
            Id = "sup_rally_pulse",
            Name = "Rally Pulse",
            Description = "Minor heal and cleanse to all allies.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Rare,
            EnergyCost = 40,
            HeatGenerated = 32,
            HeatMax = 115,
            GrantsAbility = "rally_pulse",
            MinLevel = 10,
            PlaceholderColor = Color.Gold
        });

        Register(new MicrochipDefinition
        {
            Id = "sup_battery_share",
            Name = "Battery Share",
            Description = "Transfer 25 EN to an ally.",
            Category = MicrochipCategory.Support,
            Rarity = ItemRarity.Uncommon,
            EnergyCost = 35,
            HeatGenerated = 15,
            HeatMax = 80,
            GrantsAbility = "battery_share",
            PlaceholderColor = Color.Yellow
        });
    }

    private static void RegisterUniqueChips()
    {
        // Bandit's Gravitation chip - critical to the story
        Register(new MicrochipDefinition
        {
            Id = "gravitation",
            Name = "Gravitation",
            Description = "Bandit's unique chip. Manipulates gravitational fields. The chip's degradation drives the story.",
            Category = MicrochipCategory.Element,
            Rarity = ItemRarity.Legendary,
            IsUnique = true,
            Element = ChipElement.KineticImpulse,
            EnergyCost = 35,
            HeatGenerated = 30,
            HeatMax = 100,
            GrantsAbility = "gravitation",
            PlaceholderColor = Color.MediumPurple
        });

        // Lazarus-related unique chips
        Register(new MicrochipDefinition
        {
            Id = "boost_control_fragment",
            Name = "Boost Control Fragment",
            Description = "A fragment of Lazarus's Boost Control System. Highly dangerous.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Legendary,
            IsUnique = true,
            StatMultipliers = new() { { "Special", 1.5f }, { "MaxEnergy", 1.3f } },
            StatBonuses = new() { { "EnergyRegen", 5 } },
            MinLevel = 20,
            PlaceholderColor = Color.DarkMagenta
        });

        Register(new MicrochipDefinition
        {
            Id = "archive_key",
            Name = "Archive Key",
            Description = "Grants access to Lazarus's deep archives. What secrets lie within?",
            Category = MicrochipCategory.Protocol,
            Rarity = ItemRarity.Legendary,
            IsUnique = true,
            EnergyCost = 50,
            HeatGenerated = 40,
            HeatMax = 120,
            GrantsAbility = "archive_access",
            MinLevel = 15,
            PlaceholderColor = Color.Gold
        });

        // Ancient/Boss drops
        Register(new MicrochipDefinition
        {
            Id = "ancient_core",
            Name = "Ancient Core",
            Description = "A relic from the Ancients. All stats enhanced.",
            Category = MicrochipCategory.Driver,
            Rarity = ItemRarity.Legendary,
            IsUnique = true,
            StatBonuses = new()
            {
                { "MaxHp", 25 },
                { "Attack", 10 },
                { "Defense", 10 },
                { "Speed", 10 },
                { "Special", 10 },
                { "MaxEnergy", 40 },
                { "EnergyRegen", 4 }
            },
            MinLevel = 25,
            PlaceholderColor = Color.White
        });
    }
}
