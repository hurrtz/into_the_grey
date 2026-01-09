using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Strays.Core.Game.Progression;

/// <summary>
/// Types of tutorial/help content.
/// </summary>
public enum TutorialType
{
    /// <summary>
    /// One-time popup that doesn't repeat.
    /// </summary>
    OneTime,

    /// <summary>
    /// Contextual hint that can appear multiple times.
    /// </summary>
    Contextual,

    /// <summary>
    /// Always-available help topic.
    /// </summary>
    HelpTopic,

    /// <summary>
    /// Loading screen tip.
    /// </summary>
    LoadingTip
}

/// <summary>
/// Trigger conditions for tutorials.
/// </summary>
public enum TutorialTrigger
{
    /// <summary>
    /// Triggered manually.
    /// </summary>
    Manual,

    /// <summary>
    /// First time entering combat.
    /// </summary>
    FirstCombat,

    /// <summary>
    /// First time in menu.
    /// </summary>
    FirstMenu,

    /// <summary>
    /// First microchip equipped.
    /// </summary>
    FirstMicrochip,

    /// <summary>
    /// First recruitment opportunity.
    /// </summary>
    FirstRecruitment,

    /// <summary>
    /// First dungeon entered.
    /// </summary>
    FirstDungeon,

    /// <summary>
    /// First evolution.
    /// </summary>
    FirstEvolution,

    /// <summary>
    /// Companion intervention occurs.
    /// </summary>
    GravitationUsed,

    /// <summary>
    /// Low health warning.
    /// </summary>
    LowHealth,

    /// <summary>
    /// First shop visit.
    /// </summary>
    FirstShop,

    /// <summary>
    /// New biome entered.
    /// </summary>
    NewBiome,

    /// <summary>
    /// Quest received.
    /// </summary>
    QuestReceived,

    /// <summary>
    /// Overheated chip.
    /// </summary>
    ChipOverheated
}

/// <summary>
/// A tutorial or help entry.
/// </summary>
public class TutorialEntry
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display title.
    /// </summary>
    public string Title { get; init; } = "";

    /// <summary>
    /// Main content text.
    /// </summary>
    public string Content { get; init; } = "";

    /// <summary>
    /// Additional tips.
    /// </summary>
    public List<string> Tips { get; init; } = new();

    /// <summary>
    /// Tutorial type.
    /// </summary>
    public TutorialType Type { get; init; } = TutorialType.OneTime;

    /// <summary>
    /// Trigger condition.
    /// </summary>
    public TutorialTrigger Trigger { get; init; } = TutorialTrigger.Manual;

    /// <summary>
    /// Category for help topics.
    /// </summary>
    public string Category { get; init; } = "General";

    /// <summary>
    /// Priority (higher = shown first).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Related control hints.
    /// </summary>
    public Dictionary<string, string> Controls { get; init; } = new();

    /// <summary>
    /// Icon color.
    /// </summary>
    public Color IconColor { get; init; } = Color.Yellow;
}

/// <summary>
/// State of tutorial display.
/// </summary>
public class TutorialDisplayState
{
    /// <summary>
    /// Currently displayed tutorial.
    /// </summary>
    public TutorialEntry? CurrentTutorial { get; set; }

    /// <summary>
    /// Display timer.
    /// </summary>
    public float DisplayTimer { get; set; } = 0f;

    /// <summary>
    /// Whether waiting for dismissal.
    /// </summary>
    public bool WaitingForDismiss { get; set; } = false;

    /// <summary>
    /// Fade progress (0-1).
    /// </summary>
    public float FadeProgress { get; set; } = 0f;

    /// <summary>
    /// Whether fading in or out.
    /// </summary>
    public bool IsFadingIn { get; set; } = true;
}

/// <summary>
/// System for managing tutorials and contextual help.
/// </summary>
public class TutorialSystem
{
    private static readonly Dictionary<string, TutorialEntry> _entries = new();
    private readonly HashSet<string> _seenTutorials = new();
    private readonly Queue<TutorialEntry> _pendingTutorials = new();

    /// <summary>
    /// Current display state.
    /// </summary>
    public TutorialDisplayState DisplayState { get; } = new();

    /// <summary>
    /// Whether tutorials are enabled.
    /// </summary>
    public bool TutorialsEnabled { get; set; } = true;

    /// <summary>
    /// Whether contextual hints are enabled.
    /// </summary>
    public bool HintsEnabled { get; set; } = true;

    /// <summary>
    /// Event fired when a tutorial is shown.
    /// </summary>
    public event EventHandler<TutorialEntry>? TutorialShown;

    /// <summary>
    /// Event fired when a tutorial is dismissed.
    /// </summary>
    public event EventHandler<TutorialEntry>? TutorialDismissed;

    static TutorialSystem()
    {
        RegisterTutorials();
    }

    /// <summary>
    /// Registers all tutorial entries.
    /// </summary>
    private static void RegisterTutorials()
    {
        // Combat tutorials
        Register(new TutorialEntry
        {
            Id = "tut_combat_basics",
            Title = "Combat Basics",
            Content = "Combat uses an Active Time Battle (ATB) system. Wait for your Stray's gauge to fill, then select an action.",
            Tips = new List<string>
            {
                "Attack deals physical damage based on your Attack stat",
                "Abilities use Energy Points (EP) and can have various effects",
                "Defend reduces incoming damage and recovers some EP",
                "Flee attempts to escape - success depends on Speed"
            },
            Type = TutorialType.OneTime,
            Trigger = TutorialTrigger.FirstCombat,
            Category = "Combat",
            Priority = 100,
            Controls = new Dictionary<string, string>
            {
                { "↑↓", "Navigate menu" },
                { "Enter/Space", "Confirm selection" },
                { "Esc", "Cancel/Back" }
            },
            IconColor = Color.Red
        });

        Register(new TutorialEntry
        {
            Id = "tut_abilities",
            Title = "Using Abilities",
            Content = "Abilities come from equipped Microchips. Each chip grants different powers.",
            Tips = new List<string>
            {
                "Abilities cost Energy Points (EP)",
                "Using abilities generates Heat on the chip",
                "Overheated chips cannot use abilities until they cool down",
                "Different elements are strong/weak against others"
            },
            Type = TutorialType.OneTime,
            Trigger = TutorialTrigger.FirstMicrochip,
            Category = "Combat",
            Priority = 90,
            IconColor = Color.Cyan
        });

        Register(new TutorialEntry
        {
            Id = "tut_chip_overheat",
            Title = "Chip Overheated!",
            Content = "Your microchip has overheated! It needs time to cool down before you can use its abilities again.",
            Tips = new List<string>
            {
                "Heat is generated each time you use a chip's ability",
                "Defending helps chips cool faster",
                "Some augmentations improve heat dissipation",
                "Consider spreading abilities across multiple chips"
            },
            Type = TutorialType.Contextual,
            Trigger = TutorialTrigger.ChipOverheated,
            Category = "Combat",
            IconColor = Color.OrangeRed
        });

        // Companion tutorials
        Register(new TutorialEntry
        {
            Id = "tut_gravitation",
            Title = "Bandit's Gravitation",
            Content = "Your companion can use Gravitation - a powerful ability that deals damage based on target HP percentage.",
            Tips = new List<string>
            {
                "Gravitation triggers randomly during combat",
                "As Bandit's chip degrades, Gravitation becomes unstable",
                "Unstable Gravitation may accidentally hit your party",
                "This is central to the story - watch for changes"
            },
            Type = TutorialType.OneTime,
            Trigger = TutorialTrigger.GravitationUsed,
            Category = "Companion",
            Priority = 95,
            IconColor = Color.Orange
        });

        // Recruitment tutorials
        Register(new TutorialEntry
        {
            Id = "tut_recruitment",
            Title = "Stray Recruitment",
            Content = "After winning a battle, you may have a chance to recruit one of the defeated Strays!",
            Tips = new List<string>
            {
                "Not all Strays can be recruited",
                "Some Strays have special recruitment conditions",
                "Your reputation with the Strays faction affects success rate",
                "Recruited Strays join at their defeated level"
            },
            Type = TutorialType.OneTime,
            Trigger = TutorialTrigger.FirstRecruitment,
            Category = "Strays",
            Priority = 85,
            IconColor = Color.LimeGreen
        });

        Register(new TutorialEntry
        {
            Id = "tut_evolution",
            Title = "Stray Evolution",
            Content = "Your Stray is ready to evolve! Evolution permanently transforms a Stray into a stronger form.",
            Tips = new List<string>
            {
                "Evolution improves base stats significantly",
                "Some evolutions unlock new ability slots",
                "Certain Strays require special items or conditions to evolve",
                "Evolved forms cannot be reversed"
            },
            Type = TutorialType.OneTime,
            Trigger = TutorialTrigger.FirstEvolution,
            Category = "Strays",
            Priority = 80,
            IconColor = Color.Gold
        });

        // Exploration tutorials
        Register(new TutorialEntry
        {
            Id = "tut_dungeon",
            Title = "Entering a Dungeon",
            Content = "Dungeons are multi-room challenges with increasing difficulty and valuable rewards.",
            Tips = new List<string>
            {
                "Choose your difficulty carefully - it affects rewards and enemy strength",
                "Your party's HP persists between rooms",
                "Rest rooms appear periodically to let you recover",
                "The final room always contains a boss"
            },
            Type = TutorialType.OneTime,
            Trigger = TutorialTrigger.FirstDungeon,
            Category = "Exploration",
            Priority = 75,
            IconColor = Color.Purple
        });

        Register(new TutorialEntry
        {
            Id = "tut_new_biome",
            Title = "New Area Discovered",
            Content = "Each biome has unique characteristics, native Strays, and environmental conditions.",
            Tips = new List<string>
            {
                "Check the recommended level range before exploring",
                "Weather effects can help or hinder you in combat",
                "Some biomes have environmental hazards",
                "Native Strays have adapted to their biome's conditions"
            },
            Type = TutorialType.Contextual,
            Trigger = TutorialTrigger.NewBiome,
            Category = "Exploration",
            IconColor = Color.DodgerBlue
        });

        // System tutorials
        Register(new TutorialEntry
        {
            Id = "tut_shop",
            Title = "Welcome to the Shop",
            Content = "Shops sell items, microchips, and augmentations. Your faction reputation affects prices.",
            Tips = new List<string>
            {
                "Higher reputation = better prices",
                "Some items are only available at high reputation",
                "You can sell items you don't need",
                "Shop inventory refreshes after story progression"
            },
            Type = TutorialType.OneTime,
            Trigger = TutorialTrigger.FirstShop,
            Category = "System",
            Priority = 70,
            IconColor = Color.Yellow
        });

        Register(new TutorialEntry
        {
            Id = "tut_quests",
            Title = "Quest Received",
            Content = "Quests guide your journey and provide rewards upon completion.",
            Tips = new List<string>
            {
                "Main quests advance the story",
                "Side quests provide extra rewards and lore",
                "Check the quest log to track your progress",
                "Some quests have time-sensitive objectives"
            },
            Type = TutorialType.OneTime,
            Trigger = TutorialTrigger.QuestReceived,
            Category = "System",
            Priority = 65,
            IconColor = Color.Goldenrod
        });

        Register(new TutorialEntry
        {
            Id = "tut_low_health",
            Title = "Warning: Low Health",
            Content = "Your party's health is critical! Consider using healing items or abilities.",
            Tips = new List<string>
            {
                "Healing items can be used from the inventory",
                "Some abilities heal party members",
                "Rest rooms in dungeons restore some HP",
                "Retreating from a dungeon saves your progress rewards"
            },
            Type = TutorialType.Contextual,
            Trigger = TutorialTrigger.LowHealth,
            Category = "Combat",
            IconColor = Color.Red
        });

        // Help topics (always available)
        Register(new TutorialEntry
        {
            Id = "help_elements",
            Title = "Elements & Weaknesses",
            Content = "Different elements have strengths and weaknesses against each other.",
            Tips = new List<string>
            {
                "Electric > Kinetic > Toxic > Electric",
                "Fire > Ice > Fire",
                "Psionic and Corruption are effective against each other",
                "Weather can modify elemental damage"
            },
            Type = TutorialType.HelpTopic,
            Category = "Combat",
            IconColor = Color.Magenta
        });

        Register(new TutorialEntry
        {
            Id = "help_microchips",
            Title = "Microchip Guide",
            Content = "Microchips grant abilities and stat bonuses to your Strays.",
            Tips = new List<string>
            {
                "Protocol chips: Passive stat bonuses",
                "Element chips: Elemental damage abilities",
                "Augment chips: Special enhancements",
                "Driver chips: Core stat improvements",
                "Daemon chips: Summoned effects",
                "Support chips: Healing and buffs"
            },
            Type = TutorialType.HelpTopic,
            Category = "Equipment",
            IconColor = Color.Cyan
        });

        Register(new TutorialEntry
        {
            Id = "help_factions",
            Title = "Factions Guide",
            Content = "Your reputation with different factions affects gameplay.",
            Tips = new List<string>
            {
                "NIMDOK: The central AI system",
                "Independents: Survivor communities",
                "Machinists: Tech specialists",
                "Strays: Wild creature collective",
                "Actions and choices affect reputation"
            },
            Type = TutorialType.HelpTopic,
            Category = "World",
            IconColor = Color.Orange
        });

        // Loading tips
        var loadingTips = new[]
        {
            "Defending in combat regenerates Energy Points faster.",
            "Some Strays can only be found in specific weather conditions.",
            "The Archive Scar holds secrets for those brave enough to seek them.",
            "Bandit's Gravitation grows stronger... and more unstable.",
            "Every choice matters. The wasteland remembers.",
            "Microchips level up through use. Practice makes perfect.",
            "Higher rarity augmentations can trigger special evolutions.",
            "The Diadem has three empty slots. What fills them?",
            "NIMDOK is always watching. Or is it?",
            "Some endings can only be achieved through specific choices."
        };

        for (int i = 0; i < loadingTips.Length; i++)
        {
            Register(new TutorialEntry
            {
                Id = $"tip_loading_{i}",
                Title = "Tip",
                Content = loadingTips[i],
                Type = TutorialType.LoadingTip,
                Category = "Tips"
            });
        }
    }

    /// <summary>
    /// Registers a tutorial entry.
    /// </summary>
    private static void Register(TutorialEntry entry)
    {
        _entries[entry.Id] = entry;
    }

    /// <summary>
    /// Gets all tutorials by category.
    /// </summary>
    public static IEnumerable<TutorialEntry> GetByCategory(string category)
    {
        return _entries.Values.Where(e => e.Category == category && e.Type == TutorialType.HelpTopic);
    }

    /// <summary>
    /// Gets a random loading tip.
    /// </summary>
    public static TutorialEntry GetRandomLoadingTip()
    {
        var tips = _entries.Values.Where(e => e.Type == TutorialType.LoadingTip).ToList();
        return tips[new Random().Next(tips.Count)];
    }

    /// <summary>
    /// Triggers tutorials by trigger type.
    /// </summary>
    public void TriggerTutorial(TutorialTrigger trigger)
    {
        if (!TutorialsEnabled)
        {
            return;
        }

        var tutorials = _entries.Values
            .Where(e => e.Trigger == trigger)
            .Where(e => e.Type == TutorialType.OneTime ? !_seenTutorials.Contains(e.Id) : HintsEnabled)
            .OrderByDescending(e => e.Priority);

        foreach (var tutorial in tutorials)
        {
            QueueTutorial(tutorial);
        }
    }

    /// <summary>
    /// Shows a specific tutorial by ID.
    /// </summary>
    public void ShowTutorial(string id)
    {
        if (_entries.TryGetValue(id, out var tutorial))
        {
            QueueTutorial(tutorial);
        }
    }

    /// <summary>
    /// Queues a tutorial for display.
    /// </summary>
    private void QueueTutorial(TutorialEntry tutorial)
    {
        if (!_pendingTutorials.Contains(tutorial))
        {
            _pendingTutorials.Enqueue(tutorial);
        }

        // If nothing currently displayed, start showing
        if (DisplayState.CurrentTutorial == null)
        {
            ShowNextTutorial();
        }
    }

    /// <summary>
    /// Shows the next queued tutorial.
    /// </summary>
    private void ShowNextTutorial()
    {
        if (_pendingTutorials.Count == 0)
        {
            DisplayState.CurrentTutorial = null;
            return;
        }

        var tutorial = _pendingTutorials.Dequeue();
        DisplayState.CurrentTutorial = tutorial;
        DisplayState.DisplayTimer = 0f;
        DisplayState.IsFadingIn = true;
        DisplayState.FadeProgress = 0f;
        DisplayState.WaitingForDismiss = tutorial.Type != TutorialType.Contextual;

        if (tutorial.Type == TutorialType.OneTime)
        {
            _seenTutorials.Add(tutorial.Id);
        }

        TutorialShown?.Invoke(this, tutorial);
    }

    /// <summary>
    /// Dismisses the current tutorial.
    /// </summary>
    public void Dismiss()
    {
        if (DisplayState.CurrentTutorial == null)
        {
            return;
        }

        var tutorial = DisplayState.CurrentTutorial;
        DisplayState.IsFadingIn = false;
        DisplayState.FadeProgress = 1f;

        TutorialDismissed?.Invoke(this, tutorial);
    }

    /// <summary>
    /// Updates the tutorial system.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (DisplayState.CurrentTutorial == null)
        {
            return;
        }

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        DisplayState.DisplayTimer += deltaTime;

        // Handle fading
        if (DisplayState.IsFadingIn)
        {
            DisplayState.FadeProgress = Math.Min(1f, DisplayState.FadeProgress + deltaTime * 4f);
        }
        else
        {
            DisplayState.FadeProgress = Math.Max(0f, DisplayState.FadeProgress - deltaTime * 4f);

            if (DisplayState.FadeProgress <= 0f)
            {
                ShowNextTutorial();
            }
        }

        // Auto-dismiss contextual hints after a delay
        if (DisplayState.CurrentTutorial.Type == TutorialType.Contextual && DisplayState.DisplayTimer > 5f)
        {
            Dismiss();
        }
    }

    /// <summary>
    /// Marks a tutorial as seen (won't show again if one-time).
    /// </summary>
    public void MarkSeen(string id)
    {
        _seenTutorials.Add(id);
    }

    /// <summary>
    /// Resets all seen tutorials.
    /// </summary>
    public void ResetSeen()
    {
        _seenTutorials.Clear();
    }

    /// <summary>
    /// Exports seen tutorials for saving.
    /// </summary>
    public HashSet<string> ExportSeen()
    {
        return new HashSet<string>(_seenTutorials);
    }

    /// <summary>
    /// Imports seen tutorials from save data.
    /// </summary>
    public void ImportSeen(IEnumerable<string> seen)
    {
        _seenTutorials.Clear();
        foreach (var id in seen)
        {
            _seenTutorials.Add(id);
        }
    }

    /// <summary>
    /// Draws the tutorial popup.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, Rectangle screenBounds)
    {
        if (DisplayState.CurrentTutorial == null || DisplayState.FadeProgress <= 0f)
        {
            return;
        }

        var tutorial = DisplayState.CurrentTutorial;
        float alpha = DisplayState.FadeProgress;

        // Calculate popup dimensions
        int popupWidth = 500;
        int popupHeight = 200 + tutorial.Tips.Count * 25;

        if (tutorial.Controls.Count > 0)
        {
            popupHeight += 40 + tutorial.Controls.Count * 20;
        }

        int popupX = screenBounds.Width / 2 - popupWidth / 2;
        int popupY = screenBounds.Height / 2 - popupHeight / 2;

        var popupRect = new Rectangle(popupX, popupY, popupWidth, popupHeight);

        // Dim background
        spriteBatch.Draw(pixel, screenBounds, Color.Black * 0.6f * alpha);

        // Popup background
        spriteBatch.Draw(pixel, popupRect, new Color(20, 25, 35) * alpha);

        // Border
        var borderColor = tutorial.IconColor * alpha;
        spriteBatch.Draw(pixel, new Rectangle(popupRect.X, popupRect.Y, popupRect.Width, 3), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(popupRect.X, popupRect.Bottom - 3, popupRect.Width, 3), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(popupRect.X, popupRect.Y, 3, popupRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(popupRect.Right - 3, popupRect.Y, 3, popupRect.Height), borderColor);

        // Icon
        var iconRect = new Rectangle(popupX + 15, popupY + 15, 30, 30);
        spriteBatch.Draw(pixel, iconRect, tutorial.IconColor * alpha);

        // Title
        spriteBatch.DrawString(font, tutorial.Title, new Vector2(popupX + 55, popupY + 15), tutorial.IconColor * alpha);

        // Category badge
        var categoryText = $"[{tutorial.Category}]";
        var categorySize = font.MeasureString(categoryText);
        spriteBatch.DrawString(font, categoryText, new Vector2(popupRect.Right - categorySize.X - 15, popupY + 15), Color.Gray * alpha);

        // Content
        DrawWrappedText(spriteBatch, font, tutorial.Content, new Vector2(popupX + 15, popupY + 55), popupWidth - 30, Color.White * alpha);

        // Tips
        int tipY = popupY + 100;
        foreach (var tip in tutorial.Tips)
        {
            spriteBatch.DrawString(font, $"• {tip}", new Vector2(popupX + 25, tipY), Color.LightGray * alpha);
            tipY += 22;
        }

        // Controls
        if (tutorial.Controls.Count > 0)
        {
            tipY += 15;
            spriteBatch.DrawString(font, "Controls:", new Vector2(popupX + 15, tipY), Color.Cyan * alpha);
            tipY += 25;

            foreach (var kvp in tutorial.Controls)
            {
                spriteBatch.DrawString(font, $"[{kvp.Key}] {kvp.Value}", new Vector2(popupX + 25, tipY), Color.LightGray * alpha);
                tipY += 20;
            }
        }

        // Dismiss hint
        if (DisplayState.WaitingForDismiss)
        {
            float blink = (float)Math.Sin(DisplayState.DisplayTimer * 4) * 0.3f + 0.7f;
            var dismissText = "Press any key to continue";
            var dismissSize = font.MeasureString(dismissText);
            spriteBatch.DrawString(font, dismissText, new Vector2(popupRect.Center.X - dismissSize.X / 2, popupRect.Bottom - 30), Color.Yellow * alpha * blink);
        }
    }

    /// <summary>
    /// Draws wrapped text.
    /// </summary>
    private void DrawWrappedText(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, int maxWidth, Color color)
    {
        var words = text.Split(' ');
        string currentLine = "";
        float y = position.Y;

        foreach (var word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var size = font.MeasureString(testLine);

            if (size.X > maxWidth)
            {
                spriteBatch.DrawString(font, currentLine, new Vector2(position.X, y), color);
                y += size.Y;
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            spriteBatch.DrawString(font, currentLine, new Vector2(position.X, y), color);
        }
    }
}
