using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Audio;
using Lazarus.Core.Inputs;
using Lazarus.Core.Localization;
using Lazarus.Core.Settings;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Modern tabbed settings screen with visual polish.
/// </summary>
public class SettingsMenuScreen : GameScreen
{
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _titleFont;
    private SpriteFont? _contentFont;
    private SpriteFont? _smallFont;
    private Texture2D? _pixelTexture;

    private SettingsManager<LazarusSettings>? _settingsManager;
    private AudioManager? _audioManager;
    private GraphicsDeviceManager? _graphicsManager;

    // Tabs
    private enum SettingsTab { Audio, Video, Controls, Gameplay, Accessibility }
    private SettingsTab _currentTab = SettingsTab.Audio;
    private readonly List<SettingsTab> _tabs = new()
    {
        SettingsTab.Audio, SettingsTab.Video, SettingsTab.Controls,
        SettingsTab.Gameplay, SettingsTab.Accessibility
    };

    // Settings items per tab
    private readonly Dictionary<SettingsTab, List<SettingsItem>> _tabItems = new();
    private int _selectedIndex = 0;

    // Animation
    private float _transitionAlpha = 0f;
    private float _selectionPulse = 0f;
    private float _tabSwitchProgress = 1f;
    private SettingsTab _previousTab;

    // Colors
    private static readonly Color BackgroundColor = new(15, 18, 28);
    private static readonly Color PanelColor = new(25, 30, 45);
    private static readonly Color TabActiveColor = new(50, 90, 150);
    private static readonly Color TabInactiveColor = new(35, 40, 55);
    private static readonly Color SelectedColor = new(45, 75, 120);
    private static readonly Color TextColor = new(220, 225, 235);
    private static readonly Color DimTextColor = new(130, 140, 160);
    private static readonly Color AccentColor = new(80, 180, 255);
    private static readonly Color ValueColor = new(120, 255, 180);
    private static readonly Color WarningColor = new(255, 180, 80);
    private static readonly Color SliderFillColor = new(60, 140, 220);
    private static readonly Color SliderBgColor = new(40, 45, 60);

    public SettingsMenuScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);

        InitializeSettingsItems();
    }

    private void InitializeSettingsItems()
    {
        // Audio tab
        _tabItems[SettingsTab.Audio] = new List<SettingsItem>
        {
            new() { Label = "Master Volume", Type = ItemType.Slider, Key = "master_volume" },
            new() { Label = "Music Volume", Type = ItemType.Slider, Key = "music_volume" },
            new() { Label = "Sound Effects", Type = ItemType.Slider, Key = "sfx_volume" },
            new() { Label = "Ambient Volume", Type = ItemType.Slider, Key = "ambient_volume" },
            new() { Label = "Voice Volume", Type = ItemType.Slider, Key = "voice_volume" },
            new() { Label = "UI Sounds", Type = ItemType.Slider, Key = "ui_volume" },
            new() { Type = ItemType.Separator },
            new() { Label = "Mute All", Type = ItemType.Toggle, Key = "mute_all" }
        };

        // Video tab
        _tabItems[SettingsTab.Video] = new List<SettingsItem>
        {
            new() { Label = "Display Mode", Type = ItemType.Choice, Key = "display_mode", Choices = new[] { "Windowed", "Fullscreen", "Borderless" } },
            new() { Label = "Resolution", Type = ItemType.Choice, Key = "resolution", Choices = new[] { "1280x720", "1920x1080", "2560x1440" } },
            new() { Label = "VSync", Type = ItemType.Toggle, Key = "vsync" },
            new() { Type = ItemType.Separator },
            new() { Label = "Brightness", Type = ItemType.Slider, Key = "brightness" },
            new() { Label = "Screen Shake", Type = ItemType.Slider, Key = "screen_shake" },
            new() { Label = "Show FPS", Type = ItemType.Toggle, Key = "show_fps" }
        };

        // Controls tab
        _tabItems[SettingsTab.Controls] = new List<SettingsItem>
        {
            new() { Label = "Configure Bindings", Type = ItemType.Button, Key = "rebind" },
            new() { Type = ItemType.Separator },
            new() { Label = "Stick Sensitivity", Type = ItemType.Slider, Key = "sensitivity" },
            new() { Label = "Invert Y Axis", Type = ItemType.Toggle, Key = "invert_y" },
            new() { Label = "Vibration", Type = ItemType.Toggle, Key = "vibration" },
            new() { Label = "Vibration Intensity", Type = ItemType.Slider, Key = "vibration_intensity" },
            new() { Type = ItemType.Separator },
            new() { Label = "Aim Assist", Type = ItemType.Toggle, Key = "aim_assist" }
        };

        // Gameplay tab
        _tabItems[SettingsTab.Gameplay] = new List<SettingsItem>
        {
            new() { Label = "Difficulty", Type = ItemType.Choice, Key = "difficulty", Choices = new[] { "Easy", "Normal", "Hard" } },
            new() { Label = "Auto-Save", Type = ItemType.Toggle, Key = "autosave" },
            new() { Label = "Auto-Save Interval", Type = ItemType.Choice, Key = "autosave_interval", Choices = new[] { "5 min", "10 min", "15 min" } },
            new() { Type = ItemType.Separator },
            new() { Label = "Show Tutorials", Type = ItemType.Toggle, Key = "tutorials" },
            new() { Label = "Show Damage Numbers", Type = ItemType.Toggle, Key = "damage_numbers" },
            new() { Label = "Show Mini-Map", Type = ItemType.Toggle, Key = "minimap" }
        };

        // Accessibility tab
        _tabItems[SettingsTab.Accessibility] = new List<SettingsItem>
        {
            new() { Label = "Language", Type = ItemType.Button, Key = "language" },
            new() { Type = ItemType.Separator },
            new() { Label = "Subtitles", Type = ItemType.Toggle, Key = "subtitles" },
            new() { Label = "Subtitle Size", Type = ItemType.Choice, Key = "subtitle_size", Choices = new[] { "Small", "Medium", "Large" } },
            new() { Label = "Subtitle Background", Type = ItemType.Toggle, Key = "subtitle_bg" },
            new() { Type = ItemType.Separator },
            new() { Label = "Colorblind Mode", Type = ItemType.Choice, Key = "colorblind", Choices = new[] { "Off", "Protanopia", "Deuteranopia", "Tritanopia" } },
            new() { Label = "High Contrast UI", Type = ItemType.Toggle, Key = "high_contrast" },
            new() { Label = "Reduce Motion", Type = ItemType.Toggle, Key = "reduce_motion" }
        };
    }

    public override void LoadContent()
    {
        base.LoadContent();

        if (ScreenManager == null)
        {
            return;
        }

        _spriteBatch = ScreenManager.SpriteBatch;

        try
        {
            _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");
            _contentFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
            _smallFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/Hud");
        }
        catch
        {
            _titleFont = ScreenManager.Font;
            _contentFont = ScreenManager.Font;
            _smallFont = ScreenManager.Font;
        }

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Get services
        _settingsManager = ScreenManager.Game.Services.GetService<SettingsManager<LazarusSettings>>();
        _audioManager = ScreenManager.Game.Services.GetService<AudioManager>();
        _graphicsManager = ScreenManager.Game.Services.GetService<GraphicsDeviceManager>();

        LoadCurrentValues();
    }

    private void LoadCurrentValues()
    {
        // Load audio values
        if (_audioManager != null)
        {
            SetSliderValue("master_volume", _audioManager.GetVolume(AudioCategory.Master));
            SetSliderValue("music_volume", _audioManager.GetVolume(AudioCategory.Music));
            SetSliderValue("sfx_volume", _audioManager.GetVolume(AudioCategory.SoundEffects));
            SetSliderValue("ambient_volume", _audioManager.GetVolume(AudioCategory.Ambient));
            SetSliderValue("voice_volume", _audioManager.GetVolume(AudioCategory.Voice));
            SetSliderValue("ui_volume", _audioManager.GetVolume(AudioCategory.UI));
            SetToggleValue("mute_all", _audioManager.IsMuted(AudioCategory.Master));
        }

        // Load video values
        if (_graphicsManager != null)
        {
            SetChoiceIndex("display_mode", _graphicsManager.IsFullScreen ? 1 : 0);
        }

        // Load settings values
        if (_settingsManager != null)
        {
            SetToggleValue("vsync", true); // Default values
            SetToggleValue("autosave", true);
            SetToggleValue("tutorials", true);
            SetToggleValue("damage_numbers", true);
            SetToggleValue("minimap", true);
            SetToggleValue("subtitles", true);
            SetSliderValue("brightness", 0.5f);
            SetSliderValue("screen_shake", 1f);
            SetSliderValue("sensitivity", 0.5f);
            SetSliderValue("vibration_intensity", 1f);
            SetToggleValue("vibration", true);
            SetToggleValue("aim_assist", true);
        }
    }

    private void SetSliderValue(string key, float value)
    {
        foreach (var tab in _tabItems.Values)
        {
            var item = tab.Find(i => i.Key == key);

            if (item != null)
            {
                item.SliderValue = value;

                break;
            }
        }
    }

    private void SetToggleValue(string key, bool value)
    {
        foreach (var tab in _tabItems.Values)
        {
            var item = tab.Find(i => i.Key == key);

            if (item != null)
            {
                item.ToggleValue = value;

                break;
            }
        }
    }

    private void SetChoiceIndex(string key, int index)
    {
        foreach (var tab in _tabItems.Values)
        {
            var item = tab.Find(i => i.Key == key);

            if (item != null)
            {
                item.ChoiceIndex = index;

                break;
            }
        }
    }

    public override void UnloadContent()
    {
        _pixelTexture?.Dispose();
        base.UnloadContent();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        // Tab switching
        if (input.IsNewKeyPress(Keys.Q, ControllingPlayer, out _) ||
            input.IsNewButtonPress(Buttons.LeftShoulder, ControllingPlayer, out _))
        {
            SwitchTab(-1);
        }
        else if (input.IsNewKeyPress(Keys.E, ControllingPlayer, out _) ||
                 input.IsNewButtonPress(Buttons.RightShoulder, ControllingPlayer, out _))
        {
            SwitchTab(1);
        }

        // Navigation
        if (input.IsMenuUp(ControllingPlayer))
        {
            NavigateUp();
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            NavigateDown();
        }

        // Value adjustment
        var currentItems = _tabItems[_currentTab];

        if (_selectedIndex >= 0 && _selectedIndex < currentItems.Count)
        {
            var item = currentItems[_selectedIndex];

            bool menuLeft = input.IsNewKeyPress(Keys.Left, ControllingPlayer, out _) ||
                           input.IsNewButtonPress(Buttons.DPadLeft, ControllingPlayer, out _);
            bool menuRight = input.IsNewKeyPress(Keys.Right, ControllingPlayer, out _) ||
                            input.IsNewButtonPress(Buttons.DPadRight, ControllingPlayer, out _);

            if (item.Type == ItemType.Slider)
            {
                if (menuLeft)
                {
                    AdjustSlider(item, -0.05f);
                }
                else if (menuRight)
                {
                    AdjustSlider(item, 0.05f);
                }
            }
            else if (item.Type == ItemType.Toggle)
            {
                if (input.IsMenuSelect(ControllingPlayer, out _) || menuLeft || menuRight)
                {
                    ToggleItem(item);
                }
            }
            else if (item.Type == ItemType.Choice)
            {
                if (menuLeft)
                {
                    CycleChoice(item, -1);
                }
                else if (menuRight || input.IsMenuSelect(ControllingPlayer, out _))
                {
                    CycleChoice(item, 1);
                }
            }
            else if (item.Type == ItemType.Button)
            {
                if (input.IsMenuSelect(ControllingPlayer, out var playerIndex))
                {
                    ExecuteButton(item, playerIndex);
                }
            }
        }

        // Back
        if (input.IsMenuCancel(ControllingPlayer, out _))
        {
            SaveAndExit();
        }
    }

    private void SwitchTab(int direction)
    {
        int currentIndex = _tabs.IndexOf(_currentTab);
        int newIndex = (currentIndex + direction + _tabs.Count) % _tabs.Count;

        _previousTab = _currentTab;
        _currentTab = _tabs[newIndex];
        _tabSwitchProgress = 0f;
        _selectedIndex = 0;

        // Skip to first non-separator
        var items = _tabItems[_currentTab];

        while (_selectedIndex < items.Count && items[_selectedIndex].Type == ItemType.Separator)
        {
            _selectedIndex++;
        }
    }

    private void NavigateUp()
    {
        var items = _tabItems[_currentTab];

        do
        {
            _selectedIndex--;

            if (_selectedIndex < 0)
            {
                _selectedIndex = items.Count - 1;
            }
        }
        while (items[_selectedIndex].Type == ItemType.Separator);
    }

    private void NavigateDown()
    {
        var items = _tabItems[_currentTab];

        do
        {
            _selectedIndex++;

            if (_selectedIndex >= items.Count)
            {
                _selectedIndex = 0;
            }
        }
        while (items[_selectedIndex].Type == ItemType.Separator);
    }

    private void AdjustSlider(SettingsItem item, float delta)
    {
        item.SliderValue = MathHelper.Clamp(item.SliderValue + delta, 0f, 1f);
        ApplySetting(item);
    }

    private void ToggleItem(SettingsItem item)
    {
        item.ToggleValue = !item.ToggleValue;
        ApplySetting(item);
    }

    private void CycleChoice(SettingsItem item, int direction)
    {
        if (item.Choices == null || item.Choices.Length == 0)
        {
            return;
        }

        item.ChoiceIndex = (item.ChoiceIndex + direction + item.Choices.Length) % item.Choices.Length;
        ApplySetting(item);
    }

    private void ExecuteButton(SettingsItem item, PlayerIndex playerIndex)
    {
        switch (item.Key)
        {
            case "rebind":
                // Open input settings screen
                ScreenManager?.AddScreen(new InputSettingsScreen(
                    new GamepadManager(), new ControllerSettings()), playerIndex);
                break;

            case "language":
                // Open language screen
                ScreenManager?.AddScreen(new LanguageScreen(), playerIndex);
                break;
        }
    }

    private void ApplySetting(SettingsItem item)
    {
        if (_audioManager != null)
        {
            switch (item.Key)
            {
                case "master_volume":
                    _audioManager.SetVolume(AudioCategory.Master, item.SliderValue);
                    break;
                case "music_volume":
                    _audioManager.SetVolume(AudioCategory.Music, item.SliderValue);
                    break;
                case "sfx_volume":
                    _audioManager.SetVolume(AudioCategory.SoundEffects, item.SliderValue);
                    break;
                case "ambient_volume":
                    _audioManager.SetVolume(AudioCategory.Ambient, item.SliderValue);
                    break;
                case "voice_volume":
                    _audioManager.SetVolume(AudioCategory.Voice, item.SliderValue);
                    break;
                case "ui_volume":
                    _audioManager.SetVolume(AudioCategory.UI, item.SliderValue);
                    break;
                case "mute_all":
                    _audioManager.SetMuted(AudioCategory.Master, item.ToggleValue);
                    break;
            }
        }

        if (_graphicsManager != null)
        {
            switch (item.Key)
            {
                case "display_mode":
                    _graphicsManager.IsFullScreen = item.ChoiceIndex == 1;
                    _graphicsManager.ApplyChanges();
                    break;
            }
        }
    }

    private void SaveAndExit()
    {
        _settingsManager?.Save();
        ExitScreen();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _transitionAlpha = 1f - TransitionPosition;
        _selectionPulse = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 0.1f + 0.9f;

        if (_tabSwitchProgress < 1f)
        {
            _tabSwitchProgress = Math.Min(1f, _tabSwitchProgress + deltaTime * 5f);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null || _contentFont == null || _titleFont == null || _pixelTexture == null)
        {
            return;
        }

        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        // Background
        DrawRect(new Rectangle(0, 0, viewport.Width, viewport.Height), BackgroundColor * _transitionAlpha);

        // Header
        DrawHeader(viewport);

        // Tabs
        DrawTabs(viewport);

        // Content panel
        var contentRect = new Rectangle(60, 130, viewport.Width - 120, viewport.Height - 190);
        DrawContentPanel(contentRect);

        // Footer
        DrawFooter(viewport);

        _spriteBatch.End();
    }

    private void DrawHeader(Viewport viewport)
    {
        string title = L.Get(GameStrings.Settings_Title);
        var titleSize = _titleFont!.MeasureString(title);

        _spriteBatch!.DrawString(_titleFont, title,
            new Vector2((viewport.Width - titleSize.X) / 2, 20),
            AccentColor * _transitionAlpha);
    }

    private void DrawTabs(Viewport viewport)
    {
        int tabWidth = 140;
        int tabHeight = 35;
        int spacing = 10;
        int totalWidth = _tabs.Count * tabWidth + (_tabs.Count - 1) * spacing;
        int startX = (viewport.Width - totalWidth) / 2;
        int y = 65;

        for (int i = 0; i < _tabs.Count; i++)
        {
            var tab = _tabs[i];
            bool isActive = tab == _currentTab;

            var tabRect = new Rectangle(startX + i * (tabWidth + spacing), y, tabWidth, tabHeight);

            // Tab background
            Color bgColor = isActive ? TabActiveColor * _selectionPulse : TabInactiveColor;
            DrawRect(tabRect, bgColor * _transitionAlpha);

            // Active indicator
            if (isActive)
            {
                DrawRect(new Rectangle(tabRect.X, tabRect.Bottom - 3, tabRect.Width, 3),
                    AccentColor * _transitionAlpha);
            }

            // Tab text
            string tabName = GetTabName(tab);
            var textSize = _contentFont!.MeasureString(tabName);
            Color textColor = isActive ? Color.White : DimTextColor;

            _spriteBatch!.DrawString(_contentFont, tabName,
                new Vector2(tabRect.X + (tabRect.Width - textSize.X) / 2,
                           tabRect.Y + (tabRect.Height - textSize.Y) / 2),
                textColor * _transitionAlpha);
        }

        // Tab switch hint
        string hint = "[Q/LB] < Tab > [E/RB]";
        var hintSize = _smallFont!.MeasureString(hint);
        _spriteBatch!.DrawString(_smallFont, hint,
            new Vector2((viewport.Width - hintSize.X) / 2, y + tabHeight + 8),
            DimTextColor * 0.6f * _transitionAlpha);
    }

    private void DrawContentPanel(Rectangle bounds)
    {
        // Panel background
        DrawRect(bounds, PanelColor * _transitionAlpha);

        // Content with tab transition
        float slideOffset = (1f - _tabSwitchProgress) * 50f;
        float contentAlpha = _transitionAlpha * _tabSwitchProgress;

        var items = _tabItems[_currentTab];
        int y = bounds.Y + 20;
        int itemHeight = 50;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];

            if (item.Type == ItemType.Separator)
            {
                y += 15;
                DrawRect(new Rectangle(bounds.X + 30, y, bounds.Width - 60, 1),
                    DimTextColor * 0.3f * contentAlpha);
                y += 15;

                continue;
            }

            bool isSelected = i == _selectedIndex;

            // Selection highlight
            if (isSelected)
            {
                DrawRect(new Rectangle(bounds.X + 10, y - 2, bounds.Width - 20, itemHeight - 4),
                    SelectedColor * _selectionPulse * contentAlpha);

                // Selection indicator
                DrawRect(new Rectangle(bounds.X + 10, y, 4, itemHeight - 8),
                    AccentColor * contentAlpha);
            }

            // Label
            float labelX = bounds.X + 30 + slideOffset;
            Color labelColor = isSelected ? Color.White : TextColor;
            _spriteBatch!.DrawString(_contentFont!, item.Label,
                new Vector2(labelX, y + 12), labelColor * contentAlpha);

            // Value
            int valueX = bounds.X + bounds.Width - 300;

            switch (item.Type)
            {
                case ItemType.Slider:
                    DrawSlider(valueX, y + 15, 200, item.SliderValue, contentAlpha);
                    string sliderText = $"{item.SliderValue:P0}";
                    _spriteBatch.DrawString(_contentFont!, sliderText,
                        new Vector2(valueX + 215, y + 12), ValueColor * contentAlpha);
                    break;

                case ItemType.Toggle:
                    string toggleText = item.ToggleValue ? "ON" : "OFF";
                    Color toggleColor = item.ToggleValue ? ValueColor : WarningColor;
                    _spriteBatch.DrawString(_contentFont!, toggleText,
                        new Vector2(valueX, y + 12), toggleColor * contentAlpha);
                    break;

                case ItemType.Choice:
                    if (item.Choices != null && item.ChoiceIndex < item.Choices.Length)
                    {
                        string arrows = "< " + item.Choices[item.ChoiceIndex] + " >";
                        _spriteBatch.DrawString(_contentFont!, arrows,
                            new Vector2(valueX, y + 12), ValueColor * contentAlpha);
                    }
                    break;

                case ItemType.Button:
                    _spriteBatch.DrawString(_contentFont!, "[Press Enter]",
                        new Vector2(valueX, y + 12), AccentColor * contentAlpha);
                    break;
            }

            y += itemHeight;
        }
    }

    private void DrawSlider(int x, int y, int width, float value, float alpha)
    {
        int height = 16;

        // Background
        DrawRect(new Rectangle(x, y, width, height), SliderBgColor * alpha);

        // Fill
        int fillWidth = (int)(width * value);

        if (fillWidth > 0)
        {
            DrawRect(new Rectangle(x, y, fillWidth, height), SliderFillColor * alpha);
        }

        // Handle
        int handleX = x + fillWidth - 4;
        DrawRect(new Rectangle(handleX, y - 2, 8, height + 4), Color.White * alpha);

        // Notches
        for (int i = 1; i < 10; i++)
        {
            int notchX = x + (int)(width * i / 10f);
            DrawRect(new Rectangle(notchX, y + height - 3, 1, 3), Color.Black * 0.3f * alpha);
        }
    }

    private void DrawFooter(Viewport viewport)
    {
        string hints = "[Up/Down] Navigate  [Left/Right] Adjust  [Enter] Select  [Esc] Back";
        var hintSize = _contentFont!.MeasureString(hints);

        _spriteBatch!.DrawString(_contentFont, hints,
            new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height - 40),
            DimTextColor * _transitionAlpha);
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch?.Draw(_pixelTexture, rect, color);
    }

    private static string GetTabName(SettingsTab tab)
    {
        return tab switch
        {
            SettingsTab.Audio => L.Get(GameStrings.Settings_Audio),
            SettingsTab.Video => L.Get(GameStrings.Settings_Video),
            SettingsTab.Controls => L.Get(GameStrings.Settings_Controls),
            SettingsTab.Gameplay => L.Get(GameStrings.Settings_Gameplay),
            SettingsTab.Accessibility => L.Get(GameStrings.Settings_Accessibility),
            _ => tab.ToString()
        };
    }

    private enum ItemType { Slider, Toggle, Choice, Button, Separator }

    private class SettingsItem
    {
        public string Label { get; set; } = "";
        public string Key { get; set; } = "";
        public ItemType Type { get; set; }
        public float SliderValue { get; set; } = 0.5f;
        public bool ToggleValue { get; set; }
        public string[]? Choices { get; set; }
        public int ChoiceIndex { get; set; }
    }
}
