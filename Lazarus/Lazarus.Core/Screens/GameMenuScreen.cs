using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Game.Items;
using Lazarus.Core.Game.Progression;
using Lazarus.Core.Game.Stats;
using Lazarus.Core.Game.World;
using Lazarus.Core.Inputs;
using Lazarus.Core.Localization;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Main menu tabs.
/// </summary>
public enum MenuTab
{
    Party,
    Inventory,
    Factions,
    Map,
    Ledger,
    Game
}

/// <summary>
/// Full-screen tabbed game menu accessible via ESC during gameplay.
/// </summary>
public class GameMenuScreen : GameScreen
{
    // Dependencies
    private readonly StrayRoster _roster;
    private readonly GameStateService _gameState;
    private readonly FactionReputation _factionReputation;
    private readonly GameWorld? _world;
    private readonly Bestiary _bestiary;

    // Graphics
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _titleFont;
    private SpriteFont? _font;
    private Texture2D? _pixel;

    // Tab state
    private MenuTab _currentTab = MenuTab.Party;
    private float _tabTransition = 0f;

    // Animation
    private float _pulseTimer;
    private float _transitionAlpha;

    // Party tab state
    private int _partySelectedIndex = 0;
    private bool _partyInRoster = false;
    private int _partyRosterScroll = 0;
    private bool _partySwapMode = false;
    private int _partySwapSource = -1;

    // Inventory tab state
    private InventoryTab _inventorySubTab = InventoryTab.Microchips;
    private int _inventorySelectedIndex = 0;
    private int _inventoryScroll = 0;

    // Factions tab state
    private int _factionSelectedIndex = 0;
    private FactionType[] _factions = Array.Empty<FactionType>();

    // Map tab state
    private BiomeType _mapSelectedBiome;
    private Dictionary<BiomeType, Vector2> _biomePositions = new();

    // Ledger tab state
    private int _ledgerEntryIndex = 0;
    private int _ledgerScroll = 0;
    private List<BestiaryEntry> _ledgerEntries = new();

    // Game tab state
    private int _gameOptionIndex = 0;
    private readonly string[] _gameOptions = { "Save Game", "Load Game", "Settings", "Quit to Menu" };

    // Layout constants
    private const int TabHeight = 40;
    private const int TabPadding = 15;
    private const int ContentPadding = 20;
    private const int MaxVisibleItems = 8;

    // Colors
    private static readonly Color BgColor = new(15, 18, 25);
    private static readonly Color PanelColor = new(25, 30, 40);
    private static readonly Color TabColor = new(35, 40, 55);
    private static readonly Color TabActiveColor = new(50, 70, 100);
    private static readonly Color SelectColor = new(60, 90, 140);
    private static readonly Color TextColor = new(220, 225, 235);
    private static readonly Color DimColor = new(120, 130, 150);
    private static readonly Color AccentColor = new(80, 160, 255);
    private static readonly Color WarningColor = new(255, 180, 80);

    public GameMenuScreen(
        StrayRoster roster,
        GameStateService gameState,
        FactionReputation factionReputation,
        GameWorld? world,
        Bestiary bestiary)
    {
        _roster = roster;
        _gameState = gameState;
        _factionReputation = factionReputation;
        _world = world;
        _bestiary = bestiary;

        TransitionOnTime = TimeSpan.FromSeconds(0.25);
        TransitionOffTime = TimeSpan.FromSeconds(0.15);

        // Initialize factions list
        _factions = Enum.GetValues<FactionType>()
            .Where(f => f != FactionType.None)
            .ToArray();

        // Initialize map
        if (_world != null)
        {
            _mapSelectedBiome = _world.CurrentBiome;
            InitializeBiomePositions();
        }

        // Initialize ledger
        RefreshLedger();
    }

    private void InitializeBiomePositions()
    {
        // Centered layout for 800x480 base
        float cx = 400, cy = 280;
        float spacing = 100;

        _biomePositions[BiomeType.Fringe] = new Vector2(cx, cy);
        _biomePositions[BiomeType.Rust] = new Vector2(cx - spacing, cy);
        _biomePositions[BiomeType.Green] = new Vector2(cx + spacing, cy);
        _biomePositions[BiomeType.Quiet] = new Vector2(cx, cy + spacing);
        _biomePositions[BiomeType.Teeth] = new Vector2(cx - spacing, cy + spacing);
        _biomePositions[BiomeType.Glow] = new Vector2(cx - spacing, cy + spacing * 1.8f);
        _biomePositions[BiomeType.ArchiveScar] = new Vector2(cx + spacing, cy - spacing);
    }

    private void RefreshLedger()
    {
        // Get ledger entries ordered by ledger number (catch order)
        _ledgerEntries = _bestiary.GetLedgerEntries().ToList();
        _ledgerEntryIndex = Math.Min(_ledgerEntryIndex, Math.Max(0, _ledgerEntries.Count - 1));
        _ledgerScroll = 0;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _spriteBatch = ScreenManager.SpriteBatch;
        _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");
        _font = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");

        _pixel = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public override void UnloadContent()
    {
        _pixel?.Dispose();
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        _transitionAlpha = 1f - TransitionPosition;
        _pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Smooth tab transition
        _tabTransition = MathHelper.Lerp(_tabTransition, (int)_currentTab, 0.2f);
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        // Global: ESC to close menu
        if (input.IsMenuCancel(ControllingPlayer, out _))
        {
            if (HandleTabCancel())
                return;

            ExitScreen();
            return;
        }

        // Tab switching with Q/E or shoulder buttons
        if (input.IsNewKeyPress(Keys.Q, ControllingPlayer, out _) ||
            input.IsNewButtonPress(Buttons.LeftShoulder, ControllingPlayer, out _))
        {
            SwitchTab(-1);
            return;
        }

        if (input.IsNewKeyPress(Keys.E, ControllingPlayer, out _) ||
            input.IsNewButtonPress(Buttons.RightShoulder, ControllingPlayer, out _))
        {
            SwitchTab(1);
            return;
        }

        // Tab-specific input
        switch (_currentTab)
        {
            case MenuTab.Party:
                HandlePartyInput(input);
                break;
            case MenuTab.Inventory:
                HandleInventoryInput(input);
                break;
            case MenuTab.Factions:
                HandleFactionsInput(input);
                break;
            case MenuTab.Map:
                HandleMapInput(input);
                break;
            case MenuTab.Ledger:
                HandleLedgerInput(input);
                break;
            case MenuTab.Game:
                HandleGameInput(input);
                break;
        }
    }

    private void SwitchTab(int direction)
    {
        int tabCount = Enum.GetValues<MenuTab>().Length;
        int newTab = ((int)_currentTab + direction + tabCount) % tabCount;
        _currentTab = (MenuTab)newTab;
    }

    private bool HandleTabCancel()
    {
        // Return true if cancel was handled within tab
        switch (_currentTab)
        {
            case MenuTab.Party:
                if (_partySwapMode)
                {
                    _partySwapMode = false;
                    _partySwapSource = -1;
                    return true;
                }
                if (_partyInRoster)
                {
                    _partyInRoster = false;
                    _partySelectedIndex = 0;
                    return true;
                }
                break;
        }
        return false;
    }

    #region Party Tab

    private void HandlePartyInput(InputState input)
    {
        int maxIndex = _partyInRoster
            ? _roster.Storage.Count - 1
            : Math.Max(0, _roster.Party.Count - 1);

        if (input.IsMenuUp(ControllingPlayer))
        {
            _partySelectedIndex = Math.Max(0, _partySelectedIndex - 1);
            UpdatePartyScroll();
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _partySelectedIndex = Math.Min(maxIndex, _partySelectedIndex + 1);
            UpdatePartyScroll();
        }
        else if (input.IsNewKeyPress(Keys.Left, ControllingPlayer, out _) && _partyInRoster)
        {
            _partyInRoster = false;
            _partySelectedIndex = 0;
        }
        else if (input.IsNewKeyPress(Keys.Right, ControllingPlayer, out _) && !_partyInRoster && _roster.Storage.Count > 0)
        {
            _partyInRoster = true;
            _partySelectedIndex = 0;
            _partyRosterScroll = 0;
        }
        else if (input.IsMenuSelect(ControllingPlayer, out _))
        {
            HandlePartySelect();
        }
        else if (input.IsNewKeyPress(Keys.P, ControllingPlayer, out _))
        {
            ToggleStrayPosition();
        }
    }

    private void UpdatePartyScroll()
    {
        if (_partyInRoster)
        {
            if (_partySelectedIndex < _partyRosterScroll)
                _partyRosterScroll = _partySelectedIndex;
            else if (_partySelectedIndex >= _partyRosterScroll + MaxVisibleItems)
                _partyRosterScroll = _partySelectedIndex - MaxVisibleItems + 1;
        }
    }

    private void HandlePartySelect()
    {
        if (_partyInRoster)
        {
            var stored = _roster.Storage.ToList();
            if (_partySelectedIndex < stored.Count)
            {
                var stray = stored[_partySelectedIndex];
                if (_partySwapMode && _partySwapSource >= 0)
                {
                    var partyMember = _roster.Party.ElementAtOrDefault(_partySwapSource);
                    if (partyMember != null)
                        _roster.SwapStrays(partyMember, stray);
                    _partySwapMode = false;
                    _partySwapSource = -1;
                }
                else if (_roster.Party.Count < 5)
                {
                    _roster.MoveToParty(stray);
                }
            }
        }
        else
        {
            var party = _roster.Party.ToList();
            if (_partySelectedIndex < party.Count)
            {
                if (_partySwapMode)
                {
                    if (_partySwapSource != _partySelectedIndex && _partySwapSource >= 0)
                    {
                        var source = party[_partySwapSource];
                        _roster.ReorderParty(source, _partySelectedIndex);
                    }
                    _partySwapMode = false;
                    _partySwapSource = -1;
                }
                else
                {
                    _partySwapMode = true;
                    _partySwapSource = _partySelectedIndex;
                }
            }
        }
    }

    private void ToggleStrayPosition()
    {
        Stray? stray = GetSelectedStray();
        if (stray != null)
        {
            stray.CombatRow = stray.CombatRow == CombatRow.Front ? CombatRow.Back : CombatRow.Front;
        }
    }

    private Stray? GetSelectedStray()
    {
        if (_partyInRoster)
        {
            var stored = _roster.Storage.ToList();
            return _partySelectedIndex < stored.Count ? stored[_partySelectedIndex] : null;
        }
        else
        {
            var party = _roster.Party.ToList();
            return _partySelectedIndex < party.Count ? party[_partySelectedIndex] : null;
        }
    }

    #endregion

    #region Inventory Tab

    private void HandleInventoryInput(InputState input)
    {
        var items = GetCurrentInventoryItems();
        int maxIndex = Math.Max(0, items.Count - 1);

        // Sub-tab switching with Left/Right
        if (input.IsNewKeyPress(Keys.Left, ControllingPlayer, out _))
        {
            _inventorySubTab = (InventoryTab)(((int)_inventorySubTab - 1 + 4) % 4);
            _inventorySelectedIndex = 0;
            _inventoryScroll = 0;
        }
        else if (input.IsNewKeyPress(Keys.Right, ControllingPlayer, out _))
        {
            _inventorySubTab = (InventoryTab)(((int)_inventorySubTab + 1) % 4);
            _inventorySelectedIndex = 0;
            _inventoryScroll = 0;
        }
        else if (input.IsMenuUp(ControllingPlayer))
        {
            _inventorySelectedIndex = Math.Max(0, _inventorySelectedIndex - 1);
            UpdateInventoryScroll();
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _inventorySelectedIndex = Math.Min(maxIndex, _inventorySelectedIndex + 1);
            UpdateInventoryScroll();
        }
    }

    private void UpdateInventoryScroll()
    {
        if (_inventorySelectedIndex < _inventoryScroll)
            _inventoryScroll = _inventorySelectedIndex;
        else if (_inventorySelectedIndex >= _inventoryScroll + MaxVisibleItems)
            _inventoryScroll = _inventorySelectedIndex - MaxVisibleItems + 1;
    }

    private List<string> GetCurrentInventoryItems()
    {
        return _inventorySubTab switch
        {
            InventoryTab.Microchips => _gameState.Data.OwnedMicrochips,
            InventoryTab.Augmentations => _gameState.Data.OwnedAugmentations,
            InventoryTab.Items => _gameState.Data.InventoryItems,
            InventoryTab.KeyItems => _gameState.Data.InventoryItems.Where(i => i.StartsWith("key_")).ToList(),
            _ => new List<string>()
        };
    }

    #endregion

    #region Factions Tab

    private void HandleFactionsInput(InputState input)
    {
        if (input.IsMenuUp(ControllingPlayer))
            _factionSelectedIndex = Math.Max(0, _factionSelectedIndex - 1);
        else if (input.IsMenuDown(ControllingPlayer))
            _factionSelectedIndex = Math.Min(_factions.Length - 1, _factionSelectedIndex + 1);
    }

    #endregion

    #region Map Tab

    private void HandleMapInput(InputState input)
    {
        if (_world == null) return;

        Vector2 dir = Vector2.Zero;
        if (input.IsNewKeyPress(Keys.Left, ControllingPlayer, out _)) dir.X = -1;
        else if (input.IsNewKeyPress(Keys.Right, ControllingPlayer, out _)) dir.X = 1;
        else if (input.IsNewKeyPress(Keys.Up, ControllingPlayer, out _)) dir.Y = -1;
        else if (input.IsNewKeyPress(Keys.Down, ControllingPlayer, out _)) dir.Y = 1;

        if (dir != Vector2.Zero)
            SelectNearestBiome(dir);

        if (input.IsMenuSelect(ControllingPlayer, out _))
            TryTravel();
    }

    private void SelectNearestBiome(Vector2 direction)
    {
        if (!_biomePositions.TryGetValue(_mapSelectedBiome, out var currentPos))
            return;

        BiomeType? nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var kvp in _biomePositions)
        {
            if (kvp.Key == _mapSelectedBiome) continue;
            if (!IsBiomeDiscovered(kvp.Key)) continue;

            var delta = kvp.Value - currentPos;
            bool inDir = (direction.X < 0 && delta.X < -20) ||
                         (direction.X > 0 && delta.X > 20) ||
                         (direction.Y < 0 && delta.Y < -20) ||
                         (direction.Y > 0 && delta.Y > 20);

            if (inDir && delta.Length() < nearestDist)
            {
                nearestDist = delta.Length();
                nearest = kvp.Key;
            }
        }

        if (nearest.HasValue)
            _mapSelectedBiome = nearest.Value;
    }

    private bool IsBiomeDiscovered(BiomeType biome)
    {
        if (biome == BiomeType.Fringe || biome == BiomeType.Rust || biome == BiomeType.Green)
            return true;

        return _gameState.HasFlag($"visited_{biome.ToString().ToLower()}") ||
               _gameState.HasFlag($"reached_{biome.ToString().ToLower()}");
    }

    private void TryTravel()
    {
        if (_world == null) return;
        if (_mapSelectedBiome == _world.CurrentBiome)
        {
            ExitScreen();
            return;
        }

        // Travel logic would go here - for now just exit
        ExitScreen();
    }

    #endregion

    #region Ledger Tab

    private void HandleLedgerInput(InputState input)
    {
        // Simple up/down navigation through the numbered list
        if (input.IsMenuUp(ControllingPlayer))
        {
            _ledgerEntryIndex = Math.Max(0, _ledgerEntryIndex - 1);
            UpdateLedgerScroll();
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            _ledgerEntryIndex = Math.Min(_ledgerEntries.Count - 1, _ledgerEntryIndex + 1);
            UpdateLedgerScroll();
        }
    }

    private void UpdateLedgerScroll()
    {
        if (_ledgerEntryIndex < _ledgerScroll)
            _ledgerScroll = _ledgerEntryIndex;
        else if (_ledgerEntryIndex >= _ledgerScroll + MaxVisibleItems)
            _ledgerScroll = _ledgerEntryIndex - MaxVisibleItems + 1;
    }

    #endregion

    #region Game Tab

    private void HandleGameInput(InputState input)
    {
        if (input.IsMenuUp(ControllingPlayer))
            _gameOptionIndex = Math.Max(0, _gameOptionIndex - 1);
        else if (input.IsMenuDown(ControllingPlayer))
            _gameOptionIndex = Math.Min(_gameOptions.Length - 1, _gameOptionIndex + 1);
        else if (input.IsMenuSelect(ControllingPlayer, out var playerIndex))
            ExecuteGameOption(playerIndex);
    }

    private void ExecuteGameOption(PlayerIndex playerIndex)
    {
        switch (_gameOptionIndex)
        {
            case 0: // Save
                var saveScreen = new SaveLoadScreen(SaveLoadMode.Save);
                ScreenManager.AddScreen(saveScreen, ControllingPlayer);
                break;
            case 1: // Load
                var loadScreen = new SaveLoadScreen(SaveLoadMode.Load);
                ScreenManager.AddScreen(loadScreen, ControllingPlayer);
                break;
            case 2: // Settings
                var settingsScreen = new SettingsMenuScreen();
                ScreenManager.AddScreen(settingsScreen, ControllingPlayer);
                break;
            case 3: // Quit
                ConfirmQuit();
                break;
        }
    }

    private void ConfirmQuit()
    {
        var confirm = new MessageBoxScreen(Resources.QuitQuestion);
        confirm.Accepted += (s, e) =>
        {
            var savePrompt = new MessageBoxScreen("Save before quitting?");
            savePrompt.Accepted += (s2, e2) =>
            {
                _gameState.Save(_gameState.Data.SaveSlot);
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
            };
            savePrompt.Cancelled += (s2, e2) =>
            {
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
            };
            ScreenManager.AddScreen(savePrompt, ControllingPlayer);
        };
        ScreenManager.AddScreen(confirm, ControllingPlayer);
    }

    #endregion

    #region Drawing

    public override void Draw(GameTime gameTime)
    {
        if (_spriteBatch == null || _font == null || _pixel == null) return;

        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var screenW = (int)ScreenManager.BaseScreenSize.X;
        var screenH = (int)ScreenManager.BaseScreenSize.Y;

        _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null,
            ScreenManager.GlobalTransformation);

        // Background
        DrawRect(new Rectangle(0, 0, screenW, screenH), BgColor * _transitionAlpha);

        // Tab bar
        DrawTabBar(screenW);

        // Content area
        var contentBounds = new Rectangle(ContentPadding, TabHeight + ContentPadding,
            screenW - ContentPadding * 2, screenH - TabHeight - ContentPadding * 2 - 30);

        switch (_currentTab)
        {
            case MenuTab.Party:
                DrawPartyTab(contentBounds);
                break;
            case MenuTab.Inventory:
                DrawInventoryTab(contentBounds);
                break;
            case MenuTab.Factions:
                DrawFactionsTab(contentBounds);
                break;
            case MenuTab.Map:
                DrawMapTab(contentBounds);
                break;
            case MenuTab.Ledger:
                DrawLedgerTab(contentBounds);
                break;
            case MenuTab.Game:
                DrawGameTab(contentBounds);
                break;
        }

        // Footer hints
        DrawFooter(screenW, screenH);

        _spriteBatch.End();
    }

    private void DrawTabBar(int screenW)
    {
        var tabs = Enum.GetValues<MenuTab>();
        int tabWidth = (screenW - TabPadding * 2) / tabs.Length;

        for (int i = 0; i < tabs.Length; i++)
        {
            var tab = tabs[i];
            var rect = new Rectangle(TabPadding + i * tabWidth, 5, tabWidth - 4, TabHeight - 10);
            bool isActive = tab == _currentTab;

            DrawRect(rect, (isActive ? TabActiveColor : TabColor) * _transitionAlpha);

            string label = tab switch
            {
                MenuTab.Party => "PARTY",
                MenuTab.Inventory => "INVENTORY",
                MenuTab.Factions => "FACTIONS",
                MenuTab.Map => "MAP",
                MenuTab.Ledger => "THE LEDGER",
                MenuTab.Game => "GAME",
                _ => tab.ToString()
            };

            var textSize = _font!.MeasureString(label);
            float scale = Math.Min(1f, (tabWidth - 10) / textSize.X);
            var textPos = new Vector2(rect.X + (rect.Width - textSize.X * scale) / 2,
                rect.Y + (rect.Height - textSize.Y * scale) / 2);

            _spriteBatch!.DrawString(_font, label, textPos,
                (isActive ? AccentColor : DimColor) * _transitionAlpha,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawFooter(int screenW, int screenH)
    {
        string hint = _currentTab switch
        {
            MenuTab.Party => _partySwapMode ? "[Enter] Swap target | [Esc] Cancel" :
                "[Up/Down] Select | [Left/Right] Panel | [Enter] Swap | [P] Position | [Q/E] Tab",
            MenuTab.Inventory => "[Up/Down] Select | [Left/Right] Category | [Q/E] Tab",
            MenuTab.Factions => "[Up/Down] Select | [Q/E] Tab",
            MenuTab.Map => "[Arrows] Navigate | [Enter] Travel | [Q/E] Tab",
            MenuTab.Ledger => "[Up/Down] Select | [Q/E] Tab",
            MenuTab.Game => "[Up/Down] Select | [Enter] Confirm | [Q/E] Tab",
            _ => "[Q/E] Switch Tab | [Esc] Close"
        };

        var hintSize = _font!.MeasureString(hint);
        _spriteBatch!.DrawString(_font, hint,
            new Vector2((screenW - hintSize.X) / 2, screenH - 25),
            DimColor * 0.7f * _transitionAlpha);
    }

    #region Party Tab Drawing

    private void DrawPartyTab(Rectangle bounds)
    {
        int halfWidth = (bounds.Width - ContentPadding) / 2;

        // Left panel: Active Party
        var partyBounds = new Rectangle(bounds.X, bounds.Y, halfWidth, bounds.Height);
        DrawPanel(partyBounds, !_partyInRoster);
        DrawPartyPanel(partyBounds);

        // Right panel: Storage
        var storageBounds = new Rectangle(bounds.X + halfWidth + ContentPadding, bounds.Y, halfWidth, bounds.Height);
        DrawPanel(storageBounds, _partyInRoster);
        DrawStoragePanel(storageBounds);
    }

    private void DrawPartyPanel(Rectangle bounds)
    {
        int y = bounds.Y + 10;
        _spriteBatch!.DrawString(_font!, $"Active Party ({_roster.Party.Count}/5)",
            new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
        y += 25;

        var party = _roster.Party.ToList();
        for (int i = 0; i < 5; i++)
        {
            var slotRect = new Rectangle(bounds.X + 5, y, bounds.Width - 10, 55);
            bool isSelected = !_partyInRoster && i == _partySelectedIndex;
            bool isSwapSource = _partySwapMode && i == _partySwapSource;

            if (i < party.Count)
            {
                var stray = party[i];
                DrawStraySlot(slotRect, stray, isSelected, isSwapSource);
            }
            else
            {
                DrawEmptySlot(slotRect, isSelected);
            }

            y += 60;
        }
    }

    private void DrawStoragePanel(Rectangle bounds)
    {
        int y = bounds.Y + 10;
        var stored = _roster.Storage.ToList();
        _spriteBatch!.DrawString(_font!, $"Storage ({stored.Count})",
            new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
        y += 25;

        if (stored.Count == 0)
        {
            _spriteBatch.DrawString(_font!, "No Strays in storage",
                new Vector2(bounds.X + 10, y + 20), DimColor * _transitionAlpha);
            return;
        }

        for (int i = 0; i < MaxVisibleItems && i + _partyRosterScroll < stored.Count; i++)
        {
            int idx = i + _partyRosterScroll;
            var stray = stored[idx];
            var slotRect = new Rectangle(bounds.X + 5, y, bounds.Width - 10, 40);
            bool isSelected = _partyInRoster && idx == _partySelectedIndex;

            DrawStraySlotCompact(slotRect, stray, isSelected);
            y += 45;
        }

        // Scroll indicators
        if (_partyRosterScroll > 0)
            _spriteBatch.DrawString(_font!, "^", new Vector2(bounds.Right - 20, bounds.Y + 25), AccentColor * _transitionAlpha);
        if (_partyRosterScroll + MaxVisibleItems < stored.Count)
            _spriteBatch.DrawString(_font!, "v", new Vector2(bounds.Right - 20, bounds.Bottom - 20), AccentColor * _transitionAlpha);
    }

    private void DrawStraySlot(Rectangle bounds, Stray stray, bool selected, bool swapSource)
    {
        Color bg = swapSource ? WarningColor * 0.3f : (selected ? SelectColor : PanelColor);
        DrawRect(bounds, bg * _transitionAlpha);
        DrawBorder(bounds, (swapSource ? WarningColor : (selected ? AccentColor : DimColor)) * _transitionAlpha);

        // Color indicator
        DrawRect(new Rectangle(bounds.X + 5, bounds.Y + 5, 25, 25), stray.Definition.PlaceholderColor * _transitionAlpha);

        // Position indicator
        string pos = stray.CombatRow == CombatRow.Back ? "BACK" : "FRONT";
        Color posColor = stray.CombatRow == CombatRow.Back ? new Color(100, 150, 255) : new Color(255, 150, 100);
        var posSize = _font!.MeasureString(pos);
        _spriteBatch!.DrawString(_font, pos, new Vector2(bounds.Right - posSize.X - 8, bounds.Y + 5), posColor * _transitionAlpha);

        // Name & level
        _spriteBatch.DrawString(_font, $"{stray.DisplayName} Lv.{stray.Level}",
            new Vector2(bounds.X + 35, bounds.Y + 5), TextColor * _transitionAlpha);

        // Type
        _spriteBatch.DrawString(_font, $"{stray.Definition.Category} / {stray.Definition.Role}",
            new Vector2(bounds.X + 35, bounds.Y + 20), DimColor * _transitionAlpha);

        // HP bar
        DrawHpBar(new Rectangle(bounds.X + 35, bounds.Y + 38, 100, 10), stray.CurrentHp, stray.MaxHp);
    }

    private void DrawStraySlotCompact(Rectangle bounds, Stray stray, bool selected)
    {
        DrawRect(bounds, (selected ? SelectColor : PanelColor) * _transitionAlpha);
        DrawBorder(bounds, (selected ? AccentColor : DimColor * 0.5f) * _transitionAlpha);

        DrawRect(new Rectangle(bounds.X + 5, bounds.Y + 5, 18, 18), stray.Definition.PlaceholderColor * _transitionAlpha);
        _spriteBatch!.DrawString(_font!, $"{stray.DisplayName} Lv.{stray.Level}",
            new Vector2(bounds.X + 28, bounds.Y + 5), TextColor * _transitionAlpha);
        DrawHpBar(new Rectangle(bounds.X + 28, bounds.Y + 22, 70, 8), stray.CurrentHp, stray.MaxHp);
    }

    private void DrawEmptySlot(Rectangle bounds, bool selected)
    {
        DrawRect(bounds, (selected ? SelectColor * 0.5f : PanelColor * 0.5f) * _transitionAlpha);
        DrawBorder(bounds, (selected ? AccentColor : DimColor * 0.3f) * _transitionAlpha);

        var text = "[Empty]";
        var size = _font!.MeasureString(text);
        _spriteBatch!.DrawString(_font, text,
            new Vector2(bounds.X + (bounds.Width - size.X) / 2, bounds.Y + (bounds.Height - size.Y) / 2),
            DimColor * 0.5f * _transitionAlpha);
    }

    private void DrawHpBar(Rectangle bounds, int current, int max)
    {
        DrawRect(bounds, new Color(40, 40, 50) * _transitionAlpha);
        float pct = max > 0 ? (float)current / max : 0;
        Color fill = pct > 0.5f ? Color.Green : (pct > 0.25f ? Color.Yellow : Color.Red);
        DrawRect(new Rectangle(bounds.X, bounds.Y, (int)(bounds.Width * pct), bounds.Height), fill * _transitionAlpha);
    }

    #endregion

    #region Inventory Tab Drawing

    private void DrawInventoryTab(Rectangle bounds)
    {
        // Sub-tabs
        int subTabWidth = bounds.Width / 4;
        int y = bounds.Y;

        var subTabs = new[] { "Microchips", "Augments", "Items", "Key Items" };
        for (int i = 0; i < 4; i++)
        {
            var rect = new Rectangle(bounds.X + i * subTabWidth, y, subTabWidth - 4, 25);
            bool active = (int)_inventorySubTab == i;
            DrawRect(rect, (active ? TabActiveColor : TabColor) * _transitionAlpha);
            var size = _font!.MeasureString(subTabs[i]);
            _spriteBatch!.DrawString(_font, subTabs[i],
                new Vector2(rect.X + (rect.Width - size.X) / 2, rect.Y + 3),
                (active ? AccentColor : DimColor) * _transitionAlpha);
        }

        y += 35;

        // Items list
        var items = GetCurrentInventoryItems();
        if (items.Count == 0)
        {
            _spriteBatch!.DrawString(_font!, "No items", new Vector2(bounds.X + 10, y + 20), DimColor * _transitionAlpha);
            return;
        }

        for (int i = 0; i < MaxVisibleItems && i + _inventoryScroll < items.Count; i++)
        {
            int idx = i + _inventoryScroll;
            var itemId = items[idx];
            var rect = new Rectangle(bounds.X + 5, y, bounds.Width - 10, 35);
            bool selected = idx == _inventorySelectedIndex;

            DrawRect(rect, (selected ? SelectColor : PanelColor) * _transitionAlpha);
            DrawBorder(rect, (selected ? AccentColor : DimColor * 0.3f) * _transitionAlpha);

            string displayName = GetItemDisplayName(itemId);
            _spriteBatch!.DrawString(_font!, displayName, new Vector2(rect.X + 10, rect.Y + 8), TextColor * _transitionAlpha);

            y += 40;
        }
    }

    private string GetItemDisplayName(string itemId)
    {
        // Try to get proper name from definitions
        if (_inventorySubTab == InventoryTab.Microchips)
        {
            var def = Microchips.Get(itemId);
            return def?.Name ?? itemId;
        }
        else if (_inventorySubTab == InventoryTab.Augmentations)
        {
            var def = Augmentations.Get(itemId);
            return def?.Name ?? itemId;
        }
        return itemId.Replace("_", " ");
    }

    #endregion

    #region Factions Tab Drawing

    private void DrawFactionsTab(Rectangle bounds)
    {
        int y = bounds.Y + 10;
        _spriteBatch!.DrawString(_titleFont!, "Faction Relations",
            new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
        y += 40;

        foreach (var faction in _factions)
        {
            int idx = Array.IndexOf(_factions, faction);
            var rect = new Rectangle(bounds.X + 5, y, bounds.Width - 10, 50);
            bool selected = idx == _factionSelectedIndex;

            DrawRect(rect, (selected ? SelectColor : PanelColor) * _transitionAlpha);
            DrawBorder(rect, (selected ? AccentColor : DimColor * 0.3f) * _transitionAlpha);

            // Faction name
            string name = faction.ToString();
            _spriteBatch.DrawString(_font!, name, new Vector2(rect.X + 10, rect.Y + 5), TextColor * _transitionAlpha);

            // Reputation bar
            int rep = _factionReputation.GetReputation(faction);
            var standing = _factionReputation.GetStanding(faction);
            DrawReputationBar(new Rectangle(rect.X + 10, rect.Y + 25, rect.Width - 100, 12), rep);

            // Standing text
            _spriteBatch.DrawString(_font!, standing.ToString(),
                new Vector2(rect.Right - 80, rect.Y + 22), GetStandingColor(standing) * _transitionAlpha);

            y += 55;
        }
    }

    private void DrawReputationBar(Rectangle bounds, int rep)
    {
        DrawRect(bounds, new Color(40, 40, 50) * _transitionAlpha);

        // Rep ranges from -1000 to 1000, center at 0
        float normalized = (rep + 1000) / 2000f;
        int centerX = bounds.X + bounds.Width / 2;

        // Draw center line
        DrawRect(new Rectangle(centerX - 1, bounds.Y, 2, bounds.Height), DimColor * _transitionAlpha);

        // Draw fill from center
        if (rep > 0)
        {
            int width = (int)(bounds.Width / 2 * (rep / 1000f));
            DrawRect(new Rectangle(centerX, bounds.Y, width, bounds.Height), Color.Green * _transitionAlpha);
        }
        else if (rep < 0)
        {
            int width = (int)(bounds.Width / 2 * (-rep / 1000f));
            DrawRect(new Rectangle(centerX - width, bounds.Y, width, bounds.Height), Color.Red * _transitionAlpha);
        }
    }

    private Color GetStandingColor(FactionStanding standing)
    {
        return standing switch
        {
            FactionStanding.Allied => new Color(100, 255, 100),
            FactionStanding.Friendly => new Color(150, 200, 150),
            FactionStanding.Neutral => Color.Gray,
            FactionStanding.Unfriendly => new Color(255, 200, 150),
            FactionStanding.Hostile => new Color(255, 100, 100),
            _ => Color.White
        };
    }

    #endregion

    #region Map Tab Drawing

    private void DrawMapTab(Rectangle bounds)
    {
        if (_world == null)
        {
            _spriteBatch!.DrawString(_font!, "Map unavailable", new Vector2(bounds.X + 10, bounds.Y + 20), DimColor * _transitionAlpha);
            return;
        }

        // Title
        _spriteBatch!.DrawString(_titleFont!, "World Map",
            new Vector2(bounds.X + 10, bounds.Y + 5), AccentColor * _transitionAlpha);

        // Draw connections
        DrawBiomeConnections(bounds);

        // Draw biome nodes
        foreach (var biome in _biomePositions.Keys)
        {
            DrawBiomeNode(biome);
        }

        // Travel info
        string info = _mapSelectedBiome == _world.CurrentBiome
            ? "You are here"
            : $"Selected: {BiomeData.GetName(_mapSelectedBiome)}";
        var infoSize = _font!.MeasureString(info);
        _spriteBatch.DrawString(_font, info, new Vector2(bounds.X + 10, bounds.Bottom - 25), TextColor * _transitionAlpha);
    }

    private void DrawBiomeConnections(Rectangle bounds)
    {
        var connections = new[]
        {
            (BiomeType.Fringe, BiomeType.Rust),
            (BiomeType.Fringe, BiomeType.Green),
            (BiomeType.Fringe, BiomeType.Quiet),
            (BiomeType.Rust, BiomeType.Teeth),
            (BiomeType.Teeth, BiomeType.Glow),
            (BiomeType.Green, BiomeType.ArchiveScar),
        };

        foreach (var (from, to) in connections)
        {
            if (!_biomePositions.TryGetValue(from, out var p1)) continue;
            if (!_biomePositions.TryGetValue(to, out var p2)) continue;

            bool visible = IsBiomeDiscovered(from) && IsBiomeDiscovered(to);
            Color lineColor = visible ? DimColor * 0.5f : DimColor * 0.15f;
            DrawLine(p1, p2, lineColor * _transitionAlpha, 2);
        }
    }

    private void DrawBiomeNode(BiomeType biome)
    {
        if (!_biomePositions.TryGetValue(biome, out var pos)) return;

        bool discovered = IsBiomeDiscovered(biome);
        bool selected = biome == _mapSelectedBiome;
        bool current = _world != null && biome == _world.CurrentBiome;

        int size = selected ? 55 : 50;
        if (selected) size += (int)(3 * Math.Sin(_pulseTimer * 4));

        var rect = new Rectangle((int)(pos.X - size / 2), (int)(pos.Y - size / 2), size, size);

        if (discovered)
        {
            var color = BiomeData.GetAccentColor(biome);
            DrawRect(rect, color * 0.6f * _transitionAlpha);
            DrawBorder(rect, (selected ? Color.Yellow : (current ? Color.Cyan : Color.White)) * _transitionAlpha);

            if (current)
            {
                DrawRect(new Rectangle(rect.Center.X - 4, rect.Center.Y - 4, 8, 8), Color.Cyan * _transitionAlpha);
            }

            string name = BiomeData.GetName(biome);
            var nameSize = _font!.MeasureString(name);
            _spriteBatch!.DrawString(_font, name, new Vector2(pos.X - nameSize.X / 2, rect.Bottom + 3), TextColor * _transitionAlpha);
        }
        else
        {
            DrawRect(rect, DimColor * 0.2f * _transitionAlpha);
            _spriteBatch!.DrawString(_font!, "?", new Vector2(pos.X - 4, pos.Y - 8), DimColor * _transitionAlpha);
        }
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness)
    {
        var delta = end - start;
        float length = delta.Length();
        float angle = (float)Math.Atan2(delta.Y, delta.X);

        _spriteBatch!.Draw(_pixel, start, null, color, angle, Vector2.Zero,
            new Vector2(length, thickness), SpriteEffects.None, 0);
    }

    #endregion

    #region Ledger Tab Drawing

    private void DrawLedgerTab(Rectangle bounds)
    {
        // Title with count (Pokédex style)
        string title = $"THE LEDGER - {_bestiary.LedgerCount} / {_bestiary.TotalEntries} Caught";
        _spriteBatch!.DrawString(_titleFont!, title, new Vector2(bounds.X + 10, bounds.Y + 5), AccentColor * _transitionAlpha);

        int y = bounds.Y + 40;

        // Entry list (left) and details (right)
        int listWidth = bounds.Width / 3;
        var listBounds = new Rectangle(bounds.X, y, listWidth, bounds.Height - (y - bounds.Y));
        var detailBounds = new Rectangle(bounds.X + listWidth + 10, y, bounds.Width - listWidth - 10, bounds.Height - (y - bounds.Y));

        DrawLedgerList(listBounds);
        DrawLedgerDetails(detailBounds);
    }

    private void DrawLedgerList(Rectangle bounds)
    {
        DrawPanel(bounds, true);

        if (_ledgerEntries.Count == 0)
        {
            _spriteBatch!.DrawString(_font!, "No Strays caught yet", new Vector2(bounds.X + 10, bounds.Y + 10), DimColor * _transitionAlpha);
            _spriteBatch.DrawString(_font!, "Recruit Strays to add", new Vector2(bounds.X + 10, bounds.Y + 30), DimColor * _transitionAlpha);
            _spriteBatch.DrawString(_font!, "them to your Ledger.", new Vector2(bounds.X + 10, bounds.Y + 50), DimColor * _transitionAlpha);
            return;
        }

        int y = bounds.Y + 5;
        for (int i = 0; i < MaxVisibleItems && i + _ledgerScroll < _ledgerEntries.Count; i++)
        {
            int idx = i + _ledgerScroll;
            var entry = _ledgerEntries[idx];
            bool selected = idx == _ledgerEntryIndex;

            var rect = new Rectangle(bounds.X + 3, y, bounds.Width - 6, 32);
            if (selected) DrawRect(rect, SelectColor * _transitionAlpha);

            // Ledger number (Pokédex style)
            string number = $"#{entry.LedgerNumber:D3}";
            _spriteBatch!.DrawString(_font!, number, new Vector2(rect.X + 5, rect.Y + 6),
                DimColor * _transitionAlpha);

            // Name
            string name = _bestiary.GetDisplayName(entry.DefinitionId);
            _spriteBatch.DrawString(_font!, name, new Vector2(rect.X + 50, rect.Y + 6),
                (selected ? Color.White : TextColor) * _transitionAlpha);

            y += 35;
        }

        // Scroll indicator
        if (_ledgerEntries.Count > MaxVisibleItems)
        {
            string scrollInfo = $"{_ledgerScroll + 1}-{Math.Min(_ledgerScroll + MaxVisibleItems, _ledgerEntries.Count)} of {_ledgerEntries.Count}";
            _spriteBatch!.DrawString(_font!, scrollInfo,
                new Vector2(bounds.X + 5, bounds.Y + bounds.Height - 20),
                DimColor * 0.7f * _transitionAlpha);
        }
    }

    private void DrawLedgerDetails(Rectangle bounds)
    {
        DrawPanel(bounds, false);

        if (_ledgerEntries.Count == 0 || _ledgerEntryIndex >= _ledgerEntries.Count)
        {
            _spriteBatch!.DrawString(_font!, "Catch Strays to view", new Vector2(bounds.X + 10, bounds.Y + 10), DimColor * _transitionAlpha);
            _spriteBatch.DrawString(_font!, "their details here.", new Vector2(bounds.X + 10, bounds.Y + 30), DimColor * _transitionAlpha);
            return;
        }

        var entry = _ledgerEntries[_ledgerEntryIndex];
        var def = StrayDefinitions.Get(entry.DefinitionId);
        int y = bounds.Y + 10;

        // Ledger number and name
        string header = $"#{entry.LedgerNumber:D3} - {_bestiary.GetDisplayName(entry.DefinitionId)}";
        _spriteBatch!.DrawString(_titleFont!, header, new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
        y += 35;

        // Status badge
        DrawRect(new Rectangle(bounds.X + 10, y, 90, 20), GetDiscoveryColor(entry.Status) * _transitionAlpha);
        _spriteBatch.DrawString(_font!, entry.Status.ToString(), new Vector2(bounds.X + 15, y + 2), Color.White * _transitionAlpha);
        y += 30;

        // Description
        string desc = _bestiary.GetDisplayDescription(entry.DefinitionId);
        DrawWrappedText(desc, bounds.X + 10, y, bounds.Width - 20, TextColor);
        y += 60;

        // Stats (if revealed)
        if (entry.StatsRevealed && def != null)
        {
            _spriteBatch.DrawString(_font!, "STATS", new Vector2(bounds.X + 10, y), AccentColor * _transitionAlpha);
            y += 20;

            _spriteBatch.DrawString(_font!, $"HP: {def.BaseStats.MaxHp}  ATK: {def.BaseStats.Attack}  DEF: {def.BaseStats.Defense}  SPD: {def.BaseStats.Speed}",
                new Vector2(bounds.X + 10, y), TextColor * _transitionAlpha);
            y += 25;
        }

        // Encounter data
        if (entry.EncounterCount > 0)
        {
            _spriteBatch.DrawString(_font!, $"Encountered: {entry.EncounterCount}x | Defeated: {entry.DefeatCount}x",
                new Vector2(bounds.X + 10, y), DimColor * _transitionAlpha);
        }
    }

    private Color GetDiscoveryColor(DiscoveryStatus status)
    {
        return status switch
        {
            DiscoveryStatus.Unknown => new Color(60, 60, 70),
            DiscoveryStatus.Sighted => new Color(150, 150, 100),
            DiscoveryStatus.Defeated => new Color(200, 150, 100),
            DiscoveryStatus.Recruited => new Color(100, 200, 100),
            DiscoveryStatus.Mastered => new Color(255, 200, 100),
            _ => Color.Gray
        };
    }

    private void DrawWrappedText(string text, int x, int y, int maxWidth, Color color)
    {
        var words = text.Split(' ');
        string line = "";
        int currentY = y;

        foreach (var word in words)
        {
            string test = line.Length > 0 ? line + " " + word : word;
            if (_font!.MeasureString(test).X > maxWidth)
            {
                _spriteBatch!.DrawString(_font, line, new Vector2(x, currentY), color * _transitionAlpha);
                line = word;
                currentY += 18;
            }
            else
            {
                line = test;
            }
        }
        if (line.Length > 0)
            _spriteBatch!.DrawString(_font!, line, new Vector2(x, currentY), color * _transitionAlpha);
    }

    #endregion

    #region Game Tab Drawing

    private void DrawGameTab(Rectangle bounds)
    {
        int centerX = bounds.X + bounds.Width / 2;
        int y = bounds.Y + 50;

        _spriteBatch!.DrawString(_titleFont!, "GAME OPTIONS",
            new Vector2(centerX - _titleFont!.MeasureString("GAME OPTIONS").X / 2, y),
            AccentColor * _transitionAlpha);
        y += 60;

        for (int i = 0; i < _gameOptions.Length; i++)
        {
            bool selected = i == _gameOptionIndex;
            var text = _gameOptions[i];
            var size = _font!.MeasureString(text);

            var rect = new Rectangle(centerX - 120, y, 240, 40);
            if (selected)
            {
                float pulse = 0.8f + 0.2f * (float)Math.Sin(_pulseTimer * 3);
                DrawRect(rect, SelectColor * pulse * _transitionAlpha);
            }
            DrawBorder(rect, (selected ? AccentColor : DimColor * 0.5f) * _transitionAlpha);

            _spriteBatch.DrawString(_font, text,
                new Vector2(centerX - size.X / 2, y + 10),
                (selected ? Color.White : TextColor) * _transitionAlpha);

            y += 50;
        }
    }

    #endregion

    private void DrawPanel(Rectangle bounds, bool highlighted)
    {
        DrawRect(bounds, PanelColor * _transitionAlpha);
        DrawBorder(bounds, (highlighted ? AccentColor : DimColor * 0.3f) * _transitionAlpha);
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch?.Draw(_pixel, rect, color);
    }

    private void DrawBorder(Rectangle rect, Color color, int thickness = 2)
    {
        _spriteBatch?.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        _spriteBatch?.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        _spriteBatch?.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        _spriteBatch?.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    #endregion
}
