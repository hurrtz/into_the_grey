using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Game.Items;

namespace Lazarus.Core.Game.Combat;

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
    /// Current energy for abilities (delegated to Stray).
    /// </summary>
    public int CurrentEnergy => Stray.CurrentEnergy;

    /// <summary>
    /// Maximum energy for abilities (delegated to Stray).
    /// </summary>
    public int MaxEnergy => Stray.MaxEnergy;

    /// <summary>
    /// Energy regeneration per ATB tick (delegated to Stray).
    /// </summary>
    public int EnergyRegen => Stray.EnergyRegen;

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
    /// Special stat (affects ability power).
    /// </summary>
    public int Special => Stray.Special;

    /// <summary>
    /// Combat abilities available to this combatant.
    /// </summary>
    public List<Ability> Abilities { get; } = new();

    /// <summary>
    /// Maps ability IDs to the microchip socket index that grants them (-1 for innate abilities).
    /// </summary>
    public Dictionary<string, int> AbilitySourceChips { get; } = new();

    /// <summary>
    /// Active status effects with remaining duration.
    /// </summary>
    public Dictionary<StatusEffect, int> StatusEffects { get; } = new();

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

        // Initialize energy and heat for combat
        stray.FullEnergy();
        stray.ResetChipHeat();

        LoadAbilities();
    }

    /// <summary>
    /// Loads abilities from the Stray's microchips and innate abilities.
    /// </summary>
    private void LoadAbilities()
    {
        Abilities.Clear();
        AbilitySourceChips.Clear();

        // Add innate abilities based on level
        var innateAbilityIds = new List<string>();

        // All Strays get basic strike
        innateAbilityIds.Add("strike");
        AbilitySourceChips["strike"] = -1; // Innate

        // Add abilities based on level
        if (Stray.Level >= 5)
        {
            innateAbilityIds.Add("power_strike");
            AbilitySourceChips["power_strike"] = -1;
        }
        if (Stray.Level >= 10)
        {
            innateAbilityIds.Add("fortify");
            AbilitySourceChips["fortify"] = -1;
        }

        // Add abilities from equipped microchips (new socket system)
        if (Stray.MicrochipSockets != null)
        {
            for (int socketIndex = 0; socketIndex < Stray.MicrochipSockets.Length; socketIndex++)
            {
                var socket = Stray.MicrochipSockets[socketIndex];
                if (socket?.EquippedChip?.Definition?.GrantsAbility != null)
                {
                    var abilityId = socket.EquippedChip.Definition.GrantsAbility;
                    if (!innateAbilityIds.Contains(abilityId))
                    {
                        innateAbilityIds.Add(abilityId);
                    }
                    // Track which socket this ability comes from (last one wins if duplicates)
                    AbilitySourceChips[abilityId] = socketIndex;
                }
            }
        }

        // Legacy support: Add abilities from EquippedMicrochips list
        foreach (var chipId in Stray.EquippedMicrochips)
        {
            var chipDef = Microchips.Get(chipId);
            if (chipDef?.GrantsAbility != null && !innateAbilityIds.Contains(chipDef.GrantsAbility))
            {
                innateAbilityIds.Add(chipDef.GrantsAbility);
                AbilitySourceChips[chipDef.GrantsAbility] = -2; // Legacy chip (no socket)
            }
        }

        // Add abilities from evolution
        foreach (var abilityId in Stray.EvolutionState.EvolvedAbilities)
        {
            if (!innateAbilityIds.Contains(abilityId))
            {
                innateAbilityIds.Add(abilityId);
                AbilitySourceChips[abilityId] = -1; // Evolved ability = innate
            }
        }

        // Create ability instances
        foreach (var abilityId in innateAbilityIds.Distinct())
        {
            var def = Combat.Abilities.Get(abilityId);
            if (def != null)
            {
                Abilities.Add(new Ability(def));
            }
        }
    }

    /// <summary>
    /// Gets abilities that are currently usable (has energy, not on cooldown, chip not overheated).
    /// </summary>
    public IEnumerable<Ability> GetUsableAbilities()
    {
        return Abilities.Where(a => a.IsReady &&
            a.Definition.EnergyCost <= CurrentEnergy &&
            !IsAbilityOverheated(a.Definition.Id));
    }

    /// <summary>
    /// Checks if an ability's source chip is overheated.
    /// </summary>
    public bool IsAbilityOverheated(string abilityId)
    {
        if (!AbilitySourceChips.TryGetValue(abilityId, out var socketIndex))
            return false;

        // Innate abilities (-1) and legacy chips (-2) don't overheat
        if (socketIndex < 0)
            return false;

        // Check if the chip in that socket is overheated
        if (Stray.MicrochipSockets == null || socketIndex >= Stray.MicrochipSockets.Length)
            return false;

        var socket = Stray.MicrochipSockets[socketIndex];
        return socket?.EquippedChip?.IsOverheated == true;
    }

    /// <summary>
    /// Gets the current heat of an ability's source chip.
    /// </summary>
    public (float current, float max) GetAbilityHeat(string abilityId)
    {
        if (!AbilitySourceChips.TryGetValue(abilityId, out var socketIndex))
            return (0, 0);

        if (socketIndex < 0)
            return (0, 0);

        if (Stray.MicrochipSockets == null || socketIndex >= Stray.MicrochipSockets.Length)
            return (0, 0);

        var chip = Stray.MicrochipSockets[socketIndex]?.EquippedChip;
        if (chip == null)
            return (0, 0);

        return (chip.CurrentHeat, chip.Definition.HeatMax);
    }

    /// <summary>
    /// Uses energy for an ability (delegates to Stray).
    /// </summary>
    public bool UseEnergy(int amount)
    {
        return Stray.ConsumeEnergy(amount);
    }

    /// <summary>
    /// Restores energy (adds to Stray's current energy).
    /// </summary>
    public void RestoreEnergy(int amount)
    {
        Stray.CurrentEnergy = Math.Min(Stray.MaxEnergy, Stray.CurrentEnergy + amount);
    }

    /// <summary>
    /// Applies heat to the chip that granted an ability.
    /// </summary>
    public void ApplyAbilityHeat(string abilityId)
    {
        if (!AbilitySourceChips.TryGetValue(abilityId, out var socketIndex))
            return;

        if (socketIndex < 0)
            return;

        if (Stray.MicrochipSockets == null || socketIndex >= Stray.MicrochipSockets.Length)
            return;

        var chip = Stray.MicrochipSockets[socketIndex]?.EquippedChip;
        chip?.AddHeat();
    }

    /// <summary>
    /// Awards TU to the chip that granted an ability (per-use bonus).
    /// </summary>
    public void AwardAbilityTu(string abilityId, int amount = 1)
    {
        if (!AbilitySourceChips.TryGetValue(abilityId, out var socketIndex))
            return;

        if (socketIndex < 0)
            return;

        if (Stray.MicrochipSockets == null || socketIndex >= Stray.MicrochipSockets.Length)
            return;

        var chip = Stray.MicrochipSockets[socketIndex]?.EquippedChip;
        chip?.AddTu(amount);
    }

    /// <summary>
    /// Applies a status effect.
    /// </summary>
    public void ApplyStatus(StatusEffect status, int duration)
    {
        if (StatusEffects.ContainsKey(status))
        {
            // Extend duration if already present
            StatusEffects[status] = System.Math.Max(StatusEffects[status], duration);
        }
        else
        {
            StatusEffects[status] = duration;
        }
    }

    /// <summary>
    /// Checks if combatant has a status effect.
    /// </summary>
    public bool HasStatus(StatusEffect status)
    {
        return StatusEffects.ContainsKey(status) && StatusEffects[status] > 0;
    }

    /// <summary>
    /// Removes a status effect.
    /// </summary>
    public void RemoveStatus(StatusEffect status)
    {
        StatusEffects.Remove(status);
    }

    /// <summary>
    /// Ticks all status effects and abilities at turn end.
    /// </summary>
    public int TickStatusEffects()
    {
        int damage = 0;

        // Process damage-over-time effects
        if (HasStatus(StatusEffect.Poison))
        {
            damage += MaxHp / 16; // 6.25% max HP
        }
        if (HasStatus(StatusEffect.Burn))
        {
            damage += MaxHp / 12; // ~8% max HP
        }

        // Process healing effects
        if (HasStatus(StatusEffect.Regen))
        {
            Heal(MaxHp / 10); // 10% max HP
        }

        // Tick down durations
        var expiredEffects = new List<StatusEffect>();
        foreach (var kvp in StatusEffects.ToList())
        {
            StatusEffects[kvp.Key] = kvp.Value - 1;
            if (StatusEffects[kvp.Key] <= 0)
            {
                expiredEffects.Add(kvp.Key);
            }
        }

        foreach (var effect in expiredEffects)
        {
            StatusEffects.Remove(effect);
        }

        // Tick ability cooldowns
        foreach (var ability in Abilities)
        {
            ability.TickCooldown();
        }

        // Note: Energy regeneration and heat dissipation are handled in UpdateAtb()
        // which runs continuously during ATB gauge filling.

        return damage;
    }

    /// <summary>
    /// Accumulator for energy regeneration ticks.
    /// </summary>
    private float _energyTickAccumulator = 0f;

    /// <summary>
    /// Accumulator for heat dissipation ticks.
    /// </summary>
    private float _heatTickAccumulator = 0f;

    /// <summary>
    /// How often energy regenerates (in seconds).
    /// </summary>
    private const float EnergyTickInterval = 1.0f;

    /// <summary>
    /// How often heat dissipates (in seconds).
    /// </summary>
    private const float HeatTickInterval = 0.5f;

    /// <summary>
    /// Updates the ATB gauge based on speed, and handles energy regen/heat dissipation.
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

        // Energy regeneration per tick interval
        _energyTickAccumulator += deltaTime;
        while (_energyTickAccumulator >= EnergyTickInterval)
        {
            _energyTickAccumulator -= EnergyTickInterval;
            Stray.RegenerateEnergy();
        }

        // Heat dissipation per tick interval
        _heatTickAccumulator += deltaTime;
        while (_heatTickAccumulator >= HeatTickInterval)
        {
            _heatTickAccumulator -= HeatTickInterval;
            Stray.DissipateChipHeat();
        }
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
