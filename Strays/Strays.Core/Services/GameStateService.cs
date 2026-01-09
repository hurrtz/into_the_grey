using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Data;
using Strays.Core.Game.Progression;
using Strays.Core.Game.World;

namespace Strays.Core.Services;

/// <summary>
/// Central service managing all game state, progression, and save/load functionality.
/// Register as a service in Game.Services for global access.
/// </summary>
public class GameStateService
{
    private const string SaveDirectory = "Saves";
    private const string SaveFilePattern = "save_{0}.json";
    private const string AutoSaveSlotName = "autosave";
    private const int MaxSaveSlots = 3;
    private const int AutoSaveSlot = 99;

    private GameSaveData _currentData;
    private readonly JsonSerializerOptions _jsonOptions;
    private FactionReputation? _factionReputation;

    // Auto-save configuration
    private double _autoSaveIntervalSeconds = 300; // 5 minutes
    private double _timeSinceLastAutoSave = 0;
    private bool _autoSaveEnabled = true;
    private bool _autoSavePending = false;

    /// <summary>
    /// Event fired when game state is loaded.
    /// </summary>
    public event EventHandler<GameSaveData>? StateLoaded;

    /// <summary>
    /// Event fired when game state is saved.
    /// </summary>
    public event EventHandler<GameSaveData>? StateSaved;

    /// <summary>
    /// Event fired when a story flag changes.
    /// </summary>
    public event EventHandler<string>? StoryFlagChanged;

    /// <summary>
    /// Event fired when the act changes.
    /// </summary>
    public event EventHandler<ActState>? ActChanged;

    /// <summary>
    /// Event fired when an auto-save is triggered.
    /// </summary>
    public event EventHandler? AutoSaveTriggered;

    public GameStateService()
    {
        _currentData = new GameSaveData();
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Maximum number of manual save slots.
    /// </summary>
    public int MaxSlots => MaxSaveSlots;

    /// <summary>
    /// Whether auto-save is enabled.
    /// </summary>
    public bool AutoSaveEnabled
    {
        get => _autoSaveEnabled;
        set => _autoSaveEnabled = value;
    }

    /// <summary>
    /// Auto-save interval in seconds.
    /// </summary>
    public double AutoSaveInterval
    {
        get => _autoSaveIntervalSeconds;
        set => _autoSaveIntervalSeconds = Math.Max(60, value); // Minimum 1 minute
    }

    /// <summary>
    /// Gets the faction reputation tracker for this save.
    /// </summary>
    public FactionReputation FactionReputation
    {
        get
        {
            if (_factionReputation == null)
            {
                _factionReputation = new FactionReputation();
                // Restore from save data
                foreach (var kvp in _currentData.FactionReputation)
                {
                    if (Enum.TryParse<FactionType>(kvp.Key, out var faction))
                    {
                        _factionReputation.SetReputation(faction, kvp.Value);
                    }
                }
            }
            return _factionReputation;
        }
    }

    #region Current State Properties

    /// <summary>
    /// The current game save data.
    /// </summary>
    public GameSaveData Data => _currentData;

    /// <summary>
    /// Current story act.
    /// </summary>
    public ActState CurrentAct
    {
        get => _currentData.CurrentAct;
        set
        {
            if (_currentData.CurrentAct != value)
            {
                _currentData.CurrentAct = value;
                ActChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>
    /// The player's chosen companion type.
    /// </summary>
    public CompanionType CompanionType
    {
        get => _currentData.CompanionType;
        set => _currentData.CompanionType = value;
    }

    /// <summary>
    /// Current Gravitation escalation stage.
    /// </summary>
    public GravitationStage GravitationStage
    {
        get => _currentData.GravitationStage;
        set => _currentData.GravitationStage = value;
    }

    // Gravitation escalation constants
    private const string GravitationUseCountKey = "gravitation_uses";
    private const int UsesForUnstable = 5;    // Escalates to Unstable after 5 uses
    private const int UsesForDangerous = 12;  // Escalates to Dangerous after 12 uses
    private const int UsesForCritical = 20;   // Escalates to Critical after 20 uses

    /// <summary>
    /// Number of times Gravitation has been used.
    /// </summary>
    public int GravitationUseCount
    {
        get => _currentData.Counters.TryGetValue(GravitationUseCountKey, out var count) ? count : 0;
        private set => _currentData.Counters[GravitationUseCountKey] = value;
    }

    /// <summary>
    /// Records a Gravitation use and checks for escalation.
    /// Returns true if the stage escalated.
    /// </summary>
    public bool RecordGravitationUse()
    {
        GravitationUseCount++;

        // Check for automatic escalation based on use count
        var previousStage = GravitationStage;

        // Only escalate if we haven't reached Absolute (final boss form)
        if (GravitationStage < GravitationStage.Critical)
        {
            if (GravitationUseCount >= UsesForCritical && GravitationStage < GravitationStage.Critical)
            {
                GravitationStage = GravitationStage.Critical;
            }
            else if (GravitationUseCount >= UsesForDangerous && GravitationStage < GravitationStage.Dangerous)
            {
                GravitationStage = GravitationStage.Dangerous;
            }
            else if (GravitationUseCount >= UsesForUnstable && GravitationStage < GravitationStage.Unstable)
            {
                GravitationStage = GravitationStage.Unstable;
            }
        }

        bool escalated = GravitationStage != previousStage;
        if (escalated)
        {
            System.Diagnostics.Debug.WriteLine($"[Gravitation] Stage escalated to {GravitationStage} after {GravitationUseCount} uses!");
        }

        return escalated;
    }

    /// <summary>
    /// Forces Gravitation to escalate to the next stage (story trigger).
    /// Returns true if escalation occurred.
    /// </summary>
    public bool ForceGravitationEscalation()
    {
        if (GravitationStage >= GravitationStage.Absolute)
            return false;

        GravitationStage = (GravitationStage)((int)GravitationStage + 1);
        System.Diagnostics.Debug.WriteLine($"[Gravitation] Stage force-escalated to {GravitationStage}!");
        return true;
    }

    /// <summary>
    /// Sets Gravitation directly to Absolute stage (final boss).
    /// </summary>
    public void SetGravitationAbsolute()
    {
        GravitationStage = GravitationStage.Absolute;
        System.Diagnostics.Debug.WriteLine("[Gravitation] Stage set to Absolute for final boss!");
    }

    /// <summary>
    /// Whether the companion is still with the party.
    /// </summary>
    public bool CompanionPresent
    {
        get => _currentData.CompanionPresent;
        set => _currentData.CompanionPresent = value;
    }

    /// <summary>
    /// Event fired when the companion departs.
    /// </summary>
    public event EventHandler<CompanionDepartureEventArgs>? CompanionDeparted;

    /// <summary>
    /// Triggers companion departure (Bandit leaves to protect the party).
    /// This happens when Gravitation at Critical stage nearly kills a party member.
    /// </summary>
    /// <param name="reason">The reason for departure.</param>
    public void TriggerCompanionDeparture(string reason = "")
    {
        if (!CompanionPresent)
            return;

        CompanionPresent = false;
        SetFlag("companion_departed");
        SetFlag("bandit_departure_scene_pending");

        System.Diagnostics.Debug.WriteLine($"[Companion] Bandit has departed! Reason: {reason}");

        CompanionDeparted?.Invoke(this, new CompanionDepartureEventArgs
        {
            Reason = reason,
            GravitationStage = GravitationStage,
            TotalUses = GravitationUseCount
        });
    }

    /// <summary>
    /// Checks if Gravitation should trigger companion departure.
    /// Called when Gravitation hits an ally at Critical stage.
    /// </summary>
    /// <param name="allyHp">Current HP of the ally that was hit.</param>
    /// <param name="allyMaxHp">Max HP of the ally that was hit.</param>
    /// <returns>True if departure was triggered.</returns>
    public bool CheckCriticalGravitationDeparture(int allyHp, int allyMaxHp)
    {
        if (!CompanionPresent || GravitationStage != GravitationStage.Critical)
            return false;

        // Departure triggers when ally is reduced to <5% HP (near death)
        float hpPercent = (float)allyHp / allyMaxHp;
        if (hpPercent < 0.05f)
        {
            TriggerCompanionDeparture("Nearly killed a party member with Critical Gravitation");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Protagonist's world position.
    /// </summary>
    public Vector2 ProtagonistPosition
    {
        get => _currentData.ProtagonistPosition;
        set => _currentData.ProtagonistPosition = value;
    }

    /// <summary>
    /// Current biome.
    /// </summary>
    public BiomeType CurrentBiome
    {
        get => _currentData.CurrentBiome;
        set => _currentData.CurrentBiome = value;
    }

    /// <summary>
    /// Whether the protagonist has the exoskeleton.
    /// </summary>
    public bool HasExoskeleton
    {
        get => _currentData.HasExoskeleton;
        set => _currentData.HasExoskeleton = value;
    }

    /// <summary>
    /// Whether the exoskeleton is powered (can run).
    /// </summary>
    public bool ExoskeletonPowered
    {
        get => _currentData.ExoskeletonPowered;
        set => _currentData.ExoskeletonPowered = value;
    }

    #endregion

    #region Story Flags

    /// <summary>
    /// Sets a story flag to true.
    /// </summary>
    public void SetFlag(string flagName)
    {
        if (!_currentData.StoryFlags.ContainsKey(flagName) || !_currentData.StoryFlags[flagName])
        {
            _currentData.StoryFlags[flagName] = true;
            StoryFlagChanged?.Invoke(this, flagName);
        }
    }

    /// <summary>
    /// Clears a story flag (sets to false).
    /// </summary>
    public void ClearFlag(string flagName)
    {
        if (_currentData.StoryFlags.ContainsKey(flagName) && _currentData.StoryFlags[flagName])
        {
            _currentData.StoryFlags[flagName] = false;
            StoryFlagChanged?.Invoke(this, flagName);
        }
    }

    /// <summary>
    /// Checks if a story flag is set.
    /// </summary>
    public bool HasFlag(string flagName)
    {
        return _currentData.StoryFlags.TryGetValue(flagName, out var value) && value;
    }

    /// <summary>
    /// Gets a counter value.
    /// </summary>
    public int GetCounter(string counterName)
    {
        return _currentData.Counters.TryGetValue(counterName, out var value) ? value : 0;
    }

    /// <summary>
    /// Sets a counter value.
    /// </summary>
    public void SetCounter(string counterName, int value)
    {
        _currentData.Counters[counterName] = value;
    }

    /// <summary>
    /// Increments a counter and returns the new value.
    /// </summary>
    public int IncrementCounter(string counterName, int amount = 1)
    {
        var newValue = GetCounter(counterName) + amount;
        SetCounter(counterName, newValue);
        return newValue;
    }

    #endregion

    #region Quest Management

    /// <summary>
    /// Starts a quest (adds to active quests).
    /// </summary>
    public void StartQuest(string questId)
    {
        _currentData.ActiveQuests.Add(questId);
    }

    /// <summary>
    /// Completes a quest (moves from active to completed).
    /// </summary>
    public void CompleteQuest(string questId)
    {
        _currentData.ActiveQuests.Remove(questId);
        _currentData.CompletedQuests.Add(questId);
    }

    /// <summary>
    /// Checks if a quest is active.
    /// </summary>
    public bool IsQuestActive(string questId)
    {
        return _currentData.ActiveQuests.Contains(questId);
    }

    /// <summary>
    /// Checks if a quest is completed.
    /// </summary>
    public bool IsQuestCompleted(string questId)
    {
        return _currentData.CompletedQuests.Contains(questId);
    }

    #endregion

    #region Encounter Management

    /// <summary>
    /// Marks an encounter as cleared (won't respawn).
    /// </summary>
    public void ClearEncounter(string encounterId)
    {
        _currentData.ClearedEncounters.Add(encounterId);
    }

    /// <summary>
    /// Checks if an encounter has been cleared.
    /// </summary>
    public bool IsEncounterCleared(string encounterId)
    {
        return _currentData.ClearedEncounters.Contains(encounterId);
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Creates a new game with default starting state.
    /// </summary>
    public void NewGame(CompanionType companionType = CompanionType.Dog)
    {
        _currentData = new GameSaveData
        {
            CompanionType = companionType,
            CurrentAct = ActState.Act1_Denial,
            CurrentBiome = BiomeType.Fringe,
            ProtagonistPosition = new Vector2(400, 300), // Starting position in The Fringe
            HasExoskeleton = false,
            ExoskeletonPowered = false,
            CompanionPresent = true,
            GravitationStage = GravitationStage.Normal,
            SaveTimestamp = DateTime.UtcNow.ToString("O"),
            Currency = 1000 // Starting currency for testing
        };

        // Reset faction reputation
        _factionReputation = new FactionReputation();

        // Reset auto-save timer
        _timeSinceLastAutoSave = 0;
        _autoSavePending = false;

        StateLoaded?.Invoke(this, _currentData);
    }

    /// <summary>
    /// Prepares save data by syncing transient state.
    /// </summary>
    private void PrepareSaveData()
    {
        // Update timestamp
        _currentData.SaveTimestamp = DateTime.UtcNow.ToString("O");

        // Sync faction reputation to save data
        if (_factionReputation != null)
        {
            _currentData.FactionReputation.Clear();
            foreach (FactionType faction in Enum.GetValues<FactionType>())
            {
                if (faction != FactionType.None)
                {
                    _currentData.FactionReputation[faction.ToString()] = _factionReputation.GetReputation(faction);
                }
            }
        }
    }

    /// <summary>
    /// Saves the current game state to a slot.
    /// </summary>
    public bool Save(int slot = 0)
    {
        try
        {
            _currentData.SaveSlot = slot;
            PrepareSaveData();

            var savePath = GetSavePath(slot);
            var directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_currentData, _jsonOptions);
            File.WriteAllText(savePath, json);

            _autoSavePending = false;
            StateSaved?.Invoke(this, _currentData);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save game: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Performs an auto-save.
    /// </summary>
    public bool AutoSave()
    {
        if (!_autoSaveEnabled) return false;

        var result = Save(AutoSaveSlot);
        if (result)
        {
            AutoSaveTriggered?.Invoke(this, EventArgs.Empty);
        }
        return result;
    }

    /// <summary>
    /// Triggers an auto-save on next opportunity (e.g., after combat, entering settlement).
    /// </summary>
    public void TriggerAutoSave()
    {
        _autoSavePending = true;
    }

    /// <summary>
    /// Called each frame to check if auto-save should occur.
    /// </summary>
    public void UpdateAutoSave(GameTime gameTime)
    {
        if (!_autoSaveEnabled) return;

        _timeSinceLastAutoSave += gameTime.ElapsedGameTime.TotalSeconds;

        // Time-based auto-save
        if (_timeSinceLastAutoSave >= _autoSaveIntervalSeconds)
        {
            AutoSave();
            _timeSinceLastAutoSave = 0;
        }
        // Event-triggered auto-save
        else if (_autoSavePending)
        {
            AutoSave();
            _timeSinceLastAutoSave = 0;
        }
    }

    /// <summary>
    /// Loads game state from a slot.
    /// </summary>
    public bool Load(int slot = 0)
    {
        try
        {
            var savePath = GetSavePath(slot);
            if (!File.Exists(savePath))
            {
                return false;
            }

            var json = File.ReadAllText(savePath);
            var data = JsonSerializer.Deserialize<GameSaveData>(json, _jsonOptions);

            if (data != null)
            {
                _currentData = data;

                // Reload faction reputation from save data
                _factionReputation = null; // Will be lazily reloaded

                // Reset auto-save timer
                _timeSinceLastAutoSave = 0;
                _autoSavePending = false;

                StateLoaded?.Invoke(this, _currentData);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load game: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads the auto-save if one exists.
    /// </summary>
    public bool LoadAutoSave()
    {
        return Load(AutoSaveSlot);
    }

    /// <summary>
    /// Checks if a save exists in a slot.
    /// </summary>
    public bool SaveExists(int slot = 0)
    {
        return File.Exists(GetSavePath(slot));
    }

    /// <summary>
    /// Checks if an auto-save exists.
    /// </summary>
    public bool AutoSaveExists()
    {
        return SaveExists(AutoSaveSlot);
    }

    /// <summary>
    /// Gets information about a save slot without loading the full state.
    /// </summary>
    public SaveSlotInfo? GetSaveInfo(int slot)
    {
        try
        {
            var savePath = GetSavePath(slot);
            if (!File.Exists(savePath))
            {
                return null;
            }

            var json = File.ReadAllText(savePath);
            var data = JsonSerializer.Deserialize<GameSaveData>(json, _jsonOptions);

            if (data == null) return null;

            return new SaveSlotInfo
            {
                Slot = slot,
                IsAutoSave = slot == AutoSaveSlot,
                Timestamp = DateTime.TryParse(data.SaveTimestamp, out var ts) ? ts : DateTime.MinValue,
                PlayTime = TimeSpan.FromSeconds(data.TotalPlayTimeSeconds),
                CurrentAct = data.CurrentAct,
                CurrentBiome = data.CurrentBiome,
                PartyCount = data.PartyStrayIds.Count,
                TotalStrays = data.OwnedStrays.Count,
                CompletedQuests = data.CompletedQuests.Count
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets info for the auto-save slot.
    /// </summary>
    public SaveSlotInfo? GetAutoSaveInfo()
    {
        return GetSaveInfo(AutoSaveSlot);
    }

    /// <summary>
    /// Gets all available save slot infos.
    /// </summary>
    public SaveSlotInfo[] GetAllSaveInfos()
    {
        var infos = new System.Collections.Generic.List<SaveSlotInfo>();

        for (int i = 0; i < MaxSaveSlots; i++)
        {
            var info = GetSaveInfo(i);
            if (info != null)
            {
                infos.Add(info);
            }
        }

        var autoInfo = GetAutoSaveInfo();
        if (autoInfo != null)
        {
            infos.Add(autoInfo);
        }

        return infos.ToArray();
    }

    /// <summary>
    /// Deletes a save slot.
    /// </summary>
    public bool DeleteSave(int slot = 0)
    {
        try
        {
            var savePath = GetSavePath(slot);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes the auto-save.
    /// </summary>
    public bool DeleteAutoSave()
    {
        return DeleteSave(AutoSaveSlot);
    }

    private static string GetSavePath(int slot)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var fileName = slot == AutoSaveSlot ? $"{AutoSaveSlotName}.json" : string.Format(SaveFilePattern, slot);
        return Path.Combine(baseDir, SaveDirectory, fileName);
    }

    #endregion

    #region Inventory Management

    /// <summary>
    /// Adds an item to the player's inventory.
    /// </summary>
    public void AddItem(string itemId)
    {
        _currentData.InventoryItems.Add(itemId);
    }

    /// <summary>
    /// Removes an item from the player's inventory.
    /// </summary>
    public bool RemoveItem(string itemId)
    {
        return _currentData.InventoryItems.Remove(itemId);
    }

    /// <summary>
    /// Checks if the player has an item.
    /// </summary>
    public bool HasItem(string itemId)
    {
        return _currentData.InventoryItems.Contains(itemId);
    }

    /// <summary>
    /// Gets the count of a specific item.
    /// </summary>
    public int GetItemCount(string itemId)
    {
        int count = 0;
        foreach (var item in _currentData.InventoryItems)
        {
            if (item == itemId) count++;
        }
        return count;
    }

    /// <summary>
    /// Adds a microchip to the player's owned chips.
    /// </summary>
    public void AddMicrochip(string chipId)
    {
        _currentData.OwnedMicrochips.Add(chipId);
    }

    /// <summary>
    /// Removes a microchip from the player's owned chips.
    /// </summary>
    public bool RemoveMicrochip(string chipId)
    {
        return _currentData.OwnedMicrochips.Remove(chipId);
    }

    /// <summary>
    /// Adds an augmentation to the player's owned augmentations.
    /// </summary>
    public void AddAugmentation(string augId)
    {
        _currentData.OwnedAugmentations.Add(augId);
    }

    /// <summary>
    /// Removes an augmentation from the player's owned augmentations.
    /// </summary>
    public bool RemoveAugmentation(string augId)
    {
        return _currentData.OwnedAugmentations.Remove(augId);
    }

    #endregion

    #region Currency Management

    /// <summary>
    /// Gets the player's current currency amount.
    /// </summary>
    public int Currency => _currentData.Currency;

    /// <summary>
    /// Adds currency to the player.
    /// </summary>
    public void AddCurrency(int amount)
    {
        _currentData.Currency = Math.Max(0, _currentData.Currency + amount);
    }

    /// <summary>
    /// Spends currency if the player has enough.
    /// </summary>
    /// <returns>True if successful, false if insufficient funds.</returns>
    public bool SpendCurrency(int amount)
    {
        if (_currentData.Currency >= amount)
        {
            _currentData.Currency -= amount;
            return true;
        }
        return false;
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Records a battle victory.
    /// </summary>
    public void RecordBattleWon()
    {
        _currentData.BattlesWon++;
    }

    /// <summary>
    /// Records fleeing from battle.
    /// </summary>
    public void RecordBattleFled()
    {
        _currentData.BattlesFled++;
    }

    /// <summary>
    /// Records recruiting a Stray.
    /// </summary>
    public void RecordStrayRecruited()
    {
        _currentData.TotalStraysRecruited++;
    }

    /// <summary>
    /// Marks a settlement as discovered.
    /// </summary>
    public void DiscoverSettlement(string settlementId)
    {
        _currentData.DiscoveredSettlements.Add(settlementId);
    }

    /// <summary>
    /// Checks if a settlement has been discovered.
    /// </summary>
    public bool IsSettlementDiscovered(string settlementId)
    {
        return _currentData.DiscoveredSettlements.Contains(settlementId);
    }

    /// <summary>
    /// Records sparing an enemy.
    /// </summary>
    public void RecordEnemySpared()
    {
        _currentData.EnemiesSpared++;
    }

    /// <summary>
    /// Records completing a bounty.
    /// </summary>
    public void RecordBountyCompleted()
    {
        _currentData.BountiesCompleted++;
    }

    /// <summary>
    /// Adjusts the player's morality score.
    /// </summary>
    /// <param name="amount">Positive for good deeds, negative for bad.</param>
    public void AddMorality(int amount)
    {
        _currentData.Morality = Math.Clamp(_currentData.Morality + amount, -100, 100);
    }

    #endregion

    #region Play Time Tracking

    /// <summary>
    /// Updates the total play time.
    /// </summary>
    public void UpdatePlayTime(GameTime gameTime)
    {
        _currentData.TotalPlayTimeSeconds += gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>
    /// Gets the total play time formatted as HH:MM:SS.
    /// </summary>
    public string GetFormattedPlayTime()
    {
        var time = TimeSpan.FromSeconds(_currentData.TotalPlayTimeSeconds);
        return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
    }

    #endregion
}

/// <summary>
/// Well-known story flag names used throughout the game.
/// </summary>
public static class StoryFlags
{
    // Tutorial / Act 1 Early
    public const string Awakened = "awakened";
    public const string MetCompanion = "met_companion";
    public const string FoundExoskeletonCrate = "found_exoskeleton_crate";
    public const string CompanionInstalledChip = "companion_installed_chip";
    public const string CompletedTutorialBattle = "completed_tutorial_battle";
    public const string RecruitedEchoPup = "recruited_echo_pup";
    public const string ExoskeletonPowered = "exoskeleton_powered";

    // Act 1 Major Beats
    public const string DiscoveredConversionFacility = "discovered_conversion_facility";
    public const string ReachedNimdokCore = "reached_nimdok_core";
    public const string PerformedMaintenance = "performed_maintenance";
    public const string LearnedBioShellTruth = "learned_bioshell_truth";
    public const string RequestedStrayFix = "requested_stray_fix";

    // Act 2 Major Beats
    public const string BoostControlActivated = "boost_control_activated";
    public const string CompletedDeadChannel = "completed_dead_channel";
    public const string DiscoveredQuietBuffer = "discovered_quiet_buffer";
    public const string LearnedChipIsAmplifier = "learned_chip_is_amplifier";
    public const string CompanionDeparted = "companion_departed";

    // Act 3 Major Beats
    public const string EnteredTheGlow = "entered_the_glow";
    public const string DefeatedHyperEvolvedBandit = "defeated_hyper_evolved_bandit";
    public const string LobotomizedNimdok = "lobotomized_nimdok";
    public const string ReturnedToPodField = "returned_to_pod_field";
    public const string GameComplete = "game_complete";

    // Optional Content
    public const string DefeatedPalisade = "defeated_palisade";
    public const string DefeatedLiminal = "defeated_liminal";
    public const string DefeatedCathedral = "defeated_cathedral";
    public const string FoundDiadem = "found_diadem";
    public const string FoundMarble = "found_marble";
    public const string AchievedAbsoluteSynchrony = "achieved_absolute_synchrony";
}

/// <summary>
/// Preview information about a save slot (displayed in save/load screens).
/// </summary>
public class SaveSlotInfo
{
    /// <summary>
    /// The save slot number.
    /// </summary>
    public int Slot { get; init; }

    /// <summary>
    /// Whether this is the auto-save slot.
    /// </summary>
    public bool IsAutoSave { get; init; }

    /// <summary>
    /// When the save was created/updated.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Total play time.
    /// </summary>
    public TimeSpan PlayTime { get; init; }

    /// <summary>
    /// Current story act.
    /// </summary>
    public ActState CurrentAct { get; init; }

    /// <summary>
    /// Current biome location.
    /// </summary>
    public BiomeType CurrentBiome { get; init; }

    /// <summary>
    /// Number of Strays in the party.
    /// </summary>
    public int PartyCount { get; init; }

    /// <summary>
    /// Total number of owned Strays.
    /// </summary>
    public int TotalStrays { get; init; }

    /// <summary>
    /// Number of completed quests.
    /// </summary>
    public int CompletedQuests { get; init; }

    /// <summary>
    /// Gets a display name for the slot.
    /// </summary>
    public string DisplayName => IsAutoSave ? "Auto Save" : $"Slot {Slot + 1}";

    /// <summary>
    /// Gets formatted play time as HH:MM:SS.
    /// </summary>
    public string FormattedPlayTime => $"{(int)PlayTime.TotalHours:D2}:{PlayTime.Minutes:D2}:{PlayTime.Seconds:D2}";

    /// <summary>
    /// Gets formatted timestamp.
    /// </summary>
    public string FormattedTimestamp => Timestamp == DateTime.MinValue ? "Unknown" : Timestamp.ToLocalTime().ToString("g");

    /// <summary>
    /// Gets a summary description for the save.
    /// </summary>
    public string Summary => $"{GetActDisplayName(CurrentAct)} - {CurrentBiome} | {PartyCount} Strays | {FormattedPlayTime}";

    private static string GetActDisplayName(ActState act) => act switch
    {
        ActState.Act1_Denial => "Act 1: Denial",
        ActState.Act2_Responsibility => "Act 2: Responsibility",
        ActState.Act3_Irreversibility => "Act 3: Irreversibility",
        _ => "Unknown"
    };
}

/// <summary>
/// Event arguments for companion departure.
/// </summary>
public class CompanionDepartureEventArgs : EventArgs
{
    /// <summary>
    /// The reason for departure.
    /// </summary>
    public string Reason { get; init; } = "";

    /// <summary>
    /// The Gravitation stage at the time of departure.
    /// </summary>
    public GravitationStage GravitationStage { get; init; }

    /// <summary>
    /// Total number of Gravitation uses.
    /// </summary>
    public int TotalUses { get; init; }
}
