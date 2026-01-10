# Lazarus Architecture Documentation

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           LazarusGame                                    │
│  (Main game class - initialization, services, game loop)                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐       │
│  │ScreenManager│ │GameState    │ │SettingsMgr  │ │ParticleMgr  │       │
│  │             │ │Service      │ │             │ │             │       │
│  │ Manages UI  │ │ Save/Load   │ │ Persists    │ │ Visual FX   │       │
│  │ screen stack│ │ Progression │ │ settings    │ │ system      │       │
│  └──────┬──────┘ └─────────────┘ └─────────────┘ └─────────────┘       │
│         │                                                                │
│  ┌──────▼───────────────────────────────────────────────────────────┐   │
│  │                      GameScreen (base)                            │   │
│  │  LoadContent() → Update() → HandleInput() → Draw() → Unload()    │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                        │
│  │LocalizeMgr  │ │AudioManager │ │Accessibility│                        │
│  │ i18n (5 lang)│ │ Music/SFX   │ │ Settings    │                        │
│  └─────────────┘ └─────────────┘ └─────────────┘                        │
└─────────────────────────────────────────────────────────────────────────┘
```

## Initialization Flow

```
Program.Main()
    │
    ▼
LazarusGame()
    ├── Platform detection (Mobile/Desktop)
    ├── GraphicsDeviceManager creation
    ├── SettingsManager<LazarusSettings> creation
    ├── SettingsManager<LazarusLeaderboard> creation
    └── ScreenManager creation
    │
    ▼
LazarusGame.Initialize()
    ├── LocalizationManager setup (from saved language)
    ├── GameStateService creation
    └── Add MainMenuScreen
    │
    ▼
LazarusGame.LoadContent()
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

### Screen Stack Example

```
┌─────────────────────────────────────┐
│          ScreenManager              │
│                                     │
│  Screen Stack (bottom to top):      │
│  ┌─────────────────────────────┐    │
│  │ BackgroundScreen            │ ←── Background layer
│  ├─────────────────────────────┤    │
│  │ WorldScreen                 │ ←── Main gameplay
│  ├─────────────────────────────┤    │
│  │ GameMenuScreen (popup)      │ ←── Overlay (optional)
│  ├─────────────────────────────┤    │
│  │ PartyScreen (popup)         │ ←── Sub-overlay
│  └─────────────────────────────┘    │
│                                     │
│  Only topmost non-covered screen    │
│  receives input                     │
└─────────────────────────────────────┘
```

### Screen Inheritance Hierarchy

```
GameScreen (abstract base)
│
├── MenuScreen (abstract menu base)
│   ├── MainMenuScreen
│   ├── GamePauseScreen
│   ├── SettingsMenuScreen
│   ├── LanguageScreen
│   └── AboutScreen
│
├── WorldScreen (main exploration gameplay)
├── CombatScreen (ATB battle system)
│
├── DungeonScreen (dungeon selection hub)
├── DungeonExplorationScreen (room exploration)
│
├── BestiaryScreen (creature catalog)
├── PartyScreen (party management)
├── RecruitmentScreen (recruit Strays)
├── CompanionSelectScreen
│
├── EquipmentScreen (augmentations/chips)
├── InventoryScreen (items)
├── TradingScreen (shops)
│
├── FactionScreen (faction interactions)
├── FactionReputationScreen
├── BiomeMapScreen (world map)
│
├── DialogScreen (NPC conversations)
├── EndingScreen (game endings)
│
├── SaveLoadScreen
├── LoadingScreen
├── TransitionScreen
├── BackgroundScreen
│
├── AccessibilityScreen
├── AudioSettingsScreen
├── InputSettingsScreen
└── MessageBoxScreen
```

## Core Game Architecture

### Entity System

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          Entity Hierarchy                                │
│                                                                          │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐            │
│  │   Protagonist  │  │    Companion   │  │      NPC       │            │
│  │                │  │                │  │                │            │
│  │ - Position     │  │ - Follows      │  │ - Dialog       │            │
│  │ - Animation    │  │   protagonist  │  │ - Shop         │            │
│  │ - Exoskeleton  │  │ - Combat help  │  │ - Faction      │            │
│  │ - Movement     │  │ - Bond level   │  │ - Quests       │            │
│  └────────────────┘  └────────────────┘  └────────────────┘            │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                           Stray                                  │    │
│  │                                                                  │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │    │
│  │  │ StrayStats   │  │ Equipment    │  │ Evolution    │          │    │
│  │  │ (61 stats)   │  │              │  │              │          │    │
│  │  │              │  │ Augmentations│  │ - Stage      │          │    │
│  │  │ - HP/Energy  │  │ (13-14 slots)│  │ - Stress     │          │    │
│  │  │ - ATK types  │  │              │  │ - History    │          │    │
│  │  │ - DEF types  │  │ Microchips   │  │              │          │    │
│  │  │ - Elemental  │  │ (sockets)    │  │              │          │    │
│  │  │ - Accuracy   │  │              │  │              │          │    │
│  │  │ - Evasion    │  │              │  │              │          │    │
│  │  └──────────────┘  └──────────────┘  └──────────────┘          │    │
│  │                                                                  │    │
│  │  Properties: Level, Experience, BondLevel, CombatRow, Abilities │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Combat System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           CombatState                                    │
│                                                                          │
│  Combat Phases:                                                          │
│  ┌─────────┐   ┌─────────┐   ┌─────────────┐   ┌─────────────┐         │
│  │Starting │──▶│ Running │──▶│Selecting    │──▶│Selecting    │         │
│  │         │   │ (ATB)   │   │Action       │   │Target       │         │
│  └─────────┘   └────┬────┘   └──────┬──────┘   └──────┬──────┘         │
│                     │               │                  │                 │
│                     │               ▼                  ▼                 │
│                     │        ┌─────────────┐   ┌─────────────┐         │
│                     │        │Selecting    │   │Executing    │         │
│                     │        │Ability      │   │Action       │──┐      │
│                     │        └─────────────┘   └─────────────┘  │      │
│                     │                                           │      │
│                     └───────────────────────────────────────────┘      │
│                                          │                              │
│                     ┌────────────────────┼────────────────────┐        │
│                     ▼                    ▼                    ▼        │
│              ┌─────────────┐     ┌─────────────┐     ┌─────────────┐  │
│              │  Victory    │     │   Defeat    │     │    Fled     │  │
│              └─────────────┘     └─────────────┘     └─────────────┘  │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                        Combatants                                │   │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐                 │   │
│  │  │ Combatant  │  │ Combatant  │  │ Combatant  │  ...            │   │
│  │  │            │  │            │  │            │                 │   │
│  │  │ - Stray    │  │ - ATB Gauge│  │ - Buffs    │                 │   │
│  │  │ - IsPlayer │  │ - Position │  │ - Debuffs  │                 │   │
│  │  └────────────┘  └────────────┘  └────────────┘                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  AI Systems:                                                            │
│  ┌────────────┐  ┌────────────┐                                        │
│  │  CombatAI  │  │   BossAI   │                                        │
│  │ (enemies)  │  │ (bosses)   │                                        │
│  └────────────┘  └────────────┘                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

### World System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                            GameWorld                                     │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                          Biomes                                  │   │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐            │   │
│  │  │The Fringe│ │The Rust │  │The Green│  │The Glow │  ...       │   │
│  │  └────┬────┘  └────┬────┘  └────┬────┘  └────┬────┘            │   │
│  │       │            │            │            │                   │   │
│  │       └────────────┴─────┬──────┴────────────┘                   │   │
│  │                          ▼                                       │   │
│  │                   ┌─────────────┐                                │   │
│  │                   │BiomePortals │ (transitions)                  │   │
│  │                   └─────────────┘                                │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐            │
│  │     Chunks     │  │  Settlements   │  │   Encounters   │            │
│  │                │  │                │  │                │            │
│  │ - Tile data    │  │ - NPCs         │  │ - Wild Strays  │            │
│  │ - Collision    │  │ - Shops        │  │ - Level range  │            │
│  │ - Spawn points │  │ - Services     │  │ - Conditions   │            │
│  └────────────────┘  └────────────────┘  └────────────────┘            │
│                                                                          │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐            │
│  │ WeatherSystem  │  │WorldEventSystem│  │    MiniMap     │            │
│  │                │  │                │  │                │            │
│  │ - Rain/Storm   │  │ - Migrations   │  │ - Terrain view │            │
│  │ - Fog          │  │ - Conflicts    │  │ - POI markers  │            │
│  │ - Effects      │  │ - Special spawn│  │                │            │
│  └────────────────┘  └────────────────┘  └────────────────┘            │
│                                                                          │
│  ┌────────────────┐  ┌────────────────┐                                │
│  │BuildingPortals │  │   Interiors    │                                │
│  │ (entries)      │  │ (indoor maps)  │                                │
│  └────────────────┘  └────────────────┘                                │
└─────────────────────────────────────────────────────────────────────────┘
```

### Dungeon System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          Dungeon System                                  │
│                                                                          │
│  ┌─────────────────┐                                                    │
│  │DungeonDefinition│ (template)                                         │
│  │ - Room count    │                                                    │
│  │ - Enemy types   │                                                    │
│  │ - Loot tables   │                                                    │
│  │ - Boss info     │                                                    │
│  └────────┬────────┘                                                    │
│           │ generates                                                    │
│           ▼                                                              │
│  ┌─────────────────┐                                                    │
│  │ DungeonInstance │ (runtime)                                          │
│  │                 │                                                    │
│  │  ┌───────────────────────────────────────────────────────────┐      │
│  │  │                    DungeonRooms                            │      │
│  │  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │      │
│  │  │  │  Room 1  │──│  Room 2  │──│  Room 3  │──│  Boss    │  │      │
│  │  │  │(entrance)│  │(enemies) │  │(treasure)│  │  Room    │  │      │
│  │  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │      │
│  │  └───────────────────────────────────────────────────────────┘      │
│  └─────────────────┘                                                    │
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐         │
│  │  RoomGenerator  │  │ ExplorableRoom  │  │  DungeonReward  │         │
│  │                 │  │                 │  │                 │         │
│  │ - Layout gen    │  │ - Searchable    │  │ - Loot calc     │         │
│  │ - Enemy place   │  │ - Hidden areas  │  │ - XP rewards    │         │
│  │ - Loot place    │  │                 │  │                 │         │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘         │
│                                                                          │
│  ┌─────────────────┐                                                    │
│  │  DungeonPortal  │ (world entry point)                                │
│  │ - Location      │                                                    │
│  │ - Difficulty    │                                                    │
│  │ - Preview       │                                                    │
│  └─────────────────┘                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Progression System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Progression Systems                               │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                         QuestLog                                 │   │
│  │  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐    │   │
│  │  │ Active Quests  │  │Completed Quests│  │ Failed Quests  │    │   │
│  │  └───────┬────────┘  └────────────────┘  └────────────────┘    │   │
│  │          │                                                       │   │
│  │          ▼                                                       │   │
│  │  ┌────────────────┐     ┌─────────────────────────┐             │   │
│  │  │     Quest      │────▶│    QuestDefinition      │             │   │
│  │  │ - State        │     │ - Objectives            │             │   │
│  │  │ - Progress     │     │ - Rewards               │             │   │
│  │  │ - Objectives   │     │ - Dialog                │             │   │
│  │  └────────────────┘     │ - Branching paths       │             │   │
│  │                         └─────────────────────────┘             │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      Faction System                              │   │
│  │                                                                  │   │
│  │  FactionType:  Shepherds │ Harvesters │ Archivists │ Ascendants │   │
│  │                Ferals    │ Independents│ Machinists│ Lazarus    │   │
│  │                                                                  │   │
│  │  FactionStanding: Hostile ─▶ Unfriendly ─▶ Neutral ─▶           │   │
│  │                   (-1000)    (-499)        (-99 to 99)           │   │
│  │                                                                  │   │
│  │                   ─▶ Friendly ─▶ Allied                          │   │
│  │                      (100)       (500+)                          │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐            │
│  │    Bestiary    │  │ Achievements   │  │ TutorialSystem │            │
│  │                │  │                │  │                │            │
│  │ - Discovered   │  │ - Unlockables  │  │ - First-time   │            │
│  │ - Caught       │  │ - Progress     │  │   triggers     │            │
│  │ - Completion % │  │ - Rewards      │  │ - Gating       │            │
│  └────────────────┘  └────────────────┘  └────────────────┘            │
│                                                                          │
│  ┌────────────────┐                                                     │
│  │  NewGamePlus   │                                                     │
│  │ - Carryover    │                                                     │
│  │ - Scaling      │                                                     │
│  │ - Exclusives   │                                                     │
│  └────────────────┘                                                     │
└─────────────────────────────────────────────────────────────────────────┘
```

## Input System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           InputState                                     │
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │   Keyboard   │  │   GamePad    │  │    Mouse     │                  │
│  │              │  │   [4 slots]  │  │              │                  │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘                  │
│         │                 │                 │                           │
│         └────────────┬────┴────────────┬────┘                           │
│                      ▼                 ▼                                │
│              ┌─────────────────────────────────┐                        │
│              │ CurrentCursorLocation           │                        │
│              │ (unified position)              │                        │
│              └─────────────────────────────────┘                        │
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐                                    │
│  │    Touch     │  │   Gestures   │                                    │
│  │              │  │   (taps)     │                                    │
│  └──────────────┘  └──────────────┘                                    │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                     GamepadManager                                │  │
│  │  - Button remapping                                               │  │
│  │  - Dead zone configuration                                        │  │
│  │  - Vibration control                                              │  │
│  │  - Multi-controller (up to 4)                                     │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  State Tracking:                                                        │
│  - Previous frame state                                                 │
│  - Current frame state                                                  │
│  - Delta detection (IsNewKeyPress, IsNewButtonPress, etc.)             │
└─────────────────────────────────────────────────────────────────────────┘
```

## Settings System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      SettingsManager<T>                                  │
│                                                                          │
│  ┌─────────────────┐                                                    │
│  │ ISettingsStorage│ ◄──────── Interface                                │
│  └────────┬────────┘                                                    │
│           │                                                              │
│           ├──────────────────────────────────────────────────┐          │
│           ▼                                                   ▼          │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐      │
│  │DesktopSettings   │  │MobileSettings    │  │ConsoleSettings   │      │
│  │Storage           │  │Storage           │  │Storage           │      │
│  │(file-based)      │  │(platform APIs)   │  │(console-specific)│      │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘      │
│                                                                          │
│  Data Classes:                                                          │
│  ┌──────────────────────┐  ┌──────────────────────┐                    │
│  │   LazarusSettings    │  │  LazarusLeaderboard  │                    │
│  │ - MusicVolume        │  │ - Achievement data   │                    │
│  │ - SfxVolume          │  │ - Statistics         │                    │
│  │ - Language           │  │                      │                    │
│  │ - Accessibility opts │  │                      │                    │
│  │ - Control bindings   │  │                      │                    │
│  └──────────────────────┘  └──────────────────────┘                    │
└─────────────────────────────────────────────────────────────────────────┘
```

## Game State Service Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        GameStateService                                  │
│                                                                          │
│  Central manager for all persistent game state                          │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                       GameSaveData                               │   │
│  │                                                                  │   │
│  │  Protagonist State:          │  World State:                    │   │
│  │  - Position                  │  - Current biome                 │   │
│  │  - Companion type            │  - Discovered locations          │   │
│  │  - Exoskeleton status        │  - Weather state                 │   │
│  │                              │                                   │   │
│  │  Collection:                 │  Progression:                    │   │
│  │  - StrayRoster               │  - Quest states                  │   │
│  │  - Party composition         │  - Faction standings             │   │
│  │  - Inventory                 │  - Story flags                   │   │
│  │                              │  - Act progress                  │   │
│  │  Flags:                      │                                   │   │
│  │  - Dictionary<string, bool>  │  Play Time:                      │   │
│  │  - Story triggers            │  - Total time                    │   │
│  │  - Tutorial completion       │  - Session time                  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  Methods:                                                               │
│  - NewGame(companionType)                                               │
│  - SaveGame(slot)                                                       │
│  - LoadGame(slot)                                                       │
│  - SetFlag(name) / GetFlag(name)                                        │
│  - ModifyFactionReputation(faction, amount)                             │
└─────────────────────────────────────────────────────────────────────────┘
```

## Service Registration

```
Game.Services (IServiceProvider)
│
├── GraphicsDeviceManager
│   └── Registered in LazarusGame constructor
│
├── SettingsManager<LazarusSettings>
│   └── Registered in LazarusGame constructor
│
├── SettingsManager<LazarusLeaderboard>
│   └── Registered in LazarusGame constructor
│
├── ScreenManager
│   └── Added as GameComponent in constructor
│
├── ParticleManager
│   └── Registered in LazarusGame.LoadContent()
│
└── GameStateService
    └── Registered in LazarusGame.Initialize()

Access pattern:
var gameState = game.Services.GetService<GameStateService>();
var settings = game.Services.GetService<SettingsManager<LazarusSettings>>();
```

## Platform Abstraction

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          LazarusGame                                     │
│                                                                          │
│  Platform Detection:                                                     │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │  IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()│ │
│  │  IsDesktop = OperatingSystem.IsMacOS() || IsLinux() || IsWindows()│ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  Platform-Specific Behavior:                                            │
│  ┌─────────────────────────┐  ┌─────────────────────────┐              │
│  │        Mobile           │  │        Desktop          │              │
│  │                         │  │                         │              │
│  │ - Fullscreen            │  │ - Windowed (1280x800)   │              │
│  │ - Touch input           │  │ - Mouse/keyboard        │              │
│  │ - MobileSettingsStorage │  │ - DesktopSettingsStorage│              │
│  │ - Landscape orientation │  │ - Resizable window      │              │
│  │ - Virtual gamepad       │  │ - Physical gamepad      │              │
│  └─────────────────────────┘  └─────────────────────────┘              │
└─────────────────────────────────────────────────────────────────────────┘
```

## Data Flow: Typical Gameplay Session

```
┌──────────────┐
│ MainMenuScreen│
│ "New Game"   │
└──────┬───────┘
       │
       ▼
┌──────────────┐     ┌──────────────┐
│CompanionSelect│────▶│ LoadingScreen│
│(choose dog)  │     │              │
└──────────────┘     └──────┬───────┘
                            │
       ┌────────────────────┘
       ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│ WorldScreen  │◀───▶│ GameMenu     │◀───▶│ PartyScreen  │
│ (exploration)│     │ Screen       │     │ EquipScreen  │
└──────┬───────┘     └──────────────┘     │ BestiaryScr  │
       │                                   └──────────────┘
       │ encounter
       ▼
┌──────────────┐     ┌──────────────┐
│ CombatScreen │────▶│ Recruitment  │ (if capture)
│ (ATB battle) │     │ Screen       │
└──────┬───────┘     └──────────────┘
       │
       │ victory/defeat
       ▼
┌──────────────┐
│ WorldScreen  │ (return to exploration)
└──────────────┘
```
