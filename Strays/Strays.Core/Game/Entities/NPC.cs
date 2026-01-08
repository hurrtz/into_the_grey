using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Strays.Core.Game.Data;
using Strays.Core.Services;

namespace Strays.Core.Game.Entities;

/// <summary>
/// An NPC entity in the game world.
/// </summary>
public class NPC
{
    private readonly NPCDefinition _definition;
    private readonly GameStateService _gameState;

    /// <summary>
    /// The NPC definition.
    /// </summary>
    public NPCDefinition Definition => _definition;

    /// <summary>
    /// Unique ID of this NPC.
    /// </summary>
    public string Id => _definition.Id;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name => _definition.Name;

    /// <summary>
    /// NPC type.
    /// </summary>
    public NPCType Type => _definition.Type;

    /// <summary>
    /// Faction affiliation.
    /// </summary>
    public Faction Faction => _definition.Faction;

    /// <summary>
    /// World position.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Size for rendering and collision.
    /// </summary>
    public Vector2 Size { get; set; } = new Vector2(32, 48);

    /// <summary>
    /// Bounding box for collision detection.
    /// </summary>
    public Rectangle BoundingBox => new Rectangle(
        (int)(Position.X - Size.X / 2),
        (int)(Position.Y - Size.Y / 2),
        (int)Size.X,
        (int)Size.Y
    );

    /// <summary>
    /// Interaction radius.
    /// </summary>
    public float InteractionRadius { get; set; } = 50f;

    /// <summary>
    /// Whether this NPC is currently visible.
    /// </summary>
    public bool IsVisible
    {
        get
        {
            // Check required flag
            if (!string.IsNullOrEmpty(_definition.RequiresFlag) &&
                !_gameState.HasFlag(_definition.RequiresFlag))
            {
                return false;
            }

            // Check hidden flag
            if (!string.IsNullOrEmpty(_definition.HiddenByFlag) &&
                _gameState.HasFlag(_definition.HiddenByFlag))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Whether the player can currently interact with this NPC.
    /// </summary>
    public bool CanInteract { get; set; } = true;

    /// <summary>
    /// Animation timer for idle animation.
    /// </summary>
    private float _animTimer;

    /// <summary>
    /// Whether this NPC is currently being talked to.
    /// </summary>
    public bool InConversation { get; set; }

    /// <summary>
    /// Event fired when the NPC is interacted with.
    /// </summary>
    public event EventHandler? Interacted;

    public NPC(NPCDefinition definition, GameStateService gameState)
    {
        _definition = definition;
        _gameState = gameState;
    }

    /// <summary>
    /// Gets the appropriate dialog ID based on game state.
    /// </summary>
    public string? GetCurrentDialogId()
    {
        // Check conditional dialogs in order
        foreach (var (flag, dialogId) in _definition.ConditionalDialogs)
        {
            if (_gameState.HasFlag(flag))
            {
                return dialogId;
            }
        }

        return _definition.DefaultDialogId;
    }

    /// <summary>
    /// Checks if the player is within interaction range.
    /// </summary>
    public bool IsInRange(Vector2 playerPosition)
    {
        float distance = Vector2.Distance(Position, playerPosition);
        return distance <= InteractionRadius;
    }

    /// <summary>
    /// Triggers interaction with this NPC.
    /// </summary>
    public void Interact()
    {
        if (!CanInteract || !IsVisible)
            return;

        InConversation = true;
        Interacted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates the NPC state.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        _animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>
    /// Draws the NPC.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, Vector2 cameraPosition)
    {
        if (!IsVisible)
            return;

        var screenPos = Position - cameraPosition;

        // Draw placeholder rectangle
        var rect = new Rectangle(
            (int)(screenPos.X - Size.X / 2),
            (int)(screenPos.Y - Size.Y / 2),
            (int)Size.X,
            (int)Size.Y
        );

        // Idle animation - subtle bob
        float bob = (float)Math.Sin(_animTimer * 2) * 2;
        rect.Y += (int)bob;

        // Draw body
        spriteBatch.Draw(pixelTexture, rect, _definition.PlaceholderColor);

        // Draw outline when in conversation
        if (InConversation)
        {
            DrawOutline(spriteBatch, pixelTexture, rect, Color.White);
        }

        // Draw interaction indicator
        if (CanInteract && !InConversation)
        {
            DrawInteractionIndicator(spriteBatch, pixelTexture, font, screenPos);
        }

        // Draw name
        if (font != null)
        {
            var nameSize = font.MeasureString(Name);
            var namePos = new Vector2(
                screenPos.X - nameSize.X / 2,
                rect.Y - nameSize.Y - 5
            );
            spriteBatch.DrawString(font, Name, namePos, Color.White);
        }
    }

    private void DrawOutline(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle rect, Color color)
    {
        int thickness = 2;

        // Top
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X - thickness, rect.Y - thickness, rect.Width + thickness * 2, thickness), color);
        // Bottom
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X - thickness, rect.Bottom, rect.Width + thickness * 2, thickness), color);
        // Left
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X - thickness, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.Right, rect.Y, thickness, rect.Height), color);
    }

    private void DrawInteractionIndicator(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font, Vector2 screenPos)
    {
        // Draw floating "!" or "?" indicator
        float indicatorY = screenPos.Y - Size.Y / 2 - 25;
        float pulse = (float)Math.Sin(_animTimer * 4) * 3;
        indicatorY += pulse;

        string indicator = Type switch
        {
            NPCType.QuestGiver => "!",
            NPCType.Merchant => "$",
            NPCType.Healer => "+",
            NPCType.Crafter => "*",
            _ => "?"
        };

        Color indicatorColor = Type switch
        {
            NPCType.QuestGiver => Color.Yellow,
            NPCType.Merchant => Color.Gold,
            NPCType.Healer => Color.LimeGreen,
            NPCType.Crafter => Color.Cyan,
            _ => Color.White
        };

        if (font != null)
        {
            var textSize = font.MeasureString(indicator);
            var textPos = new Vector2(screenPos.X - textSize.X / 2, indicatorY);
            spriteBatch.DrawString(font, indicator, textPos, indicatorColor);
        }
    }

    /// <summary>
    /// Creates an NPC instance from a definition.
    /// </summary>
    public static NPC? Create(string definitionId, GameStateService gameState)
    {
        var definition = NPCDefinitions.Get(definitionId);
        if (definition == null)
            return null;

        return new NPC(definition, gameState);
    }

    /// <summary>
    /// Creates an NPC instance from a definition.
    /// </summary>
    public static NPC Create(NPCDefinition definition, GameStateService gameState)
    {
        return new NPC(definition, gameState);
    }
}
