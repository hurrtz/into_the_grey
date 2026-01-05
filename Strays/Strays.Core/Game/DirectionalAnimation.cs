using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Strays.Core
{
    /// <summary>
    /// Represents an animation with 8 directional variants loaded from a sprite sheet.
    /// The sprite sheet format is: 8 rows (directions) × N columns (frames).
    /// Direction order: South, SouthEast, East, NorthEast, North, NorthWest, West, SouthWest.
    /// </summary>
    public class DirectionalAnimation
    {
        private readonly Texture2D spriteSheet;
        private readonly int frameWidth;
        private readonly int frameHeight;
        private readonly int frameCount;
        private readonly float frameTime;
        private readonly bool isLooping;

        // Direction order in the sprite sheet (row index)
        private static readonly Direction[] DirectionOrder =
        {
            Direction.South,
            Direction.SouthEast,
            Direction.East,
            Direction.NorthEast,
            Direction.North,
            Direction.NorthWest,
            Direction.West,
            Direction.SouthWest
        };

        /// <summary>
        /// Duration of time to show each frame.
        /// </summary>
        public float FrameTime => frameTime;

        /// <summary>
        /// Whether the animation loops.
        /// </summary>
        public bool IsLooping => isLooping;

        /// <summary>
        /// Gets the number of frames in the animation.
        /// </summary>
        public int FrameCount => frameCount;

        /// <summary>
        /// Gets the width of a frame.
        /// </summary>
        public int FrameWidth => frameWidth;

        /// <summary>
        /// Gets the height of a frame.
        /// </summary>
        public int FrameHeight => frameHeight;

        /// <summary>
        /// Gets the sprite sheet texture.
        /// </summary>
        public Texture2D SpriteSheet => spriteSheet;

        /// <summary>
        /// Creates a new directional animation from a sprite sheet.
        /// </summary>
        /// <param name="spriteSheet">The sprite sheet texture (8 rows × N columns).</param>
        /// <param name="frameWidth">Width of each frame in pixels.</param>
        /// <param name="frameHeight">Height of each frame in pixels.</param>
        /// <param name="frameCount">Number of frames per direction.</param>
        /// <param name="frameTime">Duration of each frame in seconds.</param>
        /// <param name="isLooping">Whether the animation should loop.</param>
        public DirectionalAnimation(Texture2D spriteSheet, int frameWidth, int frameHeight,
            int frameCount, float frameTime, bool isLooping)
        {
            this.spriteSheet = spriteSheet;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.frameCount = frameCount;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
        }

        /// <summary>
        /// Gets the source rectangle for a specific direction and frame.
        /// </summary>
        public Rectangle GetSourceRectangle(Direction direction, int frameIndex)
        {
            int row = GetRowForDirection(direction);
            int col = Math.Min(frameIndex, frameCount - 1);

            return new Rectangle(
                col * frameWidth,
                row * frameHeight,
                frameWidth,
                frameHeight);
        }

        /// <summary>
        /// Gets the row index for a direction.
        /// </summary>
        private int GetRowForDirection(Direction direction)
        {
            for (int i = 0; i < DirectionOrder.Length; i++)
            {
                if (DirectionOrder[i] == direction)
                    return i;
            }
            return 0; // Default to South
        }

        /// <summary>
        /// Loads a directional animation from a sprite sheet file.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="filePath">Path to the sprite sheet PNG file.</param>
        /// <param name="frameWidth">Width of each frame.</param>
        /// <param name="frameHeight">Height of each frame.</param>
        /// <param name="frameTime">Duration of each frame in seconds.</param>
        /// <param name="isLooping">Whether the animation should loop.</param>
        public static DirectionalAnimation LoadFromSpriteSheet(
            GraphicsDevice graphicsDevice,
            string filePath,
            int frameWidth,
            int frameHeight,
            float frameTime,
            bool isLooping)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Sprite sheet not found: {filePath}");
                return null;
            }

            Texture2D spriteSheet;
            using (var stream = File.OpenRead(filePath))
            {
                spriteSheet = Texture2D.FromStream(graphicsDevice, stream);
            }

            int frameCount = spriteSheet.Width / frameWidth;
            Console.WriteLine($"Loaded sprite sheet: {Path.GetFileName(filePath)} ({frameCount} frames, {spriteSheet.Width}x{spriteSheet.Height})");

            return new DirectionalAnimation(spriteSheet, frameWidth, frameHeight, frameCount, frameTime, isLooping);
        }

        /// <summary>
        /// Loads from ContentManager (for MGCB-processed content).
        /// </summary>
        public static DirectionalAnimation LoadFromContent(
            Texture2D spriteSheet,
            int frameWidth,
            int frameHeight,
            float frameTime,
            bool isLooping)
        {
            int frameCount = spriteSheet.Width / frameWidth;
            return new DirectionalAnimation(spriteSheet, frameWidth, frameHeight, frameCount, frameTime, isLooping);
        }
    }
}
