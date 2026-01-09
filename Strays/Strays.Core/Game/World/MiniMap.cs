using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Game.Data;
using Strays.Core.Game.Entities;

namespace Strays.Core.Game.World;

/// <summary>
/// Types of blips on the minimap.
/// </summary>
public enum MiniMapBlipType
{
    /// <summary>
    /// Player position.
    /// </summary>
    Player,

    /// <summary>
    /// Companion.
    /// </summary>
    Companion,

    /// <summary>
    /// Party member Stray.
    /// </summary>
    PartyMember,

    /// <summary>
    /// Neutral NPC.
    /// </summary>
    NeutralNpc,

    /// <summary>
    /// Friendly NPC.
    /// </summary>
    FriendlyNpc,

    /// <summary>
    /// Hostile/Enemy.
    /// </summary>
    Enemy,

    /// <summary>
    /// Quest objective.
    /// </summary>
    QuestObjective,

    /// <summary>
    /// Settlement/town.
    /// </summary>
    Settlement,

    /// <summary>
    /// Shop.
    /// </summary>
    Shop,

    /// <summary>
    /// Dungeon entrance.
    /// </summary>
    Dungeon,

    /// <summary>
    /// Biome portal.
    /// </summary>
    Portal,

    /// <summary>
    /// Treasure/loot.
    /// </summary>
    Treasure,

    /// <summary>
    /// Point of interest.
    /// </summary>
    PointOfInterest,

    /// <summary>
    /// Hazard zone.
    /// </summary>
    Hazard
}

/// <summary>
/// A blip displayed on the minimap.
/// </summary>
public class MiniMapBlip
{
    /// <summary>
    /// World position.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Blip type.
    /// </summary>
    public MiniMapBlipType Type { get; init; }

    /// <summary>
    /// Optional label.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Whether blip pulses.
    /// </summary>
    public bool Pulsing { get; init; } = false;

    /// <summary>
    /// Custom color override.
    /// </summary>
    public Color? CustomColor { get; init; }

    /// <summary>
    /// Whether this blip is currently visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Priority for display (higher = draw on top).
    /// </summary>
    public int Priority { get; init; } = 0;
}

/// <summary>
/// Minimap display modes.
/// </summary>
public enum MiniMapMode
{
    /// <summary>
    /// Small corner radar.
    /// </summary>
    Radar,

    /// <summary>
    /// Full-screen overview map.
    /// </summary>
    FullMap,

    /// <summary>
    /// Hidden.
    /// </summary>
    Hidden
}

/// <summary>
/// Configuration for the minimap.
/// </summary>
public class MiniMapConfig
{
    /// <summary>
    /// Radar mode size (pixels).
    /// </summary>
    public int RadarSize { get; set; } = 150;

    /// <summary>
    /// Radar position offset from corner.
    /// </summary>
    public Vector2 RadarOffset { get; set; } = new Vector2(10, 10);

    /// <summary>
    /// Radar corner (0=top-left, 1=top-right, 2=bottom-left, 3=bottom-right).
    /// </summary>
    public int RadarCorner { get; set; } = 1; // Top-right by default

    /// <summary>
    /// Radar zoom level (world units per pixel).
    /// </summary>
    public float RadarZoom { get; set; } = 10f;

    /// <summary>
    /// Full map zoom level.
    /// </summary>
    public float FullMapZoom { get; set; } = 5f;

    /// <summary>
    /// Background opacity.
    /// </summary>
    public float BackgroundOpacity { get; set; } = 0.7f;

    /// <summary>
    /// Border color.
    /// </summary>
    public Color BorderColor { get; set; } = Color.White;

    /// <summary>
    /// Background color.
    /// </summary>
    public Color BackgroundColor { get; set; } = new Color(20, 20, 30);

    /// <summary>
    /// Whether to show grid lines.
    /// </summary>
    public bool ShowGrid { get; set; } = true;

    /// <summary>
    /// Whether to rotate radar with player facing.
    /// </summary>
    public bool RotateWithPlayer { get; set; } = false;

    /// <summary>
    /// Whether to show compass directions.
    /// </summary>
    public bool ShowCompass { get; set; } = true;
}

/// <summary>
/// Minimap/radar system for world exploration.
/// </summary>
public class MiniMap
{
    private readonly Random _random = new();

    /// <summary>
    /// Current display mode.
    /// </summary>
    public MiniMapMode Mode { get; set; } = MiniMapMode.Radar;

    /// <summary>
    /// Configuration.
    /// </summary>
    public MiniMapConfig Config { get; } = new();

    /// <summary>
    /// All blips on the map.
    /// </summary>
    public List<MiniMapBlip> Blips { get; } = new();

    /// <summary>
    /// Player position in world.
    /// </summary>
    public Vector2 PlayerPosition { get; set; }

    /// <summary>
    /// Player facing direction (radians).
    /// </summary>
    public float PlayerRotation { get; set; }

    /// <summary>
    /// Current biome for background tinting.
    /// </summary>
    public BiomeType CurrentBiome { get; set; }

    /// <summary>
    /// Animation timer.
    /// </summary>
    private float _animationTimer = 0f;

    /// <summary>
    /// Updates the minimap.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>
    /// Clears all blips.
    /// </summary>
    public void ClearBlips()
    {
        Blips.Clear();
    }

    /// <summary>
    /// Adds a blip to the minimap.
    /// </summary>
    public void AddBlip(MiniMapBlip blip)
    {
        Blips.Add(blip);
    }

    /// <summary>
    /// Removes blips of a specific type.
    /// </summary>
    public void RemoveBlipsOfType(MiniMapBlipType type)
    {
        Blips.RemoveAll(b => b.Type == type);
    }

    /// <summary>
    /// Updates blip position.
    /// </summary>
    public void UpdateBlipPosition(MiniMapBlip blip, Vector2 newPosition)
    {
        blip.Position = newPosition;
    }

    /// <summary>
    /// Toggles between radar and full map modes.
    /// </summary>
    public void ToggleMode()
    {
        Mode = Mode switch
        {
            MiniMapMode.Radar => MiniMapMode.FullMap,
            MiniMapMode.FullMap => MiniMapMode.Radar,
            MiniMapMode.Hidden => MiniMapMode.Radar,
            _ => MiniMapMode.Radar
        };
    }

    /// <summary>
    /// Draws the minimap.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont? font, Rectangle screenBounds)
    {
        if (Mode == MiniMapMode.Hidden)
        {
            return;
        }

        if (Mode == MiniMapMode.Radar)
        {
            DrawRadar(spriteBatch, pixel, font, screenBounds);
        }
        else
        {
            DrawFullMap(spriteBatch, pixel, font, screenBounds);
        }
    }

    /// <summary>
    /// Draws the radar mode minimap.
    /// </summary>
    private void DrawRadar(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont? font, Rectangle screenBounds)
    {
        // Calculate radar position
        int size = Config.RadarSize;
        Vector2 position = Config.RadarCorner switch
        {
            0 => Config.RadarOffset, // Top-left
            1 => new Vector2(screenBounds.Width - size - Config.RadarOffset.X, Config.RadarOffset.Y), // Top-right
            2 => new Vector2(Config.RadarOffset.X, screenBounds.Height - size - Config.RadarOffset.Y), // Bottom-left
            3 => new Vector2(screenBounds.Width - size - Config.RadarOffset.X, screenBounds.Height - size - Config.RadarOffset.Y), // Bottom-right
            _ => Config.RadarOffset
        };

        var radarRect = new Rectangle((int)position.X, (int)position.Y, size, size);
        var center = new Vector2(radarRect.Center.X, radarRect.Center.Y);

        // Background
        var bgColor = Config.BackgroundColor * Config.BackgroundOpacity;
        var biomeColor = GetBiomeColor(CurrentBiome);
        bgColor = Color.Lerp(bgColor, biomeColor * 0.3f, 0.5f);
        spriteBatch.Draw(pixel, radarRect, bgColor);

        // Grid lines
        if (Config.ShowGrid)
        {
            DrawGrid(spriteBatch, pixel, radarRect, Config.RadarZoom);
        }

        // Range circles
        DrawRangeCircles(spriteBatch, pixel, center, size / 2);

        // Blips
        DrawBlips(spriteBatch, pixel, font, center, size / 2, Config.RadarZoom);

        // Player indicator (center)
        DrawPlayerIndicator(spriteBatch, pixel, center);

        // Border
        DrawBorder(spriteBatch, pixel, radarRect, Config.BorderColor);

        // Compass
        if (Config.ShowCompass && font != null)
        {
            DrawCompass(spriteBatch, font, radarRect);
        }

        // Biome label
        if (font != null)
        {
            string biomeText = GetBiomeName(CurrentBiome);
            var textSize = font.MeasureString(biomeText);
            var textPos = new Vector2(radarRect.Center.X - textSize.X / 2, radarRect.Bottom + 5);
            spriteBatch.DrawString(font, biomeText, textPos, biomeColor);
        }
    }

    /// <summary>
    /// Draws the full map mode.
    /// </summary>
    private void DrawFullMap(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont? font, Rectangle screenBounds)
    {
        int margin = 40;
        var mapRect = new Rectangle(margin, margin, screenBounds.Width - margin * 2, screenBounds.Height - margin * 2);
        var center = new Vector2(mapRect.Center.X, mapRect.Center.Y);

        // Semi-transparent overlay
        spriteBatch.Draw(pixel, screenBounds, Color.Black * 0.8f);

        // Background
        var bgColor = Config.BackgroundColor * Config.BackgroundOpacity;
        spriteBatch.Draw(pixel, mapRect, bgColor);

        // Grid
        if (Config.ShowGrid)
        {
            DrawGrid(spriteBatch, pixel, mapRect, Config.FullMapZoom);
        }

        // Blips (centered on player)
        float radius = Math.Min(mapRect.Width, mapRect.Height) / 2f;
        DrawBlips(spriteBatch, pixel, font, center, radius, Config.FullMapZoom, showLabels: true);

        // Player indicator
        DrawPlayerIndicator(spriteBatch, pixel, center, large: true);

        // Border
        DrawBorder(spriteBatch, pixel, mapRect, Config.BorderColor, thickness: 3);

        // Title
        if (font != null)
        {
            string title = GetBiomeName(CurrentBiome) + " - Area Map";
            var titleSize = font.MeasureString(title);
            var titlePos = new Vector2(mapRect.Center.X - titleSize.X / 2, mapRect.Top - 30);
            spriteBatch.DrawString(font, title, titlePos, Color.White);

            // Instructions
            string instructions = "Press M to close";
            var instrSize = font.MeasureString(instructions);
            var instrPos = new Vector2(mapRect.Center.X - instrSize.X / 2, mapRect.Bottom + 10);
            spriteBatch.DrawString(font, instructions, instrPos, Color.Gray);
        }
    }

    /// <summary>
    /// Draws grid lines.
    /// </summary>
    private void DrawGrid(SpriteBatch spriteBatch, Texture2D pixel, Rectangle bounds, float zoom)
    {
        var gridColor = Color.White * 0.1f;
        int gridSpacing = (int)(32 / zoom * 10); // Adaptive grid spacing

        if (gridSpacing < 10)
        {
            gridSpacing = 10;
        }

        // Vertical lines
        for (int x = bounds.Left + gridSpacing; x < bounds.Right; x += gridSpacing)
        {
            spriteBatch.Draw(pixel, new Rectangle(x, bounds.Top, 1, bounds.Height), gridColor);
        }

        // Horizontal lines
        for (int y = bounds.Top + gridSpacing; y < bounds.Bottom; y += gridSpacing)
        {
            spriteBatch.Draw(pixel, new Rectangle(bounds.Left, y, bounds.Width, 1), gridColor);
        }
    }

    /// <summary>
    /// Draws range circles.
    /// </summary>
    private void DrawRangeCircles(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float maxRadius)
    {
        var circleColor = Color.White * 0.15f;

        // Draw concentric circles at 33% and 66% radius
        DrawCircle(spriteBatch, pixel, center, maxRadius * 0.33f, circleColor);
        DrawCircle(spriteBatch, pixel, center, maxRadius * 0.66f, circleColor);
    }

    /// <summary>
    /// Draws a circle outline.
    /// </summary>
    private void DrawCircle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float radius, Color color)
    {
        int segments = 32;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = MathHelper.TwoPi * i / segments;
            float angle2 = MathHelper.TwoPi * (i + 1) / segments;

            var p1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
            var p2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;

            DrawLine(spriteBatch, pixel, p1, p2, color);
        }
    }

    /// <summary>
    /// Draws a line.
    /// </summary>
    private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color)
    {
        var direction = end - start;
        float length = direction.Length();

        if (length < 1)
        {
            return;
        }

        float rotation = (float)Math.Atan2(direction.Y, direction.X);

        spriteBatch.Draw(
            pixel,
            start,
            null,
            color,
            rotation,
            Vector2.Zero,
            new Vector2(length, 1),
            SpriteEffects.None,
            0
        );
    }

    /// <summary>
    /// Draws blips on the minimap.
    /// </summary>
    private void DrawBlips(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont? font,
        Vector2 center, float radius, float zoom, bool showLabels = false)
    {
        // Sort by priority
        var sortedBlips = new List<MiniMapBlip>(Blips);
        sortedBlips.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var blip in sortedBlips)
        {
            if (!blip.IsVisible)
            {
                continue;
            }

            // Calculate screen position relative to player
            var worldOffset = blip.Position - PlayerPosition;

            // Apply rotation if enabled
            if (Config.RotateWithPlayer)
            {
                float cos = (float)Math.Cos(-PlayerRotation);
                float sin = (float)Math.Sin(-PlayerRotation);
                worldOffset = new Vector2(
                    worldOffset.X * cos - worldOffset.Y * sin,
                    worldOffset.X * sin + worldOffset.Y * cos
                );
            }

            var screenOffset = worldOffset / zoom;

            // Check if within radar range
            if (screenOffset.Length() > radius)
            {
                // Clamp to edge for off-screen blips
                screenOffset.Normalize();
                screenOffset *= radius - 5;
            }

            var screenPos = center + screenOffset;

            // Get blip color and size
            var color = blip.CustomColor ?? GetBlipColor(blip.Type);
            int size = GetBlipSize(blip.Type);

            // Pulsing effect
            if (blip.Pulsing)
            {
                float pulse = (float)Math.Sin(_animationTimer * 4) * 0.3f + 0.7f;
                color *= pulse;
                size = (int)(size * (1f + (1f - pulse) * 0.3f));
            }

            // Draw blip
            DrawBlip(spriteBatch, pixel, screenPos, color, size, blip.Type);

            // Draw label
            if (showLabels && !string.IsNullOrEmpty(blip.Label) && font != null)
            {
                var labelPos = screenPos + new Vector2(size + 2, -8);
                spriteBatch.DrawString(font, blip.Label, labelPos, color * 0.8f);
            }
        }
    }

    /// <summary>
    /// Draws a single blip.
    /// </summary>
    private void DrawBlip(SpriteBatch spriteBatch, Texture2D pixel, Vector2 position, Color color, int size, MiniMapBlipType type)
    {
        var rect = new Rectangle(
            (int)(position.X - size / 2f),
            (int)(position.Y - size / 2f),
            size,
            size
        );

        // Different shapes for different types
        switch (type)
        {
            case MiniMapBlipType.Player:
            case MiniMapBlipType.Companion:
            case MiniMapBlipType.PartyMember:
                // Circle-ish (filled square)
                spriteBatch.Draw(pixel, rect, color);
                break;

            case MiniMapBlipType.QuestObjective:
                // Diamond shape (rotated square)
                spriteBatch.Draw(pixel, rect, color);
                // Add a border
                spriteBatch.Draw(pixel, new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, 1), Color.White);
                spriteBatch.Draw(pixel, new Rectangle(rect.X - 1, rect.Bottom, rect.Width + 2, 1), Color.White);
                break;

            case MiniMapBlipType.Enemy:
                // Triangle pointing down (simplified as filled square with red)
                spriteBatch.Draw(pixel, rect, color);
                break;

            default:
                spriteBatch.Draw(pixel, rect, color);
                break;
        }
    }

    /// <summary>
    /// Draws the player indicator at center.
    /// </summary>
    private void DrawPlayerIndicator(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, bool large = false)
    {
        int size = large ? 12 : 6;

        // Outer circle
        spriteBatch.Draw(pixel, new Rectangle(
            (int)(center.X - size / 2f),
            (int)(center.Y - size / 2f),
            size, size), Color.White);

        // Inner dot
        int innerSize = size / 2;
        spriteBatch.Draw(pixel, new Rectangle(
            (int)(center.X - innerSize / 2f),
            (int)(center.Y - innerSize / 2f),
            innerSize, innerSize), Color.Cyan);

        // Direction indicator
        if (!Config.RotateWithPlayer)
        {
            float indicatorLength = size + 4;
            var direction = new Vector2(
                (float)Math.Cos(PlayerRotation),
                (float)Math.Sin(PlayerRotation)
            );
            var indicatorEnd = center + direction * indicatorLength;

            DrawLine(spriteBatch, pixel, center, indicatorEnd, Color.Yellow);
        }
    }

    /// <summary>
    /// Draws the border.
    /// </summary>
    private void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle bounds, Color color, int thickness = 2)
    {
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
        spriteBatch.Draw(pixel, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
    }

    /// <summary>
    /// Draws compass directions.
    /// </summary>
    private void DrawCompass(SpriteBatch spriteBatch, SpriteFont font, Rectangle bounds)
    {
        var color = Color.White * 0.5f;

        // N
        spriteBatch.DrawString(font, "N", new Vector2(bounds.Center.X - 4, bounds.Top + 2), color);

        // S
        spriteBatch.DrawString(font, "S", new Vector2(bounds.Center.X - 4, bounds.Bottom - 16), color);

        // E
        spriteBatch.DrawString(font, "E", new Vector2(bounds.Right - 14, bounds.Center.Y - 8), color);

        // W
        spriteBatch.DrawString(font, "W", new Vector2(bounds.Left + 4, bounds.Center.Y - 8), color);
    }

    /// <summary>
    /// Gets the color for a blip type.
    /// </summary>
    private Color GetBlipColor(MiniMapBlipType type)
    {
        return type switch
        {
            MiniMapBlipType.Player => Color.Cyan,
            MiniMapBlipType.Companion => Color.Orange,
            MiniMapBlipType.PartyMember => Color.LimeGreen,
            MiniMapBlipType.NeutralNpc => Color.White,
            MiniMapBlipType.FriendlyNpc => Color.Green,
            MiniMapBlipType.Enemy => Color.Red,
            MiniMapBlipType.QuestObjective => Color.Gold,
            MiniMapBlipType.Settlement => Color.Yellow,
            MiniMapBlipType.Shop => Color.LightGreen,
            MiniMapBlipType.Dungeon => Color.Purple,
            MiniMapBlipType.Portal => Color.Magenta,
            MiniMapBlipType.Treasure => Color.Gold,
            MiniMapBlipType.PointOfInterest => Color.LightBlue,
            MiniMapBlipType.Hazard => Color.OrangeRed,
            _ => Color.White
        };
    }

    /// <summary>
    /// Gets the size for a blip type.
    /// </summary>
    private int GetBlipSize(MiniMapBlipType type)
    {
        return type switch
        {
            MiniMapBlipType.Player => 6,
            MiniMapBlipType.Companion => 5,
            MiniMapBlipType.PartyMember => 4,
            MiniMapBlipType.QuestObjective => 6,
            MiniMapBlipType.Settlement => 8,
            MiniMapBlipType.Dungeon => 7,
            MiniMapBlipType.Portal => 6,
            MiniMapBlipType.Enemy => 4,
            _ => 4
        };
    }

    /// <summary>
    /// Gets the background color for a biome.
    /// </summary>
    private Color GetBiomeColor(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Fringe => Color.DarkGreen,
            BiomeType.Suburb => Color.Gray,
            BiomeType.Green => Color.ForestGreen,
            BiomeType.Quiet => Color.LightGray,
            BiomeType.Teeth => Color.DarkRed,
            BiomeType.Glow => Color.Cyan,
            BiomeType.ArchiveScar => Color.Purple,
            _ => Color.DarkGray
        };
    }

    /// <summary>
    /// Gets the display name for a biome.
    /// </summary>
    private string GetBiomeName(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Fringe => "The Fringe",
            BiomeType.Suburb => "The Suburb",
            BiomeType.Green => "The Green",
            BiomeType.Quiet => "The Quiet",
            BiomeType.Teeth => "The Teeth",
            BiomeType.Glow => "The Glow",
            BiomeType.ArchiveScar => "Archive Scar",
            _ => "Unknown"
        };
    }
}
