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
            // Simple AI: attack a random living party member
            var targets = Party.Where(p => p.IsAlive).ToList();
            if (targets.Count > 0)
            {
                var target = targets[_random.Next(targets.Count)];
                var action = CombatAction.Attack(enemy, target);
                ExecuteAction(action);
            }
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
                // Default to first living enemy
                TargetIndex = Enemies.FindIndex(e => e.IsAlive);
                if (TargetIndex >= 0)
                    TargetedCombatant = Enemies[TargetIndex];
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
    /// Cycles to the next valid target.
    /// </summary>
    /// <param name="direction">1 for next, -1 for previous.</param>
    public void CycleTarget(int direction)
    {
        var targets = Enemies.Where(e => e.IsAlive).ToList();
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

        var action = CombatAction.Attack(ActiveCombatant, TargetedCombatant);
        ExecuteAction(action);
    }

    /// <summary>
    /// Cancels target selection and returns to action selection.
    /// </summary>
    public void CancelTargetSelection()
    {
        Phase = CombatPhase.SelectingAction;
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
        return new List<string> { "Attack", "Defend", "Flee" };
    }
}
