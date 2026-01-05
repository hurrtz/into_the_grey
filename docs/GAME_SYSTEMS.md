# Strays Game Systems Documentation

## Top-Down Mode Systems

### Player Movement

**File**: `Strays.Core/Game/TopDownPlayer.cs`

#### 8-Direction System

```
         North (0, -1)
            │
NorthWest   │   NorthEast
  (-1,-1) ╲ │ ╱ (1, -1)
           ╲│╱
West ───────┼─────── East
(-1, 0)    ╱│╲      (1, 0)
          ╱ │ ╲
SouthWest   │   SouthEast
  (-1, 1)   │   (1, 1)
            │
         South (0, 1)
```

#### Movement Constants
```csharp
WalkSpeed = 150f    // pixels per second
RunSpeed = 300f     // pixels per second
RunThreshold = 0.8f // seconds holding direction to start running
```

#### Input Mapping
| Input | Direction |
|-------|-----------|
| W / Up Arrow | North |
| S / Down Arrow | South |
| A / Left Arrow | West |
| D / Right Arrow | East |
| W+D / Up+Right | NorthEast |
| W+A / Up+Left | NorthWest |
| S+D / Down+Right | SouthEast |
| S+A / Down+Left | SouthWest |
| GamePad Left Stick | 8-way analog |

#### Speed Transition
- Default: Walk speed
- After holding direction for `RunThreshold` seconds: Run speed
- Releasing direction: Reset to walk

#### Animation Selection
```csharp
if (!IsMoving)
    PlayAnimation(idleAnimation)
else if (isRunning)
    PlayAnimation(runAnimation)
else
    PlayAnimation(walkAnimation)
```

### Directional Animation System

**Files**:
- `Strays.Core/Game/DirectionalAnimation.cs`
- `Strays.Core/Game/DirectionalAnimationPlayer.cs`

#### Sprite Organization
```
Content/Sprites/DefaultMale/animations/
├── breathing-idle/
│   ├── south/
│   │   ├── frame_0.png
│   │   ├── frame_1.png
│   │   └── ...
│   ├── south-east/
│   ├── east/
│   ├── north-east/
│   ├── north/
│   ├── north-west/
│   ├── west/
│   └── south-west/
├── walk/
│   └── [same structure]
└── running-6-frames/
    └── [same structure]
```

#### Direction-to-Folder Mapping
```csharp
Direction.South     → "south"
Direction.SouthEast → "south-east"
Direction.East      → "east"
Direction.NorthEast → "north-east"
Direction.North     → "north"
Direction.NorthWest → "north-west"
Direction.West      → "west"
Direction.SouthWest → "south-west"
```

#### Loading Pattern
```csharp
var animation = DirectionalAnimation.LoadFromFiles(
    graphicsDevice,
    basePath: "Content/Sprites/DefaultMale/animations",
    animationName: "walk",
    frameTime: 0.1f,
    isLooping: true
);
```

### Tiled Map System

**File**: `Strays.Core/Game/TiledMap.cs`

#### Supported Features
- Multiple tile layers
- External tileset references (.tsx files)
- CSV-encoded tile data
- Tile collision (basic)

#### Map File Format (.tmx)
```xml
<?xml version="1.0" encoding="UTF-8"?>
<map version="1.10" width="50" height="50" tilewidth="64" tileheight="64">
    <tileset firstgid="1" source="../tilesets/sidewalk_64.tsx"/>
    <layer name="Ground" width="50" height="50">
        <data encoding="csv">
            1,2,3,4,5,...
        </data>
    </layer>
</map>
```

#### Tileset Format (.tsx)
```xml
<?xml version="1.0" encoding="UTF-8"?>
<tileset name="sidewalk" tilewidth="64" tileheight="64" tilecount="16" columns="4">
    <image source="sidewalk_64.png" width="256" height="256"/>
</tileset>
```

#### Rendering
- Only visible tiles are rendered (viewport culling)
- Camera position determines visible area
- Layers rendered in order (bottom to top)

### Camera System

**File**: `Strays.Core/Game/TopDownLevel.cs`

#### Behavior
- Follows player position
- Centered on player (with viewport offset)
- Smooth following (direct tracking, no interpolation currently)

```csharp
cameraPosition = new Vector2(
    player.Position.X - viewport.Width / 2,
    player.Position.Y - viewport.Height / 2
);
```

---

## Classic Platformer Mode Systems

### Physics System

**File**: `Strays.Core/Game/Player.cs`

#### Constants
```csharp
// Horizontal Movement
MoveAcceleration = 13000.0f      // acceleration rate
MaxMoveSpeed = 1750.0f           // max horizontal velocity
GroundDragFactor = 0.48f         // friction when grounded
AirDragFactor = 0.58f            // air resistance

// Vertical Movement
GravityAcceleration = 3400.0f    // downward acceleration
MaxFallSpeed = 550.0f            // terminal velocity
JumpLaunchVelocity = -3500.0f    // initial jump impulse

// Jump Control
JumpControlPower = 0.14f         // air control factor
MaxJumpTime = 0.35f              // max jump button hold time
```

#### Movement Formula
```csharp
// Horizontal
velocity.X += movement * MoveAcceleration * elapsed;
velocity.X *= isOnGround ? GroundDragFactor : AirDragFactor;
velocity.X = Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

// Vertical
velocity.Y = Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

// Position update
position += velocity * elapsed;
```

#### Jump Mechanics
- Variable height based on button hold duration
- Jump cut when button released early
- No double jump
- Coyote time: None implemented

### Collision System

**File**: `Strays.Core/Game/Level.cs`, `Strays.Core/Game/Player.cs`

#### Tile Collision Types
```csharp
TileCollision.Passable    // No collision
TileCollision.Impassable  // Solid block
TileCollision.Platform    // One-way platform (pass through from below)
```

#### Collision Resolution
1. Move player to new position
2. Get bounding rectangle
3. Find all overlapping tiles
4. For each overlapping tile:
   - Calculate penetration depth
   - Push player out in the smallest direction
5. Update `IsOnGround` flag

### Collectibles System

**File**: `Strays.Core/Game/Gem.cs`

#### Gem Types
| Color | Value | Spawn Character |
|-------|-------|-----------------|
| Green | 10 | `1` |
| Yellow | 30 | `2` |
| Red | 50 | `3` |
| Blue | 100 | `4` |

#### Gem States
```csharp
GemState.Waiting     // In level, bobbing animation
GemState.Collecting  // Flying toward UI
GemState.Collected   // Removed from game
```

#### Collection Animation
- Bouncing animation (sine wave) while waiting
- Fly toward score display when collected
- Point value added when reaching destination

### Enemy System

**File**: `Strays.Core/Game/Enemy.cs`

#### Patrol Behavior
- Walk in one direction until hitting obstacle
- Turn around and continue
- No pathfinding

#### Player Interaction
- Collision with player = player death
- No damage system (one-hit kill)
- No enemy health

### Level Format

**Files**: `Strays.Core/Content/Levels/00.txt` through `06.txt`

#### Tile Characters
```
.  = Empty space (passable)
#  = Solid platform
-  = Platform (one-way)
~  = Hazard/water
P  = Player spawn point
X  = Level exit
1  = Green gem (10 pts)
2  = Yellow gem (30 pts)
3  = Red gem (50 pts)
4  = Blue gem (100 pts)
A-D = Enemy types
G  = Ground platform tile
```

#### Level Progression
- 7 levels total (00-06)
- Completing a level loads the next
- After level 06, returns to menu

---

## Particle Effects System

**File**: `Strays.Core/Effects/ParticleManager.cs`

### Effect Types

#### Confetti
- Multi-colored particles
- Random velocities
- Medium lifetime
- No tailing

#### Explosion
- Red/orange color palette
- Radial burst pattern
- Short lifetime
- Possible tailing

#### Fireworks
- Colored initial burst
- Secondary explosions on particle death
- Long travel time
- Cascade effect

#### Sparkles
- White/light colors
- Small particles
- Short lifetime
- Subtle effect

### Particle Properties
```csharp
class Particle
{
    Vector2 Position;
    Vector2 Velocity;
    Color Color;
    float Lifetime;
    float Age;
    float Scale;
    Action OnDeath;  // Event for cascade effects
}
```

### Usage
```csharp
var particles = new ParticleManager(texture);
particles.Position = explosionPosition;
particles.Emit(ParticleEffectType.Explosion, count: 50);

// In update loop
particles.Update(gameTime);

// In draw loop
particles.Draw(spriteBatch);
```

---

## Scoring System

**File**: `Strays.Core/Screens/GameplayScreen.cs`

### Score Calculation
- Gem collection: Point value per gem type
- Time bonus: Faster completion = more points
- No combo system

### Leaderboard
- High score saved per level
- Stored via SettingsManager<StraysLeaderboard>
- Persisted between sessions

---

## Audio System

### Sound Effects
| Event | Sound File |
|-------|------------|
| Jump | `PlayerJumped.wav` |
| Gem collected | `PlayerGemCollected.wav` |
| Player death | `PlayerKilled.wav` |
| Enemy death | `EnemyKilled.wav` |
| Level complete | `ExitReached.wav` |

### Music
- Background music loops per level
- Location: `Content/Sounds/Music/`

### Implementation
```csharp
// Sound effects via SoundEffect.Play()
gemSound = Content.Load<SoundEffect>("Sounds/PlayerGemCollected");
gemSound.Play();

// Music via MediaPlayer (for looping)
MediaPlayer.Play(levelMusic);
MediaPlayer.IsRepeating = true;
```

---

## Save System

### Settings Data
```csharp
class StraysSettings
{
    float MusicVolume;      // 0.0 - 1.0
    float SfxVolume;        // 0.0 - 1.0
    string Language;        // Culture code
    bool Fullscreen;
}
```

### Leaderboard Data
```csharp
class StraysLeaderboard
{
    Dictionary<int, int> HighScores;  // level → score
    Dictionary<int, float> BestTimes; // level → time
}
```

### Storage Locations
- **Desktop**: Application data folder (JSON files)
- **Mobile**: Platform-specific storage APIs
