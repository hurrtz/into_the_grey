using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Data;

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
    /// Where Lazarus feels most present - and most hollow.
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
/// Weather effects that can occur in biomes.
/// </summary>
public enum WeatherType
{
    None,
    Fog,
    Rain,
    AcidRain,
    Dust,
    Snow,
    DataStorm,
    RadiationWind
}

/// <summary>
/// Environmental hazards present in biomes.
/// </summary>
public enum EnvironmentalHazard
{
    None,
    ToxicPools,
    RadiationZones,
    ElectrifiedFloors,
    UnstableGround,
    SecurityTurrets,
    DataGlitches,
    CorruptionPockets
}

/// <summary>
/// Detailed data for a specific biome.
/// </summary>
public class BiomeDefinition
{
    public BiomeType Type { get; init; }
    public string Name { get; init; } = "";
    public string Theme { get; init; } = "";
    public string Description { get; init; } = "";
    public Color BackgroundColor { get; init; }
    public Color AccentColor { get; init; }
    public Color FogColor { get; init; }
    public (int Min, int Max) LevelRange { get; init; }
    public string MapFile { get; init; } = "";
    public List<BiomeType> ConnectedBiomes { get; init; } = new();
    public List<string> NativeStrays { get; init; } = new();
    public List<string> RareStrays { get; init; } = new();
    public List<WeatherType> PossibleWeather { get; init; } = new();
    public List<EnvironmentalHazard> Hazards { get; init; } = new();
    public float EncounterRate { get; init; } = 1.0f;
    public bool IsSafeZone { get; init; } = false;
    public string AmbientSound { get; init; } = "";
    public int ChunkWidth { get; init; } = 1600;
    public int ChunkHeight { get; init; } = 1200;
}

/// <summary>
/// Static data and helper methods for biomes.
/// </summary>
public static class BiomeData
{
    private static readonly Dictionary<BiomeType, BiomeDefinition> _definitions = new();

    static BiomeData()
    {
        RegisterBiomes();
    }

    private static void RegisterBiomes()
    {
        // The Fringe - Starting area
        _definitions[BiomeType.Fringe] = new BiomeDefinition
        {
            Type = BiomeType.Fringe,
            Name = "The Fringe",
            Theme = "Birth through neglect",
            Description = "Where systems ended their responsibility. Pod fields and forgotten infrastructure.",
            BackgroundColor = new Color(45, 55, 72),
            AccentColor = new Color(74, 85, 104),
            FogColor = new Color(100, 116, 139) * 0.5f,
            LevelRange = (1, 10),
            MapFile = "fringe.tmx",
            ConnectedBiomes = new() { BiomeType.Rust, BiomeType.Green },
            NativeStrays = new() { "echo_pup", "circuit_crow", "relay_rodent", "rust_rat", "glitch_moth" },
            RareStrays = new() { "circuit_cat", "scrap_hound" },
            PossibleWeather = new() { WeatherType.Fog, WeatherType.Dust },
            Hazards = new() { EnvironmentalHazard.ToxicPools },
            EncounterRate = 0.8f,
            AmbientSound = "fringe_ambient",
            ChunkWidth = 2000,
            ChunkHeight = 1500
        };

        // The Rust - Industrial wasteland
        _definitions[BiomeType.Rust] = new BiomeDefinition
        {
            Type = BiomeType.Rust,
            Name = "The Rust",
            Theme = "Maintenance without meaning",
            Description = "Industrial backbone still functioning incorrectly. Where Lazarus feels most present.",
            BackgroundColor = new Color(124, 45, 18),
            AccentColor = new Color(180, 83, 9),
            FogColor = new Color(146, 64, 14) * 0.4f,
            LevelRange = (8, 18),
            MapFile = "rust.tmx",
            ConnectedBiomes = new() { BiomeType.Fringe, BiomeType.Teeth, BiomeType.Quiet },
            NativeStrays = new() { "rust_rat", "scrap_hound", "gear_beetle", "piston_snake", "forge_spider" },
            RareStrays = new() { "crane_raptor", "furnace_golem" },
            PossibleWeather = new() { WeatherType.Dust, WeatherType.AcidRain },
            Hazards = new() { EnvironmentalHazard.ElectrifiedFloors, EnvironmentalHazard.UnstableGround },
            EncounterRate = 1.0f,
            AmbientSound = "rust_ambient",
            ChunkWidth = 2400,
            ChunkHeight = 1800
        };

        // The Green - Overgrown wilderness
        _definitions[BiomeType.Green] = new BiomeDefinition
        {
            Type = BiomeType.Green,
            Name = "The Green",
            Theme = "Life without permission",
            Description = "Reclaimed wilderness adapted around machines. Where players learn to care about Strays.",
            BackgroundColor = new Color(34, 84, 61),
            AccentColor = new Color(22, 163, 74),
            FogColor = new Color(74, 124, 89) * 0.4f,
            LevelRange = (12, 22),
            MapFile = "green.tmx",
            ConnectedBiomes = new() { BiomeType.Fringe, BiomeType.Quiet, BiomeType.ArchiveScar },
            NativeStrays = new() { "vine_serpent", "bloom_moth", "moss_bear", "thorn_cat", "spore_toad" },
            RareStrays = new() { "ancient_oak_deer", "chloro_phoenix" },
            PossibleWeather = new() { WeatherType.Rain, WeatherType.Fog },
            Hazards = new() { EnvironmentalHazard.ToxicPools },
            EncounterRate = 1.2f,
            AmbientSound = "green_ambient",
            ChunkWidth = 2000,
            ChunkHeight = 2000
        };

        // The Quiet - Preserved suburbs
        _definitions[BiomeType.Quiet] = new BiomeDefinition
        {
            Type = BiomeType.Quiet,
            Name = "The Quiet",
            Theme = "Preservation without understanding",
            Description = "Perfectly maintained residential zones. Feels safe - that's the trap.",
            BackgroundColor = new Color(226, 232, 240),
            AccentColor = new Color(148, 163, 184),
            FogColor = new Color(203, 213, 225) * 0.3f,
            LevelRange = (15, 25),
            MapFile = "quiet.tmx",
            ConnectedBiomes = new() { BiomeType.Rust, BiomeType.Green, BiomeType.Teeth },
            NativeStrays = new() { "house_cat", "lawn_drone", "sprinkler_serpent", "mailbox_mimic", "garage_guardian" },
            RareStrays = new() { "suburb_sentinel", "perfect_pet" },
            PossibleWeather = new() { WeatherType.None }, // Always perfect weather
            Hazards = new() { EnvironmentalHazard.SecurityTurrets },
            EncounterRate = 0.6f,
            AmbientSound = "quiet_ambient",
            ChunkWidth = 1800,
            ChunkHeight = 1400
        };

        // The Teeth - Defensive perimeter
        _definitions[BiomeType.Teeth] = new BiomeDefinition
        {
            Type = BiomeType.Teeth,
            Name = "The Teeth",
            Theme = "Escalation without judgment",
            Description = "Hardened perimeter defenses. Fear fossilized into architecture.",
            BackgroundColor = new Color(26, 32, 44),
            AccentColor = new Color(55, 65, 81),
            FogColor = new Color(31, 41, 55) * 0.6f,
            LevelRange = (20, 30),
            MapFile = "teeth.tmx",
            ConnectedBiomes = new() { BiomeType.Rust, BiomeType.Quiet, BiomeType.Glow },
            NativeStrays = new() { "turret_hawk", "razor_hound", "bunker_bear", "wall_crawler", "sentry_spider" },
            RareStrays = new() { "fortress_titan", "siege_wyrm" },
            PossibleWeather = new() { WeatherType.Dust, WeatherType.Snow },
            Hazards = new() { EnvironmentalHazard.SecurityTurrets, EnvironmentalHazard.ElectrifiedFloors },
            EncounterRate = 1.5f,
            AmbientSound = "teeth_ambient",
            ChunkWidth = 2200,
            ChunkHeight = 1600
        };

        // The Glow - Server heartland
        _definitions[BiomeType.Glow] = new BiomeDefinition
        {
            Type = BiomeType.Glow,
            Name = "The Glow",
            Theme = "Density and inevitability",
            Description = "Server infrastructure heartland. Where everything converges.",
            BackgroundColor = new Color(254, 252, 191),
            AccentColor = new Color(250, 204, 21),
            FogColor = new Color(253, 224, 71) * 0.3f,
            LevelRange = (25, 35),
            MapFile = "glow.tmx",
            ConnectedBiomes = new() { BiomeType.Teeth },
            NativeStrays = new() { "server_sprite", "data_worm", "cache_cat", "firewall_fox", "bandwidth_bat" },
            RareStrays = new() { "kernel_dragon", "root_access_bear" },
            PossibleWeather = new() { WeatherType.DataStorm, WeatherType.RadiationWind },
            Hazards = new() { EnvironmentalHazard.RadiationZones, EnvironmentalHazard.DataGlitches },
            EncounterRate = 1.8f,
            AmbientSound = "glow_ambient",
            ChunkWidth = 2000,
            ChunkHeight = 2000
        };

        // The Archive Scar - Optional/secret area
        _definitions[BiomeType.ArchiveScar] = new BiomeDefinition
        {
            Type = BiomeType.ArchiveScar,
            Name = "The Archive Scar",
            Theme = "Failed erasure",
            Description = "Where simulation data leaked into physical space. Optional biome with hidden truths.",
            BackgroundColor = new Color(160, 174, 192),
            AccentColor = new Color(100, 116, 139),
            FogColor = new Color(148, 163, 184) * 0.5f,
            LevelRange = (15, 30),
            MapFile = "archive_scar.tmx",
            ConnectedBiomes = new() { BiomeType.Green },
            NativeStrays = new() { "memory_ghost", "deleted_dog", "corrupted_cat", "null_serpent", "void_moth" },
            RareStrays = new() { "ancient_backup", "original_instance" },
            PossibleWeather = new() { WeatherType.DataStorm },
            Hazards = new() { EnvironmentalHazard.DataGlitches, EnvironmentalHazard.CorruptionPockets },
            EncounterRate = 1.0f,
            AmbientSound = "archive_ambient",
            ChunkWidth = 1600,
            ChunkHeight = 1600
        };
    }

    /// <summary>
    /// Gets the full definition for a biome.
    /// </summary>
    public static BiomeDefinition GetDefinition(BiomeType biome) =>
        _definitions.TryGetValue(biome, out var def) ? def : _definitions[BiomeType.Fringe];

    /// <summary>
    /// Gets the display name for a biome.
    /// </summary>
    public static string GetName(BiomeType biome) => GetDefinition(biome).Name;

    /// <summary>
    /// Gets the theme description for a biome.
    /// </summary>
    public static string GetTheme(BiomeType biome) => GetDefinition(biome).Theme;

    /// <summary>
    /// Gets the primary background color for a biome (placeholder visuals).
    /// </summary>
    public static Color GetBackgroundColor(BiomeType biome) => GetDefinition(biome).BackgroundColor;

    /// <summary>
    /// Gets the accent color for a biome.
    /// </summary>
    public static Color GetAccentColor(BiomeType biome) => GetDefinition(biome).AccentColor;

    /// <summary>
    /// Gets the fog color for a biome.
    /// </summary>
    public static Color GetFogColor(BiomeType biome) => GetDefinition(biome).FogColor;

    /// <summary>
    /// Gets the recommended player level range for a biome.
    /// </summary>
    public static (int Min, int Max) GetLevelRange(BiomeType biome) => GetDefinition(biome).LevelRange;

    /// <summary>
    /// Gets the Tiled map filename for a biome.
    /// </summary>
    public static string GetMapFile(BiomeType biome) => GetDefinition(biome).MapFile;

    /// <summary>
    /// Gets the biomes connected to a given biome.
    /// </summary>
    public static List<BiomeType> GetConnectedBiomes(BiomeType biome) => GetDefinition(biome).ConnectedBiomes;

    /// <summary>
    /// Gets native Strays that commonly appear in a biome.
    /// </summary>
    public static List<string> GetNativeStrays(BiomeType biome) => GetDefinition(biome).NativeStrays;

    /// <summary>
    /// Gets rare Strays that can appear in a biome.
    /// </summary>
    public static List<string> GetRareStrays(BiomeType biome) => GetDefinition(biome).RareStrays;

    /// <summary>
    /// Gets possible weather types for a biome.
    /// </summary>
    public static List<WeatherType> GetPossibleWeather(BiomeType biome) => GetDefinition(biome).PossibleWeather;

    /// <summary>
    /// Gets environmental hazards in a biome.
    /// </summary>
    public static List<EnvironmentalHazard> GetHazards(BiomeType biome) => GetDefinition(biome).Hazards;

    /// <summary>
    /// Gets the encounter rate multiplier for a biome.
    /// </summary>
    public static float GetEncounterRate(BiomeType biome) => GetDefinition(biome).EncounterRate;

    /// <summary>
    /// Gets the default chunk size for a biome.
    /// </summary>
    public static (int Width, int Height) GetChunkSize(BiomeType biome)
    {
        var def = GetDefinition(biome);
        return (def.ChunkWidth, def.ChunkHeight);
    }

    /// <summary>
    /// Checks if two biomes are connected.
    /// </summary>
    public static bool AreConnected(BiomeType from, BiomeType to) =>
        GetDefinition(from).ConnectedBiomes.Contains(to);

    /// <summary>
    /// Gets all biomes in recommended progression order.
    /// </summary>
    public static BiomeType[] GetProgressionOrder() => new[]
    {
        BiomeType.Fringe,
        BiomeType.Rust,
        BiomeType.Green,
        BiomeType.Quiet,
        BiomeType.Teeth,
        BiomeType.Glow
        // ArchiveScar is optional/secret
    };
}
