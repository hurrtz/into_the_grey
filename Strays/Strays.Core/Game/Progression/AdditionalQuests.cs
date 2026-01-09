using System.Collections.Generic;
using Strays.Core.Game.Data;
using Strays.Core.Services;

namespace Strays.Core.Game.Progression;

/// <summary>
/// Additional side quests for completionists.
/// These quests provide extra content, lore, and rewards for thorough players.
/// </summary>
public static class AdditionalQuests
{
    /// <summary>
    /// Registers all additional side quests.
    /// Call this after the main quest definitions are loaded.
    /// </summary>
    public static void RegisterAll()
    {
        RegisterCollectionQuests();
        RegisterHuntingQuests();
        RegisterLoreQuests();
        RegisterChallengeQuests();
        RegisterRelationshipQuests();
        RegisterSecretQuests();
    }

    /// <summary>
    /// Collection-focused side quests.
    /// </summary>
    private static void RegisterCollectionQuests()
    {
        // Complete Microchip Collection
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_collect_chips_protocol",
            Name = "Protocol Collector",
            Description = "A traveling merchant is seeking rare Protocol microchips. Gathering a complete set would be quite valuable.",
            Summary = "Collect all Protocol microchips",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            RequiredFlags = new List<string> { "reached_rust" },
            RecommendedLevel = 15,
            SortOrder = 500,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "collect", Description = "Collect Protocol microchips (0/6)", Type = ObjectiveType.Collect, TargetId = "protocol_chip", RequiredCount = 6 }
            },
            Reward = new QuestReward
            {
                Experience = 400,
                Currency = 800,
                ItemIds = new List<string> { "proto_override" }
            }
        });

        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_collect_augments",
            Name = "The Augmentation Collector",
            Description = "Dr. Cortex requests specimens of each augmentation type for her research. The rewards are substantial.",
            Summary = "Gather one of each augmentation type",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            BiomeId = "rust",
            RequiredFlags = new List<string> { StoryFlags.DiscoveredConversionFacility },
            RecommendedLevel = 20,
            SortOrder = 501,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "head", Description = "Find a Cranial augmentation", Type = ObjectiveType.Collect, TargetId = "aug_cranial" },
                new() { Id = "optic", Description = "Find an Optical augmentation", Type = ObjectiveType.Collect, TargetId = "aug_optical" },
                new() { Id = "torso", Description = "Find a Torso augmentation", Type = ObjectiveType.Collect, TargetId = "aug_torso" },
                new() { Id = "limb", Description = "Find a Limb augmentation", Type = ObjectiveType.Collect, TargetId = "aug_limb" },
                new() { Id = "tail", Description = "Find a Tail augmentation", Type = ObjectiveType.Collect, TargetId = "aug_tail" }
            },
            Reward = new QuestReward
            {
                Experience = 600,
                Currency = 1000,
                ItemIds = new List<string> { "rare_augment_voucher" },
                Flags = new List<string> { "augment_collector" }
            }
        });

        // Rare Consumable Collector
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_collect_consumables",
            Name = "The Alchemist's Request",
            Description = "A strange alchemist needs rare consumables for their experiments. They promise unique rewards.",
            Summary = "Gather rare consumables",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            BiomeId = "green",
            RequiredFlags = new List<string> { "reached_green" },
            RecommendedLevel = 18,
            SortOrder = 502,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "serum", Description = "Find Calm Serum", Type = ObjectiveType.Collect, TargetId = "calm_serum" },
                new() { Id = "elixir", Description = "Find Energy Elixir", Type = ObjectiveType.Collect, TargetId = "energy_elixir" },
                new() { Id = "tonic", Description = "Find Evolution Tonic", Type = ObjectiveType.Collect, TargetId = "evolution_tonic" }
            },
            Reward = new QuestReward
            {
                Experience = 350,
                Currency = 500,
                ItemIds = new List<string> { "philosophers_stone" }
            }
        });

        // Memory Fragment Collector
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_collect_memories",
            Name = "Fragments of the Past",
            Description = "Scattered memory fragments contain glimpses of the old world. Collecting them may reveal hidden truths.",
            Summary = "Collect memory fragments from each biome",
            Type = QuestType.Discovery,
            Act = ActState.Act2_Responsibility,
            RequiredFlags = new List<string> { StoryFlags.CompletedDeadChannel },
            RecommendedLevel = 24,
            SortOrder = 503,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "fringe", Description = "Find the Fringe Memory", Type = ObjectiveType.Collect, TargetId = "memory_fringe" },
                new() { Id = "rust", Description = "Find the Rust Memory", Type = ObjectiveType.Collect, TargetId = "memory_rust" },
                new() { Id = "green", Description = "Find the Green Memory", Type = ObjectiveType.Collect, TargetId = "memory_green" },
                new() { Id = "quiet", Description = "Find the Quiet Memory", Type = ObjectiveType.Collect, TargetId = "memory_quiet" },
                new() { Id = "teeth", Description = "Find the Teeth Memory", Type = ObjectiveType.Collect, TargetId = "memory_teeth" },
                new() { Id = "glow", Description = "Find the Glow Memory", Type = ObjectiveType.Collect, TargetId = "memory_glow", IsHidden = true }
            },
            Reward = new QuestReward
            {
                Experience = 800,
                Flags = new List<string> { "memory_collector", "knows_full_history" },
                ItemIds = new List<string> { "complete_memory_archive" }
            }
        });
    }

    /// <summary>
    /// Hunting-focused side quests.
    /// </summary>
    private static void RegisterHuntingQuests()
    {
        // Fringe Hunter
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_hunt_fringe",
            Name = "Fringe Predator",
            Description = "The Fringe has too many hostile Strays. Thin their numbers.",
            Summary = "Hunt hostile Strays in the Fringe",
            Type = QuestType.Side,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            RequiredFlags = new List<string> { StoryFlags.CompletedTutorialBattle },
            RecommendedLevel = 5,
            SortOrder = 510,
            IsRepeatable = true,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "hunt", Description = "Defeat hostile Strays (0/10)", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_fringe_random", RequiredCount = 10 }
            },
            Reward = new QuestReward
            {
                Experience = 150,
                Currency = 100
            }
        });

        // Elite Hunter Chain
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_hunt_elite_1",
            Name = "Elite Hunter: Rustwing Terror",
            Description = "A particularly dangerous Stray known as the Rustwing Terror has been spotted in The Rust.",
            Summary = "Hunt the Rustwing Terror",
            Type = QuestType.Side,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            RequiredFlags = new List<string> { "reached_rust" },
            RecommendedLevel = 14,
            SortOrder = 511,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find", Description = "Track the Rustwing Terror", Type = ObjectiveType.ReachLocation, TargetId = "loc_rustwing_lair" },
                new() { Id = "hunt", Description = "Defeat the Rustwing Terror", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_rustwing_terror" }
            },
            Reward = new QuestReward
            {
                Experience = 400,
                Currency = 300,
                ItemIds = new List<string> { "rustwing_feather" },
                UnlocksQuests = new List<string> { "side_hunt_elite_2" }
            }
        });

        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_hunt_elite_2",
            Name = "Elite Hunter: Thornback Alpha",
            Description = "Word has spread of your hunting prowess. A beast called the Thornback Alpha terrorizes The Green.",
            Summary = "Hunt the Thornback Alpha",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            BiomeId = "green",
            Prerequisites = new List<string> { "side_hunt_elite_1" },
            RecommendedLevel = 22,
            SortOrder = 512,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find", Description = "Track the Thornback Alpha", Type = ObjectiveType.ReachLocation, TargetId = "loc_thornback_den" },
                new() { Id = "hunt", Description = "Defeat the Thornback Alpha", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_thornback_alpha" }
            },
            Reward = new QuestReward
            {
                Experience = 600,
                Currency = 500,
                ItemIds = new List<string> { "thornback_spine" },
                UnlocksQuests = new List<string> { "side_hunt_elite_3" }
            }
        });

        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_hunt_elite_3",
            Name = "Elite Hunter: Void Stalker",
            Description = "The ultimate hunt. A creature called the Void Stalker phases between realities in the Archive Scar.",
            Summary = "Hunt the legendary Void Stalker",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "archive_scar",
            Prerequisites = new List<string> { "side_hunt_elite_2" },
            RequiredFlags = new List<string> { "reached_archive" },
            RecommendedLevel = 35,
            SortOrder = 513,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "prepare", Description = "Acquire a Void Anchor", Type = ObjectiveType.Collect, TargetId = "item_void_anchor" },
                new() { Id = "find", Description = "Locate the Void Stalker's rift", Type = ObjectiveType.ReachLocation, TargetId = "loc_void_rift" },
                new() { Id = "hunt", Description = "Defeat the Void Stalker", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_void_stalker" }
            },
            Reward = new QuestReward
            {
                Experience = 1200,
                Currency = 1000,
                ItemIds = new List<string> { "void_essence", "legendary_chip" },
                Flags = new List<string> { "master_hunter" }
            }
        });

        // Rare Stray Hunt
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_hunt_legendary_strays",
            Name = "The Legendary Hunt",
            Description = "Legends speak of three legendary Strays that predate NIMDOK. Find and face them.",
            Summary = "Encounter the three legendary Strays",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            RequiredFlags = new List<string> { "archive_worthy" },
            RecommendedLevel = 38,
            SortOrder = 514,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "kernel", Description = "Encounter the Kernel Dragon", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_kernel_dragon" },
                new() { Id = "root", Description = "Encounter the Root Access Bear", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_root_bear" },
                new() { Id = "original", Description = "Encounter the Original Instance", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_original_instance" }
            },
            Reward = new QuestReward
            {
                Experience = 2000,
                Currency = 3000,
                Flags = new List<string> { "legendary_hunter" },
                ItemIds = new List<string> { "legendary_core" }
            }
        });
    }

    /// <summary>
    /// Lore-focused side quests.
    /// </summary>
    private static void RegisterLoreQuests()
    {
        // NIMDOK's Origin
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_lore_nimdok_origin",
            Name = "Before the Fall",
            Description = "Scattered data fragments contain pieces of NIMDOK's original purpose. Understanding the past may illuminate the present.",
            Summary = "Uncover NIMDOK's origins",
            Type = QuestType.Discovery,
            Act = ActState.Act2_Responsibility,
            RequiredFlags = new List<string> { StoryFlags.LearnedBioShellTruth },
            RecommendedLevel = 20,
            SortOrder = 520,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "terminal1", Description = "Access the First Archive Terminal", Type = ObjectiveType.Interact, TargetId = "terminal_archive_1" },
                new() { Id = "terminal2", Description = "Access the Second Archive Terminal", Type = ObjectiveType.Interact, TargetId = "terminal_archive_2" },
                new() { Id = "terminal3", Description = "Access the Third Archive Terminal", Type = ObjectiveType.Interact, TargetId = "terminal_archive_3" },
                new() { Id = "piece", Description = "Piece together the truth", Type = ObjectiveType.TriggerFlag, TargetId = "learned_nimdok_origin" }
            },
            Reward = new QuestReward
            {
                Experience = 500,
                Flags = new List<string> { "learned_nimdok_origin", "scholar" }
            }
        });

        // Operation Absolute
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_lore_operation_absolute",
            Name = "Operation Absolute",
            Description = "Military records hint at a classified operation. What happened in the final days?",
            Summary = "Investigate Operation Absolute",
            Type = QuestType.Discovery,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "teeth",
            RequiredFlags = new List<string> { "reached_teeth" },
            RecommendedLevel = 28,
            SortOrder = 521,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "bunker1", Description = "Find Bunker Alpha records", Type = ObjectiveType.ReachLocation, TargetId = "loc_bunker_alpha" },
                new() { Id = "bunker2", Description = "Find Bunker Beta records", Type = ObjectiveType.ReachLocation, TargetId = "loc_bunker_beta" },
                new() { Id = "command", Description = "Access the Command Center", Type = ObjectiveType.Interact, TargetId = "terminal_command" }
            },
            Reward = new QuestReward
            {
                Experience = 600,
                ItemIds = new List<string> { "classified_data" },
                Flags = new List<string> { "operation_absolute_known" }
            }
        });

        // The First Bio-Shells
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_lore_bioshells",
            Name = "The First Generation",
            Description = "You weren't the first Bio-Shell. Find out what happened to the others.",
            Summary = "Discover the fate of earlier Bio-Shells",
            Type = QuestType.Discovery,
            Act = ActState.Act2_Responsibility,
            RequiredFlags = new List<string> { StoryFlags.LearnedBioShellTruth },
            RecommendedLevel = 22,
            SortOrder = 522,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "pod1", Description = "Find Bio-Shell Alpha's pod", Type = ObjectiveType.ReachLocation, TargetId = "loc_pod_alpha" },
                new() { Id = "pod2", Description = "Find Bio-Shell Beta's pod", Type = ObjectiveType.ReachLocation, TargetId = "loc_pod_beta" },
                new() { Id = "pod3", Description = "Find Bio-Shell Gamma's fate", Type = ObjectiveType.ReachLocation, TargetId = "loc_pod_gamma" },
                new() { Id = "truth", Description = "Learn what happened to them", Type = ObjectiveType.TriggerFlag, TargetId = "learned_bioshell_history" }
            },
            Reward = new QuestReward
            {
                Experience = 550,
                Flags = new List<string> { "learned_bioshell_history" }
            }
        });

        // The Companion's Past
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_lore_companion_past",
            Name = "Before You Met",
            Description = "Your companion had a life before finding you. Traces of it remain scattered across The Fringe.",
            Summary = "Learn about your companion's past",
            Type = QuestType.Companion,
            Act = ActState.Act1_Denial,
            BiomeId = "fringe",
            RequiredFlags = new List<string> { StoryFlags.MetCompanion },
            BlockingFlags = new List<string> { StoryFlags.CompanionDeparted },
            RecommendedLevel = 8,
            SortOrder = 523,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "den", Description = "Find your companion's old den", Type = ObjectiveType.ReachLocation, TargetId = "loc_companion_den" },
                new() { Id = "mark", Description = "Find the territorial marking", Type = ObjectiveType.Interact, TargetId = "interactable_territory_mark" },
                new() { Id = "memory", Description = "Trigger a memory", Type = ObjectiveType.TalkTo, TargetId = "npc_companion" }
            },
            Reward = new QuestReward
            {
                Experience = 200,
                Flags = new List<string> { "companion_history_known" }
            }
        });

        // The Creators
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_lore_creators",
            Name = "The Architects",
            Description = "Who built NIMDOK? Who created the Bio-Shells? Somewhere, records of the architects must exist.",
            Summary = "Find records of NIMDOK's creators",
            Type = QuestType.Discovery,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "archive_scar",
            RequiredFlags = new List<string> { "reached_archive" },
            RecommendedLevel = 34,
            SortOrder = 524,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "research", Description = "Find the Research Division records", Type = ObjectiveType.Interact, TargetId = "terminal_research" },
                new() { Id = "personnel", Description = "Find Personnel files", Type = ObjectiveType.Collect, TargetId = "item_personnel_files" },
                new() { Id = "founder", Description = "Discover the Founder's identity", Type = ObjectiveType.TriggerFlag, TargetId = "learned_founder" }
            },
            Reward = new QuestReward
            {
                Experience = 700,
                Flags = new List<string> { "learned_founder", "true_scholar" }
            }
        });
    }

    /// <summary>
    /// Challenge-focused side quests.
    /// </summary>
    private static void RegisterChallengeQuests()
    {
        // Survival Challenge
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_challenge_survival",
            Name = "Endurance Test",
            Description = "A mysterious arena offers a test of endurance. Survive wave after wave of enemies.",
            Summary = "Survive the arena challenge",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            RequiredFlags = new List<string> { "reached_teeth" },
            RecommendedLevel = 25,
            SortOrder = 530,
            IsRepeatable = true,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "wave1", Description = "Survive Wave 1", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_arena_wave_1" },
                new() { Id = "wave2", Description = "Survive Wave 2", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_arena_wave_2" },
                new() { Id = "wave3", Description = "Survive Wave 3", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_arena_wave_3" },
                new() { Id = "boss", Description = "Defeat the Arena Champion", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_arena_boss" }
            },
            Reward = new QuestReward
            {
                Experience = 800,
                Currency = 600,
                ItemIds = new List<string> { "arena_trophy" }
            }
        });

        // No-Damage Challenge
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_challenge_perfect",
            Name = "Flawless Victory",
            Description = "Complete a gauntlet without taking any damage. Only the most skilled survive.",
            Summary = "Complete the gauntlet without damage",
            Type = QuestType.Side,
            Act = ActState.Act3_Irreversibility,
            RequiredFlags = new List<string> { StoryFlags.EnteredTheGlow },
            RecommendedLevel = 32,
            SortOrder = 531,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "perfect", Description = "Complete the gauntlet flawlessly", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_perfect_gauntlet" }
            },
            Reward = new QuestReward
            {
                Experience = 1000,
                ItemIds = new List<string> { "perfect_core" },
                Flags = new List<string> { "flawless_warrior" }
            }
        });

        // Solo Challenge
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_challenge_solo",
            Name = "Lone Wolf",
            Description = "Fight alone, without any recruited Strays. Prove your independence.",
            Summary = "Complete battles solo",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            RequiredFlags = new List<string> { "reached_quiet" },
            RecommendedLevel = 24,
            SortOrder = 532,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "solo1", Description = "Win 3 battles alone", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_solo_battle", RequiredCount = 3 },
                new() { Id = "solo_boss", Description = "Defeat a boss alone", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_solo_boss" }
            },
            Reward = new QuestReward
            {
                Experience = 700,
                Flags = new List<string> { "lone_wolf" },
                ItemIds = new List<string> { "independence_medal" }
            }
        });

        // Speed Run Challenge
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_challenge_speed",
            Name = "Against the Clock",
            Description = "Complete a dungeon before time runs out. Speed is everything.",
            Summary = "Complete a timed dungeon run",
            Type = QuestType.Side,
            Act = ActState.Act2_Responsibility,
            RequiredFlags = new List<string> { "reached_green" },
            RecommendedLevel = 20,
            SortOrder = 533,
            IsRepeatable = true,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "timed", Description = "Complete the dungeon in under 5 minutes", Type = ObjectiveType.Survive, TargetId = "event_timed_dungeon" }
            },
            Reward = new QuestReward
            {
                Experience = 400,
                Currency = 300,
                ItemIds = new List<string> { "speed_boots" }
            }
        });

        // Level 1 Challenge
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_challenge_underlevel",
            Name = "David vs Goliath",
            Description = "Defeat a powerful enemy while severely underleveled. Strategy over strength.",
            Summary = "Defeat a boss while underleveled",
            Type = QuestType.Side,
            Act = ActState.Act1_Denial,
            RequiredFlags = new List<string> { StoryFlags.CompletedTutorialBattle },
            RecommendedLevel = 10,
            SortOrder = 534,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "boss", Description = "Defeat the Oversized Threat (Recommended: Lv25)", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_oversized_threat" }
            },
            Reward = new QuestReward
            {
                Experience = 1500,
                Flags = new List<string> { "giant_slayer" },
                ItemIds = new List<string> { "slingshot_chip" }
            }
        });
    }

    /// <summary>
    /// Relationship-focused side quests.
    /// </summary>
    private static void RegisterRelationshipQuests()
    {
        // Bond Quest Chain
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_bond_1",
            Name = "Growing Trust",
            Description = "Your first recruited Stray seems to want something. Pay attention to its needs.",
            Summary = "Build trust with your first Stray",
            Type = QuestType.Side,
            Act = ActState.Act1_Denial,
            RequiredFlags = new List<string> { StoryFlags.RecruitedEchoPup },
            RecommendedLevel = 6,
            SortOrder = 540,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "feed", Description = "Feed your Stray their favorite food", Type = ObjectiveType.Interact, TargetId = "interactable_feed_stray" },
                new() { Id = "play", Description = "Spend time with your Stray", Type = ObjectiveType.TalkTo, TargetId = "recruited_stray_1" },
                new() { Id = "battle", Description = "Win 5 battles together", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_any", RequiredCount = 5 }
            },
            Reward = new QuestReward
            {
                Experience = 150,
                Flags = new List<string> { "first_bond_formed" }
            }
        });

        // NPC Relationship: Machinist
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_npc_machinist",
            Name = "The Machinist's Favor",
            Description = "The Rust Machinist needs help with a project. Helping them might earn their trust.",
            Summary = "Help the Machinist",
            Type = QuestType.Faction,
            Act = ActState.Act1_Denial,
            BiomeId = "rust",
            RequiredFlags = new List<string> { "reached_rust" },
            RecommendedLevel = 12,
            SortOrder = 541,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "talk", Description = "Speak with the Machinist", Type = ObjectiveType.TalkTo, TargetId = "rust_machinist" },
                new() { Id = "parts", Description = "Gather spare parts (0/5)", Type = ObjectiveType.Collect, TargetId = "item_spare_parts", RequiredCount = 5 },
                new() { Id = "return", Description = "Return to the Machinist", Type = ObjectiveType.TalkTo, TargetId = "rust_machinist" }
            },
            Reward = new QuestReward
            {
                Experience = 250,
                Currency = 200,
                ReputationChanges = new Dictionary<string, int> { { "Machinists", 15 } }
            }
        });

        // NPC Relationship: Healer
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_npc_healer",
            Name = "The Healer's Garden",
            Description = "The Green Healer's garden is under threat. Help them protect it.",
            Summary = "Protect the Healer's garden",
            Type = QuestType.Faction,
            Act = ActState.Act2_Responsibility,
            BiomeId = "green",
            RequiredFlags = new List<string> { "reached_green" },
            RecommendedLevel = 20,
            SortOrder = 542,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "talk", Description = "Speak with the Healer", Type = ObjectiveType.TalkTo, TargetId = "green_healer" },
                new() { Id = "defend", Description = "Defend the garden from pests", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_garden_pests" },
                new() { Id = "return", Description = "Return to the Healer", Type = ObjectiveType.TalkTo, TargetId = "green_healer" }
            },
            Reward = new QuestReward
            {
                Experience = 350,
                ItemIds = new List<string> { "healing_herb", "healing_herb", "healing_herb" },
                ReputationChanges = new Dictionary<string, int> { { "Naturalists", 15 } }
            }
        });

        // Full Party Quest
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_full_party",
            Name = "Strength in Numbers",
            Description = "Recruit a full party of Strays and lead them into battle together.",
            Summary = "Fill your party and win together",
            Type = QuestType.Side,
            Act = ActState.Act1_Denial,
            RequiredFlags = new List<string> { StoryFlags.RecruitedEchoPup },
            RecommendedLevel = 10,
            SortOrder = 543,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "recruit", Description = "Recruit 4 Strays total", Type = ObjectiveType.RecruitStray, TargetId = "any_stray", RequiredCount = 4 },
                new() { Id = "battle", Description = "Win a battle with a full party", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_any_with_full_party" }
            },
            Reward = new QuestReward
            {
                Experience = 300,
                Flags = new List<string> { "pack_leader" }
            }
        });
    }

    /// <summary>
    /// Secret/hidden side quests.
    /// </summary>
    private static void RegisterSecretQuests()
    {
        // Secret Boss: The Forgotten
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_secret_forgotten",
            Name = "???",
            Description = "Something stirs in the deepest part of the Archive Scar. Something that was meant to stay forgotten.",
            Summary = "Find and face the Forgotten",
            Type = QuestType.Discovery,
            Act = ActState.Act3_Irreversibility,
            BiomeId = "archive_scar",
            RequiredFlags = new List<string> { "void_walker", "memory_collector", "master_hunter" },
            RecommendedLevel = 40,
            SortOrder = 550,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "find", Description = "Find the hidden entrance", Type = ObjectiveType.ReachLocation, TargetId = "loc_forgotten_chamber", IsHidden = true },
                new() { Id = "face", Description = "Face the Forgotten", Type = ObjectiveType.DefeatEncounter, TargetId = "enc_the_forgotten" }
            },
            Reward = new QuestReward
            {
                Experience = 5000,
                Currency = 10000,
                Flags = new List<string> { "faced_the_forgotten", "true_completionist" },
                ItemIds = new List<string> { "forgotten_essence" }
            }
        });

        // Secret: Original NIMDOK Voice
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_secret_voice",
            Name = "The Original Voice",
            Description = "NIMDOK wasn't always like this. Somewhere, the original personality still exists.",
            Summary = "Find NIMDOK's original voice",
            Type = QuestType.Discovery,
            Act = ActState.Act3_Irreversibility,
            RequiredFlags = new List<string> { "learned_nimdok_origin", "root_access_granted" },
            RecommendedLevel = 35,
            SortOrder = 551,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "access", Description = "Access the sealed archive", Type = ObjectiveType.Interact, TargetId = "terminal_sealed_archive" },
                new() { Id = "decrypt", Description = "Decrypt the original personality", Type = ObjectiveType.Interact, TargetId = "terminal_decrypt" },
                new() { Id = "listen", Description = "Listen to the original voice", Type = ObjectiveType.TriggerFlag, TargetId = "heard_original_nimdok" }
            },
            Reward = new QuestReward
            {
                Experience = 1000,
                Flags = new List<string> { "heard_original_nimdok" }
            }
        });

        // Secret: Perfect Ending Prep
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_secret_perfect_prep",
            Name = "All Paths Converge",
            Description = "You've done everything. Seen everything. Perhaps there's one more path...",
            Summary = "Unlock the secret ending",
            Type = QuestType.Discovery,
            Act = ActState.Act3_Irreversibility,
            RequiredFlags = new List<string>
            {
                "memory_collector", "master_hunter", "augment_collector",
                "learned_nimdok_origin", "operation_absolute_known", "learned_bioshell_history",
                "companion_history_known", "learned_founder"
            },
            RecommendedLevel = 35,
            SortOrder = 552,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "return", Description = "Return to where it all began", Type = ObjectiveType.ReachLocation, TargetId = "loc_pod_field" },
                new() { Id = "remember", Description = "Remember everything", Type = ObjectiveType.TriggerFlag, TargetId = "perfect_memory" }
            },
            Reward = new QuestReward
            {
                Experience = 0,
                Flags = new List<string> { "perfect_memory", "secret_ending_available" }
            }
        });

        // Easter Egg Quest
        QuestDefinitions.Register(new QuestDefinition
        {
            Id = "side_secret_developer",
            Name = "Meta",
            Description = "You found something that shouldn't exist. A message from outside the simulation.",
            Summary = "Read the developer's message",
            Type = QuestType.Discovery,
            Act = ActState.Act2_Responsibility,
            RequiredFlags = new List<string> { "found_dev_room" },
            SortOrder = 553,
            Objectives = new List<QuestObjective>
            {
                new() { Id = "read", Description = "Read the message", Type = ObjectiveType.Interact, TargetId = "interactable_dev_message" }
            },
            Reward = new QuestReward
            {
                Experience = 100,
                Flags = new List<string> { "meta_aware" }
            }
        });
    }
}
