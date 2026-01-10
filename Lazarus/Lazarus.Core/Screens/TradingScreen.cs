using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Game.Items;
using Lazarus.Core.Game.Progression;
using Lazarus.Core.Inputs;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Screen for buying and selling items at a shop.
/// </summary>
public class TradingScreen : GameScreen
{
    private readonly ShopDefinition _shop;
    private readonly GameStateService _gameState;
    private readonly FactionReputation _factionReputation;

    private SpriteFont? _font;
    private SpriteFont? _smallFont;
    private Texture2D? _pixelTexture;

    private enum TradingMode { Buy, Sell }
    private TradingMode _mode = TradingMode.Buy;

    private int _selectedIndex = 0;
    private int _scrollOffset = 0;
    private const int MaxVisibleItems = 8;

    private List<ShopItem> _buyableItems = new();
    private List<(string itemId, ShopCategory category, int count)> _sellableItems = new();

    private KeyboardState _previousKeyboardState;

    /// <summary>
    /// Event fired when the trading screen closes.
    /// </summary>
    public event EventHandler? Closed;

    public TradingScreen(ShopDefinition shop, GameStateService gameState, FactionReputation factionReputation)
    {
        _shop = shop;
        _gameState = gameState;
        _factionReputation = factionReputation;

        IsPopup = true;
        TransitionOnTime = TimeSpan.FromSeconds(0.2);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        var content = ScreenManager.Game.Content;
        _font = content.Load<SpriteFont>("Fonts/MenuFont");
        _smallFont = content.Load<SpriteFont>("Fonts/GameFont");

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        RefreshLists();
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshLists()
    {
        // Refresh buyable items (filter out sold-out items)
        _buyableItems = _shop.Inventory.Where(i => !i.IsLimited || i.Stock > 0).ToList();

        // Refresh sellable items from player inventory
        _sellableItems.Clear();

        // Group consumables
        var consumableCounts = new Dictionary<string, int>();
        foreach (var itemId in _gameState.Data.InventoryItems)
        {
            if (!consumableCounts.ContainsKey(itemId))
                consumableCounts[itemId] = 0;
            consumableCounts[itemId]++;
        }
        foreach (var kvp in consumableCounts)
        {
            if (_shop.BuyCategories.Contains(ShopCategory.Consumables))
                _sellableItems.Add((kvp.Key, ShopCategory.Consumables, kvp.Value));
        }

        // Add microchips (unequipped only)
        if (_shop.BuyCategories.Contains(ShopCategory.Microchips))
        {
            foreach (var chipId in _gameState.Data.OwnedMicrochips)
            {
                _sellableItems.Add((chipId, ShopCategory.Microchips, 1));
            }
        }

        // Add augmentations (unequipped only)
        if (_shop.BuyCategories.Contains(ShopCategory.Augmentations))
        {
            foreach (var augId in _gameState.Data.OwnedAugmentations)
            {
                _sellableItems.Add((augId, ShopCategory.Augmentations, 1));
            }
        }

        // Reset selection if out of bounds
        var currentList = _mode == TradingMode.Buy ? _buyableItems.Count : _sellableItems.Count;
        if (_selectedIndex >= currentList)
            _selectedIndex = Math.Max(0, currentList - 1);
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input == null) return;

        var keyboardState = Keyboard.GetState();

        // Exit
        if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            ExitScreen();
            return;
        }

        // Switch mode
        if (IsKeyPressed(keyboardState, Keys.Tab))
        {
            _mode = _mode == TradingMode.Buy ? TradingMode.Sell : TradingMode.Buy;
            _selectedIndex = 0;
            _scrollOffset = 0;
        }

        // Navigate
        int itemCount = _mode == TradingMode.Buy ? _buyableItems.Count : _sellableItems.Count;

        if (IsKeyPressed(keyboardState, Keys.Up) || IsKeyPressed(keyboardState, Keys.W))
        {
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
            UpdateScroll();
        }
        if (IsKeyPressed(keyboardState, Keys.Down) || IsKeyPressed(keyboardState, Keys.S))
        {
            _selectedIndex = Math.Min(itemCount - 1, _selectedIndex + 1);
            UpdateScroll();
        }

        // Buy/Sell
        if (IsKeyPressed(keyboardState, Keys.Enter) || IsKeyPressed(keyboardState, Keys.Space))
        {
            if (_mode == TradingMode.Buy)
                TryBuyItem();
            else
                TrySellItem();
        }

        _previousKeyboardState = keyboardState;
    }

    private bool IsKeyPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    private void UpdateScroll()
    {
        if (_selectedIndex < _scrollOffset)
            _scrollOffset = _selectedIndex;
        else if (_selectedIndex >= _scrollOffset + MaxVisibleItems)
            _scrollOffset = _selectedIndex - MaxVisibleItems + 1;
    }

    private void TryBuyItem()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _buyableItems.Count)
            return;

        var item = _buyableItems[_selectedIndex];
        int price = item.GetPrice(_factionReputation, _shop.Faction);

        if (_gameState.Currency < price)
        {
            // Not enough money - could show message
            return;
        }

        // Deduct currency
        _gameState.SpendCurrency(price);

        // Add item to inventory
        switch (item.Category)
        {
            case ShopCategory.Consumables:
                _gameState.AddItem(item.ItemId);
                break;
            case ShopCategory.Microchips:
                _gameState.AddMicrochip(item.ItemId);
                break;
            case ShopCategory.Augmentations:
                _gameState.AddAugmentation(item.ItemId);
                break;
        }

        // Reduce stock if limited
        if (item.IsLimited)
        {
            item.Stock--;
        }

        RefreshLists();
    }

    private void TrySellItem()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _sellableItems.Count)
            return;

        var (itemId, category, count) = _sellableItems[_selectedIndex];
        int sellPrice = ItemDatabase.GetBasePrice(itemId, category) / 2;

        // Add currency
        _gameState.AddCurrency(sellPrice);

        // Remove item from inventory
        switch (category)
        {
            case ShopCategory.Consumables:
                _gameState.RemoveItem(itemId);
                break;
            case ShopCategory.Microchips:
                _gameState.RemoveMicrochip(itemId);
                break;
            case ShopCategory.Augmentations:
                _gameState.RemoveAugmentation(itemId);
                break;
        }

        RefreshLists();
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

        // Background overlay
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.Black * 0.9f);

        // Shop header
        DrawHeader(spriteBatch, screenWidth);

        // Item list
        DrawItemList(spriteBatch, screenWidth, screenHeight);

        // Item details panel
        DrawItemDetails(spriteBatch, screenWidth, screenHeight);

        // Player currency
        DrawCurrency(spriteBatch, screenWidth, screenHeight);

        // Instructions
        DrawInstructions(spriteBatch, screenWidth, screenHeight);

        spriteBatch.End();
    }

    private void DrawHeader(SpriteBatch spriteBatch, float screenWidth)
    {
        // Shop name
        var titleSize = _font!.MeasureString(_shop.Name);
        spriteBatch.DrawString(_font, _shop.Name, new Vector2((screenWidth - titleSize.X) / 2, 15), Color.Yellow);

        // Owner greeting
        var greeting = $"\"{_shop.Greeting}\" - {_shop.OwnerName}";
        var greetSize = _smallFont!.MeasureString(greeting);
        spriteBatch.DrawString(_smallFont, greeting, new Vector2((screenWidth - greetSize.X) / 2, 45), Color.LightGray);

        // Faction price info
        if (_shop.Faction != FactionType.None)
        {
            float modifier = _factionReputation.GetPriceModifier(_shop.Faction);
            string priceInfo = modifier < 1f
                ? $"{_shop.Faction} reputation: {(1f - modifier) * 100:F0}% discount!"
                : modifier > 1f
                    ? $"{_shop.Faction} reputation: {(modifier - 1f) * 100:F0}% markup"
                    : $"{_shop.Faction} faction";

            Color priceColor = modifier < 1f ? Color.LimeGreen : modifier > 1f ? Color.Salmon : Color.Gray;
            var priceSize = _smallFont.MeasureString(priceInfo);
            spriteBatch.DrawString(_smallFont, priceInfo, new Vector2((screenWidth - priceSize.X) / 2, 65), priceColor);
        }

        // Mode tabs
        int tabY = 90;
        int buyTabX = (int)(screenWidth / 2 - 120);
        int sellTabX = (int)(screenWidth / 2 + 20);

        // Buy tab
        Color buyBg = _mode == TradingMode.Buy ? Color.DarkBlue : Color.DarkSlateGray;
        Color buyText = _mode == TradingMode.Buy ? Color.White : Color.Gray;
        spriteBatch.Draw(_pixelTexture, new Rectangle(buyTabX, tabY, 100, 25), buyBg);
        spriteBatch.DrawString(_smallFont, "Buy", new Vector2(buyTabX + 35, tabY + 3), buyText);

        // Sell tab
        Color sellBg = _mode == TradingMode.Sell ? Color.DarkBlue : Color.DarkSlateGray;
        Color sellText = _mode == TradingMode.Sell ? Color.White : Color.Gray;
        spriteBatch.Draw(_pixelTexture, new Rectangle(sellTabX, tabY, 100, 25), sellBg);
        spriteBatch.DrawString(_smallFont, "Sell", new Vector2(sellTabX + 35, tabY + 3), sellText);
    }

    private void DrawItemList(SpriteBatch spriteBatch, float screenWidth, float screenHeight)
    {
        int listX = 30;
        int listY = 125;
        int listWidth = (int)(screenWidth * 0.45f);
        int listHeight = (int)(screenHeight - 200);
        int itemHeight = 35;

        // List background
        spriteBatch.Draw(_pixelTexture, new Rectangle(listX, listY, listWidth, listHeight), new Color(30, 30, 40));
        DrawBorder(spriteBatch, new Rectangle(listX, listY, listWidth, listHeight), Color.Gray);

        if (_mode == TradingMode.Buy)
        {
            for (int i = 0; i < MaxVisibleItems && i + _scrollOffset < _buyableItems.Count; i++)
            {
                var item = _buyableItems[i + _scrollOffset];
                int y = listY + 5 + i * itemHeight;
                bool isSelected = i + _scrollOffset == _selectedIndex;

                DrawBuyItem(spriteBatch, item, listX + 5, y, listWidth - 10, itemHeight - 5, isSelected);
            }
        }
        else
        {
            for (int i = 0; i < MaxVisibleItems && i + _scrollOffset < _sellableItems.Count; i++)
            {
                var (itemId, category, count) = _sellableItems[i + _scrollOffset];
                int y = listY + 5 + i * itemHeight;
                bool isSelected = i + _scrollOffset == _selectedIndex;

                DrawSellItem(spriteBatch, itemId, category, count, listX + 5, y, listWidth - 10, itemHeight - 5, isSelected);
            }
        }

        // Scroll indicators
        int itemCount = _mode == TradingMode.Buy ? _buyableItems.Count : _sellableItems.Count;
        if (_scrollOffset > 0)
        {
            spriteBatch.DrawString(_smallFont!, "^", new Vector2(listX + listWidth - 15, listY + 5), Color.White);
        }
        if (_scrollOffset + MaxVisibleItems < itemCount)
        {
            spriteBatch.DrawString(_smallFont!, "v", new Vector2(listX + listWidth - 15, listY + listHeight - 20), Color.White);
        }

        // Empty message
        if (itemCount == 0)
        {
            string emptyMsg = _mode == TradingMode.Buy ? "Nothing for sale" : "Nothing to sell";
            var emptySize = _smallFont!.MeasureString(emptyMsg);
            spriteBatch.DrawString(_smallFont, emptyMsg,
                new Vector2(listX + (listWidth - emptySize.X) / 2, listY + listHeight / 2),
                Color.Gray);
        }
    }

    private void DrawBuyItem(SpriteBatch spriteBatch, ShopItem item, int x, int y, int width, int height, bool isSelected)
    {
        // Background
        Color bgColor = isSelected ? Color.DarkBlue * 0.7f : Color.Transparent;
        spriteBatch.Draw(_pixelTexture!, new Rectangle(x, y, width, height), bgColor);

        if (isSelected)
        {
            DrawBorder(spriteBatch, new Rectangle(x, y, width, height), Color.Yellow);
        }

        // Item name
        string name = ItemDatabase.GetItemName(item.ItemId, item.Category);
        Color nameColor = ItemDatabase.GetRarityColor(item.ItemId, item.Category);
        spriteBatch.DrawString(_smallFont!, name, new Vector2(x + 5, y + 3), nameColor);

        // Stock indicator
        if (item.IsLimited)
        {
            string stockText = $"x{item.Stock}";
            spriteBatch.DrawString(_smallFont!, stockText, new Vector2(x + width - 80, y + 3), Color.Orange);
        }

        // Price
        int price = item.GetPrice(_factionReputation, _shop.Faction);
        bool canAfford = _gameState.Currency >= price;
        string priceText = $"{price}c";
        var priceSize = _smallFont!.MeasureString(priceText);
        spriteBatch.DrawString(_smallFont, priceText,
            new Vector2(x + width - priceSize.X - 5, y + 3),
            canAfford ? Color.Gold : Color.Red);
    }

    private void DrawSellItem(SpriteBatch spriteBatch, string itemId, ShopCategory category, int count, int x, int y, int width, int height, bool isSelected)
    {
        // Background
        Color bgColor = isSelected ? Color.DarkBlue * 0.7f : Color.Transparent;
        spriteBatch.Draw(_pixelTexture!, new Rectangle(x, y, width, height), bgColor);

        if (isSelected)
        {
            DrawBorder(spriteBatch, new Rectangle(x, y, width, height), Color.Yellow);
        }

        // Item name
        string name = ItemDatabase.GetItemName(itemId, category);
        Color nameColor = ItemDatabase.GetRarityColor(itemId, category);
        spriteBatch.DrawString(_smallFont!, name, new Vector2(x + 5, y + 3), nameColor);

        // Count
        if (count > 1)
        {
            string countText = $"x{count}";
            spriteBatch.DrawString(_smallFont!, countText, new Vector2(x + width - 80, y + 3), Color.LightGray);
        }

        // Sell price
        int sellPrice = ItemDatabase.GetBasePrice(itemId, category) / 2;
        string priceText = $"+{sellPrice}c";
        var priceSize = _smallFont!.MeasureString(priceText);
        spriteBatch.DrawString(_smallFont, priceText,
            new Vector2(x + width - priceSize.X - 5, y + 3),
            Color.LimeGreen);
    }

    private void DrawItemDetails(SpriteBatch spriteBatch, float screenWidth, float screenHeight)
    {
        int detailX = (int)(screenWidth * 0.5f);
        int detailY = 125;
        int detailWidth = (int)(screenWidth * 0.45f);
        int detailHeight = (int)(screenHeight - 200);

        // Panel background
        spriteBatch.Draw(_pixelTexture!, new Rectangle(detailX, detailY, detailWidth, detailHeight), new Color(30, 30, 40));
        DrawBorder(spriteBatch, new Rectangle(detailX, detailY, detailWidth, detailHeight), Color.Gray);

        // Get selected item info
        string itemId = "";
        ShopCategory category = ShopCategory.Consumables;

        if (_mode == TradingMode.Buy && _selectedIndex >= 0 && _selectedIndex < _buyableItems.Count)
        {
            var item = _buyableItems[_selectedIndex];
            itemId = item.ItemId;
            category = item.Category;
        }
        else if (_mode == TradingMode.Sell && _selectedIndex >= 0 && _selectedIndex < _sellableItems.Count)
        {
            (itemId, category, _) = _sellableItems[_selectedIndex];
        }

        if (string.IsNullOrEmpty(itemId))
        {
            string noSelect = "Select an item";
            var noSelectSize = _smallFont!.MeasureString(noSelect);
            spriteBatch.DrawString(_smallFont, noSelect,
                new Vector2(detailX + (detailWidth - noSelectSize.X) / 2, detailY + detailHeight / 2),
                Color.Gray);
            return;
        }

        int yOffset = 10;

        // Item name
        string name = ItemDatabase.GetItemName(itemId, category);
        Color nameColor = ItemDatabase.GetRarityColor(itemId, category);
        spriteBatch.DrawString(_font!, name, new Vector2(detailX + 15, detailY + yOffset), nameColor);
        yOffset += 35;

        // Category
        spriteBatch.DrawString(_smallFont!, $"Type: {category}", new Vector2(detailX + 15, detailY + yOffset), Color.Gray);
        yOffset += 25;

        // Description
        string desc = ItemDatabase.GetItemDescription(itemId, category);
        DrawWrappedText(spriteBatch, desc, new Rectangle(detailX + 15, detailY + yOffset, detailWidth - 30, 100), Color.White);
        yOffset += 80;

        // Additional info based on category
        if (category == ShopCategory.Microchips)
        {
            var chip = Microchips.Get(itemId);
            if (chip != null)
            {
                spriteBatch.DrawString(_smallFont!, $"Category: {chip.Category}", new Vector2(detailX + 15, detailY + yOffset), Color.Cyan);
                yOffset += 20;

                if (chip.EnergyCost > 0)
                {
                    spriteBatch.DrawString(_smallFont!, $"Energy: {chip.EnergyCost}", new Vector2(detailX + 15, detailY + yOffset), Color.DeepSkyBlue);
                    yOffset += 20;
                }

                if (chip.HeatGenerated > 0)
                {
                    spriteBatch.DrawString(_smallFont!, $"Heat: {chip.HeatGenerated}/{chip.HeatMax}", new Vector2(detailX + 15, detailY + yOffset), Color.OrangeRed);
                    yOffset += 20;
                }

                if (chip.GrantsAbility != null)
                {
                    spriteBatch.DrawString(_smallFont!, $"Grants: {chip.GrantsAbility}", new Vector2(detailX + 15, detailY + yOffset), Color.Yellow);
                    yOffset += 20;
                }

                foreach (var mod in chip.StatModifiers)
                {
                    string modText = mod.IsPercent ? $"{mod.Stat}: +{mod.Value:F0}%" : $"{mod.Stat}: +{mod.Value:F0}";
                    spriteBatch.DrawString(_smallFont!, modText, new Vector2(detailX + 15, detailY + yOffset), Color.LimeGreen);
                    yOffset += 18;
                }
            }
        }
        else if (category == ShopCategory.Augmentations)
        {
            var aug = Augmentations.Get(itemId);
            if (aug != null)
            {
                spriteBatch.DrawString(_smallFont!, $"Slot: {aug.Slot}", new Vector2(detailX + 15, detailY + yOffset), Color.Cyan);
                yOffset += 20;

                foreach (var bonus in aug.StatBonuses)
                {
                    spriteBatch.DrawString(_smallFont!, $"{bonus.Key}: +{bonus.Value}", new Vector2(detailX + 15, detailY + yOffset), Color.LimeGreen);
                    yOffset += 18;
                }
            }
        }
        else if (category == ShopCategory.Consumables)
        {
            var consumable = Consumables.Get(itemId);
            if (consumable != null)
            {
                string effectText = consumable.Effect switch
                {
                    ConsumableEffect.HealHp => $"Heals {consumable.EffectPower} HP",
                    ConsumableEffect.HealHpPercent => $"Heals {consumable.EffectPower}% HP",
                    ConsumableEffect.RestoreEnergy => $"Restores {consumable.EffectPower} EP",
                    ConsumableEffect.ReduceStress => $"Reduces stress by {consumable.EffectPower}",
                    ConsumableEffect.ReviveKyn => $"Revives with {consumable.EffectPower}% HP",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(effectText))
                {
                    spriteBatch.DrawString(_smallFont!, effectText, new Vector2(detailX + 15, detailY + yOffset), Color.LimeGreen);
                    yOffset += 20;
                }

                // Usage info
                string usage = "";
                if (consumable.UsableInCombat && consumable.UsableOutOfCombat)
                    usage = "Usable anywhere";
                else if (consumable.UsableInCombat)
                    usage = "Combat only";
                else if (consumable.UsableOutOfCombat)
                    usage = "Out of combat only";

                spriteBatch.DrawString(_smallFont!, usage, new Vector2(detailX + 15, detailY + yOffset), Color.Gray);
            }
        }
    }

    private void DrawCurrency(SpriteBatch spriteBatch, float screenWidth, float screenHeight)
    {
        string currencyText = $"Currency: {_gameState.Currency}c";
        var currencySize = _font!.MeasureString(currencyText);
        spriteBatch.DrawString(_font, currencyText,
            new Vector2(screenWidth - currencySize.X - 20, screenHeight - 60),
            Color.Gold);
    }

    private void DrawInstructions(SpriteBatch spriteBatch, float screenWidth, float screenHeight)
    {
        string action = _mode == TradingMode.Buy ? "Buy" : "Sell";
        string instructions = $"[Up/Down] Navigate | [Enter] {action} | [Tab] Switch Mode | [ESC] Exit";
        var instrSize = _smallFont!.MeasureString(instructions);
        spriteBatch.DrawString(_smallFont, instructions,
            new Vector2((screenWidth - instrSize.X) / 2, screenHeight - 25),
            Color.Gray);
    }

    private void DrawWrappedText(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color)
    {
        if (string.IsNullOrEmpty(text)) return;

        var words = text.Split(' ');
        string currentLine = "";
        int y = bounds.Y;
        int lineHeight = 18;

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

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        int thickness = 2;
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y + bounds.Height - thickness, bounds.Width, thickness), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
        spriteBatch.Draw(_pixelTexture!, new Rectangle(bounds.X + bounds.Width - thickness, bounds.Y, thickness, bounds.Height), color);
    }
}
