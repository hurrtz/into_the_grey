using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Data;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Screen for selecting the companion at the start of a new game.
/// </summary>
class CompanionSelectScreen : GameScreen
{
    private SpriteFont? _font;
    private SpriteFont? _smallFont;
    private Texture2D? _pixelTexture;

    private int _selectedIndex = 0;
    private readonly CompanionOption[] _options;

    private KeyboardState _previousKeyboardState;

    private class CompanionOption
    {
        public CompanionType Type { get; init; }
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public string Personality { get; init; } = "";
        public string Ability { get; init; } = "";
        public Color Color { get; init; }
    }

    public CompanionSelectScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.3);

        _options = new[]
        {
            new CompanionOption
            {
                Type = CompanionType.Dog,
                Name = "Bandit",
                Description = "A loyal dog with fierce determination. Bandit was your first friend in the wasteland.",
                Personality = "Loyal, protective, brave",
                Ability = "Gravitation - Deals heavy damage to enemies (may misfire as corruption grows)",
                Color = Color.Orange
            },
            new CompanionOption
            {
                Type = CompanionType.Cat,
                Name = "Tinker",
                Description = "A clever cat with sharp instincts. Tinker found you wandering and decided you needed help.",
                Personality = "Curious, independent, cunning",
                Ability = "Gravitation - Deals heavy damage to enemies (may misfire as corruption grows)",
                Color = Color.Gray
            },
            new CompanionOption
            {
                Type = CompanionType.Rabbit,
                Name = "Pirate",
                Description = "A quick rabbit with boundless energy. Pirate thinks you're the key to something important.",
                Personality = "Energetic, optimistic, fast",
                Ability = "Gravitation - Deals heavy damage to enemies (may misfire as corruption grows)",
                Color = Color.White
            }
        };
    }

    public override void LoadContent()
    {
        base.LoadContent();

        var content = ScreenManager.Game.Content;
        _font = content.Load<SpriteFont>("Fonts/MenuFont");
        _smallFont = content.Load<SpriteFont>("Fonts/GameFont");

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input == null) return;

        var keyboardState = Keyboard.GetState();

        // Navigate options
        if (IsKeyPressed(keyboardState, Keys.Left) || IsKeyPressed(keyboardState, Keys.A))
        {
            _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;
        }
        if (IsKeyPressed(keyboardState, Keys.Right) || IsKeyPressed(keyboardState, Keys.D))
        {
            _selectedIndex = (_selectedIndex + 1) % _options.Length;
        }

        // Confirm selection
        if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
        {
            StartNewGame(_options[_selectedIndex].Type);
        }

        // Cancel
        if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            ExitScreen();
        }

        _previousKeyboardState = keyboardState;
    }

    private bool IsKeyPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    private void StartNewGame(CompanionType companionType)
    {
        // Get or create GameStateService
        var gameState = ScreenManager.Game.Services.GetService<GameStateService>();
        if (gameState == null)
        {
            gameState = new GameStateService();
            ScreenManager.Game.Services.AddService(typeof(GameStateService), gameState);
        }

        // Initialize new game with chosen companion
        gameState.NewGame(companionType);

        // Transition to world screen
        LoadingScreen.Load(ScreenManager, true, ControllingPlayer, new WorldScreen());
    }

    public override void Draw(GameTime gameTime)
    {
        if (_font == null || _smallFont == null || _pixelTexture == null)
            return;

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

        // Background
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), new Color(20, 20, 30));

        // Title
        string title = "Choose Your Companion";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2((screenWidth - titleSize.X) / 2, 30), Color.White);

        // Subtitle
        string subtitle = "Your companion will be with you throughout your journey.";
        var subSize = _smallFont.MeasureString(subtitle);
        spriteBatch.DrawString(_smallFont, subtitle, new Vector2((screenWidth - subSize.X) / 2, 70), Color.Gray);

        // Draw companion options
        int cardWidth = 220;
        int cardHeight = 280;
        int spacing = 30;
        int totalWidth = _options.Length * cardWidth + (_options.Length - 1) * spacing;
        int startX = (int)(screenWidth - totalWidth) / 2;
        int cardY = 110;

        for (int i = 0; i < _options.Length; i++)
        {
            var option = _options[i];
            int cardX = startX + i * (cardWidth + spacing);
            bool isSelected = i == _selectedIndex;

            DrawCompanionCard(spriteBatch, new Rectangle(cardX, cardY, cardWidth, cardHeight), option, isSelected);
        }

        // Instructions
        string instructions = "[Left/Right] Select | [Enter] Confirm | [ESC] Back";
        var instrSize = _smallFont.MeasureString(instructions);
        spriteBatch.DrawString(_smallFont, instructions, new Vector2((screenWidth - instrSize.X) / 2, screenHeight - 30), Color.Gray);

        // Warning text
        string warning = "Warning: Your companion's fate is tied to the Gravitation ability...";
        var warnSize = _smallFont.MeasureString(warning);
        spriteBatch.DrawString(_smallFont, warning, new Vector2((screenWidth - warnSize.X) / 2, screenHeight - 55), Color.DarkOrange);

        spriteBatch.End();
    }

    private void DrawCompanionCard(SpriteBatch spriteBatch, Rectangle bounds, CompanionOption option, bool isSelected)
    {
        // Card background
        Color bgColor = isSelected ? new Color(40, 40, 60) : new Color(25, 25, 35);
        spriteBatch.Draw(_pixelTexture!, bounds, bgColor);

        // Border
        Color borderColor = isSelected ? option.Color : Color.DimGray;
        int borderThickness = isSelected ? 3 : 1;
        DrawBorder(spriteBatch, bounds, borderColor, borderThickness);

        // Companion visual (placeholder circle)
        int circleSize = 60;
        var circleRect = new Rectangle(
            bounds.X + (bounds.Width - circleSize) / 2,
            bounds.Y + 20,
            circleSize,
            circleSize
        );
        spriteBatch.Draw(_pixelTexture!, circleRect, option.Color);

        // Selection indicator
        if (isSelected)
        {
            var indicatorRect = new Rectangle(
                bounds.X + (bounds.Width - 10) / 2,
                bounds.Y + 5,
                10,
                10
            );
            spriteBatch.Draw(_pixelTexture!, indicatorRect, Color.Yellow);
        }

        // Name
        var nameSize = _font!.MeasureString(option.Name);
        spriteBatch.DrawString(_font, option.Name,
            new Vector2(bounds.X + (bounds.Width - nameSize.X) / 2, bounds.Y + 90),
            isSelected ? Color.White : Color.Gray);

        // Personality
        spriteBatch.DrawString(_smallFont!, option.Personality,
            new Vector2(bounds.X + 10, bounds.Y + 120),
            Color.LightGray);

        // Description (wrapped)
        DrawWrappedText(spriteBatch, option.Description, new Rectangle(bounds.X + 10, bounds.Y + 145, bounds.Width - 20, 80), Color.White);

        // Ability
        if (isSelected)
        {
            spriteBatch.DrawString(_smallFont!, "Ability:",
                new Vector2(bounds.X + 10, bounds.Y + 230),
                Color.Yellow);
            DrawWrappedText(spriteBatch, option.Ability, new Rectangle(bounds.X + 10, bounds.Y + 248, bounds.Width - 20, 40), Color.Orange);
        }
    }

    private void DrawWrappedText(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color)
    {
        var words = text.Split(' ');
        string currentLine = "";
        int y = bounds.Y;
        int lineHeight = 16;

        foreach (var word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var testSize = _smallFont!.MeasureString(testLine);

            if (testSize.X > bounds.Width)
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    spriteBatch.DrawString(_smallFont, currentLine, new Vector2(bounds.X, y), color);
                    y += lineHeight;
                    currentLine = word;
                }
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine) && y < bounds.Y + bounds.Height)
        {
            spriteBatch.DrawString(_smallFont!, currentLine, new Vector2(bounds.X, y), color);
        }
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness)
    {
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y + bounds.Height - thickness, bounds.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X + bounds.Width - thickness, bounds.Y, thickness, bounds.Height), color);
    }
}
