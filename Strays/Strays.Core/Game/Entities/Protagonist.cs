using System;
using System.IO;
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

    // Sprite constants
    private const int FrameWidth = 104;
    private const int FrameHeight = 104;
    private const int IdleFrameCount = 4;
    private const int WalkFrameCount = 6;
    private const int RunFrameCount = 6;
    private const float AnimationSpeed = 0.15f;  // Seconds per frame

    // Collision box (smaller than sprite for better feel)
    private const int CollisionWidth = 40;
    private const int CollisionHeight = 30;

    // State
    private Vector2 _position;
    private Vector2 _velocity;
    private Direction _facing = Direction.South;
    private float _moveTime = 0f;
    private bool _isMoving = false;

    // Animation state
    private float _animationTimer = 0f;
    private int _currentFrame = 0;

    // Sprites
    private Texture2D? _idleSheet;
    private Texture2D? _walkSheet;
    private Texture2D? _runSheet;
    private bool _spritesLoaded = false;

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
        (int)_position.X - CollisionWidth / 2,
        (int)_position.Y - CollisionHeight / 2 + FrameHeight / 4, // Offset down for feet
        CollisionWidth,
        CollisionHeight
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
    /// Loads the protagonist sprites.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device for texture creation.</param>
    /// <param name="contentPath">Base path to content folder.</param>
    public void LoadContent(GraphicsDevice graphicsDevice, string contentPath)
    {
        string spritePath = Path.Combine(contentPath, "Sprites", "DefaultMale", "spritesheets");

        try
        {
            string idlePath = Path.Combine(spritePath, "idle.png");
            string walkPath = Path.Combine(spritePath, "walk.png");
            string runPath = Path.Combine(spritePath, "run.png");

            if (File.Exists(idlePath))
            {
                using var stream = File.OpenRead(idlePath);
                _idleSheet = Texture2D.FromStream(graphicsDevice, stream);
            }

            if (File.Exists(walkPath))
            {
                using var stream = File.OpenRead(walkPath);
                _walkSheet = Texture2D.FromStream(graphicsDevice, stream);
            }

            if (File.Exists(runPath))
            {
                using var stream = File.OpenRead(runPath);
                _runSheet = Texture2D.FromStream(graphicsDevice, stream);
            }

            _spritesLoaded = _idleSheet != null && _walkSheet != null && _runSheet != null;

            if (_spritesLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Protagonist sprites loaded successfully.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Warning: Some protagonist sprites failed to load.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading protagonist sprites: {ex.Message}");
            _spritesLoaded = false;
        }
    }

    /// <summary>
    /// Unloads sprite resources.
    /// </summary>
    public void UnloadContent()
    {
        _idleSheet?.Dispose();
        _walkSheet?.Dispose();
        _runSheet?.Dispose();
        _idleSheet = null;
        _walkSheet = null;
        _runSheet = null;
        _spritesLoaded = false;
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

        // Update animation
        UpdateAnimation(deltaTime);
    }

    /// <summary>
    /// Updates the animation frame.
    /// </summary>
    private void UpdateAnimation(float deltaTime)
    {
        _animationTimer += deltaTime;

        // Determine current frame count based on state
        int frameCount = _isMoving ? (IsRunning ? RunFrameCount : WalkFrameCount) : IdleFrameCount;

        // Adjust animation speed based on movement (faster when running)
        float currentAnimSpeed = IsRunning ? AnimationSpeed * 0.7f : AnimationSpeed;

        if (_animationTimer >= currentAnimSpeed)
        {
            _animationTimer -= currentAnimSpeed;
            _currentFrame = (_currentFrame + 1) % frameCount;
        }

        // Reset frame when switching animation states
        if (_currentFrame >= frameCount)
        {
            _currentFrame = 0;
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
    /// Draws the protagonist.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture for fallback drawing.</param>
    /// <param name="cameraOffset">Camera offset for world-to-screen transformation.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 cameraOffset)
    {
        // Calculate screen position
        var screenPos = _position - cameraOffset;

        if (_spritesLoaded)
        {
            // Get the appropriate spritesheet
            Texture2D? currentSheet;
            int frameCount;

            if (!_isMoving)
            {
                currentSheet = _idleSheet;
                frameCount = IdleFrameCount;
            }
            else if (IsRunning)
            {
                currentSheet = _runSheet;
                frameCount = RunFrameCount;
            }
            else
            {
                currentSheet = _walkSheet;
                frameCount = WalkFrameCount;
            }

            if (currentSheet != null)
            {
                // Get the row based on direction
                int row = GetDirectionRow(_facing);

                // Ensure frame is in bounds
                int frame = _currentFrame % frameCount;

                // Calculate source rectangle from spritesheet
                var sourceRect = new Rectangle(
                    frame * FrameWidth,
                    row * FrameHeight,
                    FrameWidth,
                    FrameHeight
                );

                // Calculate destination rectangle (centered on position)
                var destRect = new Rectangle(
                    (int)(screenPos.X - FrameWidth / 2),
                    (int)(screenPos.Y - FrameHeight / 2),
                    FrameWidth,
                    FrameHeight
                );

                spriteBatch.Draw(currentSheet, destRect, sourceRect, Color.White);
                return;
            }
        }

        // Fallback: draw placeholder rectangle if sprites not loaded
        DrawPlaceholder(spriteBatch, pixelTexture, screenPos);
    }

    /// <summary>
    /// Draws a placeholder rectangle when sprites aren't available.
    /// </summary>
    private void DrawPlaceholder(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 screenPos)
    {
        Color color;
        if (!CanWalk)
        {
            color = Color.DarkBlue;
        }
        else if (IsRunning)
        {
            color = Color.CornflowerBlue;
        }
        else
        {
            color = Color.Blue;
        }

        var drawRect = new Rectangle(
            (int)(screenPos.X - CollisionWidth / 2),
            (int)(screenPos.Y - CollisionHeight),
            CollisionWidth,
            CollisionHeight * 2
        );

        spriteBatch.Draw(pixelTexture, drawRect, color);
    }

    /// <summary>
    /// Gets the row index in the spritesheet for a given direction.
    /// Rows are: south, south-east, east, north-east, north, north-west, west, south-west
    /// </summary>
    private static int GetDirectionRow(Direction direction)
    {
        return direction switch
        {
            Direction.South => 0,
            Direction.SouthEast => 1,
            Direction.East => 2,
            Direction.NorthEast => 3,
            Direction.North => 4,
            Direction.NorthWest => 5,
            Direction.West => 6,
            Direction.SouthWest => 7,
            _ => 0
        };
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
