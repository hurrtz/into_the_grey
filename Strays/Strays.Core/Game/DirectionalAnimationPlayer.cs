using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Strays.Core
{
    /// <summary>
    /// Controls playback of a DirectionalAnimation from a sprite sheet.
    /// </summary>
    public class DirectionalAnimationPlayer
    {
        private DirectionalAnimation animation;
        private int frameIndex;
        private float time;
        private Direction currentDirection;

        /// <summary>
        /// Gets the current animation.
        /// </summary>
        public DirectionalAnimation Animation => animation;

        /// <summary>
        /// Gets the current frame index.
        /// </summary>
        public int FrameIndex => frameIndex;

        /// <summary>
        /// Gets or sets the current direction.
        /// </summary>
        public Direction CurrentDirection
        {
            get => currentDirection;
            set => currentDirection = value;
        }

        /// <summary>
        /// Gets the origin point at the center-bottom of the frame.
        /// </summary>
        public Vector2 Origin
        {
            get
            {
                if (animation != null)
                    return new Vector2(animation.FrameWidth / 2.0f, animation.FrameHeight);
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Begins or continues playback of an animation.
        /// </summary>
        public void PlayAnimation(DirectionalAnimation newAnimation)
        {
            if (animation == newAnimation)
                return;

            animation = newAnimation;
            frameIndex = 0;
            time = 0.0f;
        }

        /// <summary>
        /// Updates the animation timing and advances frames.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (animation == null || animation.FrameCount <= 0 || animation.FrameTime <= 0)
                return;

            time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (time > animation.FrameTime)
            {
                time -= animation.FrameTime;

                if (animation.IsLooping)
                {
                    frameIndex = (frameIndex + 1) % animation.FrameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, animation.FrameCount - 1);
                }
            }
        }

        /// <summary>
        /// Draws the current frame at the specified position.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, Texture2D fallbackTexture = null)
        {
            if (animation?.SpriteSheet != null)
            {
                Rectangle sourceRect = animation.GetSourceRectangle(currentDirection, frameIndex);

                spriteBatch.Draw(
                    animation.SpriteSheet,
                    position,
                    sourceRect,
                    color,
                    0.0f,
                    Origin,
                    1.0f,
                    SpriteEffects.None,
                    0.0f);
            }
            else if (fallbackTexture != null)
            {
                // Draw a fallback texture if animation didn't load
                spriteBatch.Draw(
                    fallbackTexture,
                    new Rectangle((int)(position.X - 52), (int)(position.Y - 104), 104, 104),
                    color);
            }
        }

        /// <summary>
        /// Draws the current frame at the specified position with default color.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            Draw(spriteBatch, position, Color.White, null);
        }
    }
}
