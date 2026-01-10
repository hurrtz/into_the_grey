using System;
using System.Collections.Generic;
using System.Linq;

namespace Lazarus.Core.Game.Progression;

/// <summary>
/// The current state of a quest.
/// </summary>
public enum QuestState
{
    /// <summary>
    /// Quest is available but not yet started.
    /// </summary>
    Available,

    /// <summary>
    /// Quest is active and in progress.
    /// </summary>
    Active,

    /// <summary>
    /// Quest has been completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Quest has been failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Quest is not yet available (prerequisites not met).
    /// </summary>
    Locked
}

/// <summary>
/// Tracks progress on a single objective within a quest.
/// </summary>
public class ObjectiveProgress
{
    /// <summary>
    /// The objective being tracked.
    /// </summary>
    public QuestObjective Objective { get; }

    /// <summary>
    /// Whether this objective is completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Current count for countable objectives.
    /// </summary>
    public int CurrentCount { get; private set; }

    /// <summary>
    /// Whether this objective is optional.
    /// </summary>
    public bool IsOptional => Objective.IsOptional;

    public ObjectiveProgress(QuestObjective objective)
    {
        Objective = objective;
        IsCompleted = false;
        CurrentCount = 0;
    }

    /// <summary>
    /// Increments the count for countable objectives.
    /// </summary>
    public void IncrementCount(int amount = 1)
    {
        CurrentCount += amount;
        if (CurrentCount >= Objective.RequiredCount)
        {
            IsCompleted = true;
        }
    }

    /// <summary>
    /// Marks the objective as completed.
    /// </summary>
    public void Complete()
    {
        IsCompleted = true;
        CurrentCount = Objective.RequiredCount;
    }

    /// <summary>
    /// Resets the objective progress.
    /// </summary>
    public void Reset()
    {
        IsCompleted = false;
        CurrentCount = 0;
    }
}

/// <summary>
/// A runtime instance of a quest tracking player progress.
/// </summary>
public class Quest
{
    /// <summary>
    /// The quest definition this instance is based on.
    /// </summary>
    public QuestDefinition Definition { get; }

    /// <summary>
    /// Unique ID of this quest.
    /// </summary>
    public string Id => Definition.Id;

    /// <summary>
    /// Display name of this quest.
    /// </summary>
    public string Name => Definition.Name;

    /// <summary>
    /// Current state of the quest.
    /// </summary>
    public QuestState State { get; private set; }

    /// <summary>
    /// Progress on each objective.
    /// </summary>
    public List<ObjectiveProgress> Objectives { get; }

    /// <summary>
    /// Index of the current active objective (for sequential quests).
    /// </summary>
    public int CurrentObjectiveIndex { get; private set; }

    /// <summary>
    /// When this quest was started.
    /// </summary>
    public DateTime? StartedAt { get; private set; }

    /// <summary>
    /// When this quest was completed.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Whether all required objectives are completed.
    /// </summary>
    public bool AllRequiredObjectivesComplete =>
        Objectives.Where(o => !o.IsOptional).All(o => o.IsCompleted);

    /// <summary>
    /// Whether all objectives (including optional) are completed.
    /// </summary>
    public bool AllObjectivesComplete =>
        Objectives.All(o => o.IsCompleted);

    /// <summary>
    /// The current active objective (for sequential quests).
    /// </summary>
    public ObjectiveProgress? CurrentObjective =>
        CurrentObjectiveIndex < Objectives.Count ? Objectives[CurrentObjectiveIndex] : null;

    /// <summary>
    /// Event fired when quest state changes.
    /// </summary>
    public event EventHandler<QuestState>? StateChanged;

    /// <summary>
    /// Event fired when an objective is completed.
    /// </summary>
    public event EventHandler<ObjectiveProgress>? ObjectiveCompleted;

    public Quest(QuestDefinition definition)
    {
        Definition = definition;
        State = QuestState.Locked;
        Objectives = definition.Objectives.Select(o => new ObjectiveProgress(o)).ToList();
        CurrentObjectiveIndex = 0;
    }

    /// <summary>
    /// Checks if this quest can be started.
    /// </summary>
    /// <param name="completedQuests">Set of completed quest IDs.</param>
    /// <param name="storyFlags">Dictionary of story flags (flag name -> is set).</param>
    public bool CanStart(HashSet<string> completedQuests, Dictionary<string, bool> storyFlags)
    {
        // Check prerequisite quests
        foreach (var prereq in Definition.Prerequisites)
        {
            if (!completedQuests.Contains(prereq))
                return false;
        }

        // Check required flags
        foreach (var flag in Definition.RequiredFlags)
        {
            if (!storyFlags.TryGetValue(flag, out var isSet) || !isSet)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Starts the quest.
    /// </summary>
    public void Start()
    {
        if (State != QuestState.Locked && State != QuestState.Available)
            return;

        State = QuestState.Active;
        StartedAt = DateTime.Now;
        CurrentObjectiveIndex = 0;
        StateChanged?.Invoke(this, State);
    }

    /// <summary>
    /// Updates objective progress based on an event.
    /// </summary>
    /// <param name="type">Type of objective to update.</param>
    /// <param name="targetId">Target ID (NPC, location, item, etc.).</param>
    /// <param name="count">Count to add.</param>
    /// <returns>True if any objective was updated.</returns>
    public bool UpdateProgress(ObjectiveType type, string targetId, int count = 1)
    {
        if (State != QuestState.Active)
            return false;

        bool updated = false;

        // Check all matching objectives
        foreach (var progress in Objectives)
        {
            if (progress.IsCompleted)
                continue;

            if (progress.Objective.Type != type)
                continue;

            if (!string.IsNullOrEmpty(progress.Objective.TargetId) &&
                progress.Objective.TargetId != targetId)
                continue;

            // Update progress
            progress.IncrementCount(count);
            updated = true;

            if (progress.IsCompleted)
            {
                ObjectiveCompleted?.Invoke(this, progress);

                // Advance to next objective if sequential
                if (CurrentObjectiveIndex < Objectives.Count - 1)
                {
                    CurrentObjectiveIndex++;
                }
            }
        }

        // Check for quest completion
        if (updated && AllRequiredObjectivesComplete)
        {
            Complete();
        }

        return updated;
    }

    /// <summary>
    /// Completes a specific objective by index.
    /// </summary>
    public void CompleteObjective(int index)
    {
        if (index < 0 || index >= Objectives.Count)
            return;

        var progress = Objectives[index];
        if (!progress.IsCompleted)
        {
            progress.Complete();
            ObjectiveCompleted?.Invoke(this, progress);

            if (AllRequiredObjectivesComplete)
            {
                Complete();
            }
        }
    }

    /// <summary>
    /// Completes the quest.
    /// </summary>
    public void Complete()
    {
        if (State != QuestState.Active)
            return;

        State = QuestState.Completed;
        CompletedAt = DateTime.Now;
        StateChanged?.Invoke(this, State);
    }

    /// <summary>
    /// Fails the quest.
    /// </summary>
    public void Fail()
    {
        if (State != QuestState.Active)
            return;

        State = QuestState.Failed;
        StateChanged?.Invoke(this, State);
    }

    /// <summary>
    /// Makes the quest available to start.
    /// </summary>
    public void MakeAvailable()
    {
        if (State == QuestState.Locked)
        {
            State = QuestState.Available;
            StateChanged?.Invoke(this, State);
        }
    }

    /// <summary>
    /// Gets a description of current progress.
    /// </summary>
    public string GetProgressDescription()
    {
        if (State == QuestState.Completed)
            return "Completed";

        if (State == QuestState.Failed)
            return "Failed";

        if (State != QuestState.Active)
            return "Not started";

        var current = CurrentObjective;
        if (current == null)
            return "All objectives complete";

        var obj = current.Objective;
        if (obj.RequiredCount > 1)
        {
            return $"{obj.Description} ({current.CurrentCount}/{obj.RequiredCount})";
        }

        return obj.Description;
    }
}
