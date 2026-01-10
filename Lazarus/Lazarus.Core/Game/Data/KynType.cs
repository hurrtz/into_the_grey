namespace Lazarus.Core.Game.Data;

/// <summary>
/// Biological classification of Kyn types.
/// Each type has unique augmentation capabilities and equipment compatibility.
/// </summary>
public enum KynType
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
/// Extension methods for KynType.
/// </summary>
public static class KynTypeExtensions
{
    /// <summary>
    /// Gets the display name for a Kyn type.
    /// </summary>
    public static string GetDisplayName(this KynType type) => type switch
    {
        KynType.Primate => "Primate",
        KynType.Carnivore => "Carnivore",
        KynType.Rodent => "Rodent",
        KynType.Lagomorph => "Lagomorph",
        KynType.Ungulate => "Ungulate",
        KynType.Marsupial => "Marsupial",
        KynType.Monotreme => "Monotreme",
        KynType.Chiroptera => "Chiroptera",
        KynType.Cetacean => "Cetacean",
        KynType.Proboscidean => "Proboscidean",
        KynType.Sirenian => "Sirenian",
        KynType.Eulipotyphlan => "Eulipotyphlan",
        KynType.Xenarthran => "Xenarthran",
        KynType.Pangolin => "Pangolin",
        KynType.Insect => "Insect",
        KynType.Arachnid => "Arachnid",
        KynType.Crustacean => "Crustacean",
        KynType.Mollusk => "Mollusk",
        KynType.Worm => "Worm",
        KynType.Echinoderm => "Echinoderm",
        KynType.Cnidarian => "Cnidarian",
        KynType.Sponge => "Sponge",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets whether this type is a vertebrate.
    /// </summary>
    public static bool IsVertebrate(this KynType type) => type switch
    {
        KynType.Insect or KynType.Arachnid or KynType.Crustacean or
        KynType.Mollusk or KynType.Worm or KynType.Echinoderm or
        KynType.Cnidarian or KynType.Sponge => false,
        _ => true
    };

    /// <summary>
    /// Gets whether this type is aquatic-capable.
    /// </summary>
    public static bool IsAquatic(this KynType type) => type switch
    {
        KynType.Cetacean or KynType.Sirenian or KynType.Mollusk or
        KynType.Crustacean or KynType.Echinoderm or KynType.Cnidarian or
        KynType.Sponge => true,
        _ => false
    };

    /// <summary>
    /// Gets whether this type can fly.
    /// </summary>
    public static bool CanFly(this KynType type) => type switch
    {
        KynType.Chiroptera or KynType.Insect => true,
        _ => false
    };

    /// <summary>
    /// Gets the signature ability category for this type.
    /// </summary>
    public static string GetSignatureAbility(this KynType type) => type switch
    {
        KynType.Primate => "Opposable Toolkit",
        KynType.Carnivore => "Scent Lock",
        KynType.Rodent => "Gnaw Module",
        KynType.Lagomorph => "Decoy Shed",
        KynType.Ungulate => "Herd Field",
        KynType.Marsupial => "Pouch Deploy",
        KynType.Monotreme => "Bio-Electric Pulse",
        KynType.Chiroptera => "Sonic Scramble",
        KynType.Cetacean => "Wave Cannon",
        KynType.Proboscidean => "Trunk Manipulator",
        KynType.Sirenian => "Soothing Field",
        KynType.Eulipotyphlan => "Earthsense",
        KynType.Xenarthran => "Shell Roll / Tongue Harpoon",
        KynType.Pangolin => "Scale Flare",
        KynType.Insect => "Pheromone Programs",
        KynType.Arachnid => "Web Projector",
        KynType.Crustacean => "Shell Stance",
        KynType.Mollusk => "Ink Cloud / Tentacle Multitool",
        KynType.Worm => "Soil Rewrite",
        KynType.Echinoderm => "Spine Bloom / Arm Split",
        KynType.Cnidarian => "Sting Field / Polyp Deploy",
        KynType.Sponge => "Reef Builder / Symbiote Slots",
        _ => "Unknown"
    };
}
