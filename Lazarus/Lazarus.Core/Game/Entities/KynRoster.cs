using System;
using System.Collections.Generic;
using System.Linq;
using Lazarus.Core.Game.Data;

namespace Lazarus.Core.Game.Entities;

/// <summary>
/// Manages the player's collection of Kyns, including the active party and storage.
/// </summary>
public class KynRoster
{
    /// <summary>
    /// Maximum number of Kyns in the active party.
    /// </summary>
    public const int MaxPartySize = 5;

    /// <summary>
    /// Maximum number of Kyns that can be stored.
    /// </summary>
    public const int MaxStorageSize = 185; // 190 total - 5 party

    private readonly List<Kyn> _party = new();
    private readonly List<Kyn> _storage = new();
    private readonly Dictionary<string, Kyn> _allKyns = new();

    /// <summary>
    /// The active party of Kyns (max 5).
    /// </summary>
    public IReadOnlyList<Kyn> Party => _party;

    /// <summary>
    /// Kyns in storage.
    /// </summary>
    public IReadOnlyList<Kyn> Storage => _storage;

    /// <summary>
    /// Total number of Kyns owned.
    /// </summary>
    public int TotalCount => _party.Count + _storage.Count;

    /// <summary>
    /// Number of living Kyns in the party.
    /// </summary>
    public int AlivePartyCount => _party.Count(s => s.IsAlive);

    /// <summary>
    /// Whether the party has any living Kyns.
    /// </summary>
    public bool HasAlivePartyMember => _party.Any(s => s.IsAlive);

    /// <summary>
    /// Event fired when a Kyn is added to the roster.
    /// </summary>
    public event EventHandler<Kyn>? KynAdded;

    /// <summary>
    /// Event fired when a Kyn is removed from the roster.
    /// </summary>
    public event EventHandler<Kyn>? KynRemoved;

    /// <summary>
    /// Event fired when the party composition changes.
    /// </summary>
    public event EventHandler? PartyChanged;

    /// <summary>
    /// Adds a Kyn to the roster.
    /// If the party isn't full, adds to party; otherwise adds to storage.
    /// </summary>
    /// <param name="kyn">The Kyn to add.</param>
    /// <returns>True if added successfully.</returns>
    public bool AddKyn(Kyn kyn)
    {
        if (_allKyns.ContainsKey(kyn.InstanceId))
            return false;

        if (_party.Count < MaxPartySize)
        {
            _party.Add(kyn);
            PartyChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (_storage.Count < MaxStorageSize)
        {
            _storage.Add(kyn);
        }
        else
        {
            return false; // Roster full
        }

        _allKyns[kyn.InstanceId] = kyn;
        KynAdded?.Invoke(this, kyn);
        return true;
    }

    /// <summary>
    /// Removes a Kyn from the roster entirely.
    /// </summary>
    /// <param name="kyn">The Kyn to remove.</param>
    /// <returns>True if removed successfully.</returns>
    public bool RemoveKyn(Kyn kyn)
    {
        if (!_allKyns.ContainsKey(kyn.InstanceId))
            return false;

        bool wasInParty = _party.Remove(kyn);
        bool wasInStorage = _storage.Remove(kyn);

        if (wasInParty || wasInStorage)
        {
            _allKyns.Remove(kyn.InstanceId);
            KynRemoved?.Invoke(this, kyn);

            if (wasInParty)
            {
                PartyChanged?.Invoke(this, EventArgs.Empty);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Moves a Kyn from storage to party.
    /// </summary>
    /// <param name="kyn">The Kyn to move.</param>
    /// <returns>True if moved successfully.</returns>
    public bool MoveToParty(Kyn kyn)
    {
        if (_party.Count >= MaxPartySize)
            return false;

        if (!_storage.Remove(kyn))
            return false;

        _party.Add(kyn);
        PartyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Moves a Kyn from party to storage.
    /// </summary>
    /// <param name="kyn">The Kyn to move.</param>
    /// <returns>True if moved successfully.</returns>
    public bool MoveToStorage(Kyn kyn)
    {
        if (_storage.Count >= MaxStorageSize)
            return false;

        if (!_party.Remove(kyn))
            return false;

        _storage.Add(kyn);
        PartyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Swaps a party Kyn with a storage Kyn.
    /// </summary>
    /// <param name="partyKyn">Kyn currently in party.</param>
    /// <param name="storageKyn">Kyn currently in storage.</param>
    /// <returns>True if swapped successfully.</returns>
    public bool SwapKyns(Kyn partyKyn, Kyn storageKyn)
    {
        int partyIndex = _party.IndexOf(partyKyn);
        int storageIndex = _storage.IndexOf(storageKyn);

        if (partyIndex < 0 || storageIndex < 0)
            return false;

        _party[partyIndex] = storageKyn;
        _storage[storageIndex] = partyKyn;

        PartyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Reorders a Kyn within the party.
    /// </summary>
    /// <param name="kyn">The Kyn to move.</param>
    /// <param name="newIndex">New position in party.</param>
    /// <returns>True if reordered successfully.</returns>
    public bool ReorderParty(Kyn kyn, int newIndex)
    {
        int currentIndex = _party.IndexOf(kyn);
        if (currentIndex < 0)
            return false;

        if (newIndex < 0 || newIndex >= _party.Count)
            return false;

        _party.RemoveAt(currentIndex);
        _party.Insert(newIndex, kyn);

        PartyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Gets a Kyn by instance ID.
    /// </summary>
    public Kyn? GetKyn(string instanceId)
    {
        return _allKyns.TryGetValue(instanceId, out var kyn) ? kyn : null;
    }

    /// <summary>
    /// Gets all Kyns of a specific creature type.
    /// </summary>
    public IEnumerable<Kyn> GetKynsByCreatureType(CreatureType type)
    {
        return _allKyns.Values.Where(s => s.Definition.CreatureType == type);
    }

    /// <summary>
    /// Gets all Kyns of a specific creature category.
    /// </summary>
    public IEnumerable<Kyn> GetKynsByCategory(CreatureCategory category)
    {
        return _allKyns.Values.Where(s => s.Definition.Category == category);
    }

    /// <summary>
    /// Gets all Kyns of a specific definition.
    /// </summary>
    public IEnumerable<Kyn> GetKynsByDefinition(string definitionId)
    {
        return _allKyns.Values.Where(s => s.Definition.Id == definitionId);
    }

    /// <summary>
    /// Heals all Kyns in the party to full HP.
    /// </summary>
    public void HealParty()
    {
        foreach (var kyn in _party)
        {
            kyn.FullHeal();
        }
    }

    /// <summary>
    /// Revives all dead Kyns in the party.
    /// </summary>
    /// <param name="hpPercent">HP percentage to revive with.</param>
    public void ReviveParty(float hpPercent = 0.5f)
    {
        foreach (var kyn in _party)
        {
            if (!kyn.IsAlive)
            {
                kyn.Revive(hpPercent);
            }
        }
    }

    /// <summary>
    /// Awards experience to all living party members.
    /// </summary>
    /// <param name="amount">Total experience to distribute.</param>
    /// <returns>List of Kyns that leveled up.</returns>
    public List<Kyn> AwardExperience(int amount)
    {
        var leveledUp = new List<Kyn>();
        int aliveCount = AlivePartyCount;

        if (aliveCount == 0)
            return leveledUp;

        // Distribute experience evenly among living party members
        int expPerKyn = amount / aliveCount;

        foreach (var kyn in _party.Where(s => s.IsAlive))
        {
            if (kyn.AddExperience(expPerKyn))
            {
                leveledUp.Add(kyn);
            }
        }

        return leveledUp;
    }

    /// <summary>
    /// Creates save data for the roster.
    /// </summary>
    public (List<string> PartyIds, List<string> RosterIds, Dictionary<string, KynSaveData> KynData) ToSaveData()
    {
        var partyIds = _party.Select(s => s.InstanceId).ToList();
        var rosterIds = _storage.Select(s => s.InstanceId).ToList();
        var kynData = _allKyns.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToSaveData());

        return (partyIds, rosterIds, kynData);
    }

    /// <summary>
    /// Loads the roster from save data.
    /// </summary>
    public void LoadFromSaveData(List<string> partyIds, List<string> rosterIds, Dictionary<string, KynSaveData> kynData)
    {
        _party.Clear();
        _storage.Clear();
        _allKyns.Clear();

        // Recreate all Kyns
        foreach (var kvp in kynData)
        {
            var kyn = Kyn.FromSaveData(kvp.Value);
            if (kyn != null)
            {
                _allKyns[kvp.Key] = kyn;
            }
        }

        // Rebuild party
        foreach (var id in partyIds)
        {
            if (_allKyns.TryGetValue(id, out var kyn))
            {
                _party.Add(kyn);
            }
        }

        // Rebuild storage
        foreach (var id in rosterIds)
        {
            if (_allKyns.TryGetValue(id, out var kyn))
            {
                _storage.Add(kyn);
            }
        }

        PartyChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears the entire roster.
    /// </summary>
    public void Clear()
    {
        _party.Clear();
        _storage.Clear();
        _allKyns.Clear();
        PartyChanged?.Invoke(this, EventArgs.Empty);
    }
}
