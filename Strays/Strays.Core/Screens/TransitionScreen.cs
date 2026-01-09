using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Inputs;
using Strays.ScreenManagers;

namespace Strays.Screens;

/// <summary>
/// Type of transition effect.
/// </summary>
public enum TransitionType
{
    /// <summary>Fade to black and back.</summary>
    FadeBlack,

    /// <summary>Fade to white and back.</summary>
    FadeWhite,

    /// <summary>Circular iris/wipe effect.</summary>
    Iris
}

/// <summary>
/// A screen that handles visual transitions between game states.
/// Used for building entry/exit, biome changes, etc.
/// </summary>
public class TransitionScreen : GameScreen
{
    private readonly TransitionType _type;
    private readonly string? _text;
    private readonly Action? _onMidpoint;
    private readonly Action? _onComplete;
    private readonly float _duration;

    private float _timer;
    private bool _midpointTriggered;
    private SpriteFont? _font;
    private Texture2D? _pixelTexture;

    /// <summary>
    /// Creates a new transition screen.
    /// </summary>
    /// <param name="type">Type of visual transition.</param>
    /// <param name="text">Optional text to display during transition.</param>
    /// <param name="duration">Total duration of the transition in seconds.</param>
    /// <param name="onMidpoint">Action to perform at the midpoint (full fade).</param>
    /// <param name="onComplete">Action to perform when transition completes.</param>
    public TransitionScreen(
        TransitionType type = TransitionType.FadeBlack,
        string? text = null,
        float duration = 1f,
        Action? onMidpoint = null,
        Action? onComplete = null)
    {
        _type = type;
        _text = text;
        _duration = duration;
        _onMidpoint = onMidpoint;
        _onComplete = onComplete;
        _timer = 0f;
        _midpointTriggered = false;

        // This screen overlays everything
        IsPopup = true;
        TransitionOnTime = TimeSpan.Zero;
        TransitionOffTime = TimeSpan.Zero;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _font = ScreenManager.Font;
        _pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _pixelTexture?.Dispose();
    }

    public override void HandleInput(GameTime gameTime, InputState input)
    {
        // Block all input during transition
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timer += deltaTime;

        // Trigger midpoint action when we reach the middle of the transition
        float midpoint = _duration / 2;
        if (!_midpointTriggered && _timer >= midpoint)
        {
            _midpointTriggered = true;
            _onMidpoint?.Invoke();
        }

        // Complete transition
        if (_timer >= _duration)
        {
            _onComplete?.Invoke();
            ExitScreen();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_pixelTexture == null) return;

        var spriteBatch = ScreenManager.SpriteBatch;
        var viewport = ScreenManager.GraphicsDevice.Viewport;

        // Calculate alpha based on timer position
        float midpoint = _duration / 2;
        float alpha;

        if (_timer < midpoint)
        {
            // Fading in (to black)
            alpha = _timer / midpoint;
        }
        else
        {
            // Fading out (from black)
            alpha = 1f - ((_timer - midpoint) / midpoint);
        }

        alpha = MathHelper.Clamp(alpha, 0f, 1f);

        // Get transition color based on type
        Color transitionColor = _type switch
        {
            TransitionType.FadeWhite => Color.White,
            TransitionType.Iris => Color.Black,
            _ => Color.Black
        };

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            ScreenManager.GlobalTransformation
        );

        // Draw based on transition type
        switch (_type)
        {
            case TransitionType.Iris:
                DrawIrisTransition(spriteBatch, viewport, alpha, transitionColor);
                break;

            default:
                DrawFadeTransition(spriteBatch, viewport, alpha, transitionColor);
                break;
        }

        spriteBatch.End();
    }

    private void DrawFadeTransition(SpriteBatch spriteBatch, Viewport viewport, float alpha, Color color)
    {
        // Draw full screen overlay
        var screenRect = new Rectangle(0, 0, viewport.Width, viewport.Height);
        spriteBatch.Draw(_pixelTexture, screenRect, color * alpha);

        // Draw text if present and at high alpha
        if (!string.IsNullOrEmpty(_text) && alpha > 0.5f && _font != null)
        {
            float textAlpha = (alpha - 0.5f) * 2f; // Fade text in faster
            var textSize = _font.MeasureString(_text);
            var textPos = new Vector2(
                (viewport.Width - textSize.X) / 2,
                (viewport.Height - textSize.Y) / 2
            );

            Color textColor = _type == TransitionType.FadeWhite ? Color.Black : Color.White;
            spriteBatch.DrawString(_font, _text, textPos, textColor * textAlpha);
        }
    }

    private void DrawIrisTransition(SpriteBatch spriteBatch, Viewport viewport, float alpha, Color color)
    {
        // For iris effect, we draw a black screen with a circular hole
        // As alpha increases, the hole shrinks
        // This is a simplified approximation using rectangles

        var screenRect = new Rectangle(0, 0, viewport.Width, viewport.Height);
        var centerX = viewport.Width / 2;
        var centerY = viewport.Height / 2;

        // Calculate iris radius (0 = closed, max = open)
        float maxRadius = MathF.Sqrt(centerX * centerX + centerY * centerY);
        float radius = maxRadius * (1f - alpha);

        // Draw full black for the outer area
        // Since we can't easily draw a circle hole with SpriteBatch,
        // we approximate with the fade effect for now
        spriteBatch.Draw(_pixelTexture, screenRect, color * alpha);

        // Draw text if present
        if (!string.IsNullOrEmpty(_text) && alpha > 0.7f && _font != null)
        {
            float textAlpha = (alpha - 0.7f) / 0.3f;
            var textSize = _font.MeasureString(_text);
            var textPos = new Vector2(
                (viewport.Width - textSize.X) / 2,
                (viewport.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(_font, _text, textPos, Color.White * textAlpha);
        }
    }

    /// <summary>
    /// Creates and shows a building entry transition.
    /// </summary>
    public static TransitionScreen ShowBuildingEntry(
        ScreenManager screenManager,
        string buildingName,
        Action onEnter,
        PlayerIndex? controllingPlayer = null)
    {
        var transition = new TransitionScreen(
            TransitionType.FadeBlack,
            $"Entering {buildingName}...",
            1f,
            onEnter,
            null
        );

        screenManager.AddScreen(transition, controllingPlayer);
        return transition;
    }

    /// <summary>
    /// Creates and shows a building exit transition.
    /// </summary>
    public static TransitionScreen ShowBuildingExit(
        ScreenManager screenManager,
        Action onExit,
        PlayerIndex? controllingPlayer = null)
    {
        var transition = new TransitionScreen(
            TransitionType.FadeBlack,
            null,
            0.8f,
            onExit,
            null
        );

        screenManager.AddScreen(transition, controllingPlayer);
        return transition;
    }

    /// <summary>
    /// Creates and shows a biome transition.
    /// </summary>
    public static TransitionScreen ShowBiomeTransition(
        ScreenManager screenManager,
        string biomeName,
        Action onTransition,
        PlayerIndex? controllingPlayer = null)
    {
        var transition = new TransitionScreen(
            TransitionType.FadeBlack,
            $"Entering {biomeName}",
            1.2f,
            onTransition,
            null
        );

        screenManager.AddScreen(transition, controllingPlayer);
        return transition;
    }
}
