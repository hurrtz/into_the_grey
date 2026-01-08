using System.Collections.Generic;
using System.Linq;

namespace Strays.Core.Game.Combat;

/// <summary>
/// Types of combat actions.
/// </summary>
public enum CombatActionType
{
    /// <summary>
    /// Basic physical attack.
    /// </summary>
    Attack,

    /// <summary>
    /// Defend to reduce incoming damage.
    /// </summary>
    Defend,

    /// <summary>
    /// Use a special ability.
    /// </summary>
    Ability,

    /// <summary>
    /// Use an item.
    /// </summary>
    Item,

    /// <summary>
    /// Attempt to flee from combat.
    /// </summary>
    Flee,

    /// <summary>
    /// Switch position with another party member.
    /// </summary>
    Switch,

    /// <summary>
    /// Companion's Gravitation ability.
    /// </summary>
    Gravitation
}

/// <summary>
/// Represents a combat action to be executed.
/// </summary>
public class CombatAction
{
    /// <summary>
    /// The type of action.
    /// </summary>
    public CombatActionType Type { get; set; }

    /// <summary>
    /// The combatant performing the action.
    /// </summary>
    public Combatant? Source { get; set; }

    /// <summary>
    /// The primary target of the action.
    /// </summary>
    public Combatant? Target { get; set; }

    /// <summary>
    /// All targets for multi-target abilities.
    /// </summary>
    public List<Combatant> Targets { get; set; } = new();

    /// <summary>
    /// ID of the ability being used (for Ability type).
    /// </summary>
    public string? AbilityId { get; set; }

    /// <summary>
    /// ID of the item being used (for Item type).
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>
    /// Priority modifier (higher = acts sooner).
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Creates an attack action.
    /// </summary>
    public static CombatAction Attack(Combatant source, Combatant target)
    {
        return new CombatAction
        {
            Type = CombatActionType.Attack,
            Source = source,
            Target = target,
            Targets = new List<Combatant> { target }
        };
    }

    /// <summary>
    /// Creates a defend action.
    /// </summary>
    public static CombatAction Defend(Combatant source)
    {
        return new CombatAction
        {
            Type = CombatActionType.Defend,
            Source = source,
            Target = source,
            Priority = 10 // Defend happens early
        };
    }

    /// <summary>
    /// Creates a flee action.
    /// </summary>
    public static CombatAction Flee(Combatant source)
    {
        return new CombatAction
        {
            Type = CombatActionType.Flee,
            Source = source,
            Priority = 100 // Flee happens first
        };
    }

    /// <summary>
    /// Creates a Gravitation action.
    /// </summary>
    public static CombatAction Gravitation(Combatant target, float damagePercent, bool fromCorruptedCompanion)
    {
        return new CombatAction
        {
            Type = CombatActionType.Gravitation,
            Target = target,
            Targets = new List<Combatant> { target },
            Priority = 50 // Gravitation interrupts
        };
    }

    /// <summary>
    /// Creates an ability action with a single target.
    /// </summary>
    public static CombatAction UseAbility(Combatant source, Ability ability, Combatant target)
    {
        return new CombatAction
        {
            Type = CombatActionType.Ability,
            Source = source,
            Target = target,
            Targets = new List<Combatant> { target },
            AbilityId = ability.Definition.Id,
            Priority = ability.Definition.Priority
        };
    }

    /// <summary>
    /// Creates an ability action with multiple targets.
    /// </summary>
    public static CombatAction UseAbility(Combatant source, Ability ability, List<Combatant> targets)
    {
        return new CombatAction
        {
            Type = CombatActionType.Ability,
            Source = source,
            Target = targets.FirstOrDefault(),
            Targets = targets,
            AbilityId = ability.Definition.Id,
            Priority = ability.Definition.Priority
        };
    }

    /// <summary>
    /// Creates an ability action with multiple targets (from IEnumerable).
    /// </summary>
    public static CombatAction UseAbility(Combatant source, Ability ability, IEnumerable<Combatant> targets)
    {
        var targetList = targets.ToList();
        return new CombatAction
        {
            Type = CombatActionType.Ability,
            Source = source,
            Target = targetList.FirstOrDefault(),
            Targets = targetList,
            AbilityId = ability.Definition.Id,
            Priority = ability.Definition.Priority
        };
    }
}

/// <summary>
/// Result of executing a combat action.
/// </summary>
public class CombatActionResult
{
    /// <summary>
    /// The action that was executed.
    /// </summary>
    public CombatAction Action { get; set; } = new();

    /// <summary>
    /// Whether the action was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Damage dealt by the action.
    /// </summary>
    public int DamageDealt { get; set; }

    /// <summary>
    /// Healing done by the action.
    /// </summary>
    public int HealingDone { get; set; }

    /// <summary>
    /// Whether the attack was a critical hit.
    /// </summary>
    public bool WasCritical { get; set; }

    /// <summary>
    /// Whether the attack missed.
    /// </summary>
    public bool Missed { get; set; }

    /// <summary>
    /// Whether the flee attempt succeeded.
    /// </summary>
    public bool FledSuccessfully { get; set; }

    /// <summary>
    /// Message describing the action result.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Whether a combatant was defeated by this action.
    /// </summary>
    public bool CausedDefeat { get; set; }

    /// <summary>
    /// The combatant that was defeated (if any).
    /// </summary>
    public Combatant? DefeatedCombatant { get; set; }
}
