using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Strays.Core.Game.World;

/// <summary>
/// Types of world events.
/// </summary>
public enum WorldEventType
{
    /// <summary>
    /// Random encounter with enemies or NPCs.
    /// </summary>
    Encounter,

    /// <summary>
    /// Environmental phenomenon.
    /// </summary>
    Environmental,

    /// <summary>
    /// Story-related event.
    /// </summary>
    Story,

    /// <summary>
    /// Merchant or trader appearing.
    /// </summary>
    Merchant,

    /// <summary>
    /// Treasure discovery.
    /// </summary>
    Treasure,

    /// <summary>
    /// Distress call from NPC.
    /// </summary>
    Distress,

    /// <summary>
    /// Lazarus system event.
    /// </summary>
    Lazarus,

    /// <summary>
    /// Companion-related event.
    /// </summary>
    Companion,

    /// <summary>
    /// Biome-specific phenomenon.
    /// </summary>
    BiomePhenomenon,

    /// <summary>
    /// Timed challenge.
    /// </summary>
    Challenge
}

/// <summary>
/// Urgency level of an event.
/// </summary>
public enum EventUrgency
{
    /// <summary>
    /// Can be ignored.
    /// </summary>
    Optional,

    /// <summary>
    /// Worth investigating.
    /// </summary>
    Notable,

    /// <summary>
    /// Time-sensitive.
    /// </summary>
    Urgent,

    /// <summary>
    /// Cannot be avoided.
    /// </summary>
    Forced
}

/// <summary>
/// Definition of a world event.
/// </summary>
public class WorldEventDefinition
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Description shown to player.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Event type.
    /// </summary>
    public WorldEventType Type { get; init; }

    /// <summary>
    /// Urgency level.
    /// </summary>
    public EventUrgency Urgency { get; init; } = EventUrgency.Optional;

    /// <summary>
    /// Biomes where this event can occur.
    /// </summary>
    public List<BiomeType> ValidBiomes { get; init; } = new();

    /// <summary>
    /// Required story flags.
    /// </summary>
    public List<string> RequiredFlags { get; init; } = new();

    /// <summary>
    /// Blocking story flags.
    /// </summary>
    public List<string> BlockingFlags { get; init; } = new();

    /// <summary>
    /// Minimum player level.
    /// </summary>
    public int MinLevel { get; init; } = 1;

    /// <summary>
    /// Maximum player level (0 = no max).
    /// </summary>
    public int MaxLevel { get; init; } = 0;

    /// <summary>
    /// Base spawn chance (0-1).
    /// </summary>
    public float SpawnChance { get; init; } = 0.1f;

    /// <summary>
    /// Duration in seconds before expiring (0 = no expiration).
    /// </summary>
    public float Duration { get; init; } = 0f;

    /// <summary>
    /// Cooldown in seconds before this event can occur again.
    /// </summary>
    public float Cooldown { get; init; } = 300f;

    /// <summary>
    /// Whether this is a one-time event.
    /// </summary>
    public bool IsOneTime { get; init; } = false;

    /// <summary>
    /// Icon color.
    /// </summary>
    public Color IconColor { get; init; } = Color.Yellow;

    /// <summary>
    /// Dialog ID to start when engaged.
    /// </summary>
    public string? DialogId { get; init; }

    /// <summary>
    /// Encounter ID to start when engaged.
    /// </summary>
    public string? EncounterId { get; init; }

    /// <summary>
    /// Rewards for completing the event.
    /// </summary>
    public EventReward? Reward { get; init; }

    /// <summary>
    /// Flags to set when event completes.
    /// </summary>
    public List<string> SetsFlags { get; init; } = new();

    /// <summary>
    /// Weather required for this event.
    /// </summary>
    public WeatherType? RequiredWeather { get; init; }

    /// <summary>
    /// Time of day requirement (0-24, -1 = any).
    /// </summary>
    public int RequiredHour { get; init; } = -1;
}

/// <summary>
/// Reward for completing a world event.
/// </summary>
public class EventReward
{
    public int Experience { get; init; } = 0;
    public int Currency { get; init; } = 0;
    public List<string> ItemIds { get; init; } = new();
    public int FactionReputation { get; init; } = 0;
    public string? FactionType { get; init; }
}

/// <summary>
/// An active world event instance.
/// </summary>
public class ActiveWorldEvent
{
    /// <summary>
    /// The event definition.
    /// </summary>
    public WorldEventDefinition Definition { get; init; } = null!;

    /// <summary>
    /// World position.
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// Time remaining until expiration.
    /// </summary>
    public float TimeRemaining { get; set; }

    /// <summary>
    /// When this event was spawned.
    /// </summary>
    public DateTime SpawnedAt { get; init; }

    /// <summary>
    /// Whether the player has engaged this event.
    /// </summary>
    public bool IsEngaged { get; set; } = false;

    /// <summary>
    /// Whether this event is complete.
    /// </summary>
    public bool IsComplete { get; set; } = false;

    /// <summary>
    /// Detection radius for the player.
    /// </summary>
    public float DetectionRadius { get; init; } = 100f;
}

/// <summary>
/// System for managing random world events.
/// </summary>
public class WorldEventSystem
{
    private static readonly Dictionary<string, WorldEventDefinition> _definitions = new();
    private readonly Dictionary<string, DateTime> _cooldowns = new();
    private readonly HashSet<string> _completedOneTimeEvents = new();
    private readonly Random _random = new();

    /// <summary>
    /// Currently active events.
    /// </summary>
    public List<ActiveWorldEvent> ActiveEvents { get; } = new();

    /// <summary>
    /// Maximum concurrent events.
    /// </summary>
    public int MaxActiveEvents { get; set; } = 3;

    /// <summary>
    /// Time between event spawn checks (seconds).
    /// </summary>
    public float SpawnCheckInterval { get; set; } = 30f;

    /// <summary>
    /// Time since last spawn check.
    /// </summary>
    private float _spawnCheckTimer = 0f;

    /// <summary>
    /// Event fired when a new event spawns.
    /// </summary>
    public event EventHandler<ActiveWorldEvent>? EventSpawned;

    /// <summary>
    /// Event fired when an event expires.
    /// </summary>
    public event EventHandler<ActiveWorldEvent>? EventExpired;

    /// <summary>
    /// Event fired when an event is completed.
    /// </summary>
    public event EventHandler<(ActiveWorldEvent ev, EventReward? reward)>? EventCompleted;

    /// <summary>
    /// Event fired when player enters event detection range.
    /// </summary>
    public event EventHandler<ActiveWorldEvent>? EventDetected;

    static WorldEventSystem()
    {
        RegisterEvents();
    }

    /// <summary>
    /// Registers all world events.
    /// </summary>
    private static void RegisterEvents()
    {
        // Random encounters
        Register(new WorldEventDefinition
        {
            Id = "evt_wild_stray_pack",
            Name = "Wild Stray Pack",
            Description = "A group of wild Strays has been spotted nearby. They seem agitated.",
            Type = WorldEventType.Encounter,
            Urgency = EventUrgency.Optional,
            ValidBiomes = new List<BiomeType> { BiomeType.Fringe, BiomeType.Rust, BiomeType.Green },
            SpawnChance = 0.15f,
            Duration = 120f,
            Cooldown = 180f,
            IconColor = Color.Red,
            EncounterId = "random_wild_pack",
            Reward = new EventReward { Experience = 50, Currency = 30 }
        });

        Register(new WorldEventDefinition
        {
            Id = "evt_elite_encounter",
            Name = "Elite Threat",
            Description = "An unusually powerful Stray has been detected. Approach with caution.",
            Type = WorldEventType.Encounter,
            Urgency = EventUrgency.Notable,
            ValidBiomes = new List<BiomeType> { BiomeType.Teeth, BiomeType.Glow },
            MinLevel = 15,
            SpawnChance = 0.08f,
            Duration = 180f,
            Cooldown = 600f,
            IconColor = Color.DarkRed,
            EncounterId = "elite_random",
            Reward = new EventReward { Experience = 200, Currency = 150 }
        });

        // Merchant events
        Register(new WorldEventDefinition
        {
            Id = "evt_wandering_trader",
            Name = "Wandering Trader",
            Description = "A traveling merchant has set up a temporary shop nearby.",
            Type = WorldEventType.Merchant,
            Urgency = EventUrgency.Optional,
            ValidBiomes = new List<BiomeType> { BiomeType.Fringe, BiomeType.Rust, BiomeType.Green, BiomeType.Quiet },
            SpawnChance = 0.1f,
            Duration = 300f,
            Cooldown = 600f,
            IconColor = Color.Gold,
            DialogId = "wandering_trader_greet"
        });

        Register(new WorldEventDefinition
        {
            Id = "evt_machinist_caravan",
            Name = "Machinist Caravan",
            Description = "A Machinist caravan has stopped to rest. They might have rare components.",
            Type = WorldEventType.Merchant,
            Urgency = EventUrgency.Notable,
            ValidBiomes = new List<BiomeType> { BiomeType.Rust, BiomeType.Teeth, BiomeType.Glow },
            MinLevel = 10,
            SpawnChance = 0.05f,
            Duration = 240f,
            Cooldown = 900f,
            IconColor = Color.Silver,
            DialogId = "machinist_caravan_greet",
            Reward = new EventReward { FactionReputation = 5, FactionType = "Machinists" }
        });

        // Distress events
        Register(new WorldEventDefinition
        {
            Id = "evt_stray_distress",
            Name = "Stray in Distress",
            Description = "A wounded Stray is calling for help. It might be grateful if rescued.",
            Type = WorldEventType.Distress,
            Urgency = EventUrgency.Urgent,
            ValidBiomes = new List<BiomeType> { BiomeType.Fringe, BiomeType.Green, BiomeType.Quiet },
            SpawnChance = 0.08f,
            Duration = 90f,
            Cooldown = 300f,
            IconColor = Color.Orange,
            DialogId = "stray_rescue",
            Reward = new EventReward { Experience = 40, FactionReputation = 10, FactionType = "Strays" }
        });

        Register(new WorldEventDefinition
        {
            Id = "evt_npc_ambush",
            Name = "NPC Under Attack",
            Description = "An Independent is being attacked by hostile Strays!",
            Type = WorldEventType.Distress,
            Urgency = EventUrgency.Urgent,
            ValidBiomes = new List<BiomeType> { BiomeType.Fringe, BiomeType.Rust, BiomeType.Green, BiomeType.Teeth },
            MinLevel = 5,
            SpawnChance = 0.06f,
            Duration = 60f,
            Cooldown = 400f,
            IconColor = Color.Red,
            EncounterId = "npc_rescue_battle",
            Reward = new EventReward { Experience = 80, Currency = 50, FactionReputation = 15, FactionType = "Independents" }
        });

        // Environmental events
        Register(new WorldEventDefinition
        {
            Id = "evt_radiation_surge",
            Name = "Radiation Surge",
            Description = "Dangerous radiation levels detected. Find shelter immediately!",
            Type = WorldEventType.Environmental,
            Urgency = EventUrgency.Forced,
            ValidBiomes = new List<BiomeType> { BiomeType.Glow, BiomeType.ArchiveScar },
            MinLevel = 20,
            SpawnChance = 0.04f,
            Duration = 45f,
            Cooldown = 600f,
            IconColor = Color.Yellow,
            RequiredWeather = WeatherType.RadiationWind
        });

        Register(new WorldEventDefinition
        {
            Id = "evt_data_anomaly",
            Name = "Data Anomaly",
            Description = "Strange data patterns are manifesting in physical space...",
            Type = WorldEventType.Environmental,
            Urgency = EventUrgency.Notable,
            ValidBiomes = new List<BiomeType> { BiomeType.Glow, BiomeType.ArchiveScar },
            MinLevel = 15,
            SpawnChance = 0.05f,
            Duration = 120f,
            Cooldown = 500f,
            IconColor = Color.Cyan,
            RequiredWeather = WeatherType.DataStorm,
            Reward = new EventReward { Experience = 100 }
        });

        // Treasure events
        Register(new WorldEventDefinition
        {
            Id = "evt_hidden_cache",
            Name = "Hidden Cache",
            Description = "You've detected a hidden supply cache nearby.",
            Type = WorldEventType.Treasure,
            Urgency = EventUrgency.Optional,
            ValidBiomes = new List<BiomeType> { BiomeType.Fringe, BiomeType.Rust, BiomeType.Quiet },
            SpawnChance = 0.08f,
            Duration = 180f,
            Cooldown = 400f,
            IconColor = Color.Gold,
            Reward = new EventReward { Currency = 75, ItemIds = new List<string> { "repair_kit_small", "energy_cell" } }
        });

        Register(new WorldEventDefinition
        {
            Id = "evt_rare_chip_signal",
            Name = "Strange Signal",
            Description = "A faint signal suggests a rare microchip nearby...",
            Type = WorldEventType.Treasure,
            Urgency = EventUrgency.Notable,
            ValidBiomes = new List<BiomeType> { BiomeType.Teeth, BiomeType.Glow },
            MinLevel = 20,
            SpawnChance = 0.03f,
            Duration = 150f,
            Cooldown = 900f,
            IconColor = Color.Magenta,
            EncounterId = "chip_guardian",
            Reward = new EventReward { ItemIds = new List<string> { "random_rare_chip" } }
        });

        // Lazarus events
        Register(new WorldEventDefinition
        {
            Id = "evt_nimdok_drone",
            Name = "Lazarus Drone Patrol",
            Description = "Lazarus surveillance drones are scanning the area.",
            Type = WorldEventType.Lazarus,
            Urgency = EventUrgency.Notable,
            ValidBiomes = new List<BiomeType> { BiomeType.Quiet, BiomeType.Glow },
            SpawnChance = 0.07f,
            Duration = 90f,
            Cooldown = 300f,
            IconColor = Color.Cyan
        });

        Register(new WorldEventDefinition
        {
            Id = "evt_nimdok_message",
            Name = "Lazarus Broadcast",
            Description = "A Lazarus terminal is transmitting a message...",
            Type = WorldEventType.Lazarus,
            Urgency = EventUrgency.Optional,
            ValidBiomes = new List<BiomeType> { BiomeType.Glow, BiomeType.ArchiveScar },
            MinLevel = 15,
            SpawnChance = 0.04f,
            Duration = 120f,
            Cooldown = 600f,
            IconColor = Color.Cyan,
            DialogId = "nimdok_broadcast",
            Reward = new EventReward { FactionReputation = 5, FactionType = "Lazarus" }
        });

        // Companion events
        Register(new WorldEventDefinition
        {
            Id = "evt_companion_memory",
            Name = "Bandit's Memory",
            Description = "Bandit seems to recognize this place...",
            Type = WorldEventType.Companion,
            Urgency = EventUrgency.Notable,
            ValidBiomes = new List<BiomeType> { BiomeType.Fringe, BiomeType.Green },
            RequiredFlags = new List<string> { "met_companion" },
            BlockingFlags = new List<string> { "companion_departed" },
            SpawnChance = 0.03f,
            Duration = 0f, // No expiration
            Cooldown = 1200f,
            IsOneTime = true,
            IconColor = Color.Orange,
            DialogId = "companion_memory_1"
        });

        // Story events
        Register(new WorldEventDefinition
        {
            Id = "evt_mysterious_figure",
            Name = "Mysterious Figure",
            Description = "Someone... or something... is watching you from the shadows.",
            Type = WorldEventType.Story,
            Urgency = EventUrgency.Notable,
            ValidBiomes = new List<BiomeType> { BiomeType.ArchiveScar },
            RequiredFlags = new List<string> { "discovered_archive_scar" },
            MinLevel = 20,
            SpawnChance = 0.02f,
            Duration = 60f,
            Cooldown = 1800f,
            IsOneTime = true,
            IconColor = Color.Purple,
            DialogId = "mysterious_figure",
            SetsFlags = new List<string> { "encountered_mysterious_figure" }
        });

        // Challenge events
        Register(new WorldEventDefinition
        {
            Id = "evt_gauntlet_challenge",
            Name = "Gauntlet Challenge",
            Description = "A combat challenge has appeared! Defeat waves of enemies for rewards.",
            Type = WorldEventType.Challenge,
            Urgency = EventUrgency.Optional,
            ValidBiomes = new List<BiomeType> { BiomeType.Teeth },
            MinLevel = 20,
            SpawnChance = 0.04f,
            Duration = 300f,
            Cooldown = 900f,
            IconColor = Color.Crimson,
            EncounterId = "gauntlet_wave_1",
            Reward = new EventReward { Experience = 300, Currency = 200 }
        });
    }

    /// <summary>
    /// Registers an event definition.
    /// </summary>
    private static void Register(WorldEventDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }

    /// <summary>
    /// Updates the world event system.
    /// </summary>
    public void Update(GameTime gameTime, BiomeType currentBiome, Vector2 playerPosition, int playerLevel, HashSet<string> storyFlags, WeatherType currentWeather)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update active events
        for (int i = ActiveEvents.Count - 1; i >= 0; i--)
        {
            var ev = ActiveEvents[i];

            // Check expiration
            if (ev.Definition.Duration > 0)
            {
                ev.TimeRemaining -= deltaTime;

                if (ev.TimeRemaining <= 0 && !ev.IsEngaged)
                {
                    EventExpired?.Invoke(this, ev);
                    ActiveEvents.RemoveAt(i);
                    continue;
                }
            }

            // Check if player is in detection range
            if (!ev.IsEngaged)
            {
                float distance = Vector2.Distance(playerPosition, ev.Position);

                if (distance < ev.DetectionRadius)
                {
                    EventDetected?.Invoke(this, ev);
                }
            }

            // Remove completed events
            if (ev.IsComplete)
            {
                ActiveEvents.RemoveAt(i);
            }
        }

        // Check for new event spawns
        _spawnCheckTimer += deltaTime;

        if (_spawnCheckTimer >= SpawnCheckInterval)
        {
            _spawnCheckTimer = 0f;
            TrySpawnEvent(currentBiome, playerPosition, playerLevel, storyFlags, currentWeather);
        }
    }

    /// <summary>
    /// Tries to spawn a new event.
    /// </summary>
    private void TrySpawnEvent(BiomeType biome, Vector2 playerPosition, int level, HashSet<string> flags, WeatherType weather)
    {
        if (ActiveEvents.Count >= MaxActiveEvents)
        {
            return;
        }

        // Get eligible events
        var eligible = _definitions.Values
            .Where(d => IsEventEligible(d, biome, level, flags, weather))
            .ToList();

        if (eligible.Count == 0)
        {
            return;
        }

        // Roll for each eligible event
        foreach (var def in eligible)
        {
            if (_random.NextDouble() < def.SpawnChance)
            {
                SpawnEvent(def, playerPosition);
                break; // Only spawn one event per check
            }
        }
    }

    /// <summary>
    /// Checks if an event is eligible to spawn.
    /// </summary>
    private bool IsEventEligible(WorldEventDefinition def, BiomeType biome, int level, HashSet<string> flags, WeatherType weather)
    {
        // Check biome
        if (def.ValidBiomes.Count > 0 && !def.ValidBiomes.Contains(biome))
        {
            return false;
        }

        // Check level
        if (level < def.MinLevel)
        {
            return false;
        }

        if (def.MaxLevel > 0 && level > def.MaxLevel)
        {
            return false;
        }

        // Check required flags
        foreach (var flag in def.RequiredFlags)
        {
            if (!flags.Contains(flag))
            {
                return false;
            }
        }

        // Check blocking flags
        foreach (var flag in def.BlockingFlags)
        {
            if (flags.Contains(flag))
            {
                return false;
            }
        }

        // Check weather
        if (def.RequiredWeather.HasValue && def.RequiredWeather.Value != weather)
        {
            return false;
        }

        // Check one-time events
        if (def.IsOneTime && _completedOneTimeEvents.Contains(def.Id))
        {
            return false;
        }

        // Check cooldown
        if (_cooldowns.TryGetValue(def.Id, out var lastSpawn))
        {
            if ((DateTime.Now - lastSpawn).TotalSeconds < def.Cooldown)
            {
                return false;
            }
        }

        // Check if already active
        if (ActiveEvents.Any(e => e.Definition.Id == def.Id))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Spawns an event at a position near the player.
    /// </summary>
    private void SpawnEvent(WorldEventDefinition def, Vector2 playerPosition)
    {
        // Random offset from player (100-500 units away)
        float distance = 100f + _random.NextSingle() * 400f;
        float angle = _random.NextSingle() * MathHelper.TwoPi;
        var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;

        var ev = new ActiveWorldEvent
        {
            Definition = def,
            Position = playerPosition + offset,
            TimeRemaining = def.Duration,
            SpawnedAt = DateTime.Now,
            DetectionRadius = def.Urgency == EventUrgency.Forced ? 300f : 100f
        };

        ActiveEvents.Add(ev);
        _cooldowns[def.Id] = DateTime.Now;

        EventSpawned?.Invoke(this, ev);
    }

    /// <summary>
    /// Forces an event to spawn.
    /// </summary>
    public void ForceSpawn(string eventId, Vector2 position)
    {
        if (!_definitions.TryGetValue(eventId, out var def))
        {
            return;
        }

        var ev = new ActiveWorldEvent
        {
            Definition = def,
            Position = position,
            TimeRemaining = def.Duration,
            SpawnedAt = DateTime.Now
        };

        ActiveEvents.Add(ev);
        _cooldowns[def.Id] = DateTime.Now;

        EventSpawned?.Invoke(this, ev);
    }

    /// <summary>
    /// Engages an event (player interacts with it).
    /// </summary>
    public void EngageEvent(ActiveWorldEvent ev)
    {
        ev.IsEngaged = true;
    }

    /// <summary>
    /// Completes an event.
    /// </summary>
    public void CompleteEvent(ActiveWorldEvent ev, bool success = true)
    {
        ev.IsComplete = true;

        if (success && ev.Definition.IsOneTime)
        {
            _completedOneTimeEvents.Add(ev.Definition.Id);
        }

        EventCompleted?.Invoke(this, (ev, success ? ev.Definition.Reward : null));
    }

    /// <summary>
    /// Gets all active events sorted by distance from player.
    /// </summary>
    public IEnumerable<ActiveWorldEvent> GetActiveEventsByDistance(Vector2 playerPosition)
    {
        return ActiveEvents
            .OrderBy(e => Vector2.Distance(e.Position, playerPosition));
    }

    /// <summary>
    /// Gets the nearest event of a specific type.
    /// </summary>
    public ActiveWorldEvent? GetNearestEventOfType(WorldEventType type, Vector2 playerPosition)
    {
        return ActiveEvents
            .Where(e => e.Definition.Type == type && !e.IsEngaged)
            .OrderBy(e => Vector2.Distance(e.Position, playerPosition))
            .FirstOrDefault();
    }

    /// <summary>
    /// Exports completed one-time events for saving.
    /// </summary>
    public HashSet<string> ExportCompletedEvents()
    {
        return new HashSet<string>(_completedOneTimeEvents);
    }

    /// <summary>
    /// Imports completed one-time events from save data.
    /// </summary>
    public void ImportCompletedEvents(IEnumerable<string> completed)
    {
        _completedOneTimeEvents.Clear();
        foreach (var id in completed)
        {
            _completedOneTimeEvents.Add(id);
        }
    }
}
