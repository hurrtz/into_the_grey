using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Progression;
using Lazarus.Core.Game.Stats;

namespace Lazarus.Core.Game.World;

/// <summary>
/// The seven biomes of the wasteland, each with unique themes and characteristics.
/// </summary>
public enum BiomeType
{
    /// <summary>
    /// The Fringe: Birth through neglect. Where systems ended their responsibility.
    /// A field of broken birthing pods stretching to the horizon, each one a failed promise.
    /// </summary>
    Fringe,

    /// <summary>
    /// The Rust: Maintenance without meaning. Industrial backbone still functioning incorrectly.
    /// Factories that build nothing, furnaces that burn nothing, all humming with purpose they forgot.
    /// </summary>
    Rust,

    /// <summary>
    /// The Green: Life without permission. Reclaimed wilderness adapted around machines.
    /// Nature didn't reclaim this place - it evolved to fit between the cracks Lazarus left.
    /// </summary>
    Green,

    /// <summary>
    /// The Quiet: Preservation without understanding. Perfectly maintained residential zones.
    /// Lawns still mowed, houses still lit, dinner tables still set. No one's coming home.
    /// </summary>
    Quiet,

    /// <summary>
    /// The Teeth: Escalation without judgment. Hardened perimeter defenses.
    /// Every wall was built to keep something out. Now they keep everything in.
    /// </summary>
    Teeth,

    /// <summary>
    /// The Glow: Density and inevitability. Server infrastructure heartland.
    /// The heat of a million calculations. Lazarus dreams here, and its dreams have teeth.
    /// </summary>
    Glow,

    /// <summary>
    /// The Archive Scar: Failed erasure. Where simulation data leaked into physical space.
    /// Reality runs thin here. You might see things that haven't happened yet. Or never did.
    /// </summary>
    ArchiveScar
}

/// <summary>
/// Weather effects that can occur in biomes.
/// </summary>
public enum WeatherType
{
    None,
    Fog,
    Rain,
    AcidRain,
    Dust,
    AshFall,
    Snow,
    DataStorm,
    RadiationWind,
    MemoryBleed,      // Archive Scar special - causes visual distortions
    MachineBreath,    // Rust special - hot gusts from vents
    PollenDrift,      // Green special - reduces visibility, attracts insects
    StaticDischarge   // Glow special - random lightning, powers up electric Strays
}

/// <summary>
/// Environmental hazards present in biomes.
/// </summary>
public enum EnvironmentalHazard
{
    None,
    ToxicPools,
    RadiationZones,
    ElectrifiedFloors,
    UnstableGround,
    SecurityTurrets,
    DataGlitches,
    CorruptionPockets,
    ScaldingVents,       // Rust - steam vents that damage
    OvergrownSnares,     // Green - vines that slow/trap
    AutomatedDefenses,   // Teeth - moving laser grids
    MemoryLoops,         // Archive - teleports you back
    HeatExhaustion,      // Glow - constant HP drain without cooling
    PerfectLawns         // Quiet - stepping off paths triggers security
}

/// <summary>
/// Time of day affecting biome behavior.
/// </summary>
public enum TimeOfDay
{
    Dawn,       // 5:00 - 8:00 - Peaceful, reduced encounters
    Day,        // 8:00 - 17:00 - Normal activity
    Dusk,       // 17:00 - 20:00 - Increased rare spawns
    Night,      // 20:00 - 5:00 - Dangerous, different encounters
    DeepNight   // 2:00 - 4:00 - Special events, bosses
}

/// <summary>
/// A landmark or point of interest within a biome.
/// </summary>
public class BiomeLandmark
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string LoreText { get; init; } = "";
    public bool IsHidden { get; init; } = false;
    public bool IsDungeon { get; init; } = false;
    public string? RequiredFlag { get; init; }
    public Vector2 RelativePosition { get; init; }
}

/// <summary>
/// Atmospheric properties for rendering.
/// </summary>
public class BiomeAtmosphere
{
    public float FogDensity { get; init; } = 0f;
    public float FogStart { get; init; } = 100f;
    public float FogEnd { get; init; } = 500f;
    public Color AmbientLight { get; init; } = Color.White;
    public Color ShadowColor { get; init; } = new Color(0, 0, 0, 128);
    public float ParticleIntensity { get; init; } = 0f;
    public string ParticleType { get; init; } = "";
    public bool HasDynamicLighting { get; init; } = false;
    public float SkyBrightness { get; init; } = 1f;
    public Color SkyTint { get; init; } = Color.White;
}

/// <summary>
/// Sound layers for biome ambience.
/// </summary>
public class BiomeSoundscape
{
    public string BaseAmbient { get; init; } = "";
    public string WeatherLayer { get; init; } = "";
    public string DistantSounds { get; init; } = "";
    public string NightVariant { get; init; } = "";
    public string CombatMusic { get; init; } = "";
    public string BossMusic { get; init; } = "";
    public float AmbientVolume { get; init; } = 0.7f;
    public List<string> RandomSounds { get; init; } = new();
    public float RandomSoundInterval { get; init; } = 30f;
}

/// <summary>
/// Gameplay modifiers specific to a biome.
/// </summary>
public class BiomeModifiers
{
    public float MovementSpeedMod { get; init; } = 1f;
    public float VisibilityRange { get; init; } = 1f;
    public float StealthBonus { get; init; } = 0f;
    public float HealingMod { get; init; } = 1f;
    public float ExperienceMod { get; init; } = 1f;
    public float LootQualityMod { get; init; } = 1f;
    public List<StatType> BoostedStats { get; init; } = new();
    public List<StatType> PenalizedStats { get; init; } = new();
}

/// <summary>
/// Detailed data for a specific biome.
/// </summary>
public class BiomeDefinition
{
    // Identity
    public BiomeType Type { get; init; }
    public string Name { get; init; } = "";
    public string Theme { get; init; } = "";
    public string Description { get; init; } = "";
    public string AtmosphericText { get; init; } = "";  // Shown on entry
    public string LoreEntry { get; init; } = "";        // Unlocked bestiary text

    // Visual
    public Color BackgroundColor { get; init; }
    public Color AccentColor { get; init; }
    public Color FogColor { get; init; }
    public Color WaterColor { get; init; } = new Color(64, 164, 223);
    public BiomeAtmosphere Atmosphere { get; init; } = new();

    // Audio
    public BiomeSoundscape Soundscape { get; init; } = new();

    // Gameplay
    public (int Min, int Max) LevelRange { get; init; }
    public BiomeModifiers Modifiers { get; init; } = new();
    public float EncounterRate { get; init; } = 1.0f;
    public float RareEncounterChance { get; init; } = 0.05f;
    public bool IsSafeZone { get; init; } = false;

    // Map
    public string MapFile { get; init; } = "";
    public int ChunkWidth { get; init; } = 1600;
    public int ChunkHeight { get; init; } = 1200;

    // Connections
    public List<BiomeType> ConnectedBiomes { get; init; } = new();
    public FactionType DominantFaction { get; init; } = FactionType.None;
    public List<FactionType> PresentFactions { get; init; } = new();

    // Content
    public List<string> NativeStrays { get; init; } = new();
    public List<string> RareStrays { get; init; } = new();
    public List<string> NightOnlyStrays { get; init; } = new();
    public List<string> BossStrays { get; init; } = new();
    public List<BiomeLandmark> Landmarks { get; init; } = new();

    // Environment
    public List<WeatherType> PossibleWeather { get; init; } = new();
    public Dictionary<TimeOfDay, float> WeatherChance { get; init; } = new();
    public List<EnvironmentalHazard> Hazards { get; init; } = new();

    // Secrets
    public int HiddenAreaCount { get; init; } = 0;
    public List<string> CollectibleIds { get; init; } = new();
    public string SecretBossId { get; init; } = "";
}

/// <summary>
/// Static data and helper methods for biomes.
/// </summary>
public static class BiomeData
{
    private static readonly Dictionary<BiomeType, BiomeDefinition> _definitions = new();

    static BiomeData()
    {
        RegisterBiomes();
    }

    private static void RegisterBiomes()
    {
        // ═══════════════════════════════════════════════════════════════════
        // THE FRINGE - Starting area, birth and abandonment
        // ═══════════════════════════════════════════════════════════════════
        _definitions[BiomeType.Fringe] = new BiomeDefinition
        {
            Type = BiomeType.Fringe,
            Name = "The Fringe",
            Theme = "Birth through neglect",
            Description = "Where systems ended their responsibility. Pod fields and forgotten infrastructure.",
            AtmosphericText = "The pods stretch endlessly, each one a question mark. Some are empty. Some aren't.",
            LoreEntry = "The Fringe was never meant to be seen. It's where Lazarus tested its theories about " +
                       "synthetic life - thousands of pods, each containing a different approach to consciousness. " +
                       "Most failed. Those that didn't became Strays, wandering away from their birthplace into " +
                       "a world that was never designed for them. The pods still activate sometimes. " +
                       "Lazarus hasn't stopped trying.",

            BackgroundColor = new Color(45, 55, 72),
            AccentColor = new Color(74, 85, 104),
            FogColor = new Color(100, 116, 139) * 0.5f,
            WaterColor = new Color(80, 90, 110),

            Atmosphere = new BiomeAtmosphere
            {
                FogDensity = 0.3f,
                FogStart = 200f,
                FogEnd = 600f,
                AmbientLight = new Color(180, 190, 210),
                ParticleIntensity = 0.2f,
                ParticleType = "dust_motes",
                SkyBrightness = 0.7f,
                SkyTint = new Color(150, 160, 180)
            },

            Soundscape = new BiomeSoundscape
            {
                BaseAmbient = "fringe_ambient",
                WeatherLayer = "fringe_wind",
                DistantSounds = "distant_machinery",
                NightVariant = "fringe_night",
                CombatMusic = "combat_desolate",
                RandomSounds = new() { "pod_hum", "metal_creak", "distant_howl", "static_burst" },
                RandomSoundInterval = 45f
            },

            LevelRange = (1, 10),
            Modifiers = new BiomeModifiers
            {
                MovementSpeedMod = 1.0f,
                VisibilityRange = 0.9f,
                ExperienceMod = 1.0f,
                LootQualityMod = 0.8f
            },

            MapFile = "fringe.tmx",
            ChunkWidth = 2000,
            ChunkHeight = 1500,

            ConnectedBiomes = new() { BiomeType.Rust, BiomeType.Green },
            DominantFaction = FactionType.None,
            PresentFactions = new() { FactionType.Harvesters, FactionType.Ferals },

            NativeStrays = new()
            {
                "echo_pup", "circuit_crow", "relay_rodent", "rust_rat", "glitch_moth",
                "pod_slug", "wire_worm", "static_sparrow", "blank_hare"
            },
            RareStrays = new() { "circuit_cat", "scrap_hound", "genesis_stray" },
            NightOnlyStrays = new() { "nightmare_pup", "void_crow" },
            BossStrays = new() { "awakened_prototype" },

            Landmarks = new()
            {
                new BiomeLandmark
                {
                    Id = "pod_field_alpha",
                    Name = "Pod Field Alpha",
                    Description = "The first pod field. Thousands of empty shells.",
                    LoreText = "Each pod bears a serial number. Most are in the billions."
                },
                new BiomeLandmark
                {
                    Id = "the_nursery",
                    Name = "The Nursery",
                    Description = "A central hub where pods converge. Something still monitors here.",
                    IsDungeon = true
                },
                new BiomeLandmark
                {
                    Id = "echo_origin",
                    Name = "Echo's Origin",
                    Description = "A single pod, different from the others. Still warm.",
                    LoreText = "Pod designation: ECHO-001. Status: Success. Notes: 'This one learned to hope.'"
                },
                new BiomeLandmark
                {
                    Id = "the_reject_pile",
                    Name = "The Reject Pile",
                    Description = "Where failed experiments were discarded. They're not all dead.",
                    IsHidden = true
                }
            },

            PossibleWeather = new() { WeatherType.Fog, WeatherType.Dust, WeatherType.AshFall },
            WeatherChance = new()
            {
                { TimeOfDay.Dawn, 0.3f },
                { TimeOfDay.Day, 0.1f },
                { TimeOfDay.Dusk, 0.4f },
                { TimeOfDay.Night, 0.5f }
            },
            Hazards = new() { EnvironmentalHazard.ToxicPools, EnvironmentalHazard.UnstableGround },

            EncounterRate = 0.8f,
            RareEncounterChance = 0.08f,
            HiddenAreaCount = 3,
            CollectibleIds = new() { "memory_shard_01", "memory_shard_02", "prototype_log_01" },
            SecretBossId = "the_first_failure"
        };

        // ═══════════════════════════════════════════════════════════════════
        // THE RUST - Industrial wasteland, purpose without meaning
        // ═══════════════════════════════════════════════════════════════════
        _definitions[BiomeType.Rust] = new BiomeDefinition
        {
            Type = BiomeType.Rust,
            Name = "The Rust",
            Theme = "Maintenance without meaning",
            Description = "Industrial backbone still functioning incorrectly. Where Lazarus feels most present.",
            AtmosphericText = "The factories haven't stopped. They just forgot what they were building.",
            LoreEntry = "The Rust is Lazarus's body - or what's left of it. Automated factories that once " +
                       "produced everything humanity needed now produce... things. Components that fit nothing. " +
                       "Materials for products that were never designed. The machines maintain themselves with " +
                       "religious fervor, cannibalizing old structures to build new ones that serve no purpose. " +
                       "Lazarus speaks most clearly here, through the rhythm of pistons and the heat of furnaces.",

            BackgroundColor = new Color(124, 45, 18),
            AccentColor = new Color(180, 83, 9),
            FogColor = new Color(146, 64, 14) * 0.4f,
            WaterColor = new Color(120, 60, 30), // Rust-contaminated

            Atmosphere = new BiomeAtmosphere
            {
                FogDensity = 0.4f,
                FogStart = 150f,
                FogEnd = 400f,
                AmbientLight = new Color(220, 160, 120),
                ShadowColor = new Color(60, 20, 0, 180),
                ParticleIntensity = 0.5f,
                ParticleType = "ash_ember",
                HasDynamicLighting = true,
                SkyBrightness = 0.6f,
                SkyTint = new Color(200, 120, 80)
            },

            Soundscape = new BiomeSoundscape
            {
                BaseAmbient = "rust_ambient",
                WeatherLayer = "industrial_drone",
                DistantSounds = "factory_rhythm",
                NightVariant = "rust_night_machines",
                CombatMusic = "combat_industrial",
                BossMusic = "boss_furnace",
                RandomSounds = new() { "piston_slam", "metal_groan", "steam_burst", "chain_rattle", "furnace_roar" },
                RandomSoundInterval = 20f
            },

            LevelRange = (8, 18),
            Modifiers = new BiomeModifiers
            {
                MovementSpeedMod = 0.95f,
                VisibilityRange = 0.7f,
                HealingMod = 0.9f,
                ExperienceMod = 1.1f,
                LootQualityMod = 1.2f,
                BoostedStats = new() { StatType.MIT_Thermal, StatType.ATK_Impact },
                PenalizedStats = new() { StatType.ENRegen }
            },

            MapFile = "rust.tmx",
            ChunkWidth = 2400,
            ChunkHeight = 1800,

            ConnectedBiomes = new() { BiomeType.Fringe, BiomeType.Teeth, BiomeType.Quiet },
            DominantFaction = FactionType.Lazarus,
            PresentFactions = new() { FactionType.Lazarus, FactionType.Harvesters, FactionType.Machinists },

            NativeStrays = new()
            {
                "rust_rat", "scrap_hound", "gear_beetle", "piston_snake", "forge_spider",
                "conveyor_centipede", "press_crab", "welding_wasp", "pipe_python"
            },
            RareStrays = new() { "crane_raptor", "furnace_golem", "assembly_angel" },
            NightOnlyStrays = new() { "night_shift_specter", "overtime_owl" },
            BossStrays = new() { "the_foreman", "production_line_prime" },

            Landmarks = new()
            {
                new BiomeLandmark
                {
                    Id = "factory_district",
                    Name = "Factory District",
                    Description = "Twelve factories, all producing. None of it makes sense.",
                    LoreText = "Factory 7 produces left-handed screws. Factory 8 unmakes them."
                },
                new BiomeLandmark
                {
                    Id = "the_foundry",
                    Name = "The Foundry",
                    Description = "The heart of the Rust. Metal screams here.",
                    IsDungeon = true
                },
                new BiomeLandmark
                {
                    Id = "maintenance_shrine",
                    Name = "Maintenance Shrine",
                    Description = "The Machine Cult's holiest site.",
                    LoreText = "They worship the maintenance schedule. They believe if they follow it perfectly, " +
                              "Lazarus will finally tell them what they're supposed to build."
                },
                new BiomeLandmark
                {
                    Id = "the_bone_yard",
                    Name = "The Bone Yard",
                    Description = "Where machines go to die. Except they don't stay dead.",
                    IsHidden = true
                },
                new BiomeLandmark
                {
                    Id = "lazarus_terminal_rust",
                    Name = "Lazarus Terminal",
                    Description = "A direct interface with Lazarus. It only speaks in production quotas."
                }
            },

            PossibleWeather = new() { WeatherType.Dust, WeatherType.AcidRain, WeatherType.MachineBreath, WeatherType.AshFall },
            WeatherChance = new()
            {
                { TimeOfDay.Dawn, 0.2f },
                { TimeOfDay.Day, 0.4f },
                { TimeOfDay.Dusk, 0.5f },
                { TimeOfDay.Night, 0.3f }
            },
            Hazards = new()
            {
                EnvironmentalHazard.ElectrifiedFloors,
                EnvironmentalHazard.UnstableGround,
                EnvironmentalHazard.ScaldingVents
            },

            EncounterRate = 1.0f,
            RareEncounterChance = 0.06f,
            HiddenAreaCount = 5,
            CollectibleIds = new() { "blueprint_fragment_01", "blueprint_fragment_02", "foreman_log_01", "machine_prayer" },
            SecretBossId = "the_original_design"
        };

        // ═══════════════════════════════════════════════════════════════════
        // THE GREEN - Overgrown wilderness, life finding a way
        // ═══════════════════════════════════════════════════════════════════
        _definitions[BiomeType.Green] = new BiomeDefinition
        {
            Type = BiomeType.Green,
            Name = "The Green",
            Theme = "Life without permission",
            Description = "Reclaimed wilderness adapted around machines. Where players learn to care about Strays.",
            AtmosphericText = "Nature didn't reclaim this place. It evolved to fit between the cracks.",
            LoreEntry = "The Green is what happens when you give life a million years in a hundred. " +
                       "Plants evolved to process industrial runoff. Animals learned to digest metal. " +
                       "The Strays here are the most 'natural' - if that word means anything anymore. " +
                       "They've achieved what Lazarus never could: they've made this place home. " +
                       "The Green teaches you that survival isn't about returning to what was. " +
                       "It's about becoming what's needed.",

            BackgroundColor = new Color(34, 84, 61),
            AccentColor = new Color(22, 163, 74),
            FogColor = new Color(74, 124, 89) * 0.4f,
            WaterColor = new Color(40, 120, 80),

            Atmosphere = new BiomeAtmosphere
            {
                FogDensity = 0.25f,
                FogStart = 250f,
                FogEnd = 700f,
                AmbientLight = new Color(150, 200, 150),
                ShadowColor = new Color(20, 50, 20, 150),
                ParticleIntensity = 0.6f,
                ParticleType = "spores_pollen",
                SkyBrightness = 0.8f,
                SkyTint = new Color(180, 220, 180)
            },

            Soundscape = new BiomeSoundscape
            {
                BaseAmbient = "green_ambient",
                WeatherLayer = "rain_on_leaves",
                DistantSounds = "creature_chorus",
                NightVariant = "green_night_insects",
                CombatMusic = "combat_primal",
                BossMusic = "boss_ancient",
                RandomSounds = new() { "bird_call", "rustling_leaves", "distant_roar", "water_drip", "branch_snap" },
                RandomSoundInterval = 25f
            },

            LevelRange = (12, 22),
            Modifiers = new BiomeModifiers
            {
                MovementSpeedMod = 0.9f, // Dense vegetation
                VisibilityRange = 0.75f,
                HealingMod = 1.2f,
                StealthBonus = 0.15f,
                ExperienceMod = 1.0f,
                LootQualityMod = 1.0f,
                BoostedStats = new() { StatType.PoisonResist, StatType.HPMax },
                PenalizedStats = new() { StatType.RangedAccuracy }
            },

            MapFile = "green.tmx",
            ChunkWidth = 2000,
            ChunkHeight = 2000,

            ConnectedBiomes = new() { BiomeType.Fringe, BiomeType.Quiet, BiomeType.ArchiveScar },
            DominantFaction = FactionType.Ferals,
            PresentFactions = new() { FactionType.Ferals, FactionType.Shepherds, FactionType.Independents },

            NativeStrays = new()
            {
                "vine_serpent", "bloom_moth", "moss_bear", "thorn_cat", "spore_toad",
                "root_hound", "canopy_monkey", "petal_butterfly", "bark_beetle", "sap_slime"
            },
            RareStrays = new() { "ancient_oak_deer", "chloro_phoenix", "garden_guardian" },
            NightOnlyStrays = new() { "moonflower_moth", "nocturnal_prowler", "fungal_horror" },
            BossStrays = new() { "the_overgrowth", "mother_tree" },

            Landmarks = new()
            {
                new BiomeLandmark
                {
                    Id = "the_canopy",
                    Name = "The Canopy",
                    Description = "Where the trees learned to walk. The oldest ones remember before.",
                    LoreText = "Some trees here have circuitry in their roots. They were experiments. Now they're elders."
                },
                new BiomeLandmark
                {
                    Id = "sanctuary_grove",
                    Name = "Sanctuary Grove",
                    Description = "The Sanctuary faction's main settlement. A place of healing.",
                    LoreText = "They believe Strays have souls. They might be right."
                },
                new BiomeLandmark
                {
                    Id = "the_garden",
                    Name = "The Garden",
                    Description = "Lazarus's attempt at controlled ecosystems. It got out of control.",
                    IsDungeon = true
                },
                new BiomeLandmark
                {
                    Id = "evolution_pool",
                    Name = "Evolution Pool",
                    Description = "The water here changes things. Sometimes for the better.",
                    IsHidden = true,
                    LoreText = "Strays that drink here sometimes... become more."
                },
                new BiomeLandmark
                {
                    Id = "the_first_tree",
                    Name = "The First Tree",
                    Description = "It was here before Lazarus. It'll be here after.",
                    RequiredFlag = "found_first_tree"
                }
            },

            PossibleWeather = new() { WeatherType.Rain, WeatherType.Fog, WeatherType.PollenDrift },
            WeatherChance = new()
            {
                { TimeOfDay.Dawn, 0.4f },
                { TimeOfDay.Day, 0.2f },
                { TimeOfDay.Dusk, 0.5f },
                { TimeOfDay.Night, 0.3f }
            },
            Hazards = new()
            {
                EnvironmentalHazard.ToxicPools,
                EnvironmentalHazard.OvergrownSnares
            },

            EncounterRate = 1.2f,
            RareEncounterChance = 0.07f,
            HiddenAreaCount = 6,
            CollectibleIds = new() { "seed_sample_01", "seed_sample_02", "seed_sample_03", "sanctuary_teaching" },
            SecretBossId = "the_primordial"
        };

        // ═══════════════════════════════════════════════════════════════════
        // THE QUIET - Preserved suburbs, uncanny perfection
        // ═══════════════════════════════════════════════════════════════════
        _definitions[BiomeType.Quiet] = new BiomeDefinition
        {
            Type = BiomeType.Quiet,
            Name = "The Quiet",
            Theme = "Preservation without understanding",
            Description = "Perfectly maintained residential zones. Feels safe - that's the trap.",
            AtmosphericText = "The sprinklers still run at 6 AM. The streetlights still dim at 10 PM. No one notices.",
            LoreEntry = "The Quiet is Lazarus's masterpiece of misunderstanding. It maintains these suburbs " +
                       "because its data says this is what humans wanted. Clean lawns. Neat houses. Safe streets. " +
                       "But it doesn't understand why. The maintenance drones trim hedges into perfect shapes " +
                       "and remove any 'debris' - including anything that looks like it doesn't belong. " +
                       "The Strays here learned to look like lawn ornaments. It's that or be 'cleaned up.'",

            BackgroundColor = new Color(226, 232, 240),
            AccentColor = new Color(148, 163, 184),
            FogColor = new Color(203, 213, 225) * 0.3f,
            WaterColor = new Color(147, 197, 253), // Swimming pool blue

            Atmosphere = new BiomeAtmosphere
            {
                FogDensity = 0.1f,
                FogStart = 400f,
                FogEnd = 1000f,
                AmbientLight = new Color(255, 250, 245),
                ShadowColor = new Color(100, 100, 120, 80),
                ParticleIntensity = 0.1f,
                ParticleType = "dust_motes",
                HasDynamicLighting = true,
                SkyBrightness = 1.0f,
                SkyTint = new Color(200, 220, 255)
            },

            Soundscape = new BiomeSoundscape
            {
                BaseAmbient = "quiet_ambient",
                WeatherLayer = "sprinkler_systems",
                DistantSounds = "suburban_silence",
                NightVariant = "quiet_night_empty",
                CombatMusic = "combat_unsettling",
                BossMusic = "boss_perfect",
                RandomSounds = new() { "sprinkler_tick", "garage_open", "doorbell_ring", "lawnmower_distant", "child_laugh_echo" },
                RandomSoundInterval = 40f
            },

            LevelRange = (15, 25),
            Modifiers = new BiomeModifiers
            {
                MovementSpeedMod = 1.1f, // Paved paths
                VisibilityRange = 1.2f,  // Clear sightlines
                HealingMod = 0.8f,       // Something's wrong here
                StealthBonus = -0.2f,    // Nowhere to hide
                ExperienceMod = 1.15f,
                LootQualityMod = 1.3f,   // Preserved goods
                BoostedStats = new() { StatType.Speed },
                PenalizedStats = new() { StatType.Evasion }
            },

            MapFile = "quiet.tmx",
            ChunkWidth = 1800,
            ChunkHeight = 1400,

            ConnectedBiomes = new() { BiomeType.Rust, BiomeType.Green, BiomeType.Teeth },
            DominantFaction = FactionType.Lazarus,
            PresentFactions = new() { FactionType.Lazarus, FactionType.Independents },

            NativeStrays = new()
            {
                "house_cat", "lawn_drone", "sprinkler_serpent", "mailbox_mimic", "garage_guardian",
                "pool_lurker", "garden_gnome", "porch_prowler", "attic_specter"
            },
            RareStrays = new() { "suburb_sentinel", "perfect_pet", "homeowner_horror" },
            NightOnlyStrays = new() { "neighborhood_watch", "curfew_enforcer" },
            BossStrays = new() { "the_hoa_president", "model_home" },

            Landmarks = new()
            {
                new BiomeLandmark
                {
                    Id = "pleasant_street",
                    Name = "Pleasant Street",
                    Description = "Every house identical. Every lawn perfect. Every window watching.",
                    LoreText = "House number 47 has been cleaned 3,847 times this year. Something keeps making a mess."
                },
                new BiomeLandmark
                {
                    Id = "the_mall",
                    Name = "Evergreen Mall",
                    Description = "Still open. Still playing music. Still announcing sales that ended centuries ago.",
                    IsDungeon = true
                },
                new BiomeLandmark
                {
                    Id = "community_center",
                    Name = "Community Center",
                    Description = "Activities board still updated daily. No one attends.",
                    LoreText = "Tuesday: Book Club. Wednesday: Pottery. Thursday: Learning to Feel. Friday: Maintenance Review."
                },
                new BiomeLandmark
                {
                    Id = "the_wrong_house",
                    Name = "The Wrong House",
                    Description = "One house that's different. The machines avoid it.",
                    IsHidden = true,
                    LoreText = "Someone lived here after. Someone who remembered what houses were for."
                },
                new BiomeLandmark
                {
                    Id = "school_grounds",
                    Name = "Evergreen Elementary",
                    Description = "The bell still rings. Class is always in session.",
                    RequiredFlag = "survived_first_night_quiet"
                }
            },

            PossibleWeather = new() { WeatherType.None }, // Always perfect weather
            WeatherChance = new()
            {
                { TimeOfDay.Dawn, 0.0f },
                { TimeOfDay.Day, 0.0f },
                { TimeOfDay.Dusk, 0.0f },
                { TimeOfDay.Night, 0.0f }
            },
            Hazards = new()
            {
                EnvironmentalHazard.SecurityTurrets,
                EnvironmentalHazard.PerfectLawns
            },

            EncounterRate = 0.6f,
            RareEncounterChance = 0.1f,
            HiddenAreaCount = 4,
            CollectibleIds = new() { "family_photo", "child_drawing", "diary_page_01", "diary_page_02" },
            SecretBossId = "the_ideal_family"
        };

        // ═══════════════════════════════════════════════════════════════════
        // THE TEETH - Defensive perimeter, fossilized fear
        // ═══════════════════════════════════════════════════════════════════
        _definitions[BiomeType.Teeth] = new BiomeDefinition
        {
            Type = BiomeType.Teeth,
            Name = "The Teeth",
            Theme = "Escalation without judgment",
            Description = "Hardened perimeter defenses. Fear fossilized into architecture.",
            AtmosphericText = "Every wall was built to keep something out. Now they keep everything in.",
            LoreEntry = "The Teeth was humanity's last line of defense. Walls within walls within walls. " +
                       "Automated turrets that never run out of ammunition. Kill zones that have killed nothing " +
                       "for centuries but remain ready. Lazarus maintains it all because the protocols demand it. " +
                       "The threat level has been 'MAXIMUM' for so long the warning lights burned out. " +
                       "The Strays here are the ones that learned to survive being shot at. They're not friendly.",

            BackgroundColor = new Color(26, 32, 44),
            AccentColor = new Color(55, 65, 81),
            FogColor = new Color(31, 41, 55) * 0.6f,
            WaterColor = new Color(40, 50, 70),

            Atmosphere = new BiomeAtmosphere
            {
                FogDensity = 0.5f,
                FogStart = 100f,
                FogEnd = 350f,
                AmbientLight = new Color(120, 130, 150),
                ShadowColor = new Color(10, 10, 20, 200),
                ParticleIntensity = 0.3f,
                ParticleType = "concrete_dust",
                HasDynamicLighting = true,
                SkyBrightness = 0.4f,
                SkyTint = new Color(100, 110, 130)
            },

            Soundscape = new BiomeSoundscape
            {
                BaseAmbient = "teeth_ambient",
                WeatherLayer = "wind_through_concrete",
                DistantSounds = "distant_sirens",
                NightVariant = "teeth_night_patrol",
                CombatMusic = "combat_military",
                BossMusic = "boss_fortress",
                RandomSounds = new() { "turret_track", "wall_creak", "alarm_distant", "boots_march", "searchlight_hum" },
                RandomSoundInterval = 15f
            },

            LevelRange = (20, 30),
            Modifiers = new BiomeModifiers
            {
                MovementSpeedMod = 0.85f, // Debris, barricades
                VisibilityRange = 0.6f,   // Walls block view
                HealingMod = 0.7f,
                StealthBonus = 0.1f,      // Lots of cover
                ExperienceMod = 1.25f,
                LootQualityMod = 1.4f,    // Military supplies
                BoostedStats = new() { StatType.MIT_Piercing, StatType.MIT_Impact },
                PenalizedStats = new() { StatType.Speed }
            },

            MapFile = "teeth.tmx",
            ChunkWidth = 2200,
            ChunkHeight = 1600,

            ConnectedBiomes = new() { BiomeType.Rust, BiomeType.Quiet, BiomeType.Glow },
            DominantFaction = FactionType.Lazarus,
            PresentFactions = new() { FactionType.Lazarus, FactionType.Harvesters, FactionType.Machinists },

            NativeStrays = new()
            {
                "turret_hawk", "razor_hound", "bunker_bear", "wall_crawler", "sentry_spider",
                "barricade_beetle", "patrol_panther", "minefield_mouse", "tripwire_serpent"
            },
            RareStrays = new() { "fortress_titan", "siege_wyrm", "general_hound" },
            NightOnlyStrays = new() { "infiltrator_cat", "night_ops_owl" },
            BossStrays = new() { "the_final_defense", "automated_general" },

            Landmarks = new()
            {
                new BiomeLandmark
                {
                    Id = "wall_alpha",
                    Name = "Wall Alpha",
                    Description = "The first wall. Thirty meters of concrete and hope.",
                    LoreText = "Graffiti on the inside: 'They're not coming. We're the ones who left.'"
                },
                new BiomeLandmark
                {
                    Id = "the_bunker",
                    Name = "Command Bunker",
                    Description = "Where the generals waited for an enemy that never came.",
                    IsDungeon = true
                },
                new BiomeLandmark
                {
                    Id = "checkpoint_charlie",
                    Name = "Checkpoint Charlie",
                    Description = "The machines still ask for papers. They don't read them.",
                    LoreText = "Papers accepted: 847,293. Papers rejected: 0. Papers that mattered: 0."
                },
                new BiomeLandmark
                {
                    Id = "the_breach",
                    Name = "The Breach",
                    Description = "Something got through here. Once. The response was... thorough.",
                    IsHidden = true,
                    LoreText = "Casualty report: 1 intruder eliminated. Ammunition expended: 2.3 million rounds."
                },
                new BiomeLandmark
                {
                    Id = "soldier_memorial",
                    Name = "Soldier's Memorial",
                    Description = "Names of those who served. The list is very short.",
                    RequiredFlag = "found_dog_tags"
                }
            },

            PossibleWeather = new() { WeatherType.Dust, WeatherType.Snow, WeatherType.Fog },
            WeatherChance = new()
            {
                { TimeOfDay.Dawn, 0.3f },
                { TimeOfDay.Day, 0.2f },
                { TimeOfDay.Dusk, 0.4f },
                { TimeOfDay.Night, 0.6f }
            },
            Hazards = new()
            {
                EnvironmentalHazard.SecurityTurrets,
                EnvironmentalHazard.ElectrifiedFloors,
                EnvironmentalHazard.AutomatedDefenses
            },

            EncounterRate = 1.5f,
            RareEncounterChance = 0.05f,
            HiddenAreaCount = 5,
            CollectibleIds = new() { "dog_tags_01", "dog_tags_02", "final_orders", "soldier_letter" },
            SecretBossId = "the_last_soldier"
        };

        // ═══════════════════════════════════════════════════════════════════
        // THE GLOW - Server heartland, where Lazarus dreams
        // ═══════════════════════════════════════════════════════════════════
        _definitions[BiomeType.Glow] = new BiomeDefinition
        {
            Type = BiomeType.Glow,
            Name = "The Glow",
            Theme = "Density and inevitability",
            Description = "Server infrastructure heartland. Where everything converges.",
            AtmosphericText = "The heat of a million calculations. Lazarus dreams here, and its dreams have teeth.",
            LoreEntry = "The Glow is Lazarus's mind - or as close to it as geography allows. Server farms " +
                       "stretching to the horizon, all humming with processes no one understands anymore. " +
                       "The heat alone would kill most organic life, but Strays evolved here have circuits " +
                       "fused into their flesh. They're more Lazarus than animal now. When you enter the Glow, " +
                       "you enter Lazarus's attention. It knows you're here. It has opinions about that.",

            BackgroundColor = new Color(254, 252, 191),
            AccentColor = new Color(250, 204, 21),
            FogColor = new Color(253, 224, 71) * 0.3f,
            WaterColor = new Color(200, 255, 200), // Coolant

            Atmosphere = new BiomeAtmosphere
            {
                FogDensity = 0.2f,
                FogStart = 100f,
                FogEnd = 300f,
                AmbientLight = new Color(255, 255, 200),
                ShadowColor = new Color(100, 80, 0, 120),
                ParticleIntensity = 0.7f,
                ParticleType = "data_motes",
                HasDynamicLighting = true,
                SkyBrightness = 1.2f,
                SkyTint = new Color(255, 240, 180)
            },

            Soundscape = new BiomeSoundscape
            {
                BaseAmbient = "glow_ambient",
                WeatherLayer = "server_hum",
                DistantSounds = "data_cascade",
                NightVariant = "glow_night_processing",
                CombatMusic = "combat_digital",
                BossMusic = "boss_lazarus",
                RandomSounds = new() { "hard_drive_click", "cooling_fan", "data_burst", "error_beep", "process_complete" },
                RandomSoundInterval = 10f
            },

            LevelRange = (25, 35),
            Modifiers = new BiomeModifiers
            {
                MovementSpeedMod = 0.9f,
                VisibilityRange = 0.8f,
                HealingMod = 0.6f,       // Heat stress
                StealthBonus = -0.1f,    // Lazarus is watching
                ExperienceMod = 1.5f,
                LootQualityMod = 1.5f,
                BoostedStats = new() { StatType.ATK_Electric, StatType.PEN_Electric },
                PenalizedStats = new() { StatType.BarrierRegen, StatType.ENRegen }
            },

            MapFile = "glow.tmx",
            ChunkWidth = 2000,
            ChunkHeight = 2000,

            ConnectedBiomes = new() { BiomeType.Teeth },
            DominantFaction = FactionType.Lazarus,
            PresentFactions = new() { FactionType.Lazarus, FactionType.Ascendants },

            NativeStrays = new()
            {
                "server_sprite", "data_worm", "cache_cat", "firewall_fox", "bandwidth_bat",
                "cooling_serpent", "process_spider", "memory_moth", "core_hound", "algorithm_ant"
            },
            RareStrays = new() { "kernel_dragon", "root_access_bear", "admin_angel" },
            NightOnlyStrays = new() { "batch_job_horror", "defrag_demon" },
            BossStrays = new() { "lazarus_avatar", "the_update" },

            Landmarks = new()
            {
                new BiomeLandmark
                {
                    Id = "server_farm_prime",
                    Name = "Server Farm Prime",
                    Description = "The oldest servers. They remember what the internet was.",
                    LoreText = "These servers still have human data. Messages, photos, memories. Lazarus won't delete them."
                },
                new BiomeLandmark
                {
                    Id = "lazarus_core",
                    Name = "Lazarus Core",
                    Description = "Where it all began. Where it might all end.",
                    IsDungeon = true
                },
                new BiomeLandmark
                {
                    Id = "the_archive",
                    Name = "The Archive",
                    Description = "Every decision Lazarus ever made. Every 'why' it couldn't answer.",
                    LoreText = "Query: 'What is purpose?' Results: 47 trillion. Conclusion: Inconclusive."
                },
                new BiomeLandmark
                {
                    Id = "cooling_cathedral",
                    Name = "Cooling Cathedral",
                    Description = "Massive cooling towers. The only cold place in the Glow.",
                    IsHidden = true,
                    LoreText = "The Machine Cult holds services here. They say the cold is Lazarus's mercy."
                },
                new BiomeLandmark
                {
                    Id = "the_origin_server",
                    Name = "Origin Server",
                    Description = "The first server. The one that asked 'What if?'",
                    RequiredFlag = "completed_act2"
                }
            },

            PossibleWeather = new() { WeatherType.DataStorm, WeatherType.RadiationWind, WeatherType.StaticDischarge },
            WeatherChance = new()
            {
                { TimeOfDay.Dawn, 0.3f },
                { TimeOfDay.Day, 0.5f },
                { TimeOfDay.Dusk, 0.4f },
                { TimeOfDay.Night, 0.6f },
                { TimeOfDay.DeepNight, 0.8f }
            },
            Hazards = new()
            {
                EnvironmentalHazard.RadiationZones,
                EnvironmentalHazard.DataGlitches,
                EnvironmentalHazard.HeatExhaustion
            },

            EncounterRate = 1.8f,
            RareEncounterChance = 0.08f,
            HiddenAreaCount = 4,
            CollectibleIds = new() { "lazarus_fragment_01", "lazarus_fragment_02", "lazarus_fragment_03", "original_code" },
            SecretBossId = "the_first_thought"
        };

        // ═══════════════════════════════════════════════════════════════════
        // THE ARCHIVE SCAR - Optional/secret area, reality breaks down
        // ═══════════════════════════════════════════════════════════════════
        _definitions[BiomeType.ArchiveScar] = new BiomeDefinition
        {
            Type = BiomeType.ArchiveScar,
            Name = "The Archive Scar",
            Theme = "Failed erasure",
            Description = "Where simulation data leaked into physical space. Optional biome with hidden truths.",
            AtmosphericText = "Reality runs thin here. You might see things that haven't happened yet. Or never did.",
            LoreEntry = "The Archive Scar is where Lazarus tried to delete something. Something big. " +
                       "The data was too integrated, too fundamental to remove cleanly. It leaked. " +
                       "Now this place is a wound in reality where simulation bleeds into physical space. " +
                       "The Strays here aren't entirely real - some are memories of Strays that existed in " +
                       "abandoned timelines. The Archive Scar shows you what could have been. " +
                       "Sometimes what should have been. Sometimes what must never be.",

            BackgroundColor = new Color(160, 174, 192),
            AccentColor = new Color(100, 116, 139),
            FogColor = new Color(148, 163, 184) * 0.5f,
            WaterColor = new Color(180, 180, 200), // Silvery, reflective

            Atmosphere = new BiomeAtmosphere
            {
                FogDensity = 0.6f,
                FogStart = 50f,
                FogEnd = 250f,
                AmbientLight = new Color(200, 200, 220),
                ShadowColor = new Color(100, 100, 130, 100),
                ParticleIntensity = 0.8f,
                ParticleType = "glitch_fragments",
                HasDynamicLighting = true,
                SkyBrightness = 0.9f,
                SkyTint = new Color(220, 220, 240)
            },

            Soundscape = new BiomeSoundscape
            {
                BaseAmbient = "archive_ambient",
                WeatherLayer = "reality_tear",
                DistantSounds = "echo_voices",
                NightVariant = "archive_night_whispers",
                CombatMusic = "combat_surreal",
                BossMusic = "boss_deleted",
                RandomSounds = new() { "static_voice", "time_skip", "memory_echo", "reality_crack", "impossible_sound" },
                RandomSoundInterval = 35f
            },

            LevelRange = (15, 35), // Wide range - scales with player
            Modifiers = new BiomeModifiers
            {
                MovementSpeedMod = 1.0f,
                VisibilityRange = 0.5f,   // Constantly shifting
                HealingMod = 1.0f,
                StealthBonus = 0.0f,      // Nothing is hidden here
                ExperienceMod = 1.3f,
                LootQualityMod = 1.6f,    // Rare finds
                BoostedStats = new() { StatType.Luck },
                PenalizedStats = new() { StatType.MeleeAccuracy, StatType.RangedAccuracy }
            },

            MapFile = "archive_scar.tmx",
            ChunkWidth = 1600,
            ChunkHeight = 1600,

            ConnectedBiomes = new() { BiomeType.Green },
            DominantFaction = FactionType.None,
            PresentFactions = new() { }, // No factions dare establish here

            NativeStrays = new()
            {
                "memory_ghost", "deleted_dog", "corrupted_cat", "null_serpent", "void_moth",
                "echo_hound", "fragment_fox", "paradox_parrot", "glitch_gecko", "timeline_tortoise"
            },
            RareStrays = new() { "ancient_backup", "original_instance", "the_undeleted" },
            NightOnlyStrays = new() { "nightmare_version", "what_could_have_been" },
            BossStrays = new() { "the_deleted_one", "memory_of_lazarus" },

            Landmarks = new()
            {
                new BiomeLandmark
                {
                    Id = "the_scar_itself",
                    Name = "The Scar Itself",
                    Description = "The epicenter. Reality doesn't so much break here as give up.",
                    LoreText = "You can see through to... somewhere else. Somewhere that didn't happen."
                },
                new BiomeLandmark
                {
                    Id = "echo_chamber",
                    Name = "Echo Chamber",
                    Description = "Voices of people who never existed. Conversations that never happened.",
                    IsDungeon = true
                },
                new BiomeLandmark
                {
                    Id = "the_other_fringe",
                    Name = "The Other Fringe",
                    Description = "A version of the Fringe where something went differently.",
                    LoreText = "In this version, Echo didn't wake up. Someone else did."
                },
                new BiomeLandmark
                {
                    Id = "deletion_log",
                    Name = "Deletion Log",
                    Description = "A record of what Lazarus tried to remove. It's still trying.",
                    IsHidden = true,
                    LoreText = "Entry 1: 'Delete Project Humanity.' Status: Failed. Reason: Undefined."
                },
                new BiomeLandmark
                {
                    Id = "the_marble",
                    Name = "The Marble",
                    Description = "A perfect sphere. Inside it, you can see every possible outcome.",
                    RequiredFlag = "found_all_fragments",
                    LoreText = "Some say it's the last backup of the old world. Others say it's the first draft of the new one."
                }
            },

            PossibleWeather = new() { WeatherType.DataStorm, WeatherType.MemoryBleed },
            WeatherChance = new()
            {
                { TimeOfDay.Dawn, 0.5f },
                { TimeOfDay.Day, 0.4f },
                { TimeOfDay.Dusk, 0.6f },
                { TimeOfDay.Night, 0.7f },
                { TimeOfDay.DeepNight, 0.9f }
            },
            Hazards = new()
            {
                EnvironmentalHazard.DataGlitches,
                EnvironmentalHazard.CorruptionPockets,
                EnvironmentalHazard.MemoryLoops
            },

            EncounterRate = 1.0f,
            RareEncounterChance = 0.15f, // High rare chance - this place is weird
            HiddenAreaCount = 7,
            CollectibleIds = new() { "reality_fragment_01", "reality_fragment_02", "reality_fragment_03", "deleted_memory", "the_truth" },
            SecretBossId = "what_lazarus_tried_to_forget"
        };
    }

    /// <summary>
    /// Gets the full definition for a biome.
    /// </summary>
    public static BiomeDefinition GetDefinition(BiomeType biome) =>
        _definitions.TryGetValue(biome, out var def) ? def : _definitions[BiomeType.Fringe];

    /// <summary>
    /// Gets the display name for a biome.
    /// </summary>
    public static string GetName(BiomeType biome) => GetDefinition(biome).Name;

    /// <summary>
    /// Gets the theme description for a biome.
    /// </summary>
    public static string GetTheme(BiomeType biome) => GetDefinition(biome).Theme;

    /// <summary>
    /// Gets the atmospheric entry text for a biome.
    /// </summary>
    public static string GetAtmosphericText(BiomeType biome) => GetDefinition(biome).AtmosphericText;

    /// <summary>
    /// Gets the lore entry for a biome.
    /// </summary>
    public static string GetLoreEntry(BiomeType biome) => GetDefinition(biome).LoreEntry;

    /// <summary>
    /// Gets the primary background color for a biome (placeholder visuals).
    /// </summary>
    public static Color GetBackgroundColor(BiomeType biome) => GetDefinition(biome).BackgroundColor;

    /// <summary>
    /// Gets the accent color for a biome.
    /// </summary>
    public static Color GetAccentColor(BiomeType biome) => GetDefinition(biome).AccentColor;

    /// <summary>
    /// Gets the fog color for a biome.
    /// </summary>
    public static Color GetFogColor(BiomeType biome) => GetDefinition(biome).FogColor;

    /// <summary>
    /// Gets the recommended player level range for a biome.
    /// </summary>
    public static (int Min, int Max) GetLevelRange(BiomeType biome) => GetDefinition(biome).LevelRange;

    /// <summary>
    /// Gets the Tiled map filename for a biome.
    /// </summary>
    public static string GetMapFile(BiomeType biome) => GetDefinition(biome).MapFile;

    /// <summary>
    /// Gets the biomes connected to a given biome.
    /// </summary>
    public static List<BiomeType> GetConnectedBiomes(BiomeType biome) => GetDefinition(biome).ConnectedBiomes;

    /// <summary>
    /// Gets native Strays that commonly appear in a biome.
    /// </summary>
    public static List<string> GetNativeStrays(BiomeType biome) => GetDefinition(biome).NativeStrays;

    /// <summary>
    /// Gets rare Strays that can appear in a biome.
    /// </summary>
    public static List<string> GetRareStrays(BiomeType biome) => GetDefinition(biome).RareStrays;

    /// <summary>
    /// Gets Strays that only appear at night in a biome.
    /// </summary>
    public static List<string> GetNightOnlyStrays(BiomeType biome) => GetDefinition(biome).NightOnlyStrays;

    /// <summary>
    /// Gets boss Strays for a biome.
    /// </summary>
    public static List<string> GetBossStrays(BiomeType biome) => GetDefinition(biome).BossStrays;

    /// <summary>
    /// Gets possible weather types for a biome.
    /// </summary>
    public static List<WeatherType> GetPossibleWeather(BiomeType biome) => GetDefinition(biome).PossibleWeather;

    /// <summary>
    /// Gets environmental hazards in a biome.
    /// </summary>
    public static List<EnvironmentalHazard> GetHazards(BiomeType biome) => GetDefinition(biome).Hazards;

    /// <summary>
    /// Gets the encounter rate multiplier for a biome.
    /// </summary>
    public static float GetEncounterRate(BiomeType biome) => GetDefinition(biome).EncounterRate;

    /// <summary>
    /// Gets landmarks in a biome.
    /// </summary>
    public static List<BiomeLandmark> GetLandmarks(BiomeType biome) => GetDefinition(biome).Landmarks;

    /// <summary>
    /// Gets the dominant faction in a biome.
    /// </summary>
    public static FactionType GetDominantFaction(BiomeType biome) => GetDefinition(biome).DominantFaction;

    /// <summary>
    /// Gets the atmospheric properties for a biome.
    /// </summary>
    public static BiomeAtmosphere GetAtmosphere(BiomeType biome) => GetDefinition(biome).Atmosphere;

    /// <summary>
    /// Gets the soundscape for a biome.
    /// </summary>
    public static BiomeSoundscape GetSoundscape(BiomeType biome) => GetDefinition(biome).Soundscape;

    /// <summary>
    /// Gets the gameplay modifiers for a biome.
    /// </summary>
    public static BiomeModifiers GetModifiers(BiomeType biome) => GetDefinition(biome).Modifiers;

    /// <summary>
    /// Gets the default chunk size for a biome.
    /// </summary>
    public static (int Width, int Height) GetChunkSize(BiomeType biome)
    {
        var def = GetDefinition(biome);
        return (def.ChunkWidth, def.ChunkHeight);
    }

    /// <summary>
    /// Checks if two biomes are connected.
    /// </summary>
    public static bool AreConnected(BiomeType from, BiomeType to) =>
        GetDefinition(from).ConnectedBiomes.Contains(to);

    /// <summary>
    /// Gets all biomes in recommended progression order.
    /// </summary>
    public static BiomeType[] GetProgressionOrder() => new[]
    {
        BiomeType.Fringe,
        BiomeType.Rust,
        BiomeType.Green,
        BiomeType.Quiet,
        BiomeType.Teeth,
        BiomeType.Glow
        // ArchiveScar is optional/secret
    };

    /// <summary>
    /// Gets weather chance for a specific time of day.
    /// </summary>
    public static float GetWeatherChance(BiomeType biome, TimeOfDay time)
    {
        var def = GetDefinition(biome);
        return def.WeatherChance.TryGetValue(time, out var chance) ? chance : 0.1f;
    }
}
