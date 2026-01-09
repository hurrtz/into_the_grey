using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Effects;
using Strays.Core.Game.Data;

namespace Strays.Core.Game.Entities;

/// <summary>
/// Phases of an evolution transformation sequence.
/// </summary>
public enum EvolutionPhase
{
    /// <summary>
    /// Not currently evolving.
    /// </summary>
    None,

    /// <summary>
    /// Initial glow/buildup.
    /// </summary>
    Charging,

    /// <summary>
    /// Peak transformation flash.
    /// </summary>
    Transforming,

    /// <summary>
    /// Revealing new form.
    /// </summary>
    Revealing,

    /// <summary>
    /// Stats updating.
    /// </summary>
    StatsChanging,

    /// <summary>
    /// Complete - can dismiss.
    /// </summary>
    Complete
}

/// <summary>
/// Stats change during evolution.
/// </summary>
public class EvolutionStatChange
{
    /// <summary>
    /// Stat name.
    /// </summary>
    public string StatName { get; init; } = "";

    /// <summary>
    /// Old value.
    /// </summary>
    public int OldValue { get; init; }

    /// <summary>
    /// New value.
    /// </summary>
    public int NewValue { get; init; }

    /// <summary>
    /// Change amount.
    /// </summary>
    public int Change => NewValue - OldValue;

    /// <summary>
    /// Display timer for animation.
    /// </summary>
    public float DisplayProgress { get; set; } = 0f;
}

/// <summary>
/// Visual effect configuration for evolution.
/// </summary>
public class EvolutionEffect
{
    /// <summary>
    /// The phase this effect plays during.
    /// </summary>
    public EvolutionPhase Phase { get; init; }

    /// <summary>
    /// Particle effect type.
    /// </summary>
    public ParticleEffectType ParticleType { get; init; }

    /// <summary>
    /// Number of particles.
    /// </summary>
    public int ParticleCount { get; init; }

    /// <summary>
    /// Primary color.
    /// </summary>
    public Color Color { get; init; }

    /// <summary>
    /// Duration in seconds.
    /// </summary>
    public float Duration { get; init; }
}

/// <summary>
/// Manages evolution visual sequences and stat transitions.
/// </summary>
public class EvolutionSystem
{
    private readonly Random _random = new();

    /// <summary>
    /// Current evolution phase.
    /// </summary>
    public EvolutionPhase CurrentPhase { get; private set; } = EvolutionPhase.None;

    /// <summary>
    /// Timer for current phase.
    /// </summary>
    public float PhaseTimer { get; private set; } = 0f;

    /// <summary>
    /// Total evolution sequence time.
    /// </summary>
    public float TotalTime { get; private set; } = 0f;

    /// <summary>
    /// The Stray being evolved.
    /// </summary>
    public Stray? EvolvingStray { get; private set; }

    /// <summary>
    /// Old definition (before evolution).
    /// </summary>
    public StrayDefinition? OldDefinition { get; private set; }

    /// <summary>
    /// New definition (after evolution).
    /// </summary>
    public StrayDefinition? NewDefinition { get; private set; }

    /// <summary>
    /// Stat changes to display.
    /// </summary>
    public List<EvolutionStatChange> StatChanges { get; } = new();

    /// <summary>
    /// Current stat being animated.
    /// </summary>
    public int CurrentStatIndex { get; private set; } = 0;

    /// <summary>
    /// Whether the evolution is complete.
    /// </summary>
    public bool IsComplete => CurrentPhase == EvolutionPhase.Complete;

    /// <summary>
    /// Whether evolution is in progress.
    /// </summary>
    public bool IsActive => CurrentPhase != EvolutionPhase.None;

    /// <summary>
    /// Position for effects.
    /// </summary>
    public Vector2 EffectPosition { get; set; }

    /// <summary>
    /// Glow intensity (0-1).
    /// </summary>
    public float GlowIntensity { get; private set; } = 0f;

    /// <summary>
    /// Flash intensity (0-1).
    /// </summary>
    public float FlashIntensity { get; private set; } = 0f;

    /// <summary>
    /// Scale multiplier for transformation visual.
    /// </summary>
    public float ScaleMultiplier { get; private set; } = 1f;

    /// <summary>
    /// Rotation for transformation effect.
    /// </summary>
    public float Rotation { get; private set; } = 0f;

    /// <summary>
    /// Event fired when evolution starts.
    /// </summary>
    public event EventHandler<EvolutionEventArgs>? EvolutionStarted;

    /// <summary>
    /// Event fired when phase changes.
    /// </summary>
    public event EventHandler<EvolutionPhase>? PhaseChanged;

    /// <summary>
    /// Event fired when evolution completes.
    /// </summary>
    public event EventHandler<EvolutionEventArgs>? EvolutionCompleted;

    /// <summary>
    /// Event fired when particles should be emitted.
    /// </summary>
    public event EventHandler<EvolutionEffect>? EmitParticles;

    // Phase durations
    private const float ChargingDuration = 1.5f;
    private const float TransformingDuration = 0.5f;
    private const float RevealingDuration = 1.0f;
    private const float StatsChangeDuration = 2.0f;
    private const float StatDisplayTime = 0.3f;

    /// <summary>
    /// Starts an evolution sequence.
    /// </summary>
    public void StartEvolution(Stray stray, StrayDefinition newDefinition)
    {
        if (IsActive)
        {
            return;
        }

        EvolvingStray = stray;
        OldDefinition = stray.Definition;
        NewDefinition = newDefinition;

        // Calculate stat changes
        CalculateStatChanges();

        // Start the sequence
        CurrentPhase = EvolutionPhase.Charging;
        PhaseTimer = 0f;
        TotalTime = 0f;
        CurrentStatIndex = 0;

        // Reset visual state
        GlowIntensity = 0f;
        FlashIntensity = 0f;
        ScaleMultiplier = 1f;
        Rotation = 0f;

        EvolutionStarted?.Invoke(this, new EvolutionEventArgs
        {
            Stray = stray,
            OldDefinition = OldDefinition,
            NewDefinition = NewDefinition
        });

        PhaseChanged?.Invoke(this, CurrentPhase);
    }

    /// <summary>
    /// Calculates stat changes for display.
    /// </summary>
    private void CalculateStatChanges()
    {
        StatChanges.Clear();

        if (OldDefinition == null || NewDefinition == null)
        {
            return;
        }

        var oldStats = OldDefinition.BaseStats;
        var newStats = NewDefinition.BaseStats;

        StatChanges.Add(new EvolutionStatChange
        {
            StatName = "Max HP",
            OldValue = oldStats.MaxHp,
            NewValue = newStats.MaxHp
        });

        StatChanges.Add(new EvolutionStatChange
        {
            StatName = "Attack",
            OldValue = oldStats.Attack,
            NewValue = newStats.Attack
        });

        StatChanges.Add(new EvolutionStatChange
        {
            StatName = "Defense",
            OldValue = oldStats.Defense,
            NewValue = newStats.Defense
        });

        StatChanges.Add(new EvolutionStatChange
        {
            StatName = "Speed",
            OldValue = oldStats.Speed,
            NewValue = newStats.Speed
        });

        StatChanges.Add(new EvolutionStatChange
        {
            StatName = "Special",
            OldValue = oldStats.Special,
            NewValue = newStats.Special
        });
    }

    /// <summary>
    /// Updates the evolution sequence.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (!IsActive)
        {
            return;
        }

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        PhaseTimer += deltaTime;
        TotalTime += deltaTime;

        switch (CurrentPhase)
        {
            case EvolutionPhase.Charging:
                UpdateCharging(deltaTime);
                break;

            case EvolutionPhase.Transforming:
                UpdateTransforming(deltaTime);
                break;

            case EvolutionPhase.Revealing:
                UpdateRevealing(deltaTime);
                break;

            case EvolutionPhase.StatsChanging:
                UpdateStatsChanging(deltaTime);
                break;
        }
    }

    /// <summary>
    /// Updates the charging phase.
    /// </summary>
    private void UpdateCharging(float deltaTime)
    {
        // Gradual glow buildup
        GlowIntensity = Math.Min(1f, PhaseTimer / ChargingDuration);

        // Slight scale pulsing
        ScaleMultiplier = 1f + 0.1f * (float)Math.Sin(PhaseTimer * 8);

        // Emit sparkle particles periodically
        if (PhaseTimer % 0.3f < deltaTime)
        {
            EmitParticles?.Invoke(this, new EvolutionEffect
            {
                Phase = EvolutionPhase.Charging,
                ParticleType = ParticleEffectType.Sparkles,
                ParticleCount = 5,
                Color = GetEvolutionColor(),
                Duration = 0.5f
            });
        }

        // Transition to transforming
        if (PhaseTimer >= ChargingDuration)
        {
            TransitionToPhase(EvolutionPhase.Transforming);

            // Big flash and explosion effect
            EmitParticles?.Invoke(this, new EvolutionEffect
            {
                Phase = EvolutionPhase.Transforming,
                ParticleType = ParticleEffectType.Explosions,
                ParticleCount = 30,
                Color = Color.White,
                Duration = TransformingDuration
            });
        }
    }

    /// <summary>
    /// Updates the transforming phase.
    /// </summary>
    private void UpdateTransforming(float deltaTime)
    {
        // Intense flash
        FlashIntensity = 1f - (PhaseTimer / TransformingDuration);

        // Scale up dramatically
        ScaleMultiplier = 1f + 0.5f * (1f - PhaseTimer / TransformingDuration);

        // Rapid rotation
        Rotation += deltaTime * 20f;

        // Transition to revealing
        if (PhaseTimer >= TransformingDuration)
        {
            TransitionToPhase(EvolutionPhase.Revealing);

            // Apply the actual evolution
            ApplyEvolution();

            // Fireworks effect
            EmitParticles?.Invoke(this, new EvolutionEffect
            {
                Phase = EvolutionPhase.Revealing,
                ParticleType = ParticleEffectType.Fireworks,
                ParticleCount = 50,
                Color = GetEvolutionColor(),
                Duration = RevealingDuration
            });
        }
    }

    /// <summary>
    /// Updates the revealing phase.
    /// </summary>
    private void UpdateRevealing(float deltaTime)
    {
        // Glow settles down
        GlowIntensity = 0.5f + 0.5f * (1f - PhaseTimer / RevealingDuration);

        // Scale settles to normal
        ScaleMultiplier = 1f + 0.2f * (1f - PhaseTimer / RevealingDuration);

        // Rotation slows
        Rotation += deltaTime * 10f * (1f - PhaseTimer / RevealingDuration);

        // Transition to stats changing
        if (PhaseTimer >= RevealingDuration)
        {
            TransitionToPhase(EvolutionPhase.StatsChanging);
        }
    }

    /// <summary>
    /// Updates the stats changing phase.
    /// </summary>
    private void UpdateStatsChanging(float deltaTime)
    {
        GlowIntensity = 0.3f;
        ScaleMultiplier = 1f;
        Rotation = 0f;

        // Animate stat displays one by one
        float timePerStat = StatsChangeDuration / StatChanges.Count;
        int newStatIndex = (int)(PhaseTimer / timePerStat);

        if (newStatIndex > CurrentStatIndex && CurrentStatIndex < StatChanges.Count - 1)
        {
            CurrentStatIndex = Math.Min(newStatIndex, StatChanges.Count - 1);
        }

        // Update progress on current stat
        if (CurrentStatIndex < StatChanges.Count)
        {
            float statLocalTime = PhaseTimer - (CurrentStatIndex * timePerStat);
            StatChanges[CurrentStatIndex].DisplayProgress = Math.Min(1f, statLocalTime / StatDisplayTime);
        }

        // Complete when done
        if (PhaseTimer >= StatsChangeDuration)
        {
            TransitionToPhase(EvolutionPhase.Complete);

            // Final confetti burst
            EmitParticles?.Invoke(this, new EvolutionEffect
            {
                Phase = EvolutionPhase.Complete,
                ParticleType = ParticleEffectType.Confetti,
                ParticleCount = 40,
                Color = GetEvolutionColor(),
                Duration = 2f
            });

            EvolutionCompleted?.Invoke(this, new EvolutionEventArgs
            {
                Stray = EvolvingStray!,
                OldDefinition = OldDefinition!,
                NewDefinition = NewDefinition!
            });
        }
    }

    /// <summary>
    /// Transitions to a new phase.
    /// </summary>
    private void TransitionToPhase(EvolutionPhase newPhase)
    {
        CurrentPhase = newPhase;
        PhaseTimer = 0f;
        PhaseChanged?.Invoke(this, newPhase);
    }

    /// <summary>
    /// Applies the evolution to the Stray.
    /// </summary>
    private void ApplyEvolution()
    {
        if (EvolvingStray == null || NewDefinition == null)
        {
            return;
        }

        // Store old HP percentage to preserve relative health
        float hpPercent = (float)EvolvingStray.CurrentHp / EvolvingStray.MaxHp;

        // Update the Stray's definition
        EvolvingStray.Evolve(NewDefinition);

        // Restore HP percentage
        EvolvingStray.CurrentHp = (int)(EvolvingStray.MaxHp * hpPercent);
    }

    /// <summary>
    /// Gets the evolution color based on the Stray's category.
    /// </summary>
    private Color GetEvolutionColor()
    {
        if (NewDefinition == null)
        {
            return Color.White;
        }

        return NewDefinition.Category switch
        {
            CreatureCategory.Predatoria => Color.OrangeRed,
            CreatureCategory.Colossomammalia => Color.SaddleBrown,
            CreatureCategory.Manipularis => Color.LimeGreen,
            CreatureCategory.Marsupialis => Color.HotPink,
            CreatureCategory.Medusalia => Color.DeepSkyBlue,
            CreatureCategory.Armormammalia => Color.Silver,
            CreatureCategory.Octomorpha => Color.Purple,
            CreatureCategory.Micromammalia => Color.Yellow,
            CreatureCategory.Exoskeletalis => Color.DarkGreen,
            CreatureCategory.Mollusca => Color.LightCoral,
            CreatureCategory.Obscura => Color.Magenta,
            CreatureCategory.Tardigrada => Color.ForestGreen,
            _ => Color.White
        };
    }

    /// <summary>
    /// Dismisses the evolution screen.
    /// </summary>
    public void Dismiss()
    {
        CurrentPhase = EvolutionPhase.None;
        EvolvingStray = null;
        OldDefinition = null;
        NewDefinition = null;
        StatChanges.Clear();
        GlowIntensity = 0f;
        FlashIntensity = 0f;
        ScaleMultiplier = 1f;
        Rotation = 0f;
    }

    /// <summary>
    /// Skips to the end of the evolution sequence (for impatient players).
    /// </summary>
    public void Skip()
    {
        if (!IsActive)
        {
            return;
        }

        // Make sure evolution is applied
        if (EvolvingStray != null && NewDefinition != null && OldDefinition != null)
        {
            if (EvolvingStray.Definition != NewDefinition)
            {
                ApplyEvolution();
            }
        }

        // Show all stats
        foreach (var stat in StatChanges)
        {
            stat.DisplayProgress = 1f;
        }

        CurrentStatIndex = StatChanges.Count - 1;
        CurrentPhase = EvolutionPhase.Complete;
        GlowIntensity = 0f;
        FlashIntensity = 0f;
        ScaleMultiplier = 1f;
        Rotation = 0f;

        EvolutionCompleted?.Invoke(this, new EvolutionEventArgs
        {
            Stray = EvolvingStray!,
            OldDefinition = OldDefinition!,
            NewDefinition = NewDefinition!
        });
    }

    /// <summary>
    /// Draws the evolution effect overlay.
    /// </summary>
    public void DrawOverlay(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, Rectangle screenBounds)
    {
        if (!IsActive)
        {
            return;
        }

        // Screen flash during transformation
        if (FlashIntensity > 0)
        {
            spriteBatch.Draw(pixel, screenBounds, Color.White * FlashIntensity);
        }

        // Glow circle around subject
        if (GlowIntensity > 0)
        {
            DrawGlow(spriteBatch, pixel);
        }

        // Stats display during stats phase
        if (CurrentPhase == EvolutionPhase.StatsChanging || CurrentPhase == EvolutionPhase.Complete)
        {
            DrawStats(spriteBatch, pixel, font, screenBounds);
        }

        // "Evolution Complete!" message
        if (CurrentPhase == EvolutionPhase.Complete)
        {
            DrawCompletionMessage(spriteBatch, font, screenBounds);
        }
    }

    /// <summary>
    /// Draws the glow effect.
    /// </summary>
    private void DrawGlow(SpriteBatch spriteBatch, Texture2D pixel)
    {
        var color = GetEvolutionColor() * GlowIntensity * 0.5f;
        int glowSize = (int)(100 * GlowIntensity);
        var glowRect = new Rectangle(
            (int)EffectPosition.X - glowSize / 2,
            (int)EffectPosition.Y - glowSize / 2,
            glowSize,
            glowSize
        );

        spriteBatch.Draw(pixel, glowRect, color);
    }

    /// <summary>
    /// Draws stat changes.
    /// </summary>
    private void DrawStats(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, Rectangle screenBounds)
    {
        float statsY = screenBounds.Height * 0.3f;
        float statsX = screenBounds.Width * 0.6f;

        // Background panel
        var panelRect = new Rectangle(
            (int)statsX - 10,
            (int)statsY - 10,
            250,
            StatChanges.Count * 30 + 60
        );
        spriteBatch.Draw(pixel, panelRect, Color.Black * 0.8f);

        // Border
        spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), GetEvolutionColor());
        spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), GetEvolutionColor());

        // Title
        string titleText = NewDefinition?.Name ?? "Evolution";
        spriteBatch.DrawString(font, titleText, new Vector2(statsX, statsY), GetEvolutionColor());

        // Stats
        float lineY = statsY + 35;
        for (int i = 0; i < StatChanges.Count; i++)
        {
            var stat = StatChanges[i];
            bool isVisible = i <= CurrentStatIndex;

            if (!isVisible)
            {
                continue;
            }

            // Stat name
            spriteBatch.DrawString(font, stat.StatName, new Vector2(statsX, lineY), Color.White);

            // Old value
            var oldText = stat.OldValue.ToString();
            spriteBatch.DrawString(font, oldText, new Vector2(statsX + 100, lineY), Color.Gray);

            // Arrow
            spriteBatch.DrawString(font, "->", new Vector2(statsX + 140, lineY), Color.White);

            // New value (animated)
            if (stat.DisplayProgress >= 1f || i < CurrentStatIndex)
            {
                var changeColor = stat.Change > 0 ? Color.LimeGreen :
                                 stat.Change < 0 ? Color.Red : Color.White;
                var newText = stat.NewValue.ToString();
                spriteBatch.DrawString(font, newText, new Vector2(statsX + 170, lineY), changeColor);

                // Change indicator
                if (stat.Change != 0)
                {
                    var changeText = stat.Change > 0 ? $"+{stat.Change}" : stat.Change.ToString();
                    spriteBatch.DrawString(font, changeText, new Vector2(statsX + 210, lineY), changeColor);
                }
            }
            else
            {
                // Animating - show intermediate value
                int displayValue = stat.OldValue + (int)((stat.NewValue - stat.OldValue) * stat.DisplayProgress);
                spriteBatch.DrawString(font, displayValue.ToString(), new Vector2(statsX + 170, lineY), Color.Yellow);
            }

            lineY += 25;
        }
    }

    /// <summary>
    /// Draws the completion message.
    /// </summary>
    private void DrawCompletionMessage(SpriteBatch spriteBatch, SpriteFont font, Rectangle screenBounds)
    {
        string message = $"{OldDefinition?.Name} evolved into {NewDefinition?.Name}!";
        var messageSize = font.MeasureString(message);
        var messagePos = new Vector2(
            screenBounds.Width / 2f - messageSize.X / 2f,
            screenBounds.Height * 0.15f
        );

        // Pulsing color
        float pulse = (float)Math.Sin(TotalTime * 4) * 0.3f + 0.7f;
        spriteBatch.DrawString(font, message, messagePos, GetEvolutionColor() * pulse);

        // Press to continue
        string continueText = "Press any key to continue";
        var continueSize = font.MeasureString(continueText);
        var continuePos = new Vector2(
            screenBounds.Width / 2f - continueSize.X / 2f,
            screenBounds.Height * 0.85f
        );
        spriteBatch.DrawString(font, continueText, continuePos, Color.White * 0.7f);
    }
}

/// <summary>
/// Event args for evolution events.
/// </summary>
public class EvolutionEventArgs : EventArgs
{
    /// <summary>
    /// The Stray being evolved.
    /// </summary>
    public Stray Stray { get; init; } = null!;

    /// <summary>
    /// Definition before evolution.
    /// </summary>
    public StrayDefinition OldDefinition { get; init; } = null!;

    /// <summary>
    /// Definition after evolution.
    /// </summary>
    public StrayDefinition NewDefinition { get; init; } = null!;
}
