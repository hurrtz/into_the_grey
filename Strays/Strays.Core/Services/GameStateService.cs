using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Data;
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

    private GameSaveData _currentData;
    private readonly JsonSerializerOptions _jsonOptions;

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

    public GameStateService()
    {
        _currentData = new GameSaveData();
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
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

    /// <summary>
    /// Whether the companion is still with the party.
    /// </summary>
    public bool CompanionPresent
    {
        get => _currentData.CompanionPresent;
        set => _currentData.CompanionPresent = value;
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
            GravitationStage = GravitationStage.Normal
        };

        StateLoaded?.Invoke(this, _currentData);
    }

    /// <summary>
    /// Saves the current game state to a slot.
    /// </summary>
    public bool Save(int slot = 0)
    {
        try
        {
            _currentData.SaveSlot = slot;

            var savePath = GetSavePath(slot);
            var directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_currentData, _jsonOptions);
            File.WriteAllText(savePath, json);

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
    /// Checks if a save exists in a slot.
    /// </summary>
    public bool SaveExists(int slot = 0)
    {
        return File.Exists(GetSavePath(slot));
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

    private static string GetSavePath(int slot)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, SaveDirectory, string.Format(SaveFilePattern, slot));
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
