using System;
using Lazarus.Core;
using Lazarus.Core.Inputs;
using Lazarus.Core.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lazarus.Screens;

/// <summary>
/// A popup message box screen, used to display messages and prompt user input.
/// </summary>
class MessageBoxScreen : GameScreen
{
    private string _message;
    private Texture2D? _pixelTexture;
    private readonly bool _toastMessage;
    private readonly TimeSpan _toastDuration;
    private TimeSpan _toastTimer;
    private readonly bool _showButtons;

    // Layout calculations
    private Rectangle _panelRect;
    private Vector2 _messagePosition;
    private Vector2 _yesButtonPosition;
    private Vector2 _noButtonPosition;
    private Vector2 _yesTextSize;
    private Vector2 _noTextSize;

    // Modern UI colors matching the game's style
    private static readonly Color BackgroundColor = new(25, 30, 40, 245);
    private static readonly Color PanelColor = new(35, 42, 55);
    private static readonly Color BorderColor = new(80, 160, 255);
    private static readonly Color TextColor = new(220, 225, 235);
    private static readonly Color DimTextColor = new(140, 150, 170);
    private static readonly Color YesColor = new(80, 200, 120);
    private static readonly Color NoColor = new(255, 100, 100);

    /// <summary>
    /// Event raised when the user accepts the message box.
    /// </summary>
    public event EventHandler<PlayerIndexEventArgs>? Accepted;

    /// <summary>
    /// Event raised when the user cancels the message box.
    /// </summary>
    public event EventHandler<PlayerIndexEventArgs>? Cancelled;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBoxScreen"/> class, automatically including usage text.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public MessageBoxScreen(string message)
        : this(message, true, TimeSpan.Zero)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBoxScreen"/> class, allowing customization.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="includeUsageText">Indicates whether to include usage text (shows buttons).</param>
    /// <param name="toastDuration">The duration for toast messages.</param>
    /// <param name="toastMessage">Indicates whether this is a toast message.</param>
    public MessageBoxScreen(string message, bool includeUsageText, TimeSpan toastDuration, bool toastMessage = false)
    {
        _message = message;
        _toastMessage = toastMessage;
        _toastDuration = toastDuration;
        _toastTimer = TimeSpan.Zero;
        _showButtons = includeUsageText && !toastMessage;

        IsPopup = true;

        TransitionOnTime = TimeSpan.FromSeconds(0.2);
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
    /// Responds to user input, accepting or cancelling the message box.
    /// </summary>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        if (_toastMessage)
        {
            return;
        }

        PlayerIndex playerIndex;

        if (inputState.IsMenuSelect(ControllingPlayer, out playerIndex)
            || (LazarusGame.IsMobile
                && inputState.IsUIClicked(new Rectangle((int)_yesButtonPosition.X, (int)_yesButtonPosition.Y,
                    (int)_yesTextSize.X, (int)_yesTextSize.Y))))
        {
            Accepted?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
            ExitScreen();
        }
        else if (inputState.IsMenuCancel(ControllingPlayer, out playerIndex)
                 || (LazarusGame.IsMobile
                     && inputState.IsUIClicked(new Rectangle((int)_noButtonPosition.X, (int)_noButtonPosition.Y,
                         (int)_noTextSize.X, (int)_noTextSize.Y))))
        {
            Cancelled?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
            ExitScreen();
        }
    }

    /// <summary>
    /// Updates the screen layout.
    /// </summary>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (_toastMessage)
        {
            _toastTimer += gameTime.ElapsedGameTime;
            if (_toastTimer >= _toastDuration)
            {
                Accepted?.Invoke(this, new PlayerIndexEventArgs(PlayerIndex.One));
                ExitScreen();
            }
        }

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

        Vector2 messageSize = font.MeasureString(_message);

        // Calculate panel size
        int panelWidth = Math.Max(320, (int)messageSize.X + 60);
        int panelHeight = _showButtons ? 120 : 80;

        // Center panel on screen
        int panelX = (int)(screenSize.X - panelWidth) / 2;
        int panelY = (int)(screenSize.Y - panelHeight) / 2;
        _panelRect = new Rectangle(panelX, panelY, panelWidth, panelHeight);

        // Center message in upper portion of panel
        _messagePosition = new Vector2(
            panelX + (panelWidth - messageSize.X) / 2,
            panelY + 20);

        // Button positions for mobile
        if (LazarusGame.IsMobile && _showButtons)
        {
            _yesTextSize = font.MeasureString(L.Get(GameStrings.Yes));
            _noTextSize = font.MeasureString(L.Get(GameStrings.No));

            int buttonY = panelY + panelHeight - 35;
            int buttonSpacing = 100;
            int centerX = panelX + panelWidth / 2;

            _yesButtonPosition = new Vector2(centerX - buttonSpacing - _yesTextSize.X / 2, buttonY);
            _noButtonPosition = new Vector2(centerX + buttonSpacing - _noTextSize.X / 2, buttonY);
        }
    }

    /// <summary>
    /// Draws the message box with modern styling.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        if (_pixelTexture == null || ScreenManager == null || string.IsNullOrEmpty(_message)) return;

        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        SpriteFont font = ScreenManager.Font;

        // Darken background
        ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 0.6f);

        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, ScreenManager.GlobalTransformation);

        // Panel border (accent color)
        var borderRect = new Rectangle(
            _panelRect.X - 2,
            _panelRect.Y - 2,
            _panelRect.Width + 4,
            _panelRect.Height + 4);
        spriteBatch.Draw(_pixelTexture, borderRect, BorderColor * 0.8f * alpha);

        // Panel background
        spriteBatch.Draw(_pixelTexture, _panelRect, PanelColor * alpha);

        // Inner panel highlight (top edge)
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(_panelRect.X, _panelRect.Y, _panelRect.Width, 2),
            BorderColor * 0.3f * alpha);

        // Message text
        spriteBatch.DrawString(font, _message, _messagePosition, TextColor * alpha);

        // Draw button hints at bottom
        if (_showButtons)
        {
            string hint;
            if (LazarusGame.IsMobile)
            {
                // Draw actual buttons for mobile
                string yesText = L.Get(GameStrings.Yes);
                string noText = L.Get(GameStrings.No);
                spriteBatch.DrawString(font, yesText, _yesButtonPosition, YesColor * alpha);
                spriteBatch.DrawString(font, noText, _noButtonPosition, NoColor * alpha);
            }
            else
            {
                // Draw hint text for desktop
                hint = "[Enter] Yes    [Esc] No";
                Vector2 hintSize = font.MeasureString(hint);
                Vector2 hintPos = new Vector2(
                    _panelRect.X + (_panelRect.Width - hintSize.X) / 2,
                    _panelRect.Y + _panelRect.Height - 30);
                spriteBatch.DrawString(font, hint, hintPos, DimTextColor * 0.8f * alpha);
            }
        }

        spriteBatch.End();
    }
}