# Lazarus Content Pipeline Documentation

## Overview

Lazarus uses two content loading mechanisms:
1. **MonoGame Content Builder (MGCB)** - For processed assets (fonts, sounds)
2. **Direct file loading** - For sprites and Tiled maps (runtime loading)

## Directory Structure

```
Lazarus/Lazarus.Core/Content/
├── Lazarus.mgcb               # MonoGame content project file
├── Sprites/
│   ├── DefaultMale/           # Protagonist character
│   │   ├── animations/
│   │   │   ├── breathing-idle/
│   │   │   │   ├── south/frame_000.png, frame_001.png, ...
│   │   │   │   ├── south-east/
│   │   │   │   ├── east/
│   │   │   │   ├── north-east/
│   │   │   │   ├── north/
│   │   │   │   ├── north-west/
│   │   │   │   ├── west/
│   │   │   │   └── south-west/
│   │   │   ├── walk/[8 directions]/
│   │   │   └── running-6-frames/[8 directions]/
│   │   ├── rotations/         # Static directional sprites
│   │   ├── spritesheets/      # Combined sprite sheets
│   │   └── metadata.json
│   ├── VirtualControlArrow.png  # Touch controls
│   ├── blank.png              # 1x1 white pixel (primitives)
│   └── gradient.png           # UI gradient texture
├── Fonts/
│   ├── GameFont.spritefont    # Main game font
│   ├── Hud.spritefont         # HUD font
│   ├── MenuFont.spritefont    # Menu font
│   └── Roboto-Bold.ttf        # Source font file
├── Sounds/
│   └── Music.mp3              # Background music
├── Icon.bmp                   # Application icon (bitmap)
├── Icon.ico                   # Application icon (Windows)
├── icon-1024.png              # High-res source icon
├── splash.png                 # Splash screen image
├── android-icons-generator.sh # Icon generation script
├── ios-icons-generator.sh     # Icon generation script
└── mac-icons-generator.sh     # Icon generation script

Tiled/                         # External map editor files
├── biomes/
│   └── suburb.tmx             # Biome map files
└── tilesets/
    └── *.tsx                  # Tileset definitions
```

## MonoGame Content Builder

### Configuration File: `Lazarus.mgcb`

Location: `Lazarus/Lazarus.Core/Content/Lazarus.mgcb`

This file defines which assets are processed by the MonoGame content pipeline. Assets processed here are converted to `.xnb` format.

### Adding MGCB Assets

1. Open `Lazarus.mgcb` in MGCB Editor (or text editor)
2. Add asset reference:
```
#begin Fonts/Hud.spritefont
/importer:FontDescriptionImporter
/processor:FontDescriptionProcessor
/build:Fonts/Hud.spritefont
```
3. Build project to process assets

### Loading MGCB Assets
```csharp
// In LoadContent()
var font = Content.Load<SpriteFont>("Fonts/Hud");
var sound = Content.Load<SoundEffect>("Sounds/PlayerJumped");
var music = Content.Load<Song>("Sounds/Music");
```

### Asset Types via MGCB
| Type | Importer | Processor | Output |
|------|----------|-----------|--------|
| `.spritefont` | FontDescriptionImporter | FontDescriptionProcessor | SpriteFont |
| `.wav` | WavImporter | SoundEffectProcessor | SoundEffect |
| `.mp3` | Mp3Importer | SongProcessor | Song |
| `.png/.jpg` | TextureImporter | TextureProcessor | Texture2D |

## Direct File Loading

### When to Use
- Assets that change frequently during development
- Large sprite collections with many variations
- Tiled map files
- Assets loaded conditionally at runtime

### Texture Loading
```csharp
// From file stream
using var stream = File.OpenRead(fullPath);
var texture = Texture2D.FromStream(graphicsDevice, stream);

// Used in DirectionalAnimation.LoadFromFiles()
```

### Finding Content Path
```csharp
// Content location at runtime
string contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");

// Alternative search paths
string[] searchPaths = {
    "Content",
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content"),
    Path.Combine(Directory.GetCurrentDirectory(), "Content")
};
```

## Sprite Organization

### Protagonist Sprites (8-Direction)

Individual frame files per direction:
```
Sprites/DefaultMale/animations/
├── breathing-idle/
│   ├── south/frame_000.png, frame_001.png, frame_002.png, frame_003.png
│   ├── south-east/frame_000.png, ...
│   ├── east/frame_000.png, ...
│   ├── north-east/frame_000.png, ...
│   ├── north/frame_000.png, ...
│   ├── north-west/frame_000.png, ...
│   ├── west/frame_000.png, ...
│   └── south-west/frame_000.png, ...
├── walk/
│   └── [same 8-direction structure, 6 frames each]
└── running-6-frames/
    └── [same 8-direction structure, 6 frames each]
```

### Loading Directional Animations
```csharp
var animation = DirectionalAnimation.LoadFromFiles(
    graphicsDevice,
    basePath: contentPath,
    animationName: "walk",
    frameTime: 0.15f,
    isLooping: true
);
```

### Direction Folder Names
| Direction | Folder Name |
|-----------|-------------|
| South | `south` |
| SouthEast | `south-east` |
| East | `east` |
| NorthEast | `north-east` |
| North | `north` |
| NorthWest | `north-west` |
| West | `west` |
| SouthWest | `south-west` |

### Pre-combined Sprite Sheets
Located in `Sprites/DefaultMale/spritesheets/`:
- `idle.png` - Idle animation sheet
- `run.png` - Running animation sheet
- `walk.png` - Walking animation sheet

### UI and System Sprites
| File | Purpose |
|------|---------|
| `blank.png` | 1x1 white pixel for drawing primitives (rectangles, lines) |
| `gradient.png` | Gradient texture for UI backgrounds |
| `VirtualControlArrow.png` | Touch screen virtual gamepad arrows |

## Stray Visuals (Placeholder System)

Currently, Strays are rendered as colored shapes using the primitive rendering system:

```csharp
// In Stray.Draw()
var color = IsHostile ? Color.Red : Definition.PlaceholderColor;
var size = Definition.PlaceholderSize;

spriteBatch.Draw(pixelTexture, drawRect, color);
```

### Future Stray Sprite Organization (Planned)
```
Sprites/Strays/
├── [creature_id]/
│   ├── idle/[directions]/
│   ├── attack/[directions]/
│   ├── hurt/
│   └── portrait.png
```

## Tiled Map Integration

### Map Files (.tmx)
Location: `Tiled/biomes/`

XML format with layer and tileset references:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<map version="1.10" width="50" height="50" tilewidth="64" tileheight="64">
    <tileset firstgid="1" source="../tilesets/sidewalk_64.tsx"/>
    <layer name="Ground" width="50" height="50">
        <data encoding="csv">
            1,2,3,4,5,...
        </data>
    </layer>
    <layer name="Collision" width="50" height="50">
        <data encoding="csv">...</data>
    </layer>
    <objectgroup name="Spawns">
        <object id="1" name="player_spawn" x="100" y="200"/>
        <object id="2" name="npc_spawn" x="300" y="400"/>
    </objectgroup>
</map>
```

### Tileset Files (.tsx)
Location: `Tiled/tilesets/`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<tileset name="sidewalk" tilewidth="64" tileheight="64" tilecount="16" columns="4">
    <image source="sidewalk_64.png" width="256" height="256"/>
    <tile id="0">
        <properties>
            <property name="collision" value="false"/>
        </properties>
    </tile>
</tileset>
```

### Loading Maps
```csharp
var map = TiledMap.Load("Tiled/biomes/suburb.tmx");
```

### Supported Tiled Features
- Multiple tile layers
- External tileset references (.tsx files)
- CSV-encoded tile data
- Object layers (for spawn points, triggers)
- Tile properties (for collision, etc.)
- Viewport culling (only visible tiles rendered)

### Tileset Image Location
Images are resolved relative to the `.tsx` file location.

## Audio Assets

### Sound Effects
Format: `.wav` (uncompressed for low latency)
Location: `Content/Sounds/`

```csharp
var sound = Content.Load<SoundEffect>("Sounds/ButtonClick");
sound.Play();

// With parameters
sound.Play(volume: 0.8f, pitch: 0f, pan: 0f);
```

### Music
Format: `.mp3` or `.ogg`
Location: `Content/Sounds/`

```csharp
var song = Content.Load<Song>("Sounds/Music");
MediaPlayer.Play(song);
MediaPlayer.IsRepeating = true;
MediaPlayer.Volume = settings.MusicVolume;
```

### Audio Manager
The `AudioManager` class centralizes audio control:
```csharp
// Via AudioManager
AudioManager.PlayMusic("Music");
AudioManager.PlaySfx("ButtonClick");
AudioManager.SetMusicVolume(0.5f);
AudioManager.SetSfxVolume(0.8f);
```

## Font Assets

### SpriteFont Definition
Location: `Content/Fonts/*.spritefont`

XML definition processed by MGCB:
```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:Graphics="Microsoft.Xna.Framework.Content.Pipeline.Graphics">
  <Asset Type="Graphics:FontDescription">
    <FontName>Arial</FontName>
    <Size>14</Size>
    <Spacing>0</Spacing>
    <Style>Regular</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
      <!-- Extended characters for localization -->
      <CharacterRegion>
        <Start>&#192;</Start>
        <End>&#255;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>
```

### Available Fonts
| Font | Size | Usage |
|------|------|-------|
| `GameFont` | Variable | General game text |
| `Hud` | 14 | HUD elements |
| `MenuFont` | Variable | Menu text |

## Icon Generation

### Platform-Specific Icons
Scripts in `Content/` generate icons for each platform:

```bash
# Android icons (multiple densities)
./android-icons-generator.sh icon-1024.png

# iOS icons (multiple sizes)
./ios-icons-generator.sh icon-1024.png

# macOS icons (.icns)
./mac-icons-generator.sh icon-1024.png
```

### Icon Sizes Generated
| Platform | Sizes |
|----------|-------|
| Android | 36, 48, 72, 96, 144, 192 px |
| iOS | 20, 29, 40, 58, 60, 76, 80, 87, 120, 152, 167, 180, 1024 px |
| macOS | 16, 32, 64, 128, 256, 512, 1024 px |

## Build Output

After building, processed content is placed in:
```
Lazarus.DesktopGL/bin/Debug/net9.0/Content/
├── Fonts/
│   ├── GameFont.xnb
│   ├── Hud.xnb
│   └── MenuFont.xnb
├── Sounds/
│   └── Music.xnb
└── Sprites/
    └── blank.xnb
```

Non-MGCB content is copied directly to the output folder via project configuration.

## Adding New Assets

### New MGCB Asset
1. Place file in `Content/` subfolder
2. Add to `Lazarus.mgcb`
3. Build project
4. Load via `Content.Load<T>()`

### New Direct-Load Asset
1. Place file in `Content/` subfolder
2. Ensure `.csproj` copies it to output:
```xml
<ItemGroup>
  <Content Include="Content\Sprites\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```
3. Load via `Texture2D.FromStream()` or file I/O

### New Tiled Map
1. Create `.tmx` in `Tiled/biomes/`
2. Reference tilesets from `Tiled/tilesets/`
3. Place tileset images alongside `.tsx` files
4. Load via `TiledMap.Load(path)`

### New Animation Set (8-Direction)
1. Create folder structure:
```
Content/Sprites/[Character]/animations/[animation_name]/
├── south/frame_000.png, frame_001.png, ...
├── south-east/...
├── east/...
├── north-east/...
├── north/...
├── north-west/...
├── west/...
└── south-west/...
```
2. Load via `DirectionalAnimation.LoadFromFiles()`

### New Stray Sprites (Future)
1. Create creature folder in `Content/Sprites/Strays/[creature_id]/`
2. Add animation folders following direction conventions
3. Add `portrait.png` for UI displays
4. Register in StrayDefinition with sprite path

## Localization Assets

String resources are in `Localization/` folder (not Content):
```
Lazarus.Core/Localization/
├── Resources.resx           # English (default)
├── Resources.de-DE.resx     # German
├── Resources.es-ES.resx     # Spanish
├── Resources.fr-FR.resx     # French
├── Resources.ja-JP.resx     # Japanese
└── Resources.Designer.cs    # Auto-generated accessor
```

These are compiled into the assembly, not the Content folder.

## Best Practices

1. **Use MGCB for**:
   - Fonts (required)
   - Sound effects and music
   - Assets that benefit from compression

2. **Use direct loading for**:
   - Frequently changing development assets
   - Large sprite collections
   - Tiled maps
   - Conditional/optional content

3. **File naming**:
   - Use lowercase with hyphens for folders (`south-east`, not `SouthEast`)
   - Frame files: `frame_000.png`, `frame_001.png`, etc. (3-digit padding)
   - Consistent naming across directions

4. **Resolution**:
   - Base game resolution: 800x480
   - Desktop default: 1280x800
   - Scale sprites appropriately
   - Tile sizes: typically 64x64

5. **Sprite optimization**:
   - Use sprite sheets for related animations when possible
   - Keep individual frames for 8-directional animations
   - Use power-of-two dimensions for textures (64, 128, 256, 512)

6. **Audio optimization**:
   - Use `.wav` for short sound effects (fast loading)
   - Use `.mp3` or `.ogg` for music (compressed)
   - Keep sound effects under 1 second when possible
