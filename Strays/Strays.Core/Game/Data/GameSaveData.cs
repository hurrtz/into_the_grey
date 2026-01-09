using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Strays.Core.Game.World;

namespace Strays.Core.Game.Data;

/// <summary>
/// Complete game state that can be serialized for save/load.
/// </summary>
public class GameSaveData
{
    /// <summary>
    /// Current story act.
    /// </summary>
    public ActState CurrentAct { get; set; } = ActState.Act1_Denial;

    /// <summary>
    /// The player's chosen companion type.
    /// </summary>
    public CompanionType CompanionType { get; set; } = CompanionType.Dog;

    /// <summary>
    /// Current Gravitation stage (escalates through Act 2).
    /// </summary>
    public GravitationStage GravitationStage { get; set; } = GravitationStage.Normal;

    /// <summary>
    /// Whether the companion is still with the party (leaves before Act 3 climax).
    /// </summary>
    public bool CompanionPresent { get; set; } = true;

    /// <summary>
    /// Protagonist's current position in the world.
    /// </summary>
    public Vector2 ProtagonistPosition { get; set; } = Vector2.Zero;

    /// <summary>
    /// Current biome the protagonist is in.
    /// </summary>
    public BiomeType CurrentBiome { get; set; } = BiomeType.Fringe;

    /// <summary>
    /// Whether the protagonist has the exoskeleton (enables running).
    /// </summary>
    public bool HasExoskeleton { get; set; } = false;

    /// <summary>
    /// Whether the exoskeleton has a working battery.
    /// </summary>
    public bool ExoskeletonPowered { get; set; } = false;

    /// <summary>
    /// Story flags tracking quest progress and events.
    /// Keys are flag names, values are flag states.
    /// </summary>
    public Dictionary<string, bool> StoryFlags { get; set; } = new();

    /// <summary>
    /// Integer counters for various game stats.
    /// </summary>
    public Dictionary<string, int> Counters { get; set; } = new();

    /// <summary>
    /// IDs of Strays currently in the player's party (max 5).
    /// </summary>
    public List<string> PartyStrayIds { get; set; } = new();

    /// <summary>
    /// IDs of all Strays the player has recruited (storage).
    /// </summary>
    public List<string> RosterStrayIds { get; set; } = new();

    /// <summary>
    /// Detailed data for each Stray the player owns.
    /// Key is the Stray's unique instance ID.
    /// </summary>
    public Dictionary<string, StraySaveData> OwnedStrays { get; set; } = new();

    /// <summary>
    /// IDs of encounters that have been cleared (won't respawn).
    /// </summary>
    public HashSet<string> ClearedEncounters { get; set; } = new();

    /// <summary>
    /// IDs of quests that are currently active.
    /// </summary>
    public HashSet<string> ActiveQuests { get; set; } = new();

    /// <summary>
    /// Alias for ActiveQuests (used by QuestLog).
    /// </summary>
    public HashSet<string> ActiveQuestIds => ActiveQuests;

    /// <summary>
    /// IDs of quests that have been completed.
    /// </summary>
    public HashSet<string> CompletedQuests { get; set; } = new();

    /// <summary>
    /// Alias for CompletedQuests (used by QuestLog).
    /// </summary>
    public HashSet<string> CompletedQuestIds => CompletedQuests;

    /// <summary>
    /// Player's currency (scrap/credits).
    /// </summary>
    public int Currency { get; set; } = 0;

    /// <summary>
    /// Total play time in seconds.
    /// </summary>
    public double TotalPlayTimeSeconds { get; set; } = 0;

    /// <summary>
    /// Save slot this data belongs to.
    /// </summary>
    public int SaveSlot { get; set; } = 0;

    /// <summary>
    /// Faction reputation values.
    /// Key is faction name, value is reputation (-1000 to 1000).
    /// </summary>
    public Dictionary<string, int> FactionReputation { get; set; } = new();

    /// <summary>
    /// IDs of items in the player's inventory.
    /// </summary>
    public List<string> InventoryItems { get; set; } = new();

    /// <summary>
    /// IDs of microchips owned but not equipped.
    /// </summary>
    public List<string> OwnedMicrochips { get; set; } = new();

    /// <summary>
    /// IDs of augmentations owned but not equipped.
    /// </summary>
    public List<string> OwnedAugmentations { get; set; } = new();

    /// <summary>
    /// Discovered settlement IDs.
    /// </summary>
    public HashSet<string> DiscoveredSettlements { get; set; } = new();

    /// <summary>
    /// Last save timestamp.
    /// </summary>
    public string SaveTimestamp { get; set; } = "";

    /// <summary>
    /// Number of Strays recruited total.
    /// </summary>
    public int TotalStraysRecruited { get; set; } = 0;

    /// <summary>
    /// Number of battles won.
    /// </summary>
    public int BattlesWon { get; set; } = 0;

    /// <summary>
    /// Number of battles fled from.
    /// </summary>
    public int BattlesFled { get; set; } = 0;

    /// <summary>
    /// Number of enemies spared in combat.
    /// </summary>
    public int EnemiesSpared { get; set; } = 0;

    /// <summary>
    /// Number of bounty quests completed.
    /// </summary>
    public int BountiesCompleted { get; set; } = 0;

    /// <summary>
    /// Player's morality score (-100 to 100).
    /// </summary>
    public int Morality { get; set; } = 0;
}

/// <summary>
/// Save data for an individual Stray instance.
/// </summary>
public class StraySaveData
{
    /// <summary>
    /// Unique instance ID for this Stray.
    /// </summary>
    public string InstanceId { get; set; } = "";

    /// <summary>
    /// The definition ID (species/type) of this Stray.
    /// </summary>
    public string DefinitionId { get; set; } = "";

    /// <summary>
    /// Custom nickname given by the player (null = use default name).
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Current level.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Experience points toward next level.
    /// </summary>
    public int Experience { get; set; } = 0;

    /// <summary>
    /// Current HP (may be less than max if damaged).
    /// </summary>
    public int CurrentHp { get; set; } = 100;

    /// <summary>
    /// Whether this Stray has evolved.
    /// </summary>
    public bool IsEvolved { get; set; } = false;

    /// <summary>
    /// IDs of equipped augmentations by slot.
    /// </summary>
    public Dictionary<string, string?> EquippedAugmentations { get; set; } = new();

    /// <summary>
    /// IDs of equipped microchips.
    /// </summary>
    public List<string> EquippedMicrochips { get; set; } = new();

    /// <summary>
    /// Bond level with the protagonist (affects loyalty and abilities).
    /// </summary>
    public int BondLevel { get; set; } = 0;
}
