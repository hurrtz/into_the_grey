using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Game.World;
using Strays.Core.Inputs;
using Strays.Core.Services;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// A full-screen map showing all biomes and allowing fast travel between them.
/// </summary>
public class BiomeMapScreen : GameScreen
{
    private readonly GameWorld _world;
    private readonly GameStateService _gameState;

    private Texture2D? _pixelTexture;
    private SpriteFont? _font;
    private SpriteFont? _titleFont;

    // Map layout
    private readonly Dictionary<BiomeType, Vector2> _biomePositions = new();
    private readonly Dictionary<BiomeType, bool> _biomeDiscovered = new();
    private BiomeType _selectedBiome;
    private BiomeType _currentBiome;

    // Animation
    private float _pulseTimer;
    private float _selectCooldown;

    // Layout constants
    private const int BiomeNodeSize = 80;
    private const int BiomeNodeSpacing = 120;

    /// <summary>
    /// Event fired when player selects a biome to travel to.
    /// </summary>
    public event Action<BiomeType>? BiomeSelected;

    public BiomeMapScreen(GameWorld world, GameStateService gameState)
    {
        _world = world;
        _gameState = gameState;
        _currentBiome = world.CurrentBiome;
        _selectedBiome = _currentBiome;

        TransitionOnTime = TimeSpan.FromSeconds(0.3);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);

        InitializeBiomeLayout();
        InitializeDiscoveredBiomes();
    }

    /// <summary>
    /// Sets up the visual positions of biomes on the map.
    /// </summary>
    private void InitializeBiomeLayout()
    {
        // Layout matches the world connectivity:
        //                    [Archive Scar]
        //                         |
        //    [Rust] --- [Fringe] --- [Green]
        //      |           |           |
        //   [Teeth] --- [Quiet] -------+
        //      |
        //    [Glow]

        var centerX = 400; // Assuming 800 width
        var centerY = 240; // Assuming 480 height

        _biomePositions[BiomeType.Fringe] = new Vector2(centerX, centerY);
        _biomePositions[BiomeType.Rust] = new Vector2(centerX - BiomeNodeSpacing, centerY);
        _biomePositions[BiomeType.Green] = new Vector2(centerX + BiomeNodeSpacing, centerY);
        _biomePositions[BiomeType.Quiet] = new Vector2(centerX, centerY + BiomeNodeSpacing);
        _biomePositions[BiomeType.Teeth] = new Vector2(centerX - BiomeNodeSpacing, centerY + BiomeNodeSpacing);
        _biomePositions[BiomeType.Glow] = new Vector2(centerX - BiomeNodeSpacing, centerY + BiomeNodeSpacing * 2);
        _biomePositions[BiomeType.ArchiveScar] = new Vector2(centerX + BiomeNodeSpacing, centerY - BiomeNodeSpacing);
    }

    /// <summary>
    /// Initializes which biomes have been discovered based on game state.
    /// </summary>
    private void InitializeDiscoveredBiomes()
    {
        // Fringe is always discovered
        _biomeDiscovered[BiomeType.Fringe] = true;

        // Others depend on story flags or having visited them
        _biomeDiscovered[BiomeType.Rust] = true; // Always accessible from Fringe
        _biomeDiscovered[BiomeType.Green] = true; // Always accessible from Fringe

        _biomeDiscovered[BiomeType.Quiet] = _gameState.HasFlag("reached_quiet") ||
                                            _gameState.HasFlag("visited_quiet");
        _biomeDiscovered[BiomeType.Teeth] = _gameState.HasFlag("reached_teeth") ||
                                            _gameState.HasFlag("visited_teeth");
        _biomeDiscovered[BiomeType.Glow] = _gameState.HasFlag("reached_glow") ||
                                           _gameState.HasFlag("visited_glow");
        _biomeDiscovered[BiomeType.ArchiveScar] = _gameState.HasFlag("found_archive_scar") ||
                                                  _gameState.HasFlag("visited_archive");
    }

    public override void LoadContent()
    {
        base.LoadContent();

        var content = ScreenManager!.Game.Content;

        // Create pixel texture
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Load fonts
        _font = content.Load<SpriteFont>("Fonts/Hud");
        _titleFont = content.Load<SpriteFont>("Fonts/GameFont");
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (IsActive)
        {
            _pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_selectCooldown > 0)
                _selectCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_selectCooldown > 0)
            return;

        // Check for cancel
        if (input.IsMenuCancel(null, out _))
        {
            ExitScreen();
            return;
        }

        // Navigate between biomes
        var movement = Vector2.Zero;

        if (input.IsNewKeyPress(Keys.Left, null, out _) || input.IsNewKeyPress(Keys.A, null, out _))
            movement.X = -1;
        else if (input.IsNewKeyPress(Keys.Right, null, out _) || input.IsNewKeyPress(Keys.D, null, out _))
            movement.X = 1;
        else if (input.IsNewKeyPress(Keys.Up, null, out _) || input.IsNewKeyPress(Keys.W, null, out _))
            movement.Y = -1;
        else if (input.IsNewKeyPress(Keys.Down, null, out _) || input.IsNewKeyPress(Keys.S, null, out _))
            movement.Y = 1;

        if (movement != Vector2.Zero)
        {
            SelectNearestBiome(movement);
            _selectCooldown = 0.15f;
        }

        // Confirm selection
        if (input.IsNewKeyPress(Keys.Enter, null, out _) ||
            input.IsNewKeyPress(Keys.Space, null, out _) ||
            input.IsNewKeyPress(Keys.E, null, out _))
        {
            TryTravelToBiome(_selectedBiome);
        }
    }

    /// <summary>
    /// Selects the nearest discovered biome in a direction.
    /// </summary>
    private void SelectNearestBiome(Vector2 direction)
    {
        var currentPos = _biomePositions[_selectedBiome];
        BiomeType? nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var kvp in _biomePositions)
        {
            if (kvp.Key == _selectedBiome)
                continue;

            // Only consider discovered biomes
            if (!_biomeDiscovered.GetValueOrDefault(kvp.Key))
                continue;

            var delta = kvp.Value - currentPos;

            // Check if this biome is in the right direction
            bool inDirection = false;
            if (direction.X < 0 && delta.X < -20)
                inDirection = true;
            else if (direction.X > 0 && delta.X > 20)
                inDirection = true;
            else if (direction.Y < 0 && delta.Y < -20)
                inDirection = true;
            else if (direction.Y > 0 && delta.Y > 20)
                inDirection = true;

            if (!inDirection)
                continue;

            var distance = delta.Length();
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = kvp.Key;
            }
        }

        if (nearest.HasValue)
        {
            _selectedBiome = nearest.Value;
        }
    }

    /// <summary>
    /// Attempts to travel to the selected biome.
    /// </summary>
    private void TryTravelToBiome(BiomeType biome)
    {
        if (biome == _currentBiome)
        {
            // Already here
            ExitScreen();
            return;
        }

        // Check if travel path exists
        var path = _world.GetTravelPath(_currentBiome, biome);
        if (path == null || path.Count == 0)
        {
            // Cannot reach - play error sound or show message
            return;
        }

        // Fire event and exit
        BiomeSelected?.Invoke(biome);
        ExitScreen();
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager!.SpriteBatch;
        var viewport = ScreenManager.GraphicsDevice.Viewport;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        // Draw semi-transparent background
        spriteBatch.Draw(_pixelTexture, viewport.Bounds, Color.Black * 0.85f * TransitionAlpha);

        // Draw title
        if (_titleFont != null)
        {
            var title = "WORLD MAP";
            var titleSize = _titleFont.MeasureString(title);
            var titlePos = new Vector2((viewport.Width - titleSize.X) / 2, 20);
            spriteBatch.DrawString(_titleFont, title, titlePos, Color.White * TransitionAlpha);
        }

        // Draw connections between biomes first (so they're behind nodes)
        DrawBiomeConnections(spriteBatch);

        // Draw biome nodes
        foreach (var biome in Enum.GetValues<BiomeType>())
        {
            DrawBiomeNode(spriteBatch, biome);
        }

        // Draw legend
        DrawLegend(spriteBatch, viewport);

        // Draw travel info for selected biome
        DrawTravelInfo(spriteBatch, viewport);

        // Draw controls hint
        if (_font != null)
        {
            var hint = "[Arrow Keys] Navigate | [Enter] Travel | [Esc] Close";
            var hintSize = _font.MeasureString(hint);
            var hintPos = new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height - 30);
            spriteBatch.DrawString(_font, hint, hintPos, Color.Gray * TransitionAlpha);
        }

        spriteBatch.End();
    }

    /// <summary>
    /// Draws connection lines between connected biomes.
    /// </summary>
    private void DrawBiomeConnections(SpriteBatch spriteBatch)
    {
        // Define connections
        var connections = new[]
        {
            (BiomeType.Fringe, BiomeType.Rust),
            (BiomeType.Fringe, BiomeType.Green),
            (BiomeType.Fringe, BiomeType.Quiet),
            (BiomeType.Rust, BiomeType.Teeth),
            (BiomeType.Rust, BiomeType.Quiet),
            (BiomeType.Teeth, BiomeType.Glow),
            (BiomeType.Teeth, BiomeType.Quiet),
            (BiomeType.Green, BiomeType.ArchiveScar),
            (BiomeType.Green, BiomeType.Quiet)
        };

        foreach (var (from, to) in connections)
        {
            var fromPos = _biomePositions[from];
            var toPos = _biomePositions[to];

            var fromDiscovered = _biomeDiscovered.GetValueOrDefault(from);
            var toDiscovered = _biomeDiscovered.GetValueOrDefault(to);

            Color lineColor;
            if (fromDiscovered && toDiscovered)
            {
                // Check if path is unlocked
                var portal = _world.Portals.FirstOrDefault(p =>
                    (p.FromBiome == from && p.ToBiome == to) ||
                    (p.FromBiome == to && p.ToBiome == from));

                if (portal != null && _world.IsPortalUnlocked(portal))
                    lineColor = Color.White * 0.5f;
                else
                    lineColor = Color.Red * 0.3f; // Locked path
            }
            else
            {
                lineColor = Color.DarkGray * 0.2f; // Unknown connection
            }

            DrawLine(spriteBatch, fromPos, toPos, lineColor * TransitionAlpha, 2);
        }
    }

    /// <summary>
    /// Draws a single biome node.
    /// </summary>
    private void DrawBiomeNode(SpriteBatch spriteBatch, BiomeType biome)
    {
        if (!_biomePositions.TryGetValue(biome, out var position))
            return;

        var discovered = _biomeDiscovered.GetValueOrDefault(biome);
        var isSelected = biome == _selectedBiome;
        var isCurrent = biome == _currentBiome;

        // Node bounds
        var nodeSize = BiomeNodeSize;
        if (isSelected)
        {
            var pulse = 1f + 0.1f * (float)Math.Sin(_pulseTimer * 4);
            nodeSize = (int)(BiomeNodeSize * pulse);
        }

        var bounds = new Rectangle(
            (int)(position.X - nodeSize / 2),
            (int)(position.Y - nodeSize / 2),
            nodeSize,
            nodeSize);

        if (discovered)
        {
            // Draw biome color
            var biomeColor = BiomeData.GetAccentColor(biome);

            // Background
            spriteBatch.Draw(_pixelTexture, bounds, biomeColor * 0.6f * TransitionAlpha);

            // Border
            var borderColor = isSelected ? Color.Yellow : (isCurrent ? Color.Cyan : Color.White);
            var borderWidth = isSelected ? 3 : 2;

            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderWidth), borderColor * TransitionAlpha);
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Bottom - borderWidth, bounds.Width, borderWidth), borderColor * TransitionAlpha);
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, borderWidth, bounds.Height), borderColor * TransitionAlpha);
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.Right - borderWidth, bounds.Y, borderWidth, bounds.Height), borderColor * TransitionAlpha);

            // Current biome indicator
            if (isCurrent)
            {
                var markerSize = 10;
                var markerBounds = new Rectangle(
                    bounds.Center.X - markerSize / 2,
                    bounds.Center.Y - markerSize / 2,
                    markerSize, markerSize);
                spriteBatch.Draw(_pixelTexture, markerBounds, Color.Cyan * TransitionAlpha);
            }

            // Draw biome name
            if (_font != null)
            {
                var name = BiomeData.GetName(biome);
                var nameSize = _font.MeasureString(name);
                var namePos = new Vector2(position.X - nameSize.X / 2, bounds.Bottom + 5);
                spriteBatch.DrawString(_font, name, namePos, Color.White * TransitionAlpha);

                // Draw level range
                var levelRange = BiomeData.GetLevelRange(biome);
                var levelText = $"Lv {levelRange.Min}-{levelRange.Max}";
                var levelSize = _font.MeasureString(levelText);
                var levelPos = new Vector2(position.X - levelSize.X / 2, namePos.Y + nameSize.Y);
                spriteBatch.DrawString(_font, levelText, levelPos, Color.Yellow * TransitionAlpha);
            }
        }
        else
        {
            // Undiscovered - draw as mystery
            spriteBatch.Draw(_pixelTexture, bounds, Color.DarkGray * 0.3f * TransitionAlpha);

            // Draw question mark
            if (_font != null)
            {
                var mystery = "?";
                var mysterySize = _font.MeasureString(mystery);
                var mysteryPos = new Vector2(position.X - mysterySize.X / 2, position.Y - mysterySize.Y / 2);
                spriteBatch.DrawString(_font, mystery, mysteryPos, Color.Gray * TransitionAlpha);

                var unknown = "Unknown";
                var unknownSize = _font.MeasureString(unknown);
                var unknownPos = new Vector2(position.X - unknownSize.X / 2, bounds.Bottom + 5);
                spriteBatch.DrawString(_font, unknown, unknownPos, Color.Gray * TransitionAlpha);
            }
        }
    }

    /// <summary>
    /// Draws the map legend.
    /// </summary>
    private void DrawLegend(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_font == null) return;

        var legendX = 20;
        var legendY = viewport.Height - 120;

        // Current location
        spriteBatch.Draw(_pixelTexture, new Rectangle(legendX, legendY, 15, 15), Color.Cyan * TransitionAlpha);
        spriteBatch.DrawString(_font, "Current Location", new Vector2(legendX + 20, legendY), Color.White * TransitionAlpha);

        // Selected
        spriteBatch.Draw(_pixelTexture, new Rectangle(legendX, legendY + 20, 15, 15), Color.Yellow * TransitionAlpha);
        spriteBatch.DrawString(_font, "Selected", new Vector2(legendX + 20, legendY + 20), Color.White * TransitionAlpha);

        // Locked path
        DrawLine(spriteBatch, new Vector2(legendX, legendY + 45), new Vector2(legendX + 15, legendY + 45), Color.Red * 0.5f * TransitionAlpha, 2);
        spriteBatch.DrawString(_font, "Locked Path", new Vector2(legendX + 20, legendY + 40), Color.White * TransitionAlpha);
    }

    /// <summary>
    /// Draws travel info for the selected biome.
    /// </summary>
    private void DrawTravelInfo(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_font == null) return;
        if (_selectedBiome == _currentBiome)
        {
            var currentText = "You are here";
            var textSize = _font.MeasureString(currentText);
            var textPos = new Vector2((viewport.Width - textSize.X) / 2, 60);
            spriteBatch.DrawString(_font, currentText, textPos, Color.Cyan * TransitionAlpha);
            return;
        }

        var path = _world.GetTravelPath(_currentBiome, _selectedBiome);
        if (path == null || path.Count == 0)
        {
            var noPathText = "Cannot reach this biome yet";
            var textSize = _font.MeasureString(noPathText);
            var textPos = new Vector2((viewport.Width - textSize.X) / 2, 60);
            spriteBatch.DrawString(_font, noPathText, textPos, Color.Red * TransitionAlpha);
        }
        else
        {
            var pathNames = path.Select(b => BiomeData.GetName(b));
            var pathText = $"Route: {string.Join(" -> ", pathNames)}";
            var textSize = _font.MeasureString(pathText);
            var textPos = new Vector2((viewport.Width - textSize.X) / 2, 60);
            spriteBatch.DrawString(_font, pathText, textPos, Color.Green * TransitionAlpha);
        }
    }

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
    {
        var delta = end - start;
        var length = delta.Length();
        var angle = (float)Math.Atan2(delta.Y, delta.X);

        spriteBatch.Draw(
            _pixelTexture,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0);
    }

    public override void UnloadContent()
    {
        _pixelTexture?.Dispose();
        base.UnloadContent();
    }
}
