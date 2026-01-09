using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Data;
using Strays.Core.Game.Stats;

namespace Strays.Core.Game.Items;

/// <summary>
/// Rarity tiers for items.
/// </summary>
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
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
    /// Which slot this augmentation fits (universal or category-specific).
    /// </summary>
    public SlotReference Slot { get; init; } = new SlotReference(UniversalSlot.Core);

    /// <summary>
    /// Rarity tier.
    /// </summary>
    public ItemRarity Rarity { get; init; } = ItemRarity.Common;

    /// <summary>
    /// Stat bonuses (flat values added to stats). Legacy - use StatModifiers for new code.
    /// </summary>
    public Dictionary<string, int> StatBonuses { get; init; } = new();

    /// <summary>
    /// Stat multipliers (applied after bonuses). Legacy - use StatModifiers for new code.
    /// </summary>
    public Dictionary<string, float> StatMultipliers { get; init; } = new();

    /// <summary>
    /// Stat modifiers using the new comprehensive stat system.
    /// </summary>
    public List<StatModifier> StatModifiers { get; init; } = new();

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
    /// Creature categories that can equip this (empty = all categories).
    /// </summary>
    public HashSet<CreatureCategory> CompatibleCategories { get; init; } = new();

    /// <summary>
    /// Whether this augmentation can trigger evolution.
    /// </summary>
    public bool CanTriggerEvolution { get; init; } = false;

    /// <summary>
    /// Placeholder color for visuals.
    /// </summary>
    public Color PlaceholderColor { get; init; } = Color.Gray;

    /// <summary>
    /// Special effect description (if any).
    /// </summary>
    public string? SpecialEffect { get; init; }

    /// <summary>
    /// Whether this is compatible with all categories.
    /// </summary>
    public bool IsUniversallyCompatible => CompatibleCategories.Count == 0;

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
    /// Checks if this augmentation is compatible with a creature category.
    /// </summary>
    public bool IsCompatibleWith(CreatureCategory category) =>
        CompatibleCategories.Count == 0 || CompatibleCategories.Contains(category);

    /// <summary>
    /// Checks if this augmentation can be equipped to the given slot for a category.
    /// </summary>
    public bool CanEquipToSlot(SlotReference slot, CreatureCategory category) =>
        Slot.Equals(slot) && IsCompatibleWith(category) && slot.IsValidForCategory(category);
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

        return (int)(baseBonus * (1f + UpgradeLevel * 0.1f));
    }

    /// <summary>
    /// Gets the stat multiplier with upgrade scaling.
    /// </summary>
    public float GetStatMultiplier(string stat)
    {
        if (!Definition.StatMultipliers.TryGetValue(stat, out float baseMult))
            return 1f;

        float bonus = baseMult - 1f;
        return 1f + bonus * (1f + UpgradeLevel * 0.02f);
    }

    /// <summary>
    /// Upgrades the augmentation.
    /// </summary>
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

        if (!Definition.IsCompatibleWith(stray.Definition.Category))
            return false;

        // Verify the slot is valid for this Stray's category
        if (!Definition.Slot.IsValidForCategory(stray.Definition.Category))
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
    private static bool _initialized = false;

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
    /// Gets all augmentations for a specific universal slot.
    /// </summary>
    public static IEnumerable<AugmentationDefinition> GetByUniversalSlot(UniversalSlot slot) =>
        _augmentations.Values.Where(a => a.Slot.IsUniversal && a.Slot.UniversalSlot == slot);

    /// <summary>
    /// Gets all augmentations for a specific category slot.
    /// </summary>
    public static IEnumerable<AugmentationDefinition> GetByCategorySlot(CategorySlot slot) =>
        _augmentations.Values.Where(a => !a.Slot.IsUniversal && a.Slot.CategorySlot == slot);

    /// <summary>
    /// Gets all augmentations for a specific slot reference.
    /// </summary>
    public static IEnumerable<AugmentationDefinition> GetBySlot(SlotReference slot) =>
        _augmentations.Values.Where(a => a.Slot.Equals(slot));

    /// <summary>
    /// Gets all augmentations of a specific rarity.
    /// </summary>
    public static IEnumerable<AugmentationDefinition> GetByRarity(ItemRarity rarity) =>
        _augmentations.Values.Where(a => a.Rarity == rarity);

    /// <summary>
    /// Gets all augmentations compatible with a creature category.
    /// </summary>
    public static IEnumerable<AugmentationDefinition> GetCompatibleWith(CreatureCategory category) =>
        _augmentations.Values.Where(a => a.IsCompatibleWith(category));

    /// <summary>
    /// Gets all augmentations that can be equipped to a slot for a specific category.
    /// </summary>
    public static IEnumerable<AugmentationDefinition> GetForSlotAndCategory(SlotReference slot, CreatureCategory category) =>
        _augmentations.Values.Where(a => a.CanEquipToSlot(slot, category));

    /// <summary>
    /// Registers an augmentation.
    /// </summary>
    public static void Register(AugmentationDefinition augmentation)
    {
        _augmentations[augmentation.Id] = augmentation;
    }

    /// <summary>
    /// Initializes all augmentations.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        RegisterUniversalAugmentations();
        RegisterCategoryAugmentations();
    }

    private static void RegisterUniversalAugmentations()
    {
        // Dermis slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "basic_dermis",
            Name = "Basic Dermis Plating",
            Description = "Simple armor plating. Increases defense.",
            Slot = new SlotReference(UniversalSlot.Dermis),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Defense", 10 } },
            PlaceholderColor = Color.Gray
        });

        Register(new AugmentationDefinition
        {
            Id = "reinforced_hide",
            Name = "Reinforced Hide",
            Description = "Advanced armor plating. High defense boost.",
            Slot = new SlotReference(UniversalSlot.Dermis),
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Defense", 25 } },
            MinLevel = 10,
            PlaceholderColor = Color.SteelBlue
        });

        // Optics slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "basic_optics",
            Name = "Basic Optics",
            Description = "Simple optical sensors. Improves accuracy.",
            Slot = new SlotReference(UniversalSlot.Optics),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Accuracy", 5 } },
            PlaceholderColor = Color.LightBlue
        });

        Register(new AugmentationDefinition
        {
            Id = "targeting_array",
            Name = "Targeting Array",
            Description = "Military-grade targeting. Critical hit bonus.",
            Slot = new SlotReference(UniversalSlot.Optics),
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Accuracy", 10 }, { "CritChance", 15 } },
            MinLevel = 10,
            PlaceholderColor = Color.Red
        });

        // Core slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "reinforced_core",
            Name = "Reinforced Core",
            Description = "Strengthened vital systems. Increases HP.",
            Slot = new SlotReference(UniversalSlot.Core),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "MaxHp", 20 } },
            PlaceholderColor = Color.DarkRed
        });

        Register(new AugmentationDefinition
        {
            Id = "reactor_heart",
            Name = "Reactor Heart",
            Description = "Miniature power reactor. Massive stat boost.",
            Slot = new SlotReference(UniversalSlot.Core),
            Rarity = ItemRarity.Legendary,
            StatMultipliers = new() { { "MaxHp", 1.25f }, { "Attack", 1.15f } },
            MinLevel = 25,
            PlaceholderColor = Color.Gold
        });

        // Neural slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "basic_reflexes",
            Name = "Basic Reflex Booster",
            Description = "Enhanced reaction time. Increases speed.",
            Slot = new SlotReference(UniversalSlot.Neural),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Speed", 8 } },
            PlaceholderColor = Color.LightGreen
        });

        Register(new AugmentationDefinition
        {
            Id = "neural_accelerator",
            Name = "Neural Accelerator",
            Description = "Advanced neural processing. Major speed boost.",
            Slot = new SlotReference(UniversalSlot.Neural),
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Speed", 20 } },
            MinLevel = 12,
            PlaceholderColor = Color.Cyan
        });

        // Locomotion slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "servo_legs",
            Name = "Servo Legs",
            Description = "Basic mechanical legs. Speed boost.",
            Slot = new SlotReference(UniversalSlot.Locomotion),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Speed", 10 } },
            PlaceholderColor = Color.Silver
        });

        Register(new AugmentationDefinition
        {
            Id = "sprint_actuators",
            Name = "Sprint Actuators",
            Description = "High-speed leg system. Major speed boost.",
            Slot = new SlotReference(UniversalSlot.Locomotion),
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Speed", 25 } },
            MinLevel = 12,
            PlaceholderColor = Color.LimeGreen
        });

        // Respiratory slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "basic_respirator",
            Name = "Basic Respirator",
            Description = "Enhanced breathing system. Stamina boost.",
            Slot = new SlotReference(UniversalSlot.Respiratory),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "MaxHp", 15 } },
            PlaceholderColor = Color.LightBlue
        });

        // CNS slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "basic_cns",
            Name = "Basic CNS Booster",
            Description = "Enhanced central nervous system. Improves reactions.",
            Slot = new SlotReference(UniversalSlot.CNS),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Speed", 5 }, { "Special", 5 } },
            PlaceholderColor = Color.Purple
        });

        // Aural slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "enhanced_hearing",
            Name = "Enhanced Hearing",
            Description = "Improved auditory sensors. Detection boost.",
            Slot = new SlotReference(UniversalSlot.Aural),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Detection", 10 } },
            PlaceholderColor = Color.LightYellow
        });

        // OlfactoryChem slot augmentations
        Register(new AugmentationDefinition
        {
            Id = "chemical_analyzer",
            Name = "Chemical Analyzer",
            Description = "Advanced scent/chemical detection.",
            Slot = new SlotReference(UniversalSlot.OlfactoryChem),
            Rarity = ItemRarity.Common,
            StatBonuses = new() { { "Detection", 8 } },
            PlaceholderColor = Color.LightPink
        });
    }

    private static void RegisterCategoryAugmentations()
    {
        // Predatoria category-specific augmentations
        Register(new AugmentationDefinition
        {
            Id = "predator_clamp",
            Name = "Predator Clamp Module",
            Description = "Enhanced grip for holding prey.",
            Slot = new SlotReference(CategorySlot.ClampModule),
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Attack", 20 } },
            CompatibleCategories = new() { CreatureCategory.Predatoria },
            MinLevel = 10,
            PlaceholderColor = Color.Crimson
        });

        // Colossomammalia category-specific augmentations
        Register(new AugmentationDefinition
        {
            Id = "siege_harness",
            Name = "Siege Harness",
            Description = "Heavy mount system for massive creatures.",
            Slot = new SlotReference(CategorySlot.SiegeHarness),
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Defense", 30 }, { "MaxHp", 40 } },
            CompatibleCategories = new() { CreatureCategory.Colossomammalia },
            MinLevel = 10,
            PlaceholderColor = Color.DarkGray
        });

        // Exoskeletalis category-specific augmentations
        Register(new AugmentationDefinition
        {
            Id = "carapace_segment",
            Name = "Reinforced Carapace",
            Description = "Hardened shell plating for arthropods.",
            Slot = new SlotReference(CategorySlot.CarapaceSegmentBus),
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Defense", 25 } },
            CompatibleCategories = new() { CreatureCategory.Exoskeletalis },
            MinLevel = 8,
            PlaceholderColor = Color.Brown
        });

        // Octomorpha category-specific augmentations
        Register(new AugmentationDefinition
        {
            Id = "multi_arm_tools",
            Name = "Multi-Arm Toolbus",
            Description = "Enhanced manipulation systems.",
            Slot = new SlotReference(CategorySlot.MultiArmToolbus),
            Rarity = ItemRarity.Rare,
            StatBonuses = new() { { "Attack", 15 }, { "Special", 15 } },
            CompatibleCategories = new() { CreatureCategory.Octomorpha },
            MinLevel = 10,
            PlaceholderColor = Color.DarkMagenta
        });
    }
}
