using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Strays.Core;
using Strays.Core.Inputs;
using Strays.ScreenManagers;

namespace Strays.Screens
{
    /// <summary>
    /// Gameplay screen for top-down exploration with the DefaultMale character.
    /// </summary>
    public class TopDownGameplayScreen : GameScreen
    {
        private SpriteBatch spriteBatch;
        private TopDownLevel level;
        private SpriteFont hudFont;
        private bool isPaused;

        /// <summary>
        /// Creates a new top-down gameplay screen.
        /// </summary>
        public TopDownGameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Loads content for the screen.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            spriteBatch = ScreenManager.SpriteBatch;

            // Get the content path - go up from bin/Debug/net9.0 to project root, then to Content
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // For development: Content is in Strays.Core/Content
            // The binary runs from Strays.DesktopGL/bin/Debug/net9.0/
            string contentPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "Strays.Core", "Content"));

            // Fallback to checking if Content folder exists in base directory (for published builds)
            if (!Directory.Exists(contentPath))
            {
                contentPath = Path.Combine(baseDir, "Content");
            }

            Console.WriteLine($"Base directory: {baseDir}");
            Console.WriteLine($"Content path: {contentPath}");
            Console.WriteLine($"Content exists: {Directory.Exists(contentPath)}");

            // List the Sprites folder to verify DefaultMale is there
            string spritesPath = Path.Combine(contentPath, "Sprites");
            Console.WriteLine($"Sprites path: {spritesPath}");
            Console.WriteLine($"Sprites exists: {Directory.Exists(spritesPath)}");

            string defaultMalePath = Path.Combine(spritesPath, "DefaultMale");
            Console.WriteLine($"DefaultMale path: {defaultMalePath}");
            Console.WriteLine($"DefaultMale exists: {Directory.Exists(defaultMalePath)}");

            // Path to the suburb map - go up to Strays repo root, then to Tiled folder
            string mapPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "Tiled", "biomes", "suburb.tmx"));

            Console.WriteLine($"Map path: {mapPath}");
            Console.WriteLine($"Map exists: {File.Exists(mapPath)}");

            // Create the level
            level = new TopDownLevel(ScreenManager, mapPath, contentPath);

            // Load HUD font
            hudFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/Hud");

            ScreenManager.Game.ResetElapsedTime();
        }

        /// <summary>
        /// Unloads content.
        /// </summary>
        public override void UnloadContent()
        {
            level?.Dispose();
        }

        /// <summary>
        /// Updates the screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (!IsActive)
                return;

            isPaused = coveredByOtherScreen;
        }

        /// <summary>
        /// Handles input.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState inputState)
        {
            base.HandleInput(gameTime, inputState);

            // Check for pause
            if (inputState.IsPauseGame(ControllingPlayer, null))
            {
                ScreenManager.AddScreen(new PauseScreen(), ControllingPlayer);
                return;
            }

            // Check for escape to exit
            if (inputState.CurrentKeyboardStates[0].IsKeyDown(Keys.Escape))
            {
                // Return to main menu
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
                return;
            }

            // Update level with input
            if (!isPaused)
            {
                level.Update(gameTime, inputState);
            }
        }

        /// <summary>
        /// Draws the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Clear to a nice green for the suburb
            ScreenManager.GraphicsDevice.Clear(Color.DarkGreen);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                ScreenManager.GlobalTransformation);

            // Draw level
            level.Draw(gameTime, spriteBatch);

            // Draw HUD
            DrawHud();

            spriteBatch.End();

            // Handle screen transitions
            if (TransitionPosition > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, 0);
                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        /// <summary>
        /// Draws the heads-up display.
        /// </summary>
        private void DrawHud()
        {
            // Draw instructions
            string instructions = "WASD/Arrows: Move | Hold to Run | ESC: Menu";
            Vector2 instructionPos = new Vector2(20, 20);

            // Shadow
            spriteBatch.DrawString(hudFont, instructions, instructionPos + new Vector2(1, 1), Color.Black);
            // Text
            spriteBatch.DrawString(hudFont, instructions, instructionPos, Color.White);

            // Draw player position for debugging
            string posText = $"Position: {level.Player.Position.X:F0}, {level.Player.Position.Y:F0}";
            Vector2 posPos = new Vector2(20, 50);
            spriteBatch.DrawString(hudFont, posText, posPos + new Vector2(1, 1), Color.Black);
            spriteBatch.DrawString(hudFont, posText, posPos, Color.Yellow);

            // Draw direction
            string dirText = $"Facing: {level.Player.FacingDirection}";
            Vector2 dirPos = new Vector2(20, 80);
            spriteBatch.DrawString(hudFont, dirText, dirPos + new Vector2(1, 1), Color.Black);
            spriteBatch.DrawString(hudFont, dirText, dirPos, Color.Yellow);
        }
    }
}
