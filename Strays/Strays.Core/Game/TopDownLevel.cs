using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Inputs;
using Strays.ScreenManagers;

namespace Strays.Core
{
    /// <summary>
    /// A top-down level with a Tiled map and a player character.
    /// </summary>
    public class TopDownLevel : IDisposable
    {
        private ScreenManager screenManager;
        private TiledMap map;
        private TopDownPlayer player;
        private Vector2 cameraPosition;
        private string contentPath;

        /// <summary>
        /// Gets the player.
        /// </summary>
        public TopDownPlayer Player => player;

        /// <summary>
        /// Gets the map.
        /// </summary>
        public TiledMap Map => map;

        /// <summary>
        /// Gets or sets the camera position.
        /// </summary>
        public Vector2 CameraPosition
        {
            get => cameraPosition;
            set => cameraPosition = value;
        }

        /// <summary>
        /// Creates a new top-down level.
        /// </summary>
        public TopDownLevel(ScreenManager screenManager, string mapPath, string contentPath)
        {
            this.screenManager = screenManager;
            this.contentPath = contentPath;

            // Initialize map
            map = new TiledMap(screenManager.GraphicsDevice);

            // Load map if it exists
            if (File.Exists(mapPath))
            {
                try
                {
                    map.Load(mapPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load map: {ex.Message}");
                }
            }

            // Create player at center of map (or screen center if map failed to load)
            Vector2 startPosition;
            if (map.MapWidth > 0 && map.MapHeight > 0)
            {
                startPosition = new Vector2(
                    map.PixelWidth / 2f,
                    map.PixelHeight / 2f);
            }
            else
            {
                startPosition = screenManager.BaseScreenSize / 2;
            }

            player = new TopDownPlayer(screenManager.GraphicsDevice, startPosition);

            // Load player content
            string spritesPath = Path.Combine(contentPath, "Sprites");
            player.LoadContent(spritesPath);
        }

        /// <summary>
        /// Updates the level.
        /// </summary>
        public void Update(GameTime gameTime, InputState inputState)
        {
            // Update player
            player.Update(gameTime, inputState);

            // Update camera to follow player
            UpdateCamera();
        }

        /// <summary>
        /// Updates camera position to follow the player.
        /// </summary>
        private void UpdateCamera()
        {
            Vector2 screenSize = screenManager.BaseScreenSize;

            // Center camera on player
            cameraPosition = new Vector2(
                player.Position.X - screenSize.X / 2,
                player.Position.Y - screenSize.Y / 2);

            // Clamp to map bounds
            if (map.MapWidth > 0)
            {
                cameraPosition.X = MathHelper.Clamp(
                    cameraPosition.X,
                    0,
                    Math.Max(0, map.PixelWidth - screenSize.X));
            }

            if (map.MapHeight > 0)
            {
                cameraPosition.Y = MathHelper.Clamp(
                    cameraPosition.Y,
                    0,
                    Math.Max(0, map.PixelHeight - screenSize.Y));
            }
        }

        /// <summary>
        /// Draws the level.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw map
            map.Draw(spriteBatch, cameraPosition, screenManager.BaseScreenSize);

            // Draw player (offset by camera)
            Vector2 playerScreenPos = player.Position - cameraPosition;

            // We need to draw player at screen position, but the player draws at world position
            // So we create a transform and use it
            spriteBatch.End();

            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0)
                                   * screenManager.GlobalTransformation;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cameraTransform);
            player.Draw(spriteBatch);
            spriteBatch.End();

            // Restart the regular sprite batch for any HUD drawing
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null,
                screenManager.GlobalTransformation);
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
