using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Strays.Core.Audio;

/// <summary>
/// Display settings for subtitles.
/// </summary>
public class SubtitleSettings
{
    /// <summary>
    /// Whether subtitles are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Show subtitles for dialogue.
    /// </summary>
    public bool ShowDialog { get; set; } = true;

    /// <summary>
    /// Show subtitles for sound effects.
    /// </summary>
    public bool ShowSoundEffects { get; set; } = false;

    /// <summary>
    /// Show subtitles for ambient sounds.
    /// </summary>
    public bool ShowAmbient { get; set; } = false;

    /// <summary>
    /// Font size multiplier (0.5-2.0).
    /// </summary>
    public float FontScale { get; set; } = 1.0f;

    /// <summary>
    /// Background opacity (0-1).
    /// </summary>
    public float BackgroundOpacity { get; set; } = 0.7f;

    /// <summary>
    /// Subtitle position on screen.
    /// </summary>
    public SubtitlePosition Position { get; set; } = SubtitlePosition.Bottom;

    /// <summary>
    /// Maximum width as percentage of screen (0.5-1.0).
    /// </summary>
    public float MaxWidthPercent { get; set; } = 0.8f;

    /// <summary>
    /// Text color.
    /// </summary>
    public Color TextColor { get; set; } = Color.White;

    /// <summary>
    /// Background color.
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// Whether to show speaker names.
    /// </summary>
    public bool ShowSpeakerNames { get; set; } = true;

    /// <summary>
    /// Color for speaker names.
    /// </summary>
    public Color SpeakerNameColor { get; set; } = new Color(120, 180, 255);

    /// <summary>
    /// Color coding for sound effect subtitles.
    /// </summary>
    public Color SoundEffectColor { get; set; } = new Color(200, 200, 200);

    /// <summary>
    /// Color coding for ambient subtitles.
    /// </summary>
    public Color AmbientColor { get; set; } = new Color(150, 200, 150);
}

/// <summary>
/// Subtitle position on screen.
/// </summary>
public enum SubtitlePosition
{
    Top,
    Bottom,
    Middle
}

/// <summary>
/// An active subtitle entry.
/// </summary>
public class SubtitleEntry
{
    public string Text { get; set; } = "";
    public string? SpeakerName { get; set; }
    public float RemainingTime { get; set; }
    public float TotalTime { get; set; }
    public SubtitleType Type { get; set; }
    public Color? OverrideColor { get; set; }
    public float FadeProgress => 1f - Math.Min(RemainingTime / 0.3f, 1f);
}

/// <summary>
/// Manages subtitle display for accessibility.
/// </summary>
public class SubtitleDisplay
{
    private readonly List<SubtitleEntry> _activeSubtitles = new();
    private readonly Queue<SubtitleEntry> _pendingSubtitles = new();

    private const int MAX_VISIBLE_SUBTITLES = 3;
    private const float MIN_DISPLAY_TIME = 1.5f;
    private const float MAX_DISPLAY_TIME = 8f;
    private const float CHARS_PER_SECOND = 15f;

    public SubtitleSettings Settings { get; } = new();

    /// <summary>
    /// Subscribes to an AudioManager's subtitle events.
    /// </summary>
    public void SubscribeTo(AudioManager audioManager)
    {
        audioManager.SubtitleTriggered += OnSubtitleTriggered;
    }

    /// <summary>
    /// Unsubscribes from an AudioManager's subtitle events.
    /// </summary>
    public void UnsubscribeFrom(AudioManager audioManager)
    {
        audioManager.SubtitleTriggered -= OnSubtitleTriggered;
    }

    private void OnSubtitleTriggered(object? sender, SubtitleEventArgs e)
    {
        AddSubtitle(e.Text, e.Duration, e.Type, e.Source);
    }

    /// <summary>
    /// Adds a subtitle to display.
    /// </summary>
    public void AddSubtitle(string text, float duration = 0, SubtitleType type = SubtitleType.Dialog, string? speaker = null)
    {
        if (!Settings.Enabled)
        {
            return;
        }

        // Check type filters
        if (type == SubtitleType.Dialog && !Settings.ShowDialog)
        {
            return;
        }

        if (type == SubtitleType.SoundEffect && !Settings.ShowSoundEffects)
        {
            return;
        }

        if (type == SubtitleType.Ambient && !Settings.ShowAmbient)
        {
            return;
        }

        // Calculate duration based on text length if not specified
        if (duration <= 0)
        {
            duration = Math.Max(MIN_DISPLAY_TIME, Math.Min(text.Length / CHARS_PER_SECOND, MAX_DISPLAY_TIME));
        }

        var entry = new SubtitleEntry
        {
            Text = text,
            SpeakerName = speaker,
            RemainingTime = duration,
            TotalTime = duration,
            Type = type
        };

        if (_activeSubtitles.Count < MAX_VISIBLE_SUBTITLES)
        {
            _activeSubtitles.Add(entry);
        }
        else
        {
            _pendingSubtitles.Enqueue(entry);
        }
    }

    /// <summary>
    /// Adds a dialog subtitle with speaker name.
    /// </summary>
    public void AddDialog(string speaker, string text, float duration = 0)
    {
        AddSubtitle(text, duration, SubtitleType.Dialog, speaker);
    }

    /// <summary>
    /// Adds a sound effect description subtitle.
    /// </summary>
    public void AddSoundEffect(string description, float duration = 1.5f)
    {
        AddSubtitle($"[{description}]", duration, SubtitleType.SoundEffect);
    }

    /// <summary>
    /// Adds an ambient sound description subtitle.
    /// </summary>
    public void AddAmbient(string description, float duration = 2f)
    {
        AddSubtitle($"({description})", duration, SubtitleType.Ambient);
    }

    /// <summary>
    /// Clears all subtitles.
    /// </summary>
    public void Clear()
    {
        _activeSubtitles.Clear();
        _pendingSubtitles.Clear();
    }

    /// <summary>
    /// Updates subtitle timings.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!Settings.Enabled)
        {
            return;
        }

        // Update active subtitles
        for (int i = _activeSubtitles.Count - 1; i >= 0; i--)
        {
            _activeSubtitles[i].RemainingTime -= deltaTime;

            if (_activeSubtitles[i].RemainingTime <= 0)
            {
                _activeSubtitles.RemoveAt(i);
            }
        }

        // Add pending subtitles if space available
        while (_activeSubtitles.Count < MAX_VISIBLE_SUBTITLES && _pendingSubtitles.Count > 0)
        {
            _activeSubtitles.Add(_pendingSubtitles.Dequeue());
        }
    }

    /// <summary>
    /// Draws subtitles on screen.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, Rectangle screenBounds)
    {
        if (!Settings.Enabled || _activeSubtitles.Count == 0)
        {
            return;
        }

        int maxWidth = (int)(screenBounds.Width * Settings.MaxWidthPercent);
        int padding = 12;
        int lineSpacing = 6;
        int entrySpacing = 10;

        // Calculate total height for all subtitles
        var entryLayouts = new List<(SubtitleEntry entry, List<string> lines, Vector2 size)>();
        int totalHeight = 0;

        foreach (var entry in _activeSubtitles)
        {
            var lines = WrapText(font, entry, maxWidth - padding * 2, Settings.FontScale);
            var size = MeasureSubtitle(font, lines, Settings.FontScale, padding, lineSpacing, entry.SpeakerName != null && Settings.ShowSpeakerNames);

            entryLayouts.Add((entry, lines, size));
            totalHeight += (int)size.Y + entrySpacing;
        }

        totalHeight -= entrySpacing; // Remove last spacing

        // Calculate starting Y position based on settings
        int startY = Settings.Position switch
        {
            SubtitlePosition.Top => 50,
            SubtitlePosition.Middle => (screenBounds.Height - totalHeight) / 2,
            _ => screenBounds.Height - totalHeight - 80 // Bottom with margin
        };

        // Draw each subtitle
        int currentY = startY;

        foreach (var (entry, lines, size) in entryLayouts)
        {
            float alpha = 1f;

            // Fade out when time is low
            if (entry.RemainingTime < 0.5f)
            {
                alpha = entry.RemainingTime / 0.5f;
            }

            // Fade in for first 0.2 seconds
            float elapsed = entry.TotalTime - entry.RemainingTime;

            if (elapsed < 0.2f)
            {
                alpha = Math.Min(alpha, elapsed / 0.2f);
            }

            DrawSubtitle(spriteBatch, font, pixelTexture, entry, lines, screenBounds, currentY, (int)size.X, (int)size.Y, alpha, padding, lineSpacing);

            currentY += (int)size.Y + entrySpacing;
        }
    }

    private List<string> WrapText(SpriteFont font, SubtitleEntry entry, int maxWidth, float scale)
    {
        var lines = new List<string>();
        var words = entry.Text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var lineWidth = font.MeasureString(testLine).X * scale;

            if (lineWidth > maxWidth && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    private Vector2 MeasureSubtitle(SpriteFont font, List<string> lines, float scale, int padding, int lineSpacing, bool hasSpeaker)
    {
        float maxWidth = 0;
        float totalHeight = padding * 2;

        if (hasSpeaker)
        {
            totalHeight += font.MeasureString("Speaker").Y * scale + lineSpacing;
        }

        foreach (var line in lines)
        {
            var size = font.MeasureString(line) * scale;
            maxWidth = Math.Max(maxWidth, size.X);
            totalHeight += size.Y + lineSpacing;
        }

        totalHeight -= lineSpacing; // Remove last line spacing

        return new Vector2(maxWidth + padding * 2, totalHeight);
    }

    private void DrawSubtitle(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture,
        SubtitleEntry entry, List<string> lines, Rectangle screenBounds, int y, int width, int height,
        float alpha, int padding, int lineSpacing)
    {
        int x = (screenBounds.Width - width) / 2;

        // Background
        var bgColor = Settings.BackgroundColor * Settings.BackgroundOpacity * alpha;
        spriteBatch.Draw(pixelTexture, new Rectangle(x, y, width, height), bgColor);

        // Border accent based on type
        Color accentColor = entry.Type switch
        {
            SubtitleType.Dialog => Settings.SpeakerNameColor,
            SubtitleType.SoundEffect => Settings.SoundEffectColor,
            SubtitleType.Ambient => Settings.AmbientColor,
            _ => Settings.TextColor
        };

        spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 3, height), accentColor * alpha);

        int textY = y + padding;

        // Speaker name
        if (entry.SpeakerName != null && Settings.ShowSpeakerNames && entry.Type == SubtitleType.Dialog)
        {
            spriteBatch.DrawString(font, entry.SpeakerName,
                new Vector2(x + padding, textY),
                Settings.SpeakerNameColor * alpha,
                0f, Vector2.Zero, Settings.FontScale, SpriteEffects.None, 0f);

            textY += (int)(font.MeasureString(entry.SpeakerName).Y * Settings.FontScale) + lineSpacing;
        }

        // Text lines
        Color textColor = entry.OverrideColor ?? entry.Type switch
        {
            SubtitleType.SoundEffect => Settings.SoundEffectColor,
            SubtitleType.Ambient => Settings.AmbientColor,
            _ => Settings.TextColor
        };

        foreach (var line in lines)
        {
            spriteBatch.DrawString(font, line,
                new Vector2(x + padding, textY),
                textColor * alpha,
                0f, Vector2.Zero, Settings.FontScale, SpriteEffects.None, 0f);

            textY += (int)(font.MeasureString(line).Y * Settings.FontScale) + lineSpacing;
        }
    }
}
