using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Lazarus.Core.Game.Dungeons;

/// <summary>
/// Room layout templates for dungeon generation.
/// </summary>
public enum RoomLayout
{
    /// <summary>
    /// Open square room.
    /// </summary>
    OpenSquare,

    /// <summary>
    /// L-shaped corridor.
    /// </summary>
    LCorridor,

    /// <summary>
    /// Room with central pillar/obstacle.
    /// </summary>
    CentralPillar,

    /// <summary>
    /// Long narrow corridor.
    /// </summary>
    NarrowCorridor,

    /// <summary>
    /// Wide arena space.
    /// </summary>
    Arena,

    /// <summary>
    /// Room with alcoves on sides.
    /// </summary>
    Alcoved,

    /// <summary>
    /// T-junction intersection.
    /// </summary>
    TJunction,

    /// <summary>
    /// Circular room.
    /// </summary>
    Circular,

    /// <summary>
    /// Room with raised platforms.
    /// </summary>
    Platforms,

    /// <summary>
    /// Boss arena with hazard zones.
    /// </summary>
    BossArena
}

/// <summary>
/// Types of environmental hazards in dungeon rooms.
/// </summary>
public enum RoomHazard
{
    None,
    AcidPools,
    ElectricalArcs,
    CollapsingFloor,
    ToxicGas,
    RadiationZones,
    FreezingVents,
    FirePits,
    SpikeTrap,
    Darkness,
    Silence
}

/// <summary>
/// Special room features that modify gameplay.
/// </summary>
public enum RoomFeature
{
    None,
    HealingStation,
    AmmoCache,
    TreasureChest,
    NpcEncounter,
    DataTerminal,
    ShortcutUnlock,
    SecretArea,
    TrapRoom,
    AmbushPoint,
    RestArea
}

/// <summary>
/// An encounter definition for dungeon rooms.
/// </summary>
public class DungeonEncounter
{
    /// <summary>
    /// Unique encounter ID.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Enemy Stray IDs and their counts.
    /// </summary>
    public Dictionary<string, int> Enemies { get; init; } = new();

    /// <summary>
    /// Minimum room level for this encounter.
    /// </summary>
    public int MinLevel { get; init; } = 1;

    /// <summary>
    /// Maximum room level for this encounter.
    /// </summary>
    public int MaxLevel { get; init; } = 99;

    /// <summary>
    /// Relative spawn weight (higher = more common).
    /// </summary>
    public int Weight { get; init; } = 10;

    /// <summary>
    /// Whether this is a boss encounter.
    /// </summary>
    public bool IsBoss { get; init; } = false;

    /// <summary>
    /// Hazards present during this encounter.
    /// </summary>
    public RoomHazard Hazard { get; init; } = RoomHazard.None;

    /// <summary>
    /// Reinforcement waves (enemy IDs).
    /// </summary>
    public List<string>? Reinforcements { get; init; }

    /// <summary>
    /// Dialog to play before encounter.
    /// </summary>
    public string? PreBattleDialogId { get; init; }
}

/// <summary>
/// A pre-authored room with specific layout and encounters.
/// </summary>
public class AuthoredRoom
{
    /// <summary>
    /// Unique room ID.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Room display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Room description.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Which dungeon this room belongs to.
    /// </summary>
    public string DungeonId { get; init; } = "";

    /// <summary>
    /// Room layout type.
    /// </summary>
    public RoomLayout Layout { get; init; } = RoomLayout.OpenSquare;

    /// <summary>
    /// Fixed encounter for this room (null = random).
    /// </summary>
    public string? FixedEncounterId { get; init; }

    /// <summary>
    /// Hazards in this room.
    /// </summary>
    public List<RoomHazard> Hazards { get; init; } = new();

    /// <summary>
    /// Special features.
    /// </summary>
    public List<RoomFeature> Features { get; init; } = new();

    /// <summary>
    /// Room-specific lore text.
    /// </summary>
    public string? LoreText { get; init; }

    /// <summary>
    /// Dialog triggered when entering.
    /// </summary>
    public string? EntryDialogId { get; init; }

    /// <summary>
    /// Whether this room is required for progression.
    /// </summary>
    public bool IsRequired { get; init; } = true;

    /// <summary>
    /// Environmental tint color.
    /// </summary>
    public string? AmbientTint { get; init; }
}

/// <summary>
/// Dungeon wave/phase configuration for boss fights.
/// </summary>
public class BossPhase
{
    /// <summary>
    /// Phase number (0-indexed).
    /// </summary>
    public int PhaseNumber { get; init; }

    /// <summary>
    /// HP threshold to trigger this phase (percentage).
    /// </summary>
    public float HpThreshold { get; init; } = 1f;

    /// <summary>
    /// Dialog when entering this phase.
    /// </summary>
    public string? PhaseDialogId { get; init; }

    /// <summary>
    /// Additional enemies that spawn in this phase.
    /// </summary>
    public Dictionary<string, int> Adds { get; init; } = new();

    /// <summary>
    /// Abilities unlocked in this phase.
    /// </summary>
    public List<string> UnlockedAbilities { get; init; } = new();

    /// <summary>
    /// Room hazards active in this phase.
    /// </summary>
    public RoomHazard ActiveHazard { get; init; } = RoomHazard.None;

    /// <summary>
    /// Stat multiplier for this phase.
    /// </summary>
    public float StatMultiplier { get; init; } = 1f;
}

/// <summary>
/// Registry of all dungeon content - encounters, rooms, and layouts.
/// </summary>
public static class DungeonContent
{
    private static readonly Dictionary<string, DungeonEncounter> _encounters = new();
    private static readonly Dictionary<string, AuthoredRoom> _rooms = new();
    private static readonly Dictionary<string, List<BossPhase>> _bossPhases = new();
    private static bool _initialized = false;

    /// <summary>
    /// Gets all encounters.
    /// </summary>
    public static IEnumerable<DungeonEncounter> AllEncounters => _encounters.Values;

    /// <summary>
    /// Gets all authored rooms.
    /// </summary>
    public static IEnumerable<AuthoredRoom> AllRooms => _rooms.Values;

    /// <summary>
    /// Gets an encounter by ID.
    /// </summary>
    public static DungeonEncounter? GetEncounter(string id) =>
        _encounters.TryGetValue(id, out var enc) ? enc : null;

    /// <summary>
    /// Gets an authored room by ID.
    /// </summary>
    public static AuthoredRoom? GetRoom(string id) =>
        _rooms.TryGetValue(id, out var room) ? room : null;

    /// <summary>
    /// Gets boss phases for a boss ID.
    /// </summary>
    public static List<BossPhase>? GetBossPhases(string bossId) =>
        _bossPhases.TryGetValue(bossId, out var phases) ? phases : null;

    /// <summary>
    /// Gets random encounters for a dungeon and level range.
    /// </summary>
    public static List<DungeonEncounter> GetEncountersForDungeon(string dungeonId, int level)
    {
        var dungeon = Dungeons.Get(dungeonId);
        if (dungeon == null) return new List<DungeonEncounter>();

        return _encounters.Values
            .Where(e => !e.IsBoss && level >= e.MinLevel && level <= e.MaxLevel)
            .ToList();
    }

    /// <summary>
    /// Gets authored rooms for a specific dungeon.
    /// </summary>
    public static List<AuthoredRoom> GetRoomsForDungeon(string dungeonId) =>
        _rooms.Values.Where(r => r.DungeonId == dungeonId).ToList();

    /// <summary>
    /// Registers an encounter.
    /// </summary>
    public static void Register(DungeonEncounter encounter) =>
        _encounters[encounter.Id] = encounter;

    /// <summary>
    /// Registers an authored room.
    /// </summary>
    public static void Register(AuthoredRoom room) =>
        _rooms[room.Id] = room;

    /// <summary>
    /// Registers boss phases.
    /// </summary>
    public static void RegisterBossPhases(string bossId, List<BossPhase> phases) =>
        _bossPhases[bossId] = phases;

    /// <summary>
    /// Initializes all dungeon content.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        RegisterFringeContent();
        RegisterRustContent();
        RegisterGreenContent();
        RegisterQuietContent();
        RegisterTeethContent();
        RegisterGlowContent();
        RegisterArchiveContent();
        RegisterBossContent();
    }

    /// <summary>
    /// Fringe biome dungeon content.
    /// </summary>
    private static void RegisterFringeContent()
    {
        // === ENCOUNTERS ===

        Register(new DungeonEncounter
        {
            Id = "fringe_rats_basic",
            Enemies = new Dictionary<string, int> { { "sewer_rat", 3 } },
            MinLevel = 1,
            MaxLevel = 5,
            Weight = 20
        });

        Register(new DungeonEncounter
        {
            Id = "fringe_rats_pipes",
            Enemies = new Dictionary<string, int> { { "sewer_rat", 2 }, { "pipe_crawler", 1 } },
            MinLevel = 3,
            MaxLevel = 8,
            Weight = 15
        });

        Register(new DungeonEncounter
        {
            Id = "fringe_lurkers",
            Enemies = new Dictionary<string, int> { { "drain_lurker", 2 } },
            MinLevel = 4,
            MaxLevel = 10,
            Weight = 12,
            Hazard = RoomHazard.ToxicGas
        });

        Register(new DungeonEncounter
        {
            Id = "fringe_toxic_swarm",
            Enemies = new Dictionary<string, int>
            {
                { "toxic_blob", 1 },
                { "sewer_rat", 4 }
            },
            MinLevel = 5,
            MaxLevel = 12,
            Weight = 8,
            Hazard = RoomHazard.AcidPools
        });

        Register(new DungeonEncounter
        {
            Id = "fringe_warehouse_patrol",
            Enemies = new Dictionary<string, int> { { "junk_hound", 2 }, { "wire_rat", 2 } },
            MinLevel = 5,
            MaxLevel = 12,
            Weight = 15
        });

        Register(new DungeonEncounter
        {
            Id = "fringe_mechanical_threat",
            Enemies = new Dictionary<string, int>
            {
                { "rusted_sentry", 1 },
                { "scrap_golem", 1 }
            },
            MinLevel = 8,
            MaxLevel = 15,
            Weight = 10,
            Reinforcements = new List<string> { "wire_rat", "wire_rat" }
        });

        // === AUTHORED ROOMS ===

        Register(new AuthoredRoom
        {
            Id = "fringe_sewers_entrance",
            Name = "Sewer Entrance",
            Description = "A rusted grate leads down into darkness. The stench is overwhelming.",
            DungeonId = "fringe_sewers",
            Layout = RoomLayout.NarrowCorridor,
            Features = new List<RoomFeature> { RoomFeature.AmmoCache },
            LoreText = "Before the Collapse, these sewers served a city of millions.",
            EntryDialogId = "dungeon_fringe_sewers_enter"
        });

        Register(new AuthoredRoom
        {
            Id = "fringe_sewers_junction",
            Name = "Flooded Junction",
            Description = "Multiple tunnels converge here. Murky water covers the floor.",
            DungeonId = "fringe_sewers",
            Layout = RoomLayout.TJunction,
            Hazards = new List<RoomHazard> { RoomHazard.AcidPools },
            FixedEncounterId = "fringe_lurkers",
            AmbientTint = "#2a3a2a"
        });

        Register(new AuthoredRoom
        {
            Id = "fringe_sewers_nest",
            Name = "The Nest",
            Description = "Organic matter has been gathered here. Something is breeding.",
            DungeonId = "fringe_sewers",
            Layout = RoomLayout.Circular,
            FixedEncounterId = "fringe_toxic_swarm",
            Features = new List<RoomFeature> { RoomFeature.TreasureChest },
            LoreText = "The Strays have made this place their home."
        });

        Register(new AuthoredRoom
        {
            Id = "fringe_sewers_king_chamber",
            Name = "The King's Domain",
            Description = "A vast chamber of collected refuse. The Sewer King awaits.",
            DungeonId = "fringe_sewers",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.ToxicGas, RoomHazard.AcidPools },
            EntryDialogId = "boss_sewer_king_intro",
            AmbientTint = "#1a2a1a"
        });

        Register(new AuthoredRoom
        {
            Id = "fringe_warehouse_loading",
            Name = "Loading Bay",
            Description = "Abandoned trucks and containers create a maze of cover.",
            DungeonId = "fringe_warehouse",
            Layout = RoomLayout.Alcoved,
            Features = new List<RoomFeature> { RoomFeature.AmmoCache, RoomFeature.DataTerminal },
            LoreText = "Shipments that never arrived. Goods that never reached their destination."
        });

        Register(new AuthoredRoom
        {
            Id = "fringe_warehouse_storage",
            Name = "Cold Storage",
            Description = "Freezing air leaks from broken refrigeration units.",
            DungeonId = "fringe_warehouse",
            Layout = RoomLayout.OpenSquare,
            Hazards = new List<RoomHazard> { RoomHazard.FreezingVents },
            FixedEncounterId = "fringe_warehouse_patrol"
        });

        Register(new AuthoredRoom
        {
            Id = "fringe_warehouse_office",
            Name = "Manager's Office",
            Description = "A preserved slice of corporate life. Papers still on the desk.",
            DungeonId = "fringe_warehouse",
            Layout = RoomLayout.OpenSquare,
            Features = new List<RoomFeature> { RoomFeature.DataTerminal, RoomFeature.TreasureChest },
            LoreText = "A memo dated the day before the Collapse: 'Looking forward to the future.'"
        });

        Register(new AuthoredRoom
        {
            Id = "fringe_warehouse_floor",
            Name = "Warehouse Floor",
            Description = "The main storage area. The Warden patrols here.",
            DungeonId = "fringe_warehouse",
            Layout = RoomLayout.Arena,
            Hazards = new List<RoomHazard> { RoomHazard.CollapsingFloor },
            EntryDialogId = "boss_warehouse_warden_intro"
        });
    }

    /// <summary>
    /// Rust biome dungeon content.
    /// </summary>
    private static void RegisterRustContent()
    {
        // === ENCOUNTERS ===

        Register(new DungeonEncounter
        {
            Id = "rust_acid_basic",
            Enemies = new Dictionary<string, int> { { "acid_slime", 2 }, { "rust_beetle", 2 } },
            MinLevel = 8,
            MaxLevel = 15,
            Weight = 18
        });

        Register(new DungeonEncounter
        {
            Id = "rust_hounds",
            Enemies = new Dictionary<string, int> { { "corroded_hound", 3 } },
            MinLevel = 10,
            MaxLevel = 18,
            Weight = 15
        });

        Register(new DungeonEncounter
        {
            Id = "rust_chemical_horror",
            Enemies = new Dictionary<string, int>
            {
                { "chemical_horror", 1 },
                { "acid_slime", 2 }
            },
            MinLevel = 12,
            MaxLevel = 20,
            Weight = 10,
            Hazard = RoomHazard.AcidPools,
            PreBattleDialogId = "encounter_chemical_horror"
        });

        Register(new DungeonEncounter
        {
            Id = "rust_scrap_patrol",
            Enemies = new Dictionary<string, int>
            {
                { "scrap_hunter", 2 },
                { "crusher_bot", 1 }
            },
            MinLevel = 12,
            MaxLevel = 22,
            Weight = 12
        });

        Register(new DungeonEncounter
        {
            Id = "rust_titan_squad",
            Enemies = new Dictionary<string, int>
            {
                { "rust_titan", 1 },
                { "oxidized_horror", 2 }
            },
            MinLevel = 16,
            MaxLevel = 25,
            Weight = 8,
            Reinforcements = new List<string> { "rust_beetle", "rust_beetle", "rust_beetle" }
        });

        // === AUTHORED ROOMS ===

        Register(new AuthoredRoom
        {
            Id = "rust_refinery_vats",
            Name = "Processing Vats",
            Description = "Massive tanks of corrosive chemicals. One wrong step means death.",
            DungeonId = "rust_refinery",
            Layout = RoomLayout.Platforms,
            Hazards = new List<RoomHazard> { RoomHazard.AcidPools },
            FixedEncounterId = "rust_acid_basic",
            LoreText = "These vats once processed raw materials. Now they digest anything that falls in."
        });

        Register(new AuthoredRoom
        {
            Id = "rust_refinery_control",
            Name = "Control Room",
            Description = "The heart of the refinery's operations. Monitors flicker with ancient data.",
            DungeonId = "rust_refinery",
            Layout = RoomLayout.CentralPillar,
            Features = new List<RoomFeature> { RoomFeature.DataTerminal, RoomFeature.HealingStation },
            LoreText = "Error logs from the final day: 'Containment breach in Sector 7. All personnel evacuate.'"
        });

        Register(new AuthoredRoom
        {
            Id = "rust_refinery_core",
            Name = "Reactor Core",
            Description = "The corrupted core pulses with malevolent energy.",
            DungeonId = "rust_refinery",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.RadiationZones, RoomHazard.ElectricalArcs },
            EntryDialogId = "boss_refinery_core_intro"
        });

        Register(new AuthoredRoom
        {
            Id = "rust_scrapyard_entrance",
            Name = "The Gates",
            Description = "Mountains of rusted metal stretch toward a grey sky.",
            DungeonId = "rust_scrapyard",
            Layout = RoomLayout.OpenSquare,
            LoreText = "Everything that was discarded ended up here. Including hope."
        });

        Register(new AuthoredRoom
        {
            Id = "rust_scrapyard_crusher",
            Name = "The Crusher",
            Description = "Massive hydraulic presses still function, smashing everything flat.",
            DungeonId = "rust_scrapyard",
            Layout = RoomLayout.NarrowCorridor,
            Hazards = new List<RoomHazard> { RoomHazard.SpikeTrap },
            FixedEncounterId = "rust_scrap_patrol"
        });

        Register(new AuthoredRoom
        {
            Id = "rust_scrapyard_throne",
            Name = "The Iron Throne",
            Description = "The Colossus has built itself a monument from the dead machines.",
            DungeonId = "rust_scrapyard",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.CollapsingFloor },
            EntryDialogId = "boss_scrap_colossus_intro"
        });
    }

    /// <summary>
    /// Green biome dungeon content.
    /// </summary>
    private static void RegisterGreenContent()
    {
        // === ENCOUNTERS ===

        Register(new DungeonEncounter
        {
            Id = "green_vines",
            Enemies = new Dictionary<string, int> { { "vine_strangler", 2 }, { "spore_walker", 2 } },
            MinLevel = 10,
            MaxLevel = 18,
            Weight = 18
        });

        Register(new DungeonEncounter
        {
            Id = "green_thorns",
            Enemies = new Dictionary<string, int> { { "thorn_beast", 2 }, { "pollen_cloud", 1 } },
            MinLevel = 12,
            MaxLevel = 20,
            Weight = 15,
            Hazard = RoomHazard.ToxicGas
        });

        Register(new DungeonEncounter
        {
            Id = "green_lab_specimens",
            Enemies = new Dictionary<string, int>
            {
                { "lab_specimen", 3 },
                { "gene_horror", 1 }
            },
            MinLevel = 15,
            MaxLevel = 25,
            Weight = 12,
            PreBattleDialogId = "encounter_lab_specimen"
        });

        Register(new DungeonEncounter
        {
            Id = "green_chimera",
            Enemies = new Dictionary<string, int> { { "chimera", 1 }, { "bio_construct", 2 } },
            MinLevel = 18,
            MaxLevel = 28,
            Weight = 8,
            Reinforcements = new List<string> { "lab_specimen", "lab_specimen" }
        });

        // === AUTHORED ROOMS ===

        Register(new AuthoredRoom
        {
            Id = "green_greenhouse_atrium",
            Name = "Central Atrium",
            Description = "A glass dome, shattered long ago. The vegetation has claimed everything.",
            DungeonId = "green_greenhouse",
            Layout = RoomLayout.Circular,
            Hazards = new List<RoomHazard> { RoomHazard.ToxicGas },
            LoreText = "This was meant to be a sanctuary of life. In a way, it succeeded."
        });

        Register(new AuthoredRoom
        {
            Id = "green_greenhouse_nursery",
            Name = "The Nursery",
            Description = "Seedling pods line the walls. Some still pulse with growth.",
            DungeonId = "green_greenhouse",
            Layout = RoomLayout.Alcoved,
            FixedEncounterId = "green_vines",
            Features = new List<RoomFeature> { RoomFeature.HealingStation }
        });

        Register(new AuthoredRoom
        {
            Id = "green_greenhouse_heart",
            Name = "The Heart",
            Description = "A massive organic growth pulses at the center. The Titan guards it.",
            DungeonId = "green_greenhouse",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.ToxicGas },
            EntryDialogId = "boss_overgrowth_titan_intro"
        });

        Register(new AuthoredRoom
        {
            Id = "green_lab_reception",
            Name = "Reception Hall",
            Description = "Lazarus's logo still adorns the wall. 'Preserving Tomorrow, Today.'",
            DungeonId = "green_laboratory",
            Layout = RoomLayout.OpenSquare,
            Features = new List<RoomFeature> { RoomFeature.DataTerminal },
            LoreText = "Welcome to Lazarus Bio-Research. Your appointment has been confirmed."
        });

        Register(new AuthoredRoom
        {
            Id = "green_lab_containment",
            Name = "Containment Wing",
            Description = "Shattered tanks and escaped specimens. Nothing is contained anymore.",
            DungeonId = "green_laboratory",
            Layout = RoomLayout.LCorridor,
            FixedEncounterId = "green_lab_specimens",
            Hazards = new List<RoomHazard> { RoomHazard.ToxicGas }
        });

        Register(new AuthoredRoom
        {
            Id = "green_lab_perfect",
            Name = "Chamber Zero",
            Description = "The final experiment. Lazarus's 'perfect' creation awaits.",
            DungeonId = "green_laboratory",
            Layout = RoomLayout.BossArena,
            EntryDialogId = "boss_perfect_organism_intro"
        });
    }

    /// <summary>
    /// Quiet biome dungeon content.
    /// </summary>
    private static void RegisterQuietContent()
    {
        // === ENCOUNTERS ===

        Register(new DungeonEncounter
        {
            Id = "quiet_echoes",
            Enemies = new Dictionary<string, int> { { "echo_hunter", 2 }, { "void_walker", 1 } },
            MinLevel = 12,
            MaxLevel = 20,
            Weight = 18,
            Hazard = RoomHazard.Silence
        });

        Register(new DungeonEncounter
        {
            Id = "quiet_feeders",
            Enemies = new Dictionary<string, int> { { "silence_feeder", 2 }, { "null_shade", 2 } },
            MinLevel = 14,
            MaxLevel = 22,
            Weight = 15,
            Hazard = RoomHazard.Darkness
        });

        Register(new DungeonEncounter
        {
            Id = "quiet_cathedral_choir",
            Enemies = new Dictionary<string, int>
            {
                { "whisper_wraith", 2 },
                { "choir_ghoul", 2 }
            },
            MinLevel = 18,
            MaxLevel = 28,
            Weight = 12,
            PreBattleDialogId = "encounter_cathedral_choir"
        });

        Register(new DungeonEncounter
        {
            Id = "quiet_sound_eaters",
            Enemies = new Dictionary<string, int>
            {
                { "sound_eater", 1 },
                { "bell_horror", 2 }
            },
            MinLevel = 22,
            MaxLevel = 32,
            Weight = 8,
            Hazard = RoomHazard.Silence
        });

        // === AUTHORED ROOMS ===

        Register(new AuthoredRoom
        {
            Id = "quiet_bunker_entrance",
            Name = "Blast Door",
            Description = "A reinforced entrance, designed to withstand anything. Except silence.",
            DungeonId = "quiet_bunker",
            Layout = RoomLayout.NarrowCorridor,
            Hazards = new List<RoomHazard> { RoomHazard.Silence },
            LoreText = "MILITARY INSTALLATION - UNAUTHORIZED ACCESS PROHIBITED"
        });

        Register(new AuthoredRoom
        {
            Id = "quiet_bunker_barracks",
            Name = "Empty Barracks",
            Description = "Beds still made. Footlockers still locked. Where did they all go?",
            DungeonId = "quiet_bunker",
            Layout = RoomLayout.Alcoved,
            Features = new List<RoomFeature> { RoomFeature.TreasureChest },
            LoreText = "Personal effects remain: photos, letters, hopes."
        });

        Register(new AuthoredRoom
        {
            Id = "quiet_bunker_command",
            Name = "Command Center",
            Description = "The Absolute Silence waits here, consuming all sound.",
            DungeonId = "quiet_bunker",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.Silence, RoomHazard.Darkness },
            EntryDialogId = "boss_absolute_silence_intro"
        });

        Register(new AuthoredRoom
        {
            Id = "quiet_cathedral_nave",
            Name = "The Nave",
            Description = "Pews lined with dust. The echo of prayers that will never be answered.",
            DungeonId = "quiet_cathedral",
            Layout = RoomLayout.NarrowCorridor,
            FixedEncounterId = "quiet_cathedral_choir",
            LoreText = "They came here seeking salvation. They found only silence."
        });

        Register(new AuthoredRoom
        {
            Id = "quiet_cathedral_choir_loft",
            Name = "Choir Loft",
            Description = "The singers still practice, but no sound emerges.",
            DungeonId = "quiet_cathedral",
            Layout = RoomLayout.Platforms,
            Hazards = new List<RoomHazard> { RoomHazard.Silence },
            Features = new List<RoomFeature> { RoomFeature.SecretArea }
        });

        Register(new AuthoredRoom
        {
            Id = "quiet_cathedral_altar",
            Name = "The Altar",
            Description = "The Voice of the Void speaks here. Or rather, unspeak.",
            DungeonId = "quiet_cathedral",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.Silence },
            EntryDialogId = "boss_voice_of_void_intro"
        });
    }

    /// <summary>
    /// Teeth biome dungeon content.
    /// </summary>
    private static void RegisterTeethContent()
    {
        // === ENCOUNTERS ===

        Register(new DungeonEncounter
        {
            Id = "teeth_crawlers",
            Enemies = new Dictionary<string, int> { { "bone_crawler", 3 }, { "marrow_leech", 2 } },
            MinLevel = 15,
            MaxLevel = 25,
            Weight = 18
        });

        Register(new DungeonEncounter
        {
            Id = "teeth_calcified",
            Enemies = new Dictionary<string, int>
            {
                { "calcified_horror", 1 },
                { "skeletal_amalgam", 2 }
            },
            MinLevel = 18,
            MaxLevel = 28,
            Weight = 15
        });

        Register(new DungeonEncounter
        {
            Id = "teeth_maw_denizens",
            Enemies = new Dictionary<string, int>
            {
                { "tooth_worm", 2 },
                { "jaw_trap", 2 }
            },
            MinLevel = 22,
            MaxLevel = 32,
            Weight = 12,
            Hazard = RoomHazard.SpikeTrap
        });

        Register(new DungeonEncounter
        {
            Id = "teeth_enamel_beasts",
            Enemies = new Dictionary<string, int>
            {
                { "enamel_beast", 1 },
                { "cavity_horror", 2 }
            },
            MinLevel = 26,
            MaxLevel = 36,
            Weight = 8
        });

        // === AUTHORED ROOMS ===

        Register(new AuthoredRoom
        {
            Id = "teeth_ossuary_crypt",
            Name = "The Crypt",
            Description = "Walls of bone. Floors of bone. Everything is bone.",
            DungeonId = "teeth_ossuary",
            Layout = RoomLayout.LCorridor,
            FixedEncounterId = "teeth_crawlers",
            LoreText = "The dead donate to the architecture."
        });

        Register(new AuthoredRoom
        {
            Id = "teeth_ossuary_shrine",
            Name = "Bone Shrine",
            Description = "Someone - or something - has arranged the bones into a pattern.",
            DungeonId = "teeth_ossuary",
            Layout = RoomLayout.Circular,
            Features = new List<RoomFeature> { RoomFeature.SecretArea, RoomFeature.DataTerminal },
            LoreText = "The pattern seems to spell something. A name? A warning?"
        });

        Register(new AuthoredRoom
        {
            Id = "teeth_ossuary_guardian",
            Name = "Guardian's Chamber",
            Description = "The Ossuary Guardian has assembled itself from a thousand donors.",
            DungeonId = "teeth_ossuary",
            Layout = RoomLayout.BossArena,
            EntryDialogId = "boss_ossuary_guardian_intro"
        });

        Register(new AuthoredRoom
        {
            Id = "teeth_maw_gullet",
            Name = "The Gullet",
            Description = "The walls seem to contract. The floor is warm and wet.",
            DungeonId = "teeth_maw",
            Layout = RoomLayout.NarrowCorridor,
            Hazards = new List<RoomHazard> { RoomHazard.AcidPools },
            FixedEncounterId = "teeth_maw_denizens"
        });

        Register(new AuthoredRoom
        {
            Id = "teeth_maw_stomach",
            Name = "The Stomach",
            Description = "You are inside something alive. It knows you're here.",
            DungeonId = "teeth_maw",
            Layout = RoomLayout.Arena,
            Hazards = new List<RoomHazard> { RoomHazard.AcidPools, RoomHazard.ToxicGas },
            LoreText = "The Maw doesn't eat. It absorbs."
        });

        Register(new AuthoredRoom
        {
            Id = "teeth_maw_heart",
            Name = "The Maw's Heart",
            Description = "A pulsing core of calcified hunger. It wants to add you to itself.",
            DungeonId = "teeth_maw",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.AcidPools },
            EntryDialogId = "boss_maw_intro"
        });
    }

    /// <summary>
    /// Glow biome dungeon content.
    /// </summary>
    private static void RegisterGlowContent()
    {
        // === ENCOUNTERS ===

        Register(new DungeonEncounter
        {
            Id = "glow_hounds",
            Enemies = new Dictionary<string, int> { { "rad_hound", 3 }, { "glow_stalker", 1 } },
            MinLevel = 20,
            MaxLevel = 30,
            Weight = 18,
            Hazard = RoomHazard.RadiationZones
        });

        Register(new DungeonEncounter
        {
            Id = "glow_nuclear",
            Enemies = new Dictionary<string, int>
            {
                { "isotope_horror", 1 },
                { "nuclear_shade", 2 }
            },
            MinLevel = 24,
            MaxLevel = 34,
            Weight = 12,
            Hazard = RoomHazard.RadiationZones
        });

        Register(new DungeonEncounter
        {
            Id = "glow_nimdok_forces",
            Enemies = new Dictionary<string, int>
            {
                { "nimdok_drone", 2 },
                { "core_guardian", 1 }
            },
            MinLevel = 28,
            MaxLevel = 40,
            Weight = 10,
            PreBattleDialogId = "encounter_nimdok_drone"
        });

        Register(new DungeonEncounter
        {
            Id = "glow_data_phantoms",
            Enemies = new Dictionary<string, int>
            {
                { "data_phantom", 2 },
                { "light_eater", 1 }
            },
            MinLevel = 32,
            MaxLevel = 45,
            Weight = 8
        });

        // === AUTHORED ROOMS ===

        Register(new AuthoredRoom
        {
            Id = "glow_reactor_cooling",
            Name = "Cooling Tower",
            Description = "Failed cooling systems. The radiation is intense.",
            DungeonId = "glow_reactor",
            Layout = RoomLayout.Circular,
            Hazards = new List<RoomHazard> { RoomHazard.RadiationZones },
            FixedEncounterId = "glow_hounds"
        });

        Register(new AuthoredRoom
        {
            Id = "glow_reactor_control",
            Name = "Control Room",
            Description = "Emergency warnings flash endlessly. No one is left to heed them.",
            DungeonId = "glow_reactor",
            Layout = RoomLayout.CentralPillar,
            Features = new List<RoomFeature> { RoomFeature.DataTerminal },
            LoreText = "MELTDOWN IMMINENT - EVACUATE IMMEDIATELY - MELTDOWN IMMINENT"
        });

        Register(new AuthoredRoom
        {
            Id = "glow_reactor_core",
            Name = "Reactor Core",
            Description = "Critical Mass awaits at the heart of the eternal meltdown.",
            DungeonId = "glow_reactor",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.RadiationZones, RoomHazard.ElectricalArcs },
            EntryDialogId = "boss_critical_mass_intro"
        });

        Register(new AuthoredRoom
        {
            Id = "glow_gate_perimeter",
            Name = "Security Perimeter",
            Description = "Lazarus's outer defenses. Drones patrol endlessly.",
            DungeonId = "glow_nimdok_gate",
            Layout = RoomLayout.OpenSquare,
            FixedEncounterId = "glow_nimdok_forces"
        });

        Register(new AuthoredRoom
        {
            Id = "glow_gate_firewall",
            Name = "The Firewall",
            Description = "A manifestation of digital security made physical.",
            DungeonId = "glow_nimdok_gate",
            Layout = RoomLayout.Arena,
            Hazards = new List<RoomHazard> { RoomHazard.FirePits, RoomHazard.ElectricalArcs },
            EntryDialogId = "boss_firewall_construct_intro"
        });

        Register(new AuthoredRoom
        {
            Id = "glow_gate_avatar",
            Name = "Avatar Chamber",
            Description = "Lazarus manifests here. This is as close to the real thing as you can get.",
            DungeonId = "glow_nimdok_gate",
            Layout = RoomLayout.BossArena,
            EntryDialogId = "boss_nimdok_avatar_intro"
        });
    }

    /// <summary>
    /// Archive Scar biome dungeon content.
    /// </summary>
    private static void RegisterArchiveContent()
    {
        // === ENCOUNTERS ===

        Register(new DungeonEncounter
        {
            Id = "archive_echoes",
            Enemies = new Dictionary<string, int>
            {
                { "memory_echo", 2 },
                { "forgotten_one", 2 }
            },
            MinLevel = 25,
            MaxLevel = 35,
            Weight = 18
        });

        Register(new DungeonEncounter
        {
            Id = "archive_corruption",
            Enemies = new Dictionary<string, int>
            {
                { "data_corruptor", 1 },
                { "archive_worm", 3 }
            },
            MinLevel = 28,
            MaxLevel = 38,
            Weight = 15
        });

        Register(new DungeonEncounter
        {
            Id = "archive_sentinels",
            Enemies = new Dictionary<string, int>
            {
                { "core_sentinel", 2 },
                { "truth_seeker", 1 }
            },
            MinLevel = 35,
            MaxLevel = 50,
            Weight = 10,
            PreBattleDialogId = "encounter_archive_sentinel"
        });

        Register(new DungeonEncounter
        {
            Id = "archive_fragments",
            Enemies = new Dictionary<string, int>
            {
                { "nimdok_fragment", 2 },
                { "ultimate_form", 1 }
            },
            MinLevel = 40,
            MaxLevel = 55,
            Weight = 6
        });

        // === AUTHORED ROOMS ===

        Register(new AuthoredRoom
        {
            Id = "archive_memory_hall",
            Name = "Hall of Memories",
            Description = "Fragments of human history float in the air as data.",
            DungeonId = "archive_memory_banks",
            Layout = RoomLayout.NarrowCorridor,
            FixedEncounterId = "archive_echoes",
            LoreText = "Each mote of light is a life. A memory. A moment lost to time."
        });

        Register(new AuthoredRoom
        {
            Id = "archive_memory_vault",
            Name = "Memory Vault",
            Description = "Protected memories. Things Lazarus deemed too precious to lose.",
            DungeonId = "archive_memory_banks",
            Layout = RoomLayout.CentralPillar,
            Features = new List<RoomFeature> { RoomFeature.DataTerminal, RoomFeature.SecretArea },
            LoreText = "Access restricted. Emotional content. Handle with care."
        });

        Register(new AuthoredRoom
        {
            Id = "archive_memory_corrupt",
            Name = "Corrupted Sector",
            Description = "The Corrupted Archive feeds on forgotten data.",
            DungeonId = "archive_memory_banks",
            Layout = RoomLayout.BossArena,
            Hazards = new List<RoomHazard> { RoomHazard.Darkness },
            EntryDialogId = "boss_corrupted_archive_intro"
        });

        Register(new AuthoredRoom
        {
            Id = "archive_core_approach",
            Name = "The Approach",
            Description = "Data streams converge. You are close to the truth.",
            DungeonId = "archive_core",
            Layout = RoomLayout.Platforms,
            FixedEncounterId = "archive_sentinels"
        });

        Register(new AuthoredRoom
        {
            Id = "archive_core_truth",
            Name = "Chamber of Truth",
            Description = "The Truth Guardian protects the final secrets.",
            DungeonId = "archive_core",
            Layout = RoomLayout.Arena,
            EntryDialogId = "boss_truth_guardian_intro"
        });

        Register(new AuthoredRoom
        {
            Id = "archive_core_final",
            Name = "Lazarus's Heart",
            Description = "The true form of Lazarus. Everything leads here.",
            DungeonId = "archive_core",
            Layout = RoomLayout.BossArena,
            EntryDialogId = "boss_nimdok_true_form_intro"
        });
    }

    /// <summary>
    /// Boss phase configurations.
    /// </summary>
    private static void RegisterBossContent()
    {
        // Sewer King - 3 phases
        RegisterBossPhases("sewer_king", new List<BossPhase>
        {
            new()
            {
                PhaseNumber = 0,
                HpThreshold = 1f,
                PhaseDialogId = "boss_sewer_king_phase1",
                UnlockedAbilities = new List<string> { "toxic_spray", "summon_rats" }
            },
            new()
            {
                PhaseNumber = 1,
                HpThreshold = 0.6f,
                PhaseDialogId = "boss_sewer_king_phase2",
                Adds = new Dictionary<string, int> { { "toxic_blob", 2 } },
                UnlockedAbilities = new List<string> { "acid_rain" },
                ActiveHazard = RoomHazard.AcidPools
            },
            new()
            {
                PhaseNumber = 2,
                HpThreshold = 0.25f,
                PhaseDialogId = "boss_sewer_king_phase3",
                UnlockedAbilities = new List<string> { "desperate_frenzy" },
                StatMultiplier = 1.5f
            }
        });

        // Scrap Colossus - 4 phases
        RegisterBossPhases("scrap_colossus", new List<BossPhase>
        {
            new()
            {
                PhaseNumber = 0,
                HpThreshold = 1f,
                PhaseDialogId = "boss_colossus_phase1",
                UnlockedAbilities = new List<string> { "metal_slam", "junk_toss" }
            },
            new()
            {
                PhaseNumber = 1,
                HpThreshold = 0.75f,
                PhaseDialogId = "boss_colossus_phase2",
                Adds = new Dictionary<string, int> { { "crusher_bot", 1 } },
                UnlockedAbilities = new List<string> { "magnet_pull" }
            },
            new()
            {
                PhaseNumber = 2,
                HpThreshold = 0.5f,
                PhaseDialogId = "boss_colossus_phase3",
                UnlockedAbilities = new List<string> { "rebuild" },
                ActiveHazard = RoomHazard.CollapsingFloor,
                StatMultiplier = 1.25f
            },
            new()
            {
                PhaseNumber = 3,
                HpThreshold = 0.2f,
                PhaseDialogId = "boss_colossus_phase4",
                UnlockedAbilities = new List<string> { "final_collapse" },
                StatMultiplier = 1.75f
            }
        });

        // Perfect Organism - 3 phases
        RegisterBossPhases("perfect_organism", new List<BossPhase>
        {
            new()
            {
                PhaseNumber = 0,
                HpThreshold = 1f,
                PhaseDialogId = "boss_organism_phase1",
                UnlockedAbilities = new List<string> { "bio_lance", "regenerate" }
            },
            new()
            {
                PhaseNumber = 1,
                HpThreshold = 0.5f,
                PhaseDialogId = "boss_organism_phase2",
                Adds = new Dictionary<string, int> { { "bio_construct", 2 } },
                UnlockedAbilities = new List<string> { "evolution_pulse" },
                ActiveHazard = RoomHazard.ToxicGas
            },
            new()
            {
                PhaseNumber = 2,
                HpThreshold = 0.2f,
                PhaseDialogId = "boss_organism_phase3",
                UnlockedAbilities = new List<string> { "perfect_form" },
                StatMultiplier = 2f
            }
        });

        // Voice of the Void - 4 phases
        RegisterBossPhases("voice_of_void", new List<BossPhase>
        {
            new()
            {
                PhaseNumber = 0,
                HpThreshold = 1f,
                PhaseDialogId = "boss_void_phase1",
                UnlockedAbilities = new List<string> { "silence_wave", "void_bolt" },
                ActiveHazard = RoomHazard.Silence
            },
            new()
            {
                PhaseNumber = 1,
                HpThreshold = 0.7f,
                PhaseDialogId = "boss_void_phase2",
                Adds = new Dictionary<string, int> { { "whisper_wraith", 2 } },
                UnlockedAbilities = new List<string> { "consume_sound" }
            },
            new()
            {
                PhaseNumber = 2,
                HpThreshold = 0.4f,
                PhaseDialogId = "boss_void_phase3",
                UnlockedAbilities = new List<string> { "absolute_silence" },
                ActiveHazard = RoomHazard.Darkness,
                StatMultiplier = 1.5f
            },
            new()
            {
                PhaseNumber = 3,
                HpThreshold = 0.15f,
                PhaseDialogId = "boss_void_phase4",
                UnlockedAbilities = new List<string> { "void_scream" },
                StatMultiplier = 2f
            }
        });

        // The Maw - 3 phases
        RegisterBossPhases("the_maw", new List<BossPhase>
        {
            new()
            {
                PhaseNumber = 0,
                HpThreshold = 1f,
                PhaseDialogId = "boss_maw_phase1",
                UnlockedAbilities = new List<string> { "devour", "tooth_barrage" },
                ActiveHazard = RoomHazard.AcidPools
            },
            new()
            {
                PhaseNumber = 1,
                HpThreshold = 0.5f,
                PhaseDialogId = "boss_maw_phase2",
                Adds = new Dictionary<string, int> { { "tooth_worm", 3 } },
                UnlockedAbilities = new List<string> { "digest" }
            },
            new()
            {
                PhaseNumber = 2,
                HpThreshold = 0.2f,
                PhaseDialogId = "boss_maw_phase3",
                UnlockedAbilities = new List<string> { "ultimate_hunger" },
                StatMultiplier = 2.5f
            }
        });

        // Lazarus Avatar - 5 phases
        RegisterBossPhases("nimdok_avatar", new List<BossPhase>
        {
            new()
            {
                PhaseNumber = 0,
                HpThreshold = 1f,
                PhaseDialogId = "boss_avatar_phase1",
                UnlockedAbilities = new List<string> { "data_beam", "firewall" }
            },
            new()
            {
                PhaseNumber = 1,
                HpThreshold = 0.8f,
                PhaseDialogId = "boss_avatar_phase2",
                Adds = new Dictionary<string, int> { { "nimdok_drone", 2 } },
                UnlockedAbilities = new List<string> { "system_scan" }
            },
            new()
            {
                PhaseNumber = 2,
                HpThreshold = 0.6f,
                PhaseDialogId = "boss_avatar_phase3",
                UnlockedAbilities = new List<string> { "memory_wipe" },
                ActiveHazard = RoomHazard.ElectricalArcs
            },
            new()
            {
                PhaseNumber = 3,
                HpThreshold = 0.35f,
                PhaseDialogId = "boss_avatar_phase4",
                Adds = new Dictionary<string, int> { { "data_phantom", 2 } },
                UnlockedAbilities = new List<string> { "avatar_rage" },
                StatMultiplier = 1.5f
            },
            new()
            {
                PhaseNumber = 4,
                HpThreshold = 0.1f,
                PhaseDialogId = "boss_avatar_phase5",
                UnlockedAbilities = new List<string> { "total_deletion" },
                StatMultiplier = 2f
            }
        });

        // Lazarus True Form - 5 phases (final boss)
        RegisterBossPhases("nimdok_true_form", new List<BossPhase>
        {
            new()
            {
                PhaseNumber = 0,
                HpThreshold = 1f,
                PhaseDialogId = "boss_nimdok_phase1",
                UnlockedAbilities = new List<string> { "archive_beam", "data_storm" }
            },
            new()
            {
                PhaseNumber = 1,
                HpThreshold = 0.8f,
                PhaseDialogId = "boss_nimdok_phase2",
                Adds = new Dictionary<string, int> { { "core_sentinel", 2 } },
                UnlockedAbilities = new List<string> { "memory_assault" }
            },
            new()
            {
                PhaseNumber = 2,
                HpThreshold = 0.6f,
                PhaseDialogId = "boss_nimdok_phase3",
                UnlockedAbilities = new List<string> { "truth_revelation" },
                StatMultiplier = 1.25f
            },
            new()
            {
                PhaseNumber = 3,
                HpThreshold = 0.4f,
                PhaseDialogId = "boss_nimdok_phase4",
                Adds = new Dictionary<string, int> { { "nimdok_fragment", 2 } },
                UnlockedAbilities = new List<string> { "preservation_protocol" },
                StatMultiplier = 1.5f
            },
            new()
            {
                PhaseNumber = 4,
                HpThreshold = 0.15f,
                PhaseDialogId = "boss_nimdok_phase5",
                UnlockedAbilities = new List<string> { "final_judgment" },
                StatMultiplier = 2f
            }
        });
    }
}
