using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Strays.Core.Game.Data;
using Strays.Core.Game.Progression;
using Strays.Core.Game.World;

namespace Strays.Core.Game.Items;

/// <summary>
/// Categories of tradeable items.
/// </summary>
public enum ShopCategory
{
    Consumables,
    Microchips,
    Augmentations,
    Materials
}

/// <summary>
/// Defines a consumable item.
/// </summary>
public class ConsumableDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public int BasePrice { get; init; } = 100;
    public int SellPrice => BasePrice / 2;
    public ItemRarity Rarity { get; init; } = ItemRarity.Common;

    /// <summary>
    /// Effect type when used.
    /// </summary>
    public ConsumableEffect Effect { get; init; } = ConsumableEffect.HealHp;

    /// <summary>
    /// Effect power/amount.
    /// </summary>
    public int EffectPower { get; init; } = 50;

    /// <summary>
    /// Whether this can be used in combat.
    /// </summary>
    public bool UsableInCombat { get; init; } = true;

    /// <summary>
    /// Whether this can be used outside combat.
    /// </summary>
    public bool UsableOutOfCombat { get; init; } = true;

    public Color GetRarityColor() => Rarity switch
    {
        ItemRarity.Common => Color.White,
        ItemRarity.Uncommon => Color.LimeGreen,
        ItemRarity.Rare => Color.DodgerBlue,
        ItemRarity.Epic => Color.MediumPurple,
        ItemRarity.Legendary => Color.Gold,
        _ => Color.White
    };
}

/// <summary>
/// Types of consumable effects.
/// </summary>
public enum ConsumableEffect
{
    HealHp,
    HealHpPercent,
    RestoreEnergy,
    CureStatus,
    ReviveStray,
    BoostAttack,
    BoostDefense,
    BoostSpeed,
    ReduceStress
}

/// <summary>
/// An item available for purchase at a shop.
/// </summary>
public class ShopItem
{
    public string ItemId { get; init; } = "";
    public ShopCategory Category { get; init; }
    public int Stock { get; set; } = -1; // -1 = unlimited
    public int BasePrice { get; init; }
    public bool IsLimited => Stock >= 0;

    /// <summary>
    /// Gets the actual price considering faction reputation.
    /// </summary>
    public int GetPrice(FactionReputation? reputation, FactionType shopFaction)
    {
        if (reputation == null || shopFaction == FactionType.None)
            return BasePrice;

        float modifier = reputation.GetPriceModifier(shopFaction);
        return (int)(BasePrice * modifier);
    }

    /// <summary>
    /// Gets the sell price (half of base, not affected by faction).
    /// </summary>
    public int GetSellPrice()
    {
        return Math.Max(1, BasePrice / 2);
    }
}

/// <summary>
/// Definition of a shop/vendor.
/// </summary>
public class ShopDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string OwnerName { get; init; } = "";
    public string Greeting { get; init; } = "Welcome! Take a look at my wares.";
    public FactionType Faction { get; init; } = FactionType.None;
    public List<ShopItem> Inventory { get; init; } = new();

    /// <summary>
    /// Biome where this shop is located.
    /// </summary>
    public BiomeType Biome { get; init; } = BiomeType.Fringe;

    /// <summary>
    /// Whether this shop buys items from the player.
    /// </summary>
    public bool BuysItems { get; init; } = true;

    /// <summary>
    /// Categories of items this shop will buy.
    /// </summary>
    public List<ShopCategory> BuyCategories { get; init; } = new()
    {
        ShopCategory.Consumables,
        ShopCategory.Microchips,
        ShopCategory.Augmentations,
        ShopCategory.Materials
    };
}

/// <summary>
/// Static registry of all shops.
/// </summary>
public static class Shops
{
    private static readonly Dictionary<string, ShopDefinition> _shops = new();

    public static IReadOnlyDictionary<string, ShopDefinition> All => _shops;

    public static ShopDefinition? Get(string id) =>
        _shops.TryGetValue(id, out var shop) ? shop : null;

    public static void Register(ShopDefinition shop)
    {
        _shops[shop.Id] = shop;
    }

    static Shops()
    {
        RegisterFringeShops();
        RegisterRustShops();
        RegisterGreenShops();
        RegisterQuietShops();
        RegisterTeethShops();
        RegisterGlowShops();
        RegisterArchiveShops();
    }

    private static void RegisterFringeShops()
    {
        Register(new ShopDefinition
        {
            Id = "salvager_shop",
            Name = "Salvager's Supplies",
            OwnerName = "Rust",
            Greeting = "Scavenged some good stuff today. Take a look!",
            Faction = FactionType.Independents,
            Biome = BiomeType.Fringe,
            BuysItems = true,
            Inventory = new List<ShopItem>
            {
                // Consumables
                new() { ItemId = "repair_kit_small", Category = ShopCategory.Consumables, BasePrice = 50 },
                new() { ItemId = "repair_kit_medium", Category = ShopCategory.Consumables, BasePrice = 150 },
                new() { ItemId = "energy_cell", Category = ShopCategory.Consumables, BasePrice = 75 },
                new() { ItemId = "antivirus_patch", Category = ShopCategory.Consumables, BasePrice = 100 },
                // Microchips
                new() { ItemId = "drv_attack_1", Category = ShopCategory.Microchips, BasePrice = 300 },
                new() { ItemId = "drv_defense_1", Category = ShopCategory.Microchips, BasePrice = 300 },
                new() { ItemId = "drv_speed_1", Category = ShopCategory.Microchips, BasePrice = 300 },
                // Limited items
                new() { ItemId = "proto_lunge", Category = ShopCategory.Microchips, BasePrice = 500, Stock = 1 },
            }
        });

        // Healer supplies at the Rest House
        Register(new ShopDefinition
        {
            Id = "healer_supplies_shop",
            Name = "Patch's Medical Supplies",
            OwnerName = "Patch",
            Greeting = "Need patching up? I've got what you need to keep your Strays running.",
            Faction = FactionType.Independents,
            Biome = BiomeType.Fringe,
            BuysItems = true,
            BuyCategories = new List<ShopCategory> { ShopCategory.Consumables, ShopCategory.Materials },
            Inventory = new List<ShopItem>
            {
                // Healing focused inventory
                new() { ItemId = "repair_kit_small", Category = ShopCategory.Consumables, BasePrice = 45 },
                new() { ItemId = "repair_kit_medium", Category = ShopCategory.Consumables, BasePrice = 140 },
                new() { ItemId = "antivirus_patch", Category = ShopCategory.Consumables, BasePrice = 90 },
                new() { ItemId = "calm_serum", Category = ShopCategory.Consumables, BasePrice = 140 },
                new() { ItemId = "revival_core", Category = ShopCategory.Consumables, BasePrice = 450, Stock = 2 },
                new() { ItemId = "energy_cell", Category = ShopCategory.Consumables, BasePrice = 70 },
            }
        });
    }

    private static void RegisterRustShops()
    {
        Register(new ShopDefinition
        {
            Id = "machinist_workshop",
            Name = "Machinist Workshop",
            OwnerName = "Volt",
            Greeting = "Need upgrades? You've come to the right place.",
            Faction = FactionType.Harvesters,
            Biome = BiomeType.Rust,
            BuysItems = true,
            Inventory = new List<ShopItem>
            {
                // Consumables
                new() { ItemId = "repair_kit_small", Category = ShopCategory.Consumables, BasePrice = 45 },
                new() { ItemId = "repair_kit_medium", Category = ShopCategory.Consumables, BasePrice = 135 },
                new() { ItemId = "repair_kit_large", Category = ShopCategory.Consumables, BasePrice = 350 },
                new() { ItemId = "overclock_stim", Category = ShopCategory.Consumables, BasePrice = 200 },
                // Microchips
                new() { ItemId = "drv_attack_2", Category = ShopCategory.Microchips, BasePrice = 600 },
                new() { ItemId = "drv_defense_2", Category = ShopCategory.Microchips, BasePrice = 600 },
                new() { ItemId = "sup_overclock", Category = ShopCategory.Microchips, BasePrice = 800 },
                // Augmentations
                new() { ItemId = "aug_steel_claws", Category = ShopCategory.Augmentations, BasePrice = 1000 },
                new() { ItemId = "aug_reinforced_plating", Category = ShopCategory.Augmentations, BasePrice = 1200 },
            }
        });

        // Rust Market - Parts vendor (Gears)
        Register(new ShopDefinition
        {
            Id = "rust_parts_shop",
            Name = "Gears' Parts Emporium",
            OwnerName = "Gears",
            Greeting = "Parts and augments, fresh from the scrap heaps. Best prices in the Belt!",
            Faction = FactionType.Harvesters,
            Biome = BiomeType.Rust,
            BuysItems = true,
            BuyCategories = new List<ShopCategory> { ShopCategory.Augmentations, ShopCategory.Materials },
            Inventory = new List<ShopItem>
            {
                // Augmentations - mechanical focus
                new() { ItemId = "aug_steel_claws", Category = ShopCategory.Augmentations, BasePrice = 950 },
                new() { ItemId = "aug_reinforced_plating", Category = ShopCategory.Augmentations, BasePrice = 1100 },
                new() { ItemId = "aug_hydraulic_legs", Category = ShopCategory.Augmentations, BasePrice = 1300 },
                new() { ItemId = "aug_sensor_array", Category = ShopCategory.Augmentations, BasePrice = 900 },
                // Some consumables
                new() { ItemId = "repair_kit_medium", Category = ShopCategory.Consumables, BasePrice = 130 },
                new() { ItemId = "repair_kit_large", Category = ShopCategory.Consumables, BasePrice = 340 },
            }
        });

        // Rust Market - Chips vendor (Spark)
        Register(new ShopDefinition
        {
            Id = "rust_chips_shop",
            Name = "Spark's Silicon",
            OwnerName = "Spark",
            Greeting = "Chips! Rare chips! Fresh off the assembly line... well, sort of fresh.",
            Faction = FactionType.Harvesters,
            Biome = BiomeType.Rust,
            BuysItems = true,
            BuyCategories = new List<ShopCategory> { ShopCategory.Microchips, ShopCategory.Materials },
            Inventory = new List<ShopItem>
            {
                // Microchips - varied selection
                new() { ItemId = "drv_attack_1", Category = ShopCategory.Microchips, BasePrice = 280 },
                new() { ItemId = "drv_attack_2", Category = ShopCategory.Microchips, BasePrice = 550 },
                new() { ItemId = "drv_defense_1", Category = ShopCategory.Microchips, BasePrice = 280 },
                new() { ItemId = "drv_defense_2", Category = ShopCategory.Microchips, BasePrice = 550 },
                new() { ItemId = "drv_speed_1", Category = ShopCategory.Microchips, BasePrice = 280 },
                new() { ItemId = "sup_overclock", Category = ShopCategory.Microchips, BasePrice = 750 },
                new() { ItemId = "sup_regen_field", Category = ShopCategory.Microchips, BasePrice = 650 },
                // Rare limited stock
                new() { ItemId = "proto_lunge", Category = ShopCategory.Microchips, BasePrice = 480, Stock = 2 },
                new() { ItemId = "proto_guard_stance", Category = ShopCategory.Microchips, BasePrice = 580, Stock = 1 },
            }
        });
    }

    private static void RegisterGreenShops()
    {
        Register(new ShopDefinition
        {
            Id = "shepherd_sanctuary_shop",
            Name = "Sanctuary Supplies",
            OwnerName = "Elder Moss",
            Greeting = "May your Strays find peace. How can I help?",
            Faction = FactionType.Shepherds,
            Biome = BiomeType.Green,
            BuysItems = true,
            Inventory = new List<ShopItem>
            {
                // Consumables - healing focused
                new() { ItemId = "repair_kit_medium", Category = ShopCategory.Consumables, BasePrice = 120 },
                new() { ItemId = "repair_kit_large", Category = ShopCategory.Consumables, BasePrice = 300 },
                new() { ItemId = "revival_core", Category = ShopCategory.Consumables, BasePrice = 500 },
                new() { ItemId = "calm_serum", Category = ShopCategory.Consumables, BasePrice = 150 },
                // Microchips - support focused
                new() { ItemId = "sup_regen_field", Category = ShopCategory.Microchips, BasePrice = 700 },
                new() { ItemId = "proto_guard_stance", Category = ShopCategory.Microchips, BasePrice = 600 },
                new() { ItemId = "sup_rally_pulse", Category = ShopCategory.Microchips, BasePrice = 800 },
                // Augmentations
                new() { ItemId = "aug_bio_filter", Category = ShopCategory.Augmentations, BasePrice = 900 },
            }
        });
    }

    private static void RegisterQuietShops()
    {
        // The Quiet - Uncanny suburb merchants
        Register(new ShopDefinition
        {
            Id = "quiet_general_store",
            Name = "The Corner Store",
            OwnerName = "Mr. Henderson",
            Greeting = "Welcome to our little store. Everything is perfectly normal here.",
            Faction = FactionType.Independents,
            Biome = BiomeType.Quiet,
            BuysItems = true,
            Inventory = new List<ShopItem>
            {
                // Standard supplies at slightly unsettling prices
                new() { ItemId = "repair_kit_small", Category = ShopCategory.Consumables, BasePrice = 55 },
                new() { ItemId = "repair_kit_medium", Category = ShopCategory.Consumables, BasePrice = 160 },
                new() { ItemId = "repair_kit_large", Category = ShopCategory.Consumables, BasePrice = 380 },
                new() { ItemId = "energy_cell", Category = ShopCategory.Consumables, BasePrice = 80 },
                new() { ItemId = "antivirus_patch", Category = ShopCategory.Consumables, BasePrice = 110 },
                // Peculiar chips
                new() { ItemId = "drv_speed_2", Category = ShopCategory.Microchips, BasePrice = 650 },
                new() { ItemId = "proto_evasive_stance", Category = ShopCategory.Microchips, BasePrice = 550 },
                new() { ItemId = "sup_shield_coat", Category = ShopCategory.Microchips, BasePrice = 480 },
            }
        });

        // Memory Collector - specialized rare items
        Register(new ShopDefinition
        {
            Id = "quiet_memory_collector",
            Name = "Memory Collector's Cabinet",
            OwnerName = "The Archivist",
            Greeting = "I collect things that were... forgotten. Perhaps you'll find something familiar.",
            Faction = FactionType.Independents,
            Biome = BiomeType.Quiet,
            BuysItems = true,
            BuyCategories = new List<ShopCategory> { ShopCategory.Microchips, ShopCategory.Materials },
            Inventory = new List<ShopItem>
            {
                // Rare data/memory-related chips
                new() { ItemId = "elem_data_spike", Category = ShopCategory.Microchips, BasePrice = 700 },
                new() { ItemId = "elem_signal_jam", Category = ShopCategory.Microchips, BasePrice = 950 },
                new() { ItemId = "sup_system_wipe", Category = ShopCategory.Microchips, BasePrice = 1100 },
                new() { ItemId = "daemon_holo_decoy", Category = ShopCategory.Microchips, BasePrice = 850 },
                // Limited unique items
                new() { ItemId = "proto_scan", Category = ShopCategory.Microchips, BasePrice = 400, Stock = 2 },
            }
        });
    }

    private static void RegisterTeethShops()
    {
        // The Teeth - Military surplus and combat gear
        Register(new ShopDefinition
        {
            Id = "teeth_armory",
            Name = "The Armory",
            OwnerName = "Sergeant",
            Greeting = "You need firepower? I've got firepower. Just don't ask where it came from.",
            Faction = FactionType.Harvesters,
            Biome = BiomeType.Teeth,
            BuysItems = true,
            Inventory = new List<ShopItem>
            {
                // Heavy consumables
                new() { ItemId = "repair_kit_large", Category = ShopCategory.Consumables, BasePrice = 320 },
                new() { ItemId = "emergency_repair", Category = ShopCategory.Consumables, BasePrice = 750 },
                new() { ItemId = "attack_boost", Category = ShopCategory.Consumables, BasePrice = 130 },
                new() { ItemId = "defense_boost", Category = ShopCategory.Consumables, BasePrice = 130 },
                // Combat-focused chips
                new() { ItemId = "drv_attack_2", Category = ShopCategory.Microchips, BasePrice = 580 },
                new() { ItemId = "drv_crit_chance", Category = ShopCategory.Microchips, BasePrice = 700 },
                new() { ItemId = "proto_taunt", Category = ShopCategory.Microchips, BasePrice = 650 },
                new() { ItemId = "aug_piercing", Category = ShopCategory.Microchips, BasePrice = 900 },
                new() { ItemId = "aug_counter", Category = ShopCategory.Microchips, BasePrice = 1200 },
                // Rare military gear
                new() { ItemId = "daemon_attack_drone", Category = ShopCategory.Microchips, BasePrice = 1400, Stock = 1 },
                new() { ItemId = "daemon_laser_turret", Category = ShopCategory.Microchips, BasePrice = 1600, Stock = 1 },
            }
        });

        // Field Medic - survival supplies
        Register(new ShopDefinition
        {
            Id = "teeth_medic",
            Name = "Field Hospital",
            OwnerName = "Doc",
            Greeting = "Patch you up, send you back out. That's the deal.",
            Faction = FactionType.Independents,
            Biome = BiomeType.Teeth,
            BuysItems = true,
            BuyCategories = new List<ShopCategory> { ShopCategory.Consumables },
            Inventory = new List<ShopItem>
            {
                new() { ItemId = "repair_kit_medium", Category = ShopCategory.Consumables, BasePrice = 125 },
                new() { ItemId = "repair_kit_large", Category = ShopCategory.Consumables, BasePrice = 300 },
                new() { ItemId = "revival_core", Category = ShopCategory.Consumables, BasePrice = 450 },
                new() { ItemId = "antivirus_patch", Category = ShopCategory.Consumables, BasePrice = 95 },
                new() { ItemId = "calm_serum", Category = ShopCategory.Consumables, BasePrice = 160 },
                // Support chips
                new() { ItemId = "sup_repair_pulse", Category = ShopCategory.Microchips, BasePrice = 450 },
                new() { ItemId = "sup_mass_repair", Category = ShopCategory.Microchips, BasePrice = 1100 },
                new() { ItemId = "sup_emergency_reboot", Category = ShopCategory.Microchips, BasePrice = 1300 },
            }
        });
    }

    private static void RegisterGlowShops()
    {
        // The Glow - High-tech data merchants
        Register(new ShopDefinition
        {
            Id = "glow_data_exchange",
            Name = "The Data Exchange",
            OwnerName = "Cipher",
            Greeting = "Information is currency. What are you buying? What are you selling?",
            Faction = FactionType.Harvesters,
            Biome = BiomeType.Glow,
            BuysItems = true,
            Inventory = new List<ShopItem>
            {
                // High-end consumables
                new() { ItemId = "repair_kit_large", Category = ShopCategory.Consumables, BasePrice = 280 },
                new() { ItemId = "emergency_repair", Category = ShopCategory.Consumables, BasePrice = 700 },
                new() { ItemId = "overclock_stim", Category = ShopCategory.Consumables, BasePrice = 180 },
                // Advanced chips
                new() { ItemId = "elem_emp_pulse", Category = ShopCategory.Microchips, BasePrice = 850 },
                new() { ItemId = "elem_thunderbolt", Category = ShopCategory.Microchips, BasePrice = 1200 },
                new() { ItemId = "daemon_shock_turret", Category = ShopCategory.Microchips, BasePrice = 1500 },
                new() { ItemId = "daemon_stun_field", Category = ShopCategory.Microchips, BasePrice = 1800 },
                new() { ItemId = "drv_first_strike", Category = ShopCategory.Microchips, BasePrice = 1400 },
                // Rare data chips
                new() { ItemId = "aug_chain", Category = ShopCategory.Microchips, BasePrice = 1100, Stock = 1 },
            }
        });

        // System Administrator - rare unique items
        Register(new ShopDefinition
        {
            Id = "glow_admin_shop",
            Name = "Admin Access",
            OwnerName = "Root",
            Greeting = "sudo access granted. What do you need?",
            Faction = FactionType.NIMDOK,
            Biome = BiomeType.Glow,
            BuysItems = false, // Doesn't buy, only sells
            Inventory = new List<ShopItem>
            {
                // Legendary/Epic tier items - expensive but powerful
                new() { ItemId = "drv_en_max_2", Category = ShopCategory.Microchips, BasePrice = 800 },
                new() { ItemId = "drv_en_regen_2", Category = ShopCategory.Microchips, BasePrice = 900 },
                new() { ItemId = "sup_rally_pulse", Category = ShopCategory.Microchips, BasePrice = 1000 },
                new() { ItemId = "aug_drain", Category = ShopCategory.Microchips, BasePrice = 1400 },
                new() { ItemId = "aug_battery_leech", Category = ShopCategory.Microchips, BasePrice = 1300 },
                // Very rare, limited stock
                new() { ItemId = "daemon_shield_drone", Category = ShopCategory.Microchips, BasePrice = 1600, Stock = 1 },
                new() { ItemId = "daemon_healing_field", Category = ShopCategory.Microchips, BasePrice = 1700, Stock = 1 },
            }
        });
    }

    private static void RegisterArchiveShops()
    {
        // Archive Scar - remnants and memories
        Register(new ShopDefinition
        {
            Id = "archive_remnant_dealer",
            Name = "The Remnant Dealer",
            OwnerName = "Fragment",
            Greeting = "What's lost can sometimes be found. If you know where to look...",
            Faction = FactionType.Independents,
            Biome = BiomeType.ArchiveScar,
            BuysItems = true,
            Inventory = new List<ShopItem>
            {
                // Survival essentials at premium prices (dangerous area)
                new() { ItemId = "repair_kit_large", Category = ShopCategory.Consumables, BasePrice = 350 },
                new() { ItemId = "emergency_repair", Category = ShopCategory.Consumables, BasePrice = 850 },
                new() { ItemId = "revival_core", Category = ShopCategory.Consumables, BasePrice = 550 },
                // Unique void/data chips
                new() { ItemId = "elem_data_spike", Category = ShopCategory.Microchips, BasePrice = 750 },
                new() { ItemId = "elem_signal_jam", Category = ShopCategory.Microchips, BasePrice = 1000 },
                new() { ItemId = "sup_signal_screen", Category = ShopCategory.Microchips, BasePrice = 800 },
                // Very rare recovery items
                new() { ItemId = "sup_emergency_reboot", Category = ShopCategory.Microchips, BasePrice = 1400, Stock = 2 },
                new() { ItemId = "sup_system_wipe", Category = ShopCategory.Microchips, BasePrice = 1200, Stock = 2 },
            }
        });

        // The Last Backup - end-game shop
        Register(new ShopDefinition
        {
            Id = "archive_last_backup",
            Name = "The Last Backup",
            OwnerName = "Original",
            Greeting = "I am what remains of the first archive. My wares are... authentic.",
            Faction = FactionType.None,
            Biome = BiomeType.ArchiveScar,
            BuysItems = false,
            Inventory = new List<ShopItem>
            {
                // End-game premium items
                new() { ItemId = "emergency_repair", Category = ShopCategory.Consumables, BasePrice = 650 },
                // Best chips available
                new() { ItemId = "elem_inferno", Category = ShopCategory.Microchips, BasePrice = 1500 },
                new() { ItemId = "elem_flash_freeze", Category = ShopCategory.Microchips, BasePrice = 1400 },
                new() { ItemId = "aug_multi_target", Category = ShopCategory.Microchips, BasePrice = 1600 },
                new() { ItemId = "proto_brace", Category = ShopCategory.Microchips, BasePrice = 1000 },
                // Ultra-rare limited items
                new() { ItemId = "drv_first_strike", Category = ShopCategory.Microchips, BasePrice = 1600, Stock = 1 },
            }
        });
    }
}

/// <summary>
/// Static registry of all consumable items.
/// </summary>
public static class Consumables
{
    private static readonly Dictionary<string, ConsumableDefinition> _items = new();

    public static IReadOnlyDictionary<string, ConsumableDefinition> All => _items;

    public static ConsumableDefinition? Get(string id) =>
        _items.TryGetValue(id, out var item) ? item : null;

    public static void Register(ConsumableDefinition item)
    {
        _items[item.Id] = item;
    }

    static Consumables()
    {
        // Healing items
        Register(new ConsumableDefinition
        {
            Id = "repair_kit_small",
            Name = "Small Repair Kit",
            Description = "Restores 50 HP to one Stray.",
            BasePrice = 50,
            Rarity = ItemRarity.Common,
            Effect = ConsumableEffect.HealHp,
            EffectPower = 50
        });

        Register(new ConsumableDefinition
        {
            Id = "repair_kit_medium",
            Name = "Medium Repair Kit",
            Description = "Restores 150 HP to one Stray.",
            BasePrice = 150,
            Rarity = ItemRarity.Uncommon,
            Effect = ConsumableEffect.HealHp,
            EffectPower = 150
        });

        Register(new ConsumableDefinition
        {
            Id = "repair_kit_large",
            Name = "Large Repair Kit",
            Description = "Restores 400 HP to one Stray.",
            BasePrice = 400,
            Rarity = ItemRarity.Rare,
            Effect = ConsumableEffect.HealHp,
            EffectPower = 400
        });

        Register(new ConsumableDefinition
        {
            Id = "emergency_repair",
            Name = "Emergency Repair",
            Description = "Fully restores one Stray's HP.",
            BasePrice = 800,
            Rarity = ItemRarity.Epic,
            Effect = ConsumableEffect.HealHpPercent,
            EffectPower = 100
        });

        // Energy items
        Register(new ConsumableDefinition
        {
            Id = "energy_cell",
            Name = "Energy Cell",
            Description = "Restores 30 EP to one Stray.",
            BasePrice = 75,
            Rarity = ItemRarity.Common,
            Effect = ConsumableEffect.RestoreEnergy,
            EffectPower = 30
        });

        Register(new ConsumableDefinition
        {
            Id = "overclock_stim",
            Name = "Overclock Stim",
            Description = "Fully restores EP and boosts speed for one battle.",
            BasePrice = 200,
            Rarity = ItemRarity.Uncommon,
            Effect = ConsumableEffect.RestoreEnergy,
            EffectPower = 100
        });

        // Status items
        Register(new ConsumableDefinition
        {
            Id = "antivirus_patch",
            Name = "Antivirus Patch",
            Description = "Cures poison and corruption status.",
            BasePrice = 100,
            Rarity = ItemRarity.Common,
            Effect = ConsumableEffect.CureStatus,
            EffectPower = 0
        });

        Register(new ConsumableDefinition
        {
            Id = "calm_serum",
            Name = "Calm Serum",
            Description = "Reduces stress by 25 points.",
            BasePrice = 150,
            Rarity = ItemRarity.Uncommon,
            Effect = ConsumableEffect.ReduceStress,
            EffectPower = 25
        });

        // Revival
        Register(new ConsumableDefinition
        {
            Id = "revival_core",
            Name = "Revival Core",
            Description = "Revives a defeated Stray with 50% HP.",
            BasePrice = 500,
            Rarity = ItemRarity.Rare,
            Effect = ConsumableEffect.ReviveStray,
            EffectPower = 50,
            UsableInCombat = true,
            UsableOutOfCombat = true
        });

        // Combat boosts
        Register(new ConsumableDefinition
        {
            Id = "attack_boost",
            Name = "Attack Boost",
            Description = "Increases attack by 25% for one battle.",
            BasePrice = 120,
            Rarity = ItemRarity.Uncommon,
            Effect = ConsumableEffect.BoostAttack,
            EffectPower = 25,
            UsableInCombat = true,
            UsableOutOfCombat = false
        });

        Register(new ConsumableDefinition
        {
            Id = "defense_boost",
            Name = "Defense Boost",
            Description = "Increases defense by 25% for one battle.",
            BasePrice = 120,
            Rarity = ItemRarity.Uncommon,
            Effect = ConsumableEffect.BoostDefense,
            EffectPower = 25,
            UsableInCombat = true,
            UsableOutOfCombat = false
        });

        Register(new ConsumableDefinition
        {
            Id = "speed_boost",
            Name = "Speed Boost",
            Description = "Increases speed by 25% for one battle.",
            BasePrice = 120,
            Rarity = ItemRarity.Uncommon,
            Effect = ConsumableEffect.BoostSpeed,
            EffectPower = 25,
            UsableInCombat = true,
            UsableOutOfCombat = false
        });
    }
}

/// <summary>
/// Helper to get item information across all item types.
/// </summary>
public static class ItemDatabase
{
    /// <summary>
    /// Gets the name of any item by ID and category.
    /// </summary>
    public static string GetItemName(string itemId, ShopCategory category)
    {
        return category switch
        {
            ShopCategory.Consumables => Consumables.Get(itemId)?.Name ?? itemId,
            ShopCategory.Microchips => Microchips.Get(itemId)?.Name ?? itemId,
            ShopCategory.Augmentations => Augmentations.Get(itemId)?.Name ?? itemId,
            _ => itemId
        };
    }

    /// <summary>
    /// Gets the description of any item by ID and category.
    /// </summary>
    public static string GetItemDescription(string itemId, ShopCategory category)
    {
        return category switch
        {
            ShopCategory.Consumables => Consumables.Get(itemId)?.Description ?? "",
            ShopCategory.Microchips => Microchips.Get(itemId)?.Description ?? "",
            ShopCategory.Augmentations => Augmentations.Get(itemId)?.Description ?? "",
            _ => ""
        };
    }

    /// <summary>
    /// Gets the base price of any item.
    /// </summary>
    public static int GetBasePrice(string itemId, ShopCategory category)
    {
        return category switch
        {
            ShopCategory.Consumables => Consumables.Get(itemId)?.BasePrice ?? 0,
            ShopCategory.Microchips => GetMicrochipPrice(itemId),
            ShopCategory.Augmentations => GetAugmentationPrice(itemId),
            _ => 0
        };
    }

    /// <summary>
    /// Gets the rarity color for any item.
    /// </summary>
    public static Color GetRarityColor(string itemId, ShopCategory category)
    {
        return category switch
        {
            ShopCategory.Consumables => Consumables.Get(itemId)?.GetRarityColor() ?? Color.White,
            ShopCategory.Microchips => Microchips.Get(itemId)?.GetRarityColor() ?? Color.White,
            ShopCategory.Augmentations => Augmentations.Get(itemId)?.GetRarityColor() ?? Color.White,
            _ => Color.White
        };
    }

    private static int GetMicrochipPrice(string itemId)
    {
        var chip = Microchips.Get(itemId);
        if (chip == null) return 0;

        // Price based on rarity
        return chip.Rarity switch
        {
            ItemRarity.Common => 200,
            ItemRarity.Uncommon => 400,
            ItemRarity.Rare => 800,
            ItemRarity.Epic => 1500,
            ItemRarity.Legendary => 3000,
            _ => 200
        };
    }

    private static int GetAugmentationPrice(string itemId)
    {
        var aug = Augmentations.Get(itemId);
        if (aug == null) return 0;

        // Price based on rarity
        return aug.Rarity switch
        {
            ItemRarity.Common => 500,
            ItemRarity.Uncommon => 1000,
            ItemRarity.Rare => 2000,
            ItemRarity.Epic => 4000,
            ItemRarity.Legendary => 8000,
            _ => 500
        };
    }
}
