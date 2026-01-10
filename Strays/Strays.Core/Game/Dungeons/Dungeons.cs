using System.Collections.Generic;
using System.Linq;
using Strays.Core.Game.World;

namespace Strays.Core.Game.Dungeons;

/// <summary>
/// Registry of all dungeon definitions.
/// </summary>
public static class Dungeons
{
    private static readonly Dictionary<string, DungeonDefinition> _dungeons = new();
    private static bool _initialized = false;

    /// <summary>
    /// Gets all registered dungeons.
    /// </summary>
    public static IEnumerable<DungeonDefinition> All => _dungeons.Values;

    /// <summary>
    /// Gets a dungeon by ID.
    /// </summary>
    public static DungeonDefinition? Get(string id) =>
        _dungeons.TryGetValue(id, out var dungeon) ? dungeon : null;

    /// <summary>
    /// Gets all dungeons in a specific biome.
    /// </summary>
    public static IEnumerable<DungeonDefinition> GetByBiome(BiomeType biome) =>
        _dungeons.Values.Where(d => d.Biome == biome);

    /// <summary>
    /// Gets dungeons available to a player based on flags and level.
    /// </summary>
    public static IEnumerable<DungeonDefinition> GetAvailable(HashSet<string> flags, int playerLevel) =>
        _dungeons.Values.Where(d =>
            (d.UnlockedByDefault || (d.RequiredFlag != null && flags.Contains(d.RequiredFlag))) &&
            playerLevel >= d.MinLevel - 2); // Allow slightly underleveled attempts

    /// <summary>
    /// Registers a dungeon.
    /// </summary>
    public static void Register(DungeonDefinition dungeon) =>
        _dungeons[dungeon.Id] = dungeon;

    /// <summary>
    /// Initializes all dungeon definitions.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        RegisterFringeDungeons();
        RegisterRustDungeons();
        RegisterGreenDungeons();
        RegisterQuietDungeons();
        RegisterTeethDungeons();
        RegisterGlowDungeons();
        RegisterArchiveDungeons();
    }

    /// <summary>
    /// Fringe biome dungeons - starter area.
    /// </summary>
    private static void RegisterFringeDungeons()
    {
        Register(new DungeonDefinition
        {
            Id = "fringe_sewers",
            Name = "The Forgotten Sewers",
            Description = "Beneath the suburban ruins, a network of tunnels hides feral Strays and forgotten technology.",
            Biome = BiomeType.Fringe,
            MinLevel = 3,
            MaxLevel = 8,
            RoomCount = 8,
            MidBossRoom = 3,
            FinalBossRoom = 7,
            RegularEnemyIds = new List<string> { "sewer_rat", "pipe_crawler", "drain_lurker", "toxic_blob" },
            MidBossId = "bloated_guardian",
            MidBossName = "Bloated Guardian",
            FinalBossId = "sewer_king",
            FinalBossName = "The Sewer King",
            ObjectiveText = "Navigate the sewers and defeat the Sewer King",
            BaseExpReward = 150,
            BaseCurrencyReward = 75,
            AmbientColor = "#1a2a1a",
            UnlockedByDefault = true
        });

        Register(new DungeonDefinition
        {
            Id = "fringe_warehouse",
            Name = "Abandoned Warehouse",
            Description = "A crumbling storage facility now home to territorial Strays and scavenger gangs.",
            Biome = BiomeType.Fringe,
            MinLevel = 5,
            MaxLevel = 12,
            RoomCount = 10,
            MidBossRoom = 4,
            FinalBossRoom = 9,
            RegularEnemyIds = new List<string> { "junk_hound", "wire_rat", "rusted_sentry", "scrap_golem" },
            MidBossId = "forklift_beast",
            MidBossName = "Forklift Beast",
            FinalBossId = "warehouse_warden",
            FinalBossName = "The Warehouse Warden",
            ObjectiveText = "Clear the warehouse and defeat its mechanical guardian",
            BaseExpReward = 200,
            BaseCurrencyReward = 100,
            AmbientColor = "#2a2a2a",
            RequiredFlag = "fringe_sewers_cleared"
        });
    }

    /// <summary>
    /// Rust biome dungeons - corrosive wasteland.
    /// </summary>
    private static void RegisterRustDungeons()
    {
        Register(new DungeonDefinition
        {
            Id = "rust_refinery",
            Name = "Corroded Refinery",
            Description = "Once a processing plant, now a maze of acidic pools and mutated creatures.",
            Biome = BiomeType.Rust,
            MinLevel = 8,
            MaxLevel = 15,
            RoomCount = 10,
            MidBossRoom = 4,
            FinalBossRoom = 9,
            RegularEnemyIds = new List<string> { "acid_slime", "corroded_hound", "rust_beetle", "chemical_horror" },
            MidBossId = "vat_mother",
            MidBossName = "The Vat Mother",
            FinalBossId = "refinery_core",
            FinalBossName = "Refinery Core",
            ObjectiveText = "Shut down the corrupted refinery core",
            BaseExpReward = 300,
            BaseCurrencyReward = 150,
            AmbientColor = "#3a2a1a",
            UnlockedByDefault = true
        });

        Register(new DungeonDefinition
        {
            Id = "rust_scrapyard",
            Name = "The Endless Scrapyard",
            Description = "Mountains of discarded machines, some still functioning with murderous intent.",
            Biome = BiomeType.Rust,
            MinLevel = 12,
            MaxLevel = 20,
            RoomCount = 12,
            MidBossRoom = 5,
            FinalBossRoom = 11,
            RegularEnemyIds = new List<string> { "scrap_hunter", "rust_titan", "crusher_bot", "oxidized_horror" },
            MidBossId = "magnet_crane",
            MidBossName = "Magnet Crane",
            FinalBossId = "scrap_colossus",
            FinalBossName = "Scrap Colossus",
            ObjectiveText = "Destroy the Scrap Colossus at the heart of the yard",
            BaseExpReward = 450,
            BaseCurrencyReward = 225,
            AmbientColor = "#4a3a2a",
            RequiredFlag = "rust_refinery_cleared"
        });
    }

    /// <summary>
    /// Green biome dungeons - overgrown mutation zone.
    /// </summary>
    private static void RegisterGreenDungeons()
    {
        Register(new DungeonDefinition
        {
            Id = "green_greenhouse",
            Name = "Mutant Greenhouse",
            Description = "What was meant to preserve life has become a nightmare of twisted vegetation.",
            Biome = BiomeType.Green,
            MinLevel = 10,
            MaxLevel = 18,
            RoomCount = 10,
            MidBossRoom = 4,
            FinalBossRoom = 9,
            RegularEnemyIds = new List<string> { "vine_strangler", "spore_walker", "thorn_beast", "pollen_cloud" },
            MidBossId = "greenhouse_heart",
            MidBossName = "Greenhouse Heart",
            FinalBossId = "overgrowth_titan",
            FinalBossName = "Overgrowth Titan",
            ObjectiveText = "Purge the mutant growth at its source",
            BaseExpReward = 350,
            BaseCurrencyReward = 175,
            AmbientColor = "#1a3a1a",
            UnlockedByDefault = true
        });

        Register(new DungeonDefinition
        {
            Id = "green_laboratory",
            Name = "Bio-Research Lab",
            Description = "Lazarus's genetic experiments continue here, spawning ever-more dangerous creatures.",
            Biome = BiomeType.Green,
            MinLevel = 15,
            MaxLevel = 25,
            RoomCount = 12,
            MidBossRoom = 5,
            FinalBossRoom = 11,
            RegularEnemyIds = new List<string> { "lab_specimen", "gene_horror", "bio_construct", "chimera" },
            MidBossId = "failed_experiment",
            MidBossName = "Failed Experiment #7",
            FinalBossId = "perfect_organism",
            FinalBossName = "The Perfect Organism",
            ObjectiveText = "Terminate Lazarus's 'perfect' creation",
            BaseExpReward = 500,
            BaseCurrencyReward = 250,
            AmbientColor = "#2a4a2a",
            RequiredFlag = "green_greenhouse_cleared"
        });
    }

    /// <summary>
    /// Quiet biome dungeons - sound-dampening void.
    /// </summary>
    private static void RegisterQuietDungeons()
    {
        Register(new DungeonDefinition
        {
            Id = "quiet_bunker",
            Name = "Silent Bunker",
            Description = "A military installation swallowed by The Quiet. Sound is your enemy here.",
            Biome = BiomeType.Quiet,
            MinLevel = 12,
            MaxLevel = 20,
            RoomCount = 10,
            MidBossRoom = 4,
            FinalBossRoom = 9,
            RegularEnemyIds = new List<string> { "echo_hunter", "void_walker", "silence_feeder", "null_shade" },
            MidBossId = "resonance_keeper",
            MidBossName = "Resonance Keeper",
            FinalBossId = "absolute_silence",
            FinalBossName = "Absolute Silence",
            ObjectiveText = "Break the silence that consumes this place",
            BaseExpReward = 400,
            BaseCurrencyReward = 200,
            AmbientColor = "#1a1a2a",
            UnlockedByDefault = true
        });

        Register(new DungeonDefinition
        {
            Id = "quiet_cathedral",
            Name = "Cathedral of Whispers",
            Description = "A place of worship now filled with the echoes of those who couldn't escape The Quiet.",
            Biome = BiomeType.Quiet,
            MinLevel = 18,
            MaxLevel = 28,
            RoomCount = 12,
            MidBossRoom = 5,
            FinalBossRoom = 11,
            RegularEnemyIds = new List<string> { "whisper_wraith", "choir_ghoul", "bell_horror", "sound_eater" },
            MidBossId = "choir_master",
            MidBossName = "The Choir Master",
            FinalBossId = "voice_of_void",
            FinalBossName = "Voice of the Void",
            ObjectiveText = "Silence the Voice of the Void forever",
            BaseExpReward = 550,
            BaseCurrencyReward = 275,
            AmbientColor = "#2a2a3a",
            RequiredFlag = "quiet_bunker_cleared"
        });
    }

    /// <summary>
    /// Teeth biome dungeons - bone and calcification.
    /// </summary>
    private static void RegisterTeethDungeons()
    {
        Register(new DungeonDefinition
        {
            Id = "teeth_ossuary",
            Name = "The Ossuary",
            Description = "A network of tunnels made entirely of calcified remains. Something stirs within.",
            Biome = BiomeType.Teeth,
            MinLevel = 15,
            MaxLevel = 25,
            RoomCount = 10,
            MidBossRoom = 4,
            FinalBossRoom = 9,
            RegularEnemyIds = new List<string> { "bone_crawler", "calcified_horror", "marrow_leech", "skeletal_amalgam" },
            MidBossId = "bone_collector",
            MidBossName = "The Bone Collector",
            FinalBossId = "ossuary_guardian",
            FinalBossName = "Ossuary Guardian",
            ObjectiveText = "Destroy the Ossuary Guardian and claim its power",
            BaseExpReward = 450,
            BaseCurrencyReward = 225,
            AmbientColor = "#3a3a2a",
            UnlockedByDefault = true
        });

        Register(new DungeonDefinition
        {
            Id = "teeth_maw",
            Name = "The Living Maw",
            Description = "The ground itself seems alive here, hungry, waiting to consume all who enter.",
            Biome = BiomeType.Teeth,
            MinLevel = 22,
            MaxLevel = 32,
            RoomCount = 12,
            MidBossRoom = 5,
            FinalBossRoom = 11,
            RegularEnemyIds = new List<string> { "tooth_worm", "jaw_trap", "enamel_beast", "cavity_horror" },
            MidBossId = "molar_golem",
            MidBossName = "Molar Golem",
            FinalBossId = "the_maw",
            FinalBossName = "The Maw",
            ObjectiveText = "Survive The Maw and escape its hunger",
            BaseExpReward = 600,
            BaseCurrencyReward = 300,
            AmbientColor = "#4a4a3a",
            RequiredFlag = "teeth_ossuary_cleared"
        });
    }

    /// <summary>
    /// Glow biome dungeons - radiation and light.
    /// </summary>
    private static void RegisterGlowDungeons()
    {
        Register(new DungeonDefinition
        {
            Id = "glow_reactor",
            Name = "Breached Reactor",
            Description = "The source of The Glow's radiation. Creatures here have evolved to thrive in lethal light.",
            Biome = BiomeType.Glow,
            MinLevel = 20,
            MaxLevel = 30,
            RoomCount = 10,
            MidBossRoom = 4,
            FinalBossRoom = 9,
            RegularEnemyIds = new List<string> { "rad_hound", "glow_stalker", "isotope_horror", "nuclear_shade" },
            MidBossId = "reactor_spirit",
            MidBossName = "Reactor Spirit",
            FinalBossId = "critical_mass",
            FinalBossName = "Critical Mass",
            ObjectiveText = "Contain the reactor before it goes critical",
            BaseExpReward = 500,
            BaseCurrencyReward = 250,
            AmbientColor = "#2a4a4a",
            UnlockedByDefault = true
        });

        Register(new DungeonDefinition
        {
            Id = "glow_nimdok_gate",
            Name = "Lazarus's Gate",
            Description = "The entrance to Lazarus's core systems. Only the strongest can hope to survive.",
            Biome = BiomeType.Glow,
            MinLevel = 28,
            MaxLevel = 40,
            RoomCount = 15,
            MidBossRoom = 7,
            FinalBossRoom = 14,
            RegularEnemyIds = new List<string> { "nimdok_drone", "data_phantom", "core_guardian", "light_eater" },
            MidBossId = "firewall_construct",
            MidBossName = "Firewall Construct",
            FinalBossId = "nimdok_avatar",
            FinalBossName = "Lazarus Avatar",
            ObjectiveText = "Breach Lazarus's defenses and face its Avatar",
            BaseExpReward = 800,
            BaseCurrencyReward = 400,
            AmbientColor = "#3a5a5a",
            RequiredFlag = "glow_reactor_cleared"
        });
    }

    /// <summary>
    /// Archive Scar dungeons - Lazarus's data core.
    /// </summary>
    private static void RegisterArchiveDungeons()
    {
        Register(new DungeonDefinition
        {
            Id = "archive_memory_banks",
            Name = "Memory Banks",
            Description = "Lazarus stores its memories here. Fragments of the old world, corrupted and dangerous.",
            Biome = BiomeType.ArchiveScar,
            MinLevel = 25,
            MaxLevel = 35,
            RoomCount = 12,
            MidBossRoom = 5,
            FinalBossRoom = 11,
            RegularEnemyIds = new List<string> { "memory_echo", "data_corruptor", "archive_worm", "forgotten_one" },
            MidBossId = "memory_keeper",
            MidBossName = "Memory Keeper",
            FinalBossId = "corrupted_archive",
            FinalBossName = "Corrupted Archive",
            ObjectiveText = "Purge the corrupted data and restore the Archive",
            BaseExpReward = 600,
            BaseCurrencyReward = 300,
            AmbientColor = "#2a2a4a",
            UnlockedByDefault = true
        });

        Register(new DungeonDefinition
        {
            Id = "archive_core",
            Name = "Lazarus Core",
            Description = "The heart of Lazarus itself. Here lies the truth... and the ultimate challenge.",
            Biome = BiomeType.ArchiveScar,
            MinLevel = 35,
            MaxLevel = 50,
            RoomCount = 15,
            MidBossRoom = 7,
            FinalBossRoom = 14,
            RegularEnemyIds = new List<string> { "core_sentinel", "truth_seeker", "nimdok_fragment", "ultimate_form" },
            MidBossId = "truth_guardian",
            MidBossName = "Truth Guardian",
            FinalBossId = "nimdok_true_form",
            FinalBossName = "Lazarus - True Form",
            ObjectiveText = "Face Lazarus's True Form and decide the fate of the wasteland",
            BaseExpReward = 1000,
            BaseCurrencyReward = 500,
            AmbientColor = "#3a3a5a",
            RequiredFlag = "archive_memory_banks_cleared"
        });
    }
}
