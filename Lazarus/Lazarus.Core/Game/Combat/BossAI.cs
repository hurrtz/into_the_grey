using System;
using System.Collections.Generic;
using System.Linq;
using Lazarus.Core.Game.Data;

namespace Lazarus.Core.Game.Combat;

/// <summary>
/// The type of boss, determining AI patterns.
/// </summary>
public enum BossType
{
    /// <summary>
    /// Generic boss - phases based on HP.
    /// </summary>
    Generic,

    /// <summary>
    /// Diadem Guardian - Herald of Lazarus, focuses on data manipulation.
    /// </summary>
    DiademGuardian,

    /// <summary>
    /// Liminal - Composite entity, identity-shifting attacks.
    /// </summary>
    Liminal,

    /// <summary>
    /// Hyper-Evolved Bandit - Unwinnable final boss.
    /// </summary>
    HyperEvolvedBandit
}

/// <summary>
/// Phase of a boss fight.
/// </summary>
public enum BossPhase
{
    /// <summary>
    /// Phase 1: HP > 70%
    /// </summary>
    Phase1,

    /// <summary>
    /// Phase 2: HP 40-70%
    /// </summary>
    Phase2,

    /// <summary>
    /// Phase 3: HP < 40% - Desperation
    /// </summary>
    Phase3,

    /// <summary>
    /// Enrage: HP < 20%, significantly stronger
    /// </summary>
    Enraged
}

/// <summary>
/// Special attack pattern for boss encounters.
/// </summary>
public class BossAttackPattern
{
    /// <summary>
    /// Name of the attack pattern.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Ability ID to use.
    /// </summary>
    public string AbilityId { get; init; } = "";

    /// <summary>
    /// Minimum phase required.
    /// </summary>
    public BossPhase MinPhase { get; init; } = BossPhase.Phase1;

    /// <summary>
    /// Weight for random selection (higher = more likely).
    /// </summary>
    public int Weight { get; init; } = 1;

    /// <summary>
    /// Cooldown in turns.
    /// </summary>
    public int Cooldown { get; init; } = 0;

    /// <summary>
    /// Current cooldown counter.
    /// </summary>
    public int CurrentCooldown { get; set; } = 0;

    /// <summary>
    /// Whether this pattern targets allies.
    /// </summary>
    public bool TargetsAllies { get; init; } = false;

    /// <summary>
    /// Whether this pattern targets all enemies.
    /// </summary>
    public bool IsAoE { get; init; } = false;

    /// <summary>
    /// Warning message displayed before the attack.
    /// </summary>
    public string? WarningMessage { get; init; }
}

/// <summary>
/// AI system for boss battles with special mechanics.
/// </summary>
public class BossAI
{
    private readonly Random _random = new();
    private readonly CombatAI _baseCombatAI;

    /// <summary>
    /// The type of boss.
    /// </summary>
    public BossType BossType { get; }

    /// <summary>
    /// Current phase of the fight.
    /// </summary>
    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

    /// <summary>
    /// Turn counter for the boss fight.
    /// </summary>
    public int TurnCount { get; private set; } = 0;

    /// <summary>
    /// Attack patterns for this boss.
    /// </summary>
    public List<BossAttackPattern> AttackPatterns { get; } = new();

    /// <summary>
    /// Event fired when boss changes phase.
    /// </summary>
    public event EventHandler<BossPhaseChangedEventArgs>? PhaseChanged;

    /// <summary>
    /// Event fired when boss uses a signature attack.
    /// </summary>
    public event EventHandler<BossAttackEventArgs>? SignatureAttackUsed;

    /// <summary>
    /// Creates a new BossAI for the specified boss type.
    /// </summary>
    public BossAI(BossType bossType, CombatAI baseCombatAI)
    {
        BossType = bossType;
        _baseCombatAI = baseCombatAI;
        InitializePatterns();
    }

    /// <summary>
    /// Initializes attack patterns based on boss type.
    /// </summary>
    private void InitializePatterns()
    {
        switch (BossType)
        {
            case BossType.DiademGuardian:
                InitializeDiademPatterns();
                break;

            case BossType.Liminal:
                InitializeLiminalPatterns();
                break;

            case BossType.HyperEvolvedBandit:
                InitializeHyperEvolvedBanditPatterns();
                break;

            default:
                InitializeGenericPatterns();
                break;
        }
    }

    /// <summary>
    /// Diadem Guardian patterns - data manipulation and control.
    /// </summary>
    private void InitializeDiademPatterns()
    {
        AttackPatterns.AddRange(new[]
        {
            // Phase 1: Probing attacks
            new BossAttackPattern
            {
                Name = "Data Scan",
                AbilityId = "boss_data_scan",
                MinPhase = BossPhase.Phase1,
                Weight = 3,
                WarningMessage = "The Diadem Guardian scans the battlefield..."
            },
            new BossAttackPattern
            {
                Name = "Protocol Strike",
                AbilityId = "boss_protocol_strike",
                MinPhase = BossPhase.Phase1,
                Weight = 2
            },

            // Phase 2: Control abilities
            new BossAttackPattern
            {
                Name = "System Override",
                AbilityId = "boss_system_override",
                MinPhase = BossPhase.Phase2,
                Weight = 3,
                Cooldown = 2,
                WarningMessage = "Lazarus protocols activating..."
            },
            new BossAttackPattern
            {
                Name = "Memory Wipe",
                AbilityId = "boss_memory_wipe",
                MinPhase = BossPhase.Phase2,
                Weight = 2,
                Cooldown = 3,
                TargetsAllies = false,
                WarningMessage = "Targeting memory banks..."
            },

            // Phase 3: Desperation
            new BossAttackPattern
            {
                Name = "Total Lockdown",
                AbilityId = "boss_total_lockdown",
                MinPhase = BossPhase.Phase3,
                Weight = 4,
                IsAoE = true,
                Cooldown = 4,
                WarningMessage = "INITIATING TOTAL SYSTEM LOCKDOWN!"
            },
            new BossAttackPattern
            {
                Name = "Archive Purge",
                AbilityId = "boss_archive_purge",
                MinPhase = BossPhase.Phase3,
                Weight = 3,
                IsAoE = true,
                Cooldown = 3,
                WarningMessage = "Purging all unauthorized processes..."
            }
        });
    }

    /// <summary>
    /// Liminal patterns - identity shifting and composite attacks.
    /// </summary>
    private void InitializeLiminalPatterns()
    {
        AttackPatterns.AddRange(new[]
        {
            // Phase 1: Identity probing
            new BossAttackPattern
            {
                Name = "Identity Fragment",
                AbilityId = "boss_identity_fragment",
                MinPhase = BossPhase.Phase1,
                Weight = 3,
                WarningMessage = "Liminal shifts... who is that?"
            },
            new BossAttackPattern
            {
                Name = "Borrowed Strike",
                AbilityId = "boss_borrowed_strike",
                MinPhase = BossPhase.Phase1,
                Weight = 2
            },

            // Phase 2: Memory manipulation
            new BossAttackPattern
            {
                Name = "Memory Echo",
                AbilityId = "boss_memory_echo",
                MinPhase = BossPhase.Phase2,
                Weight = 3,
                Cooldown = 2,
                WarningMessage = "You see... yourself?"
            },
            new BossAttackPattern
            {
                Name = "Composite Assault",
                AbilityId = "boss_composite_assault",
                MinPhase = BossPhase.Phase2,
                Weight = 3,
                IsAoE = true,
                Cooldown = 3,
                WarningMessage = "Multiple voices scream at once!"
            },

            // Phase 3: Identity collapse
            new BossAttackPattern
            {
                Name = "Identity Collapse",
                AbilityId = "boss_identity_collapse",
                MinPhase = BossPhase.Phase3,
                Weight = 4,
                IsAoE = true,
                Cooldown = 4,
                WarningMessage = "WHO AM I?! WHO ARE YOU?! WHO ARE WE?!"
            },
            new BossAttackPattern
            {
                Name = "Final Merge",
                AbilityId = "boss_final_merge",
                MinPhase = BossPhase.Enraged,
                Weight = 5,
                IsAoE = true,
                Cooldown = 5,
                WarningMessage = "WE WILL BECOME ONE!"
            }
        });
    }

    /// <summary>
    /// Hyper-Evolved Bandit patterns - overwhelming power, but cannot be defeated.
    /// </summary>
    private void InitializeHyperEvolvedBanditPatterns()
    {
        AttackPatterns.AddRange(new[]
        {
            // All phases: Absolute power
            new BossAttackPattern
            {
                Name = "Corrupted Gravitation",
                AbilityId = "boss_corrupted_gravitation",
                MinPhase = BossPhase.Phase1,
                Weight = 5,
                WarningMessage = "Bandit's form shudders... Gravitation building..."
            },
            new BossAttackPattern
            {
                Name = "Wild Strike",
                AbilityId = "boss_wild_strike",
                MinPhase = BossPhase.Phase1,
                Weight = 3
            },

            // Phase 2: Erratic but powerful
            new BossAttackPattern
            {
                Name = "Memory Surge",
                AbilityId = "boss_memory_surge",
                MinPhase = BossPhase.Phase2,
                Weight = 3,
                WarningMessage = "Fragments of the old Bandit surface..."
            },
            new BossAttackPattern
            {
                Name = "Gravitational Wave",
                AbilityId = "boss_gravitational_wave",
                MinPhase = BossPhase.Phase2,
                Weight = 4,
                IsAoE = true,
                Cooldown = 2,
                WarningMessage = "The very air distorts!"
            },

            // Phase 3: Absolute devastation
            new BossAttackPattern
            {
                Name = "Absolute Gravitation",
                AbilityId = "boss_absolute_gravitation",
                MinPhase = BossPhase.Phase3,
                Weight = 5,
                IsAoE = true,
                Cooldown = 3,
                WarningMessage = "EVERYTHING PULLS TOWARD BANDIT!"
            },

            // Enrage: The only way out
            new BossAttackPattern
            {
                Name = "Moment of Clarity",
                AbilityId = "boss_moment_clarity",
                MinPhase = BossPhase.Enraged,
                Weight = 10, // Always chosen when available
                TargetsAllies = true, // Targets self - Bandit's sacrifice
                Cooldown = 999, // Only happens once
                WarningMessage = "...friend...?"
            }
        });
    }

    /// <summary>
    /// Generic boss patterns.
    /// </summary>
    private void InitializeGenericPatterns()
    {
        AttackPatterns.AddRange(new[]
        {
            new BossAttackPattern
            {
                Name = "Power Strike",
                AbilityId = "boss_power_strike",
                MinPhase = BossPhase.Phase1,
                Weight = 3
            },
            new BossAttackPattern
            {
                Name = "Area Attack",
                AbilityId = "boss_area_attack",
                MinPhase = BossPhase.Phase2,
                Weight = 2,
                IsAoE = true,
                Cooldown = 2
            },
            new BossAttackPattern
            {
                Name = "Desperation Strike",
                AbilityId = "boss_desperation_strike",
                MinPhase = BossPhase.Phase3,
                Weight = 4,
                Cooldown = 3
            }
        });
    }

    /// <summary>
    /// Updates the boss phase based on current HP.
    /// </summary>
    public void UpdatePhase(Combatant boss)
    {
        float hpPercent = (float)boss.CurrentHp / boss.MaxHp;
        BossPhase newPhase;

        if (hpPercent > 0.7f)
        {
            newPhase = BossPhase.Phase1;
        }
        else if (hpPercent > 0.4f)
        {
            newPhase = BossPhase.Phase2;
        }
        else if (hpPercent > 0.2f)
        {
            newPhase = BossPhase.Phase3;
        }
        else
        {
            newPhase = BossPhase.Enraged;
        }

        if (newPhase != CurrentPhase)
        {
            var oldPhase = CurrentPhase;
            CurrentPhase = newPhase;

            PhaseChanged?.Invoke(this, new BossPhaseChangedEventArgs
            {
                Boss = boss,
                OldPhase = oldPhase,
                NewPhase = newPhase
            });
        }
    }

    /// <summary>
    /// Selects an action for the boss.
    /// </summary>
    public CombatAction SelectAction(
        Combatant boss,
        List<Combatant> allies,
        List<Combatant> enemies)
    {
        TurnCount++;

        // Update phase
        UpdatePhase(boss);

        // Tick cooldowns
        foreach (var pattern in AttackPatterns)
        {
            if (pattern.CurrentCooldown > 0)
            {
                pattern.CurrentCooldown--;
            }
        }

        // Get available patterns for current phase
        var availablePatterns = AttackPatterns
            .Where(p => p.MinPhase <= CurrentPhase && p.CurrentCooldown == 0)
            .ToList();

        if (availablePatterns.Count == 0)
        {
            // Fallback to basic attack
            var target = enemies.FirstOrDefault(e => e.IsAlive) ?? enemies.First();
            return CombatAction.Attack(boss, target);
        }

        // Weighted random selection
        var selectedPattern = SelectWeightedPattern(availablePatterns);

        // Apply cooldown
        selectedPattern.CurrentCooldown = selectedPattern.Cooldown;

        // Fire event for signature attack
        if (!string.IsNullOrEmpty(selectedPattern.WarningMessage))
        {
            SignatureAttackUsed?.Invoke(this, new BossAttackEventArgs
            {
                Boss = boss,
                PatternName = selectedPattern.Name,
                WarningMessage = selectedPattern.WarningMessage,
                Phase = CurrentPhase
            });
        }

        // Create the action
        return CreateActionFromPattern(boss, selectedPattern, allies, enemies);
    }

    /// <summary>
    /// Selects a pattern using weighted random selection.
    /// </summary>
    private BossAttackPattern SelectWeightedPattern(List<BossAttackPattern> patterns)
    {
        int totalWeight = patterns.Sum(p => p.Weight);
        int roll = _random.Next(totalWeight);

        int cumulative = 0;
        foreach (var pattern in patterns)
        {
            cumulative += pattern.Weight;
            if (roll < cumulative)
            {
                return pattern;
            }
        }

        return patterns.Last();
    }

    /// <summary>
    /// Creates a combat action from a boss attack pattern.
    /// </summary>
    private CombatAction CreateActionFromPattern(
        Combatant boss,
        BossAttackPattern pattern,
        List<Combatant> allies,
        List<Combatant> enemies)
    {
        var aliveEnemies = enemies.Where(e => e.IsAlive).ToList();
        var aliveAllies = allies.Where(a => a.IsAlive).ToList();

        // Special case: Moment of Clarity (Bandit's sacrifice)
        if (pattern.Name == "Moment of Clarity")
        {
            return new CombatAction
            {
                Type = CombatActionType.Ability,
                Source = boss,
                Target = boss, // Targets self
                Targets = new List<Combatant> { boss },
                AbilityId = pattern.AbilityId,
                Priority = 999 // Always goes first
            };
        }

        // Determine targets
        List<Combatant> targets;
        Combatant? primaryTarget;

        if (pattern.TargetsAllies)
        {
            targets = aliveAllies;
            primaryTarget = aliveAllies.Count > 0 ? aliveAllies[_random.Next(aliveAllies.Count)] : null;
        }
        else if (pattern.IsAoE)
        {
            targets = aliveEnemies;
            primaryTarget = aliveEnemies.FirstOrDefault();
        }
        else
        {
            // Single target - use threat-based selection
            primaryTarget = _baseCombatAI.ThreatTable.GetHighestThreat(aliveEnemies);
            if (primaryTarget == null && aliveEnemies.Count > 0)
            {
                primaryTarget = aliveEnemies[_random.Next(aliveEnemies.Count)];
            }
            targets = primaryTarget != null ? new List<Combatant> { primaryTarget } : new List<Combatant>();
        }

        return new CombatAction
        {
            Type = CombatActionType.Ability,
            Source = boss,
            Target = primaryTarget,
            Targets = targets,
            AbilityId = pattern.AbilityId,
            Priority = CurrentPhase == BossPhase.Enraged ? 100 : 50
        };
    }

    /// <summary>
    /// Resets the boss AI for a new fight.
    /// </summary>
    public void Reset()
    {
        CurrentPhase = BossPhase.Phase1;
        TurnCount = 0;

        foreach (var pattern in AttackPatterns)
        {
            pattern.CurrentCooldown = 0;
        }
    }

    /// <summary>
    /// Gets the phase transition message for display.
    /// </summary>
    public string GetPhaseMessage(BossPhase phase)
    {
        return BossType switch
        {
            BossType.DiademGuardian => phase switch
            {
                BossPhase.Phase2 => "The Diadem Guardian's eyes glow brighter!",
                BossPhase.Phase3 => "Lazarus PROTOCOLS ESCALATING!",
                BossPhase.Enraged => "THE GUARDIAN ACCESSES EMERGENCY OVERRIDE!",
                _ => ""
            },

            BossType.Liminal => phase switch
            {
                BossPhase.Phase2 => "Liminal's form becomes unstable...",
                BossPhase.Phase3 => "Multiple identities surface at once!",
                BossPhase.Enraged => "LIMINAL BEGINS TO COLLAPSE INTO ITSELF!",
                _ => ""
            },

            BossType.HyperEvolvedBandit => phase switch
            {
                BossPhase.Phase2 => "Bandit's attacks grow more erratic...",
                BossPhase.Phase3 => "The corruption spreads visibly...",
                BossPhase.Enraged => "For a moment... you see your friend.",
                _ => ""
            },

            _ => phase switch
            {
                BossPhase.Phase2 => "The boss grows stronger!",
                BossPhase.Phase3 => "The boss is desperate!",
                BossPhase.Enraged => "THE BOSS IS ENRAGED!",
                _ => ""
            }
        };
    }

    /// <summary>
    /// Creates a BossAI for a specific Stray definition.
    /// </summary>
    public static BossAI? CreateForStray(string strayId, CombatAI baseCombatAI)
    {
        var bossType = strayId.ToLowerInvariant() switch
        {
            "diadem_guardian" or "nimdok_herald" => BossType.DiademGuardian,
            "liminal" or "boss_liminal" => BossType.Liminal,
            "hyper_evolved_bandit" or "corrupted_bandit" => BossType.HyperEvolvedBandit,
            _ when strayId.Contains("boss") => BossType.Generic,
            _ => (BossType?)null
        };

        if (bossType == null)
        {
            return null;
        }

        return new BossAI(bossType.Value, baseCombatAI);
    }
}

/// <summary>
/// Event args for boss phase changes.
/// </summary>
public class BossPhaseChangedEventArgs : EventArgs
{
    /// <summary>
    /// The boss combatant.
    /// </summary>
    public Combatant Boss { get; init; } = null!;

    /// <summary>
    /// Previous phase.
    /// </summary>
    public BossPhase OldPhase { get; init; }

    /// <summary>
    /// New phase.
    /// </summary>
    public BossPhase NewPhase { get; init; }
}

/// <summary>
/// Event args for boss signature attacks.
/// </summary>
public class BossAttackEventArgs : EventArgs
{
    /// <summary>
    /// The boss combatant.
    /// </summary>
    public Combatant Boss { get; init; } = null!;

    /// <summary>
    /// Name of the attack pattern.
    /// </summary>
    public string PatternName { get; init; } = "";

    /// <summary>
    /// Warning message to display.
    /// </summary>
    public string? WarningMessage { get; init; }

    /// <summary>
    /// Current phase when attack was used.
    /// </summary>
    public BossPhase Phase { get; init; }
}
