# Lazarus Architecture Documentation

## System Overview

```mermaid
graph TB
    subgraph LazarusGame["LazarusGame (Main game class)"]
        SM[ScreenManager<br/>Manages UI screen stack]
        GSS[GameStateService<br/>Save/Load Progression]
        SetM[SettingsManager<br/>Persists settings]
        PM[ParticleManager<br/>Visual FX system]

        subgraph Screens["GameScreen (base)"]
            LC[LoadContent] --> U[Update] --> HI[HandleInput] --> D[Draw] --> UL[Unload]
        end

        LM[LocalizationManager<br/>i18n - 5 languages]
        AM[AudioManager<br/>Music/SFX]
        AS[AccessibilitySettings]
    end
```

## Initialization Flow

```mermaid
flowchart TD
    A[Program.Main] --> B[LazarusGame Constructor]
    B --> B1[Platform detection Mobile/Desktop]
    B --> B2[GraphicsDeviceManager creation]
    B --> B3["SettingsManager&lt;LazarusSettings&gt; creation"]
    B --> B4["SettingsManager&lt;LazarusLeaderboard&gt; creation"]
    B --> B5[ScreenManager creation]

    B5 --> C[LazarusGame.Initialize]
    C --> C1[LocalizationManager setup]
    C --> C2[GameStateService creation]
    C --> C3[Add MainMenuScreen]

    C3 --> D[LazarusGame.LoadContent]
    D --> D1[Load particle texture]
    D --> D2[Create ParticleManager]

    D2 --> E["Game.Run() → MonoGame Game Loop"]

    E --> F[Game Loop]

    subgraph F[Game Loop]
        F1[InputState.Update<br/>Poll all input devices] --> F2[ScreenManager.Update<br/>Update active screens<br/>Route input to screens]
        F2 --> F3[ScreenManager.Draw<br/>Render all screens]
        F3 --> F1
    end
```

## Screen System Architecture

### Screen Lifecycle

```mermaid
stateDiagram-v2
    [*] --> TransitionOn: AddScreen()
    TransitionOn --> Active: Fade in complete
    Active --> TransitionOff: ExitScreen()
    TransitionOff --> Hidden: Fade out complete
    Hidden --> TransitionOn: Re-activate
    Hidden --> [*]: RemoveScreen()

    note right of Active: Receiving input
    note right of TransitionOn: Fading in
    note right of TransitionOff: Fading out
```

### Screen Stack Example

```mermaid
block-beta
    columns 1
    block:stack["ScreenManager - Screen Stack (bottom to top)"]
        A["BackgroundScreen ← Background layer"]
        B["WorldScreen ← Main gameplay"]
        C["GameMenuScreen (popup) ← Overlay"]
        D["PartyScreen (popup) ← Sub-overlay (receives input)"]
    end
```

### Screen Inheritance Hierarchy

```mermaid
classDiagram
    GameScreen <|-- MenuScreen
    GameScreen <|-- WorldScreen
    GameScreen <|-- CombatScreen
    GameScreen <|-- DungeonScreen
    GameScreen <|-- DungeonExplorationScreen
    GameScreen <|-- BestiaryScreen
    GameScreen <|-- PartyScreen
    GameScreen <|-- RecruitmentScreen
    GameScreen <|-- CompanionSelectScreen
    GameScreen <|-- EquipmentScreen
    GameScreen <|-- InventoryScreen
    GameScreen <|-- TradingScreen
    GameScreen <|-- FactionScreen
    GameScreen <|-- FactionReputationScreen
    GameScreen <|-- BiomeMapScreen
    GameScreen <|-- DialogScreen
    GameScreen <|-- EndingScreen
    GameScreen <|-- SaveLoadScreen
    GameScreen <|-- LoadingScreen
    GameScreen <|-- TransitionScreen
    GameScreen <|-- BackgroundScreen
    GameScreen <|-- AccessibilityScreen
    GameScreen <|-- AudioSettingsScreen
    GameScreen <|-- InputSettingsScreen
    GameScreen <|-- MessageBoxScreen

    MenuScreen <|-- MainMenuScreen
    MenuScreen <|-- GamePauseScreen
    MenuScreen <|-- SettingsMenuScreen
    MenuScreen <|-- LanguageScreen
    MenuScreen <|-- AboutScreen

    class GameScreen {
        <<abstract>>
        +LoadContent()
        +Update()
        +HandleInput()
        +Draw()
    }

    class MenuScreen {
        <<abstract>>
        +MenuEntries
    }
```

## Core Game Architecture

### Entity System

```mermaid
classDiagram
    class Protagonist {
        +Position
        +Animation
        +Exoskeleton
        +Movement
    }

    class Companion {
        +Follows protagonist
        +Combat help
        +Bond level
    }

    class NPC {
        +Dialog
        +Shop
        +Faction
        +Quests
    }

    class Stray {
        +StrayStats (61 stats)
        +Equipment
        +Evolution
        +Level
        +Experience
        +BondLevel
        +CombatRow
        +Abilities
    }

    class StrayStats {
        +HP/Energy
        +ATK types
        +DEF types
        +Elemental
        +Accuracy
        +Evasion
    }

    class Equipment {
        +Augmentations (13-14 slots)
        +Microchips (sockets)
    }

    class Evolution {
        +Stage
        +Stress
        +History
    }

    Stray *-- StrayStats
    Stray *-- Equipment
    Stray *-- Evolution
```

### Combat System Architecture

```mermaid
stateDiagram-v2
    [*] --> Starting
    Starting --> Running: Initialize combatants
    Running --> SelectingAction: ATB gauge full
    SelectingAction --> SelectingAbility: Choose ability
    SelectingAction --> SelectingTarget: Choose target
    SelectingAbility --> SelectingTarget: Ability selected
    SelectingTarget --> ExecutingAction: Target selected
    ExecutingAction --> Running: Action complete

    Running --> Victory: All enemies defeated
    Running --> Defeat: All allies defeated
    Running --> Fled: Escape successful

    Victory --> [*]
    Defeat --> [*]
    Fled --> [*]
```

```mermaid
classDiagram
    class CombatState {
        +Phase
        +Combatants[]
        +CurrentTurn
    }

    class Combatant {
        +Stray
        +IsPlayer
        +ATB Gauge
        +Position
        +Buffs
        +Debuffs
    }

    class CombatAI {
        +SelectAction()
        +EvaluateThreats()
    }

    class BossAI {
        +PhaseTransitions
        +SpecialPatterns
        +UniqueMechanics
    }

    CombatState o-- Combatant
    CombatAI <|-- BossAI
```

### World System Architecture

```mermaid
graph TB
    subgraph GameWorld
        subgraph Biomes
            B1[The Fringe]
            B2[The Rust]
            B3[The Green]
            B4[The Glow]
            B5[...]
        end

        BP[BiomePortals<br/>transitions]

        B1 & B2 & B3 & B4 --> BP
    end

    subgraph WorldComponents
        CH[Chunks<br/>- Tile data<br/>- Collision<br/>- Spawn points]
        SE[Settlements<br/>- NPCs<br/>- Shops<br/>- Services]
        EN[Encounters<br/>- Wild Strays<br/>- Level range<br/>- Conditions]
    end

    subgraph Systems
        WS[WeatherSystem<br/>- Rain/Storm<br/>- Fog<br/>- Effects]
        WE[WorldEventSystem<br/>- Migrations<br/>- Conflicts<br/>- Special spawn]
        MM[MiniMap<br/>- Terrain view<br/>- POI markers]
    end

    subgraph Buildings
        BPO[BuildingPortals<br/>entries]
        INT[Interiors<br/>indoor maps]
    end
```

### Dungeon System Architecture

```mermaid
graph TB
    subgraph Definition
        DD[DungeonDefinition<br/>- Room count<br/>- Enemy types<br/>- Loot tables<br/>- Boss info]
    end

    DD -->|generates| DI

    subgraph DI[DungeonInstance - runtime]
        subgraph Rooms[DungeonRooms]
            R1[Room 1<br/>entrance] --> R2[Room 2<br/>enemies]
            R2 --> R3[Room 3<br/>treasure]
            R3 --> R4[Boss Room]
            R2 --> R2b[Room 2b<br/>optional]
        end
    end

    subgraph Support
        RG[RoomGenerator<br/>- Layout gen<br/>- Enemy place<br/>- Loot place]
        ER[ExplorableRoom<br/>- Searchable<br/>- Hidden areas]
        DR[DungeonReward<br/>- Loot calc<br/>- XP rewards]
    end

    DP[DungeonPortal<br/>- Location<br/>- Difficulty<br/>- Preview]
```

### Progression System Architecture

```mermaid
graph TB
    subgraph QuestLog
        AQ[Active Quests]
        CQ[Completed Quests]
        FQ[Failed Quests]

        AQ --> Q[Quest<br/>- State<br/>- Progress<br/>- Objectives]
        Q --> QD[QuestDefinition<br/>- Objectives<br/>- Rewards<br/>- Dialog<br/>- Branching paths]
    end

    subgraph FactionSystem[Faction System]
        FT[FactionTypes:<br/>Shepherds, Harvesters,<br/>Archivists, Ascendants,<br/>Ferals, Independents,<br/>Machinists, Lazarus]

        FS[FactionStanding:<br/>Hostile → Unfriendly → Neutral → Friendly → Allied<br/>-1000 → -499 → -99 to 99 → 100 → 500+]
    end

    subgraph OtherSystems
        BE[Bestiary<br/>- Discovered<br/>- Caught<br/>- Completion %]
        ACH[Achievements<br/>- Unlockables<br/>- Progress<br/>- Rewards]
        TUT[TutorialSystem<br/>- First-time triggers<br/>- Gating]
        NGP[NewGamePlus<br/>- Carryover<br/>- Scaling<br/>- Exclusives]
    end
```

## Input System Architecture

```mermaid
graph TB
    subgraph InputState
        KB[Keyboard]
        GP[GamePad<br/>4 slots]
        MS[Mouse]

        KB & GP & MS --> CCL[CurrentCursorLocation<br/>unified position]

        TO[Touch]
        GS[Gestures<br/>taps]
    end

    subgraph GamepadManager
        GM[- Button remapping<br/>- Dead zone config<br/>- Vibration control<br/>- Multi-controller up to 4]
    end

    ST[State Tracking:<br/>- Previous frame state<br/>- Current frame state<br/>- Delta detection]
```

## Settings System Architecture

```mermaid
classDiagram
    class ISettingsStorage {
        <<interface>>
        +Save()
        +Load()
    }

    class DesktopSettingsStorage {
        file-based
    }

    class MobileSettingsStorage {
        platform APIs
    }

    class ConsoleSettingsStorage {
        console-specific
    }

    ISettingsStorage <|.. DesktopSettingsStorage
    ISettingsStorage <|.. MobileSettingsStorage
    ISettingsStorage <|.. ConsoleSettingsStorage

    class LazarusSettings {
        +MusicVolume
        +SfxVolume
        +Language
        +Accessibility opts
        +Control bindings
    }

    class LazarusLeaderboard {
        +Achievement data
        +Statistics
    }

    class SettingsManager~T~ {
        +Settings: T
        +Save()
        +Load()
    }

    SettingsManager --> ISettingsStorage
```

## Game State Service Architecture

```mermaid
classDiagram
    class GameStateService {
        +NewGame(companionType)
        +SaveGame(slot)
        +LoadGame(slot)
        +SetFlag(name)
        +GetFlag(name)
        +ModifyFactionReputation(faction, amount)
    }

    class GameSaveData {
        +ProtagonistState
        +WorldState
        +Collection
        +Progression
        +Flags
        +PlayTime
    }

    class ProtagonistState {
        +Position
        +CompanionType
        +ExoskeletonStatus
    }

    class WorldState {
        +CurrentBiome
        +DiscoveredLocations
        +WeatherState
    }

    class Collection {
        +StrayRoster
        +PartyComposition
        +Inventory
    }

    class Progression {
        +QuestStates
        +FactionStandings
        +StoryFlags
        +ActProgress
    }

    GameStateService --> GameSaveData
    GameSaveData *-- ProtagonistState
    GameSaveData *-- WorldState
    GameSaveData *-- Collection
    GameSaveData *-- Progression
```

## Service Registration

```mermaid
graph TD
    GS[Game.Services<br/>IServiceProvider]

    GS --> GDM[GraphicsDeviceManager<br/>Registered in constructor]
    GS --> SM["SettingsManager&lt;LazarusSettings&gt;<br/>Registered in constructor"]
    GS --> SL["SettingsManager&lt;LazarusLeaderboard&gt;<br/>Registered in constructor"]
    GS --> SCM[ScreenManager<br/>Added as GameComponent]
    GS --> PM[ParticleManager<br/>Registered in LoadContent]
    GS --> GSS[GameStateService<br/>Registered in Initialize]
```

**Access pattern:**
```csharp
var gameState = game.Services.GetService<GameStateService>();
var settings = game.Services.GetService<SettingsManager<LazarusSettings>>();
```

## Platform Abstraction

```mermaid
graph TB
    subgraph LazarusGame
        PD[Platform Detection]

        PD --> |IsAndroid OR IsIOS| Mobile
        PD --> |IsMacOS OR IsLinux OR IsWindows| Desktop

        subgraph Mobile
            M1[Fullscreen]
            M2[Touch input]
            M3[MobileSettingsStorage]
            M4[Landscape orientation]
            M5[Virtual gamepad]
        end

        subgraph Desktop
            D1[Windowed 1280x800]
            D2[Mouse/keyboard]
            D3[DesktopSettingsStorage]
            D4[Resizable window]
            D5[Physical gamepad]
        end
    end
```

## Data Flow: Typical Gameplay Session

```mermaid
flowchart TD
    MMS[MainMenuScreen<br/>New Game] --> CS[CompanionSelect<br/>choose dog]
    CS --> LS[LoadingScreen]
    LS --> WS[WorldScreen<br/>exploration]

    WS <--> GMS[GameMenuScreen]
    GMS <--> PS[PartyScreen<br/>EquipScreen<br/>BestiaryScreen]

    WS -->|encounter| CBS[CombatScreen<br/>ATB battle]
    CBS -->|capture| RS[RecruitmentScreen]
    RS --> WS
    CBS -->|victory/defeat| WS
```
