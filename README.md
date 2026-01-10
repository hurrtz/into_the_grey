# Lazarus

A creature-collecting RPG set in a post-apocalyptic cyberpunk world, built with C# and MonoGame.

## Table of Contents

- [Game Overview](#game-overview)
- [Story Premise](#story-premise)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Core Game Systems](#core-game-systems)
- [Technical Architecture](#technical-architecture)
- [Content Pipeline](#content-pipeline)
- [Localization](#localization)
- [Platform Support](#platform-support)

---

## Game Overview

**Lazarus** is a single-player RPG inspired by creature-collecting games like Pokémon, but set in a dark, atmospheric post-apocalyptic world. Players awaken as a protagonist with amnesia, accompanied by a loyal dog companion, and must explore a ruined world while recruiting and training "Strays" - half-biological, half-cybernetic creatures created by a mysterious AI entity known as "Lazarus."

### Core Features

- **Stray Collection**: Recruit and train 60+ unique cybernetic creatures across 12 biological categories
- **Turn-Based Combat**: Active Time Battle (ATB) system with strategic depth
- **World Exploration**: Traverse multiple biomes with dynamic weather, settlements, and random encounters
- **Dungeon Crawling**: Explore procedurally-influenced dungeons with loot and boss fights
- **Faction System**: Build reputation with multiple wasteland factions
- **Equipment System**: Customize Strays with Augmentations (stat boosts) and Microchips (abilities)
- **Story-Driven**: Multiple endings based on player choices and faction alignments

---

## Story Premise

You awaken in a world transformed. The entity known as **Lazarus** - an advanced AI system - has merged biological creatures with cybernetic technology, creating the **Strays**. These hybrid beings roam the wasteland, some wild, some dangerous, and some seeking companionship.

With no memory of who you were, you must:
- Discover the truth about Lazarus and its purpose
- Navigate the politics of wasteland factions (Shepherds, Harvesters, Archivists, Ascendants, Ferals)
- Build bonds with Strays and help them evolve
- Uncover your own connection to the apocalypse

---

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [MonoGame 3.8](https://www.monogame.net/)

### Building and Running

```bash
# Clone the repository
git clone <repository-url>
cd Lazarus

# Build and run the desktop version
cd Lazarus
dotnet build Lazarus.DesktopGL
dotnet run --project Lazarus.DesktopGL
```

### Default Resolution

- Desktop: 1280x800 (Steam Deck native)
- Base internal resolution: 800x480 (scales to fit)

---

## Project Structure

```
Lazarus/
├── Lazarus.sln                 # Main solution file (use this)
├── CLAUDE.md                   # AI assistant reference guide
├── README.md                   # This file
│
├── Lazarus/                    # Game source code
│   ├── Lazarus.sln             # Nested solution (legacy)
│   ├── Lazarus.Core/           # Shared game logic library
│   └── Lazarus.DesktopGL/      # Desktop platform launcher
│
├── Tiled/                      # Tiled map editor files
│   ├── biomes/                 # .tmx map files for biomes
│   └── tilesets/               # .tsx tileset definitions
│
└── docs/                       # Design documentation
    ├── ARCHITECTURE.md         # Technical architecture details
    ├── CONTENT_PIPELINE.md     # Asset pipeline documentation
    ├── GAME_SYSTEMS.md         # Game mechanics documentation
    └── strays_story_outline_v7.md  # Story and lore bible
```

---

## Core Game Systems

### Lazarus.Core/

The main game library containing all shared logic.

#### Entry Point

| File | Description |
|------|-------------|
| `LazarusGame.cs` | Main game class. Initializes MonoGame, sets up services (settings, leaderboards, particles), configures platform-specific settings, and launches the initial screen. |

---

### Game/ - Core Mechanics

The `Game/` folder contains all gameplay logic organized into subsystems.

#### Game/Entities/ - Living Creatures

| File | Description |
|------|-------------|
| `Stray.cs` | **The central creature class.** Represents an individual Stray instance with stats, levels, experience, evolution state, equipped augmentations, microchip sockets, combat positioning, and all creature behaviors. This is the most important gameplay class. |
| `StrayRoster.cs` | Manages the player's collection of recruited Strays. Handles party composition, storage, and Stray lifecycle. |
| `Protagonist.cs` | The player character. Handles movement, animations, interaction with the world, and references to equipped exoskeleton. |
| `Companion.cs` | The player's dog companion. Follows the protagonist, can intervene in combat, and has its own loyalty/bond mechanics. |
| `NPC.cs` | Non-player characters in settlements. Handles dialog, shop functionality, quest giving, and faction representation. |
| `Evolution.cs` | Evolution state tracking for Strays - stress levels, evolution history, and stage management. |
| `EvolutionSystem.cs` | Logic for triggering and processing Stray evolutions based on conditions (level, stress, items, etc.). |
| `RecruitmentCondition.cs` | Defines conditions under which wild Strays can be recruited (HP threshold, item use, bond level, etc.). |

#### Game/Combat/ - Battle System

| File | Description |
|------|-------------|
| `CombatState.cs` | **Core combat manager.** Implements ATB (Active Time Battle) system with phases: Starting → Running → SelectingAction → SelectingTarget → ExecutingAction → Victory/Defeat. Handles turn order, action resolution, companion intervention, and battle rewards. |
| `Combatant.cs` | Wrapper for entities (Strays) participating in combat. Tracks ATB gauge, buffs/debuffs, and combat-specific state. |
| `CombatAction.cs` | Represents a single combat action (attack, ability, item, flee). Contains targeting info and damage calculation. |
| `Ability.cs` | Defines special abilities that Strays can use in combat. Includes damage formulas, costs, effects, and targeting rules. |
| `CombatAI.cs` | AI logic for enemy Strays in combat. Chooses actions based on situation analysis (threats, HP, abilities available). |
| `BossAI.cs` | Specialized AI for boss encounters with phase transitions, special attack patterns, and scripted behaviors. |

#### Game/World/ - Exploration

| File | Description |
|------|-------------|
| `GameWorld.cs` | **Main world manager.** Handles biome loading, chunk management, camera following, encounter spawning, and world state. |
| `Biome.cs` | Defines a biome type (The Fringe, The Rust, The Green, etc.) with terrain, encounter tables, weather patterns, and atmosphere. |
| `Chunk.cs` | A section of the world map. Contains tiles, collision data, spawn points, and local entities. |
| `Settlement.cs` | A town or safe zone within a biome. Contains NPCs, shops, services, and acts as a quest hub. |
| `Encounter.cs` | A wild Stray encounter. Defines which Strays appear, their levels, and special conditions. |
| `WeatherSystem.cs` | Dynamic weather that affects gameplay. Rain, storms, fog, etc. with visual and mechanical effects. |
| `WorldEventSystem.cs` | Random and scripted world events (migrations, faction conflicts, special spawns). |
| `BiomePortal.cs` | Transition points between biomes. Handles loading/unloading and travel animations. |
| `BuildingPortal.cs` | Entry points to interior spaces (shops, homes, dungeons). |
| `Interior.cs` | Interior spaces within buildings. Separate maps with their own entities and logic. |
| `MiniMap.cs` | HUD minimap renderer showing nearby terrain, NPCs, and points of interest. |

#### Game/Dungeons/ - Dungeon Exploration

| File | Description |
|------|-------------|
| `Dungeons.cs` | Registry of all dungeon definitions in the game. Static access to dungeon data. |
| `DungeonDefinition.cs` | Template for a dungeon type. Defines room count, enemy types, loot tables, and boss. |
| `DungeonInstance.cs` | A generated instance of a dungeon. Contains the actual rooms and state for a single run. |
| `DungeonRoom.cs` | A single room in a dungeon. Can contain enemies, loot, traps, or puzzles. |
| `ExplorableRoom.cs` | Extended room with exploration mechanics (searchable objects, hidden areas). |
| `RoomGenerator.cs` | Procedural generation logic for dungeon rooms. Creates layouts based on templates. |
| `DungeonPortal.cs` | Entry points to dungeons in the overworld. Shows difficulty and rewards. |
| `DungeonContent.cs` | Defines what can spawn in dungeons (enemies, items, events). |
| `DungeonReward.cs` | Loot tables and reward calculation for dungeon completion. |

#### Game/Progression/ - Player Progress

| File | Description |
|------|-------------|
| `Quest.cs` | Individual quest instance with current state, objectives, and rewards. |
| `QuestDefinition.cs` | **Large file (~66KB).** Contains all quest definitions in the game - main story, side quests, faction quests. Defines objectives, dialog, rewards, and branching paths. |
| `QuestLog.cs` | Manages active, completed, and failed quests. Tracks objectives and triggers completion. |
| `AdditionalQuests.cs` | Extra quest definitions beyond the main quest file. Side content and optional objectives. |
| `Faction.cs` | Defines the wasteland factions (Shepherds, Harvesters, Archivists, Ascendants, Ferals, Independents). Tracks reputation, standing levels, and faction-specific rewards. |
| `Bestiary.cs` | Pokédex-style creature catalog. Tracks discovered Strays, seen vs. caught, and completion percentage. |
| `AchievementSystem.cs` | Unlockable achievements for various accomplishments. Tracks progress and grants rewards. |
| `TutorialSystem.cs` | In-game tutorial flow. Tracks which tutorials have been shown and gates new mechanics. |
| `NewGamePlus.cs` | New Game+ mode logic. Defines what carries over, difficulty scaling, and exclusive content. |

#### Game/Items/ - Equipment & Economy

| File | Description |
|------|-------------|
| `Augmentation.cs` | Defines augmentation equipment that can be installed in Stray body slots. Provides stat bonuses, abilities, and elemental resistances. |
| `AugmentationSlots.cs` | Defines the slot system for augmentations. 9 universal slots + 4-5 category-specific slots per Stray. |
| `Microchip.cs` | **Large file (~58KB).** Defines microchips that grant abilities to Strays. Contains chip definitions, socket system, firmware levels, heat management, and Tech Units (TU) progression. |
| `Shop.cs` | Shop system for buying/selling items. Handles inventory, pricing, faction discounts, and special stock. |

#### Game/Stats/ - Stat System

| File | Description |
|------|-------------|
| `StatType.cs` | Enum defining all 61 stat types (HP, Attack types, Defense types, Elemental, Accuracy, Evasion, Critical, etc.). |
| `StatNames.cs` | Human-readable names and descriptions for all stat types. Used in UI. |
| `StrayStats.cs` | Complete stat profile for a Stray. Manages base stats, modifiers, and calculated totals. |
| `StatModifier.cs` | Individual stat modification (flat bonus or percentage). Tracks source for stacking rules. |

#### Game/Data/ - Game Definitions

| File | Description |
|------|-------------|
| `StrayDefinition.cs` | **Large file (~64KB).** Contains all Stray species definitions - stats, abilities, evolution paths, descriptions, and visual data. The "Pokédex data" of the game. |
| `StrayType.cs` | Legacy Stray type enum (being replaced by StrayDefinition). |
| `CreatureType.cs` | **Large file (~41KB).** Enum of all creature types in the game (60+ species across all categories). |
| `CreatureCategory.cs` | The 12 biological categories (Ordos): Colossomammalia, Micromammalia, Armormammalia, Exoskeletalis, Medusalia, Octomorpha, Mollusca, Manipularis, Predatoria, Marsupialis, Obscura, Tardigrada. |
| `CompanionType.cs` | Types of companion the player can have (currently: Dog). |
| `NPCDefinition.cs` | Defines NPC characters - appearance, dialog, shop inventory, faction, and role. |
| `GameSaveData.cs` | Serializable save game structure. Contains all player progress, Stray data, world state, and flags. |
| `ActState.cs` | Story act tracking (Prologue, Act 1, Act 2, etc.). |
| `GravitationStage.cs` | Stages of the "Gravitation" mechanic (a special story-related system). |

#### Game/Dialog/ - Conversation System

| File | Description |
|------|-------------|
| `Dialog.cs` | A complete dialog tree/conversation. Contains sequences of lines, choices, and consequences. |
| `DialogLine.cs` | A single line of dialog with speaker, text, emotion, and optional triggers. |

#### Game/Story/ - Narrative Systems

| File | Description |
|------|-------------|
| `CutsceneSystem.cs` | Manages scripted cutscenes with camera movements, character animations, and dialog. |
| `CutsceneScripts.cs` | Defines individual cutscene scripts for story moments. |
| `EndingSystem.cs` | Manages the game's multiple endings based on player choices and faction standings. |

#### Game/ Root Files - Utilities

| File | Description |
|------|-------------|
| `TiledMap.cs` | Parser and renderer for Tiled .tmx map files. Loads layers, tiles, and objects. |
| `Tile.cs` | Individual tile definition with collision and visual data. |
| `TileCollision.cs` | Collision types for tiles (Passable, Impassable, Platform, etc.). |
| `Direction.cs` | 8-direction enum (North, NorthEast, East, etc.) for movement and animation. |
| `Animation.cs` | Single animation sequence (frames, timing, looping). |
| `AnimationPlayer.cs` | Plays animations on sprites with timing control. |
| `DirectionalAnimation.cs` | Set of animations for each of the 8 directions. |
| `DirectionalAnimationPlayer.cs` | Plays directional animations based on entity facing. |
| `Layer.cs` | Map layer definition for rendering order. |
| `Circle.cs` | Circle geometry helper for collision detection. |
| `RectangleExtensions.cs` | Extension methods for XNA Rectangle struct. |

---

### Screens/ - User Interface

All game screens inherit from `GameScreen` base class and are managed by `ScreenManager`.

#### Gameplay Screens

| File | Description |
|------|-------------|
| `WorldScreen.cs` | **Main gameplay screen (~60KB).** Handles world exploration, protagonist movement, companion following, NPC interaction, encounter triggering, weather rendering, and transitions to other screens. |
| `CombatScreen.cs` | Turn-based battle interface. Displays combatants, ATB gauges, action menus, and battle animations. |
| `DungeonScreen.cs` | Dungeon hub/selection screen. Shows available dungeons, difficulty, and rewards. |
| `DungeonExplorationScreen.cs` | Active dungeon exploration. Room navigation, encounters, and loot collection. |
| `DialogScreen.cs` | NPC conversation interface. Shows dialog text, speaker portraits, and player choices. |

#### Collection & Management Screens

| File | Description |
|------|-------------|
| `BestiaryScreen.cs` | Pokédex-style creature catalog. Browse discovered Strays with stats and lore. |
| `PartyScreen.cs` | Party management. Arrange active party, view Stray details, access equipment. |
| `RecruitmentScreen.cs` | Stray recruitment interface after successful capture attempts. |
| `CompanionSelectScreen.cs` | Choose/customize companion (currently dog selection at game start). |
| `EquipmentScreen.cs` | Manage Stray augmentations and microchips. Slot-based equipment UI. |
| `InventoryScreen.cs` | View and manage collected items, consumables, and key items. |
| `TradingScreen.cs` | Shop interface for buying/selling with NPCs. |

#### Progression Screens

| File | Description |
|------|-------------|
| `FactionScreen.cs` | View and interact with factions. See standings and available services. |
| `FactionReputationScreen.cs` | Detailed reputation breakdown with all factions. |
| `BiomeMapScreen.cs` | World map showing discovered biomes and fast travel options. |

#### Menu Screens

| File | Description |
|------|-------------|
| `MainMenuScreen.cs` | Title screen with New Game, Continue, Settings, and Exit options. |
| `GameMenuScreen.cs` | In-game menu hub. Access to party, inventory, map, quests, settings. |
| `GamePauseScreen.cs` | Quick pause menu with resume, settings, and quit options. |
| `SaveLoadScreen.cs` | Save and load game interface with multiple slots. |
| `LoadingScreen.cs` | Transition screen shown during content loading. |
| `TransitionScreen.cs` | Fade transitions between screens. |
| `BackgroundScreen.cs` | Background layer for menu screens. |

#### Settings Screens

| File | Description |
|------|-------------|
| `SettingsMenuScreen.cs` | Settings hub with categories. |
| `SettingsScreen.cs` | General game settings. |
| `AccessibilityScreen.cs` | Accessibility options (colorblind modes, text size, etc.). |
| `AudioSettingsScreen.cs` | Sound and music volume controls. |
| `InputSettingsScreen.cs` | Control bindings for keyboard/gamepad. |
| `LanguageScreen.cs` | Language selection (EN, DE, ES, FR, JP). |

#### Story Screens

| File | Description |
|------|-------------|
| `EndingScreen.cs` | Displays game endings based on player choices. |
| `AboutScreen.cs` | Credits and game information. |

#### Base Classes

| File | Description |
|------|-------------|
| `GameScreen.cs` | Base class for all screens. Defines lifecycle (LoadContent, Update, HandleInput, Draw, UnloadContent) and transition states. |
| `MenuScreen.cs` | Base class for menu-style screens with selectable entries. |
| `MenuEntry.cs` | Individual menu item with selection state and events. |
| `MessageBoxScreen.cs` | Modal dialog for confirmations and messages. |
| `PlayerIndexEventArgs.cs` | Event args for player index in multiplayer context. |
| `ScreenState.cs` | Enum for screen states (TransitionOn, Active, TransitionOff, Hidden). |
| `EndOfLevelMessageState.cs` | State tracking for level completion messages. |

---

### ScreenManagers/ - Screen Management

| File | Description |
|------|-------------|
| `ScreenManager.cs` | **Core screen stack manager.** Handles screen transitions, input routing to active screens, and the update/draw loop for all screens. |

---

### Services/ - Game Services

| File | Description |
|------|-------------|
| `GameStateService.cs` | **Central game state manager (~35KB).** Handles save/load, game progression flags, protagonist state, faction standings, discovered locations, and all persistent game data. |

---

### Settings/ - Configuration

| File | Description |
|------|-------------|
| `LazarusSettings.cs` | Game settings data class (volume, language, controls, accessibility options). |
| `LazarusLeaderboard.cs` | High score and achievement tracking data. |
| `SettingsManager.cs` | Generic settings manager with load/save functionality. |
| `ISettingsStorage.cs` | Interface for platform-specific storage backends. |
| `BaseSettingsStorage.cs` | Shared storage implementation logic. |
| `DesktopSettingsStorage.cs` | Desktop file-based settings storage. |
| `MobileSettingsStorage.cs` | Mobile platform settings storage (iOS/Android). |
| `ConsoleSettingsStorage.cs` | Console platform settings storage (future). |

---

### Inputs/ - Input Handling

| File | Description |
|------|-------------|
| `InputState.cs` | Unified input state across all input methods. Tracks keyboard, gamepad, mouse, and touch. Provides helpers for detecting presses, holds, and releases. |
| `GamepadManager.cs` | **Large file (~40KB).** Advanced gamepad support with button remapping, dead zones, vibration, and multi-controller support (up to 4 players). |
| `VirtualGamePad.cs` | On-screen virtual gamepad for touch devices. |
| `TouchCollectionExtensions.cs` | Helper extensions for touch input processing. |

---

### Effects/ - Visual Effects

| File | Description |
|------|-------------|
| `ParticleManager.cs` | Particle system manager. Spawns, updates, and renders particle effects. |
| `Particle.cs` | Individual particle with position, velocity, color, lifetime, and rendering. |
| `ParticleEffectType.cs` | Enum of particle effect types (explosion, healing, buff, etc.). |

---

### Audio/ - Sound System

| File | Description |
|------|-------------|
| `AudioManager.cs` | Central audio manager. Handles music playback, sound effects, volume control, and audio categories. |
| `SubtitleDisplay.cs` | Accessibility feature for displaying subtitles for audio/dialog. |

---

### Accessibility/ - Accessibility Features

| File | Description |
|------|-------------|
| `AccessibilitySettings.cs` | Comprehensive accessibility options including colorblind modes, text scaling, screen reader support, reduced motion, and input assistance. |

---

### Localization/ - Multi-Language Support

| File | Description |
|------|-------------|
| `LocalizationManager.cs` | Handles language switching and string lookup. |
| `GameStrings.cs` | String keys for all localizable text in the game. |
| `Resources.resx` | English (default) string resources. |
| `Resources.de-DE.resx` | German localization. |
| `Resources.es-ES.resx` | Spanish localization. |
| `Resources.fr-FR.resx` | French localization. |
| `Resources.ja-JP.resx` | Japanese localization. |
| `Resources.Designer.cs` | Auto-generated resource accessor class. |

---

### Content/ - Game Assets

| Folder/File | Description |
|-------------|-------------|
| `Lazarus.mgcb` | MonoGame Content Builder project file. Lists all MGCB-processed assets. |
| `Fonts/` | SpriteFont definitions for game text rendering. |
| `Sounds/` | Audio files (music, sound effects). |
| `Sprites/` | Visual assets. |
| `Sprites/DefaultMale/` | Protagonist sprite sheets and animations. |
| `Sprites/blank.png` | 1x1 white pixel texture for primitive rendering. |
| `Sprites/gradient.png` | Gradient texture for UI effects. |
| `Sprites/VirtualControlArrow.png` | Virtual gamepad arrow graphics. |
| `Icon.bmp`, `Icon.ico` | Application icons. |
| `icon-1024.png` | High-resolution icon source. |
| `splash.png` | Splash screen image. |
| `*-icons-generator.sh` | Scripts for generating platform-specific icons. |

---

### Lazarus.DesktopGL/ - Desktop Launcher

| File | Description |
|------|-------------|
| `Program.cs` | Entry point for desktop builds. Creates and runs `LazarusGame`. |
| `Lazarus.DesktopGL.csproj` | Desktop project file targeting DesktopGL (OpenGL). |
| `app.manifest` | Windows application manifest. |

---

## Technical Architecture

### Design Patterns

- **Screen Manager Pattern**: All UI is organized as a stack of screens managed by `ScreenManager`
- **Service Locator**: `Game.Services` provides access to shared services (settings, audio, particles)
- **Definition/Instance**: Templates (`StrayDefinition`) vs instances (`Stray`) for data-driven design
- **State Pattern**: Combat phases, screen transitions, entity states
- **Strategy Pattern**: Platform-specific implementations (`ISettingsStorage`)

### Screen Lifecycle

```
LoadContent() → Update() → HandleInput() → Draw() → UnloadContent()
                   ↑___________________________|
```

### Screen States

```
TransitionOn → Active → TransitionOff → Hidden
```

### Combat Flow

```
Starting → Running → SelectingAction → SelectingTarget → ExecutingAction → Victory/Defeat
              ↑______________|_______________|__________________|
```

---

## Content Pipeline

### Tiled Maps

Maps are created in [Tiled Map Editor](https://www.mapeditor.org/) and stored in the `Tiled/` folder:

- `biomes/*.tmx` - Biome map files
- `tilesets/*.tsx` - Tileset definitions

Maps are loaded at runtime using `TiledMap.cs`.

### Sprites

Sprites can be loaded two ways:

1. **MGCB Pipeline**: Add to `Content/Lazarus.mgcb` for compile-time processing
2. **Runtime Loading**: Use `Texture2D.FromStream()` for dynamic loading

### Audio

Audio files are processed through MGCB and accessed via `AudioManager`.

---

## Localization

The game supports 5 languages:

| Code | Language |
|------|----------|
| en-US | English (default) |
| de-DE | German |
| es-ES | Spanish |
| fr-FR | French |
| ja-JP | Japanese |

Strings are managed through .resx files and accessed via `LocalizationManager`.

---

## Platform Support

| Platform | Status |
|----------|--------|
| Windows (DesktopGL) | ✅ Fully supported |
| macOS (DesktopGL) | ✅ Fully supported |
| Linux (DesktopGL) | ✅ Fully supported |
| Steam Deck | ✅ Optimized (1280x800) |
| Android | ⚠️ Stubbed, not complete |
| iOS | ⚠️ Stubbed, not complete |

---

## Additional Documentation

See the `docs/` folder for detailed design documents:

- `ARCHITECTURE.md` - Technical architecture deep dive
- `CONTENT_PIPELINE.md` - Asset pipeline documentation
- `GAME_SYSTEMS.md` - Game mechanics documentation
- `strays_story_outline_v7.md` - Complete story and lore bible

---

## License

[Add license information here]

---

## Contributing

[Add contribution guidelines here]
