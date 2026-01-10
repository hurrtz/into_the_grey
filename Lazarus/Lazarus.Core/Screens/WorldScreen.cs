using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lazarus.Core;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Dialog;
using Lazarus.Core.Game.Entities;
using Lazarus.Core.Game.Items;
using Lazarus.Core.Game.Progression;
using Lazarus.Core.Game.World;
using Lazarus.Core.Game.Dungeons;
using Lazarus.Core.Game.Story;
using Lazarus.Core.Inputs;
using Lazarus.Core.Services;
using Lazarus.ScreenManagers;

namespace Lazarus.Screens;

/// <summary>
/// The main gameplay screen for world exploration.
/// Handles protagonist movement, companion following, and encounter triggers.
/// </summary>
public class WorldScreen : GameScreen
{
    // Services
    private GameStateService _gameState = null!;
    private KynRoster _roster = null!;
    private QuestLog _questLog = null!;
    private RecruitmentManager _recruitmentManager = null!;

    // Game objects
    private GameWorld _world = null!;
    private Protagonist _protagonist = null!;
    private Companion _companion = null!;
    private List<Settlement> _settlements = new();
    private List<NPC> _wanderingNPCs = new();

    // Graphics
    private Texture2D? _pixelTexture;
    private SpriteFont? _font;

    // State
    private Encounter? _triggeredEncounter;
    private WildKyn? _triggeredWildKyn;
    private NPC? _nearbyNPC;
    private Settlement? _currentSettlement;
    private Kyn? _pendingRecruitment;
    private WildKyn? _pendingWildKynRecruitment;

    // Biome transition state
    private BiomeType _previousBiome;
    private float _biomeTransitionAlpha;
    private string _biomeTransitionText = "";

    // Portal interaction
    private BiomePortal? _nearbyPortal;

    // Dungeon portals
    private List<DungeonPortal> _dungeonPortals = new();
    private DungeonPortal? _nearbyDungeonPortal;

    // Building portals and interiors
    private List<BuildingPortal> _buildingPortals = new();
    private BuildingPortal? _nearbyBuildingPortal;
    private InteriorInstance? _currentInterior;
    private bool _isInInterior = false;

    // Wild kyn proximity (for manual interaction)
    private WildKyn? _nearbyWildKyn;

    // Weather particles
    private List<WeatherParticle> _weatherParticles = new();
    private float _weatherUpdateTimer;

    // Input
    private KeyboardState _previousKeyboardState;

    public WorldScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        var game = ScreenManager.Game;

        // Get or create GameStateService
        _gameState = game.Services.GetService<GameStateService>();
        if (_gameState == null)
        {
            _gameState = new GameStateService();
            game.Services.AddService(typeof(GameStateService), _gameState);
        }

        // Create roster
        _roster = new KynRoster();

        // Initialize game state if needed
        if (_gameState.Data.PartyKynIds.Count == 0)
        {
            _gameState.NewGame(CompanionType.Dog);
            _gameState.HasExoskeleton = true; // For testing, give exoskeleton immediately
            _gameState.ExoskeletonPowered = true; // And power it

            // Create a starter Kyn (Echo Pup)
            var echoPup = Kyn.Create("echo_pup", 5);
            if (echoPup != null)
            {
                _roster.AddKyn(echoPup);
            }

            // Trigger awakening dialog on first play
            _gameState.SetFlag("show_awakening_dialog");
        }

        // Create quest log
        _questLog = new QuestLog(_gameState);
        _questLog.QuestStarted += OnQuestStarted;
        _questLog.QuestCompleted += OnQuestCompleted;

        // Create recruitment manager
        _recruitmentManager = new RecruitmentManager(_gameState, _roster);

        // Create protagonist
        _protagonist = new Protagonist(_gameState);
        _protagonist.Position = _gameState.ProtagonistPosition;

        // Load protagonist sprites
        string contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
        _protagonist.LoadContent(ScreenManager.GraphicsDevice, contentPath);

        // Create companion
        _companion = new Companion(_gameState.CompanionType, _gameState);
        _companion.Position = _protagonist.Position + new Vector2(-40, 0);

        // Create world
        _world = new GameWorld(ScreenManager.GraphicsDevice, _gameState);
        _world.SetViewportSize(new Vector2(ScreenManager.BaseScreenSize.X, ScreenManager.BaseScreenSize.Y));
        _world.Initialize();

        // Load wild kyn sprites
        _world.LoadWildKynSprite(contentPath);

        // Subscribe to world events
        _world.BiomeChanged += OnBiomeChanged;
        _world.WeatherChanged += OnWeatherChanged;
        _previousBiome = _world.CurrentBiome;

        // Create settlements
        InitializeSettlements();

        // Initialize dungeons
        InitializeDungeons();

        // Initialize building portals
        InitializeBuildingPortals();

        // Initialize interior definitions
        InteriorDefinitions.Initialize();

        // Create pixel texture for placeholder graphics
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Load font
        _font = ScreenManager.Font;

        // Check for intro dialog
        if (_gameState.HasFlag("show_awakening_dialog"))
        {
            _gameState.ClearFlag("show_awakening_dialog");
            ShowDialog("awakening_intro");
        }
    }

    /// <summary>
    /// Initializes settlements in the world.
    /// </summary>
    private void InitializeSettlements()
    {
        // Create settlements based on current biome
        foreach (var def in SettlementDefinitions.All.Values)
        {
            if (def.Biome == _gameState.CurrentBiome)
            {
                var settlement = new Settlement(def, _gameState);
                // Position settlements in the world (for testing, spread them out)
                settlement.Position = GetSettlementPosition(def.Id);
                settlement.UpdateNPCPositions();
                settlement.PlayerEntered += OnPlayerEnteredSettlement;
                settlement.PlayerExited += OnPlayerExitedSettlement;
                _settlements.Add(settlement);
            }
        }
    }

    private Vector2 GetSettlementPosition(string settlementId)
    {
        // Temporary fixed positions for testing
        return settlementId switch
        {
            "fringe_camp" => new Vector2(600, 400),
            "nimdok_terminal_loc" => new Vector2(300, 600),
            _ => new Vector2(500, 500)
        };
    }

    /// <summary>
    /// Initializes dungeon system and portals.
    /// </summary>
    private void InitializeDungeons()
    {
        // Initialize dungeon definitions
        Dungeons.Initialize();

        // Create dungeon portals for the current biome
        _dungeonPortals.Clear();

        foreach (var dungeon in Dungeons.GetByBiome(_gameState.CurrentBiome))
        {
            // Place dungeon portals at specific locations
            var position = GetDungeonPortalPosition(dungeon.Id);
            var portal = DungeonPortal.Create(dungeon.Id, dungeon.Biome, position, dungeon.RequiredFlag);
            _dungeonPortals.Add(portal);
        }

        // Add portals for all biomes (they'll be filtered by current biome during update)
        foreach (var biome in Enum.GetValues<BiomeType>())
        {
            if (biome == _gameState.CurrentBiome) continue;

            foreach (var dungeon in Dungeons.GetByBiome(biome))
            {
                var position = GetDungeonPortalPosition(dungeon.Id);
                var portal = DungeonPortal.Create(dungeon.Id, dungeon.Biome, position, dungeon.RequiredFlag);
                _dungeonPortals.Add(portal);
            }
        }
    }

    /// <summary>
    /// Gets position for a dungeon portal based on its ID.
    /// </summary>
    private Vector2 GetDungeonPortalPosition(string dungeonId)
    {
        // Spread dungeons out in different areas of each biome
        return dungeonId switch
        {
            // Fringe dungeons
            "fringe_sewers" => new Vector2(200, 300),
            "fringe_warehouse" => new Vector2(700, 200),

            // Rust dungeons
            "rust_refinery" => new Vector2(-1800, 400),
            "rust_scrapyard" => new Vector2(-2100, 700),

            // Green dungeons
            "green_greenhouse" => new Vector2(800, -600),
            "green_laboratory" => new Vector2(1100, -900),

            // Quiet dungeons
            "quiet_bunker" => new Vector2(1600, 200),
            "quiet_cathedral" => new Vector2(2000, 500),

            // Teeth dungeons
            "teeth_ossuary" => new Vector2(400, 1200),
            "teeth_maw" => new Vector2(700, 1500),

            // Glow dungeons
            "glow_reactor" => new Vector2(2400, -200),
            "glow_nimdok_gate" => new Vector2(2700, -500),

            // Archive dungeons
            "archive_memory_banks" => new Vector2(-800, -800),
            "archive_core" => new Vector2(-1100, -1100),

            _ => new Vector2(500, 500)
        };
    }

    /// <summary>
    /// Initializes building portals for the current biome.
    /// </summary>
    private void InitializeBuildingPortals()
    {
        // Initialize static portal registry
        BuildingPortals.Initialize();

        // Get portals for current biome
        _buildingPortals.Clear();
        foreach (var portal in BuildingPortals.GetByBiome(_gameState.CurrentBiome))
        {
            _buildingPortals.Add(portal);
        }
    }

    /// <summary>
    /// Enters a building interior.
    /// </summary>
    private void EnterBuilding(BuildingPortal portal)
    {
        var interiorDef = InteriorDefinitions.Get(portal.InteriorId);
        if (interiorDef == null)
        {
            System.Diagnostics.Debug.WriteLine($"Interior not found: {portal.InteriorId}");
            return;
        }

        // Use transition screen for smooth entry
        TransitionScreen.ShowBuildingEntry(
            ScreenManager,
            portal.DisplayName,
            () => CompleteEnterBuilding(portal, interiorDef),
            ControllingPlayer
        );
    }

    /// <summary>
    /// Completes entering a building (called during transition midpoint).
    /// </summary>
    private void CompleteEnterBuilding(BuildingPortal portal, InteriorDefinition interiorDef)
    {
        // Create interior instance
        _currentInterior = new InteriorInstance(interiorDef, _gameState);
        _currentInterior.EntryPortal = portal;

        // Load the interior
        _currentInterior.Load(ScreenManager.GraphicsDevice);

        // Subscribe to exit events
        _currentInterior.ExitTriggered += OnInteriorExitTriggered;

        // Enter the interior
        _currentInterior.OnEnter();
        _isInInterior = true;

        System.Diagnostics.Debug.WriteLine($"Entered building: {portal.DisplayName}");
    }

    /// <summary>
    /// Exits the current interior.
    /// </summary>
    private void ExitInterior(InteriorExit exit)
    {
        if (_currentInterior == null) return;

        // Store data needed for transition
        var worldPos = _currentInterior.GetWorldExitPosition(exit);
        var interior = _currentInterior;

        // Use transition screen for smooth exit
        TransitionScreen.ShowBuildingExit(
            ScreenManager,
            () => CompleteExitInterior(interior, worldPos),
            ControllingPlayer
        );
    }

    /// <summary>
    /// Completes exiting an interior (called during transition midpoint).
    /// </summary>
    private void CompleteExitInterior(InteriorInstance interior, Vector2 worldPos)
    {
        // Clean up interior
        interior.ExitTriggered -= OnInteriorExitTriggered;
        interior.Unload();
        _currentInterior = null;
        _isInInterior = false;

        // Move protagonist to exit position
        _protagonist.Position = worldPos;
        _companion.Position = worldPos + new Vector2(-40, 0);

        // Update camera
        _world.TeleportTo(worldPos);

        System.Diagnostics.Debug.WriteLine($"Exited interior to position: {worldPos}");
    }

    /// <summary>
    /// Called when player triggers an interior exit.
    /// </summary>
    private void OnInteriorExitTriggered(InteriorExit exit)
    {
        ExitInterior(exit);
    }

    private void OnBiomeChanged(BiomeType oldBiome, BiomeType newBiome)
    {
        _previousBiome = oldBiome;
        _biomeTransitionAlpha = 1f;
        _biomeTransitionText = $"Entering {BiomeData.GetName(newBiome)}";
        _gameState.CurrentBiome = newBiome;

        // Notify quest system
        _questLog.NotifyReachedLocation($"biome_{newBiome}");

        System.Diagnostics.Debug.WriteLine($"Biome changed: {oldBiome} -> {newBiome}");
    }

    private void OnWeatherChanged(WeatherType newWeather)
    {
        // Clear existing particles
        _weatherParticles.Clear();
        System.Diagnostics.Debug.WriteLine($"Weather changed to: {newWeather}");
    }

    private void OnPlayerEnteredSettlement(object? sender, EventArgs e)
    {
        if (sender is Settlement settlement)
        {
            _currentSettlement = settlement;
            _questLog.NotifyReachedLocation(settlement.Id);
        }
    }

    private void OnPlayerExitedSettlement(object? sender, EventArgs e)
    {
        _currentSettlement = null;
    }

    private void OnQuestStarted(object? sender, Quest quest)
    {
        // Show notification
        System.Diagnostics.Debug.WriteLine($"Quest started: {quest.Name}");
    }

    private void OnQuestCompleted(object? sender, Quest quest)
    {
        // Show notification
        System.Diagnostics.Debug.WriteLine($"Quest completed: {quest.Name}");

        // Check if this is the final quest
        if (quest.Id == "main_29_ending")
        {
            TriggerGameEnding();
        }
    }

    /// <summary>
    /// Triggers the game ending sequence based on player choices.
    /// </summary>
    private void TriggerGameEnding()
    {
        // Create ending system and determine ending
        var endingSystem = new EndingSystem();

        // Collect game flags
        var gameFlags = new HashSet<string>();

        foreach (var flag in _gameState.Data.StoryFlags)
        {
            if (flag.Value)
            {
                gameFlags.Add(flag.Key);
            }
        }

        // Add the game_complete flag
        gameFlags.Add(StoryFlags.GameComplete);

        // Update ending state based on game progress
        if (!_gameState.CompanionPresent)
        {
            if (_gameState.HasFlag(StoryFlags.DefeatedHyperEvolvedBandit))
            {
                gameFlags.Add("companion_sacrificed");
                endingSystem.State.CompanionSacrificed = true;
            }
            else
            {
                gameFlags.Add("companion_departed");
                endingSystem.State.CompanionDeparted = true;
            }
        }
        else
        {
            gameFlags.Add("companion_alive");
        }

        // Determine ending
        var ending = endingSystem.DetermineEnding(gameFlags);

        // Show ending screen
        var endingScreen = EndingScreen.Show(ScreenManager, ending, endingSystem, ControllingPlayer);
        endingScreen.EndingComplete += OnEndingComplete;
    }

    /// <summary>
    /// Called when the ending sequence is complete.
    /// </summary>
    private void OnEndingComplete(object? sender, EventArgs e)
    {
        // Return to main menu
        LoadingScreen.Load(ScreenManager, false, null, new MainMenuScreen());
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (input == null)
            return;

        var keyboardState = Keyboard.GetState();

        // Check for pause - open full-screen game menu
        if (input.IsPauseGame(ControllingPlayer))
        {
            var menuScreen = new GameMenuScreen(
                _roster,
                _gameState,
                _gameState.FactionReputation,
                _world,
                _gameState.Bestiary);
            ScreenManager.AddScreen(menuScreen, ControllingPlayer);
            _previousKeyboardState = keyboardState;
            return;
        }

        // Check for interaction (E key)
        if (IsKeyPressed(keyboardState, Keys.E))
        {
            TryInteract();
        }

        // Check for quest log (Q key)
        if (IsKeyPressed(keyboardState, Keys.Q))
        {
            // Future: Open quest log screen
            System.Diagnostics.Debug.WriteLine(_questLog.GetProgressSummary());
        }

        // Check for world map (M key)
        if (IsKeyPressed(keyboardState, Keys.M))
        {
            OpenBiomeMap();
        }

        // Get movement input
        var movement = Vector2.Zero;

        // Keyboard input
        if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
            movement.Y -= 1;
        if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
            movement.Y += 1;
        if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            movement.X -= 1;
        if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            movement.X += 1;

        // Gamepad input (add to keyboard)
        var playerIndex = ControllingPlayer ?? PlayerIndex.One;
        var gamePadState = GamePad.GetState(playerIndex);
        if (gamePadState.IsConnected)
        {
            movement.X += gamePadState.ThumbSticks.Left.X;
            movement.Y -= gamePadState.ThumbSticks.Left.Y; // Y is inverted on gamepad

            // Gamepad interaction button (A)
            if (gamePadState.Buttons.A == ButtonState.Pressed)
            {
                TryInteract();
            }
        }

        // Handle movement based on current mode
        if (_isInInterior && _currentInterior != null)
        {
            // Interior movement
            UpdateInteriorMovement(gameTime, movement);
        }
        else
        {
            // World movement
            _protagonist.Update(gameTime, movement);
        }

        _previousKeyboardState = keyboardState;
    }

    private bool IsKeyPressed(KeyboardState current, Keys key)
    {
        return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    /// <summary>
    /// Attempts to interact with nearby NPCs or objects.
    /// </summary>
    private void TryInteract()
    {
        // If in interior, check for interior interactions
        if (_isInInterior && _currentInterior != null)
        {
            TryInteractInterior();
            return;
        }

        // Check for nearby building portal first
        if (_nearbyBuildingPortal != null && _nearbyBuildingPortal.CanUse(_gameState))
        {
            EnterBuilding(_nearbyBuildingPortal);
            return;
        }

        // Check for nearby biome portal
        if (_nearbyPortal != null && _world.IsPortalUnlocked(_nearbyPortal))
        {
            UsePortal(_nearbyPortal);
            return;
        }

        // Check for nearby dungeon portal
        if (_nearbyDungeonPortal != null && IsDungeonPortalUnlocked(_nearbyDungeonPortal))
        {
            EnterDungeon(_nearbyDungeonPortal);
            return;
        }

        // Check for nearby wild kyn
        if (_nearbyWildKyn != null)
        {
            InteractWithWildKyn(_nearbyWildKyn);
            return;
        }

        // Check for nearby NPC
        if (_nearbyNPC != null)
        {
            InteractWithNPC(_nearbyNPC);
            return;
        }

        // Check for settlement NPCs
        if (_currentSettlement != null)
        {
            var npc = _currentSettlement.GetNearestInteractableNPC(_protagonist.Position);
            if (npc != null)
            {
                InteractWithNPC(npc);
                return;
            }
        }
    }

    /// <summary>
    /// Attempts to interact with objects inside an interior.
    /// </summary>
    private void TryInteractInterior()
    {
        if (_currentInterior == null) return;

        // Check for exit collision
        var playerBounds = new Rectangle(
            (int)_currentInterior.PlayerPosition.X - 16,
            (int)_currentInterior.PlayerPosition.Y - 16,
            32, 32
        );

        var exit = _currentInterior.GetCollidingExit(playerBounds);
        if (exit != null)
        {
            _currentInterior.TriggerExit(exit);
            return;
        }

        // Check for NPC interaction
        var npc = _currentInterior.GetInteractableNpc(
            _currentInterior.PlayerPosition,
            _currentInterior.PlayerFacing
        );

        if (npc != null)
        {
            InteractWithNPC(npc);
        }
    }

    /// <summary>
    /// Checks if a dungeon portal is unlocked.
    /// </summary>
    private bool IsDungeonPortalUnlocked(DungeonPortal portal)
    {
        if (string.IsNullOrEmpty(portal.RequiredFlag)) return true;
        return _gameState.HasFlag(portal.RequiredFlag);
    }

    /// <summary>
    /// Enters a dungeon via its portal.
    /// </summary>
    private void EnterDungeon(DungeonPortal portal)
    {
        var dungeon = portal.GetDungeon();
        if (dungeon == null) return;

        var dungeonScreen = new DungeonScreen(dungeon, _roster, _gameState);
        dungeonScreen.OnDungeonExit += OnDungeonExit;
        ScreenManager.AddScreen(dungeonScreen, ControllingPlayer);
    }

    /// <summary>
    /// Called when exiting a dungeon.
    /// </summary>
    private void OnDungeonExit(DungeonReward? rewards)
    {
        if (rewards != null)
        {
            // Apply rewards
            // Experience would be applied to party Kyns
            foreach (var kyn in _roster.Party)
            {
                kyn.AddExperience(rewards.Experience / Math.Max(1, _roster.Party.Count));
            }

            // Add currency (would need a currency system)
            // _gameState.Currency += rewards.Currency;

            // Set completion flag
            // _gameState.SetFlag($"{dungeonId}_cleared");

            System.Diagnostics.Debug.WriteLine($"Dungeon rewards: {rewards.Experience} EXP, {rewards.Currency} Scrap, {rewards.TotalItemCount} items");
        }
    }

    /// <summary>
    /// Uses a portal to travel to another biome.
    /// </summary>
    private void UsePortal(BiomePortal portal)
    {
        var targetPosition = _world.UsePortal(portal);
        _protagonist.Position = targetPosition;
        _companion.Position = targetPosition + new Vector2(-40, 0);

        // Set visited flag
        var targetBiome = portal.ToBiome.ToString().ToLowerInvariant();
        _gameState.SetFlag($"visited_{targetBiome}");

        // Show transition effect
        _biomeTransitionAlpha = 1f;
        _biomeTransitionText = $"Entering {BiomeData.GetName(portal.ToBiome)}";
    }

    /// <summary>
    /// Opens the biome world map screen.
    /// </summary>
    private void OpenBiomeMap()
    {
        var mapScreen = new BiomeMapScreen(_world, _gameState);
        mapScreen.BiomeSelected += OnBiomeSelectedFromMap;
        ScreenManager.AddScreen(mapScreen, ControllingPlayer);
    }

    /// <summary>
    /// Called when player selects a biome from the map screen.
    /// </summary>
    private void OnBiomeSelectedFromMap(BiomeType targetBiome)
    {
        var portal = _world.GetPortalTo(targetBiome);
        if (portal != null)
        {
            UsePortal(portal);
        }
        else
        {
            // Direct travel (fast travel)
            var spawnPoint = _world.GetBiomeSpawnPoint(targetBiome);
            _protagonist.Position = spawnPoint;
            _companion.Position = spawnPoint + new Vector2(-40, 0);

            // Update world camera and chunk tracking
            _world.TeleportTo(spawnPoint);

            var targetBiomeName = targetBiome.ToString().ToLowerInvariant();
            _gameState.SetFlag($"visited_{targetBiomeName}");

            _biomeTransitionAlpha = 1f;
            _biomeTransitionText = $"Traveling to {BiomeData.GetName(targetBiome)}...";
        }
    }

    /// <summary>
    /// Initiates interaction with an NPC.
    /// </summary>
    private void InteractWithNPC(NPC npc)
    {
        // Check if merchant with shop
        if (npc.Type == NPCType.Merchant && !string.IsNullOrEmpty(npc.Definition.ShopId))
        {
            var shop = Shops.Get(npc.Definition.ShopId);
            if (shop != null)
            {
                OpenShop(npc, shop);
                return;
            }
        }

        // Check if healer - heal party and show confirmation
        if (npc.Type == NPCType.Healer)
        {
            // Heal the entire party
            _roster.HealParty();
            _roster.ReviveParty();

            // Show healing dialog
            npc.InConversation = true;
            var dialogScreen = ShowDialog("healer_service");
            if (dialogScreen != null)
            {
                dialogScreen.DialogEnded += (_, _) =>
                {
                    npc.InConversation = false;
                    _questLog.NotifyTalkedTo(npc.Id);
                };
            }
            else
            {
                // Fallback if no dialog exists - just end conversation
                npc.InConversation = false;
            }
            return;
        }

        // Default: open dialog
        var dialogId = npc.GetCurrentDialogId();
        if (!string.IsNullOrEmpty(dialogId))
        {
            npc.InConversation = true;
            var dialogScreen = ShowDialog(dialogId);
            if (dialogScreen != null)
            {
                dialogScreen.DialogEnded += (_, _) =>
                {
                    npc.InConversation = false;
                    _questLog.NotifyTalkedTo(npc.Id);
                };
            }
        }
    }

    /// <summary>
    /// Opens a shop for trading.
    /// </summary>
    private void OpenShop(NPC npc, ShopDefinition shop)
    {
        npc.InConversation = true;

        var tradingScreen = new TradingScreen(shop, _gameState, _gameState.FactionReputation);
        ScreenManager.AddScreen(tradingScreen, ControllingPlayer);

        // When trading screen closes, end conversation
        tradingScreen.Closed += (_, _) =>
        {
            npc.InConversation = false;
            _questLog.NotifyTalkedTo(npc.Id);
        };
    }

    /// <summary>
    /// Shows a dialog screen.
    /// </summary>
    private DialogScreen? ShowDialog(string dialogId)
    {
        return DialogScreen.Show(ScreenManager, dialogId, ControllingPlayer);
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (coveredByOtherScreen || otherScreenHasFocus)
            return;

        // Handle interior mode separately
        if (_isInInterior && _currentInterior != null)
        {
            UpdateInterior(gameTime);
            return;
        }

        // Update companion
        _companion.Update(gameTime, _protagonist.Position, _protagonist.Facing);

        // Update world
        _world.Update(gameTime, _protagonist.Position);

        // Update portals and check for nearby portal
        _world.UpdatePortals(gameTime, _protagonist.BoundingBox);
        UpdateNearbyPortal();
        UpdateNearbyDungeonPortal();
        UpdateNearbyBuildingPortal();

        // Update building portals
        foreach (var portal in _buildingPortals)
        {
            portal.Update(gameTime, _protagonist.BoundingBox);
        }

        // Update settlements
        foreach (var settlement in _settlements)
        {
            settlement.Update(gameTime, _protagonist.Position);
        }

        // Update wandering NPCs
        foreach (var npc in _wanderingNPCs)
        {
            npc.Update(gameTime);
        }

        // Find nearby NPCs for interaction prompt
        UpdateNearbyNPC();

        // Update play time
        _gameState.UpdatePlayTime(gameTime);

        // Check for encounter collisions (only if not in safe zone)
        if (_currentSettlement == null || !_currentSettlement.IsSafeZone)
        {
            CheckEncounters();
        }

        // Update nearby wild kyn for interaction prompt (always check, but interaction requires E key)
        UpdateNearbyWildKyn();

        // Update biome transition
        if (_biomeTransitionAlpha > 0)
        {
            _biomeTransitionAlpha -= (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;
        }

        // Update weather particles
        UpdateWeatherParticles(gameTime);
    }

    /// <summary>
    /// Updates state when inside an interior.
    /// </summary>
    private void UpdateInterior(GameTime gameTime)
    {
        if (_currentInterior == null) return;

        // Update interior
        _currentInterior.Update(gameTime);

        // Update play time
        _gameState.UpdatePlayTime(gameTime);

        // Update biome transition
        if (_biomeTransitionAlpha > 0)
        {
            _biomeTransitionAlpha -= (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;
        }
    }

    /// <summary>
    /// Updates interior player movement.
    /// </summary>
    private void UpdateInteriorMovement(GameTime gameTime, Vector2 movement)
    {
        if (_currentInterior == null) return;

        if (movement.LengthSquared() > 0)
        {
            movement.Normalize();

            // Apply movement speed
            float speed = 150f; // Walk speed
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Move player in interior
            var newPos = _currentInterior.PlayerPosition + movement * speed * deltaTime;

            // Clamp to interior bounds
            var interiorSize = _currentInterior.Definition.Size;
            newPos.X = MathHelper.Clamp(newPos.X, 20, interiorSize.X - 20);
            newPos.Y = MathHelper.Clamp(newPos.Y, 20, interiorSize.Y - 20);

            _currentInterior.PlayerPosition = newPos;

            // Update facing direction
            _currentInterior.PlayerFacing = GetDirectionFromMovement(movement);
        }
    }

    /// <summary>
    /// Gets a Direction from a movement vector.
    /// </summary>
    private static Direction GetDirectionFromMovement(Vector2 movement)
    {
        if (movement.LengthSquared() < 0.01f)
            return Direction.South;

        float angle = MathF.Atan2(movement.Y, movement.X);
        float degrees = MathHelper.ToDegrees(angle);

        // Convert angle to 8-direction
        if (degrees >= -22.5f && degrees < 22.5f)
            return Direction.East;
        else if (degrees >= 22.5f && degrees < 67.5f)
            return Direction.SouthEast;
        else if (degrees >= 67.5f && degrees < 112.5f)
            return Direction.South;
        else if (degrees >= 112.5f && degrees < 157.5f)
            return Direction.SouthWest;
        else if (degrees >= 157.5f || degrees < -157.5f)
            return Direction.West;
        else if (degrees >= -157.5f && degrees < -112.5f)
            return Direction.NorthWest;
        else if (degrees >= -112.5f && degrees < -67.5f)
            return Direction.North;
        else
            return Direction.NorthEast;
    }

    /// <summary>
    /// Updates the nearby building portal reference.
    /// </summary>
    private void UpdateNearbyBuildingPortal()
    {
        _nearbyBuildingPortal = null;

        foreach (var portal in _buildingPortals)
        {
            if (!portal.IsActive) continue;
            if (portal.Biome != _world.CurrentBiome) continue;

            if (portal.IsPlayerOverlapping)
            {
                _nearbyBuildingPortal = portal;
                return;
            }
        }
    }

    /// <summary>
    /// Updates weather particles based on current weather.
    /// </summary>
    private void UpdateWeatherParticles(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _weatherUpdateTimer += deltaTime;

        var weather = _world.CurrentWeather;
        var screenSize = ScreenManager.BaseScreenSize;
        var random = new Random();

        // Spawn new particles
        if (_weatherUpdateTimer >= 0.05f)
        {
            _weatherUpdateTimer = 0f;

            switch (weather)
            {
                case WeatherType.Rain:
                case WeatherType.AcidRain:
                    for (int i = 0; i < 5; i++)
                    {
                        _weatherParticles.Add(new WeatherParticle
                        {
                            Position = new Vector2(random.Next((int)screenSize.X), -10),
                            Velocity = new Vector2(random.Next(-20, 20), 400 + random.Next(100)),
                            Color = weather == WeatherType.AcidRain ? Color.LimeGreen * 0.5f : Color.LightBlue * 0.5f,
                            Size = new Vector2(2, 8)
                        });
                    }
                    break;

                case WeatherType.Snow:
                    for (int i = 0; i < 2; i++)
                    {
                        _weatherParticles.Add(new WeatherParticle
                        {
                            Position = new Vector2(random.Next((int)screenSize.X), -10),
                            Velocity = new Vector2(random.Next(-30, 30), 50 + random.Next(50)),
                            Color = Color.White * 0.7f,
                            Size = new Vector2(3, 3)
                        });
                    }
                    break;

                case WeatherType.Dust:
                    if (random.NextDouble() < 0.3f)
                    {
                        _weatherParticles.Add(new WeatherParticle
                        {
                            Position = new Vector2(-10, random.Next((int)screenSize.Y)),
                            Velocity = new Vector2(150 + random.Next(100), random.Next(-20, 20)),
                            Color = Color.SandyBrown * 0.3f,
                            Size = new Vector2(4 + random.Next(4), 4 + random.Next(4))
                        });
                    }
                    break;

                case WeatherType.DataStorm:
                    for (int i = 0; i < 3; i++)
                    {
                        _weatherParticles.Add(new WeatherParticle
                        {
                            Position = new Vector2(random.Next((int)screenSize.X), random.Next((int)screenSize.Y)),
                            Velocity = new Vector2(random.Next(-50, 50), random.Next(-50, 50)),
                            Color = Color.Cyan * 0.4f,
                            Size = new Vector2(1, 1),
                            Lifetime = 0.5f + (float)random.NextDouble() * 0.5f
                        });
                    }
                    break;
            }
        }

        // Update particles
        for (int i = _weatherParticles.Count - 1; i >= 0; i--)
        {
            var particle = _weatherParticles[i];
            particle.Position += particle.Velocity * deltaTime;
            particle.Lifetime -= deltaTime;

            // Remove particles off screen or expired
            if (particle.Position.Y > screenSize.Y + 10 ||
                particle.Position.X > screenSize.X + 10 ||
                particle.Position.X < -10 ||
                particle.Lifetime <= 0)
            {
                _weatherParticles.RemoveAt(i);
            }
        }

        // Limit particle count
        while (_weatherParticles.Count > 500)
        {
            _weatherParticles.RemoveAt(0);
        }
    }

    /// <summary>
    /// Updates the nearby NPC reference for interaction prompts.
    /// </summary>
    private void UpdateNearbyNPC()
    {
        _nearbyNPC = null;
        float nearestDistance = float.MaxValue;

        // Check wandering NPCs
        foreach (var npc in _wanderingNPCs)
        {
            if (!npc.IsVisible || !npc.CanInteract)
                continue;

            float distance = Vector2.Distance(_protagonist.Position, npc.Position);
            if (npc.IsInRange(_protagonist.Position) && distance < nearestDistance)
            {
                _nearbyNPC = npc;
                nearestDistance = distance;
            }
        }

        // Settlement NPCs are handled separately via _currentSettlement
    }

    /// <summary>
    /// Updates the nearby portal reference for interaction prompts.
    /// </summary>
    private void UpdateNearbyPortal()
    {
        _nearbyPortal = null;

        // Check all portals that player is colliding with
        var collidingPortals = _world.GetCollidingPortals(_protagonist.BoundingBox);
        _nearbyPortal = collidingPortals.FirstOrDefault(p => p.IsActive);
    }

    /// <summary>
    /// Updates the nearby dungeon portal reference for interaction prompts.
    /// </summary>
    private void UpdateNearbyDungeonPortal()
    {
        _nearbyDungeonPortal = null;

        // Check all dungeon portals in current biome
        foreach (var portal in _dungeonPortals)
        {
            if (portal.Biome != _world.CurrentBiome) continue;
            if (!portal.IsActive) continue;

            // Check collision
            if (_protagonist.BoundingBox.Intersects(portal.Bounds))
            {
                _nearbyDungeonPortal = portal;
                return;
            }
        }
    }

    /// <summary>
    /// Checks if the protagonist has collided with any encounters.
    /// </summary>
    private void CheckEncounters()
    {
        if (_triggeredEncounter != null)
            return; // Already in combat

        var collidingEncounters = _world.GetCollidingEncounters(_protagonist.BoundingBox);
        var encounter = collidingEncounters.FirstOrDefault();

        if (encounter != null && !_gameState.IsEncounterCleared(encounter.Id))
        {
            _triggeredEncounter = encounter;
            StartCombat(encounter);
        }
    }

    /// <summary>
    /// Updates the nearby wild kyn reference for interaction prompts.
    /// Does NOT automatically trigger interaction - player must press E.
    /// </summary>
    private void UpdateNearbyWildKyn()
    {
        _nearbyWildKyn = null;

        if (_triggeredWildKyn != null || _triggeredEncounter != null)
            return; // Already in combat or interacting

        var collidingWildKyns = _world.GetCollidingWildKyns(_protagonist.BoundingBox);
        var wildKyn = collidingWildKyns.FirstOrDefault();

        if (wildKyn != null && !_gameState.IsWildKynRecruited(wildKyn.Id))
        {
            _nearbyWildKyn = wildKyn;
        }
    }

    /// <summary>
    /// Initiates interaction with a wild kyn (called when player presses E).
    /// </summary>
    private void InteractWithWildKyn(WildKyn wildKyn)
    {
        _triggeredWildKyn = wildKyn;

        if (wildKyn.IsDefeated || _gameState.IsWildKynDefeated(wildKyn.Id))
        {
            // Already defeated - go directly to recruitment
            wildKyn.IsDefeated = true;
            ShowWildKynRecruitment(wildKyn);
        }
        else
        {
            // First encounter - must fight
            StartWildKynCombat(wildKyn);
        }
    }

    /// <summary>
    /// Starts combat with a wild kyn.
    /// </summary>
    private void StartWildKynCombat(WildKyn wildKyn)
    {
        // Create enemy kyn for combat
        var enemyKyn = wildKyn.CreateKynForCombat();
        if (enemyKyn == null)
        {
            _triggeredWildKyn = null;
            return;
        }

        var enemies = new List<Kyn> { enemyKyn };

        // Create and add combat screen (no encounter, combat is with wild kyn)
        var combatScreen = new CombatScreen(_roster.Party.ToList(), enemies, null, _companion, _gameState);
        combatScreen.CombatEnded += OnWildKynCombatEnded;
        ScreenManager.AddScreen(combatScreen, ControllingPlayer);
    }

    /// <summary>
    /// Called when combat with a wild kyn ends.
    /// </summary>
    private void OnWildKynCombatEnded(object? sender, CombatEndedEventArgs e)
    {
        if (e.Victory && _triggeredWildKyn != null)
        {
            // Mark wild kyn as defeated
            _triggeredWildKyn.IsDefeated = true;
            _gameState.DefeatWildKyn(_triggeredWildKyn.Id);
            _world.DefeatWildKyn(_triggeredWildKyn.Id);

            // Award experience
            _roster.AwardExperience(e.ExperienceEarned);

            // Award currency
            _gameState.AddCurrency(e.CurrencyEarned);

            // Track battle won
            _gameState.RecordBattleWon();

            // Show victory screen
            var entityName = _triggeredWildKyn.KynDefinitionId;
            ShowVictoryScreen(e.ExperienceEarned, e.CurrencyEarned, e.TelemetryUnitsEarned, entityName);
        }
        else if (e.Fled)
        {
            // Move protagonist away
            _protagonist.ApplyCollisionPush(new Vector2(0, -50));
            _triggeredWildKyn = null;
        }
        else if (e.Defeat)
        {
            // Game over handling - heal and respawn
            _roster.HealParty();
            _roster.ReviveParty();
            _triggeredWildKyn = null;
        }
    }

    /// <summary>
    /// Shows the victory screen after combat.
    /// </summary>
    private void ShowVictoryScreen(int experience, int currency, int telemetryUnits, string? entityName = null)
    {
        var victoryScreen = new VictoryScreen(experience, currency, telemetryUnits, entityName);
        victoryScreen.Dismissed += OnVictoryScreenDismissed;
        ScreenManager.AddScreen(victoryScreen, ControllingPlayer);
    }

    /// <summary>
    /// Called when the victory screen is dismissed.
    /// </summary>
    private void OnVictoryScreenDismissed(object? sender, EventArgs e)
    {
        // For wild kyns, don't auto-show recruitment - player must re-approach
        _triggeredWildKyn = null;
        _triggeredEncounter = null;
    }

    /// <summary>
    /// Shows the recruitment screen for a defeated wild kyn.
    /// </summary>
    private void ShowWildKynRecruitment(WildKyn wildKyn)
    {
        var kyn = wildKyn.CreateKynForRecruitment();
        if (kyn == null)
        {
            _triggeredWildKyn = null;
            return;
        }

        _pendingWildKynRecruitment = wildKyn;
        var recruitScreen = new RecruitmentScreen(kyn, _recruitmentManager);
        recruitScreen.RecruitmentComplete += OnWildKynRecruitmentComplete;
        ScreenManager.AddScreen(recruitScreen, ControllingPlayer);
    }

    /// <summary>
    /// Called when wild kyn recruitment completes.
    /// </summary>
    private void OnWildKynRecruitmentComplete(object? sender, RecruitmentResult result)
    {
        if (result == RecruitmentResult.Success && _pendingWildKynRecruitment != null)
        {
            // Mark as recruited - remove from world
            _gameState.RecruitWildKyn(_pendingWildKynRecruitment.Id);
            _world.RecruitWildKyn(_pendingWildKynRecruitment.Id);
            _questLog.NotifyRecruitedKyn(_pendingWildKynRecruitment.KynDefinitionId);
        }

        // Clear state - if refused, player can re-approach
        _pendingWildKynRecruitment = null;
        _triggeredWildKyn = null;
    }

    /// <summary>
    /// Starts combat with an encounter.
    /// </summary>
    private void StartCombat(Encounter encounter)
    {
        // Generate enemy party
        var random = new Random();
        var enemyData = encounter.GenerateEnemyParty(random);

        var enemies = enemyData
            .Select(e => Kyn.Create(e.DefinitionId, e.Level))
            .Where(s => s != null)
            .Cast<Kyn>()
            .ToList();

        // If no valid enemies, create generic ones
        if (enemies.Count == 0)
        {
            for (int i = 0; i < encounter.EnemyCount; i++)
            {
                var wildKyn = Kyn.Create("wild_kyn", random.Next(encounter.LevelRange.Min, encounter.LevelRange.Max + 1));
                if (wildKyn != null)
                    enemies.Add(wildKyn);
            }
        }

        // Create and add combat screen
        var combatScreen = new CombatScreen(_roster.Party.ToList(), enemies, encounter, _companion, _gameState);
        combatScreen.CombatEnded += OnCombatEnded;
        ScreenManager.AddScreen(combatScreen, ControllingPlayer);
    }

    /// <summary>
    /// Called when combat with an encounter ends.
    /// </summary>
    private void OnCombatEnded(object? sender, CombatEndedEventArgs e)
    {
        if (e.Victory && _triggeredEncounter != null)
        {
            // Mark encounter as cleared
            _world.ClearEncounter(_triggeredEncounter.Id);

            // Notify quest system
            _questLog.NotifyDefeatedEncounter(_triggeredEncounter.Id);

            // Award experience
            _roster.AwardExperience(e.ExperienceEarned);

            // Award currency
            _gameState.AddCurrency(e.CurrencyEarned);

            // Track battle won
            _gameState.RecordBattleWon();

            // Show victory screen (encounters are hostile enemies - no recruitment)
            ShowVictoryScreen(e.ExperienceEarned, e.CurrencyEarned, e.TelemetryUnitsEarned, "Enemy");
        }
        else if (e.Fled)
        {
            // Move protagonist away from encounter
            _protagonist.ApplyCollisionPush(new Vector2(0, -50));
            _triggeredEncounter = null;
        }
        else if (e.Defeat)
        {
            // Game over handling - for now, just heal and respawn
            _roster.HealParty();
            _roster.ReviveParty();
            _triggeredEncounter = null;
        }
    }

    /// <summary>
    /// Shows the recruitment screen for a defeated Kyn.
    /// </summary>
    private void ShowRecruitmentScreen(Kyn kyn)
    {
        var recruitScreen = new RecruitmentScreen(kyn, _recruitmentManager);
        recruitScreen.RecruitmentComplete += OnRecruitmentComplete;
        ScreenManager.AddScreen(recruitScreen, ControllingPlayer);
    }

    private void OnRecruitmentComplete(object? sender, RecruitmentResult result)
    {
        if (result == RecruitmentResult.Success && _pendingRecruitment != null)
        {
            _questLog.NotifyRecruitedKyn(_pendingRecruitment.Definition.Id);
        }
        _pendingRecruitment = null;
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var graphicsDevice = ScreenManager.GraphicsDevice;

        // Handle interior drawing separately
        if (_isInInterior && _currentInterior != null)
        {
            DrawInterior(spriteBatch, graphicsDevice);
            return;
        }

        // Clear with biome background color
        graphicsDevice.Clear(BiomeData.GetBackgroundColor(_gameState.CurrentBiome));

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        // Draw world (map, encounters)
        _world.Draw(spriteBatch, _pixelTexture!, _font);

        // Draw portals
        _world.DrawPortals(spriteBatch, _pixelTexture!, _font);

        // Draw dungeon portals
        DrawDungeonPortals(spriteBatch);

        // Draw building portals
        DrawBuildingPortals(spriteBatch);

        // Draw settlements
        foreach (var settlement in _settlements)
        {
            settlement.Draw(spriteBatch, _pixelTexture!, _font, _world.CameraPosition);
        }

        // Draw wandering NPCs
        foreach (var npc in _wanderingNPCs)
        {
            npc.Draw(spriteBatch, _pixelTexture!, _font, _world.CameraPosition);
        }

        // Draw companion (behind protagonist)
        _companion.Draw(spriteBatch, _pixelTexture!, _world.CameraPosition);

        // Draw protagonist
        _protagonist.Draw(spriteBatch, _pixelTexture!, _world.CameraPosition);

        // Draw weather effects
        DrawWeather(spriteBatch);

        // Draw biome transition overlay
        DrawBiomeTransition(spriteBatch);

        // Draw UI overlay
        DrawUI(spriteBatch);

        spriteBatch.End();
    }

    /// <summary>
    /// Draws the current interior.
    /// </summary>
    private void DrawInterior(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        if (_currentInterior == null || _pixelTexture == null) return;

        // Clear with interior background color
        var interiorDef = _currentInterior.Definition;
        graphicsDevice.Clear(interiorDef.PlaceholderColor * 0.3f);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        // Calculate camera offset (center on player)
        var viewportSize = ScreenManager.BaseScreenSize;
        var cameraOffset = _currentInterior.PlayerPosition - viewportSize / 2;

        // Draw interior
        _currentInterior.Draw(spriteBatch, _pixelTexture, _font, cameraOffset, viewportSize);

        // Draw protagonist in interior
        var screenPos = _currentInterior.PlayerPosition - cameraOffset;
        var playerRect = new Rectangle(
            (int)(screenPos.X - 16),
            (int)(screenPos.Y - 24),
            32, 48
        );
        spriteBatch.Draw(_pixelTexture, playerRect, Color.Blue);

        // Draw biome transition overlay
        DrawBiomeTransition(spriteBatch);

        // Draw interior UI
        DrawInteriorUI(spriteBatch);

        spriteBatch.End();
    }

    /// <summary>
    /// Draws UI when inside an interior.
    /// </summary>
    private void DrawInteriorUI(SpriteBatch spriteBatch)
    {
        if (_font == null || _pixelTexture == null || _currentInterior == null) return;

        // Draw interior name at top
        var interiorName = _currentInterior.Definition.Name;
        var nameSize = _font.MeasureString(interiorName);
        var namePos = new Vector2(
            (ScreenManager.BaseScreenSize.X - nameSize.X) / 2,
            10
        );

        // Background
        var bgRect = new Rectangle(
            (int)namePos.X - 10,
            (int)namePos.Y - 5,
            (int)nameSize.X + 20,
            (int)nameSize.Y + 10
        );
        spriteBatch.Draw(_pixelTexture, bgRect, Color.Black * 0.7f);
        spriteBatch.DrawString(_font, interiorName, namePos, Color.White);

        // Draw hint
        var hint = "WASD: Move | E: Interact/Exit | ESC: Pause";
        var hintPos = new Vector2(10, ScreenManager.BaseScreenSize.Y - 20);
        spriteBatch.DrawString(_font, hint, hintPos, Color.Gray);
    }

    /// <summary>
    /// Draws building portals in the world.
    /// </summary>
    private void DrawBuildingPortals(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null || _font == null) return;

        foreach (var portal in _buildingPortals)
        {
            // Only draw portals in current biome
            if (portal.Biome != _world.CurrentBiome) continue;
            if (!portal.IsActive) continue;

            // Draw the portal
            portal.Draw(spriteBatch, _pixelTexture, _world.CameraPosition, _font);
        }
    }

    /// <summary>
    /// Draws dungeon portals in the world.
    /// </summary>
    private void DrawDungeonPortals(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null || _font == null) return;

        foreach (var portal in _dungeonPortals)
        {
            // Only draw portals in current biome
            if (portal.Biome != _world.CurrentBiome) continue;
            if (!portal.IsActive) continue;

            // Get screen position
            var screenPos = portal.Position - _world.CameraPosition;

            // Skip if off screen
            if (screenPos.X < -50 || screenPos.X > ScreenManager.BaseScreenSize.X + 50 ||
                screenPos.Y < -50 || screenPos.Y > ScreenManager.BaseScreenSize.Y + 50)
                continue;

            // Draw portal base (dark purple circle)
            var portalRect = new Rectangle(
                (int)(screenPos.X - 20),
                (int)(screenPos.Y - 20),
                40, 40
            );
            spriteBatch.Draw(_pixelTexture, portalRect, new Color(80, 40, 120) * 0.8f);

            // Draw inner glow
            var innerRect = new Rectangle(
                (int)(screenPos.X - 12),
                (int)(screenPos.Y - 12),
                24, 24
            );
            var isUnlocked = IsDungeonPortalUnlocked(portal);
            var glowColor = isUnlocked ? new Color(150, 100, 200) : new Color(100, 50, 50);
            spriteBatch.Draw(_pixelTexture, innerRect, glowColor);

            // Draw dungeon icon (D)
            var iconText = "D";
            var iconSize = _font.MeasureString(iconText);
            spriteBatch.DrawString(_font, iconText,
                screenPos - iconSize / 2,
                isUnlocked ? Color.White : Color.Gray);

            // Draw name label if nearby
            if (portal == _nearbyDungeonPortal)
            {
                var dungeon = portal.GetDungeon();
                if (dungeon != null)
                {
                    var labelText = dungeon.Name;
                    var labelSize = _font.MeasureString(labelText);
                    var labelPos = new Vector2(
                        screenPos.X - labelSize.X / 2,
                        screenPos.Y - 40
                    );

                    // Background
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle((int)labelPos.X - 4, (int)labelPos.Y - 2,
                            (int)labelSize.X + 8, (int)labelSize.Y + 4),
                        Color.Black * 0.7f);

                    // Text
                    spriteBatch.DrawString(_font, labelText, labelPos,
                        isUnlocked ? Color.Magenta : Color.Red);
                }
            }
        }
    }

    /// <summary>
    /// Draws weather effects.
    /// </summary>
    private void DrawWeather(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null)
            return;

        // Draw weather overlay tint
        var weather = _world.CurrentWeather;
        var screenSize = ScreenManager.BaseScreenSize;
        var overlayColor = GetWeatherOverlayColor(weather);

        if (overlayColor.A > 0)
        {
            var overlayRect = new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y);
            spriteBatch.Draw(_pixelTexture, overlayRect, overlayColor);
        }

        // Draw weather particles
        foreach (var particle in _weatherParticles)
        {
            var rect = new Rectangle(
                (int)particle.Position.X,
                (int)particle.Position.Y,
                (int)particle.Size.X,
                (int)particle.Size.Y
            );
            spriteBatch.Draw(_pixelTexture, rect, particle.Color);
        }
    }

    /// <summary>
    /// Gets the overlay color for weather effects.
    /// </summary>
    private Color GetWeatherOverlayColor(WeatherType weather)
    {
        return weather switch
        {
            WeatherType.Fog => new Color(200, 200, 200, 60),
            WeatherType.Rain => new Color(50, 50, 80, 30),
            WeatherType.AcidRain => new Color(50, 100, 50, 40),
            WeatherType.Dust => new Color(139, 119, 101, 40),
            WeatherType.Snow => new Color(230, 240, 255, 20),
            WeatherType.DataStorm => new Color(0, 80, 100, 30),
            WeatherType.RadiationWind => new Color(100, 100, 50, 30),
            _ => Color.Transparent
        };
    }

    /// <summary>
    /// Draws biome transition overlay.
    /// </summary>
    private void DrawBiomeTransition(SpriteBatch spriteBatch)
    {
        if (_biomeTransitionAlpha <= 0 || _pixelTexture == null || _font == null)
            return;

        var screenSize = ScreenManager.BaseScreenSize;

        // Fade between biome colors
        var fromColor = BiomeData.GetBackgroundColor(_previousBiome);
        var toColor = BiomeData.GetBackgroundColor(_world.CurrentBiome);
        var transitionColor = Color.Lerp(toColor, fromColor, _biomeTransitionAlpha) * (_biomeTransitionAlpha * 0.5f);

        // Draw edge vignette
        var vignetteRect = new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y);
        spriteBatch.Draw(_pixelTexture, vignetteRect, transitionColor);

        // Draw biome name text
        if (!string.IsNullOrEmpty(_biomeTransitionText))
        {
            var textSize = _font.MeasureString(_biomeTransitionText);
            var textPos = new Vector2(
                screenSize.X / 2 - textSize.X / 2,
                screenSize.Y / 3
            );

            // Draw shadow
            spriteBatch.DrawString(_font, _biomeTransitionText,
                textPos + new Vector2(2, 2), Color.Black * _biomeTransitionAlpha);

            // Draw text
            spriteBatch.DrawString(_font, _biomeTransitionText,
                textPos, Color.White * _biomeTransitionAlpha);

            // Draw biome theme
            var themeName = BiomeData.GetTheme(_world.CurrentBiome);
            var themeSize = _font.MeasureString(themeName);
            var themePos = new Vector2(
                screenSize.X / 2 - themeSize.X / 2,
                textPos.Y + textSize.Y + 10
            );
            spriteBatch.DrawString(_font, themeName,
                themePos, Color.LightGray * _biomeTransitionAlpha);
        }
    }

    /// <summary>
    /// Draws the UI overlay.
    /// </summary>
    private void DrawUI(SpriteBatch spriteBatch)
    {
        if (_font == null || _pixelTexture == null)
            return;

        // Draw party status
        float y = 10;
        foreach (var kyn in _roster.Party)
        {
            var text = $"{kyn.DisplayName} HP:{kyn.CurrentHp}/{kyn.MaxHp} Lv{kyn.Level}";
            spriteBatch.DrawString(_font, text, new Vector2(10, y), Color.White);
            y += 20;
        }

        // Draw companion status
        if (_companion.IsPresent)
        {
            var companionText = $"{_companion.Name} (Companion)";
            if (_companion.GravitationStage > GravitationStage.Normal)
            {
                companionText += $" [!{_companion.GravitationStage}]";
            }
            spriteBatch.DrawString(_font, companionText, new Vector2(10, y), Color.Orange);
            y += 20;
        }

        // Draw tracked quest
        var trackedQuest = _questLog.TrackedQuest;
        if (trackedQuest != null)
        {
            y += 10;
            var questText = $"Quest: {trackedQuest.Name}";
            spriteBatch.DrawString(_font, questText, new Vector2(10, y), Color.Yellow);
            y += 20;
            var progressText = trackedQuest.GetProgressDescription();
            spriteBatch.DrawString(_font, progressText, new Vector2(20, y), Color.LightGray);
        }

        // Draw current biome
        var biomeName = BiomeData.GetName(_world.CurrentBiome);
        var biomePos = new Vector2(ScreenManager.BaseScreenSize.X - _font.MeasureString(biomeName).X - 10, 10);
        spriteBatch.DrawString(_font, biomeName, biomePos, Color.White);

        // Draw weather
        var weather = _world.CurrentWeather;
        if (weather != WeatherType.None)
        {
            var weatherText = GetWeatherDisplayName(weather);
            var weatherPos = new Vector2(ScreenManager.BaseScreenSize.X - _font.MeasureString(weatherText).X - 10, 30);
            spriteBatch.DrawString(_font, weatherText, weatherPos, GetWeatherTextColor(weather));
        }

        // Draw play time
        var playTime = _gameState.GetFormattedPlayTime();
        var timePos = new Vector2(ScreenManager.BaseScreenSize.X - _font.MeasureString(playTime).X - 10,
            weather != WeatherType.None ? 50 : 30);
        spriteBatch.DrawString(_font, playTime, timePos, Color.LightGray);

        // Draw settlement name if inside one
        if (_currentSettlement != null)
        {
            var settlementText = $"[{_currentSettlement.Name}]";
            var settlementSize = _font.MeasureString(settlementText);
            var settlementPos = new Vector2(ScreenManager.BaseScreenSize.X - settlementSize.X - 10, 50);
            spriteBatch.DrawString(_font, settlementText, settlementPos, Color.LimeGreen);
        }

        // Draw interaction prompt if near NPC or portal
        bool canInteractNPC = _nearbyNPC != null ||
            (_currentSettlement?.GetNearestInteractableNPC(_protagonist.Position) != null);
        bool canInteractPortal = _nearbyPortal != null;

        if (canInteractPortal)
        {
            var portalText = _world.IsPortalUnlocked(_nearbyPortal!)
                ? $"[E] Travel to {BiomeData.GetName(_nearbyPortal!.ToBiome)}"
                : $"[LOCKED] {_nearbyPortal!.RequiresFlag}";
            var portalColor = _world.IsPortalUnlocked(_nearbyPortal!) ? Color.Cyan : Color.Red;
            var portalSize = _font.MeasureString(portalText);
            var portalPos = new Vector2(
                ScreenManager.BaseScreenSize.X / 2 - portalSize.X / 2,
                ScreenManager.BaseScreenSize.Y - 80
            );
            spriteBatch.DrawString(_font, portalText, portalPos, portalColor);
        }

        // Draw dungeon portal interaction prompt
        bool canInteractDungeon = _nearbyDungeonPortal != null;
        if (canInteractDungeon)
        {
            var dungeon = _nearbyDungeonPortal!.GetDungeon();
            var isUnlocked = IsDungeonPortalUnlocked(_nearbyDungeonPortal!);

            var dungeonText = isUnlocked
                ? $"[E] Enter {dungeon?.Name ?? "Dungeon"} (Lv.{dungeon?.MinLevel}-{dungeon?.MaxLevel})"
                : $"[LOCKED] Requires: {_nearbyDungeonPortal!.RequiredFlag}";
            var dungeonColor = isUnlocked ? Color.Magenta : Color.Red;
            var dungeonSize = _font.MeasureString(dungeonText);
            var dungeonPos = new Vector2(
                ScreenManager.BaseScreenSize.X / 2 - dungeonSize.X / 2,
                ScreenManager.BaseScreenSize.Y - 100
            );
            spriteBatch.DrawString(_font, dungeonText, dungeonPos, dungeonColor);
        }

        // Draw wild kyn interaction prompt
        bool canInteractWildKyn = _nearbyWildKyn != null;
        if (canInteractWildKyn)
        {
            var isDefeated = _nearbyWildKyn!.IsDefeated || _gameState.IsWildKynDefeated(_nearbyWildKyn.Id);
            var wildKynText = isDefeated
                ? $"[E] Talk to {_nearbyWildKyn.KynDefinitionId} (Recruit)"
                : $"[E] Challenge {_nearbyWildKyn.KynDefinitionId} (Lv.{_nearbyWildKyn.Level})";
            var wildKynColor = isDefeated ? Color.LightGreen : Color.Orange;
            var wildKynSize = _font.MeasureString(wildKynText);
            var wildKynPos = new Vector2(
                ScreenManager.BaseScreenSize.X / 2 - wildKynSize.X / 2,
                ScreenManager.BaseScreenSize.Y - 80
            );
            spriteBatch.DrawString(_font, wildKynText, wildKynPos, wildKynColor);
        }

        if (canInteractNPC)
        {
            var interactText = "[E] Interact";
            var interactSize = _font.MeasureString(interactText);
            var interactPos = new Vector2(
                ScreenManager.BaseScreenSize.X / 2 - interactSize.X / 2,
                ScreenManager.BaseScreenSize.Y - 60
            );
            spriteBatch.DrawString(_font, interactText, interactPos, Color.Yellow);
        }

        // Draw controls hint
        var hint = "WASD: Move | E: Interact | M: Map | Q: Quests | ESC: Pause";
        var hintPos = new Vector2(10, ScreenManager.BaseScreenSize.Y - 20);
        spriteBatch.DrawString(_font, hint, hintPos, Color.Gray);
    }

    /// <summary>
    /// Gets display name for weather type.
    /// </summary>
    private string GetWeatherDisplayName(WeatherType weather)
    {
        return weather switch
        {
            WeatherType.Fog => "Fog",
            WeatherType.Rain => "Rain",
            WeatherType.AcidRain => "Acid Rain",
            WeatherType.Dust => "Dust Storm",
            WeatherType.Snow => "Snow",
            WeatherType.DataStorm => "Data Storm",
            WeatherType.RadiationWind => "Radiation Wind",
            _ => ""
        };
    }

    /// <summary>
    /// Gets text color for weather display.
    /// </summary>
    private Color GetWeatherTextColor(WeatherType weather)
    {
        return weather switch
        {
            WeatherType.Fog => Color.LightGray,
            WeatherType.Rain => Color.LightBlue,
            WeatherType.AcidRain => Color.LimeGreen,
            WeatherType.Dust => Color.SandyBrown,
            WeatherType.Snow => Color.White,
            WeatherType.DataStorm => Color.Cyan,
            WeatherType.RadiationWind => Color.Yellow,
            _ => Color.White
        };
    }
}

/// <summary>
/// Simple particle for weather effects.
/// </summary>
public class WeatherParticle
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Color Color { get; set; }
    public Vector2 Size { get; set; }
    public float Lifetime { get; set; } = 10f;
}

/// <summary>
/// Event arguments for combat ending.
/// </summary>
public class CombatEndedEventArgs : EventArgs
{
    public bool Victory { get; init; }
    public bool Defeat { get; init; }
    public bool Fled { get; init; }
    public int ExperienceEarned { get; init; }
    public int CurrencyEarned { get; init; }
    public int TelemetryUnitsEarned { get; init; }
    public Kyn? RecruitedKyn { get; init; }
}
