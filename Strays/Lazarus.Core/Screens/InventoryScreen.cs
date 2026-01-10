using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Game.Items;
using Lazarus.Core.Inputs;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Tabs for different inventory categories.
/// </summary>
public enum InventoryTab
{
    Microchips,
    Augmentations,
    Items,
    KeyItems
}

/// <summary>
/// Screen for managing inventory items, microchips, and augmentations.
/// </summary>
public class InventoryScreen : GameScreen
{
    private readonly StrayRoster _roster;
    private readonly List<string> _ownedMicrochips;
    private readonly List<string> _ownedAugmentations;
    private readonly List<string> _ownedItems;

    private SpriteFont _font;
    private SpriteFont _smallFont;
    private Texture2D _pixelTexture;

    private InventoryTab _currentTab = InventoryTab.Microchips;
    private int _selectedIndex = 0;
    private int _scrollOffset = 0;
    private bool _equipMode = false;
    private int _selectedStrayIndex = 0;

    private const int MaxVisibleItems = 10;

    public InventoryScreen(StrayRoster roster, List<string> microchips, List<string> augmentations, List<string> items)
    {
        _roster = roster;
        _ownedMicrochips = microchips ?? new List<string>();
        _ownedAugmentations = augmentations ?? new List<string>();
        _ownedItems = items ?? new List<string>();

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
            if (_equipMode)
            {
                _equipMode = false;
            }
            else
            {
                ExitScreen();
            }
            return;
        }

        if (_equipMode)
        {
            HandleEquipInput(input);
            return;
        }

        // Tab switching
        if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Q, ControllingPlayer, out _))
        {
            _currentTab = (InventoryTab)(((int)_currentTab - 1 + 4) % 4);
            _selectedIndex = 0;
            _scrollOffset = 0;
        }
        else if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.E, ControllingPlayer, out _))
        {
            _currentTab = (InventoryTab)(((int)_currentTab + 1) % 4);
            _selectedIndex = 0;
            _scrollOffset = 0;
        }

        // Navigation
        var items = GetCurrentItems();
        int maxIndex = items.Count - 1;

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

        // Select item
        if (input.IsMenuSelect(ControllingPlayer, out playerIndex) && items.Count > 0)
        {
            if (_currentTab == InventoryTab.Microchips || _currentTab == InventoryTab.Augmentations)
            {
                _equipMode = true;
                _selectedStrayIndex = 0;
            }
        }
    }

    private void HandleEquipInput(InputState input)
    {
        var partyList = _roster.Party.ToList();
        PlayerIndex playerIndex;

        if (input.IsMenuUp(ControllingPlayer))
        {
            _selectedStrayIndex = Math.Max(0, _selectedStrayIndex - 1);
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _selectedStrayIndex = Math.Min(partyList.Count - 1, _selectedStrayIndex + 1);
        }
        else if (input.IsMenuSelect(ControllingPlayer, out playerIndex) && partyList.Count > 0)
        {
            var stray = partyList[_selectedStrayIndex];
            var items = GetCurrentItems();
            if (_selectedIndex >= 0 && _selectedIndex < items.Count)
            {
                string itemId = items[_selectedIndex];
                EquipItem(stray, itemId);
                _equipMode = false;
            }
        }
    }

    private void EquipItem(Stray stray, string itemId)
    {
        if (_currentTab == InventoryTab.Microchips)
        {
            var chipDef = Microchips.Get(itemId);
            if (chipDef != null && stray.EquippedMicrochips.Count < stray.Definition.MicrochipSlots)
            {
                stray.EquippedMicrochips.Add(itemId);
                _ownedMicrochips.Remove(itemId);
            }
        }
        else if (_currentTab == InventoryTab.Augmentations)
        {
            var augDef = Augmentations.Get(itemId);
            if (augDef != null)
            {
                // Check if slot is empty and category is compatible
                var slot = augDef.Slot;
                var slotKey = slot.ToKey();
                if (stray.EquippedAugmentations.TryGetValue(slotKey, out var equipped) &&
                    equipped == null &&
                    augDef.IsCompatibleWith(stray.Definition.Category))
                {
                    stray.EquipAugmentation(itemId, slot);
                    _ownedAugmentations.Remove(itemId);
                }
            }
        }
    }

    private List<string> GetCurrentItems()
    {
        return _currentTab switch
        {
            InventoryTab.Microchips => _ownedMicrochips,
            InventoryTab.Augmentations => _ownedAugmentations,
            InventoryTab.Items => _ownedItems,
            InventoryTab.KeyItems => new List<string>(), // TODO: Key items
            _ => new List<string>()
        };
    }

    private void UpdateScrollOffset()
    {
        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + MaxVisibleItems)
        {
            _scrollOffset = _selectedIndex - MaxVisibleItems + 1;
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
        string title = "Inventory";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2((viewport.Width - titleSize.X) / 2, 15), Color.White);

        // Draw tabs
        DrawTabs(spriteBatch, new Rectangle(20, 55, viewport.Width - 40, 35));

        // Draw main content area
        var contentBounds = new Rectangle(20, 100, viewport.Width - 40, viewport.Height - 160);

        if (_equipMode)
        {
            DrawEquipPanel(spriteBatch, contentBounds);
        }
        else
        {
            DrawItemList(spriteBatch, new Rectangle(contentBounds.X, contentBounds.Y, contentBounds.Width / 2 - 5, contentBounds.Height));
            DrawItemDetails(spriteBatch, new Rectangle(contentBounds.X + contentBounds.Width / 2 + 5, contentBounds.Y, contentBounds.Width / 2 - 5, contentBounds.Height));
        }

        // Draw instructions
        string instructions = _equipMode
            ? "[Arrow Keys] Select Stray | [Enter] Equip | [ESC] Cancel"
            : "[Q/E] Switch Tab | [Arrow Keys] Navigate | [Enter] Equip | [ESC] Back";
        var instrSize = _smallFont.MeasureString(instructions);
        spriteBatch.DrawString(_smallFont, instructions, new Vector2((viewport.Width - instrSize.X) / 2, viewport.Height - 30), Color.Gray);

        spriteBatch.End();
    }

    private void DrawTabs(SpriteBatch spriteBatch, Rectangle bounds)
    {
        string[] tabNames = { "Microchips", "Augmentations", "Items", "Key Items" };
        int tabWidth = bounds.Width / tabNames.Length;

        for (int i = 0; i < tabNames.Length; i++)
        {
            var tabBounds = new Rectangle(bounds.X + i * tabWidth, bounds.Y, tabWidth - 2, bounds.Height);
            bool isSelected = (int)_currentTab == i;

            // Background
            spriteBatch.Draw(_pixelTexture, tabBounds, isSelected ? Color.DarkSlateGray : Color.DimGray * 0.5f);

            // Border
            DrawBorder(spriteBatch, tabBounds, isSelected ? Color.Cyan : Color.Gray);

            // Text
            var textSize = _smallFont.MeasureString(tabNames[i]);
            spriteBatch.DrawString(_smallFont, tabNames[i],
                new Vector2(tabBounds.X + (tabBounds.Width - textSize.X) / 2, tabBounds.Y + (tabBounds.Height - textSize.Y) / 2),
                isSelected ? Color.White : Color.Gray);
        }
    }

    private void DrawItemList(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.4f);
        DrawBorder(spriteBatch, bounds, Color.Gray);

        var items = GetCurrentItems();

        if (items.Count == 0)
        {
            string emptyText = "No items";
            var textSize = _smallFont.MeasureString(emptyText);
            spriteBatch.DrawString(_smallFont, emptyText,
                new Vector2(bounds.X + (bounds.Width - textSize.X) / 2, bounds.Y + bounds.Height / 2),
                Color.Gray);
            return;
        }

        int yOffset = 5;
        for (int i = 0; i < MaxVisibleItems && i + _scrollOffset < items.Count; i++)
        {
            int actualIndex = i + _scrollOffset;
            string itemId = items[actualIndex];
            string itemName = GetItemName(itemId);

            var slotBounds = new Rectangle(bounds.X + 5, bounds.Y + yOffset + i * 35, bounds.Width - 10, 30);
            bool isSelected = actualIndex == _selectedIndex;

            // Background
            spriteBatch.Draw(_pixelTexture, slotBounds, isSelected ? Color.DarkBlue * 0.5f : Color.Transparent);

            // Selection indicator
            if (isSelected)
            {
                DrawBorder(spriteBatch, slotBounds, Color.Yellow);
            }

            // Item name
            Color itemColor = GetItemRarityColor(itemId);
            spriteBatch.DrawString(_smallFont, itemName, new Vector2(slotBounds.X + 5, slotBounds.Y + 5), itemColor);
        }

        // Scroll indicators
        if (_scrollOffset > 0)
        {
            spriteBatch.DrawString(_smallFont, "^", new Vector2(bounds.X + bounds.Width - 15, bounds.Y + 5), Color.White);
        }
        if (_scrollOffset + MaxVisibleItems < items.Count)
        {
            spriteBatch.DrawString(_smallFont, "v", new Vector2(bounds.X + bounds.Width - 15, bounds.Y + bounds.Height - 20), Color.White);
        }
    }

    private void DrawItemDetails(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.4f);
        DrawBorder(spriteBatch, bounds, Color.Gray);

        var items = GetCurrentItems();
        if (items.Count == 0 || _selectedIndex >= items.Count)
        {
            return;
        }

        string itemId = items[_selectedIndex];
        int yOffset = 10;

        // Item name
        string name = GetItemName(itemId);
        Color nameColor = GetItemRarityColor(itemId);
        spriteBatch.DrawString(_font, name, new Vector2(bounds.X + 10, bounds.Y + yOffset), nameColor);
        yOffset += 35;

        // Rarity
        string rarity = GetItemRarity(itemId);
        spriteBatch.DrawString(_smallFont, $"Rarity: {rarity}", new Vector2(bounds.X + 10, bounds.Y + yOffset), Color.Gray);
        yOffset += 25;

        // Description
        string description = GetItemDescription(itemId);
        var wrappedDesc = WrapText(description, bounds.Width - 20);
        foreach (var line in wrappedDesc)
        {
            spriteBatch.DrawString(_smallFont, line, new Vector2(bounds.X + 10, bounds.Y + yOffset), Color.White);
            yOffset += 18;
        }
        yOffset += 10;

        // Stats/Effects
        var effects = GetItemEffects(itemId);
        if (effects.Count > 0)
        {
            spriteBatch.DrawString(_smallFont, "Effects:", new Vector2(bounds.X + 10, bounds.Y + yOffset), Color.Yellow);
            yOffset += 20;
            foreach (var effect in effects)
            {
                spriteBatch.DrawString(_smallFont, $"  {effect}", new Vector2(bounds.X + 10, bounds.Y + yOffset), Color.LightGreen);
                yOffset += 18;
            }
        }
    }

    private void DrawEquipPanel(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.6f);
        DrawBorder(spriteBatch, bounds, Color.Cyan);

        // Title
        spriteBatch.DrawString(_font, "Select Stray to Equip", new Vector2(bounds.X + 10, bounds.Y + 10), Color.White);

        var partyList = _roster.Party.ToList();
        if (partyList.Count == 0)
        {
            spriteBatch.DrawString(_smallFont, "No Strays in party!", new Vector2(bounds.X + 20, bounds.Y + 60), Color.Red);
            return;
        }

        int yOffset = 50;
        for (int i = 0; i < partyList.Count; i++)
        {
            var stray = partyList[i];
            var slotBounds = new Rectangle(bounds.X + 10, bounds.Y + yOffset + i * 60, bounds.Width - 20, 55);
            bool isSelected = i == _selectedStrayIndex;

            // Background
            spriteBatch.Draw(_pixelTexture, slotBounds, isSelected ? Color.DarkBlue * 0.5f : Color.DimGray * 0.3f);
            DrawBorder(spriteBatch, slotBounds, isSelected ? Color.Yellow : Color.Gray);

            // Stray color
            var colorRect = new Rectangle(slotBounds.X + 5, slotBounds.Y + 5, 25, 25);
            spriteBatch.Draw(_pixelTexture, colorRect, stray.Definition.PlaceholderColor);

            // Name and level
            spriteBatch.DrawString(_smallFont, $"{stray.DisplayName} Lv.{stray.Level}",
                new Vector2(slotBounds.X + 40, slotBounds.Y + 5), Color.White);

            // Current equipment count
            string equipInfo = _currentTab == InventoryTab.Microchips
                ? $"Chips: {stray.EquippedMicrochips.Count}/{stray.Definition.MicrochipSlots}"
                : $"Augmentations: {stray.EquippedAugmentations.Count(kvp => kvp.Value != null)}";
            spriteBatch.DrawString(_smallFont, equipInfo,
                new Vector2(slotBounds.X + 40, slotBounds.Y + 25), Color.Gray);
        }
    }

    private string GetItemName(string itemId)
    {
        return _currentTab switch
        {
            InventoryTab.Microchips => Microchips.Get(itemId)?.Name ?? itemId,
            InventoryTab.Augmentations => Augmentations.Get(itemId)?.Name ?? itemId,
            _ => itemId
        };
    }

    private string GetItemDescription(string itemId)
    {
        return _currentTab switch
        {
            InventoryTab.Microchips => Microchips.Get(itemId)?.Description ?? "No description",
            InventoryTab.Augmentations => Augmentations.Get(itemId)?.Description ?? "No description",
            _ => "No description"
        };
    }

    private string GetItemRarity(string itemId)
    {
        return _currentTab switch
        {
            InventoryTab.Microchips => Microchips.Get(itemId)?.Rarity.ToString() ?? "Common",
            InventoryTab.Augmentations => Augmentations.Get(itemId)?.Rarity.ToString() ?? "Common",
            _ => "Common"
        };
    }

    private Color GetItemRarityColor(string itemId)
    {
        ItemRarity rarity = ItemRarity.Common;

        if (_currentTab == InventoryTab.Microchips)
        {
            rarity = Microchips.Get(itemId)?.Rarity ?? ItemRarity.Common;
        }
        else if (_currentTab == InventoryTab.Augmentations)
        {
            rarity = Augmentations.Get(itemId)?.Rarity ?? ItemRarity.Common;
        }

        return rarity switch
        {
            ItemRarity.Common => Color.White,
            ItemRarity.Uncommon => Color.LightGreen,
            ItemRarity.Rare => Color.CornflowerBlue,
            ItemRarity.Epic => Color.MediumPurple,
            ItemRarity.Legendary => Color.Gold,
            _ => Color.White
        };
    }

    private List<string> GetItemEffects(string itemId)
    {
        var effects = new List<string>();

        if (_currentTab == InventoryTab.Microchips)
        {
            var chip = Microchips.Get(itemId);
            if (chip != null)
            {
                foreach (var bonus in chip.StatBonuses)
                {
                    effects.Add($"{bonus.Key} +{bonus.Value}");
                }
                foreach (var mult in chip.StatMultipliers)
                {
                    var percent = (mult.Value - 1f) * 100f;
                    effects.Add($"{mult.Key} {(percent >= 0 ? "+" : "")}{percent:F0}%");
                }
                if (!string.IsNullOrEmpty(chip.GrantsAbility))
                    effects.Add($"Grants: {chip.GrantsAbility}");
            }
        }
        else if (_currentTab == InventoryTab.Augmentations)
        {
            var aug = Augmentations.Get(itemId);
            if (aug != null)
            {
                foreach (var bonus in aug.StatBonuses)
                {
                    effects.Add($"{bonus.Key} +{bonus.Value}");
                }
                foreach (var mult in aug.StatMultipliers)
                {
                    var percent = (mult.Value - 1f) * 100f;
                    effects.Add($"{mult.Key} {(percent >= 0 ? "+" : "")}{percent:F0}%");
                }
                effects.Add($"Slot: {aug.Slot}");
            }
        }

        return effects;
    }

    private List<string> WrapText(string text, float maxWidth)
    {
        var lines = new List<string>();
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
