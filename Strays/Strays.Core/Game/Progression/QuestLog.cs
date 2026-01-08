using System;
using System.Collections.Generic;
using System.Linq;
using Strays.Core.Services;

namespace Strays.Core.Game.Progression;

/// <summary>
/// Manages the player's active and completed quests.
/// </summary>
public class QuestLog
{
    private readonly Dictionary<string, Quest> _quests = new();
    private readonly GameStateService _gameState;

    /// <summary>
    /// All quests in the log.
    /// </summary>
    public IEnumerable<Quest> AllQuests => _quests.Values;

    /// <summary>
    /// Active quests.
    /// </summary>
    public IEnumerable<Quest> ActiveQuests =>
        _quests.Values.Where(q => q.State == QuestState.Active);

    /// <summary>
    /// Completed quests.
    /// </summary>
    public IEnumerable<Quest> CompletedQuests =>
        _quests.Values.Where(q => q.State == QuestState.Completed);

    /// <summary>
    /// Available (but not started) quests.
    /// </summary>
    public IEnumerable<Quest> AvailableQuests =>
        _quests.Values.Where(q => q.State == QuestState.Available);

    /// <summary>
    /// Main story quests.
    /// </summary>
    public IEnumerable<Quest> MainQuests =>
        _quests.Values.Where(q => q.Definition.Type == QuestType.Main);

    /// <summary>
    /// Side quests.
    /// </summary>
    public IEnumerable<Quest> SideQuests =>
        _quests.Values.Where(q => q.Definition.Type == QuestType.Side);

    /// <summary>
    /// The currently tracked/highlighted quest.
    /// </summary>
    public Quest? TrackedQuest { get; private set; }

    /// <summary>
    /// Event fired when a quest is started.
    /// </summary>
    public event EventHandler<Quest>? QuestStarted;

    /// <summary>
    /// Event fired when a quest is completed.
    /// </summary>
    public event EventHandler<Quest>? QuestCompleted;

    /// <summary>
    /// Event fired when quest progress is updated.
    /// </summary>
    public event EventHandler<Quest>? QuestProgressUpdated;

    public QuestLog(GameStateService gameState)
    {
        _gameState = gameState;
        InitializeQuests();
    }

    /// <summary>
    /// Initializes all quest instances from definitions.
    /// </summary>
    private void InitializeQuests()
    {
        foreach (var definition in QuestDefinitions.All.Values)
        {
            var quest = new Quest(definition);
            quest.StateChanged += OnQuestStateChanged;
            quest.ObjectiveCompleted += OnObjectiveCompleted;
            _quests[definition.Id] = quest;
        }

        // Check which quests are already completed (from save)
        foreach (var completedId in _gameState.Data.CompletedQuestIds)
        {
            if (_quests.TryGetValue(completedId, out var quest))
            {
                // Mark as completed without firing events
                quest.Start();
                quest.Complete();
            }
        }

        // Check which quests are active (from save)
        foreach (var activeId in _gameState.Data.ActiveQuestIds)
        {
            if (_quests.TryGetValue(activeId, out var quest))
            {
                quest.MakeAvailable();
                quest.Start();
            }
        }

        // Update availability for remaining quests
        UpdateQuestAvailability();
    }

    /// <summary>
    /// Updates which quests are available based on current progress.
    /// </summary>
    public void UpdateQuestAvailability()
    {
        var completedIds = new HashSet<string>(
            _quests.Values
                .Where(q => q.State == QuestState.Completed)
                .Select(q => q.Id)
        );

        var storyFlags = _gameState.Data.StoryFlags;

        foreach (var quest in _quests.Values)
        {
            if (quest.State == QuestState.Locked)
            {
                if (quest.CanStart(completedIds, storyFlags))
                {
                    quest.MakeAvailable();
                }
            }
        }
    }

    /// <summary>
    /// Gets a quest by ID.
    /// </summary>
    public Quest? GetQuest(string id)
    {
        return _quests.TryGetValue(id, out var quest) ? quest : null;
    }

    /// <summary>
    /// Starts a quest by ID.
    /// </summary>
    public bool StartQuest(string id)
    {
        var quest = GetQuest(id);
        if (quest == null)
            return false;

        if (quest.State != QuestState.Available && quest.State != QuestState.Locked)
            return false;

        // Force available if still locked but prerequisites met
        var completedIds = new HashSet<string>(CompletedQuests.Select(q => q.Id));
        if (quest.State == QuestState.Locked && quest.CanStart(completedIds, _gameState.Data.StoryFlags))
        {
            quest.MakeAvailable();
        }

        if (quest.State != QuestState.Available)
            return false;

        quest.Start();
        _gameState.Data.ActiveQuestIds.Add(id);

        // Auto-track main quests
        if (quest.Definition.Type == QuestType.Main && TrackedQuest == null)
        {
            TrackedQuest = quest;
        }

        QuestStarted?.Invoke(this, quest);
        return true;
    }

    /// <summary>
    /// Completes a quest by ID.
    /// </summary>
    public bool CompleteQuest(string id)
    {
        var quest = GetQuest(id);
        if (quest == null || quest.State != QuestState.Active)
            return false;

        quest.Complete();
        return true;
    }

    /// <summary>
    /// Updates quest progress based on a game event.
    /// </summary>
    /// <param name="type">Type of objective.</param>
    /// <param name="targetId">Target ID.</param>
    /// <param name="count">Count to add.</param>
    public void NotifyProgress(ObjectiveType type, string targetId, int count = 1)
    {
        foreach (var quest in ActiveQuests)
        {
            if (quest.UpdateProgress(type, targetId, count))
            {
                QuestProgressUpdated?.Invoke(this, quest);
            }
        }
    }

    /// <summary>
    /// Notifies that a flag has been triggered.
    /// </summary>
    public void NotifyFlag(string flagId)
    {
        NotifyProgress(ObjectiveType.TriggerFlag, flagId);
        UpdateQuestAvailability();
    }

    /// <summary>
    /// Notifies that the player talked to an NPC.
    /// </summary>
    public void NotifyTalkedTo(string npcId)
    {
        NotifyProgress(ObjectiveType.TalkTo, npcId);
    }

    /// <summary>
    /// Notifies that the player reached a location.
    /// </summary>
    public void NotifyReachedLocation(string locationId)
    {
        NotifyProgress(ObjectiveType.ReachLocation, locationId);
    }

    /// <summary>
    /// Notifies that the player defeated an encounter.
    /// </summary>
    public void NotifyDefeatedEncounter(string encounterId)
    {
        NotifyProgress(ObjectiveType.DefeatEncounter, encounterId);
    }

    /// <summary>
    /// Notifies that the player collected an item.
    /// </summary>
    public void NotifyCollected(string itemId, int count = 1)
    {
        NotifyProgress(ObjectiveType.Collect, itemId, count);
    }

    /// <summary>
    /// Notifies that the player recruited a Stray.
    /// </summary>
    public void NotifyRecruitedStray(string strayId)
    {
        NotifyProgress(ObjectiveType.RecruitStray, strayId);
    }

    /// <summary>
    /// Notifies that the player interacted with something.
    /// </summary>
    public void NotifyInteracted(string objectId)
    {
        NotifyProgress(ObjectiveType.Interact, objectId);
    }

    /// <summary>
    /// Sets the tracked quest.
    /// </summary>
    public void TrackQuest(string? id)
    {
        if (id == null)
        {
            TrackedQuest = null;
            return;
        }

        var quest = GetQuest(id);
        if (quest != null && quest.State == QuestState.Active)
        {
            TrackedQuest = quest;
        }
    }

    private void OnQuestStateChanged(object? sender, QuestState state)
    {
        if (sender is not Quest quest)
            return;

        switch (state)
        {
            case QuestState.Completed:
                _gameState.Data.ActiveQuestIds.Remove(quest.Id);
                _gameState.Data.CompletedQuestIds.Add(quest.Id);

                // Apply rewards
                ApplyRewards(quest);

                // Set completion flags
                foreach (var flag in quest.Definition.SetsFlags)
                {
                    _gameState.SetFlag(flag);
                }

                // Update next quest in chain
                if (!string.IsNullOrEmpty(quest.Definition.NextQuestId))
                {
                    var nextQuest = GetQuest(quest.Definition.NextQuestId);
                    nextQuest?.MakeAvailable();
                }

                // Update tracked quest
                if (TrackedQuest == quest)
                {
                    TrackedQuest = ActiveQuests.FirstOrDefault(q => q.Definition.Type == QuestType.Main)
                        ?? ActiveQuests.FirstOrDefault();
                }

                QuestCompleted?.Invoke(this, quest);
                UpdateQuestAvailability();
                break;
        }
    }

    private void OnObjectiveCompleted(object? sender, ObjectiveProgress progress)
    {
        if (sender is Quest quest)
        {
            QuestProgressUpdated?.Invoke(this, quest);
        }
    }

    private void ApplyRewards(Quest quest)
    {
        var reward = quest.Definition.Reward;
        if (reward == null)
            return;

        // Add experience to party
        if (reward.Experience > 0)
        {
            // Experience is handled by StrayRoster elsewhere
        }

        // Add currency
        if (reward.Currency > 0)
        {
            _gameState.Data.Currency += reward.Currency;
        }

        // Add items
        foreach (var itemId in reward.ItemIds)
        {
            // Item system will handle this
            _gameState.SetFlag($"item_{itemId}");
        }

        // Unlock Strays
        foreach (var strayId in reward.UnlockedStrayIds)
        {
            _gameState.SetFlag($"stray_unlocked_{strayId}");
        }

        // Set reputation
        foreach (var (factionId, amount) in reward.ReputationChanges)
        {
            // Faction system will handle this
            _gameState.SetFlag($"reputation_{factionId}_{(amount > 0 ? "up" : "down")}");
        }
    }

    /// <summary>
    /// Gets a summary of quest progress for display.
    /// </summary>
    public string GetProgressSummary()
    {
        var active = ActiveQuests.Count();
        var completed = CompletedQuests.Count();
        var total = _quests.Count;

        return $"Quests: {completed}/{total} complete, {active} active";
    }
}
