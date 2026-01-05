# Strays Architecture Documentation

## System Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         StraysGame                                   │
│  (Main game class - initialization, services, game loop)            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │
│  │ScreenManager│  │SettingsMgr  │  │ParticleMgr  │  │LocalizeMgr  │ │
│  │             │  │             │  │             │  │             │ │
│  │ Manages UI  │  │ Persists    │  │ Visual FX   │  │ i18n        │ │
│  │ screen stack│  │ settings    │  │ system      │  │ support     │ │
│  └──────┬──────┘  └─────────────┘  └─────────────┘  └─────────────┘ │
│         │                                                            │
│  ┌──────▼──────────────────────────────────────────────────────────┐│
│  │                      GameScreen (base)                          ││
│  │  LoadContent() → Update() → HandleInput() → Draw() → Unload()   ││
│  └─────────────────────────────────────────────────────────────────┘│
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

## Initialization Flow

```
Program.Main()
    │
    ▼
StraysGame()
    ├── Platform detection (Mobile/Desktop)
    ├── GraphicsDeviceManager creation
    ├── SettingsManager creation
    ├── LeaderboardManager creation
    └── ScreenManager creation
    │
    ▼
StraysGame.Initialize()
    ├── LocalizationManager setup
    └── Add initial screen (TopDownGameplayScreen)
    │
    ▼
StraysGame.LoadContent()
    ├── Load particle texture
    └── Create ParticleManager
    │
    ▼
Game.Run() → MonoGame Game Loop
    │
    ▼
┌───────────────────────────────┐
│         GAME LOOP             │
│  ┌─────────────────────────┐  │
│  │ InputState.Update()     │  │
│  │ - Poll all input devices│  │
│  └───────────┬─────────────┘  │
│              ▼                │
│  ┌─────────────────────────┐  │
│  │ ScreenManager.Update()  │  │
│  │ - Update active screens │  │
│  │ - Route input to screens│  │
│  └───────────┬─────────────┘  │
│              ▼                │
│  ┌─────────────────────────┐  │
│  │ ScreenManager.Draw()    │  │
│  │ - Render all screens    │  │
│  └─────────────────────────┘  │
└───────────────────────────────┘
```

## Screen System Architecture

### Screen Lifecycle

```
                    ┌──────────────┐
                    │  AddScreen() │
                    └───────┬──────┘
                            ▼
                    ┌──────────────┐
          ┌────────│ TransitionOn │ (Fading in)
          │        └───────┬──────┘
          │                ▼
          │        ┌──────────────┐
          │        │    Active    │ (Receiving input)
          │        └───────┬──────┘
          │                ▼
          │        ┌──────────────┐
          │        │TransitionOff │ (Fading out)
          │        └───────┬──────┘
          │                ▼
          │        ┌──────────────┐
          └───────▶│    Hidden    │
                   └───────┬──────┘
                           ▼
                   ┌──────────────┐
                   │RemoveScreen()│
                   └──────────────┘
```

### Screen Stack

```
┌─────────────────────────────────────┐
│          ScreenManager              │
│                                     │
│  Screen Stack (bottom to top):      │
│  ┌─────────────────────────────┐    │
│  │ BackgroundScreen            │ ←── Background layer
│  ├─────────────────────────────┤    │
│  │ GameplayScreen              │ ←── Main gameplay
│  │ OR TopDownGameplayScreen    │    │
│  ├─────────────────────────────┤    │
│  │ PauseScreen (popup)         │ ←── Overlay (optional)
│  └─────────────────────────────┘    │
│                                     │
│  Only topmost non-covered screen    │
│  receives input                     │
└─────────────────────────────────────┘
```

### Screen Inheritance Hierarchy

```
GameScreen (abstract base)
├── MenuScreen (abstract menu base)
│   ├── MainMenuScreen
│   ├── PauseScreen
│   ├── SettingsScreen
│   └── AboutScreen
├── GameplayScreen (classic platformer)
├── TopDownGameplayScreen (new top-down mode)
├── BackgroundScreen
├── LoadingScreen
└── MessageBoxScreen
```

## Input System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        InputState                                │
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐           │
│  │   Keyboard   │  │   GamePad    │  │    Mouse     │           │
│  │   [4 slots]  │  │   [4 slots]  │  │              │           │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘           │
│         │                 │                 │                    │
│         └────────────┬────┴────────────┬────┘                    │
│                      ▼                 ▼                         │
│              ┌─────────────────────────────────┐                 │
│              │ CurrentCursorLocation           │                 │
│              │ (unified position)              │                 │
│              └─────────────────────────────────┘                 │
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐                             │
│  │    Touch     │  │   Gestures   │                             │
│  │              │  │   (taps)     │                             │
│  └──────────────┘  └──────────────┘                             │
│                                                                  │
│  State Tracking:                                                 │
│  - Previous frame state                                         │
│  - Current frame state                                          │
│  - Delta detection (IsNewKeyPress, etc.)                        │
└─────────────────────────────────────────────────────────────────┘
```

## Settings System Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                   SettingsManager<T>                              │
│                                                                   │
│  ┌─────────────────┐                                             │
│  │ ISettingsStorage│ ◄──────── Interface                         │
│  └────────┬────────┘                                             │
│           │                                                       │
│           ├──────────────────────────────────────────────────┐   │
│           ▼                                                   ▼   │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐│
│  │DesktopSettings   │  │MobileSettings    │  │ConsoleSettings   ││
│  │Storage           │  │Storage           │  │Storage           ││
│  │(file-based)      │  │(platform APIs)   │  │(console-specific)││
│  └──────────────────┘  └──────────────────┘  └──────────────────┘│
│                                                                   │
│  Data Classes:                                                    │
│  ┌──────────────────┐  ┌──────────────────┐                      │
│  │ StraysSettings   │  │ StraysLeaderboard│                      │
│  │ - MusicVolume    │  │ - HighScores[]   │                      │
│  │ - SfxVolume      │  │ - per level      │                      │
│  │ - Language       │  │                  │                      │
│  └──────────────────┘  └──────────────────┘                      │
└──────────────────────────────────────────────────────────────────┘
```

## Particle System Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                      ParticleManager                              │
│                                                                   │
│  Properties:                                                      │
│  - Position (emission origin)                                    │
│  - ParticleCount                                                 │
│  - Finished (all particles dead)                                 │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    Particle Pool                             │ │
│  │  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐               │ │
│  │  │ P1 │ │ P2 │ │ P3 │ │ P4 │ │... │ │ Pn │               │ │
│  │  └────┘ └────┘ └────┘ └────┘ └────┘ └────┘               │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
│  Effect Types (ParticleEffectType enum):                         │
│  ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐        │
│  │ Confetti  │ │ Explosion │ │ Fireworks │ │ Sparkles  │        │
│  │ (random   │ │ (radial,  │ │ (cascade  │ │ (light    │        │
│  │  colors)  │ │  red/org) │ │  effect)  │ │  white)   │        │
│  └───────────┘ └───────────┘ └───────────┘ └───────────┘        │
│                                                                   │
│  Emit() → Creates particles with:                                │
│  - Initial velocity                                              │
│  - Color                                                         │
│  - Lifetime                                                      │
│  - Tailing (particle trails)                                     │
└──────────────────────────────────────────────────────────────────┘
```

## Game Mode Architectures

### Top-Down Mode

```
┌──────────────────────────────────────────────────────────────────┐
│                   TopDownGameplayScreen                           │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │                      TopDownLevel                          │  │
│  │                                                            │  │
│  │  ┌────────────────┐     ┌────────────────────────────┐    │  │
│  │  │   TiledMap     │     │      TopDownPlayer         │    │  │
│  │  │                │     │                            │    │  │
│  │  │ - Layers[]     │     │ - Position                 │    │  │
│  │  │ - Tilesets[]   │     │ - FacingDirection          │    │  │
│  │  │ - Draw()       │     │ - Velocity                 │    │  │
│  │  │ - IsBlocked()  │     │ - WalkSpeed/RunSpeed       │    │  │
│  │  └────────────────┘     │                            │    │  │
│  │                         │ ┌────────────────────────┐ │    │  │
│  │                         │ │DirectionalAnimPlayer   │ │    │  │
│  │                         │ │ - idle animation       │ │    │  │
│  │                         │ │ - walk animation       │ │    │  │
│  │                         │ │ - run animation        │ │    │  │
│  │                         │ └────────────────────────┘ │    │  │
│  │                         └────────────────────────────┘    │  │
│  │                                                            │  │
│  │  Camera System:                                            │  │
│  │  - Follows player position                                │  │
│  │  - Viewport culling for rendering                         │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### Classic Platformer Mode

```
┌──────────────────────────────────────────────────────────────────┐
│                      GameplayScreen                               │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │                         Level                              │  │
│  │                                                            │  │
│  │  ┌─────────────────┐  ┌─────────────────┐                 │  │
│  │  │    Tile[,]      │  │     Player      │                 │  │
│  │  │    grid         │  │                 │                 │  │
│  │  │                 │  │ - Position      │                 │  │
│  │  │ Platform tiles  │  │ - Velocity      │                 │  │
│  │  │ Empty spaces    │  │ - IsOnGround    │                 │  │
│  │  │ Exit tile       │  │ - Physics       │                 │  │
│  │  └─────────────────┘  └─────────────────┘                 │  │
│  │                                                            │  │
│  │  ┌─────────────────┐  ┌─────────────────┐                 │  │
│  │  │   List<Gem>     │  │   List<Enemy>   │                 │  │
│  │  │                 │  │                 │                 │  │
│  │  │ Collectibles    │  │ AI patrol       │                 │  │
│  │  │ Point values    │  │ Collision=death │                 │  │
│  │  └─────────────────┘  └─────────────────┘                 │  │
│  │                                                            │  │
│  │  Game State:                                               │  │
│  │  - Score, TimeTaken, GemsCollected                        │  │
│  │  - Level index (00-06)                                    │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

## Animation System

### Classic Animation (2D Sprite Sheet)

```
Animation
├── Texture2D texture (sprite sheet)
├── int frameCount
├── float frameTime
├── bool isLooping
└── GetSourceRectangle(frameIndex) → Rectangle

AnimationPlayer
├── Animation animation
├── int frameIndex
├── float time
├── Update(gameTime)
└── Draw(spriteBatch, position, flip)
```

### Directional Animation (8-Way)

```
DirectionalAnimation
├── Dictionary<Direction, Texture2D[]> frames
│   ├── South → [frame0, frame1, ...]
│   ├── SouthEast → [frame0, frame1, ...]
│   ├── East → [frame0, frame1, ...]
│   ├── NorthEast → [frame0, frame1, ...]
│   ├── North → [frame0, frame1, ...]
│   ├── NorthWest → [frame0, frame1, ...]
│   ├── West → [frame0, frame1, ...]
│   └── SouthWest → [frame0, frame1, ...]
├── float frameTime
├── bool isLooping
└── GetFrame(direction, frameIndex) → Texture2D

DirectionalAnimationPlayer
├── DirectionalAnimation animation
├── int frameIndex
├── float time
├── Direction currentDirection
├── Update(gameTime)
└── Draw(spriteBatch, position)
```

## Service Registration

```
Game.Services (IServiceProvider)
│
├── ScreenManager
│   └── Registered in StraysGame constructor
│
├── SettingsManager<StraysSettings>
│   └── Registered in StraysGame constructor
│
├── LeaderboardManager (SettingsManager<StraysLeaderboard>)
│   └── Registered in StraysGame constructor
│
└── ParticleManager
    └── Registered in StraysGame.LoadContent()

Access pattern:
var screenManager = game.Services.GetService<ScreenManager>();
```

## Platform Abstraction

```
┌─────────────────────────────────────────────────────────────────┐
│                        StraysGame                                │
│                                                                  │
│  Platform Detection:                                             │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  IsMobile = CurrentPlatform == iOS || Android             │  │
│  │  IsDesktop = CurrentPlatform == Windows || Mac || Linux   │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  Platform-Specific Behavior:                                     │
│  ┌─────────────────────┐  ┌─────────────────────┐               │
│  │      Mobile         │  │      Desktop        │               │
│  │                     │  │                     │               │
│  │ - Fullscreen        │  │ - Windowed          │               │
│  │ - Touch input       │  │ - Mouse/keyboard    │               │
│  │ - MobileSettings    │  │ - DesktopSettings   │               │
│  │ - Landscape orient  │  │ - Any resolution    │               │
│  └─────────────────────┘  └─────────────────────┘               │
└─────────────────────────────────────────────────────────────────┘
```
