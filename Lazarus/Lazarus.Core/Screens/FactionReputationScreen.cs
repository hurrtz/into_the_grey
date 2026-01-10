using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Progression;
using Lazarus.Core.Inputs;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Screen displaying faction reputation standings.
/// </summary>
public class FactionReputationScreen : GameScreen
{
    private readonly GameStateService _gameState;
    private Texture2D? _pixelTexture;
    private SpriteFont? _font;
    private KeyboardState _previousKeyboardState;

    private int _selectedFactionIndex = 0;
    private float _animationTimer = 0f;

    /// <summary>
    /// Faction display data.
    /// </summary>
    private readonly List<FactionDisplayData> _factionData = new();

    public FactionReputationScreen(GameStateService gameState)
    {
        _gameState = gameState;
        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _font = ScreenManager.Font;

        LoadFactionData();
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    /// <summary>
    /// Loads faction display data.
    /// </summary>
    private void LoadFactionData()
    {
        _factionData.Clear();

        // Lazarus
        _factionData.Add(new FactionDisplayData
        {
            Type = FactionType.Lazarus,
            Name = "Lazarus",
            Description = "The central AI system that maintains the wasteland. Its intentions remain unclear.",
            IconColor = Color.Cyan,
            ReputationTiers = new[]
            {
                (-100, "Hostile", "Lazarus's systems actively hunt you."),
                (-50, "Untrusted", "Lazarus monitors your every move."),
                (0, "Unknown", "Lazarus observes but does not interfere."),
                (50, "Recognized", "Lazarus grants limited system access."),
                (100, "Integrated", "Lazarus considers you an extension of itself.")
            },
            Benefits = new Dictionary<int, string>
            {
                { -50, "Hostile drones patrol nearby areas" },
                { 0, "Neutral system interactions" },
                { 50, "Access to Lazarus terminals" },
                { 75, "Reduced encounter rates in Glow" },
                { 100, "Full archive access" }
            }
        });

        // Independents
        _factionData.Add(new FactionDisplayData
        {
            Type = FactionType.Independents,
            Name = "The Independents",
            Description = "Survivors who live outside Lazarus's direct control. They value freedom above all.",
            IconColor = Color.Orange,
            ReputationTiers = new[]
            {
                (-100, "Enemy", "Independents attack on sight."),
                (-50, "Distrusted", "Independents refuse to trade with you."),
                (0, "Stranger", "Independents are wary but not hostile."),
                (50, "Friend", "Independents offer fair trades and tips."),
                (100, "Family", "Independents share their best resources.")
            },
            Benefits = new Dictionary<int, string>
            {
                { -50, "Settlements closed to you" },
                { 0, "Basic trading available" },
                { 50, "Discounted prices (15%)" },
                { 75, "Access to rare items" },
                { 100, "Free healing at settlements" }
            }
        });

        // Machinists
        _factionData.Add(new FactionDisplayData
        {
            Type = FactionType.Machinists,
            Name = "The Machinists",
            Description = "Experts in Stray augmentation and microchip technology. Knowledge is their currency.",
            IconColor = Color.Silver,
            ReputationTiers = new[]
            {
                (-100, "Blacklisted", "Machinists sabotage your equipment."),
                (-50, "Unwelcome", "Machinists refuse service."),
                (0, "Customer", "Standard crafting services available."),
                (50, "Partner", "Advanced augmentation unlocked."),
                (100, "Master", "Legendary equipment crafting.")
            },
            Benefits = new Dictionary<int, string>
            {
                { -50, "Equipment degrades faster" },
                { 0, "Basic crafting available" },
                { 50, "Microchip fusion unlocked" },
                { 75, "Augmentation upgrading" },
                { 100, "Legendary augmentations available" }
            }
        });

        // Strays (collective)
        _factionData.Add(new FactionDisplayData
        {
            Type = FactionType.Strays,
            Name = "The Strays",
            Description = "The collective trust of the wild Strays. Treating them well earns their loyalty.",
            IconColor = Color.LimeGreen,
            ReputationTiers = new[]
            {
                (-100, "Hunter", "Wild Strays flee or attack immediately."),
                (-50, "Threat", "Strays are aggressive and won't join you."),
                (0, "Neutral", "Normal encounter and recruitment rates."),
                (50, "Protector", "Strays are friendlier and easier to recruit."),
                (100, "Alpha", "Rare Strays seek you out.")
            },
            Benefits = new Dictionary<int, string>
            {
                { -50, "Recruitment chance -50%" },
                { 0, "Normal recruitment rates" },
                { 50, "Recruitment chance +25%" },
                { 75, "Wild Strays may help in combat" },
                { 100, "Legendary Strays recruitable" }
            }
        });

        // Hostile (environmental danger)
        _factionData.Add(new FactionDisplayData
        {
            Type = FactionType.Hostile,
            Name = "Wasteland Hostiles",
            Description = "The dangerous elements of the wasteland. Reputation affects encounter difficulty.",
            IconColor = Color.Red,
            ReputationTiers = new[]
            {
                (-100, "Terror", "You are feared. Some hostiles flee."),
                (-50, "Dangerous", "Hostiles hesitate before attacking."),
                (0, "Target", "Normal hostile behavior."),
                (50, "Weak", "Hostiles are more aggressive."),
                (100, "Prey", "Hostiles actively hunt you.")
            },
            Benefits = new Dictionary<int, string>
            {
                { -75, "Some hostiles flee on sight" },
                { -50, "First strike advantage in combat" },
                { 0, "Normal encounter behavior" },
                { 50, "Increased encounter rates" },
                { 100, "Elite enemies appear more often" }
            }
        });
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input == null)
        {
            return;
        }

        var keyboardState = Keyboard.GetState();

        // Navigate factions
        if (IsKeyPressed(keyboardState, Keys.Up) || IsKeyPressed(keyboardState, Keys.W))
        {
            _selectedFactionIndex = (_selectedFactionIndex - 1 + _factionData.Count) % _factionData.Count;
        }

        if (IsKeyPressed(keyboardState, Keys.Down) || IsKeyPressed(keyboardState, Keys.S))
        {
            _selectedFactionIndex = (_selectedFactionIndex + 1) % _factionData.Count;
        }

        // Close screen
        if (IsKeyPressed(keyboardState, Keys.Escape) || IsKeyPressed(keyboardState, Keys.Back))
        {
            ExitScreen();
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

        _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        DrawBackground(spriteBatch);
        DrawFactionList(spriteBatch);
        DrawFactionDetails(spriteBatch);
        DrawInstructions(spriteBatch);

        spriteBatch.End();
    }

    private void DrawBackground(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null)
        {
            return;
        }

        var screenRect = new Rectangle(0, 0, (int)ScreenManager.BaseScreenSize.X, (int)ScreenManager.BaseScreenSize.Y);

        // Dark background
        spriteBatch.Draw(_pixelTexture, screenRect, new Color(15, 15, 25) * 0.95f);

        // Title
        if (_font != null)
        {
            string title = "FACTION STANDINGS";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2(screenRect.Width / 2f - titleSize.X / 2f, 20);
            spriteBatch.DrawString(_font, title, titlePos, Color.Gold);
        }
    }

    private void DrawFactionList(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null || _font == null)
        {
            return;
        }

        int listX = 30;
        int listY = 60;
        int itemHeight = 50;
        int listWidth = 250;

        for (int i = 0; i < _factionData.Count; i++)
        {
            var faction = _factionData[i];
            bool isSelected = i == _selectedFactionIndex;
            int reputation = _gameState.FactionReputation.GetReputation(faction.Type);

            var itemRect = new Rectangle(listX, listY + i * itemHeight, listWidth, itemHeight - 5);

            // Background
            var bgColor = isSelected ? new Color(40, 40, 60) : new Color(25, 25, 35);
            spriteBatch.Draw(_pixelTexture, itemRect, bgColor);

            // Selection indicator
            if (isSelected)
            {
                float pulse = (float)Math.Sin(_animationTimer * 4) * 0.3f + 0.7f;
                spriteBatch.Draw(_pixelTexture, new Rectangle(itemRect.X, itemRect.Y, 3, itemRect.Height), faction.IconColor * pulse);
            }

            // Icon
            var iconRect = new Rectangle(itemRect.X + 10, itemRect.Y + 10, 30, 30);
            spriteBatch.Draw(_pixelTexture, iconRect, faction.IconColor);

            // Name
            var namePos = new Vector2(itemRect.X + 50, itemRect.Y + 5);
            spriteBatch.DrawString(_font, faction.Name, namePos, isSelected ? Color.White : Color.Gray);

            // Reputation bar
            DrawReputationBar(spriteBatch, new Rectangle(itemRect.X + 50, itemRect.Y + 28, 180, 12), reputation, faction.IconColor);
        }
    }

    private void DrawReputationBar(SpriteBatch spriteBatch, Rectangle bounds, int reputation, Color color)
    {
        if (_pixelTexture == null)
        {
            return;
        }

        // Background
        spriteBatch.Draw(_pixelTexture, bounds, new Color(30, 30, 40));

        // Calculate fill (reputation is -100 to +100, map to 0-1)
        float normalized = (reputation + 100f) / 200f;
        int centerX = bounds.X + bounds.Width / 2;

        if (reputation >= 0)
        {
            // Positive reputation - fill right from center
            int fillWidth = (int)(bounds.Width / 2 * normalized * 2) - bounds.Width / 2;
            fillWidth = Math.Max(0, fillWidth);
            var fillRect = new Rectangle(centerX, bounds.Y, fillWidth, bounds.Height);
            spriteBatch.Draw(_pixelTexture, fillRect, color);
        }
        else
        {
            // Negative reputation - fill left from center
            int fillWidth = bounds.Width / 2 - (int)(bounds.Width / 2 * normalized * 2);
            fillWidth = Math.Max(0, fillWidth);
            var fillRect = new Rectangle(centerX - fillWidth, bounds.Y, fillWidth, bounds.Height);
            spriteBatch.Draw(_pixelTexture, fillRect, Color.Red);
        }

        // Center line
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 1, bounds.Y, 2, bounds.Height), Color.White * 0.5f);

        // Border
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), Color.White * 0.3f);
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), Color.White * 0.3f);
    }

    private void DrawFactionDetails(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null || _font == null || _selectedFactionIndex >= _factionData.Count)
        {
            return;
        }

        var faction = _factionData[_selectedFactionIndex];
        int reputation = _gameState.FactionReputation.GetReputation(faction.Type);

        int detailsX = 300;
        int detailsY = 60;
        int detailsWidth = (int)ScreenManager.BaseScreenSize.X - detailsX - 30;
        int detailsHeight = (int)ScreenManager.BaseScreenSize.Y - detailsY - 60;

        var detailsRect = new Rectangle(detailsX, detailsY, detailsWidth, detailsHeight);

        // Background
        spriteBatch.Draw(_pixelTexture, detailsRect, new Color(20, 20, 30));

        // Border
        spriteBatch.Draw(_pixelTexture, new Rectangle(detailsRect.X, detailsRect.Y, detailsRect.Width, 2), faction.IconColor);

        // Header
        int headerY = detailsY + 10;
        spriteBatch.DrawString(_font, faction.Name, new Vector2(detailsX + 15, headerY), faction.IconColor);

        // Description
        int descY = headerY + 30;
        DrawWrappedText(spriteBatch, faction.Description, new Vector2(detailsX + 15, descY), detailsWidth - 30, Color.LightGray);

        // Current reputation
        int repY = descY + 60;
        string repText = $"Current Standing: {reputation}";
        spriteBatch.DrawString(_font, repText, new Vector2(detailsX + 15, repY), Color.White);

        // Large reputation bar
        var bigBarRect = new Rectangle(detailsX + 15, repY + 25, detailsWidth - 30, 20);
        DrawReputationBar(spriteBatch, bigBarRect, reputation, faction.IconColor);

        // Reputation tier labels
        int tierLabelY = repY + 50;
        spriteBatch.DrawString(_font, "-100", new Vector2(bigBarRect.X, tierLabelY), Color.Red);
        spriteBatch.DrawString(_font, "0", new Vector2(bigBarRect.Center.X - 5, tierLabelY), Color.Gray);
        var plusText = "+100";
        spriteBatch.DrawString(_font, plusText, new Vector2(bigBarRect.Right - _font.MeasureString(plusText).X, tierLabelY), Color.Green);

        // Current tier
        int tierY = tierLabelY + 30;
        var currentTier = GetCurrentTier(faction, reputation);
        if (currentTier != null)
        {
            spriteBatch.DrawString(_font, $"Status: {currentTier.Value.name}", new Vector2(detailsX + 15, tierY), Color.Yellow);
            DrawWrappedText(spriteBatch, currentTier.Value.description, new Vector2(detailsX + 15, tierY + 25), detailsWidth - 30, Color.LightGray);
        }

        // Benefits section
        int benefitsY = tierY + 80;
        spriteBatch.DrawString(_font, "Current Benefits:", new Vector2(detailsX + 15, benefitsY), Color.Cyan);

        int benefitLineY = benefitsY + 25;
        foreach (var (threshold, benefit) in faction.Benefits)
        {
            if (reputation >= threshold)
            {
                var bulletColor = threshold <= reputation ? Color.LimeGreen : Color.Gray;
                spriteBatch.DrawString(_font, $"• {benefit}", new Vector2(detailsX + 20, benefitLineY), bulletColor);
                benefitLineY += 20;
            }
        }

        // Tiers reference
        int tiersY = benefitLineY + 20;
        spriteBatch.DrawString(_font, "Reputation Tiers:", new Vector2(detailsX + 15, tiersY), Color.Orange);

        int tierLineY = tiersY + 25;
        foreach (var (threshold, name, _) in faction.ReputationTiers)
        {
            bool isCurrentTier = reputation >= threshold && (GetNextThreshold(faction, threshold) == null || reputation < GetNextThreshold(faction, threshold));
            var tierColor = isCurrentTier ? Color.Yellow : Color.Gray;
            string indicator = isCurrentTier ? ">" : " ";
            spriteBatch.DrawString(_font, $"{indicator} {threshold,4}: {name}", new Vector2(detailsX + 20, tierLineY), tierColor);
            tierLineY += 18;
        }
    }

    private (int threshold, string name, string description)? GetCurrentTier(FactionDisplayData faction, int reputation)
    {
        (int threshold, string name, string description)? current = null;

        foreach (var tier in faction.ReputationTiers)
        {
            if (reputation >= tier.threshold)
            {
                current = tier;
            }
        }

        return current;
    }

    private int? GetNextThreshold(FactionDisplayData faction, int currentThreshold)
    {
        bool foundCurrent = false;

        foreach (var tier in faction.ReputationTiers)
        {
            if (foundCurrent)
            {
                return tier.threshold;
            }

            if (tier.threshold == currentThreshold)
            {
                foundCurrent = true;
            }
        }

        return null;
    }

    private void DrawWrappedText(SpriteBatch spriteBatch, string text, Vector2 position, int maxWidth, Color color)
    {
        if (_font == null)
        {
            return;
        }

        var words = text.Split(' ');
        string currentLine = "";
        float y = position.Y;

        foreach (var word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var size = _font.MeasureString(testLine);

            if (size.X > maxWidth)
            {
                spriteBatch.DrawString(_font, currentLine, new Vector2(position.X, y), color);
                y += size.Y;
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            spriteBatch.DrawString(_font, currentLine, new Vector2(position.X, y), color);
        }
    }

    private void DrawInstructions(SpriteBatch spriteBatch)
    {
        if (_font == null)
        {
            return;
        }

        string instructions = "Up/Down Navigate   ESC Close";
        var instrSize = _font.MeasureString(instructions);
        var instrPos = new Vector2(
            ScreenManager.BaseScreenSize.X / 2f - instrSize.X / 2f,
            ScreenManager.BaseScreenSize.Y - 30
        );

        spriteBatch.DrawString(_font, instructions, instrPos, Color.Gray);
    }
}

/// <summary>
/// Display data for a faction.
/// </summary>
public class FactionDisplayData
{
    public FactionType Type { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public Color IconColor { get; init; }
    public (int threshold, string name, string description)[] ReputationTiers { get; init; } = Array.Empty<(int, string, string)>();
    public Dictionary<int, string> Benefits { get; init; } = new();
}
