using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Game.Data;
using Strays.Core.Services;

namespace Strays.Core.Game.Entities;

/// <summary>
/// The companion Stray - Bandit (dog), Tinker (cat), or Pirate (rabbit).
/// Follows the protagonist on the overworld and can intervene in combat.
/// Named in tribute to We3 by Grant Morrison.
/// </summary>
public class Companion
{
    // Movement constants
    private const float FollowSpeed = 200f;      // Speed when following
    private const float FollowDistance = 50f;    // Preferred distance behind protagonist
    private const float MaxDistance = 150f;      // Max distance before teleporting to catch up
    private const float MinDistance = 30f;       // Minimum distance to maintain

    // Visual constants (placeholder)
    private const int Radius = 12;

    // State
    private Vector2 _position;
    private Vector2 _targetPosition;
    private Direction _facing = Direction.South;
    private bool _isMoving = false;

    // Reference to game state
    private readonly GameStateService? _gameState;

    /// <summary>
    /// The type of companion (determines appearance and name).
    /// </summary>
    public CompanionType Type { get; }

    /// <summary>
    /// The companion's name (Bandit, Tinker, or Pirate).
    /// </summary>
    public string Name => Type.GetCompanionName();

    /// <summary>
    /// The name of the pet as remembered from the simulation.
    /// </summary>
    public string SimulationPetName => Type.GetSimulationPetName();

    /// <summary>
    /// Current world position.
    /// </summary>
    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    /// <summary>
    /// Current facing direction.
    /// </summary>
    public Direction Facing => _facing;

    /// <summary>
    /// Whether the companion is currently moving.
    /// </summary>
    public bool IsMoving => _isMoving;

    /// <summary>
    /// Whether the companion is present with the party.
    /// In Act 3, Bandit leaves to protect the protagonist.
    /// </summary>
    public bool IsPresent => _gameState?.CompanionPresent ?? true;

    /// <summary>
    /// Current Gravitation stage.
    /// </summary>
    public GravitationStage GravitationStage => _gameState?.GravitationStage ?? GravitationStage.Normal;

    /// <summary>
    /// Bounding circle for collision (stored as rectangle for simplicity).
    /// </summary>
    public Rectangle BoundingBox => new Rectangle(
        (int)_position.X - Radius,
        (int)_position.Y - Radius,
        Radius * 2,
        Radius * 2
    );

    /// <summary>
    /// Event fired when Gravitation is used.
    /// </summary>
    public event EventHandler<GravitationEventArgs>? GravitationUsed;

    /// <summary>
    /// Creates a new companion.
    /// </summary>
    /// <param name="type">The type of companion.</param>
    /// <param name="gameState">Reference to game state service.</param>
    public Companion(CompanionType type, GameStateService? gameState = null)
    {
        Type = type;
        _gameState = gameState;
        _position = Vector2.Zero;
    }

    /// <summary>
    /// Updates the companion to follow the protagonist.
    /// </summary>
    /// <param name="gameTime">Current game time.</param>
    /// <param name="protagonistPosition">Position of the protagonist to follow.</param>
    /// <param name="protagonistFacing">Direction the protagonist is facing.</param>
    public void Update(GameTime gameTime, Vector2 protagonistPosition, Direction protagonistFacing)
    {
        if (!IsPresent)
            return;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Calculate ideal position behind the protagonist
        var behindVector = GetOppositeDirectionVector(protagonistFacing);
        _targetPosition = protagonistPosition + behindVector * FollowDistance;

        // Check distance to protagonist
        float distanceToProtagonist = Vector2.Distance(_position, protagonistPosition);

        // If too far, teleport to catch up
        if (distanceToProtagonist > MaxDistance)
        {
            _position = _targetPosition;
            _isMoving = false;
            return;
        }

        // Calculate distance to target position
        float distanceToTarget = Vector2.Distance(_position, _targetPosition);

        // If close enough, stop moving
        if (distanceToTarget < MinDistance)
        {
            _isMoving = false;
            _facing = protagonistFacing;
            return;
        }

        // Move toward target position
        _isMoving = true;
        var direction = Vector2.Normalize(_targetPosition - _position);
        _position += direction * FollowSpeed * deltaTime;
        _facing = GetDirectionFromVector(direction);
    }

    /// <summary>
    /// Attempts to use Gravitation in combat.
    /// Returns whether the ability was used.
    /// </summary>
    /// <param name="random">Random number generator for targeting decisions.</param>
    /// <returns>True if Gravitation was used.</returns>
    public bool TryUseGravitation(Random random)
    {
        if (!IsPresent)
            return false;

        var stage = GravitationStage;
        float damagePercent = stage.GetDamagePercent();
        float allyTargetChance = stage.GetAllyTargetChance();

        // Determine if targeting ally or enemy
        bool targetAlly = random.NextDouble() < allyTargetChance;

        // Fire event for combat system to handle
        GravitationUsed?.Invoke(this, new GravitationEventArgs
        {
            Stage = stage,
            DamagePercent = damagePercent,
            TargetsAlly = targetAlly
        });

        return true;
    }

    /// <summary>
    /// Draws the companion (placeholder: orange circle).
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture for drawing shapes.</param>
    /// <param name="cameraOffset">Camera offset for world-to-screen transformation.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 cameraOffset)
    {
        if (!IsPresent)
            return;

        // Calculate screen position
        var screenPos = _position - cameraOffset;

        // Choose color based on companion type and Gravitation stage
        Color color = GetColor();

        // Draw as a circle (approximated with a square for now)
        var drawRect = new Rectangle(
            (int)(screenPos.X - Radius),
            (int)(screenPos.Y - Radius),
            Radius * 2,
            Radius * 2
        );

        spriteBatch.Draw(pixelTexture, drawRect, color);

        // Draw Gravitation warning indicator if unstable
        if (GravitationStage > GravitationStage.Normal)
        {
            var warningColor = GravitationStage switch
            {
                GravitationStage.Unstable => Color.Yellow,
                GravitationStage.Dangerous => Color.Orange,
                GravitationStage.Critical => Color.Red,
                _ => Color.Transparent
            };

            // Draw a small pulsing ring around the companion
            var ringRect = new Rectangle(
                (int)(screenPos.X - Radius - 4),
                (int)(screenPos.Y - Radius - 4),
                (Radius + 4) * 2,
                (Radius + 4) * 2
            );
            spriteBatch.Draw(pixelTexture, ringRect, warningColor * 0.5f);
        }
    }

    /// <summary>
    /// Gets the color for this companion based on type.
    /// </summary>
    private Color GetColor()
    {
        return Type switch
        {
            CompanionType.Dog => Color.Orange,        // Bandit
            CompanionType.Cat => Color.Purple,        // Tinker
            CompanionType.Rabbit => Color.LightGray,  // Pirate
            _ => Color.Orange
        };
    }

    /// <summary>
    /// Gets a unit vector for the opposite of a direction (for following behind).
    /// </summary>
    private static Vector2 GetOppositeDirectionVector(Direction direction)
    {
        return direction switch
        {
            Direction.North => new Vector2(0, 1),
            Direction.NorthEast => Vector2.Normalize(new Vector2(-1, 1)),
            Direction.East => new Vector2(-1, 0),
            Direction.SouthEast => Vector2.Normalize(new Vector2(-1, -1)),
            Direction.South => new Vector2(0, -1),
            Direction.SouthWest => Vector2.Normalize(new Vector2(1, -1)),
            Direction.West => new Vector2(1, 0),
            Direction.NorthWest => Vector2.Normalize(new Vector2(1, 1)),
            _ => Vector2.Zero
        };
    }

    /// <summary>
    /// Converts a movement vector to the nearest Direction.
    /// </summary>
    private static Direction GetDirectionFromVector(Vector2 vector)
    {
        if (vector.LengthSquared() < 0.01f)
            return Direction.South;

        float angle = MathF.Atan2(vector.Y, vector.X);
        float degrees = MathHelper.ToDegrees(angle);
        if (degrees < 0) degrees += 360;

        return degrees switch
        {
            >= 337.5f or < 22.5f => Direction.East,
            >= 22.5f and < 67.5f => Direction.SouthEast,
            >= 67.5f and < 112.5f => Direction.South,
            >= 112.5f and < 157.5f => Direction.SouthWest,
            >= 157.5f and < 202.5f => Direction.West,
            >= 202.5f and < 247.5f => Direction.NorthWest,
            >= 247.5f and < 292.5f => Direction.North,
            >= 292.5f and < 337.5f => Direction.NorthEast,
            _ => Direction.South
        };
    }

    /// <summary>
    /// Teleports the companion to a position.
    /// </summary>
    public void Teleport(Vector2 newPosition)
    {
        _position = newPosition;
        _isMoving = false;
    }
}

/// <summary>
/// Event arguments for Gravitation usage.
/// </summary>
public class GravitationEventArgs : EventArgs
{
    /// <summary>
    /// The current Gravitation stage.
    /// </summary>
    public GravitationStage Stage { get; init; }

    /// <summary>
    /// The percentage of HP to remove (0.5 = 50%).
    /// </summary>
    public float DamagePercent { get; init; }

    /// <summary>
    /// Whether this Gravitation targets an ally (due to instability).
    /// </summary>
    public bool TargetsAlly { get; init; }
}
