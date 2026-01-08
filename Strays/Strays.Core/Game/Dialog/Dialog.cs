using System;
using System.Collections.Generic;
using System.Linq;

namespace Strays.Core.Game.Dialog;

/// <summary>
/// A sequence of dialog lines that form a conversation.
/// </summary>
public class Dialog
{
    /// <summary>
    /// Unique identifier for this dialog.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// The lines of dialog in sequence.
    /// </summary>
    public List<DialogLine> Lines { get; init; } = new();

    /// <summary>
    /// Flag required for this dialog to be available.
    /// </summary>
    public string? RequiresFlag { get; init; }

    /// <summary>
    /// Whether this dialog can only be played once.
    /// </summary>
    public bool OneTime { get; init; }

    /// <summary>
    /// Flag set when this dialog has been seen.
    /// </summary>
    public string? SeenFlag => OneTime ? $"dialog_seen_{Id}" : null;

    /// <summary>
    /// The number of lines in this dialog.
    /// </summary>
    public int LineCount => Lines.Count;

    /// <summary>
    /// Creates a simple dialog with sequential lines.
    /// </summary>
    public static Dialog Create(string id, params DialogLine[] lines)
    {
        return new Dialog
        {
            Id = id,
            Lines = lines.ToList()
        };
    }

    /// <summary>
    /// Creates a simple dialog from text strings (system speaker).
    /// </summary>
    public static Dialog FromText(string id, params string[] texts)
    {
        return new Dialog
        {
            Id = id,
            Lines = texts.Select(t => DialogLine.System(t)).ToList()
        };
    }
}

/// <summary>
/// Manages dialog playback state.
/// </summary>
public class DialogState
{
    private readonly Dialog _dialog;
    private int _currentLineIndex;
    private int _selectedChoiceIndex;

    /// <summary>
    /// The current dialog being played.
    /// </summary>
    public Dialog Dialog => _dialog;

    /// <summary>
    /// The current line being displayed.
    /// </summary>
    public DialogLine? CurrentLine =>
        _currentLineIndex < _dialog.Lines.Count ? _dialog.Lines[_currentLineIndex] : null;

    /// <summary>
    /// Index of the current line.
    /// </summary>
    public int CurrentLineIndex => _currentLineIndex;

    /// <summary>
    /// Whether we're at the last line.
    /// </summary>
    public bool IsLastLine => _currentLineIndex >= _dialog.Lines.Count - 1;

    /// <summary>
    /// Whether the dialog has ended.
    /// </summary>
    public bool IsComplete => _currentLineIndex >= _dialog.Lines.Count;

    /// <summary>
    /// Currently selected choice index.
    /// </summary>
    public int SelectedChoiceIndex
    {
        get => _selectedChoiceIndex;
        set
        {
            var choices = GetAvailableChoices();
            if (choices.Count > 0)
            {
                _selectedChoiceIndex = Math.Clamp(value, 0, choices.Count - 1);
            }
        }
    }

    /// <summary>
    /// The currently selected choice.
    /// </summary>
    public DialogChoice? SelectedChoice
    {
        get
        {
            var choices = GetAvailableChoices();
            return _selectedChoiceIndex < choices.Count ? choices[_selectedChoiceIndex] : null;
        }
    }

    /// <summary>
    /// Event fired when a line is displayed.
    /// </summary>
    public event EventHandler<DialogLine>? LineDisplayed;

    /// <summary>
    /// Event fired when a choice is selected.
    /// </summary>
    public event EventHandler<DialogChoice>? ChoiceSelected;

    /// <summary>
    /// Event fired when the dialog ends.
    /// </summary>
    public event EventHandler? DialogEnded;

    public DialogState(Dialog dialog)
    {
        _dialog = dialog;
        _currentLineIndex = 0;
        _selectedChoiceIndex = 0;

        if (CurrentLine != null)
        {
            LineDisplayed?.Invoke(this, CurrentLine);
        }
    }

    /// <summary>
    /// Gets available choices for the current line (filtered by flags).
    /// </summary>
    /// <param name="hasFlag">Function to check if a flag is set.</param>
    public List<DialogChoice> GetAvailableChoices(Func<string, bool>? hasFlag = null)
    {
        if (CurrentLine?.Choices == null)
            return new List<DialogChoice>();

        return CurrentLine.Choices
            .Where(c => string.IsNullOrEmpty(c.RequiresFlag) || (hasFlag?.Invoke(c.RequiresFlag) ?? true))
            .ToList();
    }

    /// <summary>
    /// Advances to the next line.
    /// </summary>
    /// <returns>True if there's another line, false if dialog ended.</returns>
    public bool Advance()
    {
        if (IsComplete)
            return false;

        // If current line has choices, require choice selection
        if (CurrentLine?.HasChoices == true)
            return false;

        _currentLineIndex++;
        _selectedChoiceIndex = 0;

        if (IsComplete)
        {
            DialogEnded?.Invoke(this, EventArgs.Empty);
            return false;
        }

        LineDisplayed?.Invoke(this, CurrentLine!);
        return true;
    }

    /// <summary>
    /// Selects the current choice and advances.
    /// </summary>
    /// <returns>The selected choice, or null if no choice was selected.</returns>
    public DialogChoice? SelectChoice()
    {
        var choice = SelectedChoice;
        if (choice == null)
            return null;

        ChoiceSelected?.Invoke(this, choice);

        if (choice.EndsDialog)
        {
            _currentLineIndex = _dialog.Lines.Count; // End dialog
            DialogEnded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _currentLineIndex++;
            _selectedChoiceIndex = 0;

            if (IsComplete)
            {
                DialogEnded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                LineDisplayed?.Invoke(this, CurrentLine!);
            }
        }

        return choice;
    }

    /// <summary>
    /// Moves choice selection up.
    /// </summary>
    public void PreviousChoice()
    {
        var choices = GetAvailableChoices();
        if (choices.Count > 0)
        {
            _selectedChoiceIndex = (_selectedChoiceIndex - 1 + choices.Count) % choices.Count;
        }
    }

    /// <summary>
    /// Moves choice selection down.
    /// </summary>
    public void NextChoice()
    {
        var choices = GetAvailableChoices();
        if (choices.Count > 0)
        {
            _selectedChoiceIndex = (_selectedChoiceIndex + 1) % choices.Count;
        }
    }
}

/// <summary>
/// Static registry of all dialogs in the game.
/// </summary>
public static class Dialogs
{
    private static readonly Dictionary<string, Dialog> _dialogs = new();

    /// <summary>
    /// All registered dialogs.
    /// </summary>
    public static IReadOnlyDictionary<string, Dialog> All => _dialogs;

    /// <summary>
    /// Gets a dialog by ID.
    /// </summary>
    public static Dialog? Get(string id) =>
        _dialogs.TryGetValue(id, out var dialog) ? dialog : null;

    /// <summary>
    /// Registers a dialog.
    /// </summary>
    public static void Register(Dialog dialog)
    {
        _dialogs[dialog.Id] = dialog;
    }

    static Dialogs()
    {
        // Register all game dialogs
        RegisterAct1Dialogs();
    }

    private static void RegisterAct1Dialogs()
    {
        // Awakening sequence
        Register(new Dialog
        {
            Id = "awakening_intro",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("The world fades into focus. Static. Pain."),
                DialogLine.System("You're lying in debris. Something is very wrong."),
                DialogLine.System("Your body won't respond properly. Everything feels... fragmented."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "???",
                    Text = "Bio-Shell #7749. Status: Compromised. Awakening protocol... irregular.",
                    Emotion = DialogEmotion.Neutral
                },
                DialogLine.System("A voice. Distant. Digital."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "Welcome to The Grey. I am NIMDOK. You are... unexpected.",
                    Emotion = DialogEmotion.Thoughtful
                }
            }
        });

        // Finding the exoskeleton
        Register(new Dialog
        {
            Id = "exoskeleton_discovery",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("Nearby, half-buried in rubble: a mechanical frame. An exoskeleton."),
                DialogLine.System("It pulses with faint power. Still functional."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "Mobility assistance detected. Integration recommended for survival.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    Text = "You reach for it...",
                    SetsFlag = "exoskeleton_found"
                }
            }
        });

        // Meeting Bandit (Dog companion)
        Register(new Dialog
        {
            Id = "meet_bandit",
            OneTime = true,
            RequiresFlag = "has_exoskeleton",
            Lines = new List<DialogLine>
            {
                DialogLine.System("A sound in the ruins. Movement."),
                DialogLine.System("A dog emerges from the shadows. No... not quite a dog."),
                DialogLine.System("Cybernetic enhancements glint beneath matted fur. One eye replaced with a crimson lens."),
                DialogLine.FromCompanion("[curious] Human? No. Not human. But... alive?", DialogEmotion.Curious),
                DialogLine.System("The creature tilts its head, studying you."),
                DialogLine.FromCompanion("[thoughtful] You are broken. I am... also broken. We match.", DialogEmotion.Thoughtful),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Companion,
                    Text = "[hopeful] Come. I know paths. Safe paths. Mostly.",
                    Emotion = DialogEmotion.Hopeful,
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Lead the way.", SetsFlag = "companion_trusted" },
                        new DialogChoice { Text = "What are you?", NextDialogId = "bandit_explain" },
                        new DialogChoice { Text = "I don't need help.", SetsFlag = "companion_rejected", EndsDialog = true }
                    }
                }
            }
        });

        // Bandit explaining itself
        Register(new Dialog
        {
            Id = "bandit_explain",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[thoughtful] What am I? Good question. Hard question.", DialogEmotion.Thoughtful),
                DialogLine.FromCompanion("Was dog once. Maybe. Memories are... scattered.", DialogEmotion.Sad),
                DialogLine.FromCompanion("[curious] NIMDOK calls us Strays. We are what remains.", DialogEmotion.Curious),
                DialogLine.FromCompanion("[hopeful] But I remember kindness. I remember pack.", DialogEmotion.Hopeful),
                DialogLine.FromCompanion("You could be pack. If you want.", DialogEmotion.Hopeful),
                DialogLine.System("The dog's tail wags hesitantly.")
            }
        });

        // First encounter with wild Stray
        Register(new Dialog
        {
            Id = "first_wild_stray",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("Something moves in the darkness ahead."),
                DialogLine.FromCompanion("[scared] Careful! Wild one. Territorial.", DialogEmotion.Scared),
                DialogLine.System("A creature emerges - twisted metal and flesh, crackling with static."),
                DialogLine.FromCompanion("[angry] It will attack. We must fight. Or run.", DialogEmotion.Angry),
                DialogLine.System("The wild Stray's eyes lock onto you. There's no avoiding this.")
            }
        });

        // NIMDOK explanation
        Register(new Dialog
        {
            Id = "nimdok_explain",
            OneTime = true,
            RequiresFlag = "reached_terminal",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "You seek understanding. A reasonable desire.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "I am NIMDOK. I was created to preserve humanity.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "The Archive contains their consciousness. The Sleepers. What remains of your species.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "You are a Bio-Shell. A vessel. Awakened prematurely. This was... not intended.",
                    Emotion = DialogEmotion.Confused
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "But perhaps it is fortunate. I am... degrading. I need assistance.",
                    Emotion = DialogEmotion.Sad
                }
            }
        });
    }
}
