using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lazarus.Core.Game.Story;

/// <summary>
/// Types of cutscene elements.
/// </summary>
public enum CutsceneElementType
{
    /// <summary>
    /// Display text/dialog.
    /// </summary>
    Text,

    /// <summary>
    /// Fade to black.
    /// </summary>
    FadeOut,

    /// <summary>
    /// Fade from black.
    /// </summary>
    FadeIn,

    /// <summary>
    /// Wait for time.
    /// </summary>
    Wait,

    /// <summary>
    /// Flash the screen.
    /// </summary>
    Flash,

    /// <summary>
    /// Shake the screen.
    /// </summary>
    Shake,

    /// <summary>
    /// Change background.
    /// </summary>
    Background,

    /// <summary>
    /// Show/hide character portrait.
    /// </summary>
    Portrait,

    /// <summary>
    /// Play sound effect.
    /// </summary>
    Sound,

    /// <summary>
    /// Play music.
    /// </summary>
    Music,

    /// <summary>
    /// Set a story flag.
    /// </summary>
    SetFlag,

    /// <summary>
    /// Branch based on condition.
    /// </summary>
    Branch,

    /// <summary>
    /// Present a choice to the player.
    /// </summary>
    Choice,

    /// <summary>
    /// Show title card.
    /// </summary>
    TitleCard
}

/// <summary>
/// A single element in a cutscene sequence.
/// </summary>
public class CutsceneElement
{
    /// <summary>
    /// Element type.
    /// </summary>
    public CutsceneElementType Type { get; init; }

    /// <summary>
    /// Text content (for Text, TitleCard).
    /// </summary>
    public string Text { get; init; } = "";

    /// <summary>
    /// Speaker name (for Text).
    /// </summary>
    public string? Speaker { get; init; }

    /// <summary>
    /// Duration in seconds.
    /// </summary>
    public float Duration { get; init; } = 0f;

    /// <summary>
    /// Color (for Flash, Text).
    /// </summary>
    public Color Color { get; init; } = Color.White;

    /// <summary>
    /// Intensity (for Shake, Flash).
    /// </summary>
    public float Intensity { get; init; } = 1f;

    /// <summary>
    /// Asset/resource name (for Background, Portrait, Sound, Music).
    /// </summary>
    public string? AssetName { get; init; }

    /// <summary>
    /// Position for portrait display.
    /// </summary>
    public PortraitPosition PortraitPosition { get; init; } = PortraitPosition.Center;

    /// <summary>
    /// Flag name (for SetFlag, Branch).
    /// </summary>
    public string? FlagName { get; init; }

    /// <summary>
    /// Flag value (for SetFlag).
    /// </summary>
    public bool FlagValue { get; init; } = true;

    /// <summary>
    /// Jump target if condition is true (for Branch).
    /// </summary>
    public string? JumpIfTrue { get; init; }

    /// <summary>
    /// Jump target if condition is false (for Branch).
    /// </summary>
    public string? JumpIfFalse { get; init; }

    /// <summary>
    /// Choices (for Choice element).
    /// </summary>
    public List<CutsceneChoice>? Choices { get; init; }

    /// <summary>
    /// Label for jumping.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Whether to auto-advance (no input required).
    /// </summary>
    public bool AutoAdvance { get; init; } = false;

    /// <summary>
    /// Whether text should type out character by character.
    /// </summary>
    public bool TypewriterEffect { get; init; } = true;

    /// <summary>
    /// Typewriter speed (characters per second).
    /// </summary>
    public float TypewriterSpeed { get; init; } = 30f;
}

/// <summary>
/// A choice in a cutscene.
/// </summary>
public class CutsceneChoice
{
    /// <summary>
    /// Display text for the choice.
    /// </summary>
    public string Text { get; init; } = "";

    /// <summary>
    /// Jump target when selected.
    /// </summary>
    public string? JumpTo { get; init; }

    /// <summary>
    /// Flag to set when selected.
    /// </summary>
    public string? SetsFlag { get; init; }

    /// <summary>
    /// Required flag to show this choice.
    /// </summary>
    public string? RequiresFlag { get; init; }

    /// <summary>
    /// Whether this choice is currently available.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// Portrait display position.
/// </summary>
public enum PortraitPosition
{
    Left,
    Center,
    Right,
    Hidden
}

/// <summary>
/// A complete cutscene definition.
/// </summary>
public class CutsceneDefinition
{
    /// <summary>
    /// Unique ID.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Elements in order.
    /// </summary>
    public List<CutsceneElement> Elements { get; init; } = new();

    /// <summary>
    /// Whether the cutscene can be skipped.
    /// </summary>
    public bool Skippable { get; init; } = true;

    /// <summary>
    /// Background music ID.
    /// </summary>
    public string? BackgroundMusic { get; init; }
}

/// <summary>
/// Current state of a running cutscene.
/// </summary>
public class CutsceneState
{
    /// <summary>
    /// The cutscene being played.
    /// </summary>
    public CutsceneDefinition Definition { get; init; } = null!;

    /// <summary>
    /// Current element index.
    /// </summary>
    public int CurrentIndex { get; set; } = 0;

    /// <summary>
    /// Timer for current element.
    /// </summary>
    public float ElementTimer { get; set; } = 0f;

    /// <summary>
    /// Whether waiting for input.
    /// </summary>
    public bool WaitingForInput { get; set; } = false;

    /// <summary>
    /// Currently displayed text.
    /// </summary>
    public string DisplayedText { get; set; } = "";

    /// <summary>
    /// Full text being typed.
    /// </summary>
    public string FullText { get; set; } = "";

    /// <summary>
    /// Characters typed so far.
    /// </summary>
    public int CharactersTyped { get; set; } = 0;

    /// <summary>
    /// Fade level (0-1).
    /// </summary>
    public float FadeLevel { get; set; } = 0f;

    /// <summary>
    /// Flash level (0-1).
    /// </summary>
    public float FlashLevel { get; set; } = 0f;

    /// <summary>
    /// Screen shake offset.
    /// </summary>
    public Vector2 ShakeOffset { get; set; } = Vector2.Zero;

    /// <summary>
    /// Current background asset.
    /// </summary>
    public string? CurrentBackground { get; set; }

    /// <summary>
    /// Current speaker.
    /// </summary>
    public string? CurrentSpeaker { get; set; }

    /// <summary>
    /// Current portrait positions.
    /// </summary>
    public Dictionary<string, PortraitPosition> Portraits { get; } = new();

    /// <summary>
    /// Currently selected choice index.
    /// </summary>
    public int SelectedChoiceIndex { get; set; } = 0;

    /// <summary>
    /// Whether in choice mode.
    /// </summary>
    public bool InChoiceMode { get; set; } = false;

    /// <summary>
    /// Current choices being displayed.
    /// </summary>
    public List<CutsceneChoice> CurrentChoices { get; set; } = new();
}

/// <summary>
/// System for playing cutscenes and story sequences.
/// </summary>
public class CutsceneSystem
{
    private readonly Random _random = new();
    private KeyboardState _previousKeyboardState;

    /// <summary>
    /// Current cutscene state.
    /// </summary>
    public CutsceneState? State { get; private set; }

    /// <summary>
    /// Whether a cutscene is currently playing.
    /// </summary>
    public bool IsPlaying => State != null;

    /// <summary>
    /// Flags dictionary (integrate with GameStateService).
    /// </summary>
    public Dictionary<string, bool> Flags { get; } = new();

    /// <summary>
    /// Event fired when cutscene starts.
    /// </summary>
    public event EventHandler<CutsceneDefinition>? CutsceneStarted;

    /// <summary>
    /// Event fired when cutscene ends.
    /// </summary>
    public event EventHandler<CutsceneDefinition>? CutsceneEnded;

    /// <summary>
    /// Event fired when a flag is set.
    /// </summary>
    public event EventHandler<(string flag, bool value)>? FlagSet;

    /// <summary>
    /// Event fired when a choice is made.
    /// </summary>
    public event EventHandler<CutsceneChoice>? ChoiceMade;

    /// <summary>
    /// Event fired when sound should play.
    /// </summary>
    public event EventHandler<string>? PlaySound;

    /// <summary>
    /// Event fired when music should change.
    /// </summary>
    public event EventHandler<string?>? ChangeMusic;

    /// <summary>
    /// Starts a cutscene.
    /// </summary>
    public void StartCutscene(CutsceneDefinition definition)
    {
        State = new CutsceneState
        {
            Definition = definition,
            CurrentIndex = 0,
            ElementTimer = 0f
        };

        if (!string.IsNullOrEmpty(definition.BackgroundMusic))
        {
            ChangeMusic?.Invoke(this, definition.BackgroundMusic);
        }

        CutsceneStarted?.Invoke(this, definition);
        ProcessCurrentElement();
    }

    /// <summary>
    /// Updates the cutscene system.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (State == null)
        {
            return;
        }

        var keyboardState = Keyboard.GetState();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update screen effects
        UpdateEffects(deltaTime);

        // Handle input
        bool advancePressed = IsKeyPressed(keyboardState, Keys.Enter) ||
                             IsKeyPressed(keyboardState, Keys.Space) ||
                             IsKeyPressed(keyboardState, Keys.Z);

        bool skipPressed = IsKeyPressed(keyboardState, Keys.Escape) &&
                          State.Definition.Skippable;

        if (skipPressed)
        {
            EndCutscene();
            return;
        }

        // Process current element
        var currentElement = GetCurrentElement();
        if (currentElement == null)
        {
            EndCutscene();
            return;
        }

        // Handle choice mode
        if (State.InChoiceMode)
        {
            HandleChoiceInput(keyboardState);
        }
        else
        {
            // Update element timer
            State.ElementTimer += deltaTime;

            // Handle typewriter effect
            if (currentElement.Type == CutsceneElementType.Text && currentElement.TypewriterEffect)
            {
                UpdateTypewriter(currentElement, deltaTime);
            }

            // Handle advance
            if (State.WaitingForInput && advancePressed)
            {
                // If still typing, complete the text
                if (State.CharactersTyped < State.FullText.Length)
                {
                    State.CharactersTyped = State.FullText.Length;
                    State.DisplayedText = State.FullText;
                }
                else
                {
                    AdvanceToNextElement();
                }
            }
            else if (currentElement.AutoAdvance && State.ElementTimer >= currentElement.Duration)
            {
                AdvanceToNextElement();
            }
        }

        _previousKeyboardState = keyboardState;
    }

    /// <summary>
    /// Updates screen effects.
    /// </summary>
    private void UpdateEffects(float deltaTime)
    {
        if (State == null)
        {
            return;
        }

        // Fade transitions
        var element = GetCurrentElement();
        if (element?.Type == CutsceneElementType.FadeOut)
        {
            State.FadeLevel = Math.Min(1f, State.ElementTimer / element.Duration);
        }
        else if (element?.Type == CutsceneElementType.FadeIn)
        {
            State.FadeLevel = Math.Max(0f, 1f - State.ElementTimer / element.Duration);
        }

        // Flash decay
        if (State.FlashLevel > 0)
        {
            State.FlashLevel = Math.Max(0f, State.FlashLevel - deltaTime * 3f);
        }

        // Shake decay
        if (element?.Type == CutsceneElementType.Shake)
        {
            float intensity = element.Intensity * (1f - State.ElementTimer / element.Duration);
            State.ShakeOffset = new Vector2(
                (_random.NextSingle() * 2 - 1) * intensity * 10,
                (_random.NextSingle() * 2 - 1) * intensity * 10
            );
        }
        else if (State.ShakeOffset != Vector2.Zero)
        {
            State.ShakeOffset = Vector2.Lerp(State.ShakeOffset, Vector2.Zero, deltaTime * 10f);
        }
    }

    /// <summary>
    /// Updates the typewriter effect.
    /// </summary>
    private void UpdateTypewriter(CutsceneElement element, float deltaTime)
    {
        if (State == null)
        {
            return;
        }

        if (State.CharactersTyped < State.FullText.Length)
        {
            float charsToAdd = element.TypewriterSpeed * deltaTime;
            State.CharactersTyped = Math.Min(
                State.FullText.Length,
                State.CharactersTyped + (int)Math.Ceiling(charsToAdd)
            );
            State.DisplayedText = State.FullText.Substring(0, State.CharactersTyped);
        }
    }

    /// <summary>
    /// Handles input during choice selection.
    /// </summary>
    private void HandleChoiceInput(KeyboardState keyboardState)
    {
        if (State == null)
        {
            return;
        }

        var availableChoices = State.CurrentChoices.FindAll(c => c.IsAvailable);
        if (availableChoices.Count == 0)
        {
            return;
        }

        // Navigate choices
        if (IsKeyPressed(keyboardState, Keys.Up) || IsKeyPressed(keyboardState, Keys.W))
        {
            State.SelectedChoiceIndex = (State.SelectedChoiceIndex - 1 + availableChoices.Count) % availableChoices.Count;
        }

        if (IsKeyPressed(keyboardState, Keys.Down) || IsKeyPressed(keyboardState, Keys.S))
        {
            State.SelectedChoiceIndex = (State.SelectedChoiceIndex + 1) % availableChoices.Count;
        }

        // Select choice
        if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
        {
            var choice = availableChoices[State.SelectedChoiceIndex];

            // Set flag if specified
            if (!string.IsNullOrEmpty(choice.SetsFlag))
            {
                SetFlag(choice.SetsFlag, true);
            }

            ChoiceMade?.Invoke(this, choice);

            // Jump to target or advance
            if (!string.IsNullOrEmpty(choice.JumpTo))
            {
                JumpToLabel(choice.JumpTo);
            }
            else
            {
                AdvanceToNextElement();
            }

            State.InChoiceMode = false;
        }
    }

    /// <summary>
    /// Processes the current element.
    /// </summary>
    private void ProcessCurrentElement()
    {
        var element = GetCurrentElement();
        if (element == null || State == null)
        {
            return;
        }

        State.ElementTimer = 0f;
        State.WaitingForInput = false;

        switch (element.Type)
        {
            case CutsceneElementType.Text:
                State.CurrentSpeaker = element.Speaker;
                State.FullText = element.Text;
                State.CharactersTyped = element.TypewriterEffect ? 0 : element.Text.Length;
                State.DisplayedText = element.TypewriterEffect ? "" : element.Text;
                State.WaitingForInput = true;
                break;

            case CutsceneElementType.TitleCard:
                State.FullText = element.Text;
                State.DisplayedText = element.Text;
                State.WaitingForInput = !element.AutoAdvance;
                break;

            case CutsceneElementType.FadeIn:
            case CutsceneElementType.FadeOut:
            case CutsceneElementType.Wait:
            case CutsceneElementType.Shake:
                // These auto-advance after duration
                break;

            case CutsceneElementType.Flash:
                State.FlashLevel = element.Intensity;
                AdvanceToNextElement();
                break;

            case CutsceneElementType.Background:
                State.CurrentBackground = element.AssetName;
                AdvanceToNextElement();
                break;

            case CutsceneElementType.Portrait:
                if (!string.IsNullOrEmpty(element.AssetName))
                {
                    State.Portraits[element.AssetName] = element.PortraitPosition;
                }
                AdvanceToNextElement();
                break;

            case CutsceneElementType.Sound:
                if (!string.IsNullOrEmpty(element.AssetName))
                {
                    PlaySound?.Invoke(this, element.AssetName);
                }
                AdvanceToNextElement();
                break;

            case CutsceneElementType.Music:
                ChangeMusic?.Invoke(this, element.AssetName);
                AdvanceToNextElement();
                break;

            case CutsceneElementType.SetFlag:
                if (!string.IsNullOrEmpty(element.FlagName))
                {
                    SetFlag(element.FlagName, element.FlagValue);
                }
                AdvanceToNextElement();
                break;

            case CutsceneElementType.Branch:
                bool condition = !string.IsNullOrEmpty(element.FlagName) && GetFlag(element.FlagName);
                string? target = condition ? element.JumpIfTrue : element.JumpIfFalse;

                if (!string.IsNullOrEmpty(target))
                {
                    JumpToLabel(target);
                }
                else
                {
                    AdvanceToNextElement();
                }
                break;

            case CutsceneElementType.Choice:
                if (element.Choices != null && element.Choices.Count > 0)
                {
                    State.CurrentChoices = new List<CutsceneChoice>(element.Choices);

                    // Update availability based on flags
                    foreach (var choice in State.CurrentChoices)
                    {
                        choice.IsAvailable = string.IsNullOrEmpty(choice.RequiresFlag) ||
                                            GetFlag(choice.RequiresFlag);
                    }

                    State.InChoiceMode = true;
                    State.SelectedChoiceIndex = 0;
                }
                else
                {
                    AdvanceToNextElement();
                }
                break;
        }
    }

    /// <summary>
    /// Advances to the next element.
    /// </summary>
    private void AdvanceToNextElement()
    {
        if (State == null)
        {
            return;
        }

        State.CurrentIndex++;

        if (State.CurrentIndex >= State.Definition.Elements.Count)
        {
            EndCutscene();
        }
        else
        {
            ProcessCurrentElement();
        }
    }

    /// <summary>
    /// Jumps to a labeled element.
    /// </summary>
    private void JumpToLabel(string label)
    {
        if (State == null)
        {
            return;
        }

        int targetIndex = State.Definition.Elements.FindIndex(e => e.Label == label);

        if (targetIndex >= 0)
        {
            State.CurrentIndex = targetIndex;
            ProcessCurrentElement();
        }
        else
        {
            // Label not found, just advance
            AdvanceToNextElement();
        }
    }

    /// <summary>
    /// Gets the current element.
    /// </summary>
    private CutsceneElement? GetCurrentElement()
    {
        if (State == null || State.CurrentIndex >= State.Definition.Elements.Count)
        {
            return null;
        }

        return State.Definition.Elements[State.CurrentIndex];
    }

    /// <summary>
    /// Sets a flag.
    /// </summary>
    private void SetFlag(string name, bool value)
    {
        Flags[name] = value;
        FlagSet?.Invoke(this, (name, value));
    }

    /// <summary>
    /// Gets a flag value.
    /// </summary>
    private bool GetFlag(string name)
    {
        return Flags.TryGetValue(name, out bool value) && value;
    }

    /// <summary>
    /// Ends the current cutscene.
    /// </summary>
    private void EndCutscene()
    {
        if (State == null)
        {
            return;
        }

        var definition = State.Definition;
        State = null;
        CutsceneEnded?.Invoke(this, definition);
    }

    /// <summary>
    /// Checks if a key was just pressed.
    /// </summary>
    private bool IsKeyPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    /// <summary>
    /// Draws the cutscene overlay.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, Rectangle screenBounds)
    {
        if (State == null)
        {
            return;
        }

        // Apply shake offset
        var offset = State.ShakeOffset;

        // Draw fade overlay
        if (State.FadeLevel > 0)
        {
            spriteBatch.Draw(pixel, screenBounds, Color.Black * State.FadeLevel);
        }

        // Draw flash overlay
        if (State.FlashLevel > 0)
        {
            var element = GetCurrentElement();
            var flashColor = element?.Color ?? Color.White;
            spriteBatch.Draw(pixel, screenBounds, flashColor * State.FlashLevel);
        }

        // Draw text box
        var currentElement = GetCurrentElement();
        if (currentElement?.Type == CutsceneElementType.Text ||
            currentElement?.Type == CutsceneElementType.TitleCard)
        {
            DrawTextBox(spriteBatch, pixel, font, screenBounds, currentElement, offset);
        }

        // Draw choice menu
        if (State.InChoiceMode)
        {
            DrawChoiceMenu(spriteBatch, pixel, font, screenBounds, offset);
        }

        // Skip hint
        if (State.Definition.Skippable)
        {
            var skipText = "Press ESC to skip";
            var skipPos = new Vector2(screenBounds.Width - font.MeasureString(skipText).X - 10, 10) + offset;
            spriteBatch.DrawString(font, skipText, skipPos, Color.White * 0.5f);
        }
    }

    /// <summary>
    /// Draws the text box.
    /// </summary>
    private void DrawTextBox(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font,
        Rectangle screenBounds, CutsceneElement element, Vector2 offset)
    {
        if (State == null)
        {
            return;
        }

        bool isTitleCard = element.Type == CutsceneElementType.TitleCard;

        if (isTitleCard)
        {
            // Title card - centered large text
            var textSize = font.MeasureString(State.DisplayedText);
            var textPos = new Vector2(
                screenBounds.Width / 2f - textSize.X / 2f,
                screenBounds.Height / 2f - textSize.Y / 2f
            ) + offset;

            // Background
            var bgRect = new Rectangle(
                (int)(textPos.X - 20),
                (int)(textPos.Y - 10),
                (int)(textSize.X + 40),
                (int)(textSize.Y + 20)
            );
            spriteBatch.Draw(pixel, bgRect, Color.Black * 0.8f);

            spriteBatch.DrawString(font, State.DisplayedText, textPos, element.Color);
        }
        else
        {
            // Regular text box at bottom
            int boxHeight = 120;
            int boxMargin = 20;
            var boxRect = new Rectangle(
                boxMargin + (int)offset.X,
                screenBounds.Height - boxHeight - boxMargin + (int)offset.Y,
                screenBounds.Width - boxMargin * 2,
                boxHeight
            );

            // Box background
            spriteBatch.Draw(pixel, boxRect, new Color(20, 20, 30) * 0.95f);

            // Border
            spriteBatch.Draw(pixel, new Rectangle(boxRect.X, boxRect.Y, boxRect.Width, 2), Color.White * 0.5f);
            spriteBatch.Draw(pixel, new Rectangle(boxRect.X, boxRect.Y, 2, boxRect.Height), Color.White * 0.5f);
            spriteBatch.Draw(pixel, new Rectangle(boxRect.X, boxRect.Bottom - 2, boxRect.Width, 2), Color.White * 0.5f);
            spriteBatch.Draw(pixel, new Rectangle(boxRect.Right - 2, boxRect.Y, 2, boxRect.Height), Color.White * 0.5f);

            // Speaker name
            if (!string.IsNullOrEmpty(State.CurrentSpeaker))
            {
                var speakerPos = new Vector2(boxRect.X + 15, boxRect.Y - 25);

                // Name plate background
                var nameSize = font.MeasureString(State.CurrentSpeaker);
                var nameBg = new Rectangle(
                    (int)speakerPos.X - 5,
                    (int)speakerPos.Y - 2,
                    (int)nameSize.X + 10,
                    (int)nameSize.Y + 4
                );
                spriteBatch.Draw(pixel, nameBg, new Color(40, 40, 60));

                spriteBatch.DrawString(font, State.CurrentSpeaker, speakerPos, Color.Cyan);
            }

            // Text content
            var textPos = new Vector2(boxRect.X + 15, boxRect.Y + 15);
            spriteBatch.DrawString(font, State.DisplayedText, textPos, element.Color);

            // Continue indicator
            if (State.WaitingForInput && State.CharactersTyped >= State.FullText.Length)
            {
                float blink = (float)Math.Sin(State.ElementTimer * 6) > 0 ? 1f : 0.3f;
                var indicatorPos = new Vector2(boxRect.Right - 30, boxRect.Bottom - 25);
                spriteBatch.DrawString(font, "v", indicatorPos, Color.White * blink);
            }
        }
    }

    /// <summary>
    /// Draws the choice menu.
    /// </summary>
    private void DrawChoiceMenu(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font,
        Rectangle screenBounds, Vector2 offset)
    {
        if (State == null)
        {
            return;
        }

        var availableChoices = State.CurrentChoices.FindAll(c => c.IsAvailable);
        if (availableChoices.Count == 0)
        {
            return;
        }

        int menuWidth = 400;
        int menuHeight = availableChoices.Count * 35 + 20;
        int menuX = screenBounds.Width / 2 - menuWidth / 2 + (int)offset.X;
        int menuY = screenBounds.Height / 2 - menuHeight / 2 + (int)offset.Y;

        // Background
        var menuRect = new Rectangle(menuX, menuY, menuWidth, menuHeight);
        spriteBatch.Draw(pixel, menuRect, new Color(30, 30, 50) * 0.95f);

        // Border
        spriteBatch.Draw(pixel, new Rectangle(menuRect.X, menuRect.Y, menuRect.Width, 2), Color.Gold);
        spriteBatch.Draw(pixel, new Rectangle(menuRect.X, menuRect.Bottom - 2, menuRect.Width, 2), Color.Gold);
        spriteBatch.Draw(pixel, new Rectangle(menuRect.X, menuRect.Y, 2, menuRect.Height), Color.Gold);
        spriteBatch.Draw(pixel, new Rectangle(menuRect.Right - 2, menuRect.Y, 2, menuRect.Height), Color.Gold);

        // Choices
        for (int i = 0; i < availableChoices.Count; i++)
        {
            var choice = availableChoices[i];
            bool isSelected = i == State.SelectedChoiceIndex;

            var choicePos = new Vector2(menuX + 20, menuY + 10 + i * 35);
            var prefix = isSelected ? "> " : "  ";
            var color = isSelected ? Color.Yellow : Color.White;

            // Selection highlight
            if (isSelected)
            {
                var highlightRect = new Rectangle(
                    menuX + 5,
                    (int)choicePos.Y - 2,
                    menuWidth - 10,
                    30
                );
                spriteBatch.Draw(pixel, highlightRect, Color.White * 0.1f);
            }

            spriteBatch.DrawString(font, prefix + choice.Text, choicePos, color);
        }
    }
}
