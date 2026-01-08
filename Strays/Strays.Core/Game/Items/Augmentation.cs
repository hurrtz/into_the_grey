using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Strays.Core.Game.Items;

/// <summary>
/// Body slots where augmentations can be equipped.
/// </summary>
public enum AugmentationSlot
{
    /// <summary>
    /// Head - sensors, processors, optics.
    /// </summary>
    Head,

    /// <summary>
    /// Torso - core systems, power units.
    /// </summary>
    Torso,

    /// <summary>
    /// Front left limb.
    /// </summary>
    FrontLeft,

    /// <summary>
    /// Front right limb.
    /// </summary>
    FrontRight,

    /// <summary>
    /// Rear left limb.
    /// </summary>
    RearLeft,

    /// <summary>
    /// Rear right limb.
    /// </summary>
    RearRight,

    /// <summary>
    /// Tail or wings (special appendage).
    /// </summary>
    Appendage
}

/// <summary>
/// Rarity tiers for items.
/// </summary>
public enum ItemRarity
{
    /// <summary>
    /// Common - easily found.
    /// </summary>
    Common,

    /// <summary>
    /// Uncommon - somewhat rare.
    /// </summary>
    Uncommon,

    /// <summary>
    /// Rare - hard to find.
    /// </summary>
    Rare,

    /// <summary>
    /// Epic - very rare, powerful.
    /// </summary>
    Epic,

    /// <summary>
    /// Legendary - unique or extremely rare.
    /// </summary>
    Legendary,

    /// <summary>
    /// Corrupted - tainted by NIMDOK's influence.
    /// </summary>
    Corrupted
}

/// <summary>
/// Definition of an augmentation that can be equipped to a Stray.
/// </summary>
public class AugmentationDefinition
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
    /// Which slot this augmentation fits.
    /// </summary>
    public AugmentationSlot Slot { get; init; } = AugmentationSlot.Torso;

    /// <summary>
    /// Rarity tier.
    /// </summary>
    public ItemRarity Rarity { get; init; } = ItemRarity.Common;

    /// <summary>
    /// Stat bonuses (flat values added to stats).
    /// </summary>
    public Dictionary<string, int> StatBonuses { get; init; } = new();

    /// <summary>
    /// Stat multipliers (applied after bonuses).
    /// </summary>
    public Dictionary<string, float> StatMultipliers { get; init; } = new();

    /// <summary>
    /// Ability granted by this augmentation.
    /// </summary>
    public string? GrantsAbility { get; init; }

    /// <summary>
    /// Element resistance granted (reduces damage from this element).
    /// </summary>
    public Combat.Element? ElementResistance { get; init; }

    /// <summary>
    /// Element weakness added (increases damage from this element).
    /// </summary>
    public Combat.Element? ElementWeakness { get; init; }

    /// <summary>
    /// Minimum level to equip.
    /// </summary>
    public int MinLevel { get; init; } = 1;

    /// <summary>
    /// Stray types that can equip this (empty = all).
    /// </summary>
    public List<string> CompatibleTypes { get; init; } = new();

    /// <summary>
    /// Whether this augmentation can trigger evolution.
    /// </summary>
    public bool CanTriggerEvolution { get; init; } = false;

    /// <summary>
    /// Placeholder color for visuals.
    /// </summary>
    public Color PlaceholderColor { get; init; } = Color.Gray;

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
/// An instance of an augmentation in the player's inventory or equipped.
/// </summary>
public class Augmentation
{
    private static int _nextInstanceId = 1;

    /// <summary>
    /// Unique instance ID.
    /// </summary>
    public string InstanceId { get; }

    /// <summary>
    /// The augmentation definition.
    /// </summary>
    public AugmentationDefinition Definition { get; }

    /// <summary>
    /// Upgrade level (0 = base).
    /// </summary>
    public int UpgradeLevel { get; private set; } = 0;

    /// <summary>
    /// Maximum upgrade level.
    /// </summary>
    public const int MaxUpgradeLevel = 5;

    /// <summary>
    /// Whether this is equipped to a Stray.
    /// </summary>
    public bool IsEquipped { get; set; } = false;

    /// <summary>
    /// ID of the Stray this is equipped to (if any).
    /// </summary>
    public string? EquippedToStrayId { get; set; }

    public Augmentation(AugmentationDefinition definition)
    {
        InstanceId = $"aug_{_nextInstanceId++}";
        Definition = definition;
    }

    /// <summary>
    /// Gets the stat bonus with upgrade scaling.
    /// </summary>
    public int GetStatBonus(string stat)
    {
        if (!Definition.StatBonuses.TryGetValue(stat, out int baseBonus))
            return 0;

        // Each upgrade level adds 10% to the bonus
        return (int)(baseBonus * (1f + UpgradeLevel * 0.1f));
    }

    /// <summary>
    /// Gets the stat multiplier with upgrade scaling.
    /// </summary>
    public float GetStatMultiplier(string stat)
    {
        if (!Definition.StatMultipliers.TryGetValue(stat, out float baseMult))
            return 1f;

        // Each upgrade level adds 2% to the multiplier bonus
        float bonus = baseMult - 1f;
        return 1f + bonus * (1f + UpgradeLevel * 0.02f);
    }

    /// <summary>
    /// Upgrades the augmentation.
    /// </summary>
    /// <returns>True if upgrade succeeded.</returns>
    public bool Upgrade()
    {
        if (UpgradeLevel >= MaxUpgradeLevel)
            return false;

        UpgradeLevel++;
        return true;
    }

    /// <summary>
    /// Checks if a Stray can equip this augmentation.
    /// </summary>
    public bool CanEquip(Entities.Stray stray)
    {
        if (stray.Level < Definition.MinLevel)
            return false;

        if (Definition.CompatibleTypes.Count > 0 &&
            !Definition.CompatibleTypes.Contains(stray.Definition.Type.ToString()))
            return false;

        return true;
    }
}

/// <summary>
/// Static registry of all augmentations in the game.
/// </summary>
public static class Augmentations
{
    private static readonly Dictionary<string, AugmentationDefinition> _augmentations = new();

    /// <summary>
    /// All registered augmentations.
    /// </summary>
    public static IReadOnlyDictionary<string, AugmentationDefinition> All => _augmentations;

    /// <summary>
    /// Gets an augmentation by ID.
    /// </summary>
    public static AugmentationDefinition? Get(string id) =>
        _augmentations.TryGetValue(id, out var aug) ? aug : null;

    /// <summary>
    /// Gets all augmentations for a specific slot.
    /// </summary>
    public static IEnumerable<AugmentationDefinition> GetBySlot(AugmentationSlot slot) =>
        _augmentations.Values.Where(a => a.Slot == slot);

    /// <summary>
    /// Gets all augmentations of a specific rarity.
    /// </summary>
    public static IEnumerable<AugmentationDefinition> GetByRarity(ItemRarity rarity) =>
        _augmentations.Values.Where(a => a.Rarity == rarity);

    /// <summary>
    /// Registers an augmentation.
    /// </summary>
    public static void Register(AugmentationDefinition augmentation)
    {
        _augmentations[augmentation.Id] = augmentation;
    }

    static Augmentations()
    {
        RegisterHeadAugmentations();
        RegisterTorsoAugmentations();
        RegisterLimbAugmentations();
        RegisterAppendageAugmentations();
    }

    private static void RegisterHeadAugmentations()
    {
        Register(new AugmentationDefinition
        {
            Id = "basic_optics",
            Name = "Basic Optics",
            Description = "Simple optical sensors. Improves accuracy.",
            Slot = AugmentationSlot.Head,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Accuracy", 5 } },
            PlaceholderColor = Color.LightBlue
        });

        Register(new AugmentationDefinition
        {
            Id = "advanced_processor",
            Name = "Advanced Processor",
            Description = "Enhanced neural processor. Increases Special.",
            Slot = AugmentationSlot.Head,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "Special", 10 } },
            MinLevel = 5,
            PlaceholderColor = Color.Cyan
        });

        Register(new AugmentationDefinition
        {
            Id = "targeting_array",
            Name = "Targeting Array",
            Description = "Military-grade targeting. Critical hit bonus.",
            Slot = AugmentationSlot.Head,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Accuracy", 10 }, { "CritChance", 15 } },
            MinLevel = 10,
            PlaceholderColor = Color.Red
        });

        Register(new AugmentationDefinition
        {
            Id = "psionic_amplifier",
            Name = "Psionic Amplifier",
            Description = "Amplifies mental abilities. Grants Mind Blast.",
            Slot = AugmentationSlot.Head,
            Rarity = ItemRarity.Epic,
            StatBonuses = new() { { "Special", 20 } },
            GrantsAbility = "mind_blast",
            MinLevel = 15,
            PlaceholderColor = Color.Magenta
        });

        Register(new AugmentationDefinition
        {
            Id = "nimdok_cortex",
            Name = "NIMDOK Cortex",
            Description = "A fragment of NIMDOK's consciousness. Dangerous power.",
            Slot = AugmentationSlot.Head,
            Rarity = ItemRarity.Corrupted,
            StatBonuses = new() { { "Special", 30 }, { "Attack", 15 } },
            StatMultipliers = new() { { "MaxHp", 0.9f } },
            GrantsAbility = "corrupted_strike",
            CanTriggerEvolution = true,
            MinLevel = 20,
            PlaceholderColor = Color.DarkMagenta
        });
    }

    private static void RegisterTorsoAugmentations()
    {
        Register(new AugmentationDefinition
        {
            Id = "reinforced_chassis",
            Name = "Reinforced Chassis",
            Description = "Strengthened body frame. Increases HP.",
            Slot = AugmentationSlot.Torso,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "MaxHp", 20 } },
            PlaceholderColor = Color.Gray
        });

        Register(new AugmentationDefinition
        {
            Id = "power_core",
            Name = "Power Core",
            Description = "Enhanced power supply. Boosts all stats slightly.",
            Slot = AugmentationSlot.Torso,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "MaxHp", 10 }, { "Attack", 5 }, { "Defense", 5 } },
            MinLevel = 5,
            PlaceholderColor = Color.Orange
        });

        Register(new AugmentationDefinition
        {
            Id = "ablative_plating",
            Name = "Ablative Plating",
            Description = "Damage-absorbing armor. High defense boost.",
            Slot = AugmentationSlot.Torso,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Defense", 25 } },
            StatMultipliers = new() { { "Speed", 0.9f } },
            ElementResistance = Combat.Element.Kinetic,
            MinLevel = 10,
            PlaceholderColor = Color.SteelBlue
        });

        Register(new AugmentationDefinition
        {
            Id = "reactor_heart",
            Name = "Reactor Heart",
            Description = "Miniature fusion reactor. Massive stat boost.",
            Slot = AugmentationSlot.Torso,
            Rarity = ItemRarity.Legendary,
            StatMultipliers = new() { { "MaxHp", 1.25f }, { "Attack", 1.15f }, { "Special", 1.15f } },
            MinLevel = 25,
            PlaceholderColor = Color.Gold
        });
    }

    private static void RegisterLimbAugmentations()
    {
        Register(new AugmentationDefinition
        {
            Id = "servo_leg",
            Name = "Servo Leg",
            Description = "Basic mechanical leg. Slight speed boost.",
            Slot = AugmentationSlot.FrontLeft,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Speed", 5 } },
            PlaceholderColor = Color.Silver
        });

        Register(new AugmentationDefinition
        {
            Id = "hydraulic_leg",
            Name = "Hydraulic Leg",
            Description = "Powerful hydraulic leg. Attack and speed.",
            Slot = AugmentationSlot.FrontRight,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "Attack", 8 }, { "Speed", 8 } },
            MinLevel = 5,
            PlaceholderColor = Color.DarkGray
        });

        Register(new AugmentationDefinition
        {
            Id = "combat_claw",
            Name = "Combat Claw",
            Description = "Razor-sharp combat appendage. High attack.",
            Slot = AugmentationSlot.FrontLeft,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Attack", 20 }, { "CritChance", 10 } },
            MinLevel = 10,
            PlaceholderColor = Color.Crimson
        });

        Register(new AugmentationDefinition
        {
            Id = "sprint_actuators",
            Name = "Sprint Actuators",
            Description = "High-speed leg system. Major speed boost.",
            Slot = AugmentationSlot.RearLeft,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Speed", 25 } },
            MinLevel = 12,
            PlaceholderColor = Color.LightGreen
        });

        Register(new AugmentationDefinition
        {
            Id = "shock_absorbers",
            Name = "Shock Absorbers",
            Description = "Impact-dampening legs. Defense boost.",
            Slot = AugmentationSlot.RearRight,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "Defense", 12 } },
            MinLevel = 8,
            PlaceholderColor = Color.SlateGray
        });
    }

    private static void RegisterAppendageAugmentations()
    {
        Register(new AugmentationDefinition
        {
            Id = "sensor_tail",
            Name = "Sensor Tail",
            Description = "Tail with environmental sensors. Evasion boost.",
            Slot = AugmentationSlot.Appendage,
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Evasion", 5 } },
            PlaceholderColor = Color.Teal
        });

        Register(new AugmentationDefinition
        {
            Id = "blade_tail",
            Name = "Blade Tail",
            Description = "Weaponized tail appendage. Attack boost.",
            Slot = AugmentationSlot.Appendage,
            Rarity = ItemRarity.Uncommon,
            StatBonuses = new() { { "Attack", 15 } },
            MinLevel = 7,
            PlaceholderColor = Color.Silver
        });

        Register(new AugmentationDefinition
        {
            Id = "glider_wings",
            Name = "Glider Wings",
            Description = "Lightweight wings. Major speed and evasion.",
            Slot = AugmentationSlot.Appendage,
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Speed", 15 }, { "Evasion", 15 } },
            StatMultipliers = new() { { "Defense", 0.9f } },
            MinLevel = 15,
            PlaceholderColor = Color.SkyBlue
        });

        Register(new AugmentationDefinition
        {
            Id = "tesla_coil",
            Name = "Tesla Coil",
            Description = "Electric discharge appendage. Grants Shock.",
            Slot = AugmentationSlot.Appendage,
            Rarity = ItemRarity.Epic,
            StatBonuses = new() { { "Special", 15 } },
            GrantsAbility = "shock",
            ElementResistance = Combat.Element.Electric,
            MinLevel = 18,
            PlaceholderColor = Color.Yellow
        });
    }
}
