using System;
using System.Collections.Generic;
using System.Linq;
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
/// Screen for exploring dungeons with free movement through rooms.
/// The player walks through rooms, encounters enemies that trigger combat,
/// and doors are blocked until all enemies in a room are defeated.
/// </summary>
public class DungeonExplorationScreen : GameScreen
{
    // Dependencies
    private readonly DungeonDefinition _definition;
    private readonly StrayRoster _roster;
    private readonly GameStateService _gameState;
    private readonly DungeonDifficulty _difficulty;

    // Graphics
    private SpriteBatch? _spriteBatch;
    private Texture2D? _pixel;
    private SpriteFont? _font;
    private SpriteFont? _titleFont;

    // Dungeon state
    private DungeonInstance _instance = null!;
    private List<ExplorableRoom> _rooms = new();
    private int _currentRoomIndex = 0;
    private ExplorableRoom CurrentRoom => _rooms[_currentRoomIndex];

    // Player state
    private Vector2 _playerPosition;
    private Vector2 _playerVelocity;
    private const float PlayerSpeed = 150f;
    private const int PlayerSize = 24;
    private Rectangle PlayerBounds => new(
        (int)_playerPosition.X - PlayerSize / 2,
        (int)_playerPosition.Y - PlayerSize / 2,
        PlayerSize, PlayerSize);

    // Camera
    private Vector2 _cameraPosition;
    private const float CameraSmoothing = 8f;

    // Combat state
    private bool _inCombat = false;
    private List<DungeonEnemy> _currentCombatEnemies = new();

    // UI state
    private float _transitionTimer;
    private float _messageTimer;
    private string _currentMessage = "";
    private bool _showingRoomTransition;
    private float _roomTransitionTimer;

    // Colors
    private static readonly Color FloorColor = new(40, 45, 55);
    private static readonly Color WallColor = new(20, 22, 28);
    private static readonly Color DoorLockedColor = new(150, 50, 50);
    private static readonly Color DoorOpenColor = new(50, 150, 50);
    private static readonly Color PlayerColor = new(100, 150, 255);
    private static readonly Color EnemyColor = new(255, 80, 80);
    private static readonly Color HazardColor = new(200, 150, 50);

    /// <summary>
    /// Event raised when dungeon is exited.
    /// </summary>
    public event Action<DungeonReward?>? OnDungeonExit;

    public DungeonExplorationScreen(
        DungeonDefinition definition,
        StrayRoster roster,
        GameStateService gameState,
        DungeonDifficulty difficulty)
    {
        _definition = definition;
        _roster = roster;
        _gameState = gameState;
        _difficulty = difficulty;

        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.3);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _spriteBatch = ScreenManager.SpriteBatch;
        _pixel = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
        _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");

        InitializeDungeon();
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixel?.Dispose();
    }

    private void InitializeDungeon()
    {
        // Create dungeon instance
        _instance = new DungeonInstance(_definition);
        _instance.Start(_difficulty);

        // Generate explorable rooms
        _rooms.Clear();
        for (int i = 0; i < _instance.Rooms.Count; i++)
        {
            var room = ExplorableRoom.Create(_instance.Rooms[i], i, _instance.Rooms.Count);
            _rooms.Add(room);
        }

        // Start in first room
        _currentRoomIndex = 0;
        _playerPosition = CurrentRoom.PlayerSpawn;
        _cameraPosition = _playerPosition;

        ShowMessage($"Entering {_definition.Name}");
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        base.HandleInput(gameTime, input);

        if (_inCombat || _showingRoomTransition)
            return;

        // Movement
        var movement = Vector2.Zero;
        var keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
            movement.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
            movement.Y += 1;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
            movement.X -= 1;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
            movement.X += 1;

        if (movement != Vector2.Zero)
        {
            movement.Normalize();
            _playerVelocity = movement * PlayerSpeed;
        }
        else
        {
            _playerVelocity = Vector2.Zero;
        }

        // Interaction
        if (input.IsNewKeyPress(Keys.E, ControllingPlayer, out _) ||
            input.IsNewKeyPress(Keys.Enter, ControllingPlayer, out _))
        {
            TryInteract();
        }

        // Pause/Menu
        if (input.IsNewKeyPress(Keys.Escape, ControllingPlayer, out _))
        {
            ShowRetreatConfirmation();
        }

        // Open menu
        if (input.IsNewKeyPress(Keys.Tab, ControllingPlayer, out _))
        {
            var menuScreen = new GameMenuScreen(
                _roster,
                _gameState,
                _gameState.FactionReputation,
                null,
                _gameState.Bestiary);
            ScreenManager.AddScreen(menuScreen, ControllingPlayer);
        }
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _transitionTimer += dt;

        // Update message timer
        if (_messageTimer > 0)
        {
            _messageTimer -= dt;
            if (_messageTimer <= 0)
                _currentMessage = "";
        }

        // Room transition animation
        if (_showingRoomTransition)
        {
            _roomTransitionTimer -= dt;
            if (_roomTransitionTimer <= 0)
            {
                _showingRoomTransition = false;
            }
            return;
        }

        if (_inCombat || IsExiting)
            return;

        // Update player movement
        UpdatePlayerMovement(dt);

        // Update camera
        UpdateCamera(dt);

        // Check for enemy encounters
        CheckEnemyEncounters();

        // Check for door interactions (auto-transition when walking into unlocked door)
        CheckDoorTransitions();
    }

    private void UpdatePlayerMovement(float dt)
    {
        if (_playerVelocity == Vector2.Zero)
            return;

        var newPosition = _playerPosition + _playerVelocity * dt;
        var newBounds = new Rectangle(
            (int)newPosition.X - PlayerSize / 2,
            (int)newPosition.Y - PlayerSize / 2,
            PlayerSize, PlayerSize);

        // Check collision with walls
        if (!CurrentRoom.CollidesWithWalls(newBounds))
        {
            _playerPosition = newPosition;
        }
        else
        {
            // Try sliding along walls
            var slideX = new Vector2(newPosition.X, _playerPosition.Y);
            var slideXBounds = new Rectangle(
                (int)slideX.X - PlayerSize / 2,
                (int)_playerPosition.Y - PlayerSize / 2,
                PlayerSize, PlayerSize);

            if (!CurrentRoom.CollidesWithWalls(slideXBounds))
            {
                _playerPosition.X = slideX.X;
            }

            var slideY = new Vector2(_playerPosition.X, newPosition.Y);
            var slideYBounds = new Rectangle(
                (int)_playerPosition.X - PlayerSize / 2,
                (int)slideY.Y - PlayerSize / 2,
                PlayerSize, PlayerSize);

            if (!CurrentRoom.CollidesWithWalls(slideYBounds))
            {
                _playerPosition.Y = slideY.Y;
            }
        }
    }

    private void UpdateCamera(float dt)
    {
        // Smooth camera follow
        var targetCamera = _playerPosition;

        // Clamp to room bounds
        var screenWidth = ScreenManager.BaseScreenSize.X;
        var screenHeight = ScreenManager.BaseScreenSize.Y;

        float minX = screenWidth / 2;
        float maxX = CurrentRoom.WidthPixels - screenWidth / 2;
        float minY = screenHeight / 2;
        float maxY = CurrentRoom.HeightPixels - screenHeight / 2;

        if (maxX < minX) targetCamera.X = CurrentRoom.WidthPixels / 2;
        else targetCamera.X = MathHelper.Clamp(targetCamera.X, minX, maxX);

        if (maxY < minY) targetCamera.Y = CurrentRoom.HeightPixels / 2;
        else targetCamera.Y = MathHelper.Clamp(targetCamera.Y, minY, maxY);

        _cameraPosition = Vector2.Lerp(_cameraPosition, targetCamera, CameraSmoothing * dt);
    }

    private void CheckEnemyEncounters()
    {
        var enemy = CurrentRoom.GetEnemyAt(PlayerBounds);
        if (enemy != null && !enemy.IsDefeated)
        {
            StartCombat(enemy);
        }
    }

    private void CheckDoorTransitions()
    {
        var door = CurrentRoom.GetDoorAt(PlayerBounds);
        if (door != null && !door.IsLocked)
        {
            if (door.TargetRoomIndex == -1)
            {
                // Dungeon complete!
                CompleteDungeon();
            }
            else
            {
                TransitionToRoom(door.TargetRoomIndex);
            }
        }
    }

    private void TryInteract()
    {
        // Check for door interaction
        var door = CurrentRoom.GetDoorAt(PlayerBounds);
        if (door != null)
        {
            if (door.IsLocked)
            {
                ShowMessage($"Door locked! Defeat {CurrentRoom.RemainingEnemies} enemies to proceed.");
            }
        }
    }

    private void StartCombat(DungeonEnemy enemy)
    {
        _inCombat = true;
        _currentCombatEnemies = CurrentRoom.GetCombatGroup(enemy);

        // Create enemy Strays for combat
        var enemies = new List<Stray>();
        foreach (var e in _currentCombatEnemies)
        {
            var def = StrayDefinitions.Get(e.DefinitionId);
            if (def == null)
            {
                // Create generic enemy if definition not found
                def = new StrayDefinition
                {
                    Id = e.DefinitionId,
                    Name = e.DisplayName,
                    BaseStats = new StrayBaseStats
                    {
                        MaxHp = 50 + e.Level * 10,
                        Attack = 10 + e.Level * 2,
                        Defense = 5 + e.Level,
                        Speed = 8 + e.Level
                    }
                };
            }

            var stray = new Stray(def, e.Level);
            stray.CurrentHp = stray.MaxHp;
            enemies.Add(stray);
        }

        var partyStrays = new List<Stray>(_roster.Party);

        var combatScreen = new CombatScreen(partyStrays, enemies, encounter: null, companion: null, gameState: _gameState);
        combatScreen.CombatEnded += OnCombatComplete;
        ScreenManager.AddScreen(combatScreen, ControllingPlayer);
    }

    private void OnCombatComplete(object? sender, CombatEndedEventArgs e)
    {
        _inCombat = false;

        if (e.Victory)
        {
            // Mark enemies as defeated
            foreach (var enemy in _currentCombatEnemies)
            {
                CurrentRoom.DefeatEnemy(enemy.Id);
            }

            // Update dungeon instance
            if (CurrentRoom.IsCleared)
            {
                _instance.CompleteRoom();

                if (CurrentRoom.RemainingEnemies == 0)
                {
                    ShowMessage("Room cleared! Door unlocked.");
                }
            }

            // Record bestiary encounters
            foreach (var enemy in _currentCombatEnemies)
            {
                _gameState.Bestiary.RecordDefeat(enemy.DefinitionId);
            }
        }
        else
        {
            // Defeat - player can retry or retreat
            ShowMessage("Defeated! Press ESC to retreat.");
        }

        _currentCombatEnemies.Clear();
    }

    private void TransitionToRoom(int targetRoomIndex)
    {
        _instance.AdvanceToNextRoom();
        _currentRoomIndex = targetRoomIndex;
        _playerPosition = CurrentRoom.PlayerSpawn;
        _cameraPosition = _playerPosition;

        _showingRoomTransition = true;
        _roomTransitionTimer = 1f;

        var roomName = CurrentRoom.RoomData.Type switch
        {
            RoomType.MidBoss => $"MINI-BOSS: {CurrentRoom.RoomData.Name}",
            RoomType.FinalBoss => $"FINAL BOSS: {CurrentRoom.RoomData.Name}",
            _ => CurrentRoom.RoomData.Name
        };

        ShowMessage(roomName, 2f);
    }

    private void CompleteDungeon()
    {
        _instance.CompleteRoom(); // Mark final room as complete

        var rewards = _instance.GetFinalRewards();

        // Show results screen
        var resultsScreen = new DungeonResultsScreen(_definition, _instance, rewards);
        resultsScreen.OnExit += (reward) =>
        {
            OnDungeonExit?.Invoke(reward);
            ExitScreen();
        };
        ScreenManager.AddScreen(resultsScreen, ControllingPlayer);
    }

    private void ShowRetreatConfirmation()
    {
        var confirmBox = new MessageBoxScreen("Retreat from dungeon?\nYou'll keep rewards from cleared rooms.");
        confirmBox.Accepted += (s, e) =>
        {
            _instance.Retreat();
            var rewards = _instance.GetFinalRewards();
            OnDungeonExit?.Invoke(rewards);
            ExitScreen();
        };
        ScreenManager.AddScreen(confirmBox, ControllingPlayer);
    }

    private void ShowMessage(string message, float duration = 2f)
    {
        _currentMessage = message;
        _messageTimer = duration;
    }

    public override void Draw(GameTime gameTime)
    {
        var screenWidth = (int)ScreenManager.BaseScreenSize.X;
        var screenHeight = (int)ScreenManager.BaseScreenSize.Y;

        _spriteBatch!.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation);

        // Calculate camera offset
        var cameraOffset = new Vector2(
            screenWidth / 2 - _cameraPosition.X,
            screenHeight / 2 - _cameraPosition.Y);

        // Draw room
        DrawRoom(cameraOffset);

        // Draw enemies
        DrawEnemies(cameraOffset);

        // Draw doors
        DrawDoors(cameraOffset);

        // Draw player
        DrawPlayer(cameraOffset);

        // Draw UI overlay
        DrawUI(screenWidth, screenHeight);

        // Draw room transition overlay
        if (_showingRoomTransition)
        {
            float alpha = Math.Min(1f, _roomTransitionTimer * 2);
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, screenWidth, screenHeight),
                Color.Black * alpha * TransitionAlpha);
        }

        _spriteBatch.End();
    }

    private void DrawRoom(Vector2 offset)
    {
        var layout = CurrentRoom.Layout;
        int tileSize = ExplorableRoom.TileSize;

        for (int y = 0; y < layout.Height; y++)
        {
            for (int x = 0; x < layout.Width; x++)
            {
                var tile = layout.Tiles[y, x];
                var rect = new Rectangle(
                    (int)(x * tileSize + offset.X),
                    (int)(y * tileSize + offset.Y),
                    tileSize, tileSize);

                var color = tile switch
                {
                    RoomTile.Wall => WallColor,
                    RoomTile.Hazard => HazardColor,
                    RoomTile.HealingStation => Color.Green * 0.3f,
                    RoomTile.Treasure => Color.Gold * 0.3f,
                    _ => FloorColor
                };

                _spriteBatch!.Draw(_pixel, rect, color * TransitionAlpha);

                // Draw tile borders for walls
                if (tile == RoomTile.Wall)
                {
                    _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2),
                        Color.Black * 0.5f * TransitionAlpha);
                    _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height),
                        Color.Black * 0.5f * TransitionAlpha);
                }
            }
        }
    }

    private void DrawEnemies(Vector2 offset)
    {
        foreach (var enemy in CurrentRoom.Enemies)
        {
            if (enemy.IsDefeated) continue;

            var pos = enemy.Position + offset;
            var rect = new Rectangle(
                (int)pos.X - 16,
                (int)pos.Y - 16,
                32, 32);

            // Draw enemy placeholder (circle approximation with rectangle)
            _spriteBatch!.Draw(_pixel, rect, enemy.PlaceholderColor * TransitionAlpha);

            // Draw "!" indicator
            var exclamation = "!";
            var excSize = _font!.MeasureString(exclamation);
            _spriteBatch.DrawString(_font, exclamation,
                new Vector2(pos.X - excSize.X / 2, pos.Y - 28),
                Color.Red * TransitionAlpha);

            // Draw level
            var levelText = $"Lv{enemy.Level}";
            var levelSize = _font.MeasureString(levelText);
            _spriteBatch.DrawString(_font, levelText,
                new Vector2(pos.X - levelSize.X / 2, pos.Y + 18),
                Color.White * 0.8f * TransitionAlpha);
        }
    }

    private void DrawDoors(Vector2 offset)
    {
        foreach (var door in CurrentRoom.Doors)
        {
            var pos = door.Position + offset;
            var rect = new Rectangle(
                (int)pos.X - 24,
                (int)pos.Y - 24,
                48, 48);

            var color = door.IsLocked ? DoorLockedColor : DoorOpenColor;
            _spriteBatch!.Draw(_pixel, rect, color * TransitionAlpha);

            // Draw door icon
            var doorText = door.IsLocked ? "X" : door.TargetRoomIndex == -1 ? "EXIT" : ">>";
            var textSize = _font!.MeasureString(doorText);
            _spriteBatch.DrawString(_font, doorText,
                new Vector2(pos.X - textSize.X / 2, pos.Y - textSize.Y / 2),
                Color.White * TransitionAlpha);
        }
    }

    private void DrawPlayer(Vector2 offset)
    {
        var pos = _playerPosition + offset;
        var rect = new Rectangle(
            (int)pos.X - PlayerSize / 2,
            (int)pos.Y - PlayerSize / 2,
            PlayerSize, PlayerSize);

        _spriteBatch!.Draw(_pixel, rect, PlayerColor * TransitionAlpha);
    }

    private void DrawUI(int screenWidth, int screenHeight)
    {
        // Dungeon name and progress
        var header = $"{_definition.Name} - Room {_currentRoomIndex + 1}/{_rooms.Count}";
        _spriteBatch!.DrawString(_font!, header, new Vector2(10, 10), Color.White * TransitionAlpha);

        // Enemies remaining
        var enemyText = CurrentRoom.IsCleared
            ? "Room Cleared!"
            : $"Enemies: {CurrentRoom.RemainingEnemies}";
        var enemyColor = CurrentRoom.IsCleared ? Color.Green : Color.Orange;
        _spriteBatch.DrawString(_font!, enemyText, new Vector2(10, 30), enemyColor * TransitionAlpha);

        // Difficulty
        var diffText = $"Difficulty: {DifficultyModifiers.GetDisplayName(_difficulty)}";
        _spriteBatch.DrawString(_font!, diffText, new Vector2(10, 50), Color.Yellow * TransitionAlpha);

        // Current message
        if (!string.IsNullOrEmpty(_currentMessage))
        {
            var msgSize = _titleFont!.MeasureString(_currentMessage);
            var msgPos = new Vector2(
                (screenWidth - msgSize.X) / 2,
                screenHeight / 3);

            // Background
            _spriteBatch.Draw(_pixel, new Rectangle(
                (int)msgPos.X - 10,
                (int)msgPos.Y - 5,
                (int)msgSize.X + 20,
                (int)msgSize.Y + 10),
                Color.Black * 0.7f * TransitionAlpha);

            _spriteBatch.DrawString(_titleFont, _currentMessage, msgPos, Color.White * TransitionAlpha);
        }

        // Controls hint
        var hint = "[WASD] Move | [ESC] Menu | [TAB] Inventory";
        _spriteBatch.DrawString(_font!, hint,
            new Vector2(10, screenHeight - 25),
            Color.Gray * 0.7f * TransitionAlpha);
    }
}

/// <summary>
/// Screen shown after completing or retreating from a dungeon exploration.
/// </summary>
public class DungeonResultsScreen : GameScreen
{
    private readonly DungeonDefinition _definition;
    private readonly DungeonInstance _instance;
    private readonly DungeonReward _rewards;

    private Texture2D? _pixel;
    private SpriteFont? _font;
    private SpriteFont? _titleFont;

    private bool _showingRewards;

    public event Action<DungeonReward>? OnExit;

    public DungeonResultsScreen(DungeonDefinition definition, DungeonInstance instance, DungeonReward rewards)
    {
        _definition = definition;
        _instance = instance;
        _rewards = rewards;

        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _pixel = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameFont");
        _titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/MenuFont");
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixel?.Dispose();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        base.HandleInput(gameTime, input);

        if (input.IsNewKeyPress(Keys.Enter, ControllingPlayer, out _) ||
            input.IsNewKeyPress(Keys.Space, ControllingPlayer, out _))
        {
            if (!_showingRewards)
            {
                _showingRewards = true;
            }
            else
            {
                OnExit?.Invoke(_rewards);
                ExitScreen();
            }
        }
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
            ScreenManager.GlobalTransformation);

        // Background
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, screenWidth, screenHeight),
            new Color(15, 18, 25) * TransitionAlpha);

        if (!_showingRewards)
        {
            DrawResults(spriteBatch, screenWidth, screenHeight);
        }
        else
        {
            DrawRewards(spriteBatch, screenWidth, screenHeight);
        }

        spriteBatch.End();
    }

    private void DrawResults(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        // Result title
        var resultText = _instance.State switch
        {
            DungeonState.Completed => "DUNGEON COMPLETE!",
            DungeonState.Failed => "DEFEATED",
            DungeonState.Retreated => "RETREATED",
            _ => "RUN ENDED"
        };
        var resultColor = _instance.State == DungeonState.Completed ? Color.Gold : Color.Red;

        var titleSize = _titleFont!.MeasureString(resultText);
        spriteBatch.DrawString(_titleFont, resultText,
            new Vector2((screenWidth - titleSize.X) / 2, 80),
            resultColor * TransitionAlpha);

        // Stats
        int y = 150;
        spriteBatch.DrawString(_font!, $"Dungeon: {_definition.Name}",
            new Vector2(50, y), Color.White * TransitionAlpha);
        y += 30;

        spriteBatch.DrawString(_font!, $"Rooms Cleared: {_instance.RoomsCleared}/{_instance.TotalRooms}",
            new Vector2(50, y), Color.White * TransitionAlpha);
        y += 25;

        spriteBatch.DrawString(_font!, $"Difficulty: {DifficultyModifiers.GetDisplayName(_instance.Difficulty)}",
            new Vector2(50, y), Color.Yellow * TransitionAlpha);
        y += 25;

        spriteBatch.DrawString(_font!, $"Time: {_instance.ElapsedTime:mm\\:ss}",
            new Vector2(50, y), Color.White * TransitionAlpha);
        y += 35;

        if (_instance.MidBossDefeated)
        {
            spriteBatch.DrawString(_font!, $"Mini-Boss: DEFEATED",
                new Vector2(50, y), Color.Green * TransitionAlpha);
            y += 25;
        }

        if (_instance.FinalBossDefeated)
        {
            spriteBatch.DrawString(_font!, $"Final Boss: DEFEATED",
                new Vector2(50, y), Color.Gold * TransitionAlpha);
        }

        // Continue prompt
        var prompt = "Press [ENTER] to see rewards";
        var promptSize = _font!.MeasureString(prompt);
        spriteBatch.DrawString(_font, prompt,
            new Vector2((screenWidth - promptSize.X) / 2, screenHeight - 60),
            Color.Yellow * TransitionAlpha);
    }

    private void DrawRewards(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        var titleSize = _titleFont!.MeasureString("REWARDS");
        spriteBatch.DrawString(_titleFont, "REWARDS",
            new Vector2((screenWidth - titleSize.X) / 2, 60),
            Color.Gold * TransitionAlpha);

        int y = 130;

        // Experience and currency
        spriteBatch.DrawString(_font!, $"Experience: +{_rewards.Experience}",
            new Vector2(50, y), Color.Cyan * TransitionAlpha);
        y += 30;

        spriteBatch.DrawString(_font!, $"Scrap: +{_rewards.Currency}",
            new Vector2(50, y), Color.Gold * TransitionAlpha);
        y += 40;

        // Items
        if (_rewards.Items.Count > 0 || _rewards.BonusItems.Count > 0)
        {
            spriteBatch.DrawString(_font!, "Items Obtained:", new Vector2(50, y), Color.White * TransitionAlpha);
            y += 25;

            foreach (var item in _rewards.Items)
            {
                var color = GetRarityColor(item.Rarity);
                spriteBatch.DrawString(_font!, $"  [{item.Rarity}] {item.Name} x{item.Quantity}",
                    new Vector2(60, y), color * TransitionAlpha);
                y += 22;
            }

            foreach (var item in _rewards.BonusItems)
            {
                var color = GetRarityColor(item.Rarity);
                spriteBatch.DrawString(_font!, $"  [{item.Rarity}] {item.Name} x{item.Quantity} (BONUS)",
                    new Vector2(60, y), color * TransitionAlpha);
                y += 22;
            }
        }

        // Exit prompt
        var prompt = "Press [ENTER] to claim rewards and exit";
        var promptSize = _font!.MeasureString(prompt);
        spriteBatch.DrawString(_font, prompt,
            new Vector2((screenWidth - promptSize.X) / 2, screenHeight - 60),
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
}
