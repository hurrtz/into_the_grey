using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Strays.Core.Game.World;

/// <summary>
/// Direction a portal leads to.
/// </summary>
public enum PortalDirection
{
    North,
    South,
    East,
    West
}

/// <summary>
/// A portal that allows transition between biomes.
/// </summary>
public class BiomePortal
{
    /// <summary>
    /// Unique ID for this portal.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// The biome this portal leads FROM.
    /// </summary>
    public BiomeType FromBiome { get; init; }

    /// <summary>
    /// The biome this portal leads TO.
    /// </summary>
    public BiomeType ToBiome { get; init; }

    /// <summary>
    /// World position of the portal.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Size of the portal trigger area.
    /// </summary>
    public Vector2 Size { get; set; } = new Vector2(100, 100);

    /// <summary>
    /// Direction the portal faces.
    /// </summary>
    public PortalDirection Direction { get; init; }

    /// <summary>
    /// The chunk ID where the player should spawn after using this portal.
    /// </summary>
    public string TargetChunkId { get; init; } = "";

    /// <summary>
    /// Spawn position offset in the target chunk.
    /// </summary>
    public Vector2 TargetSpawnOffset { get; init; }

    /// <summary>
    /// Story flag required to use this portal (null = always available).
    /// </summary>
    public string? RequiresFlag { get; init; }

    /// <summary>
    /// Minimum player level recommended (0 = no restriction).
    /// </summary>
    public int RecommendedLevel { get; init; }

    /// <summary>
    /// Whether this portal is currently active (usable).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the player is currently overlapping this portal.
    /// </summary>
    public bool IsPlayerInRange { get; set; }

    /// <summary>
    /// Animation timer for visual effects.
    /// </summary>
    private float _animTimer;

    /// <summary>
    /// Bounding rectangle for collision detection.
    /// </summary>
    public Rectangle Bounds => new Rectangle(
        (int)(Position.X - Size.X / 2),
        (int)(Position.Y - Size.Y / 2),
        (int)Size.X,
        (int)Size.Y
    );

    /// <summary>
    /// Checks if a rectangle collides with this portal.
    /// </summary>
    public bool CheckCollision(Rectangle bounds)
    {
        return IsActive && Bounds.Intersects(bounds);
    }

    /// <summary>
    /// Checks if a point is inside the portal.
    /// </summary>
    public bool Contains(Vector2 point)
    {
        return IsActive && Bounds.Contains((int)point.X, (int)point.Y);
    }

    /// <summary>
    /// Updates the portal animation.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        _animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>
    /// Draws the portal.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, Vector2 cameraPosition)
    {
        var screenPos = Position - cameraPosition;
        var bounds = new Rectangle(
            (int)(screenPos.X - Size.X / 2),
            (int)(screenPos.Y - Size.Y / 2),
            (int)Size.X,
            (int)Size.Y
        );

        // Get target biome color
        var targetColor = BiomeData.GetAccentColor(ToBiome);

        // Pulse effect
        float pulse = 0.5f + 0.5f * (float)Math.Sin(_animTimer * 3);
        var color = targetColor * (0.3f + 0.4f * pulse);

        // Draw portal background
        spriteBatch.Draw(pixelTexture, bounds, color);

        // Draw border with pulse
        var borderColor = targetColor * (0.6f + 0.4f * pulse);
        int borderWidth = 3;

        // Top
        spriteBatch.Draw(pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderWidth), borderColor);
        // Bottom
        spriteBatch.Draw(pixelTexture, new Rectangle(bounds.X, bounds.Bottom - borderWidth, bounds.Width, borderWidth), borderColor);
        // Left
        spriteBatch.Draw(pixelTexture, new Rectangle(bounds.X, bounds.Y, borderWidth, bounds.Height), borderColor);
        // Right
        spriteBatch.Draw(pixelTexture, new Rectangle(bounds.Right - borderWidth, bounds.Y, borderWidth, bounds.Height), borderColor);

        // Draw direction arrow
        DrawDirectionArrow(spriteBatch, pixelTexture, screenPos, borderColor);

        // Draw label
        if (font != null)
        {
            var label = $"To: {BiomeData.GetName(ToBiome)}";
            var labelSize = font.MeasureString(label);
            var labelPos = new Vector2(screenPos.X - labelSize.X / 2, bounds.Y - labelSize.Y - 5);
            spriteBatch.DrawString(font, label, labelPos, Color.White);

            // Show level recommendation
            if (RecommendedLevel > 0)
            {
                var levelLabel = $"Lv.{BiomeData.GetLevelRange(ToBiome).Min}+";
                var levelSize = font.MeasureString(levelLabel);
                var levelPos = new Vector2(screenPos.X - levelSize.X / 2, bounds.Bottom + 5);
                spriteBatch.DrawString(font, levelLabel, levelPos, Color.Yellow);
            }

            // Show interaction hint if player is in range
            if (IsPlayerInRange)
            {
                var hint = "[E] Enter";
                var hintSize = font.MeasureString(hint);
                var hintPos = new Vector2(screenPos.X - hintSize.X / 2, screenPos.Y - hintSize.Y / 2);
                spriteBatch.DrawString(font, hint, hintPos, Color.White);
            }
        }
    }

    /// <summary>
    /// Draws an arrow indicating the portal direction.
    /// </summary>
    private void DrawDirectionArrow(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 screenPos, Color color)
    {
        int arrowSize = 20;

        // Draw simple arrow based on direction
        switch (Direction)
        {
            case PortalDirection.North:
                // Triangle pointing up
                for (int i = 0; i < arrowSize; i++)
                {
                    int width = (arrowSize - i) * 2;
                    spriteBatch.Draw(pixelTexture, new Rectangle(
                        (int)screenPos.X - width / 2,
                        (int)screenPos.Y - arrowSize / 2 + i,
                        width, 1), color);
                }
                break;

            case PortalDirection.South:
                // Triangle pointing down
                for (int i = 0; i < arrowSize; i++)
                {
                    int width = i * 2;
                    spriteBatch.Draw(pixelTexture, new Rectangle(
                        (int)screenPos.X - width / 2,
                        (int)screenPos.Y - arrowSize / 2 + i,
                        Math.Max(1, width), 1), color);
                }
                break;

            case PortalDirection.East:
                // Triangle pointing right
                for (int i = 0; i < arrowSize; i++)
                {
                    int height = (arrowSize - i) * 2;
                    spriteBatch.Draw(pixelTexture, new Rectangle(
                        (int)screenPos.X - arrowSize / 2 + i,
                        (int)screenPos.Y - height / 2,
                        1, height), color);
                }
                break;

            case PortalDirection.West:
                // Triangle pointing left
                for (int i = 0; i < arrowSize; i++)
                {
                    int height = i * 2;
                    spriteBatch.Draw(pixelTexture, new Rectangle(
                        (int)screenPos.X - arrowSize / 2 + i,
                        (int)screenPos.Y - height / 2,
                        1, Math.Max(1, height)), color);
                }
                break;
        }
    }

    /// <summary>
    /// Creates a portal connecting two biomes.
    /// </summary>
    public static BiomePortal Create(
        BiomeType from,
        BiomeType to,
        PortalDirection direction,
        Vector2 position,
        string targetChunkId,
        Vector2 targetSpawnOffset,
        string? requiresFlag = null)
    {
        return new BiomePortal
        {
            Id = $"portal_{from}_{to}_{direction}",
            FromBiome = from,
            ToBiome = to,
            Direction = direction,
            Position = position,
            TargetChunkId = targetChunkId,
            TargetSpawnOffset = targetSpawnOffset,
            RequiresFlag = requiresFlag,
            RecommendedLevel = BiomeData.GetLevelRange(to).Min
        };
    }
}
