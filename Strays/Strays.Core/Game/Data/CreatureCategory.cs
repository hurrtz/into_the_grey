namespace Strays.Core.Game.Data;

/// <summary>
/// The 12 creature categories (Ordos) in the game.
/// 10 main categories + 2 "fun" categories with single creature types.
/// </summary>
public enum CreatureCategory
{
    /// <summary>
    /// Big chassis / bruiser / boss-frame mammals.
    /// Elephants, rhinos, hippos, giraffes, bison, etc.
    /// </summary>
    Colossomammalia,

    /// <summary>
    /// Small / fast / evasive / sabotage mammals.
    /// Rodents, rabbits, shrews, moles, etc.
    /// </summary>
    Micromammalia,

    /// <summary>
    /// Plated / spined / defensive gimmick mammals.
    /// Pangolins, armadillos, porcupines, hedgehogs, etc.
    /// </summary>
    Armormammalia,

    /// <summary>
    /// Arthropod umbrella: traps / venom / swarm / mech-shell.
    /// Insects, spiders, scorpions, crabs, etc.
    /// </summary>
    Exoskeletalis,

    /// <summary>
    /// CC / AoE / "bioelectric neon" jellyfish and similar.
    /// Jellyfish, man o' war, sea nettles, etc.
    /// </summary>
    Medusalia,

    /// <summary>
    /// Stealth / grapple / ink / "smart horror" octopuses.
    /// All octopus species.
    /// </summary>
    Octomorpha,

    /// <summary>
    /// Shell / blade / toxin / "biotech material" mollusks.
    /// Squid, cuttlefish, nautilus, clams, snails, etc.
    /// </summary>
    Mollusca,

    /// <summary>
    /// Operator / tool-user / "hands" fantasy primates.
    /// Apes, monkeys, lemurs, etc.
    /// </summary>
    Manipularis,

    /// <summary>
    /// Hunter / burst / pursuit predators.
    /// Wolves, big cats, bears, hyenas, etc.
    /// </summary>
    Predatoria,

    /// <summary>
    /// Skirmisher / weird utility / "pouch tech" marsupials.
    /// Kangaroos, koalas, wombats, possums, etc.
    /// </summary>
    Marsupialis,

    /// <summary>
    /// Fun category: The Platypus. Weird-tech specialist.
    /// Only contains: Platypus.
    /// </summary>
    Obscura,

    /// <summary>
    /// Fun category: The Sloth. Zen tank specialist.
    /// Only contains: Sloth.
    /// </summary>
    Tardigrada
}

/// <summary>
/// Extension methods for CreatureCategory.
/// </summary>
public static class CreatureCategoryExtensions
{
    /// <summary>
    /// Gets the full Latin-style name for a category.
    /// </summary>
    public static string GetOrdoName(this CreatureCategory category) => category switch
    {
        CreatureCategory.Colossomammalia => "Ordo Colossomammalia",
        CreatureCategory.Micromammalia => "Ordo Micromammalia",
        CreatureCategory.Armormammalia => "Ordo Armormammalia",
        CreatureCategory.Exoskeletalis => "Ordo Exoskeletalis",
        CreatureCategory.Medusalia => "Ordo Medusalia",
        CreatureCategory.Octomorpha => "Ordo Octomorpha",
        CreatureCategory.Mollusca => "Ordo Mollusca",
        CreatureCategory.Manipularis => "Ordo Manipularis",
        CreatureCategory.Predatoria => "Ordo Predatoria",
        CreatureCategory.Marsupialis => "Ordo Marsupialis",
        CreatureCategory.Obscura => "Ordo Obscura",
        CreatureCategory.Tardigrada => "Ordo Tardigrada",
        _ => "Unknown Ordo"
    };

    /// <summary>
    /// Gets a short display name for a category.
    /// </summary>
    public static string GetDisplayName(this CreatureCategory category) => category switch
    {
        CreatureCategory.Colossomammalia => "Colossal Mammals",
        CreatureCategory.Micromammalia => "Micro Mammals",
        CreatureCategory.Armormammalia => "Armored Mammals",
        CreatureCategory.Exoskeletalis => "Exoskeletons",
        CreatureCategory.Medusalia => "Jellyfish",
        CreatureCategory.Octomorpha => "Octopuses",
        CreatureCategory.Mollusca => "Mollusks",
        CreatureCategory.Manipularis => "Primates",
        CreatureCategory.Predatoria => "Predators",
        CreatureCategory.Marsupialis => "Marsupials",
        CreatureCategory.Obscura => "Obscura",
        CreatureCategory.Tardigrada => "Tardigrada",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets a description of the category's combat role.
    /// </summary>
    public static string GetRoleDescription(this CreatureCategory category) => category switch
    {
        CreatureCategory.Colossomammalia => "Big chassis / bruiser / boss-frame",
        CreatureCategory.Micromammalia => "Small / fast / evasive / sabotage",
        CreatureCategory.Armormammalia => "Plated / spined / defensive gimmicks",
        CreatureCategory.Exoskeletalis => "Traps / venom / swarm / mech-shell",
        CreatureCategory.Medusalia => "CC / AoE / bioelectric neon",
        CreatureCategory.Octomorpha => "Stealth / grapple / ink / smart horror",
        CreatureCategory.Mollusca => "Shell / blade / toxin / biotech material",
        CreatureCategory.Manipularis => "Operator / tool-user / hands fantasy",
        CreatureCategory.Predatoria => "Hunter / burst / pursuit",
        CreatureCategory.Marsupialis => "Skirmisher / weird utility / pouch tech",
        CreatureCategory.Obscura => "Weird-tech specialist / electroreception",
        CreatureCategory.Tardigrada => "Zen tank / slow but unstoppable",
        _ => "Unknown role"
    };

    /// <summary>
    /// Gets whether this is a "fun" category with only one creature type.
    /// </summary>
    public static bool IsFunCategory(this CreatureCategory category) =>
        category == CreatureCategory.Obscura || category == CreatureCategory.Tardigrada;

    /// <summary>
    /// Gets the number of category-specific augmentation slots.
    /// Most have 4, Exoskeletalis has 5.
    /// </summary>
    public static int GetSpecificSlotCount(this CreatureCategory category) =>
        category == CreatureCategory.Exoskeletalis ? 5 : 4;
}
