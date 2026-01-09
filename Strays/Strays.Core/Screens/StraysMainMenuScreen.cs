using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Localization;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Main menu screen for Into The Grey (Strays).
/// </summary>
class StraysMainMenuScreen : MenuScreen
{
    private GameStateService? _gameState;
    private Texture2D? _pixelTexture;
    private bool _hasContinueData;

    private MenuEntry _newGameEntry = null!;
    private MenuEntry _continueEntry = null!;
    private MenuEntry _loadGameEntry = null!;
    private MenuEntry _settingsEntry = null!;
    private MenuEntry _exitEntry = null!;

    public StraysMainMenuScreen()
        : base("Into The Grey")
    {
    }

    public override void LoadContent()
    {
        base.LoadContent();

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

        // Create pixel texture
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Create menu entries
        _newGameEntry = new MenuEntry("New Game");
        _continueEntry = new MenuEntry("Continue");
        _loadGameEntry = new MenuEntry("Load Game");
        _settingsEntry = new MenuEntry(Resources.Settings);
        _exitEntry = new MenuEntry(Resources.Exit);

        // Hook up event handlers
        _newGameEntry.Selected += NewGameSelected;
        _continueEntry.Selected += ContinueSelected;
        _loadGameEntry.Selected += LoadGameSelected;
        _settingsEntry.Selected += SettingsSelected;
        _exitEntry.Selected += OnCancel;

        // Add entries to menu
        MenuEntries.Add(_newGameEntry);

        if (_hasContinueData)
        {
            MenuEntries.Add(_continueEntry);
            MenuEntries.Add(_loadGameEntry);
        }

        MenuEntries.Add(_settingsEntry);
        MenuEntries.Add(_exitEntry);
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    private void NewGameSelected(object? sender, PlayerIndexEventArgs e)
    {
        if (_hasContinueData)
        {
            // Warn about overwriting
            var confirmBox = new MessageBoxScreen("Start a new game? You can still load previous saves.");
            confirmBox.Accepted += (s, args) =>
            {
                ScreenManager.AddScreen(new CompanionSelectScreen(), e.PlayerIndex);
            };
            ScreenManager.AddScreen(confirmBox, e.PlayerIndex);
        }
        else
        {
            ScreenManager.AddScreen(new CompanionSelectScreen(), e.PlayerIndex);
        }
    }

    private void ContinueSelected(object? sender, PlayerIndexEventArgs e)
    {
        // Try to load auto-save first, then most recent manual save
        if (_gameState!.AutoSaveExists())
        {
            if (_gameState.LoadAutoSave())
            {
                LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new WorldScreen());
                return;
            }
        }

        // Try manual saves (most recent first would require timestamp check)
        for (int i = 0; i < 3; i++)
        {
            if (_gameState.SaveExists(i))
            {
                if (_gameState.Load(i))
                {
                    LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new WorldScreen());
                    return;
                }
            }
        }
    }

    private void LoadGameSelected(object? sender, PlayerIndexEventArgs e)
    {
        var loadScreen = new SaveLoadScreen(_gameState!, isSaving: false);
        ScreenManager.AddScreen(loadScreen, e.PlayerIndex);
    }

    private void SettingsSelected(object? sender, PlayerIndexEventArgs e)
    {
        ScreenManager.AddScreen(new SettingsScreen(), e.PlayerIndex);
    }

    protected override void OnCancel(PlayerIndex playerIndex)
    {
        var confirmExit = new MessageBoxScreen(Resources.ExitQuestion);
        confirmExit.Accepted += (s, e) => ScreenManager.Game.Exit();
        ScreenManager.AddScreen(confirmExit, playerIndex);
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var screenWidth = ScreenManager.BaseScreenSize.X;
        var screenHeight = ScreenManager.BaseScreenSize.Y;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        // Draw background
        DrawBackground(spriteBatch, screenWidth, screenHeight);

        spriteBatch.End();

        // Draw menu entries (handled by base class)
        base.Draw(gameTime);

        // Draw version and subtitle
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        var font = ScreenManager.Font;

        // Subtitle
        string subtitle = "A journey through the wasteland";
        var subSize = font.MeasureString(subtitle);
        spriteBatch.DrawString(font, subtitle,
            new Vector2((screenWidth - subSize.X) / 2, 80),
            Color.Gray * TransitionAlpha);

        // Version
        string version = "v0.1 - Development Build";
        spriteBatch.DrawString(font, version,
            new Vector2(10, screenHeight - 25),
            Color.DimGray * TransitionAlpha);

        spriteBatch.End();
    }

    private void DrawBackground(SpriteBatch spriteBatch, float screenWidth, float screenHeight)
    {
        // Dark gradient background
        spriteBatch.Draw(_pixelTexture!, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight),
            new Color(15, 15, 25) * TransitionAlpha);

        // Decorative elements - distant wasteland silhouette
        int groundY = (int)(screenHeight * 0.7f);

        // Ground
        spriteBatch.Draw(_pixelTexture!,
            new Rectangle(0, groundY, (int)screenWidth, (int)(screenHeight - groundY)),
            new Color(25, 25, 35) * TransitionAlpha);

        // Some ruined buildings silhouettes
        DrawBuilding(spriteBatch, 50, groundY, 40, 80, new Color(20, 20, 30));
        DrawBuilding(spriteBatch, 120, groundY, 60, 120, new Color(18, 18, 28));
        DrawBuilding(spriteBatch, 200, groundY, 35, 60, new Color(22, 22, 32));

        DrawBuilding(spriteBatch, (int)screenWidth - 150, groundY, 50, 100, new Color(20, 20, 30));
        DrawBuilding(spriteBatch, (int)screenWidth - 80, groundY, 45, 70, new Color(18, 18, 28));

        // Stars
        var random = new Random(42); // Fixed seed for consistent stars
        for (int i = 0; i < 50; i++)
        {
            int x = random.Next((int)screenWidth);
            int y = random.Next(groundY - 50);
            int size = random.Next(1, 3);
            float brightness = (float)random.NextDouble() * 0.5f + 0.3f;
            spriteBatch.Draw(_pixelTexture!, new Rectangle(x, y, size, size),
                Color.White * brightness * TransitionAlpha);
        }

        // Distant glow (The Glow biome hint)
        for (int i = 0; i < 3; i++)
        {
            int glowX = (int)(screenWidth * 0.7f) + i * 5;
            int glowY = groundY - 30 - i * 10;
            spriteBatch.Draw(_pixelTexture!, new Rectangle(glowX, glowY, 3, 3),
                Color.Cyan * (0.3f - i * 0.1f) * TransitionAlpha);
        }
    }

    private void DrawBuilding(SpriteBatch spriteBatch, int x, int groundY, int width, int height, Color color)
    {
        spriteBatch.Draw(_pixelTexture!, new Rectangle(x, groundY - height, width, height), color * TransitionAlpha);

        // Some broken top edges
        var random = new Random(x);
        for (int i = 0; i < width; i += 8)
        {
            int dip = random.Next(5, 15);
            spriteBatch.Draw(_pixelTexture!, new Rectangle(x + i, groundY - height - dip, 6, dip), color * TransitionAlpha);
        }
    }
}
