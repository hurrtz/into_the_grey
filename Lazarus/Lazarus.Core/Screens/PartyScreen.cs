using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Game.Stats;
using Lazarus.Core.Inputs;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// Screen for managing the player's Kyn party and roster.
/// </summary>
public class PartyScreen : GameScreen
{
    private readonly KynRoster _roster;
    private readonly GameStateService _gameState;
    private SpriteFont _font;
    private SpriteFont _smallFont;
    private Texture2D _pixelTexture;

    private int _selectedIndex = 0;
    private int _rosterScrollOffset = 0;
    private bool _inRosterView = false;
    private bool _swapMode = false;
    private int _swapSourceIndex = -1;
    private bool _showStatsDetail = false;
    private int _statsScrollOffset = 0;
    private StatCategory _selectedStatCategory = StatCategory.Tempo;

    private const int MaxVisibleRoster = 8;
    private const int PartySlots = 5;
    private const int MaxVisibleStats = 10;

    public PartyScreen(KynRoster roster, GameStateService gameState)
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

        // E key to open equipment screen for selected Kyn
        if (input.IsNewKeyPress(Keys.E, ControllingPlayer, out _) && !_swapMode)
        {
            OpenEquipmentScreen();
        }

        // P key to toggle combat position (Front/Back)
        if (input.IsNewKeyPress(Keys.P, ControllingPlayer, out _) && !_swapMode && !_showStatsDetail)
        {
            TogglePosition();
        }

        // S key to toggle stats detail view
        if (input.IsNewKeyPress(Keys.S, ControllingPlayer, out _) && !_swapMode)
        {
            _showStatsDetail = !_showStatsDetail;
            _statsScrollOffset = 0;
            _selectedStatCategory = StatCategory.Tempo;
        }

        // Stats view navigation
        if (_showStatsDetail)
        {
            // Q/A to change category
            if (input.IsNewKeyPress(Keys.Q, ControllingPlayer, out _))
            {
                int cat = (int)_selectedStatCategory - 1;
                if (cat < 0) cat = 8; // Wrap to StatusResistance
                _selectedStatCategory = (StatCategory)cat;
                _statsScrollOffset = 0;
            }
            if (input.IsNewKeyPress(Keys.A, ControllingPlayer, out _))
            {
                int cat = (int)_selectedStatCategory + 1;
                if (cat > 8) cat = 0; // Wrap to Tempo
                _selectedStatCategory = (StatCategory)cat;
                _statsScrollOffset = 0;
            }
        }
    }

    private void TogglePosition()
    {
        Kyn? selectedKyn = null;

        if (_inRosterView)
        {
            var storedKyns = _roster.Storage.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < storedKyns.Count)
            {
                selectedKyn = storedKyns[_selectedIndex];
            }
        }
        else
        {
            var partyList = _roster.Party.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < partyList.Count)
            {
                selectedKyn = partyList[_selectedIndex];
            }
        }

        if (selectedKyn != null)
        {
            selectedKyn.CombatRow = selectedKyn.CombatRow == CombatRow.Front
                ? CombatRow.Back
                : CombatRow.Front;
        }
    }

    private void OpenEquipmentScreen()
    {
        Kyn? selectedKyn = null;

        if (_inRosterView)
        {
            var storedKyns = _roster.Storage.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < storedKyns.Count)
            {
                selectedKyn = storedKyns[_selectedIndex];
            }
        }
        else
        {
            var partyList = _roster.Party.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < partyList.Count)
            {
                selectedKyn = partyList[_selectedIndex];
            }
        }

        if (selectedKyn != null)
        {
            var equipmentScreen = new EquipmentScreen(selectedKyn, _gameState);
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
            var storedKyns = _roster.Storage.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < storedKyns.Count)
            {
                var kyn = storedKyns[_selectedIndex];

                if (_swapMode && _swapSourceIndex >= 0)
                {
                    // Complete swap: move party member to storage, storage member to party
                    var partyMember = _roster.Party.ElementAtOrDefault(_swapSourceIndex);
                    if (partyMember != null)
                    {
                        _roster.SwapKyns(partyMember, kyn);
                    }
                    _swapMode = false;
                    _swapSourceIndex = -1;
                }
                else
                {
                    // Add to party if there's room
                    if (_roster.Party.Count < PartySlots)
                    {
                        _roster.MoveToParty(kyn);
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
        string title = _showStatsDetail ? "Kyn Stats" : "Party Management";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2((viewport.Width - titleSize.X) / 2, 20), Color.White);

        if (_showStatsDetail)
        {
            // Stats detail view
            DrawStatsDetailView(spriteBatch, viewport);
        }
        else
        {
            // Draw party panel (left side)
            DrawPartyPanel(spriteBatch, new Rectangle(20, 70, viewport.Width / 2 - 30, viewport.Height - 120));

            // Draw roster panel (right side)
            DrawRosterPanel(spriteBatch, new Rectangle(viewport.Width / 2 + 10, 70, viewport.Width / 2 - 30, viewport.Height - 120));
        }

        // Draw instructions
        string instructions = _showStatsDetail
            ? "[Q/A] Category | [S] Close | [ESC] Back"
            : (_swapMode
                ? "Select target to swap | [ESC] Cancel"
                : "[Arrows] Navigate | [Enter] Swap | [E] Equip | [S] Stats | [P] Position | [Tab] Panel | [ESC] Back");
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
                var kyn = partyList[i];
                bool isBackRow = kyn.CombatRow == CombatRow.Back;
                int indent = isBackRow ? backRowIndent : 0;

                var slotBounds = new Rectangle(bounds.X + 5 + indent, bounds.Y + yOffset + i * 70, bounds.Width - 10 - indent, 65);

                bool isSelected = !_inRosterView && i == _selectedIndex;
                bool isSwapSource = _swapMode && i == _swapSourceIndex;

                DrawKynSlot(spriteBatch, slotBounds, kyn, isSelected, isSwapSource);
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
        var storedKyns = _roster.Storage.ToList();
        string title = $"Kyn Roster ({storedKyns.Count})";
        spriteBatch.DrawString(_smallFont, title, new Vector2(bounds.X + 10, bounds.Y + 5), Color.Yellow);

        if (storedKyns.Count == 0)
        {
            string emptyText = "No Kyns in storage";
            var textSize = _smallFont.MeasureString(emptyText);
            spriteBatch.DrawString(_smallFont, emptyText,
                new Vector2(bounds.X + (bounds.Width - textSize.X) / 2, bounds.Y + bounds.Height / 2),
                Color.Gray);
            return;
        }

        // Draw visible roster entries
        int yOffset = 35;
        for (int i = 0; i < MaxVisibleRoster && i + _rosterScrollOffset < storedKyns.Count; i++)
        {
            int actualIndex = i + _rosterScrollOffset;
            var kyn = storedKyns[actualIndex];
            var slotBounds = new Rectangle(bounds.X + 5, bounds.Y + yOffset + i * 50, bounds.Width - 10, 45);

            bool isSelected = _inRosterView && actualIndex == _selectedIndex;
            DrawKynSlotCompact(spriteBatch, slotBounds, kyn, isSelected);
        }

        // Draw scroll indicators
        if (_rosterScrollOffset > 0)
        {
            spriteBatch.DrawString(_smallFont, "^", new Vector2(bounds.X + bounds.Width - 20, bounds.Y + 30), Color.White);
        }
        if (_rosterScrollOffset + MaxVisibleRoster < storedKyns.Count)
        {
            spriteBatch.DrawString(_smallFont, "v", new Vector2(bounds.X + bounds.Width - 20, bounds.Y + bounds.Height - 20), Color.White);
        }
    }

    private void DrawKynSlot(SpriteBatch spriteBatch, Rectangle bounds, Kyn kyn, bool isSelected, bool isSwapSource)
    {
        // Background
        Color bgColor = isSwapSource ? Color.DarkOrange * 0.5f : (isSelected ? Color.DarkBlue * 0.5f : Color.DarkSlateGray * 0.3f);
        spriteBatch.Draw(_pixelTexture, bounds, bgColor);

        // Border
        Color borderColor = isSwapSource ? Color.Orange : (isSelected ? Color.Yellow : Color.Gray);
        DrawBorder(spriteBatch, bounds, borderColor);

        // Position indicator (Front/Back)
        bool isBackRow = kyn.CombatRow == CombatRow.Back;
        string posIndicator = isBackRow ? "BACK" : "FRONT";
        Color posColor = isBackRow ? Color.CornflowerBlue : Color.OrangeRed;
        spriteBatch.DrawString(_smallFont, posIndicator, new Vector2(bounds.X + bounds.Width - 45, bounds.Y + 5), posColor);

        // Kyn placeholder color
        var colorRect = new Rectangle(bounds.X + 5, bounds.Y + 5, 30, 30);
        spriteBatch.Draw(_pixelTexture, colorRect, kyn.Definition.PlaceholderColor);

        // Name and level
        string nameText = $"{kyn.DisplayName} Lv.{kyn.Level}";
        spriteBatch.DrawString(_smallFont, nameText, new Vector2(bounds.X + 45, bounds.Y + 5), Color.White);

        // Category and role
        string typeText = $"{kyn.Definition.Category} / {kyn.Definition.Role}";
        spriteBatch.DrawString(_smallFont, typeText, new Vector2(bounds.X + 45, bounds.Y + 22), Color.LightGray);

        // HP bar
        DrawHpBar(spriteBatch, new Rectangle(bounds.X + 45, bounds.Y + 42, 120, 12), kyn.CurrentHp, kyn.MaxHp);

        // Stats summary
        string statsText = $"ATK:{kyn.Attack} DEF:{kyn.Defense} SPD:{kyn.Speed}";
        spriteBatch.DrawString(_smallFont, statsText, new Vector2(bounds.X + 180, bounds.Y + 22), Color.Gray);
    }

    private void DrawKynSlotCompact(SpriteBatch spriteBatch, Rectangle bounds, Kyn kyn, bool isSelected)
    {
        // Background
        Color bgColor = isSelected ? Color.DarkBlue * 0.5f : Color.DarkSlateGray * 0.3f;
        spriteBatch.Draw(_pixelTexture, bounds, bgColor);

        // Border
        DrawBorder(spriteBatch, bounds, isSelected ? Color.Yellow : Color.Gray);

        // Kyn placeholder color
        var colorRect = new Rectangle(bounds.X + 5, bounds.Y + 5, 20, 20);
        spriteBatch.Draw(_pixelTexture, colorRect, kyn.Definition.PlaceholderColor);

        // Name and level
        string nameText = $"{kyn.DisplayName} Lv.{kyn.Level}";
        spriteBatch.DrawString(_smallFont, nameText, new Vector2(bounds.X + 30, bounds.Y + 5), Color.White);

        // HP bar
        DrawHpBar(spriteBatch, new Rectangle(bounds.X + 30, bounds.Y + 25, 80, 8), kyn.CurrentHp, kyn.MaxHp);

        // Creature type
        string typeText = kyn.Definition.CreatureType.ToString();
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

    private void DrawStatsDetailView(SpriteBatch spriteBatch, Viewport viewport)
    {
        // Get selected Kyn
        Kyn? selectedKyn = GetSelectedKyn();

        if (selectedKyn == null)
        {
            string noSelection = "No Kyn selected";
            var textSize = _font.MeasureString(noSelection);
            spriteBatch.DrawString(_font, noSelection,
                new Vector2((viewport.Width - textSize.X) / 2, viewport.Height / 2),
                Color.Gray);
            return;
        }

        int panelWidth = viewport.Width - 40;
        int panelHeight = viewport.Height - 140;
        var panelBounds = new Rectangle(20, 70, panelWidth, panelHeight);

        // Panel background
        spriteBatch.Draw(_pixelTexture, panelBounds, Color.DarkSlateGray * 0.6f);
        DrawBorder(spriteBatch, panelBounds, Color.Cyan);

        // Kyn header info
        int yPos = panelBounds.Y + 10;
        string headerText = $"{selectedKyn.DisplayName} (Lv.{selectedKyn.Level}) - {selectedKyn.Definition.Category} / {selectedKyn.Definition.Role}";
        spriteBatch.DrawString(_smallFont, headerText, new Vector2(panelBounds.X + 10, yPos), Color.White);
        yPos += 25;

        // Category tabs
        DrawCategoryTabs(spriteBatch, new Rectangle(panelBounds.X + 10, yPos, panelWidth - 20, 25));
        yPos += 35;

        // Column headers
        int col1 = panelBounds.X + 20;
        int col2 = panelBounds.X + 180;
        int col3 = panelBounds.X + 260;
        int col4 = panelBounds.X + 340;
        int col5 = panelBounds.X + 420;

        spriteBatch.DrawString(_smallFont, "Stat", new Vector2(col1, yPos), Color.Yellow);
        spriteBatch.DrawString(_smallFont, "Base", new Vector2(col2, yPos), Color.Yellow);
        spriteBatch.DrawString(_smallFont, "Bonus", new Vector2(col3, yPos), Color.Yellow);
        spriteBatch.DrawString(_smallFont, "Total", new Vector2(col4, yPos), Color.Yellow);
        spriteBatch.DrawString(_smallFont, "Sources", new Vector2(col5, yPos), Color.Yellow);
        yPos += 20;

        // Separator line
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelBounds.X + 10, yPos, panelWidth - 20, 1), Color.Gray);
        yPos += 5;

        // Get stats for selected category
        var statsInCategory = GetStatsForCategory(_selectedStatCategory);
        var stats = selectedKyn.Stats;

        foreach (var statType in statsInCategory)
        {
            if (yPos > panelBounds.Y + panelHeight - 30) break;

            float baseValue = stats.GetBase(statType);
            float totalValue = stats.GetTotal(statType);
            float bonusValue = totalValue - baseValue;

            // Stat name
            string shortName = StatNames.GetShortName(statType);
            string fullName = StatNames.GetFullName(statType);
            spriteBatch.DrawString(_smallFont, shortName, new Vector2(col1, yPos), Color.White);

            // Base value
            string baseStr = StatNames.FormatValue(statType, baseValue);
            spriteBatch.DrawString(_smallFont, baseStr, new Vector2(col2, yPos), Color.LightGray);

            // Bonus value
            string bonusStr = bonusValue >= 0 ? $"+{StatNames.FormatValue(statType, bonusValue)}" : StatNames.FormatValue(statType, bonusValue);
            Color bonusColor = bonusValue > 0 ? Color.LimeGreen : (bonusValue < 0 ? Color.Red : Color.Gray);
            spriteBatch.DrawString(_smallFont, bonusStr, new Vector2(col3, yPos), bonusColor);

            // Total value
            string totalStr = StatNames.FormatValue(statType, totalValue);
            spriteBatch.DrawString(_smallFont, totalStr, new Vector2(col4, yPos), Color.Cyan);

            // Sources summary
            var modifiers = stats.GetModifiersForStat(statType).ToList();
            string sourceStr = modifiers.Count > 0 ? $"[{modifiers.Count} source(s)]" : "-";
            spriteBatch.DrawString(_smallFont, sourceStr, new Vector2(col5, yPos), Color.DarkGray);

            yPos += 18;
        }

        // Category description
        string catDesc = StatNames.GetCategoryName(_selectedStatCategory);
        spriteBatch.DrawString(_smallFont, $"Category: {catDesc}",
            new Vector2(panelBounds.X + 10, panelBounds.Y + panelHeight - 25), Color.Gray);
    }

    private void DrawCategoryTabs(SpriteBatch spriteBatch, Rectangle bounds)
    {
        var categories = Enum.GetValues<StatCategory>();
        int tabWidth = bounds.Width / categories.Length;

        for (int i = 0; i < categories.Length; i++)
        {
            var cat = categories[i];
            var tabBounds = new Rectangle(bounds.X + i * tabWidth, bounds.Y, tabWidth - 2, bounds.Height);

            bool isSelected = cat == _selectedStatCategory;
            Color bgColor = isSelected ? Color.DarkCyan : Color.DarkSlateGray;
            spriteBatch.Draw(_pixelTexture, tabBounds, bgColor);

            string catName = GetCategoryShortName(cat);
            var textSize = _smallFont.MeasureString(catName);
            float scale = Math.Min(1f, (tabWidth - 4) / textSize.X);
            Color textColor = isSelected ? Color.White : Color.Gray;

            spriteBatch.DrawString(_smallFont, catName,
                new Vector2(tabBounds.X + (tabBounds.Width - textSize.X * scale) / 2, tabBounds.Y + 3),
                textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    private string GetCategoryShortName(StatCategory category)
    {
        return category switch
        {
            StatCategory.Tempo => "TEMPO",
            StatCategory.Survival => "SURV",
            StatCategory.Energy => "EN",
            StatCategory.Heat => "HEAT",
            StatCategory.Combat => "COMB",
            StatCategory.PhysicalDamage => "PHYS",
            StatCategory.ElementalDamage => "ELEM",
            StatCategory.StatusApplication => "STS+",
            StatCategory.StatusResistance => "STS-",
            _ => category.ToString()
        };
    }

    private IEnumerable<StatType> GetStatsForCategory(StatCategory category)
    {
        return category switch
        {
            StatCategory.Tempo => new[] { StatType.Speed, StatType.ATBStartPercent, StatType.ATBDelayResist },
            StatCategory.Survival => new[] { StatType.HPMax, StatType.BarrierMax, StatType.BarrierRegen },
            StatCategory.Energy => new[] { StatType.ENMax, StatType.ENRegen },
            StatCategory.Heat => new[] { StatType.HeatCapacityMod, StatType.HeatDissipationMod, StatType.OverheatRecoveryBonus },
            StatCategory.Combat => new[] {
                StatType.MeleeAccuracy, StatType.RangedAccuracy, StatType.Evasion,
                StatType.MeleeCritChance, StatType.RangedCritChance, StatType.CritSeverity,
                StatType.Luck, StatType.Threat
            },
            StatCategory.PhysicalDamage => new[] {
                StatType.ATK_Impact, StatType.PEN_Impact, StatType.MIT_Impact,
                StatType.ATK_Piercing, StatType.PEN_Piercing, StatType.MIT_Piercing,
                StatType.ATK_Slashing, StatType.PEN_Slashing, StatType.MIT_Slashing
            },
            StatCategory.ElementalDamage => new[] {
                StatType.ATK_Thermal, StatType.PEN_Thermal, StatType.MIT_Thermal,
                StatType.ATK_Cryo, StatType.PEN_Cryo, StatType.MIT_Cryo,
                StatType.ATK_Electric, StatType.PEN_Electric, StatType.MIT_Electric,
                StatType.ATK_Corrosive, StatType.PEN_Corrosive, StatType.MIT_Corrosive,
                StatType.ATK_Toxic, StatType.PEN_Toxic, StatType.MIT_Toxic,
                StatType.ATK_Sonic, StatType.PEN_Sonic, StatType.MIT_Sonic,
                StatType.ATK_Radiant, StatType.PEN_Radiant, StatType.MIT_Radiant
            },
            StatCategory.StatusApplication => new[] {
                StatType.BleedApplication, StatType.PoisonApplication, StatType.BurnApplication,
                StatType.FreezeApplication, StatType.ShockApplication, StatType.BlindApplication
            },
            StatCategory.StatusResistance => new[] {
                StatType.BleedResist, StatType.PoisonResist, StatType.BurnResist,
                StatType.FreezeResist, StatType.ShockResist, StatType.BlindResist
            },
            _ => Array.Empty<StatType>()
        };
    }

    private Kyn? GetSelectedKyn()
    {
        if (_inRosterView)
        {
            var storedKyns = _roster.Storage.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < storedKyns.Count)
            {
                return storedKyns[_selectedIndex];
            }
        }
        else
        {
            var partyList = _roster.Party.ToList();
            if (_selectedIndex >= 0 && _selectedIndex < partyList.Count)
            {
                return partyList[_selectedIndex];
            }
        }
        return null;
    }
}
