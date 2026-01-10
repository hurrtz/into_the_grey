using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Story;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Screen for displaying game endings, epilogue scenes, and credits.
/// </summary>
public class EndingScreen : GameScreen
{
    private readonly EndingDefinition _ending;
    private readonly EndingSystem _endingSystem;

    private Texture2D? _pixelTexture;
    private SpriteFont? _font;

    // Display state
    private EndingPhase _phase = EndingPhase.FadeIn;
    private int _currentSceneIndex;
    private int _currentLineIndex;
    private float _timer;
    private float _fadeLevel;

    // Text animation
    private string _displayedText = "";
    private string _targetText = "";
    private float _textTimer;
    private float _textSpeed = 40f;
    private bool _textComplete;

    // Input handling
    private KeyboardState _previousKeyboardState;

    // Credits scroll
    private float _creditsScrollY;
    private List<CreditLine> _creditLines = new();

    /// <summary>
    /// Event fired when the ending is complete and should return to main menu.
    /// </summary>
    public event EventHandler? EndingComplete;

    private enum EndingPhase
    {
        FadeIn,
        Title,
        Summary,
        Epilogue,
        Credits,
        FadeOut
    }

    private struct CreditLine
    {
        public string Text;
        public Color Color;
        public float Scale;
        public bool IsHeader;
    }

    public EndingScreen(EndingDefinition ending, EndingSystem endingSystem)
    {
        _ending = ending;
        _endingSystem = endingSystem;

        IsPopup = false;
        TransitionOnTime = TimeSpan.FromSeconds(1.0);
        TransitionOffTime = TimeSpan.FromSeconds(1.0);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Create pixel texture
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _font = ScreenManager.Font;

        // Build credits
        BuildCredits();

        // Start with fade in
        _fadeLevel = 1f;
        _phase = EndingPhase.FadeIn;
        _timer = 0f;
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    private void BuildCredits()
    {
        _creditLines = new List<CreditLine>
        {
            new() { Text = "INTO THE GREY", Color = Color.Gray, Scale = 2f, IsHeader = true },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = _ending.Title, Color = GetEndingColor(), Scale = 1.5f, IsHeader = true },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = _ending.Summary, Color = Color.White, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "DEVELOPMENT", Color = Color.Gold, Scale = 1.2f, IsHeader = true },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "Game Design & Programming", Color = Color.Gray, Scale = 0.9f },
            new() { Text = "You", Color = Color.White, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "Story & Writing", Color = Color.Gray, Scale = 0.9f },
            new() { Text = "You", Color = Color.White, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "Art Direction", Color = Color.Gray, Scale = 0.9f },
            new() { Text = "Placeholder Art Team", Color = Color.White, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "SPECIAL THANKS", Color = Color.Gold, Scale = 1.2f, IsHeader = true },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "The MonoGame Community", Color = Color.White, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "All the Strays who journeyed with us", Color = Color.LimeGreen, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
            new() { Text = "", Color = Color.White, Scale = 1f },
        };

        // Add New Game+ unlocks if any
        if (_ending.NewGamePlusUnlocks.Count > 0)
        {
            _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });
            _creditLines.Add(new CreditLine { Text = "NEW GAME+ UNLOCKED", Color = Color.Gold, Scale = 1.2f, IsHeader = true });
            _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });

            foreach (var unlock in _ending.NewGamePlusUnlocks)
            {
                string unlockName = unlock.Replace("_", " ");
                unlockName = char.ToUpper(unlockName[0]) + unlockName.Substring(1);
                _creditLines.Add(new CreditLine { Text = unlockName, Color = Color.Cyan, Scale = 1f });
            }
        }

        // Achievement
        if (!string.IsNullOrEmpty(_ending.AchievementId))
        {
            _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });
            _creditLines.Add(new CreditLine { Text = "ACHIEVEMENT UNLOCKED", Color = Color.Gold, Scale = 1.2f, IsHeader = true });
            _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });
            _creditLines.Add(new CreditLine { Text = _ending.Title, Color = GetEndingColor(), Scale = 1f });
        }

        // Final message
        _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });
        _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });
        _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });
        _creditLines.Add(new CreditLine { Text = "Thank you for playing", Color = Color.White, Scale = 1.3f, IsHeader = true });
        _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });
        _creditLines.Add(new CreditLine { Text = "", Color = Color.White, Scale = 1f });
        _creditLines.Add(new CreditLine { Text = "Press any key to continue", Color = Color.Gray, Scale = 0.8f });
    }

    private Color GetEndingColor()
    {
        return _ending.Type switch
        {
            EndingType.Rejection => Color.Gray,
            EndingType.Integration => Color.Cyan,
            EndingType.Balance => Color.Silver,
            EndingType.Sacrifice => Color.Red,
            EndingType.HollowVictory => Color.DarkGray,
            EndingType.SecretArchive => Color.Gold,
            EndingType.CompanionSacrifice => Color.Orange,
            _ => Color.White
        };
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input == null)
            return;

        var keyboardState = Keyboard.GetState();
        bool anyKeyPressed = keyboardState.GetPressedKeyCount() > 0 &&
                            _previousKeyboardState.GetPressedKeyCount() == 0;

        switch (_phase)
        {
            case EndingPhase.Title:
            case EndingPhase.Summary:
                if (anyKeyPressed)
                {
                    AdvancePhase();
                }
                break;

            case EndingPhase.Epilogue:
                if (anyKeyPressed)
                {
                    if (!_textComplete)
                    {
                        // Complete text immediately
                        _displayedText = _targetText;
                        _textComplete = true;
                    }
                    else
                    {
                        // Next line or scene
                        AdvanceEpilogue();
                    }
                }
                break;

            case EndingPhase.Credits:
                if (anyKeyPressed)
                {
                    // Speed up credits or skip to end
                    if (_creditsScrollY < GetTotalCreditsHeight() - 200)
                    {
                        _creditsScrollY = GetTotalCreditsHeight() - 200;
                    }
                    else
                    {
                        AdvancePhase();
                    }
                }
                break;
        }

        _previousKeyboardState = keyboardState;
    }

    private void AdvancePhase()
    {
        _timer = 0f;

        switch (_phase)
        {
            case EndingPhase.FadeIn:
                _phase = EndingPhase.Title;
                break;

            case EndingPhase.Title:
                _phase = EndingPhase.Summary;
                break;

            case EndingPhase.Summary:
                if (_ending.EpilogueScenes.Count > 0)
                {
                    _phase = EndingPhase.Epilogue;
                    _currentSceneIndex = 0;
                    _currentLineIndex = 0;
                    StartCurrentEpilogueLine();
                }
                else
                {
                    _phase = EndingPhase.Credits;
                    _creditsScrollY = 0;
                }
                break;

            case EndingPhase.Epilogue:
                _phase = EndingPhase.Credits;
                _creditsScrollY = 0;
                break;

            case EndingPhase.Credits:
                _phase = EndingPhase.FadeOut;
                break;

            case EndingPhase.FadeOut:
                EndingComplete?.Invoke(this, EventArgs.Empty);
                ExitScreen();
                break;
        }
    }

    private void AdvanceEpilogue()
    {
        var currentScene = _ending.EpilogueScenes[_currentSceneIndex];

        _currentLineIndex++;

        if (_currentLineIndex >= currentScene.TextLines.Count)
        {
            // Move to next scene
            _currentSceneIndex++;
            _currentLineIndex = 0;

            if (_currentSceneIndex >= _ending.EpilogueScenes.Count)
            {
                // All scenes done, go to credits
                AdvancePhase();
                return;
            }
        }

        StartCurrentEpilogueLine();
    }

    private void StartCurrentEpilogueLine()
    {
        if (_currentSceneIndex >= _ending.EpilogueScenes.Count)
            return;

        var scene = _ending.EpilogueScenes[_currentSceneIndex];

        if (_currentLineIndex >= scene.TextLines.Count)
            return;

        _targetText = scene.TextLines[_currentLineIndex];
        _displayedText = "";
        _textTimer = 0f;
        _textComplete = false;
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timer += deltaTime;

        switch (_phase)
        {
            case EndingPhase.FadeIn:
                _fadeLevel = Math.Max(0f, 1f - _timer / 2f);
                if (_timer >= 2f)
                {
                    AdvancePhase();
                }
                break;

            case EndingPhase.Title:
                if (_timer >= 4f)
                {
                    AdvancePhase();
                }
                break;

            case EndingPhase.Summary:
                if (_timer >= 5f)
                {
                    AdvancePhase();
                }
                break;

            case EndingPhase.Epilogue:
                UpdateEpilogueText(deltaTime);
                break;

            case EndingPhase.Credits:
                _creditsScrollY += deltaTime * 40f; // Scroll speed
                break;

            case EndingPhase.FadeOut:
                _fadeLevel = Math.Min(1f, _timer / 2f);
                if (_timer >= 2f)
                {
                    AdvancePhase();
                }
                break;
        }
    }

    private void UpdateEpilogueText(float deltaTime)
    {
        if (_textComplete)
            return;

        _textTimer += deltaTime * _textSpeed;
        int charsToShow = (int)_textTimer;

        if (charsToShow >= _targetText.Length)
        {
            _displayedText = _targetText;
            _textComplete = true;
        }
        else
        {
            _displayedText = _targetText.Substring(0, charsToShow);
        }
    }

    private float GetTotalCreditsHeight()
    {
        if (_font == null)
            return 1000f;

        float height = 0;
        float lineHeight = _font.MeasureString("A").Y;

        foreach (var line in _creditLines)
        {
            height += lineHeight * line.Scale * 1.5f;
        }

        return height + 400; // Extra padding at end
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

        // Black background
        var bgRect = new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y);
        spriteBatch.Draw(_pixelTexture, bgRect, Color.Black);

        // Draw current phase content
        switch (_phase)
        {
            case EndingPhase.FadeIn:
            case EndingPhase.FadeOut:
                DrawTitle(spriteBatch, screenSize);
                break;

            case EndingPhase.Title:
                DrawTitle(spriteBatch, screenSize);
                break;

            case EndingPhase.Summary:
                DrawSummary(spriteBatch, screenSize);
                break;

            case EndingPhase.Epilogue:
                DrawEpilogue(spriteBatch, screenSize);
                break;

            case EndingPhase.Credits:
                DrawCredits(spriteBatch, screenSize);
                break;
        }

        // Fade overlay
        if (_fadeLevel > 0)
        {
            spriteBatch.Draw(_pixelTexture, bgRect, Color.Black * _fadeLevel);
        }

        spriteBatch.End();
    }

    private void DrawTitle(SpriteBatch spriteBatch, Vector2 screenSize)
    {
        if (_font == null)
            return;

        var titleText = _ending.Title;
        var titleSize = _font.MeasureString(titleText);
        var titlePos = new Vector2(
            screenSize.X / 2 - titleSize.X / 2,
            screenSize.Y / 2 - titleSize.Y / 2
        );

        // Glow effect (draw multiple times with offset)
        var glowColor = GetEndingColor() * 0.3f * TransitionAlpha;
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                if (x != 0 || y != 0)
                {
                    spriteBatch.DrawString(_font, titleText,
                        titlePos + new Vector2(x, y), glowColor);
                }
            }
        }

        // Main text
        spriteBatch.DrawString(_font, titleText, titlePos, GetEndingColor() * TransitionAlpha);
    }

    private void DrawSummary(SpriteBatch spriteBatch, Vector2 screenSize)
    {
        if (_font == null)
            return;

        // Title at top
        var titleText = _ending.Title;
        var titleSize = _font.MeasureString(titleText);
        var titlePos = new Vector2(
            screenSize.X / 2 - titleSize.X / 2,
            screenSize.Y * 0.3f
        );
        spriteBatch.DrawString(_font, titleText, titlePos, GetEndingColor() * TransitionAlpha);

        // Summary centered
        var summaryText = WrapText(_ending.Summary, screenSize.X * 0.7f);
        var summaryLines = summaryText.Split('\n');
        float lineHeight = _font.MeasureString("A").Y;
        float startY = screenSize.Y * 0.5f;

        for (int i = 0; i < summaryLines.Length; i++)
        {
            var line = summaryLines[i];
            var lineSize = _font.MeasureString(line);
            var linePos = new Vector2(
                screenSize.X / 2 - lineSize.X / 2,
                startY + i * lineHeight * 1.5f
            );
            spriteBatch.DrawString(_font, line, linePos, Color.White * TransitionAlpha);
        }
    }

    private void DrawEpilogue(SpriteBatch spriteBatch, Vector2 screenSize)
    {
        if (_font == null || _currentSceneIndex >= _ending.EpilogueScenes.Count)
            return;

        var scene = _ending.EpilogueScenes[_currentSceneIndex];

        // Scene title at top
        if (!string.IsNullOrEmpty(scene.Title))
        {
            var titleSize = _font.MeasureString(scene.Title);
            var titlePos = new Vector2(
                screenSize.X / 2 - titleSize.X / 2,
                screenSize.Y * 0.15f
            );
            spriteBatch.DrawString(_font, scene.Title, titlePos, scene.TextColor * 0.7f * TransitionAlpha);
        }

        // Current text
        var wrappedText = WrapText(_displayedText, screenSize.X * 0.7f);
        var textLines = wrappedText.Split('\n');
        float lineHeight = _font.MeasureString("A").Y;
        float startY = screenSize.Y * 0.4f;

        for (int i = 0; i < textLines.Length; i++)
        {
            var line = textLines[i];
            var lineSize = _font.MeasureString(line);
            var linePos = new Vector2(
                screenSize.X / 2 - lineSize.X / 2,
                startY + i * lineHeight * 1.5f
            );
            spriteBatch.DrawString(_font, line, linePos, scene.TextColor * TransitionAlpha);
        }

        // Progress indicator
        string progress = $"{_currentSceneIndex + 1}/{_ending.EpilogueScenes.Count}";
        var progressPos = new Vector2(screenSize.X - 60, screenSize.Y - 30);
        spriteBatch.DrawString(_font, progress, progressPos, Color.Gray * 0.5f * TransitionAlpha);

        // Continue prompt
        if (_textComplete)
        {
            float pulse = (float)Math.Sin(_timer * 4) * 0.3f + 0.7f;
            var promptText = "Press any key to continue";
            var promptSize = _font.MeasureString(promptText);
            var promptPos = new Vector2(
                screenSize.X / 2 - promptSize.X / 2,
                screenSize.Y * 0.85f
            );
            spriteBatch.DrawString(_font, promptText, promptPos, Color.Gray * pulse * TransitionAlpha);
        }
    }

    private void DrawCredits(SpriteBatch spriteBatch, Vector2 screenSize)
    {
        if (_font == null)
            return;

        float lineHeight = _font.MeasureString("A").Y;
        float currentY = screenSize.Y - _creditsScrollY;

        foreach (var creditLine in _creditLines)
        {
            // Only draw if visible
            float scaledHeight = lineHeight * creditLine.Scale;
            if (currentY + scaledHeight > -50 && currentY < screenSize.Y + 50)
            {
                var text = creditLine.Text;
                if (string.IsNullOrEmpty(text))
                {
                    currentY += lineHeight * creditLine.Scale * 1.5f;
                    continue;
                }

                Vector2 textSize;
                Vector2 textPos;

                if (creditLine.Scale != 1f)
                {
                    // For scaled text, estimate size
                    textSize = _font.MeasureString(text);
                    textPos = new Vector2(
                        screenSize.X / 2 - (textSize.X * creditLine.Scale) / 2,
                        currentY
                    );

                    // Draw with scale would require different approach - just draw normally for now
                    textPos = new Vector2(
                        screenSize.X / 2 - textSize.X / 2,
                        currentY
                    );
                    spriteBatch.DrawString(_font, text, textPos, creditLine.Color * TransitionAlpha);
                }
                else
                {
                    textSize = _font.MeasureString(text);
                    textPos = new Vector2(
                        screenSize.X / 2 - textSize.X / 2,
                        currentY
                    );
                    spriteBatch.DrawString(_font, text, textPos, creditLine.Color * TransitionAlpha);
                }
            }

            currentY += lineHeight * creditLine.Scale * 1.5f;
        }

        // Gradient fade at top and bottom
        DrawGradientFade(spriteBatch, screenSize, true);  // Top
        DrawGradientFade(spriteBatch, screenSize, false); // Bottom
    }

    private void DrawGradientFade(SpriteBatch spriteBatch, Vector2 screenSize, bool top)
    {
        if (_pixelTexture == null)
            return;

        int fadeHeight = 60;

        for (int i = 0; i < fadeHeight; i++)
        {
            float alpha = top ? (1f - (float)i / fadeHeight) : ((float)i / fadeHeight);
            int y = top ? i : (int)screenSize.Y - fadeHeight + i;

            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, y, (int)screenSize.X, 1),
                Color.Black * alpha);
        }
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
    /// Creates and shows an ending screen.
    /// </summary>
    public static EndingScreen Show(ScreenManager screenManager, EndingDefinition ending,
        EndingSystem endingSystem, PlayerIndex? controllingPlayer = null)
    {
        var screen = new EndingScreen(ending, endingSystem);
        screenManager.AddScreen(screen, controllingPlayer);
        return screen;
    }
}
