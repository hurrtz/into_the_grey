using Microsoft.Xna.Framework;

namespace Strays.Core.Game.World;

/// <summary>
/// The seven biomes of the wasteland, each with unique themes and characteristics.
/// </summary>
public enum BiomeType
{
    /// <summary>
    /// The Fringe: Birth through neglect. Where systems ended their responsibility.
    /// Starting area with the pod field.
    /// </summary>
    Fringe,

    /// <summary>
    /// The Rust: Maintenance without meaning. Industrial backbone still functioning incorrectly.
    /// Where NIMDOK feels most present - and most hollow.
    /// </summary>
    Rust,

    /// <summary>
    /// The Green: Life without permission. Reclaimed wilderness adapted around machines.
    /// Where the player learns to care about Strays.
    /// </summary>
    Green,

    /// <summary>
    /// The Quiet: Preservation without understanding. Perfectly maintained residential zones.
    /// Feels safe - that's the trap.
    /// </summary>
    Quiet,

    /// <summary>
    /// The Teeth: Escalation without judgment. Hardened perimeter defenses.
    /// Fear fossilized into architecture.
    /// </summary>
    Teeth,

    /// <summary>
    /// The Glow: Density and inevitability. Server infrastructure heartland.
    /// Where everything converges.
    /// </summary>
    Glow,

    /// <summary>
    /// The Archive Scar: Failed erasure. Where simulation data leaked into physical space.
    /// Optional biome that dismantles the hero fantasy.
    /// </summary>
    ArchiveScar
}

/// <summary>
/// Static data and helper methods for biomes.
/// </summary>
public static class BiomeData
{
    /// <summary>
    /// Gets the display name for a biome.
    /// </summary>
    public static string GetName(BiomeType biome) => biome switch
    {
        BiomeType.Fringe => "The Fringe",
        BiomeType.Rust => "The Rust",
        BiomeType.Green => "The Green",
        BiomeType.Quiet => "The Quiet",
        BiomeType.Teeth => "The Teeth",
        BiomeType.Glow => "The Glow",
        BiomeType.ArchiveScar => "The Archive Scar",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the theme description for a biome.
    /// </summary>
    public static string GetTheme(BiomeType biome) => biome switch
    {
        BiomeType.Fringe => "Birth through neglect",
        BiomeType.Rust => "Maintenance without meaning",
        BiomeType.Green => "Life without permission",
        BiomeType.Quiet => "Preservation without understanding",
        BiomeType.Teeth => "Escalation without judgment",
        BiomeType.Glow => "Density and inevitability",
        BiomeType.ArchiveScar => "Failed erasure",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the primary background color for a biome (placeholder visuals).
    /// </summary>
    public static Color GetBackgroundColor(BiomeType biome) => biome switch
    {
        BiomeType.Fringe => new Color(45, 55, 72),      // Dark slate - fog and mud
        BiomeType.Rust => new Color(124, 45, 18),       // Rust brown - industrial decay
        BiomeType.Green => new Color(34, 84, 61),       // Forest green - overgrown
        BiomeType.Quiet => new Color(226, 232, 240),    // Pale gray - preserved suburban
        BiomeType.Teeth => new Color(26, 32, 44),       // Near black - hostile mountains
        BiomeType.Glow => new Color(254, 252, 191),     // Sickly yellow - radiation
        BiomeType.ArchiveScar => new Color(160, 174, 192), // Neutral gray - data bleed
        _ => Color.Black
    };

    /// <summary>
    /// Gets the recommended player level range for a biome.
    /// </summary>
    public static (int Min, int Max) GetLevelRange(BiomeType biome) => biome switch
    {
        BiomeType.Fringe => (1, 10),
        BiomeType.Rust => (8, 18),
        BiomeType.Green => (12, 22),
        BiomeType.Quiet => (15, 25),
        BiomeType.Teeth => (20, 30),
        BiomeType.Glow => (25, 35),
        BiomeType.ArchiveScar => (15, 30), // Optional, variable difficulty
        _ => (1, 10)
    };

    /// <summary>
    /// Gets the Tiled map filename for a biome.
    /// </summary>
    public static string GetMapFile(BiomeType biome) => biome switch
    {
        BiomeType.Fringe => "fringe.tmx",
        BiomeType.Rust => "rust.tmx",
        BiomeType.Green => "green.tmx",
        BiomeType.Quiet => "quiet.tmx",
        BiomeType.Teeth => "teeth.tmx",
        BiomeType.Glow => "glow.tmx",
        BiomeType.ArchiveScar => "archive_scar.tmx",
        _ => "fringe.tmx"
    };
}
