using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Accessibility;
using Strays.Core.Input;
using Strays.Core.Inputs;
using Strays.Core.Screens;

namespace Strays.Core.Screens;

/// <summary>
/// Accessibility options screen.
/// </summary>
public class AccessibilityScreen : GameScreen
{
    private readonly AccessibilitySettings _settings;
    private readonly List<AccessibilityOption> _options = new();
    private int _selectedIndex;
    private int _scrollOffset;
    private const int VISIBLE_OPTIONS = 10;

    private int _selectedCategory;
    private readonly string[] _categories = { "Visual", "Audio", "Input", "Gameplay" };

    private float _fadeIn;

    private readonly Color _backgroundColor = new(18, 22, 28);
    private readonly Color _panelColor = new(28, 35, 45);
    private readonly Color _selectedColor = new(50, 70, 100);
    private readonly Color _textColor = new(200, 210, 220);
    private readonly Color _accentColor = new(100, 200, 255);
    private readonly Color _categoryColor = new(80, 150, 200);
    private readonly Color _valueColor = new(150, 220, 150);

    public AccessibilityScreen(AccessibilitySettings settings)
    {
        _settings = settings;
        BuildOptionsForCategory(0);
    }

    private void BuildOptionsForCategory(int categoryIndex)
    {
        _options.Clear();
        _selectedIndex = 0;
        _scrollOffset = 0;

        switch (categoryIndex)
        {
            case 0: // Visual
                BuildVisualOptions();
                break;

            case 1: // Audio
                BuildAudioOptions();
                break;

            case 2: // Input
                BuildInputOptions();
                break;

            case 3: // Gameplay
                BuildGameplayOptions();
                break;
        }
    }

    private void BuildVisualOptions()
    {
        _options.Add(new EnumOption<ColorblindMode>("Colorblind Mode", () => _settings.ColorblindMode, v => _settings.ColorblindMode = v, "Color correction for various types of color blindness"));
        _options.Add(new EnumOption<FontSize>("Font Size", () => _settings.FontSize, v => _settings.FontSize = v, "Adjust text size throughout the game"));
        _options.Add(new SliderOption("UI Scale", () => _settings.UIScale, v => _settings.UIScale = v, 0.75f, 2f, 0.05f, "Scale all UI elements"));
        _options.Add(new ToggleOption("High Contrast UI", () => _settings.HighContrastUI, v => _settings.HighContrastUI = v, "Increase UI contrast for better visibility"));
        _options.Add(new EnumOption<ScreenShakeIntensity>("Screen Shake", () => _settings.ScreenShake, v => _settings.ScreenShake = v, "Adjust or disable screen shake effects"));
        _options.Add(new ToggleOption("Reduce Motion", () => _settings.ReduceMotion, v => _settings.ReduceMotion = v, "Reduce particle effects and animations"));
        _options.Add(new ToggleOption("Flashing Effects", () => _settings.FlashingEffectsEnabled, v => _settings.FlashingEffectsEnabled = v, "Enable or disable flashing visual effects"));
        _options.Add(new SliderOption("Brightness", () => _settings.Brightness, v => _settings.Brightness = v, 0.5f, 1.5f, 0.05f, "Adjust screen brightness"));
        _options.Add(new SliderOption("Contrast", () => _settings.Contrast, v => _settings.Contrast = v, 0.5f, 1.5f, 0.05f, "Adjust screen contrast"));
    }

    private void BuildAudioOptions()
    {
        _options.Add(new ToggleOption("Subtitles", () => _settings.SubtitlesEnabled, v => _settings.SubtitlesEnabled = v, "Show text for spoken dialogue"));
        _options.Add(new ToggleOption("Closed Captions", () => _settings.ClosedCaptionsEnabled, v => _settings.ClosedCaptionsEnabled = v, "Show captions for sound effects and music"));
        _options.Add(new ToggleOption("Audio Description", () => _settings.AudioDescriptionEnabled, v => _settings.AudioDescriptionEnabled = v, "Enable narrated descriptions of visual events"));
        _options.Add(new SliderOption("Dialogue Volume", () => _settings.DialogueVolume, v => _settings.DialogueVolume = v, 0f, 2f, 0.1f, "Boost dialogue audio relative to other sounds"));
        _options.Add(new ToggleOption("Mono Audio", () => _settings.MonoAudio, v => _settings.MonoAudio = v, "Output all audio to both speakers equally"));
    }

    private void BuildInputOptions()
    {
        _options.Add(new ToggleOption("Hold to Confirm", () => _settings.HoldToConfirm, v => _settings.HoldToConfirm = v, "Require holding button to confirm important actions"));
        _options.Add(new SliderOption("Hold Duration", () => _settings.HoldDuration, v => _settings.HoldDuration = v, 0.25f, 2f, 0.25f, "Time required to hold for confirmation"));
        _options.Add(new ToggleOption("Sticky Keys", () => _settings.StickyKeys, v => _settings.StickyKeys = v, "Toggle modifiers instead of holding them"));
        _options.Add(new SliderOption("Input Delay", () => _settings.InputDelay, v => _settings.InputDelay = v, 0f, 1f, 0.1f, "Add delay between button presses"));
        _options.Add(new ToggleOption("Auto-Target", () => _settings.AutoTarget, v => _settings.AutoTarget = v, "Automatically target nearest enemy in combat"));
        _options.Add(new EnumOption<CombatSpeed>("Combat Speed", () => _settings.CombatSpeed, v => _settings.CombatSpeed = v, "Adjust combat animation speed"));
        _options.Add(new ToggleOption("Auto-Advance Text", () => _settings.AutoAdvanceText, v => _settings.AutoAdvanceText = v, "Automatically advance dialogue after reading"));
        _options.Add(new SliderOption("Text Speed", () => _settings.TextSpeed, v => _settings.TextSpeed = v, 0.25f, 4f, 0.25f, "Speed of typewriter text effect"));
    }

    private void BuildGameplayOptions()
    {
        _options.Add(new ToggleOption("Invincibility Mode", () => _settings.InvincibilityMode, v => _settings.InvincibilityMode = v, "Take no damage (disables some achievements)"));
        _options.Add(new ToggleOption("Skip Combat", () => _settings.SkipCombat, v => _settings.SkipCombat = v, "Option to skip non-story combat encounters"));
        _options.Add(new ToggleOption("Auto-Heal", () => _settings.AutoHeal, v => _settings.AutoHeal = v, "Automatically restore health between battles"));
        _options.Add(new ToggleOption("Navigation Assist", () => _settings.NavigationAssist, v => _settings.NavigationAssist = v, "Show guidance to current objective"));
        _options.Add(new ToggleOption("Quest Markers", () => _settings.QuestMarkers, v => _settings.QuestMarkers = v, "Show objective markers on minimap"));
        _options.Add(new ToggleOption("Extended Timers", () => _settings.ExtendedTimers, v => _settings.ExtendedTimers = v, "Double time limits for timed events"));
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_fadeIn < 1f)
        {
            _fadeIn = Math.Min(1f, _fadeIn + deltaTime * 4f);
        }
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        // Category switching
        if (input.WasKeyPressed(Keys.Tab) || input.WasKeyPressed(Keys.Q))
        {
            _selectedCategory = (_selectedCategory - 1 + _categories.Length) % _categories.Length;
            BuildOptionsForCategory(_selectedCategory);
        }
        else if (input.WasKeyPressed(Keys.E))
        {
            _selectedCategory = (_selectedCategory + 1) % _categories.Length;
            BuildOptionsForCategory(_selectedCategory);
        }

        // Navigation
        if (input.WasKeyPressed(Keys.Up) || input.WasKeyPressed(Keys.W))
        {
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
            UpdateScroll();
        }
        else if (input.WasKeyPressed(Keys.Down) || input.WasKeyPressed(Keys.S))
        {
            _selectedIndex = Math.Min(_options.Count - 1, _selectedIndex + 1);
            UpdateScroll();
        }

        // Value adjustment
        if (input.WasKeyPressed(Keys.Left) || input.WasKeyPressed(Keys.A))
        {
            if (_selectedIndex >= 0 && _selectedIndex < _options.Count)
            {
                _options[_selectedIndex].Decrease();
            }
        }
        else if (input.WasKeyPressed(Keys.Right) || input.WasKeyPressed(Keys.D))
        {
            if (_selectedIndex >= 0 && _selectedIndex < _options.Count)
            {
                _options[_selectedIndex].Increase();
            }
        }

        // Toggle/Select
        if (input.WasKeyPressed(Keys.Enter) || input.WasKeyPressed(Keys.Space))
        {
            if (_selectedIndex >= 0 && _selectedIndex < _options.Count)
            {
                _options[_selectedIndex].Toggle();
            }
        }

        // Back
        if (input.WasKeyPressed(Keys.Escape) || input.WasKeyPressed(Keys.Back))
        {
            ExitScreen();
        }

        // Reset option
        if (input.WasKeyPressed(Keys.R))
        {
            if (_selectedIndex >= 0 && _selectedIndex < _options.Count)
            {
                _options[_selectedIndex].Reset();
            }
        }
    }

    private void UpdateScroll()
    {
        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + VISIBLE_OPTIONS)
        {
            _scrollOffset = _selectedIndex - VISIBLE_OPTIONS + 1;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var font = ScreenManager.Font;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        float alpha = _fadeIn;

        // Background
        DrawFilledRectangle(spriteBatch, viewport.Bounds, _backgroundColor * alpha);

        // Title
        string title = "ACCESSIBILITY OPTIONS";
        var titleSize = font.MeasureString(title) * 1.3f;
        var titlePos = new Vector2((viewport.Width - titleSize.X) / 2, 25);
        spriteBatch.DrawString(font, title, titlePos, _accentColor * alpha, 0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0f);

        // Category tabs
        int tabWidth = 150;
        int tabStartX = (viewport.Width - tabWidth * _categories.Length) / 2;
        int tabY = 70;

        for (int i = 0; i < _categories.Length; i++)
        {
            var tabBounds = new Rectangle(tabStartX + i * tabWidth, tabY, tabWidth - 5, 30);
            Color tabColor = i == _selectedCategory ? _selectedColor : _panelColor;
            DrawFilledRectangle(spriteBatch, tabBounds, tabColor * alpha);

            var catSize = font.MeasureString(_categories[i]);
            var catPos = new Vector2(tabBounds.X + (tabBounds.Width - catSize.X) / 2, tabBounds.Y + 5);
            Color catTextColor = i == _selectedCategory ? _accentColor : _categoryColor;
            spriteBatch.DrawString(font, _categories[i], catPos, catTextColor * alpha);
        }

        // Options panel
        int panelX = 100;
        int panelY = 115;
        int panelWidth = viewport.Width - 200;
        int panelHeight = viewport.Height - 200;
        var panelBounds = new Rectangle(panelX, panelY, panelWidth, panelHeight);
        DrawFilledRectangle(spriteBatch, panelBounds, _panelColor * alpha);

        // Draw options
        int optionHeight = 50;
        int startY = panelY + 15;

        for (int i = 0; i < VISIBLE_OPTIONS && i + _scrollOffset < _options.Count; i++)
        {
            int idx = i + _scrollOffset;
            var option = _options[idx];
            int y = startY + i * optionHeight;
            bool isSelected = idx == _selectedIndex;

            // Selection highlight
            if (isSelected)
            {
                var highlightBounds = new Rectangle(panelX + 5, y - 2, panelWidth - 10, optionHeight - 4);
                DrawFilledRectangle(spriteBatch, highlightBounds, _selectedColor * alpha * 0.5f);
            }

            // Option name
            Color nameColor = isSelected ? _accentColor : _textColor;
            spriteBatch.DrawString(font, option.Name, new Vector2(panelX + 20, y + 5), nameColor * alpha);

            // Option value
            string valueText = option.GetValueString();
            var valueSize = font.MeasureString(valueText);
            spriteBatch.DrawString(font, valueText, new Vector2(panelX + panelWidth - valueSize.X - 30, y + 5), _valueColor * alpha);

            // Description
            if (isSelected && !string.IsNullOrEmpty(option.Description))
            {
                spriteBatch.DrawString(font, option.Description, new Vector2(panelX + 20, y + 25), _textColor * alpha * 0.6f);
            }

            // Arrows for adjustable options
            if (isSelected && option.IsAdjustable)
            {
                spriteBatch.DrawString(font, "◄", new Vector2(panelX + panelWidth - valueSize.X - 55, y + 5), _accentColor * alpha);
                spriteBatch.DrawString(font, "►", new Vector2(panelX + panelWidth - 15, y + 5), _accentColor * alpha);
            }
        }

        // Scroll indicators
        if (_scrollOffset > 0)
        {
            spriteBatch.DrawString(font, "▲", new Vector2(panelX + panelWidth / 2, panelY + 5), _textColor * alpha * 0.5f);
        }

        if (_scrollOffset + VISIBLE_OPTIONS < _options.Count)
        {
            spriteBatch.DrawString(font, "▼", new Vector2(panelX + panelWidth / 2, panelY + panelHeight - 20), _textColor * alpha * 0.5f);
        }

        // Instructions
        string instructions = "[Q/E] Category  [↑↓] Navigate  [←→] Adjust  [Enter] Toggle  [R] Reset  [Esc] Back";
        var instSize = font.MeasureString(instructions);
        spriteBatch.DrawString(font, instructions, new Vector2((viewport.Width - instSize.X) / 2, viewport.Height - 40), _textColor * alpha * 0.6f);

        spriteBatch.End();
    }

    private void DrawFilledRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        var pixel = ScreenManager.PixelTexture;
        spriteBatch.Draw(pixel, bounds, color);
    }
}

/// <summary>
/// Base class for accessibility options.
/// </summary>
public abstract class AccessibilityOption
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public virtual bool IsAdjustable => true;

    public abstract string GetValueString();
    public abstract void Increase();
    public abstract void Decrease();
    public abstract void Toggle();
    public abstract void Reset();
}

/// <summary>
/// Boolean toggle option.
/// </summary>
public class ToggleOption : AccessibilityOption
{
    private readonly Func<bool> _getter;
    private readonly Action<bool> _setter;
    private readonly bool _defaultValue;

    public override bool IsAdjustable => false;

    public ToggleOption(string name, Func<bool> getter, Action<bool> setter, string description, bool defaultValue = false)
    {
        Name = name;
        Description = description;
        _getter = getter;
        _setter = setter;
        _defaultValue = defaultValue;
    }

    public override string GetValueString() => _getter() ? "ON" : "OFF";
    public override void Increase() => Toggle();
    public override void Decrease() => Toggle();
    public override void Toggle() => _setter(!_getter());
    public override void Reset() => _setter(_defaultValue);
}

/// <summary>
/// Slider option for numeric values.
/// </summary>
public class SliderOption : AccessibilityOption
{
    private readonly Func<float> _getter;
    private readonly Action<float> _setter;
    private readonly float _min;
    private readonly float _max;
    private readonly float _step;
    private readonly float _defaultValue;

    public SliderOption(string name, Func<float> getter, Action<float> setter, float min, float max, float step, string description, float? defaultValue = null)
    {
        Name = name;
        Description = description;
        _getter = getter;
        _setter = setter;
        _min = min;
        _max = max;
        _step = step;
        _defaultValue = defaultValue ?? min;
    }

    public override string GetValueString() => $"{_getter():F2}";

    public override void Increase()
    {
        float newValue = Math.Min(_max, _getter() + _step);
        _setter(newValue);
    }

    public override void Decrease()
    {
        float newValue = Math.Max(_min, _getter() - _step);
        _setter(newValue);
    }

    public override void Toggle() => Increase();
    public override void Reset() => _setter(_defaultValue);
}

/// <summary>
/// Enum selection option.
/// </summary>
public class EnumOption<T> : AccessibilityOption where T : struct, Enum
{
    private readonly Func<T> _getter;
    private readonly Action<T> _setter;
    private readonly T[] _values;
    private readonly T _defaultValue;

    public EnumOption(string name, Func<T> getter, Action<T> setter, string description, T? defaultValue = null)
    {
        Name = name;
        Description = description;
        _getter = getter;
        _setter = setter;
        _values = Enum.GetValues<T>();
        _defaultValue = defaultValue ?? _values[0];
    }

    public override string GetValueString() => _getter().ToString();

    public override void Increase()
    {
        int currentIndex = Array.IndexOf(_values, _getter());
        int nextIndex = (currentIndex + 1) % _values.Length;
        _setter(_values[nextIndex]);
    }

    public override void Decrease()
    {
        int currentIndex = Array.IndexOf(_values, _getter());
        int prevIndex = (currentIndex - 1 + _values.Length) % _values.Length;
        _setter(_values[prevIndex]);
    }

    public override void Toggle() => Increase();
    public override void Reset() => _setter(_defaultValue);
}

