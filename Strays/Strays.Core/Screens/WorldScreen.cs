using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.Data;
using Strays.Core.Game.Dialog;
using Strays.Core.Game.Entities;
using Strays.Core.Game.Progression;
using Strays.Core.Game.World;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// The main gameplay screen for world exploration.
/// Handles protagonist movement, companion following, and encounter triggers.
/// </summary>
public class WorldScreen : GameScreen
{
    // Services
    private GameStateService _gameState = null!;
    private StrayRoster _roster = null!;
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
    private NPC? _nearbyNPC;
    private Settlement? _currentSettlement;
    private Stray? _pendingRecruitment;

    // Biome transition state
    private BiomeType _previousBiome;
    private float _biomeTransitionAlpha;
    private string _biomeTransitionText = "";

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
        _roster = new StrayRoster();

        // Initialize game state if needed
        if (_gameState.Data.PartyStrayIds.Count == 0)
        {
            _gameState.NewGame(CompanionType.Dog);
            _gameState.HasExoskeleton = true; // For testing, give exoskeleton immediately
            _gameState.ExoskeletonPowered = true; // And power it

            // Create a starter Stray (Echo Pup)
            var echoPup = Stray.Create("echo_pup", 5);
            if (echoPup != null)
            {
                _roster.AddStray(echoPup);
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

        // Create companion
        _companion = new Companion(_gameState.CompanionType, _gameState);
        _companion.Position = _protagonist.Position + new Vector2(-40, 0);

        // Create world
        _world = new GameWorld(ScreenManager.GraphicsDevice, _gameState);
        _world.SetViewportSize(new Vector2(ScreenManager.BaseScreenSize.X, ScreenManager.BaseScreenSize.Y));
        _world.Initialize();

        // Subscribe to world events
        _world.BiomeChanged += OnBiomeChanged;
        _world.WeatherChanged += OnWeatherChanged;
        _previousBiome = _world.CurrentBiome;

        // Create settlements
        InitializeSettlements();

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

        // Check for pause
        if (input.IsPauseGame(ControllingPlayer))
        {
            // Add pause screen
            ScreenManager.AddScreen(new PauseScreen(), ControllingPlayer);
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

        // Update protagonist
        _protagonist.Update(gameTime, movement);

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
    /// Initiates interaction with an NPC.
    /// </summary>
    private void InteractWithNPC(NPC npc)
    {
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

        // Update companion
        _companion.Update(gameTime, _protagonist.Position, _protagonist.Facing);

        // Update world
        _world.Update(gameTime, _protagonist.Position);

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

        // Update biome transition
        if (_biomeTransitionAlpha > 0)
        {
            _biomeTransitionAlpha -= (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;
        }

        // Update weather particles
        UpdateWeatherParticles(gameTime);
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
    /// Starts combat with an encounter.
    /// </summary>
    private void StartCombat(Encounter encounter)
    {
        // Generate enemy party
        var random = new Random();
        var enemyData = encounter.GenerateEnemyParty(random);

        var enemies = enemyData
            .Select(e => Stray.Create(e.DefinitionId, e.Level))
            .Where(s => s != null)
            .Cast<Stray>()
            .ToList();

        // If no valid enemies, create generic ones
        if (enemies.Count == 0)
        {
            for (int i = 0; i < encounter.EnemyCount; i++)
            {
                var wildStray = Stray.Create("wild_stray", random.Next(encounter.LevelRange.Min, encounter.LevelRange.Max + 1));
                if (wildStray != null)
                    enemies.Add(wildStray);
            }
        }

        // Create and add combat screen
        var combatScreen = new CombatScreen(_roster.Party.ToList(), enemies, encounter, _companion);
        combatScreen.CombatEnded += OnCombatEnded;
        ScreenManager.AddScreen(combatScreen, ControllingPlayer);
    }

    /// <summary>
    /// Called when combat ends.
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
            var leveledUp = _roster.AwardExperience(e.ExperienceEarned);

            // Check for recruitment opportunity
            if (e.RecruitedStray != null && e.RecruitedStray.Definition.CanRecruit)
            {
                _pendingRecruitment = e.RecruitedStray;
                ShowRecruitmentScreen(e.RecruitedStray);
            }
        }
        else if (e.Fled)
        {
            // Move protagonist away from encounter
            _protagonist.ApplyCollisionPush(new Vector2(0, -50));
        }
        else if (e.Defeat)
        {
            // Game over handling - for now, just heal and respawn
            _roster.HealParty();
            _roster.ReviveParty();
        }

        _triggeredEncounter = null;
    }

    /// <summary>
    /// Shows the recruitment screen for a defeated Stray.
    /// </summary>
    private void ShowRecruitmentScreen(Stray stray)
    {
        var recruitScreen = new RecruitmentScreen(stray, _recruitmentManager);
        recruitScreen.RecruitmentComplete += OnRecruitmentComplete;
        ScreenManager.AddScreen(recruitScreen, ControllingPlayer);
    }

    private void OnRecruitmentComplete(object? sender, RecruitmentResult result)
    {
        if (result == RecruitmentResult.Success && _pendingRecruitment != null)
        {
            _questLog.NotifyRecruitedStray(_pendingRecruitment.Definition.Id);
        }
        _pendingRecruitment = null;
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var graphicsDevice = ScreenManager.GraphicsDevice;

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
        foreach (var stray in _roster.Party)
        {
            var text = $"{stray.DisplayName} HP:{stray.CurrentHp}/{stray.MaxHp} Lv{stray.Level}";
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

        // Draw interaction prompt if near NPC
        bool canInteract = _nearbyNPC != null ||
            (_currentSettlement?.GetNearestInteractableNPC(_protagonist.Position) != null);
        if (canInteract)
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
        var hint = "WASD: Move | E: Interact | Q: Quests | ESC: Pause";
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
    public Stray? RecruitedStray { get; init; }
}
