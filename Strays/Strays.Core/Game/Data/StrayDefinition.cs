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
        // Initialize all Strays by biome
        RegisterFringeStrays();
        RegisterRustStrays();
        RegisterGreenStrays();
        RegisterQuietStrays();
        RegisterTeethStrays();
        RegisterGlowStrays();
        RegisterArchiveScarStrays();
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

    /// <summary>
    /// Registers the Strays found in The Rust biome.
    /// </summary>
    private static void RegisterRustStrays()
    {
        // Rust Rat - Common industrial scavenger
        Register(new StrayDefinition
        {
            Id = "rust_rat",
            Name = "Rust Rat",
            Description = "Thrives in industrial decay. Can corrode metal with its bite.",
            Type = StrayType.Rodent,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 70, Attack = 14, Defense = 8, Speed = 16, Special = 10 },
            Biomes = new List<string> { "fringe", "rust" },
            EvolutionLevel = 18,
            EvolvedFormId = "corrosion_king",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "corrode", "scavenge" },
            PlaceholderColor = Color.SaddleBrown,
            PlaceholderSize = 14
        });

        // Scrap Hound - Pack hunter
        Register(new StrayDefinition
        {
            Id = "scrap_hound",
            Name = "Scrap Hound",
            Description = "Assembled from discarded parts. Loyal once befriended.",
            Type = StrayType.Canine,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 95, Attack = 16, Defense = 12, Speed = 14, Special = 8 },
            Biomes = new List<string> { "fringe", "rust" },
            EvolutionLevel = 20,
            EvolvedFormId = "junkyard_alpha",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "pack_tactics", "salvage" },
            PlaceholderColor = Color.DarkGray,
            PlaceholderSize = 20
        });

        // Gear Beetle - Defensive insect
        Register(new StrayDefinition
        {
            Id = "gear_beetle",
            Name = "Gear Beetle",
            Description = "Its shell is made of interlocking gears. Incredibly durable.",
            Type = StrayType.Insect,
            Role = StrayRole.Tank,
            BaseStats = new StrayBaseStats { MaxHp = 130, Attack = 8, Defense = 20, Speed = 6, Special = 6 },
            Biomes = new List<string> { "rust" },
            EvolutionLevel = 22,
            EvolvedFormId = "clockwork_colossus",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "gear_shell", "grind" },
            PlaceholderColor = new Color(205, 127, 50), // Bronze
            PlaceholderSize = 18
        });

        // Piston Snake - Mechanical serpent
        Register(new StrayDefinition
        {
            Id = "piston_snake",
            Name = "Piston Snake",
            Description = "Hydraulic muscles give it crushing strength.",
            Type = StrayType.Reptile,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 85, Attack = 18, Defense = 10, Speed = 12, Special = 10 },
            Biomes = new List<string> { "rust" },
            EvolutionLevel = 24,
            EvolvedFormId = "hydraulic_wyrm",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "constrict", "pressure_strike" },
            PlaceholderColor = Color.Silver,
            PlaceholderSize = 22
        });

        // Forge Spider - Fire-resistant arachnid
        Register(new StrayDefinition
        {
            Id = "forge_spider",
            Name = "Forge Spider",
            Description = "Webs of molten metal. Immune to heat.",
            Type = StrayType.Arachnid,
            Role = StrayRole.Control,
            BaseStats = new StrayBaseStats { MaxHp = 75, Attack = 12, Defense = 8, Speed = 14, Special = 16 },
            Biomes = new List<string> { "rust" },
            EvolutionLevel = 20,
            EvolvedFormId = "smelter_queen",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "molten_web", "heat_resist" },
            PlaceholderColor = Color.OrangeRed,
            PlaceholderSize = 16
        });

        // Crane Raptor - Rare aerial predator
        Register(new StrayDefinition
        {
            Id = "crane_raptor",
            Name = "Crane Raptor",
            Description = "Built from construction equipment. Drops from great heights to strike.",
            Type = StrayType.Avian,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 90, Attack = 22, Defense = 8, Speed = 18, Special = 12 },
            Biomes = new List<string> { "rust" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "dive_bomb", "aerial_superiority" },
            PlaceholderColor = Color.Gold,
            PlaceholderSize = 24,
            RecruitChance = 0.25f
        });

        // Furnace Golem - Rare tank
        Register(new StrayDefinition
        {
            Id = "furnace_golem",
            Name = "Furnace Golem",
            Description = "A walking smelter. Radiates intense heat.",
            Type = StrayType.Mammal,
            Role = StrayRole.Tank,
            BaseStats = new StrayBaseStats { MaxHp = 180, Attack = 14, Defense = 18, Speed = 4, Special = 14 },
            Biomes = new List<string> { "rust" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "radiant_heat", "molten_core", "slag_armor" },
            PlaceholderColor = Color.Red,
            PlaceholderSize = 32,
            RecruitChance = 0.15f
        });

        // Corrosion King - Rust Rat evolution
        Register(new StrayDefinition
        {
            Id = "corrosion_king",
            Name = "Corrosion King",
            Description = "Evolved Rust Rat. Its presence accelerates decay.",
            Type = StrayType.Rodent,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 100, Attack = 20, Defense = 12, Speed = 18, Special = 16 },
            Biomes = new List<string> { "rust" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "corrode", "scavenge", "entropy_aura" },
            PlaceholderColor = Color.Maroon,
            PlaceholderSize = 20,
            CanRecruit = false
        });

        // Junkyard Alpha - Scrap Hound evolution
        Register(new StrayDefinition
        {
            Id = "junkyard_alpha",
            Name = "Junkyard Alpha",
            Description = "Evolved Scrap Hound. Commands packs of lesser machines.",
            Type = StrayType.Canine,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 130, Attack = 22, Defense = 16, Speed = 16, Special = 12 },
            Biomes = new List<string> { "rust" },
            MicrochipSlots = 4,
            InnateAbilities = new List<string> { "pack_tactics", "salvage", "alpha_howl", "summon_pack" },
            PlaceholderColor = Color.Black,
            PlaceholderSize = 26,
            CanRecruit = false
        });
    }

    /// <summary>
    /// Registers the Strays found in The Green biome.
    /// </summary>
    private static void RegisterGreenStrays()
    {
        // Vine Serpent - Camouflaged ambusher
        Register(new StrayDefinition
        {
            Id = "vine_serpent",
            Name = "Vine Serpent",
            Description = "Indistinguishable from plant life until it strikes.",
            Type = StrayType.Reptile,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 80, Attack = 16, Defense = 8, Speed = 14, Special = 14 },
            Biomes = new List<string> { "green" },
            EvolutionLevel = 22,
            EvolvedFormId = "jungle_hydra",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "camouflage", "vine_whip", "poison_bite" },
            PlaceholderColor = Color.DarkGreen,
            PlaceholderSize = 20
        });

        // Bloom Moth - Healing support
        Register(new StrayDefinition
        {
            Id = "bloom_moth",
            Name = "Bloom Moth",
            Description = "Its scales carry healing pollen. A gentle presence.",
            Type = StrayType.Insect,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 65, Attack = 6, Defense = 6, Speed = 16, Special = 22 },
            Biomes = new List<string> { "green" },
            EvolutionLevel = 18,
            EvolvedFormId = "aurora_monarch",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "pollen_heal", "sleep_dust" },
            PlaceholderColor = Color.Pink,
            PlaceholderSize = 14
        });

        // Moss Bear - Regenerating tank
        Register(new StrayDefinition
        {
            Id = "moss_bear",
            Name = "Moss Bear",
            Description = "Covered in symbiotic moss that heals its wounds.",
            Type = StrayType.Mammal,
            Role = StrayRole.Tank,
            BaseStats = new StrayBaseStats { MaxHp = 160, Attack = 14, Defense = 14, Speed = 6, Special = 12 },
            Biomes = new List<string> { "green" },
            EvolutionLevel = 25,
            EvolvedFormId = "ancient_grove_ursine",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "regenerate", "hibernate", "nature_shield" },
            PlaceholderColor = Color.ForestGreen,
            PlaceholderSize = 28
        });

        // Thorn Cat - Retaliating damage dealer
        Register(new StrayDefinition
        {
            Id = "thorn_cat",
            Name = "Thorn Cat",
            Description = "Its fur is made of thorns. Striking it hurts.",
            Type = StrayType.Feline,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 75, Attack = 18, Defense = 10, Speed = 18, Special = 8 },
            Biomes = new List<string> { "green" },
            EvolutionLevel = 20,
            EvolvedFormId = "bramble_panther",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "thorn_coat", "pounce" },
            PlaceholderColor = Color.Olive,
            PlaceholderSize = 18
        });

        // Spore Toad - Status effect specialist
        Register(new StrayDefinition
        {
            Id = "spore_toad",
            Name = "Spore Toad",
            Description = "Releases clouds of disorienting spores.",
            Type = StrayType.Amphibian,
            Role = StrayRole.Control,
            BaseStats = new StrayBaseStats { MaxHp = 90, Attack = 8, Defense = 12, Speed = 8, Special = 18 },
            Biomes = new List<string> { "green" },
            EvolutionLevel = 19,
            EvolvedFormId = "fungal_emperor",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "spore_cloud", "toxic_skin", "confusion" },
            PlaceholderColor = Color.MediumPurple,
            PlaceholderSize = 16
        });

        // Ancient Oak Deer - Rare majestic creature
        Register(new StrayDefinition
        {
            Id = "ancient_oak_deer",
            Name = "Ancient Oak Deer",
            Description = "Its antlers are living trees. Said to be centuries old.",
            Type = StrayType.Mammal,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 120, Attack = 12, Defense = 14, Speed = 14, Special = 24 },
            Biomes = new List<string> { "green" },
            MicrochipSlots = 4,
            InnateAbilities = new List<string> { "nature_blessing", "forest_communion", "antler_barrier" },
            PlaceholderColor = Color.SaddleBrown,
            PlaceholderSize = 26,
            RecruitChance = 0.2f
        });

        // Chloro Phoenix - Rare legendary bird
        Register(new StrayDefinition
        {
            Id = "chloro_phoenix",
            Name = "Chloro Phoenix",
            Description = "Burns with green flame. Can resurrect once per battle.",
            Type = StrayType.Avian,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 100, Attack = 16, Defense = 10, Speed = 20, Special = 26 },
            Biomes = new List<string> { "green" },
            MicrochipSlots = 4,
            InnateAbilities = new List<string> { "green_flame", "rebirth", "photosynthesis" },
            PlaceholderColor = Color.LimeGreen,
            PlaceholderSize = 24,
            RecruitChance = 0.1f
        });

        // Jungle Hydra - Vine Serpent evolution
        Register(new StrayDefinition
        {
            Id = "jungle_hydra",
            Name = "Jungle Hydra",
            Description = "Evolved Vine Serpent. Multiple heads strike simultaneously.",
            Type = StrayType.Reptile,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 120, Attack = 22, Defense = 12, Speed = 16, Special = 18 },
            Biomes = new List<string> { "green" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "camouflage", "vine_whip", "poison_bite", "multi_strike" },
            PlaceholderColor = Color.DarkOliveGreen,
            PlaceholderSize = 28,
            CanRecruit = false
        });

        // Aurora Monarch - Bloom Moth evolution
        Register(new StrayDefinition
        {
            Id = "aurora_monarch",
            Name = "Aurora Monarch",
            Description = "Evolved Bloom Moth. Its wings shimmer with healing light.",
            Type = StrayType.Insect,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 90, Attack = 10, Defense = 10, Speed = 18, Special = 30 },
            Biomes = new List<string> { "green" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "pollen_heal", "sleep_dust", "aurora_wings", "mass_heal" },
            PlaceholderColor = Color.HotPink,
            PlaceholderSize = 20,
            CanRecruit = false
        });
    }

    /// <summary>
    /// Registers the Strays found in The Quiet biome.
    /// </summary>
    private static void RegisterQuietStrays()
    {
        // House Cat - Deceptively domestic
        Register(new StrayDefinition
        {
            Id = "house_cat",
            Name = "House Cat",
            Description = "Appears normal. Its eyes betray something else.",
            Type = StrayType.Feline,
            Role = StrayRole.Speed,
            BaseStats = new StrayBaseStats { MaxHp = 70, Attack = 14, Defense = 8, Speed = 22, Special = 12 },
            Biomes = new List<string> { "quiet" },
            EvolutionLevel = 20,
            EvolvedFormId = "uncanny_feline",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "nine_lives", "silent_paws" },
            PlaceholderColor = Color.Wheat,
            PlaceholderSize = 16
        });

        // Lawn Drone - Maintenance machine
        Register(new StrayDefinition
        {
            Id = "lawn_drone",
            Name = "Lawn Drone",
            Description = "Still tending lawns that no one lives in.",
            Type = StrayType.Insect,
            Role = StrayRole.Utility,
            BaseStats = new StrayBaseStats { MaxHp = 60, Attack = 10, Defense = 10, Speed = 14, Special = 14 },
            Biomes = new List<string> { "quiet" },
            EvolutionLevel = 16,
            EvolvedFormId = "garden_sentinel",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "blade_spin", "maintain" },
            PlaceholderColor = Color.LawnGreen,
            PlaceholderSize = 12
        });

        // Sprinkler Serpent - Water elemental
        Register(new StrayDefinition
        {
            Id = "sprinkler_serpent",
            Name = "Sprinkler Serpent",
            Description = "Emerged from the irrigation system. Controls water pressure.",
            Type = StrayType.Reptile,
            Role = StrayRole.Control,
            BaseStats = new StrayBaseStats { MaxHp = 85, Attack = 12, Defense = 10, Speed = 14, Special = 16 },
            Biomes = new List<string> { "quiet" },
            EvolutionLevel = 18,
            EvolvedFormId = "hydrant_hydra",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "water_jet", "pressure_blast" },
            PlaceholderColor = Color.CornflowerBlue,
            PlaceholderSize = 18
        });

        // Mailbox Mimic - Ambush predator
        Register(new StrayDefinition
        {
            Id = "mailbox_mimic",
            Name = "Mailbox Mimic",
            Description = "Waits patiently for prey. Has been waiting a long time.",
            Type = StrayType.Chimera,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 100, Attack = 18, Defense = 14, Speed = 8, Special = 10 },
            Biomes = new List<string> { "quiet" },
            EvolutionLevel = 22,
            EvolvedFormId = "postal_horror",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "ambush", "chomp", "disguise" },
            PlaceholderColor = Color.DarkBlue,
            PlaceholderSize = 18
        });

        // Garage Guardian - Protective entity
        Register(new StrayDefinition
        {
            Id = "garage_guardian",
            Name = "Garage Guardian",
            Description = "Protects a home that no longer exists.",
            Type = StrayType.Mammal,
            Role = StrayRole.Tank,
            BaseStats = new StrayBaseStats { MaxHp = 140, Attack = 12, Defense = 18, Speed = 6, Special = 10 },
            Biomes = new List<string> { "quiet" },
            EvolutionLevel = 24,
            EvolvedFormId = "home_fortress",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "guardian_stance", "territorial", "door_slam" },
            PlaceholderColor = Color.Gray,
            PlaceholderSize = 24
        });

        // Suburb Sentinel - Rare security system
        Register(new StrayDefinition
        {
            Id = "suburb_sentinel",
            Name = "Suburb Sentinel",
            Description = "The neighborhood watch evolved. Sees everything.",
            Type = StrayType.Avian,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 100, Attack = 14, Defense = 12, Speed = 16, Special = 20 },
            Biomes = new List<string> { "quiet" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "surveillance", "alert_system", "coordinate_defense" },
            PlaceholderColor = Color.SteelBlue,
            PlaceholderSize = 22,
            RecruitChance = 0.2f
        });

        // Perfect Pet - Rare uncanny entity
        Register(new StrayDefinition
        {
            Id = "perfect_pet",
            Name = "Perfect Pet",
            Description = "Too perfect. Too obedient. Something is wrong.",
            Type = StrayType.Canine,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 90, Attack = 10, Defense = 10, Speed = 14, Special = 24 },
            Biomes = new List<string> { "quiet" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "perfect_loyalty", "uncanny_presence", "mimic_emotion" },
            PlaceholderColor = Color.White,
            PlaceholderSize = 18,
            RecruitChance = 0.15f
        });

        // Uncanny Feline - House Cat evolution
        Register(new StrayDefinition
        {
            Id = "uncanny_feline",
            Name = "Uncanny Feline",
            Description = "Evolved House Cat. Its wrongness is now visible.",
            Type = StrayType.Feline,
            Role = StrayRole.Speed,
            BaseStats = new StrayBaseStats { MaxHp = 95, Attack = 18, Defense = 12, Speed = 26, Special = 16 },
            Biomes = new List<string> { "quiet" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "nine_lives", "silent_paws", "reality_slip", "dread_aura" },
            PlaceholderColor = Color.Ivory,
            PlaceholderSize = 20,
            CanRecruit = false
        });
    }

    /// <summary>
    /// Registers the Strays found in The Teeth biome.
    /// </summary>
    private static void RegisterTeethStrays()
    {
        // Turret Hawk - Aerial artillery
        Register(new StrayDefinition
        {
            Id = "turret_hawk",
            Name = "Turret Hawk",
            Description = "Integrated targeting systems make it a deadly marksman.",
            Type = StrayType.Avian,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 75, Attack = 22, Defense = 6, Speed = 20, Special = 12 },
            Biomes = new List<string> { "teeth" },
            EvolutionLevel = 26,
            EvolvedFormId = "artillery_eagle",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "lock_on", "precision_strike", "evasive_maneuvers" },
            PlaceholderColor = Color.Crimson,
            PlaceholderSize = 18
        });

        // Razor Hound - Weaponized canine
        Register(new StrayDefinition
        {
            Id = "razor_hound",
            Name = "Razor Hound",
            Description = "Blades instead of fur. Built for war.",
            Type = StrayType.Canine,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 95, Attack = 20, Defense = 12, Speed = 16, Special = 8 },
            Biomes = new List<string> { "teeth" },
            EvolutionLevel = 24,
            EvolvedFormId = "war_wolf",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "blade_fur", "rend", "bloodlust" },
            PlaceholderColor = Color.DarkRed,
            PlaceholderSize = 22
        });

        // Bunker Bear - Armored fortress
        Register(new StrayDefinition
        {
            Id = "bunker_bear",
            Name = "Bunker Bear",
            Description = "Armor-plated survivor. Impervious to most attacks.",
            Type = StrayType.Mammal,
            Role = StrayRole.Tank,
            BaseStats = new StrayBaseStats { MaxHp = 200, Attack = 14, Defense = 24, Speed = 4, Special = 8 },
            Biomes = new List<string> { "teeth" },
            EvolutionLevel = 28,
            EvolvedFormId = "fortress_ursine",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "bunker_mode", "armor_plating", "siege_stance" },
            PlaceholderColor = Color.DimGray,
            PlaceholderSize = 32
        });

        // Wall Crawler - Stealth infiltrator
        Register(new StrayDefinition
        {
            Id = "wall_crawler",
            Name = "Wall Crawler",
            Description = "Moves through any terrain. Silent and deadly.",
            Type = StrayType.Arachnid,
            Role = StrayRole.Speed,
            BaseStats = new StrayBaseStats { MaxHp = 65, Attack = 16, Defense = 8, Speed = 24, Special = 12 },
            Biomes = new List<string> { "teeth" },
            EvolutionLevel = 22,
            EvolvedFormId = "shadow_stalker",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "wall_cling", "shadow_step", "assassinate" },
            PlaceholderColor = Color.Black,
            PlaceholderSize = 14
        });

        // Sentry Spider - Defensive web builder
        Register(new StrayDefinition
        {
            Id = "sentry_spider",
            Name = "Sentry Spider",
            Description = "Webs that detect and trap intruders.",
            Type = StrayType.Arachnid,
            Role = StrayRole.Control,
            BaseStats = new StrayBaseStats { MaxHp = 80, Attack = 12, Defense = 14, Speed = 12, Special = 18 },
            Biomes = new List<string> { "teeth" },
            EvolutionLevel = 24,
            EvolvedFormId = "web_fortress",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "trap_web", "sensor_network", "entangle" },
            PlaceholderColor = Color.DarkSlateGray,
            PlaceholderSize = 16
        });

        // Fortress Titan - Rare boss creature
        Register(new StrayDefinition
        {
            Id = "fortress_titan",
            Name = "Fortress Titan",
            Description = "A walking fortress. Nearly indestructible.",
            Type = StrayType.Mammal,
            Role = StrayRole.Tank,
            BaseStats = new StrayBaseStats { MaxHp = 280, Attack = 16, Defense = 28, Speed = 2, Special = 10 },
            Biomes = new List<string> { "teeth" },
            MicrochipSlots = 4,
            InnateAbilities = new List<string> { "fortress_mode", "artillery_barrage", "unbreakable", "rally_defense" },
            PlaceholderColor = Color.SlateGray,
            PlaceholderSize = 40,
            RecruitChance = 0.1f
        });

        // Siege Wyrm - Rare destructive force
        Register(new StrayDefinition
        {
            Id = "siege_wyrm",
            Name = "Siege Wyrm",
            Description = "Burrows through fortifications. Unstoppable advance.",
            Type = StrayType.Reptile,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 150, Attack = 26, Defense = 18, Speed = 8, Special = 14 },
            Biomes = new List<string> { "teeth" },
            MicrochipSlots = 4,
            InnateAbilities = new List<string> { "burrow", "siege_breath", "wall_breaker", "earthquake" },
            PlaceholderColor = Color.Maroon,
            PlaceholderSize = 36,
            RecruitChance = 0.1f
        });

        // War Wolf - Razor Hound evolution
        Register(new StrayDefinition
        {
            Id = "war_wolf",
            Name = "War Wolf",
            Description = "Evolved Razor Hound. A living weapon.",
            Type = StrayType.Canine,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 130, Attack = 28, Defense = 16, Speed = 18, Special = 12 },
            Biomes = new List<string> { "teeth" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "blade_fur", "rend", "bloodlust", "war_cry", "execution" },
            PlaceholderColor = Color.DarkSlateBlue,
            PlaceholderSize = 28,
            CanRecruit = false
        });
    }

    /// <summary>
    /// Registers the Strays found in The Glow biome.
    /// </summary>
    private static void RegisterGlowStrays()
    {
        // Server Sprite - Data elemental
        Register(new StrayDefinition
        {
            Id = "server_sprite",
            Name = "Server Sprite",
            Description = "Pure data given form. Flickers in and out of existence.",
            Type = StrayType.Invertebrate,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 55, Attack = 8, Defense = 6, Speed = 22, Special = 24 },
            Biomes = new List<string> { "glow" },
            EvolutionLevel = 24,
            EvolvedFormId = "data_djinn",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "data_transfer", "buffer", "phase_shift" },
            PlaceholderColor = Color.Cyan,
            PlaceholderSize = 12
        });

        // Data Worm - System infiltrator
        Register(new StrayDefinition
        {
            Id = "data_worm",
            Name = "Data Worm",
            Description = "Burrows through code. Corrupts what it touches.",
            Type = StrayType.Invertebrate,
            Role = StrayRole.Control,
            BaseStats = new StrayBaseStats { MaxHp = 70, Attack = 14, Defense = 8, Speed = 16, Special = 18 },
            Biomes = new List<string> { "glow" },
            EvolutionLevel = 26,
            EvolvedFormId = "virus_wyrm",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "corrupt", "replicate", "system_exploit" },
            PlaceholderColor = Color.Purple,
            PlaceholderSize = 16
        });

        // Cache Cat - Memory keeper
        Register(new StrayDefinition
        {
            Id = "cache_cat",
            Name = "Cache Cat",
            Description = "Stores memories in its fur. Never forgets.",
            Type = StrayType.Feline,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 75, Attack = 12, Defense = 10, Speed = 18, Special = 20 },
            Biomes = new List<string> { "glow" },
            EvolutionLevel = 24,
            EvolvedFormId = "archive_panther",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "memory_cache", "recall", "data_shield" },
            PlaceholderColor = Color.MediumPurple,
            PlaceholderSize = 18
        });

        // Firewall Fox - Security system
        Register(new StrayDefinition
        {
            Id = "firewall_fox",
            Name = "Firewall Fox",
            Description = "Burns with protective fire. Blocks intrusions.",
            Type = StrayType.Canine,
            Role = StrayRole.Tank,
            BaseStats = new StrayBaseStats { MaxHp = 110, Attack = 14, Defense = 16, Speed = 14, Special = 16 },
            Biomes = new List<string> { "glow" },
            EvolutionLevel = 26,
            EvolvedFormId = "security_kitsune",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "firewall", "intrusion_detect", "burn_attack" },
            PlaceholderColor = Color.OrangeRed,
            PlaceholderSize = 20
        });

        // Bandwidth Bat - Speed specialist
        Register(new StrayDefinition
        {
            Id = "bandwidth_bat",
            Name = "Bandwidth Bat",
            Description = "Moves at transmission speeds. Here and gone.",
            Type = StrayType.Avian,
            Role = StrayRole.Speed,
            BaseStats = new StrayBaseStats { MaxHp = 60, Attack = 14, Defense = 6, Speed = 28, Special = 14 },
            Biomes = new List<string> { "glow" },
            EvolutionLevel = 22,
            EvolvedFormId = "quantum_flyer",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "high_speed", "echo_location", "data_burst" },
            PlaceholderColor = Color.DarkViolet,
            PlaceholderSize = 14
        });

        // Kernel Dragon - Rare core entity
        Register(new StrayDefinition
        {
            Id = "kernel_dragon",
            Name = "Kernel Dragon",
            Description = "A manifestation of core system processes. Incredibly powerful.",
            Type = StrayType.Reptile,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 160, Attack = 28, Defense = 16, Speed = 16, Special = 26 },
            Biomes = new List<string> { "glow" },
            MicrochipSlots = 5,
            InnateAbilities = new List<string> { "kernel_panic", "system_override", "process_breath", "root_access" },
            PlaceholderColor = Color.Gold,
            PlaceholderSize = 36,
            RecruitChance = 0.05f
        });

        // Root Access Bear - Rare admin entity
        Register(new StrayDefinition
        {
            Id = "root_access_bear",
            Name = "Root Access Bear",
            Description = "Has administrator privileges to reality itself.",
            Type = StrayType.Mammal,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 140, Attack = 16, Defense = 18, Speed = 10, Special = 30 },
            Biomes = new List<string> { "glow" },
            MicrochipSlots = 5,
            InnateAbilities = new List<string> { "sudo", "permission_grant", "system_restore", "admin_shield" },
            PlaceholderColor = Color.Yellow,
            PlaceholderSize = 30,
            RecruitChance = 0.05f
        });

        // Data Djinn - Server Sprite evolution
        Register(new StrayDefinition
        {
            Id = "data_djinn",
            Name = "Data Djinn",
            Description = "Evolved Server Sprite. Grants digital wishes.",
            Type = StrayType.Invertebrate,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 85, Attack = 12, Defense = 10, Speed = 24, Special = 32 },
            Biomes = new List<string> { "glow" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "data_transfer", "buffer", "phase_shift", "wish_grant", "reality_edit" },
            PlaceholderColor = Color.Aqua,
            PlaceholderSize = 20,
            CanRecruit = false
        });

        // Virus Wyrm - Data Worm evolution
        Register(new StrayDefinition
        {
            Id = "virus_wyrm",
            Name = "Virus Wyrm",
            Description = "Evolved Data Worm. A catastrophic system threat.",
            Type = StrayType.Invertebrate,
            Role = StrayRole.Control,
            BaseStats = new StrayBaseStats { MaxHp = 100, Attack = 20, Defense = 12, Speed = 18, Special = 26 },
            Biomes = new List<string> { "glow" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "corrupt", "replicate", "system_exploit", "mass_infection", "zero_day" },
            PlaceholderColor = Color.DarkMagenta,
            PlaceholderSize = 24,
            CanRecruit = false
        });
    }

    /// <summary>
    /// Registers the Strays found in The Archive Scar biome.
    /// </summary>
    private static void RegisterArchiveScarStrays()
    {
        // Memory Ghost - Fragmentary entity
        Register(new StrayDefinition
        {
            Id = "memory_ghost",
            Name = "Memory Ghost",
            Description = "An echo of something that was deleted. Half-real.",
            Type = StrayType.Invertebrate,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 50, Attack = 10, Defense = 4, Speed = 20, Special = 22 },
            Biomes = new List<string> { "archive_scar" },
            EvolutionLevel = 20,
            EvolvedFormId = "phantom_archive",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "intangible", "memory_drain", "haunt" },
            PlaceholderColor = Color.LightGray,
            PlaceholderSize = 14
        });

        // Deleted Dog - Partially erased
        Register(new StrayDefinition
        {
            Id = "deleted_dog",
            Name = "Deleted Dog",
            Description = "Parts of it are missing. Still loyal.",
            Type = StrayType.Canine,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 80, Attack = 16, Defense = 8, Speed = 16, Special = 14 },
            Biomes = new List<string> { "archive_scar" },
            EvolutionLevel = 22,
            EvolvedFormId = "void_hound",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "glitch_bite", "fragment_form", "loyalty" },
            PlaceholderColor = Color.DarkGray,
            PlaceholderSize = 18
        });

        // Corrupted Cat - Data decay
        Register(new StrayDefinition
        {
            Id = "corrupted_cat",
            Name = "Corrupted Cat",
            Description = "Its data is scrambled. Unpredictable behavior.",
            Type = StrayType.Feline,
            Role = StrayRole.Control,
            BaseStats = new StrayBaseStats { MaxHp = 70, Attack = 14, Defense = 8, Speed = 18, Special = 18 },
            Biomes = new List<string> { "archive_scar" },
            EvolutionLevel = 20,
            EvolvedFormId = "entropy_panther",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "data_corrupt", "random_glitch", "unstable_form" },
            PlaceholderColor = Color.MediumVioletRed,
            PlaceholderSize = 16
        });

        // Null Serpent - Void creature
        Register(new StrayDefinition
        {
            Id = "null_serpent",
            Name = "Null Serpent",
            Description = "Made of nothing. Its bite erases.",
            Type = StrayType.Reptile,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 75, Attack = 20, Defense = 6, Speed = 16, Special = 16 },
            Biomes = new List<string> { "archive_scar" },
            EvolutionLevel = 24,
            EvolvedFormId = "void_wyrm",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "null_bite", "erase", "void_coil" },
            PlaceholderColor = Color.Black,
            PlaceholderSize = 20
        });

        // Void Moth - Darkness flyer
        Register(new StrayDefinition
        {
            Id = "void_moth",
            Name = "Void Moth",
            Description = "Drawn to the gaps in reality.",
            Type = StrayType.Insect,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 55, Attack = 8, Defense = 6, Speed = 20, Special = 22 },
            Biomes = new List<string> { "archive_scar" },
            EvolutionLevel = 18,
            EvolvedFormId = "oblivion_butterfly",
            MicrochipSlots = 2,
            InnateAbilities = new List<string> { "void_dust", "darkness_shroud", "memory_siphon" },
            PlaceholderColor = Color.DarkSlateBlue,
            PlaceholderSize = 12
        });

        // Ancient Backup - Rare preserved entity
        Register(new StrayDefinition
        {
            Id = "ancient_backup",
            Name = "Ancient Backup",
            Description = "A complete backup from before the fall. Pristine.",
            Type = StrayType.Mammal,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 120, Attack = 14, Defense = 14, Speed = 14, Special = 26 },
            Biomes = new List<string> { "archive_scar" },
            MicrochipSlots = 4,
            InnateAbilities = new List<string> { "restore", "archive_knowledge", "pristine_form", "time_lock" },
            PlaceholderColor = Color.White,
            PlaceholderSize = 24,
            RecruitChance = 0.1f
        });

        // Original Instance - Rare first version
        Register(new StrayDefinition
        {
            Id = "original_instance",
            Name = "Original Instance",
            Description = "The first of its kind. All others are copies.",
            Type = StrayType.Chimera,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 130, Attack = 24, Defense = 16, Speed = 16, Special = 22 },
            Biomes = new List<string> { "archive_scar" },
            MicrochipSlots = 4,
            InnateAbilities = new List<string> { "original_power", "spawn_copy", "prime_strike", "legacy" },
            PlaceholderColor = Color.Gold,
            PlaceholderSize = 26,
            RecruitChance = 0.05f
        });

        // Phantom Archive - Memory Ghost evolution
        Register(new StrayDefinition
        {
            Id = "phantom_archive",
            Name = "Phantom Archive",
            Description = "Evolved Memory Ghost. Contains entire deleted histories.",
            Type = StrayType.Invertebrate,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 80, Attack = 14, Defense = 8, Speed = 22, Special = 30 },
            Biomes = new List<string> { "archive_scar" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "intangible", "memory_drain", "haunt", "archive_recall", "spectral_library" },
            PlaceholderColor = Color.GhostWhite,
            PlaceholderSize = 22,
            CanRecruit = false
        });

        // Void Hound - Deleted Dog evolution
        Register(new StrayDefinition
        {
            Id = "void_hound",
            Name = "Void Hound",
            Description = "Evolved Deleted Dog. Hunts between realities.",
            Type = StrayType.Canine,
            Role = StrayRole.Damage,
            BaseStats = new StrayBaseStats { MaxHp = 110, Attack = 22, Defense = 12, Speed = 20, Special = 18 },
            Biomes = new List<string> { "archive_scar" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "glitch_bite", "fragment_form", "loyalty", "void_hunt", "reality_tear" },
            PlaceholderColor = Color.MidnightBlue,
            PlaceholderSize = 24,
            CanRecruit = false
        });

        // Oblivion Butterfly - Void Moth evolution
        Register(new StrayDefinition
        {
            Id = "oblivion_butterfly",
            Name = "Oblivion Butterfly",
            Description = "Evolved Void Moth. Beautiful and terrifying.",
            Type = StrayType.Insect,
            Role = StrayRole.Support,
            BaseStats = new StrayBaseStats { MaxHp = 80, Attack = 12, Defense = 10, Speed = 22, Special = 30 },
            Biomes = new List<string> { "archive_scar" },
            MicrochipSlots = 3,
            InnateAbilities = new List<string> { "void_dust", "darkness_shroud", "memory_siphon", "oblivion_wings", "entropy_dance" },
            PlaceholderColor = Color.Indigo,
            PlaceholderSize = 18,
            CanRecruit = false
        });
    }
}
