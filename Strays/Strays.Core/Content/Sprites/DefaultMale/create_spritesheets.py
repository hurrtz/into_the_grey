#!/usr/bin/env python3
"""
Combines individual animation frames into sprite sheets for the DefaultMale character.

Sprite sheet format:
- 8 rows (one per direction): South, SouthEast, East, NorthEast, North, NorthWest, West, SouthWest
- N columns (one per frame)
- Each frame is 104x104 pixels

Usage:
    python create_spritesheets.py

Output:
    spritesheets/idle.png  - breathing-idle animation
    spritesheets/walk.png  - walk animation
    spritesheets/run.png   - running-6-frames animation
"""

import os
from PIL import Image

# Frame size
FRAME_WIDTH = 104
FRAME_HEIGHT = 104

# Direction order (must match the C# DirectionalAnimation.DirectionOrder)
DIRECTIONS = [
    "south",
    "south-east",
    "east",
    "north-east",
    "north",
    "north-west",
    "west",
    "south-west"
]

# Animation mappings: output name -> source folder name
ANIMATIONS = {
    "idle": "breathing-idle",
    "walk": "walk",
    "run": "running-6-frames"
}

def get_frame_files(animation_path, direction):
    """Get sorted list of frame files for a direction."""
    dir_path = os.path.join(animation_path, direction)
    if not os.path.exists(dir_path):
        print(f"  Warning: Directory not found: {dir_path}")
        return []

    files = [f for f in os.listdir(dir_path) if f.startswith("frame_") and f.endswith(".png")]
    files.sort()
    return [os.path.join(dir_path, f) for f in files]

def create_spritesheet(animation_name, source_folder, output_path):
    """Create a sprite sheet from individual frame files."""
    animations_path = os.path.join(os.path.dirname(__file__), "animations")
    animation_path = os.path.join(animations_path, source_folder)

    if not os.path.exists(animation_path):
        print(f"Animation folder not found: {animation_path}")
        return False

    print(f"\nCreating {animation_name}.png from {source_folder}...")

    # Find the maximum number of frames across all directions
    max_frames = 0
    direction_frames = {}

    for direction in DIRECTIONS:
        frames = get_frame_files(animation_path, direction)
        direction_frames[direction] = frames
        max_frames = max(max_frames, len(frames))
        print(f"  {direction}: {len(frames)} frames")

    if max_frames == 0:
        print(f"  No frames found for {animation_name}")
        return False

    # Create the sprite sheet
    sheet_width = max_frames * FRAME_WIDTH
    sheet_height = len(DIRECTIONS) * FRAME_HEIGHT

    print(f"  Creating {sheet_width}x{sheet_height} sprite sheet ({max_frames} frames x 8 directions)")

    spritesheet = Image.new("RGBA", (sheet_width, sheet_height), (0, 0, 0, 0))

    for row, direction in enumerate(DIRECTIONS):
        frames = direction_frames[direction]
        for col, frame_path in enumerate(frames):
            try:
                frame = Image.open(frame_path)
                x = col * FRAME_WIDTH
                y = row * FRAME_HEIGHT
                spritesheet.paste(frame, (x, y))
            except Exception as e:
                print(f"  Error loading {frame_path}: {e}")

    # Save the sprite sheet
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    spritesheet.save(output_path, "PNG")
    print(f"  Saved: {output_path}")

    return True

def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_dir = os.path.join(script_dir, "spritesheets")

    print("=" * 60)
    print("DefaultMale Sprite Sheet Generator")
    print("=" * 60)
    print(f"Script directory: {script_dir}")
    print(f"Output directory: {output_dir}")

    success_count = 0
    for output_name, source_folder in ANIMATIONS.items():
        output_path = os.path.join(output_dir, f"{output_name}.png")
        if create_spritesheet(output_name, source_folder, output_path):
            success_count += 1

    print("\n" + "=" * 60)
    print(f"Done! Created {success_count}/{len(ANIMATIONS)} sprite sheets.")
    print(f"Sprite sheets saved to: {output_dir}")
    print("=" * 60)

if __name__ == "__main__":
    main()
