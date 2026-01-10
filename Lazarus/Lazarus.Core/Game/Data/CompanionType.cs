namespace Lazarus.Core.Game.Data;

/// <summary>
/// The type of companion the player chose at the start.
/// This determines the form and name of the companion Kyn.
/// </summary>
public enum CompanionType
{
    /// <summary>
    /// Dog - Bandit. Named in tribute to We3.
    /// The default choice, referencing Seymour from Futurama.
    /// </summary>
    Dog,

    /// <summary>
    /// Cat - Tinker.
    /// References Jonesy from Alien.
    /// </summary>
    Cat,

    /// <summary>
    /// Rabbit - Pirate.
    /// References Hazel from Watership Down.
    /// </summary>
    Rabbit
}

/// <summary>
/// Extension methods for CompanionType.
/// </summary>
public static class CompanionTypeExtensions
{
    /// <summary>
    /// Gets the companion's name based on their type.
    /// </summary>
    public static string GetCompanionName(this CompanionType type) => type switch
    {
        CompanionType.Dog => "Bandit",
        CompanionType.Cat => "Tinker",
        CompanionType.Rabbit => "Pirate",
        _ => "Bandit"
    };

    /// <summary>
    /// Gets the name of the pet as remembered from the simulation.
    /// </summary>
    public static string GetSimulationPetName(this CompanionType type) => type switch
    {
        CompanionType.Dog => "Seymour",
        CompanionType.Cat => "Jonesy",
        CompanionType.Rabbit => "Hazel",
        _ => "Seymour"
    };
}
