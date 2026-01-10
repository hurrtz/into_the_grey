using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lazarus.Core.Game.Data;
using Lazarus.Core.Game.Items;
using Lazarus.Core.Game.Stats;

namespace Lazarus.Core.Game.Entities;

/// <summary>
/// Combat row positioning for a Kyn.
/// </summary>
public enum CombatRow
{
    /// <summary>
    /// Front row: +20% physical damage dealt and taken.
    /// </summary>
    Front,

    /// <summary>
    /// Back row: -20% physical damage dealt and taken.
    /// </summary>
    Back
}

/// <summary>
/// Represents an individual Kyn instance - a recruited or wild cybernetic creature.
/// Kyns are half-biological, half-cybernetic animals created by Lazarus.
/// </summary>
public class Kyn
{
    private static int _nextInstanceId = 1;

    /// <summary>
    /// The complete stat profile for this Kyn.
    /// </summary>
    private readonly KynStats _stats = new();

    /// <summary>
    /// Unique instance ID for this specific Kyn.
    /// </summary>
    public string InstanceId { get; }

    /// <summary>
    /// Gets the complete stat profile for this Kyn.
    /// Use this to access all 61 stats with base/bonus/total values.
    /// </summary>
    public KynStats Stats => _stats;

    /// <summary>
    /// The definition (species/type) of this Kyn.
    /// </summary>
    public KynDefinition Definition { get; private set; }

    /// <summary>
    /// Custom nickname (null = use default name).
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Display name (nickname if set, otherwise definition name).
    /// </summary>
    public string DisplayName => Nickname ?? Definition.Name;

    /// <summary>
    /// Current level.
    /// </summary>
    public int Level { get; private set; } = 1;

    /// <summary>
    /// Experience points toward next level.
    /// </summary>
    public int Experience { get; private set; } = 0;

    /// <summary>
    /// Experience required for next level.
    /// </summary>
    public int ExperienceToNextLevel => Level * 100;

    /// <summary>
    /// Current HP.
    /// </summary>
    public int CurrentHp { get; set; }

    /// <summary>
    /// Maximum HP (from stat system).
    /// </summary>
    public int MaxHp => (int)_stats.GetTotal(StatType.HPMax);

    /// <summary>
    /// Attack stat (average of physical ATK stats for backward compatibility).
    /// </summary>
    public int Attack => (int)((_stats.GetTotal(StatType.ATK_Impact) +
                                _stats.GetTotal(StatType.ATK_Piercing) +
                                _stats.GetTotal(StatType.ATK_Slashing)) / 3f);

    /// <summary>
    /// Defense stat (average of physical MIT stats for backward compatibility).
    /// </summary>
    public int Defense => (int)((_stats.GetTotal(StatType.MIT_Impact) +
                                 _stats.GetTotal(StatType.MIT_Piercing) +
                                 _stats.GetTotal(StatType.MIT_Slashing)) / 3f);

    /// <summary>
    /// Speed stat (affects ATB fill rate).
    /// </summary>
    public int Speed => (int)_stats.GetTotal(StatType.Speed);

    /// <summary>
    /// Special stat (average of elemental ATK stats for backward compatibility).
    /// </summary>
    public int Special => (int)((_stats.GetTotal(StatType.ATK_Thermal) +
                                 _stats.GetTotal(StatType.ATK_Cryo) +
                                 _stats.GetTotal(StatType.ATK_Electric) +
                                 _stats.GetTotal(StatType.ATK_Corrosive) +
                                 _stats.GetTotal(StatType.ATK_Toxic) +
                                 _stats.GetTotal(StatType.ATK_Sonic) +
                                 _stats.GetTotal(StatType.ATK_Radiant)) / 7f);

    /// <summary>
    /// Maximum energy pool (for microchip abilities).
    /// </summary>
    public int MaxEnergy => (int)_stats.GetTotal(StatType.ENMax);

    /// <summary>
    /// Energy regeneration per ATB tick.
    /// </summary>
    public int EnergyRegen => (int)_stats.GetTotal(StatType.ENRegen);

    /// <summary>
    /// Current energy (0 to MaxEnergy).
    /// </summary>
    public int CurrentEnergy { get; set; }

    /// <summary>
    /// Energy as a percentage (0-1).
    /// </summary>
    public float EnergyPercent => MaxEnergy > 0 ? (float)CurrentEnergy / MaxEnergy : 0f;

    /// <summary>
    /// Whether this Kyn is alive.
    /// </summary>
    public bool IsAlive => CurrentHp > 0;

    /// <summary>
    /// Current evolution stage (0 = base form).
    /// </summary>
    public int EvolutionStage { get; private set; } = 0;

    /// <summary>
    /// Whether this Kyn has evolved at least once.
    /// </summary>
    public bool IsEvolved { get; private set; } = false;

    /// <summary>
    /// Evolution state tracking (stress, evolution history).
    /// </summary>
    public EvolutionState EvolutionState { get; } = new();

    /// <summary>
    /// Bond level with the protagonist (0-100).
    /// Affects loyalty, combat bonuses, and recruitment of similar Kyns.
    /// </summary>
    public int BondLevel { get; set; } = 0;

    /// <summary>
    /// Combat row (front or back).
    /// Front row: +20% physical damage dealt/taken.
    /// Back row: -20% physical damage dealt/taken.
    /// </summary>
    public CombatRow CombatRow { get; set; } = CombatRow.Front;

    /// <summary>
    /// Equipped augmentations by slot key.
    /// Key: SlotReference.ToKey(), Value: Augmentation definition ID (null = empty slot).
    /// </summary>
    public Dictionary<string, string?> EquippedAugmentations { get; } = new();

    /// <summary>
    /// Equipped microchips (legacy - use MicrochipSockets for new code).
    /// </summary>
    public List<string> EquippedMicrochips { get; } = new();

    /// <summary>
    /// Microchip sockets for this Kyn.
    /// </summary>
    public MicrochipSocket[] MicrochipSockets { get; private set; } = Array.Empty<MicrochipSocket>();

    /// <summary>
    /// Gets the socket configuration for the current evolution stage.
    /// </summary>
    public SocketConfiguration CurrentSocketConfiguration =>
        Definition.GetSocketConfiguration(EvolutionStage);

    /// <summary>
    /// Gets all equipped microchip instances.
    /// </summary>
    public IEnumerable<Microchip> GetEquippedChips()
    {
        foreach (var socket in MicrochipSockets)
        {
            if (socket.EquippedChip != null)
                yield return socket.EquippedChip;
        }
    }

    /// <summary>
    /// Gets the chip in a specific socket.
    /// </summary>
    public Microchip? GetChipInSocket(int socketIndex)
    {
        if (socketIndex < 0 || socketIndex >= MicrochipSockets.Length)
            return null;
        return MicrochipSockets[socketIndex].EquippedChip;
    }

    /// <summary>
    /// Gets the chip linked to a socket (if any).
    /// </summary>
    public Microchip? GetLinkedChip(int socketIndex)
    {
        if (socketIndex < 0 || socketIndex >= MicrochipSockets.Length)
            return null;

        var socket = MicrochipSockets[socketIndex];
        if (!socket.IsLinked)
            return null;

        return GetChipInSocket(socket.LinkedSocketIndex);
    }

    /// <summary>
    /// Equips a microchip to a socket.
    /// </summary>
    /// <param name="chip">The chip to equip.</param>
    /// <param name="socketIndex">The socket index.</param>
    /// <returns>The previously equipped chip, or null.</returns>
    public Microchip? EquipMicrochip(Microchip chip, int socketIndex)
    {
        if (socketIndex < 0 || socketIndex >= MicrochipSockets.Length)
            return null;

        // Check level requirement
        if (Level < chip.Definition.MinLevel)
            return null;

        var socket = MicrochipSockets[socketIndex];
        var previousChip = socket.EquippedChip;

        // Unequip from previous location if already equipped
        if (chip.IsEquipped && chip.EquippedToKynId == InstanceId)
        {
            UnequipMicrochip(chip.SocketIndex);
        }

        // Update previous chip state
        if (previousChip != null)
        {
            previousChip.IsEquipped = false;
            previousChip.EquippedToKynId = null;
            previousChip.SocketIndex = -1;
        }

        // Equip new chip
        socket.EquippedChip = chip;
        chip.IsEquipped = true;
        chip.EquippedToKynId = InstanceId;
        chip.SocketIndex = socketIndex;

        // Recalculate stat modifiers from equipment
        RecalculateStatModifiers();

        return previousChip;
    }

    /// <summary>
    /// Unequips a microchip from a socket.
    /// </summary>
    /// <param name="socketIndex">The socket index.</param>
    /// <returns>The removed chip, or null.</returns>
    public Microchip? UnequipMicrochip(int socketIndex)
    {
        if (socketIndex < 0 || socketIndex >= MicrochipSockets.Length)
            return null;

        var socket = MicrochipSockets[socketIndex];
        var chip = socket.EquippedChip;

        if (chip != null)
        {
            chip.IsEquipped = false;
            chip.EquippedToKynId = null;
            chip.SocketIndex = -1;
            socket.EquippedChip = null;

            // Recalculate stat modifiers from equipment
            RecalculateStatModifiers();
        }

        return chip;
    }

    /// <summary>
    /// Initializes microchip sockets for the current evolution stage.
    /// </summary>
    private void InitializeMicrochipSockets()
    {
        var config = CurrentSocketConfiguration;
        MicrochipSockets = config.CreateSockets();
    }

    /// <summary>
    /// Upgrades sockets after evolution, preserving existing chips.
    /// </summary>
    private void UpgradeMicrochipSockets()
    {
        var oldSockets = MicrochipSockets;
        var config = CurrentSocketConfiguration;
        var newSockets = config.CreateSockets();

        // Preserve chips from old sockets
        for (int i = 0; i < Math.Min(oldSockets.Length, newSockets.Length); i++)
        {
            newSockets[i].EquippedChip = oldSockets[i].EquippedChip;
        }

        MicrochipSockets = newSockets;
    }

    /// <summary>
    /// Gets all abilities from equipped microchips.
    /// </summary>
    public IEnumerable<string> GetMicrochipAbilities()
    {
        foreach (var chip in GetEquippedChips())
        {
            if (!string.IsNullOrEmpty(chip.Definition.GrantsAbility))
            {
                yield return chip.Definition.GrantsAbility;
            }
        }
    }

    /// <summary>
    /// Regenerates energy (called per ATB tick in combat).
    /// </summary>
    /// <returns>Amount of energy regenerated.</returns>
    public int RegenerateEnergy()
    {
        int regenAmount = EnergyRegen;
        int previousEnergy = CurrentEnergy;
        CurrentEnergy = Math.Min(MaxEnergy, CurrentEnergy + regenAmount);
        return CurrentEnergy - previousEnergy;
    }

    /// <summary>
    /// Consumes energy for using a chip ability.
    /// </summary>
    /// <param name="amount">Energy to consume.</param>
    /// <returns>True if successful, false if not enough energy.</returns>
    public bool ConsumeEnergy(int amount)
    {
        if (CurrentEnergy < amount)
            return false;

        CurrentEnergy -= amount;
        return true;
    }

    /// <summary>
    /// Restores energy to full (called at combat start or out of combat).
    /// </summary>
    public void FullEnergy()
    {
        CurrentEnergy = MaxEnergy;
    }

    /// <summary>
    /// Dissipates heat on all equipped chips (called per ATB tick).
    /// </summary>
    public void DissipateChipHeat()
    {
        foreach (var chip in GetEquippedChips())
        {
            chip.DissipateHeat();
        }
    }

    /// <summary>
    /// Resets heat on all chips (called at combat start).
    /// </summary>
    public void ResetChipHeat()
    {
        foreach (var chip in GetEquippedChips())
        {
            chip.ResetHeat();
        }
    }

    /// <summary>
    /// Awards TU to all equipped chips (called after battle victory).
    /// </summary>
    /// <param name="amount">Base TU to award.</param>
    /// <returns>List of chips that leveled up.</returns>
    public List<Microchip> AwardBattleTu(int amount)
    {
        var leveledUp = new List<Microchip>();

        foreach (var chip in GetEquippedChips())
        {
            if (chip.AddTu(amount))
            {
                leveledUp.Add(chip);
            }
        }

        return leveledUp;
    }

    /// <summary>
    /// Initializes the augmentation slots for this Kyn based on its category.
    /// </summary>
    private void InitializeAugmentationSlots()
    {
        // Add all 9 universal slots
        foreach (var slot in AugmentationSlotUtility.GetUniversalSlots())
        {
            var slotRef = new SlotReference(slot);
            EquippedAugmentations[slotRef.ToKey()] = null;
        }

        // Add category-specific slots (4-5 depending on category)
        foreach (var slot in AugmentationSlotUtility.GetCategorySlotsFor(Definition.Category))
        {
            var slotRef = new SlotReference(slot);
            EquippedAugmentations[slotRef.ToKey()] = null;
        }
    }

    /// <summary>
    /// Gets all available slots for this Kyn (universal + category-specific).
    /// </summary>
    public IEnumerable<SlotReference> GetAvailableSlots()
    {
        foreach (var slot in AugmentationSlotUtility.GetUniversalSlots())
            yield return new SlotReference(slot);

        foreach (var slot in AugmentationSlotUtility.GetCategorySlotsFor(Definition.Category))
            yield return new SlotReference(slot);
    }

    /// <summary>
    /// Gets the augmentation equipped in a slot.
    /// </summary>
    public string? GetEquippedAugmentationId(SlotReference slot)
    {
        var key = slot.ToKey();
        return EquippedAugmentations.TryGetValue(key, out var augId) ? augId : null;
    }

    /// <summary>
    /// Gets whether a slot has an augmentation equipped.
    /// </summary>
    public bool HasAugmentationInSlot(SlotReference slot)
    {
        return GetEquippedAugmentationId(slot) != null;
    }

    /// <summary>
    /// Equips an augmentation to a slot.
    /// </summary>
    /// <param name="augmentationId">The augmentation to equip.</param>
    /// <param name="slot">The slot to equip to.</param>
    /// <returns>The ID of the previously equipped augmentation, or null.</returns>
    public string? EquipAugmentation(string augmentationId, SlotReference slot)
    {
        // Verify slot is valid for this creature's category
        if (!slot.IsValidForCategory(Definition.Category))
            return null;

        var key = slot.ToKey();
        if (!EquippedAugmentations.ContainsKey(key))
            return null;

        var previousAugment = EquippedAugmentations[key];
        EquippedAugmentations[key] = augmentationId;

        // Recalculate stat modifiers from equipment
        RecalculateStatModifiers();

        // Adjust HP if MaxHp changed
        var newMaxHp = MaxHp;
        if (CurrentHp > newMaxHp)
            CurrentHp = newMaxHp;

        return previousAugment;
    }

    /// <summary>
    /// Unequips an augmentation from a slot.
    /// </summary>
    /// <param name="slot">The slot to clear.</param>
    /// <returns>The ID of the removed augmentation, or null if slot was empty.</returns>
    public string? UnequipAugmentation(SlotReference slot)
    {
        var key = slot.ToKey();
        if (!EquippedAugmentations.ContainsKey(key))
            return null;

        var previousAugment = EquippedAugmentations[key];
        EquippedAugmentations[key] = null;

        // Recalculate stat modifiers from equipment
        RecalculateStatModifiers();

        // Adjust HP if MaxHp changed
        var newMaxHp = MaxHp;
        if (CurrentHp > newMaxHp)
            CurrentHp = newMaxHp;

        return previousAugment;
    }

    /// <summary>
    /// Gets the augmentation definition equipped in a slot.
    /// </summary>
    /// <param name="slot">The slot to check.</param>
    /// <returns>The augmentation definition, or null if slot is empty.</returns>
    public AugmentationDefinition? GetEquippedAugmentation(SlotReference slot)
    {
        var augId = GetEquippedAugmentationId(slot);
        return augId != null ? Augmentations.Get(augId) : null;
    }

    /// <summary>
    /// Gets all abilities granted by equipped augmentations.
    /// </summary>
    public IEnumerable<string> GetAugmentationAbilities()
    {
        foreach (var slotKey in EquippedAugmentations.Keys)
        {
            var augId = EquippedAugmentations[slotKey];
            if (augId != null)
            {
                var augDef = Augmentations.Get(augId);
                if (augDef?.GrantsAbility != null)
                {
                    yield return augDef.GrantsAbility;
                }
            }
        }
    }

    /// <summary>
    /// Gets all abilities this Kyn has (innate + augmentation-granted + microchip-granted).
    /// </summary>
    public IEnumerable<string> GetAllAbilities()
    {
        return Definition.InnateAbilities
            .Concat(GetAugmentationAbilities())
            .Concat(GetMicrochipAbilities());
    }

    /// <summary>
    /// Whether this Kyn is hostile (enemy in combat).
    /// </summary>
    public bool IsHostile { get; set; } = false;

    /// <summary>
    /// Position in combat (for visual placement).
    /// </summary>
    public Vector2 CombatPosition { get; set; }

    /// <summary>
    /// Creates a new Kyn from a definition.
    /// </summary>
    /// <param name="definition">The Kyn definition to use.</param>
    /// <param name="level">Starting level.</param>
    public Kyn(KynDefinition definition, int level = 1)
    {
        InstanceId = $"kyn_{_nextInstanceId++}";
        Definition = definition;
        Level = Math.Max(1, level);
        InitializeAugmentationSlots();
        InitializeMicrochipSockets();
        InitializeBaseStats();
        CurrentHp = MaxHp;
        CurrentEnergy = MaxEnergy;
    }

    /// <summary>
    /// Creates a Kyn from a definition ID.
    /// </summary>
    /// <param name="definitionId">The definition ID.</param>
    /// <param name="level">Starting level.</param>
    public static Kyn? Create(string definitionId, int level = 1)
    {
        var definition = KynDefinitions.Get(definitionId);

        if (definition == null)
        {
            return null;
        }

        return new Kyn(definition, level);
    }

    /// <summary>
    /// Sets the level directly (for NG+ or special cases).
    /// </summary>
    /// <param name="newLevel">The new level to set.</param>
    public void SetLevel(int newLevel)
    {
        Level = Math.Max(1, newLevel);
        Experience = 0; // Reset experience after level change
        InitializeBaseStats(); // Recalculate stats for new level
        CurrentHp = MaxHp; // Heal to full
        CurrentEnergy = MaxEnergy;
    }

    /// <summary>
    /// Initializes the base stats from the definition.
    /// Call this after setting the definition or on level up.
    /// </summary>
    private void InitializeBaseStats()
    {
        // Scale base stats by level (10% per level after level 1)
        float levelMultiplier = 1f + (Level - 1) * 0.1f;
        var baseStats = Definition.BaseStats;

        // Map old simple stats to new comprehensive system
        _stats.SetBase(StatType.HPMax, baseStats.MaxHp * levelMultiplier);
        _stats.SetBase(StatType.Speed, baseStats.Speed * levelMultiplier);
        _stats.SetBase(StatType.ENMax, baseStats.MaxEnergy * levelMultiplier);
        _stats.SetBase(StatType.ENRegen, baseStats.EnergyRegen * levelMultiplier);

        // Distribute Attack stat across physical damage types
        float attackPerType = baseStats.Attack * levelMultiplier / 3f;
        _stats.SetBase(StatType.ATK_Impact, attackPerType);
        _stats.SetBase(StatType.ATK_Piercing, attackPerType);
        _stats.SetBase(StatType.ATK_Slashing, attackPerType);

        // Distribute Defense stat across physical mitigation types
        float defensePerType = baseStats.Defense * levelMultiplier / 3f;
        _stats.SetBase(StatType.MIT_Impact, defensePerType);
        _stats.SetBase(StatType.MIT_Piercing, defensePerType);
        _stats.SetBase(StatType.MIT_Slashing, defensePerType);

        // Distribute Special stat across elemental damage types
        float specialPerType = baseStats.Special * levelMultiplier / 7f;
        _stats.SetBase(StatType.ATK_Thermal, specialPerType);
        _stats.SetBase(StatType.ATK_Cryo, specialPerType);
        _stats.SetBase(StatType.ATK_Electric, specialPerType);
        _stats.SetBase(StatType.ATK_Corrosive, specialPerType);
        _stats.SetBase(StatType.ATK_Toxic, specialPerType);
        _stats.SetBase(StatType.ATK_Sonic, specialPerType);
        _stats.SetBase(StatType.ATK_Radiant, specialPerType);

        // Recalculate modifiers from equipment
        RecalculateStatModifiers();
    }

    /// <summary>
    /// Recalculates all stat modifiers from augmentations and microchips.
    /// Call this after equipping/unequipping any gear.
    /// </summary>
    public void RecalculateStatModifiers()
    {
        // Clear existing modifiers from equipment
        _stats.RemoveModifiersFromSource("augmentation");
        _stats.RemoveModifiersFromSource("microchip");

        // Add augmentation modifiers
        foreach (var kvp in EquippedAugmentations)
        {
            var augId = kvp.Value;
            if (augId == null)
                continue;

            var augDef = Augmentations.Get(augId);
            if (augDef == null)
                continue;

            // Add stat modifiers from augmentation
            foreach (var statMod in augDef.StatModifiers)
            {
                _stats.AddModifier(new StatModifier
                {
                    Stat = statMod.Stat,
                    Value = statMod.Value,
                    IsPercent = statMod.IsPercent,
                    Source = "augmentation",
                    SourceType = ModifierSource.Augmentation,
                    SourceName = augDef.Name
                });
            }

            // Legacy support: map old stat bonuses to new system
            foreach (var bonus in augDef.StatBonuses)
            {
                var statType = MapLegacyStatName(bonus.Key);
                if (statType.HasValue)
                {
                    _stats.AddModifier(StatModifier.Flat(statType.Value, bonus.Value, "augmentation", ModifierSource.Augmentation, augDef.Name));
                }
            }

            // Legacy support: map old stat multipliers to new system
            foreach (var mult in augDef.StatMultipliers)
            {
                var statType = MapLegacyStatName(mult.Key);
                if (statType.HasValue)
                {
                    // Convert multiplier to percentage bonus (e.g., 1.2 -> +20%)
                    float percentBonus = (mult.Value - 1f) * 100f;
                    _stats.AddModifier(StatModifier.Percent(statType.Value, percentBonus, "augmentation", ModifierSource.Augmentation, augDef.Name));
                }
            }
        }

        // Add microchip modifiers (from Driver chips)
        foreach (var chip in GetEquippedChips())
        {
            // Add stat modifiers from microchip definition
            foreach (var statMod in chip.Definition.StatModifiers)
            {
                // Scale by firmware level for Driver chips
                float value = statMod.Value;
                if (chip.Definition.Category == MicrochipCategory.Driver)
                {
                    value *= 1f + ((int)chip.FirmwareLevel - 1) * 0.1f;
                }

                _stats.AddModifier(new StatModifier
                {
                    Stat = statMod.Stat,
                    Value = value,
                    IsPercent = statMod.IsPercent,
                    Source = "microchip",
                    SourceType = ModifierSource.Microchip,
                    SourceName = chip.Definition.Name
                });
            }
        }
    }

    /// <summary>
    /// Maps legacy stat name strings to the new StatType enum.
    /// </summary>
    private static StatType? MapLegacyStatName(string legacyName)
    {
        return legacyName switch
        {
            "MaxHp" or "HP" => StatType.HPMax,
            "MaxEnergy" or "Energy" => StatType.ENMax,
            "EnergyRegen" => StatType.ENRegen,
            "Speed" => StatType.Speed,
            "Attack" => StatType.ATK_Impact, // Default to Impact for legacy attack
            "Defense" => StatType.MIT_Impact, // Default to Impact for legacy defense
            "Special" => StatType.ATK_Thermal, // Default to Thermal for legacy special
            "MeleeAccuracy" => StatType.MeleeAccuracy,
            "RangedAccuracy" => StatType.RangedAccuracy,
            "Evasion" => StatType.Evasion,
            "CritChance" => StatType.MeleeCritChance,
            "CritDamage" => StatType.CritSeverity,
            _ => null
        };
    }

    /// <summary>
    /// Calculates a stat value based on level and augmentations.
    /// Deprecated: Use Stats.GetTotal(StatType) instead.
    /// </summary>
    [Obsolete("Use Stats.GetTotal(StatType) instead")]
    private int CalculateStat(int baseStat, string statName = "")
    {
        // Scale by level (10% per level)
        float levelMultiplier = 1f + (Level - 1) * 0.1f;
        float scaledStat = baseStat * levelMultiplier;

        // Add augmentation bonuses
        int flatBonus = 0;
        float multiplier = 1f;

        foreach (var kvp in EquippedAugmentations)
        {
            var augId = kvp.Value;
            if (augId == null)
                continue;

            var augDef = Augmentations.Get(augId);
            if (augDef == null)
                continue;

            // Flat bonuses
            if (!string.IsNullOrEmpty(statName) && augDef.StatBonuses.TryGetValue(statName, out var bonus))
            {
                flatBonus += bonus;
            }

            // Multipliers
            if (!string.IsNullOrEmpty(statName) && augDef.StatMultipliers.TryGetValue(statName, out var mult))
            {
                multiplier *= mult;
            }
        }

        // Add microchip bonuses (from Driver chips)
        foreach (var chip in GetEquippedChips())
        {
            if (chip.Definition.Category != MicrochipCategory.Driver)
                continue;

            // Flat bonuses (scaled by firmware level)
            if (!string.IsNullOrEmpty(statName))
            {
                flatBonus += chip.GetStatBonus(statName);
            }

            // Multipliers
            var chipMult = chip.GetStatMultiplier(statName);
            if (chipMult != 1f)
            {
                multiplier *= chipMult;
            }
        }

        return (int)((scaledStat + flatBonus) * multiplier);
    }

    /// <summary>
    /// Gets the total stat bonuses from all equipped augmentations for a stat.
    /// </summary>
    private (int flatBonus, float multiplier) GetAugmentationStatBonus(string statName)
    {
        int flatBonus = 0;
        float multiplier = 1f;

        foreach (var kvp in EquippedAugmentations)
        {
            var augId = kvp.Value;
            if (augId == null)
                continue;

            var augDef = Augmentations.Get(augId);
            if (augDef == null)
                continue;

            if (augDef.StatBonuses.TryGetValue(statName, out var bonus))
                flatBonus += bonus;

            if (augDef.StatMultipliers.TryGetValue(statName, out var mult))
                multiplier *= mult;
        }

        return (flatBonus, multiplier);
    }

    /// <summary>
    /// Adds experience and checks for level up.
    /// </summary>
    /// <param name="amount">Experience to add.</param>
    /// <returns>True if leveled up.</returns>
    public bool AddExperience(int amount)
    {
        Experience += amount;
        bool leveledUp = false;

        while (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            Level++;
            leveledUp = true;
        }

        if (leveledUp)
        {
            // Recalculate stats for new level
            InitializeBaseStats();
            // Heal to full on level up
            CurrentHp = MaxHp;
            CurrentEnergy = MaxEnergy;
        }

        return leveledUp;
    }

    /// <summary>
    /// Takes damage, reducing HP.
    /// </summary>
    /// <param name="damage">Raw damage amount.</param>
    /// <returns>Actual damage taken after defense.</returns>
    public int TakeDamage(int damage)
    {
        // Simple damage formula: damage - defense/2, minimum 1
        int actualDamage = Math.Max(1, damage - Defense / 2);
        CurrentHp = Math.Max(0, CurrentHp - actualDamage);
        return actualDamage;
    }

    /// <summary>
    /// Takes percentage-based damage (for Gravitation).
    /// </summary>
    /// <param name="percent">Percentage of max HP to remove (0-1).</param>
    /// <returns>Damage taken.</returns>
    public int TakePercentDamage(float percent)
    {
        int damage = (int)(MaxHp * percent);
        CurrentHp = Math.Max(1, CurrentHp - damage); // Gravitation leaves at least 1 HP
        return damage;
    }

    /// <summary>
    /// Heals the Kyn.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    /// <returns>Actual amount healed.</returns>
    public int Heal(int amount)
    {
        int previousHp = CurrentHp;
        CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
        return CurrentHp - previousHp;
    }

    /// <summary>
    /// Fully heals the Kyn.
    /// </summary>
    public void FullHeal()
    {
        CurrentHp = MaxHp;
    }

    /// <summary>
    /// Revives the Kyn with a percentage of max HP.
    /// </summary>
    /// <param name="hpPercent">Percentage of max HP to revive with (0-1).</param>
    public void Revive(float hpPercent = 0.5f)
    {
        if (!IsAlive)
        {
            CurrentHp = Math.Max(1, (int)(MaxHp * hpPercent));
        }
    }

    /// <summary>
    /// Checks if this Kyn can evolve.
    /// </summary>
    public bool CanEvolve()
    {
        if (IsEvolved)
            return false;

        if (Definition.EvolutionLevel <= 0)
            return false;

        if (Level < Definition.EvolutionLevel)
            return false;

        // TODO: Check evolution trigger conditions

        return true;
    }

    /// <summary>
    /// Evolves the Kyn to its next form.
    /// </summary>
    /// <returns>True if evolution succeeded.</returns>
    public bool TryEvolve()
    {
        if (!CanEvolve())
            return false;

        if (string.IsNullOrEmpty(Definition.EvolvedFormId))
            return false;

        // Check if max evolutions reached
        if (EvolutionStage >= Definition.MaxEvolutions)
            return false;

        // Increment evolution stage
        EvolutionStage++;
        IsEvolved = true;

        // Upgrade microchip sockets for new evolution stage
        UpgradeMicrochipSockets();

        // Heal to full on evolution
        CurrentHp = MaxHp;
        CurrentEnergy = MaxEnergy;

        return true;
    }

    /// <summary>
    /// Evolves the Kyn to a new definition (used by EvolutionSystem).
    /// </summary>
    /// <param name="newDefinition">The new definition for the evolved form.</param>
    public void Evolve(KynDefinition newDefinition)
    {
        Definition = newDefinition;
        EvolutionStage++;
        IsEvolved = true;

        // Upgrade microchip sockets for new evolution stage
        UpgradeMicrochipSockets();

        // Re-initialize augmentation slots for the new category/type if needed
        // Keep existing augments that are still compatible
        foreach (var slot in GetAvailableSlots())
        {
            var key = slot.ToKey();
            if (!EquippedAugmentations.ContainsKey(key))
            {
                EquippedAugmentations[key] = null;
            }
        }
    }

    /// <summary>
    /// Increases bond level.
    /// </summary>
    /// <param name="amount">Amount to increase.</param>
    public void IncreaseBond(int amount)
    {
        BondLevel = Math.Min(100, BondLevel + amount);
    }

    /// <summary>
    /// Decreases bond level.
    /// </summary>
    /// <param name="amount">Amount to decrease.</param>
    public void DecreaseBond(int amount)
    {
        BondLevel = Math.Max(0, BondLevel - amount);
    }

    /// <summary>
    /// Draws the Kyn (placeholder visual).
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw with.</param>
    /// <param name="pixelTexture">1x1 white pixel texture.</param>
    /// <param name="position">Screen position to draw at.</param>
    /// <param name="showHealth">Whether to show health bar.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 position, bool showHealth = true)
    {
        var size = Definition.PlaceholderSize;
        var color = IsHostile ? Color.Red : Definition.PlaceholderColor;

        // Dim color if dead
        if (!IsAlive)
        {
            color = color * 0.3f;
        }

        // Draw the Kyn as a colored circle (square for simplicity)
        var drawRect = new Rectangle(
            (int)(position.X - size / 2),
            (int)(position.Y - size / 2),
            size,
            size
        );

        spriteBatch.Draw(pixelTexture, drawRect, color);

        // Draw health bar if alive and requested
        if (showHealth && IsAlive)
        {
            var healthBarWidth = size + 4;
            var healthBarHeight = 4;
            var healthBarY = position.Y - size / 2 - healthBarHeight - 2;

            // Background
            var bgRect = new Rectangle(
                (int)(position.X - healthBarWidth / 2),
                (int)healthBarY,
                healthBarWidth,
                healthBarHeight
            );
            spriteBatch.Draw(pixelTexture, bgRect, Color.DarkGray);

            // Health fill
            var healthPercent = (float)CurrentHp / MaxHp;
            var healthColor = healthPercent > 0.5f ? Color.Green :
                             healthPercent > 0.25f ? Color.Yellow : Color.Red;

            var fillRect = new Rectangle(
                (int)(position.X - healthBarWidth / 2),
                (int)healthBarY,
                (int)(healthBarWidth * healthPercent),
                healthBarHeight
            );
            spriteBatch.Draw(pixelTexture, fillRect, healthColor);
        }
    }

    /// <summary>
    /// Creates save data for this Kyn.
    /// </summary>
    public KynSaveData ToSaveData()
    {
        // Convert AugmentationSlot keys to strings for serialization
        var augmentationData = new Dictionary<string, string?>();
        foreach (var kvp in EquippedAugmentations)
        {
            augmentationData[kvp.Key.ToString()] = kvp.Value;
        }

        return new KynSaveData
        {
            InstanceId = InstanceId,
            DefinitionId = Definition.Id,
            Nickname = Nickname,
            Level = Level,
            Experience = Experience,
            CurrentHp = CurrentHp,
            IsEvolved = IsEvolved,
            EquippedAugmentations = augmentationData,
            EquippedMicrochips = new List<string>(EquippedMicrochips),
            BondLevel = BondLevel
        };
    }

    /// <summary>
    /// Creates a Kyn from save data.
    /// </summary>
    public static Kyn? FromSaveData(KynSaveData data)
    {
        var definition = KynDefinitions.Get(data.DefinitionId);
        if (definition == null)
            return null;

        var kyn = new Kyn(definition, data.Level)
        {
            Nickname = data.Nickname,
            CurrentHp = data.CurrentHp,
            IsEvolved = data.IsEvolved,
            BondLevel = data.BondLevel
        };

        kyn.Experience = data.Experience;

        // Restore augmentations from string keys (e.g., "U_Dermis", "C_Chassis")
        foreach (var aug in data.EquippedAugmentations)
        {
            // Only restore if the slot key is valid for this Kyn
            if (kyn.EquippedAugmentations.ContainsKey(aug.Key))
            {
                kyn.EquippedAugmentations[aug.Key] = aug.Value;
            }
        }

        kyn.EquippedMicrochips.AddRange(data.EquippedMicrochips);

        return kyn;
    }
}
