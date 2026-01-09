using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Strays.Core.Accessibility;

/// <summary>
/// Colorblind mode types.
/// </summary>
public enum ColorblindMode
{
    /// <summary>
    /// No color correction.
    /// </summary>
    None,

    /// <summary>
    /// Protanopia - reduced sensitivity to red light.
    /// </summary>
    Protanopia,

    /// <summary>
    /// Deuteranopia - reduced sensitivity to green light.
    /// </summary>
    Deuteranopia,

    /// <summary>
    /// Tritanopia - reduced sensitivity to blue light.
    /// </summary>
    Tritanopia,

    /// <summary>
    /// High contrast mode.
    /// </summary>
    HighContrast
}

/// <summary>
/// Font size presets.
/// </summary>
public enum FontSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

/// <summary>
/// Screen shake intensity options.
/// </summary>
public enum ScreenShakeIntensity
{
    Off,
    Low,
    Medium,
    High
}

/// <summary>
/// Combat speed options.
/// </summary>
public enum CombatSpeed
{
    Slow,
    Normal,
    Fast
}

/// <summary>
/// Accessibility settings for the game.
/// </summary>
public class AccessibilitySettings : INotifyPropertyChanged
{
    // Visual Settings
    private ColorblindMode _colorblindMode = ColorblindMode.None;
    private FontSize _fontSize = FontSize.Medium;
    private float _uiScale = 1.0f;
    private bool _highContrastUI = false;
    private ScreenShakeIntensity _screenShake = ScreenShakeIntensity.Medium;
    private bool _reduceMotion = false;
    private bool _flashingEffectsEnabled = true;
    private float _brightness = 1.0f;
    private float _contrast = 1.0f;

    // Audio Settings
    private bool _subtitlesEnabled = true;
    private bool _closedCaptionsEnabled = false;
    private bool _audioDescriptionEnabled = false;
    private float _dialogueVolume = 1.0f;
    private bool _monoAudio = false;

    // Input Settings
    private bool _holdToConfirm = false;
    private float _holdDuration = 0.5f;
    private bool _stickyKeys = false;
    private float _inputDelay = 0f;
    private bool _autoTarget = true;
    private CombatSpeed _combatSpeed = CombatSpeed.Normal;
    private bool _autoAdvanceText = false;
    private float _textSpeed = 1.0f;

    // Gameplay Assists
    private bool _invincibilityMode = false;
    private bool _skipCombat = false;
    private bool _autoHeal = false;
    private bool _navigationAssist = false;
    private bool _questMarkers = true;
    private bool _extendedTimers = false;

    #region Visual Properties

    /// <summary>
    /// Colorblind correction mode.
    /// </summary>
    public ColorblindMode ColorblindMode
    {
        get => _colorblindMode;
        set
        {
            if (_colorblindMode != value)
            {
                _colorblindMode = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Font size preset.
    /// </summary>
    public FontSize FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// UI scale multiplier (0.75-2.0).
    /// </summary>
    public float UIScale
    {
        get => _uiScale;
        set
        {
            value = MathHelper.Clamp(value, 0.75f, 2.0f);

            if (Math.Abs(_uiScale - value) > 0.001f)
            {
                _uiScale = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// High contrast UI mode.
    /// </summary>
    public bool HighContrastUI
    {
        get => _highContrastUI;
        set
        {
            if (_highContrastUI != value)
            {
                _highContrastUI = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Screen shake intensity.
    /// </summary>
    public ScreenShakeIntensity ScreenShake
    {
        get => _screenShake;
        set
        {
            if (_screenShake != value)
            {
                _screenShake = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Reduce motion effects.
    /// </summary>
    public bool ReduceMotion
    {
        get => _reduceMotion;
        set
        {
            if (_reduceMotion != value)
            {
                _reduceMotion = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Allow flashing effects.
    /// </summary>
    public bool FlashingEffectsEnabled
    {
        get => _flashingEffectsEnabled;
        set
        {
            if (_flashingEffectsEnabled != value)
            {
                _flashingEffectsEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Screen brightness (0.5-1.5).
    /// </summary>
    public float Brightness
    {
        get => _brightness;
        set
        {
            value = MathHelper.Clamp(value, 0.5f, 1.5f);

            if (Math.Abs(_brightness - value) > 0.001f)
            {
                _brightness = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Screen contrast (0.5-1.5).
    /// </summary>
    public float Contrast
    {
        get => _contrast;
        set
        {
            value = MathHelper.Clamp(value, 0.5f, 1.5f);

            if (Math.Abs(_contrast - value) > 0.001f)
            {
                _contrast = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Audio Properties

    /// <summary>
    /// Show subtitles for dialogue.
    /// </summary>
    public bool SubtitlesEnabled
    {
        get => _subtitlesEnabled;
        set
        {
            if (_subtitlesEnabled != value)
            {
                _subtitlesEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show closed captions for sound effects.
    /// </summary>
    public bool ClosedCaptionsEnabled
    {
        get => _closedCaptionsEnabled;
        set
        {
            if (_closedCaptionsEnabled != value)
            {
                _closedCaptionsEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Enable audio descriptions.
    /// </summary>
    public bool AudioDescriptionEnabled
    {
        get => _audioDescriptionEnabled;
        set
        {
            if (_audioDescriptionEnabled != value)
            {
                _audioDescriptionEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Dialogue volume multiplier.
    /// </summary>
    public float DialogueVolume
    {
        get => _dialogueVolume;
        set
        {
            value = MathHelper.Clamp(value, 0f, 2f);

            if (Math.Abs(_dialogueVolume - value) > 0.001f)
            {
                _dialogueVolume = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Force mono audio output.
    /// </summary>
    public bool MonoAudio
    {
        get => _monoAudio;
        set
        {
            if (_monoAudio != value)
            {
                _monoAudio = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Input Properties

    /// <summary>
    /// Require hold to confirm actions.
    /// </summary>
    public bool HoldToConfirm
    {
        get => _holdToConfirm;
        set
        {
            if (_holdToConfirm != value)
            {
                _holdToConfirm = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Hold duration for confirm (seconds).
    /// </summary>
    public float HoldDuration
    {
        get => _holdDuration;
        set
        {
            value = MathHelper.Clamp(value, 0.25f, 2f);

            if (Math.Abs(_holdDuration - value) > 0.001f)
            {
                _holdDuration = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Enable sticky keys (toggle instead of hold).
    /// </summary>
    public bool StickyKeys
    {
        get => _stickyKeys;
        set
        {
            if (_stickyKeys != value)
            {
                _stickyKeys = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Input delay for button presses (seconds).
    /// </summary>
    public float InputDelay
    {
        get => _inputDelay;
        set
        {
            value = MathHelper.Clamp(value, 0f, 1f);

            if (Math.Abs(_inputDelay - value) > 0.001f)
            {
                _inputDelay = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Auto-target enemies in combat.
    /// </summary>
    public bool AutoTarget
    {
        get => _autoTarget;
        set
        {
            if (_autoTarget != value)
            {
                _autoTarget = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Combat speed setting.
    /// </summary>
    public CombatSpeed CombatSpeed
    {
        get => _combatSpeed;
        set
        {
            if (_combatSpeed != value)
            {
                _combatSpeed = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Auto-advance dialogue text.
    /// </summary>
    public bool AutoAdvanceText
    {
        get => _autoAdvanceText;
        set
        {
            if (_autoAdvanceText != value)
            {
                _autoAdvanceText = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Text display speed multiplier.
    /// </summary>
    public float TextSpeed
    {
        get => _textSpeed;
        set
        {
            value = MathHelper.Clamp(value, 0.25f, 4f);

            if (Math.Abs(_textSpeed - value) > 0.001f)
            {
                _textSpeed = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Gameplay Assist Properties

    /// <summary>
    /// Enable invincibility mode.
    /// </summary>
    public bool InvincibilityMode
    {
        get => _invincibilityMode;
        set
        {
            if (_invincibilityMode != value)
            {
                _invincibilityMode = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Option to skip combat encounters.
    /// </summary>
    public bool SkipCombat
    {
        get => _skipCombat;
        set
        {
            if (_skipCombat != value)
            {
                _skipCombat = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Auto-heal between battles.
    /// </summary>
    public bool AutoHeal
    {
        get => _autoHeal;
        set
        {
            if (_autoHeal != value)
            {
                _autoHeal = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Enable navigation assistance.
    /// </summary>
    public bool NavigationAssist
    {
        get => _navigationAssist;
        set
        {
            if (_navigationAssist != value)
            {
                _navigationAssist = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Show quest markers on screen.
    /// </summary>
    public bool QuestMarkers
    {
        get => _questMarkers;
        set
        {
            if (_questMarkers != value)
            {
                _questMarkers = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Extend time-limited events.
    /// </summary>
    public bool ExtendedTimers
    {
        get => _extendedTimers;
        set
        {
            if (_extendedTimers != value)
            {
                _extendedTimers = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Gets the font scale multiplier based on font size setting.
    /// </summary>
    public float GetFontScale()
    {
        return FontSize switch
        {
            FontSize.Small => 0.85f,
            FontSize.Medium => 1.0f,
            FontSize.Large => 1.25f,
            FontSize.ExtraLarge => 1.5f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets the screen shake multiplier.
    /// </summary>
    public float GetScreenShakeMultiplier()
    {
        return ScreenShake switch
        {
            ScreenShakeIntensity.Off => 0f,
            ScreenShakeIntensity.Low => 0.5f,
            ScreenShakeIntensity.Medium => 1f,
            ScreenShakeIntensity.High => 1.5f,
            _ => 1f
        };
    }

    /// <summary>
    /// Gets the combat speed multiplier.
    /// </summary>
    public float GetCombatSpeedMultiplier()
    {
        return CombatSpeed switch
        {
            CombatSpeed.Slow => 0.5f,
            CombatSpeed.Normal => 1f,
            CombatSpeed.Fast => 2f,
            _ => 1f
        };
    }

    /// <summary>
    /// Gets the timer multiplier (for extended timers).
    /// </summary>
    public float GetTimerMultiplier()
    {
        return ExtendedTimers ? 2f : 1f;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Provides colorblind-safe color alternatives.
/// </summary>
public static class ColorblindPalette
{
    private static readonly Dictionary<string, Dictionary<ColorblindMode, Color>> _palettes = new()
    {
        ["Positive"] = new()
        {
            [ColorblindMode.None] = new Color(50, 200, 50),
            [ColorblindMode.Protanopia] = new Color(100, 180, 255),
            [ColorblindMode.Deuteranopia] = new Color(100, 180, 255),
            [ColorblindMode.Tritanopia] = new Color(50, 200, 50),
            [ColorblindMode.HighContrast] = Color.White
        },
        ["Negative"] = new()
        {
            [ColorblindMode.None] = new Color(220, 60, 60),
            [ColorblindMode.Protanopia] = new Color(255, 200, 50),
            [ColorblindMode.Deuteranopia] = new Color(255, 200, 50),
            [ColorblindMode.Tritanopia] = new Color(255, 100, 100),
            [ColorblindMode.HighContrast] = Color.Yellow
        },
        ["Neutral"] = new()
        {
            [ColorblindMode.None] = new Color(200, 200, 200),
            [ColorblindMode.Protanopia] = new Color(200, 200, 200),
            [ColorblindMode.Deuteranopia] = new Color(200, 200, 200),
            [ColorblindMode.Tritanopia] = new Color(200, 200, 200),
            [ColorblindMode.HighContrast] = Color.White
        },
        ["Warning"] = new()
        {
            [ColorblindMode.None] = new Color(255, 180, 50),
            [ColorblindMode.Protanopia] = new Color(255, 180, 255),
            [ColorblindMode.Deuteranopia] = new Color(255, 180, 255),
            [ColorblindMode.Tritanopia] = new Color(255, 180, 50),
            [ColorblindMode.HighContrast] = new Color(255, 255, 0)
        },
        ["Health"] = new()
        {
            [ColorblindMode.None] = new Color(50, 220, 50),
            [ColorblindMode.Protanopia] = new Color(50, 200, 255),
            [ColorblindMode.Deuteranopia] = new Color(50, 200, 255),
            [ColorblindMode.Tritanopia] = new Color(50, 220, 50),
            [ColorblindMode.HighContrast] = Color.Green
        },
        ["Mana"] = new()
        {
            [ColorblindMode.None] = new Color(80, 120, 255),
            [ColorblindMode.Protanopia] = new Color(255, 200, 100),
            [ColorblindMode.Deuteranopia] = new Color(255, 200, 100),
            [ColorblindMode.Tritanopia] = new Color(180, 80, 255),
            [ColorblindMode.HighContrast] = Color.Cyan
        },
        ["Experience"] = new()
        {
            [ColorblindMode.None] = new Color(200, 160, 255),
            [ColorblindMode.Protanopia] = new Color(200, 200, 150),
            [ColorblindMode.Deuteranopia] = new Color(200, 200, 150),
            [ColorblindMode.Tritanopia] = new Color(200, 160, 255),
            [ColorblindMode.HighContrast] = Color.Magenta
        },
        ["Enemy"] = new()
        {
            [ColorblindMode.None] = new Color(255, 80, 80),
            [ColorblindMode.Protanopia] = new Color(255, 150, 50),
            [ColorblindMode.Deuteranopia] = new Color(255, 150, 50),
            [ColorblindMode.Tritanopia] = new Color(255, 80, 80),
            [ColorblindMode.HighContrast] = Color.Red
        },
        ["Ally"] = new()
        {
            [ColorblindMode.None] = new Color(80, 180, 255),
            [ColorblindMode.Protanopia] = new Color(80, 255, 180),
            [ColorblindMode.Deuteranopia] = new Color(80, 255, 180),
            [ColorblindMode.Tritanopia] = new Color(80, 180, 255),
            [ColorblindMode.HighContrast] = Color.Blue
        },
        ["Quest"] = new()
        {
            [ColorblindMode.None] = new Color(255, 220, 50),
            [ColorblindMode.Protanopia] = new Color(255, 220, 50),
            [ColorblindMode.Deuteranopia] = new Color(255, 220, 50),
            [ColorblindMode.Tritanopia] = new Color(255, 220, 50),
            [ColorblindMode.HighContrast] = Color.Yellow
        }
    };

    /// <summary>
    /// Gets a colorblind-safe color for a given palette name.
    /// </summary>
    public static Color GetColor(string paletteName, ColorblindMode mode)
    {
        if (_palettes.TryGetValue(paletteName, out var palette))
        {
            if (palette.TryGetValue(mode, out var color))
            {
                return color;
            }
        }

        return Color.White;
    }

    /// <summary>
    /// Gets the health bar color.
    /// </summary>
    public static Color GetHealthColor(ColorblindMode mode) => GetColor("Health", mode);

    /// <summary>
    /// Gets the mana/energy bar color.
    /// </summary>
    public static Color GetManaColor(ColorblindMode mode) => GetColor("Mana", mode);

    /// <summary>
    /// Gets the enemy indicator color.
    /// </summary>
    public static Color GetEnemyColor(ColorblindMode mode) => GetColor("Enemy", mode);

    /// <summary>
    /// Gets the ally indicator color.
    /// </summary>
    public static Color GetAllyColor(ColorblindMode mode) => GetColor("Ally", mode);

    /// <summary>
    /// Gets the positive feedback color.
    /// </summary>
    public static Color GetPositiveColor(ColorblindMode mode) => GetColor("Positive", mode);

    /// <summary>
    /// Gets the negative feedback color.
    /// </summary>
    public static Color GetNegativeColor(ColorblindMode mode) => GetColor("Negative", mode);

    /// <summary>
    /// Applies colorblind simulation to a color.
    /// </summary>
    public static Color SimulateColorblindness(Color color, ColorblindMode mode)
    {
        if (mode == ColorblindMode.None)
        {
            return color;
        }

        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float newR, newG, newB;

        switch (mode)
        {
            case ColorblindMode.Protanopia:
                newR = 0.567f * r + 0.433f * g;
                newG = 0.558f * r + 0.442f * g;
                newB = 0.242f * g + 0.758f * b;
                break;

            case ColorblindMode.Deuteranopia:
                newR = 0.625f * r + 0.375f * g;
                newG = 0.7f * r + 0.3f * g;
                newB = 0.3f * g + 0.7f * b;
                break;

            case ColorblindMode.Tritanopia:
                newR = 0.95f * r + 0.05f * g;
                newG = 0.433f * g + 0.567f * b;
                newB = 0.475f * g + 0.525f * b;
                break;

            case ColorblindMode.HighContrast:
                // Convert to grayscale and enhance contrast
                float luminance = 0.299f * r + 0.587f * g + 0.114f * b;
                luminance = luminance > 0.5f ? 1f : 0f;
                newR = newG = newB = luminance;
                break;

            default:
                return color;
        }

        return new Color(
            (int)(MathHelper.Clamp(newR, 0f, 1f) * 255),
            (int)(MathHelper.Clamp(newG, 0f, 1f) * 255),
            (int)(MathHelper.Clamp(newB, 0f, 1f) * 255),
            color.A);
    }
}

/// <summary>
/// Manages closed captions for audio events.
/// </summary>
public class ClosedCaptionManager
{
    private readonly List<CaptionEntry> _activeCaptions = new();
    private const float CAPTION_DURATION = 3f;
    private const int MAX_CAPTIONS = 4;

    /// <summary>
    /// Active captions to display.
    /// </summary>
    public IReadOnlyList<CaptionEntry> ActiveCaptions => _activeCaptions;

    /// <summary>
    /// Updates caption timers.
    /// </summary>
    public void Update(float deltaTime)
    {
        for (int i = _activeCaptions.Count - 1; i >= 0; i--)
        {
            _activeCaptions[i].RemainingTime -= deltaTime;

            if (_activeCaptions[i].RemainingTime <= 0)
            {
                _activeCaptions.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Adds a caption.
    /// </summary>
    public void AddCaption(string text, CaptionType type = CaptionType.SoundEffect)
    {
        // Remove oldest if at capacity
        while (_activeCaptions.Count >= MAX_CAPTIONS)
        {
            _activeCaptions.RemoveAt(0);
        }

        _activeCaptions.Add(new CaptionEntry
        {
            Text = text,
            Type = type,
            RemainingTime = CAPTION_DURATION
        });
    }

    /// <summary>
    /// Clears all captions.
    /// </summary>
    public void Clear()
    {
        _activeCaptions.Clear();
    }
}

/// <summary>
/// A closed caption entry.
/// </summary>
public class CaptionEntry
{
    public string Text { get; init; } = "";
    public CaptionType Type { get; init; }
    public float RemainingTime { get; set; }

    public float Alpha => MathHelper.Clamp(RemainingTime, 0f, 1f);
}

/// <summary>
/// Types of closed captions.
/// </summary>
public enum CaptionType
{
    Dialogue,
    SoundEffect,
    Music,
    Ambient
}
