using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Entities;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Screen for attempting to recruit a Stray after combat.
/// </summary>
public class RecruitmentScreen : GameScreen
{
    private readonly Stray _stray;
    private readonly RecruitmentManager _recruitmentManager;

    private Texture2D? _pixelTexture;
    private SpriteFont? _font;

    private int _selectedOption;
    private bool _attemptComplete;
    private RecruitmentResult _result;
    private string _resultMessage = "";

    // Animation
    private float _strayBobTimer;
    private float _fadeInProgress;

    // Input
    private KeyboardState _previousKeyboardState;

    /// <summary>
    /// Event fired when recruitment is complete.
    /// </summary>
    public event EventHandler<RecruitmentResult>? RecruitmentComplete;

    public RecruitmentScreen(Stray stray, RecruitmentManager recruitmentManager)
    {
        _stray = stray;
        _recruitmentManager = recruitmentManager;

        IsPopup = true;
        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.3);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _font = ScreenManager.Font;
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input == null)
            return;

        var keyboardState = Keyboard.GetState();

        if (_attemptComplete)
        {
            // Any key to close
            if (IsKeyPressed(keyboardState, Keys.Enter) ||
                IsKeyPressed(keyboardState, Keys.Space) ||
                IsKeyPressed(keyboardState, Keys.Escape))
            {
                RecruitmentComplete?.Invoke(this, _result);
                ExitScreen();
            }
        }
        else
        {
            // Navigate options
            if (IsKeyPressed(keyboardState, Keys.Up) || IsKeyPressed(keyboardState, Keys.W))
            {
                _selectedOption = _selectedOption == 0 ? 1 : 0;
            }
            if (IsKeyPressed(keyboardState, Keys.Down) || IsKeyPressed(keyboardState, Keys.S))
            {
                _selectedOption = _selectedOption == 1 ? 0 : 1;
            }

            // Select option
            if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
            {
                if (_selectedOption == 0)
                {
                    // Attempt recruitment
                    _result = _recruitmentManager.AttemptRecruitment(_stray, out _resultMessage);
                    _attemptComplete = true;
                }
                else
                {
                    // Skip
                    _result = RecruitmentResult.Refused;
                    _resultMessage = "You decided not to recruit the Stray.";
                    RecruitmentComplete?.Invoke(this, _result);
                    ExitScreen();
                }
            }

            // Cancel
            if (IsKeyPressed(keyboardState, Keys.Escape))
            {
                _result = RecruitmentResult.Refused;
                RecruitmentComplete?.Invoke(this, _result);
                ExitScreen();
            }
        }

        _previousKeyboardState = keyboardState;
    }

    private bool IsKeyPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _strayBobTimer += deltaTime;
        _fadeInProgress = Math.Min(1f, _fadeInProgress + deltaTime * 3f);
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var screenSize = ScreenManager.BaseScreenSize;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        // Background overlay
        var overlayRect = new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y);
        spriteBatch.Draw(_pixelTexture, overlayRect, Color.Black * 0.7f * TransitionAlpha);

        // Draw panel
        var panelWidth = 450;
        var panelHeight = _attemptComplete ? 200 : 280;
        var panelX = (int)(screenSize.X / 2 - panelWidth / 2);
        var panelY = (int)(screenSize.Y / 2 - panelHeight / 2);
        var panelRect = new Rectangle(panelX, panelY, panelWidth, panelHeight);

        // Panel background
        spriteBatch.Draw(_pixelTexture, panelRect, new Color(30, 30, 45) * TransitionAlpha);

        // Panel border
        DrawBorder(spriteBatch, panelRect, Color.Cyan * TransitionAlpha);

        if (_font != null && _pixelTexture != null)
        {
            if (_attemptComplete)
            {
                DrawResult(spriteBatch, panelRect);
            }
            else
            {
                DrawRecruitmentPrompt(spriteBatch, panelRect);
            }
        }

        spriteBatch.End();
    }

    private void DrawRecruitmentPrompt(SpriteBatch spriteBatch, Rectangle panelRect)
    {
        if (_font == null || _pixelTexture == null)
            return;

        float centerX = panelRect.X + panelRect.Width / 2;

        // Title
        var title = "Recruitment Opportunity!";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2(centerX - titleSize.X / 2, panelRect.Y + 15);
        spriteBatch.DrawString(_font, title, titlePos, Color.Yellow * TransitionAlpha);

        // Draw Stray representation
        float strayY = panelRect.Y + 70;
        float bob = (float)Math.Sin(_strayBobTimer * 3) * 3;
        var strayRect = new Rectangle(
            (int)(centerX - 30),
            (int)(strayY + bob),
            60,
            60
        );
        spriteBatch.Draw(_pixelTexture, strayRect, _stray.Definition.PlaceholderColor * TransitionAlpha);

        // Stray name
        var strayName = _stray.DisplayName;
        var nameSize = _font.MeasureString(strayName);
        var namePos = new Vector2(centerX - nameSize.X / 2, strayY + 70);
        spriteBatch.DrawString(_font, strayName, namePos, Color.White * TransitionAlpha);

        // Level
        var levelText = $"Level {_stray.Level}";
        var levelSize = _font.MeasureString(levelText);
        var levelPos = new Vector2(centerX - levelSize.X / 2, strayY + 90);
        spriteBatch.DrawString(_font, levelText, levelPos, Color.Gray * TransitionAlpha);

        // Prompt
        var prompt = "Attempt to recruit this Stray?";
        var promptSize = _font.MeasureString(prompt);
        var promptPos = new Vector2(centerX - promptSize.X / 2, panelRect.Y + 180);
        spriteBatch.DrawString(_font, prompt, promptPos, Color.LightGray * TransitionAlpha);

        // Options
        string[] options = { "Yes, recruit!", "No, leave it" };
        float optionY = panelRect.Y + 215;

        for (int i = 0; i < options.Length; i++)
        {
            bool isSelected = i == _selectedOption;
            var prefix = isSelected ? "> " : "  ";
            var color = isSelected ? Color.Yellow : Color.White;

            var optionText = prefix + options[i];
            var optionSize = _font.MeasureString(optionText);
            var optionPos = new Vector2(centerX - optionSize.X / 2, optionY + i * 25);

            spriteBatch.DrawString(_font, optionText, optionPos, color * TransitionAlpha);
        }
    }

    private void DrawResult(SpriteBatch spriteBatch, Rectangle panelRect)
    {
        if (_font == null || _pixelTexture == null)
            return;

        float centerX = panelRect.X + panelRect.Width / 2;

        // Title based on result
        var (title, titleColor) = _result switch
        {
            RecruitmentResult.Success => ("Success!", Color.LimeGreen),
            RecruitmentResult.Refused => ("Refused...", Color.Orange),
            RecruitmentResult.ConditionsNotMet => ("Failed", Color.Red),
            RecruitmentResult.PartyFull => ("No Room!", Color.Red),
            _ => ("Result", Color.White)
        };

        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2(centerX - titleSize.X / 2, panelRect.Y + 30);
        spriteBatch.DrawString(_font, title, titlePos, titleColor * TransitionAlpha);

        // Draw Stray (celebratory bob on success)
        float strayY = panelRect.Y + 70;
        float bob = _result == RecruitmentResult.Success
            ? (float)Math.Sin(_strayBobTimer * 6) * 5
            : 0;
        var strayRect = new Rectangle(
            (int)(centerX - 25),
            (int)(strayY + bob),
            50,
            50
        );
        spriteBatch.Draw(_pixelTexture, strayRect, _stray.Definition.PlaceholderColor * TransitionAlpha);

        // Result message
        var messageLines = WrapText(_resultMessage, panelRect.Width - 40);
        var messageY = panelRect.Y + 130;
        foreach (var line in messageLines.Split('\n'))
        {
            var lineSize = _font.MeasureString(line);
            var linePos = new Vector2(centerX - lineSize.X / 2, messageY);
            spriteBatch.DrawString(_font, line, linePos, Color.White * TransitionAlpha);
            messageY += 20;
        }

        // Continue prompt
        var continueText = "Press Enter to continue";
        var continueSize = _font.MeasureString(continueText);
        var pulse = (float)Math.Sin(DateTime.Now.Millisecond / 200.0) * 0.3f + 0.7f;
        var continuePos = new Vector2(centerX - continueSize.X / 2, panelRect.Bottom - 30);
        spriteBatch.DrawString(_font, continueText, continuePos, Color.Gray * pulse * TransitionAlpha);
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        int thickness = 2;

        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    private string WrapText(string text, float maxWidth)
    {
        if (_font == null || string.IsNullOrEmpty(text))
            return text;

        var words = text.Split(' ');
        var lines = new System.Collections.Generic.List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var size = _font.MeasureString(testLine);

            if (size.X > maxWidth && !string.IsNullOrEmpty(currentLine))
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
}
