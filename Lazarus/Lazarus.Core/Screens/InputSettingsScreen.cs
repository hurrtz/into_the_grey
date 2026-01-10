using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Inputs;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Screen for configuring input bindings and controller settings.
/// </summary>
public class InputSettingsScreen : GameScreen
{
    private readonly GamepadManager _gamepadManager;
    private readonly ControllerSettings _settings;

    private SpriteBatch? _spriteBatch;
    private SpriteFont? _titleFont;
    private SpriteFont? _contentFont;
    private Texture2D? _pixelTexture;

    // UI State
    private enum Tab { Bindings, Controller, Advanced }
    private Tab _currentTab = Tab.Bindings;

    private int _selectedIndex = 0;
    private int _scrollOffset = 0;
    private const int MAX_VISIBLE_ITEMS = 10;

    private bool _isRebinding = false;
    private GameAction? _rebindingAction = null;
    private float _rebindTimeout = 0f;
    private const float REBIND_TIMEOUT_SECONDS = 5f;

    // Binding categories
    private readonly List<BindingCategory> _categories = new();
    private List<BindingItem> _currentItems = new();

    // Animation
    private float _transitionAlpha = 0f;
    private float _selectionPulse = 0f;
    private float _rebindFlash = 0f;

    // Colors
    private static readonly Color BackgroundColor = new(20, 25, 35);
    private static readonly Color PanelColor = new(35, 40, 55);
    private static readonly Color TabActiveColor = new(60, 100, 160);
    private static readonly Color TabInactiveColor = new(40, 50, 70);
    private static readonly Color SelectedColor = new(50, 80, 130);
    private static readonly Color TextColor = new(220, 225, 235);
    private static readonly Color DimTextColor = new(140, 150, 170);
    private static readonly Color AccentColor = new(80, 160, 255);
    private static readonly Color WarningColor = new(255, 180, 80);
    private static readonly Color RebindColor = new(255, 100, 100);

    public InputSettingsScreen(GamepadManager gamepadManager, ControllerSettings settings)
    {
        _gamepadManager = gamepadManager;
        _settings = settings;

        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);

        InitializeCategories();
    }

    private void InitializeCategories()
    {
        _categories.Add(new BindingCategory("Movement", new[]
        {
            GameAction.MoveUp, GameAction.MoveDown, GameAction.MoveLeft, GameAction.MoveRight,
            GameAction.Sprint, GameAction.ToggleRun
        }));

        _categories.Add(new BindingCategory("Combat", new[]
        {
            GameAction.Attack, GameAction.Defend, GameAction.UseAbility,
            GameAction.SwitchTarget, GameAction.Flee
        }));

        _categories.Add(new BindingCategory("Menu", new[]
        {
            GameAction.MenuUp, GameAction.MenuDown, GameAction.MenuLeft, GameAction.MenuRight,
            GameAction.MenuSelect, GameAction.MenuCancel, GameAction.MenuPause
        }));

        _categories.Add(new BindingCategory("UI", new[]
        {
            GameAction.OpenInventory, GameAction.OpenMap, GameAction.OpenQuests,
            GameAction.OpenStrays, GameAction.OpenSettings
        }));

        _categories.Add(new BindingCategory("Quick Slots", new[]
        {
            GameAction.QuickItem1, GameAction.QuickItem2, GameAction.QuickItem3, GameAction.QuickItem4
        }));

        _categories.Add(new BindingCategory("Camera", new[]
        {
            GameAction.CameraZoomIn, GameAction.CameraZoomOut, GameAction.CameraReset
        }));

        _categories.Add(new BindingCategory("Misc", new[]
        {
            GameAction.Interact, GameAction.QuickSave, GameAction.QuickLoad
        }));

        RefreshBindingsList();
    }

    private void RefreshBindingsList()
    {
        _currentItems.Clear();

        foreach (var category in _categories)
        {
            _currentItems.Add(new BindingItem { IsHeader = true, HeaderText = category.Name });

            foreach (var action in category.Actions)
            {
                _currentItems.Add(new BindingItem
                {
                    Action = action,
                    IsHeader = false
                });
            }
        }
    }

    public override void LoadContent()
    {
        base.LoadContent();

        if (ScreenManager == null)
        {
            return;
        }

        _spriteBatch = ScreenManager.SpriteBatch;
        _contentFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
        _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public override void UnloadContent()
    {
        _pixelTexture?.Dispose();
        base.UnloadContent();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_isRebinding)
        {
            HandleRebindInput(input);

            return;
        }

        if (input.IsMenuCancel(ControllingPlayer, out _))
        {
            ExitScreen();

            return;
        }

        // Tab switching
        if (input.IsNewKeyPress(Keys.Q, ControllingPlayer, out _) ||
            input.IsNewButtonPress(Buttons.LeftShoulder, ControllingPlayer, out _))
        {
            _currentTab = _currentTab == Tab.Bindings ? Tab.Advanced : (Tab)((int)_currentTab - 1);
            _selectedIndex = 0;
            _scrollOffset = 0;
        }
        else if (input.IsNewKeyPress(Keys.E, ControllingPlayer, out _) ||
                 input.IsNewButtonPress(Buttons.RightShoulder, ControllingPlayer, out _))
        {
            _currentTab = _currentTab == Tab.Advanced ? Tab.Bindings : (Tab)((int)_currentTab + 1);
            _selectedIndex = 0;
            _scrollOffset = 0;
        }

        // Navigation
        if (input.IsMenuUp(ControllingPlayer))
        {
            NavigateUp();
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            NavigateDown();
        }

        // Selection
        if (input.IsMenuSelect(ControllingPlayer, out _))
        {
            HandleSelection();
        }

        // Left/Right for sliders
        if (_currentTab == Tab.Controller || _currentTab == Tab.Advanced)
        {
            if (input.IsNewKeyPress(Keys.Left, ControllingPlayer, out _) ||
                input.IsNewButtonPress(Buttons.DPadLeft, ControllingPlayer, out _))
            {
                AdjustValue(-0.1f);
            }
            else if (input.IsNewKeyPress(Keys.Right, ControllingPlayer, out _) ||
                     input.IsNewButtonPress(Buttons.DPadRight, ControllingPlayer, out _))
            {
                AdjustValue(0.1f);
            }
        }

        // Reset binding
        if (input.IsNewKeyPress(Keys.Delete, ControllingPlayer, out _) ||
            input.IsNewButtonPress(Buttons.Y, ControllingPlayer, out _))
        {
            if (_currentTab == Tab.Bindings)
            {
                ResetCurrentBinding();
            }
        }
    }

    private void HandleRebindInput(InputState input)
    {
        int idx = (int)(ControllingPlayer ?? PlayerIndex.One);

        // Check for cancel
        if (input.IsNewKeyPress(Keys.Escape, ControllingPlayer, out _))
        {
            CancelRebind();

            return;
        }

        // Check for keyboard input
        var keysPressed = input.CurrentKeyboardStates[idx].GetPressedKeys();

        if (keysPressed.Length > 0)
        {
            var key = keysPressed[0];

            // Ignore modifier keys
            if (key != Keys.LeftShift && key != Keys.RightShift &&
                key != Keys.LeftControl && key != Keys.RightControl &&
                key != Keys.LeftAlt && key != Keys.RightAlt &&
                key != Keys.Escape)
            {
                CompleteRebind(key);

                return;
            }
        }

        // Check for gamepad input
        if (input.CurrentGamePadStates[idx].IsConnected)
        {
            // Check buttons
            foreach (Buttons button in Enum.GetValues<Buttons>())
            {
                if (input.IsNewButtonPress(button, ControllingPlayer, out _))
                {
                    // Ignore guide button
                    if (button != Buttons.BigButton)
                    {
                        CompleteRebind(button);

                        return;
                    }
                }
            }
        }
    }

    private void NavigateUp()
    {
        if (_currentTab == Tab.Bindings)
        {
            do
            {
                _selectedIndex--;

                if (_selectedIndex < 0)
                {
                    _selectedIndex = _currentItems.Count - 1;
                }
            }
            while (_currentItems[_selectedIndex].IsHeader);
        }
        else
        {
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
        }

        UpdateScrollOffset();
    }

    private void NavigateDown()
    {
        if (_currentTab == Tab.Bindings)
        {
            do
            {
                _selectedIndex++;

                if (_selectedIndex >= _currentItems.Count)
                {
                    _selectedIndex = 0;
                }
            }
            while (_currentItems[_selectedIndex].IsHeader);
        }
        else
        {
            int maxIndex = _currentTab == Tab.Controller ? 8 : 5;
            _selectedIndex = Math.Min(maxIndex, _selectedIndex + 1);
        }

        UpdateScrollOffset();
    }

    private void UpdateScrollOffset()
    {
        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + MAX_VISIBLE_ITEMS)
        {
            _scrollOffset = _selectedIndex - MAX_VISIBLE_ITEMS + 1;
        }
    }

    private void HandleSelection()
    {
        if (_currentTab == Tab.Bindings && _selectedIndex >= 0 && _selectedIndex < _currentItems.Count)
        {
            var item = _currentItems[_selectedIndex];

            if (!item.IsHeader && item.Action.HasValue)
            {
                StartRebind(item.Action.Value);
            }
        }
        else if (_currentTab == Tab.Controller)
        {
            // Handle toggle settings
            switch (_selectedIndex)
            {
                case 4: // Invert Left Y
                    _settings.InvertLeftStickY = !_settings.InvertLeftStickY;
                    break;
                case 5: // Invert Right Y
                    _settings.InvertRightStickY = !_settings.InvertRightStickY;
                    break;
                case 6: // Vibration
                    _settings.VibrationEnabled = !_settings.VibrationEnabled;
                    break;
                case 7: // Aim Assist
                    _settings.AimAssistEnabled = !_settings.AimAssistEnabled;
                    break;
                case 8: // Swap Confirm/Cancel
                    _settings.SwapConfirmCancel = !_settings.SwapConfirmCancel;
                    break;
            }
        }
        else if (_currentTab == Tab.Advanced)
        {
            // Handle toggle settings
            switch (_selectedIndex)
            {
                case 5: // Reset All
                    ResetAllBindings();
                    break;
            }
        }
    }

    private void AdjustValue(float delta)
    {
        if (_currentTab == Tab.Controller)
        {
            switch (_selectedIndex)
            {
                case 0: // Left Stick Deadzone
                    _settings.LeftStickDeadzone = MathHelper.Clamp(_settings.LeftStickDeadzone + delta, 0.05f, 0.5f);
                    break;
                case 1: // Right Stick Deadzone
                    _settings.RightStickDeadzone = MathHelper.Clamp(_settings.RightStickDeadzone + delta, 0.05f, 0.5f);
                    break;
                case 2: // Trigger Deadzone
                    _settings.TriggerDeadzone = MathHelper.Clamp(_settings.TriggerDeadzone + delta, 0.05f, 0.5f);
                    break;
                case 3: // Stick Sensitivity
                    _settings.StickSensitivity = MathHelper.Clamp(_settings.StickSensitivity + delta, 0.5f, 2f);
                    break;
            }
        }
        else if (_currentTab == Tab.Advanced)
        {
            switch (_selectedIndex)
            {
                case 0: // Vibration Intensity
                    _settings.VibrationIntensity = MathHelper.Clamp(_settings.VibrationIntensity + delta, 0f, 1f);
                    break;
                case 1: // Aim Assist Strength
                    _settings.AimAssistStrength = MathHelper.Clamp(_settings.AimAssistStrength + delta, 0f, 1f);
                    break;
                case 2: // Input Buffer Window
                    _settings.InputBufferWindow = MathHelper.Clamp(_settings.InputBufferWindow + delta * 0.05f, 0.05f, 0.3f);
                    break;
            }
        }
    }

    private void StartRebind(GameAction action)
    {
        _isRebinding = true;
        _rebindingAction = action;
        _rebindTimeout = REBIND_TIMEOUT_SECONDS;
    }

    private void CancelRebind()
    {
        _isRebinding = false;
        _rebindingAction = null;
    }

    private void CompleteRebind(Keys key)
    {
        if (!_rebindingAction.HasValue)
        {
            return;
        }

        var binding = _gamepadManager.GetBinding(_rebindingAction.Value);

        if (binding != null)
        {
            binding.PrimaryKey = key;
            _gamepadManager.SetBinding(_rebindingAction.Value, binding);
        }

        _isRebinding = false;
        _rebindingAction = null;
    }

    private void CompleteRebind(Buttons button)
    {
        if (!_rebindingAction.HasValue)
        {
            return;
        }

        var binding = _gamepadManager.GetBinding(_rebindingAction.Value);

        if (binding != null)
        {
            binding.GamepadButton = button;
            _gamepadManager.SetBinding(_rebindingAction.Value, binding);
        }

        _isRebinding = false;
        _rebindingAction = null;
    }

    private void ResetCurrentBinding()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _currentItems.Count)
        {
            var item = _currentItems[_selectedIndex];

            if (!item.IsHeader && item.Action.HasValue)
            {
                _gamepadManager.ResetBinding(item.Action.Value);
            }
        }
    }

    private void ResetAllBindings()
    {
        _gamepadManager.ResetAllBindings();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _transitionAlpha = 1f - TransitionPosition;
        _selectionPulse = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 0.1f + 0.9f;
        _rebindFlash = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 8) * 0.3f + 0.7f;

        if (_isRebinding)
        {
            _rebindTimeout -= deltaTime;

            if (_rebindTimeout <= 0)
            {
                CancelRebind();
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null || _contentFont == null || _titleFont == null || _pixelTexture == null)
        {
            return;
        }

        var viewport = ScreenManager?.GraphicsDevice.Viewport ?? new Viewport(0, 0, 1280, 720);

        _spriteBatch.Begin();

        // Background
        DrawRect(new Rectangle(0, 0, viewport.Width, viewport.Height), BackgroundColor * _transitionAlpha);

        // Header
        DrawHeader(viewport);

        // Tabs
        DrawTabs(viewport);

        // Content
        var contentRect = new Rectangle(40, 140, viewport.Width - 80, viewport.Height - 200);
        DrawContent(contentRect);

        // Rebinding overlay
        if (_isRebinding)
        {
            DrawRebindOverlay(viewport);
        }

        // Footer
        DrawFooter(viewport);

        _spriteBatch.End();
    }

    private void DrawHeader(Viewport viewport)
    {
        string title = "INPUT SETTINGS";
        var titleSize = _titleFont!.MeasureString(title);
        _spriteBatch!.DrawString(_titleFont, title, new Vector2((viewport.Width - titleSize.X) / 2, 20), AccentColor * _transitionAlpha);

        // Controller status
        bool gamepadConnected = _gamepadManager.IsGamepadConnected();
        string status = gamepadConnected ? "Controller Connected" : "No Controller";
        Color statusColor = gamepadConnected ? Color.LightGreen : WarningColor;
        _spriteBatch.DrawString(_contentFont!, status, new Vector2(viewport.Width - 200, 30), statusColor * _transitionAlpha);
    }

    private void DrawTabs(Viewport viewport)
    {
        int tabWidth = 180;
        int tabHeight = 35;
        int startX = (viewport.Width - tabWidth * 3 - 20) / 2;
        int y = 70;

        string[] tabNames = { "Bindings", "Controller", "Advanced" };

        for (int i = 0; i < 3; i++)
        {
            bool isActive = (int)_currentTab == i;
            var tabRect = new Rectangle(startX + i * (tabWidth + 10), y, tabWidth, tabHeight);
            Color tabColor = isActive ? TabActiveColor * _selectionPulse : TabInactiveColor;

            DrawRect(tabRect, tabColor * _transitionAlpha);
            var textSize = _contentFont!.MeasureString(tabNames[i]);
            _spriteBatch!.DrawString(_contentFont, tabNames[i],
                new Vector2(tabRect.X + (tabRect.Width - textSize.X) / 2, tabRect.Y + (tabRect.Height - textSize.Y) / 2),
                (isActive ? Color.White : DimTextColor) * _transitionAlpha);
        }
    }

    private void DrawContent(Rectangle bounds)
    {
        DrawRect(bounds, PanelColor * _transitionAlpha);

        switch (_currentTab)
        {
            case Tab.Bindings:
                DrawBindingsTab(bounds);
                break;
            case Tab.Controller:
                DrawControllerTab(bounds);
                break;
            case Tab.Advanced:
                DrawAdvancedTab(bounds);
                break;
        }
    }

    private void DrawBindingsTab(Rectangle bounds)
    {
        int y = bounds.Y + 10;
        int itemHeight = 40;

        for (int i = _scrollOffset; i < Math.Min(_scrollOffset + MAX_VISIBLE_ITEMS, _currentItems.Count); i++)
        {
            var item = _currentItems[i];

            if (item.IsHeader)
            {
                _spriteBatch!.DrawString(_contentFont!, item.HeaderText ?? "",
                    new Vector2(bounds.X + 20, y + 10), AccentColor * _transitionAlpha);
            }
            else if (item.Action.HasValue)
            {
                bool isSelected = i == _selectedIndex;

                if (isSelected)
                {
                    DrawRect(new Rectangle(bounds.X + 10, y, bounds.Width - 20, itemHeight - 5),
                        SelectedColor * _selectionPulse * _transitionAlpha);
                }

                // Action name
                string actionName = FormatActionName(item.Action.Value);
                _spriteBatch!.DrawString(_contentFont!, actionName,
                    new Vector2(bounds.X + 30, y + 10),
                    (isSelected ? Color.White : TextColor) * _transitionAlpha);

                // Keyboard binding
                var binding = _gamepadManager.GetBinding(item.Action.Value);
                string keyText = binding?.PrimaryKey.ToString() ?? "None";
                _spriteBatch.DrawString(_contentFont!, keyText,
                    new Vector2(bounds.X + 300, y + 10), DimTextColor * _transitionAlpha);

                // Gamepad binding
                string buttonText = binding?.GamepadButton != 0
                    ? ControllerGlyphs.GetButtonName(binding!.GamepadButton, _settings.ControllerType)
                    : "-";
                _spriteBatch.DrawString(_contentFont!, buttonText,
                    new Vector2(bounds.X + 450, y + 10), DimTextColor * _transitionAlpha);
            }

            y += itemHeight;
        }

        // Column headers
        _spriteBatch!.DrawString(_contentFont!, "Keyboard",
            new Vector2(bounds.X + 300, bounds.Y - 25), DimTextColor * 0.7f * _transitionAlpha);
        _spriteBatch.DrawString(_contentFont!, "Gamepad",
            new Vector2(bounds.X + 450, bounds.Y - 25), DimTextColor * 0.7f * _transitionAlpha);
    }

    private void DrawControllerTab(Rectangle bounds)
    {
        int y = bounds.Y + 20;
        int itemHeight = 45;

        string[] labels = {
            "Left Stick Deadzone",
            "Right Stick Deadzone",
            "Trigger Deadzone",
            "Stick Sensitivity",
            "Invert Left Stick Y",
            "Invert Right Stick Y",
            "Vibration Enabled",
            "Aim Assist Enabled",
            "Swap Confirm/Cancel (Nintendo)"
        };

        float[] values = {
            _settings.LeftStickDeadzone,
            _settings.RightStickDeadzone,
            _settings.TriggerDeadzone,
            _settings.StickSensitivity
        };

        bool[] toggles = {
            _settings.InvertLeftStickY,
            _settings.InvertRightStickY,
            _settings.VibrationEnabled,
            _settings.AimAssistEnabled,
            _settings.SwapConfirmCancel
        };

        for (int i = 0; i < labels.Length; i++)
        {
            bool isSelected = i == _selectedIndex;

            if (isSelected)
            {
                DrawRect(new Rectangle(bounds.X + 10, y, bounds.Width - 20, itemHeight - 5),
                    SelectedColor * _selectionPulse * _transitionAlpha);
            }

            _spriteBatch!.DrawString(_contentFont!, labels[i],
                new Vector2(bounds.X + 30, y + 10),
                (isSelected ? Color.White : TextColor) * _transitionAlpha);

            if (i < 4)
            {
                // Slider
                DrawSlider(bounds.X + 400, y + 12, 200, values[i], i < 3 ? 0.5f : 2f);
                string valueText = i == 3 ? $"{values[i]:F1}x" : $"{values[i]:P0}";
                _spriteBatch.DrawString(_contentFont!, valueText,
                    new Vector2(bounds.X + 620, y + 10), DimTextColor * _transitionAlpha);
            }
            else
            {
                // Toggle
                bool isOn = toggles[i - 4];
                string toggleText = isOn ? "ON" : "OFF";
                Color toggleColor = isOn ? Color.LightGreen : WarningColor;
                _spriteBatch.DrawString(_contentFont!, toggleText,
                    new Vector2(bounds.X + 400, y + 10), toggleColor * _transitionAlpha);
            }

            y += itemHeight;
        }
    }

    private void DrawAdvancedTab(Rectangle bounds)
    {
        int y = bounds.Y + 20;
        int itemHeight = 45;

        string[] labels = {
            "Vibration Intensity",
            "Aim Assist Strength",
            "Input Buffer Window",
            "Controller Type",
            "",
            "Reset All Bindings"
        };

        for (int i = 0; i < labels.Length; i++)
        {
            if (string.IsNullOrEmpty(labels[i]))
            {
                y += itemHeight / 2;
                continue;
            }

            bool isSelected = i == _selectedIndex;

            if (isSelected)
            {
                DrawRect(new Rectangle(bounds.X + 10, y, bounds.Width - 20, itemHeight - 5),
                    SelectedColor * _selectionPulse * _transitionAlpha);
            }

            _spriteBatch!.DrawString(_contentFont!, labels[i],
                new Vector2(bounds.X + 30, y + 10),
                (isSelected ? Color.White : TextColor) * _transitionAlpha);

            switch (i)
            {
                case 0:
                    DrawSlider(bounds.X + 400, y + 12, 200, _settings.VibrationIntensity, 1f);
                    _spriteBatch.DrawString(_contentFont!, $"{_settings.VibrationIntensity:P0}",
                        new Vector2(bounds.X + 620, y + 10), DimTextColor * _transitionAlpha);
                    break;
                case 1:
                    DrawSlider(bounds.X + 400, y + 12, 200, _settings.AimAssistStrength, 1f);
                    _spriteBatch.DrawString(_contentFont!, $"{_settings.AimAssistStrength:P0}",
                        new Vector2(bounds.X + 620, y + 10), DimTextColor * _transitionAlpha);
                    break;
                case 2:
                    DrawSlider(bounds.X + 400, y + 12, 200, _settings.InputBufferWindow / 0.3f, 1f);
                    _spriteBatch.DrawString(_contentFont!, $"{_settings.InputBufferWindow * 1000:F0}ms",
                        new Vector2(bounds.X + 620, y + 10), DimTextColor * _transitionAlpha);
                    break;
                case 3:
                    _spriteBatch.DrawString(_contentFont!, _settings.ControllerType.ToString(),
                        new Vector2(bounds.X + 400, y + 10), AccentColor * _transitionAlpha);
                    break;
                case 5:
                    _spriteBatch.DrawString(_contentFont!, "[Press to Reset]",
                        new Vector2(bounds.X + 400, y + 10), WarningColor * _transitionAlpha);
                    break;
            }

            y += itemHeight;
        }
    }

    private void DrawSlider(int x, int y, int width, float value, float maxValue)
    {
        int height = 16;

        // Background
        DrawRect(new Rectangle(x, y, width, height), new Color(60, 60, 80) * _transitionAlpha);

        // Fill
        int fillWidth = (int)(width * (value / maxValue));
        DrawRect(new Rectangle(x, y, fillWidth, height), AccentColor * _transitionAlpha);

        // Handle
        int handleX = x + fillWidth - 4;
        DrawRect(new Rectangle(handleX, y - 2, 8, height + 4), Color.White * _transitionAlpha);
    }

    private void DrawRebindOverlay(Viewport viewport)
    {
        // Darken background
        DrawRect(new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.7f);

        // Center panel
        int panelWidth = 500;
        int panelHeight = 200;
        var panelRect = new Rectangle(
            (viewport.Width - panelWidth) / 2,
            (viewport.Height - panelHeight) / 2,
            panelWidth, panelHeight);

        DrawRect(panelRect, PanelColor);
        DrawRect(new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 4), RebindColor * _rebindFlash);

        // Title
        string title = "PRESS A KEY OR BUTTON";
        var titleSize = _titleFont!.MeasureString(title);
        _spriteBatch!.DrawString(_titleFont, title,
            new Vector2(panelRect.X + (panelRect.Width - titleSize.X) / 2, panelRect.Y + 30),
            RebindColor * _rebindFlash);

        // Action name
        if (_rebindingAction.HasValue)
        {
            string actionName = FormatActionName(_rebindingAction.Value);
            var actionSize = _contentFont!.MeasureString(actionName);
            _spriteBatch.DrawString(_contentFont, actionName,
                new Vector2(panelRect.X + (panelRect.Width - actionSize.X) / 2, panelRect.Y + 80),
                TextColor);
        }

        // Timeout
        string timeoutText = $"Timeout: {_rebindTimeout:F1}s";
        var timeoutSize = _contentFont!.MeasureString(timeoutText);
        _spriteBatch.DrawString(_contentFont, timeoutText,
            new Vector2(panelRect.X + (panelRect.Width - timeoutSize.X) / 2, panelRect.Y + 120),
            DimTextColor);

        // Cancel hint
        string cancelText = "Press ESC to cancel";
        var cancelSize = _contentFont!.MeasureString(cancelText);
        _spriteBatch.DrawString(_contentFont, cancelText,
            new Vector2(panelRect.X + (panelRect.Width - cancelSize.X) / 2, panelRect.Y + 160),
            DimTextColor);
    }

    private void DrawFooter(Viewport viewport)
    {
        string hints = _currentTab == Tab.Bindings
            ? "[Q/E] Tab  [Up/Down] Navigate  [Enter] Rebind  [Del/Y] Reset  [Esc] Back"
            : "[Q/E] Tab  [Up/Down] Navigate  [Left/Right] Adjust  [Enter] Toggle  [Esc] Back";

        var hintSize = _contentFont!.MeasureString(hints);
        _spriteBatch!.DrawString(_contentFont, hints,
            new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height - 35),
            DimTextColor * _transitionAlpha);
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch?.Draw(_pixelTexture, rect, color);
    }

    private static string FormatActionName(GameAction action)
    {
        string name = action.ToString();

        // Insert spaces before capital letters
        var result = new System.Text.StringBuilder();

        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
            {
                result.Append(' ');
            }

            result.Append(name[i]);
        }

        return result.ToString();
    }

    private class BindingCategory
    {
        public string Name { get; }
        public GameAction[] Actions { get; }

        public BindingCategory(string name, GameAction[] actions)
        {
            Name = name;
            Actions = actions;
        }
    }

    private class BindingItem
    {
        public bool IsHeader { get; set; }
        public string? HeaderText { get; set; }
        public GameAction? Action { get; set; }
    }
}
