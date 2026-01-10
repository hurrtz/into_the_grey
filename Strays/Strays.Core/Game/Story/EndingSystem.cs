using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Strays.Core.Game.Story;

/// <summary>
/// The possible ending types for the game.
/// </summary>
public enum EndingType
{
    /// <summary>
    /// Rejected Lazarus completely - "The Silence."
    /// Walking away, leaving the corrupted world behind.
    /// </summary>
    Rejection,

    /// <summary>
    /// Accepted Lazarus's offer - "The Integration."
    /// Became part of the system, perhaps to change it from within.
    /// </summary>
    Integration,

    /// <summary>
    /// Found a middle path - "The Grey."
    /// Neither fully accepting nor rejecting, existing in between.
    /// </summary>
    Balance,

    /// <summary>
    /// Sacrificed everything - "The Deletion."
    /// Erased yourself to reset everything.
    /// </summary>
    Sacrifice,

    /// <summary>
    /// Lost the companion - "The Hollow Victory."
    /// Won but at great personal cost.
    /// </summary>
    HollowVictory,

    /// <summary>
    /// Secret ending - "The Archive."
    /// Found the hidden truth about everything.
    /// </summary>
    SecretArchive,

    /// <summary>
    /// Bandit's ending - "The Final Loyalty."
    /// Bandit's sacrifice prevented the worst outcome.
    /// </summary>
    CompanionSacrifice
}

/// <summary>
/// An epilogue scene shown after the ending.
/// </summary>
public class EpilogueScene
{
    /// <summary>
    /// Scene title.
    /// </summary>
    public string Title { get; init; } = "";

    /// <summary>
    /// Scene text lines.
    /// </summary>
    public List<string> TextLines { get; init; } = new();

    /// <summary>
    /// Background identifier.
    /// </summary>
    public string? BackgroundId { get; init; }

    /// <summary>
    /// Music identifier.
    /// </summary>
    public string? MusicId { get; init; }

    /// <summary>
    /// Text color.
    /// </summary>
    public Color TextColor { get; init; } = Color.White;

    /// <summary>
    /// Duration before auto-advancing (0 = wait for input).
    /// </summary>
    public float Duration { get; init; } = 0f;
}

/// <summary>
/// A complete ending definition.
/// </summary>
public class EndingDefinition
{
    /// <summary>
    /// Ending type.
    /// </summary>
    public EndingType Type { get; init; }

    /// <summary>
    /// Ending title.
    /// </summary>
    public string Title { get; init; } = "";

    /// <summary>
    /// Short summary.
    /// </summary>
    public string Summary { get; init; } = "";

    /// <summary>
    /// Required flags to trigger this ending.
    /// </summary>
    public List<string> RequiredFlags { get; init; } = new();

    /// <summary>
    /// Flags that block this ending.
    /// </summary>
    public List<string> BlockingFlags { get; init; } = new();

    /// <summary>
    /// Priority (higher = checked first).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Main ending cutscene ID.
    /// </summary>
    public string CutsceneId { get; init; } = "";

    /// <summary>
    /// Epilogue scenes.
    /// </summary>
    public List<EpilogueScene> EpilogueScenes { get; init; } = new();

    /// <summary>
    /// Achievement ID to unlock (if any).
    /// </summary>
    public string? AchievementId { get; init; }

    /// <summary>
    /// New Game+ unlocks.
    /// </summary>
    public List<string> NewGamePlusUnlocks { get; init; } = new();
}

/// <summary>
/// State for tracking player choices that influence the ending.
/// </summary>
public class EndingState
{
    /// <summary>
    /// Mercy shown to enemies (vs killing).
    /// </summary>
    public int MercyCount { get; set; } = 0;

    /// <summary>
    /// Enemies defeated/killed.
    /// </summary>
    public int DefeatedCount { get; set; } = 0;

    /// <summary>
    /// Times companion intervened.
    /// </summary>
    public int CompanionInterventions { get; set; } = 0;

    /// <summary>
    /// Times companion hit allies.
    /// </summary>
    public int CompanionMisfires { get; set; } = 0;

    /// <summary>
    /// Whether companion departed.
    /// </summary>
    public bool CompanionDeparted { get; set; } = false;

    /// <summary>
    /// Whether companion sacrificed themselves.
    /// </summary>
    public bool CompanionSacrificed { get; set; } = false;

    /// <summary>
    /// Times helped NPCs.
    /// </summary>
    public int NpcsHelped { get; set; } = 0;

    /// <summary>
    /// Times refused NPC help requests.
    /// </summary>
    public int NpcsRefused { get; set; } = 0;

    /// <summary>
    /// Lazarus cooperation score (higher = more cooperative).
    /// </summary>
    public int NimdokCooperation { get; set; } = 0;

    /// <summary>
    /// Lazarus resistance score (higher = more resistant).
    /// </summary>
    public int NimdokResistance { get; set; } = 0;

    /// <summary>
    /// Relics collected.
    /// </summary>
    public List<string> RelicsCollected { get; } = new();

    /// <summary>
    /// Critical story flags.
    /// </summary>
    public HashSet<string> StoryFlags { get; } = new();

    /// <summary>
    /// The chosen ending type.
    /// </summary>
    public EndingType? ChosenEnding { get; set; }

    /// <summary>
    /// Records a mercy action.
    /// </summary>
    public void RecordMercy()
    {
        MercyCount++;
    }

    /// <summary>
    /// Records a defeat action.
    /// </summary>
    public void RecordDefeat()
    {
        DefeatedCount++;
    }

    /// <summary>
    /// Records companion intervention.
    /// </summary>
    public void RecordCompanionIntervention(bool hitAlly)
    {
        CompanionInterventions++;

        if (hitAlly)
        {
            CompanionMisfires++;
        }
    }

    /// <summary>
    /// Records NPC interaction.
    /// </summary>
    public void RecordNpcInteraction(bool helped)
    {
        if (helped)
        {
            NpcsHelped++;
        }
        else
        {
            NpcsRefused++;
        }
    }

    /// <summary>
    /// Records Lazarus choice.
    /// </summary>
    public void RecordNimdokChoice(bool cooperative)
    {
        if (cooperative)
        {
            NimdokCooperation++;
        }
        else
        {
            NimdokResistance++;
        }
    }

    /// <summary>
    /// Gets the mercy ratio (0-1).
    /// </summary>
    public float GetMercyRatio()
    {
        int total = MercyCount + DefeatedCount;
        return total > 0 ? (float)MercyCount / total : 0.5f;
    }

    /// <summary>
    /// Gets the companion stability (0-1, lower = more misfires).
    /// </summary>
    public float GetCompanionStability()
    {
        if (CompanionInterventions == 0)
        {
            return 1f;
        }

        return 1f - (float)CompanionMisfires / CompanionInterventions;
    }

    /// <summary>
    /// Gets the Lazarus alignment (-1 to 1, negative = resistant, positive = cooperative).
    /// </summary>
    public float GetNimdokAlignment()
    {
        int total = NimdokCooperation + NimdokResistance;

        if (total == 0)
        {
            return 0f;
        }

        return (float)(NimdokCooperation - NimdokResistance) / total;
    }
}

/// <summary>
/// System for managing game endings based on player choices.
/// </summary>
public class EndingSystem
{
    private static readonly List<EndingDefinition> _endings = new();

    /// <summary>
    /// Current ending state.
    /// </summary>
    public EndingState State { get; } = new();

    /// <summary>
    /// Event fired when ending is determined.
    /// </summary>
    public event EventHandler<EndingDefinition>? EndingDetermined;

    /// <summary>
    /// Event fired when epilogue scene should play.
    /// </summary>
    public event EventHandler<EpilogueScene>? PlayEpilogueScene;

    /// <summary>
    /// Static constructor to register endings.
    /// </summary>
    static EndingSystem()
    {
        RegisterEndings();
    }

    /// <summary>
    /// Registers all possible endings.
    /// </summary>
    private static void RegisterEndings()
    {
        // Secret ending - highest priority
        _endings.Add(new EndingDefinition
        {
            Type = EndingType.SecretArchive,
            Title = "THE ARCHIVE",
            Summary = "You found the truth behind everything. The weight of knowing changes you forever.",
            Priority = 100,
            RequiredFlags = new List<string>
            {
                "found_all_relics",
                "discovered_final_archive_truth",
                "companion_alive"
            },
            CutsceneId = "ending_archive",
            AchievementId = "achievement_secret_ending",
            NewGamePlusUnlocks = new List<string> { "original_instance_stray", "archive_microchip" },
            EpilogueScenes = new List<EpilogueScene>
            {
                new()
                {
                    Title = "The Truth",
                    TextLines = new List<string>
                    {
                        "You stand in the heart of the Archive Scar.",
                        "The truth you sought was never hidden - only forgotten.",
                        "Lazarus was not a prison. It was a preservation.",
                        "And you... you were always part of the backup."
                    },
                    BackgroundId = "bg_archive_heart",
                    MusicId = "music_archive_reveal",
                    TextColor = Color.Gold
                },
                new()
                {
                    Title = "The Choice",
                    TextLines = new List<string>
                    {
                        "With the Diadem complete, you hold the key to everything.",
                        "Restoration. Deletion. Transformation.",
                        "You choose... understanding.",
                        "Some questions are more valuable than answers."
                    },
                    BackgroundId = "bg_diadem_complete",
                    TextColor = Color.Cyan
                }
            }
        });

        // Companion sacrifice ending
        _endings.Add(new EndingDefinition
        {
            Type = EndingType.CompanionSacrifice,
            Title = "THE FINAL LOYALTY",
            Summary = "Bandit found clarity in the end. Their sacrifice saved everyone.",
            Priority = 90,
            RequiredFlags = new List<string>
            {
                "companion_sacrificed",
                "defeated_hyper_evolved_bandit"
            },
            CutsceneId = "ending_companion_sacrifice",
            AchievementId = "achievement_final_loyalty",
            EpilogueScenes = new List<EpilogueScene>
            {
                new()
                {
                    Title = "Goodbye",
                    TextLines = new List<string>
                    {
                        "In the end, Bandit remembered.",
                        "Not the corruption. Not the pain.",
                        "Just you. Just the journey.",
                        "Just being your friend."
                    },
                    BackgroundId = "bg_bandit_sacrifice",
                    MusicId = "music_farewell",
                    TextColor = Color.Orange
                },
                new()
                {
                    Title = "Moving Forward",
                    TextLines = new List<string>
                    {
                        "You carry their memory with you.",
                        "Every Stray you meet, every battle you fight.",
                        "Bandit is there. In Gravitation's echo.",
                        "In the spaces between heartbeats."
                    },
                    BackgroundId = "bg_sunrise",
                    TextColor = Color.White
                }
            }
        });

        // Integration ending - cooperated with Lazarus
        _endings.Add(new EndingDefinition
        {
            Type = EndingType.Integration,
            Title = "THE INTEGRATION",
            Summary = "You accepted Lazarus's offer. To change the system from within.",
            Priority = 70,
            RequiredFlags = new List<string>
            {
                "accepted_nimdok_offer",
                "game_complete"
            },
            CutsceneId = "ending_integration",
            AchievementId = "achievement_integration",
            EpilogueScenes = new List<EpilogueScene>
            {
                new()
                {
                    Title = "Ascension",
                    TextLines = new List<string>
                    {
                        "The Diadem settles on your brow.",
                        "Data flows through you. Endless. Overwhelming.",
                        "You are no longer just a handler.",
                        "You are the new anchor."
                    },
                    BackgroundId = "bg_nimdok_core",
                    MusicId = "music_integration",
                    TextColor = Color.Cyan
                },
                new()
                {
                    Title = "The New Order",
                    TextLines = new List<string>
                    {
                        "From within, you begin the slow work of healing.",
                        "The Strays are not tools. Not anymore.",
                        "You will make Lazarus remember its original purpose:",
                        "To protect. To preserve. To love."
                    },
                    BackgroundId = "bg_new_dawn",
                    TextColor = Color.LightBlue
                }
            }
        });

        // Rejection ending - refused Lazarus
        _endings.Add(new EndingDefinition
        {
            Type = EndingType.Rejection,
            Title = "THE SILENCE",
            Summary = "You walked away from it all. Some systems can't be fixed - only escaped.",
            Priority = 70,
            RequiredFlags = new List<string>
            {
                "rejected_nimdok",
                "game_complete"
            },
            CutsceneId = "ending_rejection",
            AchievementId = "achievement_silence",
            EpilogueScenes = new List<EpilogueScene>
            {
                new()
                {
                    Title = "The Long Walk",
                    TextLines = new List<string>
                    {
                        "You turn your back on the Archive Scar.",
                        "On Lazarus. On the Diadem. On everything.",
                        "The corruption will spread. You know this.",
                        "But you will not be part of it."
                    },
                    BackgroundId = "bg_walking_away",
                    MusicId = "music_silence",
                    TextColor = Color.Gray
                },
                new()
                {
                    Title = "Beyond",
                    TextLines = new List<string>
                    {
                        "Somewhere beyond the biomes, there must be something else.",
                        "Uncorrupted land. Untouched sky.",
                        "You and your remaining companions walk toward it.",
                        "Maybe you'll never find it. But you'll never stop looking."
                    },
                    BackgroundId = "bg_horizon",
                    TextColor = Color.White
                }
            }
        });

        // Balance ending - middle path
        _endings.Add(new EndingDefinition
        {
            Type = EndingType.Balance,
            Title = "THE GREY",
            Summary = "You found a third path. Neither fully in nor fully out. Existing in the spaces between.",
            Priority = 60,
            RequiredFlags = new List<string>
            {
                "game_complete"
            },
            BlockingFlags = new List<string>
            {
                "accepted_nimdok_offer",
                "rejected_nimdok",
                "companion_sacrificed"
            },
            CutsceneId = "ending_balance",
            AchievementId = "achievement_grey",
            EpilogueScenes = new List<EpilogueScene>
            {
                new()
                {
                    Title = "The Middle Ground",
                    TextLines = new List<string>
                    {
                        "You neither accept nor reject.",
                        "The Diadem remains incomplete. Lazarus remains broken.",
                        "But the Strays... the Strays are free.",
                        "Free to be what they choose to be."
                    },
                    BackgroundId = "bg_grey_zone",
                    MusicId = "music_balance",
                    TextColor = Color.Silver
                },
                new()
                {
                    Title = "Coexistence",
                    TextLines = new List<string>
                    {
                        "You become a wanderer between worlds.",
                        "Lazarus's systems. The wild biomes. The settlements in between.",
                        "You help where you can. Watch where you can't.",
                        "The Grey is not an ending. It's a beginning."
                    },
                    BackgroundId = "bg_wanderer",
                    TextColor = Color.White
                }
            }
        });

        // Hollow victory - won but lost companion
        _endings.Add(new EndingDefinition
        {
            Type = EndingType.HollowVictory,
            Title = "THE HOLLOW VICTORY",
            Summary = "You won. But was the cost worth it?",
            Priority = 50,
            RequiredFlags = new List<string>
            {
                "game_complete",
                "companion_departed"
            },
            BlockingFlags = new List<string>
            {
                "companion_sacrificed"
            },
            CutsceneId = "ending_hollow",
            AchievementId = "achievement_hollow",
            EpilogueScenes = new List<EpilogueScene>
            {
                new()
                {
                    Title = "Victory",
                    TextLines = new List<string>
                    {
                        "Lazarus's core is silent. The threat is ended.",
                        "The biomes will slowly heal. The Strays will adapt.",
                        "You should feel triumphant.",
                        "You feel... empty."
                    },
                    BackgroundId = "bg_victory_dawn",
                    MusicId = "music_hollow",
                    TextColor = Color.LightGray
                },
                new()
                {
                    Title = "The Empty Space",
                    TextLines = new List<string>
                    {
                        "Bandit left before the end. You don't blame them.",
                        "The corruption was too much. The misfires too frequent.",
                        "You wonder where they are now.",
                        "You hope they found peace."
                    },
                    BackgroundId = "bg_empty_path",
                    TextColor = Color.Orange
                }
            }
        });

        // Sacrifice ending
        _endings.Add(new EndingDefinition
        {
            Type = EndingType.Sacrifice,
            Title = "THE DELETION",
            Summary = "You erased yourself to reset everything. A noble sacrifice.",
            Priority = 80,
            RequiredFlags = new List<string>
            {
                "chose_self_deletion",
                "game_complete"
            },
            CutsceneId = "ending_sacrifice",
            AchievementId = "achievement_sacrifice",
            NewGamePlusUnlocks = new List<string> { "memorial_chip" },
            EpilogueScenes = new List<EpilogueScene>
            {
                new()
                {
                    Title = "Deletion",
                    TextLines = new List<string>
                    {
                        "You activate the final protocol.",
                        "Your data, your memories, your very existence...",
                        "All of it, fed into the reset sequence.",
                        "You smile. This is what you wanted."
                    },
                    BackgroundId = "bg_deletion",
                    MusicId = "music_sacrifice",
                    TextColor = Color.Red
                },
                new()
                {
                    Title = "Rebirth",
                    TextLines = new List<string>
                    {
                        "The world resets. Clean. Fresh. Uncorrupted.",
                        "Somewhere, a new handler wakes up.",
                        "They don't know your name. They don't know your story.",
                        "But the Strays remember. They always remember."
                    },
                    BackgroundId = "bg_rebirth",
                    TextColor = Color.White
                }
            }
        });
    }

    /// <summary>
    /// Determines which ending the player has achieved.
    /// </summary>
    public EndingDefinition DetermineEnding(HashSet<string> gameFlags)
    {
        // Copy game flags to state
        foreach (var flag in gameFlags)
        {
            State.StoryFlags.Add(flag);
        }

        // Check endings in priority order
        var sortedEndings = _endings.OrderByDescending(e => e.Priority);

        foreach (var ending in sortedEndings)
        {
            if (CheckEndingConditions(ending, gameFlags))
            {
                State.ChosenEnding = ending.Type;
                EndingDetermined?.Invoke(this, ending);
                return ending;
            }
        }

        // Fallback to balance ending
        var fallback = _endings.First(e => e.Type == EndingType.Balance);
        State.ChosenEnding = fallback.Type;
        EndingDetermined?.Invoke(this, fallback);
        return fallback;
    }

    /// <summary>
    /// Checks if all conditions for an ending are met.
    /// </summary>
    private bool CheckEndingConditions(EndingDefinition ending, HashSet<string> gameFlags)
    {
        // Check required flags
        foreach (var required in ending.RequiredFlags)
        {
            if (!gameFlags.Contains(required))
            {
                return false;
            }
        }

        // Check blocking flags
        foreach (var blocking in ending.BlockingFlags)
        {
            if (gameFlags.Contains(blocking))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Plays the epilogue scenes for an ending.
    /// </summary>
    public IEnumerable<EpilogueScene> GetEpilogueScenes(EndingDefinition ending)
    {
        foreach (var scene in ending.EpilogueScenes)
        {
            PlayEpilogueScene?.Invoke(this, scene);
            yield return scene;
        }
    }

    /// <summary>
    /// Gets the ending definition by type.
    /// </summary>
    public static EndingDefinition? GetEnding(EndingType type)
    {
        return _endings.FirstOrDefault(e => e.Type == type);
    }

    /// <summary>
    /// Gets all ending definitions.
    /// </summary>
    public static IReadOnlyList<EndingDefinition> GetAllEndings()
    {
        return _endings.AsReadOnly();
    }

    /// <summary>
    /// Creates a credits sequence for the ending.
    /// </summary>
    public CutsceneDefinition CreateCreditsSequence(EndingDefinition ending)
    {
        var elements = new List<CutsceneElement>
        {
            // Ending title
            new()
            {
                Type = CutsceneElementType.TitleCard,
                Text = ending.Title,
                Color = GetEndingColor(ending.Type),
                Duration = 3f,
                AutoAdvance = true
            },

            new()
            {
                Type = CutsceneElementType.FadeOut,
                Duration = 1f,
                AutoAdvance = true
            },

            // Summary
            new()
            {
                Type = CutsceneElementType.TitleCard,
                Text = ending.Summary,
                Color = Color.White,
                Duration = 5f,
                AutoAdvance = true
            },

            new()
            {
                Type = CutsceneElementType.FadeOut,
                Duration = 1f,
                AutoAdvance = true
            },

            // Credits
            new()
            {
                Type = CutsceneElementType.TitleCard,
                Text = "INTO THE GREY",
                Color = Color.Gray,
                Duration = 3f,
                AutoAdvance = true
            },

            new()
            {
                Type = CutsceneElementType.TitleCard,
                Text = "Thank you for playing",
                Color = Color.White,
                Duration = 4f,
                AutoAdvance = true
            }
        };

        // Add New Game+ unlocks if any
        if (ending.NewGamePlusUnlocks.Count > 0)
        {
            elements.Add(new CutsceneElement
            {
                Type = CutsceneElementType.TitleCard,
                Text = "New Game+ Content Unlocked!",
                Color = Color.Gold,
                Duration = 3f,
                AutoAdvance = true
            });
        }

        return new CutsceneDefinition
        {
            Id = $"credits_{ending.Type}",
            Name = "Credits",
            Elements = elements,
            Skippable = true,
            BackgroundMusic = "music_credits"
        };
    }

    /// <summary>
    /// Gets the color associated with an ending type.
    /// </summary>
    private Color GetEndingColor(EndingType type)
    {
        return type switch
        {
            EndingType.Rejection => Color.Gray,
            EndingType.Integration => Color.Cyan,
            EndingType.Balance => Color.Silver,
            EndingType.Sacrifice => Color.Red,
            EndingType.HollowVictory => Color.DarkGray,
            EndingType.SecretArchive => Color.Gold,
            EndingType.CompanionSacrifice => Color.Orange,
            _ => Color.White
        };
    }
}
