using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Game.Data;

namespace Strays.Core.Game.Entities;

/// <summary>
/// Represents an individual Stray instance - a recruited or wild cybernetic creature.
/// Strays are half-biological, half-cybernetic animals created by NIMDOK.
/// </summary>
public class Stray
{
    private static int _nextInstanceId = 1;

    /// <summary>
    /// Unique instance ID for this specific Stray.
    /// </summary>
    public string InstanceId { get; }

    /// <summary>
    /// The definition (species/type) of this Stray.
    /// </summary>
    public StrayDefinition Definition { get; }

    /// <summary>
    /// Custom nickname (null = use default name).
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Display name (nickname if set, otherwise definition name).
    /// </summary>
    public string DisplayName => Nickname ?? Definition.Name;

    /// <summary>
    /// Current level.
    /// </summary>
    public int Level { get; private set; } = 1;

    /// <summary>
    /// Experience points toward next level.
    /// </summary>
    public int Experience { get; private set; } = 0;

    /// <summary>
    /// Experience required for next level.
    /// </summary>
    public int ExperienceToNextLevel => Level * 100;

    /// <summary>
    /// Current HP.
    /// </summary>
    public int CurrentHp { get; set; }

    /// <summary>
    /// Maximum HP (scaled by level and augmentations).
    /// </summary>
    public int MaxHp => CalculateStat(Definition.BaseStats.MaxHp);

    /// <summary>
    /// Attack stat.
    /// </summary>
    public int Attack => CalculateStat(Definition.BaseStats.Attack);

    /// <summary>
    /// Defense stat.
    /// </summary>
    public int Defense => CalculateStat(Definition.BaseStats.Defense);

    /// <summary>
    /// Speed stat (affects ATB fill rate).
    /// </summary>
    public int Speed => CalculateStat(Definition.BaseStats.Speed);

    /// <summary>
    /// Special stat (affects ability power).
    /// </summary>
    public int Special => CalculateStat(Definition.BaseStats.Special);

    /// <summary>
    /// Whether this Stray is alive.
    /// </summary>
    public bool IsAlive => CurrentHp > 0;

    /// <summary>
    /// Whether this Stray has evolved.
    /// </summary>
    public bool IsEvolved { get; private set; } = false;

    /// <summary>
    /// Evolution state tracking (stress, evolution history).
    /// </summary>
    public EvolutionState EvolutionState { get; } = new();

    /// <summary>
    /// Bond level with the protagonist (0-100).
    /// Affects loyalty, combat bonuses, and recruitment of similar Strays.
    /// </summary>
    public int BondLevel { get; set; } = 0;

    /// <summary>
    /// Equipped augmentations by slot name.
    /// </summary>
    public Dictionary<string, string?> EquippedAugmentations { get; } = new();

    /// <summary>
    /// Equipped microchips.
    /// </summary>
    public List<string> EquippedMicrochips { get; } = new();

    /// <summary>
    /// Whether this Stray is hostile (enemy in combat).
    /// </summary>
    public bool IsHostile { get; set; } = false;

    /// <summary>
    /// Position in combat (for visual placement).
    /// </summary>
    public Vector2 CombatPosition { get; set; }

    /// <summary>
    /// Creates a new Stray from a definition.
    /// </summary>
    /// <param name="definition">The Stray definition to use.</param>
    /// <param name="level">Starting level.</param>
    public Stray(StrayDefinition definition, int level = 1)
    {
        InstanceId = $"stray_{_nextInstanceId++}";
        Definition = definition;
        Level = Math.Max(1, level);
        CurrentHp = MaxHp;
    }

    /// <summary>
    /// Creates a Stray from a definition ID.
    /// </summary>
    /// <param name="definitionId">The definition ID.</param>
    /// <param name="level">Starting level.</param>
    public static Stray? Create(string definitionId, int level = 1)
    {
        var definition = StrayDefinitions.Get(definitionId);
        if (definition == null)
            return null;

        return new Stray(definition, level);
    }

    /// <summary>
    /// Calculates a stat value based on level and augmentations.
    /// </summary>
    private int CalculateStat(int baseStat)
    {
        // Scale by level (10% per level)
        float levelMultiplier = 1f + (Level - 1) * 0.1f;

        // TODO: Add augmentation bonuses

        return (int)(baseStat * levelMultiplier);
    }

    /// <summary>
    /// Adds experience and checks for level up.
    /// </summary>
    /// <param name="amount">Experience to add.</param>
    /// <returns>True if leveled up.</returns>
    public bool AddExperience(int amount)
    {
        Experience += amount;
        bool leveledUp = false;

        while (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            Level++;
            leveledUp = true;

            // Heal to full on level up
            CurrentHp = MaxHp;
        }

        return leveledUp;
    }

    /// <summary>
    /// Takes damage, reducing HP.
    /// </summary>
    /// <param name="damage">Raw damage amount.</param>
    /// <returns>Actual damage taken after defense.</returns>
    public int TakeDamage(int damage)
    {
        // Simple damage formula: damage - defense/2, minimum 1
        int actualDamage = Math.Max(1, damage - Defense / 2);
        CurrentHp = Math.Max(0, CurrentHp - actualDamage);
        return actualDamage;
    }

    /// <summary>
    /// Takes percentage-based damage (for Gravitation).
    /// </summary>
    /// <param name="percent">Percentage of max HP to remove (0-1).</param>
    /// <returns>Damage taken.</returns>
    public int TakePercentDamage(float percent)
    {
        int damage = (int)(MaxHp * percent);
        CurrentHp = Math.Max(1, CurrentHp - damage); // Gravitation leaves at least 1 HP
        return damage;
    }

    /// <summary>
    /// Heals the Stray.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    /// <returns>Actual amount healed.</returns>
    public int Heal(int amount)
    {
        int previousHp = CurrentHp;
        CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
        return CurrentHp - previousHp;
    }

    /// <summary>
    /// Fully heals the Stray.
    /// </summary>
    public void FullHeal()
    {
        CurrentHp = MaxHp;
    }

    /// <summary>
    /// Revives the Stray with a percentage of max HP.
    /// </summary>
    /// <param name="hpPercent">Percentage of max HP to revive with (0-1).</param>
    public void Revive(float hpPercent = 0.5f)
    {
        if (!IsAlive)
        {
            CurrentHp = Math.Max(1, (int)(MaxHp * hpPercent));
        }
    }

    /// <summary>
    /// Checks if this Stray can evolve.
    /// </summary>
    public bool CanEvolve()
    {
        if (IsEvolved)
            return false;

        if (Definition.EvolutionLevel <= 0)
            return false;

        if (Level < Definition.EvolutionLevel)
            return false;

        // TODO: Check evolution trigger conditions

        return true;
    }

    /// <summary>
    /// Evolves the Stray to its next form.
    /// </summary>
    /// <returns>True if evolution succeeded.</returns>
    public bool TryEvolve()
    {
        if (!CanEvolve())
            return false;

        if (string.IsNullOrEmpty(Definition.EvolvedFormId))
            return false;

        // Mark as evolved (the definition stays the same, but stats are boosted)
        IsEvolved = true;

        // Heal to full on evolution
        CurrentHp = MaxHp;

        return true;
    }

    /// <summary>
    /// Increases bond level.
    /// </summary>
    /// <param name="amount">Amount to increase.</param>
    public void IncreaseBond(int amount)
    {
        BondLevel = Math.Min(100, BondLevel + amount);
    }

    /// <summary>
    /// Decreases bond level.
    /// </summary>
    /// <param name="amount">Amount to decrease.</param>
    public void DecreaseBond(int amount)
    {
        BondLevel = Math.Max(0, BondLevel - amount);
    }

    /// <summary>
    /// Draws the Stray (placeholder visual).
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture.</param>
    /// <param name="position">Screen position to draw at.</param>
    /// <param name="showHealth">Whether to show health bar.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 position, bool showHealth = true)
    {
        var size = Definition.PlaceholderSize;
        var color = IsHostile ? Color.Red : Definition.PlaceholderColor;

        // Dim color if dead
        if (!IsAlive)
        {
            color = color * 0.3f;
        }

        // Draw the Stray as a colored circle (square for simplicity)
        var drawRect = new Rectangle(
            (int)(position.X - size / 2),
            (int)(position.Y - size / 2),
            size,
            size
        );

        spriteBatch.Draw(pixelTexture, drawRect, color);

        // Draw health bar if alive and requested
        if (showHealth && IsAlive)
        {
            var healthBarWidth = size + 4;
            var healthBarHeight = 4;
            var healthBarY = position.Y - size / 2 - healthBarHeight - 2;

            // Background
            var bgRect = new Rectangle(
                (int)(position.X - healthBarWidth / 2),
                (int)healthBarY,
                healthBarWidth,
                healthBarHeight
            );
            spriteBatch.Draw(pixelTexture, bgRect, Color.DarkGray);

            // Health fill
            var healthPercent = (float)CurrentHp / MaxHp;
            var healthColor = healthPercent > 0.5f ? Color.Green :
                             healthPercent > 0.25f ? Color.Yellow : Color.Red;

            var fillRect = new Rectangle(
                (int)(position.X - healthBarWidth / 2),
                (int)healthBarY,
                (int)(healthBarWidth * healthPercent),
                healthBarHeight
            );
            spriteBatch.Draw(pixelTexture, fillRect, healthColor);
        }
    }

    /// <summary>
    /// Creates save data for this Stray.
    /// </summary>
    public StraySaveData ToSaveData()
    {
        return new StraySaveData
        {
            InstanceId = InstanceId,
            DefinitionId = Definition.Id,
            Nickname = Nickname,
            Level = Level,
            Experience = Experience,
            CurrentHp = CurrentHp,
            IsEvolved = IsEvolved,
            EquippedAugmentations = new Dictionary<string, string?>(EquippedAugmentations),
            EquippedMicrochips = new List<string>(EquippedMicrochips),
            BondLevel = BondLevel
        };
    }

    /// <summary>
    /// Creates a Stray from save data.
    /// </summary>
    public static Stray? FromSaveData(StraySaveData data)
    {
        var definition = StrayDefinitions.Get(data.DefinitionId);
        if (definition == null)
            return null;

        var stray = new Stray(definition, data.Level)
        {
            Nickname = data.Nickname,
            CurrentHp = data.CurrentHp,
            IsEvolved = data.IsEvolved,
            BondLevel = data.BondLevel
        };

        stray.Experience = data.Experience;

        foreach (var aug in data.EquippedAugmentations)
        {
            stray.EquippedAugmentations[aug.Key] = aug.Value;
        }

        stray.EquippedMicrochips.AddRange(data.EquippedMicrochips);

        return stray;
    }
}
