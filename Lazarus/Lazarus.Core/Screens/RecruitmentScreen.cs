using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Inputs;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Screen for attempting to recruit a Kyn after combat.
/// </summary>
public class RecruitmentScreen : GameScreen
{
    private enum RecruitmentState
    {
        CheckingConditions,
        Introduction,
        Negotiation,
        Result
    }

    private readonly Kyn _kyn;
    private readonly RecruitmentManager _recruitmentManager;

    private Texture2D? _pixelTexture;
    private SpriteFont? _font;

    private RecruitmentState _state;
    private int _selectedOption;
    private RecruitmentResult _result;
    private string _resultMessage = "";
    private bool _conditionsMet;
    private string _conditionFailureMessage = "";

    // Animation
    private float _kynBobTimer;
    private float _fadeInProgress;

    // Input
    private KeyboardState _previousKeyboardState;

    /// <summary>
    /// Event fired when recruitment is complete.
    /// </summary>
    public event EventHandler<RecruitmentResult>? RecruitmentComplete;

    public RecruitmentScreen(Kyn kyn, RecruitmentManager recruitmentManager)
    {
        _kyn = kyn;
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

        // Initial state
        _state = RecruitmentState.CheckingConditions;
        _conditionsMet = _recruitmentManager.CanAttemptRecruitment(_kyn, out _conditionFailureMessage);
        _state = RecruitmentState.Introduction;
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

        switch (_state)
        {
            case RecruitmentState.Introduction:
                if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
                {
                    if (_conditionsMet)
                        _state = RecruitmentState.Negotiation;
                    else
                    {
                        _result = RecruitmentResult.ConditionsNotMet;
                        _resultMessage = _conditionFailureMessage;
                        _state = RecruitmentState.Result;
                    }
                }
                break;
            case RecruitmentState.Negotiation:
                HandleNegotiationInput(keyboardState);
                break;
            case RecruitmentState.Result:
                if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space) || IsKeyPressed(keyboardState, Keys.Escape))
                {
                    RecruitmentComplete?.Invoke(this, _result);
                    ExitScreen();
                }
                break;
        }

        _previousKeyboardState = keyboardState;
    }

    private void HandleNegotiationInput(KeyboardState keyboardState)
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
            if (_selectedOption == 0) // Yes, recruit
            {
                _result = _recruitmentManager.AttemptRecruitment(_kyn, out _resultMessage);
            }
            else // No, leave it
            {
                _result = RecruitmentResult.Refused;
                _resultMessage = "You decided not to recruit the Kyn.";
            }
            _state = RecruitmentState.Result;
        }

        // Cancel
        if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            _result = RecruitmentResult.Refused;
            RecruitmentComplete?.Invoke(this, _result);
            ExitScreen();
        }
    }

    private bool IsKeyPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _kynBobTimer += deltaTime;
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

        var panelWidth = 450;
        var panelHeight = 280;
        var panelX = (int)(screenSize.X / 2 - panelWidth / 2);
        var panelY = (int)(screenSize.Y / 2 - panelHeight / 2);
        var panelRect = new Rectangle(panelX, panelY, panelWidth, panelHeight);

        // Panel background
        spriteBatch.Draw(_pixelTexture, panelRect, new Color(30, 30, 45) * TransitionAlpha);

        // Panel border
        DrawBorder(spriteBatch, panelRect, Color.Cyan * TransitionAlpha);

        if (_font != null && _pixelTexture != null)
        {
            switch (_state)
            {
                case RecruitmentState.Introduction:
                    DrawIntroduction(spriteBatch, panelRect);
                    break;
                case RecruitmentState.Negotiation:
                    DrawNegotiation(spriteBatch, panelRect);
                    break;
                case RecruitmentState.Result:
                    DrawResult(spriteBatch, panelRect);
                    break;
            }
        }

        spriteBatch.End();
    }

    private void DrawIntroduction(SpriteBatch spriteBatch, Rectangle panelRect)
    {
        if (_font == null || _pixelTexture == null) return;

        float centerX = panelRect.X + panelRect.Width / 2;

        var title = "A Kyn Approaches...";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2(centerX - titleSize.X / 2, panelRect.Y + 15);
        spriteBatch.DrawString(_font, title, titlePos, Color.Yellow * TransitionAlpha);

        float kynY = panelRect.Y + 70;
        float bob = (float)Math.Sin(_kynBobTimer * 3) * 3;
        var kynRect = new Rectangle((int)(centerX - 30), (int)(kynY + bob), 60, 60);
        spriteBatch.Draw(_pixelTexture, kynRect, _kyn.Definition.PlaceholderColor * TransitionAlpha);

        var kynName = _kyn.DisplayName;
        var nameSize = _font.MeasureString(kynName);
        var namePos = new Vector2(centerX - nameSize.X / 2, kynY + 70);
        spriteBatch.DrawString(_font, kynName, namePos, Color.White * TransitionAlpha);

        var levelText = $"Level {_kyn.Level}";
        var levelSize = _font.MeasureString(levelText);
        var levelPos = new Vector2(centerX - levelSize.X / 2, kynY + 90);
        spriteBatch.DrawString(_font, levelText, levelPos, Color.Gray * TransitionAlpha);
        
        var dialogue = _kyn.Definition.RecruitmentDialogue.TryGetValue("introduction", out var introDialogue)
            ? introDialogue
            : new System.Collections.Generic.List<string> { "It watches you silently." };
        var prompt = string.Join("\n", dialogue);
        var promptSize = _font.MeasureString(prompt);
        var promptPos = new Vector2(centerX - promptSize.X / 2, panelRect.Y + 180);
        spriteBatch.DrawString(_font, prompt, promptPos, Color.LightGray * TransitionAlpha);

        var continueText = "Press Enter to continue";
        var continueSize = _font.MeasureString(continueText);
        var pulse = (float)Math.Sin(DateTime.Now.Millisecond / 200.0) * 0.3f + 0.7f;
        var continuePos = new Vector2(centerX - continueSize.X / 2, panelRect.Bottom - 30);
        spriteBatch.DrawString(_font, continueText, continuePos, Color.Gray * pulse * TransitionAlpha);
    }

    private void DrawNegotiation(SpriteBatch spriteBatch, Rectangle panelRect)
    {
        if (_font == null) return;

        float centerX = panelRect.X + panelRect.Width / 2;

        var title = "Negotiation";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2(centerX - titleSize.X / 2, panelRect.Y + 15);
        spriteBatch.DrawString(_font, title, titlePos, Color.Yellow * TransitionAlpha);

        var condition = _kyn.Definition.RecruitCondition;
        var prompt = condition?.FailureMessage ?? "Will you accept this Kyn?";
        
        var wrappedPrompt = WrapText(prompt, panelRect.Width - 40);
        var promptSize = _font.MeasureString(wrappedPrompt);
        var promptPos = new Vector2(centerX - promptSize.X / 2, panelRect.Y + 80);
        spriteBatch.DrawString(_font, wrappedPrompt, promptPos, Color.LightGray * TransitionAlpha);

        string[] options = { "Yes, join us.", "No, go your own way." };
        float optionY = panelRect.Y + 180;

        for (int i = 0; i < options.Length; i++)
        {
            bool isSelected = i == _selectedOption;
            var prefix = isSelected ? "> " : "  ";
            var color = isSelected ? Color.Yellow : Color.White;

            var optionText = prefix + options[i];
            var optionSize = _font.MeasureString(optionText);
            var optionPos = new Vector2(centerX - optionSize.X / 2, optionY + i * 30);

            spriteBatch.DrawString(_font, optionText, optionPos, color * TransitionAlpha);
        }
    }

    private void DrawResult(SpriteBatch spriteBatch, Rectangle panelRect)
    {
        if (_font == null || _pixelTexture == null)
            return;

        float centerX = panelRect.X + panelRect.Width / 2;

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

        float kynY = panelRect.Y + 70;
        float bob = _result == RecruitmentResult.Success ? (float)Math.Sin(_kynBobTimer * 6) * 5 : 0;
        var kynRect = new Rectangle((int)(centerX - 25), (int)(kynY + bob), 50, 50);
        spriteBatch.Draw(_pixelTexture, kynRect, _kyn.Definition.PlaceholderColor * TransitionAlpha);

        var dialogueKey = _result == RecruitmentResult.Success ? "success" : "failure";
        var dialogue = _kyn.Definition.RecruitmentDialogue.TryGetValue(dialogueKey, out var resultDialogue)
            ? resultDialogue
            : new System.Collections.Generic.List<string> { _resultMessage };
        _resultMessage = string.Join("\n", dialogue);

        var messageLines = WrapText(_resultMessage, panelRect.Width - 40);
        var messageY = panelRect.Y + 130;
        foreach (var line in messageLines.Split('\n'))
        {
            var lineSize = _font.MeasureString(line);
            var linePos = new Vector2(centerX - lineSize.X / 2, messageY);
            spriteBatch.DrawString(_font, line, linePos, Color.White * TransitionAlpha);
            messageY += 20;
        }

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
