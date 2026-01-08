using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Combat;

namespace Strays.Core.Game.Items;

/// <summary>
/// Types of microchip effects.
/// </summary>
public enum MicrochipType
{
    /// <summary>
    /// Grants a combat ability.
    /// </summary>
    Ability,

    /// <summary>
    /// Provides passive stat bonuses.
    /// </summary>
    Passive,

    /// <summary>
    /// Provides elemental affinity.
    /// </summary>
    Elemental,

    /// <summary>
    /// Special story-related chips.
    /// </summary>
    Special
}

/// <summary>
/// Definition of a microchip that can be equipped to a Stray.
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
    /// Type of microchip.
    /// </summary>
    public MicrochipType Type { get; init; } = MicrochipType.Passive;

    /// <summary>
    /// Rarity tier.
    /// </summary>
    public ItemRarity Rarity { get; init; } = ItemRarity.Common;

    /// <summary>
    /// Ability granted by this chip (for Ability type).
    /// </summary>
    public string? GrantsAbility { get; init; }

    /// <summary>
    /// Stat bonuses provided.
    /// </summary>
    public Dictionary<string, int> StatBonuses { get; init; } = new();

    /// <summary>
    /// Stat multipliers provided.
    /// </summary>
    public Dictionary<string, float> StatMultipliers { get; init; } = new();

    /// <summary>
    /// Element affinity granted.
    /// </summary>
    public Element? ElementAffinity { get; init; }

    /// <summary>
    /// Element resistance granted.
    /// </summary>
    public Element? ElementResistance { get; init; }

    /// <summary>
    /// How many chip slots this uses.
    /// </summary>
    public int SlotCost { get; init; } = 1;

    /// <summary>
    /// Minimum level to equip.
    /// </summary>
    public int MinLevel { get; init; } = 1;

    /// <summary>
    /// Whether this chip is unique (only one can be equipped).
    /// </summary>
    public bool IsUnique { get; init; } = false;

    /// <summary>
    /// Whether this is Bandit's special amplifier chip.
    /// </summary>
    public bool IsBanditAmplifier { get; init; } = false;

    /// <summary>
    /// Placeholder color.
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
}

/// <summary>
/// An instance of a microchip.
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
    /// Whether this is equipped to a Stray.
    /// </summary>
    public bool IsEquipped { get; set; } = false;

    /// <summary>
    /// ID of the Stray this is equipped to (if any).
    /// </summary>
    public string? EquippedToStrayId { get; set; }

    public Microchip(MicrochipDefinition definition)
    {
        InstanceId = $"chip_{_nextInstanceId++}";
        Definition = definition;
    }

    /// <summary>
    /// Gets the stat bonus from this chip.
    /// </summary>
    public int GetStatBonus(string stat)
    {
        return Definition.StatBonuses.TryGetValue(stat, out int bonus) ? bonus : 0;
    }

    /// <summary>
    /// Gets the stat multiplier from this chip.
    /// </summary>
    public float GetStatMultiplier(string stat)
    {
        return Definition.StatMultipliers.TryGetValue(stat, out float mult) ? mult : 1f;
    }

    /// <summary>
    /// Checks if a Stray can equip this chip.
    /// </summary>
    public bool CanEquip(Entities.Stray stray, int currentSlotUsage, int maxSlots)
    {
        if (stray.Level < Definition.MinLevel)
            return false;

        if (currentSlotUsage + Definition.SlotCost > maxSlots)
            return false;

        return true;
    }
}

/// <summary>
/// Static registry of all microchips in the game.
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
    /// Gets all microchips of a specific type.
    /// </summary>
    public static IEnumerable<MicrochipDefinition> GetByType(MicrochipType type) =>
        _chips.Values.Where(c => c.Type == type);

    /// <summary>
    /// Gets all microchips of a specific rarity.
    /// </summary>
    public static IEnumerable<MicrochipDefinition> GetByRarity(ItemRarity rarity) =>
        _chips.Values.Where(c => c.Rarity == rarity);

    /// <summary>
    /// Registers a microchip.
    /// </summary>
    public static void Register(MicrochipDefinition chip)
    {
        _chips[chip.Id] = chip;
    }

    static Microchips()
    {
        RegisterAbilityChips();
        RegisterPassiveChips();
        RegisterElementalChips();
        RegisterSpecialChips();
    }

    private static void RegisterAbilityChips()
    {
        Register(new MicrochipDefinition
        {
            Id = "chip_strike",
            Name = "Strike Chip",
            Description = "Teaches the basic Strike ability.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Common,
            GrantsAbility = "strike",
            PlaceholderColor = Color.Gray
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_repair",
            Name = "Repair Chip",
            Description = "Teaches the Repair healing ability.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Uncommon,
            GrantsAbility = "repair",
            MinLevel = 3,
            PlaceholderColor = Color.LimeGreen
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_shock",
            Name = "Shock Chip",
            Description = "Teaches the electric Shock ability.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Uncommon,
            GrantsAbility = "shock",
            MinLevel = 5,
            PlaceholderColor = Color.Yellow
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_ember",
            Name = "Ember Chip",
            Description = "Teaches the fire Ember ability.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Uncommon,
            GrantsAbility = "ember",
            MinLevel = 5,
            PlaceholderColor = Color.Orange
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_frost",
            Name = "Frost Chip",
            Description = "Teaches the ice Frost Bite ability.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Uncommon,
            GrantsAbility = "frost_bite",
            MinLevel = 5,
            PlaceholderColor = Color.LightBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_fortify",
            Name = "Fortify Chip",
            Description = "Teaches the defensive Fortify ability.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Uncommon,
            GrantsAbility = "fortify",
            MinLevel = 4,
            PlaceholderColor = Color.SteelBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_power_strike",
            Name = "Power Strike Chip",
            Description = "Teaches the powerful Power Strike ability.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Rare,
            GrantsAbility = "power_strike",
            MinLevel = 8,
            PlaceholderColor = Color.DarkRed
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_thunderbolt",
            Name = "Thunderbolt Chip",
            Description = "Teaches the devastating Thunderbolt ability.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Rare,
            GrantsAbility = "thunderbolt",
            SlotCost = 2,
            MinLevel = 12,
            PlaceholderColor = Color.Gold
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_mass_repair",
            Name = "Mass Repair Chip",
            Description = "Teaches Mass Repair to heal all allies.",
            Type = MicrochipType.Ability,
            Rarity = ItemRarity.Epic,
            GrantsAbility = "mass_repair",
            SlotCost = 2,
            MinLevel = 15,
            PlaceholderColor = Color.Green
        });
    }

    private static void RegisterPassiveChips()
    {
        Register(new MicrochipDefinition
        {
            Id = "chip_vitality",
            Name = "Vitality Chip",
            Description = "Increases maximum HP.",
            Type = MicrochipType.Passive,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "MaxHp", 15 } },
            PlaceholderColor = Color.Pink
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_strength",
            Name = "Strength Chip",
            Description = "Increases Attack stat.",
            Type = MicrochipType.Passive,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Attack", 8 } },
            PlaceholderColor = Color.Red
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_armor",
            Name = "Armor Chip",
            Description = "Increases Defense stat.",
            Type = MicrochipType.Passive,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Defense", 8 } },
            PlaceholderColor = Color.SlateGray
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_agility",
            Name = "Agility Chip",
            Description = "Increases Speed stat.",
            Type = MicrochipType.Passive,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Speed", 8 } },
            PlaceholderColor = Color.LightGreen
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_focus",
            Name = "Focus Chip",
            Description = "Increases Special stat.",
            Type = MicrochipType.Passive,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Special", 8 } },
            PlaceholderColor = Color.Purple
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_all_rounder",
            Name = "All-Rounder Chip",
            Description = "Moderate boost to all stats.",
            Type = MicrochipType.Passive,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "MaxHp", 10 }, { "Attack", 5 }, { "Defense", 5 }, { "Speed", 5 }, { "Special", 5 } },
            SlotCost = 2,
            MinLevel = 10,
            PlaceholderColor = Color.White
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_berserker",
            Name = "Berserker Chip",
            Description = "Major Attack boost, reduces Defense.",
            Type = MicrochipType.Passive,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Attack", 25 } },
            StatMultipliers = new() { { "Defense", 0.8f } },
            MinLevel = 12,
            PlaceholderColor = Color.DarkRed
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_tank",
            Name = "Tank Chip",
            Description = "Major HP and Defense boost, reduces Speed.",
            Type = MicrochipType.Passive,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "MaxHp", 30 }, { "Defense", 15 } },
            StatMultipliers = new() { { "Speed", 0.85f } },
            MinLevel = 12,
            PlaceholderColor = Color.DarkGray
        });
    }

    private static void RegisterElementalChips()
    {
        Register(new MicrochipDefinition
        {
            Id = "chip_electric_affinity",
            Name = "Electric Affinity",
            Description = "Boosts Electric damage and grants resistance.",
            Type = MicrochipType.Elemental,
            Rarity = ItemRarity.Uncommon,
            ElementAffinity = Element.Electric,
            ElementResistance = Element.Electric,
            MinLevel = 6,
            PlaceholderColor = Color.Yellow
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_fire_affinity",
            Name = "Fire Affinity",
            Description = "Boosts Fire damage and grants resistance.",
            Type = MicrochipType.Elemental,
            Rarity = ItemRarity.Uncommon,
            ElementAffinity = Element.Fire,
            ElementResistance = Element.Fire,
            MinLevel = 6,
            PlaceholderColor = Color.OrangeRed
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_ice_affinity",
            Name = "Ice Affinity",
            Description = "Boosts Ice damage and grants resistance.",
            Type = MicrochipType.Elemental,
            Rarity = ItemRarity.Uncommon,
            ElementAffinity = Element.Ice,
            ElementResistance = Element.Ice,
            MinLevel = 6,
            PlaceholderColor = Color.CornflowerBlue
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_toxic_affinity",
            Name = "Toxic Affinity",
            Description = "Boosts Toxic damage and grants resistance.",
            Type = MicrochipType.Elemental,
            Rarity = ItemRarity.Uncommon,
            ElementAffinity = Element.Toxic,
            ElementResistance = Element.Toxic,
            MinLevel = 6,
            PlaceholderColor = Color.Purple
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_psionic_affinity",
            Name = "Psionic Affinity",
            Description = "Boosts Psionic damage and grants resistance.",
            Type = MicrochipType.Elemental,
            Rarity = ItemRarity.Rare,
            ElementAffinity = Element.Psionic,
            ElementResistance = Element.Psionic,
            MinLevel = 10,
            PlaceholderColor = Color.Magenta
        });
    }

    private static void RegisterSpecialChips()
    {
        // Bandit's special amplifier chip - critical to the story
        Register(new MicrochipDefinition
        {
            Id = "bandit_amplifier",
            Name = "Amplifier Chip",
            Description = "A unique chip that amplifies Gravitation. Given to Bandit by NIMDOK.",
            Type = MicrochipType.Special,
            Rarity = ItemRarity.Legendary,
            IsBanditAmplifier = true,
            IsUnique = true,
            StatMultipliers = new() { { "Special", 1.5f } },
            PlaceholderColor = Color.Gold
        });

        // Corrupted chips - found in dangerous areas
        Register(new MicrochipDefinition
        {
            Id = "chip_corruption",
            Name = "Corruption Chip",
            Description = "A chip tainted by NIMDOK's influence. Great power at a cost.",
            Type = MicrochipType.Special,
            Rarity = ItemRarity.Corrupted,
            StatBonuses = new() { { "Attack", 20 }, { "Special", 20 } },
            StatMultipliers = new() { { "MaxHp", 0.85f } },
            ElementAffinity = Element.Corruption,
            MinLevel = 15,
            PlaceholderColor = Color.DarkMagenta
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_overload",
            Name = "Overload Chip",
            Description = "Dangerous experimental chip. Massive stats but unstable.",
            Type = MicrochipType.Special,
            Rarity = ItemRarity.Epic,
            StatMultipliers = new() { { "Attack", 1.4f }, { "Speed", 1.4f }, { "Defense", 0.7f } },
            SlotCost = 3,
            MinLevel = 20,
            PlaceholderColor = Color.Red
        });

        Register(new MicrochipDefinition
        {
            Id = "chip_harmony",
            Name = "Harmony Chip",
            Description = "Balances all systems. Restores HP each turn.",
            Type = MicrochipType.Special,
            Rarity = ItemRarity.Epic,
            StatBonuses = new() { { "MaxHp", 20 } },
            // Grants passive regen effect (handled in combat)
            SlotCost = 2,
            MinLevel = 18,
            PlaceholderColor = Color.LightGreen
        });
    }
}
