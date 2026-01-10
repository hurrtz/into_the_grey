using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Strays.Core.Game.Combat;

/// <summary>
/// Element types for abilities and Strays.
/// </summary>
public enum Element
{
    /// <summary>
    /// No element (neutral).
    /// </summary>
    None,

    /// <summary>
    /// Electric - effective against mechanical.
    /// </summary>
    Electric,

    /// <summary>
    /// Fire - effective against organic.
    /// </summary>
    Fire,

    /// <summary>
    /// Ice - effective against electric.
    /// </summary>
    Ice,

    /// <summary>
    /// Toxic - effective against organic, weak to fire.
    /// </summary>
    Toxic,

    /// <summary>
    /// Kinetic - physical force, no weaknesses.
    /// </summary>
    Kinetic,

    /// <summary>
    /// Psionic - mental attacks, rare.
    /// </summary>
    Psionic,

    /// <summary>
    /// Corruption - Lazarus's dark influence.
    /// </summary>
    Corruption
}

/// <summary>
/// Target type for abilities.
/// </summary>
public enum AbilityTarget
{
    /// <summary>
    /// Single enemy target.
    /// </summary>
    SingleEnemy,

    /// <summary>
    /// All enemies.
    /// </summary>
    AllEnemies,

    /// <summary>
    /// Single ally.
    /// </summary>
    SingleAlly,

    /// <summary>
    /// All allies.
    /// </summary>
    AllAllies,

    /// <summary>
    /// Self only.
    /// </summary>
    Self,

    /// <summary>
    /// Random enemy.
    /// </summary>
    RandomEnemy,

    /// <summary>
    /// Everyone (allies and enemies).
    /// </summary>
    All
}

/// <summary>
/// Category of ability effect.
/// </summary>
public enum AbilityCategory
{
    /// <summary>
    /// Deals damage.
    /// </summary>
    Damage,

    /// <summary>
    /// Heals HP.
    /// </summary>
    Healing,

    /// <summary>
    /// Applies a buff to allies.
    /// </summary>
    Buff,

    /// <summary>
    /// Applies a debuff to enemies.
    /// </summary>
    Debuff,

    /// <summary>
    /// Status effect (stun, poison, etc.).
    /// </summary>
    Status,

    /// <summary>
    /// Utility (switch, flee boost, etc.).
    /// </summary>
    Utility
}

/// <summary>
/// Status effects that can be applied in combat.
/// </summary>
[Flags]
public enum StatusEffect
{
    None = 0,
    Poison = 1 << 0,      // Damage over time
    Burn = 1 << 1,        // Damage over time, reduces attack
    Freeze = 1 << 2,      // Skip turn chance
    Paralysis = 1 << 3,   // Reduced speed
    Stun = 1 << 4,        // Skip next turn
    Blind = 1 << 5,       // Reduced accuracy
    Weaken = 1 << 6,      // Reduced attack
    Vulnerable = 1 << 7,  // Reduced defense
    Regen = 1 << 8,       // Heal over time
    Shield = 1 << 9,      // Damage absorption
    Haste = 1 << 10,      // Increased speed
    Berserk = 1 << 11,    // Increased attack, can't use abilities
    Corrupted = 1 << 12   // Special - Lazarus influence
}

/// <summary>
/// Definition of a combat ability.
/// </summary>
public class AbilityDefinition
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Description of the ability.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Element type.
    /// </summary>
    public Element Element { get; init; } = Element.None;

    /// <summary>
    /// Target type.
    /// </summary>
    public AbilityTarget Target { get; init; } = AbilityTarget.SingleEnemy;

    /// <summary>
    /// Primary category.
    /// </summary>
    public AbilityCategory Category { get; init; } = AbilityCategory.Damage;

    /// <summary>
    /// Base power (damage/healing amount).
    /// </summary>
    public int Power { get; init; } = 50;

    /// <summary>
    /// Accuracy percentage (0-100).
    /// </summary>
    public int Accuracy { get; init; } = 100;

    /// <summary>
    /// Critical hit chance bonus.
    /// </summary>
    public int CritBonus { get; init; } = 0;

    /// <summary>
    /// Energy cost to use.
    /// </summary>
    public int EnergyCost { get; init; } = 10;

    /// <summary>
    /// Cooldown in turns after use.
    /// </summary>
    public int Cooldown { get; init; } = 0;

    /// <summary>
    /// Priority modifier (higher = acts sooner).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Status effect to apply.
    /// </summary>
    public StatusEffect AppliesStatus { get; init; } = StatusEffect.None;

    /// <summary>
    /// Chance to apply status (0-100).
    /// </summary>
    public int StatusChance { get; init; } = 0;

    /// <summary>
    /// Duration of status effect in turns.
    /// </summary>
    public int StatusDuration { get; init; } = 3;

    /// <summary>
    /// Stat multipliers for buff/debuff.
    /// </summary>
    public Dictionary<string, float> StatModifiers { get; init; } = new();

    /// <summary>
    /// Whether this ability can only be used once per battle.
    /// </summary>
    public bool OncePerBattle { get; init; } = false;

    /// <summary>
    /// Whether this is the companion's Gravitation ability.
    /// </summary>
    public bool IsGravitation { get; init; } = false;

    /// <summary>
    /// Level required to learn this ability.
    /// </summary>
    public int LearnLevel { get; init; } = 1;

    /// <summary>
    /// Placeholder color for visual representation.
    /// </summary>
    public Color PlaceholderColor { get; init; } = Color.White;
}

/// <summary>
/// Runtime instance of an ability for a specific combatant.
/// </summary>
public class Ability
{
    /// <summary>
    /// The ability definition.
    /// </summary>
    public AbilityDefinition Definition { get; }

    /// <summary>
    /// Current cooldown remaining (0 = ready).
    /// </summary>
    public int CurrentCooldown { get; private set; }

    /// <summary>
    /// Whether this ability has been used this battle (for once-per-battle abilities).
    /// </summary>
    public bool UsedThisBattle { get; private set; }

    /// <summary>
    /// Whether this ability is currently usable.
    /// </summary>
    public bool IsReady => CurrentCooldown <= 0 && (!Definition.OncePerBattle || !UsedThisBattle);

    public Ability(AbilityDefinition definition)
    {
        Definition = definition;
        CurrentCooldown = 0;
        UsedThisBattle = false;
    }

    /// <summary>
    /// Marks the ability as used and starts cooldown.
    /// </summary>
    public void Use()
    {
        CurrentCooldown = Definition.Cooldown;
        if (Definition.OncePerBattle)
        {
            UsedThisBattle = true;
        }
    }

    /// <summary>
    /// Reduces cooldown by one turn.
    /// </summary>
    public void TickCooldown()
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown--;
        }
    }

    /// <summary>
    /// Resets for a new battle.
    /// </summary>
    public void ResetForBattle()
    {
        CurrentCooldown = 0;
        UsedThisBattle = false;
    }
}

/// <summary>
/// Static registry of all abilities in the game.
/// </summary>
public static class Abilities
{
    private static readonly Dictionary<string, AbilityDefinition> _abilities = new();

    /// <summary>
    /// All registered abilities.
    /// </summary>
    public static IReadOnlyDictionary<string, AbilityDefinition> All => _abilities;

    /// <summary>
    /// Gets an ability by ID.
    /// </summary>
    public static AbilityDefinition? Get(string id) =>
        _abilities.TryGetValue(id, out var ability) ? ability : null;

    /// <summary>
    /// Registers an ability.
    /// </summary>
    public static void Register(AbilityDefinition ability)
    {
        _abilities[ability.Id] = ability;
    }

    /// <summary>
    /// Gets all abilities of a specific element.
    /// </summary>
    public static IEnumerable<AbilityDefinition> GetByElement(Element element) =>
        _abilities.Values.Where(a => a.Element == element);

    /// <summary>
    /// Gets all abilities of a specific category.
    /// </summary>
    public static IEnumerable<AbilityDefinition> GetByCategory(AbilityCategory category) =>
        _abilities.Values.Where(a => a.Category == category);

    static Abilities()
    {
        RegisterCoreAbilities();
        RegisterElementalAbilities();
        RegisterStatusAbilities();
        RegisterSpecialAbilities();
    }

    private static void RegisterCoreAbilities()
    {
        // Basic attacks
        Register(new AbilityDefinition
        {
            Id = "strike",
            Name = "Strike",
            Description = "A basic physical attack.",
            Element = Element.Kinetic,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 40,
            EnergyCost = 0,
            PlaceholderColor = Color.Gray
        });

        Register(new AbilityDefinition
        {
            Id = "power_strike",
            Name = "Power Strike",
            Description = "A powerful physical attack.",
            Element = Element.Kinetic,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 80,
            EnergyCost = 15,
            LearnLevel = 5,
            PlaceholderColor = Color.DarkGray
        });

        Register(new AbilityDefinition
        {
            Id = "multi_strike",
            Name = "Multi-Strike",
            Description = "Attacks all enemies.",
            Element = Element.Kinetic,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.AllEnemies,
            Power = 50,
            EnergyCost = 25,
            LearnLevel = 10,
            PlaceholderColor = Color.SlateGray
        });

        // Healing
        Register(new AbilityDefinition
        {
            Id = "repair",
            Name = "Repair",
            Description = "Restores HP to an ally.",
            Category = AbilityCategory.Healing,
            Target = AbilityTarget.SingleAlly,
            Power = 50,
            EnergyCost = 20,
            PlaceholderColor = Color.LimeGreen
        });

        Register(new AbilityDefinition
        {
            Id = "mass_repair",
            Name = "Mass Repair",
            Description = "Restores HP to all allies.",
            Category = AbilityCategory.Healing,
            Target = AbilityTarget.AllAllies,
            Power = 35,
            EnergyCost = 40,
            LearnLevel = 15,
            PlaceholderColor = Color.Green
        });

        // Buffs
        Register(new AbilityDefinition
        {
            Id = "fortify",
            Name = "Fortify",
            Description = "Increases defense.",
            Category = AbilityCategory.Buff,
            Target = AbilityTarget.SingleAlly,
            EnergyCost = 15,
            StatModifiers = new Dictionary<string, float> { { "Defense", 1.5f } },
            StatusDuration = 3,
            PlaceholderColor = Color.SteelBlue
        });

        Register(new AbilityDefinition
        {
            Id = "overclock",
            Name = "Overclock",
            Description = "Greatly increases speed.",
            Category = AbilityCategory.Buff,
            Target = AbilityTarget.Self,
            EnergyCost = 20,
            AppliesStatus = StatusEffect.Haste,
            StatusChance = 100,
            StatusDuration = 3,
            PlaceholderColor = Color.Yellow
        });
    }

    private static void RegisterElementalAbilities()
    {
        // Electric
        Register(new AbilityDefinition
        {
            Id = "shock",
            Name = "Shock",
            Description = "An electric attack that may paralyze.",
            Element = Element.Electric,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 55,
            EnergyCost = 12,
            AppliesStatus = StatusEffect.Paralysis,
            StatusChance = 20,
            PlaceholderColor = Color.Yellow
        });

        Register(new AbilityDefinition
        {
            Id = "thunderbolt",
            Name = "Thunderbolt",
            Description = "A powerful electric blast.",
            Element = Element.Electric,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 90,
            EnergyCost = 25,
            LearnLevel = 12,
            PlaceholderColor = Color.Gold
        });

        Register(new AbilityDefinition
        {
            Id = "chain_lightning",
            Name = "Chain Lightning",
            Description = "Electric attack hitting all enemies.",
            Element = Element.Electric,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.AllEnemies,
            Power = 65,
            EnergyCost = 35,
            LearnLevel = 20,
            PlaceholderColor = Color.LightYellow
        });

        // Fire
        Register(new AbilityDefinition
        {
            Id = "ember",
            Name = "Ember",
            Description = "A small fire attack.",
            Element = Element.Fire,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 50,
            EnergyCost = 10,
            AppliesStatus = StatusEffect.Burn,
            StatusChance = 10,
            PlaceholderColor = Color.Orange
        });

        Register(new AbilityDefinition
        {
            Id = "flamethrower",
            Name = "Flamethrower",
            Description = "Engulfs enemies in flames.",
            Element = Element.Fire,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.AllEnemies,
            Power = 70,
            EnergyCost = 30,
            AppliesStatus = StatusEffect.Burn,
            StatusChance = 25,
            LearnLevel = 15,
            PlaceholderColor = Color.OrangeRed
        });

        // Ice
        Register(new AbilityDefinition
        {
            Id = "frost_bite",
            Name = "Frost Bite",
            Description = "A chilling attack.",
            Element = Element.Ice,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 55,
            EnergyCost = 12,
            AppliesStatus = StatusEffect.Freeze,
            StatusChance = 15,
            PlaceholderColor = Color.LightBlue
        });

        Register(new AbilityDefinition
        {
            Id = "blizzard",
            Name = "Blizzard",
            Description = "An icy storm hitting all enemies.",
            Element = Element.Ice,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.AllEnemies,
            Power = 60,
            EnergyCost = 35,
            AppliesStatus = StatusEffect.Freeze,
            StatusChance = 20,
            LearnLevel = 18,
            PlaceholderColor = Color.CornflowerBlue
        });

        // Toxic
        Register(new AbilityDefinition
        {
            Id = "toxic_spray",
            Name = "Toxic Spray",
            Description = "Poisons the target.",
            Element = Element.Toxic,
            Category = AbilityCategory.Status,
            Target = AbilityTarget.SingleEnemy,
            Power = 30,
            EnergyCost = 15,
            AppliesStatus = StatusEffect.Poison,
            StatusChance = 80,
            StatusDuration = 4,
            PlaceholderColor = Color.Purple
        });

        Register(new AbilityDefinition
        {
            Id = "acid_rain",
            Name = "Acid Rain",
            Description = "Toxic rain damages and poisons all enemies.",
            Element = Element.Toxic,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.AllEnemies,
            Power = 55,
            EnergyCost = 30,
            AppliesStatus = StatusEffect.Poison,
            StatusChance = 40,
            LearnLevel = 16,
            PlaceholderColor = Color.MediumPurple
        });

        // Psionic
        Register(new AbilityDefinition
        {
            Id = "mind_blast",
            Name = "Mind Blast",
            Description = "A psychic attack that may confuse.",
            Element = Element.Psionic,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 70,
            EnergyCost = 20,
            AppliesStatus = StatusEffect.Stun,
            StatusChance = 15,
            LearnLevel = 10,
            PlaceholderColor = Color.Magenta
        });
    }

    private static void RegisterStatusAbilities()
    {
        // Debuffs
        Register(new AbilityDefinition
        {
            Id = "weaken",
            Name = "Weaken",
            Description = "Reduces enemy attack.",
            Category = AbilityCategory.Debuff,
            Target = AbilityTarget.SingleEnemy,
            EnergyCost = 12,
            AppliesStatus = StatusEffect.Weaken,
            StatusChance = 100,
            StatusDuration = 3,
            PlaceholderColor = Color.DarkRed
        });

        Register(new AbilityDefinition
        {
            Id = "expose",
            Name = "Expose",
            Description = "Reduces enemy defense.",
            Category = AbilityCategory.Debuff,
            Target = AbilityTarget.SingleEnemy,
            EnergyCost = 12,
            AppliesStatus = StatusEffect.Vulnerable,
            StatusChance = 100,
            StatusDuration = 3,
            PlaceholderColor = Color.Brown
        });

        Register(new AbilityDefinition
        {
            Id = "blind",
            Name = "Blind",
            Description = "Reduces enemy accuracy.",
            Category = AbilityCategory.Debuff,
            Target = AbilityTarget.SingleEnemy,
            EnergyCost = 10,
            AppliesStatus = StatusEffect.Blind,
            StatusChance = 85,
            StatusDuration = 2,
            PlaceholderColor = Color.DarkGray
        });

        // Utility
        Register(new AbilityDefinition
        {
            Id = "regenerate",
            Name = "Regenerate",
            Description = "Grants regeneration over time.",
            Category = AbilityCategory.Buff,
            Target = AbilityTarget.SingleAlly,
            EnergyCost = 25,
            AppliesStatus = StatusEffect.Regen,
            StatusChance = 100,
            StatusDuration = 5,
            PlaceholderColor = Color.LightGreen
        });

        Register(new AbilityDefinition
        {
            Id = "shield",
            Name = "Shield",
            Description = "Creates a protective barrier.",
            Category = AbilityCategory.Buff,
            Target = AbilityTarget.SingleAlly,
            EnergyCost = 20,
            AppliesStatus = StatusEffect.Shield,
            StatusChance = 100,
            StatusDuration = 3,
            PlaceholderColor = Color.CadetBlue
        });
    }

    private static void RegisterSpecialAbilities()
    {
        // Companion's Gravitation ability
        Register(new AbilityDefinition
        {
            Id = "gravitation",
            Name = "Gravitation",
            Description = "The companion's unique ability. Draws enemies in.",
            Element = Element.Corruption,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.AllEnemies,
            Power = 100,
            Accuracy = 100,
            EnergyCost = 0,
            IsGravitation = true,
            OncePerBattle = true,
            PlaceholderColor = Color.DarkMagenta
        });

        // Evolution-unlocked abilities
        Register(new AbilityDefinition
        {
            Id = "corrupted_strike",
            Name = "Corrupted Strike",
            Description = "An attack infused with Lazarus's corruption.",
            Element = Element.Corruption,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 120,
            EnergyCost = 30,
            AppliesStatus = StatusEffect.Corrupted,
            StatusChance = 25,
            LearnLevel = 25,
            PlaceholderColor = Color.DarkViolet
        });

        Register(new AbilityDefinition
        {
            Id = "system_override",
            Name = "System Override",
            Description = "Stuns mechanical enemies.",
            Element = Element.Electric,
            Category = AbilityCategory.Status,
            Target = AbilityTarget.SingleEnemy,
            Power = 0,
            EnergyCost = 35,
            AppliesStatus = StatusEffect.Stun,
            StatusChance = 75,
            LearnLevel = 20,
            PlaceholderColor = Color.Cyan
        });

        Register(new AbilityDefinition
        {
            Id = "sacrifice",
            Name = "Sacrifice",
            Description = "Deal massive damage at the cost of HP.",
            Element = Element.Kinetic,
            Category = AbilityCategory.Damage,
            Target = AbilityTarget.SingleEnemy,
            Power = 200,
            EnergyCost = 50,
            Cooldown = 5,
            LearnLevel = 30,
            PlaceholderColor = Color.DarkRed
        });
    }
}
