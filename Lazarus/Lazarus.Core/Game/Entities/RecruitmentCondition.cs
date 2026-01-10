using System;
using System.Collections.Generic;
using System.Linq;
using Lazarus.Core.Services;

namespace Lazarus.Core.Game.Entities;

/// <summary>
/// Types of recruitment conditions.
/// </summary>
public enum RecruitmentConditionType
{
    /// <summary>
    /// No special condition - can always attempt.
    /// </summary>
    None,

    /// <summary>
    /// Must have a specific story flag.
    /// </summary>
    RequiresFlag,

    /// <summary>
    /// Must have completed a specific quest.
    /// </summary>
    RequiresQuest,

    /// <summary>
    /// Must have a specific item.
    /// </summary>
    RequiresItem,

    /// <summary>
    /// Must have high enough reputation with a faction.
    /// </summary>
    RequiresReputation,

    /// <summary>
    /// Must have a party member of a specific type.
    /// </summary>
    RequiresPartyMember,

    /// <summary>
    /// Protagonist must have the exoskeleton powered.
    /// </summary>
    RequiresExoskeleton,

    /// <summary>
    /// Companion must be at a certain Gravitation stage.
    /// </summary>
    RequiresGravitationStage,

    /// <summary>
    /// Must be at a specific act in the story.
    /// </summary>
    RequiresAct,

    /// <summary>
    /// Must have spared a certain number of enemies.
    /// </summary>
    SparedEnemies,

    /// <summary>
    /// Must have completed a certain number of bounties.
    /// </summary>
    CompletedBounty,

    /// <summary>
    /// Must have a high enough morality level.
    /// </summary>
    HasHighMorality
}

/// <summary>
/// A condition that must be met to recruit a Kyn.
/// </summary>
public class RecruitmentCondition
{
    /// <summary>
    /// Type of condition.
    /// </summary>
    public RecruitmentConditionType Type { get; init; } = RecruitmentConditionType.None;

    /// <summary>
    /// Target ID (flag, quest, item, faction, etc.).
    /// </summary>
    public string? TargetId { get; init; }

    /// <summary>
    /// Required value (for reputation, gravitation stage, act).
    /// </summary>
    public int RequiredValue { get; init; }

    /// <summary>
    /// Message to display if condition is not met.
    /// </summary>
    public string? FailureMessage { get; init; }

    /// <summary>
    /// Checks if this condition is met.
    /// </summary>
    public bool IsMet(GameStateService gameState, KynRoster roster)
    {
        return Type switch
        {
            RecruitmentConditionType.None => true,
            RecruitmentConditionType.RequiresFlag => !string.IsNullOrEmpty(TargetId) && gameState.HasFlag(TargetId),
            RecruitmentConditionType.RequiresQuest => !string.IsNullOrEmpty(TargetId) && gameState.IsQuestCompleted(TargetId),
            RecruitmentConditionType.RequiresItem => !string.IsNullOrEmpty(TargetId) && gameState.HasFlag($"item_{TargetId}"),
            RecruitmentConditionType.RequiresReputation => CheckReputation(gameState),
            RecruitmentConditionType.RequiresPartyMember => CheckPartyMember(roster),
            RecruitmentConditionType.RequiresExoskeleton => gameState.HasExoskeleton && gameState.ExoskeletonPowered,
            RecruitmentConditionType.RequiresGravitationStage => (int)gameState.Data.GravitationStage >= RequiredValue,
            RecruitmentConditionType.RequiresAct => (int)gameState.CurrentAct >= RequiredValue,
            RecruitmentConditionType.SparedEnemies => gameState.Data.EnemiesSpared >= RequiredValue,
            RecruitmentConditionType.CompletedBounty => gameState.Data.BountiesCompleted >= RequiredValue,
            RecruitmentConditionType.HasHighMorality => gameState.Data.Morality >= RequiredValue,
            _ => true
        };
    }

    private bool CheckReputation(GameStateService gameState)
    {
        // Simplified reputation check via flags
        if (string.IsNullOrEmpty(TargetId))
            return true;

        // Check for reputation flag like "reputation_salvagers_high"
        return gameState.HasFlag($"reputation_{TargetId}_high") ||
               (RequiredValue <= 50 && gameState.HasFlag($"reputation_{TargetId}_medium"));
    }

    private bool CheckPartyMember(KynRoster roster)
    {
        if (string.IsNullOrEmpty(TargetId))
            return true;

        foreach (var kyn in roster.Party)
        {
            if (kyn.Definition.Id == TargetId ||
                kyn.Definition.CreatureType.ToString() == TargetId ||
                kyn.Definition.Category.ToString() == TargetId)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a simple flag requirement.
    /// </summary>
    public static RecruitmentCondition Flag(string flagId, string? failureMessage = null)
    {
        return new RecruitmentCondition
        {
            Type = RecruitmentConditionType.RequiresFlag,
            TargetId = flagId,
            FailureMessage = failureMessage ?? "Special conditions not met."
        };
    }

    /// <summary>
    /// Creates a quest completion requirement.
    /// </summary>
    public static RecruitmentCondition Quest(string questId, string? failureMessage = null)
    {
        return new RecruitmentCondition
        {
            Type = RecruitmentConditionType.RequiresQuest,
            TargetId = questId,
            FailureMessage = failureMessage ?? "You must complete a certain quest first."
        };
    }

    /// <summary>
    /// Creates a reputation requirement.
    /// </summary>
    public static RecruitmentCondition Reputation(string factionId, int minReputation, string? failureMessage = null)
    {
        return new RecruitmentCondition
        {
            Type = RecruitmentConditionType.RequiresReputation,
            TargetId = factionId,
            RequiredValue = minReputation,
            FailureMessage = failureMessage ?? $"You need better standing with {factionId}."
        };
    }

    /// <summary>
    /// Creates a requirement for sparing a number of enemies.
    /// </summary>
    public static RecruitmentCondition Spared(int count, string? failureMessage = null)
    {
        return new RecruitmentCondition
        {
            Type = RecruitmentConditionType.SparedEnemies,
            RequiredValue = count,
            FailureMessage = failureMessage ?? $"You haven't shown enough mercy. (Spared: {count})"
        };
    }

    /// <summary>
    /// Creates a requirement for completing a number of bounties.
    /// </summary>
    public static RecruitmentCondition Bounty(int count, string? failureMessage = null)
    {
        return new RecruitmentCondition
        {
            Type = RecruitmentConditionType.CompletedBounty,
            RequiredValue = count,
            FailureMessage = failureMessage ?? $"You haven't proven your strength. (Bounties: {count})"
        };
    }

    /// <summary>
    /// Creates a requirement for a minimum morality level.
    /// </summary>
    public static RecruitmentCondition Morality(int minMorality, string? failureMessage = null)
    {
        return new RecruitmentCondition
        {
            Type = RecruitmentConditionType.HasHighMorality,
            RequiredValue = minMorality,
            FailureMessage = failureMessage ?? "Your heart isn't in the right place."
        };
    }
}

/// <summary>
/// Result of a recruitment attempt.
/// </summary>
public enum RecruitmentResult
{
    /// <summary>
    /// Recruitment was successful.
    /// </summary>
    Success,

    /// <summary>
    /// The Kyn refused to join.
    /// </summary>
    Refused,

    /// <summary>
    /// Conditions were not met.
    /// </summary>
    ConditionsNotMet,

    /// <summary>
    /// Party is full.
    /// </summary>
    PartyFull,

    /// <summary>
    /// This Kyn cannot be recruited.
    /// </summary>
    NotRecruitaible,

    /// <summary>
    /// The Kyn fled before recruitment could be attempted.
    /// </summary>
    Fled
}

/// <summary>
/// Manages the recruitment process for Kyns.
/// </summary>
public class RecruitmentManager
{
    private readonly GameStateService _gameState;
    private readonly KynRoster _roster;
    private readonly Random _random = new();

    public RecruitmentManager(GameStateService gameState, KynRoster roster)
    {
        _gameState = gameState;
        _roster = roster;
    }

    /// <summary>
    /// Checks if a Kyn can be recruited.
    /// </summary>
    public bool CanAttemptRecruitment(Kyn kyn, out string? failureReason)
    {
        failureReason = null;

        // Check if Kyn is recruitaible
        if (!kyn.Definition.CanRecruit)
        {
            failureReason = "This Kyn cannot be recruited.";
            return false;
        }

        // Check party space
        if (_roster.Party.Count >= KynRoster.MaxPartySize && _roster.Storage.Count >= KynRoster.MaxStorageSize)
        {
            failureReason = "No room for more Kyns!";
            return false;
        }

        // Check conditions
        var condition = kyn.Definition.RecruitCondition;
        if (condition != null && !condition.IsMet(_gameState, _roster))
        {
            failureReason = condition.FailureMessage ?? "Recruitment conditions not met.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to recruit a Kyn.
    /// </summary>
    public RecruitmentResult AttemptRecruitment(Kyn kyn, out string message)
    {
        message = "";

        if (!CanAttemptRecruitment(kyn, out var failureReason))
        {
            message = failureReason ?? "Cannot recruit this Kyn.";
            return failureReason?.Contains("room") == true ? RecruitmentResult.PartyFull :
                   failureReason?.Contains("cannot be recruited") == true ? RecruitmentResult.NotRecruitaible :
                   RecruitmentResult.ConditionsNotMet;
        }

        // Calculate recruitment chance
        float baseChance = kyn.Definition.RecruitChance;

        // Modify by Kyn's remaining HP (lower HP = easier to recruit)
        float hpModifier = 1f + (1f - (float)kyn.CurrentHp / kyn.MaxHp) * 0.5f;

        // Modify by level difference
        int avgPartyLevel = _roster.Party.Count > 0
            ? (int)_roster.Party.Average(s => s.Level)
            : 1;
        float levelModifier = avgPartyLevel >= kyn.Level ? 1.2f : 0.8f;

        float finalChance = baseChance * hpModifier * levelModifier;

        // Roll for recruitment
        if (_random.NextDouble() < finalChance)
        {
            // Success! Revive and heal the Kyn before adding to party
            if (!kyn.IsAlive)
            {
                kyn.Revive(1.0f); // Revive at full HP
            }
            else
            {
                kyn.FullHeal(); // Heal to full HP
            }

            _roster.AddKyn(kyn);

            message = $"{kyn.DisplayName} joined your party!";
            _gameState.SetFlag($"recruited_{kyn.Definition.Id}");

            return RecruitmentResult.Success;
        }
        else
        {
            // Failed
            message = GetRefusalMessage(kyn);
            return RecruitmentResult.Refused;
        }
    }

    private string GetRefusalMessage(Kyn kyn)
    {
        var messages = new[]
        {
            $"{kyn.DisplayName} isn't interested...",
            $"{kyn.DisplayName} backs away warily.",
            $"{kyn.DisplayName} shakes its head.",
            $"{kyn.DisplayName} doesn't trust you yet.",
            $"{kyn.DisplayName} growls and retreats.",
            $"{kyn.DisplayName} looks at you suspiciously.",
            $"{kyn.DisplayName} isn't ready to join you."
        };

        return messages[_random.Next(messages.Length)];
    }

    /// <summary>
    /// Gets dialog text for recruitment attempt.
    /// </summary>
    public string GetRecruitmentPrompt(Kyn kyn)
    {
        return $"{kyn.DisplayName} seems interested in joining you. Attempt recruitment?";
    }
}
