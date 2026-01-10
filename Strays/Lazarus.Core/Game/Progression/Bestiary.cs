using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Entities;

namespace Lazarus.Core.Game.Progression;

/// <summary>
/// Discovery status for a bestiary entry.
/// </summary>
public enum DiscoveryStatus
{
    /// <summary>
    /// Never encountered.
    /// </summary>
    Unknown,

    /// <summary>
    /// Encountered but not studied.
    /// </summary>
    Sighted,

    /// <summary>
    /// Defeated in combat.
    /// </summary>
    Defeated,

    /// <summary>
    /// Recruited or captured.
    /// </summary>
    Recruited,

    /// <summary>
    /// Fully researched.
    /// </summary>
    Mastered
}

/// <summary>
/// A single bestiary entry for a Stray type.
/// </summary>
public class BestiaryEntry
{
    /// <summary>
    /// The Stray definition ID.
    /// </summary>
    public string DefinitionId { get; init; } = "";

    /// <summary>
    /// Current discovery status.
    /// </summary>
    public DiscoveryStatus Status { get; set; } = DiscoveryStatus.Unknown;

    /// <summary>
    /// Number of times encountered.
    /// </summary>
    public int EncounterCount { get; set; } = 0;

    /// <summary>
    /// Number of times defeated.
    /// </summary>
    public int DefeatCount { get; set; } = 0;

    /// <summary>
    /// Number of times recruited.
    /// </summary>
    public int RecruitCount { get; set; } = 0;

    /// <summary>
    /// Whether stats are revealed.
    /// </summary>
    public bool StatsRevealed => Status >= DiscoveryStatus.Defeated;

    /// <summary>
    /// Whether abilities are revealed.
    /// </summary>
    public bool AbilitiesRevealed => Status >= DiscoveryStatus.Recruited;

    /// <summary>
    /// Whether lore is revealed.
    /// </summary>
    public bool LoreRevealed => Status >= DiscoveryStatus.Mastered;

    /// <summary>
    /// Whether drop tables are revealed.
    /// </summary>
    public bool DropsRevealed => DefeatCount >= 5;

    /// <summary>
    /// Whether recruitment conditions are revealed.
    /// </summary>
    public bool RecruitmentRevealed => RecruitCount > 0 || EncounterCount >= 10;

    /// <summary>
    /// First encounter date.
    /// </summary>
    public DateTime? FirstEncounter { get; set; }

    /// <summary>
    /// Ledger number (order caught). 0 = not yet in ledger.
    /// </summary>
    public int LedgerNumber { get; set; } = 0;

    /// <summary>
    /// Biomes where this Stray has been encountered.
    /// </summary>
    public HashSet<string> EncounteredBiomes { get; set; } = new();

    /// <summary>
    /// Notes added by the player.
    /// </summary>
    public string PlayerNotes { get; set; } = "";

    /// <summary>
    /// Gets the completion percentage for this entry.
    /// </summary>
    public float CompletionPercent
    {
        get
        {
            int total = 5; // Status, Stats, Abilities, Lore, Drops
            int complete = 0;

            if (Status > DiscoveryStatus.Unknown) complete++;
            if (StatsRevealed) complete++;
            if (AbilitiesRevealed) complete++;
            if (LoreRevealed) complete++;
            if (DropsRevealed) complete++;

            return (float)complete / total * 100f;
        }
    }
}

/// <summary>
/// Category grouping for bestiary display.
/// </summary>
public class BestiaryCategory
{
    /// <summary>
    /// Category identifier.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Category description.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Associated color for UI.
    /// </summary>
    public Color Color { get; init; } = Color.White;

    /// <summary>
    /// Stray definition IDs in this category.
    /// </summary>
    public List<string> StrayIds { get; init; } = new();
}

/// <summary>
/// The in-game bestiary/encyclopedia for discovered Strays.
/// </summary>
public class Bestiary
{
    private readonly Dictionary<string, BestiaryEntry> _entries = new();
    private readonly List<BestiaryCategory> _categories = new();
    private int _nextLedgerNumber = 1;

    /// <summary>
    /// All bestiary entries.
    /// </summary>
    public IReadOnlyDictionary<string, BestiaryEntry> Entries => _entries;

    /// <summary>
    /// All categories.
    /// </summary>
    public IReadOnlyList<BestiaryCategory> Categories => _categories;

    /// <summary>
    /// Total number of discoverable entries.
    /// </summary>
    public int TotalEntries { get; private set; }

    /// <summary>
    /// Number of discovered entries.
    /// </summary>
    public int DiscoveredCount => _entries.Values.Count(e => e.Status > DiscoveryStatus.Unknown);

    /// <summary>
    /// Number of mastered entries.
    /// </summary>
    public int MasteredCount => _entries.Values.Count(e => e.Status >= DiscoveryStatus.Mastered);

    /// <summary>
    /// Number of entries in the ledger (caught/recruited).
    /// </summary>
    public int LedgerCount => _entries.Values.Count(e => e.LedgerNumber > 0);

    /// <summary>
    /// Overall completion percentage.
    /// </summary>
    public float CompletionPercent => TotalEntries > 0 ? (float)DiscoveredCount / TotalEntries * 100f : 0f;

    /// <summary>
    /// Event fired when a new Stray is discovered.
    /// </summary>
    public event EventHandler<BestiaryEntry>? EntryDiscovered;

    /// <summary>
    /// Event fired when an entry status changes.
    /// </summary>
    public event EventHandler<BestiaryEntry>? EntryUpdated;

    /// <summary>
    /// Event fired when a milestone is reached.
    /// </summary>
    public event EventHandler<BestiaryMilestone>? MilestoneReached;

    public Bestiary()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes the bestiary with all Stray definitions.
    /// </summary>
    private void Initialize()
    {
        // Create entries for all Stray definitions
        foreach (var def in StrayDefinitions.GetAll())
        {
            var entry = new BestiaryEntry
            {
                DefinitionId = def.Id
            };

            // Pre-register companions with their fixed ledger numbers
            if (def.IsCompanion && def.LedgerNumber > 0)
            {
                entry.LedgerNumber = def.LedgerNumber;
                entry.Status = DiscoveryStatus.Recruited;
                entry.RecruitCount = 1;
                entry.EncounterCount = 1;
                entry.FirstEncounter = DateTime.Now;

                // Track highest ledger number for next assignment
                if (def.LedgerNumber >= _nextLedgerNumber)
                {
                    _nextLedgerNumber = def.LedgerNumber + 1;
                }
            }

            _entries[def.Id] = entry;
        }

        TotalEntries = _entries.Count;

        // Create categories
        _categories.Add(new BestiaryCategory
        {
            Id = "fringe",
            Name = "The Fringe",
            Description = "Creatures found in the pod fields and foggy outskirts.",
            Color = new Color(150, 150, 170),
            StrayIds = StrayDefinitions.GetAll().Where(s => s.Biomes.Contains("fringe")).Select(s => s.Id).ToList()
        });

        _categories.Add(new BestiaryCategory
        {
            Id = "rust",
            Name = "The Rust",
            Description = "Industrial creatures of metal and decay.",
            Color = new Color(180, 100, 60),
            StrayIds = StrayDefinitions.GetAll().Where(s => s.Biomes.Contains("rust")).Select(s => s.Id).ToList()
        });

        _categories.Add(new BestiaryCategory
        {
            Id = "green",
            Name = "The Green",
            Description = "Overgrown creatures of nature reclaimed.",
            Color = new Color(80, 180, 80),
            StrayIds = StrayDefinitions.GetAll().Where(s => s.Biomes.Contains("green")).Select(s => s.Id).ToList()
        });

        _categories.Add(new BestiaryCategory
        {
            Id = "quiet",
            Name = "The Quiet",
            Description = "Uncanny creatures of the perfect suburbs.",
            Color = new Color(200, 200, 150),
            StrayIds = StrayDefinitions.GetAll().Where(s => s.Biomes.Contains("quiet")).Select(s => s.Id).ToList()
        });

        _categories.Add(new BestiaryCategory
        {
            Id = "teeth",
            Name = "The Teeth",
            Description = "Militarized creatures of war and ruin.",
            Color = new Color(150, 50, 50),
            StrayIds = StrayDefinitions.GetAll().Where(s => s.Biomes.Contains("teeth")).Select(s => s.Id).ToList()
        });

        _categories.Add(new BestiaryCategory
        {
            Id = "glow",
            Name = "The Glow",
            Description = "Digital creatures of data and code.",
            Color = new Color(100, 200, 255),
            StrayIds = StrayDefinitions.GetAll().Where(s => s.Biomes.Contains("glow")).Select(s => s.Id).ToList()
        });

        _categories.Add(new BestiaryCategory
        {
            Id = "archive",
            Name = "The Archive Scar",
            Description = "Fragmentary creatures of deleted memories.",
            Color = new Color(150, 100, 200),
            StrayIds = StrayDefinitions.GetAll().Where(s => s.Biomes.Contains("archive_scar")).Select(s => s.Id).ToList()
        });

        _categories.Add(new BestiaryCategory
        {
            Id = "boss",
            Name = "Legends",
            Description = "Unique and powerful entities.",
            Color = new Color(255, 200, 100),
            StrayIds = StrayDefinitions.GetAll().Where(s => s.IsBoss).Select(s => s.Id).ToList()
        });

        _categories.Add(new BestiaryCategory
        {
            Id = "evolved",
            Name = "Evolved Forms",
            Description = "Strays that have transcended their original forms.",
            Color = new Color(255, 150, 255),
            StrayIds = StrayDefinitions.GetAll().Where(s => !s.CanRecruit && !s.IsBoss).Select(s => s.Id).ToList()
        });
    }

    /// <summary>
    /// Records an encounter with a Stray.
    /// </summary>
    public void RecordEncounter(string definitionId, string biome)
    {
        if (!_entries.TryGetValue(definitionId, out var entry))
        {
            return;
        }

        bool wasNew = entry.Status == DiscoveryStatus.Unknown;
        var oldStatus = entry.Status;

        entry.EncounterCount++;
        entry.EncounteredBiomes.Add(biome);

        if (entry.FirstEncounter == null)
        {
            entry.FirstEncounter = DateTime.Now;
        }

        if (entry.Status < DiscoveryStatus.Sighted)
        {
            entry.Status = DiscoveryStatus.Sighted;
        }

        if (wasNew)
        {
            EntryDiscovered?.Invoke(this, entry);
        }

        if (oldStatus != entry.Status)
        {
            EntryUpdated?.Invoke(this, entry);
            CheckMilestones();
        }
    }

    /// <summary>
    /// Records defeating a Stray.
    /// </summary>
    public void RecordDefeat(string definitionId)
    {
        if (!_entries.TryGetValue(definitionId, out var entry))
        {
            return;
        }

        var oldStatus = entry.Status;
        entry.DefeatCount++;

        if (entry.Status < DiscoveryStatus.Defeated)
        {
            entry.Status = DiscoveryStatus.Defeated;
        }

        // Check for mastery through combat
        if (entry.DefeatCount >= 10 && entry.Status < DiscoveryStatus.Mastered)
        {
            entry.Status = DiscoveryStatus.Mastered;
        }

        if (oldStatus != entry.Status)
        {
            EntryUpdated?.Invoke(this, entry);
            CheckMilestones();
        }
    }

    /// <summary>
    /// Records recruiting a Stray.
    /// </summary>
    public void RecordRecruitment(string definitionId)
    {
        if (!_entries.TryGetValue(definitionId, out var entry))
        {
            return;
        }

        var oldStatus = entry.Status;
        bool isFirstRecruit = entry.RecruitCount == 0;
        entry.RecruitCount++;

        // Assign ledger number on first recruitment
        if (isFirstRecruit && entry.LedgerNumber == 0)
        {
            entry.LedgerNumber = _nextLedgerNumber++;
        }

        if (entry.Status < DiscoveryStatus.Recruited)
        {
            entry.Status = DiscoveryStatus.Recruited;
        }

        // Recruitment counts toward mastery
        if (entry.RecruitCount >= 3 || (entry.DefeatCount >= 5 && entry.RecruitCount >= 1))
        {
            entry.Status = DiscoveryStatus.Mastered;
        }

        if (oldStatus != entry.Status)
        {
            EntryUpdated?.Invoke(this, entry);
            CheckMilestones();
        }
    }

    /// <summary>
    /// Sets player notes for an entry.
    /// </summary>
    public void SetNotes(string definitionId, string notes)
    {
        if (_entries.TryGetValue(definitionId, out var entry))
        {
            entry.PlayerNotes = notes;
        }
    }

    /// <summary>
    /// Gets an entry by ID.
    /// </summary>
    public BestiaryEntry? GetEntry(string definitionId)
    {
        return _entries.TryGetValue(definitionId, out var entry) ? entry : null;
    }

    /// <summary>
    /// Gets all entries for a category.
    /// </summary>
    public IEnumerable<BestiaryEntry> GetEntriesForCategory(string categoryId)
    {
        var category = _categories.FirstOrDefault(c => c.Id == categoryId);

        if (category == null)
        {
            yield break;
        }

        foreach (var id in category.StrayIds)
        {
            if (_entries.TryGetValue(id, out var entry))
            {
                yield return entry;
            }
        }
    }

    /// <summary>
    /// Gets all discovered entries.
    /// </summary>
    public IEnumerable<BestiaryEntry> GetDiscoveredEntries()
    {
        return _entries.Values.Where(e => e.Status > DiscoveryStatus.Unknown);
    }

    /// <summary>
    /// Gets entries by discovery status.
    /// </summary>
    public IEnumerable<BestiaryEntry> GetEntriesByStatus(DiscoveryStatus status)
    {
        return _entries.Values.Where(e => e.Status == status);
    }

    /// <summary>
    /// Gets all entries that have been added to the ledger (recruited),
    /// ordered by their ledger number.
    /// </summary>
    public IEnumerable<BestiaryEntry> GetLedgerEntries()
    {
        return _entries.Values
            .Where(e => e.LedgerNumber > 0)
            .OrderBy(e => e.LedgerNumber);
    }

    /// <summary>
    /// Gets the display name for an entry (respects discovery status).
    /// </summary>
    public string GetDisplayName(string definitionId)
    {
        if (!_entries.TryGetValue(definitionId, out var entry))
        {
            return "???";
        }

        if (entry.Status == DiscoveryStatus.Unknown)
        {
            return "???";
        }

        var def = StrayDefinitions.Get(definitionId);

        return def?.Name ?? "Unknown";
    }

    /// <summary>
    /// Gets the display description for an entry (respects discovery status).
    /// </summary>
    public string GetDisplayDescription(string definitionId)
    {
        if (!_entries.TryGetValue(definitionId, out var entry))
        {
            return "This creature has never been encountered.";
        }

        if (entry.Status == DiscoveryStatus.Unknown)
        {
            return "This creature has never been encountered.";
        }

        var def = StrayDefinitions.Get(definitionId);

        if (def == null)
        {
            return "Unknown creature.";
        }

        if (!entry.LoreRevealed)
        {
            // Partial description
            string partial = def.Description;

            if (partial.Length > 50)
            {
                partial = partial[..47] + "...";
            }

            return partial + " [Recruit or defeat more to reveal full lore]";
        }

        return def.Description;
    }

    /// <summary>
    /// Gets category completion percentage.
    /// </summary>
    public float GetCategoryCompletion(string categoryId)
    {
        var category = _categories.FirstOrDefault(c => c.Id == categoryId);

        if (category == null || category.StrayIds.Count == 0)
        {
            return 0f;
        }

        int discovered = category.StrayIds.Count(id => _entries.TryGetValue(id, out var e) && e.Status > DiscoveryStatus.Unknown);

        return (float)discovered / category.StrayIds.Count * 100f;
    }

    private void CheckMilestones()
    {
        // Check for milestone achievements
        int discovered = DiscoveredCount;

        if (discovered == 10)
        {
            MilestoneReached?.Invoke(this, new BestiaryMilestone("first_10", "Naturalist", "Discovered 10 unique Strays"));
        }
        else if (discovered == 25)
        {
            MilestoneReached?.Invoke(this, new BestiaryMilestone("first_25", "Field Researcher", "Discovered 25 unique Strays"));
        }
        else if (discovered == 50)
        {
            MilestoneReached?.Invoke(this, new BestiaryMilestone("first_50", "Biologist", "Discovered 50 unique Strays"));
        }
        else if (discovered == TotalEntries)
        {
            MilestoneReached?.Invoke(this, new BestiaryMilestone("complete", "Master Zoologist", "Discovered all Strays!"));
        }

        // Check for mastery milestones
        int mastered = MasteredCount;

        if (mastered == 10)
        {
            MilestoneReached?.Invoke(this, new BestiaryMilestone("master_10", "Expert", "Mastered 10 bestiary entries"));
        }
        else if (mastered == TotalEntries)
        {
            MilestoneReached?.Invoke(this, new BestiaryMilestone("master_all", "True Scholar", "Mastered all bestiary entries!"));
        }

        // Check for category milestones
        foreach (var category in _categories)
        {
            float completion = GetCategoryCompletion(category.Id);

            if (Math.Abs(completion - 100f) < 0.01f)
            {
                MilestoneReached?.Invoke(this, new BestiaryMilestone(
                    $"category_{category.Id}",
                    $"{category.Name} Expert",
                    $"Discovered all Strays in {category.Name}"));
            }
        }
    }

    /// <summary>
    /// Exports bestiary data for saving.
    /// </summary>
    public BestiarySaveData Export()
    {
        return new BestiarySaveData
        {
            NextLedgerNumber = _nextLedgerNumber,
            Entries = _entries.Values.Select(e => new BestiaryEntrySaveData
            {
                DefinitionId = e.DefinitionId,
                Status = (int)e.Status,
                EncounterCount = e.EncounterCount,
                DefeatCount = e.DefeatCount,
                RecruitCount = e.RecruitCount,
                LedgerNumber = e.LedgerNumber,
                FirstEncounter = e.FirstEncounter,
                EncounteredBiomes = e.EncounteredBiomes.ToList(),
                PlayerNotes = e.PlayerNotes
            }).ToList()
        };
    }

    /// <summary>
    /// Imports bestiary data from save.
    /// </summary>
    public void Import(BestiarySaveData data)
    {
        _nextLedgerNumber = data.NextLedgerNumber > 0 ? data.NextLedgerNumber : 1;

        foreach (var entryData in data.Entries)
        {
            if (_entries.TryGetValue(entryData.DefinitionId, out var entry))
            {
                entry.Status = (DiscoveryStatus)entryData.Status;
                entry.EncounterCount = entryData.EncounterCount;
                entry.DefeatCount = entryData.DefeatCount;
                entry.RecruitCount = entryData.RecruitCount;
                entry.LedgerNumber = entryData.LedgerNumber;
                entry.FirstEncounter = entryData.FirstEncounter;
                entry.EncounteredBiomes = new HashSet<string>(entryData.EncounteredBiomes);
                entry.PlayerNotes = entryData.PlayerNotes ?? "";

                // Update next ledger number if needed
                if (entry.LedgerNumber >= _nextLedgerNumber)
                {
                    _nextLedgerNumber = entry.LedgerNumber + 1;
                }
            }
        }
    }

    /// <summary>
    /// Gets unlocked definition IDs for NG+ carry-over.
    /// </summary>
    public HashSet<string> GetUnlockedIds()
    {
        return new HashSet<string>(_entries.Where(kvp => kvp.Value.Status > DiscoveryStatus.Unknown).Select(kvp => kvp.Key));
    }

    /// <summary>
    /// Unlocks entries from a previous save (for NG+).
    /// </summary>
    public void UnlockFromPrevious(HashSet<string> unlockedIds)
    {
        foreach (var id in unlockedIds)
        {
            if (_entries.TryGetValue(id, out var entry))
            {
                if (entry.Status == DiscoveryStatus.Unknown)
                {
                    entry.Status = DiscoveryStatus.Sighted;
                }
            }
        }
    }
}

/// <summary>
/// Bestiary milestone achievement.
/// </summary>
public class BestiaryMilestone
{
    public string Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }

    public BestiaryMilestone(string id, string title, string description)
    {
        Id = id;
        Title = title;
        Description = description;
    }
}

/// <summary>
/// Serializable bestiary save data.
/// </summary>
public class BestiarySaveData
{
    public int NextLedgerNumber { get; set; } = 1;
    public List<BestiaryEntrySaveData> Entries { get; set; } = new();
}

/// <summary>
/// Serializable bestiary entry save data.
/// </summary>
public class BestiaryEntrySaveData
{
    public string DefinitionId { get; set; } = "";
    public int Status { get; set; }
    public int EncounterCount { get; set; }
    public int DefeatCount { get; set; }
    public int RecruitCount { get; set; }
    public int LedgerNumber { get; set; }
    public DateTime? FirstEncounter { get; set; }
    public List<string> EncounteredBiomes { get; set; } = new();
    public string? PlayerNotes { get; set; }
}
