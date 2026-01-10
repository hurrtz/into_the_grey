using System.Collections.Generic;

namespace Strays.Core.Game.Dialog;

/// <summary>
/// Emotion/tone tags that can be used in Stray speech.
/// </summary>
public enum DialogEmotion
{
    Neutral,
    Happy,
    Sad,
    Angry,
    Scared,
    Curious,
    Confused,
    Excited,
    Tired,
    Hopeful,
    Desperate,
    Sarcastic,
    Thoughtful
}

/// <summary>
/// Types of speakers in dialog.
/// </summary>
public enum SpeakerType
{
    /// <summary>
    /// The protagonist (internal monologue or rare speech).
    /// </summary>
    Protagonist,

    /// <summary>
    /// The companion (Bandit/Tinker/Pirate).
    /// </summary>
    Companion,

    /// <summary>
    /// A party Stray.
    /// </summary>
    PartyStray,

    /// <summary>
    /// An NPC.
    /// </summary>
    Npc,

    /// <summary>
    /// A wild or enemy Stray.
    /// </summary>
    WildStray,

    /// <summary>
    /// System/narrator text.
    /// </summary>
    System,

    /// <summary>
    /// Lazarus or other AI system.
    /// </summary>
    AI
}

/// <summary>
/// A choice option presented to the player during dialog.
/// </summary>
public class DialogChoice
{
    /// <summary>
    /// Text displayed for this choice.
    /// </summary>
    public string Text { get; init; } = "";

    /// <summary>
    /// ID of the dialog to jump to if this choice is selected.
    /// </summary>
    public string? NextDialogId { get; init; }

    /// <summary>
    /// Flag to set if this choice is selected.
    /// </summary>
    public string? SetsFlag { get; init; }

    /// <summary>
    /// Flag required for this choice to appear.
    /// </summary>
    public string? RequiresFlag { get; init; }

    /// <summary>
    /// Whether this choice ends the dialog.
    /// </summary>
    public bool EndsDialog { get; init; }

    /// <summary>
    /// Quest to start when this choice is selected.
    /// </summary>
    public string? StartsQuest { get; init; }

    /// <summary>
    /// Quest to complete when this choice is selected.
    /// </summary>
    public string? CompletesQuest { get; init; }
}

/// <summary>
/// A single line of dialog.
/// </summary>
public class DialogLine
{
    /// <summary>
    /// Type of speaker.
    /// </summary>
    public SpeakerType SpeakerType { get; init; } = SpeakerType.System;

    /// <summary>
    /// ID of the specific speaker (NPC ID, Stray ID, etc.).
    /// </summary>
    public string? SpeakerId { get; init; }

    /// <summary>
    /// Display name of the speaker (overrides default if set).
    /// </summary>
    public string? SpeakerName { get; init; }

    /// <summary>
    /// The emotion/tone of this line (for Stray speech patterns).
    /// </summary>
    public DialogEmotion Emotion { get; init; } = DialogEmotion.Neutral;

    /// <summary>
    /// The text content of the line.
    /// </summary>
    public string Text { get; init; } = "";

    /// <summary>
    /// Choices presented after this line (if any).
    /// </summary>
    public List<DialogChoice> Choices { get; init; } = new();

    /// <summary>
    /// Whether this line has choices.
    /// </summary>
    public bool HasChoices => Choices.Count > 0;

    /// <summary>
    /// Flag to set when this line is displayed.
    /// </summary>
    public string? SetsFlag { get; init; }

    /// <summary>
    /// Quest to start when this line is displayed.
    /// </summary>
    public string? StartsQuest { get; init; }

    /// <summary>
    /// Quest to complete when this line is displayed.
    /// </summary>
    public string? CompletesQuest { get; init; }

    /// <summary>
    /// Objective to progress when this line is displayed.
    /// </summary>
    public string? ProgressesObjective { get; init; }

    /// <summary>
    /// Duration to display the line (0 = wait for input).
    /// </summary>
    public float Duration { get; init; } = 0;

    /// <summary>
    /// Creates a simple dialog line.
    /// </summary>
    public static DialogLine Create(SpeakerType type, string text, DialogEmotion emotion = DialogEmotion.Neutral)
    {
        return new DialogLine
        {
            SpeakerType = type,
            Text = text,
            Emotion = emotion
        };
    }

    /// <summary>
    /// Creates a dialog line from an NPC.
    /// </summary>
    public static DialogLine FromNpc(string npcId, string name, string text)
    {
        return new DialogLine
        {
            SpeakerType = SpeakerType.Npc,
            SpeakerId = npcId,
            SpeakerName = name,
            Text = text
        };
    }

    /// <summary>
    /// Creates a dialog line from the companion.
    /// </summary>
    public static DialogLine FromCompanion(string text, DialogEmotion emotion = DialogEmotion.Neutral)
    {
        return new DialogLine
        {
            SpeakerType = SpeakerType.Companion,
            Text = text,
            Emotion = emotion
        };
    }

    /// <summary>
    /// Creates a system/narrator line.
    /// </summary>
    public static DialogLine System(string text)
    {
        return new DialogLine
        {
            SpeakerType = SpeakerType.System,
            Text = text
        };
    }

    /// <summary>
    /// Gets the formatted text with emotion tag for Stray speech.
    /// </summary>
    public string GetFormattedText()
    {
        if (Emotion == DialogEmotion.Neutral)
            return Text;

        var emotionTag = Emotion switch
        {
            DialogEmotion.Happy => "[happy]",
            DialogEmotion.Sad => "[sad]",
            DialogEmotion.Angry => "[angry]",
            DialogEmotion.Scared => "[scared]",
            DialogEmotion.Curious => "[curious]",
            DialogEmotion.Confused => "[confused]",
            DialogEmotion.Excited => "[excited]",
            DialogEmotion.Tired => "[tired]",
            DialogEmotion.Hopeful => "[hopeful]",
            DialogEmotion.Desperate => "[desperate]",
            DialogEmotion.Sarcastic => "[sarcastic]",
            DialogEmotion.Thoughtful => "[thoughtful]",
            _ => ""
        };

        return $"{emotionTag} {Text}";
    }
}
