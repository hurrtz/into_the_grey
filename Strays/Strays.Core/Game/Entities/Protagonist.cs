using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Services;

namespace Strays.Core.Game.Entities;

/// <summary>
/// The protagonist - a Bio-Shell awakened prematurely from stasis.
/// Physically weak and dependent on Strays for survival.
/// </summary>
public class Protagonist
{
    // Movement constants
    private const float CrawlSpeed = 50f;        // Speed without exoskeleton (can barely move)
    private const float WalkSpeed = 150f;        // Speed with unpowered exoskeleton
    private const float RunSpeed = 300f;         // Speed with powered exoskeleton
    private const float RunThreshold = 0.8f;     // Seconds of movement before running

    // Visual constants (placeholder)
    private const int Width = 32;
    private const int Height = 48;

    // State
    private Vector2 _position;
    private Vector2 _velocity;
    private Direction _facing = Direction.South;
    private float _moveTime = 0f;
    private bool _isMoving = false;

    // Reference to game state for checking exoskeleton status
    private readonly GameStateService? _gameState;

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
    public Direction Facing
    {
        get => _facing;
        set => _facing = value;
    }

    /// <summary>
    /// Whether the protagonist is currently moving.
    /// </summary>
    public bool IsMoving => _isMoving;

    /// <summary>
    /// Whether the protagonist is running (powered exoskeleton + sustained movement).
    /// </summary>
    public bool IsRunning => _isMoving && _moveTime > RunThreshold && CanRun;

    /// <summary>
    /// Whether the protagonist can run (has powered exoskeleton).
    /// </summary>
    public bool CanRun => _gameState?.ExoskeletonPowered ?? false;

    /// <summary>
    /// Whether the protagonist can walk normally (has exoskeleton).
    /// </summary>
    public bool CanWalk => _gameState?.HasExoskeleton ?? false;

    /// <summary>
    /// Bounding rectangle for collision detection.
    /// </summary>
    public Rectangle BoundingBox => new Rectangle(
        (int)_position.X - Width / 2,
        (int)_position.Y - Height / 2,
        Width,
        Height
    );

    /// <summary>
    /// Creates a new protagonist.
    /// </summary>
    /// <param name="gameState">Reference to game state service (optional but recommended).</param>
    public Protagonist(GameStateService? gameState = null)
    {
        _gameState = gameState;
        _position = Vector2.Zero;
    }

    /// <summary>
    /// Updates the protagonist based on input.
    /// </summary>
    /// <param name="gameTime">Current game time.</param>
    /// <param name="movementInput">Normalized movement input vector (-1 to 1 on each axis).</param>
    public void Update(GameTime gameTime, Vector2 movementInput)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Check if there's any movement input
        _isMoving = movementInput.LengthSquared() > 0.01f;

        if (_isMoving)
        {
            // Update movement time for run threshold
            _moveTime += deltaTime;

            // Normalize input if needed
            if (movementInput.Length() > 1f)
            {
                movementInput.Normalize();
            }

            // Calculate speed based on state
            float speed = GetCurrentSpeed();

            // Apply movement
            _velocity = movementInput * speed;
            _position += _velocity * deltaTime;

            // Update facing direction based on movement
            _facing = GetDirectionFromVector(movementInput);

            // Sync position to game state
            if (_gameState != null)
            {
                _gameState.ProtagonistPosition = _position;
            }
        }
        else
        {
            // Reset movement time when stopped
            _moveTime = 0f;
            _velocity = Vector2.Zero;
        }
    }

    /// <summary>
    /// Gets the current movement speed based on state.
    /// </summary>
    private float GetCurrentSpeed()
    {
        if (!CanWalk)
        {
            // No exoskeleton - can barely crawl
            return CrawlSpeed;
        }

        if (CanRun && _moveTime > RunThreshold)
        {
            // Powered exoskeleton and sustained movement - running
            return RunSpeed;
        }

        // Walking speed
        return WalkSpeed;
    }

    /// <summary>
    /// Converts a movement vector to the nearest Direction.
    /// </summary>
    private static Direction GetDirectionFromVector(Vector2 vector)
    {
        if (vector.LengthSquared() < 0.01f)
            return Direction.South;

        // Calculate angle from vector (0 = right, counter-clockwise)
        float angle = MathF.Atan2(vector.Y, vector.X);

        // Convert to degrees and normalize to 0-360
        float degrees = MathHelper.ToDegrees(angle);
        if (degrees < 0) degrees += 360;

        // Map to 8 directions (each direction covers 45 degrees)
        // East = 0, South = 90, West = 180, North = 270
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
    /// Draws the protagonist (placeholder: blue rectangle).
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture for drawing shapes.</param>
    /// <param name="cameraOffset">Camera offset for world-to-screen transformation.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 cameraOffset)
    {
        // Calculate screen position
        var screenPos = _position - cameraOffset;

        // Choose color based on state
        Color color;
        if (!CanWalk)
        {
            color = Color.DarkBlue; // Crawling - darker
        }
        else if (IsRunning)
        {
            color = Color.CornflowerBlue; // Running - lighter
        }
        else
        {
            color = Color.Blue; // Walking - normal
        }

        // Draw the protagonist as a rectangle
        var drawRect = new Rectangle(
            (int)(screenPos.X - Width / 2),
            (int)(screenPos.Y - Height / 2),
            Width,
            Height
        );

        spriteBatch.Draw(pixelTexture, drawRect, color);

        // Draw a small direction indicator
        var indicatorOffset = GetDirectionVector(_facing) * 20f;
        var indicatorPos = screenPos + indicatorOffset;
        var indicatorRect = new Rectangle(
            (int)(indicatorPos.X - 4),
            (int)(indicatorPos.Y - 4),
            8,
            8
        );
        spriteBatch.Draw(pixelTexture, indicatorRect, Color.White);
    }

    /// <summary>
    /// Gets a unit vector for a direction.
    /// </summary>
    private static Vector2 GetDirectionVector(Direction direction)
    {
        return direction switch
        {
            Direction.North => new Vector2(0, -1),
            Direction.NorthEast => Vector2.Normalize(new Vector2(1, -1)),
            Direction.East => new Vector2(1, 0),
            Direction.SouthEast => Vector2.Normalize(new Vector2(1, 1)),
            Direction.South => new Vector2(0, 1),
            Direction.SouthWest => Vector2.Normalize(new Vector2(-1, 1)),
            Direction.West => new Vector2(-1, 0),
            Direction.NorthWest => Vector2.Normalize(new Vector2(-1, -1)),
            _ => Vector2.Zero
        };
    }

    /// <summary>
    /// Applies collision resolution by moving the protagonist out of a solid area.
    /// </summary>
    /// <param name="pushVector">Vector to push the protagonist by.</param>
    public void ApplyCollisionPush(Vector2 pushVector)
    {
        _position += pushVector;

        if (_gameState != null)
        {
            _gameState.ProtagonistPosition = _position;
        }
    }

    /// <summary>
    /// Teleports the protagonist to a new position.
    /// </summary>
    public void Teleport(Vector2 newPosition)
    {
        _position = newPosition;
        _velocity = Vector2.Zero;
        _moveTime = 0f;
        _isMoving = false;

        if (_gameState != null)
        {
            _gameState.ProtagonistPosition = _position;
        }
    }
}
