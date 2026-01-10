using System;
using System.Collections.Generic;
using System.Linq;

namespace Lazarus.Core.Game.Combat;

/// <summary>
/// AI behavior patterns for enemies.
/// </summary>
public enum AIBehavior
{
    /// <summary>
    /// Randomly selects actions and targets.
    /// </summary>
    Random,

    /// <summary>
    /// Prioritizes attacking the weakest enemy.
    /// </summary>
    Aggressive,

    /// <summary>
    /// Focuses on self-preservation and healing.
    /// </summary>
    Defensive,

    /// <summary>
    /// Uses status effects and debuffs.
    /// </summary>
    Support,

    /// <summary>
    /// Balanced mix of offense and defense.
    /// </summary>
    Balanced,

    /// <summary>
    /// Focuses on the highest threat target.
    /// </summary>
    ThreatBased,

    /// <summary>
    /// Boss-specific complex behavior.
    /// </summary>
    Boss
}

/// <summary>
/// AI difficulty settings.
/// </summary>
public enum AIDifficulty
{
    /// <summary>
    /// Makes suboptimal choices, telegraphs attacks.
    /// </summary>
    Easy,

    /// <summary>
    /// Standard AI behavior.
    /// </summary>
    Normal,

    /// <summary>
    /// Makes smarter choices, exploits weaknesses.
    /// </summary>
    Hard,

    /// <summary>
    /// Ruthless optimization, no mercy.
    /// </summary>
    Nightmare
}

/// <summary>
/// Tracks threat levels for AI targeting.
/// </summary>
public class ThreatTable
{
    private readonly Dictionary<Combatant, int> _threatLevels = new();

    /// <summary>
    /// Gets the threat level for a combatant.
    /// </summary>
    public int GetThreat(Combatant combatant) =>
        _threatLevels.TryGetValue(combatant, out int threat) ? threat : 0;

    /// <summary>
    /// Adds threat from a combatant.
    /// </summary>
    public void AddThreat(Combatant combatant, int amount)
    {
        if (!_threatLevels.ContainsKey(combatant))
            _threatLevels[combatant] = 0;

        _threatLevels[combatant] += amount;
    }

    /// <summary>
    /// Gets the highest threat target.
    /// </summary>
    public Combatant? GetHighestThreat(IEnumerable<Combatant> validTargets)
    {
        Combatant? highest = null;
        int highestThreat = -1;

        foreach (var target in validTargets.Where(t => t.IsAlive))
        {
            int threat = GetThreat(target);
            if (threat > highestThreat)
            {
                highestThreat = threat;
                highest = target;
            }
        }

        return highest;
    }

    /// <summary>
    /// Decays all threat by a percentage.
    /// </summary>
    public void DecayThreat(float percent)
    {
        foreach (var key in _threatLevels.Keys.ToList())
        {
            _threatLevels[key] = (int)(_threatLevels[key] * (1f - percent));
        }
    }

    /// <summary>
    /// Clears all threat.
    /// </summary>
    public void Clear()
    {
        _threatLevels.Clear();
    }
}

/// <summary>
/// AI decision-making for combat.
/// </summary>
public class CombatAI
{
    private readonly Random _random = new();
    private readonly ThreatTable _threatTable = new();

    /// <summary>
    /// Current difficulty setting.
    /// </summary>
    public AIDifficulty Difficulty { get; set; } = AIDifficulty.Normal;

    /// <summary>
    /// The threat table for this combat.
    /// </summary>
    public ThreatTable ThreatTable => _threatTable;

    /// <summary>
    /// Selects an action for an AI-controlled combatant.
    /// </summary>
    public CombatAction SelectAction(
        Combatant actor,
        List<Combatant> allies,
        List<Combatant> enemies,
        AIBehavior behavior)
    {
        // Filter to alive combatants
        var aliveAllies = allies.Where(a => a.IsAlive).ToList();
        var aliveEnemies = enemies.Where(e => e.IsAlive).ToList();

        if (aliveEnemies.Count == 0)
            return CombatAction.Defend(actor);

        // Get available abilities
        var abilities = actor.Abilities.Where(a => a.IsReady).ToList();

        // Select action based on behavior
        return behavior switch
        {
            AIBehavior.Random => SelectRandomAction(actor, aliveAllies, aliveEnemies, abilities),
            AIBehavior.Aggressive => SelectAggressiveAction(actor, aliveAllies, aliveEnemies, abilities),
            AIBehavior.Defensive => SelectDefensiveAction(actor, aliveAllies, aliveEnemies, abilities),
            AIBehavior.Support => SelectSupportAction(actor, aliveAllies, aliveEnemies, abilities),
            AIBehavior.Balanced => SelectBalancedAction(actor, aliveAllies, aliveEnemies, abilities),
            AIBehavior.ThreatBased => SelectThreatBasedAction(actor, aliveAllies, aliveEnemies, abilities),
            AIBehavior.Boss => SelectBossAction(actor, aliveAllies, aliveEnemies, abilities),
            _ => SelectRandomAction(actor, aliveAllies, aliveEnemies, abilities)
        };
    }

    private CombatAction SelectRandomAction(
        Combatant actor,
        List<Combatant> allies,
        List<Combatant> enemies,
        List<Ability> abilities)
    {
        var target = enemies[_random.Next(enemies.Count)];

        // 70% basic attack, 30% use ability
        if (abilities.Count > 0 && _random.NextDouble() < 0.3)
        {
            var ability = abilities[_random.Next(abilities.Count)];
            return CreateAbilityAction(actor, ability, allies, enemies);
        }

        return CombatAction.Attack(actor, target);
    }

    private CombatAction SelectAggressiveAction(
        Combatant actor,
        List<Combatant> allies,
        List<Combatant> enemies,
        List<Ability> abilities)
    {
        // Find the weakest enemy (lowest HP percentage)
        var target = enemies
            .OrderBy(e => (float)e.CurrentHp / e.MaxHp)
            .First();

        // Prefer high-damage abilities
        var damageAbilities = abilities
            .Where(a => a.Definition.Category == AbilityCategory.Damage)
            .OrderByDescending(a => a.Definition.Power)
            .ToList();

        if (damageAbilities.Count > 0 && _random.NextDouble() < GetAbilityUseChance())
        {
            var ability = damageAbilities.First();
            return CreateAbilityAction(actor, ability, allies, enemies, target);
        }

        return CombatAction.Attack(actor, target);
    }

    private CombatAction SelectDefensiveAction(
        Combatant actor,
        List<Combatant> allies,
        List<Combatant> enemies,
        List<Ability> abilities)
    {
        // Check if we need healing
        float hpPercent = (float)actor.CurrentHp / actor.MaxHp;

        if (hpPercent < 0.3f)
        {
            // Try to heal self
            var healAbility = abilities
                .FirstOrDefault(a => a.Definition.Category == AbilityCategory.Healing &&
                                    (a.Definition.Target == AbilityTarget.Self ||
                                     a.Definition.Target == AbilityTarget.SingleAlly));

            if (healAbility != null)
            {
                return CreateAbilityAction(actor, healAbility, allies, enemies, actor);
            }

            // No heal available, defend
            if (_random.NextDouble() < 0.5)
            {
                return CombatAction.Defend(actor);
            }
        }

        // Check if allies need healing
        var woundedAlly = allies
            .Where(a => a != actor && (float)a.CurrentHp / a.MaxHp < 0.5f)
            .OrderBy(a => (float)a.CurrentHp / a.MaxHp)
            .FirstOrDefault();

        if (woundedAlly != null)
        {
            var healAbility = abilities
                .FirstOrDefault(a => a.Definition.Category == AbilityCategory.Healing &&
                                    a.Definition.Target == AbilityTarget.SingleAlly);

            if (healAbility != null)
            {
                return CreateAbilityAction(actor, healAbility, allies, enemies, woundedAlly);
            }
        }

        // If healthy, attack weakest enemy
        var target = enemies.OrderBy(e => e.CurrentHp).First();
        return CombatAction.Attack(actor, target);
    }

    private CombatAction SelectSupportAction(
        Combatant actor,
        List<Combatant> allies,
        List<Combatant> enemies,
        List<Ability> abilities)
    {
        // Prioritize buffs and debuffs
        var supportAbilities = abilities
            .Where(a => a.Definition.Category == AbilityCategory.Buff ||
                       a.Definition.Category == AbilityCategory.Debuff ||
                       a.Definition.Category == AbilityCategory.Status)
            .ToList();

        if (supportAbilities.Count > 0 && _random.NextDouble() < GetAbilityUseChance() + 0.2)
        {
            var ability = supportAbilities[_random.Next(supportAbilities.Count)];
            return CreateAbilityAction(actor, ability, allies, enemies);
        }

        // Fallback to basic attack
        var target = enemies[_random.Next(enemies.Count)];
        return CombatAction.Attack(actor, target);
    }

    private CombatAction SelectBalancedAction(
        Combatant actor,
        List<Combatant> allies,
        List<Combatant> enemies,
        List<Ability> abilities)
    {
        float hpPercent = (float)actor.CurrentHp / actor.MaxHp;

        // Low HP - be defensive
        if (hpPercent < 0.3f)
        {
            return SelectDefensiveAction(actor, allies, enemies, abilities);
        }

        // Check if any ally needs support
        bool allyNeedsHelp = allies.Any(a => (float)a.CurrentHp / a.MaxHp < 0.4f);
        if (allyNeedsHelp && _random.NextDouble() < 0.4)
        {
            return SelectDefensiveAction(actor, allies, enemies, abilities);
        }

        // Otherwise be aggressive
        return SelectAggressiveAction(actor, allies, enemies, abilities);
    }

    private CombatAction SelectThreatBasedAction(
        Combatant actor,
        List<Combatant> allies,
        List<Combatant> enemies,
        List<Ability> abilities)
    {
        // Target highest threat
        var target = _threatTable.GetHighestThreat(enemies) ?? enemies.First();

        var damageAbilities = abilities
            .Where(a => a.Definition.Category == AbilityCategory.Damage)
            .ToList();

        if (damageAbilities.Count > 0 && _random.NextDouble() < GetAbilityUseChance())
        {
            var ability = damageAbilities[_random.Next(damageAbilities.Count)];
            return CreateAbilityAction(actor, ability, allies, enemies, target);
        }

        return CombatAction.Attack(actor, target);
    }

    private CombatAction SelectBossAction(
        Combatant actor,
        List<Combatant> allies,
        List<Combatant> enemies,
        List<Ability> abilities)
    {
        float hpPercent = (float)actor.CurrentHp / actor.MaxHp;

        // Phase-based behavior
        if (hpPercent > 0.7f)
        {
            // Phase 1: Normal attacks with occasional abilities
            return SelectBalancedAction(actor, allies, enemies, abilities);
        }
        else if (hpPercent > 0.3f)
        {
            // Phase 2: More aggressive, use AOE abilities
            var aoeAbilities = abilities
                .Where(a => a.Definition.Target == AbilityTarget.AllEnemies)
                .ToList();

            if (aoeAbilities.Count > 0 && _random.NextDouble() < 0.6)
            {
                var ability = aoeAbilities[_random.Next(aoeAbilities.Count)];
                return CreateAbilityAction(actor, ability, allies, enemies);
            }

            return SelectAggressiveAction(actor, allies, enemies, abilities);
        }
        else
        {
            // Phase 3: Desperate - use strongest abilities
            var strongestAbility = abilities
                .Where(a => a.Definition.Category == AbilityCategory.Damage)
                .OrderByDescending(a => a.Definition.Power)
                .FirstOrDefault();

            if (strongestAbility != null)
            {
                return CreateAbilityAction(actor, strongestAbility, allies, enemies);
            }

            // Target lowest HP enemy for the kill
            var target = enemies.OrderBy(e => e.CurrentHp).First();
            return CombatAction.Attack(actor, target);
        }
    }

    private CombatAction CreateAbilityAction(
        Combatant actor,
        Ability ability,
        List<Combatant> allies,
        List<Combatant> enemies,
        Combatant? preferredTarget = null)
    {
        var action = new CombatAction
        {
            Type = CombatActionType.Ability,
            Source = actor,
            AbilityId = ability.Definition.Id,
            Priority = ability.Definition.Priority
        };

        // Determine targets based on ability target type
        switch (ability.Definition.Target)
        {
            case AbilityTarget.Self:
                action.Target = actor;
                action.Targets = new List<Combatant> { actor };
                break;

            case AbilityTarget.SingleEnemy:
                var enemyTarget = preferredTarget ?? enemies[_random.Next(enemies.Count)];
                action.Target = enemyTarget;
                action.Targets = new List<Combatant> { enemyTarget };
                break;

            case AbilityTarget.AllEnemies:
                action.Target = enemies.First();
                action.Targets = enemies.ToList();
                break;

            case AbilityTarget.SingleAlly:
                var allyTarget = preferredTarget ?? allies[_random.Next(allies.Count)];
                action.Target = allyTarget;
                action.Targets = new List<Combatant> { allyTarget };
                break;

            case AbilityTarget.AllAllies:
                action.Target = allies.First();
                action.Targets = allies.ToList();
                break;

            case AbilityTarget.RandomEnemy:
                var randomTarget = enemies[_random.Next(enemies.Count)];
                action.Target = randomTarget;
                action.Targets = new List<Combatant> { randomTarget };
                break;

            case AbilityTarget.All:
                action.Targets = allies.Concat(enemies).ToList();
                action.Target = action.Targets.First();
                break;
        }

        return action;
    }

    private float GetAbilityUseChance()
    {
        return Difficulty switch
        {
            AIDifficulty.Easy => 0.2f,
            AIDifficulty.Normal => 0.4f,
            AIDifficulty.Hard => 0.6f,
            AIDifficulty.Nightmare => 0.8f,
            _ => 0.4f
        };
    }

    /// <summary>
    /// Records damage dealt for threat tracking.
    /// </summary>
    public void RecordDamage(Combatant source, Combatant target, int damage)
    {
        _threatTable.AddThreat(source, damage);
    }

    /// <summary>
    /// Records healing done for threat tracking.
    /// </summary>
    public void RecordHealing(Combatant source, int healing)
    {
        _threatTable.AddThreat(source, healing / 2);
    }

    /// <summary>
    /// Called at the end of each turn to decay threat.
    /// </summary>
    public void EndTurn()
    {
        _threatTable.DecayThreat(0.1f);
    }

    /// <summary>
    /// Resets AI state for a new battle.
    /// </summary>
    public void Reset()
    {
        _threatTable.Clear();
    }

    /// <summary>
    /// Gets AI behavior for a Kyn based on its creature category.
    /// </summary>
    public static AIBehavior GetBehaviorForCategory(Data.CreatureCategory category)
    {
        return category switch
        {
            // Aggressive hunters
            Data.CreatureCategory.Predatoria => AIBehavior.Aggressive,
            Data.CreatureCategory.Exoskeletalis => AIBehavior.Aggressive,

            // Balanced fighters
            Data.CreatureCategory.Manipularis => AIBehavior.Balanced,
            Data.CreatureCategory.Marsupialis => AIBehavior.Balanced,

            // Support roles - Jellyfish use CC/AoE
            Data.CreatureCategory.Medusalia => AIBehavior.Support,

            // Defensive tanks - Big chassis and armored
            Data.CreatureCategory.Colossomammalia => AIBehavior.Defensive,
            Data.CreatureCategory.Armormammalia => AIBehavior.Defensive,

            // Threat-based/tactical - Stealth and smart creatures
            Data.CreatureCategory.Octomorpha => AIBehavior.ThreatBased,
            Data.CreatureCategory.Mollusca => AIBehavior.ThreatBased,

            // Unpredictable - Small/fast creatures
            Data.CreatureCategory.Micromammalia => AIBehavior.Random,

            // Fun categories - unique behaviors
            Data.CreatureCategory.Obscura => AIBehavior.ThreatBased, // Platypus - weird tech
            Data.CreatureCategory.Tardigrada => AIBehavior.Defensive, // Sloth - zen tank

            _ => AIBehavior.Balanced
        };
    }
}
