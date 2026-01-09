using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Entities;
using Strays.Core.Game.Items;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Screen for equipping augmentations and microchips to a Stray.
/// </summary>
public class EquipmentScreen : GameScreen
{
    private readonly Stray _stray;
    private readonly GameStateService _gameState;
    private SpriteFont _font = null!;
    private SpriteFont _smallFont = null!;
    private Texture2D _pixelTexture = null!;

    // View state
    private enum Tab { Augmentations, Microchips }
    private Tab _currentTab = Tab.Augmentations;

    // Augmentation state
    private List<string> _augSlotKeys = new();
    private int _augSelectedIndex = 0;
    private bool _augPickerOpen = false;
    private int _augPickerIndex = 0;
    private List<string> _availableAugmentations = new();

    // Microchip state
    private int _chipSelectedIndex = 0;
    private bool _chipPickerOpen = false;
    private int _chipPickerIndex = 0;
    private List<string> _availableChipIds = new();

    public EquipmentScreen(Stray stray, GameStateService gameState)
    {
        _stray = stray;
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

        // Build list of augmentation slot keys
        _augSlotKeys.Clear();
        foreach (var slot in AugmentationSlotUtility.GetUniversalSlots())
        {
            _augSlotKeys.Add(new SlotReference(slot).ToKey());
        }
        foreach (var slot in AugmentationSlotUtility.GetCategorySlotsFor(_stray.Definition.Category))
        {
            _augSlotKeys.Add(new SlotReference(slot).ToKey());
        }
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        PlayerIndex playerIndex;

        if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            if (_augPickerOpen)
            {
                _augPickerOpen = false;
            }
            else if (_chipPickerOpen)
            {
                _chipPickerOpen = false;
            }
            else
            {
                ExitScreen();
            }
            return;
        }

        // Tab switching
        if (input.IsNewKeyPress(Keys.Tab, ControllingPlayer, out _) && !_augPickerOpen && !_chipPickerOpen)
        {
            _currentTab = _currentTab == Tab.Augmentations ? Tab.Microchips : Tab.Augmentations;
        }

        if (_currentTab == Tab.Augmentations)
        {
            HandleAugmentationInput(input);
        }
        else
        {
            HandleMicrochipInput(input);
        }
    }

    private void HandleAugmentationInput(InputState input)
    {
        if (_augPickerOpen)
        {
            // Navigate picker
            if (input.IsMenuUp(ControllingPlayer))
            {
                _augPickerIndex = Math.Max(0, _augPickerIndex - 1);
            }
            else if (input.IsMenuDown(ControllingPlayer))
            {
                _augPickerIndex = Math.Min(_availableAugmentations.Count, _augPickerIndex + 1);
            }
            else if (input.IsMenuSelect(ControllingPlayer, out _))
            {
                if (_augPickerIndex == 0)
                {
                    // Remove current augmentation
                    var slotKey = _augSlotKeys[_augSelectedIndex];
                    var slotRef = SlotReference.FromKey(slotKey);
                    if (slotRef.HasValue)
                    {
                        var currentAugId = _stray.GetEquippedAugmentationId(slotRef.Value);
                        if (currentAugId != null)
                        {
                            _stray.UnequipAugmentation(slotRef.Value);
                            _gameState.Data.OwnedAugmentations.Add(currentAugId);
                        }
                    }
                }
                else if (_augPickerIndex - 1 < _availableAugmentations.Count)
                {
                    // Equip selected augmentation
                    var augId = _availableAugmentations[_augPickerIndex - 1];
                    var slotKey = _augSlotKeys[_augSelectedIndex];
                    var slotRef = SlotReference.FromKey(slotKey);
                    if (slotRef.HasValue)
                    {
                        // Return current augmentation to inventory
                        var currentAugId = _stray.GetEquippedAugmentationId(slotRef.Value);
                        if (currentAugId != null)
                        {
                            _gameState.Data.OwnedAugmentations.Add(currentAugId);
                        }

                        // Remove from inventory and equip
                        _gameState.Data.OwnedAugmentations.Remove(augId);
                        _stray.EquipAugmentation(augId, slotRef.Value);
                    }
                }
                _augPickerOpen = false;
            }
        }
        else
        {
            // Navigate slots
            if (input.IsMenuUp(ControllingPlayer))
            {
                _augSelectedIndex = Math.Max(0, _augSelectedIndex - 1);
            }
            else if (input.IsMenuDown(ControllingPlayer))
            {
                _augSelectedIndex = Math.Min(_augSlotKeys.Count - 1, _augSelectedIndex + 1);
            }
            else if (input.IsMenuSelect(ControllingPlayer, out _))
            {
                // Open picker for this slot
                var slotKey = _augSlotKeys[_augSelectedIndex];
                var slotRef = SlotReference.FromKey(slotKey);
                if (slotRef.HasValue)
                {
                    // Find augmentations that fit this slot
                    _availableAugmentations = _gameState.Data.OwnedAugmentations
                        .Where(id => AugmentationCanFitSlot(id, slotRef.Value))
                        .Distinct()
                        .ToList();
                    _augPickerIndex = 0;
                    _augPickerOpen = true;
                }
            }
        }
    }

    private void HandleMicrochipInput(InputState input)
    {
        if (_chipPickerOpen)
        {
            // Navigate picker
            if (input.IsMenuUp(ControllingPlayer))
            {
                _chipPickerIndex = Math.Max(0, _chipPickerIndex - 1);
            }
            else if (input.IsMenuDown(ControllingPlayer))
            {
                _chipPickerIndex = Math.Min(_availableChipIds.Count, _chipPickerIndex + 1);
            }
            else if (input.IsMenuSelect(ControllingPlayer, out _))
            {
                if (_chipPickerIndex == 0)
                {
                    // Remove current chip
                    var socket = _stray.MicrochipSockets[_chipSelectedIndex];
                    if (socket.EquippedChip != null)
                    {
                        var chipDefId = socket.EquippedChip.Definition.Id;
                        _stray.UnequipMicrochip(_chipSelectedIndex);
                        _gameState.Data.OwnedMicrochips.Add(chipDefId);
                    }
                }
                else if (_chipPickerIndex - 1 < _availableChipIds.Count)
                {
                    // Equip selected chip
                    var chipDefId = _availableChipIds[_chipPickerIndex - 1];
                    var chipDef = Microchips.Get(chipDefId);
                    if (chipDef != null)
                    {
                        // Return current chip to inventory
                        var socket = _stray.MicrochipSockets[_chipSelectedIndex];
                        if (socket.EquippedChip != null)
                        {
                            _gameState.Data.OwnedMicrochips.Add(socket.EquippedChip.Definition.Id);
                        }

                        // Remove from inventory and equip
                        _gameState.Data.OwnedMicrochips.Remove(chipDefId);
                        var newChip = new Microchip(chipDef);
                        _stray.EquipMicrochip(newChip, _chipSelectedIndex);
                    }
                }
                _chipPickerOpen = false;
            }
        }
        else
        {
            // Navigate sockets
            if (input.IsMenuUp(ControllingPlayer))
            {
                _chipSelectedIndex = Math.Max(0, _chipSelectedIndex - 1);
            }
            else if (input.IsMenuDown(ControllingPlayer))
            {
                _chipSelectedIndex = Math.Min(_stray.MicrochipSockets.Length - 1, _chipSelectedIndex + 1);
            }
            else if (input.IsMenuSelect(ControllingPlayer, out _))
            {
                if (_chipSelectedIndex < _stray.MicrochipSockets.Length)
                {
                    // Build list of available chips
                    _availableChipIds = _gameState.Data.OwnedMicrochips.Distinct().ToList();
                    _chipPickerIndex = 0;
                    _chipPickerOpen = true;
                }
            }
        }
    }

    private bool AugmentationCanFitSlot(string augId, SlotReference slot)
    {
        var aug = Augmentations.Get(augId);
        if (aug == null) return false;

        // Check if augmentation's slot matches and is compatible with this Stray
        return aug.CanEquipToSlot(slot, _stray.Definition.Category);
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var viewport = ScreenManager.GraphicsDevice.Viewport;

        spriteBatch.Begin();

        // Draw semi-transparent background
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.9f);

        // Title
        string title = $"Equipment - {_stray.DisplayName}";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2((viewport.Width - titleSize.X) / 2, 15), Color.White);

        // Tab bar
        DrawTabBar(spriteBatch, new Rectangle(20, 50, viewport.Width - 40, 30));

        // Main content area
        var contentBounds = new Rectangle(20, 90, viewport.Width - 40, viewport.Height - 130);

        if (_currentTab == Tab.Augmentations)
        {
            DrawAugmentationsTab(spriteBatch, contentBounds);
        }
        else
        {
            DrawMicrochipsTab(spriteBatch, contentBounds);
        }

        // Instructions
        string instructions = _augPickerOpen || _chipPickerOpen
            ? "[Up/Down] Select | [Enter] Confirm | [ESC] Cancel"
            : "[Tab] Switch Tab | [Up/Down] Navigate | [Enter] Equip/Change | [ESC] Back";
        var instrSize = _smallFont.MeasureString(instructions);
        spriteBatch.DrawString(_smallFont, instructions, new Vector2((viewport.Width - instrSize.X) / 2, viewport.Height - 25), Color.Gray);

        spriteBatch.End();
    }

    private void DrawTabBar(SpriteBatch spriteBatch, Rectangle bounds)
    {
        int tabWidth = bounds.Width / 2;

        // Augmentations tab
        var augTabBounds = new Rectangle(bounds.X, bounds.Y, tabWidth, bounds.Height);
        bool augActive = _currentTab == Tab.Augmentations;
        spriteBatch.Draw(_pixelTexture, augTabBounds, augActive ? Color.DarkSlateBlue : Color.DarkSlateGray);
        DrawBorder(spriteBatch, augTabBounds, augActive ? Color.Cyan : Color.Gray);
        var augText = "Augmentations";
        var augTextSize = _smallFont.MeasureString(augText);
        spriteBatch.DrawString(_smallFont, augText,
            new Vector2(augTabBounds.X + (augTabBounds.Width - augTextSize.X) / 2, augTabBounds.Y + 5),
            augActive ? Color.White : Color.Gray);

        // Microchips tab
        var chipTabBounds = new Rectangle(bounds.X + tabWidth, bounds.Y, tabWidth, bounds.Height);
        bool chipActive = _currentTab == Tab.Microchips;
        spriteBatch.Draw(_pixelTexture, chipTabBounds, chipActive ? Color.DarkSlateBlue : Color.DarkSlateGray);
        DrawBorder(spriteBatch, chipTabBounds, chipActive ? Color.Cyan : Color.Gray);
        var chipText = "Microchips";
        var chipTextSize = _smallFont.MeasureString(chipText);
        spriteBatch.DrawString(_smallFont, chipText,
            new Vector2(chipTabBounds.X + (chipTabBounds.Width - chipTextSize.X) / 2, chipTabBounds.Y + 5),
            chipActive ? Color.White : Color.Gray);
    }

    private void DrawAugmentationsTab(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Left panel: slot list
        var slotListBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width / 2 - 10, bounds.Height);
        DrawSlotList(spriteBatch, slotListBounds);

        // Right panel: details or picker
        var detailBounds = new Rectangle(bounds.X + bounds.Width / 2 + 10, bounds.Y, bounds.Width / 2 - 10, bounds.Height);
        if (_augPickerOpen)
        {
            DrawAugmentationPicker(spriteBatch, detailBounds);
        }
        else
        {
            DrawSlotDetails(spriteBatch, detailBounds);
        }
    }

    private void DrawSlotList(SpriteBatch spriteBatch, Rectangle bounds)
    {
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.5f);
        DrawBorder(spriteBatch, bounds, Color.Gray);

        string header = "Augmentation Slots";
        spriteBatch.DrawString(_smallFont, header, new Vector2(bounds.X + 5, bounds.Y + 5), Color.Yellow);

        int y = bounds.Y + 30;
        int slotHeight = 26;

        // Universal slots header
        spriteBatch.DrawString(_smallFont, "Universal:", new Vector2(bounds.X + 5, y), Color.Cyan);
        y += 18;

        int universalCount = AugmentationSlotUtility.GetUniversalSlots().Count();

        for (int i = 0; i < _augSlotKeys.Count; i++)
        {
            if (i == universalCount)
            {
                y += 8;
                spriteBatch.DrawString(_smallFont, "Category:", new Vector2(bounds.X + 5, y), Color.Orange);
                y += 18;
            }

            var slotKey = _augSlotKeys[i];
            var slotRef = SlotReference.FromKey(slotKey);
            bool isSelected = i == _augSelectedIndex;

            var slotBounds = new Rectangle(bounds.X + 5, y, bounds.Width - 10, slotHeight - 2);
            if (isSelected)
            {
                spriteBatch.Draw(_pixelTexture, slotBounds, Color.DarkBlue * 0.6f);
            }

            string slotName = slotRef?.GetDisplayName() ?? slotKey;
            string equipped = _stray.EquippedAugmentations.TryGetValue(slotKey, out var augId) && augId != null
                ? GetAugmentationDisplayName(augId)
                : "[Empty]";

            Color slotColor = isSelected ? Color.White : Color.LightGray;
            Color equipColor = augId != null ? Color.LimeGreen : Color.Gray;

            // Truncate names if too long
            if (slotName.Length > 12) slotName = slotName[..11] + "..";
            if (equipped.Length > 15) equipped = equipped[..14] + "..";

            spriteBatch.DrawString(_smallFont, slotName, new Vector2(slotBounds.X + 2, slotBounds.Y + 2), slotColor);
            spriteBatch.DrawString(_smallFont, equipped, new Vector2(slotBounds.X + 110, slotBounds.Y + 2), equipColor);

            y += slotHeight;

            if (y > bounds.Y + bounds.Height - 20) break;
        }
    }

    private void DrawSlotDetails(SpriteBatch spriteBatch, Rectangle bounds)
    {
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.5f);
        DrawBorder(spriteBatch, bounds, Color.Gray);

        if (_augSelectedIndex >= 0 && _augSelectedIndex < _augSlotKeys.Count)
        {
            var slotKey = _augSlotKeys[_augSelectedIndex];
            var slotRef = SlotReference.FromKey(slotKey);

            string header = slotRef?.GetDisplayName() ?? "Unknown Slot";
            spriteBatch.DrawString(_font, header, new Vector2(bounds.X + 10, bounds.Y + 10), Color.Yellow);

            // Show slot description
            string desc = slotRef?.IsUniversal == true && slotRef?.UniversalSlot != null
                ? AugmentationSlotUtility.GetDescription(slotRef.Value.UniversalSlot!.Value)
                : "Category-specific slot";
            spriteBatch.DrawString(_smallFont, desc, new Vector2(bounds.X + 10, bounds.Y + 40), Color.Gray);

            // Show equipped augmentation details
            if (_stray.EquippedAugmentations.TryGetValue(slotKey, out var augId) && augId != null)
            {
                var aug = Augmentations.Get(augId);
                if (aug != null)
                {
                    spriteBatch.DrawString(_smallFont, "Equipped:", new Vector2(bounds.X + 10, bounds.Y + 80), Color.Cyan);
                    spriteBatch.DrawString(_font, aug.Name, new Vector2(bounds.X + 10, bounds.Y + 100), Color.White);

                    // Wrap description
                    var wrappedDesc = WrapText(aug.Description, bounds.Width - 20);
                    spriteBatch.DrawString(_smallFont, wrappedDesc, new Vector2(bounds.X + 10, bounds.Y + 130), Color.LightGray);

                    // Show stat bonuses
                    int bonusY = bounds.Y + 180;
                    foreach (var bonus in aug.StatBonuses)
                    {
                        string bonusText = $"+{bonus.Value} {bonus.Key}";
                        spriteBatch.DrawString(_smallFont, bonusText, new Vector2(bounds.X + 10, bonusY), Color.LimeGreen);
                        bonusY += 18;
                    }
                }
            }
            else
            {
                spriteBatch.DrawString(_smallFont, "No augmentation equipped", new Vector2(bounds.X + 10, bounds.Y + 80), Color.Gray);
                spriteBatch.DrawString(_smallFont, "Press Enter to equip", new Vector2(bounds.X + 10, bounds.Y + 100), Color.Yellow);
            }
        }
    }

    private void DrawAugmentationPicker(SpriteBatch spriteBatch, Rectangle bounds)
    {
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.8f);
        DrawBorder(spriteBatch, bounds, Color.Cyan);

        spriteBatch.DrawString(_smallFont, "Select Augmentation:", new Vector2(bounds.X + 10, bounds.Y + 5), Color.Yellow);

        int y = bounds.Y + 30;
        int itemHeight = 28;

        // Remove option
        bool removeSelected = _augPickerIndex == 0;
        var removeBounds = new Rectangle(bounds.X + 5, y, bounds.Width - 10, itemHeight - 2);
        if (removeSelected)
        {
            spriteBatch.Draw(_pixelTexture, removeBounds, Color.DarkRed * 0.5f);
        }
        spriteBatch.DrawString(_smallFont, "[Remove]", new Vector2(removeBounds.X + 5, removeBounds.Y + 5),
            removeSelected ? Color.Red : Color.Gray);
        y += itemHeight;

        // Available augmentations
        for (int i = 0; i < _availableAugmentations.Count; i++)
        {
            bool selected = _augPickerIndex == i + 1;
            var itemBounds = new Rectangle(bounds.X + 5, y, bounds.Width - 10, itemHeight - 2);

            if (selected)
            {
                spriteBatch.Draw(_pixelTexture, itemBounds, Color.DarkBlue * 0.6f);
            }

            var aug = Augmentations.Get(_availableAugmentations[i]);
            string name = aug?.Name ?? _availableAugmentations[i];
            spriteBatch.DrawString(_smallFont, name, new Vector2(itemBounds.X + 5, itemBounds.Y + 5),
                selected ? Color.White : Color.LightGray);

            y += itemHeight;
            if (y > bounds.Y + bounds.Height - 20) break;
        }

        if (_availableAugmentations.Count == 0)
        {
            spriteBatch.DrawString(_smallFont, "No augmentations available", new Vector2(bounds.X + 10, y), Color.Gray);
        }
    }

    private void DrawMicrochipsTab(SpriteBatch spriteBatch, Rectangle bounds)
    {
        // Left panel: socket list
        var socketListBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width / 2 - 10, bounds.Height);
        DrawSocketList(spriteBatch, socketListBounds);

        // Right panel: details or picker
        var detailBounds = new Rectangle(bounds.X + bounds.Width / 2 + 10, bounds.Y, bounds.Width / 2 - 10, bounds.Height);
        if (_chipPickerOpen)
        {
            DrawMicrochipPicker(spriteBatch, detailBounds);
        }
        else
        {
            DrawSocketDetails(spriteBatch, detailBounds);
        }
    }

    private void DrawSocketList(SpriteBatch spriteBatch, Rectangle bounds)
    {
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.5f);
        DrawBorder(spriteBatch, bounds, Color.Gray);

        string header = $"Microchip Sockets ({_stray.MicrochipSockets.Length})";
        spriteBatch.DrawString(_smallFont, header, new Vector2(bounds.X + 5, bounds.Y + 5), Color.Yellow);

        int y = bounds.Y + 30;
        int socketHeight = 50;

        for (int i = 0; i < _stray.MicrochipSockets.Length; i++)
        {
            var socket = _stray.MicrochipSockets[i];
            bool isSelected = i == _chipSelectedIndex;

            var socketBounds = new Rectangle(bounds.X + 5, y, bounds.Width - 10, socketHeight - 2);

            // Background
            Color bgColor = isSelected ? Color.DarkBlue * 0.6f : Color.DarkSlateGray * 0.3f;
            spriteBatch.Draw(_pixelTexture, socketBounds, bgColor);

            // Border
            Color borderColor = isSelected ? Color.Yellow : Color.Gray;
            DrawBorder(spriteBatch, socketBounds, borderColor);

            // Socket info
            string socketLabel = $"Socket {i + 1}";
            if (socket.IsLinked) socketLabel += $" (Link:{socket.LinkedSocketIndex + 1})";
            spriteBatch.DrawString(_smallFont, socketLabel, new Vector2(socketBounds.X + 5, socketBounds.Y + 3), Color.White);

            // Equipped chip
            if (socket.EquippedChip != null)
            {
                var chip = socket.EquippedChip;
                string chipName = $"{chip.Definition.Name} ({chip.FirmwareLevel})";
                spriteBatch.DrawString(_smallFont, chipName, new Vector2(socketBounds.X + 5, socketBounds.Y + 20), Color.LimeGreen);

                // Heat bar
                DrawHeatBar(spriteBatch, new Rectangle(socketBounds.X + 5, socketBounds.Y + 38, 100, 8), chip);
            }
            else
            {
                spriteBatch.DrawString(_smallFont, "[Empty]", new Vector2(socketBounds.X + 5, socketBounds.Y + 20), Color.Gray);
            }

            y += socketHeight;
            if (y > bounds.Y + bounds.Height - 20) break;
        }
    }

    private void DrawSocketDetails(SpriteBatch spriteBatch, Rectangle bounds)
    {
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.5f);
        DrawBorder(spriteBatch, bounds, Color.Gray);

        if (_chipSelectedIndex >= 0 && _chipSelectedIndex < _stray.MicrochipSockets.Length)
        {
            var socket = _stray.MicrochipSockets[_chipSelectedIndex];

            string header = $"Socket {_chipSelectedIndex + 1}";
            if (socket.IsLinked) header += $" (Linked to {socket.LinkedSocketIndex + 1})";
            spriteBatch.DrawString(_font, header, new Vector2(bounds.X + 10, bounds.Y + 10), Color.Yellow);

            if (socket.EquippedChip != null)
            {
                var chip = socket.EquippedChip;
                spriteBatch.DrawString(_smallFont, "Equipped:", new Vector2(bounds.X + 10, bounds.Y + 50), Color.Cyan);
                spriteBatch.DrawString(_font, chip.Definition.Name, new Vector2(bounds.X + 10, bounds.Y + 70), Color.White);

                var wrappedDesc = WrapText(chip.Definition.Description, bounds.Width - 20);
                spriteBatch.DrawString(_smallFont, wrappedDesc, new Vector2(bounds.X + 10, bounds.Y + 100), Color.LightGray);

                // Chip stats
                int statY = bounds.Y + 160;
                spriteBatch.DrawString(_smallFont, $"Firmware: {chip.FirmwareLevel}", new Vector2(bounds.X + 10, statY), Color.Cyan);
                statY += 18;

                int tuToNext = chip.Definition.GetTuToNextLevel(chip.FirmwareLevel);
                spriteBatch.DrawString(_smallFont, $"TU: {chip.CurrentTu}/{tuToNext}", new Vector2(bounds.X + 10, statY), Color.LimeGreen);
                statY += 18;

                spriteBatch.DrawString(_smallFont, $"Heat: {chip.CurrentHeat:F0}/{chip.Definition.HeatMax}", new Vector2(bounds.X + 10, statY), chip.IsOverheated ? Color.Red : Color.Orange);
                statY += 18;

                spriteBatch.DrawString(_smallFont, $"Energy Cost: {chip.Definition.EnergyCost}", new Vector2(bounds.X + 10, statY), Color.Yellow);

                // Ability granted
                if (!string.IsNullOrEmpty(chip.Definition.GrantsAbility))
                {
                    statY += 25;
                    spriteBatch.DrawString(_smallFont, "Grants Ability:", new Vector2(bounds.X + 10, statY), Color.Magenta);
                    statY += 18;
                    spriteBatch.DrawString(_smallFont, chip.Definition.GrantsAbility, new Vector2(bounds.X + 20, statY), Color.White);
                }
            }
            else
            {
                spriteBatch.DrawString(_smallFont, "No chip equipped", new Vector2(bounds.X + 10, bounds.Y + 50), Color.Gray);
                spriteBatch.DrawString(_smallFont, "Press Enter to equip", new Vector2(bounds.X + 10, bounds.Y + 70), Color.Yellow);
            }
        }
    }

    private void DrawMicrochipPicker(SpriteBatch spriteBatch, Rectangle bounds)
    {
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkSlateGray * 0.8f);
        DrawBorder(spriteBatch, bounds, Color.Cyan);

        spriteBatch.DrawString(_smallFont, "Select Microchip:", new Vector2(bounds.X + 10, bounds.Y + 5), Color.Yellow);

        int y = bounds.Y + 30;
        int itemHeight = 38;

        // Remove option
        bool removeSelected = _chipPickerIndex == 0;
        var removeBounds = new Rectangle(bounds.X + 5, y, bounds.Width - 10, itemHeight - 2);
        if (removeSelected)
        {
            spriteBatch.Draw(_pixelTexture, removeBounds, Color.DarkRed * 0.5f);
        }
        spriteBatch.DrawString(_smallFont, "[Remove]", new Vector2(removeBounds.X + 5, removeBounds.Y + 10),
            removeSelected ? Color.Red : Color.Gray);
        y += itemHeight;

        // Available chips
        for (int i = 0; i < _availableChipIds.Count; i++)
        {
            bool selected = _chipPickerIndex == i + 1;
            var itemBounds = new Rectangle(bounds.X + 5, y, bounds.Width - 10, itemHeight - 2);

            if (selected)
            {
                spriteBatch.Draw(_pixelTexture, itemBounds, Color.DarkBlue * 0.6f);
            }

            var chipDef = Microchips.Get(_availableChipIds[i]);
            if (chipDef != null)
            {
                spriteBatch.DrawString(_smallFont, chipDef.Name,
                    new Vector2(itemBounds.X + 5, itemBounds.Y + 3),
                    selected ? Color.White : Color.LightGray);

                // Truncate description
                string desc = chipDef.Description;
                if (desc.Length > 40) desc = desc[..37] + "...";
                spriteBatch.DrawString(_smallFont, desc,
                    new Vector2(itemBounds.X + 5, itemBounds.Y + 20),
                    Color.Gray);
            }

            y += itemHeight;
            if (y > bounds.Y + bounds.Height - 20) break;
        }

        if (_availableChipIds.Count == 0)
        {
            spriteBatch.DrawString(_smallFont, "No microchips in inventory", new Vector2(bounds.X + 10, y), Color.Gray);
        }
    }

    private void DrawHeatBar(SpriteBatch spriteBatch, Rectangle bounds, Microchip chip)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, bounds, Color.DarkGray);

        // Heat fill
        float percent = chip.Definition.HeatMax > 0 ? chip.CurrentHeat / chip.Definition.HeatMax : 0;
        Color heatColor = percent < 0.5f ? Color.Yellow :
                         percent < 0.8f ? Color.Orange : Color.Red;
        var fillRect = new Rectangle(bounds.X, bounds.Y, (int)(bounds.Width * Math.Min(1f, percent)), bounds.Height);
        spriteBatch.Draw(_pixelTexture, fillRect, heatColor);
    }

    private string GetAugmentationDisplayName(string augId)
    {
        var aug = Augmentations.Get(augId);
        return aug?.Name ?? augId;
    }

    private string WrapText(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return "";

        // Simple word wrap - estimate ~7 pixels per character
        int charsPerLine = maxWidth / 7;
        if (text.Length <= charsPerLine) return text;

        var result = new System.Text.StringBuilder();
        var words = text.Split(' ');
        int currentLineLength = 0;

        foreach (var word in words)
        {
            if (currentLineLength + word.Length + 1 > charsPerLine)
            {
                result.AppendLine();
                currentLineLength = 0;
            }
            else if (currentLineLength > 0)
            {
                result.Append(' ');
                currentLineLength++;
            }
            result.Append(word);
            currentLineLength += word.Length;
        }

        return result.ToString();
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        int t = 1;
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, t), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y + bounds.Height - t, bounds.Width, t), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, t, bounds.Height), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X + bounds.Width - t, bounds.Y, t, bounds.Height), color);
    }
}
