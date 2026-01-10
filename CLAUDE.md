# Lazarus - Claude Code Project Guide

## Project Overview

**Lazarus** is a creature-collecting RPG built with **C# and MonoGame 3.8**, targeting .NET 9.0. Set in a post-apocalyptic cyberpunk world, players control a protagonist who awakens with amnesia and must collect and train "Strays" - half-biological, half-cybernetic creatures created by the mysterious entity "Lazarus."

### Core Gameplay Features
- **Stray Collection** - Recruit and train 60+ cybernetic creatures across different categories
- **Turn-Based Combat** - ATB (Active Time Battle) system with physical/elemental damage types
- **World Exploration** - Multiple biomes with settlements, NPCs, weather, and random encounters
- **Dungeon Exploration** - Procedurally generated rooms with loot and bosses
- **Progression Systems** - Quests, factions, achievements, bestiary completion
- **Equipment** - Augmentations (stat boosts) and Microchips (abilities)

## Quick Navigation

### Solution Structure
```
Lazarus/
├── Lazarus.sln                    # Root solution (use this one)
├── CLAUDE.md                      # This file
├── Lazarus/
│   ├── Lazarus.sln                # Nested solution (legacy)
│   ├── Lazarus.Core/              # Shared game logic (main codebase)
│   │   ├── LazarusGame.cs         # Main game class entry point
│   │   ├── Game/                  # Core game mechanics
│   │   │   ├── Combat/            # Turn-based combat system
│   │   │   ├── Data/              # Game data definitions
│   │   │   ├── Dialog/            # Dialog system
│   │   │   ├── Dungeons/          # Dungeon generation/exploration
│   │   │   ├── Entities/          # Protagonist, Strays, NPCs, Companions
│   │   │   ├── Items/             # Augmentations, Microchips, Shop
│   │   │   ├── Progression/       # Quests, Factions, Achievements
│   │   │   ├── Stats/             # Comprehensive stat system (61 stats)
│   │   │   ├── Story/             # Cutscenes, endings
│   │   │   └── World/             # Biomes, chunks, settlements, weather
│   │   ├── Screens/               # UI and gameplay screens (37 screens)
│   │   ├── ScreenManagers/        # Screen state management
│   │   ├── Services/              # Game state service
│   │   ├── Settings/              # Configuration/persistence
│   │   ├── Effects/               # Particle system
│   │   ├── Inputs/                # Input handling
│   │   ├── Accessibility/         # Accessibility features
│   │   ├── Audio/                 # Audio management
│   │   ├── Localization/          # Multi-language support (5 languages)
│   │   └── Content/               # Game assets (sprites, sounds, fonts)
│   └── Lazarus.DesktopGL/         # Desktop platform launcher
├── Tiled/                         # Tiled map editor files
│   ├── biomes/                    # .tmx map files
│   └── tilesets/                  # .tsx tileset files
└── docs/                          # Documentation
```

### Key Entry Points
- **Game Initialization**: `Lazarus.Core/LazarusGame.cs`
- **Desktop Launcher**: `Lazarus.DesktopGL/Program.cs`
- **Main Gameplay**: `Screens/WorldScreen.cs` (exploration)
- **Combat**: `Screens/CombatScreen.cs`
- **Start Screen**: `MainMenuScreen` → `WorldScreen`

## Architecture Overview

### Screen System
All screens inherit from `GameScreen` base class and are managed by `ScreenManager`:
- **Lifecycle**: `LoadContent()` → `Update()` → `HandleInput()` → `Draw()` → `UnloadContent()`
- **States**: `TransitionOn` → `Active` → `TransitionOff` → `Hidden`

### Key Screens (37 total)

| Category | Screen | Purpose |
|----------|--------|---------|
| **Gameplay** | `WorldScreen` | Main world exploration |
| | `CombatScreen` | Turn-based battles |
| | `DungeonScreen` | Dungeon hub/selection |
| | `DungeonExplorationScreen` | Dungeon room exploration |
| **Collection** | `BestiaryScreen` | Pokédex-style creature catalog |
| | `PartyScreen` | Party management |
| | `RecruitmentScreen` | Recruit new Strays |
| **Equipment** | `EquipmentScreen` | Manage augmentations/chips |
| | `InventoryScreen` | Item inventory |
| | `TradingScreen` | Buy/sell items |
| **Progression** | `FactionScreen` | Faction interactions |
| | `FactionReputationScreen` | Reputation standings |
| | `BiomeMapScreen` | World map |
| **Menu** | `MainMenuScreen` | Title/main menu |
| | `GameMenuScreen` | In-game menu hub |
| | `GamePauseScreen` | Pause menu |
| | `SaveLoadScreen` | Save/load games |
| **Settings** | `SettingsMenuScreen` | Settings hub |
| | `AccessibilityScreen` | Accessibility options |
| | `AudioSettingsScreen` | Sound settings |
| | `InputSettingsScreen` | Control bindings |
| | `LanguageScreen` | Language selection |
| **Story** | `DialogScreen` | NPC conversations |
| | `EndingScreen` | Game endings |

### Core Game Systems

#### Stray System (`Game/Entities/Stray.cs`)
The central creature class with:
- **Stats**: 61 different stat types (HP, ATK types, DEF types, elemental, etc.)
- **Levels**: 1-100, with 10% stat scaling per level
- **Evolution**: Multi-stage evolution system
- **Equipment**: Augmentation slots (13-14 per creature) + Microchip sockets
- **Combat Row**: Front (+20% physical damage dealt/taken) or Back (-20%)

#### Combat System (`Game/Combat/`)
- **ATB-based**: Speed stat determines action frequency
- **Damage Types**: Physical (Impact, Piercing, Slashing) + Elemental (7 types)
- **Phases**: Starting → Running → SelectingAction → SelectingTarget → ExecutingAction → Victory/Defeat
- **Companion Intervention**: Dog companion can help during battles

#### World System (`Game/World/`)
- **Biomes**: Different terrain types with unique encounters
- **Chunks**: World divided into explorable chunks
- **Settlements**: Towns with NPCs and services
- **Weather**: Dynamic weather affecting gameplay
- **Encounters**: Random wild Stray encounters

#### Progression (`Game/Progression/`)
- **Quests**: Main story and side quests via `QuestLog`
- **Factions**: Multiple factions with reputation tracking
- **Achievements**: Unlockable achievements
- **Bestiary**: Track discovered Strays

### Manager Classes
| Manager | File | Responsibility |
|---------|------|----------------|
| `ScreenManager` | `ScreenManagers/ScreenManager.cs` | Screen stack, input routing |
| `GameStateService` | `Services/GameStateService.cs` | Save/load, game progression |
| `SettingsManager<T>` | `Settings/SettingsManager.cs` | Generic settings persistence |
| `ParticleManager` | `Effects/ParticleManager.cs` | Particle effects |
| `LocalizationManager` | `Localization/LocalizationManager.cs` | Multi-language (EN, DE, ES, FR, JP) |

## Common Development Tasks

### Adding a New Screen
1. Create class inheriting `GameScreen` in `Screens/`
2. Override `LoadContent()`, `Update()`, `HandleInput()`, `Draw()`
3. Add to screen stack via `ScreenManager.AddScreen()`

### Adding a New Stray
1. Add `CreatureType` enum value in `Game/Data/CreatureType.cs`
2. Create `StrayDefinition` in `Game/Data/StrayDefinition.cs`
3. Register in `StrayDefinitions` static class

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
# From repository root
cd Lazarus
dotnet build Lazarus.DesktopGL
dotnet run --project Lazarus.DesktopGL
```

## Project Conventions

### Naming
- **Namespaces**: `Lazarus.Core.{System}` (e.g., `Lazarus.Core.Game.Combat`)
- **Classes**: PascalCase
- **Private fields**: `_camelCase` with underscore prefix

### Design Patterns Used
- **Manager Pattern**: Centralized system controllers (ScreenManager, ParticleManager)
- **Service Locator**: `Game.Services` for dependency injection
- **State Pattern**: Screen states, combat phases
- **Strategy Pattern**: ISettingsStorage implementations
- **Definition/Instance**: StrayDefinition (template) → Stray (instance)

### Code Style
- XML documentation comments on public members
- Debug output via `System.Diagnostics.Debug.WriteLine()`
- Platform checks via `LazarusGame.IsMobile` / `LazarusGame.IsDesktop`

## Important Constants

### Screen Resolution
- Base size: 800x480 pixels (in `ScreenManager`)
- Desktop default: 1280x800 (Steam Deck native)
- Scales to fit actual display

### Stray Stats
- Level scaling: +10% per level
- Energy scaling: +5% per level
- Energy regen: +1 every 5 levels

### Combat
- Front row: +20% physical damage dealt/taken
- Back row: -20% physical damage dealt/taken
- Companion intervention: 15% base chance, checked every 3 seconds

## File Locations Quick Reference

| What | Where |
|------|-------|
| Main game class | `Lazarus/Lazarus.Core/LazarusGame.cs` |
| All screens | `Lazarus/Lazarus.Core/Screens/` |
| Combat system | `Lazarus/Lazarus.Core/Game/Combat/` |
| Stray entities | `Lazarus/Lazarus.Core/Game/Entities/` |
| World/biomes | `Lazarus/Lazarus.Core/Game/World/` |
| Dungeons | `Lazarus/Lazarus.Core/Game/Dungeons/` |
| Stat system | `Lazarus/Lazarus.Core/Game/Stats/` |
| Quest/progression | `Lazarus/Lazarus.Core/Game/Progression/` |
| Items/equipment | `Lazarus/Lazarus.Core/Game/Items/` |
| Data definitions | `Lazarus/Lazarus.Core/Game/Data/` |
| Sprite assets | `Lazarus/Lazarus.Core/Content/Sprites/` |
| Sound effects | `Lazarus/Lazarus.Core/Content/Sounds/` |
| Fonts | `Lazarus/Lazarus.Core/Content/Fonts/` |
| Tiled maps | `Tiled/biomes/` |
| Settings code | `Lazarus/Lazarus.Core/Settings/` |
| Localization | `Lazarus/Lazarus.Core/Localization/` |

## Current Development State

The game starts at `MainMenuScreen` and progresses to `WorldScreen` for main gameplay. Key systems implemented:
- ✅ World exploration with biomes and weather
- ✅ Turn-based ATB combat
- ✅ Stray collection and evolution
- ✅ Dungeon exploration
- ✅ Equipment system (augmentations + microchips)
- ✅ Quest and faction systems
- ✅ Save/load functionality
- ✅ Localization (5 languages)
- ⚠️ Mobile platforms (Android/iOS) stubbed but not fully implemented
