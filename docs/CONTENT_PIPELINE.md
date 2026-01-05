# Strays Content Pipeline Documentation

## Overview

Strays uses two content loading mechanisms:
1. **MonoGame Content Builder (MGCB)** - For processed assets (fonts, sounds)
2. **Direct file loading** - For sprites and Tiled maps (runtime loading)

## Directory Structure

```
Strays.Core/Content/
├── Strays.mgcb              # MonoGame content project file
├── Sprites/
│   ├── DefaultMale/         # Top-down player character
│   │   ├── animations/
│   │   │   ├── breathing-idle/
│   │   │   │   ├── south/frame_0.png, frame_1.png, ...
│   │   │   │   ├── south-east/
│   │   │   │   ├── east/
│   │   │   │   ├── north-east/
│   │   │   │   ├── north/
│   │   │   │   ├── north-west/
│   │   │   │   ├── west/
│   │   │   │   └── south-west/
│   │   │   ├── walk/[8 directions]/
│   │   │   └── running-6-frames/[8 directions]/
│   │   └── metadata.json
│   ├── Player/              # Classic platformer sprites
│   ├── Gem.png
│   ├── gradient.png
│   └── blank.png
├── Tiles/                   # Tileset images
├── Backgrounds/             # Menu and level backgrounds
├── Fonts/
│   └── Hud.spritefont      # Processed by MGCB
├── Sounds/
│   ├── Music/
│   ├── PlayerGemCollected.wav
│   ├── PlayerKilled.wav
│   └── ...
├── Levels/
│   ├── 00.txt through 06.txt
└── Overlays/

Tiled/                       # External map editor files
├── biomes/
│   └── suburb.tmx
└── tilesets/
    └── sidewalk_64.tsx
```

## MonoGame Content Builder

### Configuration File: `Strays.mgcb`

Location: `Strays.Core/Content/Strays.mgcb`

This file defines which assets are processed by the MonoGame content pipeline. Assets processed here are converted to `.xnb` format.

### Adding MGCB Assets

1. Open `Strays.mgcb` in MGCB Editor (or text editor)
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
- Large sprite sheets with many variations
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
// Content can be in different locations depending on build
string[] searchPaths = {
    "Content",
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content"),
    Path.Combine(Directory.GetCurrentDirectory(), "Content")
};

foreach (var path in searchPaths)
{
    if (Directory.Exists(path))
        return path;
}
```

## Sprite Organization

### Classic Platformer Sprites
Single sprite sheet with animation frames in a row:
```
Player_Idle.png     → [frame0][frame1][frame2]...
Player_Run.png      → [frame0][frame1][frame2][frame3]...
Player_Jump.png     → [frame0][frame1]...
```

Loading:
```csharp
var animation = new Animation(texture, frameWidth: 64, frameCount: 8, frameTime: 0.1f, isLooping: true);
```

### Top-Down 8-Direction Sprites
Individual frame files per direction:
```
animations/walk/south/frame_0.png
animations/walk/south/frame_1.png
animations/walk/south-east/frame_0.png
...
```

Loading:
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

## Tiled Map Integration

### Map Files (.tmx)
Location: `Tiled/biomes/`

XML format with layer and tileset references:
```xml
<map width="50" height="50" tilewidth="64" tileheight="64">
    <tileset firstgid="1" source="../tilesets/sidewalk_64.tsx"/>
    <layer name="Ground">
        <data encoding="csv">1,2,3,...</data>
    </layer>
</map>
```

### Tileset Files (.tsx)
Location: `Tiled/tilesets/`

```xml
<tileset name="sidewalk" tilewidth="64" tileheight="64">
    <image source="sidewalk_64.png"/>
</tileset>
```

### Loading Maps
```csharp
var map = TiledMap.Load("Tiled/biomes/suburb.tmx");
```

### Tileset Image Location
Images are resolved relative to the `.tsx` file location.

## Level Files

### Format
Plain text files with character-based tile definitions.

Location: `Strays.Core/Content/Levels/`

### Example Level
```
................
......XXXX......
......####......
....##....##....
.P..........1...
################
```

### Tile Characters
```
. = Empty (passable)
# = Solid platform
- = One-way platform
P = Player spawn
X = Exit
1-4 = Gems (different values)
A-D = Enemies
```

## Audio Assets

### Sound Effects
Format: `.wav` (uncompressed)
Location: `Content/Sounds/`

Loaded via MGCB pipeline:
```csharp
var sound = Content.Load<SoundEffect>("Sounds/PlayerJumped");
sound.Play();
```

### Music
Format: `.mp3` or `.ogg`
Location: `Content/Sounds/Music/`

Played via MediaPlayer:
```csharp
var song = Content.Load<Song>("Sounds/Music/Level1");
MediaPlayer.Play(song);
MediaPlayer.IsRepeating = true;
```

## Font Assets

### SpriteFont Definition
Location: `Content/Fonts/Hud.spritefont`

XML definition processed by MGCB:
```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:Graphics="Microsoft.Xna.Framework.Content.Pipeline.Graphics">
  <Asset Type="Graphics:FontDescription">
    <FontName>Arial</FontName>
    <Size>14</Size>
    <Spacing>0</Spacing>
    <Style>Regular</Style>
  </Asset>
</XnaContent>
```

## Build Output

After building, processed content is placed in:
```
Strays.DesktopGL/bin/Debug/net9.0/Content/
├── Fonts/Hud.xnb
├── Sounds/PlayerJumped.xnb
└── ...
```

Non-MGCB content is copied directly to the output folder via project configuration.

## Adding New Assets

### New MGCB Asset
1. Place file in `Content/` subfolder
2. Add to `Strays.mgcb`
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
├── south/frame_0.png, frame_1.png, ...
├── south-east/...
├── east/...
├── north-east/...
├── north/...
├── north-west/...
├── west/...
└── south-west/...
```
2. Load via `DirectionalAnimation.LoadFromFiles()`

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
   - Use lowercase with hyphens for folders
   - Frame files: `frame_0.png`, `frame_1.png`, etc.
   - Consistent naming across directions

4. **Resolution**:
   - Base game resolution: 800x480
   - Scale sprites appropriately
   - Tile sizes: typically 32x32 or 64x64
