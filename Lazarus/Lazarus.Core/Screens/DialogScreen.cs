using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Dialog;
using Lazarus.Core.Game.Progression;
using Lazarus.Core.Inputs;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Screen overlay for displaying dialogs and conversations.
/// </summary>
public class DialogScreen : GameScreen
{
    private readonly Dialog _dialog;
    private DialogState _state;
    private GameStateService? _gameState;
    private QuestLog? _questLog;

    private Texture2D? _pixelTexture;
    private SpriteFont? _font;

    // Text display
    private string _displayedText = "";
    private int _targetTextLength;
    private float _textSpeed = 60f; // Characters per second
    private float _textTimer;
    private bool _textComplete;

    // Input handling
    private KeyboardState _previousKeyboardState;

    // Animation
    private float _boxSlideProgress;
    private const float BoxSlideSpeed = 4f;

    /// <summary>
    /// Event fired when the dialog ends.
    /// </summary>
    public event EventHandler? DialogEnded;

    /// <summary>
    /// Event fired when a choice is made.
    /// </summary>
    public event EventHandler<DialogChoice>? ChoiceMade;

    public DialogScreen(Dialog dialog)
    {
        _dialog = dialog;
        _state = new DialogState(dialog);

        IsPopup = true;
        TransitionOnTime = TimeSpan.FromSeconds(0.2);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        var game = ScreenManager.Game;

        // Get services
        _gameState = game.Services.GetService<GameStateService>();
        // QuestLog would be retrieved from services if registered

        // Create pixel texture
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _font = ScreenManager.Font;

        // Set up state events
        _state.LineDisplayed += OnLineDisplayed;
        _state.ChoiceSelected += OnChoiceSelected;
        _state.DialogEnded += OnDialogEnded;

        // Initialize with first line
        if (_state.CurrentLine != null)
        {
            StartTextReveal(_state.CurrentLine.GetFormattedText());
        }
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    private void OnLineDisplayed(object? sender, DialogLine line)
    {
        StartTextReveal(line.GetFormattedText());

        // Handle line effects
        if (!string.IsNullOrEmpty(line.SetsFlag))
        {
            _gameState?.SetFlag(line.SetsFlag);
        }

        if (!string.IsNullOrEmpty(line.StartsQuest))
        {
            _questLog?.StartQuest(line.StartsQuest);
        }

        if (!string.IsNullOrEmpty(line.CompletesQuest))
        {
            _questLog?.CompleteQuest(line.CompletesQuest);
        }
    }

    private void OnChoiceSelected(object? sender, DialogChoice choice)
    {
        if (!string.IsNullOrEmpty(choice.SetsFlag))
        {
            _gameState?.SetFlag(choice.SetsFlag);
        }

        ChoiceMade?.Invoke(this, choice);
    }

    private void OnDialogEnded(object? sender, EventArgs e)
    {
        // Mark dialog as seen
        if (_dialog.OneTime && !string.IsNullOrEmpty(_dialog.SeenFlag))
        {
            _gameState?.SetFlag(_dialog.SeenFlag);
        }

        DialogEnded?.Invoke(this, EventArgs.Empty);
        ExitScreen();
    }

    private void StartTextReveal(string text)
    {
        _displayedText = "";
        _targetTextLength = text.Length;
        _textTimer = 0;
        _textComplete = false;
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input == null)
            return;

        var keyboardState = Keyboard.GetState();
        var line = _state.CurrentLine;

        if (line == null)
            return;

        // Advance/confirm key
        if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
        {
            if (!_textComplete)
            {
                // Skip to full text
                _displayedText = line.GetFormattedText();
                _textComplete = true;
            }
            else if (line.HasChoices)
            {
                // Select choice
                _state.SelectChoice();
            }
            else
            {
                // Advance to next line
                _state.Advance();
            }
        }

        // Choice navigation
        if (line.HasChoices && _textComplete)
        {
            if (IsKeyPressed(keyboardState, Keys.Up) || IsKeyPressed(keyboardState, Keys.W))
            {
                _state.PreviousChoice();
            }
            if (IsKeyPressed(keyboardState, Keys.Down) || IsKeyPressed(keyboardState, Keys.S))
            {
                _state.NextChoice();
            }
        }

        // Cancel/skip (for non-choice lines)
        if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            if (!line.HasChoices)
            {
                // Skip entire dialog if possible
                while (!_state.IsComplete && _state.CurrentLine?.HasChoices != true)
                {
                    _state.Advance();
                }
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

        // Animate box slide
        if (_boxSlideProgress < 1f)
        {
            _boxSlideProgress = Math.Min(1f, _boxSlideProgress + deltaTime * BoxSlideSpeed);
        }

        // Update text reveal
        if (!_textComplete && _state.CurrentLine != null)
        {
            _textTimer += deltaTime * _textSpeed;
            int charsToShow = (int)_textTimer;

            if (charsToShow >= _targetTextLength)
            {
                _displayedText = _state.CurrentLine.GetFormattedText();
                _textComplete = true;
            }
            else
            {
                _displayedText = _state.CurrentLine.GetFormattedText().Substring(0, charsToShow);
            }
        }
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

        // Semi-transparent overlay
        var overlayRect = new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y);
        spriteBatch.Draw(_pixelTexture, overlayRect, Color.Black * 0.5f * TransitionAlpha);

        if (_state.CurrentLine != null)
        {
            DrawDialogBox(spriteBatch, screenSize);
        }

        spriteBatch.End();
    }

    private void DrawDialogBox(SpriteBatch spriteBatch, Vector2 screenSize)
    {
        if (_font == null || _pixelTexture == null)
            return;

        var line = _state.CurrentLine!;

        // Calculate box dimensions
        float boxHeight = 140;
        float boxMargin = 20;
        float boxWidth = screenSize.X - boxMargin * 2;

        // Slide animation
        float slideOffset = (1f - _boxSlideProgress) * boxHeight;

        // Box position (bottom of screen)
        var boxRect = new Rectangle(
            (int)boxMargin,
            (int)(screenSize.Y - boxHeight - boxMargin + slideOffset),
            (int)boxWidth,
            (int)boxHeight
        );

        // Draw box background
        spriteBatch.Draw(_pixelTexture, boxRect, new Color(20, 20, 30) * TransitionAlpha);

        // Draw box border
        DrawBorder(spriteBatch, boxRect, GetSpeakerColor(line.SpeakerType));

        // Draw speaker name
        string speakerName = GetSpeakerName(line);
        if (!string.IsNullOrEmpty(speakerName))
        {
            var namePos = new Vector2(boxRect.X + 15, boxRect.Y + 10);
            spriteBatch.DrawString(_font, speakerName, namePos, GetSpeakerColor(line.SpeakerType) * TransitionAlpha);
        }

        // Draw text
        float textY = boxRect.Y + (string.IsNullOrEmpty(speakerName) ? 15 : 35);
        var textPos = new Vector2(boxRect.X + 15, textY);
        var textColor = line.SpeakerType == SpeakerType.System ? Color.Gray : Color.White;

        // Word wrap the text
        string wrappedText = WrapText(_displayedText, boxWidth - 30);
        spriteBatch.DrawString(_font, wrappedText, textPos, textColor * TransitionAlpha);

        // Draw choices if applicable
        if (line.HasChoices && _textComplete)
        {
            DrawChoices(spriteBatch, boxRect);
        }

        // Draw continue prompt
        if (_textComplete && !line.HasChoices)
        {
            var promptText = "v"; // Continue prompt
            var promptPos = new Vector2(
                boxRect.Right - 25,
                boxRect.Bottom - 25
            );
            float pulse = (float)Math.Sin(DateTime.Now.Millisecond / 200.0) * 0.3f + 0.7f;
            spriteBatch.DrawString(_font, promptText, promptPos, Color.White * pulse * TransitionAlpha);
        }
    }

    private void DrawChoices(SpriteBatch spriteBatch, Rectangle boxRect)
    {
        if (_font == null || _pixelTexture == null)
            return;

        var choices = _state.GetAvailableChoices(flag => _gameState?.HasFlag(flag) ?? false);
        if (choices.Count == 0)
            return;

        // Draw choices on the right side of the box
        float choiceX = boxRect.Right - 250;
        float choiceY = boxRect.Y + 20;
        float choiceSpacing = 25;

        for (int i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];
            bool isSelected = i == _state.SelectedChoiceIndex;

            var color = isSelected ? Color.Yellow : Color.LightGray;
            var prefix = isSelected ? "> " : "  ";

            var pos = new Vector2(choiceX, choiceY + i * choiceSpacing);
            spriteBatch.DrawString(_font, prefix + choice.Text, pos, color * TransitionAlpha);
        }
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        int thickness = 2;
        var alpha = color * TransitionAlpha;

        // Top
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(rect.X, rect.Y, rect.Width, thickness), alpha);
        // Bottom
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), alpha);
        // Left
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(rect.X, rect.Y, thickness, rect.Height), alpha);
        // Right
        spriteBatch.Draw(_pixelTexture,
            new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), alpha);
    }

    private string GetSpeakerName(DialogLine line)
    {
        if (!string.IsNullOrEmpty(line.SpeakerName))
            return line.SpeakerName;

        return line.SpeakerType switch
        {
            SpeakerType.Protagonist => "You",
            SpeakerType.Companion => _gameState?.CompanionType.GetCompanionName() ?? "Companion",
            SpeakerType.System => "",
            SpeakerType.AI => "Lazarus",
            _ => line.SpeakerId ?? "???"
        };
    }

    private Color GetSpeakerColor(SpeakerType type)
    {
        return type switch
        {
            SpeakerType.Protagonist => Color.Cyan,
            SpeakerType.Companion => Color.Orange,
            SpeakerType.PartyKyn => Color.LimeGreen,
            SpeakerType.Npc => Color.White,
            SpeakerType.WildKyn => Color.Yellow,
            SpeakerType.System => Color.Gray,
            SpeakerType.AI => Color.Magenta,
            _ => Color.White
        };
    }

    private string WrapText(string text, float maxWidth)
    {
        if (_font == null || string.IsNullOrEmpty(text))
            return text;

        var words = text.Split(' ');
        var lines = new List<string>();
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

    /// <summary>
    /// Creates and shows a dialog screen.
    /// </summary>
    public static DialogScreen? Show(ScreenManager screenManager, string dialogId, PlayerIndex? controllingPlayer = null)
    {
        var dialog = Dialogs.Get(dialogId);
        if (dialog == null)
            return null;

        var screen = new DialogScreen(dialog);
        screenManager.AddScreen(screen, controllingPlayer);
        return screen;
    }
}
