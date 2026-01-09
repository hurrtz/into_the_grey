using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Strays.Core.Inputs;

/// <summary>
/// Game actions that can be mapped to input.
/// </summary>
public enum GameAction
{
    // Menu Navigation
    MenuUp,
    MenuDown,
    MenuLeft,
    MenuRight,
    MenuSelect,
    MenuCancel,
    MenuPause,

    // Movement
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Sprint,

    // Combat
    Attack,
    Defend,
    UseAbility,
    SwitchTarget,
    Flee,

    // UI
    OpenInventory,
    OpenMap,
    OpenQuests,
    OpenStrays,
    OpenSettings,

    // Quick Actions
    QuickItem1,
    QuickItem2,
    QuickItem3,
    QuickItem4,

    // Camera/View
    CameraZoomIn,
    CameraZoomOut,
    CameraReset,

    // Misc
    Interact,
    ToggleRun,
    QuickSave,
    QuickLoad
}

/// <summary>
/// Represents a button binding.
/// </summary>
public class InputBinding
{
    /// <summary>
    /// Primary keyboard key.
    /// </summary>
    public Keys PrimaryKey { get; set; } = Keys.None;

    /// <summary>
    /// Secondary keyboard key.
    /// </summary>
    public Keys SecondaryKey { get; set; } = Keys.None;

    /// <summary>
    /// Gamepad button.
    /// </summary>
    public Buttons GamepadButton { get; set; } = 0;

    /// <summary>
    /// Whether this uses the left stick axis.
    /// </summary>
    public bool UsesLeftStick { get; set; } = false;

    /// <summary>
    /// Whether this uses the right stick axis.
    /// </summary>
    public bool UsesRightStick { get; set; } = false;

    /// <summary>
    /// Stick direction (-1 to 1 for each axis).
    /// </summary>
    public Vector2 StickDirection { get; set; } = Vector2.Zero;

    /// <summary>
    /// Whether this uses a trigger.
    /// </summary>
    public bool UsesLeftTrigger { get; set; } = false;

    /// <summary>
    /// Whether this uses a trigger.
    /// </summary>
    public bool UsesRightTrigger { get; set; } = false;
}

/// <summary>
/// Manages gamepad input with customizable bindings and vibration.
/// </summary>
public class GamepadManager
{
    private static readonly Dictionary<GameAction, InputBinding> _defaultBindings = new();
    private readonly Dictionary<GameAction, InputBinding> _bindings = new();
    private readonly Dictionary<PlayerIndex, VibrationState> _vibrationStates = new();

    private const float STICK_DEADZONE = 0.25f;
    private const float TRIGGER_DEADZONE = 0.15f;

    /// <summary>
    /// Whether gamepad vibration is enabled.
    /// </summary>
    public bool VibrationEnabled { get; set; } = true;

    /// <summary>
    /// Vibration intensity multiplier (0-1).
    /// </summary>
    public float VibrationIntensity { get; set; } = 1.0f;

    /// <summary>
    /// Event fired when a gamepad is connected.
    /// </summary>
    public event EventHandler<PlayerIndex>? GamepadConnected;

    /// <summary>
    /// Event fired when a gamepad is disconnected.
    /// </summary>
    public event EventHandler<PlayerIndex>? GamepadDisconnected;

    private readonly bool[] _wasConnected = new bool[4];

    static GamepadManager()
    {
        SetupDefaultBindings();
    }

    public GamepadManager()
    {
        // Copy default bindings
        foreach (var kvp in _defaultBindings)
        {
            _bindings[kvp.Key] = new InputBinding
            {
                PrimaryKey = kvp.Value.PrimaryKey,
                SecondaryKey = kvp.Value.SecondaryKey,
                GamepadButton = kvp.Value.GamepadButton,
                UsesLeftStick = kvp.Value.UsesLeftStick,
                UsesRightStick = kvp.Value.UsesRightStick,
                StickDirection = kvp.Value.StickDirection,
                UsesLeftTrigger = kvp.Value.UsesLeftTrigger,
                UsesRightTrigger = kvp.Value.UsesRightTrigger
            };
        }

        // Initialize vibration states
        for (int i = 0; i < 4; i++)
        {
            _vibrationStates[(PlayerIndex)i] = new VibrationState();
        }
    }

    private static void SetupDefaultBindings()
    {
        // Menu Navigation
        _defaultBindings[GameAction.MenuUp] = new InputBinding { PrimaryKey = Keys.Up, SecondaryKey = Keys.W, GamepadButton = Buttons.DPadUp, UsesLeftStick = true, StickDirection = new Vector2(0, 1) };
        _defaultBindings[GameAction.MenuDown] = new InputBinding { PrimaryKey = Keys.Down, SecondaryKey = Keys.S, GamepadButton = Buttons.DPadDown, UsesLeftStick = true, StickDirection = new Vector2(0, -1) };
        _defaultBindings[GameAction.MenuLeft] = new InputBinding { PrimaryKey = Keys.Left, SecondaryKey = Keys.A, GamepadButton = Buttons.DPadLeft, UsesLeftStick = true, StickDirection = new Vector2(-1, 0) };
        _defaultBindings[GameAction.MenuRight] = new InputBinding { PrimaryKey = Keys.Right, SecondaryKey = Keys.D, GamepadButton = Buttons.DPadRight, UsesLeftStick = true, StickDirection = new Vector2(1, 0) };
        _defaultBindings[GameAction.MenuSelect] = new InputBinding { PrimaryKey = Keys.Enter, SecondaryKey = Keys.Space, GamepadButton = Buttons.A };
        _defaultBindings[GameAction.MenuCancel] = new InputBinding { PrimaryKey = Keys.Escape, SecondaryKey = Keys.Back, GamepadButton = Buttons.B };
        _defaultBindings[GameAction.MenuPause] = new InputBinding { PrimaryKey = Keys.Escape, GamepadButton = Buttons.Start };

        // Movement
        _defaultBindings[GameAction.MoveUp] = new InputBinding { PrimaryKey = Keys.W, SecondaryKey = Keys.Up, UsesLeftStick = true, StickDirection = new Vector2(0, 1) };
        _defaultBindings[GameAction.MoveDown] = new InputBinding { PrimaryKey = Keys.S, SecondaryKey = Keys.Down, UsesLeftStick = true, StickDirection = new Vector2(0, -1) };
        _defaultBindings[GameAction.MoveLeft] = new InputBinding { PrimaryKey = Keys.A, SecondaryKey = Keys.Left, UsesLeftStick = true, StickDirection = new Vector2(-1, 0) };
        _defaultBindings[GameAction.MoveRight] = new InputBinding { PrimaryKey = Keys.D, SecondaryKey = Keys.Right, UsesLeftStick = true, StickDirection = new Vector2(1, 0) };
        _defaultBindings[GameAction.Sprint] = new InputBinding { PrimaryKey = Keys.LeftShift, GamepadButton = Buttons.LeftStick };

        // Combat
        _defaultBindings[GameAction.Attack] = new InputBinding { PrimaryKey = Keys.Space, SecondaryKey = Keys.Z, GamepadButton = Buttons.A };
        _defaultBindings[GameAction.Defend] = new InputBinding { PrimaryKey = Keys.LeftControl, SecondaryKey = Keys.X, GamepadButton = Buttons.B };
        _defaultBindings[GameAction.UseAbility] = new InputBinding { PrimaryKey = Keys.E, SecondaryKey = Keys.C, GamepadButton = Buttons.X };
        _defaultBindings[GameAction.SwitchTarget] = new InputBinding { PrimaryKey = Keys.Tab, GamepadButton = Buttons.RightShoulder };
        _defaultBindings[GameAction.Flee] = new InputBinding { PrimaryKey = Keys.R, GamepadButton = Buttons.Back };

        // UI
        _defaultBindings[GameAction.OpenInventory] = new InputBinding { PrimaryKey = Keys.I, GamepadButton = Buttons.Y };
        _defaultBindings[GameAction.OpenMap] = new InputBinding { PrimaryKey = Keys.M, GamepadButton = Buttons.Back };
        _defaultBindings[GameAction.OpenQuests] = new InputBinding { PrimaryKey = Keys.J, GamepadButton = Buttons.LeftShoulder };
        _defaultBindings[GameAction.OpenStrays] = new InputBinding { PrimaryKey = Keys.P, GamepadButton = Buttons.RightShoulder };
        _defaultBindings[GameAction.OpenSettings] = new InputBinding { PrimaryKey = Keys.Escape, GamepadButton = Buttons.Start };

        // Quick Actions
        _defaultBindings[GameAction.QuickItem1] = new InputBinding { PrimaryKey = Keys.D1, GamepadButton = Buttons.DPadUp };
        _defaultBindings[GameAction.QuickItem2] = new InputBinding { PrimaryKey = Keys.D2, GamepadButton = Buttons.DPadRight };
        _defaultBindings[GameAction.QuickItem3] = new InputBinding { PrimaryKey = Keys.D3, GamepadButton = Buttons.DPadDown };
        _defaultBindings[GameAction.QuickItem4] = new InputBinding { PrimaryKey = Keys.D4, GamepadButton = Buttons.DPadLeft };

        // Camera
        _defaultBindings[GameAction.CameraZoomIn] = new InputBinding { PrimaryKey = Keys.OemPlus, UsesRightTrigger = true };
        _defaultBindings[GameAction.CameraZoomOut] = new InputBinding { PrimaryKey = Keys.OemMinus, UsesLeftTrigger = true };
        _defaultBindings[GameAction.CameraReset] = new InputBinding { PrimaryKey = Keys.Home, GamepadButton = Buttons.RightStick };

        // Misc
        _defaultBindings[GameAction.Interact] = new InputBinding { PrimaryKey = Keys.E, SecondaryKey = Keys.Enter, GamepadButton = Buttons.A };
        _defaultBindings[GameAction.ToggleRun] = new InputBinding { PrimaryKey = Keys.CapsLock, GamepadButton = Buttons.LeftStick };
        _defaultBindings[GameAction.QuickSave] = new InputBinding { PrimaryKey = Keys.F5 };
        _defaultBindings[GameAction.QuickLoad] = new InputBinding { PrimaryKey = Keys.F9 };
    }

    /// <summary>
    /// Updates gamepad connection states and vibration.
    /// </summary>
    public void Update(GameTime gameTime, GamePadState[] currentStates)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = 0; i < 4; i++)
        {
            var playerIndex = (PlayerIndex)i;
            bool isConnected = currentStates[i].IsConnected;

            // Check for connection changes
            if (isConnected && !_wasConnected[i])
            {
                GamepadConnected?.Invoke(this, playerIndex);
            }
            else if (!isConnected && _wasConnected[i])
            {
                GamepadDisconnected?.Invoke(this, playerIndex);
            }

            _wasConnected[i] = isConnected;

            // Update vibration
            if (isConnected && _vibrationStates.TryGetValue(playerIndex, out var vibration))
            {
                vibration.Update(deltaTime);

                if (VibrationEnabled && vibration.IsActive)
                {
                    float intensity = vibration.CurrentIntensity * VibrationIntensity;
                    GamePad.SetVibration(playerIndex, vibration.LeftMotor * intensity, vibration.RightMotor * intensity);
                }
                else
                {
                    GamePad.SetVibration(playerIndex, 0f, 0f);
                }
            }
        }
    }

    /// <summary>
    /// Checks if an action was just pressed.
    /// </summary>
    public bool IsActionPressed(GameAction action, InputState input, PlayerIndex playerIndex = PlayerIndex.One)
    {
        if (!_bindings.TryGetValue(action, out var binding))
        {
            return false;
        }

        int idx = (int)playerIndex;

        // Check keyboard
        if (binding.PrimaryKey != Keys.None)
        {
            if (input.CurrentKeyboardStates[idx].IsKeyDown(binding.PrimaryKey) &&
                input.LastKeyboardStates[idx].IsKeyUp(binding.PrimaryKey))
            {
                return true;
            }
        }

        if (binding.SecondaryKey != Keys.None)
        {
            if (input.CurrentKeyboardStates[idx].IsKeyDown(binding.SecondaryKey) &&
                input.LastKeyboardStates[idx].IsKeyUp(binding.SecondaryKey))
            {
                return true;
            }
        }

        // Check gamepad button
        if (binding.GamepadButton != 0)
        {
            if (input.CurrentGamePadStates[idx].IsButtonDown(binding.GamepadButton) &&
                input.LastGamePadStates[idx].IsButtonUp(binding.GamepadButton))
            {
                return true;
            }
        }

        // Check stick direction (with threshold)
        if (binding.UsesLeftStick && binding.StickDirection != Vector2.Zero)
        {
            var currentStick = input.CurrentGamePadStates[idx].ThumbSticks.Left;
            var lastStick = input.LastGamePadStates[idx].ThumbSticks.Left;

            bool wasInDirection = IsStickInDirection(lastStick, binding.StickDirection);
            bool isInDirection = IsStickInDirection(currentStick, binding.StickDirection);

            if (isInDirection && !wasInDirection)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if an action is currently held.
    /// </summary>
    public bool IsActionHeld(GameAction action, InputState input, PlayerIndex playerIndex = PlayerIndex.One)
    {
        if (!_bindings.TryGetValue(action, out var binding))
        {
            return false;
        }

        int idx = (int)playerIndex;

        // Check keyboard
        if (binding.PrimaryKey != Keys.None && input.CurrentKeyboardStates[idx].IsKeyDown(binding.PrimaryKey))
        {
            return true;
        }

        if (binding.SecondaryKey != Keys.None && input.CurrentKeyboardStates[idx].IsKeyDown(binding.SecondaryKey))
        {
            return true;
        }

        // Check gamepad button
        if (binding.GamepadButton != 0 && input.CurrentGamePadStates[idx].IsButtonDown(binding.GamepadButton))
        {
            return true;
        }

        // Check stick direction
        if (binding.UsesLeftStick && binding.StickDirection != Vector2.Zero)
        {
            var stick = input.CurrentGamePadStates[idx].ThumbSticks.Left;

            if (IsStickInDirection(stick, binding.StickDirection))
            {
                return true;
            }
        }

        // Check triggers
        if (binding.UsesLeftTrigger && input.CurrentGamePadStates[idx].Triggers.Left > TRIGGER_DEADZONE)
        {
            return true;
        }

        if (binding.UsesRightTrigger && input.CurrentGamePadStates[idx].Triggers.Right > TRIGGER_DEADZONE)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the movement vector from input.
    /// </summary>
    public Vector2 GetMovementVector(InputState input, PlayerIndex playerIndex = PlayerIndex.One)
    {
        int idx = (int)playerIndex;
        var movement = Vector2.Zero;

        // Keyboard input
        if (input.CurrentKeyboardStates[idx].IsKeyDown(_bindings[GameAction.MoveUp].PrimaryKey) ||
            input.CurrentKeyboardStates[idx].IsKeyDown(_bindings[GameAction.MoveUp].SecondaryKey))
        {
            movement.Y -= 1f;
        }

        if (input.CurrentKeyboardStates[idx].IsKeyDown(_bindings[GameAction.MoveDown].PrimaryKey) ||
            input.CurrentKeyboardStates[idx].IsKeyDown(_bindings[GameAction.MoveDown].SecondaryKey))
        {
            movement.Y += 1f;
        }

        if (input.CurrentKeyboardStates[idx].IsKeyDown(_bindings[GameAction.MoveLeft].PrimaryKey) ||
            input.CurrentKeyboardStates[idx].IsKeyDown(_bindings[GameAction.MoveLeft].SecondaryKey))
        {
            movement.X -= 1f;
        }

        if (input.CurrentKeyboardStates[idx].IsKeyDown(_bindings[GameAction.MoveRight].PrimaryKey) ||
            input.CurrentKeyboardStates[idx].IsKeyDown(_bindings[GameAction.MoveRight].SecondaryKey))
        {
            movement.X += 1f;
        }

        // Gamepad stick input (takes priority if non-zero)
        var stick = input.CurrentGamePadStates[idx].ThumbSticks.Left;

        if (Math.Abs(stick.X) > STICK_DEADZONE || Math.Abs(stick.Y) > STICK_DEADZONE)
        {
            movement.X = stick.X;
            movement.Y = -stick.Y; // Invert Y for screen coordinates
        }

        // Normalize if needed
        if (movement.Length() > 1f)
        {
            movement.Normalize();
        }

        return movement;
    }

    /// <summary>
    /// Gets the right stick look vector.
    /// </summary>
    public Vector2 GetLookVector(InputState input, PlayerIndex playerIndex = PlayerIndex.One)
    {
        int idx = (int)playerIndex;
        var stick = input.CurrentGamePadStates[idx].ThumbSticks.Right;

        if (Math.Abs(stick.X) < STICK_DEADZONE && Math.Abs(stick.Y) < STICK_DEADZONE)
        {
            return Vector2.Zero;
        }

        return new Vector2(stick.X, -stick.Y);
    }

    /// <summary>
    /// Gets the trigger value (0-1).
    /// </summary>
    public float GetTriggerValue(bool leftTrigger, InputState input, PlayerIndex playerIndex = PlayerIndex.One)
    {
        int idx = (int)playerIndex;
        float value = leftTrigger
            ? input.CurrentGamePadStates[idx].Triggers.Left
            : input.CurrentGamePadStates[idx].Triggers.Right;

        return value > TRIGGER_DEADZONE ? value : 0f;
    }

    private bool IsStickInDirection(Vector2 stick, Vector2 direction)
    {
        if (stick.Length() < STICK_DEADZONE)
        {
            return false;
        }

        float dot = Vector2.Dot(Vector2.Normalize(stick), Vector2.Normalize(direction));

        return dot > 0.7f;
    }

    /// <summary>
    /// Sets a custom binding for an action.
    /// </summary>
    public void SetBinding(GameAction action, InputBinding binding)
    {
        _bindings[action] = binding;
    }

    /// <summary>
    /// Gets the current binding for an action.
    /// </summary>
    public InputBinding? GetBinding(GameAction action)
    {
        return _bindings.TryGetValue(action, out var binding) ? binding : null;
    }

    /// <summary>
    /// Resets a binding to default.
    /// </summary>
    public void ResetBinding(GameAction action)
    {
        if (_defaultBindings.TryGetValue(action, out var defaultBinding))
        {
            _bindings[action] = new InputBinding
            {
                PrimaryKey = defaultBinding.PrimaryKey,
                SecondaryKey = defaultBinding.SecondaryKey,
                GamepadButton = defaultBinding.GamepadButton,
                UsesLeftStick = defaultBinding.UsesLeftStick,
                UsesRightStick = defaultBinding.UsesRightStick,
                StickDirection = defaultBinding.StickDirection,
                UsesLeftTrigger = defaultBinding.UsesLeftTrigger,
                UsesRightTrigger = defaultBinding.UsesRightTrigger
            };
        }
    }

    /// <summary>
    /// Resets all bindings to defaults.
    /// </summary>
    public void ResetAllBindings()
    {
        foreach (var action in Enum.GetValues<GameAction>())
        {
            ResetBinding(action);
        }
    }

    /// <summary>
    /// Exports bindings for saving.
    /// </summary>
    public Dictionary<string, InputBinding> ExportBindings()
    {
        var export = new Dictionary<string, InputBinding>();

        foreach (var kvp in _bindings)
        {
            export[kvp.Key.ToString()] = kvp.Value;
        }

        return export;
    }

    /// <summary>
    /// Imports bindings from save data.
    /// </summary>
    public void ImportBindings(Dictionary<string, InputBinding>? bindings)
    {
        if (bindings == null)
        {
            return;
        }

        foreach (var kvp in bindings)
        {
            if (Enum.TryParse<GameAction>(kvp.Key, out var action))
            {
                _bindings[action] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Triggers controller vibration.
    /// </summary>
    public void Vibrate(PlayerIndex playerIndex, float leftMotor, float rightMotor, float duration)
    {
        if (_vibrationStates.TryGetValue(playerIndex, out var state))
        {
            state.Start(leftMotor, rightMotor, duration);
        }
    }

    /// <summary>
    /// Stops vibration immediately.
    /// </summary>
    public void StopVibration(PlayerIndex playerIndex)
    {
        if (_vibrationStates.TryGetValue(playerIndex, out var state))
        {
            state.Stop();
        }

        GamePad.SetVibration(playerIndex, 0f, 0f);
    }

    /// <summary>
    /// Preset vibration for light impact.
    /// </summary>
    public void VibrateLight(PlayerIndex playerIndex = PlayerIndex.One)
    {
        Vibrate(playerIndex, 0.2f, 0.2f, 0.1f);
    }

    /// <summary>
    /// Preset vibration for medium impact.
    /// </summary>
    public void VibrateMedium(PlayerIndex playerIndex = PlayerIndex.One)
    {
        Vibrate(playerIndex, 0.5f, 0.5f, 0.2f);
    }

    /// <summary>
    /// Preset vibration for heavy impact.
    /// </summary>
    public void VibrateHeavy(PlayerIndex playerIndex = PlayerIndex.One)
    {
        Vibrate(playerIndex, 1.0f, 1.0f, 0.4f);
    }

    /// <summary>
    /// Preset vibration for damage taken.
    /// </summary>
    public void VibrateDamage(float damagePercent, PlayerIndex playerIndex = PlayerIndex.One)
    {
        float intensity = MathHelper.Clamp(damagePercent, 0.1f, 1.0f);
        Vibrate(playerIndex, intensity, intensity * 0.5f, 0.15f + intensity * 0.2f);
    }

    /// <summary>
    /// Checks if a gamepad is connected.
    /// </summary>
    public bool IsGamepadConnected(PlayerIndex playerIndex = PlayerIndex.One)
    {
        return _wasConnected[(int)playerIndex];
    }

    /// <summary>
    /// Gets all connected gamepads.
    /// </summary>
    public IEnumerable<PlayerIndex> GetConnectedGamepads()
    {
        for (int i = 0; i < 4; i++)
        {
            if (_wasConnected[i])
            {
                yield return (PlayerIndex)i;
            }
        }
    }
}

/// <summary>
/// Tracks vibration state for a single controller.
/// </summary>
internal class VibrationState
{
    public float LeftMotor { get; private set; }
    public float RightMotor { get; private set; }
    public float Duration { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool IsActive => Duration > 0 && ElapsedTime < Duration;
    public float CurrentIntensity => IsActive ? 1f - (ElapsedTime / Duration) : 0f;

    public void Start(float left, float right, float duration)
    {
        LeftMotor = left;
        RightMotor = right;
        Duration = duration;
        ElapsedTime = 0f;
    }

    public void Stop()
    {
        Duration = 0f;
        ElapsedTime = 0f;
    }

    public void Update(float deltaTime)
    {
        if (IsActive)
        {
            ElapsedTime += deltaTime;
        }
    }
}

