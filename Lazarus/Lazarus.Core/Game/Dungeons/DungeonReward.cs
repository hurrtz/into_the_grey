using System;
using System.Collections.Generic;

namespace Lazarus.Core.Game.Dungeons;

/// <summary>
/// Rarity tiers for dungeon rewards.
/// </summary>
public enum RewardRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// A single reward item from a dungeon.
/// </summary>
public class RewardItem
{
    /// <summary>
    /// Type of reward (augment, microchip, material, currency).
    /// </summary>
    public string Type { get; init; } = "material";

    /// <summary>
    /// Item ID for augments/microchips, or material name.
    /// </summary>
    public string ItemId { get; init; } = "";

    /// <summary>
    /// Display name of the reward.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Quantity of this reward.
    /// </summary>
    public int Quantity { get; init; } = 1;

    /// <summary>
    /// Rarity tier.
    /// </summary>
    public RewardRarity Rarity { get; init; } = RewardRarity.Common;
}

/// <summary>
/// Rewards earned from completing a dungeon or room.
/// </summary>
public class DungeonReward
{
    /// <summary>
    /// Experience points earned.
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Currency (scrap/credits) earned.
    /// </summary>
    public int Currency { get; set; }

    /// <summary>
    /// Items earned.
    /// </summary>
    public List<RewardItem> Items { get; set; } = new();

    /// <summary>
    /// Bonus rewards (from difficulty, completion bonus, etc).
    /// </summary>
    public List<RewardItem> BonusItems { get; set; } = new();

    /// <summary>
    /// Whether this reward has been claimed.
    /// </summary>
    public bool Claimed { get; set; } = false;

    /// <summary>
    /// Combines another reward into this one.
    /// </summary>
    public void Add(DungeonReward other)
    {
        Experience += other.Experience;
        Currency += other.Currency;
        Items.AddRange(other.Items);
        BonusItems.AddRange(other.BonusItems);
    }

    /// <summary>
    /// Applies a multiplier to currency and experience.
    /// </summary>
    public void ApplyMultiplier(float multiplier)
    {
        Experience = (int)(Experience * multiplier);
        Currency = (int)(Currency * multiplier);
    }

    /// <summary>
    /// Gets total item count.
    /// </summary>
    public int TotalItemCount => Items.Count + BonusItems.Count;
}

/// <summary>
/// Generates rewards for dungeon completion.
/// </summary>
public static class DungeonRewardGenerator
{
    private static readonly Random _random = new();

    /// <summary>
    /// Standard material rewards by biome.
    /// </summary>
    private static readonly Dictionary<string, string[]> BiomeMaterials = new()
    {
        ["fringe"] = new[] { "Scrap Metal", "Frayed Wire", "Cracked Lens", "Rusted Gear" },
        ["rust"] = new[] { "Corroded Plate", "Acid Residue", "Rust Chunk", "Degraded Circuit" },
        ["green"] = new[] { "Bio-Matter", "Mutated Fiber", "Toxic Sap", "Overgrown Core" },
        ["quiet"] = new[] { "Silent Crystal", "Dampened Node", "Echo Fragment", "Null Essence" },
        ["teeth"] = new[] { "Bone Shard", "Calcified Shell", "Fang Fragment", "Marrow Extract" },
        ["glow"] = new[] { "Radiant Dust", "Glow Essence", "Charged Crystal", "Luminous Core" },
        ["archive"] = new[] { "Data Fragment", "Memory Chip", "Archive Core", "Lazarus Shard" }
    };

    /// <summary>
    /// Generates room clear reward.
    /// </summary>
    public static DungeonReward GenerateRoomReward(DungeonDefinition dungeon, DungeonRoom room,
        DungeonDifficulty difficulty)
    {
        var reward = new DungeonReward
        {
            Experience = room.ExpReward,
            Currency = (int)(dungeon.BaseCurrencyReward * 0.1f * DifficultyModifiers.GetRewardMultiplier(difficulty))
        };

        // Add material drops
        var biomeKey = dungeon.Biome.ToString().ToLowerInvariant();
        if (BiomeMaterials.TryGetValue(biomeKey, out var materials))
        {
            int dropCount = room.Type switch
            {
                RoomType.MidBoss => 2,
                RoomType.FinalBoss => 4,
                _ => _random.Next(0, 2)
            };

            for (int i = 0; i < dropCount; i++)
            {
                var material = materials[_random.Next(materials.Length)];
                reward.Items.Add(new RewardItem
                {
                    Type = "material",
                    ItemId = material.ToLowerInvariant().Replace(" ", "_"),
                    Name = material,
                    Quantity = _random.Next(1, 4),
                    Rarity = GetRandomRarity(difficulty, room.Type == RoomType.FinalBoss)
                });
            }
        }

        // Boss rooms drop special items
        if (room.Type == RoomType.FinalBoss)
        {
            reward.BonusItems.Add(GenerateBossReward(dungeon, difficulty));
        }
        else if (room.Type == RoomType.MidBoss)
        {
            if (_random.NextDouble() < 0.5 + (int)difficulty * 0.15)
            {
                reward.BonusItems.Add(GenerateMidBossReward(dungeon, difficulty));
            }
        }

        return reward;
    }

    /// <summary>
    /// Generates completion bonus reward.
    /// </summary>
    public static DungeonReward GenerateCompletionBonus(DungeonDefinition dungeon,
        DungeonDifficulty difficulty, int roomsCleared, int totalRooms)
    {
        float completionRatio = (float)roomsCleared / totalRooms;
        float multiplier = DifficultyModifiers.GetRewardMultiplier(difficulty);

        var reward = new DungeonReward
        {
            Experience = (int)(dungeon.BaseExpReward * multiplier * completionRatio),
            Currency = (int)(dungeon.BaseCurrencyReward * multiplier * completionRatio)
        };

        // Full clear bonus
        if (roomsCleared == totalRooms)
        {
            reward.Experience = (int)(reward.Experience * 1.5f);
            reward.Currency = (int)(reward.Currency * 1.5f);

            reward.BonusItems.Add(new RewardItem
            {
                Type = "bonus",
                ItemId = "completion_bonus",
                Name = "Full Clear Bonus",
                Quantity = 1,
                Rarity = RewardRarity.Rare
            });
        }

        return reward;
    }

    /// <summary>
    /// Generates a boss-specific reward.
    /// </summary>
    private static RewardItem GenerateBossReward(DungeonDefinition dungeon, DungeonDifficulty difficulty)
    {
        var rarity = difficulty switch
        {
            DungeonDifficulty.Brutal => RewardRarity.Legendary,
            DungeonDifficulty.Hard => RewardRarity.Epic,
            DungeonDifficulty.Normal => RewardRarity.Rare,
            _ => RewardRarity.Uncommon
        };

        // Could be augment, microchip, or evolution item
        string[] bossDropTypes = { "augment", "microchip", "evolution_catalyst" };
        var dropType = bossDropTypes[_random.Next(bossDropTypes.Length)];

        return new RewardItem
        {
            Type = dropType,
            ItemId = $"{dungeon.Id}_{dropType}_{(int)difficulty}",
            Name = $"{dungeon.FinalBossName}'s {GetDropTypeName(dropType)}",
            Quantity = 1,
            Rarity = rarity
        };
    }

    /// <summary>
    /// Generates a mid-boss reward.
    /// </summary>
    private static RewardItem GenerateMidBossReward(DungeonDefinition dungeon, DungeonDifficulty difficulty)
    {
        var rarity = difficulty switch
        {
            DungeonDifficulty.Brutal => RewardRarity.Epic,
            DungeonDifficulty.Hard => RewardRarity.Rare,
            _ => RewardRarity.Uncommon
        };

        return new RewardItem
        {
            Type = "microchip",
            ItemId = $"{dungeon.Id}_midboss_chip",
            Name = $"{dungeon.MidBossName}'s Circuit",
            Quantity = 1,
            Rarity = rarity
        };
    }

    /// <summary>
    /// Gets a random rarity based on difficulty.
    /// </summary>
    private static RewardRarity GetRandomRarity(DungeonDifficulty difficulty, bool isBoss)
    {
        double roll = _random.NextDouble();
        double bossBonus = isBoss ? 0.2 : 0;
        double difficultyBonus = (int)difficulty * 0.1;

        double adjusted = roll + bossBonus + difficultyBonus;

        return adjusted switch
        {
            > 0.95 => RewardRarity.Legendary,
            > 0.85 => RewardRarity.Epic,
            > 0.65 => RewardRarity.Rare,
            > 0.35 => RewardRarity.Uncommon,
            _ => RewardRarity.Common
        };
    }

    /// <summary>
    /// Gets display name for drop type.
    /// </summary>
    private static string GetDropTypeName(string dropType) => dropType switch
    {
        "augment" => "Augmentation",
        "microchip" => "Neural Chip",
        "evolution_catalyst" => "Evolution Catalyst",
        _ => "Trophy"
    };
}
