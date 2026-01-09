using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Strays.Core.Game.Dungeons;

/// <summary>
/// Room layout template type.
/// </summary>
public enum RoomLayoutType
{
    /// <summary>
    /// Simple open room.
    /// </summary>
    Open,

    /// <summary>
    /// Room with pillars/obstacles.
    /// </summary>
    Pillared,

    /// <summary>
    /// L-shaped corridor room.
    /// </summary>
    LShape,

    /// <summary>
    /// Room with central obstacle.
    /// </summary>
    Central,

    /// <summary>
    /// Narrow corridor.
    /// </summary>
    Corridor,

    /// <summary>
    /// Large arena.
    /// </summary>
    Arena,

    /// <summary>
    /// Boss arena - extra large.
    /// </summary>
    BossArena
}

/// <summary>
/// A tile in a dungeon room layout.
/// </summary>
public enum RoomTile
{
    /// <summary>
    /// Walkable floor.
    /// </summary>
    Floor,

    /// <summary>
    /// Impassable wall.
    /// </summary>
    Wall,

    /// <summary>
    /// Entry point for player.
    /// </summary>
    Entry,

    /// <summary>
    /// Exit point to next room.
    /// </summary>
    Exit,

    /// <summary>
    /// Enemy spawn point.
    /// </summary>
    EnemySpawn,

    /// <summary>
    /// Treasure/loot location.
    /// </summary>
    Treasure,

    /// <summary>
    /// Hazard tile (damage on contact).
    /// </summary>
    Hazard,

    /// <summary>
    /// Cover position (defensive bonus).
    /// </summary>
    Cover,

    /// <summary>
    /// Healing station.
    /// </summary>
    HealingStation
}

/// <summary>
/// Generated room layout with tile data.
/// </summary>
public class RoomLayout
{
    /// <summary>
    /// Layout type.
    /// </summary>
    public RoomLayoutType Type { get; init; }

    /// <summary>
    /// Room width in tiles.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Room height in tiles.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Tile data (row-major order).
    /// </summary>
    public RoomTile[,] Tiles { get; init; } = new RoomTile[0, 0];

    /// <summary>
    /// Player entry position.
    /// </summary>
    public Point EntryPoint { get; set; }

    /// <summary>
    /// Exit position.
    /// </summary>
    public Point ExitPoint { get; set; }

    /// <summary>
    /// Enemy spawn positions.
    /// </summary>
    public List<Point> EnemySpawns { get; } = new();

    /// <summary>
    /// Treasure positions.
    /// </summary>
    public List<Point> TreasureLocations { get; } = new();

    /// <summary>
    /// Cover positions.
    /// </summary>
    public List<Point> CoverPositions { get; } = new();

    /// <summary>
    /// Gets tile at position.
    /// </summary>
    public RoomTile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return RoomTile.Wall;
        }

        return Tiles[y, x];
    }

    /// <summary>
    /// Gets tile at position.
    /// </summary>
    public RoomTile GetTile(Point position) => GetTile(position.X, position.Y);

    /// <summary>
    /// Checks if a position is walkable.
    /// </summary>
    public bool IsWalkable(int x, int y)
    {
        var tile = GetTile(x, y);
        return tile != RoomTile.Wall;
    }

    /// <summary>
    /// Checks if a position is walkable.
    /// </summary>
    public bool IsWalkable(Point position) => IsWalkable(position.X, position.Y);

    /// <summary>
    /// Gets world position for a tile (centered).
    /// </summary>
    public Vector2 GetWorldPosition(Point tile, int tileSize = 32)
    {
        return new Vector2(
            tile.X * tileSize + tileSize / 2f,
            tile.Y * tileSize + tileSize / 2f
        );
    }
}

/// <summary>
/// Room template for procedural generation.
/// </summary>
public class RoomTemplate
{
    /// <summary>
    /// Layout type.
    /// </summary>
    public RoomLayoutType Type { get; init; }

    /// <summary>
    /// Template pattern (string representation).
    /// </summary>
    public string[] Pattern { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Legend for pattern characters.
    /// </summary>
    public Dictionary<char, RoomTile> Legend { get; init; } = new();

    /// <summary>
    /// Default template legend.
    /// </summary>
    public static readonly Dictionary<char, RoomTile> DefaultLegend = new()
    {
        { '.', RoomTile.Floor },
        { '#', RoomTile.Wall },
        { 'E', RoomTile.Entry },
        { 'X', RoomTile.Exit },
        { 'S', RoomTile.EnemySpawn },
        { 'T', RoomTile.Treasure },
        { 'H', RoomTile.Hazard },
        { 'C', RoomTile.Cover },
        { '+', RoomTile.HealingStation }
    };
}

/// <summary>
/// Procedural dungeon room generator.
/// </summary>
public static class RoomGenerator
{
    private static readonly Random _random = new();

    /// <summary>
    /// Pre-defined room templates.
    /// </summary>
    private static readonly Dictionary<RoomLayoutType, List<RoomTemplate>> _templates = new()
    {
        {
            RoomLayoutType.Open, new List<RoomTemplate>
            {
                new()
                {
                    Type = RoomLayoutType.Open,
                    Pattern = new[]
                    {
                        "###########",
                        "#.........#",
                        "#.........#",
                        "#....S....#",
                        "E...S.S...X",
                        "#....S....#",
                        "#.........#",
                        "#.........#",
                        "###########"
                    }
                },
                new()
                {
                    Type = RoomLayoutType.Open,
                    Pattern = new[]
                    {
                        "#############",
                        "#...........#",
                        "#...S...S...#",
                        "#...........#",
                        "E...........X",
                        "#...........#",
                        "#...S...S...#",
                        "#...........#",
                        "#############"
                    }
                }
            }
        },
        {
            RoomLayoutType.Pillared, new List<RoomTemplate>
            {
                new()
                {
                    Type = RoomLayoutType.Pillared,
                    Pattern = new[]
                    {
                        "###############",
                        "#.............#",
                        "#..C.......C..#",
                        "#..#...S...#..#",
                        "E..............X",
                        "#..#...S...#..#",
                        "#..C.......C..#",
                        "#.............#",
                        "###############"
                    }
                },
                new()
                {
                    Type = RoomLayoutType.Pillared,
                    Pattern = new[]
                    {
                        "#################",
                        "#...............#",
                        "#.#.....S.....#.#",
                        "#...............#",
                        "#..#.........#..#",
                        "E.......S.......X",
                        "#..#.........#..#",
                        "#...............#",
                        "#.#.....S.....#.#",
                        "#...............#",
                        "#################"
                    }
                }
            }
        },
        {
            RoomLayoutType.LShape, new List<RoomTemplate>
            {
                new()
                {
                    Type = RoomLayoutType.LShape,
                    Pattern = new[]
                    {
                        "########.....",
                        "#......#.....",
                        "#..S...#.....",
                        "#......######",
                        "E............X",
                        "######.....S.#",
                        ".....#.......#",
                        ".....#..S....#",
                        ".....#########"
                    }
                }
            }
        },
        {
            RoomLayoutType.Central, new List<RoomTemplate>
            {
                new()
                {
                    Type = RoomLayoutType.Central,
                    Pattern = new[]
                    {
                        "###############",
                        "#.............#",
                        "#..S.......S..#",
                        "#.....###.....#",
                        "E.....#T#.....X",
                        "#.....###.....#",
                        "#..S.......S..#",
                        "#.............#",
                        "###############"
                    }
                },
                new()
                {
                    Type = RoomLayoutType.Central,
                    Pattern = new[]
                    {
                        "###############",
                        "#.............#",
                        "#.....S.S.....#",
                        "#...#######...#",
                        "E...#.....#...X",
                        "#...#..T..#...#",
                        "#...#.....#...#",
                        "#...#######...#",
                        "#.....S.S.....#",
                        "#.............#",
                        "###############"
                    }
                }
            }
        },
        {
            RoomLayoutType.Corridor, new List<RoomTemplate>
            {
                new()
                {
                    Type = RoomLayoutType.Corridor,
                    Pattern = new[]
                    {
                        "#######################",
                        "##.....................X",
                        "E..S.....S.....S.....##",
                        "#######################"
                    }
                }
            }
        },
        {
            RoomLayoutType.Arena, new List<RoomTemplate>
            {
                new()
                {
                    Type = RoomLayoutType.Arena,
                    Pattern = new[]
                    {
                        "###################",
                        "#.................#",
                        "#.C.............C.#",
                        "#.................#",
                        "#...S.........S...#",
                        "E.................X",
                        "#...S.........S...#",
                        "#.................#",
                        "#.C.............C.#",
                        "#.................#",
                        "###################"
                    }
                }
            }
        },
        {
            RoomLayoutType.BossArena, new List<RoomTemplate>
            {
                new()
                {
                    Type = RoomLayoutType.BossArena,
                    Pattern = new[]
                    {
                        "#########################",
                        "#.......................#",
                        "#..C.................C..#",
                        "#.......................#",
                        "#.......................#",
                        "#..........S............#",
                        "E.......................X",
                        "#..........S............#",
                        "#.......................#",
                        "#.......................#",
                        "#..C.................C..#",
                        "#.......................#",
                        "#########################"
                    }
                },
                new()
                {
                    Type = RoomLayoutType.BossArena,
                    Pattern = new[]
                    {
                        "###########################",
                        "#.........................#",
                        "#.........................#",
                        "#..#...................#..#",
                        "#.........................#",
                        "#.....C...........C.......#",
                        "#...........S.............#",
                        "E.........................X",
                        "#...........S.............#",
                        "#.....C...........C.......#",
                        "#.........................#",
                        "#..#...................#..#",
                        "#.........................#",
                        "#.........................#",
                        "###########################"
                    }
                }
            }
        }
    };

    /// <summary>
    /// Generates a room layout for the specified room type.
    /// </summary>
    public static RoomLayout GenerateRoom(RoomType roomType, int enemyCount = 3)
    {
        var layoutType = GetLayoutTypeForRoom(roomType);
        return GenerateFromTemplate(layoutType, enemyCount);
    }

    /// <summary>
    /// Gets the appropriate layout type for a room type.
    /// </summary>
    private static RoomLayoutType GetLayoutTypeForRoom(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Entrance => RoomLayoutType.Open,
            RoomType.Combat => GetRandomCombatLayout(),
            RoomType.MidBoss => RoomLayoutType.Arena,
            RoomType.FinalBoss => RoomLayoutType.BossArena,
            RoomType.Rest => RoomLayoutType.Open,
            RoomType.Treasure => RoomLayoutType.Central,
            _ => RoomLayoutType.Open
        };
    }

    /// <summary>
    /// Gets a random combat layout type.
    /// </summary>
    private static RoomLayoutType GetRandomCombatLayout()
    {
        var combatLayouts = new[]
        {
            RoomLayoutType.Open,
            RoomLayoutType.Pillared,
            RoomLayoutType.LShape,
            RoomLayoutType.Central,
            RoomLayoutType.Corridor
        };

        return combatLayouts[_random.Next(combatLayouts.Length)];
    }

    /// <summary>
    /// Generates a room from a template.
    /// </summary>
    public static RoomLayout GenerateFromTemplate(RoomLayoutType layoutType, int enemyCount = 3)
    {
        if (!_templates.TryGetValue(layoutType, out var templates) || templates.Count == 0)
        {
            // Fallback to open template
            templates = _templates[RoomLayoutType.Open];
        }

        var template = templates[_random.Next(templates.Count)];
        return ParseTemplate(template, enemyCount);
    }

    /// <summary>
    /// Parses a template into a room layout.
    /// </summary>
    private static RoomLayout ParseTemplate(RoomTemplate template, int enemyCount)
    {
        var pattern = template.Pattern;
        var legend = template.Legend.Count > 0 ? template.Legend : RoomTemplate.DefaultLegend;

        int height = pattern.Length;
        int width = pattern.Max(row => row.Length);

        var tiles = new RoomTile[height, width];
        var layout = new RoomLayout
        {
            Type = template.Type,
            Width = width,
            Height = height,
            Tiles = tiles
        };

        var potentialSpawns = new List<Point>();

        for (int y = 0; y < height; y++)
        {
            var row = pattern[y];

            for (int x = 0; x < width; x++)
            {
                char c = x < row.Length ? row[x] : '#';

                if (legend.TryGetValue(c, out var tile))
                {
                    tiles[y, x] = tile;

                    switch (tile)
                    {
                        case RoomTile.Entry:
                            layout.EntryPoint = new Point(x, y);
                            tiles[y, x] = RoomTile.Floor; // Entry is walkable
                            break;

                        case RoomTile.Exit:
                            layout.ExitPoint = new Point(x, y);
                            tiles[y, x] = RoomTile.Floor; // Exit is walkable
                            break;

                        case RoomTile.EnemySpawn:
                            potentialSpawns.Add(new Point(x, y));
                            tiles[y, x] = RoomTile.Floor; // Spawn is walkable
                            break;

                        case RoomTile.Treasure:
                            layout.TreasureLocations.Add(new Point(x, y));
                            tiles[y, x] = RoomTile.Floor;
                            break;

                        case RoomTile.Cover:
                            layout.CoverPositions.Add(new Point(x, y));
                            tiles[y, x] = RoomTile.Floor;
                            break;
                    }
                }
                else
                {
                    tiles[y, x] = RoomTile.Wall;
                }
            }
        }

        // Select actual enemy spawns from potential spawns
        SelectEnemySpawns(layout, potentialSpawns, enemyCount);

        return layout;
    }

    /// <summary>
    /// Selects enemy spawn points from potential locations.
    /// </summary>
    private static void SelectEnemySpawns(RoomLayout layout, List<Point> potentialSpawns, int enemyCount)
    {
        // Shuffle potential spawns
        var shuffled = potentialSpawns.OrderBy(_ => _random.Next()).ToList();

        // Take up to enemyCount spawns
        int spawnsToUse = Math.Min(enemyCount, shuffled.Count);
        for (int i = 0; i < spawnsToUse; i++)
        {
            layout.EnemySpawns.Add(shuffled[i]);
        }

        // If we need more spawns than template provides, generate random ones
        if (layout.EnemySpawns.Count < enemyCount)
        {
            var additionalNeeded = enemyCount - layout.EnemySpawns.Count;
            var additionalSpawns = GenerateRandomSpawnPoints(layout, additionalNeeded);
            layout.EnemySpawns.AddRange(additionalSpawns);
        }
    }

    /// <summary>
    /// Generates random spawn points in walkable areas.
    /// </summary>
    private static List<Point> GenerateRandomSpawnPoints(RoomLayout layout, int count)
    {
        var spawns = new List<Point>();
        var attempts = 0;
        var maxAttempts = count * 100;

        while (spawns.Count < count && attempts < maxAttempts)
        {
            attempts++;

            int x = _random.Next(1, layout.Width - 1);
            int y = _random.Next(1, layout.Height - 1);
            var point = new Point(x, y);

            if (layout.IsWalkable(point) &&
                point != layout.EntryPoint &&
                point != layout.ExitPoint &&
                !spawns.Contains(point) &&
                !layout.EnemySpawns.Contains(point))
            {
                spawns.Add(point);
            }
        }

        return spawns;
    }

    /// <summary>
    /// Generates a procedural room without using templates.
    /// </summary>
    public static RoomLayout GenerateProcedural(int width, int height, int enemyCount = 3)
    {
        var tiles = new RoomTile[height, width];

        // Fill with floor
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Walls around edges
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    tiles[y, x] = RoomTile.Wall;
                }
                else
                {
                    tiles[y, x] = RoomTile.Floor;
                }
            }
        }

        var layout = new RoomLayout
        {
            Type = RoomLayoutType.Open,
            Width = width,
            Height = height,
            Tiles = tiles,
            EntryPoint = new Point(1, height / 2),
            ExitPoint = new Point(width - 2, height / 2)
        };

        // Add random pillars
        int pillarCount = _random.Next(0, 5);
        for (int i = 0; i < pillarCount; i++)
        {
            int px = _random.Next(3, width - 3);
            int py = _random.Next(3, height - 3);

            // Don't block entry/exit paths
            if (Math.Abs(py - layout.EntryPoint.Y) > 1 && Math.Abs(py - layout.ExitPoint.Y) > 1)
            {
                tiles[py, px] = RoomTile.Wall;
            }
        }

        // Generate spawn points
        var spawns = GenerateRandomSpawnPoints(layout, enemyCount);
        layout.EnemySpawns.AddRange(spawns);

        return layout;
    }

    /// <summary>
    /// Gets a healing room layout.
    /// </summary>
    public static RoomLayout GenerateHealingRoom()
    {
        var pattern = new[]
        {
            "###########",
            "#.........#",
            "#....+....#",
            "#.........#",
            "E.........X",
            "#.........#",
            "#....+....#",
            "#.........#",
            "###########"
        };

        return ParseTemplate(new RoomTemplate
        {
            Type = RoomLayoutType.Open,
            Pattern = pattern
        }, 0);
    }

    /// <summary>
    /// Gets a treasure room layout.
    /// </summary>
    public static RoomLayout GenerateTreasureRoom()
    {
        var pattern = new[]
        {
            "###########",
            "#.........#",
            "#..T...T..#",
            "#.........#",
            "E....T....X",
            "#.........#",
            "#..T...T..#",
            "#.........#",
            "###########"
        };

        return ParseTemplate(new RoomTemplate
        {
            Type = RoomLayoutType.Open,
            Pattern = pattern
        }, 0);
    }
}

