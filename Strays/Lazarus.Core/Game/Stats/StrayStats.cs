using System;
using System.Collections.Generic;
using System.Linq;

namespace Lazarus.Core.Game.Stats;

/// <summary>
/// Represents a Stray's complete stat profile including base stats and modifiers.
/// </summary>
public class StrayStats
{
    private readonly Dictionary<StatType, float> _baseStats = new();
    private readonly List<StatModifier> _modifiers = new();
    private readonly Dictionary<StatType, float> _cachedTotals = new();
    private bool _cacheValid = false;

    /// <summary>
    /// Creates a new stat container with default values.
    /// </summary>
    public StrayStats()
    {
        InitializeDefaults();
    }

    /// <summary>
    /// Creates a copy of another stat container.
    /// </summary>
    public StrayStats(StrayStats other)
    {
        foreach (var kvp in other._baseStats)
        {
            _baseStats[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Initializes all stats to their default values.
    /// </summary>
    private void InitializeDefaults()
    {
        // Tempo & Action Economy
        _baseStats[StatType.Speed] = 100f;
        _baseStats[StatType.ATBStartPercent] = 0f;
        _baseStats[StatType.ATBDelayResist] = 0f;

        // Core Survival
        _baseStats[StatType.HPMax] = 100f;
        _baseStats[StatType.BarrierMax] = 0f;
        _baseStats[StatType.BarrierRegen] = 0f;

        // Energy
        _baseStats[StatType.ENMax] = 100f;
        _baseStats[StatType.ENRegen] = 5f;

        // Heat Modifiers (multipliers default to 1.0)
        _baseStats[StatType.HeatCapacityMod] = 1f;
        _baseStats[StatType.HeatDissipationMod] = 1f;
        _baseStats[StatType.OverheatRecoveryBonus] = 0f;

        // Accuracy, Evasion, Crit, Luck
        _baseStats[StatType.MeleeAccuracy] = 90f;
        _baseStats[StatType.RangedAccuracy] = 85f;
        _baseStats[StatType.Evasion] = 5f;
        _baseStats[StatType.MeleeCritChance] = 5f;
        _baseStats[StatType.RangedCritChance] = 5f;
        _baseStats[StatType.CritSeverity] = 150f; // 150% damage
        _baseStats[StatType.Luck] = 0f;
        _baseStats[StatType.Threat] = 100f;

        // Physical Damage Types (default to 10 ATK, 0 PEN, 0 MIT)
        InitializeDamageType(StatType.ATK_Impact, StatType.PEN_Impact, StatType.MIT_Impact);
        InitializeDamageType(StatType.ATK_Piercing, StatType.PEN_Piercing, StatType.MIT_Piercing);
        InitializeDamageType(StatType.ATK_Slashing, StatType.PEN_Slashing, StatType.MIT_Slashing);

        // Elemental Damage Types
        InitializeDamageType(StatType.ATK_Thermal, StatType.PEN_Thermal, StatType.MIT_Thermal);
        InitializeDamageType(StatType.ATK_Cryo, StatType.PEN_Cryo, StatType.MIT_Cryo);
        InitializeDamageType(StatType.ATK_Electric, StatType.PEN_Electric, StatType.MIT_Electric);
        InitializeDamageType(StatType.ATK_Corrosive, StatType.PEN_Corrosive, StatType.MIT_Corrosive);
        InitializeDamageType(StatType.ATK_Toxic, StatType.PEN_Toxic, StatType.MIT_Toxic);
        InitializeDamageType(StatType.ATK_Sonic, StatType.PEN_Sonic, StatType.MIT_Sonic);
        InitializeDamageType(StatType.ATK_Radiant, StatType.PEN_Radiant, StatType.MIT_Radiant);

        // Status Application (default 0)
        _baseStats[StatType.BleedApplication] = 0f;
        _baseStats[StatType.PoisonApplication] = 0f;
        _baseStats[StatType.BurnApplication] = 0f;
        _baseStats[StatType.FreezeApplication] = 0f;
        _baseStats[StatType.ShockApplication] = 0f;
        _baseStats[StatType.BlindApplication] = 0f;

        // Status Resistance (default 0)
        _baseStats[StatType.BleedResist] = 0f;
        _baseStats[StatType.PoisonResist] = 0f;
        _baseStats[StatType.BurnResist] = 0f;
        _baseStats[StatType.FreezeResist] = 0f;
        _baseStats[StatType.ShockResist] = 0f;
        _baseStats[StatType.BlindResist] = 0f;
    }

    private void InitializeDamageType(StatType atk, StatType pen, StatType mit)
    {
        _baseStats[atk] = 10f;
        _baseStats[pen] = 0f;
        _baseStats[mit] = 0f;
    }

    /// <summary>
    /// Gets a base stat value (without modifiers).
    /// </summary>
    public float GetBase(StatType stat)
    {
        return _baseStats.TryGetValue(stat, out var value) ? value : 0f;
    }

    /// <summary>
    /// Sets a base stat value.
    /// </summary>
    public void SetBase(StatType stat, float value)
    {
        _baseStats[stat] = value;
        InvalidateCache();
    }

    /// <summary>
    /// Gets the total bonus from all modifiers for a stat.
    /// </summary>
    public float GetBonus(StatType stat)
    {
        float flatBonus = 0f;
        float percentBonus = 0f;

        foreach (var mod in _modifiers)
        {
            if (mod.Stat == stat)
            {
                if (mod.IsPercent)
                {
                    percentBonus += mod.Value;
                }
                else
                {
                    flatBonus += mod.Value;
                }
            }
        }

        float baseValue = GetBase(stat);
        return flatBonus + (baseValue * percentBonus / 100f);
    }

    /// <summary>
    /// Gets the final stat value (base + all modifiers).
    /// </summary>
    public float GetTotal(StatType stat)
    {
        if (_cacheValid && _cachedTotals.TryGetValue(stat, out var cached))
        {
            return cached;
        }

        float baseValue = GetBase(stat);
        float flatBonus = 0f;
        float percentBonus = 0f;

        foreach (var mod in _modifiers)
        {
            if (mod.Stat == stat)
            {
                if (mod.IsPercent)
                {
                    percentBonus += mod.Value;
                }
                else
                {
                    flatBonus += mod.Value;
                }
            }
        }

        float total = (baseValue + flatBonus) * (1f + percentBonus / 100f);

        // Apply minimum values for certain stats
        total = ApplyStatConstraints(stat, total);

        _cachedTotals[stat] = total;
        return total;
    }

    /// <summary>
    /// Applies minimum/maximum constraints to stat values.
    /// </summary>
    private float ApplyStatConstraints(StatType stat, float value)
    {
        return stat switch
        {
            // HP/EN can't go below 1
            StatType.HPMax or StatType.ENMax => Math.Max(1f, value),

            // Percentages generally capped at 0-100
            StatType.ATBStartPercent => Math.Clamp(value, 0f, 100f),
            StatType.MeleeAccuracy or StatType.RangedAccuracy => Math.Clamp(value, 5f, 100f),
            StatType.Evasion => Math.Clamp(value, 0f, 95f),
            StatType.MeleeCritChance or StatType.RangedCritChance => Math.Clamp(value, 0f, 100f),

            // Crit severity minimum 100%
            StatType.CritSeverity => Math.Max(100f, value),

            // Heat modifiers minimum 0.1
            StatType.HeatCapacityMod or StatType.HeatDissipationMod => Math.Max(0.1f, value),

            // Speed minimum 10
            StatType.Speed => Math.Max(10f, value),

            // Non-negative stats
            StatType.BarrierMax or StatType.BarrierRegen or StatType.ENRegen => Math.Max(0f, value),

            // Resistances cap at 90%
            StatType.BleedResist or StatType.PoisonResist or StatType.BurnResist or
            StatType.FreezeResist or StatType.ShockResist or StatType.BlindResist
                => Math.Clamp(value, 0f, 90f),

            // ATB Delay Resist caps at 90%
            StatType.ATBDelayResist => Math.Clamp(value, 0f, 90f),

            // Default: no constraints
            _ => value
        };
    }

    /// <summary>
    /// Adds a stat modifier.
    /// </summary>
    public void AddModifier(StatModifier modifier)
    {
        _modifiers.Add(modifier);
        InvalidateCache();
    }

    /// <summary>
    /// Removes a specific modifier.
    /// </summary>
    public void RemoveModifier(StatModifier modifier)
    {
        _modifiers.Remove(modifier);
        InvalidateCache();
    }

    /// <summary>
    /// Removes all modifiers from a specific source.
    /// </summary>
    public void RemoveModifiersFromSource(string source)
    {
        _modifiers.RemoveAll(m => m.Source == source);
        InvalidateCache();
    }

    /// <summary>
    /// Clears all modifiers.
    /// </summary>
    public void ClearModifiers()
    {
        _modifiers.Clear();
        InvalidateCache();
    }

    /// <summary>
    /// Gets all current modifiers.
    /// </summary>
    public IReadOnlyList<StatModifier> GetModifiers() => _modifiers.AsReadOnly();

    /// <summary>
    /// Gets all modifiers for a specific stat.
    /// </summary>
    public IEnumerable<StatModifier> GetModifiersForStat(StatType stat)
    {
        return _modifiers.Where(m => m.Stat == stat);
    }

    /// <summary>
    /// Invalidates the cached totals.
    /// </summary>
    private void InvalidateCache()
    {
        _cacheValid = false;
        _cachedTotals.Clear();
    }

    /// <summary>
    /// Gets all stats organized by category.
    /// </summary>
    public Dictionary<StatCategory, List<StatType>> GetStatsByCategory()
    {
        return new Dictionary<StatCategory, List<StatType>>
        {
            [StatCategory.Tempo] = new()
            {
                StatType.Speed, StatType.ATBStartPercent, StatType.ATBDelayResist
            },
            [StatCategory.Survival] = new()
            {
                StatType.HPMax, StatType.BarrierMax, StatType.BarrierRegen
            },
            [StatCategory.Energy] = new()
            {
                StatType.ENMax, StatType.ENRegen
            },
            [StatCategory.Heat] = new()
            {
                StatType.HeatCapacityMod, StatType.HeatDissipationMod, StatType.OverheatRecoveryBonus
            },
            [StatCategory.Combat] = new()
            {
                StatType.MeleeAccuracy, StatType.RangedAccuracy, StatType.Evasion,
                StatType.MeleeCritChance, StatType.RangedCritChance, StatType.CritSeverity,
                StatType.Luck, StatType.Threat
            },
            [StatCategory.PhysicalDamage] = new()
            {
                StatType.ATK_Impact, StatType.PEN_Impact, StatType.MIT_Impact,
                StatType.ATK_Piercing, StatType.PEN_Piercing, StatType.MIT_Piercing,
                StatType.ATK_Slashing, StatType.PEN_Slashing, StatType.MIT_Slashing
            },
            [StatCategory.ElementalDamage] = new()
            {
                StatType.ATK_Thermal, StatType.PEN_Thermal, StatType.MIT_Thermal,
                StatType.ATK_Cryo, StatType.PEN_Cryo, StatType.MIT_Cryo,
                StatType.ATK_Electric, StatType.PEN_Electric, StatType.MIT_Electric,
                StatType.ATK_Corrosive, StatType.PEN_Corrosive, StatType.MIT_Corrosive,
                StatType.ATK_Toxic, StatType.PEN_Toxic, StatType.MIT_Toxic,
                StatType.ATK_Sonic, StatType.PEN_Sonic, StatType.MIT_Sonic,
                StatType.ATK_Radiant, StatType.PEN_Radiant, StatType.MIT_Radiant
            },
            [StatCategory.StatusApplication] = new()
            {
                StatType.BleedApplication, StatType.PoisonApplication, StatType.BurnApplication,
                StatType.FreezeApplication, StatType.ShockApplication, StatType.BlindApplication
            },
            [StatCategory.StatusResistance] = new()
            {
                StatType.BleedResist, StatType.PoisonResist, StatType.BurnResist,
                StatType.FreezeResist, StatType.ShockResist, StatType.BlindResist
            }
        };
    }

    /// <summary>
    /// Gets the ATK stat for a damage type.
    /// </summary>
    public float GetAttack(DamageType type) => GetTotal(GetAtkStat(type));

    /// <summary>
    /// Gets the PEN stat for a damage type.
    /// </summary>
    public float GetPenetration(DamageType type) => GetTotal(GetPenStat(type));

    /// <summary>
    /// Gets the MIT stat for a damage type.
    /// </summary>
    public float GetMitigation(DamageType type) => GetTotal(GetMitStat(type));

    /// <summary>
    /// Gets the application stat for a status effect.
    /// </summary>
    public float GetStatusApplication(StatusEffectType status) => GetTotal(GetApplicationStat(status));

    /// <summary>
    /// Gets the resistance stat for a status effect.
    /// </summary>
    public float GetStatusResist(StatusEffectType status) => GetTotal(GetResistStat(status));

    private static StatType GetAtkStat(DamageType type) => type switch
    {
        DamageType.Impact => StatType.ATK_Impact,
        DamageType.Piercing => StatType.ATK_Piercing,
        DamageType.Slashing => StatType.ATK_Slashing,
        DamageType.Thermal => StatType.ATK_Thermal,
        DamageType.Cryo => StatType.ATK_Cryo,
        DamageType.Electric => StatType.ATK_Electric,
        DamageType.Corrosive => StatType.ATK_Corrosive,
        DamageType.Toxic => StatType.ATK_Toxic,
        DamageType.Sonic => StatType.ATK_Sonic,
        DamageType.Radiant => StatType.ATK_Radiant,
        _ => StatType.ATK_Impact
    };

    private static StatType GetPenStat(DamageType type) => type switch
    {
        DamageType.Impact => StatType.PEN_Impact,
        DamageType.Piercing => StatType.PEN_Piercing,
        DamageType.Slashing => StatType.PEN_Slashing,
        DamageType.Thermal => StatType.PEN_Thermal,
        DamageType.Cryo => StatType.PEN_Cryo,
        DamageType.Electric => StatType.PEN_Electric,
        DamageType.Corrosive => StatType.PEN_Corrosive,
        DamageType.Toxic => StatType.PEN_Toxic,
        DamageType.Sonic => StatType.PEN_Sonic,
        DamageType.Radiant => StatType.PEN_Radiant,
        _ => StatType.PEN_Impact
    };

    private static StatType GetMitStat(DamageType type) => type switch
    {
        DamageType.Impact => StatType.MIT_Impact,
        DamageType.Piercing => StatType.MIT_Piercing,
        DamageType.Slashing => StatType.MIT_Slashing,
        DamageType.Thermal => StatType.MIT_Thermal,
        DamageType.Cryo => StatType.MIT_Cryo,
        DamageType.Electric => StatType.MIT_Electric,
        DamageType.Corrosive => StatType.MIT_Corrosive,
        DamageType.Toxic => StatType.MIT_Toxic,
        DamageType.Sonic => StatType.MIT_Sonic,
        DamageType.Radiant => StatType.MIT_Radiant,
        _ => StatType.MIT_Impact
    };

    private static StatType GetApplicationStat(StatusEffectType status) => status switch
    {
        StatusEffectType.Bleed => StatType.BleedApplication,
        StatusEffectType.Poison => StatType.PoisonApplication,
        StatusEffectType.Burn => StatType.BurnApplication,
        StatusEffectType.Freeze => StatType.FreezeApplication,
        StatusEffectType.Shock => StatType.ShockApplication,
        StatusEffectType.Blind => StatType.BlindApplication,
        _ => StatType.BleedApplication
    };

    private static StatType GetResistStat(StatusEffectType status) => status switch
    {
        StatusEffectType.Bleed => StatType.BleedResist,
        StatusEffectType.Poison => StatType.PoisonResist,
        StatusEffectType.Burn => StatType.BurnResist,
        StatusEffectType.Freeze => StatType.FreezeResist,
        StatusEffectType.Shock => StatType.ShockResist,
        StatusEffectType.Blind => StatType.BlindResist,
        _ => StatType.BleedResist
    };
}
