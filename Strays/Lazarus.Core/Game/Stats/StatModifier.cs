using System;

namespace Lazarus.Core.Game.Stats;

/// <summary>
/// Source types for stat modifiers.
/// </summary>
public enum ModifierSource
{
    /// <summary>From an equipped augmentation.</summary>
    Augmentation,

    /// <summary>From an equipped microchip.</summary>
    Microchip,

    /// <summary>From a held item.</summary>
    Item,

    /// <summary>From a temporary buff/debuff.</summary>
    Buff,

    /// <summary>From evolution or level-up.</summary>
    Evolution,

    /// <summary>From a passive ability.</summary>
    Passive,

    /// <summary>From environmental effects.</summary>
    Environment,

    /// <summary>From faction reputation.</summary>
    Faction,

    /// <summary>From companion bonuses.</summary>
    Companion,

    /// <summary>Generic/other source.</summary>
    Other
}

/// <summary>
/// Represents a modifier to a stat value.
/// </summary>
public class StatModifier
{
    /// <summary>
    /// The stat being modified.
    /// </summary>
    public StatType Stat { get; init; }

    /// <summary>
    /// The modifier value. If IsPercent is true, this is a percentage bonus.
    /// </summary>
    public float Value { get; init; }

    /// <summary>
    /// If true, the value is a percentage modifier. If false, it's a flat bonus.
    /// </summary>
    public bool IsPercent { get; init; }

    /// <summary>
    /// Identifier for the source of this modifier (e.g., item ID, buff name).
    /// Used for removing modifiers when equipment is unequipped.
    /// </summary>
    public string Source { get; init; } = "";

    /// <summary>
    /// Type of source for categorization.
    /// </summary>
    public ModifierSource SourceType { get; init; } = ModifierSource.Other;

    /// <summary>
    /// Display name for the source (shown in UI).
    /// </summary>
    public string SourceName { get; init; } = "";

    /// <summary>
    /// Duration in seconds for temporary modifiers. -1 for permanent.
    /// </summary>
    public float Duration { get; set; } = -1f;

    /// <summary>
    /// Creates a flat stat modifier.
    /// </summary>
    public static StatModifier Flat(StatType stat, float value, string source, ModifierSource sourceType = ModifierSource.Other, string sourceName = "")
    {
        return new StatModifier
        {
            Stat = stat,
            Value = value,
            IsPercent = false,
            Source = source,
            SourceType = sourceType,
            SourceName = string.IsNullOrEmpty(sourceName) ? source : sourceName
        };
    }

    /// <summary>
    /// Creates a percentage stat modifier.
    /// </summary>
    public static StatModifier Percent(StatType stat, float value, string source, ModifierSource sourceType = ModifierSource.Other, string sourceName = "")
    {
        return new StatModifier
        {
            Stat = stat,
            Value = value,
            IsPercent = true,
            Source = source,
            SourceType = sourceType,
            SourceName = string.IsNullOrEmpty(sourceName) ? source : sourceName
        };
    }

    /// <summary>
    /// Creates a temporary buff modifier.
    /// </summary>
    public static StatModifier Buff(StatType stat, float value, bool isPercent, float duration, string source, string sourceName = "")
    {
        return new StatModifier
        {
            Stat = stat,
            Value = value,
            IsPercent = isPercent,
            Source = source,
            SourceType = ModifierSource.Buff,
            SourceName = string.IsNullOrEmpty(sourceName) ? source : sourceName,
            Duration = duration
        };
    }

    /// <summary>
    /// Gets a formatted string showing the modifier value.
    /// </summary>
    public string GetFormattedValue()
    {
        string sign = Value >= 0 ? "+" : "";

        if (IsPercent)
        {
            return $"{sign}{Value:F0}%";
        }

        // Format based on typical stat ranges
        if (Math.Abs(Value) < 10)
        {
            return $"{sign}{Value:F1}";
        }

        return $"{sign}{Value:F0}";
    }

    public override string ToString()
    {
        return $"{Stat}: {GetFormattedValue()} ({SourceName})";
    }
}
