using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Inputs;
using Strays.Core.Localization;
using Strays.Core.Services;
using Strays.Core.ScreenManagers;

namespace Strays.Core.Screens;

/// <summary>
/// Polished main menu screen for Into The Grey (Strays).
/// Features animated title, particle effects, and stylized menu entries.
/// </summary>
public class StraysMainMenuScreen : GameScreen
{
    private GameStateService? _gameState;
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _titleFont;
    private SpriteFont? _menuFont;
    private SpriteFont? _smallFont;
    private Texture2D? _pixelTexture;

    private bool _hasContinueData;
    private readonly List<MainMenuItem> _menuItems = new();
    private int _selectedIndex = 0;

    // Animation state
    private float _titleGlitchTimer = 0f;
    private float _titleGlitchOffset = 0f;
    private bool _titleGlitching = false;
    private float _menuAppearProgress = 0f;
    private float _totalTime = 0f;

    // Background particles
    private readonly List<DataFragment> _fragments = new();
    private readonly List<Star> _stars = new();
    private readonly Random _random = new();

    // Colors
    private static readonly Color PrimaryColor = new(80, 180, 255);
    private static readonly Color SecondaryColor = new(255, 100, 80);
    private static readonly Color AccentColor = new(120, 255, 180);
    private static readonly Color DarkBackground = new(12, 14, 20);
    private static readonly Color MediumBackground = new(20, 24, 35);

    // Title text
    private const string TITLE_TEXT = "INTO THE GREY";
    private const string SUBTITLE_TEXT = "A journey through the wasteland";

    public StraysMainMenuScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.8);
        TransitionOffTime = TimeSpan.FromSeconds(0.4);
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
            _menuFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
            _smallFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/Hud");
        }
        catch
        {
            _titleFont = ScreenManager.Font;
            _menuFont = ScreenManager.Font;
            _smallFont = ScreenManager.Font;
        }

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Get or create GameStateService
        _gameState = ScreenManager.Game.Services.GetService<GameStateService>();

        if (_gameState == null)
        {
            _gameState = new GameStateService();
            ScreenManager.Game.Services.AddService(typeof(GameStateService), _gameState);
        }

        // Check for existing saves
        _hasContinueData = _gameState.AutoSaveExists() || _gameState.SaveExists(0) ||
                           _gameState.SaveExists(1) || _gameState.SaveExists(2);

        InitializeMenuItems();
        InitializeBackground();
    }

    private void InitializeMenuItems()
    {
        _menuItems.Clear();

        _menuItems.Add(new MainMenuItem
        {
            Text = L.Get(GameStrings.Menu_NewGame),
            Action = OnNewGame,
            Icon = "▶"
        });

        if (_hasContinueData)
        {
            _menuItems.Add(new MainMenuItem
            {
                Text = L.Get(GameStrings.Menu_Continue),
                Action = OnContinue,
                Icon = "↻"
            });

            _menuItems.Add(new MainMenuItem
            {
                Text = L.Get(GameStrings.Menu_LoadGame),
                Action = OnLoadGame,
                Icon = "📁"
            });
        }

        _menuItems.Add(new MainMenuItem
        {
            Text = L.Get(GameStrings.Menu_Settings),
            Action = OnSettings,
            Icon = "⚙"
        });

        _menuItems.Add(new MainMenuItem
        {
            Text = L.Get(GameStrings.Menu_Quit),
            Action = OnQuit,
            Icon = "✕"
        });
    }

    private void InitializeBackground()
    {
        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);

        // Initialize stars
        for (int i = 0; i < 80; i++)
        {
            _stars.Add(new Star
            {
                Position = new Vector2(_random.Next(viewport.Width), _random.Next((int)(viewport.Height * 0.6f))),
                Size = _random.Next(1, 3),
                Twinkle = (float)_random.NextDouble() * MathHelper.TwoPi,
                TwinkleSpeed = 1f + (float)_random.NextDouble() * 2f
            });
        }

        // Initialize data fragments
        for (int i = 0; i < 20; i++)
        {
            SpawnFragment(viewport);
        }
    }

    private void SpawnFragment(Viewport viewport)
    {
        _fragments.Add(new DataFragment
        {
            Position = new Vector2(_random.Next(viewport.Width), viewport.Height + _random.Next(100)),
            Velocity = new Vector2((_random.NextSingle() - 0.5f) * 20f, -20f - _random.NextSingle() * 40f),
            Size = 2 + _random.Next(4),
            Lifetime = 8f + _random.NextSingle() * 4f,
            Color = _random.NextSingle() > 0.7f ? PrimaryColor : AccentColor,
            Rotation = _random.NextSingle() * MathHelper.TwoPi
        });
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    public override void HandleInput(InputState input)
    {
        if (input.IsMenuUp(ControllingPlayer))
        {
            _selectedIndex--;

            if (_selectedIndex < 0)
            {
                _selectedIndex = _menuItems.Count - 1;
            }
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _selectedIndex++;

            if (_selectedIndex >= _menuItems.Count)
            {
                _selectedIndex = 0;
            }
        }

        if (input.IsMenuSelect(ControllingPlayer, out var playerIndex))
        {
            _menuItems[_selectedIndex].Action?.Invoke(playerIndex);
        }

        if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            OnQuit(playerIndex);
        }
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _totalTime += deltaTime;

        // Menu appear animation
        if (_menuAppearProgress < 1f)
        {
            _menuAppearProgress = Math.Min(1f, _menuAppearProgress + deltaTime * 2f);
        }

        // Title glitch effect
        _titleGlitchTimer += deltaTime;

        if (!_titleGlitching && _random.NextSingle() < 0.002f)
        {
            _titleGlitching = true;
            _titleGlitchTimer = 0f;
        }

        if (_titleGlitching)
        {
            _titleGlitchOffset = (_random.NextSingle() - 0.5f) * 8f;

            if (_titleGlitchTimer > 0.15f)
            {
                _titleGlitching = false;
                _titleGlitchOffset = 0f;
            }
        }

        // Update menu items
        for (int i = 0; i < _menuItems.Count; i++)
        {
            var item = _menuItems[i];
            float targetScale = i == _selectedIndex ? 1.1f : 1f;
            item.Scale = MathHelper.Lerp(item.Scale, targetScale, deltaTime * 10f);
            item.Offset = MathHelper.Lerp(item.Offset, i == _selectedIndex ? 20f : 0f, deltaTime * 8f);
        }

        // Update stars
        foreach (var star in _stars)
        {
            star.Twinkle += deltaTime * star.TwinkleSpeed;
        }

        // Update fragments
        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);

        for (int i = _fragments.Count - 1; i >= 0; i--)
        {
            var frag = _fragments[i];
            frag.Position += frag.Velocity * deltaTime;
            frag.Rotation += deltaTime * 2f;
            frag.Lifetime -= deltaTime;

            if (frag.Lifetime <= 0 || frag.Position.Y < -50)
            {
                _fragments.RemoveAt(i);
                SpawnFragment(viewport);
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null || _pixelTexture == null || _titleFont == null)
        {
            return;
        }

        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);
        float alpha = 1f - TransitionPosition;

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        // Background
        DrawBackground(viewport, alpha);

        // Title
        DrawTitle(viewport, alpha);

        // Menu
        DrawMenu(viewport, alpha);

        // Footer
        DrawFooter(viewport, alpha);

        _spriteBatch.End();
    }

    private void DrawBackground(Viewport viewport, float alpha)
    {
        // Dark gradient
        _spriteBatch!.Draw(_pixelTexture!, new Rectangle(0, 0, viewport.Width, viewport.Height),
            DarkBackground * alpha);

        // Horizon glow
        int horizonY = (int)(viewport.Height * 0.65f);

        for (int i = 0; i < 50; i++)
        {
            float glowAlpha = (1f - i / 50f) * 0.15f;
            _spriteBatch.Draw(_pixelTexture!,
                new Rectangle(0, horizonY - i * 3, viewport.Width, 3),
                PrimaryColor * glowAlpha * alpha);
        }

        // Stars
        foreach (var star in _stars)
        {
            float twinkle = 0.4f + (float)Math.Sin(star.Twinkle) * 0.3f;
            _spriteBatch.Draw(_pixelTexture!,
                new Rectangle((int)star.Position.X, (int)star.Position.Y, star.Size, star.Size),
                Color.White * twinkle * alpha);
        }

        // Silhouette ground
        DrawSilhouette(viewport, horizonY, alpha);

        // Data fragments
        foreach (var frag in _fragments)
        {
            float fragAlpha = Math.Min(1f, frag.Lifetime / 2f);
            _spriteBatch.Draw(_pixelTexture!,
                new Rectangle((int)frag.Position.X, (int)frag.Position.Y, frag.Size, frag.Size),
                frag.Color * fragAlpha * 0.6f * alpha);
        }

        // Scanline effect
        for (int y = 0; y < viewport.Height; y += 4)
        {
            _spriteBatch.Draw(_pixelTexture!,
                new Rectangle(0, y, viewport.Width, 1),
                Color.Black * 0.1f * alpha);
        }

        // Vignette effect (corners)
        DrawVignette(viewport, alpha);
    }

    private void DrawSilhouette(Viewport viewport, int horizonY, float alpha)
    {
        // Ground
        _spriteBatch!.Draw(_pixelTexture!,
            new Rectangle(0, horizonY, viewport.Width, viewport.Height - horizonY),
            MediumBackground * alpha);

        // Buildings
        DrawBuilding(80, horizonY, 50, 120, 0.8f, alpha);
        DrawBuilding(160, horizonY, 70, 160, 0.7f, alpha);
        DrawBuilding(260, horizonY, 45, 90, 0.85f, alpha);
        DrawBuilding(viewport.Width - 200, horizonY, 60, 140, 0.75f, alpha);
        DrawBuilding(viewport.Width - 100, horizonY, 50, 100, 0.8f, alpha);

        // Distant radio tower
        int towerX = viewport.Width / 2 + 150;
        _spriteBatch.Draw(_pixelTexture!, new Rectangle(towerX, horizonY - 200, 3, 200),
            new Color(30, 35, 50) * alpha);

        // Blinking light on tower
        float blink = (float)Math.Sin(_totalTime * 3) > 0.5f ? 1f : 0.2f;
        _spriteBatch.Draw(_pixelTexture!, new Rectangle(towerX - 1, horizonY - 205, 5, 5),
            SecondaryColor * blink * alpha);
    }

    private void DrawBuilding(int x, int groundY, int width, int height, float darkness, float alpha)
    {
        Color buildingColor = new Color(
            (int)(20 * darkness),
            (int)(25 * darkness),
            (int)(35 * darkness)
        );

        _spriteBatch!.Draw(_pixelTexture!, new Rectangle(x, groundY - height, width, height),
            buildingColor * alpha);

        // Broken top
        var random = new Random(x);

        for (int i = 0; i < width; i += 10)
        {
            int dip = random.Next(3, 15);
            _spriteBatch.Draw(_pixelTexture!,
                new Rectangle(x + i, groundY - height - dip, 8, dip),
                buildingColor * alpha);
        }

        // Window lights (few)
        for (int i = 0; i < 3; i++)
        {
            int wx = x + random.Next(5, width - 8);
            int wy = groundY - random.Next(20, height - 10);

            if (random.NextSingle() > 0.7f)
            {
                Color windowColor = random.NextSingle() > 0.5f ? PrimaryColor : AccentColor;
                _spriteBatch.Draw(_pixelTexture!, new Rectangle(wx, wy, 4, 6),
                    windowColor * 0.4f * alpha);
            }
        }
    }

    private void DrawVignette(Viewport viewport, float alpha)
    {
        int vignetteSize = 200;

        // Top corners
        for (int i = 0; i < vignetteSize; i++)
        {
            float vigAlpha = (1f - i / (float)vignetteSize) * 0.5f;
            _spriteBatch!.Draw(_pixelTexture!, new Rectangle(0, i, viewport.Width, 1),
                Color.Black * vigAlpha * alpha);
        }

        // Bottom
        for (int i = 0; i < vignetteSize; i++)
        {
            float vigAlpha = (i / (float)vignetteSize) * 0.6f;
            _spriteBatch!.Draw(_pixelTexture!,
                new Rectangle(0, viewport.Height - vignetteSize + i, viewport.Width, 1),
                Color.Black * vigAlpha * alpha);
        }
    }

    private void DrawTitle(Viewport viewport, float alpha)
    {
        float appearProgress = EaseOutCubic(_menuAppearProgress);
        float titleY = 60 + (1f - appearProgress) * -50f;

        // Title shadow layers for depth
        var titleSize = _titleFont!.MeasureString(TITLE_TEXT);
        float titleX = (viewport.Width - titleSize.X) / 2;

        // Glitch effect - RGB split
        if (_titleGlitching)
        {
            _spriteBatch!.DrawString(_titleFont, TITLE_TEXT,
                new Vector2(titleX - 3 + _titleGlitchOffset, titleY),
                new Color(255, 0, 0, 100) * alpha * appearProgress);

            _spriteBatch.DrawString(_titleFont, TITLE_TEXT,
                new Vector2(titleX + 3 - _titleGlitchOffset, titleY),
                new Color(0, 255, 255, 100) * alpha * appearProgress);
        }

        // Main title
        _spriteBatch!.DrawString(_titleFont, TITLE_TEXT,
            new Vector2(titleX + _titleGlitchOffset, titleY),
            Color.White * alpha * appearProgress);

        // Subtitle
        float subtitleDelay = Math.Max(0, appearProgress - 0.3f) / 0.7f;
        var subtitleSize = _menuFont!.MeasureString(SUBTITLE_TEXT);
        float subtitleX = (viewport.Width - subtitleSize.X) / 2;

        _spriteBatch.DrawString(_menuFont, SUBTITLE_TEXT,
            new Vector2(subtitleX, titleY + titleSize.Y + 10),
            new Color(150, 160, 180) * alpha * subtitleDelay);

        // Decorative line under title
        int lineWidth = (int)(300 * appearProgress);
        int lineX = viewport.Width / 2 - lineWidth / 2;
        int lineY = (int)(titleY + titleSize.Y + subtitleSize.Y + 25);

        _spriteBatch.Draw(_pixelTexture!, new Rectangle(lineX, lineY, lineWidth, 2),
            PrimaryColor * 0.5f * alpha * appearProgress);

        // Glowing dot in center of line
        _spriteBatch.Draw(_pixelTexture!,
            new Rectangle(viewport.Width / 2 - 3, lineY - 1, 6, 4),
            PrimaryColor * alpha * appearProgress);
    }

    private void DrawMenu(Viewport viewport, float alpha)
    {
        int startY = viewport.Height / 2 - 20;
        int itemHeight = 50;

        for (int i = 0; i < _menuItems.Count; i++)
        {
            var item = _menuItems[i];
            bool isSelected = i == _selectedIndex;

            // Staggered appear animation
            float itemDelay = Math.Max(0, _menuAppearProgress - 0.2f - i * 0.1f) / (0.8f - i * 0.1f);
            itemDelay = Math.Min(1f, itemDelay);
            float itemAlpha = alpha * itemDelay;

            int y = startY + i * itemHeight;
            float offsetX = item.Offset + (1f - itemDelay) * 100f;

            // Selection highlight background
            if (isSelected)
            {
                float pulse = 0.3f + (float)Math.Sin(_totalTime * 4) * 0.1f;
                _spriteBatch!.Draw(_pixelTexture!,
                    new Rectangle(viewport.Width / 2 - 150 + (int)offsetX, y - 5, 300, itemHeight - 5),
                    PrimaryColor * pulse * itemAlpha);

                // Selection indicator line
                _spriteBatch.Draw(_pixelTexture!,
                    new Rectangle(viewport.Width / 2 - 155 + (int)offsetX, y, 4, itemHeight - 10),
                    AccentColor * itemAlpha);
            }

            // Menu text
            string text = item.Text;
            var textSize = _menuFont!.MeasureString(text);
            float textX = viewport.Width / 2 - textSize.X / 2 + offsetX;
            float textY = y + (itemHeight - textSize.Y) / 2 - 5;

            Color textColor = isSelected ? Color.White : new Color(180, 185, 200);
            float scale = item.Scale;

            // Draw with scale
            Vector2 origin = textSize / 2;
            _spriteBatch!.DrawString(_menuFont, text,
                new Vector2(textX + textSize.X / 2, textY + textSize.Y / 2),
                textColor * itemAlpha, 0f, origin, scale, SpriteEffects.None, 0f);

            // Icon
            if (!string.IsNullOrEmpty(item.Icon) && isSelected)
            {
                _spriteBatch.DrawString(_menuFont, item.Icon,
                    new Vector2(viewport.Width / 2 - 140 + offsetX, textY),
                    AccentColor * itemAlpha * 0.8f);
            }
        }
    }

    private void DrawFooter(Viewport viewport, float alpha)
    {
        float footerAlpha = alpha * Math.Min(1f, _menuAppearProgress * 2f);

        // Version
        string version = "v0.1.0 - Development Build";
        _spriteBatch!.DrawString(_smallFont!, version,
            new Vector2(15, viewport.Height - 30),
            new Color(80, 85, 100) * footerAlpha);

        // Controls hint
        string controls = "[↑↓] Select   [Enter] Confirm   [Esc] Quit";
        var controlsSize = _smallFont!.MeasureString(controls);
        _spriteBatch.DrawString(_smallFont, controls,
            new Vector2(viewport.Width - controlsSize.X - 15, viewport.Height - 30),
            new Color(80, 85, 100) * footerAlpha);

        // Copyright
        string copyright = "© 2024 Strays Dev Team";
        var copyrightSize = _smallFont!.MeasureString(copyright);
        _spriteBatch.DrawString(_smallFont, copyright,
            new Vector2((viewport.Width - copyrightSize.X) / 2, viewport.Height - 30),
            new Color(60, 65, 80) * footerAlpha);
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - (float)Math.Pow(1f - t, 3);
    }

    #region Menu Actions

    private void OnNewGame(PlayerIndex playerIndex)
    {
        if (_hasContinueData)
        {
            var confirmBox = new MessageBoxScreen("Start a new game? You can still load previous saves.");
            confirmBox.Accepted += (s, args) =>
            {
                ScreenManager?.AddScreen(new CompanionSelectScreen(), playerIndex);
            };
            ScreenManager?.AddScreen(confirmBox, playerIndex);
        }
        else
        {
            ScreenManager?.AddScreen(new CompanionSelectScreen(), playerIndex);
        }
    }

    private void OnContinue(PlayerIndex playerIndex)
    {
        if (_gameState == null || ScreenManager == null)
        {
            return;
        }

        if (_gameState.AutoSaveExists())
        {
            if (_gameState.LoadAutoSave())
            {
                LoadingScreen.Load(ScreenManager, true, playerIndex, new WorldScreen());

                return;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (_gameState.SaveExists(i))
            {
                if (_gameState.Load(i))
                {
                    LoadingScreen.Load(ScreenManager, true, playerIndex, new WorldScreen());

                    return;
                }
            }
        }
    }

    private void OnLoadGame(PlayerIndex playerIndex)
    {
        if (_gameState != null)
        {
            var loadScreen = new SaveLoadScreen(_gameState, isSaving: false);
            ScreenManager?.AddScreen(loadScreen, playerIndex);
        }
    }

    private void OnSettings(PlayerIndex playerIndex)
    {
        ScreenManager?.AddScreen(new SettingsMenuScreen(), playerIndex);
    }

    private void OnQuit(PlayerIndex playerIndex)
    {
        var confirmExit = new MessageBoxScreen(L.Get(GameStrings.Menu_QuitConfirm));
        confirmExit.Accepted += (s, e) => ScreenManager?.Game.Exit();
        ScreenManager?.AddScreen(confirmExit, playerIndex);
    }

    #endregion

    #region Helper Classes

    private class MainMenuItem
    {
        public string Text { get; set; } = "";
        public string Icon { get; set; } = "";
        public Action<PlayerIndex>? Action { get; set; }
        public float Scale { get; set; } = 1f;
        public float Offset { get; set; } = 0f;
    }

    private class DataFragment
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public int Size { get; set; }
        public float Lifetime { get; set; }
        public Color Color { get; set; }
        public float Rotation { get; set; }
    }

    private class Star
    {
        public Vector2 Position { get; set; }
        public int Size { get; set; }
        public float Twinkle { get; set; }
        public float TwinkleSpeed { get; set; }
    }

    #endregion
}
