using System;
using System.Collections.Generic;
using System.Linq;
using Strays.Core.Game.Data;

namespace Strays.Core.Game.Items;

/// <summary>
/// Universal augmentation slots shared by all creature categories (9 slots).
/// </summary>
public enum UniversalSlot
{
    /// <summary>
    /// Skin / outer layer / surface tech.
    /// Affects: Armor, camouflage, environmental resistance.
    /// </summary>
    Dermis,

    /// <summary>
    /// Eyes / vision sensor suite.
    /// Affects: Detection, targeting, awareness.
    /// </summary>
    Optics,

    /// <summary>
    /// Hearing / vibration mic / sonar-lite.
    /// Affects: Sound detection, echolocation, vibration sensing.
    /// </summary>
    Aural,

    /// <summary>
    /// Smell / chemoreception / toxin sensing.
    /// Affects: Tracking, poison detection, pheromone reading.
    /// </summary>
    OlfactoryChem,

    /// <summary>
    /// Heart + metabolism + heat management as one slot.
    /// Affects: Stamina, regeneration, temperature control.
    /// </summary>
    Core,

    /// <summary>
    /// Lungs / filters / oxygen storage.
    /// Affects: Endurance, environmental survival, breath capacity.
    /// </summary>
    Respiratory,

    /// <summary>
    /// Brain interface / cyberdeck port / cognition.
    /// Affects: Hacking, intelligence, ability power.
    /// </summary>
    Neural,

    /// <summary>
    /// Spine/nerve trunk: reflexes, stun resistance, signal routing.
    /// Affects: Speed, status resistance, reaction time.
    /// </summary>
    CNS,

    /// <summary>
    /// Legs/fins/wings equivalent: movement module.
    /// Affects: Movement speed, traversal options, mobility abilities.
    /// </summary>
    Locomotion
}

/// <summary>
/// Category-specific augmentation slots (4-5 per Ordo).
/// </summary>
public enum CategorySlot
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO COLOSSOMAMMALIA SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Heavy mount / carry / brace system.
    /// Colossomammalia only.
    /// </summary>
    SiegeHarness,

    /// <summary>
    /// Charge/ram mechanics socket.
    /// Colossomammalia only.
    /// </summary>
    MomentumCore,

    /// <summary>
    /// Stomp/groundwave socket.
    /// Colossomammalia only.
    /// </summary>
    SeismicActuator,

    /// <summary>
    /// Big cooldown/overheat handling.
    /// Colossomammalia only.
    /// </summary>
    BulkThermalStack,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MICROMAMMALIA SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Small-body rig: vents, squeeze paths.
    /// Micromammalia only.
    /// </summary>
    Microframe,

    /// <summary>
    /// Dig/exit points socket.
    /// Micromammalia only.
    /// </summary>
    BurrowKit,

    /// <summary>
    /// Panic-escape / decoy / auto-disengage socket.
    /// Micromammalia only.
    /// </summary>
    EvasionPod,

    /// <summary>
    /// Tiny deployable compartment.
    /// Micromammalia only.
    /// </summary>
    PocketBay,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO ARMORMAMMALIA SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Curl/roll mode socket.
    /// Armormammalia only.
    /// </summary>
    RollChassis,

    /// <summary>
    /// Armor plates interlink socket.
    /// Armormammalia only.
    /// </summary>
    ScaleLattice,

    /// <summary>
    /// Spines/caltrops/hazard drop socket.
    /// Armormammalia only.
    /// </summary>
    SpineRack,

    /// <summary>
    /// Turtle-up / mobile cover socket.
    /// Armormammalia only.
    /// </summary>
    AnchorBracing,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO EXOSKELETALIS SLOTS (5 - exception!)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Extra plating segments / modular shell socket.
    /// Exoskeletalis only.
    /// </summary>
    CarapaceSegmentBus,

    /// <summary>
    /// Delivery system socket for venom/chemicals.
    /// Exoskeletalis only.
    /// </summary>
    VenomChemInjector,

    /// <summary>
    /// Trapline/anchorline socket.
    /// Exoskeletalis only.
    /// </summary>
    WebLineProjector,

    /// <summary>
    /// Swarm/team-link socket.
    /// Exoskeletalis only.
    /// </summary>
    HiveTransceiver,

    /// <summary>
    /// Flight capability for winged arthropods.
    /// Exoskeletalis only.
    /// </summary>
    WingMount,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MEDUSALIA SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sting/aura socket.
    /// Medusalia only.
    /// </summary>
    NematocystRing,

    /// <summary>
    /// Shockburst / discharge socket.
    /// Medusalia only.
    /// </summary>
    PulseOrgan,

    /// <summary>
    /// Buoyancy/float control socket.
    /// Medusalia only.
    /// </summary>
    DriftMatrix,

    /// <summary>
    /// Deployable node/turret socket.
    /// Medusalia only.
    /// </summary>
    PolypSeeder,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO OCTOMORPHA SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Extra "hand" channels socket.
    /// Octomorpha only.
    /// </summary>
    MultiArmToolbus,

    /// <summary>
    /// Hard tether / yank / pin socket.
    /// Octomorpha only.
    /// </summary>
    GrappleAnchor,

    /// <summary>
    /// LOS-break / sensor jam socket.
    /// Octomorpha only.
    /// </summary>
    InkGland,

    /// <summary>
    /// Deception/copy mechanics socket.
    /// Octomorpha only.
    /// </summary>
    MimicSuite,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MOLLUSCA SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mounts-on-shell socket for shelled builds.
    /// Mollusca only.
    /// </summary>
    ShellLattice,

    /// <summary>
    /// Jet propulsion socket for cephalopod-ish movers.
    /// Mollusca only.
    /// </summary>
    MantleJet,

    /// <summary>
    /// Mouth-tool weapon socket.
    /// Mollusca only.
    /// </summary>
    RadulaCarriage,

    /// <summary>
    /// Slime/zone-control socket.
    /// Mollusca only.
    /// </summary>
    MucusTrail,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MANIPULARIS SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Extra utility channel socket.
    /// Manipularis only.
    /// </summary>
    MicrotoolRig,

    /// <summary>
    /// Combo-casting/skill chaining socket.
    /// Manipularis only.
    /// </summary>
    GestureConduit,

    /// <summary>
    /// Climb/disarm/throw control socket.
    /// Manipularis only.
    /// </summary>
    PrecisionGrip,

    /// <summary>
    /// Micro-drone docking socket.
    /// Manipularis only.
    /// </summary>
    DroneCradle,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO PREDATORIA SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mark/track socket.
    /// Predatoria only.
    /// </summary>
    PreyLock,

    /// <summary>
    /// Leap-to-attack socket.
    /// Predatoria only.
    /// </summary>
    PounceRails,

    /// <summary>
    /// Hold/drag/disable socket.
    /// Predatoria only.
    /// </summary>
    ClampModule,

    /// <summary>
    /// Fear/pressure aura socket.
    /// Predatoria only.
    /// </summary>
    TerrorProjector,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO MARSUPIALIS SLOTS (4)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Inventory + instant deploy socket.
    /// Marsupialis only.
    /// </summary>
    PouchBay,

    /// <summary>
    /// Drop/throw without action cost socket.
    /// Marsupialis only.
    /// </summary>
    PouchLauncher,

    /// <summary>
    /// For glider-style variants socket.
    /// Marsupialis only.
    /// </summary>
    GlideRig,

    /// <summary>
    /// Protect carried payload/consumables socket.
    /// Marsupialis only.
    /// </summary>
    CradleStabilizer,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO OBSCURA SLOTS (4) - Platypus
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Electroreception enhancement.
    /// Obscura only.
    /// </summary>
    ElectroBill,

    /// <summary>
    /// Venomous spur system.
    /// Obscura only.
    /// </summary>
    VenomSpur,

    /// <summary>
    /// Amphibious adaptation.
    /// Obscura only.
    /// </summary>
    AquaticModule,

    /// <summary>
    /// Weird-tech hybrid system.
    /// Obscura only.
    /// </summary>
    AnomalyCore,

    // ═══════════════════════════════════════════════════════════════════════════
    // ORDO TARDIGRADA SLOTS (4) - Sloth
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Extreme patience / regeneration.
    /// Tardigrada only.
    /// </summary>
    ZenCore,

    /// <summary>
    /// Symbiotic algae / ecosystem.
    /// Tardigrada only.
    /// </summary>
    SymbioteGarden,

    /// <summary>
    /// Slow but inevitable grip.
    /// Tardigrada only.
    /// </summary>
    IronGrasp,

    /// <summary>
    /// Damage absorption over time.
    /// Tardigrada only.
    /// </summary>
    EntropyBuffer
}

/// <summary>
/// Utility class for working with augmentation slots.
/// </summary>
public static class AugmentationSlotUtility
{
    private static readonly Dictionary<CategorySlot, CreatureCategory> _slotToCategory = new();
    private static bool _initialized = false;

    /// <summary>
    /// Gets all universal slots.
    /// </summary>
    public static IEnumerable<UniversalSlot> GetUniversalSlots()
    {
        return Enum.GetValues<UniversalSlot>();
    }

    /// <summary>
    /// Gets the category-specific slots for a given category.
    /// </summary>
    public static IEnumerable<CategorySlot> GetCategorySlotsFor(CreatureCategory category)
    {
        EnsureInitialized();
        foreach (var kvp in _slotToCategory)
        {
            if (kvp.Value == category)
                yield return kvp.Key;
        }
    }

    /// <summary>
    /// Gets the category that owns a specific slot.
    /// </summary>
    public static CreatureCategory? GetCategoryForSlot(CategorySlot slot)
    {
        EnsureInitialized();
        return _slotToCategory.TryGetValue(slot, out var category) ? category : null;
    }

    /// <summary>
    /// Checks if a category slot is valid for a creature category.
    /// </summary>
    public static bool IsSlotValidForCategory(CategorySlot slot, CreatureCategory category)
    {
        EnsureInitialized();
        return _slotToCategory.TryGetValue(slot, out var slotCategory) && slotCategory == category;
    }

    /// <summary>
    /// Gets the total number of augmentation slots for a category.
    /// 9 universal + 4-5 category-specific.
    /// </summary>
    public static int GetTotalSlotCount(CreatureCategory category)
    {
        return 9 + category.GetSpecificSlotCount();
    }

    /// <summary>
    /// Gets the display name for a universal slot.
    /// </summary>
    public static string GetDisplayName(UniversalSlot slot) => slot switch
    {
        UniversalSlot.Dermis => "Dermis",
        UniversalSlot.Optics => "Optics",
        UniversalSlot.Aural => "Aural",
        UniversalSlot.OlfactoryChem => "Olfactory/Chem",
        UniversalSlot.Core => "Core",
        UniversalSlot.Respiratory => "Respiratory",
        UniversalSlot.Neural => "Neural",
        UniversalSlot.CNS => "CNS",
        UniversalSlot.Locomotion => "Locomotion",
        _ => slot.ToString()
    };

    /// <summary>
    /// Gets the display name for a category slot.
    /// </summary>
    public static string GetDisplayName(CategorySlot slot) => slot switch
    {
        // Colossomammalia
        CategorySlot.SiegeHarness => "Siege Harness",
        CategorySlot.MomentumCore => "Momentum Core",
        CategorySlot.SeismicActuator => "Seismic Actuator",
        CategorySlot.BulkThermalStack => "Bulk Thermal Stack",

        // Micromammalia
        CategorySlot.Microframe => "Microframe",
        CategorySlot.BurrowKit => "Burrow Kit",
        CategorySlot.EvasionPod => "Evasion Pod",
        CategorySlot.PocketBay => "Pocket Bay",

        // Armormammalia
        CategorySlot.RollChassis => "Roll Chassis",
        CategorySlot.ScaleLattice => "Scale Lattice",
        CategorySlot.SpineRack => "Spine Rack",
        CategorySlot.AnchorBracing => "Anchor Bracing",

        // Exoskeletalis
        CategorySlot.CarapaceSegmentBus => "Carapace Segment Bus",
        CategorySlot.VenomChemInjector => "Venom/Chem Injector",
        CategorySlot.WebLineProjector => "Web/Line Projector",
        CategorySlot.HiveTransceiver => "Hive Transceiver",
        CategorySlot.WingMount => "Wing Mount",

        // Medusalia
        CategorySlot.NematocystRing => "Nematocyst Ring",
        CategorySlot.PulseOrgan => "Pulse Organ",
        CategorySlot.DriftMatrix => "Drift Matrix",
        CategorySlot.PolypSeeder => "Polyp Seeder",

        // Octomorpha
        CategorySlot.MultiArmToolbus => "Multi-Arm Toolbus",
        CategorySlot.GrappleAnchor => "Grapple Anchor",
        CategorySlot.InkGland => "Ink Gland",
        CategorySlot.MimicSuite => "Mimic Suite",

        // Mollusca
        CategorySlot.ShellLattice => "Shell Lattice",
        CategorySlot.MantleJet => "Mantle Jet",
        CategorySlot.RadulaCarriage => "Radula Carriage",
        CategorySlot.MucusTrail => "Mucus Trail",

        // Manipularis
        CategorySlot.MicrotoolRig => "Microtool Rig",
        CategorySlot.GestureConduit => "Gesture Conduit",
        CategorySlot.PrecisionGrip => "Precision Grip",
        CategorySlot.DroneCradle => "Drone Cradle",

        // Predatoria
        CategorySlot.PreyLock => "Prey-Lock",
        CategorySlot.PounceRails => "Pounce Rails",
        CategorySlot.ClampModule => "Clamp Module",
        CategorySlot.TerrorProjector => "Terror Projector",

        // Marsupialis
        CategorySlot.PouchBay => "Pouch Bay",
        CategorySlot.PouchLauncher => "Pouch Launcher",
        CategorySlot.GlideRig => "Glide Rig",
        CategorySlot.CradleStabilizer => "Cradle Stabilizer",

        // Obscura
        CategorySlot.ElectroBill => "Electro-Bill",
        CategorySlot.VenomSpur => "Venom Spur",
        CategorySlot.AquaticModule => "Aquatic Module",
        CategorySlot.AnomalyCore => "Anomaly Core",

        // Tardigrada
        CategorySlot.ZenCore => "Zen Core",
        CategorySlot.SymbioteGarden => "Symbiote Garden",
        CategorySlot.IronGrasp => "Iron Grasp",
        CategorySlot.EntropyBuffer => "Entropy Buffer",

        _ => slot.ToString()
    };

    /// <summary>
    /// Gets a description for a universal slot.
    /// </summary>
    public static string GetDescription(UniversalSlot slot) => slot switch
    {
        UniversalSlot.Dermis => "Skin / outer layer / surface tech",
        UniversalSlot.Optics => "Eyes / vision sensor suite",
        UniversalSlot.Aural => "Hearing / vibration mic / sonar-lite",
        UniversalSlot.OlfactoryChem => "Smell / chemoreception / toxin sensing",
        UniversalSlot.Core => "Heart + metabolism + heat management",
        UniversalSlot.Respiratory => "Lungs / filters / oxygen storage",
        UniversalSlot.Neural => "Brain interface / cyberdeck port / cognition",
        UniversalSlot.CNS => "Spine/nerve trunk: reflexes, stun resistance",
        UniversalSlot.Locomotion => "Legs/fins/wings: movement module",
        _ => "Unknown slot"
    };

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;
        Initialize();
    }

    private static void Initialize()
    {
        // Colossomammalia (4)
        _slotToCategory[CategorySlot.SiegeHarness] = CreatureCategory.Colossomammalia;
        _slotToCategory[CategorySlot.MomentumCore] = CreatureCategory.Colossomammalia;
        _slotToCategory[CategorySlot.SeismicActuator] = CreatureCategory.Colossomammalia;
        _slotToCategory[CategorySlot.BulkThermalStack] = CreatureCategory.Colossomammalia;

        // Micromammalia (4)
        _slotToCategory[CategorySlot.Microframe] = CreatureCategory.Micromammalia;
        _slotToCategory[CategorySlot.BurrowKit] = CreatureCategory.Micromammalia;
        _slotToCategory[CategorySlot.EvasionPod] = CreatureCategory.Micromammalia;
        _slotToCategory[CategorySlot.PocketBay] = CreatureCategory.Micromammalia;

        // Armormammalia (4)
        _slotToCategory[CategorySlot.RollChassis] = CreatureCategory.Armormammalia;
        _slotToCategory[CategorySlot.ScaleLattice] = CreatureCategory.Armormammalia;
        _slotToCategory[CategorySlot.SpineRack] = CreatureCategory.Armormammalia;
        _slotToCategory[CategorySlot.AnchorBracing] = CreatureCategory.Armormammalia;

        // Exoskeletalis (5)
        _slotToCategory[CategorySlot.CarapaceSegmentBus] = CreatureCategory.Exoskeletalis;
        _slotToCategory[CategorySlot.VenomChemInjector] = CreatureCategory.Exoskeletalis;
        _slotToCategory[CategorySlot.WebLineProjector] = CreatureCategory.Exoskeletalis;
        _slotToCategory[CategorySlot.HiveTransceiver] = CreatureCategory.Exoskeletalis;
        _slotToCategory[CategorySlot.WingMount] = CreatureCategory.Exoskeletalis;

        // Medusalia (4)
        _slotToCategory[CategorySlot.NematocystRing] = CreatureCategory.Medusalia;
        _slotToCategory[CategorySlot.PulseOrgan] = CreatureCategory.Medusalia;
        _slotToCategory[CategorySlot.DriftMatrix] = CreatureCategory.Medusalia;
        _slotToCategory[CategorySlot.PolypSeeder] = CreatureCategory.Medusalia;

        // Octomorpha (4)
        _slotToCategory[CategorySlot.MultiArmToolbus] = CreatureCategory.Octomorpha;
        _slotToCategory[CategorySlot.GrappleAnchor] = CreatureCategory.Octomorpha;
        _slotToCategory[CategorySlot.InkGland] = CreatureCategory.Octomorpha;
        _slotToCategory[CategorySlot.MimicSuite] = CreatureCategory.Octomorpha;

        // Mollusca (4)
        _slotToCategory[CategorySlot.ShellLattice] = CreatureCategory.Mollusca;
        _slotToCategory[CategorySlot.MantleJet] = CreatureCategory.Mollusca;
        _slotToCategory[CategorySlot.RadulaCarriage] = CreatureCategory.Mollusca;
        _slotToCategory[CategorySlot.MucusTrail] = CreatureCategory.Mollusca;

        // Manipularis (4)
        _slotToCategory[CategorySlot.MicrotoolRig] = CreatureCategory.Manipularis;
        _slotToCategory[CategorySlot.GestureConduit] = CreatureCategory.Manipularis;
        _slotToCategory[CategorySlot.PrecisionGrip] = CreatureCategory.Manipularis;
        _slotToCategory[CategorySlot.DroneCradle] = CreatureCategory.Manipularis;

        // Predatoria (4)
        _slotToCategory[CategorySlot.PreyLock] = CreatureCategory.Predatoria;
        _slotToCategory[CategorySlot.PounceRails] = CreatureCategory.Predatoria;
        _slotToCategory[CategorySlot.ClampModule] = CreatureCategory.Predatoria;
        _slotToCategory[CategorySlot.TerrorProjector] = CreatureCategory.Predatoria;

        // Marsupialis (4)
        _slotToCategory[CategorySlot.PouchBay] = CreatureCategory.Marsupialis;
        _slotToCategory[CategorySlot.PouchLauncher] = CreatureCategory.Marsupialis;
        _slotToCategory[CategorySlot.GlideRig] = CreatureCategory.Marsupialis;
        _slotToCategory[CategorySlot.CradleStabilizer] = CreatureCategory.Marsupialis;

        // Obscura (4)
        _slotToCategory[CategorySlot.ElectroBill] = CreatureCategory.Obscura;
        _slotToCategory[CategorySlot.VenomSpur] = CreatureCategory.Obscura;
        _slotToCategory[CategorySlot.AquaticModule] = CreatureCategory.Obscura;
        _slotToCategory[CategorySlot.AnomalyCore] = CreatureCategory.Obscura;

        // Tardigrada (4)
        _slotToCategory[CategorySlot.ZenCore] = CreatureCategory.Tardigrada;
        _slotToCategory[CategorySlot.SymbioteGarden] = CreatureCategory.Tardigrada;
        _slotToCategory[CategorySlot.IronGrasp] = CreatureCategory.Tardigrada;
        _slotToCategory[CategorySlot.EntropyBuffer] = CreatureCategory.Tardigrada;
    }
}

/// <summary>
/// Represents a slot reference that can be either universal or category-specific.
/// </summary>
public readonly struct SlotReference : IEquatable<SlotReference>
{
    /// <summary>
    /// True if this is a universal slot, false if category-specific.
    /// </summary>
    public bool IsUniversal { get; }

    /// <summary>
    /// The universal slot (valid if IsUniversal is true).
    /// </summary>
    public UniversalSlot? UniversalSlot { get; }

    /// <summary>
    /// The category slot (valid if IsUniversal is false).
    /// </summary>
    public CategorySlot? CategorySlot { get; }

    /// <summary>
    /// Creates a reference to a universal slot.
    /// </summary>
    public SlotReference(UniversalSlot slot)
    {
        IsUniversal = true;
        UniversalSlot = slot;
        CategorySlot = null;
    }

    /// <summary>
    /// Creates a reference to a category-specific slot.
    /// </summary>
    public SlotReference(CategorySlot slot)
    {
        IsUniversal = false;
        UniversalSlot = null;
        CategorySlot = slot;
    }

    /// <summary>
    /// Gets the display name of this slot.
    /// </summary>
    public string GetDisplayName()
    {
        if (IsUniversal && UniversalSlot.HasValue)
            return AugmentationSlotUtility.GetDisplayName(UniversalSlot.Value);
        if (!IsUniversal && CategorySlot.HasValue)
            return AugmentationSlotUtility.GetDisplayName(CategorySlot.Value);
        return "Unknown";
    }

    /// <summary>
    /// Gets the unique string key for this slot (for serialization).
    /// </summary>
    public string ToKey()
    {
        if (IsUniversal && UniversalSlot.HasValue)
            return $"U_{UniversalSlot.Value}";
        if (!IsUniversal && CategorySlot.HasValue)
            return $"C_{CategorySlot.Value}";
        return "Unknown";
    }

    /// <summary>
    /// Creates a SlotReference from a serialized key.
    /// </summary>
    public static SlotReference? FromKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 3)
            return null;

        var prefix = key[..2];
        var value = key[2..];

        if (prefix == "U_" && Enum.TryParse<UniversalSlot>(value, out var uSlot))
            return new SlotReference(uSlot);
        if (prefix == "C_" && Enum.TryParse<CategorySlot>(value, out var cSlot))
            return new SlotReference(cSlot);

        return null;
    }

    /// <summary>
    /// Checks if this slot is valid for a given creature category.
    /// Universal slots are always valid; category slots must match.
    /// </summary>
    public bool IsValidForCategory(CreatureCategory category)
    {
        if (IsUniversal)
            return true;

        if (CategorySlot.HasValue)
            return AugmentationSlotUtility.IsSlotValidForCategory(CategorySlot.Value, category);

        return false;
    }

    public override string ToString() => GetDisplayName();

    public bool Equals(SlotReference other) =>
        IsUniversal == other.IsUniversal &&
        UniversalSlot == other.UniversalSlot &&
        CategorySlot == other.CategorySlot;

    public override bool Equals(object? obj) => obj is SlotReference other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(IsUniversal, UniversalSlot, CategorySlot);

    public static bool operator ==(SlotReference left, SlotReference right) => left.Equals(right);
    public static bool operator !=(SlotReference left, SlotReference right) => !left.Equals(right);

    public static implicit operator SlotReference(UniversalSlot slot) => new(slot);
    public static implicit operator SlotReference(CategorySlot slot) => new(slot);
}
