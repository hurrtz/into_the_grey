using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lazarus.Core.Game.Progression;
using Lazarus.Core.Inputs;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Screen for viewing faction standings and information.
/// </summary>
public class FactionScreen : GameScreen
{
    private readonly FactionReputation _reputation;
    private SpriteFont _font;
    private SpriteFont _smallFont;
    private Texture2D _pixelTexture;

    private int _selectedIndex = 0;
    private FactionType[] _factions;

    public FactionScreen(FactionReputation reputation)
    {
        _reputation = reputation;
        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);

        // Get all factions except None
        _factions = Enum.GetValues<FactionType>()
            .Where(f => f != FactionType.None)
            .ToArray();
    }

    public override void LoadContent()
    {
        var content = ScreenManager.Game.Content;
        _font = content.Load<SpriteFont>("Fonts/MenuFont");
        _smallFont = content.Load<SpriteFont>("Fonts/GameFont");

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        PlayerIndex playerIndex;

        if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            ExitScreen();
            return;
        }

        if (input.IsMenuUp(ControllingPlayer))
        {
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _selectedIndex = Math.Min(_factions.Length - 1, _selectedIndex + 1);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var viewport = ScreenManager.GraphicsDevice.Viewport;

        spriteBatch.Begin();

        // Draw semi-transparent background
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.85f);

        // Title
        string title = "Faction Relations";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2((viewport.Width - titleSize.X) / 2, 20), Color.White);

        // Draw faction list (left side)
        DrawFactionList(spriteBatch, new Rectangle(20, 70, viewport.Width / 2 - 30, viewport.Height - 120));

        // Draw faction details (right side)
        DrawFactionDetails(spriteBatch, new Rectangle(viewport.Width / 2 + 10, 70, viewport.Width / 2 - 30, viewport.Height - 120));

        // Draw instructions
        string instructions = "[Arrow Keys] Navigate | [ESC] Back";
        var instrSize = _smallFont.MeasureString(instructions);
        spriteBatch.DrawString(_smallFont, instructions, new Vector2((viewport.Width - instrSize.X) / 2, viewport.Height - 30), Color.Gray);

        spriteBatch.End();
    }

    private void DrawFactionList(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.4f);
        DrawBorder(spriteBatch, bounds, Color.Gray);

        int yOffset = 10;
        for (int i = 0; i < _factions.Length; i++)
        {
            var faction = _factions[i];
            var def = FactionData.GetDefinition(faction);
            var standing = _reputation.GetStanding(faction);
            int rep = _reputation.GetReputation(faction);

            var slotBounds = new Rectangle(bounds.X + 5, bounds.Y + yOffset + i * 55, bounds.Width - 10, 50);
            bool isSelected = i == _selectedIndex;

            // Background
            spriteBatch.Draw(_pixelTexture, slotBounds, isSelected ? Color.DarkBlue * 0.5f : Color.Transparent);
            if (isSelected)
            {
                DrawBorder(spriteBatch, slotBounds, Color.Yellow);
            }

            // Banner color indicator
            var colorRect = new Rectangle(slotBounds.X + 5, slotBounds.Y + 5, 8, slotBounds.Height - 10);
            spriteBatch.Draw(_pixelTexture, colorRect, def.BannerColor);

            // Faction name
            spriteBatch.DrawString(_smallFont, def.Name, new Vector2(slotBounds.X + 20, slotBounds.Y + 5), Color.White);

            // Standing
            Color standingColor = FactionData.GetStandingColor(standing);
            string standingText = FactionData.GetStandingName(standing);
            spriteBatch.DrawString(_smallFont, standingText, new Vector2(slotBounds.X + 20, slotBounds.Y + 22), standingColor);

            // Reputation bar
            DrawReputationBar(spriteBatch, new Rectangle(slotBounds.X + 150, slotBounds.Y + 25, slotBounds.Width - 165, 12), rep);

            // Reputation value
            spriteBatch.DrawString(_smallFont, rep.ToString(), new Vector2(slotBounds.Right - 45, slotBounds.Y + 5), Color.Gray);
        }
    }

    private void DrawFactionDetails(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.4f);
        DrawBorder(spriteBatch, bounds, Color.Gray);

        if (_selectedIndex < 0 || _selectedIndex >= _factions.Length)
            return;

        var faction = _factions[_selectedIndex];
        var def = FactionData.GetDefinition(faction);
        var standing = _reputation.GetStanding(faction);
        int rep = _reputation.GetReputation(faction);

        int yOffset = 10;

        // Faction name with banner color
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X + 10, bounds.Y + yOffset, 20, 25), def.BannerColor);
        spriteBatch.DrawString(_font, def.Name, new Vector2(bounds.X + 40, bounds.Y + yOffset), Color.White);
        yOffset += 40;

        // Philosophy
        spriteBatch.DrawString(_smallFont, $"\"{def.Philosophy}\"", new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.LightGray);
        yOffset += 30;

        // Description
        var wrappedDesc = WrapText(def.Description, bounds.Width - 30);
        foreach (var line in wrappedDesc)
        {
            spriteBatch.DrawString(_smallFont, line, new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.White);
            yOffset += 18;
        }
        yOffset += 15;

        // Current standing
        Color standingColor = FactionData.GetStandingColor(standing);
        spriteBatch.DrawString(_smallFont, "Current Standing:", new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.Yellow);
        yOffset += 20;
        spriteBatch.DrawString(_smallFont, $"  {FactionData.GetStandingName(standing)} ({rep})", new Vector2(bounds.X + 15, bounds.Y + yOffset), standingColor);
        yOffset += 25;

        // Price modifier
        float priceModifier = _reputation.GetPriceModifier(faction);
        string priceText = priceModifier <= 1f ? $"{(1f - priceModifier) * 100:F0}% discount" : $"{(priceModifier - 1f) * 100:F0}% markup";
        spriteBatch.DrawString(_smallFont, $"Trade Prices: {priceText}", new Vector2(bounds.X + 15, bounds.Y + yOffset), priceModifier <= 1f ? Color.LightGreen : Color.Salmon);
        yOffset += 30;

        // Allies
        if (def.Allies.Count > 0)
        {
            spriteBatch.DrawString(_smallFont, "Allied With:", new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.LightGreen);
            yOffset += 18;
            foreach (var ally in def.Allies)
            {
                spriteBatch.DrawString(_smallFont, $"  {FactionData.GetName(ally)}", new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.Gray);
                yOffset += 16;
            }
            yOffset += 5;
        }

        // Enemies
        if (def.Enemies.Count > 0)
        {
            spriteBatch.DrawString(_smallFont, "At War With:", new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.Salmon);
            yOffset += 18;
            foreach (var enemy in def.Enemies)
            {
                spriteBatch.DrawString(_smallFont, $"  {FactionData.GetName(enemy)}", new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.Gray);
                yOffset += 16;
            }
            yOffset += 5;
        }

        // Home biomes
        if (def.HomeBiomes.Count > 0)
        {
            spriteBatch.DrawString(_smallFont, "Found In:", new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.CornflowerBlue);
            yOffset += 18;
            string biomeList = string.Join(", ", def.HomeBiomes.Select(b => b.ToUpperInvariant()));
            spriteBatch.DrawString(_smallFont, $"  {biomeList}", new Vector2(bounds.X + 15, bounds.Y + yOffset), Color.Gray);
        }
    }

    private void DrawReputationBar(SpriteBatch spriteBatch, Rectangle bounds, int reputation)
    {
        // Background (full range)
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkGray);

        // Neutral marker (center)
        int centerX = bounds.X + bounds.Width / 2;
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 1, bounds.Y, 2, bounds.Height), Color.White * 0.5f);

        // Fill based on reputation
        float normalizedRep = (reputation + 1000f) / 2000f; // 0 to 1
        int fillWidth = (int)(bounds.Width * normalizedRep);

        Color fillColor = reputation switch
        {
            <= -500 => Color.DarkRed,
            <= -100 => Color.Orange,
            < 100 => Color.Gray,
            < 500 => Color.LightGreen,
            _ => Color.Cyan
        };

        // Draw from left to current position
        var fillRect = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);
        spriteBatch.Draw(_pixelTexture, fillRect, fillColor * 0.7f);

        // Current position marker
        int markerX = bounds.X + fillWidth - 2;
        spriteBatch.Draw(_pixelTexture, new Rectangle(markerX, bounds.Y - 2, 4, bounds.Height + 4), fillColor);
    }

    private System.Collections.Generic.List<string> WrapText(string text, float maxWidth)
    {
        var lines = new System.Collections.Generic.List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            if (_smallFont.MeasureString(testLine).X > maxWidth)
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    lines.Add(word);
                }
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

        return lines;
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        int thickness = 2;
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y + bounds.Height - thickness, bounds.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X + bounds.Width - thickness, bounds.Y, thickness, bounds.Height), color);
    }
}
