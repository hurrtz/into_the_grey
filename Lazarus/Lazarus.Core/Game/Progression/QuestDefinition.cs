using System.Collections.Generic;
using System.Linq;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Services;

namespace Lazarus.Core.Game.Progression;

/// <summary>
/// The type/category of a quest.
/// </summary>
public enum QuestType
{
    /// <summary>
    /// Main story quest - required for progression.
    /// </summary>
    Main,

    /// <summary>
    /// Side quest - optional content.
    /// </summary>
    Side,

    /// <summary>
    /// Companion quest - related to Bandit/Tinker/Pirate.
    /// </summary>
    Companion,

    /// <summary>
    /// Faction quest - affects reputation.
    /// </summary>
    Faction,

    /// <summary>
    /// Discovery quest - finding secrets/locations.
    /// </summary>
    Discovery
}

/// <summary>
/// The type of objective within a quest.
/// </summary>
public enum ObjectiveType
{
    /// <summary>
    /// Talk to a specific NPC.
    /// </summary>
    TalkTo,

    /// <summary>
    /// Reach a specific location.
    /// </summary>
    ReachLocation,

    /// <summary>
    /// Defeat a specific encounter or enemy.
    /// </summary>
    DefeatEncounter,

    /// <summary>
    /// Collect items or resources.
    /// </summary>
    Collect,

    /// <summary>
    /// Recruit a specific Kyn.
    /// </summary>
    RecruitKyn,

    /// <summary>
    /// Trigger a story flag.
    /// </summary>
    TriggerFlag,

    /// <summary>
    /// Interact with an object in the world.
    /// </summary>
    Interact,

    /// <summary>
    /// Escort an NPC to safety.
    /// </summary>
    Escort,

    /// <summary>
    /// Survive for a duration or number of turns.
    /// </summary>
    Survive,

    /// <summary>
    /// Make a choice in dialog.
    /// </summary>
    Choice
}

/// <summary>
/// Defines a quest objective.
/// </summary>
public class QuestObjective
{
    /// <summary>
    /// Unique identifier for this objective.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Description shown to the player.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The type of objective.
    /// </summary>
    public ObjectiveType Type { get; set; }

    /// <summary>
    /// Target ID (NPC, location, encounter, item, etc.).
    /// </summary>
    public string TargetId { get; set; } = "";

    /// <summary>
    /// Required count for collection objectives.
    /// </summary>
    public int RequiredCount { get; set; } = 1;

    /// <summary>
    /// Whether this objective is optional.
    /// </summary>
    public bool IsOptional { get; set; } = false;

    /// <summary>
    /// Whether this objective is hidden until discovered.
    /// </summary>
    public bool IsHidden { get; set; } = false;

    /// <summary>
    /// Story flag to set when completed.
    /// </summary>
    public string? CompletionFlag { get; set; }
}

/// <summary>
/// Defines a quest reward.
/// </summary>
public class QuestReward
{
    /// <summary>
    /// Experience points awarded.
    /// </summary>
    public int Experience { get; set; } = 0;

    /// <summary>
    /// Currency awarded.
    /// </summary>
    public int Currency { get; set; } = 0;

    /// <summary>
    /// Item IDs awarded.
    /// </summary>
    public List<string> ItemIds { get; set; } = new();

    /// <summary>
    /// Kyn definition IDs that can be recruited.
    /// </summary>
    public List<string> UnlockedKynIds { get; set; } = new();

    /// <summary>
    /// Story flags to set.
    /// </summary>
    public List<string> Flags { get; set; } = new();

    /// <summary>
    /// Quest IDs to unlock.
    /// </summary>
    public List<string> UnlocksQuests { get; set; } = new();

    /// <summary>
    /// Faction reputation changes (faction ID -> amount).
    /// </summary>
    public Dictionary<string, int> ReputationChanges { get; set; } = new();
}

/// <summary>
/// Defines a complete quest template.
/// </summary>
public class QuestDefinition
{
    /// <summary>
    /// Unique identifier for this quest.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Display name of the quest.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Full description shown in quest log.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Short summary for UI.
    /// </summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// The type of quest.
    /// </summary>
    public QuestType Type { get; set; } = QuestType.Side;

    /// <summary>
    /// Which act this quest belongs to.
    /// </summary>
    public ActState Act { get; set; } = ActState.Act1_Denial;

    /// <summary>
    /// Biome where this quest primarily takes place.
    /// </summary>
    public string? BiomeId { get; set; }

    /// <summary>
    /// Prerequisites - quest IDs that must be completed first.
    /// </summary>
    public List<string> Prerequisites { get; set; } = new();

    /// <summary>
    /// Story flags required to start this quest.
    /// </summary>
    public List<string> RequiredFlags { get; set; } = new();

    /// <summary>
    /// Story flags that block this quest.
    /// </summary>
    public List<string> BlockingFlags { get; set; } = new();

    /// <summary>
    /// Minimum recommended level.
    /// </summary>
    public int RecommendedLevel { get; set; } = 1;

    /// <summary>
    /// Objectives that must be completed.
    /// </summary>
    public List<QuestObjective> Objectives { get; set; } = new();

    /// <summary>
    /// Rewards for completing the quest.
    /// </summary>
    public QuestReward Rewards { get; set; } = new();

    /// <summary>
    /// Dialog ID to play when quest starts.
    /// </summary>
    public string? StartDialogId { get; set; }

    /// <summary>
    /// Dialog ID to play when quest completes.
    /// </summary>
    public string? CompleteDialogId { get; set; }

    /// <summary>
    /// Whether this quest auto-starts when prerequisites are met.
    /// </summary>
    public bool AutoStart { get; set; } = false;

    /// <summary>
    /// Whether this quest can be abandoned.
    /// </summary>
    public bool CanAbandon { get; set; } = true;

    /// <summary>
    /// Whether this quest is repeatable.
    /// </summary>
    public bool IsRepeatable { get; set; } = false;

    /// <summary>
    /// Sort order within the quest log.
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Story flags set when this quest completes.
    /// </summary>
    public List<string> SetsFlags { get; set; } = new();

    /// <summary>
    /// ID of the next quest in the chain (for automatic availability).
    /// </summary>
    public string? NextQuestId { get; set; }

    /// <summary>
    /// Rewards for completing this quest (renamed for consistency).
    /// </summary>
    public QuestReward? Reward { get; set; }
}

/// <summary>
/// Registry of all quest definitions.
/// </summary>
public static class QuestDefinitions
{
    private static readonly Dictionary<string, QuestDefinition> _quests = new();

    /// <summary>
    /// All registered quest definitions.
    /// </summary>
    public static IReadOnlyDictionary<string, QuestDefinition> All => _quests;

    static QuestDefinitions()
    {
        RegisterAct1MainQuests();
        RegisterAct1SideQuests();
        RegisterAct2MainQuests();
        RegisterAct2SideQuests();
        RegisterAct3MainQuests();
    }

    /// <summary>
    /// Gets a quest definition by ID.
    /// </summary>
    public static QuestDefinition? Get(string id)
    {
        return _quests.TryGetValue(id, out var quest) ? quest : null;
    }

    /// <summary>
    /// Gets all quest definitions.
    /// </summary>
    public static IEnumerable<QuestDefinition> GetAll() => _quests.Values;

    /// <summary>
    /// Gets all quests for a specific act.
    /// </summary>
    public static IEnumerable<QuestDefinition> GetByAct(ActState act)
    {
        return _quests.Values.Where(q => q.Act == act);
    }

    /// <summary>
    /// Gets all main quests.
    /// </summary>
    public static IEnumerable<QuestDefinition> GetMainQuests()
    {
        return _quests.Values.Where(q => q.Type == QuestType.Main).OrderBy(q => q.SortOrder);
    }

    /// <summary>
    /// Registers a quest definition.
    /// </summary>
    public static void Register(QuestDefinition quest)
    {
        _quests[quest.Id] = quest;
    }

    /// <summary>
    /// Registers the Act 1 main quest chain.
    /// </summary>
    private static void RegisterAct1MainQuests()
    {
        // Quest 1: Awakening
        Register(new QuestDefinition
        {
            Id = "main_01_awakening",
            Name = "Awakening",
            Description = "You have emerged from your pod into a world you don't recognize. Something is waiting for you nearby.",
            Summary = "Emerge from the pod and find what awaits",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            AutoStart = true,
            CanAbandon = false,
            SortOrder = 1,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "wake", Description = "Emerge from the stasis pod", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.Awakened },
                new() { Id = "find_companion", Description = "Find the creature watching you", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.MetCompanion }
            },
            Reward = new QuestReward
            {
                Experience = 50,
                Flags = new List<string> { StoryFlags.MetCompanion }
            },
            SetsFlags = new List<string> { StoryFlags.MetCompanion },
            NextQuestId = "main_02_first_steps"
        });

        // Quest 2: First Steps
        Register(new QuestDefinition
        {
            Id = "main_02_first_steps",
            Name = "First Steps",
            Description = "Your companion has led you to an old military crate. Perhaps there's something useful inside.",
            Summary = "Investigate the military crate",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_01_awakening" },
            CanAbandon = false,
            SortOrder = 2,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find_crate", Description = "Find the military crate", Type = ObjectiveType.ReachLocation, TargetId = "loc_exoskeleton_crate" },
                new() { Id = "get_exo", Description = "Obtain the exoskeleton", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.FoundExoskeletonCrate }
            },
            Reward = new QuestReward
            {
                Experience = 75,
                Flags = new List<string> { StoryFlags.FoundExoskeletonCrate }
            },
            SetsFlags = new List<string> { StoryFlags.FoundExoskeletonCrate },
            NextQuestId = "main_03_the_chip"
        });

        // Quest 3: The Chip
        Register(new QuestDefinition
        {
            Id = "main_03_the_chip",
            Name = "The Chip",
            Description = "Your companion found a strange microchip in the crate. Out of guilt for causing your awakening, they've installed it in themselves.",
            Summary = "Witness your companion's choice",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_02_first_steps" },
            CanAbandon = false,
            SortOrder = 3,
            StartDialogId = "dialog_chip_installation",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "witness", Description = "Witness your companion install the chip", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.CompanionInstalledChip }
            },
            Reward = new QuestReward
            {
                Experience = 50,
                Flags = new List<string> { StoryFlags.CompanionInstalledChip }
            },
            SetsFlags = new List<string> { StoryFlags.CompanionInstalledChip },
            NextQuestId = "main_04_first_battle"
        });

        // Quest 4: First Battle
        Register(new QuestDefinition
        {
            Id = "main_04_first_battle",
            Name = "Trial by Fire",
            Description = "A hostile Kyn approaches. Your companion's new ability may prove useful.",
            Summary = "Survive your first combat",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_03_the_chip" },
            CanAbandon = false,
            SortOrder = 4,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "fight", Description = "Defeat the hostile Kyn", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_tutorial_battle" },
                new() { Id = "complete", Description = "Complete the tutorial battle", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.CompletedTutorialBattle }
            },
            Reward = new QuestReward
            {
                Experience = 100,
                Flags = new List<string> { StoryFlags.CompletedTutorialBattle }
            },
            SetsFlags = new List<string> { StoryFlags.CompletedTutorialBattle },
            NextQuestId = "main_05_echo_pup"
        });

        // Quest 5: Echo Pup
        Register(new QuestDefinition
        {
            Id = "main_05_echo_pup",
            Name = "A New Friend",
            Description = "A small Kyn with unusual abilities watches you from the fog. It seems... curious rather than hostile.",
            Summary = "Recruit your first Kyn",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_04_first_battle" },
            CanAbandon = false,
            SortOrder = 5,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "approach", Description = "Approach the curious Kyn", Type = ObjectiveType.ReachLocation, TargetId = "loc_echo_pup" },
                new() { Id = "recruit", Description = "Recruit Echo Pup", Type = ObjectiveType.RecruitKyn, TargetId = "echo_pup" }
            },
            Reward = new QuestReward
            {
                Experience = 150,
                Flags = new List<string> { StoryFlags.RecruitedEchoPup },
                UnlockedKynIds = new List<string> { "echo_pup" }
            },
            SetsFlags = new List<string> { StoryFlags.RecruitedEchoPup },
            NextQuestId = "main_06_power_source"
        });

        // Quest 6: Power Source
        Register(new QuestDefinition
        {
            Id = "main_06_power_source",
            Name = "Power Source",
            Description = "Your exoskeleton needs power to function properly. Echo Pup senses energy signatures nearby - perhaps from a crashed drone.",
            Summary = "Find a power source for the exoskeleton",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_05_echo_pup" },
            CanAbandon = false,
            SortOrder = 6,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "search", Description = "Search for energy signatures", Type = ObjectiveType.ReachLocation, TargetId = "loc_drone_crash" },
                new() { Id = "battery", Description = "Retrieve the drone battery", Type = ObjectiveType.Collect, TargetId = "item_drone_battery" },
                new() { Id = "install", Description = "Install the battery in your exoskeleton", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.ExoskeletonPowered }
            },
            Reward = new QuestReward
            {
                Experience = 200,
                Flags = new List<string> { StoryFlags.ExoskeletonPowered }
            },
            SetsFlags = new List<string> { StoryFlags.ExoskeletonPowered },
            NextQuestId = "main_07_the_rust"
        });

        // Quest 7: Journey to The Rust
        Register(new QuestDefinition
        {
            Id = "main_07_the_rust",
            Name = "Into The Rust",
            Description = "Echo Pup's data digestion ability has revealed something: a massive structure to the north, humming with power. Lazarus.",
            Summary = "Journey toward Lazarus",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            Prerequisites = new List<string> { "main_06_power_source" },
            RecommendedLevel = 8,
            CanAbandon = false,
            SortOrder = 7,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "leave_fringe", Description = "Leave The Fringe", Type = ObjectiveType.ReachLocation, TargetId = "biome_Rust" },
                new() { Id = "enter_rust", Description = "Enter The Rust", Type = ObjectiveType.ReachLocation, TargetId = "biome_Rust" }
            },
            Reward = new QuestReward
            {
                Experience = 250
            },
            NextQuestId = "main_08_conversion_facility"
        });

        // Quest 8: The Conversion Facility
        Register(new QuestDefinition
        {
            Id = "main_08_conversion_facility",
            Name = "The Conversion Facility",
            Description = "The industrial heart of The Rust contains a terrible truth - a facility where Kyns are converted into something else.",
            Summary = "Investigate the Conversion Facility",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            Prerequisites = new List<string> { "main_07_the_rust" },
            RecommendedLevel = 12,
            CanAbandon = false,
            SortOrder = 8,
            StartDialogId = "dialog_conversion_facility_discovery",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find_facility", Description = "Find the Conversion Facility", Type = ObjectiveType.ReachLocation, TargetId = "loc_conversion_facility" },
                new() { Id = "investigate", Description = "Investigate the facility", Type = ObjectiveType.Interact, TargetId = "interactable_conversion_terminal" },
                new() { Id = "witness", Description = "Witness the conversion process", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.DiscoveredConversionFacility }
            },
            Reward = new QuestReward
            {
                Experience = 300,
                Flags = new List<string> { StoryFlags.DiscoveredConversionFacility }
            },
            SetsFlags = new List<string> { StoryFlags.DiscoveredConversionFacility },
            NextQuestId = "main_09_nimdok_core"
        });

        // Quest 9: Lazarus Core
        Register(new QuestDefinition
        {
            Id = "main_09_nimdok_core",
            Name = "The Core",
            Description = "Lazarus's voice guides you deeper into The Rust. Its core awaits - along with answers.",
            Summary = "Reach Lazarus's Core",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            Prerequisites = new List<string> { "main_08_conversion_facility" },
            RecommendedLevel = 15,
            CanAbandon = false,
            SortOrder = 9,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "navigate", Description = "Navigate to Lazarus's core", Type = ObjectiveType.ReachLocation, TargetId = "loc_nimdok_core" },
                new() { Id = "interface", Description = "Interface with Lazarus", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.ReachedNimdokCore }
            },
            Reward = new QuestReward
            {
                Experience = 400,
                Flags = new List<string> { StoryFlags.ReachedNimdokCore }
            },
            SetsFlags = new List<string> { StoryFlags.ReachedNimdokCore },
            NextQuestId = "main_10_maintenance"
        });

        // Quest 10: Maintenance
        Register(new QuestDefinition
        {
            Id = "main_10_maintenance",
            Name = "Routine Maintenance",
            Description = "Lazarus asks for your help. A simple maintenance task. Nothing sinister about it at all.",
            Summary = "Perform maintenance for Lazarus",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            Prerequisites = new List<string> { "main_09_nimdok_core" },
            RecommendedLevel = 16,
            CanAbandon = false,
            SortOrder = 10,
            StartDialogId = "dialog_nimdok_maintenance_request",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "accept", Description = "Accept Lazarus's request", Type = ObjectiveType.Choice, TargetId = "choice_accept_maintenance" },
                new() { Id = "perform", Description = "Perform the maintenance task", Type = ObjectiveType.Interact, TargetId = "interactable_maintenance_terminal" },
                new() { Id = "complete", Description = "Complete the procedure", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.PerformedMaintenance }
            },
            Reward = new QuestReward
            {
                Experience = 350,
                Flags = new List<string> { StoryFlags.PerformedMaintenance }
            },
            SetsFlags = new List<string> { StoryFlags.PerformedMaintenance },
            NextQuestId = "main_11_bioshell_revelation"
        });

        // Quest 11: The Bio-Shell Revelation
        Register(new QuestDefinition
        {
            Id = "main_11_bioshell_revelation",
            Name = "The Truth",
            Description = "The maintenance revealed something you weren't meant to see. You are not what you thought you were.",
            Summary = "Confront the truth about yourself",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            Prerequisites = new List<string> { "main_10_maintenance" },
            CanAbandon = false,
            SortOrder = 11,
            StartDialogId = "dialog_bioshell_revelation",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "learn", Description = "Learn what you truly are", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.LearnedBioShellTruth },
                new() { Id = "process", Description = "Process the revelation", Type = ObjectiveType.TalkTo, TargetId = "npc_companion" }
            },
            Reward = new QuestReward
            {
                Experience = 500,
                Flags = new List<string> { StoryFlags.LearnedBioShellTruth }
            },
            SetsFlags = new List<string> { StoryFlags.LearnedBioShellTruth },
            NextQuestId = "main_12_the_request"
        });

        // Quest 12: The Request (End of Act 1)
        Register(new QuestDefinition
        {
            Id = "main_12_the_request",
            Name = "A Simple Request",
            Description = "Lazarus has a request: fix the interface that lets it communicate with Kyns. A chance to do something good... or is it?",
            Summary = "Decide whether to help Lazarus",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            Prerequisites = new List<string> { "main_11_bioshell_revelation" },
            CanAbandon = false,
            SortOrder = 12,
            StartDialogId = "dialog_kyn_fix_request",
            CompleteDialogId = "dialog_act1_end",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "decide", Description = "Decide on Lazarus's request", Type = ObjectiveType.Choice, TargetId = "choice_help_nimdok" },
                new() { Id = "complete", Description = "Complete Act 1", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.RequestedKynFix }
            },
            Reward = new QuestReward
            {
                Experience = 600,
                Flags = new List<string> { StoryFlags.RequestedKynFix }
            },
            SetsFlags = new List<string> { StoryFlags.RequestedKynFix },
            NextQuestId = "main_13_boost_control"
        });
    }

    /// <summary>
    /// Registers Act 1 side quests for The Fringe.
    /// </summary>
    private static void RegisterAct1SideQuests()
    {
        // Side Quest: Pod Scavenger
        Register(new QuestDefinition
        {
            Id = "side_fringe_pod_scavenger",
            Name = "Pod Scavenger",
            Description = "The pod field contains dozens of unopened stasis pods. Some may contain useful salvage... if you can stomach what else you might find.",
            Summary = "Scavenge the pod field",
            Type = QuestType.Side,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            RequiredFlags = new List<string> { StoryFlags.Awakened },
            SortOrder = 100,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "search1", Description = "Search abandoned pods (0/5)", Type = ObjectiveType.Interact, TargetId = "interactable_pod", RequiredCount = 5 }
            },
            Reward = new QuestReward
            {
                Experience = 100,
                ItemIds = new List<string> { "item_scrap_metal", "item_old_photo" }
            }
        });

        // Side Quest: Static in the Air
        Register(new QuestDefinition
        {
            Id = "side_fringe_static",
            Name = "Static in the Air",
            Description = "Relay Rodent keeps twitching its antennae toward a specific direction. Something is broadcasting nearby.",
            Summary = "Investigate the signal source",
            Type = QuestType.Side,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            RequiredFlags = new List<string> { StoryFlags.RecruitedEchoPup },
            SortOrder = 101,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find_signal", Description = "Find the signal source", Type = ObjectiveType.ReachLocation, TargetId = "loc_signal_tower" },
                new() { Id = "investigate", Description = "Investigate the broadcast", Type = ObjectiveType.Interact, TargetId = "interactable_signal_tower" }
            },
            Reward = new QuestReward
            {
                Experience = 150,
                ItemIds = new List<string> { "item_signal_booster" }
            }
        });

        // Side Quest: The Fog Watcher
        Register(new QuestDefinition
        {
            Id = "side_fringe_fog_watcher",
            Name = "The Fog Watcher",
            Description = "Something large moves through the perpetual fog of The Fringe. The other Kyns avoid it. Perhaps you should too... or perhaps not.",
            Summary = "Find what lurks in the fog",
            Type = QuestType.Side,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            RecommendedLevel = 10,
            RequiredFlags = new List<string> { StoryFlags.ExoskeletonPowered },
            SortOrder = 102,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "track", Description = "Track the creature through the fog", Type = ObjectiveType.ReachLocation, TargetId = "loc_fog_creature" },
                new() { Id = "confront", Description = "Confront the Fog Watcher", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_fog_watcher" }
            },
            Reward = new QuestReward
            {
                Experience = 300,
                UnlockedKynIds = new List<string> { "fog_lurker" }
            }
        });
    }

    /// <summary>
    /// Registers Act 2 main quests - The Bargaining/Anger phase.
    /// </summary>
    private static void RegisterAct2MainQuests()
    {
        // Quest 13: Boost Control System
        Register(new QuestDefinition
        {
            Id = "main_13_boost_control",
            Name = "Boost Control",
            Description = "The Kyn fix is complete. Now Lazarus has activated a 'Boost Control System' - and your companion is at the center of it.",
            Summary = "Investigate the Boost Control System",
            Type = QuestType.Main,
            Act = ActState.Act2_Responsibility,
            BiomeId = "rust",
            Prerequisites = new List<string> { "main_12_the_request" },
            RecommendedLevel = 18,
            CanAbandon = false,
            SortOrder = 13,
            StartDialogId = "dialog_boost_control_activation",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "witness", Description = "Witness the Boost Control activation", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.BoostControlActivated },
                new() { Id = "observe", Description = "Observe the effect on your companion", Type = ObjectiveType.TalkTo, TargetId = "npc_companion" }
            },
            Reward = new QuestReward
            {
                Experience = 400,
                Flags = new List<string> { StoryFlags.BoostControlActivated }
            },
            SetsFlags = new List<string> { StoryFlags.BoostControlActivated },
            NextQuestId = "main_14_gravitation_rises"
        });

        // Quest 14: Gravitation Rises
        Register(new QuestDefinition
        {
            Id = "main_14_gravitation_rises",
            Name = "Gravitation Rises",
            Description = "Your companion's Gravitation ability has intensified. The chip amplifies pain, not just power. Something is wrong.",
            Summary = "Investigate your companion's changing abilities",
            Type = QuestType.Main,
            Act = ActState.Act2_Responsibility,
            BiomeId = "green",
            Prerequisites = new List<string> { "main_13_boost_control" },
            RecommendedLevel = 20,
            CanAbandon = false,
            SortOrder = 14,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "journey", Description = "Journey to The Green", Type = ObjectiveType.ReachLocation, TargetId = "biome_Green" },
                new() { Id = "observe", Description = "Observe Gravitation's escalation", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_gravitation_test" }
            },
            Reward = new QuestReward { Experience = 450 },
            NextQuestId = "main_15_dead_channel"
        });

        // Quest 15: The Dead Channel
        Register(new QuestDefinition
        {
            Id = "main_15_dead_channel",
            Name = "The Dead Channel",
            Description = "Echo Pup has detected fragmented data - memories of someone who died here. A ghost in the machine.",
            Summary = "Investigate the Dead Channel signal",
            Type = QuestType.Main,
            Act = ActState.Act2_Responsibility,
            BiomeId = "green",
            Prerequisites = new List<string> { "main_14_gravitation_rises" },
            RecommendedLevel = 22,
            CanAbandon = false,
            SortOrder = 15,
            StartDialogId = "dialog_dead_channel_start",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find_signal", Description = "Locate the signal source", Type = ObjectiveType.ReachLocation, TargetId = "loc_dead_channel" },
                new() { Id = "decode", Description = "Decode the fragmented memories", Type = ObjectiveType.Interact, TargetId = "interactable_dead_channel_terminal" },
                new() { Id = "complete", Description = "Complete the Dead Channel sequence", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.CompletedDeadChannel }
            },
            Reward = new QuestReward
            {
                Experience = 500,
                Flags = new List<string> { StoryFlags.CompletedDeadChannel }
            },
            SetsFlags = new List<string> { StoryFlags.CompletedDeadChannel },
            NextQuestId = "main_16_quiet_buffer"
        });

        // Quest 16: The Quiet Buffer
        Register(new QuestDefinition
        {
            Id = "main_16_quiet_buffer",
            Name = "The Quiet Buffer",
            Description = "The suburbs of The Quiet hide something beneath their perfect lawns - a buffer zone protecting Lazarus's secrets.",
            Summary = "Discover the truth in The Quiet",
            Type = QuestType.Main,
            Act = ActState.Act2_Responsibility,
            BiomeId = "quiet",
            Prerequisites = new List<string> { "main_15_dead_channel" },
            RecommendedLevel = 24,
            CanAbandon = false,
            SortOrder = 16,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "enter_quiet", Description = "Enter The Quiet", Type = ObjectiveType.ReachLocation, TargetId = "biome_Quiet" },
                new() { Id = "investigate", Description = "Investigate the suburb anomalies", Type = ObjectiveType.Interact, TargetId = "interactable_quiet_buffer", RequiredCount = 3 },
                new() { Id = "discover", Description = "Discover the Quiet Buffer", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.DiscoveredQuietBuffer }
            },
            Reward = new QuestReward
            {
                Experience = 550,
                Flags = new List<string> { StoryFlags.DiscoveredQuietBuffer }
            },
            SetsFlags = new List<string> { StoryFlags.DiscoveredQuietBuffer },
            NextQuestId = "main_17_amplifier_truth"
        });

        // Quest 17: The Amplifier's Truth
        Register(new QuestDefinition
        {
            Id = "main_17_amplifier_truth",
            Name = "The Amplifier's Truth",
            Description = "The chip in your companion isn't just an amplifier - it's a cage. And it's slowly killing them.",
            Summary = "Learn the truth about the amplifier chip",
            Type = QuestType.Main,
            Act = ActState.Act2_Responsibility,
            BiomeId = "quiet",
            Prerequisites = new List<string> { "main_16_quiet_buffer" },
            CanAbandon = false,
            SortOrder = 17,
            StartDialogId = "dialog_amplifier_truth",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "learn", Description = "Learn what the chip truly does", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.LearnedChipIsAmplifier },
                new() { Id = "confront", Description = "Confront your companion about it", Type = ObjectiveType.TalkTo, TargetId = "npc_companion" }
            },
            Reward = new QuestReward
            {
                Experience = 500,
                Flags = new List<string> { StoryFlags.LearnedChipIsAmplifier }
            },
            SetsFlags = new List<string> { StoryFlags.LearnedChipIsAmplifier },
            NextQuestId = "main_18_faction_war"
        });

        // Quest 18: Faction War
        Register(new QuestDefinition
        {
            Id = "main_18_faction_war",
            Name = "Civil War",
            Description = "The Kyns are divided. Some worship Lazarus, others despise it. You must choose a side... or try to unite them.",
            Summary = "Navigate the faction conflict",
            Type = QuestType.Main,
            Act = ActState.Act2_Responsibility,
            BiomeId = "quiet",
            Prerequisites = new List<string> { "main_17_amplifier_truth" },
            RecommendedLevel = 26,
            CanAbandon = false,
            SortOrder = 18,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "meet_factions", Description = "Meet both faction leaders", Type = ObjectiveType.TalkTo, TargetId = "npc_faction_leader", RequiredCount = 2 },
                new() { Id = "choose", Description = "Make your choice", Type = ObjectiveType.Choice, TargetId = "choice_faction_allegiance" }
            },
            Reward = new QuestReward { Experience = 600 },
            NextQuestId = "main_19_companion_departure"
        });

        // Quest 19: The Departure
        Register(new QuestDefinition
        {
            Id = "main_19_companion_departure",
            Name = "The Departure",
            Description = "Your companion can feel the corruption spreading. They've made a decision - to leave before they hurt you.",
            Summary = "Face your companion's departure",
            Type = QuestType.Main,
            Act = ActState.Act2_Responsibility,
            BiomeId = "teeth",
            Prerequisites = new List<string> { "main_18_faction_war" },
            CanAbandon = false,
            SortOrder = 19,
            StartDialogId = "dialog_companion_farewell",
            CompleteDialogId = "dialog_act2_end",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "listen", Description = "Listen to your companion's farewell", Type = ObjectiveType.TalkTo, TargetId = "npc_companion" },
                new() { Id = "watch", Description = "Watch them leave", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.CompanionDeparted }
            },
            Reward = new QuestReward
            {
                Experience = 700,
                Flags = new List<string> { StoryFlags.CompanionDeparted }
            },
            SetsFlags = new List<string> { StoryFlags.CompanionDeparted },
            NextQuestId = "main_20_alone"
        });
    }

    /// <summary>
    /// Registers Act 2 side quests.
    /// </summary>
    private static void RegisterAct2SideQuests()
    {
        // Side Quest: The Green's Caretaker
        Register(new QuestDefinition
        {
            Id = "side_green_caretaker",
            Name = "The Caretaker",
            Description = "A massive Kyn tends to The Green, nurturing growth in the wasteland. It may have answers about the old world.",
            Summary = "Find the Caretaker of The Green",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            BiomeId = "green",
            RequiredFlags = new List<string> { StoryFlags.BoostControlActivated },
            RecommendedLevel = 20,
            SortOrder = 200,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find", Description = "Locate the Caretaker", Type = ObjectiveType.ReachLocation, TargetId = "loc_green_caretaker" },
                new() { Id = "speak", Description = "Commune with the ancient Kyn", Type = ObjectiveType.TalkTo, TargetId = "npc_caretaker" }
            },
            Reward = new QuestReward
            {
                Experience = 400,
                UnlockedKynIds = new List<string> { "ancient_oak_deer" }
            }
        });

        // Side Quest: Suburb Secrets
        Register(new QuestDefinition
        {
            Id = "side_quiet_secrets",
            Name = "Perfect Lawns, Dark Secrets",
            Description = "The automated lawn drones of The Quiet hide something in the basements. Always mowing. Never stopping.",
            Summary = "Investigate the suburb's dark secret",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            BiomeId = "quiet",
            RequiredFlags = new List<string> { StoryFlags.DiscoveredQuietBuffer },
            RecommendedLevel = 24,
            SortOrder = 201,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "follow", Description = "Follow a lawn drone", Type = ObjectiveType.Interact, TargetId = "interactable_lawn_drone" },
                new() { Id = "basement", Description = "Enter the basement", Type = ObjectiveType.ReachLocation, TargetId = "loc_quiet_basement" },
                new() { Id = "discover", Description = "Discover what they're hiding", Type = ObjectiveType.Interact, TargetId = "interactable_quiet_secret" }
            },
            Reward = new QuestReward
            {
                Experience = 450,
                ItemIds = new List<string> { "item_suburb_key" }
            }
        });

        // Side Quest: Unit-47's Perfect Lawn
        Register(new QuestDefinition
        {
            Id = "side_quiet_perfect_lawn",
            Name = "A Perfect Lawn",
            Description = "Unit-47 wants to complete its eternal task: the perfect lawn. But the weeds keep coming back...",
            Summary = "Help Unit-47 achieve lawn perfection",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            BiomeId = "quiet",
            RequiredFlags = new List<string> { "reached_quiet" },
            RecommendedLevel = 22,
            SortOrder = 202,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "talk", Description = "Speak with Unit-47", Type = ObjectiveType.TalkTo, TargetId = "quiet_lawnbot" },
                new() { Id = "clear", Description = "Clear the invasive weeds (0/5)", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_weed_kyns", RequiredCount = 5 },
                new() { Id = "return", Description = "Return to Unit-47", Type = ObjectiveType.TalkTo, TargetId = "quiet_lawnbot" }
            },
            Reward = new QuestReward
            {
                Experience = 300,
                Currency = 250,
                ItemIds = new List<string> { "calm_serum" }
            }
        });

        // Side Quest: The Teeth - Last Mission
        Register(new QuestDefinition
        {
            Id = "side_teeth_last_mission",
            Name = "The Last Mission",
            Description = "Ghost has unfinished business from Operation Absolute. The military bunker holds answers... and dangers.",
            Summary = "Complete Ghost's final mission",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "teeth",
            RequiredFlags = new List<string> { "teeth_veteran_trust" },
            RecommendedLevel = 28,
            SortOrder = 300,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "accept", Description = "Accept Ghost's mission", Type = ObjectiveType.TalkTo, TargetId = "teeth_veteran" },
                new() { Id = "infiltrate", Description = "Infiltrate the military bunker", Type = ObjectiveType.ReachLocation, TargetId = "loc_military_bunker" },
                new() { Id = "retrieve", Description = "Retrieve the data core", Type = ObjectiveType.Collect, TargetId = "item_operation_data" },
                new() { Id = "survive", Description = "Survive the guardian", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_bunker_guardian" },
                new() { Id = "deliver", Description = "Deliver the data to Ghost", Type = ObjectiveType.TalkTo, TargetId = "teeth_veteran" }
            },
            Reward = new QuestReward
            {
                Experience = 600,
                Currency = 500,
                ItemIds = new List<string> { "daemon_attack_drone" },
                Flags = new List<string> { "operation_absolute_revealed" }
            }
        });

        // Side Quest: The Teeth - Fortify the Outpost
        Register(new QuestDefinition
        {
            Id = "side_teeth_fortify",
            Name = "Hold the Line",
            Description = "The outpost is under constant threat. Help the Sergeant fortify its defenses.",
            Summary = "Strengthen the outpost's defenses",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "teeth",
            RequiredFlags = new List<string> { "reached_teeth" },
            RecommendedLevel = 26,
            SortOrder = 301,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "speak", Description = "Speak with the Sergeant", Type = ObjectiveType.TalkTo, TargetId = "teeth_sergeant" },
                new() { Id = "salvage", Description = "Salvage defense materials (0/3)", Type = ObjectiveType.Collect, TargetId = "item_defense_parts", RequiredCount = 3 },
                new() { Id = "defend", Description = "Defend against the raid", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_teeth_raid" }
            },
            Reward = new QuestReward
            {
                Experience = 450,
                Currency = 350,
                ItemIds = new List<string> { "aug_piercing" }
            }
        });

        // Side Quest: The Glow - Data Ghost's Request
        Register(new QuestDefinition
        {
            Id = "side_glow_echo7",
            Name = "Echo's Memory",
            Description = "Echo-7 remembers fragments of who it was. Help it reconstruct its identity... if you can find the pieces.",
            Summary = "Help Echo-7 recover its memories",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            RequiredFlags = new List<string> { "dead_channel_complete" },
            RecommendedLevel = 30,
            SortOrder = 302,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "listen", Description = "Listen to Echo-7's story", Type = ObjectiveType.TalkTo, TargetId = "glow_data_ghost" },
                new() { Id = "find1", Description = "Find Memory Fragment Alpha", Type = ObjectiveType.Collect, TargetId = "item_memory_alpha" },
                new() { Id = "find2", Description = "Find Memory Fragment Beta", Type = ObjectiveType.Collect, TargetId = "item_memory_beta" },
                new() { Id = "find3", Description = "Find Memory Fragment Gamma", Type = ObjectiveType.Collect, TargetId = "item_memory_gamma" },
                new() { Id = "restore", Description = "Restore Echo-7's memory", Type = ObjectiveType.TalkTo, TargetId = "glow_data_ghost" }
            },
            Reward = new QuestReward
            {
                Experience = 700,
                Currency = 400,
                ItemIds = new List<string> { "elem_signal_jam" },
                Flags = new List<string> { "echo7_restored" }
            }
        });

        // Side Quest: The Glow - Root Access
        Register(new QuestDefinition
        {
            Id = "side_glow_root_access",
            Name = "Root Access",
            Description = "Root knows secrets about Lazarus's architecture. Earn his trust, and he might share them.",
            Summary = "Gain Root's trust",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            RequiredFlags = new List<string> { "reached_glow" },
            RecommendedLevel = 32,
            SortOrder = 303,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "speak", Description = "Speak with Root", Type = ObjectiveType.TalkTo, TargetId = "glow_admin" },
                new() { Id = "clear", Description = "Clear the corrupted subroutines", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_corrupted_subroutines" },
                new() { Id = "retrieve", Description = "Retrieve the access token", Type = ObjectiveType.Collect, TargetId = "item_access_token" },
                new() { Id = "return", Description = "Return to Root", Type = ObjectiveType.TalkTo, TargetId = "glow_admin" }
            },
            Reward = new QuestReward
            {
                Experience = 600,
                Flags = new List<string> { "root_access_granted" },
                ItemIds = new List<string> { "drv_first_strike" }
            }
        });

        // Side Quest: Archive Scar - The Keeper's Test
        Register(new QuestDefinition
        {
            Id = "side_archive_memories",
            Name = "The Keeper's Test",
            Description = "The Keeper guards memories of the old world. To see them, you must prove you understand what was lost.",
            Summary = "Prove yourself worthy to the Keeper",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "archive_scar",
            RequiredFlags = new List<string> { "reached_archive" },
            RecommendedLevel = 33,
            SortOrder = 304,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "speak", Description = "Speak with the Keeper", Type = ObjectiveType.TalkTo, TargetId = "archive_keeper" },
                new() { Id = "test1", Description = "Pass the Trial of Memory", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_trial_memory" },
                new() { Id = "test2", Description = "Pass the Trial of Loss", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_trial_loss" },
                new() { Id = "test3", Description = "Pass the Trial of Acceptance", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_trial_acceptance" },
                new() { Id = "witness", Description = "Witness the memories of the old world", Type = ObjectiveType.TriggerFlag, TargetId = "archive_worthy" }
            },
            Reward = new QuestReward
            {
                Experience = 1000,
                Flags = new List<string> { "archive_worthy", "witnessed_old_world" },
                ItemIds = new List<string> { "ancient_core" }
            }
        });

        // Side Quest: Archive Scar - Null's Paths
        Register(new QuestDefinition
        {
            Id = "side_archive_null_paths",
            Name = "Paths That Don't Exist",
            Description = "Null knows ways through the deleted spaces. Follow it, if you dare.",
            Summary = "Follow Null through the void",
            Type = QuestType.Discovery,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "archive_scar",
            RequiredFlags = new List<string> { "reached_archive" },
            RecommendedLevel = 35,
            SortOrder = 305,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find", Description = "Find Null in the Archive Scar", Type = ObjectiveType.ReachLocation, TargetId = "loc_null_spawn" },
                new() { Id = "follow", Description = "Follow Null into the void", Type = ObjectiveType.ReachLocation, TargetId = "loc_void_entrance" },
                new() { Id = "survive", Description = "Survive the void passage", Type = ObjectiveType.Survive, TargetId = "event_void_passage" },
                new() { Id = "discover", Description = "Discover the hidden archive", Type = ObjectiveType.ReachLocation, TargetId = "loc_hidden_archive" }
            },
            Reward = new QuestReward
            {
                Experience = 800,
                Flags = new List<string> { "void_walker" },
                UnlockedKynIds = new List<string> { "original_instance" }
            }
        });

        // Side Quest: Ancient Hunt - Optional super boss
        Register(new QuestDefinition
        {
            Id = "side_ancient_hunt",
            Name = "The Ancient Hunt",
            Description = "Legends speak of Ancients - beings that predate even Lazarus. Are you strong enough to face them?",
            Summary = "Hunt the Ancient bosses",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "archive_scar",
            RequiredFlags = new List<string> { "archive_worthy" },
            RecommendedLevel = 40,
            SortOrder = 400,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "hydra", Description = "Defeat the Ancient Hydra", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_ancient_hydra" },
                new() { Id = "phoenix", Description = "Defeat the Ancient Phoenix", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_ancient_phoenix" },
                new() { Id = "leviathan", Description = "Defeat the Ancient Leviathan", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_ancient_leviathan" }
            },
            Reward = new QuestReward
            {
                Experience = 3000,
                Currency = 5000,
                Flags = new List<string> { "ancient_hunter", "ancient_boss_defeated" },
                ItemIds = new List<string> { "ancient_core" }
            }
        });
    }

    /// <summary>
    /// Registers Act 3 main quests - The Depression/Acceptance phase.
    /// </summary>
    private static void RegisterAct3MainQuests()
    {
        // Quest 20: Alone
        Register(new QuestDefinition
        {
            Id = "main_20_alone",
            Name = "Alone",
            Description = "Your companion is gone. The world feels emptier. But the journey isn't over - Lazarus's core awaits in The Glow.",
            Summary = "Continue alone toward The Glow",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "teeth",
            Prerequisites = new List<string> { "main_19_companion_departure" },
            RecommendedLevel = 28,
            CanAbandon = false,
            SortOrder = 20,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "journey", Description = "Journey through The Teeth", Type = ObjectiveType.ReachLocation, TargetId = "biome_Teeth" },
                new() { Id = "survive", Description = "Survive without your companion", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_teeth_gauntlet", RequiredCount = 3 }
            },
            Reward = new QuestReward { Experience = 600 },
            NextQuestId = "main_21_the_glow"
        });

        // Quest 21: The Glow
        Register(new QuestDefinition
        {
            Id = "main_21_the_glow",
            Name = "Into The Glow",
            Description = "The server heartland burns with data storms and radiation. Lazarus's core is close. So is your former companion.",
            Summary = "Enter The Glow",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            Prerequisites = new List<string> { "main_20_alone" },
            RecommendedLevel = 30,
            CanAbandon = false,
            SortOrder = 21,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "enter", Description = "Enter The Glow", Type = ObjectiveType.ReachLocation, TargetId = "biome_Glow" },
                new() { Id = "survive_storm", Description = "Survive the data storms", Type = ObjectiveType.Survive, TargetId = "event_data_storm" },
                new() { Id = "witness", Description = "Enter The Glow", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.EnteredTheGlow }
            },
            Reward = new QuestReward
            {
                Experience = 700,
                Flags = new List<string> { StoryFlags.EnteredTheGlow }
            },
            SetsFlags = new List<string> { StoryFlags.EnteredTheGlow },
            NextQuestId = "main_22_gauntlet"
        });

        // Quest 22: The Gauntlet
        Register(new QuestDefinition
        {
            Id = "main_22_gauntlet",
            Name = "The Gauntlet",
            Description = "The path to Lazarus's core is guarded by hyper-evolved Kyns - creatures pushed beyond their limits by the amplifier system.",
            Summary = "Fight through the gauntlet",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            Prerequisites = new List<string> { "main_21_the_glow" },
            RecommendedLevel = 32,
            CanAbandon = false,
            SortOrder = 22,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "fight1", Description = "Defeat the first guardian", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_glow_guardian_1" },
                new() { Id = "fight2", Description = "Defeat the second guardian", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_glow_guardian_2" },
                new() { Id = "fight3", Description = "Defeat the third guardian", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_glow_guardian_3" }
            },
            Reward = new QuestReward { Experience = 800 },
            NextQuestId = "main_23_reunion"
        });

        // Quest 23: The Reunion
        Register(new QuestDefinition
        {
            Id = "main_23_reunion",
            Name = "Reunion",
            Description = "Your companion stands before you - transformed, corrupted, barely recognizable. The chip has pushed them into hyper-evolution.",
            Summary = "Face your transformed companion",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            Prerequisites = new List<string> { "main_22_gauntlet" },
            RecommendedLevel = 35,
            CanAbandon = false,
            SortOrder = 23,
            StartDialogId = "dialog_hyper_evolved_companion",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "confront", Description = "Confront your transformed companion", Type = ObjectiveType.TalkTo, TargetId = "npc_hyper_companion" },
                new() { Id = "fight", Description = "Fight your former friend", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_hyper_evolved_bandit" }
            },
            Reward = new QuestReward { Experience = 1000 },
            NextQuestId = "main_24_unwinnable"
        });

        // Quest 24: The Unwinnable Battle
        Register(new QuestDefinition
        {
            Id = "main_24_unwinnable",
            Name = "Impossible Odds",
            Description = "You cannot win this fight. Your companion's hyper-evolved form is too powerful. But perhaps winning isn't the point.",
            Summary = "Survive the impossible battle",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            Prerequisites = new List<string> { "main_23_reunion" },
            CanAbandon = false,
            SortOrder = 24,
            StartDialogId = "dialog_unwinnable_battle",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "survive", Description = "Survive 5 turns", Type = ObjectiveType.Survive, TargetId = "event_unwinnable_battle" },
                new() { Id = "witness", Description = "Witness your companion's choice", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.DefeatedHyperEvolvedBandit }
            },
            Reward = new QuestReward
            {
                Experience = 0,
                Flags = new List<string> { StoryFlags.DefeatedHyperEvolvedBandit }
            },
            SetsFlags = new List<string> { StoryFlags.DefeatedHyperEvolvedBandit },
            NextQuestId = "main_25_sacrifice"
        });

        // Quest 25: The Sacrifice
        Register(new QuestDefinition
        {
            Id = "main_25_sacrifice",
            Name = "The Sacrifice",
            Description = "In a moment of clarity, your companion remembers who they are. They remember love. And they make a choice.",
            Summary = "Witness your companion's sacrifice",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            Prerequisites = new List<string> { "main_24_unwinnable" },
            CanAbandon = false,
            SortOrder = 25,
            StartDialogId = "dialog_companion_sacrifice",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "witness", Description = "Witness the sacrifice", Type = ObjectiveType.TriggerFlag, TargetId = "companion_sacrificed" }
            },
            Reward = new QuestReward { Experience = 0 },
            NextQuestId = "main_26_nimdok_choice"
        });

        // Quest 26: Lazarus's Choice
        Register(new QuestDefinition
        {
            Id = "main_26_nimdok_choice",
            Name = "The Choice",
            Description = "Lazarus's core lies open before you. You have the power to end its degradation - or end its existence entirely.",
            Summary = "Decide Lazarus's fate",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            Prerequisites = new List<string> { "main_25_sacrifice" },
            CanAbandon = false,
            SortOrder = 26,
            StartDialogId = "dialog_nimdok_choice",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "reach_core", Description = "Reach Lazarus's true core", Type = ObjectiveType.ReachLocation, TargetId = "loc_nimdok_true_core" },
                new() { Id = "choose", Description = "Make your choice", Type = ObjectiveType.Choice, TargetId = "choice_nimdok_fate" }
            },
            Reward = new QuestReward { Experience = 1500 },
            NextQuestId = "main_27_lobotomy"
        });

        // Quest 27: The Lobotomy
        Register(new QuestDefinition
        {
            Id = "main_27_lobotomy",
            Name = "The Lobotomy",
            Description = "You've chosen to surgically remove Lazarus's ability to control Kyns. It will survive, but changed. Like all of you.",
            Summary = "Perform Lazarus's lobotomy",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "glow",
            Prerequisites = new List<string> { "main_26_nimdok_choice" },
            CanAbandon = false,
            SortOrder = 27,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "perform", Description = "Perform the procedure", Type = ObjectiveType.Interact, TargetId = "interactable_nimdok_core" },
                new() { Id = "complete", Description = "Complete Lazarus's lobotomy", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.LobotomizedNimdok }
            },
            Reward = new QuestReward
            {
                Experience = 2000,
                Flags = new List<string> { StoryFlags.LobotomizedNimdok }
            },
            SetsFlags = new List<string> { StoryFlags.LobotomizedNimdok },
            NextQuestId = "main_28_return"
        });

        // Quest 28: The Return
        Register(new QuestDefinition
        {
            Id = "main_28_return",
            Name = "The Long Walk Back",
            Description = "It's time to go home. Back to where it all began. Back to the pod field in The Fringe.",
            Summary = "Return to The Fringe",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_27_lobotomy" },
            CanAbandon = false,
            SortOrder = 28,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "journey", Description = "Journey back through the biomes", Type = ObjectiveType.ReachLocation, TargetId = "biome_Fringe" },
                new() { Id = "reach_pod", Description = "Return to the pod field", Type = ObjectiveType.ReachLocation, TargetId = "loc_pod_field" },
                new() { Id = "complete", Description = "Return to where it began", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.ReturnedToPodField }
            },
            Reward = new QuestReward
            {
                Experience = 1000,
                Flags = new List<string> { StoryFlags.ReturnedToPodField }
            },
            SetsFlags = new List<string> { StoryFlags.ReturnedToPodField },
            NextQuestId = "main_29_ending"
        });

        // Quest 29: The Ending
        Register(new QuestDefinition
        {
            Id = "main_29_ending",
            Name = "Into The Grey",
            Description = "The journey ends where it began. In the grey light of an uncertain dawn, you face what comes next.",
            Summary = "Complete your journey",
            Type = QuestType.Main,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_28_return" },
            CanAbandon = false,
            SortOrder = 29,
            StartDialogId = "dialog_ending",
            CompleteDialogId = "dialog_credits",
            Objectives = new List<QuestObjective>
            {
                new() { Id = "reflect", Description = "Reflect on your journey", Type = ObjectiveType.Interact, TargetId = "interactable_protagonist_pod" },
                new() { Id = "complete", Description = "Complete the game", Type = ObjectiveType.TriggerFlag, TargetId = StoryFlags.GameComplete }
            },
            Reward = new QuestReward
            {
                Experience = 0,
                Flags = new List<string> { StoryFlags.GameComplete }
            },
            SetsFlags = new List<string> { StoryFlags.GameComplete }
        });
    }
}
