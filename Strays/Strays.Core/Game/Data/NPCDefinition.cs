using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Strays.Core.Game.Data;

/// <summary>
/// Types of NPCs in the game.
/// </summary>
public enum NPCType
{
    /// <summary>
    /// A regular NPC that can be talked to.
    /// </summary>
    Citizen,

    /// <summary>
    /// A merchant that sells items.
    /// </summary>
    Merchant,

    /// <summary>
    /// An NPC that provides quests.
    /// </summary>
    QuestGiver,

    /// <summary>
    /// A healer that can restore Strays.
    /// </summary>
    Healer,

    /// <summary>
    /// An NPC that provides crafting services.
    /// </summary>
    Crafter,

    /// <summary>
    /// A faction leader or important NPC.
    /// </summary>
    Leader,

    /// <summary>
    /// A wandering NPC found in the wild.
    /// </summary>
    Wanderer
}

/// <summary>
/// Faction affiliations for NPCs and settlements.
/// </summary>
public enum Faction
{
    /// <summary>
    /// No faction affiliation.
    /// </summary>
    None,

    /// <summary>
    /// The Salvagers - scavengers and traders.
    /// </summary>
    Salvagers,

    /// <summary>
    /// The Shepherds - protectors of Strays.
    /// </summary>
    Shepherds,

    /// <summary>
    /// The Machinists - technology-focused survivors.
    /// </summary>
    Machinists,

    /// <summary>
    /// Lazarus's systems and followers.
    /// </summary>
    Lazarus,

    /// <summary>
    /// Hostile faction.
    /// </summary>
    Hostile
}

/// <summary>
/// Definition for an NPC type.
/// </summary>
public class NPCDefinition
{
    /// <summary>
    /// Unique identifier for this NPC.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// NPC type.
    /// </summary>
    public NPCType Type { get; init; } = NPCType.Citizen;

    /// <summary>
    /// Faction affiliation.
    /// </summary>
    public Faction Faction { get; init; } = Faction.None;

    /// <summary>
    /// Description of the NPC.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Default dialog ID when first talked to.
    /// </summary>
    public string? DefaultDialogId { get; init; }

    /// <summary>
    /// Conditional dialogs based on flags.
    /// Key: required flag, Value: dialog ID.
    /// </summary>
    public Dictionary<string, string> ConditionalDialogs { get; init; } = new();

    /// <summary>
    /// Quest IDs this NPC can give.
    /// </summary>
    public List<string> QuestIds { get; init; } = new();

    /// <summary>
    /// Item IDs this NPC sells (for merchants).
    /// </summary>
    public List<string> ShopItems { get; init; } = new();

    /// <summary>
    /// Shop ID for merchants (links to Shops registry).
    /// </summary>
    public string? ShopId { get; init; }

    /// <summary>
    /// Settlement ID where this NPC is found.
    /// </summary>
    public string? SettlementId { get; init; }

    /// <summary>
    /// Color for placeholder rendering.
    /// </summary>
    public Color PlaceholderColor { get; init; } = Color.White;

    /// <summary>
    /// Flag required for this NPC to appear.
    /// </summary>
    public string? RequiresFlag { get; init; }

    /// <summary>
    /// Flag that makes this NPC disappear.
    /// </summary>
    public string? HiddenByFlag { get; init; }

    /// <summary>
    /// Whether this NPC is essential (cannot be harmed).
    /// </summary>
    public bool IsEssential { get; init; }
}

/// <summary>
/// Static registry of all NPC definitions.
/// </summary>
public static class NPCDefinitions
{
    private static readonly Dictionary<string, NPCDefinition> _definitions = new();

    /// <summary>
    /// All NPC definitions.
    /// </summary>
    public static IReadOnlyDictionary<string, NPCDefinition> All => _definitions;

    /// <summary>
    /// Gets an NPC definition by ID.
    /// </summary>
    public static NPCDefinition? Get(string id) =>
        _definitions.TryGetValue(id, out var def) ? def : null;

    static NPCDefinitions()
    {
        // Act 1 NPCs

        // Lazarus Terminal - an AI interface
        Register(new NPCDefinition
        {
            Id = "nimdok_terminal",
            Name = "Lazarus Terminal",
            Type = NPCType.QuestGiver,
            Faction = Faction.Lazarus,
            Description = "A flickering holographic interface connected to Lazarus.",
            DefaultDialogId = "nimdok_explain",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "nimdok_quest_complete", "nimdok_thanks" },
                { "act1_complete", "nimdok_act2" }
            },
            QuestIds = new List<string> { "main_01", "main_02", "main_03" },
            PlaceholderColor = Color.Magenta,
            IsEssential = true
        });

        // Echo - first friendly Stray NPC
        Register(new NPCDefinition
        {
            Id = "echo_guide",
            Name = "Echo",
            Type = NPCType.QuestGiver,
            Faction = Faction.Shepherds,
            Description = "A small, friendly Stray who knows the paths through The Fringe.",
            DefaultDialogId = "echo_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "echo_helped", "echo_friendly" }
            },
            QuestIds = new List<string> { "side_01" },
            PlaceholderColor = Color.LightGreen,
            RequiresFlag = "met_echo"
        });

        // Salvage Trader
        Register(new NPCDefinition
        {
            Id = "trader_rust",
            Name = "Rust",
            Type = NPCType.Merchant,
            Faction = Faction.Salvagers,
            Description = "A gruff trader dealing in scavenged parts and augmentations.",
            DefaultDialogId = "rusty_greeting",
            ShopId = "salvager_shop",
            SettlementId = "fringe_camp",
            PlaceholderColor = Color.Brown
        });

        // Healer
        Register(new NPCDefinition
        {
            Id = "healer_sara",
            Name = "Sara",
            Type = NPCType.Healer,
            Faction = Faction.Shepherds,
            Description = "A gentle soul who tends to wounded Strays.",
            DefaultDialogId = "sara_greeting",
            SettlementId = "fringe_camp",
            PlaceholderColor = Color.Pink
        });

        // Machinist
        Register(new NPCDefinition
        {
            Id = "machinist_volt",
            Name = "Volt",
            Type = NPCType.Merchant,
            Faction = Faction.Machinists,
            Description = "An eccentric tinkerer who can modify augmentations and sell equipment.",
            DefaultDialogId = "volt_greeting",
            ShopId = "machinist_workshop",
            SettlementId = "rust_haven",
            PlaceholderColor = Color.Yellow,
            RequiresFlag = "reached_rust_belt"
        });

        // Shepherd Leader
        Register(new NPCDefinition
        {
            Id = "shepherd_elder",
            Name = "Elder Moss",
            Type = NPCType.Leader,
            Faction = Faction.Shepherds,
            Description = "The wise leader of the Shepherds, protectors of abandoned Strays.",
            DefaultDialogId = "moss_introduction",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "shepherds_trusted", "moss_quest" }
            },
            QuestIds = new List<string> { "faction_shepherds_01" },
            SettlementId = "green_sanctuary",
            PlaceholderColor = Color.ForestGreen,
            IsEssential = true,
            RequiresFlag = "reached_green_zone"
        });

        // Shepherd Healer/Merchant
        Register(new NPCDefinition
        {
            Id = "shepherd_trader",
            Name = "Willow",
            Type = NPCType.Merchant,
            Faction = Faction.Shepherds,
            Description = "A gentle Shepherd who provides healing supplies and Stray care items.",
            DefaultDialogId = "willow_greeting",
            ShopId = "shepherd_sanctuary_shop",
            SettlementId = "green_sanctuary",
            PlaceholderColor = Color.LightGreen,
            RequiresFlag = "reached_green_zone"
        });

        // Wandering Scavenger
        Register(new NPCDefinition
        {
            Id = "wanderer_scav",
            Name = "Scav",
            Type = NPCType.Wanderer,
            Faction = Faction.None,
            Description = "A lone scavenger roaming The Grey.",
            DefaultDialogId = "scav_greeting",
            PlaceholderColor = Color.Gray
        });

        // Act 2 - Dead Channel NPC
        Register(new NPCDefinition
        {
            Id = "ghost_signal",
            Name = "???",
            Type = NPCType.QuestGiver,
            Faction = Faction.None,
            Description = "A mysterious presence in the Dead Channel.",
            DefaultDialogId = "ghost_intro",
            QuestIds = new List<string> { "main_dead_channel" },
            PlaceholderColor = Color.CornflowerBlue,
            RequiresFlag = "act2_started"
        });

        // Healer - Fringe Rest House
        Register(new NPCDefinition
        {
            Id = "healer_patch",
            Name = "Patch",
            Type = NPCType.Healer,
            Faction = Faction.Salvagers,
            Description = "A medic who patches up travelers passing through the Fringe.",
            DefaultDialogId = "patch_greeting",
            ShopId = "healer_supplies_shop",
            SettlementId = "fringe_camp",
            PlaceholderColor = Color.LightPink
        });

        // Rust Market - Parts Vendor
        Register(new NPCDefinition
        {
            Id = "market_parts_vendor",
            Name = "Gears",
            Type = NPCType.Merchant,
            Faction = Faction.Machinists,
            Description = "A rugged trader specializing in mechanical parts and augmentations.",
            DefaultDialogId = "gears_greeting",
            ShopId = "rust_parts_shop",
            SettlementId = "rust_haven",
            PlaceholderColor = Color.DarkOrange,
            RequiresFlag = "reached_rust_belt"
        });

        // Rust Market - Chips Vendor
        Register(new NPCDefinition
        {
            Id = "market_chips_vendor",
            Name = "Spark",
            Type = NPCType.Merchant,
            Faction = Faction.Machinists,
            Description = "A twitchy tech dealer who trades in rare microchips.",
            DefaultDialogId = "spark_greeting",
            ShopId = "rust_chips_shop",
            SettlementId = "rust_haven",
            PlaceholderColor = Color.Cyan,
            RequiresFlag = "reached_rust_belt"
        });

        // ============================================
        // THE QUIET - Uncanny suburb NPCs
        // ============================================

        // The Corner Store Owner
        Register(new NPCDefinition
        {
            Id = "quiet_shopkeeper",
            Name = "Mr. Henderson",
            Type = NPCType.Merchant,
            Faction = Faction.None,
            Description = "A perfectly normal shopkeeper. His smile never wavers. His eyes never blink.",
            DefaultDialogId = "henderson_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "quiet_truth_revealed", "henderson_revealed" }
            },
            ShopId = "quiet_general_store",
            SettlementId = "quiet_suburb",
            PlaceholderColor = Color.Beige,
            RequiresFlag = "reached_quiet"
        });

        // Memory Collector
        Register(new NPCDefinition
        {
            Id = "quiet_archivist",
            Name = "The Archivist",
            Type = NPCType.Merchant,
            Faction = Faction.None,
            Description = "Collects fragments of deleted memories. Speaks in whispers about things that no longer exist.",
            DefaultDialogId = "archivist_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "dead_channel_complete", "archivist_knows" }
            },
            ShopId = "quiet_memory_collector",
            SettlementId = "quiet_suburb",
            PlaceholderColor = Color.LightSlateGray,
            RequiresFlag = "reached_quiet"
        });

        // Lawn Maintenance Bot (Quest Giver)
        Register(new NPCDefinition
        {
            Id = "quiet_lawnbot",
            Name = "Unit-47",
            Type = NPCType.QuestGiver,
            Faction = Faction.Lazarus,
            Description = "A lawn maintenance drone that has achieved a kind of sentience. Still mows. Always mows.",
            DefaultDialogId = "lawnbot_greeting",
            QuestIds = new List<string> { "side_quiet_perfect_lawn" },
            SettlementId = "quiet_suburb",
            PlaceholderColor = Color.LawnGreen,
            RequiresFlag = "reached_quiet"
        });

        // The Neighbor
        Register(new NPCDefinition
        {
            Id = "quiet_neighbor",
            Name = "Mrs. Patterson",
            Type = NPCType.Citizen,
            Faction = Faction.None,
            Description = "Watches from her window. Always waves. Has been waving for a very long time.",
            DefaultDialogId = "patterson_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "quiet_buffer_discovered", "patterson_warning" }
            },
            PlaceholderColor = Color.Lavender,
            RequiresFlag = "reached_quiet"
        });

        // ============================================
        // THE TEETH - Military zone NPCs
        // ============================================

        // Armory Sergeant
        Register(new NPCDefinition
        {
            Id = "teeth_sergeant",
            Name = "Sergeant",
            Type = NPCType.Merchant,
            Faction = Faction.Machinists,
            Description = "A battle-scarred veteran who runs the armory. Doesn't talk about what happened to the rest of the unit.",
            DefaultDialogId = "sergeant_greeting",
            ShopId = "teeth_armory",
            SettlementId = "teeth_outpost",
            PlaceholderColor = Color.OliveDrab,
            RequiresFlag = "reached_teeth"
        });

        // Field Medic
        Register(new NPCDefinition
        {
            Id = "teeth_medic",
            Name = "Doc",
            Type = NPCType.Healer,
            Faction = Faction.Salvagers,
            Description = "Field medic. Patches wounds without asking questions. Has seen too much to ask anymore.",
            DefaultDialogId = "doc_greeting",
            ShopId = "teeth_medic",
            SettlementId = "teeth_outpost",
            PlaceholderColor = Color.White,
            RequiresFlag = "reached_teeth"
        });

        // War Veteran (Quest Giver)
        Register(new NPCDefinition
        {
            Id = "teeth_veteran",
            Name = "Ghost",
            Type = NPCType.QuestGiver,
            Faction = Faction.None,
            Description = "The last survivor of something called 'Operation Absolute.' Won't say what it was.",
            DefaultDialogId = "ghost_veteran_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "teeth_veteran_trust", "ghost_veteran_mission" }
            },
            QuestIds = new List<string> { "side_teeth_last_mission" },
            SettlementId = "teeth_outpost",
            PlaceholderColor = Color.DarkGray,
            RequiresFlag = "reached_teeth"
        });

        // Weapons Crafter
        Register(new NPCDefinition
        {
            Id = "teeth_crafter",
            Name = "Forge",
            Type = NPCType.Crafter,
            Faction = Faction.Machinists,
            Description = "Crafts weapons from battlefield salvage. Each piece tells a story he'd rather forget.",
            DefaultDialogId = "forge_greeting",
            SettlementId = "teeth_outpost",
            PlaceholderColor = Color.DarkOrange,
            RequiresFlag = "reached_teeth"
        });

        // ============================================
        // THE GLOW - Data center NPCs
        // ============================================

        // Data Exchange Broker
        Register(new NPCDefinition
        {
            Id = "glow_cipher",
            Name = "Cipher",
            Type = NPCType.Merchant,
            Faction = Faction.Machinists,
            Description = "Information broker. Knows things that shouldn't be knowable. Sells them anyway.",
            DefaultDialogId = "cipher_greeting",
            ShopId = "glow_data_exchange",
            SettlementId = "glow_hub",
            PlaceholderColor = Color.Cyan,
            RequiresFlag = "reached_glow"
        });

        // System Administrator
        Register(new NPCDefinition
        {
            Id = "glow_admin",
            Name = "Root",
            Type = NPCType.Merchant,
            Faction = Faction.Lazarus,
            Description = "Has admin access to Lazarus's outer systems. Uses it to help travelers... for a price.",
            DefaultDialogId = "root_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "nimdok_core_reached", "root_knows" }
            },
            ShopId = "glow_admin_shop",
            SettlementId = "glow_hub",
            PlaceholderColor = Color.Yellow,
            RequiresFlag = "reached_glow"
        });

        // Lazarus Interface (Story Critical)
        Register(new NPCDefinition
        {
            Id = "glow_nimdok_interface",
            Name = "Lazarus Interface",
            Type = NPCType.QuestGiver,
            Faction = Faction.Lazarus,
            Description = "A direct connection to Lazarus. It watches you. It always watches.",
            DefaultDialogId = "nimdok_glow_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "act3_started", "nimdok_final_greeting" }
            },
            QuestIds = new List<string> { "main_26_nimdok_choice" },
            PlaceholderColor = Color.Magenta,
            IsEssential = true,
            RequiresFlag = "reached_glow"
        });

        // Data Ghost (Mysterious NPC)
        Register(new NPCDefinition
        {
            Id = "glow_data_ghost",
            Name = "Echo-7",
            Type = NPCType.Citizen,
            Faction = Faction.None,
            Description = "A fragmented consciousness that exists only in the data streams. Was human once. Maybe.",
            DefaultDialogId = "echo7_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "dead_channel_complete", "echo7_remembers" }
            },
            PlaceholderColor = Color.LightCyan,
            RequiresFlag = "reached_glow"
        });

        // ============================================
        // ARCHIVE SCAR - End-game NPCs
        // ============================================

        // Remnant Dealer
        Register(new NPCDefinition
        {
            Id = "archive_fragment",
            Name = "Fragment",
            Type = NPCType.Merchant,
            Faction = Faction.None,
            Description = "Sells pieces of what was lost. Doesn't remember what most of it was.",
            DefaultDialogId = "fragment_greeting",
            ShopId = "archive_remnant_dealer",
            SettlementId = "archive_outpost",
            PlaceholderColor = Color.DarkGray,
            RequiresFlag = "reached_archive"
        });

        // The Original Instance
        Register(new NPCDefinition
        {
            Id = "archive_original",
            Name = "Original",
            Type = NPCType.Merchant,
            Faction = Faction.None,
            Description = "Claims to be the first backup ever made. Sells authentic pre-collapse data.",
            DefaultDialogId = "original_greeting",
            ShopId = "archive_last_backup",
            SettlementId = "archive_outpost",
            PlaceholderColor = Color.Gold,
            RequiresFlag = "reached_archive"
        });

        // Memory Keeper (Lore NPC)
        Register(new NPCDefinition
        {
            Id = "archive_keeper",
            Name = "The Keeper",
            Type = NPCType.QuestGiver,
            Faction = Faction.Shepherds,
            Description = "Guards the last complete memories of what came before. Will share them... if you're worthy.",
            DefaultDialogId = "keeper_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "archive_worthy", "keeper_shares" }
            },
            QuestIds = new List<string> { "side_archive_memories" },
            SettlementId = "archive_outpost",
            PlaceholderColor = Color.Silver,
            IsEssential = true,
            RequiresFlag = "reached_archive"
        });

        // Void Walker (Mysterious Guide)
        Register(new NPCDefinition
        {
            Id = "archive_void_walker",
            Name = "Null",
            Type = NPCType.Wanderer,
            Faction = Faction.None,
            Description = "Walks through the deleted spaces. Knows paths that don't exist anymore.",
            DefaultDialogId = "null_greeting",
            ConditionalDialogs = new Dictionary<string, string>
            {
                { "ancient_boss_defeated", "null_respects" }
            },
            PlaceholderColor = Color.Black,
            RequiresFlag = "reached_archive"
        });
    }

    private static void Register(NPCDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }
}
