using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Game.Entities;

namespace Strays.Core.Game.Combat;

/// <summary>
/// Represents a participant in combat - either a party Stray or an enemy.
/// </summary>
public class Combatant
{
    /// <summary>
    /// The underlying Stray.
    /// </summary>
    public Stray Stray { get; }

    /// <summary>
    /// Whether this combatant is an enemy.
    /// </summary>
    public bool IsEnemy { get; }

    /// <summary>
    /// Position in the combat UI.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Current ATB (Active Time Battle) gauge value (0-100).
    /// When it reaches 100, the combatant can act.
    /// </summary>
    public float AtbGauge { get; set; } = 0;

    /// <summary>
    /// Whether this combatant is ready to act.
    /// </summary>
    public bool IsReady => AtbGauge >= 100 && IsAlive;

    /// <summary>
    /// Whether this combatant is alive.
    /// </summary>
    public bool IsAlive => Stray.IsAlive;

    /// <summary>
    /// Whether this combatant is defending this turn.
    /// </summary>
    public bool IsDefending { get; set; }

    /// <summary>
    /// Temporary defense bonus from defending.
    /// </summary>
    public int DefenseBonus => IsDefending ? Stray.Defense : 0;

    /// <summary>
    /// The action this combatant has selected (null if not yet selected).
    /// </summary>
    public CombatAction? SelectedAction { get; set; }

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name => Stray.DisplayName;

    /// <summary>
    /// Current HP.
    /// </summary>
    public int CurrentHp => Stray.CurrentHp;

    /// <summary>
    /// Maximum HP.
    /// </summary>
    public int MaxHp => Stray.MaxHp;

    /// <summary>
    /// Speed stat (affects ATB fill rate).
    /// </summary>
    public int Speed => Stray.Speed;

    /// <summary>
    /// Attack stat.
    /// </summary>
    public int Attack => Stray.Attack;

    /// <summary>
    /// Defense stat (including bonus from defending).
    /// </summary>
    public int Defense => Stray.Defense + DefenseBonus;

    /// <summary>
    /// Combat abilities available to this combatant.
    /// </summary>
    public List<Ability> Abilities { get; } = new();

    /// <summary>
    /// Creates a combatant from a Stray.
    /// </summary>
    /// <param name="stray">The Stray.</param>
    /// <param name="isEnemy">Whether this is an enemy.</param>
    public Combatant(Stray stray, bool isEnemy)
    {
        Stray = stray;
        IsEnemy = isEnemy;
        stray.IsHostile = isEnemy;
    }

    /// <summary>
    /// Updates the ATB gauge based on speed.
    /// </summary>
    /// <param name="deltaTime">Time elapsed in seconds.</param>
    public void UpdateAtb(float deltaTime)
    {
        if (!IsAlive)
            return;

        // ATB fills based on speed stat
        // Base rate: 20 ATB per second, modified by speed
        float fillRate = 20f * (1f + Speed / 50f);
        AtbGauge += fillRate * deltaTime;

        if (AtbGauge > 100)
            AtbGauge = 100;
    }

    /// <summary>
    /// Resets the ATB gauge after taking an action.
    /// </summary>
    public void ResetAtb()
    {
        AtbGauge = 0;
        SelectedAction = null;
        IsDefending = false;
    }

    /// <summary>
    /// Takes damage.
    /// </summary>
    /// <param name="damage">Raw damage amount.</param>
    /// <returns>Actual damage taken.</returns>
    public int TakeDamage(int damage)
    {
        // Apply defense (including bonus from defending)
        int actualDamage = System.Math.Max(1, damage - Defense / 2);

        // If defending, further reduce damage
        if (IsDefending)
        {
            actualDamage = actualDamage / 2;
        }

        Stray.CurrentHp = System.Math.Max(0, Stray.CurrentHp - actualDamage);
        return actualDamage;
    }

    /// <summary>
    /// Takes percentage-based damage (for Gravitation).
    /// </summary>
    /// <param name="percent">Percentage of max HP (0-1).</param>
    /// <returns>Damage taken.</returns>
    public int TakePercentDamage(float percent)
    {
        return Stray.TakePercentDamage(percent);
    }

    /// <summary>
    /// Heals the combatant.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    /// <returns>Actual amount healed.</returns>
    public int Heal(int amount)
    {
        return Stray.Heal(amount);
    }

    /// <summary>
    /// Sets up defending for this turn.
    /// </summary>
    public void Defend()
    {
        IsDefending = true;
    }

    /// <summary>
    /// Draws the combatant in the combat UI.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture.</param>
    /// <param name="font">Font for text.</param>
    /// <param name="isSelected">Whether this combatant is selected.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, bool isSelected)
    {
        // Draw the Stray
        Stray.Draw(spriteBatch, pixelTexture, Position, showHealth: true);

        // Draw ATB gauge
        var atbWidth = 40;
        var atbHeight = 4;
        var atbY = Position.Y + Stray.Definition.PlaceholderSize / 2 + 4;

        // Background
        var bgRect = new Rectangle(
            (int)(Position.X - atbWidth / 2),
            (int)atbY,
            atbWidth,
            atbHeight
        );
        spriteBatch.Draw(pixelTexture, bgRect, Color.DarkGray);

        // ATB fill
        var atbColor = IsReady ? Color.Cyan : Color.Blue;
        var fillRect = new Rectangle(
            (int)(Position.X - atbWidth / 2),
            (int)atbY,
            (int)(atbWidth * (AtbGauge / 100f)),
            atbHeight
        );
        spriteBatch.Draw(pixelTexture, fillRect, atbColor);

        // Selection indicator
        if (isSelected)
        {
            var indicatorSize = 8;
            var indicatorY = Position.Y - Stray.Definition.PlaceholderSize / 2 - indicatorSize - 10;
            var indicatorRect = new Rectangle(
                (int)(Position.X - indicatorSize / 2),
                (int)indicatorY,
                indicatorSize,
                indicatorSize
            );
            spriteBatch.Draw(pixelTexture, indicatorRect, Color.Yellow);
        }

        // Defending indicator
        if (IsDefending)
        {
            var shieldSize = 6;
            var shieldRect = new Rectangle(
                (int)(Position.X + Stray.Definition.PlaceholderSize / 2 + 2),
                (int)(Position.Y - shieldSize / 2),
                shieldSize,
                shieldSize
            );
            spriteBatch.Draw(pixelTexture, shieldRect, Color.LightBlue);
        }

        // Draw name
        if (font != null)
        {
            var namePos = new Vector2(
                Position.X - font.MeasureString(Name).X / 2,
                atbY + atbHeight + 2
            );
            spriteBatch.DrawString(font, Name, namePos, IsAlive ? Color.White : Color.Gray);
        }
    }
}
