using System;
using System.Collections.Generic;
using System.Linq;
using Strays.Core.Game.Data;

namespace Strays.Core.Game.Entities;

/// <summary>
/// Manages the player's collection of Strays, including the active party and storage.
/// </summary>
public class StrayRoster
{
    /// <summary>
    /// Maximum number of Strays in the active party.
    /// </summary>
    public const int MaxPartySize = 5;

    /// <summary>
    /// Maximum number of Strays that can be stored.
    /// </summary>
    public const int MaxStorageSize = 185; // 190 total - 5 party

    private readonly List<Stray> _party = new();
    private readonly List<Stray> _storage = new();
    private readonly Dictionary<string, Stray> _allStrays = new();

    /// <summary>
    /// The active party of Strays (max 5).
    /// </summary>
    public IReadOnlyList<Stray> Party => _party;

    /// <summary>
    /// Strays in storage.
    /// </summary>
    public IReadOnlyList<Stray> Storage => _storage;

    /// <summary>
    /// Total number of Strays owned.
    /// </summary>
    public int TotalCount => _party.Count + _storage.Count;

    /// <summary>
    /// Number of living Strays in the party.
    /// </summary>
    public int AlivePartyCount => _party.Count(s => s.IsAlive);

    /// <summary>
    /// Whether the party has any living Strays.
    /// </summary>
    public bool HasAlivePartyMember => _party.Any(s => s.IsAlive);

    /// <summary>
    /// Event fired when a Stray is added to the roster.
    /// </summary>
    public event EventHandler<Stray>? StrayAdded;

    /// <summary>
    /// Event fired when a Stray is removed from the roster.
    /// </summary>
    public event EventHandler<Stray>? StrayRemoved;

    /// <summary>
    /// Event fired when the party composition changes.
    /// </summary>
    public event EventHandler? PartyChanged;

    /// <summary>
    /// Adds a Stray to the roster.
    /// If the party isn't full, adds to party; otherwise adds to storage.
    /// </summary>
    /// <param name="stray">The Stray to add.</param>
    /// <returns>True if added successfully.</returns>
    public bool AddStray(Stray stray)
    {
        if (_allStrays.ContainsKey(stray.InstanceId))
            return false;

        if (_party.Count < MaxPartySize)
        {
            _party.Add(stray);
            PartyChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (_storage.Count < MaxStorageSize)
        {
            _storage.Add(stray);
        }
        else
        {
            return false; // Roster full
        }

        _allStrays[stray.InstanceId] = stray;
        StrayAdded?.Invoke(this, stray);
        return true;
    }

    /// <summary>
    /// Removes a Stray from the roster entirely.
    /// </summary>
    /// <param name="stray">The Stray to remove.</param>
    /// <returns>True if removed successfully.</returns>
    public bool RemoveStray(Stray stray)
    {
        if (!_allStrays.ContainsKey(stray.InstanceId))
            return false;

        bool wasInParty = _party.Remove(stray);
        bool wasInStorage = _storage.Remove(stray);

        if (wasInParty || wasInStorage)
        {
            _allStrays.Remove(stray.InstanceId);
            StrayRemoved?.Invoke(this, stray);

            if (wasInParty)
            {
                PartyChanged?.Invoke(this, EventArgs.Empty);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Moves a Stray from storage to party.
    /// </summary>
    /// <param name="stray">The Stray to move.</param>
    /// <returns>True if moved successfully.</returns>
    public bool MoveToParty(Stray stray)
    {
        if (_party.Count >= MaxPartySize)
            return false;

        if (!_storage.Remove(stray))
            return false;

        _party.Add(stray);
        PartyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Moves a Stray from party to storage.
    /// </summary>
    /// <param name="stray">The Stray to move.</param>
    /// <returns>True if moved successfully.</returns>
    public bool MoveToStorage(Stray stray)
    {
        if (_storage.Count >= MaxStorageSize)
            return false;

        if (!_party.Remove(stray))
            return false;

        _storage.Add(stray);
        PartyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Swaps a party Stray with a storage Stray.
    /// </summary>
    /// <param name="partyStray">Stray currently in party.</param>
    /// <param name="storageStray">Stray currently in storage.</param>
    /// <returns>True if swapped successfully.</returns>
    public bool SwapStrays(Stray partyStray, Stray storageStray)
    {
        int partyIndex = _party.IndexOf(partyStray);
        int storageIndex = _storage.IndexOf(storageStray);

        if (partyIndex < 0 || storageIndex < 0)
            return false;

        _party[partyIndex] = storageStray;
        _storage[storageIndex] = partyStray;

        PartyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Reorders a Stray within the party.
    /// </summary>
    /// <param name="stray">The Stray to move.</param>
    /// <param name="newIndex">New position in party.</param>
    /// <returns>True if reordered successfully.</returns>
    public bool ReorderParty(Stray stray, int newIndex)
    {
        int currentIndex = _party.IndexOf(stray);
        if (currentIndex < 0)
            return false;

        if (newIndex < 0 || newIndex >= _party.Count)
            return false;

        _party.RemoveAt(currentIndex);
        _party.Insert(newIndex, stray);

        PartyChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Gets a Stray by instance ID.
    /// </summary>
    public Stray? GetStray(string instanceId)
    {
        return _allStrays.TryGetValue(instanceId, out var stray) ? stray : null;
    }

    /// <summary>
    /// Gets all Strays of a specific type.
    /// </summary>
    public IEnumerable<Stray> GetStraysByType(StrayType type)
    {
        return _allStrays.Values.Where(s => s.Definition.Type == type);
    }

    /// <summary>
    /// Gets all Strays of a specific definition.
    /// </summary>
    public IEnumerable<Stray> GetStraysByDefinition(string definitionId)
    {
        return _allStrays.Values.Where(s => s.Definition.Id == definitionId);
    }

    /// <summary>
    /// Heals all Strays in the party to full HP.
    /// </summary>
    public void HealParty()
    {
        foreach (var stray in _party)
        {
            stray.FullHeal();
        }
    }

    /// <summary>
    /// Revives all dead Strays in the party.
    /// </summary>
    /// <param name="hpPercent">HP percentage to revive with.</param>
    public void ReviveParty(float hpPercent = 0.5f)
    {
        foreach (var stray in _party)
        {
            if (!stray.IsAlive)
            {
                stray.Revive(hpPercent);
            }
        }
    }

    /// <summary>
    /// Awards experience to all living party members.
    /// </summary>
    /// <param name="amount">Total experience to distribute.</param>
    /// <returns>List of Strays that leveled up.</returns>
    public List<Stray> AwardExperience(int amount)
    {
        var leveledUp = new List<Stray>();
        int aliveCount = AlivePartyCount;

        if (aliveCount == 0)
            return leveledUp;

        // Distribute experience evenly among living party members
        int expPerStray = amount / aliveCount;

        foreach (var stray in _party.Where(s => s.IsAlive))
        {
            if (stray.AddExperience(expPerStray))
            {
                leveledUp.Add(stray);
            }
        }

        return leveledUp;
    }

    /// <summary>
    /// Creates save data for the roster.
    /// </summary>
    public (List<string> PartyIds, List<string> RosterIds, Dictionary<string, StraySaveData> StrayData) ToSaveData()
    {
        var partyIds = _party.Select(s => s.InstanceId).ToList();
        var rosterIds = _storage.Select(s => s.InstanceId).ToList();
        var strayData = _allStrays.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToSaveData());

        return (partyIds, rosterIds, strayData);
    }

    /// <summary>
    /// Loads the roster from save data.
    /// </summary>
    public void LoadFromSaveData(List<string> partyIds, List<string> rosterIds, Dictionary<string, StraySaveData> strayData)
    {
        _party.Clear();
        _storage.Clear();
        _allStrays.Clear();

        // Recreate all Strays
        foreach (var kvp in strayData)
        {
            var stray = Stray.FromSaveData(kvp.Value);
            if (stray != null)
            {
                _allStrays[kvp.Key] = stray;
            }
        }

        // Rebuild party
        foreach (var id in partyIds)
        {
            if (_allStrays.TryGetValue(id, out var stray))
            {
                _party.Add(stray);
            }
        }

        // Rebuild storage
        foreach (var id in rosterIds)
        {
            if (_allStrays.TryGetValue(id, out var stray))
            {
                _storage.Add(stray);
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
        _allStrays.Clear();
        PartyChanged?.Invoke(this, EventArgs.Empty);
    }
}
