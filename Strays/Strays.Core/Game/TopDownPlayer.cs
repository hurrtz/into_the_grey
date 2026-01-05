using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core.Inputs;

namespace Strays.Core
{
    /// <summary>
    /// Player character for top-down gameplay with 8-directional movement.
    /// </summary>
    public class TopDownPlayer
    {
        // Animation fields
        private DirectionalAnimation idleAnimation;
        private DirectionalAnimation walkAnimation;
        private DirectionalAnimation runAnimation;
        private DirectionalAnimationPlayer sprite;
        private Texture2D fallbackTexture;

        // Movement constants
        private const float WalkSpeed = 150.0f;
        private const float RunSpeed = 300.0f;
        private const float RunThreshold = 0.8f; // How long to hold before running

        // Frame size for DefaultMale character
        private const int FrameWidth = 104;
        private const int FrameHeight = 104;

        // State
        private Vector2 position;
        private Vector2 velocity;
        private Direction facingDirection;
        private float moveHoldTime;
        private bool isMoving;

        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// Gets or sets the player's position.
        /// </summary>
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// Gets the player's velocity.
        /// </summary>
        public Vector2 Velocity => velocity;

        /// <summary>
        /// Gets the player's facing direction.
        /// </summary>
        public Direction FacingDirection => facingDirection;

        /// <summary>
        /// Gets whether the player is currently moving.
        /// </summary>
        public bool IsMoving => isMoving;

        /// <summary>
        /// Gets the bounding rectangle for collision detection.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int width = 40;
                int height = 40;
                return new Rectangle(
                    (int)(position.X - width / 2),
                    (int)(position.Y - height),
                    width,
                    height);
            }
        }

        /// <summary>
        /// Creates a new top-down player.
        /// </summary>
        public TopDownPlayer(GraphicsDevice graphicsDevice, Vector2 startPosition)
        {
            this.graphicsDevice = graphicsDevice;
            this.position = startPosition;
            this.facingDirection = Direction.South;
            this.sprite = new DirectionalAnimationPlayer();
        }

        /// <summary>
        /// Loads the player's content from sprite sheets.
        /// </summary>
        public void LoadContent(string spritesPath)
        {
            // Create a fallback texture (a simple colored rectangle)
            fallbackTexture = new Texture2D(graphicsDevice, FrameWidth, FrameHeight);
            Color[] colorData = new Color[FrameWidth * FrameHeight];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = Color.CornflowerBlue;
            }
            fallbackTexture.SetData(colorData);

            string sheetsPath = Path.Combine(spritesPath, "DefaultMale", "spritesheets");
            Console.WriteLine($"TopDownPlayer loading sprite sheets from: {sheetsPath}");

            // Load animations from sprite sheets
            idleAnimation = DirectionalAnimation.LoadFromSpriteSheet(
                graphicsDevice,
                Path.Combine(sheetsPath, "idle.png"),
                FrameWidth,
                FrameHeight,
                0.2f,
                true);

            walkAnimation = DirectionalAnimation.LoadFromSpriteSheet(
                graphicsDevice,
                Path.Combine(sheetsPath, "walk.png"),
                FrameWidth,
                FrameHeight,
                0.12f,
                true);

            runAnimation = DirectionalAnimation.LoadFromSpriteSheet(
                graphicsDevice,
                Path.Combine(sheetsPath, "run.png"),
                FrameWidth,
                FrameHeight,
                0.08f,
                true);

            sprite.PlayAnimation(idleAnimation);
            sprite.CurrentDirection = facingDirection;
        }

        /// <summary>
        /// Updates the player state.
        /// </summary>
        public void Update(GameTime gameTime, InputState inputState)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Get movement input
            Vector2 movement = GetMovementInput(inputState);

            if (movement != Vector2.Zero)
            {
                // Normalize for consistent speed in all directions
                movement.Normalize();

                // Track how long we've been moving
                moveHoldTime += elapsed;
                isMoving = true;

                // Determine speed based on how long movement is held
                float speed = moveHoldTime > RunThreshold ? RunSpeed : WalkSpeed;
                velocity = movement * speed;

                // Update position
                position += velocity * elapsed;

                // Update facing direction based on movement
                facingDirection = GetDirectionFromVector(movement);
                sprite.CurrentDirection = facingDirection;

                // Play appropriate animation
                if (moveHoldTime > RunThreshold)
                    sprite.PlayAnimation(runAnimation);
                else
                    sprite.PlayAnimation(walkAnimation);
            }
            else
            {
                // Not moving
                moveHoldTime = 0f;
                velocity = Vector2.Zero;
                isMoving = false;
                sprite.PlayAnimation(idleAnimation);
            }

            // Update animation
            sprite.Update(gameTime);
        }

        /// <summary>
        /// Gets movement input from keyboard.
        /// </summary>
        private Vector2 GetMovementInput(InputState inputState)
        {
            Vector2 movement = Vector2.Zero;

            var keyboard = inputState.CurrentKeyboardStates[0];

            // WASD and Arrow keys
            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
                movement.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
                movement.Y += 1;
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
                movement.X -= 1;
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
                movement.X += 1;

            // Gamepad support
            var gamepad = inputState.CurrentGamePadStates[0];
            if (gamepad.IsConnected)
            {
                movement += gamepad.ThumbSticks.Left * new Vector2(1, -1); // Y is inverted
            }

            return movement;
        }

        /// <summary>
        /// Converts a movement vector to an 8-way direction.
        /// </summary>
        private Direction GetDirectionFromVector(Vector2 movement)
        {
            if (movement == Vector2.Zero)
                return facingDirection;

            // Calculate angle in degrees (0 = right, 90 = down in screen space)
            float angle = MathHelper.ToDegrees((float)Math.Atan2(movement.Y, movement.X));

            // Normalize to 0-360
            if (angle < 0)
                angle += 360;

            // Convert angle to direction (each direction covers 45 degrees)
            // East = 0, SouthEast = 45, South = 90, etc.
            if (angle >= 337.5f || angle < 22.5f)
                return Direction.East;
            else if (angle >= 22.5f && angle < 67.5f)
                return Direction.SouthEast;
            else if (angle >= 67.5f && angle < 112.5f)
                return Direction.South;
            else if (angle >= 112.5f && angle < 157.5f)
                return Direction.SouthWest;
            else if (angle >= 157.5f && angle < 202.5f)
                return Direction.West;
            else if (angle >= 202.5f && angle < 247.5f)
                return Direction.NorthWest;
            else if (angle >= 247.5f && angle < 292.5f)
                return Direction.North;
            else
                return Direction.NorthEast;
        }

        /// <summary>
        /// Draws the player.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch, position, Color.White, fallbackTexture);
        }
    }
}
