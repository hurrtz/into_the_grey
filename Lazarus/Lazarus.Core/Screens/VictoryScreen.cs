using System;
using Lazarus.Core;
using Lazarus.Core.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lazarus.Screens;

/// <summary>
/// A screen displayed after winning a battle, showing rewards and requiring
/// manual dismissal by the player. Unlike toast messages, this screen stays
/// until the player presses a button.
/// </summary>
public class VictoryScreen : GameScreen
{
    private readonly int _experienceEarned;
    private readonly int _currencyEarned;
    private readonly int _telemetryUnitsEarned;
    private readonly string? _entityName;

    private Texture2D? _pixelTexture;

    // Layout calculations
    private Rectangle _panelRect;
    private Vector2 _titlePosition;
    private Vector2 _expPosition;
    private Vector2 _currencyPosition;
    private Vector2 _tuPosition;
    private Vector2 _hintPosition;

    // Modern UI colors matching the game's style
    private static readonly Color BackgroundColor = new(25, 30, 40, 245);
    private static readonly Color PanelColor = new(35, 42, 55);
    private static readonly Color BorderColor = new(255, 215, 0); // Gold for victory
    private static readonly Color TitleColor = new(255, 215, 0);
    private static readonly Color TextColor = new(220, 225, 235);
    private static readonly Color ValueColor = new(80, 200, 120); // Green for gains
    private static readonly Color DimTextColor = new(140, 150, 170);

    /// <summary>
    /// Event fired when the victory screen is dismissed.
    /// </summary>
    public event EventHandler? Dismissed;

    /// <summary>
    /// Creates a new victory screen showing battle rewards.
    /// </summary>
    /// <param name="experience">Experience points earned.</param>
    /// <param name="currency">Currency (scrap) earned.</param>
    /// <param name="telemetryUnits">TU earned for microchips.</param>
    /// <param name="entityName">Optional: Name of defeated entity for display.</param>
    public VictoryScreen(int experience, int currency, int telemetryUnits, string? entityName = null)
    {
        _experienceEarned = experience;
        _currencyEarned = currency;
        _telemetryUnitsEarned = telemetryUnits;
        _entityName = entityName;

        IsPopup = true;

        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    /// <summary>
    /// Loads graphics content for this screen.
    /// </summary>
    public override void LoadContent()
    {
        base.LoadContent();

        // Create a 1x1 white pixel texture for drawing rectangles
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    /// <summary>
    /// Unloads graphics content for this screen.
    /// </summary>
    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
        _pixelTexture = null;
    }

    /// <summary>
    /// Responds to user input - any action dismisses the screen.
    /// </summary>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        PlayerIndex playerIndex;

        // Any of these actions dismisses the screen
        if (inputState.IsMenuSelect(ControllingPlayer, out playerIndex) ||
            inputState.IsMenuCancel(ControllingPlayer, out playerIndex) ||
            inputState.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Space, ControllingPlayer, out playerIndex) ||
            inputState.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Enter, ControllingPlayer, out playerIndex) ||
            inputState.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Escape, ControllingPlayer, out playerIndex))
        {
            Dismiss();
        }

        // Mobile: tap anywhere to dismiss
        if (LazarusGame.IsMobile && inputState.Gestures.Count > 0)
        {
            Dismiss();
        }
    }

    /// <summary>
    /// Dismisses the victory screen.
    /// </summary>
    private void Dismiss()
    {
        Dismissed?.Invoke(this, EventArgs.Empty);
        ExitScreen();
    }

    /// <summary>
    /// Updates the screen layout.
    /// </summary>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (ScreenManager?.Font == null)
        {
            return;
        }

        CalculateLayout();
    }

    private void CalculateLayout()
    {
        var font = ScreenManager!.Font;
        var screenSize = ScreenManager.BaseScreenSize;

        // Panel dimensions
        int panelWidth = 300;
        int panelHeight = 200;

        // Center panel on screen
        int panelX = (int)(screenSize.X - panelWidth) / 2;
        int panelY = (int)(screenSize.Y - panelHeight) / 2;
        _panelRect = new Rectangle(panelX, panelY, panelWidth, panelHeight);

        // Title position (centered)
        string title = "VICTORY!";
        Vector2 titleSize = font.MeasureString(title);
        _titlePosition = new Vector2(
            panelX + (panelWidth - titleSize.X) / 2,
            panelY + 20);

        // Reward lines (left-aligned within panel)
        int leftMargin = panelX + 30;
        int lineHeight = 28;
        int startY = panelY + 60;

        _expPosition = new Vector2(leftMargin, startY);
        _currencyPosition = new Vector2(leftMargin, startY + lineHeight);
        _tuPosition = new Vector2(leftMargin, startY + lineHeight * 2);

        // Hint position (centered at bottom)
        string hint = LazarusGame.IsMobile ? "Tap to continue" : "[Press any key to continue]";
        Vector2 hintSize = font.MeasureString(hint);
        _hintPosition = new Vector2(
            panelX + (panelWidth - hintSize.X) / 2,
            panelY + panelHeight - 35);
    }

    /// <summary>
    /// Draws the victory screen with rewards summary.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        if (_pixelTexture == null || ScreenManager == null) return;

        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        SpriteFont font = ScreenManager.Font;

        // Darken background
        ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 0.7f);

        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, ScreenManager.GlobalTransformation);

        // Panel border (gold accent)
        var borderRect = new Rectangle(
            _panelRect.X - 3,
            _panelRect.Y - 3,
            _panelRect.Width + 6,
            _panelRect.Height + 6);
        spriteBatch.Draw(_pixelTexture, borderRect, BorderColor * 0.9f * alpha);

        // Panel background
        spriteBatch.Draw(_pixelTexture, _panelRect, PanelColor * alpha);

        // Inner panel highlight (top edge)
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(_panelRect.X, _panelRect.Y, _panelRect.Width, 3),
            BorderColor * 0.5f * alpha);

        // Decorative lines at top and bottom
        DrawDecorativeLine(spriteBatch, _panelRect.X + 20, (int)_titlePosition.Y - 5, _panelRect.Width - 40, alpha);
        DrawDecorativeLine(spriteBatch, _panelRect.X + 20, (int)_titlePosition.Y + 25, _panelRect.Width - 40, alpha);

        // Title
        spriteBatch.DrawString(font, "VICTORY!", _titlePosition, TitleColor * alpha);

        // Entity name if provided
        if (!string.IsNullOrEmpty(_entityName))
        {
            string defeatText = $"Defeated {_entityName}";
            Vector2 defeatSize = font.MeasureString(defeatText);
            Vector2 defeatPos = new Vector2(
                _panelRect.X + (_panelRect.Width - defeatSize.X) / 2,
                _titlePosition.Y + 30);
            spriteBatch.DrawString(font, defeatText, defeatPos, DimTextColor * alpha);
        }

        // Rewards
        DrawRewardLine(spriteBatch, font, "XP Gained:", $"+{_experienceEarned}", _expPosition, alpha);
        DrawRewardLine(spriteBatch, font, "Currency:", $"+{_currencyEarned}", _currencyPosition, alpha);
        DrawRewardLine(spriteBatch, font, "TU Gained:", $"+{_telemetryUnitsEarned}", _tuPosition, alpha);

        // Hint to continue
        string hint = LazarusGame.IsMobile ? "Tap to continue" : "[Press any key to continue]";
        spriteBatch.DrawString(font, hint, _hintPosition, DimTextColor * 0.8f * alpha);

        spriteBatch.End();
    }

    private void DrawRewardLine(SpriteBatch spriteBatch, SpriteFont font, string label, string value, Vector2 position, float alpha)
    {
        // Draw label
        spriteBatch.DrawString(font, label, position, TextColor * alpha);

        // Draw value (right-aligned within reward area)
        Vector2 valueSize = font.MeasureString(value);
        Vector2 valuePos = new Vector2(
            _panelRect.Right - 30 - valueSize.X,
            position.Y);
        spriteBatch.DrawString(font, value, valuePos, ValueColor * alpha);
    }

    private void DrawDecorativeLine(SpriteBatch spriteBatch, int x, int y, int width, float alpha)
    {
        if (_pixelTexture == null) return;

        // Center line
        spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, width, 1), BorderColor * 0.4f * alpha);

        // Fade out ends
        int fadeWidth = 20;
        for (int i = 0; i < fadeWidth; i++)
        {
            float fadeAlpha = (float)i / fadeWidth * 0.4f * alpha;
            spriteBatch.Draw(_pixelTexture, new Rectangle(x + i, y, 1, 1), BorderColor * fadeAlpha);
            spriteBatch.Draw(_pixelTexture, new Rectangle(x + width - i - 1, y, 1, 1), BorderColor * fadeAlpha);
        }
    }
}
