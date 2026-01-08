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
    /// NIMDOK's systems and followers.
    /// </summary>
    NIMDOK,

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

        // NIMDOK Terminal - an AI interface
        Register(new NPCDefinition
        {
            Id = "nimdok_terminal",
            Name = "NIMDOK Terminal",
            Type = NPCType.QuestGiver,
            Faction = Faction.NIMDOK,
            Description = "A flickering holographic interface connected to NIMDOK.",
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
            Name = "Rusty",
            Type = NPCType.Merchant,
            Faction = Faction.Salvagers,
            Description = "A gruff trader dealing in scavenged parts and augmentations.",
            DefaultDialogId = "rusty_greeting",
            ShopItems = new List<string> { "basic_augment", "repair_kit", "stim_pack" },
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
            Type = NPCType.Crafter,
            Faction = Faction.Machinists,
            Description = "An eccentric tinkerer who can modify augmentations.",
            DefaultDialogId = "volt_greeting",
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
    }

    private static void Register(NPCDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }
}
