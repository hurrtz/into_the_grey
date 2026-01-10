using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lazarus.Core.Game.World;

/// <summary>
/// Intensity levels for weather effects.
/// </summary>
public enum WeatherIntensity
{
    /// <summary>
    /// Light effects, minimal impact.
    /// </summary>
    Light,

    /// <summary>
    /// Moderate effects.
    /// </summary>
    Moderate,

    /// <summary>
    /// Heavy effects, significant impact.
    /// </summary>
    Heavy,

    /// <summary>
    /// Extreme weather, dangerous conditions.
    /// </summary>
    Extreme
}

/// <summary>
/// A weather particle for visual effects.
/// </summary>
public class WeatherParticle
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Lifetime { get; set; }
    public float MaxLifetime { get; set; }
    public float Size { get; set; }
    public float Rotation { get; set; }
    public float RotationSpeed { get; set; }
    public Color Color { get; set; }
    public float Alpha { get; set; } = 1f;

    public bool IsAlive => Lifetime > 0;
    public float LifetimePercent => Lifetime / MaxLifetime;
}

/// <summary>
/// Configuration for weather effects.
/// </summary>
public class WeatherConfig
{
    /// <summary>
    /// Weather type.
    /// </summary>
    public WeatherType Type { get; init; }

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Particle color.
    /// </summary>
    public Color ParticleColor { get; init; } = Color.White;

    /// <summary>
    /// Secondary color for variation.
    /// </summary>
    public Color SecondaryColor { get; init; } = Color.White;

    /// <summary>
    /// Overlay tint applied to the screen.
    /// </summary>
    public Color OverlayTint { get; init; } = Color.Transparent;

    /// <summary>
    /// Overlay opacity (0-1).
    /// </summary>
    public float OverlayOpacity { get; init; } = 0f;

    /// <summary>
    /// Base particle velocity.
    /// </summary>
    public Vector2 BaseVelocity { get; init; } = Vector2.Zero;

    /// <summary>
    /// Velocity variance.
    /// </summary>
    public Vector2 VelocityVariance { get; init; } = Vector2.Zero;

    /// <summary>
    /// Base particle size.
    /// </summary>
    public float BaseSize { get; init; } = 2f;

    /// <summary>
    /// Size variance.
    /// </summary>
    public float SizeVariance { get; init; } = 1f;

    /// <summary>
    /// Particle lifetime in seconds.
    /// </summary>
    public float ParticleLifetime { get; init; } = 2f;

    /// <summary>
    /// Particles spawned per second at moderate intensity.
    /// </summary>
    public int ParticlesPerSecond { get; init; } = 50;

    /// <summary>
    /// Whether particles rotate.
    /// </summary>
    public bool ParticlesRotate { get; init; } = false;

    /// <summary>
    /// Whether this weather affects visibility.
    /// </summary>
    public bool AffectsVisibility { get; init; } = false;

    /// <summary>
    /// Visibility multiplier (1 = full, 0 = no visibility).
    /// </summary>
    public float VisibilityMultiplier { get; init; } = 1f;

    /// <summary>
    /// Whether this weather causes damage.
    /// </summary>
    public bool CausesDamage { get; init; } = false;

    /// <summary>
    /// Damage per second (at moderate intensity).
    /// </summary>
    public float DamagePerSecond { get; init; } = 0f;

    /// <summary>
    /// Combat stat modifiers during this weather.
    /// </summary>
    public Dictionary<string, float> CombatModifiers { get; init; } = new();

    /// <summary>
    /// Ambient sound ID.
    /// </summary>
    public string? AmbientSound { get; init; }
}

/// <summary>
/// Current weather state.
/// </summary>
public class WeatherState
{
    /// <summary>
    /// Current weather type.
    /// </summary>
    public WeatherType CurrentWeather { get; set; } = WeatherType.None;

    /// <summary>
    /// Current intensity.
    /// </summary>
    public WeatherIntensity Intensity { get; set; } = WeatherIntensity.Moderate;

    /// <summary>
    /// Time until weather changes.
    /// </summary>
    public float TimeUntilChange { get; set; } = 0f;

    /// <summary>
    /// Transition progress (0-1) when changing weather.
    /// </summary>
    public float TransitionProgress { get; set; } = 1f;

    /// <summary>
    /// Previous weather (for transitions).
    /// </summary>
    public WeatherType PreviousWeather { get; set; } = WeatherType.None;

    /// <summary>
    /// Whether transitioning between weather states.
    /// </summary>
    public bool IsTransitioning => TransitionProgress < 1f;
}

/// <summary>
/// Manages weather effects and environmental conditions.
/// </summary>
public class WeatherSystem
{
    private readonly Random _random = new();
    private readonly List<WeatherParticle> _particles = new();
    private float _spawnAccumulator = 0f;
    private float _damageAccumulator = 0f;

    /// <summary>
    /// Current weather state.
    /// </summary>
    public WeatherState State { get; } = new();

    /// <summary>
    /// Screen bounds for particle spawning.
    /// </summary>
    public Rectangle ScreenBounds { get; set; } = new Rectangle(0, 0, 800, 480);

    /// <summary>
    /// Current biome.
    /// </summary>
    public BiomeType CurrentBiome { get; set; } = BiomeType.Fringe;

    /// <summary>
    /// Wind direction and strength.
    /// </summary>
    public Vector2 WindVector { get; private set; } = Vector2.Zero;

    /// <summary>
    /// Event fired when weather changes.
    /// </summary>
    public event EventHandler<WeatherType>? WeatherChanged;

    /// <summary>
    /// Event fired when weather causes damage.
    /// </summary>
    public event EventHandler<float>? WeatherDamage;

    /// <summary>
    /// Weather configurations.
    /// </summary>
    private static readonly Dictionary<WeatherType, WeatherConfig> _configs = new()
    {
        {
            WeatherType.None, new WeatherConfig
            {
                Type = WeatherType.None,
                Name = "Clear",
                ParticlesPerSecond = 0
            }
        },
        {
            WeatherType.Fog, new WeatherConfig
            {
                Type = WeatherType.Fog,
                Name = "Fog",
                ParticleColor = Color.LightGray,
                OverlayTint = Color.Gray,
                OverlayOpacity = 0.3f,
                BaseVelocity = new Vector2(20, 5),
                VelocityVariance = new Vector2(10, 5),
                BaseSize = 30f,
                SizeVariance = 20f,
                ParticleLifetime = 8f,
                ParticlesPerSecond = 10,
                AffectsVisibility = true,
                VisibilityMultiplier = 0.6f,
                AmbientSound = "weather_fog"
            }
        },
        {
            WeatherType.Rain, new WeatherConfig
            {
                Type = WeatherType.Rain,
                Name = "Rain",
                ParticleColor = new Color(150, 180, 220),
                SecondaryColor = new Color(120, 150, 200),
                OverlayTint = new Color(50, 70, 100),
                OverlayOpacity = 0.15f,
                BaseVelocity = new Vector2(50, 400),
                VelocityVariance = new Vector2(20, 50),
                BaseSize = 2f,
                SizeVariance = 1f,
                ParticleLifetime = 1.5f,
                ParticlesPerSecond = 200,
                AffectsVisibility = true,
                VisibilityMultiplier = 0.85f,
                AmbientSound = "weather_rain",
                CombatModifiers = new() { { "electric_damage", 1.3f }, { "fire_damage", 0.7f } }
            }
        },
        {
            WeatherType.AcidRain, new WeatherConfig
            {
                Type = WeatherType.AcidRain,
                Name = "Acid Rain",
                ParticleColor = new Color(150, 255, 100),
                SecondaryColor = new Color(200, 255, 50),
                OverlayTint = new Color(100, 150, 50),
                OverlayOpacity = 0.2f,
                BaseVelocity = new Vector2(30, 350),
                VelocityVariance = new Vector2(20, 40),
                BaseSize = 3f,
                SizeVariance = 1f,
                ParticleLifetime = 1.5f,
                ParticlesPerSecond = 150,
                CausesDamage = true,
                DamagePerSecond = 2f,
                AffectsVisibility = true,
                VisibilityMultiplier = 0.75f,
                AmbientSound = "weather_acid_rain",
                CombatModifiers = new() { { "toxic_damage", 1.5f }, { "defense", 0.9f } }
            }
        },
        {
            WeatherType.Dust, new WeatherConfig
            {
                Type = WeatherType.Dust,
                Name = "Dust Storm",
                ParticleColor = new Color(180, 150, 100),
                SecondaryColor = new Color(150, 120, 80),
                OverlayTint = new Color(140, 110, 70),
                OverlayOpacity = 0.25f,
                BaseVelocity = new Vector2(200, 30),
                VelocityVariance = new Vector2(80, 50),
                BaseSize = 4f,
                SizeVariance = 3f,
                ParticleLifetime = 3f,
                ParticlesPerSecond = 100,
                ParticlesRotate = true,
                AffectsVisibility = true,
                VisibilityMultiplier = 0.5f,
                AmbientSound = "weather_dust",
                CombatModifiers = new() { { "accuracy", 0.8f } }
            }
        },
        {
            WeatherType.Snow, new WeatherConfig
            {
                Type = WeatherType.Snow,
                Name = "Snow",
                ParticleColor = Color.White,
                SecondaryColor = new Color(220, 230, 255),
                OverlayTint = new Color(200, 220, 255),
                OverlayOpacity = 0.1f,
                BaseVelocity = new Vector2(20, 80),
                VelocityVariance = new Vector2(40, 20),
                BaseSize = 3f,
                SizeVariance = 2f,
                ParticleLifetime = 5f,
                ParticlesPerSecond = 80,
                ParticlesRotate = true,
                AffectsVisibility = true,
                VisibilityMultiplier = 0.8f,
                AmbientSound = "weather_snow",
                CombatModifiers = new() { { "speed", 0.9f }, { "ice_damage", 1.2f } }
            }
        },
        {
            WeatherType.DataStorm, new WeatherConfig
            {
                Type = WeatherType.DataStorm,
                Name = "Data Storm",
                ParticleColor = Color.Cyan,
                SecondaryColor = Color.Magenta,
                OverlayTint = new Color(0, 150, 200),
                OverlayOpacity = 0.2f,
                BaseVelocity = new Vector2(100, 150),
                VelocityVariance = new Vector2(150, 100),
                BaseSize = 2f,
                SizeVariance = 2f,
                ParticleLifetime = 0.8f,
                ParticlesPerSecond = 250,
                ParticlesRotate = true,
                CausesDamage = true,
                DamagePerSecond = 1f,
                AffectsVisibility = true,
                VisibilityMultiplier = 0.7f,
                AmbientSound = "weather_data_storm",
                CombatModifiers = new() { { "psionic_damage", 1.4f }, { "electric_damage", 1.3f } }
            }
        },
        {
            WeatherType.RadiationWind, new WeatherConfig
            {
                Type = WeatherType.RadiationWind,
                Name = "Radiation Wind",
                ParticleColor = new Color(255, 255, 100),
                SecondaryColor = new Color(200, 255, 50),
                OverlayTint = new Color(200, 200, 50),
                OverlayOpacity = 0.15f,
                BaseVelocity = new Vector2(150, 20),
                VelocityVariance = new Vector2(50, 30),
                BaseSize = 5f,
                SizeVariance = 3f,
                ParticleLifetime = 2f,
                ParticlesPerSecond = 60,
                CausesDamage = true,
                DamagePerSecond = 3f,
                AffectsVisibility = true,
                VisibilityMultiplier = 0.9f,
                AmbientSound = "weather_radiation",
                CombatModifiers = new() { { "max_hp", 0.95f }, { "corruption_damage", 1.5f } }
            }
        }
    };

    /// <summary>
    /// Updates the weather system.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update weather change timer
        if (State.TimeUntilChange > 0)
        {
            State.TimeUntilChange -= deltaTime;

            if (State.TimeUntilChange <= 0)
            {
                TriggerWeatherChange();
            }
        }

        // Update transition
        if (State.IsTransitioning)
        {
            State.TransitionProgress += deltaTime * 0.5f; // 2 second transition
            State.TransitionProgress = Math.Min(1f, State.TransitionProgress);
        }

        // Update wind
        UpdateWind(deltaTime);

        // Spawn particles
        SpawnParticles(deltaTime);

        // Update particles
        UpdateParticles(deltaTime);

        // Apply weather damage
        ApplyWeatherDamage(deltaTime);
    }

    /// <summary>
    /// Updates wind direction.
    /// </summary>
    private void UpdateWind(float deltaTime)
    {
        var config = GetCurrentConfig();
        var targetWind = config.BaseVelocity * GetIntensityMultiplier();

        // Smoothly interpolate wind
        WindVector = Vector2.Lerp(WindVector, targetWind, deltaTime * 2f);

        // Add some randomness
        WindVector += new Vector2(
            (_random.NextSingle() - 0.5f) * 10f,
            (_random.NextSingle() - 0.5f) * 5f
        ) * deltaTime;
    }

    /// <summary>
    /// Spawns weather particles.
    /// </summary>
    private void SpawnParticles(float deltaTime)
    {
        var config = GetCurrentConfig();

        if (config.ParticlesPerSecond <= 0)
        {
            return;
        }

        float intensityMult = GetIntensityMultiplier();
        float spawnRate = config.ParticlesPerSecond * intensityMult;

        _spawnAccumulator += spawnRate * deltaTime;

        while (_spawnAccumulator >= 1f)
        {
            _spawnAccumulator -= 1f;
            SpawnParticle(config);
        }
    }

    /// <summary>
    /// Spawns a single particle.
    /// </summary>
    private void SpawnParticle(WeatherConfig config)
    {
        // Determine spawn position (top or side of screen depending on velocity)
        Vector2 position;

        if (Math.Abs(config.BaseVelocity.Y) > Math.Abs(config.BaseVelocity.X))
        {
            // Vertical weather (rain, snow) - spawn from top
            position = new Vector2(
                _random.Next(ScreenBounds.Left - 50, ScreenBounds.Right + 50),
                ScreenBounds.Top - 20
            );
        }
        else
        {
            // Horizontal weather (dust, wind) - spawn from side
            position = new Vector2(
                config.BaseVelocity.X > 0 ? ScreenBounds.Left - 20 : ScreenBounds.Right + 20,
                _random.Next(ScreenBounds.Top - 50, ScreenBounds.Bottom + 50)
            );
        }

        // Calculate velocity with variance
        var velocity = config.BaseVelocity + new Vector2(
            (_random.NextSingle() - 0.5f) * 2f * config.VelocityVariance.X,
            (_random.NextSingle() - 0.5f) * 2f * config.VelocityVariance.Y
        );

        // Apply wind
        velocity += WindVector * 0.3f;

        // Choose color
        var color = _random.NextDouble() > 0.5 ? config.ParticleColor : config.SecondaryColor;

        var particle = new WeatherParticle
        {
            Position = position,
            Velocity = velocity,
            Lifetime = config.ParticleLifetime * (0.8f + _random.NextSingle() * 0.4f),
            MaxLifetime = config.ParticleLifetime,
            Size = config.BaseSize + (_random.NextSingle() - 0.5f) * 2f * config.SizeVariance,
            Rotation = config.ParticlesRotate ? _random.NextSingle() * MathHelper.TwoPi : 0f,
            RotationSpeed = config.ParticlesRotate ? (_random.NextSingle() - 0.5f) * 4f : 0f,
            Color = color,
            Alpha = 1f
        };

        _particles.Add(particle);
    }

    /// <summary>
    /// Updates all particles.
    /// </summary>
    private void UpdateParticles(float deltaTime)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];

            // Update position
            particle.Position += particle.Velocity * deltaTime;

            // Update rotation
            particle.Rotation += particle.RotationSpeed * deltaTime;

            // Update lifetime
            particle.Lifetime -= deltaTime;

            // Fade out near end of lifetime
            if (particle.LifetimePercent < 0.2f)
            {
                particle.Alpha = particle.LifetimePercent / 0.2f;
            }

            // Remove dead particles
            if (!particle.IsAlive)
            {
                _particles.RemoveAt(i);
            }
        }

        // Cap particle count
        while (_particles.Count > 2000)
        {
            _particles.RemoveAt(0);
        }
    }

    /// <summary>
    /// Applies weather damage if applicable.
    /// </summary>
    private void ApplyWeatherDamage(float deltaTime)
    {
        var config = GetCurrentConfig();

        if (!config.CausesDamage)
        {
            return;
        }

        float damage = config.DamagePerSecond * GetIntensityMultiplier() * deltaTime;
        _damageAccumulator += damage;

        if (_damageAccumulator >= 1f)
        {
            int wholeDamage = (int)_damageAccumulator;
            _damageAccumulator -= wholeDamage;
            WeatherDamage?.Invoke(this, wholeDamage);
        }
    }

    /// <summary>
    /// Triggers a weather change based on current biome.
    /// </summary>
    private void TriggerWeatherChange()
    {
        var biomeData = BiomeData.GetDefinition(CurrentBiome);
        var possibleWeather = biomeData.PossibleWeather;

        if (possibleWeather.Count == 0)
        {
            SetWeather(WeatherType.None, WeatherIntensity.Light);
            return;
        }

        // Chance to clear up
        if (State.CurrentWeather != WeatherType.None && _random.NextDouble() < 0.3)
        {
            SetWeather(WeatherType.None, WeatherIntensity.Light);
            return;
        }

        // Pick random weather
        var newWeather = possibleWeather[_random.Next(possibleWeather.Count)];

        // Pick random intensity
        var intensity = (WeatherIntensity)_random.Next(0, 4);

        SetWeather(newWeather, intensity);
    }

    /// <summary>
    /// Sets the current weather.
    /// </summary>
    public void SetWeather(WeatherType weather, WeatherIntensity intensity = WeatherIntensity.Moderate)
    {
        if (weather != State.CurrentWeather)
        {
            State.PreviousWeather = State.CurrentWeather;
            State.TransitionProgress = 0f;
        }

        State.CurrentWeather = weather;
        State.Intensity = intensity;

        // Set time until next change (3-10 minutes)
        State.TimeUntilChange = 180f + _random.NextSingle() * 420f;

        WeatherChanged?.Invoke(this, weather);
    }

    /// <summary>
    /// Forces an immediate weather change.
    /// </summary>
    public void ForceWeatherChange()
    {
        State.TimeUntilChange = 0;
    }

    /// <summary>
    /// Gets the current weather config.
    /// </summary>
    public WeatherConfig GetCurrentConfig()
    {
        return _configs.TryGetValue(State.CurrentWeather, out var config)
            ? config
            : _configs[WeatherType.None];
    }

    /// <summary>
    /// Gets the intensity multiplier.
    /// </summary>
    public float GetIntensityMultiplier()
    {
        return State.Intensity switch
        {
            WeatherIntensity.Light => 0.5f,
            WeatherIntensity.Moderate => 1.0f,
            WeatherIntensity.Heavy => 1.5f,
            WeatherIntensity.Extreme => 2.0f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets the current visibility multiplier.
    /// </summary>
    public float GetVisibilityMultiplier()
    {
        var config = GetCurrentConfig();

        if (!config.AffectsVisibility)
        {
            return 1f;
        }

        float base_visibility = config.VisibilityMultiplier;
        float intensityEffect = (1f - base_visibility) * (GetIntensityMultiplier() - 1f) * 0.5f;

        return Math.Max(0.3f, base_visibility - intensityEffect);
    }

    /// <summary>
    /// Gets combat modifier for a stat.
    /// </summary>
    public float GetCombatModifier(string statName)
    {
        var config = GetCurrentConfig();

        if (config.CombatModifiers.TryGetValue(statName, out float modifier))
        {
            // Scale by intensity
            float deviation = modifier - 1f;
            return 1f + deviation * GetIntensityMultiplier();
        }

        return 1f;
    }

    /// <summary>
    /// Draws weather effects.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        var config = GetCurrentConfig();

        // Draw overlay
        if (config.OverlayOpacity > 0)
        {
            float opacity = config.OverlayOpacity * GetIntensityMultiplier() * State.TransitionProgress;
            spriteBatch.Draw(pixel, ScreenBounds, config.OverlayTint * opacity);
        }

        // Draw particles
        foreach (var particle in _particles)
        {
            if (!particle.IsAlive)
            {
                continue;
            }

            var rect = new Rectangle(
                (int)(particle.Position.X - particle.Size / 2),
                (int)(particle.Position.Y - particle.Size / 2),
                (int)particle.Size,
                (int)(particle.Size * (config.Type == WeatherType.Rain ? 4f : 1f)) // Rain is elongated
            );

            var color = particle.Color * particle.Alpha * State.TransitionProgress;

            if (config.ParticlesRotate)
            {
                var origin = new Vector2(particle.Size / 2, particle.Size / 2);
                spriteBatch.Draw(pixel, particle.Position, null, color, particle.Rotation, origin, particle.Size / 2f, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(pixel, rect, color);
            }
        }
    }

    /// <summary>
    /// Draws weather UI indicator.
    /// </summary>
    public void DrawIndicator(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, Vector2 position)
    {
        if (State.CurrentWeather == WeatherType.None)
        {
            return;
        }

        var config = GetCurrentConfig();

        // Background
        string text = $"{config.Name} ({State.Intensity})";
        var textSize = font.MeasureString(text);
        var bgRect = new Rectangle(
            (int)position.X - 5,
            (int)position.Y - 2,
            (int)textSize.X + 10,
            (int)textSize.Y + 4
        );

        spriteBatch.Draw(pixel, bgRect, Color.Black * 0.5f);

        // Weather icon color indicator
        var iconRect = new Rectangle((int)position.X, (int)position.Y + 2, 8, (int)textSize.Y - 4);
        spriteBatch.Draw(pixel, iconRect, config.ParticleColor);

        // Text
        var textPos = new Vector2(position.X + 12, position.Y);
        spriteBatch.DrawString(font, text, textPos, Color.White);

        // Damage warning
        if (config.CausesDamage)
        {
            var warningText = "!";
            var warningPos = new Vector2(bgRect.Right + 5, position.Y);
            spriteBatch.DrawString(font, warningText, warningPos, Color.Red);
        }
    }
}
