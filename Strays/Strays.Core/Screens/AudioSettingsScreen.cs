using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Audio;
using Strays.Core.Inputs;
using Strays.Core.ScreenManagers;

namespace Strays.Core.Screens;

/// <summary>
/// Screen for configuring audio settings.
/// </summary>
public class AudioSettingsScreen : GameScreen
{
    private readonly AudioManager _audioManager;

    private SpriteBatch? _spriteBatch;
    private SpriteFont? _titleFont;
    private SpriteFont? _contentFont;
    private Texture2D? _pixelTexture;

    private int _selectedIndex = 0;
    private float _transitionAlpha = 0f;
    private float _selectionPulse = 0f;

    // Test sound cooldown
    private float _testSoundCooldown = 0f;

    // Audio visualizer
    private readonly float[] _visualizerBars = new float[16];
    private readonly Random _random = new();

    // Colors
    private static readonly Color BackgroundColor = new(20, 25, 35);
    private static readonly Color PanelColor = new(35, 40, 55);
    private static readonly Color SelectedColor = new(50, 80, 130);
    private static readonly Color TextColor = new(220, 225, 235);
    private static readonly Color DimTextColor = new(140, 150, 170);
    private static readonly Color AccentColor = new(80, 160, 255);
    private static readonly Color MutedColor = new(255, 100, 100);
    private static readonly Color SliderBgColor = new(60, 60, 80);
    private static readonly Color VolumeGreen = new(80, 200, 120);
    private static readonly Color VolumeYellow = new(255, 200, 80);
    private static readonly Color VolumeRed = new(255, 100, 80);

    // Settings items
    private readonly List<AudioSettingItem> _items = new();

    public AudioSettingsScreen(AudioManager audioManager)
    {
        _audioManager = audioManager;

        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);

        InitializeItems();
    }

    private void InitializeItems()
    {
        _items.Add(new AudioSettingItem
        {
            Label = "Master Volume",
            Category = AudioCategory.Master,
            Type = SettingType.Slider,
            Description = "Controls all game audio"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Music Volume",
            Category = AudioCategory.Music,
            Type = SettingType.Slider,
            Description = "Background music and soundtracks"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Sound Effects",
            Category = AudioCategory.SoundEffects,
            Type = SettingType.Slider,
            Description = "Combat, movement, and interaction sounds"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Ambient",
            Category = AudioCategory.Ambient,
            Type = SettingType.Slider,
            Description = "Environmental and atmospheric sounds"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Voice",
            Category = AudioCategory.Voice,
            Type = SettingType.Slider,
            Description = "Character dialogue and voice lines"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "UI Sounds",
            Category = AudioCategory.UI,
            Type = SettingType.Slider,
            Description = "Menu navigation and interface sounds"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "",
            Type = SettingType.Separator
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Mute Master",
            Category = AudioCategory.Master,
            Type = SettingType.Toggle,
            Description = "Mute all game audio"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Mute Music",
            Category = AudioCategory.Music,
            Type = SettingType.Toggle,
            Description = "Mute background music only"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "",
            Type = SettingType.Separator
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Test Sound",
            Type = SettingType.Action,
            ActionId = "test_sound",
            Description = "Play a test sound effect"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Test Music",
            Type = SettingType.Action,
            ActionId = "test_music",
            Description = "Preview background music"
        });

        _items.Add(new AudioSettingItem
        {
            Label = "",
            Type = SettingType.Separator
        });

        _items.Add(new AudioSettingItem
        {
            Label = "Reset to Defaults",
            Type = SettingType.Action,
            ActionId = "reset",
            Description = "Reset all audio settings to default values"
        });
    }

    public override void LoadContent()
    {
        base.LoadContent();

        if (ScreenManager == null)
        {
            return;
        }

        _spriteBatch = ScreenManager.SpriteBatch;
        _contentFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
        _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public override void UnloadContent()
    {
        _pixelTexture?.Dispose();
        base.UnloadContent();
    }

    public override void HandleInput(InputState input)
    {
        if (input.IsMenuCancel(ControllingPlayer, out _))
        {
            ExitScreen();

            return;
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

        // Adjustment
        var currentItem = GetCurrentItem();

        if (currentItem != null)
        {
            if (currentItem.Type == SettingType.Slider)
            {
                float delta = 0f;

                if (input.IsMenuLeft(ControllingPlayer))
                {
                    delta = -0.05f;
                }
                else if (input.IsMenuRight(ControllingPlayer))
                {
                    delta = 0.05f;
                }

                // Fine adjustment with triggers
                if (input.CurrentGamePadStates[0].Triggers.Left > 0.5f)
                {
                    delta = -0.01f;
                }
                else if (input.CurrentGamePadStates[0].Triggers.Right > 0.5f)
                {
                    delta = 0.01f;
                }

                if (delta != 0f && currentItem.Category.HasValue)
                {
                    float current = _audioManager.GetVolume(currentItem.Category.Value);
                    _audioManager.SetVolume(currentItem.Category.Value, MathHelper.Clamp(current + delta, 0f, 1f));
                }
            }
            else if (currentItem.Type == SettingType.Toggle)
            {
                if (input.IsMenuSelect(ControllingPlayer, out _) ||
                    input.IsMenuLeft(ControllingPlayer) ||
                    input.IsMenuRight(ControllingPlayer))
                {
                    if (currentItem.Category.HasValue)
                    {
                        _audioManager.ToggleMute(currentItem.Category.Value);
                    }
                }
            }
            else if (currentItem.Type == SettingType.Action)
            {
                if (input.IsMenuSelect(ControllingPlayer, out _))
                {
                    ExecuteAction(currentItem.ActionId);
                }
            }
        }
    }

    private void NavigateUp()
    {
        do
        {
            _selectedIndex--;

            if (_selectedIndex < 0)
            {
                _selectedIndex = _items.Count - 1;
            }
        }
        while (_items[_selectedIndex].Type == SettingType.Separator);
    }

    private void NavigateDown()
    {
        do
        {
            _selectedIndex++;

            if (_selectedIndex >= _items.Count)
            {
                _selectedIndex = 0;
            }
        }
        while (_items[_selectedIndex].Type == SettingType.Separator);
    }

    private AudioSettingItem? GetCurrentItem()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
        {
            return _items[_selectedIndex];
        }

        return null;
    }

    private void ExecuteAction(string? actionId)
    {
        switch (actionId)
        {
            case "test_sound":
                if (_testSoundCooldown <= 0)
                {
                    _audioManager.PlayUIConfirm();
                    _testSoundCooldown = 0.5f;
                }
                break;

            case "test_music":
                if (_audioManager.CurrentTrack == MusicTrack.None)
                {
                    _audioManager.PlayMusic(MusicTrack.MainMenu);
                }
                else
                {
                    _audioManager.StopMusic();
                }
                break;

            case "reset":
                ResetToDefaults();
                break;
        }
    }

    private void ResetToDefaults()
    {
        _audioManager.SetVolume(AudioCategory.Master, 1.0f);
        _audioManager.SetVolume(AudioCategory.Music, 0.7f);
        _audioManager.SetVolume(AudioCategory.SoundEffects, 0.8f);
        _audioManager.SetVolume(AudioCategory.Ambient, 0.5f);
        _audioManager.SetVolume(AudioCategory.Voice, 1.0f);
        _audioManager.SetVolume(AudioCategory.UI, 0.6f);

        foreach (var category in Enum.GetValues<AudioCategory>())
        {
            _audioManager.SetMuted(category, false);
        }
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _transitionAlpha = 1f - TransitionPosition;
        _selectionPulse = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 0.1f + 0.9f;

        if (_testSoundCooldown > 0)
        {
            _testSoundCooldown -= deltaTime;
        }

        // Update visualizer bars
        for (int i = 0; i < _visualizerBars.Length; i++)
        {
            float target = _audioManager.IsMuted(AudioCategory.Master) ? 0f : _random.NextSingle() * 0.8f;
            _visualizerBars[i] = MathHelper.Lerp(_visualizerBars[i], target, deltaTime * 10f);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null || _contentFont == null || _titleFont == null || _pixelTexture == null)
        {
            return;
        }

        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);

        _spriteBatch.Begin();

        // Background
        DrawRect(new Rectangle(0, 0, viewport.Width, viewport.Height), BackgroundColor * _transitionAlpha);

        // Header
        string title = "AUDIO SETTINGS";
        var titleSize = _titleFont.MeasureString(title);
        _spriteBatch.DrawString(_titleFont, title,
            new Vector2((viewport.Width - titleSize.X) / 2, 25), AccentColor * _transitionAlpha);

        // Audio visualizer
        DrawVisualizer(new Rectangle(viewport.Width / 2 - 200, 70, 400, 40));

        // Main panel
        var panelRect = new Rectangle(80, 130, viewport.Width - 160, viewport.Height - 200);
        DrawRect(panelRect, PanelColor * _transitionAlpha);

        // Settings items
        int y = panelRect.Y + 20;
        int itemHeight = 50;

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];

            if (item.Type == SettingType.Separator)
            {
                y += itemHeight / 2;
                DrawRect(new Rectangle(panelRect.X + 20, y - 5, panelRect.Width - 40, 1),
                    DimTextColor * 0.3f * _transitionAlpha);
                y += itemHeight / 2;
                continue;
            }

            bool isSelected = i == _selectedIndex;

            if (isSelected)
            {
                DrawRect(new Rectangle(panelRect.X + 10, y - 5, panelRect.Width - 20, itemHeight - 5),
                    SelectedColor * _selectionPulse * _transitionAlpha);
            }

            // Label
            _spriteBatch.DrawString(_contentFont, item.Label,
                new Vector2(panelRect.X + 30, y + 5),
                (isSelected ? Color.White : TextColor) * _transitionAlpha);

            // Description
            if (!string.IsNullOrEmpty(item.Description))
            {
                _spriteBatch.DrawString(_contentFont, item.Description,
                    new Vector2(panelRect.X + 30, y + 25), DimTextColor * 0.7f * _transitionAlpha);
            }

            // Value display
            switch (item.Type)
            {
                case SettingType.Slider when item.Category.HasValue:
                    float volume = _audioManager.GetVolume(item.Category.Value);
                    bool isMuted = _audioManager.IsMuted(item.Category.Value);
                    DrawVolumeSlider(panelRect.X + 400, y + 8, 250, volume, isMuted);

                    string volumeText = isMuted ? "MUTED" : $"{volume:P0}";
                    Color volumeColor = isMuted ? MutedColor : DimTextColor;
                    _spriteBatch.DrawString(_contentFont, volumeText,
                        new Vector2(panelRect.X + 670, y + 5), volumeColor * _transitionAlpha);
                    break;

                case SettingType.Toggle when item.Category.HasValue:
                    bool muted = _audioManager.IsMuted(item.Category.Value);
                    string toggleText = muted ? "YES" : "NO";
                    Color toggleColor = muted ? MutedColor : VolumeGreen;
                    _spriteBatch.DrawString(_contentFont, toggleText,
                        new Vector2(panelRect.X + 400, y + 5), toggleColor * _transitionAlpha);
                    break;

                case SettingType.Action:
                    string actionText = item.ActionId == "test_music"
                        ? (_audioManager.CurrentTrack != MusicTrack.None ? "[Stop]" : "[Play]")
                        : "[Press Enter]";
                    _spriteBatch.DrawString(_contentFont, actionText,
                        new Vector2(panelRect.X + 400, y + 5), AccentColor * _transitionAlpha);
                    break;
            }

            y += itemHeight;
        }

        // Footer
        DrawFooter(viewport);

        _spriteBatch.End();
    }

    private void DrawVolumeSlider(int x, int y, int width, float value, bool muted)
    {
        int height = 20;

        // Background
        DrawRect(new Rectangle(x, y, width, height), SliderBgColor * _transitionAlpha);

        if (muted)
        {
            DrawRect(new Rectangle(x, y, width, height), MutedColor * 0.3f * _transitionAlpha);

            return;
        }

        // Fill with gradient colors based on level
        int fillWidth = (int)(width * value);

        if (fillWidth > 0)
        {
            // Green section (0-70%)
            int greenWidth = Math.Min(fillWidth, (int)(width * 0.7f));
            DrawRect(new Rectangle(x, y, greenWidth, height), VolumeGreen * _transitionAlpha);

            // Yellow section (70-90%)
            if (fillWidth > width * 0.7f)
            {
                int yellowStart = (int)(width * 0.7f);
                int yellowWidth = Math.Min(fillWidth - yellowStart, (int)(width * 0.2f));
                DrawRect(new Rectangle(x + yellowStart, y, yellowWidth, height), VolumeYellow * _transitionAlpha);
            }

            // Red section (90-100%)
            if (fillWidth > width * 0.9f)
            {
                int redStart = (int)(width * 0.9f);
                int redWidth = fillWidth - redStart;
                DrawRect(new Rectangle(x + redStart, y, redWidth, height), VolumeRed * _transitionAlpha);
            }
        }

        // Tick marks
        for (int i = 1; i < 10; i++)
        {
            int tickX = x + (int)(width * i / 10f);
            DrawRect(new Rectangle(tickX, y + height - 4, 1, 4), Color.Black * 0.3f * _transitionAlpha);
        }

        // Handle
        int handleX = x + fillWidth - 3;
        DrawRect(new Rectangle(handleX, y - 2, 6, height + 4), Color.White * _transitionAlpha);
    }

    private void DrawVisualizer(Rectangle bounds)
    {
        int barWidth = bounds.Width / _visualizerBars.Length - 4;
        float masterVolume = _audioManager.GetEffectiveVolume(AudioCategory.Master);

        for (int i = 0; i < _visualizerBars.Length; i++)
        {
            int barHeight = (int)(_visualizerBars[i] * masterVolume * bounds.Height);
            int x = bounds.X + i * (barWidth + 4);
            int y = bounds.Y + bounds.Height - barHeight;

            Color barColor = Color.Lerp(AccentColor, VolumeGreen, _visualizerBars[i]);
            DrawRect(new Rectangle(x, y, barWidth, barHeight), barColor * _transitionAlpha);
        }

        // Border
        DrawRect(new Rectangle(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, 2), PanelColor * _transitionAlpha);
        DrawRect(new Rectangle(bounds.X - 2, bounds.Y + bounds.Height, bounds.Width + 4, 2), PanelColor * _transitionAlpha);
    }

    private void DrawFooter(Viewport viewport)
    {
        string hints = "[↑↓] Navigate  [←→] Adjust Volume  [Enter] Select  [Esc] Back";
        var hintSize = _contentFont!.MeasureString(hints);

        _spriteBatch!.DrawString(_contentFont, hints,
            new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height - 35),
            DimTextColor * _transitionAlpha);
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch?.Draw(_pixelTexture, rect, color);
    }

    private enum SettingType
    {
        Slider,
        Toggle,
        Action,
        Separator
    }

    private class AudioSettingItem
    {
        public string Label { get; set; } = "";
        public string? Description { get; set; }
        public AudioCategory? Category { get; set; }
        public SettingType Type { get; set; }
        public string? ActionId { get; set; }
    }
}

