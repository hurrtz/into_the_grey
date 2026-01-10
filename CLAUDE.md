# Lazarus - Claude Code Project Guide

## Project Overview

**Lazarus** is a cross-platform 2D game built with **C# and MonoGame 3.8**, targeting .NET 9.0. The game features two gameplay modes:
1. **Classic Platformer** - Tile-based levels with gems, enemies, and platforming mechanics
2. **Top-Down Exploration** - 8-directional movement with Tiled map support (newer feature)

## Quick Navigation

### Solution Structure
```
Lazarus/
├── Lazarus.sln                    # Main solution
├── Lazarus/
│   ├── Lazarus.Core/              # Shared game logic (this is the main codebase)
│   │   ├── LazarusGame.cs         # Main game class entry point
│   │   ├── Game/                 # Core game mechanics
│   │   ├── Screens/              # UI and gameplay screens
│   │   ├── ScreenManagers/       # Screen state management
│   │   ├── Settings/             # Configuration/persistence
│   │   ├── Effects/              # Particle system
│   │   ├── Inputs/               # Input handling
│   │   ├── Localization/         # Multi-language support
│   │   └── Content/              # Game assets (sprites, sounds, levels)
│   └── Lazarus.DesktopGL/         # Desktop platform launcher
└── Tiled/                        # Tiled map editor files (.tmx, .tsx)
```

### Key Entry Points
- **Game Initialization**: `Lazarus.Core/LazarusGame.cs`
- **Desktop Launcher**: `Lazarus.DesktopGL/Program.cs`
- **Current Start Screen**: `TopDownGameplayScreen` (development mode - normally `MainMenuScreen`)

## Architecture Overview

### Screen System
All screens inherit from `GameScreen` base class and are managed by `ScreenManager`:
- **Lifecycle**: `LoadContent()` → `Update()` → `HandleInput()` → `Draw()` → `UnloadContent()`
- **States**: `TransitionOn` → `Active` → `TransitionOff` → `Hidden`

### Key Screens
| Screen | File | Purpose |
|--------|------|---------|
| `TopDownGameplayScreen` | `Screens/TopDownGameplayScreen.cs` | Top-down exploration mode |
| `GameplayScreen` | `Screens/GameplayScreen.cs` | Classic platformer mode |
| `MainMenuScreen` | `Screens/MainMenuScreen.cs` | Main menu |
| `PauseScreen` | `Screens/PauseScreen.cs` | In-game pause menu |
| `SettingsScreen` | `Screens/SettingsScreen.cs` | Game settings |

### Core Game Classes

#### Top-Down Mode (Newer)
| Class | File | Responsibility |
|-------|------|----------------|
| `TopDownLevel` | `Game/TopDownLevel.cs` | Level container with camera |
| `TopDownPlayer` | `Game/TopDownPlayer.cs` | 8-directional player character |
| `TiledMap` | `Game/TiledMap.cs` | Tiled .tmx file parser/renderer |
| `DirectionalAnimation` | `Game/DirectionalAnimation.cs` | Per-direction animation sets |
| `Direction` | `Game/Direction.cs` | 8-way direction enum |

#### Classic Platformer Mode
| Class | File | Responsibility |
|-------|------|----------------|
| `Level` | `Game/Level.cs` | Classic level logic |
| `Player` | `Game/Player.cs` | Platformer physics/movement |
| `Enemy` | `Game/Enemy.cs` | AI enemies |
| `Gem` | `Game/Gem.cs` | Collectibles |
| `Tile` | `Game/Tile.cs` | Tile definitions |

### Manager Classes
| Manager | File | Responsibility |
|---------|------|----------------|
| `ScreenManager` | `ScreenManagers/ScreenManager.cs` | Screen stack management, input routing |
| `SettingsManager<T>` | `Settings/SettingsManager.cs` | Generic settings persistence |
| `ParticleManager` | `Effects/ParticleManager.cs` | Particle effects system |
| `LocalizationManager` | `Localization/LocalizationManager.cs` | Multi-language support |

## Common Development Tasks

### Adding a New Screen
1. Create class inheriting `GameScreen` in `Screens/`
2. Override `LoadContent()`, `Update()`, `HandleInput()`, `Draw()`
3. Add to screen stack via `ScreenManager.AddScreen()`

### Adding New Content/Assets
1. Place assets in `Lazarus.Core/Content/` appropriate subfolder
2. For sprites loaded at runtime: use `Texture2D.FromStream()`
3. For MGCB-managed assets: add to `Content/Lazarus.mgcb`

### Working with Tiled Maps
- Map files: `Tiled/biomes/*.tmx`
- Tilesets: `Tiled/tilesets/*.tsx`
- Parser: `TiledMap.Load(path)` in `Game/TiledMap.cs`

### Input Handling
- All input through `InputState` class
- Supports keyboard, gamepad (4 players), mouse, touch
- Access via `HandleInput(GameTime, InputState)` in screens

## Build & Run

```bash
# From solution root
cd Lazarus
dotnet build Lazarus.DesktopGL
dotnet run --project Lazarus.DesktopGL
```

## Project Conventions

### Naming
- **Namespaces**: `Lazarus.Core.{System}` (e.g., `Lazarus.Core.Settings`)
- **Classes**: PascalCase
- **Private fields**: camelCase (sometimes with underscore prefix)

### Design Patterns Used
- **Manager Pattern**: Centralized system controllers (ScreenManager, ParticleManager)
- **Service Locator**: `Game.Services` for dependency injection
- **State Pattern**: Screen states, gem states
- **Strategy Pattern**: ISettingsStorage implementations

### Code Style
- XML documentation comments on public members
- Debug output via `System.Diagnostics.Debug.WriteLine()`
- Platform checks via `LazarusGame.IsMobile` / `LazarusGame.IsDesktop`

## Important Constants

### Screen Resolution
- Base size: 800x480 pixels (in `ScreenManager`)
- Scales to fit actual display

### Player Movement (Top-Down)
- Walk speed: 150 px/sec
- Run speed: 300 px/sec (hold movement >0.8s)

### Player Movement (Platformer)
- Move acceleration: 13000
- Max speed: 1750
- Jump velocity: -3500
- Gravity: 3400

## File Locations Quick Reference

| What | Where |
|------|-------|
| Main game class | `Lazarus.Core/LazarusGame.cs` |
| All screens | `Lazarus.Core/Screens/` |
| Game mechanics | `Lazarus.Core/Game/` |
| Sprite assets | `Lazarus.Core/Content/Sprites/` |
| Level files | `Lazarus.Core/Content/Levels/` |
| Tiled maps | `Tiled/biomes/` |
| Sound effects | `Lazarus.Core/Content/Sounds/` |
| Fonts | `Lazarus.Core/Content/Fonts/` |
| Settings code | `Lazarus.Core/Settings/` |

## Current Development State

The game currently starts directly in `TopDownGameplayScreen` (bypassing main menu) for development/testing. The normal flow would be:
`MainMenuScreen` → `LoadingScreen` → `GameplayScreen`

Mobile platforms (Android/iOS) are stubbed but not fully implemented.
