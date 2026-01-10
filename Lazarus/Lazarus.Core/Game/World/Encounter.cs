using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lazarus.Core.Game.World;

/// <summary>
/// Represents a visible enemy encounter on the world map.
/// Following Mystic Quest philosophy - no random encounters, all enemies visible.
/// </summary>
public class Encounter
{
    // Visual constants (placeholder)
    private const int Size = 24;

    /// <summary>
    /// Unique identifier for this encounter.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// World position of this encounter.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The biome this encounter belongs to.
    /// </summary>
    public BiomeType Biome { get; }

    /// <summary>
    /// Whether this encounter has been defeated and cleared.
    /// </summary>
    public bool IsCleared { get; set; }

    /// <summary>
    /// Whether this encounter respawns after being cleared.
    /// </summary>
    public bool Respawns { get; set; } = true;

    /// <summary>
    /// The level range of enemies in this encounter.
    /// </summary>
    public (int Min, int Max) LevelRange { get; set; } = (1, 5);

    /// <summary>
    /// IDs of Kyn definitions that can appear in this encounter.
    /// </summary>
    public List<string> PossibleKyns { get; } = new();

    /// <summary>
    /// Number of enemies in this encounter.
    /// </summary>
    public int EnemyCount { get; set; } = 1;

    /// <summary>
    /// Whether this encounter can result in recruitment after victory.
    /// </summary>
    public bool CanRecruit { get; set; } = true;

    /// <summary>
    /// Bounding rectangle for collision detection.
    /// </summary>
    public Rectangle BoundingBox => new Rectangle(
        (int)Position.X - Size / 2,
        (int)Position.Y - Size / 2,
        Size,
        Size
    );

    /// <summary>
    /// Creates a new encounter.
    /// </summary>
    /// <param name="id">Unique identifier.</param>
    /// <param name="position">World position.</param>
    /// <param name="biome">Biome this encounter belongs to.</param>
    public Encounter(string id, Vector2 position, BiomeType biome)
    {
        Id = id;
        Position = position;
        Biome = biome;
        IsCleared = false;

        // Set default level range based on biome
        LevelRange = BiomeData.GetLevelRange(biome);
    }

    /// <summary>
    /// Checks if the protagonist collides with this encounter.
    /// </summary>
    /// <param name="protagonistBounds">Protagonist's bounding box.</param>
    /// <returns>True if collision detected.</returns>
    public bool CheckCollision(Rectangle protagonistBounds)
    {
        if (IsCleared)
            return false;

        return BoundingBox.Intersects(protagonistBounds);
    }

    /// <summary>
    /// Marks this encounter as cleared.
    /// </summary>
    public void Clear()
    {
        IsCleared = true;
    }

    /// <summary>
    /// Respawns this encounter if it's allowed to respawn.
    /// </summary>
    public void TryRespawn()
    {
        if (Respawns)
        {
            IsCleared = false;
        }
    }

    /// <summary>
    /// Generates the enemy party for combat.
    /// </summary>
    /// <param name="random">Random number generator.</param>
    /// <returns>List of Kyn definition IDs to spawn.</returns>
    public List<(string DefinitionId, int Level)> GenerateEnemyParty(Random random)
    {
        var result = new List<(string, int)>();

        for (int i = 0; i < EnemyCount; i++)
        {
            // Pick a random Kyn from possible list
            string kynId = PossibleKyns.Count > 0
                ? PossibleKyns[random.Next(PossibleKyns.Count)]
                : "wild_kyn"; // Default placeholder

            // Pick a random level in range
            int level = random.Next(LevelRange.Min, LevelRange.Max + 1);

            result.Add((kynId, level));
        }

        return result;
    }

    /// <summary>
    /// Draws the encounter (placeholder: red square with "!").
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture.</param>
    /// <param name="font">Font for drawing the "!" indicator.</param>
    /// <param name="cameraOffset">Camera offset for world-to-screen transformation.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, Vector2 cameraOffset)
    {
        if (IsCleared)
            return;

        var screenPos = Position - cameraOffset;

        // Draw red square
        var drawRect = new Rectangle(
            (int)(screenPos.X - Size / 2),
            (int)(screenPos.Y - Size / 2),
            Size,
            Size
        );

        spriteBatch.Draw(pixelTexture, drawRect, Color.Red);

        // Draw "!" indicator
        if (font != null)
        {
            var text = "!";
            var textSize = font.MeasureString(text);
            var textPos = new Vector2(
                screenPos.X - textSize.X / 2,
                screenPos.Y - Size / 2 - textSize.Y - 2
            );
            spriteBatch.DrawString(font, text, textPos, Color.Yellow);
        }
    }
}

/// <summary>
/// Result of an encounter battle.
/// </summary>
public class EncounterResult
{
    /// <summary>
    /// Whether the player won the battle.
    /// </summary>
    public bool Victory { get; set; }

    /// <summary>
    /// Whether the player fled from battle.
    /// </summary>
    public bool Fled { get; set; }

    /// <summary>
    /// Experience gained from the battle.
    /// </summary>
    public int ExperienceGained { get; set; }

    /// <summary>
    /// ID of a Kyn that was recruited (null if none).
    /// </summary>
    public string? RecruitedKynId { get; set; }

    /// <summary>
    /// Items dropped from the battle.
    /// </summary>
    public List<string> DroppedItems { get; } = new();
}
