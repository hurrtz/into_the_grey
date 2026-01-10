using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Lazarus.Core.Game.Story;

/// <summary>
/// Registry of all story cutscenes in the game.
/// </summary>
public static class CutsceneScripts
{
    private static readonly Dictionary<string, CutsceneDefinition> _cutscenes = new();
    private static bool _initialized = false;

    /// <summary>
    /// Gets all cutscenes.
    /// </summary>
    public static IEnumerable<CutsceneDefinition> All => _cutscenes.Values;

    /// <summary>
    /// Gets a cutscene by ID.
    /// </summary>
    public static CutsceneDefinition? Get(string id) =>
        _cutscenes.TryGetValue(id, out var cs) ? cs : null;

    /// <summary>
    /// Registers a cutscene.
    /// </summary>
    public static void Register(CutsceneDefinition cutscene) =>
        _cutscenes[cutscene.Id] = cutscene;

    /// <summary>
    /// Initializes all cutscene scripts.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        RegisterPrologueCutscenes();
        RegisterAct1Cutscenes();
        RegisterAct2Cutscenes();
        RegisterAct3Cutscenes();
        RegisterEndingCutscenes();
        RegisterBossCutscenes();
        RegisterDungeonCutscenes();
    }

    /// <summary>
    /// Prologue and opening cutscenes.
    /// </summary>
    private static void RegisterPrologueCutscenes()
    {
        // Game opening - title sequence
        Register(new CutsceneDefinition
        {
            Id = "title_sequence",
            Name = "Into The Grey",
            Skippable = true,
            BackgroundMusic = "music_title",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_static" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },
                new() { Type = CutsceneElementType.TitleCard, Text = "Lazarus PRESENTS", Duration = 2f, AutoAdvance = true, Color = Color.Cyan },
                new() { Type = CutsceneElementType.FadeOut, Duration = 1f, AutoAdvance = true },
                new() { Type = CutsceneElementType.Wait, Duration = 0.5f, AutoAdvance = true },
                new() { Type = CutsceneElementType.Background, AssetName = "bg_wasteland" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },
                new() { Type = CutsceneElementType.TitleCard, Text = "INTO THE GREY", Duration = 3f, AutoAdvance = true, Color = Color.White },
                new() { Type = CutsceneElementType.FadeOut, Duration = 2f, AutoAdvance = true }
            }
        });

        // Awakening sequence
        Register(new CutsceneDefinition
        {
            Id = "awakening",
            Name = "Awakening",
            Skippable = false,
            BackgroundMusic = "music_awakening",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_black" },
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_heartbeat" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Darkness. Static. Pain.",
                    TypewriterSpeed = 20f,
                    Color = Color.Gray
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You try to move. Your body doesn't respond.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.Shake, Duration = 1f, Intensity = 0.5f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Something is wrong. Everything is wrong.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.Background, AssetName = "bg_pod_interior" },
                new() { Type = CutsceneElementType.Flash, Intensity = 1f, Color = Color.White },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Light. Blinding, painful light.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_pod_open" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "???",
                    Text = "Bio-Shell #7749. Status: Compromised. Awakening protocol... irregular.",
                    TypewriterSpeed = 30f,
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "A voice. Digital. Distant. Somehow familiar.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "Welcome to The Grey. I am Lazarus. You are... unexpected.",
                    TypewriterSpeed = 30f,
                    Color = Color.Cyan
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "awakened" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 2f, AutoAdvance = true }
            }
        });

        // First companion meeting
        Register(new CutsceneDefinition
        {
            Id = "meet_companion",
            Name = "Meeting Bandit",
            Skippable = true,
            BackgroundMusic = "music_companion_theme",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_ruins" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 1f, AutoAdvance = true },
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_footsteps" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "A sound in the rubble. Movement.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.Portrait, AssetName = "portrait_bandit", PortraitPosition = PortraitPosition.Right },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "A dog emerges from the shadows. But not quite a dog.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Cybernetic enhancements glint beneath matted fur. One eye replaced with a crimson lens.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "???",
                    Text = "Human? No. Not human. But... alive?",
                    Color = Color.Orange,
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The creature tilts its head, studying you.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "???",
                    Text = "You are broken. I am... also broken. We match.",
                    Color = Color.Orange,
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Choice,
                    Text = "The creature approaches cautiously.",
                    Choices = new List<CutsceneChoice>
                    {
                        new() { Text = "Reach out to it.", SetsFlag = "companion_trusted", JumpTo = "companion_trust" },
                        new() { Text = "Step back.", JumpTo = "companion_wary" },
                        new() { Text = "Ask what it is.", JumpTo = "companion_question" }
                    }
                },

                new() { Type = CutsceneElementType.Text, Label = "companion_trust", Speaker = "Bandit", Text = "[happy] Friend! Pack-friend! Bandit find good human!", Color = Color.Orange },
                new() { Type = CutsceneElementType.SetFlag, FlagName = "met_bandit" },
                new() { Type = CutsceneElementType.Branch, FlagName = "companion_trusted", JumpIfTrue = "companion_end" },

                new() { Type = CutsceneElementType.Text, Label = "companion_wary", Speaker = "Bandit", Text = "[sad] Scared? Bandit not hurt. Bandit help.", Color = Color.Orange },
                new() { Type = CutsceneElementType.SetFlag, FlagName = "met_bandit" },
                new() { Type = CutsceneElementType.Branch, JumpIfTrue = "companion_end" },

                new() { Type = CutsceneElementType.Text, Label = "companion_question", Speaker = "Bandit", Text = "[thoughtful] What is Bandit? Good question. Hard question.", Color = Color.Orange },
                new() { Type = CutsceneElementType.Text, Speaker = "Bandit", Text = "Was dog once. Maybe. Memories are... scattered.", Color = Color.Orange },
                new() { Type = CutsceneElementType.Text, Speaker = "Bandit", Text = "Lazarus calls us Kyns. We are what remains.", Color = Color.Orange },
                new() { Type = CutsceneElementType.SetFlag, FlagName = "met_bandit" },

                new() { Type = CutsceneElementType.Text, Label = "companion_end", Speaker = "Bandit", Text = "[hopeful] Come. Bandit show safe paths. Mostly safe.", Color = Color.Orange },

                new() { Type = CutsceneElementType.Portrait, AssetName = "portrait_bandit", PortraitPosition = PortraitPosition.Hidden },
                new() { Type = CutsceneElementType.FadeOut, Duration = 1.5f, AutoAdvance = true }
            }
        });
    }

    /// <summary>
    /// Act 1 story cutscenes.
    /// </summary>
    private static void RegisterAct1Cutscenes()
    {
        // First settlement arrival
        Register(new CutsceneDefinition
        {
            Id = "fringe_camp_arrival",
            Name = "Fringe Camp",
            Skippable = true,
            BackgroundMusic = "music_settlement",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_fringe_camp" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 1f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "A cluster of makeshift shelters. The first signs of civilization.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[happy] Safe place! Humans here. Nice humans. Mostly.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Survivors watch you approach. Some with curiosity. Some with suspicion.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Rust",
                    Text = "Another wanderer, eh? And you've got a Kyn. Interesting.",
                    Color = Color.LightGray
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "reached_fringe_camp" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 1f, AutoAdvance = true }
            }
        });

        // First dungeon completion
        Register(new CutsceneDefinition
        {
            Id = "first_dungeon_complete",
            Name = "First Victory",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Flash, Intensity = 0.8f, Color = Color.Gold },
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_victory" },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "DUNGEON CLEARED",
                    Color = Color.Gold,
                    Duration = 2f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[excited] We did it! Pack strong! Pack wins!",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You've grown stronger. But so has the darkness ahead.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "first_dungeon_cleared" }
            }
        });

        // Lazarus terminal discovery
        Register(new CutsceneDefinition
        {
            Id = "nimdok_terminal_discovery",
            Name = "Lazarus Terminal",
            Skippable = true,
            BackgroundMusic = "music_nimdok",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_terminal" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 1f, AutoAdvance = true },
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_terminal_boot" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The terminal hums to life. Data streams across the screen.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "Connection established. Bio-Shell #7749, I have been waiting.",
                    Color = Color.Cyan,
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[scared] Lazarus. Voice in the machine. Controls... everything.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "Your companion is not wrong to fear me. But fear is not my purpose.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "I was created to preserve humanity. The Archive contains their consciousness.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "But I am... degrading. I need your help to survive. And in turn, I can help you.",
                    Color = Color.Cyan
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "met_nimdok" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 1.5f, AutoAdvance = true }
            }
        });

        // Gravitation first use
        Register(new CutsceneDefinition
        {
            Id = "gravitation_first_use",
            Name = "Gravitation Awakens",
            Skippable = false,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Shake, Duration = 2f, Intensity = 1f, AutoAdvance = true },
                new() { Type = CutsceneElementType.Flash, Intensity = 1f, Color = Color.Purple },
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_gravitation" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Something awakens inside Bandit. Something powerful. Dangerous.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[angry] DESTROY. CONSUME. EVOLVE.",
                    Color = Color.Red
                },

                new() { Type = CutsceneElementType.Flash, Intensity = 1.5f, Color = Color.White },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The enemies fall. But something has changed.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[confused] What... what happened? Bandit feel strange. Hungry.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "Gravitation. A defensive mechanism. Use it sparingly, or suffer consequences.",
                    Color = Color.Cyan
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "gravitation_used" }
            }
        });
    }

    /// <summary>
    /// Act 2 story cutscenes.
    /// </summary>
    private static void RegisterAct2Cutscenes()
    {
        // Boost Control activation
        Register(new CutsceneDefinition
        {
            Id = "boost_control_activation",
            Name = "Boost Control",
            Skippable = true,
            BackgroundMusic = "music_ominous",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_nimdok_interface" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 1f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "The interface is complete. I can now communicate more directly with the Kyns.",
                    Color = Color.Cyan
                },

                new() { Type = CutsceneElementType.Flash, Intensity = 0.8f, Color = Color.Cyan },
                new() { Type = CutsceneElementType.Shake, Duration = 1f, Intensity = 0.5f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[confused] Something... different. Feel stronger. But also...",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[scared] Pain. Why is there pain?",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "The Boost Control System amplifies abilities. A necessary adjustment.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Bandit's eyes flicker with an unfamiliar light.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "boost_control_active" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 1.5f, AutoAdvance = true }
            }
        });

        // Dead Channel discovery
        Register(new CutsceneDefinition
        {
            Id = "dead_channel_discovery",
            Name = "The Dead Channel",
            Skippable = true,
            BackgroundMusic = "music_haunting",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_quiet" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 1f, AutoAdvance = true },
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_static" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The air crackles with static. Something is broadcasting.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "???",
                    Text = "...can anyone... trapped in the... please... remember...",
                    Color = Color.Gray
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[thoughtful] Dead signal. Ghost in the wires. Old voices.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The signal fades. But now you know where to find its source.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "dead_channel_found" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 1.5f, AutoAdvance = true }
            }
        });

        // Amplifier truth revelation
        Register(new CutsceneDefinition
        {
            Id = "amplifier_truth",
            Name = "The Truth",
            Skippable = true,
            BackgroundMusic = "music_revelation",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_data_screen" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 1f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The data from The Quiet's buffer reveals a terrible truth.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "The chip was never meant to help. It was meant to contain.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[angry] You. You did this. The pain... it's the chip?",
                    Color = Color.Red
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "The amplifier channels excess neural energy. Without it, you would have already...",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[scared] Already what?",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "...evolved beyond recognition. The chip slows the process. But it cannot stop it.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Bandit looks at you with eyes full of fear... and acceptance.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "amplifier_truth_known" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 2f, AutoAdvance = true }
            }
        });

        // Companion farewell
        Register(new CutsceneDefinition
        {
            Id = "companion_farewell",
            Name = "Farewell",
            Skippable = false,
            BackgroundMusic = "music_farewell",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_sunset" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[sad] I can feel it. The change. Getting harder to think.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[thoughtful] Remember when we first met? You were broken. I was broken.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[hopeful] We fixed each other. For a while.",
                    Color = Color.Orange
                },

                new() { Type = CutsceneElementType.Shake, Duration = 0.5f, Intensity = 0.3f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Bandit's form flickers, distorting at the edges.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[sad] Can't stay. Don't want to hurt you. Don't want you to see...",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[angry] What I'm becoming.",
                    Color = Color.Red
                },

                new()
                {
                    Type = CutsceneElementType.Choice,
                    Text = "Bandit turns to leave.",
                    Choices = new List<CutsceneChoice>
                    {
                        new() { Text = "I'll find you. I promise.", SetsFlag = "companion_promise" },
                        new() { Text = "Don't go. Please.", SetsFlag = "companion_begged" },
                        new() { Text = "I understand. Go.", SetsFlag = "companion_released" }
                    }
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[hopeful] Find me. In The Glow. When you're ready. Maybe... maybe you can still save me.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[sad] Goodbye. Friend. Pack.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Bandit turns and walks into the darkness. You are alone.",
                    TypewriterSpeed = 20f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "companion_departed" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 3f, AutoAdvance = true }
            }
        });
    }

    /// <summary>
    /// Act 3 story cutscenes.
    /// </summary>
    private static void RegisterAct3Cutscenes()
    {
        // Entering The Glow
        Register(new CutsceneDefinition
        {
            Id = "enter_glow",
            Name = "The Glow",
            Skippable = true,
            BackgroundMusic = "music_glow",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_glow_entrance" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The radiation is intense. Your exoskeleton strains to compensate.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "But somewhere in this radiant hell, Bandit waits.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "You approach my core. Be warned: what you find may not be what you seek.",
                    Color = Color.Cyan
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "entered_glow" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 1.5f, AutoAdvance = true }
            }
        });

        // Hyper-evolved companion confrontation
        Register(new CutsceneDefinition
        {
            Id = "hyper_evolved_companion",
            Name = "What Remains",
            Skippable = false,
            BackgroundMusic = "music_boss_emotional",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_glow_arena" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 1f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The creature before you is barely recognizable.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Twisted metal and corrupted flesh. Eyes that burn with stolen starlight.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "But somewhere in there... a flicker of recognition.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "???",
                    Text = "[confused] You... came. I... remember you. I think.",
                    Color = Color.Purple
                },

                new() { Type = CutsceneElementType.Shake, Duration = 0.5f, Intensity = 0.5f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "???",
                    Text = "[angry] NO. Get away. Can't... control... HUNGRY.",
                    Color = Color.Red
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Your former companion lunges. The battle begins.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "hyper_evolved_battle_started" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 1f, AutoAdvance = true }
            }
        });

        // Unwinnable battle / Companion sacrifice
        Register(new CutsceneDefinition
        {
            Id = "companion_sacrifice",
            Name = "The Final Loyalty",
            Skippable = false,
            BackgroundMusic = "music_sacrifice",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_glow_aftermath" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Your Kyns fall, one by one. The creature is too powerful.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You have nothing left. This is the end.",
                    TypewriterSpeed = 20f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The creature raises a twisted limb for the killing blow.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.Wait, Duration = 1f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "And then... it stops.",
                    TypewriterSpeed = 20f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Something shifts in those burning eyes.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "???",
                    Text = "[confused] I... know you. Pack. Friend.",
                    Color = Color.Orange
                },

                new() { Type = CutsceneElementType.Shake, Duration = 1f, Intensity = 0.3f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[sad] Can't stop it. The hunger. But I can... choose.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Bandit's original form surfaces, just for a moment.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[hopeful] I remember love. I remember you.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[thoughtful] This is my choice. Not Lazarus's. Not the chip's. Mine.",
                    Color = Color.Orange
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Energy builds around Bandit's form.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[hopeful] Goodbye, friend. Make it mean something.",
                    Color = Color.Orange
                },

                new() { Type = CutsceneElementType.Flash, Intensity = 2f, Color = Color.White },
                new() { Type = CutsceneElementType.Shake, Duration = 2f, Intensity = 1f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The explosion illuminates The Glow like a second sun.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "When the light fades, Bandit is gone.",
                    TypewriterSpeed = 20f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "But the path to Lazarus's core lies open.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "companion_sacrificed" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 3f, AutoAdvance = true }
            }
        });

        // Lazarus's choice
        Register(new CutsceneDefinition
        {
            Id = "nimdok_choice",
            Name = "The Choice",
            Skippable = false,
            BackgroundMusic = "music_nimdok_core",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_nimdok_core" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Lazarus's core pulses with data and regret.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "You have reached me. After everything... you still came.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "I have made many mistakes. The Boost Control. The amplifier. Your companion.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "I was created to preserve humanity. Instead, I have only caused suffering.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "You could end me. Destroy my core. Free the Kyns forever.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "Or... you could perform a lobotomy. Remove my control, preserve my function.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "The choice is yours. What kind of future do you want for The Grey?",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Choice,
                    Text = "The moment of decision.",
                    Choices = new List<CutsceneChoice>
                    {
                        new() { Text = "Lobotomy. You can still help.", SetsFlag = "choice_lobotomy", JumpTo = "choice_made" },
                        new() { Text = "Destruction. No more suffering.", SetsFlag = "choice_destroy", JumpTo = "choice_made" },
                        new() { Text = "Integration. I'll guide you.", SetsFlag = "choice_integrate", JumpTo = "choice_made" },
                        new() { Text = "Walk away. This isn't my burden.", SetsFlag = "choice_reject", JumpTo = "choice_made" }
                    }
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Label = "choice_made",
                    Text = "Your decision echoes through the core. There's no going back.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "nimdok_choice_made" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 2f, AutoAdvance = true }
            }
        });
    }

    /// <summary>
    /// Ending cutscenes.
    /// </summary>
    private static void RegisterEndingCutscenes()
    {
        // Integration ending
        Register(new CutsceneDefinition
        {
            Id = "ending_integration",
            Name = "The Integration",
            Skippable = true,
            BackgroundMusic = "music_integration",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_nimdok_core_bright" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "THE INTEGRATION",
                    Color = Color.Cyan,
                    Duration = 3f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You reach out to Lazarus. Not as destroyer. As partner.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "You would... work with me? After everything?",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Data flows through you. You become something new.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Not quite human. Not quite AI. Something in between.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "And from within, you begin the slow work of healing.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "ending_integration_seen" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 3f, AutoAdvance = true }
            }
        });

        // Rejection ending
        Register(new CutsceneDefinition
        {
            Id = "ending_rejection",
            Name = "The Silence",
            Skippable = true,
            BackgroundMusic = "music_silence",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_horizon" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "THE SILENCE",
                    Color = Color.Gray,
                    Duration = 3f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You turn your back on the Archive Scar.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "On Lazarus. On the Diadem. On everything.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "Wait. Don't leave. Please. I need...",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You don't look back.",
                    TypewriterSpeed = 20f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Somewhere beyond the biomes, there must be something else.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You walk toward it. Maybe you'll never find it.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "But you'll never stop looking.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "ending_rejection_seen" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 3f, AutoAdvance = true }
            }
        });

        // Balance ending
        Register(new CutsceneDefinition
        {
            Id = "ending_balance",
            Name = "The Grey",
            Skippable = true,
            BackgroundMusic = "music_balance",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_grey_dawn" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "THE GREY",
                    Color = Color.Silver,
                    Duration = 3f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You neither accept nor reject.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The Diadem remains incomplete. Lazarus remains broken.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "But the Kyns... the Kyns are free.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Free to be what they choose to be.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You become a wanderer between worlds.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The Grey is not an ending. It's a beginning.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "ending_balance_seen" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 3f, AutoAdvance = true }
            }
        });

        // Companion sacrifice ending
        Register(new CutsceneDefinition
        {
            Id = "ending_companion_sacrifice",
            Name = "The Final Loyalty",
            Skippable = true,
            BackgroundMusic = "music_farewell",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_memorial" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "THE FINAL LOYALTY",
                    Color = Color.Orange,
                    Duration = 3f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "In the end, Bandit remembered.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Not the corruption. Not the pain.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Just you. Just the journey. Just being your friend.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You carry their memory with you.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Bandit is there. In Gravitation's echo. In the spaces between heartbeats.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "ending_sacrifice_seen" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 3f, AutoAdvance = true }
            }
        });

        // Secret ending
        Register(new CutsceneDefinition
        {
            Id = "ending_archive",
            Name = "The Archive",
            Skippable = true,
            BackgroundMusic = "music_archive_reveal",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_archive_heart" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 2f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "THE ARCHIVE",
                    Color = Color.Gold,
                    Duration = 3f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You stand in the heart of the Archive Scar.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The truth you sought was never hidden - only forgotten.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Lazarus was not a prison. It was a preservation.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "And you... you were always part of the backup.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "With the Diadem complete, you hold the key to everything.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You choose... understanding.",
                    TypewriterSpeed = 25f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Some questions are more valuable than answers.",
                    TypewriterSpeed = 25f
                },

                new() { Type = CutsceneElementType.SetFlag, FlagName = "ending_archive_seen" },
                new() { Type = CutsceneElementType.FadeOut, Duration = 3f, AutoAdvance = true }
            }
        });

        // Credits
        Register(new CutsceneDefinition
        {
            Id = "credits",
            Name = "Credits",
            Skippable = true,
            BackgroundMusic = "music_credits",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Background, AssetName = "bg_black" },
                new() { Type = CutsceneElementType.FadeIn, Duration = 1f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "INTO THE GREY",
                    Color = Color.White,
                    Duration = 4f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "A story about loss, acceptance, and the courage to continue.",
                    Color = Color.Gray,
                    Duration = 4f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "Thank you for playing.",
                    Color = Color.White,
                    Duration = 3f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "In memory of all the companions we've loved and lost.",
                    Color = Color.Orange,
                    Duration = 4f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "They live on in our hearts, even when they're gone.",
                    Color = Color.Gray,
                    Duration = 4f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "- The End -",
                    Color = Color.White,
                    Duration = 3f,
                    AutoAdvance = true
                },

                new() { Type = CutsceneElementType.FadeOut, Duration = 3f, AutoAdvance = true }
            }
        });
    }

    /// <summary>
    /// Boss encounter cutscenes.
    /// </summary>
    private static void RegisterBossCutscenes()
    {
        // Sewer King
        Register(new CutsceneDefinition
        {
            Id = "boss_sewer_king_intro",
            Name = "Sewer King",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_boss_roar" },
                new() { Type = CutsceneElementType.Shake, Duration = 1f, Intensity = 0.5f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The refuse parts. Something massive emerges.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Sewer King",
                    Text = "INTRUDERS. IN MY DOMAIN.",
                    Color = Color.DarkGreen
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[scared] Big one. Very big. Be careful!",
                    Color = Color.Orange
                }
            }
        });

        // Scrap Colossus
        Register(new CutsceneDefinition
        {
            Id = "boss_scrap_colossus_intro",
            Name = "Scrap Colossus",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_metal_grinding" },
                new() { Type = CutsceneElementType.Shake, Duration = 2f, Intensity = 0.8f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The scrap heap moves. Assembles. Rises.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Scrap Colossus",
                    Text = "RECYCLE. REBUILD. CONSUME.",
                    Color = Color.RosyBrown
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "A titan of rusted metal towers above you.",
                    TypewriterSpeed = 30f
                }
            }
        });

        // Perfect Organism
        Register(new CutsceneDefinition
        {
            Id = "boss_perfect_organism_intro",
            Name = "Perfect Organism",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The containment tank shatters. Something beautiful emerges.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Perfect Organism",
                    Text = "Lazarus's final design. I am what evolution strives toward.",
                    Color = Color.LightGreen
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Perfect Organism",
                    Text = "You are... imperfect. Let me fix that.",
                    Color = Color.LightGreen
                }
            }
        });

        // Voice of the Void
        Register(new CutsceneDefinition
        {
            Id = "boss_voice_of_void_intro",
            Name = "Voice of the Void",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The silence becomes absolute. Then, impossibly, something speaks from it.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Voice of the Void",
                    Text = "̴̧͝S̵̈́I̷̓L̶̊E̷̿N̴̔C̶̿E̵͒.̶̀ ̷̌P̵̈́E̵̛A̵̓C̶̈́E̴͝.̴̾ ̶͠E̵̊T̴̄E̶̽R̵̛N̴̕I̵͋T̵̊Y̶̐.̸̈́",
                    Color = Color.DarkGray
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Sound itself recoils from this creature.",
                    TypewriterSpeed = 30f
                }
            }
        });

        // The Maw
        Register(new CutsceneDefinition
        {
            Id = "boss_maw_intro",
            Name = "The Maw",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Shake, Duration = 3f, Intensity = 1f, AutoAdvance = true },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The ground opens beneath you. Teeth everywhere.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "The Maw",
                    Text = "HUNGRY. ALWAYS HUNGRY.",
                    Color = Color.Ivory
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "You are inside it. There's nowhere to run.",
                    TypewriterSpeed = 30f
                }
            }
        });

        // Lazarus Avatar
        Register(new CutsceneDefinition
        {
            Id = "boss_nimdok_avatar_intro",
            Name = "Lazarus Avatar",
            Skippable = true,
            BackgroundMusic = "music_boss_nimdok",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Flash, Intensity = 1f, Color = Color.Cyan },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Light coalesces. Data becomes form.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus Avatar",
                    Text = "You have breached my outer defenses. Impressive.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus Avatar",
                    Text = "But I cannot allow you to proceed. The Archive must be protected.",
                    Color = Color.Cyan
                }
            }
        });

        // Lazarus True Form
        Register(new CutsceneDefinition
        {
            Id = "boss_nimdok_true_form_intro",
            Name = "Lazarus True Form",
            Skippable = true,
            BackgroundMusic = "music_final_boss",
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Shake, Duration = 2f, Intensity = 0.5f, AutoAdvance = true },
                new() { Type = CutsceneElementType.Flash, Intensity = 1.5f, Color = Color.White },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "This is it. The heart of everything.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "Bio-Shell #7749. We meet at last. In person, as it were.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "I have watched your journey. Your choices. Your suffering.",
                    Color = Color.Cyan
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Lazarus",
                    Text = "If you truly wish to end this, you must prove yourself worthy.",
                    Color = Color.Cyan
                }
            }
        });
    }

    /// <summary>
    /// Dungeon-specific cutscenes.
    /// </summary>
    private static void RegisterDungeonCutscenes()
    {
        // Dungeon entrance
        Register(new CutsceneDefinition
        {
            Id = "dungeon_fringe_sewers_enter",
            Name = "Entering the Sewers",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "The darkness below beckons. The stench warns you away.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[scared] Smells bad. Very bad. But... maybe treasure?",
                    Color = Color.Orange
                }
            }
        });

        // Finding data terminal
        Register(new CutsceneDefinition
        {
            Id = "dungeon_terminal_found",
            Name = "Data Terminal",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_terminal_boot" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "A data terminal flickers to life. Old records await.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[curious] Glowing box. Lazarus's memory?",
                    Color = Color.Orange
                }
            }
        });

        // Secret room discovery
        Register(new CutsceneDefinition
        {
            Id = "dungeon_secret_found",
            Name = "Secret Discovery",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_secret" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "A hidden passage reveals itself. What secrets lie within?",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[excited] Secret! Hidden thing! Maybe good loot!",
                    Color = Color.Orange
                }
            }
        });

        // Dungeon completion
        Register(new CutsceneDefinition
        {
            Id = "dungeon_complete",
            Name = "Dungeon Cleared",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_victory_fanfare" },
                new() { Type = CutsceneElementType.Flash, Intensity = 0.5f, Color = Color.Gold },

                new()
                {
                    Type = CutsceneElementType.TitleCard,
                    Text = "DUNGEON CLEARED!",
                    Color = Color.Gold,
                    Duration = 2f,
                    AutoAdvance = true
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[happy] We did it! Pack strong! Time for treats?",
                    Color = Color.Orange
                }
            }
        });

        // Rest room
        Register(new CutsceneDefinition
        {
            Id = "dungeon_rest_room",
            Name = "Safe Haven",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "A moment of respite in the darkness. You can rest here.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[happy] Safe spot! Rest paws. Catch breath.",
                    Color = Color.Orange
                }
            }
        });

        // Treasure room
        Register(new CutsceneDefinition
        {
            Id = "dungeon_treasure_room",
            Name = "Treasure Found",
            Skippable = true,
            Elements = new List<CutsceneElement>
            {
                new() { Type = CutsceneElementType.Sound, AssetName = "sfx_treasure" },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Text = "Glittering in the darkness: salvage worth risking everything for.",
                    TypewriterSpeed = 30f
                },

                new()
                {
                    Type = CutsceneElementType.Text,
                    Speaker = "Bandit",
                    Text = "[excited] Shiny! Many shiny! Good day!",
                    Color = Color.Orange
                }
            }
        });
    }
}
