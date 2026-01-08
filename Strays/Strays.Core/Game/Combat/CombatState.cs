using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Data;
using Strays.Core.Game.Entities;
using Strays.Core.Game.World;

namespace Strays.Core.Game.Combat;

/// <summary>
/// The current phase of combat.
/// </summary>
public enum CombatPhase
{
    /// <summary>
    /// Combat is starting, setting up combatants.
    /// </summary>
    Starting,

    /// <summary>
    /// ATB gauges are filling, waiting for actions.
    /// </summary>
    Running,

    /// <summary>
    /// Player is selecting an action for a ready combatant.
    /// </summary>
    SelectingAction,

    /// <summary>
    /// Player is selecting an ability from the ability menu.
    /// </summary>
    SelectingAbility,

    /// <summary>
    /// Player is selecting a target for the chosen action.
    /// </summary>
    SelectingTarget,

    /// <summary>
    /// An action is being executed.
    /// </summary>
    ExecutingAction,

    /// <summary>
    /// Combat has ended in victory.
    /// </summary>
    Victory,

    /// <summary>
    /// Combat has ended in defeat.
    /// </summary>
    Defeat,

    /// <summary>
    /// Player fled from combat.
    /// </summary>
    Fled
}

/// <summary>
/// Manages the state and logic of turn-based ATB combat.
/// </summary>
public class CombatState
{
    private readonly Random _random = new();

    /// <summary>
    /// Current combat phase.
    /// </summary>
    public CombatPhase Phase { get; private set; } = CombatPhase.Starting;

    /// <summary>
    /// All player party combatants.
    /// </summary>
    public List<Combatant> Party { get; } = new();

    /// <summary>
    /// All enemy combatants.
    /// </summary>
    public List<Combatant> Enemies { get; } = new();

    /// <summary>
    /// All combatants (party + enemies).
    /// </summary>
    public IEnumerable<Combatant> AllCombatants => Party.Concat(Enemies);

    /// <summary>
    /// The combatant currently selecting an action.
    /// </summary>
    public Combatant? ActiveCombatant { get; private set; }

    /// <summary>
    /// The combatant currently being targeted.
    /// </summary>
    public Combatant? TargetedCombatant { get; set; }

    /// <summary>
    /// Index of the currently selected target.
    /// </summary>
    public int TargetIndex { get; set; } = 0;

    /// <summary>
    /// Index of the currently selected action.
    /// </summary>
    public int ActionIndex { get; set; } = 0;

    /// <summary>
    /// Index of the currently selected ability.
    /// </summary>
    public int AbilityIndex { get; set; } = 0;

    /// <summary>
    /// The currently selected ability.
    /// </summary>
    public Ability? SelectedAbility { get; set; }

    /// <summary>
    /// The combat AI for enemy actions.
    /// </summary>
    public CombatAI CombatAI { get; } = new();

    /// <summary>
    /// The encounter that triggered this combat.
    /// </summary>
    public Encounter? SourceEncounter { get; set; }

    /// <summary>
    /// Results of actions executed this combat.
    /// </summary>
    public List<CombatActionResult> ActionHistory { get; } = new();

    /// <summary>
    /// The most recent action result.
    /// </summary>
    public CombatActionResult? LastResult { get; private set; }

    /// <summary>
    /// Total experience earned from this combat.
    /// </summary>
    public int ExperienceEarned { get; private set; } = 0;

    /// <summary>
    /// Stray that can be recruited after victory (if any).
    /// </summary>
    public Stray? RecruitableStray { get; private set; }

    /// <summary>
    /// Timer for displaying action results.
    /// </summary>
    public float ResultDisplayTimer { get; private set; } = 0;

    /// <summary>
    /// Event fired when an action is executed.
    /// </summary>
    public event EventHandler<CombatActionResult>? ActionExecuted;

    /// <summary>
    /// Event fired when combat ends.
    /// </summary>
    public event EventHandler<CombatPhase>? CombatEnded;

    /// <summary>
    /// Initializes combat with the party and enemies.
    /// </summary>
    /// <param name="partyStrays">Player's party Strays.</param>
    /// <param name="enemyStrays">Enemy Strays.</param>
    /// <param name="encounter">The encounter that triggered combat.</param>
    public void Initialize(IEnumerable<Stray> partyStrays, IEnumerable<Stray> enemyStrays, Encounter? encounter = null)
    {
        Party.Clear();
        Enemies.Clear();
        ActionHistory.Clear();
        SourceEncounter = encounter;
        ExperienceEarned = 0;
        RecruitableStray = null;

        // Create party combatants
        float partyX = 150;
        float partyYStart = 150;
        float partyYSpacing = 70;

        int i = 0;
        foreach (var stray in partyStrays.Where(s => s.IsAlive))
        {
            var combatant = new Combatant(stray, isEnemy: false)
            {
                Position = new Vector2(partyX, partyYStart + i * partyYSpacing)
            };
            Party.Add(combatant);
            i++;
        }

        // Create enemy combatants
        float enemyX = 650;
        float enemyYStart = 150;
        float enemyYSpacing = 70;

        i = 0;
        foreach (var stray in enemyStrays)
        {
            var combatant = new Combatant(stray, isEnemy: true)
            {
                Position = new Vector2(enemyX, enemyYStart + i * enemyYSpacing)
            };
            Enemies.Add(combatant);
            i++;
        }

        Phase = CombatPhase.Running;
    }

    /// <summary>
    /// Updates the combat state.
    /// </summary>
    /// <param name="gameTime">Current game time.</param>
    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update result display timer
        if (ResultDisplayTimer > 0)
        {
            ResultDisplayTimer -= deltaTime;
            if (ResultDisplayTimer <= 0)
            {
                LastResult = null;
            }
        }

        switch (Phase)
        {
            case CombatPhase.Running:
                UpdateAtbGauges(deltaTime);
                CheckForReadyCombatants();
                ProcessEnemyActions();
                CheckCombatEnd();
                break;

            case CombatPhase.ExecutingAction:
                // Wait for action animation/display
                if (ResultDisplayTimer <= 0)
                {
                    Phase = CombatPhase.Running;
                    CheckCombatEnd();
                }
                break;
        }
    }

    /// <summary>
    /// Updates all ATB gauges.
    /// </summary>
    private void UpdateAtbGauges(float deltaTime)
    {
        foreach (var combatant in AllCombatants)
        {
            combatant.UpdateAtb(deltaTime);
        }
    }

    /// <summary>
    /// Checks for combatants that are ready to act.
    /// </summary>
    private void CheckForReadyCombatants()
    {
        // Find the first ready party member that needs input
        var readyPartyMember = Party.FirstOrDefault(c => c.IsReady && c.SelectedAction == null);
        if (readyPartyMember != null)
        {
            ActiveCombatant = readyPartyMember;
            Phase = CombatPhase.SelectingAction;
            ActionIndex = 0;
        }
    }

    /// <summary>
    /// Processes AI actions for enemies.
    /// </summary>
    private void ProcessEnemyActions()
    {
        foreach (var enemy in Enemies.Where(e => e.IsReady && e.SelectedAction == null))
        {
            // Get AI behavior based on Stray type
            var behavior = CombatAI.GetBehaviorForStray(enemy.Stray.Definition.Type);

            // Use combat AI to select action
            var action = CombatAI.SelectAction(
                enemy,
                Enemies.ToList(),
                Party.ToList(),
                behavior
            );

            ExecuteAction(action);
        }
    }

    /// <summary>
    /// Checks if combat has ended.
    /// </summary>
    private void CheckCombatEnd()
    {
        if (!Party.Any(p => p.IsAlive))
        {
            Phase = CombatPhase.Defeat;
            CombatEnded?.Invoke(this, Phase);
        }
        else if (!Enemies.Any(e => e.IsAlive))
        {
            // Calculate experience
            ExperienceEarned = Enemies.Sum(e => e.Stray.Level * 20);

            // Check for recruitable Stray
            if (SourceEncounter?.CanRecruit == true && Enemies.Count > 0)
            {
                var defeated = Enemies[_random.Next(Enemies.Count)];
                if (defeated.Stray.Definition.CanRecruit)
                {
                    RecruitableStray = defeated.Stray;
                }
            }

            Phase = CombatPhase.Victory;
            CombatEnded?.Invoke(this, Phase);
        }
    }

    /// <summary>
    /// Selects an action for the active combatant.
    /// </summary>
    /// <param name="actionType">The type of action to perform.</param>
    public void SelectAction(CombatActionType actionType)
    {
        if (ActiveCombatant == null)
            return;

        switch (actionType)
        {
            case CombatActionType.Attack:
                Phase = CombatPhase.SelectingTarget;
                SelectedAbility = null;
                // Default to first living enemy
                TargetIndex = Enemies.FindIndex(e => e.IsAlive);
                if (TargetIndex >= 0)
                    TargetedCombatant = Enemies[TargetIndex];
                break;

            case CombatActionType.Ability:
                Phase = CombatPhase.SelectingAbility;
                AbilityIndex = 0;
                break;

            case CombatActionType.Defend:
                var defendAction = CombatAction.Defend(ActiveCombatant);
                ExecuteAction(defendAction);
                break;

            case CombatActionType.Flee:
                var fleeAction = CombatAction.Flee(ActiveCombatant);
                ExecuteAction(fleeAction);
                break;
        }
    }

    /// <summary>
    /// Gets the available abilities for the active combatant.
    /// </summary>
    public List<Ability> GetAvailableAbilities()
    {
        if (ActiveCombatant == null)
            return new List<Ability>();

        return ActiveCombatant.Abilities.ToList();
    }

    /// <summary>
    /// Selects an ability and moves to target selection.
    /// </summary>
    public void SelectAbility(int index)
    {
        if (ActiveCombatant == null)
            return;

        var abilities = GetAvailableAbilities();
        if (index < 0 || index >= abilities.Count)
            return;

        var ability = abilities[index];

        // Check if ability is usable
        if (!ability.IsReady || ability.Definition.EnergyCost > ActiveCombatant.CurrentEnergy)
            return;

        SelectedAbility = ability;

        // Determine if we need target selection
        var targetType = ability.Definition.Target;
        switch (targetType)
        {
            case AbilityTarget.Self:
                // Execute immediately on self
                var selfAction = CombatAction.UseAbility(ActiveCombatant, ability, ActiveCombatant);
                ExecuteAction(selfAction);
                break;

            case AbilityTarget.SingleEnemy:
            case AbilityTarget.RandomEnemy:
                // Select enemy target
                Phase = CombatPhase.SelectingTarget;
                TargetIndex = Enemies.FindIndex(e => e.IsAlive);
                if (TargetIndex >= 0)
                    TargetedCombatant = Enemies[TargetIndex];
                break;

            case AbilityTarget.SingleAlly:
                // Select ally target
                Phase = CombatPhase.SelectingTarget;
                TargetIndex = Party.FindIndex(p => p.IsAlive);
                if (TargetIndex >= 0)
                    TargetedCombatant = Party[TargetIndex];
                break;

            case AbilityTarget.AllEnemies:
                // Execute on all enemies
                var aoeAction = CombatAction.UseAbility(ActiveCombatant, ability, Enemies.Where(e => e.IsAlive).ToList());
                ExecuteAction(aoeAction);
                break;

            case AbilityTarget.AllAllies:
                // Execute on all allies
                var healAllAction = CombatAction.UseAbility(ActiveCombatant, ability, Party.Where(p => p.IsAlive).ToList());
                ExecuteAction(healAllAction);
                break;

            case AbilityTarget.All:
                // Execute on everyone
                var allAction = CombatAction.UseAbility(ActiveCombatant, ability, AllCombatants.Where(c => c.IsAlive).ToList());
                ExecuteAction(allAction);
                break;
        }
    }

    /// <summary>
    /// Cancels ability selection and returns to action menu.
    /// </summary>
    public void CancelAbilitySelection()
    {
        Phase = CombatPhase.SelectingAction;
        SelectedAbility = null;
    }

    /// <summary>
    /// Cycles to the next valid target.
    /// </summary>
    /// <param name="direction">1 for next, -1 for previous.</param>
    public void CycleTarget(int direction)
    {
        // Determine valid targets based on selected ability
        List<Combatant> targets;
        if (SelectedAbility != null)
        {
            var targetType = SelectedAbility.Definition.Target;
            targets = targetType == AbilityTarget.SingleAlly
                ? Party.Where(p => p.IsAlive).ToList()
                : Enemies.Where(e => e.IsAlive).ToList();
        }
        else
        {
            targets = Enemies.Where(e => e.IsAlive).ToList();
        }

        if (targets.Count == 0)
            return;

        int currentIndex = targets.IndexOf(TargetedCombatant);
        if (currentIndex < 0) currentIndex = 0;

        currentIndex = (currentIndex + direction + targets.Count) % targets.Count;
        TargetedCombatant = targets[currentIndex];
    }

    /// <summary>
    /// Confirms the current target and executes the action.
    /// </summary>
    public void ConfirmTarget()
    {
        if (ActiveCombatant == null || TargetedCombatant == null)
            return;

        CombatAction action;
        if (SelectedAbility != null)
        {
            action = CombatAction.UseAbility(ActiveCombatant, SelectedAbility, TargetedCombatant);
        }
        else
        {
            action = CombatAction.Attack(ActiveCombatant, TargetedCombatant);
        }
        ExecuteAction(action);
    }

    /// <summary>
    /// Cancels target selection and returns to action selection.
    /// </summary>
    public void CancelTargetSelection()
    {
        if (SelectedAbility != null)
        {
            // Go back to ability selection
            Phase = CombatPhase.SelectingAbility;
            SelectedAbility = null;
        }
        else
        {
            Phase = CombatPhase.SelectingAction;
        }
        TargetedCombatant = null;
    }

    /// <summary>
    /// Executes a combat action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void ExecuteAction(CombatAction action)
    {
        var result = new CombatActionResult { Action = action, Success = true };

        switch (action.Type)
        {
            case CombatActionType.Attack:
                ExecuteAttack(action, result);
                break;

            case CombatActionType.Ability:
                ExecuteAbility(action, result);
                break;

            case CombatActionType.Defend:
                ExecuteDefend(action, result);
                break;

            case CombatActionType.Flee:
                ExecuteFlee(action, result);
                break;

            case CombatActionType.Gravitation:
                ExecuteGravitation(action, result);
                break;
        }

        // Record result
        ActionHistory.Add(result);
        LastResult = result;
        ResultDisplayTimer = 1.5f; // Show result for 1.5 seconds

        // Reset the acting combatant
        action.Source?.ResetAtb();

        // Clear selected ability
        SelectedAbility = null;

        // Fire event
        ActionExecuted?.Invoke(this, result);

        // Update phase
        Phase = CombatPhase.ExecutingAction;
        ActiveCombatant = null;
    }

    private void ExecuteAttack(CombatAction action, CombatActionResult result)
    {
        if (action.Source == null || action.Target == null)
            return;

        // Calculate damage
        int baseDamage = action.Source.Attack;

        // Random variance (90-110%)
        float variance = 0.9f + (float)_random.NextDouble() * 0.2f;
        int damage = (int)(baseDamage * variance);

        // Critical hit chance (10%)
        if (_random.NextDouble() < 0.1)
        {
            damage = (int)(damage * 1.5f);
            result.WasCritical = true;
        }

        // Apply damage
        int actualDamage = action.Target.TakeDamage(damage);
        result.DamageDealt = actualDamage;
        result.Message = $"{action.Source.Name} attacks {action.Target.Name} for {actualDamage} damage!";

        if (result.WasCritical)
            result.Message += " Critical hit!";

        if (!action.Target.IsAlive)
        {
            result.CausedDefeat = true;
            result.DefeatedCombatant = action.Target;
            result.Message += $" {action.Target.Name} is defeated!";
        }
    }

    private void ExecuteDefend(CombatAction action, CombatActionResult result)
    {
        if (action.Source == null)
            return;

        action.Source.Defend();
        result.Message = $"{action.Source.Name} defends!";
    }

    private void ExecuteFlee(CombatAction action, CombatActionResult result)
    {
        // 50% base chance to flee, modified by speed difference
        float fleeChance = 0.5f;

        if (_random.NextDouble() < fleeChance)
        {
            result.FledSuccessfully = true;
            result.Message = "Got away safely!";
            Phase = CombatPhase.Fled;
            CombatEnded?.Invoke(this, Phase);
        }
        else
        {
            result.FledSuccessfully = false;
            result.Message = "Couldn't escape!";
        }
    }

    private void ExecuteGravitation(CombatAction action, CombatActionResult result)
    {
        if (action.Target == null)
            return;

        // Gravitation deals percentage-based damage
        float damagePercent = 0.50f; // Default 50%, can be modified by GravitationStage

        int damage = action.Target.TakePercentDamage(damagePercent);
        result.DamageDealt = damage;
        result.Message = $"GRAVITATION hits {action.Target.Name} for {damage} damage!";

        if (!action.Target.IsAlive)
        {
            result.CausedDefeat = true;
            result.DefeatedCombatant = action.Target;
        }
    }

    private void ExecuteAbility(CombatAction action, CombatActionResult result)
    {
        if (action.Source == null || string.IsNullOrEmpty(action.AbilityId))
            return;

        var abilityDef = Abilities.Get(action.AbilityId);
        if (abilityDef == null)
            return;

        // Find the ability instance on the source
        var ability = action.Source.Abilities.FirstOrDefault(a => a.Definition.Id == action.AbilityId);
        if (ability == null)
            return;

        // Use energy
        if (!action.Source.UseEnergy(abilityDef.EnergyCost))
        {
            result.Success = false;
            result.Message = $"{action.Source.Name} doesn't have enough energy!";
            return;
        }

        // Mark ability as used
        ability.Use();

        // Execute based on category
        switch (abilityDef.Category)
        {
            case AbilityCategory.Damage:
                ExecuteDamageAbility(action, abilityDef, result);
                break;

            case AbilityCategory.Healing:
                ExecuteHealingAbility(action, abilityDef, result);
                break;

            case AbilityCategory.Buff:
            case AbilityCategory.Debuff:
            case AbilityCategory.Status:
                ExecuteStatusAbility(action, abilityDef, result);
                break;

            default:
                result.Message = $"{action.Source.Name} uses {abilityDef.Name}!";
                break;
        }

        // Record for AI threat tracking
        if (result.DamageDealt > 0)
        {
            CombatAI.RecordDamage(action.Source, action.Target!, result.DamageDealt);
        }
        if (result.HealingDone > 0)
        {
            CombatAI.RecordHealing(action.Source, result.HealingDone);
        }
    }

    private void ExecuteDamageAbility(CombatAction action, AbilityDefinition abilityDef, CombatActionResult result)
    {
        if (action.Source == null)
            return;

        int totalDamage = 0;
        var defeatedTargets = new List<string>();

        foreach (var target in action.Targets)
        {
            if (!target.IsAlive)
                continue;

            // Check accuracy
            if (_random.Next(100) >= abilityDef.Accuracy)
            {
                result.Missed = true;
                continue;
            }

            // Calculate damage: base power + special stat scaling
            int baseDamage = abilityDef.Power + action.Source.Special / 2;

            // Random variance (90-110%)
            float variance = 0.9f + (float)_random.NextDouble() * 0.2f;
            int damage = (int)(baseDamage * variance);

            // Critical hit
            int critChance = 10 + abilityDef.CritBonus;
            if (_random.Next(100) < critChance)
            {
                damage = (int)(damage * 1.5f);
                result.WasCritical = true;
            }

            // Apply damage
            int actualDamage = target.TakeDamage(damage);
            totalDamage += actualDamage;

            // Apply status effect
            if (abilityDef.AppliesStatus != StatusEffect.None && _random.Next(100) < abilityDef.StatusChance)
            {
                target.ApplyStatus(abilityDef.AppliesStatus, abilityDef.StatusDuration);
            }

            if (!target.IsAlive)
            {
                defeatedTargets.Add(target.Name);
                result.CausedDefeat = true;
                result.DefeatedCombatant = target;
            }
        }

        result.DamageDealt = totalDamage;

        if (action.Targets.Count == 1)
        {
            result.Message = $"{action.Source.Name} uses {abilityDef.Name} on {action.Target?.Name} for {totalDamage} damage!";
        }
        else
        {
            result.Message = $"{action.Source.Name} uses {abilityDef.Name} for {totalDamage} total damage!";
        }

        if (result.WasCritical)
            result.Message += " Critical!";

        if (defeatedTargets.Count > 0)
            result.Message += $" {string.Join(", ", defeatedTargets)} defeated!";
    }

    private void ExecuteHealingAbility(CombatAction action, AbilityDefinition abilityDef, CombatActionResult result)
    {
        if (action.Source == null)
            return;

        int totalHealing = 0;

        foreach (var target in action.Targets)
        {
            if (!target.IsAlive)
                continue;

            // Calculate healing: base power + special stat scaling
            int healing = abilityDef.Power + action.Source.Special / 2;

            int actualHealing = target.Heal(healing);
            totalHealing += actualHealing;
        }

        result.HealingDone = totalHealing;

        if (action.Targets.Count == 1)
        {
            result.Message = $"{action.Source.Name} uses {abilityDef.Name} on {action.Target?.Name}, restoring {totalHealing} HP!";
        }
        else
        {
            result.Message = $"{action.Source.Name} uses {abilityDef.Name}, restoring {totalHealing} total HP!";
        }
    }

    private void ExecuteStatusAbility(CombatAction action, AbilityDefinition abilityDef, CombatActionResult result)
    {
        if (action.Source == null)
            return;

        int affected = 0;

        foreach (var target in action.Targets)
        {
            if (!target.IsAlive)
                continue;

            // Check accuracy for debuffs
            if (abilityDef.Category == AbilityCategory.Debuff && _random.Next(100) >= abilityDef.Accuracy)
            {
                continue;
            }

            // Apply status effect
            if (abilityDef.AppliesStatus != StatusEffect.None && _random.Next(100) < abilityDef.StatusChance)
            {
                target.ApplyStatus(abilityDef.AppliesStatus, abilityDef.StatusDuration);
                affected++;
            }

            // Apply damage if any
            if (abilityDef.Power > 0)
            {
                int damage = abilityDef.Power + action.Source.Special / 4;
                target.TakeDamage(damage);
                result.DamageDealt += damage;
            }
        }

        result.Message = $"{action.Source.Name} uses {abilityDef.Name}!";
        if (affected > 0 && abilityDef.AppliesStatus != StatusEffect.None)
        {
            result.Message += $" {abilityDef.AppliesStatus} applied!";
        }
    }

    /// <summary>
    /// Triggers companion Gravitation intervention.
    /// </summary>
    /// <param name="stage">Current Gravitation stage.</param>
    /// <param name="targetsAlly">Whether it targets an ally due to corruption.</param>
    public void TriggerGravitation(GravitationStage stage, bool targetsAlly)
    {
        Combatant? target;
        float damagePercent = stage.GetDamagePercent();

        if (targetsAlly)
        {
            // Target a random party member
            var targets = Party.Where(p => p.IsAlive).ToList();
            target = targets.Count > 0 ? targets[_random.Next(targets.Count)] : null;
        }
        else
        {
            // Target a random enemy
            var targets = Enemies.Where(e => e.IsAlive).ToList();
            target = targets.Count > 0 ? targets[_random.Next(targets.Count)] : null;
        }

        if (target != null)
        {
            var action = CombatAction.Gravitation(target, damagePercent, targetsAlly);
            ExecuteAction(action);
        }
    }

    /// <summary>
    /// Gets the available actions for the current state.
    /// </summary>
    public List<string> GetAvailableActions()
    {
        var actions = new List<string> { "Attack" };

        // Add Abilities option if the combatant has any
        if (ActiveCombatant != null && ActiveCombatant.Abilities.Count > 0)
        {
            actions.Add("Abilities");
        }

        actions.Add("Defend");
        actions.Add("Flee");

        return actions;
    }
}
