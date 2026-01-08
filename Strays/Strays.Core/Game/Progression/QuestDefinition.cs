using System.Collections.Generic;
using System.Linq;
using Strays.Core.Game.Data;
using Strays.Core.Services;

namespace Strays.Core.Game.Progression;

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
    /// Recruit a specific Stray.
    /// </summary>
    RecruitStray,

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
    /// Stray definition IDs that can be recruited.
    /// </summary>
    public List<string> UnlockedStrayIds { get; set; } = new();

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
            Description = "A hostile Stray approaches. Your companion's new ability may prove useful.",
            Summary = "Survive your first combat",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_03_the_chip" },
            CanAbandon = false,
            SortOrder = 4,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "fight", Description = "Defeat the hostile Stray", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_tutorial_battle" },
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
            Description = "A small Stray with unusual abilities watches you from the fog. It seems... curious rather than hostile.",
            Summary = "Recruit your first Stray",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            Prerequisites = new List<string> { "main_04_first_battle" },
            CanAbandon = false,
            SortOrder = 5,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "approach", Description = "Approach the curious Stray", Type = ObjectiveType.ReachLocation, TargetId = "loc_echo_pup" },
                new() { Id = "recruit", Description = "Recruit Echo Pup", Type = ObjectiveType.RecruitStray, TargetId = "echo_pup" }
            },
            Reward = new QuestReward
            {
                Experience = 150,
                Flags = new List<string> { StoryFlags.RecruitedEchoPup },
                UnlockedStrayIds = new List<string> { "echo_pup" }
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
            Description = "Echo Pup's data digestion ability has revealed something: a massive structure to the north, humming with power. NIMDOK.",
            Summary = "Journey toward NIMDOK",
            Type = QuestType.Main,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            Prerequisites = new List<string> { "main_06_power_source" },
            RecommendedLevel = 8,
            CanAbandon = false,
            SortOrder = 7,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "leave_fringe", Description = "Leave The Fringe", Type = ObjectiveType.ReachLocation, TargetId = "loc_fringe_exit" },
                new() { Id = "enter_rust", Description = "Enter The Rust", Type = ObjectiveType.ReachLocation, TargetId = "loc_rust_entrance" }
            },
            Reward = new QuestReward
            {
                Experience = 250
            },
            NextQuestId = "main_08_conversion_facility"
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
            Description = "Something large moves through the perpetual fog of The Fringe. The other Strays avoid it. Perhaps you should too... or perhaps not.",
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
                UnlockedStrayIds = new List<string> { "fog_lurker" }
            }
        });
    }
}
