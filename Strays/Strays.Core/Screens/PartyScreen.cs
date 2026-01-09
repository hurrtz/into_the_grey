using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Data;
using Strays.Core.Game.Entities;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Screen for managing the player's Stray party and roster.
/// </summary>
public class PartyScreen : GameScreen
{
    private readonly StrayRoster _roster;
    private readonly GameStateService _gameState;
    private SpriteFont _font;
    private SpriteFont _smallFont;
    private Texture2D _pixelTexture;

    private int _selectedIndex = 0;
    private int _rosterScrollOffset = 0;
    private bool _inRosterView = false;
    private bool _swapMode = false;
    private int _swapSourceIndex = -1;

    private const int MaxVisibleRoster = 8;
    private const int PartySlots = 5;

    public PartyScreen(StrayRoster roster, GameStateService gameState)
    {
        _roster = roster;
        _gameState = gameState;
        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
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
            if (_swapMode)
            {
                _swapMode = false;
                _swapSourceIndex = -1;
            }
            else if (_inRosterView)
            {
                _inRosterView = false;
                _selectedIndex = 0;
            }
            else
            {
                ExitScreen();
            }
            return;
        }

        int maxIndex = _inRosterView ? _roster.Storage.Count - 1 : PartySlots - 1;

        if (input.IsMenuUp(ControllingPlayer))
        {
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
            UpdateScrollOffset();
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _selectedIndex = Math.Min(maxIndex, _selectedIndex + 1);
            UpdateScrollOffset();
        }
        else if (input.IsNewKeyPress(Keys.Left, ControllingPlayer, out _) && _inRosterView)
        {
            _inRosterView = false;
            _selectedIndex = 0;
        }
        else if (input.IsNewKeyPress(Keys.Right, ControllingPlayer, out _) && !_inRosterView)
        {
            _inRosterView = true;
            _selectedIndex = 0;
            _rosterScrollOffset = 0;
        }

        if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
        {
            HandleSelection();
        }

        // Tab to switch views
        if (input.IsNewKeyPress(Keys.Tab, ControllingPlayer, out _))
        {
            _inRosterView = !_inRosterView;
            _selectedIndex = 0;
            if (_swapMode)
            {
                _swapMode = false;
                _swapSourceIndex = -1;
            }
        }

        // E key to open equipment screen for selected Stray
        if (input.IsNewKeyPress(Keys.E, ControllingPlayer, out _) && !_swapMode)
        {
            OpenEquipmentScreen();
        }

        // P key to toggle combat position (Front/Back)
        if (input.IsNewKeyPress(Keys.P, ControllingPlayer, out _) && !_swapMode)
        {
            TogglePosition();
        }
    }

    private void TogglePosition()
    {
        Stray? selectedStray = null;

        if (_inRosterView)
        {
            var storedStrays = _roster.Storage.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < storedStrays.Count)
            {
                selectedStray = storedStrays[_selectedIndex];
            }
        }
        else
        {
            var partyList = _roster.Party.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < partyList.Count)
            {
                selectedStray = partyList[_selectedIndex];
            }
        }

        if (selectedStray != null)
        {
            selectedStray.CombatRow = selectedStray.CombatRow == CombatRow.Front
                ? CombatRow.Back
                : CombatRow.Front;
        }
    }

    private void OpenEquipmentScreen()
    {
        Stray? selectedStray = null;

        if (_inRosterView)
        {
            var storedStrays = _roster.Storage.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < storedStrays.Count)
            {
                selectedStray = storedStrays[_selectedIndex];
            }
        }
        else
        {
            var partyList = _roster.Party.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < partyList.Count)
            {
                selectedStray = partyList[_selectedIndex];
            }
        }

        if (selectedStray != null)
        {
            var equipmentScreen = new EquipmentScreen(selectedStray, _gameState);
            ScreenManager.AddScreen(equipmentScreen, ControllingPlayer);
        }
    }

    private void UpdateScrollOffset()
    {
        if (_inRosterView)
        {
            if (_selectedIndex < _rosterScrollOffset)
            {
                _rosterScrollOffset = _selectedIndex;
            }
            else if (_selectedIndex >= _rosterScrollOffset + MaxVisibleRoster)
            {
                _rosterScrollOffset = _selectedIndex - MaxVisibleRoster + 1;
            }
        }
    }

    private void HandleSelection()
    {
        if (_inRosterView)
        {
            // In roster view
            var storedStrays = _roster.Storage.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < storedStrays.Count)
            {
                var stray = storedStrays[_selectedIndex];

                if (_swapMode && _swapSourceIndex >= 0)
                {
                    // Complete swap: move party member to storage, storage member to party
                    var partyMember = _roster.Party.ElementAtOrDefault(_swapSourceIndex);
                    if (partyMember != null)
                    {
                        _roster.SwapStrays(partyMember, stray);
                    }
                    _swapMode = false;
                    _swapSourceIndex = -1;
                }
                else
                {
                    // Add to party if there's room
                    if (_roster.Party.Count < PartySlots)
                    {
                        _roster.MoveToParty(stray);
                    }
                }
            }
        }
        else
        {
            // In party view
            var partyList = _roster.Party.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < partyList.Count)
            {
                if (_swapMode)
                {
                    // Swap two party members
                    if (_swapSourceIndex != _selectedIndex && _swapSourceIndex >= 0)
                    {
                        var source = partyList[_swapSourceIndex];
                        _roster.ReorderParty(source, _selectedIndex);
                        _swapMode = false;
                        _swapSourceIndex = -1;
                    }
                }
                else
                {
                    // Enter swap mode
                    _swapMode = true;
                    _swapSourceIndex = _selectedIndex;
                }
            }
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
        string title = "Party Management";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2((viewport.Width - titleSize.X) / 2, 20), Color.White);

        // Draw party panel (left side)
        DrawPartyPanel(spriteBatch, new Rectangle(20, 70, viewport.Width / 2 - 30, viewport.Height - 120));

        // Draw roster panel (right side)
        DrawRosterPanel(spriteBatch, new Rectangle(viewport.Width / 2 + 10, 70, viewport.Width / 2 - 30, viewport.Height - 120));

        // Draw instructions
        string instructions = _swapMode
            ? "Select target to swap | [ESC] Cancel"
            : "[Arrows] Navigate | [Enter] Swap | [E] Equip | [P] Position | [Tab] Panel | [ESC] Back";
        var instrSize = _smallFont.MeasureString(instructions);
        spriteBatch.DrawString(_smallFont, instructions, new Vector2((viewport.Width - instrSize.X) / 2, viewport.Height - 30), Color.Gray);

        spriteBatch.End();
    }

    private void DrawPartyPanel(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Panel background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.5f);

        // Panel border
        DrawBorder(spriteBatch, bounds, !_inRosterView ? Color.Cyan : Color.Gray);

        // Title
        string title = $"Active Party ({_roster.Party.Count}/{PartySlots})";
        spriteBatch.DrawString(_smallFont, title, new Vector2(bounds.X + 10, bounds.Y + 5), Color.Yellow);

        // Draw party members
        var partyList = _roster.Party.ToList();
        int yOffset = 35;
        int backRowIndent = 25; // Indent for back row position

        for (int i = 0; i < PartySlots; i++)
        {
            if (i < partyList.Count)
            {
                var stray = partyList[i];
                bool isBackRow = stray.CombatRow == CombatRow.Back;
                int indent = isBackRow ? backRowIndent : 0;

                var slotBounds = new Rectangle(bounds.X + 5 + indent, bounds.Y + yOffset + i * 70, bounds.Width - 10 - indent, 65);

                bool isSelected = !_inRosterView && i == _selectedIndex;
                bool isSwapSource = _swapMode && i == _swapSourceIndex;

                DrawStraySlot(spriteBatch, slotBounds, stray, isSelected, isSwapSource);
            }
            else
            {
                // Empty slot
                var slotBounds = new Rectangle(bounds.X + 5, bounds.Y + yOffset + i * 70, bounds.Width - 10, 65);
                bool isSelected = !_inRosterView && i == _selectedIndex;
                spriteBatch.Draw(_pixelTexture, slotBounds, (isSelected ? Color.DarkGray : Color.DimGray) * 0.3f);
                DrawBorder(spriteBatch, slotBounds, isSelected ? Color.Yellow : Color.DimGray);

                string emptyText = "[Empty Slot]";
                var textSize = _smallFont.MeasureString(emptyText);
                spriteBatch.DrawString(_smallFont, emptyText,
                    new Vector2(slotBounds.X + (slotBounds.Width - textSize.X) / 2, slotBounds.Y + (slotBounds.Height - textSize.Y) / 2),
                    Color.Gray);
            }
        }
    }

    private void DrawRosterPanel(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Panel background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.5f);

        // Panel border
        DrawBorder(spriteBatch, bounds, _inRosterView ? Color.Cyan : Color.Gray);

        // Title
        var storedStrays = _roster.Storage.ToList();
        string title = $"Stray Roster ({storedStrays.Count})";
        spriteBatch.DrawString(_smallFont, title, new Vector2(bounds.X + 10, bounds.Y + 5), Color.Yellow);

        if (storedStrays.Count == 0)
        {
            string emptyText = "No Strays in storage";
            var textSize = _smallFont.MeasureString(emptyText);
            spriteBatch.DrawString(_smallFont, emptyText,
                new Vector2(bounds.X + (bounds.Width - textSize.X) / 2, bounds.Y + bounds.Height / 2),
                Color.Gray);
            return;
        }

        // Draw visible roster entries
        int yOffset = 35;
        for (int i = 0; i < MaxVisibleRoster && i + _rosterScrollOffset < storedStrays.Count; i++)
        {
            int actualIndex = i + _rosterScrollOffset;
            var stray = storedStrays[actualIndex];
            var slotBounds = new Rectangle(bounds.X + 5, bounds.Y + yOffset + i * 50, bounds.Width - 10, 45);

            bool isSelected = _inRosterView && actualIndex == _selectedIndex;
            DrawStraySlotCompact(spriteBatch, slotBounds, stray, isSelected);
        }

        // Draw scroll indicators
        if (_rosterScrollOffset > 0)
        {
            spriteBatch.DrawString(_smallFont, "^", new Vector2(bounds.X + bounds.Width - 20, bounds.Y + 30), Color.White);
        }
        if (_rosterScrollOffset + MaxVisibleRoster < storedStrays.Count)
        {
            spriteBatch.DrawString(_smallFont, "v", new Vector2(bounds.X + bounds.Width - 20, bounds.Y + bounds.Height - 20), Color.White);
        }
    }

    private void DrawStraySlot(SpriteBatch spriteBatch, Rectangle bounds, Stray stray, bool isSelected, bool isSwapSource)
    {
        // Background
        Color bgColor = isSwapSource ? Color.DarkOrange * 0.5f : (isSelected ? Color.DarkBlue * 0.5f : Color.DarkSlateGray * 0.3f);
        spriteBatch.Draw(_pixelTexture, bounds, bgColor);

        // Border
        Color borderColor = isSwapSource ? Color.Orange : (isSelected ? Color.Yellow : Color.Gray);
        DrawBorder(spriteBatch, bounds, borderColor);

        // Position indicator (Front/Back)
        bool isBackRow = stray.CombatRow == CombatRow.Back;
        string posIndicator = isBackRow ? "BACK" : "FRONT";
        Color posColor = isBackRow ? Color.CornflowerBlue : Color.OrangeRed;
        spriteBatch.DrawString(_smallFont, posIndicator, new Vector2(bounds.X + bounds.Width - 45, bounds.Y + 5), posColor);

        // Stray placeholder color
        var colorRect = new Rectangle(bounds.X + 5, bounds.Y + 5, 30, 30);
        spriteBatch.Draw(_pixelTexture, colorRect, stray.Definition.PlaceholderColor);

        // Name and level
        string nameText = $"{stray.DisplayName} Lv.{stray.Level}";
        spriteBatch.DrawString(_smallFont, nameText, new Vector2(bounds.X + 45, bounds.Y + 5), Color.White);

        // Category and role
        string typeText = $"{stray.Definition.Category} / {stray.Definition.Role}";
        spriteBatch.DrawString(_smallFont, typeText, new Vector2(bounds.X + 45, bounds.Y + 22), Color.LightGray);

        // HP bar
        DrawHpBar(spriteBatch, new Rectangle(bounds.X + 45, bounds.Y + 42, 120, 12), stray.CurrentHp, stray.MaxHp);

        // Stats summary
        string statsText = $"ATK:{stray.Attack} DEF:{stray.Defense} SPD:{stray.Speed}";
        spriteBatch.DrawString(_smallFont, statsText, new Vector2(bounds.X + 180, bounds.Y + 22), Color.Gray);
    }

    private void DrawStraySlotCompact(SpriteBatch spriteBatch, Rectangle bounds, Stray stray, bool isSelected)
    {
        // Background
        Color bgColor = isSelected ? Color.DarkBlue * 0.5f : Color.DarkSlateGray * 0.3f;
        spriteBatch.Draw(_pixelTexture, bounds, bgColor);

        // Border
        DrawBorder(spriteBatch, bounds, isSelected ? Color.Yellow : Color.Gray);

        // Stray placeholder color
        var colorRect = new Rectangle(bounds.X + 5, bounds.Y + 5, 20, 20);
        spriteBatch.Draw(_pixelTexture, colorRect, stray.Definition.PlaceholderColor);

        // Name and level
        string nameText = $"{stray.DisplayName} Lv.{stray.Level}";
        spriteBatch.DrawString(_smallFont, nameText, new Vector2(bounds.X + 30, bounds.Y + 5), Color.White);

        // HP bar
        DrawHpBar(spriteBatch, new Rectangle(bounds.X + 30, bounds.Y + 25, 80, 8), stray.CurrentHp, stray.MaxHp);

        // Creature type
        string typeText = stray.Definition.CreatureType.ToString();
        spriteBatch.DrawString(_smallFont, typeText, new Vector2(bounds.X + 120, bounds.Y + 5), Color.Gray);
    }

    private void DrawHpBar(SpriteBatch spriteBatch, Rectangle bounds, int current, int max)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkGray);

        // Fill
        float percent = max > 0 ? (float)current / max : 0;
        Color fillColor = percent > 0.5f ? Color.Green : (percent > 0.25f ? Color.Yellow : Color.Red);
        var fillRect = new Rectangle(bounds.X, bounds.Y, (int)(bounds.Width * percent), bounds.Height);
        spriteBatch.Draw(_pixelTexture, fillRect, fillColor);

        // Text
        string hpText = $"{current}/{max}";
        var textSize = _smallFont.MeasureString(hpText);
        float scale = 0.6f;
        spriteBatch.DrawString(_smallFont, hpText,
            new Vector2(bounds.X + (bounds.Width - textSize.X * scale) / 2, bounds.Y),
            Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
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
