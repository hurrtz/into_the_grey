namespace Lazarus.Core.Game.Stats;

/// <summary>
/// All stat types available for Strays.
/// </summary>
public enum StatType
{
    // ═══════════════════════════════════════════════════════════════
    // TEMPO & ACTION ECONOMY (3 stats)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>ATB fill rate - how fast the action bar fills.</summary>
    Speed,

    /// <summary>How full ATB is at battle start (0-100%).</summary>
    ATBStartPercent,

    /// <summary>Resistance to Slow and ATB reduction effects.</summary>
    ATBDelayResist,

    // ═══════════════════════════════════════════════════════════════
    // CORE SURVIVAL (3 stats)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Maximum health points.</summary>
    HPMax,

    /// <summary>Shield HP pool that absorbs damage before HP.</summary>
    BarrierMax,

    /// <summary>Shield restored per ATB tick.</summary>
    BarrierRegen,

    // ═══════════════════════════════════════════════════════════════
    // ENERGY (2 stats)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Maximum energy for chip usage.</summary>
    ENMax,

    /// <summary>Energy restored per ATB tick.</summary>
    ENRegen,

    // ═══════════════════════════════════════════════════════════════
    // HEAT MODIFIERS (3 stats)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Multiplies chip heat ceiling.</summary>
    HeatCapacityMod,

    /// <summary>Multiplies chip cooling rate.</summary>
    HeatDissipationMod,

    /// <summary>Extra cooling while chip is >100% heat.</summary>
    OverheatRecoveryBonus,

    // ═══════════════════════════════════════════════════════════════
    // ACCURACY, EVASION, CRIT, LUCK (8 stats)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Hit chance for melee attacks.</summary>
    MeleeAccuracy,

    /// <summary>Hit chance for ranged attacks.</summary>
    RangedAccuracy,

    /// <summary>Chance to avoid incoming attacks.</summary>
    Evasion,

    /// <summary>Critical hit chance for melee attacks.</summary>
    MeleeCritChance,

    /// <summary>Critical hit chance for ranged attacks.</summary>
    RangedCritChance,

    /// <summary>Critical damage multiplier.</summary>
    CritSeverity,

    /// <summary>Affects procs, lucky saves, rare events.</summary>
    Luck,

    /// <summary>Aggro generation modifier.</summary>
    Threat,

    // ═══════════════════════════════════════════════════════════════
    // PHYSICAL DAMAGE TYPES (3 types × 3 = 9 stats)
    // ═══════════════════════════════════════════════════════════════

    // Impact (Blunt force, crushing, ballistic)
    /// <summary>Impact attack power.</summary>
    ATK_Impact,
    /// <summary>Impact armor penetration.</summary>
    PEN_Impact,
    /// <summary>Impact damage mitigation.</summary>
    MIT_Impact,

    // Piercing (Puncture, stabbing)
    /// <summary>Piercing attack power.</summary>
    ATK_Piercing,
    /// <summary>Piercing armor penetration.</summary>
    PEN_Piercing,
    /// <summary>Piercing damage mitigation.</summary>
    MIT_Piercing,

    // Slashing (Cuts, claws, blades)
    /// <summary>Slashing attack power.</summary>
    ATK_Slashing,
    /// <summary>Slashing armor penetration.</summary>
    PEN_Slashing,
    /// <summary>Slashing damage mitigation.</summary>
    MIT_Slashing,

    // ═══════════════════════════════════════════════════════════════
    // ELEMENTAL DAMAGE TYPES (7 types × 3 = 21 stats)
    // ═══════════════════════════════════════════════════════════════

    // Thermal (Fire, heat)
    /// <summary>Thermal attack power.</summary>
    ATK_Thermal,
    /// <summary>Thermal armor penetration.</summary>
    PEN_Thermal,
    /// <summary>Thermal damage mitigation.</summary>
    MIT_Thermal,

    // Cryo (Ice, cold)
    /// <summary>Cryo attack power.</summary>
    ATK_Cryo,
    /// <summary>Cryo armor penetration.</summary>
    PEN_Cryo,
    /// <summary>Cryo damage mitigation.</summary>
    MIT_Cryo,

    // Electric (Shock, lightning)
    /// <summary>Electric attack power.</summary>
    ATK_Electric,
    /// <summary>Electric armor penetration.</summary>
    PEN_Electric,
    /// <summary>Electric damage mitigation.</summary>
    MIT_Electric,

    // Corrosive (Acid, armor melt)
    /// <summary>Corrosive attack power.</summary>
    ATK_Corrosive,
    /// <summary>Corrosive armor penetration.</summary>
    PEN_Corrosive,
    /// <summary>Corrosive damage mitigation.</summary>
    MIT_Corrosive,

    // Toxic (Poison, bio damage)
    /// <summary>Toxic attack power.</summary>
    ATK_Toxic,
    /// <summary>Toxic armor penetration.</summary>
    PEN_Toxic,
    /// <summary>Toxic damage mitigation.</summary>
    MIT_Toxic,

    // Sonic (Sound, stagger)
    /// <summary>Sonic attack power.</summary>
    ATK_Sonic,
    /// <summary>Sonic armor penetration.</summary>
    PEN_Sonic,
    /// <summary>Sonic damage mitigation.</summary>
    MIT_Sonic,

    // Radiant (Laser, beam, light)
    /// <summary>Radiant attack power.</summary>
    ATK_Radiant,
    /// <summary>Radiant armor penetration.</summary>
    PEN_Radiant,
    /// <summary>Radiant damage mitigation.</summary>
    MIT_Radiant,

    // ═══════════════════════════════════════════════════════════════
    // STATUS APPLICATION (6 stats)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Chance/power to apply Bleed (physical trauma DoT).</summary>
    BleedApplication,

    /// <summary>Chance/power to apply Poison (bio DoT).</summary>
    PoisonApplication,

    /// <summary>Chance/power to apply Burn (thermal DoT).</summary>
    BurnApplication,

    /// <summary>Chance/power to apply Freeze (slow, shatter vulnerability).</summary>
    FreezeApplication,

    /// <summary>Chance/power to apply Shock (stun, action delay).</summary>
    ShockApplication,

    /// <summary>Chance/power to apply Blind (accuracy debuff).</summary>
    BlindApplication,

    // ═══════════════════════════════════════════════════════════════
    // STATUS RESISTANCE (6 stats)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Resistance to Bleed status.</summary>
    BleedResist,

    /// <summary>Resistance to Poison status.</summary>
    PoisonResist,

    /// <summary>Resistance to Burn status.</summary>
    BurnResist,

    /// <summary>Resistance to Freeze status.</summary>
    FreezeResist,

    /// <summary>Resistance to Shock status.</summary>
    ShockResist,

    /// <summary>Resistance to Blind status.</summary>
    BlindResist
}

/// <summary>
/// Damage type categories.
/// </summary>
public enum DamageType
{
    // Physical
    Impact,
    Piercing,
    Slashing,

    // Elemental
    Thermal,
    Cryo,
    Electric,
    Corrosive,
    Toxic,
    Sonic,
    Radiant
}

/// <summary>
/// Status effect types.
/// </summary>
public enum StatusEffectType
{
    Bleed,
    Poison,
    Burn,
    Freeze,
    Shock,
    Blind
}

/// <summary>
/// Stat categories for UI grouping.
/// </summary>
public enum StatCategory
{
    Tempo,
    Survival,
    Energy,
    Heat,
    Combat,
    PhysicalDamage,
    ElementalDamage,
    StatusApplication,
    StatusResistance
}
