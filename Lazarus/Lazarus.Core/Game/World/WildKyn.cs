using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Entities;

namespace Lazarus.Core.Game.World;

/// <summary>
/// Represents a wild Kyn on the world map that can be fought and recruited.
/// Unlike Encounters (hostile enemies), wild Kyns:
/// - Must be defeated once before recruitment is possible
/// - Can be approached repeatedly after defeat for recruitment attempts
/// - Have individual recruitment conditions ("agendas")
/// </summary>
public class WildKyn
{
    // Sprite constants
    private const int SpriteSize = 60;
    private const int SpriteSheetFrameCount = 8;
    private const string SpriteFileName = "sprite.png";

    // Sprite resources (loaded once)
    private static Texture2D? _sharedSprite;
    private static bool _spriteLoadAttempted;

    /// <summary>
    /// Unique identifier for this wild Kyn instance.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The Kyn definition ID (species/type).
    /// </summary>
    public string KynDefinitionId { get; }

    /// <summary>
    /// World position of this wild Kyn.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The biome this wild Kyn belongs to.
    /// </summary>
    public BiomeType Biome { get; }

    /// <summary>
    /// Level of this wild Kyn.
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Whether this wild Kyn has been defeated in combat.
    /// Defeated wild Kyns can be approached for recruitment without fighting.
    /// </summary>
    public bool IsDefeated { get; set; }

    /// <summary>
    /// Whether this wild Kyn has been recruited (removed from world).
    /// </summary>
    public bool IsRecruited { get; set; }

    /// <summary>
    /// Direction this wild Kyn is facing.
    /// </summary>
    public Direction FacingDirection { get; set; } = Direction.South;

    /// <summary>
    /// Optional recruitment condition (the Kyn's "agenda" for joining).
    /// If null, standard recruitment chance applies.
    /// </summary>
    public RecruitmentCondition? RecruitCondition { get; set; }

    /// <summary>
    /// Custom dialog to show when recruitment conditions aren't met.
    /// </summary>
    public string? AgendaDialog { get; set; }

    /// <summary>
    /// Bounding rectangle for collision detection.
    /// </summary>
    public Rectangle BoundingBox => new Rectangle(
        (int)Position.X - SpriteSize / 2,
        (int)Position.Y - SpriteSize / 2,
        SpriteSize,
        SpriteSize
    );

    /// <summary>
    /// Creates a new wild Kyn.
    /// </summary>
    /// <param name="id">Unique instance identifier.</param>
    /// <param name="kynDefinitionId">The Kyn definition/species ID.</param>
    /// <param name="position">World position.</param>
    /// <param name="biome">Biome this Kyn inhabits.</param>
    /// <param name="level">Level of this Kyn.</param>
    public WildKyn(string id, string kynDefinitionId, Vector2 position, BiomeType biome, int level)
    {
        Id = id;
        KynDefinitionId = kynDefinitionId;
        Position = position;
        Biome = biome;
        Level = Math.Max(1, level);
        IsDefeated = false;
        IsRecruited = false;
    }

    /// <summary>
    /// Loads the shared sprite texture.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device for texture loading.</param>
    /// <param name="contentPath">Path to the Content folder.</param>
    public static void LoadContent(GraphicsDevice graphicsDevice, string contentPath)
    {
        if (_spriteLoadAttempted)
            return;

        _spriteLoadAttempted = true;

        try
        {
            // Build sprite path using content directory (matches Protagonist pattern)
            var spritePath = Path.Combine(contentPath, "Sprites", "DefaultKyn", SpriteFileName);

            if (File.Exists(spritePath))
            {
                using var stream = File.OpenRead(spritePath);
                _sharedSprite = Texture2D.FromStream(graphicsDevice, stream);
                System.Diagnostics.Debug.WriteLine($"[WildKyn] Loaded sprite from: {spritePath}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[WildKyn] Sprite not found at: {spritePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WildKyn] Failed to load sprite: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the player collides with this wild Kyn.
    /// </summary>
    /// <param name="playerBounds">Player's bounding box.</param>
    /// <returns>True if collision detected.</returns>
    public bool CheckCollision(Rectangle playerBounds)
    {
        if (IsRecruited)
            return false;

        return BoundingBox.Intersects(playerBounds);
    }

    /// <summary>
    /// Updates the facing direction to look toward the player.
    /// </summary>
    /// <param name="playerPosition">Player's current position.</param>
    public void UpdateFacing(Vector2 playerPosition)
    {
        var direction = playerPosition - Position;

        if (direction.LengthSquared() < 1f)
            return;

        // Calculate angle and convert to 8-way direction
        float angle = MathF.Atan2(direction.Y, direction.X);
        float degrees = MathHelper.ToDegrees(angle);

        // Normalize to 0-360
        if (degrees < 0)
            degrees += 360;

        // Map to 8 directions (each direction covers 45 degrees)
        FacingDirection = degrees switch
        {
            >= 337.5f or < 22.5f => Direction.East,
            >= 22.5f and < 67.5f => Direction.SouthEast,
            >= 67.5f and < 112.5f => Direction.South,
            >= 112.5f and < 157.5f => Direction.SouthWest,
            >= 157.5f and < 202.5f => Direction.West,
            >= 202.5f and < 247.5f => Direction.NorthWest,
            >= 247.5f and < 292.5f => Direction.North,
            _ => Direction.NorthEast
        };
    }

    /// <summary>
    /// Gets the sprite frame index for the current facing direction.
    /// </summary>
    private int GetSpriteFrameIndex()
    {
        return FacingDirection switch
        {
            Direction.South => 0,
            Direction.SouthWest => 1,
            Direction.West => 2,
            Direction.NorthWest => 3,
            Direction.North => 4,
            Direction.NorthEast => 5,
            Direction.East => 6,
            Direction.SouthEast => 7,
            _ => 0
        };
    }

    /// <summary>
    /// Draws the wild Kyn.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture for fallback/indicators.</param>
    /// <param name="font">Font for status indicators.</param>
    /// <param name="cameraOffset">Camera offset for world-to-screen transformation.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, Vector2 cameraOffset)
    {
        if (IsRecruited)
            return;

        var screenPos = Position - cameraOffset;

        if (_sharedSprite != null)
        {
            // Draw sprite frame based on facing direction
            int frameIndex = GetSpriteFrameIndex();
            var sourceRect = new Rectangle(frameIndex * SpriteSize, 0, SpriteSize, SpriteSize);
            var destRect = new Rectangle(
                (int)(screenPos.X - SpriteSize / 2),
                (int)(screenPos.Y - SpriteSize / 2),
                SpriteSize,
                SpriteSize
            );

            // Tint differently if defeated (ready for recruitment)
            var tint = IsDefeated ? new Color(180, 220, 255) : Color.White;
            spriteBatch.Draw(_sharedSprite, destRect, sourceRect, tint);
        }
        else
        {
            // Fallback: colored square
            var color = IsDefeated ? Color.LightBlue : Color.CornflowerBlue;
            var drawRect = new Rectangle(
                (int)(screenPos.X - SpriteSize / 2),
                (int)(screenPos.Y - SpriteSize / 2),
                SpriteSize,
                SpriteSize
            );
            spriteBatch.Draw(pixelTexture, drawRect, color);
        }

        // Draw status indicator
        if (font != null)
        {
            string indicator = IsDefeated ? "?" : "!";
            var indicatorColor = IsDefeated ? Color.LightGreen : Color.Yellow;
            var textSize = font.MeasureString(indicator);
            var textPos = new Vector2(
                screenPos.X - textSize.X / 2,
                screenPos.Y - SpriteSize / 2 - textSize.Y - 2
            );
            spriteBatch.DrawString(font, indicator, textPos, indicatorColor);
        }
    }

    /// <summary>
    /// Creates a Kyn instance for combat (first encounter).
    /// </summary>
    /// <returns>A hostile Kyn ready for combat.</returns>
    public Kyn? CreateKynForCombat()
    {
        var kyn = Kyn.Create(KynDefinitionId, Level);
        if (kyn != null)
        {
            kyn.IsHostile = true;
        }
        return kyn;
    }

    /// <summary>
    /// Creates a Kyn instance for recruitment (after defeat).
    /// </summary>
    /// <returns>A non-hostile Kyn ready for recruitment.</returns>
    public Kyn? CreateKynForRecruitment()
    {
        var kyn = Kyn.Create(KynDefinitionId, Level);
        if (kyn != null)
        {
            kyn.IsHostile = false;
            kyn.FullHeal(); // Restored for recruitment
        }
        return kyn;
    }

    /// <summary>
    /// Creates a wild Kyn with a random position in a chunk area.
    /// </summary>
    /// <param name="id">Unique identifier.</param>
    /// <param name="kynDefinitionId">Kyn species/type.</param>
    /// <param name="biome">Biome type.</param>
    /// <param name="chunkBounds">Chunk boundaries.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>A new wild Kyn instance.</returns>
    public static WildKyn CreateInChunk(
        string id,
        string kynDefinitionId,
        BiomeType biome,
        Rectangle chunkBounds,
        Random random)
    {
        // Random position within chunk (with margin)
        int margin = SpriteSize;
        var position = new Vector2(
            random.Next(chunkBounds.X + margin, chunkBounds.Right - margin),
            random.Next(chunkBounds.Y + margin, chunkBounds.Bottom - margin)
        );

        // Level based on biome
        var levelRange = BiomeData.GetLevelRange(biome);
        int level = random.Next(levelRange.Min, levelRange.Max + 1);

        return new WildKyn(id, kynDefinitionId, position, biome, level)
        {
            FacingDirection = (Direction)random.Next(8)
        };
    }
}
