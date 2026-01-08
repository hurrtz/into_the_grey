using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Entities;

namespace Strays.Core.Game.Data;

/// <summary>
/// The type/species category of a Stray.
/// </summary>
public enum StrayType
{
    Canine,     // Dogs, wolves, foxes
    Feline,     // Cats, big cats
    Avian,      // Birds
    Rodent,     // Rats, mice, rabbits
    Mammal,     // Other mammals (bears, boars, etc.)
    Reptile,    // Snakes, lizards, gators
    Amphibian,  // Frogs, newts
    Insect,     // Bugs, bees, wasps
    Arachnid,   // Spiders
    Primate,    // Apes, monkeys
    Mustelid,   // Weasels, badgers
    Procyonid,  // Raccoons
    Marsupial,  // Possums
    Chimera,    // Hybrid/mixed types
    Invertebrate // Slugs, etc.
}

/// <summary>
/// Role a Stray naturally fills in combat.
/// </summary>
public enum StrayRole
{
    Tank,       // High HP, defense
    Damage,     // High attack
    Support,    // Healing, buffs
    Control,    // Debuffs, status effects
    Speed,      // High ATB speed
    Utility     // Special abilities
}

/// <summary>
/// Base stats for a Stray at level 1.
/// </summary>
public class StrayBaseStats
{
    public int MaxHp { get; set; } = 100;
    public int Attack { get; set; } = 10;
    public int Defense { get; set; } = 10;
    public int Speed { get; set; } = 10;
    public int Special { get; set; } = 10;

    /// <summary>
    /// Creates stats scaled to a level.
    /// </summary>
    public StrayBaseStats ScaleToLevel(int level)
    {
        // Simple linear scaling: stats increase by 10% per level
        float multiplier = 1f + (level - 1) * 0.1f;
        return new StrayBaseStats
        {
            MaxHp = (int)(MaxHp * multiplier),
            Attack = (int)(Attack * multiplier),
            Defense = (int)(Defense * multiplier),
            Speed = (int)(Speed * multiplier),
            Special = (int)(Special * multiplier)
        };
    }
}

/// <summary>
/// Definition of a Stray species/type.
/// This is the template from which individual Strays are created.
/// </summary>
public class StrayDefinition
{
    /// <summary>
    /// Unique identifier for this definition.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Display name of this Stray type.
    /// </summary>
    public string Name { get; set; } = "Unknown Stray";

    /// <summary>
    /// Description of this Stray.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The species type.
    /// </summary>
    public StrayType Type { get; set; } = StrayType.Canine;

    /// <summary>
    /// Combat role this Stray naturally fills.
    /// </summary>
    public StrayRole Role { get; set; } = StrayRole.Damage;

    /// <summary>
    /// Base stats at level 1.
    /// </summary>
    public StrayBaseStats BaseStats { get; set; } = new();

    /// <summary>
    /// Biomes where this Stray can be found.
    /// </summary>
    public List<string> Biomes { get; set; } = new();

    /// <summary>
    /// Level required for evolution (0 = no evolution).
    /// </summary>
    public int EvolutionLevel { get; set; } = 0;

    /// <summary>
    /// ID of the evolved form (null = no evolution).
    /// </summary>
    public string? EvolvedFormId { get; set; }

    /// <summary>
    /// Special trigger required for evolution (e.g., "combat_stress", "plated_torso_augment").
    /// </summary>
    public string? EvolutionTrigger { get; set; }

    /// <summary>
    /// Number of microchip slots.
    /// </summary>
    public int MicrochipSlots { get; set; } = 2;

    /// <summary>
    /// Innate abilities this Stray has.
    /// </summary>
    public List<string> InnateAbilities { get; set; } = new();

    /// <summary>
    /// Color used for placeholder visuals.
    /// </summary>
    public Color PlaceholderColor { get; set; } = Color.Green;

    /// <summary>
    /// Size in pixels for placeholder visuals.
    /// </summary>
    public int PlaceholderSize { get; set; } = 20;

    /// <summary>
    /// Whether this Stray can be recruited.
    /// </summary>
    public bool CanRecruit { get; set; } = true;

    /// <summary>
    /// Base recruitment chance (0.0 - 1.0).
    /// </summary>
    public float RecruitChance { get; set; } = 0.5f;

    /// <summary>
    /// Special recruitment conditions (if any).
    /// </summary>
    public RecruitmentCondition? RecruitCondition { get; set; }
}

/// <summary>
/// Static registry of all Stray definitions.
/// </summary>
public static class StrayDefinitions
{
    private static readonly Dictionary<string, StrayDefinition> _definitions = new();

    static StrayDefinitions()
    {
        // Initialize with some starter Strays from The Fringe
        RegisterFringeStrays();
    }

    /// <summary>
    /// Gets a Stray definition by ID.
    /// </summary>
    public static StrayDefinition? Get(string id)
    {
        return _definitions.TryGetValue(id, out var def) ? def : null;
    }

    /// <summary>
    /// Gets all registered definitions.
    /// </summary>
    public static IEnumerable<StrayDefinition> GetAll() => _definitions.Values;

    /// <summary>
    /// Registers a Stray definition.
    /// </summary>
    public static void Register(StrayDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }

    /// <summary>
    /// Registers the Strays found in The Fringe biome.
    /// </summary>
    private static void RegisterFringeStrays()
    {
        // Echo Pup - The first recruited Stray, good at detecting glitches
        Register(new StrayDefinition
        {
            Id = "echo_pup",
            Name = "Echo Pup",
            Description = "Detects glitches and digests digital data. A reliable first companion.",
            Type = StrayType.Canine,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 90, Attack = 12, Defense = 8, Speed = 14, Special = 16 },
            Biomes = new List<string> { "fringe" },
            EvolutionLevel = 16,
            EvolvedFormId = "resonator_hound",
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "glitch_detect", "data_digest" },
            PlaceholderColor = Color.LightGreen,
            PlaceholderSize = 18
        });

        // Circuit Crow - Accuracy and vision
        Register(new StrayDefinition
        {
            Id = "circuit_crow",
            Name = "Circuit Crow",
            Description = "Enhanced vision grants party accuracy. Can spot threats from afar.",
            Type = StrayType.Avian,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 70, Attack = 14, Defense = 6, Speed = 16, Special = 14 },
            Biomes = new List<string> { "fringe" },
            EvolutionLevel = 18,
            EvolvedFormId = "scan_raven",
            EvolutionTrigger = "optical_sensor_augment",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "eagle_eye" },
            PlaceholderColor = Color.DarkGray,
            PlaceholderSize = 16
        });

        // Relay Rodent - Energy regen
        Register(new StrayDefinition
        {
            Id = "relay_rodent",
            Name = "Relay Rodent",
            Description = "Generates static energy. Resists shock damage and helps with energy regen.",
            Type = StrayType.Rodent,
            Role = StrayRole.Utility,
            BaseStats = new StrayBaseStats { MaxHp = 65, Attack = 10, Defense = 8, Speed = 18, Special = 12 },
            Biomes = new List<string> { "fringe" },
            EvolutionLevel = 15,
            EvolvedFormId = "capacitor_hare",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "static_resist", "energy_regen" },
            PlaceholderColor = Color.Yellow,
            PlaceholderSize = 14
        });

        // Static Feline - Stealth and dodging
        Register(new StrayDefinition
        {
            Id = "static_feline",
            Name = "Static Feline",
            Description = "Phases through attacks with unnatural grace. A stealth specialist.",
            Type = StrayType.Feline,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 75, Attack = 16, Defense = 6, Speed = 20, Special = 10 },
            Biomes = new List<string> { "fringe" },
            EvolutionLevel = 20,
            EvolvedFormId = "phase_panther",
            EvolutionTrigger = "combat_stress",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "phase_dodge", "stealth" },
            PlaceholderColor = Color.Purple,
            PlaceholderSize = 18
        });

        // Buffer Badger - Tank
        Register(new StrayDefinition
        {
            Id = "buffer_badger",
            Name = "Buffer Badger",
            Description = "Stores damage in reserve systems. A reliable tank that protects the party.",
            Type = StrayType.Mustelid,
            Role = StrayRole.Tank,
            BaseStats = new StrayBaseStats { MaxHp = 140, Attack = 10, Defense = 16, Speed = 8, Special = 8 },
            Biomes = new List<string> { "fringe" },
            EvolutionLevel = 18,
            EvolvedFormId = "fortress_meles",
            EvolutionTrigger = "plated_torso_augment",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "damage_buffer", "party_shield" },
            PlaceholderColor = Color.Gray,
            PlaceholderSize = 22
        });

        // Resonator Hound - Echo Pup evolution
        Register(new StrayDefinition
        {
            Id = "resonator_hound",
            Name = "Resonator Hound",
            Description = "Evolved Echo Pup. Its howl resonates with data streams.",
            Type = StrayType.Canine,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 120, Attack = 16, Defense = 12, Speed = 16, Special = 22 },
            Biomes = new List<string> { "fringe", "rust" },
            MicrochipSlots = 4,
            InnateAbilities = new List<string> { "glitch_detect", "data_digest", "resonance_howl" },
            PlaceholderColor = Color.Lime,
            PlaceholderSize = 24,
            CanRecruit = false // Only obtained through evolution
        });

        // Wild Stray - Generic placeholder for testing
        Register(new StrayDefinition
        {
            Id = "wild_stray",
            Name = "Wild Stray",
            Description = "A generic wild Stray. Not particularly special, but dangerous in groups.",
            Type = StrayType.Mammal,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 80, Attack = 12, Defense = 10, Speed = 12, Special = 10 },
            Biomes = new List<string> { "fringe", "rust", "green", "quiet", "teeth", "glow", "archive_scar" },
            MicrochipSlots = 1,
            PlaceholderColor = Color.Brown,
            PlaceholderSize = 16
        });
    }
}
