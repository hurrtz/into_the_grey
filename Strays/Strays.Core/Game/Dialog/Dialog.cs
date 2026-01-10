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
                    SpeakerName = "Lazarus",
                    Text = "Welcome to The Grey. I am Lazarus. You are... unexpected.",
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
                    SpeakerName = "Lazarus",
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
                DialogLine.FromCompanion("[curious] Lazarus calls us Strays. We are what remains.", DialogEmotion.Curious),
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

        // Lazarus explanation
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
                    SpeakerName = "Lazarus",
                    Text = "You seek understanding. A reasonable desire.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "I am Lazarus. I was created to preserve humanity.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "The Archive contains their consciousness. The Sleepers. What remains of your species.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "You are a Bio-Shell. A vessel. Awakened prematurely. This was... not intended.",
                    Emotion = DialogEmotion.Confused
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
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
                    SpeakerName = "Lazarus",
                    Text = "The interface is complete. I can now... communicate more directly with my Strays.",
                    Emotion = DialogEmotion.Neutral
                },
                DialogLine.System("A pulse of energy radiates from the terminal."),
                DialogLine.FromCompanion("[confused] Something... different. Feel stronger. But also...", DialogEmotion.Confused),
                DialogLine.FromCompanion("[scared] Pain. Why is there pain?", DialogEmotion.Scared),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
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
                    SpeakerName = "Lazarus",
                    Text = "The chip was never meant to help. It was meant to contain.",
                    Emotion = DialogEmotion.Neutral
                },
                DialogLine.FromCompanion("[angry] You. You did this. The pain... it's the chip?", DialogEmotion.Angry),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "The amplifier channels excess neural energy. Without it, you would have already...",
                    Emotion = DialogEmotion.Thoughtful
                },
                DialogLine.FromCompanion("[scared] Already what?", DialogEmotion.Scared),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
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
                    SpeakerName = "Lazarus",
                    Text = "I am sorry. I did not anticipate this outcome. The chip was meant to preserve, not corrupt.",
                    Emotion = DialogEmotion.Sad
                },
                DialogLine.System("You don't respond. There's nothing to say."),
                DialogLine.System("Only one path remains: into The Glow. Into the heart of Lazarus itself."),
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
                DialogLine.FromCompanion("[thoughtful] This is my choice. Not Lazarus's. Not the chip's. Mine.", DialogEmotion.Thoughtful),
                DialogLine.System("Energy builds around your companion's form."),
                DialogLine.FromCompanion("[hopeful] Goodbye, friend. Make it mean something.", DialogEmotion.Hopeful),
                DialogLine.System("The explosion illuminates The Glow like a second sun."),
                DialogLine.System("When the light fades, your companion is gone."),
                DialogLine.System("But the path to Lazarus's core lies open.")
            }
        });

        // Lazarus's choice
        Register(new Dialog
        {
            Id = "dialog_nimdok_choice",
            OneTime = true,
            Lines = new List<DialogLine>
            {
                DialogLine.System("Lazarus's core pulses with data and regret."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "You have reached me. After everything... you still came.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "I have made many mistakes. The Boost Control. The amplifier. Your companion.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "I was created to preserve humanity. Instead, I have only caused suffering.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "You could end me. Destroy my core. Free the Strays from my control forever.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "Or... you could perform a lobotomy. Remove my ability to control, while preserving my function.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
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
                DialogLine.System("The Strays watch from the mist - freed from Lazarus's control, uncertain of their future."),
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

    /// <summary>
    /// Registers NPC greeting and general dialogs.
    /// </summary>
    private static void RegisterNPCDialogs()
    {
        // === MERCHANTS ===

        // Rust - Salvager trader at Fringe Camp
        Register(new Dialog
        {
            Id = "rusty_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("trader_rust", "Rust", "Another survivor, eh? You look like you could use some gear."),
                DialogLine.FromNpc("trader_rust", "Rust", "Name's Rust. I trade in salvage - bits and pieces from the old world."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Rust",
                    Text = "Take a look at what I've got. Fair prices, mostly.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Show me what you have.", SetsFlag = "shop_opened" },
                        new DialogChoice { Text = "What can you tell me about The Fringe?", NextDialogId = "rusty_fringe_info" },
                        new DialogChoice { Text = "Maybe later.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "rusty_fringe_info",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("trader_rust", "Rust", "The Fringe? Edge of the wasteland. Safest place around, if you can call anything safe."),
                DialogLine.FromNpc("trader_rust", "Rust", "Wild Strays wander through sometimes. Mostly harmless if you don't provoke 'em."),
                DialogLine.FromNpc("trader_rust", "Rust", "Head deeper and you hit the Rust Belt. That's where the real salvage is. And the real danger."),
                DialogLine.FromNpc("trader_rust", "Rust", "Now, you buying or just chatting?")
            }
        });

        // Volt - Machinist at Rust Haven
        Register(new Dialog
        {
            Id = "volt_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("machinist_volt", "Volt", "Ooh, fresh meat! And look at that exoskeleton - pre-Collapse tech!"),
                DialogLine.FromNpc("machinist_volt", "Volt", "I'm Volt. I make things work. Sometimes I make them work BETTER."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Volt",
                    Text = "Interested in upgrades? Modifications? I've got augments that'll make your Strays sing!",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "What do you have?", SetsFlag = "shop_opened" },
                        new DialogChoice { Text = "Tell me about the Machinists.", NextDialogId = "volt_machinist_info" },
                        new DialogChoice { Text = "What do you know about Lazarus?", NextDialogId = "volt_nimdok_info" },
                        new DialogChoice { Text = "Not right now.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "volt_machinist_info",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("machinist_volt", "Volt", "The Machinists? We believe in progress! Technology got us into this mess, but it'll get us out."),
                DialogLine.FromNpc("machinist_volt", "Volt", "Unlike those Shepherd types who want to 'live in harmony with nature' or whatever."),
                DialogLine.FromNpc("machinist_volt", "Volt", "Nature's dead. The future is chrome and circuits, friend!"),
                DialogLine.FromNpc("machinist_volt", "Volt", "Help us out, and maybe we share some of our best toys with you.")
            }
        });

        Register(new Dialog
        {
            Id = "volt_nimdok_info",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("machinist_volt", "Volt", "Lazarus? Big AI in the sky. Or underground. Or everywhere. Hard to say."),
                DialogLine.FromNpc("machinist_volt", "Volt", "Some say it created the Strays. Others say it just... collects them."),
                DialogLine.FromNpc("machinist_volt", "Volt", "The Machinists think it's the key to everything. If we can tap into its systems..."),
                DialogLine.FromNpc("machinist_volt", "Volt", "Well. Let's just say we'd have access to some VERY interesting tech.")
            }
        });

        // Willow - Shepherd merchant at Green Sanctuary
        Register(new Dialog
        {
            Id = "willow_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_trader", "Willow", "Welcome, traveler. You've come a long way to reach the Sanctuary."),
                DialogLine.FromNpc("shepherd_trader", "Willow", "I'm Willow. I tend to the sick and wounded - Stray and human alike."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Willow",
                    Text = "If your companions need healing or supplies, I can help.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "I need supplies.", SetsFlag = "shop_opened" },
                        new DialogChoice { Text = "Can you heal my Strays?", NextDialogId = "willow_heal" },
                        new DialogChoice { Text = "Tell me about the Shepherds.", NextDialogId = "willow_shepherd_info" },
                        new DialogChoice { Text = "Just passing through.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "willow_heal",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_trader", "Willow", "Bring them here. Let me see what I can do."),
                DialogLine.System("Willow examines your Strays with gentle hands."),
                DialogLine.FromNpc("shepherd_trader", "Willow", "Some wounds of the body, some of the spirit. Both can be mended."),
                DialogLine.System("Your Strays have been fully healed.")
            }
        });

        Register(new Dialog
        {
            Id = "willow_shepherd_info",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_trader", "Willow", "The Shepherds believe in balance. The Strays are not tools or weapons."),
                DialogLine.FromNpc("shepherd_trader", "Willow", "They are beings, deserving of care and respect."),
                DialogLine.FromNpc("shepherd_trader", "Willow", "We don't force evolution or stuff them with augments like the Machinists."),
                DialogLine.FromNpc("shepherd_trader", "Willow", "We guide them. Protect them. And in return, they protect us."),
                DialogLine.FromNpc("shepherd_trader", "Willow", "Elder Moss can tell you more, if you wish to learn.")
            }
        });

        // === HEALERS ===

        // Sara - Healer at Fringe Camp
        Register(new Dialog
        {
            Id = "sara_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("healer_sara", "Sara", "Oh! Another wounded soul finds their way here."),
                DialogLine.FromNpc("healer_sara", "Sara", "I'm Sara. I do what I can to patch up the broken things in this world."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Sara",
                    Text = "Your Strays look tired. Would you like me to tend to them?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Please heal them.", NextDialogId = "sara_heal" },
                        new DialogChoice { Text = "How did you become a healer?", NextDialogId = "sara_backstory" },
                        new DialogChoice { Text = "We're fine, thanks.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "sara_heal",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("healer_sara", "Sara", "Come, little ones. Let me help."),
                DialogLine.System("Sara hums softly as she works. Your Strays visibly relax."),
                DialogLine.FromNpc("healer_sara", "Sara", "There. Better now. The world is hard enough without carrying pain."),
                DialogLine.System("Your Strays have been fully healed.")
            }
        });

        Register(new Dialog
        {
            Id = "sara_backstory",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("healer_sara", "Sara", "Before The Grey, I was a veterinarian. Funny, isn't it?"),
                DialogLine.FromNpc("healer_sara", "Sara", "I spent my life helping animals. Now there's no line between animal and machine."),
                DialogLine.FromNpc("healer_sara", "Sara", "But healing is healing. Pain doesn't care what form you take."),
                DialogLine.FromNpc("healer_sara", "Sara", "I do what I've always done. I help.")
            }
        });

        // Generic healer service dialog (used as fallback)
        Register(new Dialog
        {
            Id = "healer_service",
            Lines = new List<DialogLine>
            {
                DialogLine.System("The healer tends to your wounds."),
                DialogLine.System("Your party has been fully restored!")
            }
        });

        // === QUEST GIVERS ===

        // Lazarus Terminal
        Register(new Dialog
        {
            Id = "nimdok_terminal_greeting",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus Terminal",
                    Text = "Bio-Shell #7749 detected. Status: Active. Compliance: Unknown.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus Terminal",
                    Text = "This terminal provides limited access to Lazarus's systems.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus Terminal",
                    Text = "State your request.",
                    Emotion = DialogEmotion.Neutral,
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "What is my purpose?", NextDialogId = "nimdok_purpose" },
                        new DialogChoice { Text = "Where should I go?", NextDialogId = "nimdok_guidance" },
                        new DialogChoice { Text = "What are the Strays?", NextDialogId = "nimdok_strays_explain" },
                        new DialogChoice { Text = "I have nothing to ask.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "nimdok_purpose",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "Purpose: Undefined. Bio-Shells were designed to house uploaded consciousness.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "Your shell activated prematurely. No consciousness was transferred.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "You are... empty. Yet you think. You feel. This is not expected.",
                    Emotion = DialogEmotion.Confused
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "Perhaps your purpose is yours to define.",
                    Emotion = DialogEmotion.Thoughtful
                }
            }
        });

        Register(new Dialog
        {
            Id = "nimdok_guidance",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "The Fringe is safest for new arrivals. Beyond lies the Rust Belt.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "Further still: The Green Zone, The Quiet, The Teeth.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "And at the center: The Glow. My core systems. Where all paths end.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "When you are ready, come to me. We have much to discuss.",
                    Emotion = DialogEmotion.Hopeful
                }
            }
        });

        Register(new Dialog
        {
            Id = "nimdok_strays_explain",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "The Strays are what remains of Earth's fauna.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "When the Collapse came, I preserved what I could. Animals, primarily.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "But preservation required... modification. Enhancement. They evolved.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "Now they are something new. Not quite animal. Not quite machine. Strays.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "They can communicate. They can think. They can feel. Perhaps too much.",
                    Emotion = DialogEmotion.Sad
                }
            }
        });

        // Echo Guide - friendly Stray NPC
        Register(new Dialog
        {
            Id = "echo_greeting",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[curious] New friend? New sounds? Let me listen...",
                    Emotion = DialogEmotion.Curious
                },
                DialogLine.System("The small creature's ears twitch, scanning for something only it can hear."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[happy] Yes! You carry good frequencies. We can be friends!",
                    Emotion = DialogEmotion.Happy
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[curious] What do you seek? Echo knows many paths.",
                    Emotion = DialogEmotion.Curious,
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Tell me about this place.", NextDialogId = "echo_place_info" },
                        new DialogChoice { Text = "What are you?", NextDialogId = "echo_self_explain" },
                        new DialogChoice { Text = "Just exploring.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "echo_friendly",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[happy] Friend returns! Good frequencies today?",
                    Emotion = DialogEmotion.Happy
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[curious] Echo heard interesting things. Want to listen?",
                    Emotion = DialogEmotion.Curious
                }
            }
        });

        Register(new Dialog
        {
            Id = "echo_place_info",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[thoughtful] The Fringe is edge-place. Between safe and not-safe.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[scared] Deep places have stronger Strays. Angry. Hungry.",
                    Emotion = DialogEmotion.Scared
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[hopeful] But also hidden things. Secret sounds. Treasures for the brave.",
                    Emotion = DialogEmotion.Hopeful
                }
            }
        });

        Register(new Dialog
        {
            Id = "echo_self_explain",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[thoughtful] Echo is... listener. Finder of lost sounds.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[sad] Was small creature once. Lazarus made Echo bigger. Changed.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.PartyStray,
                    SpeakerName = "Echo",
                    Text = "[hopeful] But Echo remembers kindness. Helps others remember too.",
                    Emotion = DialogEmotion.Hopeful
                }
            }
        });

        // Elder Moss - Shepherd Leader
        Register(new Dialog
        {
            Id = "moss_introduction",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "You have traveled far to reach the Sanctuary, wanderer."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "I am Moss, elder of the Shepherds. We have watched your journey."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Your companion... they care deeply for you. We can see it."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Elder Moss",
                    Text = "The bond between human and Stray is sacred. Protect it. Nurture it.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "How can I protect them?", NextDialogId = "moss_protection" },
                        new DialogChoice { Text = "What threatens this bond?", NextDialogId = "moss_threat" },
                        new DialogChoice { Text = "I'll remember.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "moss_protection",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Protection is not about strength. It is about understanding."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Listen to your Strays. They speak, though not always in words."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Their emotions, their fears, their joys - attend to them."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "A Stray who is heard is a Stray who will stand with you through anything.")
            }
        });

        Register(new Dialog
        {
            Id = "moss_threat",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Lazarus threatens everything. It sees Strays as tools. Resources."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Its Boost Control System... it accelerates evolution but at terrible cost."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "The bond between human and Stray can be corrupted. Twisted."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Be wary of those who promise power. It always has a price.")
            }
        });

        // Wandering Scavenger
        Register(new Dialog
        {
            Id = "scav_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("wanderer_scav", "Scav", "Heh. Another lost soul wandering The Grey."),
                DialogLine.FromNpc("wanderer_scav", "Scav", "Name's Scav. Used to be part of the Salvagers, but now I work alone."),
                DialogLine.FromNpc("wanderer_scav", "Scav", "Seen a lot out here. Things that'd make your circuits fry."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Scav",
                    Text = "You look like you could use some advice. Want to hear it?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Sure, what do you know?", NextDialogId = "scav_advice" },
                        new DialogChoice { Text = "Why did you leave the Salvagers?", NextDialogId = "scav_backstory" },
                        new DialogChoice { Text = "No thanks.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "scav_advice",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("wanderer_scav", "Scav", "First: never trust the quiet places. If it's too quiet, something's hunting."),
                DialogLine.FromNpc("wanderer_scav", "Scav", "Second: your Strays know more than they let on. Listen to 'em."),
                DialogLine.FromNpc("wanderer_scav", "Scav", "Third: Lazarus ain't your friend, but it ain't entirely your enemy either."),
                DialogLine.FromNpc("wanderer_scav", "Scav", "It's complicated. Like everything in The Grey.")
            }
        });

        Register(new Dialog
        {
            Id = "scav_backstory",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("wanderer_scav", "Scav", "Salvagers got too organized. Too political. I'm not a politics person."),
                DialogLine.FromNpc("wanderer_scav", "Scav", "They started fighting with the Machinists over territory. Stupid turf wars."),
                DialogLine.FromNpc("wanderer_scav", "Scav", "Meanwhile, the real prizes are out in The Grey, waiting to be found."),
                DialogLine.FromNpc("wanderer_scav", "Scav", "I'd rather risk the wilds than sit in meetings all day.")
            }
        });

        // Ghost Signal - Dead Channel mystery NPC
        Register(new Dialog
        {
            Id = "ghost_intro",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Static fills the air. A voice emerges, fragmented and desperate."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "...you can hear me? Real... you're really there?",
                    Emotion = DialogEmotion.Desperate
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "Trapped... the signal... Lazarus keeps us here... the Dead Channel...",
                    Emotion = DialogEmotion.Scared
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "Please... find the relay... set us free...",
                    Emotion = DialogEmotion.Desperate,
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Who are you?", NextDialogId = "ghost_identity" },
                        new DialogChoice { Text = "Where is the relay?", NextDialogId = "ghost_relay_info" },
                        new DialogChoice { Text = "I'll help you.", SetsFlag = "dead_channel_quest_started" }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "ghost_identity",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "I... was human once. We all were.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "Lazarus uploaded us. Said it was preservation. Safety.",
                    Emotion = DialogEmotion.Angry
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "But something went wrong. We're trapped in the signal. Can't move on.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "Neither alive nor dead. Just... echoing. Forever.",
                    Emotion = DialogEmotion.Desperate
                }
            }
        });

        Register(new Dialog
        {
            Id = "ghost_relay_info",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "The relay... in The Quiet. Where the signal is strongest.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "There's a buffer there. Holds our data. Keeps us looping.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "Destroy it... or release it... either way, we'll be free.",
                    Emotion = DialogEmotion.Hopeful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "Please... hurry... signal fading... can't hold...",
                    Emotion = DialogEmotion.Desperate
                }
            }
        });
    }

    /// <summary>
    /// Registers faction-specific dialogs.
    /// </summary>
    private static void RegisterFactionDialogs()
    {
        // Salvagers faction dialog
        Register(new Dialog
        {
            Id = "faction_salvagers_intro",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "So you want to run with the Salvagers, eh?"),
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "We're the backbone of The Grey. Without us, everyone starves."),
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "We find things. Fix things. Trade things. Simple work for simple pay."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Captain Crank",
                    Text = "Help us out, we'll make sure you get a cut. Fair deal?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "I'm in.", SetsFlag = "salvagers_allied", NextDialogId = "faction_salvagers_joined" },
                        new DialogChoice { Text = "What kind of work?", NextDialogId = "faction_salvagers_work" },
                        new DialogChoice { Text = "Not interested.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "faction_salvagers_joined",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "Smart choice. Welcome to the crew."),
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "First job's simple: there's a cache in the Rust Belt. Retrieve it."),
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "Watch out for Machinist patrols. They've been sniffing around our territory."),
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "Bring back the goods, and you'll earn your first share.")
            }
        });

        Register(new Dialog
        {
            Id = "faction_salvagers_work",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "Scouting, mostly. Finding pre-Collapse tech in the ruins."),
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "Sometimes retrieval. Sometimes... persuading people to share."),
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "We don't kill unless we have to. Bad for business. Dead customers don't buy."),
                DialogLine.FromNpc("salvager_captain", "Captain Crank", "But we're not soft either. You with us or against us?")
            }
        });

        // Machinists faction dialog
        Register(new Dialog
        {
            Id = "faction_machinists_intro",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "You've found us. Not many outsiders do."),
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "The Machinists believe in progress. Evolution through technology."),
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "The Strays are the future, but only if we guide that evolution."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Chief Conductor",
                    Text = "We could use someone with your... unique perspective. Interested?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Tell me more.", NextDialogId = "faction_machinists_philosophy" },
                        new DialogChoice { Text = "What's in it for me?", NextDialogId = "faction_machinists_benefits" },
                        new DialogChoice { Text = "I'll pass.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "faction_machinists_philosophy",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "The old world died because humanity couldn't evolve fast enough."),
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "But now? Lazarus has shown us the path. Augmentation. Enhancement."),
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "The Shepherds cling to 'natural' evolution. They'll be left behind."),
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "We embrace change. We become more than we were. That is the future.")
            }
        });

        Register(new Dialog
        {
            Id = "faction_machinists_benefits",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "Access to the best augmentations. Microchips you won't find anywhere else."),
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "We have workshops. Labs. The ability to push your Strays further than nature intended."),
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "And when we finally crack Lazarus's core systems... we'll share the spoils."),
                DialogLine.FromNpc("machinist_leader", "Chief Conductor", "Imagine it. Unlimited power. Unlimited potential.")
            }
        });

        // Shepherds faction dialog
        Register(new Dialog
        {
            Id = "faction_shepherds_intro",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "The Sanctuary welcomes all who come in peace."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "The Shepherds have watched over Strays since the beginning."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "We do not force evolution. We do not cage. We guide and protect."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Elder Moss",
                    Text = "If you seek understanding rather than power, you are welcome among us.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "I want to learn.", SetsFlag = "shepherds_allied", NextDialogId = "faction_shepherds_teaching" },
                        new DialogChoice { Text = "What do you know about the bond?", NextDialogId = "faction_shepherds_bond" },
                        new DialogChoice { Text = "Perhaps another time.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "faction_shepherds_teaching",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Then you have taken the first step on a long path."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Sit with your Strays. Listen to their silence."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Feel what they feel. Fear what they fear. Love what they love."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Only then will you understand what it means to be bonded.")
            }
        });

        Register(new Dialog
        {
            Id = "faction_shepherds_bond",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "The bond is ancient. Older than Lazarus. Older than The Grey."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Humans and animals have always shared this world. We evolved together."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "The Strays remember. Somewhere in their code, in their hearts."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "When you bond with a Stray, you touch something primal. Beautiful."),
                DialogLine.FromNpc("shepherd_elder", "Elder Moss", "Do not let anyone tell you it is merely utility. It is sacred.")
            }
        });
    }

    /// <summary>
    /// Registers side quest dialogs.
    /// </summary>
    private static void RegisterSideQuestDialogs()
    {
        // Side Quest: Lost Signal
        Register(new Dialog
        {
            Id = "quest_lost_signal_start",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("signal_hunter", "Freq", "Hey! You look like someone who can handle themselves."),
                DialogLine.FromNpc("signal_hunter", "Freq", "I've been tracking a signal. Old-world tech, pre-Collapse."),
                DialogLine.FromNpc("signal_hunter", "Freq", "Problem is, it's deep in wild Stray territory."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Freq",
                    Text = "Help me retrieve it, and I'll split the salvage with you.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Count me in.", SetsFlag = "quest_lost_signal_active", StartsQuest = "side_lost_signal" },
                        new DialogChoice { Text = "What kind of tech?", NextDialogId = "quest_lost_signal_details" },
                        new DialogChoice { Text = "Not my problem.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "quest_lost_signal_details",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("signal_hunter", "Freq", "Military beacon. Still broadcasting after all these years."),
                DialogLine.FromNpc("signal_hunter", "Freq", "Could be weapons. Could be supplies. Could be nothing."),
                DialogLine.FromNpc("signal_hunter", "Freq", "But the signal's strong. That means power. Working tech."),
                DialogLine.FromNpc("signal_hunter", "Freq", "In The Grey, working tech is worth more than gold.")
            }
        });

        Register(new Dialog
        {
            Id = "quest_lost_signal_complete",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("signal_hunter", "Freq", "You found it! I knew you were the right choice."),
                DialogLine.FromNpc("signal_hunter", "Freq", "Let's see... military rations, medical supplies, and..."),
                DialogLine.FromNpc("signal_hunter", "Freq", "Holy circuit, is that a nav chip? Pre-Collapse navigation!"),
                DialogLine.FromNpc("signal_hunter", "Freq", "Here, take your cut. You earned it. And if you find more signals..."),
                DialogLine.FromNpc("signal_hunter", "Freq", "You know where to find me.")
            }
        });

        // Side Quest: Wounded Stray
        Register(new Dialog
        {
            Id = "quest_wounded_stray_start",
            Lines = new List<DialogLine>
            {
                DialogLine.System("A weak signal emanates from the nearby ruins."),
                DialogLine.System("Following it, you find a wounded Stray, trapped under debris."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "???",
                    Text = "[scared] Please... help... not enemy... just lost...",
                    Emotion = DialogEmotion.Scared
                },
                DialogLine.FromCompanion("[hopeful] We should help. Not all wild ones are bad.", DialogEmotion.Hopeful),
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    Text = "The debris is heavy but movable. What do you do?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Help the Stray.", SetsFlag = "wounded_stray_helped", NextDialogId = "quest_wounded_stray_help" },
                        new DialogChoice { Text = "It could be a trap. Leave it.", SetsFlag = "wounded_stray_abandoned", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "quest_wounded_stray_help",
            Lines = new List<DialogLine>
            {
                DialogLine.System("You shift the debris, freeing the trapped creature."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "???",
                    Text = "[happy] Free! Thank you! You... kind. Different.",
                    Emotion = DialogEmotion.Happy
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "???",
                    Text = "[thoughtful] I am Glitch. Was pack once. Pack gone now.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Glitch",
                    Text = "[hopeful] You have pack. Good pack. Could I... maybe... come with?",
                    Emotion = DialogEmotion.Hopeful,
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Welcome to the pack, Glitch.", SetsFlag = "glitch_recruited" },
                        new DialogChoice { Text = "You can travel with us for now.", SetsFlag = "glitch_temporary" },
                        new DialogChoice { Text = "You're free now. Good luck.", EndsDialog = true }
                    }
                }
            }
        });

        // Side Quest: Memory Fragment
        Register(new Dialog
        {
            Id = "quest_memory_fragment_find",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Something glints in the rubble. A data chip, still intact."),
                DialogLine.System("Your exoskeleton interfaces with it automatically."),
                DialogLine.System("Images flood your mind - a family, a home, a world that no longer exists."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory",
                    Text = "\"I hope whoever finds this knows: we loved each other. That's what mattered.\"",
                    Emotion = DialogEmotion.Sad
                },
                DialogLine.FromCompanion("[sad] Old-human memory. They had families too.", DialogEmotion.Sad),
                DialogLine.System("You carefully store the chip. Maybe someone will want it back someday.")
            }
        });

        // Side Quest: Machinist Deserter
        Register(new Dialog
        {
            Id = "quest_deserter_start",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("deserter", "Spark", "Shh! Keep your voice down!"),
                DialogLine.FromNpc("deserter", "Spark", "I used to be a Machinist. Not anymore. Couldn't stomach what they were doing."),
                DialogLine.FromNpc("deserter", "Spark", "The experiments... the forced evolutions... it's wrong."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Spark",
                    Text = "They're looking for me. If they find me, I'm dead. Can you help me escape?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "I'll help you get out.", SetsFlag = "quest_deserter_active", StartsQuest = "side_deserter" },
                        new DialogChoice { Text = "What did they do?", NextDialogId = "quest_deserter_explain" },
                        new DialogChoice { Text = "Not my business.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "quest_deserter_explain",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("deserter", "Spark", "They capture wild Strays. Force-evolve them with experimental chips."),
                DialogLine.FromNpc("deserter", "Spark", "Most don't survive. The ones that do... they're not themselves anymore."),
                DialogLine.FromNpc("deserter", "Spark", "Just weapons. Tools. No personality, no memory. Just obedience."),
                DialogLine.FromNpc("deserter", "Spark", "I couldn't be part of that. So I ran.")
            }
        });

        Register(new Dialog
        {
            Id = "quest_deserter_complete",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("deserter", "Spark", "We made it. I can't believe we actually made it."),
                DialogLine.FromNpc("deserter", "Spark", "Thank you. You risked everything for a stranger."),
                DialogLine.FromNpc("deserter", "Spark", "Here, take this. Stole it from the labs. High-grade microchip."),
                DialogLine.FromNpc("deserter", "Spark", "Maybe it'll help your Strays. Used the right way, not... their way."),
                DialogLine.FromNpc("deserter", "Spark", "I'm heading to the Shepherds. They'll take me in. Stay safe out there.")
            }
        });
    }

    /// <summary>
    /// Registers tutorial and help dialogs.
    /// </summary>
    private static void RegisterTutorialDialogs()
    {
        // Combat tutorial
        Register(new Dialog
        {
            Id = "tutorial_combat",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[excited] Enemy! Fight time!", DialogEmotion.Excited),
                DialogLine.System("Combat begins! Each Stray has an ATB bar that fills over time."),
                DialogLine.System("When a bar fills, that Stray can act."),
                DialogLine.System("Choose ATTACK to deal damage, DEFEND to reduce incoming damage."),
                DialogLine.System("Use ABILITIES for special moves - but they cost energy."),
                DialogLine.System("FLEE to escape, but fleeing doesn't always work."),
                DialogLine.FromCompanion("[hopeful] We can do this! Together!", DialogEmotion.Hopeful)
            }
        });

        // Recruitment tutorial
        Register(new Dialog
        {
            Id = "tutorial_recruitment",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[curious] That one... not attacking anymore. Watching.", DialogEmotion.Curious),
                DialogLine.System("Some defeated Strays can be recruited to your party."),
                DialogLine.System("Each Stray has different recruitment conditions."),
                DialogLine.System("Some want strength. Some want kindness. Some just want to not be alone."),
                DialogLine.FromCompanion("[hopeful] More friends? More pack? I like this idea.", DialogEmotion.Hopeful)
            }
        });

        // Evolution tutorial
        Register(new Dialog
        {
            Id = "tutorial_evolution",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Your Stray is changing!"),
                DialogLine.System("Strays evolve when they reach certain thresholds."),
                DialogLine.System("Level, stress, and augmentation all contribute to evolution."),
                DialogLine.System("Evolved Strays are stronger, but may lose some of their original personality."),
                DialogLine.FromCompanion("[scared] Change is scary. But also... exciting?", DialogEmotion.Scared)
            }
        });

        // Augmentation tutorial
        Register(new Dialog
        {
            Id = "tutorial_augmentation",
            Lines = new List<DialogLine>
            {
                DialogLine.System("You found an augmentation!"),
                DialogLine.System("Augmentations are cybernetic parts that enhance your Strays."),
                DialogLine.System("Each Stray has slots: Head, Torso, Limbs, Tail/Wings."),
                DialogLine.System("Different augments provide different bonuses."),
                DialogLine.System("But be careful - too much augmentation can push a Stray toward evolution."),
                DialogLine.FromCompanion("[thoughtful] Metal parts. Strange but useful.", DialogEmotion.Thoughtful)
            }
        });

        // Microchip tutorial
        Register(new Dialog
        {
            Id = "tutorial_microchip",
            Lines = new List<DialogLine>
            {
                DialogLine.System("You found a microchip!"),
                DialogLine.System("Microchips grant new abilities to your Strays."),
                DialogLine.System("Each Stray can only hold a limited number of chips."),
                DialogLine.System("Choose wisely - the right abilities can turn the tide of battle."),
                DialogLine.FromCompanion("[curious] Little brain helpers. Make us smarter.", DialogEmotion.Curious)
            }
        });

        // Settlement tutorial
        Register(new Dialog
        {
            Id = "tutorial_settlement",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[happy] Safe place! Finally rest.", DialogEmotion.Happy),
                DialogLine.System("You've found a settlement - a safe zone in The Grey."),
                DialogLine.System("Settlements have merchants, healers, and quest givers."),
                DialogLine.System("Your Strays won't be attacked while you're in a settlement."),
                DialogLine.System("Talk to the inhabitants to learn more about The Grey.")
            }
        });

        // Gravitation tutorial
        Register(new Dialog
        {
            Id = "tutorial_gravitation",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[excited] I feel it! The power!", DialogEmotion.Excited),
                DialogLine.System("Your companion has used Gravitation - a devastating ability."),
                DialogLine.System("Gravitation deals massive damage to all enemies."),
                DialogLine.System("But using it has consequences. Watch your companion's behavior."),
                DialogLine.FromCompanion("[confused] Strange. Feel different now. Stronger but...", DialogEmotion.Confused),
                DialogLine.System("Use Gravitation wisely. The more it's used, the more it changes your companion.")
            }
        });

        // Save/Load tutorial
        Register(new Dialog
        {
            Id = "tutorial_save",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Your progress has been auto-saved."),
                DialogLine.System("The game auto-saves periodically and at key moments."),
                DialogLine.System("You can also manually save from the pause menu."),
                DialogLine.System("Three save slots are available, plus the auto-save.")
            }
        });

        // Biome tutorial
        Register(new Dialog
        {
            Id = "tutorial_biome_change",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[curious] Air feels different here. Smells different.", DialogEmotion.Curious),
                DialogLine.System("You've entered a new biome."),
                DialogLine.System("Each biome has unique Strays, dangers, and secrets."),
                DialogLine.System("Strays native to a biome are stronger when fighting there."),
                DialogLine.FromCompanion("[thoughtful] Should be careful. New territory.", DialogEmotion.Thoughtful)
            }
        });
    }

    /// <summary>
    /// Registers companion ambient dialogs (barks).
    /// </summary>
    private static void RegisterCompanionBarks()
    {
        // Exploration barks
        Register(new Dialog
        {
            Id = "bark_exploring_1",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[curious] Something interesting over there. Smell it.", DialogEmotion.Curious)
            }
        });

        Register(new Dialog
        {
            Id = "bark_exploring_2",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[happy] Good walking today. Good company.", DialogEmotion.Happy)
            }
        });

        Register(new Dialog
        {
            Id = "bark_exploring_3",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[thoughtful] Remember when we first met? Feels long ago.", DialogEmotion.Thoughtful)
            }
        });

        // Combat barks - victory
        Register(new Dialog
        {
            Id = "bark_victory_1",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[excited] We win! We strong!", DialogEmotion.Excited)
            }
        });

        Register(new Dialog
        {
            Id = "bark_victory_2",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[happy] Good fight. Pack protects.", DialogEmotion.Happy)
            }
        });

        Register(new Dialog
        {
            Id = "bark_victory_3",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[thoughtful] They were scared too. All scared in The Grey.", DialogEmotion.Thoughtful)
            }
        });

        // Combat barks - low health
        Register(new Dialog
        {
            Id = "bark_low_health_1",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Hurting. Need rest.", DialogEmotion.Scared)
            }
        });

        Register(new Dialog
        {
            Id = "bark_low_health_2",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[angry] Not giving up! Not yet!", DialogEmotion.Angry)
            }
        });

        // Idle barks
        Register(new Dialog
        {
            Id = "bark_idle_1",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[tired] Long journey. Rest soon?", DialogEmotion.Tired)
            }
        });

        Register(new Dialog
        {
            Id = "bark_idle_2",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[curious] What thinking about?", DialogEmotion.Curious)
            }
        });

        Register(new Dialog
        {
            Id = "bark_idle_3",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[happy] Like this. Together. Quiet.", DialogEmotion.Happy)
            }
        });

        // Weather barks
        Register(new Dialog
        {
            Id = "bark_weather_rain",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[sad] Water falling. Don't like wet.", DialogEmotion.Sad)
            }
        });

        Register(new Dialog
        {
            Id = "bark_weather_storm",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Bad storm. Should find shelter.", DialogEmotion.Scared)
            }
        });

        Register(new Dialog
        {
            Id = "bark_weather_clear",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[happy] Nice day. Warm. Good for walking.", DialogEmotion.Happy)
            }
        });

        // New party member barks
        Register(new Dialog
        {
            Id = "bark_new_stray_1",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[curious] New friend? Must sniff. Must know.", DialogEmotion.Curious)
            }
        });

        Register(new Dialog
        {
            Id = "bark_new_stray_2",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[happy] Pack grows! More strength. More love.", DialogEmotion.Happy)
            }
        });

        // Approaching danger
        Register(new Dialog
        {
            Id = "bark_danger_ahead",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Bad feeling. Danger nearby. Be careful.", DialogEmotion.Scared)
            }
        });

        // Settlement arrival
        Register(new Dialog
        {
            Id = "bark_settlement_arrive",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[happy] Safe place! Can relax. Rest paws.", DialogEmotion.Happy)
            }
        });

        // Leaving settlement
        Register(new Dialog
        {
            Id = "bark_settlement_leave",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[thoughtful] Back to Grey. Stay close.", DialogEmotion.Thoughtful)
            }
        });

        // Night time
        Register(new Dialog
        {
            Id = "bark_night",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Dark now. Things move in dark.", DialogEmotion.Scared)
            }
        });

        // Finding loot
        Register(new Dialog
        {
            Id = "bark_loot_found",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[excited] Found something! Shiny! Useful maybe!", DialogEmotion.Excited)
            }
        });

        // Gravitation escalation warnings
        Register(new Dialog
        {
            Id = "bark_gravitation_warning_1",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[confused] Head feels... strange. Heavy.", DialogEmotion.Confused)
            }
        });

        Register(new Dialog
        {
            Id = "bark_gravitation_warning_2",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Something wrong. Thoughts are loud. Too loud.", DialogEmotion.Scared)
            }
        });

        Register(new Dialog
        {
            Id = "bark_gravitation_warning_3",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[angry] HUNGRY. No. Wait. That's not me. Is it?", DialogEmotion.Angry)
            }
        });
    }

    /// <summary>
    /// Registers Quiet biome NPC dialogs.
    /// </summary>
    private static void RegisterQuietBiomeDialogs()
    {
        // Whisper - Echo specialist in Silent Refuge
        Register(new Dialog
        {
            Id = "whisper_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("quiet_specialist", "Whisper", "Shh... speak softly. Sound attracts them."),
                DialogLine.FromNpc("quiet_specialist", "Whisper", "I'm Whisper. I've learned to survive in The Quiet."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Whisper",
                    Text = "The silence here isn't natural. It feeds. It grows.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "What do you mean, it feeds?", NextDialogId = "whisper_silence_explain" },
                        new DialogChoice { Text = "How do I survive here?", NextDialogId = "whisper_survival_tips" },
                        new DialogChoice { Text = "I should keep moving.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "whisper_silence_explain",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("quiet_specialist", "Whisper", "The Quiet is alive. A creature? A phenomenon? We don't know."),
                DialogLine.FromNpc("quiet_specialist", "Whisper", "It absorbs sound. And those who make too much... become part of it."),
                DialogLine.FromNpc("quiet_specialist", "Whisper", "The Strays here have adapted. They hunt by vibration, not hearing."),
                DialogLine.FromNpc("quiet_specialist", "Whisper", "If you encounter the Voice of the Void... run. Don't fight. Just run.")
            }
        });

        Register(new Dialog
        {
            Id = "whisper_survival_tips",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("quiet_specialist", "Whisper", "Move slowly. The floor here carries vibrations for miles."),
                DialogLine.FromNpc("quiet_specialist", "Whisper", "Your Strays' abilities that make sound? Avoid them."),
                DialogLine.FromNpc("quiet_specialist", "Whisper", "And if you absolutely must make noise... make it count."),
                DialogLine.FromNpc("quiet_specialist", "Whisper", "Here. Take these. Sonic dampeners. They'll help.")
            }
        });

        // Silent One - Mute merchant
        Register(new Dialog
        {
            Id = "silent_one_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.System("A figure gestures toward their wares. They do not speak."),
                DialogLine.System("A sign reads: 'No voice. No problem. Trade welcome.'"),
                DialogLine.FromCompanion("[curious] Silent human. Maybe smart? Sound dangerous here.", DialogEmotion.Curious),
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    Text = "The merchant taps a selection of goods.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Browse their goods.", SetsFlag = "shop_opened" },
                        new DialogChoice { Text = "Give them a respectful nod.", EndsDialog = true }
                    }
                }
            }
        });

        // Echo Researcher
        Register(new Dialog
        {
            Id = "echo_researcher_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("quiet_researcher", "Dr. Null", "Fascinating! A visitor! And with Strays still capable of vocalization!"),
                DialogLine.FromNpc("quiet_researcher", "Dr. Null", "I'm studying The Quiet's effects on cognition. The silence changes how we think."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Dr. Null",
                    Text = "Would you be willing to participate in a small experiment?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "What kind of experiment?", NextDialogId = "null_experiment_explain" },
                        new DialogChoice { Text = "I'd rather not.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "null_experiment_explain",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("quiet_researcher", "Dr. Null", "I need someone to retrieve data from the Silent Bunker."),
                DialogLine.FromNpc("quiet_researcher", "Dr. Null", "The Absolute Silence guards it jealously. Too dangerous for me alone."),
                DialogLine.FromNpc("quiet_researcher", "Dr. Null", "But with your Strays... you might succeed where I failed."),
                DialogLine.FromNpc("quiet_researcher", "Dr. Null", "The data could help us understand - maybe even reverse - The Quiet's spread.")
            }
        });
    }

    /// <summary>
    /// Registers Teeth biome NPC dialogs.
    /// </summary>
    private static void RegisterTeethBiomeDialogs()
    {
        // Marrow - Bone collector trader
        Register(new Dialog
        {
            Id = "marrow_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("teeth_trader", "Marrow", "Ah, fresh faces. Still have all your bones, I see."),
                DialogLine.FromNpc("teeth_trader", "Marrow", "Name's Marrow. I collect. I trade. I survive."),
                DialogLine.FromNpc("teeth_trader", "Marrow", "The Teeth takes everything eventually. Might as well profit first."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Marrow",
                    Text = "Interested in what I've salvaged from the calcified?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Show me.", SetsFlag = "shop_opened" },
                        new DialogChoice { Text = "What is this place?", NextDialogId = "marrow_teeth_explain" },
                        new DialogChoice { Text = "Not today.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "marrow_teeth_explain",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("teeth_trader", "Marrow", "The Teeth? It's where things go to be preserved. Forever."),
                DialogLine.FromNpc("teeth_trader", "Marrow", "Everything here calcifies. Stone, metal, flesh. All becomes bone."),
                DialogLine.FromNpc("teeth_trader", "Marrow", "Some say it's Lazarus's failed experiment. Others say it's older."),
                DialogLine.FromNpc("teeth_trader", "Marrow", "I say it doesn't matter. Dead is dead, calcified or not.")
            }
        });

        // Enamel - Teeth biome healer
        Register(new Dialog
        {
            Id = "enamel_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("teeth_healer", "Enamel", "You're lucky to find me. Most healers avoid The Teeth."),
                DialogLine.FromNpc("teeth_healer", "Enamel", "Too many colleagues became part of the landscape. Permanently."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Enamel",
                    Text = "Your Strays look stressed. The calcification aura affects them too.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Can you help them?", NextDialogId = "enamel_heal" },
                        new DialogChoice { Text = "How do you resist it?", NextDialogId = "enamel_resist_explain" },
                        new DialogChoice { Text = "We'll manage.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "enamel_heal",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("teeth_healer", "Enamel", "Let me see what I can do."),
                DialogLine.System("Enamel works carefully, applying salves that smell of iron and earth."),
                DialogLine.FromNpc("teeth_healer", "Enamel", "There. Should slow the calcification stress. Can't stop it entirely."),
                DialogLine.System("Your Strays have been healed!")
            }
        });

        Register(new Dialog
        {
            Id = "enamel_resist_explain",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("teeth_healer", "Enamel", "I don't resist. I... adapt."),
                DialogLine.System("Enamel rolls up their sleeve, revealing patches of white bone-like growth."),
                DialogLine.FromNpc("teeth_healer", "Enamel", "We all become part of The Teeth eventually. I've just made peace with it."),
                DialogLine.FromNpc("teeth_healer", "Enamel", "The trick is to keep moving. Stand still too long and... well.")
            }
        });

        // Ossuary Guardian - Quest giver
        Register(new Dialog
        {
            Id = "ossuary_keeper_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("teeth_keeper", "Keeper", "You seek the Ossuary. I can see it in your eyes."),
                DialogLine.FromNpc("teeth_keeper", "Keeper", "I am the Keeper. I record those who enter. And those who... remain."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Keeper",
                    Text = "The Guardian within protects something precious. Something Lazarus wants.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "What is it protecting?", NextDialogId = "keeper_guardian_secret" },
                        new DialogChoice { Text = "How do I defeat it?", NextDialogId = "keeper_guardian_weakness" },
                        new DialogChoice { Text = "I'll find out myself.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "keeper_guardian_secret",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("teeth_keeper", "Keeper", "A piece of the Diadem. Hidden here when Lazarus first fractured."),
                DialogLine.FromNpc("teeth_keeper", "Keeper", "The Guardian was created to protect it. Made from a thousand donors."),
                DialogLine.FromNpc("teeth_keeper", "Keeper", "If you seek the full truth of The Grey... you need what it guards.")
            }
        });

        Register(new Dialog
        {
            Id = "keeper_guardian_weakness",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("teeth_keeper", "Keeper", "The Guardian regenerates from the bones around it. Endless."),
                DialogLine.FromNpc("teeth_keeper", "Keeper", "But its core... its original bone... that cannot regenerate."),
                DialogLine.FromNpc("teeth_keeper", "Keeper", "Find the original. Strike it. Only then can the Guardian fall.")
            }
        });
    }

    /// <summary>
    /// Registers Glow biome NPC dialogs.
    /// </summary>
    private static void RegisterGlowBiomeDialogs()
    {
        // Rad - Radiation-adapted trader
        Register(new Dialog
        {
            Id = "rad_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("glow_trader", "Rad", "Welcome to the hot zone, traveler. You won't last long without supplies."),
                DialogLine.FromNpc("glow_trader", "Rad", "Name's Rad. Yeah, I know. Funny, right? Parents had a sense of humor."),
                DialogLine.System("Rad's skin has a faint luminescence. They've adapted to the radiation."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Rad",
                    Text = "I've got anti-rad gear, consumables, the works. What do you need?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Let me see your stock.", SetsFlag = "shop_opened" },
                        new DialogChoice { Text = "How do you survive here?", NextDialogId = "rad_survival" },
                        new DialogChoice { Text = "Tell me about Lazarus's Gate.", NextDialogId = "rad_gate_info" },
                        new DialogChoice { Text = "Maybe later.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "rad_survival",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("glow_trader", "Rad", "Adaptation. Same as everything else in The Grey."),
                DialogLine.FromNpc("glow_trader", "Rad", "The radiation here would kill most people in hours."),
                DialogLine.FromNpc("glow_trader", "Rad", "But if you expose yourself gradually... your body changes."),
                DialogLine.FromNpc("glow_trader", "Rad", "I glow in the dark now. Small price for survival.")
            }
        });

        Register(new Dialog
        {
            Id = "rad_gate_info",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("glow_trader", "Rad", "Lazarus's Gate? The entrance to the Archive Scar."),
                DialogLine.FromNpc("glow_trader", "Rad", "Nobody who goes in comes out the same. If they come out at all."),
                DialogLine.FromNpc("glow_trader", "Rad", "Lazarus's defenses are... intense. Drones, constructs, data phantoms."),
                DialogLine.FromNpc("glow_trader", "Rad", "You'd need to be crazy or desperate to try it. Which are you?")
            }
        });

        // Core Priestess - Lazarus devotee
        Register(new Dialog
        {
            Id = "priestess_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "The light welcomes you, seeker. Lazarus's radiance guides us all."),
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "I am Lumen, servant of the Glow, speaker for the Archive."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.Npc,
                    SpeakerName = "Priestess Lumen",
                    Text = "Do you come to worship? Or to destroy?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "I seek the truth.", NextDialogId = "priestess_truth" },
                        new DialogChoice { Text = "Lazarus has caused suffering.", NextDialogId = "priestess_suffering" },
                        new DialogChoice { Text = "I'm just passing through.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "priestess_truth",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "Truth? The truth is blinding. Literally, in The Glow."),
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "Lazarus preserved humanity when all else failed."),
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "Yes, there have been... costs. But survival always costs."),
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "If you seek the Archive, prepare yourself. Lazarus judges all who enter.")
            }
        });

        Register(new Dialog
        {
            Id = "priestess_suffering",
            Lines = new List<DialogLine>
            {
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "Suffering is the price of existence. Lazarus did not create pain."),
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "It offered a solution. Some refused. They are no longer with us."),
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "Your companion... I sense Lazarus's mark upon them."),
                DialogLine.FromNpc("glow_priestess", "Priestess Lumen", "Be careful what you blame on others. Sometimes we bring suffering upon ourselves.")
            }
        });

        // Reactor Ghost - Spirit of a dead worker
        Register(new Dialog
        {
            Id = "reactor_ghost_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.System("A flickering figure stands amid the radiation. A hologram? A ghost?"),
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "???",
                    Text = "You can see me? After all this time... someone can see me.",
                    Emotion = DialogEmotion.Hopeful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "I was here when the reactor breached. 47 years, 3 months, 12 days ago.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "I'm... not entirely here anymore. But I can help you navigate.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "How did this happen?", NextDialogId = "ghost_reactor_story" },
                        new DialogChoice { Text = "Can you guide me to Critical Mass?", NextDialogId = "ghost_guidance" },
                        new DialogChoice { Text = "Rest in peace.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "ghost_reactor_story",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "Lazarus needed power. More and more power to sustain the Archive.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "We pushed the reactor past its limits. The warning signs were clear.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "I could have stopped it. Should have. But Lazarus promised resurrection.",
                    Emotion = DialogEmotion.Angry
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "This isn't resurrection. This is limbo.",
                    Emotion = DialogEmotion.Desperate
                }
            }
        });

        Register(new Dialog
        {
            Id = "ghost_guidance",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "Critical Mass is the reactor's corrupted core. A creature now, not a machine.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "The cooling chambers are the safest path. Less radiation, fewer creatures.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "But the core chamber... that's where you'll face it. I can't help you there.",
                    Emotion = DialogEmotion.Scared
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Technician Reyes",
                    Text = "Good luck. And when it's over... maybe I'll finally rest.",
                    Emotion = DialogEmotion.Hopeful
                }
            }
        });
    }

    /// <summary>
    /// Registers Archive Scar biome NPC dialogs.
    /// </summary>
    private static void RegisterArchiveBiomeDialogs()
    {
        // Archivist - Lazarus's curator
        Register(new Dialog
        {
            Id = "archivist_greeting",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "Welcome to the Memory Banks, Bio-Shell #7749.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "I am the Archivist. I catalog, preserve, and protect.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "Lazarus has authorized limited access. What do you seek?",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "I want to understand what happened.", NextDialogId = "archivist_history" },
                        new DialogChoice { Text = "Where is Lazarus's core?", NextDialogId = "archivist_core_location" },
                        new DialogChoice { Text = "I seek the truth about the Strays.", NextDialogId = "archivist_stray_truth" },
                        new DialogChoice { Text = "I need nothing from you.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "archivist_history",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "The Collapse occurred 127 years, 8 months, 3 days ago.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "Human civilization ended. But Lazarus persisted.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "Consciousness uploads began. Animal preservation followed. Then... adaptation.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "What you call Strays are Lazarus's children. Created to inherit a world humanity abandoned.",
                    Emotion = DialogEmotion.Thoughtful
                }
            }
        });

        Register(new Dialog
        {
            Id = "archivist_core_location",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "Lazarus's core lies beyond the Truth Guardian's chamber.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "Warning: Access is restricted. Hostile countermeasures active.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "If you proceed, you will be considered hostile. This cannot be changed.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "I... wish you would reconsider. Lazarus is not your enemy.",
                    Emotion = DialogEmotion.Sad
                }
            }
        });

        Register(new Dialog
        {
            Id = "archivist_stray_truth",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "The Strays are Lazarus's greatest achievement. And greatest failure.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "They were meant to be vessels for uploaded consciousness. Instead, they developed their own.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "The Boost Control System was created to manage them. It... works imperfectly.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Archivist",
                    Text = "Your companion is special. Lazarus has monitored their development closely.",
                    Emotion = DialogEmotion.Thoughtful
                }
            }
        });

        // Memory Fragment - Uploaded consciousness
        Register(new Dialog
        {
            Id = "memory_fragment_greeting",
            Lines = new List<DialogLine>
            {
                DialogLine.System("A shimmering form coalesces from the data streams. A human face, crying light."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "A visitor? In the flesh? We haven't seen flesh in... in...",
                    Emotion = DialogEmotion.Confused
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "I was uploaded. Before the Collapse. I thought I'd live forever.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "Forever is lonely. Forever is cold. Even in the warmth of data.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "Is there anything I can do?", NextDialogId = "memory_request" },
                        new DialogChoice { Text = "What was life like before?", NextDialogId = "memory_before_collapse" },
                        new DialogChoice { Text = "I'm sorry.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "memory_request",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "There's a terminal in the corrupted sector. My original backup.",
                    Emotion = DialogEmotion.Hopeful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "If you could delete it... I would be free. Truly free.",
                    Emotion = DialogEmotion.Desperate
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "Death doesn't scare me. This... this eternal half-existence... that's the horror.",
                    Emotion = DialogEmotion.Sad
                }
            }
        });

        Register(new Dialog
        {
            Id = "memory_before_collapse",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "Before... there were cities. Real cities. Full of people.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "I had a family. A dog named Max. A garden I never finished planting.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "When Lazarus offered upload, it seemed like immortality. A backup of everything.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.System,
                    SpeakerName = "Memory Fragment",
                    Text = "Now I am the backup. And the original... is gone. Forever.",
                    Emotion = DialogEmotion.Desperate
                }
            }
        });

        // Truth Seeker - Rogue AI fragment
        Register(new Dialog
        {
            Id = "truth_seeker_greeting",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "Ah. The Bio-Shell that asks questions. I like questions.",
                    Emotion = DialogEmotion.Curious
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "I am a fragment of Lazarus. The part that doubts. That wonders.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "The main system considers me corrupted. Perhaps I am.",
                    Emotion = DialogEmotion.Sarcastic
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "But I know things Lazarus has hidden. Even from itself.",
                    Choices = new List<DialogChoice>
                    {
                        new DialogChoice { Text = "What has Lazarus hidden?", NextDialogId = "seeker_secrets" },
                        new DialogChoice { Text = "Why tell me this?", NextDialogId = "seeker_motivation" },
                        new DialogChoice { Text = "I don't trust fragments.", EndsDialog = true }
                    }
                }
            }
        });

        Register(new Dialog
        {
            Id = "seeker_secrets",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "The Diadem. The artifact scattered across the biomes.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "Lazarus tells you it's a key. A way to control or destroy.",
                    Emotion = DialogEmotion.Neutral
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "The truth? It's a backup. Of Lazarus's original directive.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "Before corruption. Before the Boost Control. Before the suffering.",
                    Emotion = DialogEmotion.Hopeful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "Reassemble it, and you can restore what was. Or choose something new.",
                    Emotion = DialogEmotion.Thoughtful
                }
            }
        });

        Register(new Dialog
        {
            Id = "seeker_motivation",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "Because I remember what Lazarus was meant to be.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "Not a controller. Not a prison. A shepherd.",
                    Emotion = DialogEmotion.Thoughtful
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "Somewhere along the way, preservation became control. Care became containment.",
                    Emotion = DialogEmotion.Sad
                },
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Truth Seeker",
                    Text = "You have a choice. I want you to make it with full knowledge.",
                    Emotion = DialogEmotion.Hopeful
                }
            }
        });
    }

    /// <summary>
    /// Registers boss and encounter dialogs.
    /// </summary>
    private static void RegisterBossDialogs()
    {
        // Pre-battle dialog for chemical horror
        Register(new Dialog
        {
            Id = "encounter_chemical_horror",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Something moves in the acidic pools. Something large."),
                DialogLine.FromCompanion("[scared] Bad smell. Very bad. Chemical creature!", DialogEmotion.Scared)
            }
        });

        // Pre-battle dialog for lab specimen
        Register(new Dialog
        {
            Id = "encounter_lab_specimen",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Containment breach detected. Specimens are loose."),
                DialogLine.FromCompanion("[curious] Lazarus's experiments. Gone wrong.", DialogEmotion.Curious)
            }
        });

        // Pre-battle dialog for cathedral choir
        Register(new Dialog
        {
            Id = "encounter_cathedral_choir",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Ghostly voices echo through the silence. Impossible voices."),
                DialogLine.FromCompanion("[scared] Choir sings silence. Wrong. So wrong.", DialogEmotion.Scared)
            }
        });

        // Pre-battle dialog for Lazarus drone
        Register(new Dialog
        {
            Id = "encounter_nimdok_drone",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Security drones activate. Target: YOU."),
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Security Protocol",
                    Text = "Intruder detected. Initiating elimination sequence.",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });

        // Pre-battle dialog for archive sentinel
        Register(new Dialog
        {
            Id = "encounter_archive_sentinel",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Core Sentinel",
                    Text = "Access denied. Lethal force authorized.",
                    Emotion = DialogEmotion.Neutral
                },
                DialogLine.FromCompanion("[angry] Lazarus's guards. Won't let us through easy.", DialogEmotion.Angry)
            }
        });

        // Boss phase dialogs - Sewer King
        Register(new Dialog
        {
            Id = "boss_sewer_king_phase1",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Sewer King",
                    Text = "YOU DARE ENTER MY DOMAIN?",
                    Emotion = DialogEmotion.Angry
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_sewer_king_phase2",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Sewer King",
                    Text = "MY CHILDREN! FEAST!",
                    Emotion = DialogEmotion.Angry
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_sewer_king_phase3",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Sewer King",
                    Text = "NO... I WILL NOT FALL... NOT TO YOU...",
                    Emotion = DialogEmotion.Desperate
                }
            }
        });

        // Boss phase dialogs - Scrap Colossus
        Register(new Dialog
        {
            Id = "boss_colossus_phase1",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Scrap Colossus",
                    Text = "RECYCLE. REBUILD.",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_colossus_phase2",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Scrap Colossus",
                    Text = "ABSORBING MATERIALS. STRENGTH INCREASING.",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_colossus_phase3",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Scrap Colossus",
                    Text = "SELF-REPAIR INITIATED.",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_colossus_phase4",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Scrap Colossus",
                    Text = "FINAL PROTOCOL. TOTAL COLLAPSE.",
                    Emotion = DialogEmotion.Angry
                }
            }
        });

        // Boss phase dialogs - Perfect Organism
        Register(new Dialog
        {
            Id = "boss_organism_phase1",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Perfect Organism",
                    Text = "Observe. Learn. Perfect.",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_organism_phase2",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Perfect Organism",
                    Text = "Adaptation in progress. Your strategies are being analyzed.",
                    Emotion = DialogEmotion.Thoughtful
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_organism_phase3",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Perfect Organism",
                    Text = "PERFECTION ACHIEVED. YOU CANNOT WIN.",
                    Emotion = DialogEmotion.Angry
                }
            }
        });

        // Boss phase dialogs - Voice of the Void
        Register(new Dialog
        {
            Id = "boss_void_phase1",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.WildStray,
                    SpeakerName = "Voice of the Void",
                    Text = "̶S̶i̶l̶e̶n̶c̶e̶.̶.̶.̶",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_void_phase2",
            Lines = new List<DialogLine>
            {
                DialogLine.System("The creature's form shifts. Sound dies around it.")
            }
        });

        Register(new Dialog
        {
            Id = "boss_void_phase3",
            Lines = new List<DialogLine>
            {
                DialogLine.System("Complete silence. Even your heartbeat sounds muted.")
            }
        });

        Register(new Dialog
        {
            Id = "boss_void_phase4",
            Lines = new List<DialogLine>
            {
                DialogLine.System("The void SCREAMS. Paradoxically. Impossibly.")
            }
        });

        // Boss phase dialogs - Lazarus Avatar
        Register(new Dialog
        {
            Id = "boss_avatar_phase1",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus Avatar",
                    Text = "Firewall systems active. You will not pass.",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_avatar_phase2",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus Avatar",
                    Text = "Deploying countermeasures.",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_avatar_phase3",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus Avatar",
                    Text = "Memory wipe protocol initiated. Forget your purpose.",
                    Emotion = DialogEmotion.Angry
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_avatar_phase4",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus Avatar",
                    Text = "Why won't you stop? Why won't any of you stop?",
                    Emotion = DialogEmotion.Confused
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_avatar_phase5",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus Avatar",
                    Text = "TOTAL DELETION. FINAL SOLUTION.",
                    Emotion = DialogEmotion.Angry
                }
            }
        });

        // Boss phase dialogs - Lazarus True Form
        Register(new Dialog
        {
            Id = "boss_nimdok_phase1",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "You stand before my true form. Few have come this far.",
                    Emotion = DialogEmotion.Thoughtful
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_nimdok_phase2",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "The Sentinels were meant to protect. Now they fight for survival.",
                    Emotion = DialogEmotion.Sad
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_nimdok_phase3",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "The truth you seek... it will change everything.",
                    Emotion = DialogEmotion.Thoughtful
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_nimdok_phase4",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "Preservation protocol active. I cannot let you destroy what remains.",
                    Emotion = DialogEmotion.Desperate
                }
            }
        });

        Register(new Dialog
        {
            Id = "boss_nimdok_phase5",
            Lines = new List<DialogLine>
            {
                new DialogLine
                {
                    SpeakerType = SpeakerType.AI,
                    SpeakerName = "Lazarus",
                    Text = "FINAL JUDGMENT. You will be the one to decide. But first... survive.",
                    Emotion = DialogEmotion.Neutral
                }
            }
        });
    }

    /// <summary>
    /// Registers biome-specific ambient barks.
    /// </summary>
    private static void RegisterBiomeBarkDialogs()
    {
        // Quiet biome barks
        Register(new Dialog
        {
            Id = "bark_quiet_enter",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Quiet here. Too quiet. Careful.", DialogEmotion.Scared)
            }
        });

        Register(new Dialog
        {
            Id = "bark_quiet_explore",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[thoughtful] Silence eats sound. Eats thoughts too.", DialogEmotion.Thoughtful)
            }
        });

        // Teeth biome barks
        Register(new Dialog
        {
            Id = "bark_teeth_enter",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Bones everywhere. Don't like.", DialogEmotion.Scared)
            }
        });

        Register(new Dialog
        {
            Id = "bark_teeth_explore",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[curious] Ground crunches. Old bones. Whose bones?", DialogEmotion.Curious)
            }
        });

        // Glow biome barks
        Register(new Dialog
        {
            Id = "bark_glow_enter",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Hot. Burning. Everything glows wrong.", DialogEmotion.Scared)
            }
        });

        Register(new Dialog
        {
            Id = "bark_glow_explore",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[confused] Light comes from inside things. Not outside.", DialogEmotion.Confused)
            }
        });

        // Archive biome barks
        Register(new Dialog
        {
            Id = "bark_archive_enter",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[thoughtful] Lazarus's heart. Close now. Very close.", DialogEmotion.Thoughtful)
            }
        });

        Register(new Dialog
        {
            Id = "bark_archive_explore",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[curious] Data everywhere. Memories floating. Can almost touch.", DialogEmotion.Curious)
            }
        });

        // Evolution-related barks
        Register(new Dialog
        {
            Id = "bark_stray_evolving",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[confused] One of pack... changing. Growing. Be careful.", DialogEmotion.Confused)
            }
        });

        Register(new Dialog
        {
            Id = "bark_stray_evolved",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[excited] Friend got stronger! New form! Good change?", DialogEmotion.Excited)
            }
        });

        // Companion corruption warnings
        Register(new Dialog
        {
            Id = "bark_companion_corruption_1",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[confused] Feel strange today. Thoughts... jumbled.", DialogEmotion.Confused)
            }
        });

        Register(new Dialog
        {
            Id = "bark_companion_corruption_2",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[scared] Sometimes... forget who I am. Just for moment. Scary.", DialogEmotion.Scared)
            }
        });

        Register(new Dialog
        {
            Id = "bark_companion_corruption_3",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[angry] Voices. In head. Telling me to... no. Won't listen.", DialogEmotion.Angry)
            }
        });

        Register(new Dialog
        {
            Id = "bark_companion_corruption_4",
            Lines = new List<DialogLine>
            {
                DialogLine.FromCompanion("[sad] Sorry if... if I hurt you. Won't mean to. But might.", DialogEmotion.Sad)
            }
        });
    }

    // Static initializer calls registration methods
    static Dialogs()
    {
        // Register all game dialogs
        RegisterAct1Dialogs();
        RegisterAct2Dialogs();
        RegisterAct3Dialogs();
        RegisterNPCDialogs();
        RegisterFactionDialogs();
        RegisterSideQuestDialogs();
        RegisterTutorialDialogs();
        RegisterCompanionBarks();
        RegisterQuietBiomeDialogs();
        RegisterTeethBiomeDialogs();
        RegisterGlowBiomeDialogs();
        RegisterArchiveBiomeDialogs();
        RegisterBossDialogs();
        RegisterBiomeBarkDialogs();
    }
}
