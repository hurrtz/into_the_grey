using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Entities;

namespace Lazarus.Core.Game.Dungeons;

/// <summary>
/// State of an enemy in an explorable dungeon room.
/// </summary>
public class DungeonEnemy
{
    /// <summary>
    /// Unique ID for this enemy instance.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The Stray definition ID for this enemy.
    /// </summary>
    public string DefinitionId { get; init; } = "";

    /// <summary>
    /// Enemy level.
    /// </summary>
    public int Level { get; init; } = 1;

    /// <summary>
    /// Current position in world coordinates.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Whether this enemy has been defeated.
    /// </summary>
    public bool IsDefeated { get; set; }

    /// <summary>
    /// Collision bounds for interaction.
    /// </summary>
    public Rectangle Bounds => new(
        (int)Position.X - 16,
        (int)Position.Y - 16,
        32, 32);

    /// <summary>
    /// Display name for the enemy.
    /// </summary>
    public string DisplayName
    {
        get
        {
            var def = StrayDefinitions.Get(DefinitionId);
            return def?.Name ?? FormatName(DefinitionId);
        }
    }

    /// <summary>
    /// Placeholder color for rendering.
    /// </summary>
    public Color PlaceholderColor
    {
        get
        {
            var def = StrayDefinitions.Get(DefinitionId);
            return def?.PlaceholderColor ?? Color.Red;
        }
    }

    private static string FormatName(string id)
    {
        var words = id.Split('_');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
                words[i] = char.ToUpper(words[i][0]) + words[i][1..];
        }
        return string.Join(" ", words);
    }
}

/// <summary>
/// State of a door/exit in an explorable room.
/// </summary>
public class RoomDoor
{
    /// <summary>
    /// Position of the door in world coordinates.
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// Whether the door is currently locked.
    /// </summary>
    public bool IsLocked { get; set; } = true;

    /// <summary>
    /// Target room index (-1 for dungeon exit).
    /// </summary>
    public int TargetRoomIndex { get; init; }

    /// <summary>
    /// Collision bounds for interaction.
    /// </summary>
    public Rectangle Bounds => new(
        (int)Position.X - 24,
        (int)Position.Y - 24,
        48, 48);
}

/// <summary>
/// An explorable dungeon room with enemies, doors, and traversable space.
/// </summary>
public class ExplorableRoom
{
    private static int _nextEnemyId = 1;

    /// <summary>
    /// Room index in the dungeon.
    /// </summary>
    public int RoomIndex { get; init; }

    /// <summary>
    /// The underlying room data.
    /// </summary>
    public DungeonRoom RoomData { get; init; } = null!;

    /// <summary>
    /// Generated layout with tile data.
    /// </summary>
    public GeneratedRoomLayout Layout { get; init; } = null!;

    /// <summary>
    /// Size of each tile in pixels.
    /// </summary>
    public const int TileSize = 32;

    /// <summary>
    /// Room width in pixels.
    /// </summary>
    public int WidthPixels => Layout.Width * TileSize;

    /// <summary>
    /// Room height in pixels.
    /// </summary>
    public int HeightPixels => Layout.Height * TileSize;

    /// <summary>
    /// Player spawn position.
    /// </summary>
    public Vector2 PlayerSpawn { get; set; }

    /// <summary>
    /// Enemies in this room.
    /// </summary>
    public List<DungeonEnemy> Enemies { get; } = new();

    /// <summary>
    /// Doors/exits in this room.
    /// </summary>
    public List<RoomDoor> Doors { get; } = new();

    /// <summary>
    /// Whether all enemies have been defeated.
    /// </summary>
    public bool IsCleared => Enemies.TrueForAll(e => e.IsDefeated);

    /// <summary>
    /// Number of remaining enemies.
    /// </summary>
    public int RemainingEnemies => Enemies.Count(e => !e.IsDefeated);

    /// <summary>
    /// Creates an explorable room from dungeon room data.
    /// </summary>
    public static ExplorableRoom Create(DungeonRoom roomData, int roomIndex, int totalRooms)
    {
        // Generate layout based on room type (RoomGenerator handles the layout type internally)
        var layout = RoomGenerator.GenerateRoom(roomData.Type, roomData.EnemyIds.Count);

        var room = new ExplorableRoom
        {
            RoomIndex = roomIndex,
            RoomData = roomData,
            Layout = layout,
            PlayerSpawn = new Vector2(
                layout.EntryPoint.X * TileSize + TileSize / 2,
                layout.EntryPoint.Y * TileSize + TileSize / 2)
        };

        // Place enemies at spawn points
        for (int i = 0; i < roomData.EnemyIds.Count && i < layout.EnemySpawns.Count; i++)
        {
            var spawnPoint = layout.EnemySpawns[i];
            var level = i < roomData.EnemyLevels.Count ? roomData.EnemyLevels[i] : 5;

            room.Enemies.Add(new DungeonEnemy
            {
                Id = _nextEnemyId++,
                DefinitionId = roomData.EnemyIds[i],
                Level = level,
                Position = new Vector2(
                    spawnPoint.X * TileSize + TileSize / 2,
                    spawnPoint.Y * TileSize + TileSize / 2)
            });
        }

        // Add any remaining enemies at random floor tiles
        for (int i = layout.EnemySpawns.Count; i < roomData.EnemyIds.Count; i++)
        {
            var pos = FindRandomFloorPosition(layout);
            var level = i < roomData.EnemyLevels.Count ? roomData.EnemyLevels[i] : 5;

            room.Enemies.Add(new DungeonEnemy
            {
                Id = _nextEnemyId++,
                DefinitionId = roomData.EnemyIds[i],
                Level = level,
                Position = pos
            });
        }

        // Create door to next room (or exit)
        var exitPos = new Vector2(
            layout.ExitPoint.X * TileSize + TileSize / 2,
            layout.ExitPoint.Y * TileSize + TileSize / 2);

        room.Doors.Add(new RoomDoor
        {
            Position = exitPos,
            IsLocked = roomData.EnemyIds.Count > 0, // Lock if there are enemies
            TargetRoomIndex = roomIndex < totalRooms - 1 ? roomIndex + 1 : -1
        });

        return room;
    }

    private static Vector2 FindRandomFloorPosition(GeneratedRoomLayout layout)
    {
        for (int attempts = 0; attempts < 100; attempts++)
        {
            int x = Random.Shared.Next(1, layout.Width - 1);
            int y = Random.Shared.Next(1, layout.Height - 1);

            if (layout.Tiles[y, x] == RoomTile.Floor)
            {
                return new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
            }
        }

        // Fallback to center
        return new Vector2(layout.Width * TileSize / 2, layout.Height * TileSize / 2);
    }

    /// <summary>
    /// Checks if a position is walkable.
    /// </summary>
    public bool IsWalkable(Vector2 position)
    {
        int tileX = (int)(position.X / TileSize);
        int tileY = (int)(position.Y / TileSize);

        if (tileX < 0 || tileX >= Layout.Width || tileY < 0 || tileY >= Layout.Height)
            return false;

        var tile = Layout.Tiles[tileY, tileX];
        return tile == RoomTile.Floor ||
               tile == RoomTile.Entry ||
               tile == RoomTile.Exit ||
               tile == RoomTile.EnemySpawn ||
               tile == RoomTile.Cover ||
               tile == RoomTile.HealingStation;
    }

    /// <summary>
    /// Checks if a rectangle collides with walls.
    /// </summary>
    public bool CollidesWithWalls(Rectangle bounds)
    {
        // Check all corners and center
        var points = new[]
        {
            new Vector2(bounds.Left, bounds.Top),
            new Vector2(bounds.Right, bounds.Top),
            new Vector2(bounds.Left, bounds.Bottom),
            new Vector2(bounds.Right, bounds.Bottom),
            new Vector2(bounds.Center.X, bounds.Center.Y)
        };

        foreach (var point in points)
        {
            if (!IsWalkable(point))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the enemy at a position, if any.
    /// </summary>
    public DungeonEnemy? GetEnemyAt(Rectangle bounds)
    {
        foreach (var enemy in Enemies)
        {
            if (!enemy.IsDefeated && enemy.Bounds.Intersects(bounds))
                return enemy;
        }
        return null;
    }

    /// <summary>
    /// Gets the door at a position, if any.
    /// </summary>
    public RoomDoor? GetDoorAt(Rectangle bounds)
    {
        foreach (var door in Doors)
        {
            if (door.Bounds.Intersects(bounds))
                return door;
        }
        return null;
    }

    /// <summary>
    /// Updates door lock states based on cleared enemies.
    /// </summary>
    public void UpdateDoors()
    {
        if (IsCleared)
        {
            foreach (var door in Doors)
            {
                door.IsLocked = false;
            }
        }
    }

    /// <summary>
    /// Marks an enemy as defeated.
    /// </summary>
    public void DefeatEnemy(int enemyId)
    {
        var enemy = Enemies.FirstOrDefault(e => e.Id == enemyId);
        if (enemy != null)
        {
            enemy.IsDefeated = true;
            UpdateDoors();
        }
    }

    /// <summary>
    /// Gets all enemies for a combat encounter (supports groups).
    /// </summary>
    public List<DungeonEnemy> GetCombatGroup(DungeonEnemy triggered)
    {
        // For now, return just the triggered enemy
        // Could be extended to include nearby enemies for group battles
        return new List<DungeonEnemy> { triggered };
    }
}
