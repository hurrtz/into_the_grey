using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lazarus.Core.Localization;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Enhanced loading screen with animated progress indicator, tips, and visual polish.
/// Coordinates transitions between screens while showing loading progress.
/// </summary>
public class LoadingScreen : GameScreen
{
    private readonly bool _loadingIsSlow;
    private bool _otherScreensAreGone;
    private readonly GameScreen[] _screensToLoad;

    private SpriteBatch? _spriteBatch;
    private SpriteFont? _titleFont;
    private SpriteFont? _contentFont;
    private SpriteFont? _smallFont;
    private Texture2D? _pixelTexture;

    // Animation state
    private float _progress = 0f;
    private float _displayProgress = 0f;
    private float _spinnerAngle = 0f;
    private float _pulseTimer = 0f;
    private float _tipTimer = 0f;
    private int _currentTipIndex = 0;
    private float _tipFadeAlpha = 1f;

    // Particle effects
    private readonly List<LoadingParticle> _particles = new();
    private readonly Random _random = new();

    // Colors
    private static readonly Color BackgroundColor = new(12, 14, 20);
    private static readonly Color PrimaryColor = new(80, 180, 255);
    private static readonly Color SecondaryColor = new(120, 255, 180);
    private static readonly Color AccentColor = new(255, 100, 80);
    private static readonly Color TextColor = new(200, 205, 215);
    private static readonly Color DimTextColor = new(100, 110, 130);
    private static readonly Color ProgressBgColor = new(30, 35, 50);
    private static readonly Color ProgressFillColor = new(60, 140, 220);

    // Loading tips
    private static readonly string[] LoadingTips =
    {
        "Recruit Strays to expand your party and discover new abilities.",
        "Different biomes contain unique Strays - explore to find them all!",
        "Microchips can be upgraded to enhance your Strays' powers.",
        "Pay attention to type matchups in combat for super effective attacks.",
        "Some Strays can evolve into more powerful forms at higher levels.",
        "Visit merchants in settlements to stock up on healing items.",
        "Keep an eye on faction reputation - it affects NPC interactions.",
        "The mini-map shows nearby points of interest and quest markers.",
        "Auto-save occurs when entering new areas. Manual saves are recommended!",
        "Weather affects combat - Data Storms boost certain elemental attacks.",
        "Your companion will intervene in dire situations through Gravitation.",
        "Corrupted Strays are more powerful but may have unpredictable behavior.",
        "Check the Bestiary to track Stray encounters and evolution requirements.",
        "Some quests have multiple solutions based on your choices.",
        "The Archive Scar is the most dangerous region - prepare carefully!"
    };

    /// <summary>
    /// The constructor is private: loading screens should
    /// be activated via the static Load method instead.
    /// </summary>
    private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow, GameScreen[] screensToLoad)
    {
        _loadingIsSlow = loadingIsSlow;
        _screensToLoad = screensToLoad;
        _currentTipIndex = new Random().Next(LoadingTips.Length);

        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.3);
    }

    /// <summary>
    /// Activates the loading screen.
    /// </summary>
    public static void Load(ScreenManager screenManager, bool loadingIsSlow, PlayerIndex? controllingPlayer,
        params GameScreen[] screensToLoad)
    {
        // Tell all the current screens to transition off.
        foreach (GameScreen screen in screenManager.GetScreens())
        {
            screen.ExitScreen();
        }

        // Create and activate the loading screen.
        LoadingScreen loadingScreen = new(screenManager, loadingIsSlow, screensToLoad);
        screenManager.AddScreen(loadingScreen, controllingPlayer);
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

        // Initialize particles
        var viewport = ScreenManager.GraphicsDevice.Viewport;

        for (int i = 0; i < 30; i++)
        {
            SpawnParticle(viewport, true);
        }
    }

    private void SpawnParticle(Viewport viewport, bool randomY = false)
    {
        _particles.Add(new LoadingParticle
        {
            Position = new Vector2(
                _random.Next(viewport.Width),
                randomY ? _random.Next(viewport.Height) : viewport.Height + 10),
            Velocity = new Vector2(
                (_random.NextSingle() - 0.5f) * 30f,
                -30f - _random.NextSingle() * 50f),
            Size = 1 + _random.Next(3),
            Lifetime = 5f + _random.NextSingle() * 5f,
            Color = _random.NextSingle() > 0.7f ? PrimaryColor : SecondaryColor,
            Alpha = 0.3f + _random.NextSingle() * 0.4f
        });
    }

    public override void UnloadContent()
    {
        _pixelTexture?.Dispose();
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // If all the previous screens have finished transitioning
        // off, it is time to actually perform the load.
        if (_otherScreensAreGone)
        {
            ScreenManager?.RemoveScreen(this);

            foreach (GameScreen screen in _screensToLoad)
            {
                if (screen != null)
                {
                    ScreenManager?.AddScreen(screen, ControllingPlayer);
                }
            }

            ScreenManager?.Game.ResetElapsedTime();

            return;
        }

        // Simulate progress (since we can't get actual loading progress)
        if (_loadingIsSlow)
        {
            // Accelerating progress that slows near 90%
            float targetProgress = 0.9f + (ScreenState == ScreenState.Active ? 0.1f : 0f);
            float speed = (1f - _progress) * 0.5f + 0.1f;
            _progress = Math.Min(targetProgress, _progress + deltaTime * speed);
        }
        else
        {
            _progress = Math.Min(1f, _progress + deltaTime * 2f);
        }

        // Smooth display progress
        _displayProgress = MathHelper.Lerp(_displayProgress, _progress, deltaTime * 8f);

        // Spinner animation
        _spinnerAngle += deltaTime * 4f;

        // Pulse animation
        _pulseTimer += deltaTime;

        // Tip rotation
        _tipTimer += deltaTime;

        if (_tipTimer > 6f)
        {
            _tipTimer = 0f;
            _tipFadeAlpha = 0f;
            _currentTipIndex = (_currentTipIndex + 1) % LoadingTips.Length;
        }

        // Tip fade in
        if (_tipFadeAlpha < 1f)
        {
            _tipFadeAlpha = Math.Min(1f, _tipFadeAlpha + deltaTime * 2f);
        }

        // Update particles
        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);

        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            particle.Position += particle.Velocity * deltaTime;
            particle.Lifetime -= deltaTime;

            if (particle.Lifetime <= 0 || particle.Position.Y < -20)
            {
                _particles.RemoveAt(i);
                SpawnParticle(viewport);
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Check if other screens are gone
        if ((ScreenState == ScreenState.Active) && (ScreenManager?.GetScreens().Length == 1))
        {
            _otherScreensAreGone = true;
        }

        if (!_loadingIsSlow)
        {
            return;
        }

        if (_spriteBatch == null || _pixelTexture == null || _contentFont == null)
        {
            return;
        }

        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);
        float alpha = TransitionAlpha;

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        // Background
        DrawRect(new Rectangle(0, 0, viewport.Width, viewport.Height), BackgroundColor * alpha);

        // Particles
        DrawParticles(alpha);

        // Main content
        DrawCenterContent(viewport, alpha);

        // Progress bar
        DrawProgressBar(viewport, alpha);

        // Tip
        DrawTip(viewport, alpha);

        // Scanlines
        DrawScanlines(viewport, alpha);

        _spriteBatch.End();
    }

    private void DrawParticles(float alpha)
    {
        foreach (var particle in _particles)
        {
            float particleAlpha = Math.Min(1f, particle.Lifetime / 2f) * particle.Alpha;
            DrawRect(new Rectangle((int)particle.Position.X, (int)particle.Position.Y, particle.Size, particle.Size),
                particle.Color * particleAlpha * alpha);
        }
    }

    private void DrawCenterContent(Viewport viewport, float alpha)
    {
        int centerY = viewport.Height / 2 - 60;

        // Animated spinner
        DrawSpinner(viewport.Width / 2, centerY - 30, 40, alpha);

        // Title
        string title = "LOADING";
        var titleSize = _titleFont!.MeasureString(title);
        float titleX = (viewport.Width - titleSize.X) / 2;

        // Glitch effect on title
        float glitch = (float)Math.Sin(_pulseTimer * 10) * 2f;
        _spriteBatch!.DrawString(_titleFont, title,
            new Vector2(titleX + glitch, centerY + 30), PrimaryColor * alpha);

        // Animated dots
        int dotCount = (int)(_pulseTimer * 3) % 4;
        string dots = new('.', dotCount);
        _spriteBatch.DrawString(_titleFont, dots,
            new Vector2(titleX + titleSize.X + 5, centerY + 30), PrimaryColor * alpha);

        // Percentage
        string percent = $"{_displayProgress * 100:F0}%";
        var percentSize = _contentFont!.MeasureString(percent);
        _spriteBatch.DrawString(_contentFont, percent,
            new Vector2((viewport.Width - percentSize.X) / 2, centerY + 70),
            TextColor * alpha);
    }

    private void DrawSpinner(int centerX, int centerY, int radius, float alpha)
    {
        int segments = 8;

        for (int i = 0; i < segments; i++)
        {
            float angle = _spinnerAngle + (i * MathHelper.TwoPi / segments);
            float segmentAlpha = (i / (float)segments);

            int x = centerX + (int)(Math.Cos(angle) * radius);
            int y = centerY + (int)(Math.Sin(angle) * radius);

            Color color = Color.Lerp(PrimaryColor, SecondaryColor, segmentAlpha);
            DrawRect(new Rectangle(x - 3, y - 3, 6, 6), color * segmentAlpha * alpha);
        }

        // Center dot
        DrawRect(new Rectangle(centerX - 4, centerY - 4, 8, 8), PrimaryColor * alpha);
    }

    private void DrawProgressBar(Viewport viewport, float alpha)
    {
        int barWidth = 400;
        int barHeight = 8;
        int barX = (viewport.Width - barWidth) / 2;
        int barY = viewport.Height / 2 + 50;

        // Background
        DrawRect(new Rectangle(barX - 2, barY - 2, barWidth + 4, barHeight + 4), ProgressBgColor * alpha);

        // Fill
        int fillWidth = (int)(barWidth * _displayProgress);

        if (fillWidth > 0)
        {
            // Gradient fill effect
            for (int i = 0; i < fillWidth; i++)
            {
                float t = i / (float)barWidth;
                Color fillColor = Color.Lerp(PrimaryColor, SecondaryColor, t);
                DrawRect(new Rectangle(barX + i, barY, 1, barHeight), fillColor * alpha);
            }

            // Animated highlight
            int highlightX = barX + (int)((_pulseTimer * 100) % fillWidth);

            if (highlightX < barX + fillWidth - 20)
            {
                for (int i = 0; i < 20; i++)
                {
                    float highlightAlpha = 1f - (i / 20f);
                    DrawRect(new Rectangle(highlightX + i, barY, 1, barHeight),
                        Color.White * highlightAlpha * 0.3f * alpha);
                }
            }
        }

        // Border glow
        DrawRect(new Rectangle(barX - 1, barY - 1, barWidth + 2, 1), PrimaryColor * 0.3f * alpha);
        DrawRect(new Rectangle(barX - 1, barY + barHeight, barWidth + 2, 1), PrimaryColor * 0.3f * alpha);
    }

    private void DrawTip(Viewport viewport, float alpha)
    {
        if (_currentTipIndex >= LoadingTips.Length)
        {
            return;
        }

        string tip = LoadingTips[_currentTipIndex];
        int maxWidth = viewport.Width - 200;

        // Word wrap if needed
        string wrappedTip = WrapText(tip, maxWidth);

        // Tip header
        string header = "TIP";
        var headerSize = _smallFont!.MeasureString(header);
        int headerX = (viewport.Width - (int)headerSize.X) / 2;
        int tipY = viewport.Height - 120;

        _spriteBatch!.DrawString(_smallFont, header,
            new Vector2(headerX, tipY), AccentColor * alpha * _tipFadeAlpha);

        // Tip text
        var tipSize = _contentFont!.MeasureString(wrappedTip);
        int tipX = (viewport.Width - (int)tipSize.X) / 2;

        _spriteBatch.DrawString(_contentFont, wrappedTip,
            new Vector2(tipX, tipY + 25), DimTextColor * alpha * _tipFadeAlpha);

        // Decorative lines
        int lineWidth = 50;
        int lineY = tipY + 10;

        DrawRect(new Rectangle(headerX - lineWidth - 15, lineY, lineWidth, 1), AccentColor * 0.5f * alpha * _tipFadeAlpha);
        DrawRect(new Rectangle(headerX + (int)headerSize.X + 15, lineY, lineWidth, 1), AccentColor * 0.5f * alpha * _tipFadeAlpha);
    }

    private string WrapText(string text, int maxWidth)
    {
        if (_contentFont == null)
        {
            return text;
        }

        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var testWidth = _contentFont.MeasureString(testLine).X;

            if (testWidth > maxWidth && !string.IsNullOrEmpty(currentLine))
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

        return string.Join("\n", lines);
    }

    private void DrawScanlines(Viewport viewport, float alpha)
    {
        for (int y = 0; y < viewport.Height; y += 4)
        {
            DrawRect(new Rectangle(0, y, viewport.Width, 1), Color.Black * 0.08f * alpha);
        }
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch?.Draw(_pixelTexture, rect, color);
    }

    private class LoadingParticle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public int Size { get; set; }
        public float Lifetime { get; set; }
        public Color Color { get; set; }
        public float Alpha { get; set; }
    }
}
