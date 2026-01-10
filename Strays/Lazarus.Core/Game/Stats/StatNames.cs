using System;

namespace Lazarus.Core.Game.Stats;

/// <summary>
/// Provides display names and descriptions for stats.
/// </summary>
public static class StatNames
{
    /// <summary>
    /// Gets the short display name for a stat.
    /// </summary>
    public static string GetShortName(StatType stat) => stat switch
    {
        // Tempo
        StatType.Speed => "SPD",
        StatType.ATBStartPercent => "ATB%",
        StatType.ATBDelayResist => "DELAY RES",

        // Survival
        StatType.HPMax => "HP",
        StatType.BarrierMax => "BARRIER",
        StatType.BarrierRegen => "B.REGEN",

        // Energy
        StatType.ENMax => "EN",
        StatType.ENRegen => "E.REGEN",

        // Heat
        StatType.HeatCapacityMod => "HEAT CAP",
        StatType.HeatDissipationMod => "COOLING",
        StatType.OverheatRecoveryBonus => "OVH REC",

        // Combat
        StatType.MeleeAccuracy => "M.ACC",
        StatType.RangedAccuracy => "R.ACC",
        StatType.Evasion => "EVA",
        StatType.MeleeCritChance => "M.CRIT",
        StatType.RangedCritChance => "R.CRIT",
        StatType.CritSeverity => "CRIT DMG",
        StatType.Luck => "LUCK",
        StatType.Threat => "THREAT",

        // Physical ATK
        StatType.ATK_Impact => "ATK IMP",
        StatType.ATK_Piercing => "ATK PIE",
        StatType.ATK_Slashing => "ATK SLA",

        // Physical PEN
        StatType.PEN_Impact => "PEN IMP",
        StatType.PEN_Piercing => "PEN PIE",
        StatType.PEN_Slashing => "PEN SLA",

        // Physical MIT
        StatType.MIT_Impact => "MIT IMP",
        StatType.MIT_Piercing => "MIT PIE",
        StatType.MIT_Slashing => "MIT SLA",

        // Elemental ATK
        StatType.ATK_Thermal => "ATK THR",
        StatType.ATK_Cryo => "ATK CRY",
        StatType.ATK_Electric => "ATK ELE",
        StatType.ATK_Corrosive => "ATK COR",
        StatType.ATK_Toxic => "ATK TOX",
        StatType.ATK_Sonic => "ATK SON",
        StatType.ATK_Radiant => "ATK RAD",

        // Elemental PEN
        StatType.PEN_Thermal => "PEN THR",
        StatType.PEN_Cryo => "PEN CRY",
        StatType.PEN_Electric => "PEN ELE",
        StatType.PEN_Corrosive => "PEN COR",
        StatType.PEN_Toxic => "PEN TOX",
        StatType.PEN_Sonic => "PEN SON",
        StatType.PEN_Radiant => "PEN RAD",

        // Elemental MIT
        StatType.MIT_Thermal => "MIT THR",
        StatType.MIT_Cryo => "MIT CRY",
        StatType.MIT_Electric => "MIT ELE",
        StatType.MIT_Corrosive => "MIT COR",
        StatType.MIT_Toxic => "MIT TOX",
        StatType.MIT_Sonic => "MIT SON",
        StatType.MIT_Radiant => "MIT RAD",

        // Status Application
        StatType.BleedApplication => "BLEED APP",
        StatType.PoisonApplication => "POISON APP",
        StatType.BurnApplication => "BURN APP",
        StatType.FreezeApplication => "FREEZE APP",
        StatType.ShockApplication => "SHOCK APP",
        StatType.BlindApplication => "BLIND APP",

        // Status Resist
        StatType.BleedResist => "BLEED RES",
        StatType.PoisonResist => "POISON RES",
        StatType.BurnResist => "BURN RES",
        StatType.FreezeResist => "FREEZE RES",
        StatType.ShockResist => "SHOCK RES",
        StatType.BlindResist => "BLIND RES",

        _ => stat.ToString()
    };

    /// <summary>
    /// Gets the full display name for a stat.
    /// </summary>
    public static string GetFullName(StatType stat) => stat switch
    {
        // Tempo
        StatType.Speed => "Speed",
        StatType.ATBStartPercent => "ATB Start %",
        StatType.ATBDelayResist => "ATB Delay Resist",

        // Survival
        StatType.HPMax => "Max HP",
        StatType.BarrierMax => "Max Barrier",
        StatType.BarrierRegen => "Barrier Regen",

        // Energy
        StatType.ENMax => "Max Energy",
        StatType.ENRegen => "Energy Regen",

        // Heat
        StatType.HeatCapacityMod => "Heat Capacity",
        StatType.HeatDissipationMod => "Heat Dissipation",
        StatType.OverheatRecoveryBonus => "Overheat Recovery",

        // Combat
        StatType.MeleeAccuracy => "Melee Accuracy",
        StatType.RangedAccuracy => "Ranged Accuracy",
        StatType.Evasion => "Evasion",
        StatType.MeleeCritChance => "Melee Crit %",
        StatType.RangedCritChance => "Ranged Crit %",
        StatType.CritSeverity => "Crit Damage",
        StatType.Luck => "Luck",
        StatType.Threat => "Threat",

        // Physical
        StatType.ATK_Impact => "Impact ATK",
        StatType.ATK_Piercing => "Piercing ATK",
        StatType.ATK_Slashing => "Slashing ATK",
        StatType.PEN_Impact => "Impact Penetration",
        StatType.PEN_Piercing => "Piercing Penetration",
        StatType.PEN_Slashing => "Slashing Penetration",
        StatType.MIT_Impact => "Impact Mitigation",
        StatType.MIT_Piercing => "Piercing Mitigation",
        StatType.MIT_Slashing => "Slashing Mitigation",

        // Elemental
        StatType.ATK_Thermal => "Thermal ATK",
        StatType.ATK_Cryo => "Cryo ATK",
        StatType.ATK_Electric => "Electric ATK",
        StatType.ATK_Corrosive => "Corrosive ATK",
        StatType.ATK_Toxic => "Toxic ATK",
        StatType.ATK_Sonic => "Sonic ATK",
        StatType.ATK_Radiant => "Radiant ATK",
        StatType.PEN_Thermal => "Thermal Penetration",
        StatType.PEN_Cryo => "Cryo Penetration",
        StatType.PEN_Electric => "Electric Penetration",
        StatType.PEN_Corrosive => "Corrosive Penetration",
        StatType.PEN_Toxic => "Toxic Penetration",
        StatType.PEN_Sonic => "Sonic Penetration",
        StatType.PEN_Radiant => "Radiant Penetration",
        StatType.MIT_Thermal => "Thermal Mitigation",
        StatType.MIT_Cryo => "Cryo Mitigation",
        StatType.MIT_Electric => "Electric Mitigation",
        StatType.MIT_Corrosive => "Corrosive Mitigation",
        StatType.MIT_Toxic => "Toxic Mitigation",
        StatType.MIT_Sonic => "Sonic Mitigation",
        StatType.MIT_Radiant => "Radiant Mitigation",

        // Status
        StatType.BleedApplication => "Bleed Application",
        StatType.PoisonApplication => "Poison Application",
        StatType.BurnApplication => "Burn Application",
        StatType.FreezeApplication => "Freeze Application",
        StatType.ShockApplication => "Shock Application",
        StatType.BlindApplication => "Blind Application",
        StatType.BleedResist => "Bleed Resistance",
        StatType.PoisonResist => "Poison Resistance",
        StatType.BurnResist => "Burn Resistance",
        StatType.FreezeResist => "Freeze Resistance",
        StatType.ShockResist => "Shock Resistance",
        StatType.BlindResist => "Blind Resistance",

        _ => stat.ToString()
    };

    /// <summary>
    /// Gets a description for a stat.
    /// </summary>
    public static string GetDescription(StatType stat) => stat switch
    {
        StatType.Speed => "Determines how fast the ATB bar fills during combat.",
        StatType.ATBStartPercent => "Percentage of ATB filled at the start of battle.",
        StatType.ATBDelayResist => "Reduces the effect of slow and ATB-reducing abilities.",
        StatType.HPMax => "Maximum health points. Reaching 0 HP causes knockout.",
        StatType.BarrierMax => "Shield that absorbs damage before HP is affected.",
        StatType.BarrierRegen => "Amount of barrier restored each ATB tick.",
        StatType.ENMax => "Maximum energy for using microchip abilities.",
        StatType.ENRegen => "Amount of energy restored each ATB tick.",
        StatType.HeatCapacityMod => "Multiplier for microchip heat capacity.",
        StatType.HeatDissipationMod => "Multiplier for microchip cooling rate.",
        StatType.OverheatRecoveryBonus => "Bonus cooling when chips are overheated.",
        StatType.MeleeAccuracy => "Hit chance for melee attacks.",
        StatType.RangedAccuracy => "Hit chance for ranged attacks.",
        StatType.Evasion => "Chance to dodge incoming attacks.",
        StatType.MeleeCritChance => "Critical hit chance for melee attacks.",
        StatType.RangedCritChance => "Critical hit chance for ranged attacks.",
        StatType.CritSeverity => "Damage multiplier when landing a critical hit.",
        StatType.Luck => "Affects proc chances, rare drops, and lucky events.",
        StatType.Threat => "How much aggro this unit generates in combat.",
        _ => ""
    };

    /// <summary>
    /// Gets the display name for a stat category.
    /// </summary>
    public static string GetCategoryName(StatCategory category) => category switch
    {
        StatCategory.Tempo => "Tempo",
        StatCategory.Survival => "Survival",
        StatCategory.Energy => "Energy",
        StatCategory.Heat => "Heat Management",
        StatCategory.Combat => "Combat",
        StatCategory.PhysicalDamage => "Physical Damage",
        StatCategory.ElementalDamage => "Elemental Damage",
        StatCategory.StatusApplication => "Status Application",
        StatCategory.StatusResistance => "Status Resistance",
        _ => category.ToString()
    };

    /// <summary>
    /// Gets the format string for displaying a stat value.
    /// </summary>
    public static string FormatValue(StatType stat, float value)
    {
        // Percentage stats
        if (IsPercentStat(stat))
        {
            return $"{value:F0}%";
        }

        // Multiplier stats
        if (IsMultiplierStat(stat))
        {
            return $"x{value:F2}";
        }

        // Small decimal values
        if (value != 0 && Math.Abs(value) < 10)
        {
            return $"{value:F1}";
        }

        return $"{value:F0}";
    }

    /// <summary>
    /// Returns true if the stat is displayed as a percentage.
    /// </summary>
    public static bool IsPercentStat(StatType stat) => stat switch
    {
        StatType.ATBStartPercent or StatType.ATBDelayResist or
        StatType.MeleeAccuracy or StatType.RangedAccuracy or
        StatType.Evasion or StatType.MeleeCritChance or StatType.RangedCritChance or
        StatType.CritSeverity or
        StatType.BleedResist or StatType.PoisonResist or StatType.BurnResist or
        StatType.FreezeResist or StatType.ShockResist or StatType.BlindResist => true,
        _ => false
    };

    /// <summary>
    /// Returns true if the stat is a multiplier (displayed as x1.5 etc).
    /// </summary>
    public static bool IsMultiplierStat(StatType stat) => stat switch
    {
        StatType.HeatCapacityMod or StatType.HeatDissipationMod => true,
        _ => false
    };
}
