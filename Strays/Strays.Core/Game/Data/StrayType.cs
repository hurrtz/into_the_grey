namespace Strays.Core.Game.Data;

/// <summary>
/// Biological classification of Stray types.
/// Each type has unique augmentation capabilities and equipment compatibility.
/// </summary>
public enum StrayType
{
    /// <summary>
    /// Tool-users / stealth-tech generalists.
    /// Specialties: Hacking, gadgets, parkour.
    /// </summary>
    Primate,

    /// <summary>
    /// Predators / burst damage / pursuit.
    /// Specialties: Tracking, pounce attacks, marking targets.
    /// </summary>
    Carnivore,

    /// <summary>
    /// Scouts / sabotage / swarmy mobility.
    /// Specialties: Tunneling, device tampering, evasion.
    /// </summary>
    Rodent,

    /// <summary>
    /// Escape artists / speed / evasion.
    /// Specialties: Dash, decoys, near-360 awareness.
    /// </summary>
    Lagomorph,

    /// <summary>
    /// Tanks / momentum / team utility.
    /// Specialties: Charge attacks, herd buffs, stability.
    /// </summary>
    Ungulate,

    /// <summary>
    /// Adaptive skirmishers / inventory tricks.
    /// Specialties: Extra storage, deployables, climbing.
    /// </summary>
    Marsupial,

    /// <summary>
    /// Weird-tech specialists / utility & control.
    /// Specialties: Electroreception, EMP, amphibious.
    /// </summary>
    Monotreme,

    /// <summary>
    /// Aerial recon / sonar control (bats).
    /// Specialties: Echolocation, flight, sonic attacks.
    /// </summary>
    Chiroptera,

    /// <summary>
    /// Open-water powerhouses / sonar & ramming.
    /// Specialties: Deep diving, ramming, force pulses.
    /// </summary>
    Cetacean,

    /// <summary>
    /// Siege tanks / crowd control (elephants).
    /// Specialties: Massive armor, trunk manipulation, shockwaves.
    /// </summary>
    Proboscidean,

    /// <summary>
    /// Support bruisers / calm-zone controllers (manatees/dugongs).
    /// Specialties: Sustain regen, stealth swim, soothing auras.
    /// </summary>
    Sirenian,

    /// <summary>
    /// Tunnelers / close-range control (hedgehogs/moles/shrews).
    /// Specialties: Ground sonar, burrowing, trap detection.
    /// </summary>
    Eulipotyphlan,

    /// <summary>
    /// Armor or reach specialists (sloths/armadillos/anteaters).
    /// Specialties: Shell defense, grapple hooks, resource detection.
    /// </summary>
    Xenarthran,

    /// <summary>
    /// Rolling assassins / spike defense.
    /// Specialties: Roll attacks, scale armor, reflective shields.
    /// </summary>
    Pangolin,

    /// <summary>
    /// Specialists / modular builds.
    /// Specialties: Flight/jumps, pheromone clouds, hive coordination.
    /// </summary>
    Insect,

    /// <summary>
    /// Trappers / precision ambush.
    /// Specialties: Web traps, wall traversal, movement prediction.
    /// </summary>
    Arachnid,

    /// <summary>
    /// Bruisers / shield walls.
    /// Specialties: Heavy armor, grapple claws, deployable cover.
    /// </summary>
    Crustacean,

    /// <summary>
    /// Stealth & utility kings (cephalopods/snails/clams).
    /// Specialties: Active camo, ink clouds, jet propulsion.
    /// </summary>
    Mollusk,

    /// <summary>
    /// Infiltrators / terrain hackers.
    /// Specialties: Burrowing, regeneration, tunnel creation.
    /// </summary>
    Worm,

    /// <summary>
    /// Regeneration tanks / zone control (sea stars/urchins).
    /// Specialties: Limb regen, spike hazards, clone limbs.
    /// </summary>
    Echinoderm,

    /// <summary>
    /// Crowd control / AoE (jellyfish/coral/anemones).
    /// Specialties: Sting paralysis, deployable polyps, shock resistance.
    /// </summary>
    Cnidarian,

    /// <summary>
    /// Ultimate support / absorption / upgrade economy.
    /// Specialties: Damage absorption, toxin conversion, reef building.
    /// </summary>
    Sponge
}

/// <summary>
/// Extension methods for StrayType.
/// </summary>
public static class StrayTypeExtensions
{
    /// <summary>
    /// Gets the display name for a Stray type.
    /// </summary>
    public static string GetDisplayName(this StrayType type) => type switch
    {
        StrayType.Primate => "Primate",
        StrayType.Carnivore => "Carnivore",
        StrayType.Rodent => "Rodent",
        StrayType.Lagomorph => "Lagomorph",
        StrayType.Ungulate => "Ungulate",
        StrayType.Marsupial => "Marsupial",
        StrayType.Monotreme => "Monotreme",
        StrayType.Chiroptera => "Chiroptera",
        StrayType.Cetacean => "Cetacean",
        StrayType.Proboscidean => "Proboscidean",
        StrayType.Sirenian => "Sirenian",
        StrayType.Eulipotyphlan => "Eulipotyphlan",
        StrayType.Xenarthran => "Xenarthran",
        StrayType.Pangolin => "Pangolin",
        StrayType.Insect => "Insect",
        StrayType.Arachnid => "Arachnid",
        StrayType.Crustacean => "Crustacean",
        StrayType.Mollusk => "Mollusk",
        StrayType.Worm => "Worm",
        StrayType.Echinoderm => "Echinoderm",
        StrayType.Cnidarian => "Cnidarian",
        StrayType.Sponge => "Sponge",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets whether this type is a vertebrate.
    /// </summary>
    public static bool IsVertebrate(this StrayType type) => type switch
    {
        StrayType.Insect or StrayType.Arachnid or StrayType.Crustacean or
        StrayType.Mollusk or StrayType.Worm or StrayType.Echinoderm or
        StrayType.Cnidarian or StrayType.Sponge => false,
        _ => true
    };

    /// <summary>
    /// Gets whether this type is aquatic-capable.
    /// </summary>
    public static bool IsAquatic(this StrayType type) => type switch
    {
        StrayType.Cetacean or StrayType.Sirenian or StrayType.Mollusk or
        StrayType.Crustacean or StrayType.Echinoderm or StrayType.Cnidarian or
        StrayType.Sponge => true,
        _ => false
    };

    /// <summary>
    /// Gets whether this type can fly.
    /// </summary>
    public static bool CanFly(this StrayType type) => type switch
    {
        StrayType.Chiroptera or StrayType.Insect => true,
        _ => false
    };

    /// <summary>
    /// Gets the signature ability category for this type.
    /// </summary>
    public static string GetSignatureAbility(this StrayType type) => type switch
    {
        StrayType.Primate => "Opposable Toolkit",
        StrayType.Carnivore => "Scent Lock",
        StrayType.Rodent => "Gnaw Module",
        StrayType.Lagomorph => "Decoy Shed",
        StrayType.Ungulate => "Herd Field",
        StrayType.Marsupial => "Pouch Deploy",
        StrayType.Monotreme => "Bio-Electric Pulse",
        StrayType.Chiroptera => "Sonic Scramble",
        StrayType.Cetacean => "Wave Cannon",
        StrayType.Proboscidean => "Trunk Manipulator",
        StrayType.Sirenian => "Soothing Field",
        StrayType.Eulipotyphlan => "Earthsense",
        StrayType.Xenarthran => "Shell Roll / Tongue Harpoon",
        StrayType.Pangolin => "Scale Flare",
        StrayType.Insect => "Pheromone Programs",
        StrayType.Arachnid => "Web Projector",
        StrayType.Crustacean => "Shell Stance",
        StrayType.Mollusk => "Ink Cloud / Tentacle Multitool",
        StrayType.Worm => "Soil Rewrite",
        StrayType.Echinoderm => "Spine Bloom / Arm Split",
        StrayType.Cnidarian => "Sting Field / Polyp Deploy",
        StrayType.Sponge => "Reef Builder / Symbiote Slots",
        _ => "Unknown"
    };
}
