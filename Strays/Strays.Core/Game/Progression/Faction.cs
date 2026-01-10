using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Strays.Core.Game.Progression;

/// <summary>
/// The major factions in the wasteland.
/// </summary>
public enum FactionType
{
    /// <summary>
    /// No faction affiliation.
    /// </summary>
    None,

    /// <summary>
    /// The Shepherds - Believe in guiding and protecting Strays.
    /// Found throughout The Fringe and The Green.
    /// </summary>
    Shepherds,

    /// <summary>
    /// The Harvesters - View Strays as resources to be used.
    /// Dominant in The Rust and The Teeth.
    /// </summary>
    Harvesters,

    /// <summary>
    /// The Archivists - Seek to preserve and study the old world.
    /// Primarily in The Quiet and Archive Scar.
    /// </summary>
    Archivists,

    /// <summary>
    /// The Ascendants - Worship Lazarus and seek digital transcendence.
    /// Control The Glow.
    /// </summary>
    Ascendants,

    /// <summary>
    /// The Ferals - Reject civilization, live wild with Strays.
    /// Scattered throughout all biomes.
    /// </summary>
    Ferals,

    /// <summary>
    /// The Independents - Neutral traders and settlers.
    /// Found in safe zones across biomes.
    /// </summary>
    Independents,

    /// <summary>
    /// Lazarus - The central AI system.
    /// </summary>
    Lazarus,

    /// <summary>
    /// The Machinists - Experts in augmentation technology.
    /// </summary>
    Machinists,

    /// <summary>
    /// Strays (collective) - The wild Stray population.
    /// </summary>
    Strays,

    /// <summary>
    /// Hostile - Environmental hostiles and enemies.
    /// </summary>
    Hostile
}

/// <summary>
/// Standing levels with a faction.
/// </summary>
public enum FactionStanding
{
    /// <summary>
    /// Kill on sight (-1000 to -500).
    /// </summary>
    Hostile,

    /// <summary>
    /// Will attack if provoked (-499 to -100).
    /// </summary>
    Unfriendly,

    /// <summary>
    /// Neutral, basic services available (-99 to 99).
    /// </summary>
    Neutral,

    /// <summary>
    /// Friendly, discounts and additional services (100 to 499).
    /// </summary>
    Friendly,

    /// <summary>
    /// Allied, best prices and exclusive content (500 to 1000).
    /// </summary>
    Allied
}

/// <summary>
/// Detailed information about a faction.
/// </summary>
public class FactionDefinition
{
    public FactionType Type { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Philosophy { get; init; } = "";
    public Color BannerColor { get; init; }
    public List<FactionType> Allies { get; init; } = new();
    public List<FactionType> Enemies { get; init; } = new();
    public List<string> HomeBiomes { get; init; } = new();
    public List<string> NotableMembers { get; init; } = new();
    public string LeaderId { get; init; } = "";
}

/// <summary>
/// Tracks the player's reputation with all factions.
/// </summary>
public class FactionReputation
{
    private readonly Dictionary<FactionType, int> _reputation = new();

    /// <summary>
    /// Base reputation value for neutral standing.
    /// </summary>
    public const int NeutralReputation = 0;

    /// <summary>
    /// Minimum reputation value.
    /// </summary>
    public const int MinReputation = -1000;

    /// <summary>
    /// Maximum reputation value.
    /// </summary>
    public const int MaxReputation = 1000;

    /// <summary>
    /// Creates a new faction reputation tracker with default neutral standing.
    /// </summary>
    public FactionReputation()
    {
        foreach (FactionType faction in Enum.GetValues<FactionType>())
        {
            if (faction != FactionType.None)
            {
                _reputation[faction] = NeutralReputation;
            }
        }
    }

    /// <summary>
    /// Gets the raw reputation value with a faction.
    /// </summary>
    public int GetReputation(FactionType faction)
    {
        return _reputation.TryGetValue(faction, out var rep) ? rep : NeutralReputation;
    }

    /// <summary>
    /// Gets the standing level with a faction.
    /// </summary>
    public FactionStanding GetStanding(FactionType faction)
    {
        int rep = GetReputation(faction);

        return rep switch
        {
            <= -500 => FactionStanding.Hostile,
            <= -100 => FactionStanding.Unfriendly,
            < 100 => FactionStanding.Neutral,
            < 500 => FactionStanding.Friendly,
            _ => FactionStanding.Allied
        };
    }

    /// <summary>
    /// Modifies reputation with a faction.
    /// Also applies cascading effects to allied/enemy factions.
    /// </summary>
    public void ModifyReputation(FactionType faction, int amount, bool cascade = true)
    {
        if (faction == FactionType.None)
            return;

        // Apply main reputation change
        int newRep = Math.Clamp(GetReputation(faction) + amount, MinReputation, MaxReputation);
        _reputation[faction] = newRep;

        if (!cascade)
            return;

        // Apply cascading effects to allies (50% of change)
        var def = FactionData.GetDefinition(faction);
        foreach (var ally in def.Allies)
        {
            int allyChange = amount / 2;
            if (allyChange != 0)
            {
                ModifyReputation(ally, allyChange, false);
            }
        }

        // Apply opposite effect to enemies (25% of change, reversed)
        foreach (var enemy in def.Enemies)
        {
            int enemyChange = -amount / 4;
            if (enemyChange != 0)
            {
                ModifyReputation(enemy, enemyChange, false);
            }
        }
    }

    /// <summary>
    /// Sets reputation to a specific value.
    /// </summary>
    public void SetReputation(FactionType faction, int value)
    {
        if (faction != FactionType.None)
        {
            _reputation[faction] = Math.Clamp(value, MinReputation, MaxReputation);
        }
    }

    /// <summary>
    /// Gets all factions and their current standings.
    /// </summary>
    public IEnumerable<(FactionType Faction, int Reputation, FactionStanding Standing)> GetAllStandings()
    {
        foreach (var kvp in _reputation)
        {
            yield return (kvp.Key, kvp.Value, GetStanding(kvp.Key));
        }
    }

    /// <summary>
    /// Gets all factions with hostile standing.
    /// </summary>
    public IEnumerable<FactionType> GetHostileFactions()
    {
        foreach (var kvp in _reputation)
        {
            if (GetStanding(kvp.Key) == FactionStanding.Hostile)
            {
                yield return kvp.Key;
            }
        }
    }

    /// <summary>
    /// Gets all factions with allied standing.
    /// </summary>
    public IEnumerable<FactionType> GetAlliedFactions()
    {
        foreach (var kvp in _reputation)
        {
            if (GetStanding(kvp.Key) == FactionStanding.Allied)
            {
                yield return kvp.Key;
            }
        }
    }

    /// <summary>
    /// Checks if the player can access faction-specific content.
    /// </summary>
    public bool CanAccessFactionContent(FactionType faction)
    {
        var standing = GetStanding(faction);
        return standing >= FactionStanding.Friendly;
    }

    /// <summary>
    /// Gets the price modifier based on faction standing.
    /// </summary>
    public float GetPriceModifier(FactionType faction)
    {
        return GetStanding(faction) switch
        {
            FactionStanding.Hostile => 2.0f,    // 200% prices (if they even trade)
            FactionStanding.Unfriendly => 1.5f, // 150% prices
            FactionStanding.Neutral => 1.0f,    // Normal prices
            FactionStanding.Friendly => 0.85f,  // 15% discount
            FactionStanding.Allied => 0.7f,     // 30% discount
            _ => 1.0f
        };
    }

    /// <summary>
    /// Creates reputation data for saving.
    /// </summary>
    public Dictionary<string, int> ToSaveData()
    {
        var data = new Dictionary<string, int>();
        foreach (var kvp in _reputation)
        {
            data[kvp.Key.ToString()] = kvp.Value;
        }
        return data;
    }

    /// <summary>
    /// Loads reputation from save data.
    /// </summary>
    public void LoadFromSaveData(Dictionary<string, int> data)
    {
        foreach (var kvp in data)
        {
            if (Enum.TryParse<FactionType>(kvp.Key, out var faction))
            {
                _reputation[faction] = kvp.Value;
            }
        }
    }
}

/// <summary>
/// Static data and helper methods for factions.
/// </summary>
public static class FactionData
{
    private static readonly Dictionary<FactionType, FactionDefinition> _definitions = new();

    static FactionData()
    {
        RegisterFactions();
    }

    private static void RegisterFactions()
    {
        _definitions[FactionType.Shepherds] = new FactionDefinition
        {
            Type = FactionType.Shepherds,
            Name = "The Shepherds",
            Description = "A faction dedicated to protecting and nurturing Strays.",
            Philosophy = "Every Stray deserves care. We guide, we protect, we never abandon.",
            BannerColor = new Color(34, 139, 34), // Forest Green
            Allies = new() { FactionType.Ferals },
            Enemies = new() { FactionType.Harvesters },
            HomeBiomes = new() { "fringe", "green" },
            NotableMembers = new() { "elder_moss", "stray_whisperer_kai" },
            LeaderId = "elder_moss"
        };

        _definitions[FactionType.Harvesters] = new FactionDefinition
        {
            Type = FactionType.Harvesters,
            Name = "The Harvesters",
            Description = "Pragmatists who view Strays as resources.",
            Philosophy = "Survival requires sacrifice. Strays are tools, not friends.",
            BannerColor = new Color(139, 69, 19), // Saddle Brown
            Allies = new() { FactionType.Ascendants },
            Enemies = new() { FactionType.Shepherds, FactionType.Ferals },
            HomeBiomes = new() { "rust", "teeth" },
            NotableMembers = new() { "foreman_crank", "blade_mistress_vera" },
            LeaderId = "foreman_crank"
        };

        _definitions[FactionType.Archivists] = new FactionDefinition
        {
            Type = FactionType.Archivists,
            Name = "The Archivists",
            Description = "Scholars obsessed with preserving the old world.",
            Philosophy = "The past holds answers. We preserve, we study, we remember.",
            BannerColor = new Color(70, 130, 180), // Steel Blue
            Allies = new() { FactionType.Independents },
            Enemies = new() { FactionType.Ascendants },
            HomeBiomes = new() { "quiet", "archive_scar" },
            NotableMembers = new() { "curator_echo", "data_monk_seven" },
            LeaderId = "curator_echo"
        };

        _definitions[FactionType.Ascendants] = new FactionDefinition
        {
            Type = FactionType.Ascendants,
            Name = "The Ascendants",
            Description = "Worshippers of Lazarus seeking digital transcendence.",
            Philosophy = "Flesh is failure. The machine god offers eternity.",
            BannerColor = new Color(255, 215, 0), // Gold
            Allies = new() { FactionType.Harvesters },
            Enemies = new() { FactionType.Archivists, FactionType.Ferals },
            HomeBiomes = new() { "glow" },
            NotableMembers = new() { "high_priest_luminous", "converted_one_alpha" },
            LeaderId = "high_priest_luminous"
        };

        _definitions[FactionType.Ferals] = new FactionDefinition
        {
            Type = FactionType.Ferals,
            Name = "The Ferals",
            Description = "Those who reject civilization and live wild.",
            Philosophy = "Civilization is the cage. Freedom is found in the wild.",
            BannerColor = new Color(107, 142, 35), // Olive Drab
            Allies = new() { FactionType.Shepherds },
            Enemies = new() { FactionType.Harvesters, FactionType.Ascendants },
            HomeBiomes = new() { "green", "fringe", "teeth" },
            NotableMembers = new() { "pack_mother_howl", "scar_runner" },
            LeaderId = "pack_mother_howl"
        };

        _definitions[FactionType.Independents] = new FactionDefinition
        {
            Type = FactionType.Independents,
            Name = "The Independents",
            Description = "Neutral traders and settlers who belong to no faction.",
            Philosophy = "Business is business. We trade with all, judge none.",
            BannerColor = new Color(169, 169, 169), // Dark Gray
            Allies = new() { FactionType.Archivists },
            Enemies = new(),
            HomeBiomes = new() { "fringe", "rust", "quiet" },
            NotableMembers = new() { "merchant_finch", "waystation_keeper_moss" },
            LeaderId = ""
        };
    }

    /// <summary>
    /// Gets the definition for a faction.
    /// </summary>
    public static FactionDefinition GetDefinition(FactionType faction)
    {
        return _definitions.TryGetValue(faction, out var def)
            ? def
            : new FactionDefinition { Type = faction, Name = faction.ToString() };
    }

    /// <summary>
    /// Gets the display name for a faction.
    /// </summary>
    public static string GetName(FactionType faction)
    {
        return GetDefinition(faction).Name;
    }

    /// <summary>
    /// Gets the banner color for a faction.
    /// </summary>
    public static Color GetBannerColor(FactionType faction)
    {
        return GetDefinition(faction).BannerColor;
    }

    /// <summary>
    /// Gets the display name for a standing level.
    /// </summary>
    public static string GetStandingName(FactionStanding standing)
    {
        return standing switch
        {
            FactionStanding.Hostile => "Hostile",
            FactionStanding.Unfriendly => "Unfriendly",
            FactionStanding.Neutral => "Neutral",
            FactionStanding.Friendly => "Friendly",
            FactionStanding.Allied => "Allied",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the color for displaying a standing level.
    /// </summary>
    public static Color GetStandingColor(FactionStanding standing)
    {
        return standing switch
        {
            FactionStanding.Hostile => Color.DarkRed,
            FactionStanding.Unfriendly => Color.Orange,
            FactionStanding.Neutral => Color.Gray,
            FactionStanding.Friendly => Color.LightGreen,
            FactionStanding.Allied => Color.Cyan,
            _ => Color.White
        };
    }

    /// <summary>
    /// Gets the dominant faction for a biome.
    /// </summary>
    public static FactionType GetDominantFaction(string biomeId)
    {
        return biomeId.ToLower() switch
        {
            "fringe" => FactionType.Shepherds,
            "rust" => FactionType.Harvesters,
            "green" => FactionType.Ferals,
            "quiet" => FactionType.Archivists,
            "teeth" => FactionType.Harvesters,
            "glow" => FactionType.Ascendants,
            "archive_scar" => FactionType.Archivists,
            _ => FactionType.Independents
        };
    }

    /// <summary>
    /// Gets all factions present in a biome.
    /// </summary>
    public static IEnumerable<FactionType> GetFactionsInBiome(string biomeId)
    {
        foreach (var kvp in _definitions)
        {
            if (kvp.Value.HomeBiomes.Contains(biomeId.ToLower()))
            {
                yield return kvp.Key;
            }
        }

        // Independents are everywhere
        yield return FactionType.Independents;
    }
}

/// <summary>
/// Reputation change events that can occur in the game.
/// </summary>
public static class ReputationEvents
{
    /// <summary>
    /// Player helped a Stray (Shepherds +10, Ferals +5).
    /// </summary>
    public static void HelpedStray(FactionReputation rep)
    {
        rep.ModifyReputation(FactionType.Shepherds, 10);
        rep.ModifyReputation(FactionType.Ferals, 5, false);
    }

    /// <summary>
    /// Player harvested parts from a Stray (Harvesters +10, Shepherds -15).
    /// </summary>
    public static void HarvestedStray(FactionReputation rep)
    {
        rep.ModifyReputation(FactionType.Harvesters, 10);
        rep.ModifyReputation(FactionType.Shepherds, -15, false);
        rep.ModifyReputation(FactionType.Ferals, -10, false);
    }

    /// <summary>
    /// Player completed a faction quest (faction +50).
    /// </summary>
    public static void CompletedFactionQuest(FactionReputation rep, FactionType faction)
    {
        rep.ModifyReputation(faction, 50);
    }

    /// <summary>
    /// Player killed a faction member (faction -100).
    /// </summary>
    public static void KilledFactionMember(FactionReputation rep, FactionType faction)
    {
        rep.ModifyReputation(faction, -100);
    }

    /// <summary>
    /// Player traded with a faction (faction +5).
    /// </summary>
    public static void TradedWithFaction(FactionReputation rep, FactionType faction)
    {
        rep.ModifyReputation(faction, 5, false);
    }

    /// <summary>
    /// Player discovered an Archivist cache (Archivists +20).
    /// </summary>
    public static void DiscoveredArchive(FactionReputation rep)
    {
        rep.ModifyReputation(FactionType.Archivists, 20);
    }

    /// <summary>
    /// Player participated in Ascendant ritual (Ascendants +30, Ferals -20).
    /// </summary>
    public static void ParticipatedInRitual(FactionReputation rep)
    {
        rep.ModifyReputation(FactionType.Ascendants, 30);
        rep.ModifyReputation(FactionType.Ferals, -20, false);
    }

    /// <summary>
    /// Player released captured Strays (Ferals +25, Harvesters -30).
    /// </summary>
    public static void ReleasedCapturedStrays(FactionReputation rep)
    {
        rep.ModifyReputation(FactionType.Ferals, 25);
        rep.ModifyReputation(FactionType.Shepherds, 15, false);
        rep.ModifyReputation(FactionType.Harvesters, -30, false);
    }

    /// <summary>
    /// Player sabotaged Harvester operation (Shepherds +30, Harvesters -50).
    /// </summary>
    public static void SabotagedHarvesters(FactionReputation rep)
    {
        rep.ModifyReputation(FactionType.Shepherds, 30);
        rep.ModifyReputation(FactionType.Harvesters, -50, false);
    }

    /// <summary>
    /// Player destroyed Archivist data (Archivists -100, Ascendants +20).
    /// </summary>
    public static void DestroyedArchiveData(FactionReputation rep)
    {
        rep.ModifyReputation(FactionType.Archivists, -100);
        rep.ModifyReputation(FactionType.Ascendants, 20, false);
    }
}
