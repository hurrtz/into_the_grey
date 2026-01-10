namespace Lazarus.Core.Game.Data;

/// <summary>
/// The escalation stages of Bandit's Gravitation ability.
/// As the Boost Control System tightens its grip, Gravitation becomes more dangerous.
/// </summary>
public enum GravitationStage
{
    /// <summary>
    /// Act 1 / Early Act 2: 50% HP damage to enemies only.
    /// The player sees this as a helpful companion ability.
    /// </summary>
    Normal,

    /// <summary>
    /// Mid Act 2: 70% HP damage, can hit random targets including allies.
    /// First signs of corruption.
    /// </summary>
    Unstable,

    /// <summary>
    /// Late Act 2: 90% HP damage, frequently hits allies.
    /// The companion becomes a liability.
    /// </summary>
    Dangerous,

    /// <summary>
    /// Pre-departure: 99% HP damage, uncontrollable.
    /// Nearly kills a party member, causing Bandit to leave.
    /// </summary>
    Critical,

    /// <summary>
    /// Final boss form: 99% HP, absolute, battlefield-reshaping.
    /// Bandit cannot be defeated - only survived.
    /// </summary>
    Absolute
}

/// <summary>
/// Extension methods for GravitationStage.
/// </summary>
public static class GravitationStageExtensions
{
    /// <summary>
    /// Gets the HP percentage damage for this stage.
    /// </summary>
    public static float GetDamagePercent(this GravitationStage stage) => stage switch
    {
        GravitationStage.Normal => 0.50f,
        GravitationStage.Unstable => 0.70f,
        GravitationStage.Dangerous => 0.90f,
        GravitationStage.Critical => 0.99f,
        GravitationStage.Absolute => 0.99f,
        _ => 0.50f
    };

    /// <summary>
    /// Gets the chance that Gravitation will hit an ally instead of an enemy.
    /// </summary>
    public static float GetAllyTargetChance(this GravitationStage stage) => stage switch
    {
        GravitationStage.Normal => 0.0f,
        GravitationStage.Unstable => 0.20f,
        GravitationStage.Dangerous => 0.50f,
        GravitationStage.Critical => 0.80f,
        GravitationStage.Absolute => 0.0f, // Always targets party in final boss fight
        _ => 0.0f
    };
}
