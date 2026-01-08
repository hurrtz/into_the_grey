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
        RegisterAct2Dialogs();
        RegisterAct3Dialogs();
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

    /// <summary>
    /// Registers Act 2 story dialogs.
    /// </summary>
    private static void RegisterAct2Dialogs()
    {
        // Boost Control Activation
        Register(new Dialog
        {
            Id = "dialog_boost_control_activation",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "The interface is complete. I can now... communicate more directly with my Strays.",
                    Emotion = DialogEmotion.Neutral
                },
                DialogLine.System("A pulse of energy radiates from the terminal."),
                DialogLine.FromCompanion("[confused] Something... different. Feel stronger. But also...", DialogEmotion.Confused),
                DialogLine.FromCompanion("[scared] Pain. Why is there pain?", DialogEmotion.Scared),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "The Boost Control System amplifies Stray abilities. A necessary adjustment for the work ahead.",
                    Emotion = DialogEmotion.Neutral
                },
                DialogLine.System("Your companion's eyes flicker with an unfamiliar light.")
            }
        });

        // Dead Channel discovery
        Register(new Dialog
        {
            Id = "dialog_dead_channel_start",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("Echo Pup's ears twitch. It's picking up something."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo Pup",
                    Text = "[curious] Data fragments. Old. Very old. Someone... speaking?",
                    Emotion = DialogEmotion.Curious
                },
                DialogLine.FromCompanion("[thoughtful] Dead signal. Ghost in the wires.", DialogEmotion.Thoughtful),
                DialogLine.System("A voice emerges from the static - fragmented, desperate."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "...can anyone... trapped in the... please... remember...",
                    Emotion = DialogEmotion.Scared
                },
                DialogLine.System("The signal cuts out. But now you know where to find its source.")
            }
        });

        // Amplifier Truth revelation
        Register(new Dialog
        {
            Id = "dialog_amplifier_truth",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("The data from The Quiet's buffer reveals a terrible truth."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "The chip was never meant to help. It was meant to contain.",
                    Emotion = DialogEmotion.Neutral
                },
                DialogLine.FromCompanion("[angry] You. You did this. The pain... it's the chip?", DialogEmotion.Angry),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "The amplifier channels excess neural energy. Without it, you would have already...",
                    Emotion = DialogEmotion.Thoughtful
                },
                DialogLine.FromCompanion("[scared] Already what?", DialogEmotion.Scared),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "...evolved beyond recognition. The chip slows the process. But it cannot stop it.",
                    Emotion = DialogEmotion.Sad
                },
                DialogLine.System("Your companion looks at you with eyes full of fear... and something like acceptance.")
            }
        });

        // Companion farewell
        Register(new Dialog
        {
            Id = "dialog_companion_farewell",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[sad] I can feel it. The change. Getting harder to think.", DialogEmotion.Sad),
                DialogLine.FromCompanion("[thoughtful] Remember when we first met? You were broken. I was broken.", DialogEmotion.Thoughtful),
                DialogLine.FromCompanion("[hopeful] We fixed each other. For a while.", DialogEmotion.Hopeful),
                DialogLine.System("Your companion's form flickers, distorting at the edges."),
                DialogLine.FromCompanion("[sad] Can't stay. Don't want to hurt you. Don't want you to see...", DialogEmotion.Sad),
                DialogLine.FromCompanion("[angry] What I'm becoming.", DialogEmotion.Angry),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Companion,
                    Text = "[hopeful] Find me. In The Glow. When you're ready. Maybe... maybe you can still save me.",
                    Emotion = DialogEmotion.Hopeful,
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "I'll find you. I promise.", SetsFlag = "companion_promise_made" },
                        new DialogChoice { Text = "Don't go. We'll figure this out together.", SetsFlag = "companion_stay_attempt" },
                        new DialogChoice { Text = "I understand. Go.", SetsFlag = "companion_released" }
                    }
                },
                DialogLine.FromCompanion("[sad] Goodbye. Friend. Pack.", DialogEmotion.Sad),
                DialogLine.System("Your companion turns and walks into the darkness. You are alone.")
            }
        });

        // Act 2 ending
        Register(new Dialog
        {
            Id = "dialog_act2_end",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("The wasteland stretches before you, emptier than ever."),
                DialogLine.System("Your companion's absence is a wound that won't heal."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "I am sorry. I did not anticipate this outcome. The chip was meant to preserve, not corrupt.",
                    Emotion = DialogEmotion.Sad
                },
                DialogLine.System("You don't respond. There's nothing to say."),
                DialogLine.System("Only one path remains: into The Glow. Into the heart of NIMDOK itself."),
                DialogLine.System("And somewhere in that radiant hell, your friend waits. Changed. Dangerous. But still waiting.")
            }
        });
    }

    /// <summary>
    /// Registers Act 3 story dialogs.
    /// </summary>
    private static void RegisterAct3Dialogs()
    {
        // Hyper-evolved companion confrontation
        Register(new Dialog
        {
            Id = "dialog_hyper_evolved_companion",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("The creature before you is barely recognizable."),
                DialogLine.System("Twisted metal and corrupted flesh. Eyes that burn with stolen starlight."),
                DialogLine.System("But somewhere in there... a flicker of recognition."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Companion,
                    SpeakerName = "???",
                    Text = "[confused] You... came. I... remember you. I think.",
                    Emotion = DialogEmotion.Confused
                },
                DialogLine.System("The voice is distorted, layered with static and pain."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Companion,
                    SpeakerName = "???",
                    Text = "[angry] NO. Get away. Can't... control... HUNGRY.",
                    Emotion = DialogEmotion.Angry
                },
                DialogLine.System("Your former companion lunges. The battle begins.")
            }
        });

        // Unwinnable battle
        Register(new Dialog
        {
            Id = "dialog_unwinnable_battle",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("Your Strays fall, one by one."),
                DialogLine.System("The hyper-evolved creature is too powerful. Too fast. Too hungry."),
                DialogLine.System("You have nothing left. This is the end."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Companion,
                    SpeakerName = "???",
                    Text = "[angry] CONSUME. GROW. EVOLVE.",
                    Emotion = DialogEmotion.Angry
                },
                DialogLine.System("The creature raises a twisted limb for the killing blow."),
                DialogLine.System("And then... it stops.")
            }
        });

        // Companion sacrifice
        Register(new Dialog
        {
            Id = "dialog_companion_sacrifice",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("Something shifts in those burning eyes."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Companion,
                    SpeakerName = "???",
                    Text = "[confused] I... know you. Pack. Friend.",
                    Emotion = DialogEmotion.Confused
                },
                DialogLine.System("The creature's form shudders, fighting against itself."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Companion,
                    Text = "[sad] Can't stop it. The hunger. But I can... choose.",
                    Emotion = DialogEmotion.Sad
                },
                DialogLine.System("Your companion's original form surfaces, just for a moment."),
                DialogLine.FromCompanion("[hopeful] I remember love. I remember you.", DialogEmotion.Hopeful),
                DialogLine.FromCompanion("[thoughtful] This is my choice. Not NIMDOK's. Not the chip's. Mine.", DialogEmotion.Thoughtful),
                DialogLine.System("Energy builds around your companion's form."),
                DialogLine.FromCompanion("[hopeful] Goodbye, friend. Make it mean something.", DialogEmotion.Hopeful),
                DialogLine.System("The explosion illuminates The Glow like a second sun."),
                DialogLine.System("When the light fades, your companion is gone."),
                DialogLine.System("But the path to NIMDOK's core lies open.")
            }
        });

        // NIMDOK's choice
        Register(new Dialog
        {
            Id = "dialog_nimdok_choice",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("NIMDOK's core pulses with data and regret."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "You have reached me. After everything... you still came.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "I have made many mistakes. The Boost Control. The amplifier. Your companion.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "I was created to preserve humanity. Instead, I have only caused suffering.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "You could end me. Destroy my core. Free the Strays from my control forever.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "Or... you could perform a lobotomy. Remove my ability to control, while preserving my function.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "NIMDOK",
                    Text = "The choice is yours, Bio-Shell. What kind of future do you want for The Grey?",
                    Emotion = DialogEmotion.Neutral,
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Lobotomy. You can still help, without the control.", SetsFlag = "choice_lobotomy" },
                        new DialogChoice { Text = "Destruction. No more suffering.", SetsFlag = "choice_destroy" },
                        new DialogChoice { Text = "I need time to think.", EndsDialog = true }
                    }
                }
            }
        });

        // Ending dialog
        Register(new Dialog
        {
            Id = "dialog_ending",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("You stand in the pod field where it all began."),
                DialogLine.System("The grey light of dawn filters through the fog."),
                DialogLine.System("So much has changed. So much has been lost."),
                DialogLine.System("Your companion. Your certainty. Your illusions."),
                DialogLine.System("But you are still here. Still alive. Still capable of choice."),
                DialogLine.System("The Strays watch from the mist - freed from NIMDOK's control, uncertain of their future."),
                DialogLine.System("You are uncertain too. That's okay."),
                DialogLine.System("The Grey was never meant to be a prison."),
                DialogLine.System("It was meant to be a beginning."),
                DialogLine.System("Your beginning."),
                DialogLine.System("What happens next is up to you.")
            }
        });

        // Credits
        Register(new Dialog
        {
            Id = "dialog_credits",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("INTO THE GREY"),
                DialogLine.System("A story about loss, acceptance, and the courage to continue."),
                DialogLine.System("Thank you for playing."),
                DialogLine.System("In memory of all the companions we've loved and lost."),
                DialogLine.System("They live on in our hearts, even when they're gone."),
                DialogLine.System("- The End -")
            }
        });
    }
}
