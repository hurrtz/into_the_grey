using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Combat;
using Strays.Core.Game.Data;
using Strays.Core.Game.Dungeons;
using Strays.Core.Game.Entities;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// The current phase of the dungeon screen.
/// </summary>
public enum DungeonScreenPhase
{
    DifficultySelect,
    RoomOverview,
    PreCombat,
    InCombat,
    PostCombat,
    Results,
    Rewards
}

/// <summary>
/// Screen for dungeon instance gameplay.
/// </summary>
public class DungeonScreen : GameScreen
{
    private readonly DungeonDefinition _definition;
    private readonly StrayRoster _roster;
    private readonly GameStateService _gameState;

    private DungeonInstance? _instance;
    private DungeonScreenPhase _phase = DungeonScreenPhase.DifficultySelect;

    private Texture2D? _pixelTexture;
    private SpriteFont? _font;
    private SpriteFont? _titleFont;

    private int _selectedDifficulty = 1; // Default to Normal
    private int _selectedRoomIndex = 0;
    private float _transitionTimer = 0f;

    private readonly DungeonDifficulty[] _difficulties =
    {
        DungeonDifficulty.Easy,
        DungeonDifficulty.Normal,
        DungeonDifficulty.Hard,
        DungeonDifficulty.Brutal
    };

    /// <summary>
    /// Event raised when dungeon is exited.
    /// </summary>
    public event Action<DungeonReward?>? OnDungeonExit;

    public DungeonScreen(DungeonDefinition definition, StrayRoster roster, GameStateService gameState)
    {
        _definition = definition;
        _roster = roster;
        _gameState = gameState;

        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.3);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _font = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
        _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        base.HandleInput(gameTime, input);

        PlayerIndex playerIndex;

        switch (_phase)
        {
            case DungeonScreenPhase.DifficultySelect:
                HandleDifficultySelectInput(input, out playerIndex);
                break;

            case DungeonScreenPhase.RoomOverview:
                HandleRoomOverviewInput(input, out playerIndex);
                break;

            case DungeonScreenPhase.PreCombat:
                HandlePreCombatInput(input, out playerIndex);
                break;

            case DungeonScreenPhase.PostCombat:
                HandlePostCombatInput(input, out playerIndex);
                break;

            case DungeonScreenPhase.Results:
            case DungeonScreenPhase.Rewards:
                HandleResultsInput(input, out playerIndex);
                break;
        }

        // Universal escape to retreat
        if (input.IsNewKeyPress(Keys.Escape, null, out playerIndex) &&
            _phase != DungeonScreenPhase.DifficultySelect &&
            _phase != DungeonScreenPhase.InCombat &&
            _phase != DungeonScreenPhase.Results &&
            _phase != DungeonScreenPhase.Rewards)
        {
            ShowRetreatConfirmation();
        }
    }

    private void HandleDifficultySelectInput(InputState input, out PlayerIndex playerIndex)
    {
        if (input.IsNewKeyPress(Keys.Up, null, out playerIndex) ||
            input.IsNewKeyPress(Keys.W, null, out playerIndex))
        {
            _selectedDifficulty = Math.Max(0, _selectedDifficulty - 1);
        }
        else if (input.IsNewKeyPress(Keys.Down, null, out playerIndex) ||
                 input.IsNewKeyPress(Keys.S, null, out playerIndex))
        {
            _selectedDifficulty = Math.Min(_difficulties.Length - 1, _selectedDifficulty + 1);
        }
        else if (input.IsNewKeyPress(Keys.Enter, null, out playerIndex) ||
                 input.IsNewKeyPress(Keys.Space, null, out playerIndex))
        {
            StartDungeon(_difficulties[_selectedDifficulty]);
        }
        else if (input.IsNewKeyPress(Keys.Escape, null, out playerIndex))
        {
            ExitDungeon(null);
        }
    }

    private void HandleRoomOverviewInput(InputState input, out PlayerIndex playerIndex)
    {
        if (input.IsNewKeyPress(Keys.Enter, null, out playerIndex) ||
            input.IsNewKeyPress(Keys.Space, null, out playerIndex))
        {
            if (_instance?.CurrentRoom != null)
            {
                _phase = DungeonScreenPhase.PreCombat;
            }
        }
    }

    private void HandlePreCombatInput(InputState input, out PlayerIndex playerIndex)
    {
        if (input.IsNewKeyPress(Keys.Enter, null, out playerIndex) ||
            input.IsNewKeyPress(Keys.Space, null, out playerIndex))
        {
            StartCombat();
        }
        else if (input.IsNewKeyPress(Keys.Escape, null, out playerIndex))
        {
            _phase = DungeonScreenPhase.RoomOverview;
        }
    }

    private void HandlePostCombatInput(InputState input, out PlayerIndex playerIndex)
    {
        if (input.IsNewKeyPress(Keys.Enter, null, out playerIndex) ||
            input.IsNewKeyPress(Keys.Space, null, out playerIndex))
        {
            if (_instance != null)
            {
                if (_instance.State == DungeonState.Completed ||
                    _instance.State == DungeonState.Failed)
                {
                    _phase = DungeonScreenPhase.Results;
                }
                else if (_instance.AdvanceToNextRoom())
                {
                    _phase = DungeonScreenPhase.RoomOverview;
                }
            }
        }
    }

    private void HandleResultsInput(InputState input, out PlayerIndex playerIndex)
    {
        if (input.IsNewKeyPress(Keys.Enter, null, out playerIndex) ||
            input.IsNewKeyPress(Keys.Space, null, out playerIndex))
        {
            if (_phase == DungeonScreenPhase.Results)
            {
                _phase = DungeonScreenPhase.Rewards;
            }
            else
            {
                // Claim rewards and exit
                var rewards = _instance?.GetFinalRewards();
                ExitDungeon(rewards);
            }
        }
    }

    private void StartDungeon(DungeonDifficulty difficulty)
    {
        _instance = new DungeonInstance(_definition);
        _instance.Start(difficulty);
        _phase = DungeonScreenPhase.RoomOverview;
    }

    private void StartCombat()
    {
        if (_instance?.CurrentRoom == null) return;

        _instance.EnterCombat();
        _phase = DungeonScreenPhase.InCombat;

        // Create combat encounter from room data
        var enemies = CreateEnemiesFromRoom(_instance.CurrentRoom);
        var partyStrays = new List<Stray>(_roster.Party);

        var combatScreen = new CombatScreen(partyStrays, enemies, encounter: null, companion: null, gameState: _gameState);
        combatScreen.CombatEnded += OnCombatComplete;
        ScreenManager.AddScreen(combatScreen, ControllingPlayer);
    }

    private List<Stray> CreateEnemiesFromRoom(DungeonRoom room)
    {
        var enemies = new List<Stray>();

        for (int i = 0; i < room.EnemyIds.Count; i++)
        {
            var enemyId = room.EnemyIds[i];
            var level = i < room.EnemyLevels.Count ? room.EnemyLevels[i] : 5;

            // Try to get stray definition
            var definition = StrayDefinitions.Get(enemyId);
            if (definition == null)
            {
                // Create a generic enemy if not found
                // Apply HP multiplier to base stats for dungeon scaling
                int baseHp = (int)((50 + level * 10) * room.HpMultiplier);
                definition = new StrayDefinition
                {
                    Id = enemyId,
                    Name = FormatEnemyName(enemyId),
                    BaseStats = new StrayBaseStats
                    {
                        MaxHp = baseHp,
                        Attack = 10 + level * 2,
                        Defense = 5 + level,
                        Speed = 8 + level
                    }
                };
            }

            var stray = new Stray(definition, level);
            stray.CurrentHp = stray.MaxHp; // Start at full health

            enemies.Add(stray);
        }

        return enemies;
    }

    private string FormatEnemyName(string id)
    {
        // Convert snake_case to Title Case
        var words = id.Split('_');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
        }
        return string.Join(" ", words);
    }

    private void OnCombatComplete(object? sender, CombatEndedEventArgs e)
    {
        if (_instance == null) return;

        if (e.Victory)
        {
            _instance.CompleteRoom();
            _phase = DungeonScreenPhase.PostCombat;
        }
        else
        {
            _instance.OnDefeat();
            _phase = DungeonScreenPhase.Results;
        }
    }

    private void ShowRetreatConfirmation()
    {
        var confirmBox = new MessageBoxScreen("Retreat from dungeon? You'll keep rewards from cleared rooms.");
        confirmBox.Accepted += (s, e) =>
        {
            _instance?.Retreat();
            _phase = DungeonScreenPhase.Results;
        };
        ScreenManager.AddScreen(confirmBox, ControllingPlayer);
    }

    private void ExitDungeon(DungeonReward? rewards)
    {
        OnDungeonExit?.Invoke(rewards);
        ExitScreen();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        _transitionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var screenWidth = (int)ScreenManager.BaseScreenSize.X;
        var screenHeight = (int)ScreenManager.BaseScreenSize.Y;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        // Background
        var bgColor = ParseColor(_definition.AmbientColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, screenWidth, screenHeight),
            bgColor * TransitionAlpha);

        // Draw based on phase
        switch (_phase)
        {
            case DungeonScreenPhase.DifficultySelect:
                DrawDifficultySelect(spriteBatch, screenWidth, screenHeight);
                break;

            case DungeonScreenPhase.RoomOverview:
                DrawRoomOverview(spriteBatch, screenWidth, screenHeight);
                break;

            case DungeonScreenPhase.PreCombat:
                DrawPreCombat(spriteBatch, screenWidth, screenHeight);
                break;

            case DungeonScreenPhase.PostCombat:
                DrawPostCombat(spriteBatch, screenWidth, screenHeight);
                break;

            case DungeonScreenPhase.Results:
                DrawResults(spriteBatch, screenWidth, screenHeight);
                break;

            case DungeonScreenPhase.Rewards:
                DrawRewards(spriteBatch, screenWidth, screenHeight);
                break;
        }

        spriteBatch.End();
    }

    private void DrawDifficultySelect(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        // Title
        var title = _definition.Name;
        var titleSize = _titleFont!.MeasureString(title);
        spriteBatch.DrawString(_titleFont, title,
            new Vector2((screenWidth - titleSize.X) / 2, 40),
            Color.White * TransitionAlpha);

        // Description
        var desc = _definition.Description;
        var descSize = _font!.MeasureString(desc);
        spriteBatch.DrawString(_font, desc,
            new Vector2((screenWidth - descSize.X) / 2, 90),
            Color.Gray * TransitionAlpha);

        // Level range
        var levelText = $"Recommended Level: {_definition.MinLevel} - {_definition.MaxLevel}";
        spriteBatch.DrawString(_font, levelText,
            new Vector2((screenWidth - _font.MeasureString(levelText).X) / 2, 120),
            Color.Yellow * TransitionAlpha);

        // Objective
        spriteBatch.DrawString(_font, $"Objective: {_definition.ObjectiveText}",
            new Vector2(50, 160), Color.Cyan * TransitionAlpha);

        // Difficulty selection
        spriteBatch.DrawString(_titleFont, "Select Difficulty",
            new Vector2(50, 210), Color.White * TransitionAlpha);

        for (int i = 0; i < _difficulties.Length; i++)
        {
            var diff = _difficulties[i];
            var isSelected = i == _selectedDifficulty;
            var yPos = 260 + i * 50;

            // Selection indicator
            if (isSelected)
            {
                spriteBatch.Draw(_pixelTexture, new Rectangle(40, yPos - 5, screenWidth - 80, 45),
                    Color.White * 0.2f * TransitionAlpha);
                spriteBatch.DrawString(_font, ">", new Vector2(50, yPos), Color.Yellow * TransitionAlpha);
            }

            // Difficulty name
            var diffName = DifficultyModifiers.GetDisplayName(diff);
            var nameColor = diff switch
            {
                DungeonDifficulty.Easy => Color.LightGreen,
                DungeonDifficulty.Normal => Color.White,
                DungeonDifficulty.Hard => Color.Orange,
                DungeonDifficulty.Brutal => Color.Red,
                _ => Color.White
            };
            spriteBatch.DrawString(_font, diffName, new Vector2(80, yPos), nameColor * TransitionAlpha);

            // Difficulty description
            var diffDesc = DifficultyModifiers.GetDescription(diff);
            spriteBatch.DrawString(_font, diffDesc, new Vector2(200, yPos), Color.Gray * TransitionAlpha);
        }

        // Instructions
        spriteBatch.DrawString(_font, "[UP/DOWN] Select   [ENTER] Start   [ESC] Cancel",
            new Vector2(50, screenHeight - 40), Color.DimGray * TransitionAlpha);
    }

    private void DrawRoomOverview(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        if (_instance == null) return;

        // Header
        spriteBatch.DrawString(_titleFont!, _definition.Name,
            new Vector2(50, 30), Color.White * TransitionAlpha);

        var diffText = $"Difficulty: {DifficultyModifiers.GetDisplayName(_instance.Difficulty)}";
        spriteBatch.DrawString(_font!, diffText,
            new Vector2(50, 70), Color.Yellow * TransitionAlpha);

        // Progress bar
        var progressWidth = screenWidth - 100;
        var progressHeight = 20;
        var progressY = 100;

        spriteBatch.Draw(_pixelTexture, new Rectangle(50, progressY, progressWidth, progressHeight),
            Color.DarkGray * TransitionAlpha);

        var filledWidth = (int)(progressWidth * _instance.GetProgressPercent() / 100f);
        spriteBatch.Draw(_pixelTexture, new Rectangle(50, progressY, filledWidth, progressHeight),
            Color.Green * TransitionAlpha);

        spriteBatch.DrawString(_font!, $"{_instance.RoomsCleared}/{_instance.TotalRooms} Rooms Cleared",
            new Vector2(50, progressY + 25), Color.White * TransitionAlpha);

        // Room list
        int startY = 170;
        int roomsToShow = Math.Min(8, _instance.Rooms.Count);
        int startIndex = Math.Max(0, _instance.CurrentRoomIndex - 3);

        for (int i = 0; i < roomsToShow && startIndex + i < _instance.Rooms.Count; i++)
        {
            var room = _instance.Rooms[startIndex + i];
            var yPos = startY + i * 35;
            var isCurrent = room == _instance.CurrentRoom;

            // Background for current room
            if (isCurrent)
            {
                spriteBatch.Draw(_pixelTexture, new Rectangle(40, yPos - 5, screenWidth - 80, 30),
                    Color.White * 0.15f * TransitionAlpha);
            }

            // Room info
            var roomColor = room.State switch
            {
                RoomState.Cleared => Color.Green,
                RoomState.Current => Color.Yellow,
                RoomState.Failed => Color.Red,
                _ => Color.Gray
            };

            spriteBatch.DrawString(_font!, room.GetDisplayName(),
                new Vector2(60, yPos), roomColor * TransitionAlpha);
        }

        // Current room details
        if (_instance.CurrentRoom != null)
        {
            var detailY = screenHeight - 120;
            spriteBatch.Draw(_pixelTexture, new Rectangle(40, detailY - 10, screenWidth - 80, 80),
                Color.Black * 0.5f * TransitionAlpha);

            spriteBatch.DrawString(_font!, $"Current: {_instance.CurrentRoom.Name}",
                new Vector2(60, detailY), Color.White * TransitionAlpha);
            spriteBatch.DrawString(_font!, _instance.CurrentRoom.Description,
                new Vector2(60, detailY + 25), Color.Gray * TransitionAlpha);

            var enemyText = $"Enemies: {_instance.CurrentRoom.EnemyIds.Count}";
            spriteBatch.DrawString(_font!, enemyText,
                new Vector2(60, detailY + 50), Color.Orange * TransitionAlpha);
        }

        // Instructions
        spriteBatch.DrawString(_font!, "[ENTER] Enter Room   [ESC] Retreat",
            new Vector2(50, screenHeight - 30), Color.DimGray * TransitionAlpha);
    }

    private void DrawPreCombat(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        if (_instance?.CurrentRoom == null) return;

        var room = _instance.CurrentRoom;

        // Room name
        spriteBatch.DrawString(_titleFont!, room.Name,
            new Vector2((screenWidth - _titleFont!.MeasureString(room.Name).X) / 2, 100),
            Color.White * TransitionAlpha);

        // Room type indicator
        var typeText = room.Type switch
        {
            RoomType.MidBoss => "-- MINI-BOSS --",
            RoomType.FinalBoss => "-- FINAL BOSS --",
            _ => "-- COMBAT --"
        };
        var typeColor = room.Type switch
        {
            RoomType.MidBoss => Color.Orange,
            RoomType.FinalBoss => Color.Red,
            _ => Color.Yellow
        };
        spriteBatch.DrawString(_font!, typeText,
            new Vector2((screenWidth - _font!.MeasureString(typeText).X) / 2, 150),
            typeColor * TransitionAlpha);

        // Description
        spriteBatch.DrawString(_font!, room.Description,
            new Vector2((screenWidth - _font.MeasureString(room.Description).X) / 2, 200),
            Color.Gray * TransitionAlpha);

        // Enemy preview
        int enemyY = 260;
        spriteBatch.DrawString(_font!, "Enemies:", new Vector2(50, enemyY), Color.White * TransitionAlpha);

        for (int i = 0; i < room.EnemyIds.Count; i++)
        {
            var enemyName = FormatEnemyName(room.EnemyIds[i]);
            var level = i < room.EnemyLevels.Count ? room.EnemyLevels[i] : 5;
            spriteBatch.DrawString(_font!, $"  - {enemyName} (Lv.{level})",
                new Vector2(70, enemyY + 25 + i * 22), Color.Orange * TransitionAlpha);
        }

        // Ready prompt
        var readyText = "Press [ENTER] to begin combat!";
        var pulse = (float)Math.Sin(_transitionTimer * 3) * 0.3f + 0.7f;
        spriteBatch.DrawString(_font!, readyText,
            new Vector2((screenWidth - _font.MeasureString(readyText).X) / 2, screenHeight - 80),
            Color.Yellow * pulse * TransitionAlpha);

        spriteBatch.DrawString(_font!, "[ESC] Back",
            new Vector2(50, screenHeight - 30), Color.DimGray * TransitionAlpha);
    }

    private void DrawPostCombat(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        if (_instance?.CurrentRoom == null) return;

        // Victory text
        spriteBatch.DrawString(_titleFont!, "VICTORY!",
            new Vector2((screenWidth - _titleFont!.MeasureString("VICTORY!").X) / 2, 100),
            Color.Green * TransitionAlpha);

        var room = _instance.CurrentRoom;
        spriteBatch.DrawString(_font!, $"{room.Name} Cleared!",
            new Vector2((screenWidth - _font!.MeasureString($"{room.Name} Cleared!").X) / 2, 150),
            Color.White * TransitionAlpha);

        // Room rewards
        if (room.RoomReward != null)
        {
            int rewardY = 200;
            spriteBatch.DrawString(_font!, "Rewards:", new Vector2(50, rewardY), Color.Yellow * TransitionAlpha);
            spriteBatch.DrawString(_font!, $"  +{room.RoomReward.Experience} EXP",
                new Vector2(70, rewardY + 25), Color.Cyan * TransitionAlpha);
            spriteBatch.DrawString(_font!, $"  +{room.RoomReward.Currency} Scrap",
                new Vector2(70, rewardY + 50), Color.Gold * TransitionAlpha);

            int itemY = rewardY + 80;
            foreach (var item in room.RoomReward.Items)
            {
                var rarityColor = GetRarityColor(item.Rarity);
                spriteBatch.DrawString(_font!, $"  {item.Name} x{item.Quantity}",
                    new Vector2(70, itemY), rarityColor * TransitionAlpha);
                itemY += 22;
            }
        }

        // Next room or completion
        var nextText = _instance.State == DungeonState.Completed
            ? "Dungeon Complete! Press [ENTER] to see results"
            : "Press [ENTER] to continue to next room";

        spriteBatch.DrawString(_font!, nextText,
            new Vector2((screenWidth - _font.MeasureString(nextText).X) / 2, screenHeight - 60),
            Color.Yellow * TransitionAlpha);
    }

    private void DrawResults(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        if (_instance == null) return;

        // Result title
        var resultText = _instance.State switch
        {
            DungeonState.Completed => "DUNGEON COMPLETE!",
            DungeonState.Failed => "DEFEATED",
            DungeonState.Retreated => "RETREATED",
            _ => "RUN ENDED"
        };
        var resultColor = _instance.State == DungeonState.Completed ? Color.Gold : Color.Red;

        spriteBatch.DrawString(_titleFont!, resultText,
            new Vector2((screenWidth - _titleFont!.MeasureString(resultText).X) / 2, 80),
            resultColor * TransitionAlpha);

        // Stats
        int statY = 150;
        spriteBatch.DrawString(_font!, $"Rooms Cleared: {_instance.RoomsCleared}/{_instance.TotalRooms}",
            new Vector2(50, statY), Color.White * TransitionAlpha);
        spriteBatch.DrawString(_font!, $"Difficulty: {DifficultyModifiers.GetDisplayName(_instance.Difficulty)}",
            new Vector2(50, statY + 25), Color.Yellow * TransitionAlpha);
        spriteBatch.DrawString(_font!, $"Time: {_instance.ElapsedTime:mm\\:ss}",
            new Vector2(50, statY + 50), Color.White * TransitionAlpha);

        if (_instance.MidBossDefeated)
        {
            spriteBatch.DrawString(_font!, $"Mini-Boss: {_definition.MidBossName} - DEFEATED",
                new Vector2(50, statY + 80), Color.Green * TransitionAlpha);
        }

        if (_instance.FinalBossDefeated)
        {
            spriteBatch.DrawString(_font!, $"Final Boss: {_definition.FinalBossName} - DEFEATED",
                new Vector2(50, statY + 105), Color.Gold * TransitionAlpha);
        }

        spriteBatch.DrawString(_font!, "Press [ENTER] to see rewards",
            new Vector2((screenWidth - _font!.MeasureString("Press [ENTER] to see rewards").X) / 2, screenHeight - 60),
            Color.Yellow * TransitionAlpha);
    }

    private void DrawRewards(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        if (_instance == null) return;

        var rewards = _instance.GetFinalRewards();

        spriteBatch.DrawString(_titleFont!, "REWARDS",
            new Vector2((screenWidth - _titleFont!.MeasureString("REWARDS").X) / 2, 60),
            Color.Gold * TransitionAlpha);

        int rewardY = 130;

        // Experience and currency
        spriteBatch.DrawString(_font!, $"Experience: +{rewards.Experience}",
            new Vector2(50, rewardY), Color.Cyan * TransitionAlpha);
        spriteBatch.DrawString(_font!, $"Scrap: +{rewards.Currency}",
            new Vector2(50, rewardY + 30), Color.Gold * TransitionAlpha);

        // Items
        if (rewards.Items.Count > 0 || rewards.BonusItems.Count > 0)
        {
            rewardY += 70;
            spriteBatch.DrawString(_font!, "Items Obtained:", new Vector2(50, rewardY), Color.White * TransitionAlpha);
            rewardY += 25;

            foreach (var item in rewards.Items)
            {
                var color = GetRarityColor(item.Rarity);
                spriteBatch.DrawString(_font!, $"  [{item.Rarity}] {item.Name} x{item.Quantity}",
                    new Vector2(60, rewardY), color * TransitionAlpha);
                rewardY += 22;
            }

            foreach (var item in rewards.BonusItems)
            {
                var color = GetRarityColor(item.Rarity);
                spriteBatch.DrawString(_font!, $"  [{item.Rarity}] {item.Name} x{item.Quantity} (BONUS)",
                    new Vector2(60, rewardY), color * TransitionAlpha);
                rewardY += 22;
            }
        }

        spriteBatch.DrawString(_font!, "Press [ENTER] to claim rewards and exit",
            new Vector2((screenWidth - _font!.MeasureString("Press [ENTER] to claim rewards and exit").X) / 2,
                screenHeight - 60),
            Color.Yellow * TransitionAlpha);
    }

    private Color GetRarityColor(RewardRarity rarity) => rarity switch
    {
        RewardRarity.Common => Color.White,
        RewardRarity.Uncommon => Color.LightGreen,
        RewardRarity.Rare => Color.DeepSkyBlue,
        RewardRarity.Epic => Color.MediumPurple,
        RewardRarity.Legendary => Color.Gold,
        _ => Color.White
    };

    private Color ParseColor(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length < 7) return Color.Black;

        hex = hex.TrimStart('#');
        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
        return new Color(r, g, b);
    }
}
