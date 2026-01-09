using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Strays.Core.Audio;

/// <summary>
/// Categories of audio.
/// </summary>
public enum AudioCategory
{
    Master,
    Music,
    SoundEffects,
    Ambient,
    Voice,
    UI
}

/// <summary>
/// Music tracks available in the game.
/// </summary>
public enum MusicTrack
{
    None,

    // Menu Music
    MainMenu,
    Settings,

    // Biome Music
    BiomeFringe,
    BiomeRust,
    BiomeGreen,
    BiomeQuiet,
    BiomeTeeth,
    BiomeGlow,
    BiomeArchive,

    // Combat Music
    CombatNormal,
    CombatElite,
    CombatBoss,
    CombatDiadem,
    CombatLiminal,

    // Story Music
    StoryTense,
    StoryEmotional,
    StoryVictory,
    StoryDefeat,

    // Special
    Credits,
    Ending
}

/// <summary>
/// Sound effects available in the game.
/// </summary>
public enum SoundEffect
{
    // UI
    UISelect,
    UIConfirm,
    UICancel,
    UIOpen,
    UIClose,
    UIError,
    UINotification,

    // Combat
    AttackPhysical,
    AttackEnergy,
    AttackFire,
    AttackIce,
    AttackElectric,
    AttackPoison,
    AttackVoid,
    DefendBlock,
    DefendDodge,
    Hit,
    Critical,
    Miss,
    Heal,
    Buff,
    Debuff,
    Death,

    // Movement
    Footstep,
    FootstepMetal,
    FootstepGrass,
    FootstepWater,
    Run,
    Jump,
    Land,

    // Interaction
    ItemPickup,
    ItemDrop,
    ItemUse,
    DoorOpen,
    DoorClose,
    ChestOpen,
    PortalEnter,
    PortalExit,
    ShopBuy,
    ShopSell,

    // Stray
    StrayRecruit,
    StrayEvolution,
    StrayLevelUp,
    StrayCall,
    StrayDismiss,

    // Ambient
    AmbientWind,
    AmbientRain,
    AmbientThunder,
    AmbientMachinery,
    AmbientNature,
    AmbientData,

    // Notifications
    QuestStart,
    QuestComplete,
    QuestObjective,
    Achievement,
    Save,
    Load,

    // Special
    Gravitation,
    Corruption,
    NIMDOK,
    Warning,
    Error
}

/// <summary>
/// Manages all audio playback including music, sound effects, and ambient sounds.
/// </summary>
public class AudioManager
{
    private readonly ContentManager _content;
    private readonly Dictionary<MusicTrack, Song> _music = new();
    private readonly Dictionary<SoundEffect, SoundEffectInstance[]> _soundPools = new();
    private readonly Dictionary<string, SoundEffectInstance> _ambientLoops = new();

    private MusicTrack _currentTrack = MusicTrack.None;
    private MusicTrack _targetTrack = MusicTrack.None;
    private float _musicFadeTime = 0f;
    private const float MUSIC_FADE_DURATION = 1.5f;
    private bool _isFadingOut = false;

    private readonly Dictionary<AudioCategory, float> _volumes = new();
    private readonly Dictionary<AudioCategory, bool> _muted = new();

    private readonly Random _random = new();
    private readonly int _poolSize = 8;

    /// <summary>
    /// Whether the audio system is initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Current music track playing.
    /// </summary>
    public MusicTrack CurrentTrack => _currentTrack;

    /// <summary>
    /// Event fired when a track finishes playing.
    /// </summary>
    public event EventHandler<MusicTrack>? TrackEnded;

    public AudioManager(ContentManager content)
    {
        _content = content;

        // Set default volumes
        _volumes[AudioCategory.Master] = 1.0f;
        _volumes[AudioCategory.Music] = 0.7f;
        _volumes[AudioCategory.SoundEffects] = 0.8f;
        _volumes[AudioCategory.Ambient] = 0.5f;
        _volumes[AudioCategory.Voice] = 1.0f;
        _volumes[AudioCategory.UI] = 0.6f;

        foreach (var category in Enum.GetValues<AudioCategory>())
        {
            _muted[category] = false;
        }
    }

    /// <summary>
    /// Initializes the audio system and loads audio content.
    /// </summary>
    public void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        try
        {
            LoadMusic();
            LoadSoundEffects();
            IsInitialized = true;
        }
        catch (Exception)
        {
            // Audio loading failed, continue without audio
            IsInitialized = false;
        }
    }

    private void LoadMusic()
    {
        // Try to load each music track, skip if not found
        foreach (MusicTrack track in Enum.GetValues<MusicTrack>())
        {
            if (track == MusicTrack.None)
            {
                continue;
            }

            try
            {
                string path = $"Audio/Music/{track}";
                var song = _content.Load<Song>(path);
                _music[track] = song;
            }
            catch
            {
                // Track not found, skip
            }
        }
    }

    private void LoadSoundEffects()
    {
        foreach (SoundEffect sfx in Enum.GetValues<SoundEffect>())
        {
            try
            {
                string path = $"Audio/SFX/{sfx}";
                var effect = _content.Load<Microsoft.Xna.Framework.Audio.SoundEffect>(path);

                // Create a pool of instances
                var pool = new SoundEffectInstance[_poolSize];

                for (int i = 0; i < _poolSize; i++)
                {
                    pool[i] = effect.CreateInstance();
                }

                _soundPools[sfx] = pool;
            }
            catch
            {
                // Sound effect not found, skip
            }
        }
    }

    /// <summary>
    /// Updates the audio manager (handles music fading).
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (!IsInitialized)
        {
            return;
        }

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Handle music fading
        if (_isFadingOut)
        {
            _musicFadeTime += deltaTime;
            float fadeProgress = _musicFadeTime / MUSIC_FADE_DURATION;

            if (fadeProgress >= 1f)
            {
                // Fade out complete, switch track
                MediaPlayer.Stop();
                _isFadingOut = false;
                _musicFadeTime = 0f;
                _currentTrack = MusicTrack.None;

                if (_targetTrack != MusicTrack.None)
                {
                    PlayMusicImmediate(_targetTrack);
                }
            }
            else
            {
                // Apply fade
                float volume = GetEffectiveVolume(AudioCategory.Music) * (1f - fadeProgress);
                MediaPlayer.Volume = volume;
            }
        }
        else if (_musicFadeTime > 0f && _musicFadeTime < MUSIC_FADE_DURATION)
        {
            // Fading in
            _musicFadeTime += deltaTime;
            float fadeProgress = Math.Min(_musicFadeTime / MUSIC_FADE_DURATION, 1f);
            MediaPlayer.Volume = GetEffectiveVolume(AudioCategory.Music) * fadeProgress;
        }

        // Check if current track ended
        if (_currentTrack != MusicTrack.None && MediaPlayer.State == MediaState.Stopped && !_isFadingOut)
        {
            TrackEnded?.Invoke(this, _currentTrack);
        }
    }

    /// <summary>
    /// Plays a music track with optional fade.
    /// </summary>
    public void PlayMusic(MusicTrack track, bool loop = true, bool fade = true)
    {
        if (!IsInitialized || track == _currentTrack)
        {
            return;
        }

        _targetTrack = track;

        if (fade && _currentTrack != MusicTrack.None)
        {
            // Fade out current track
            _isFadingOut = true;
            _musicFadeTime = 0f;
        }
        else
        {
            PlayMusicImmediate(track, loop);
        }
    }

    private void PlayMusicImmediate(MusicTrack track, bool loop = true)
    {
        if (!_music.TryGetValue(track, out var song))
        {
            return;
        }

        try
        {
            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Volume = 0f;
            MediaPlayer.Play(song);

            _currentTrack = track;
            _targetTrack = MusicTrack.None;
            _musicFadeTime = 0.01f; // Start fade in
        }
        catch
        {
            // Playback failed
        }
    }

    /// <summary>
    /// Stops the current music with optional fade.
    /// </summary>
    public void StopMusic(bool fade = true)
    {
        if (!IsInitialized || _currentTrack == MusicTrack.None)
        {
            return;
        }

        _targetTrack = MusicTrack.None;

        if (fade)
        {
            _isFadingOut = true;
            _musicFadeTime = 0f;
        }
        else
        {
            MediaPlayer.Stop();
            _currentTrack = MusicTrack.None;
        }
    }

    /// <summary>
    /// Pauses the current music.
    /// </summary>
    public void PauseMusic()
    {
        if (IsInitialized && MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Pause();
        }
    }

    /// <summary>
    /// Resumes paused music.
    /// </summary>
    public void ResumeMusic()
    {
        if (IsInitialized && MediaPlayer.State == MediaState.Paused)
        {
            MediaPlayer.Resume();
        }
    }

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    public void PlaySound(SoundEffect sound, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        if (!IsInitialized || !_soundPools.TryGetValue(sound, out var pool))
        {
            return;
        }

        var category = GetSoundCategory(sound);

        if (IsMuted(category))
        {
            return;
        }

        // Find an available instance
        foreach (var instance in pool)
        {
            if (instance.State != SoundState.Playing)
            {
                instance.Volume = volume * GetEffectiveVolume(category);
                instance.Pitch = pitch;
                instance.Pan = pan;
                instance.Play();

                return;
            }
        }

        // All instances busy, reuse the first one
        pool[0].Stop();
        pool[0].Volume = volume * GetEffectiveVolume(category);
        pool[0].Pitch = pitch;
        pool[0].Pan = pan;
        pool[0].Play();
    }

    /// <summary>
    /// Plays a sound effect with random pitch variation.
    /// </summary>
    public void PlaySoundVaried(SoundEffect sound, float volume = 1f, float pitchVariation = 0.1f)
    {
        float pitch = (_random.NextSingle() - 0.5f) * 2f * pitchVariation;
        PlaySound(sound, volume, pitch);
    }

    /// <summary>
    /// Plays a positional sound effect (with panning).
    /// </summary>
    public void PlaySound3D(SoundEffect sound, Vector2 soundPosition, Vector2 listenerPosition, float maxDistance = 500f, float volume = 1f)
    {
        var offset = soundPosition - listenerPosition;
        float distance = offset.Length();

        if (distance > maxDistance)
        {
            return;
        }

        // Calculate volume falloff
        float distanceVolume = 1f - (distance / maxDistance);
        distanceVolume = MathHelper.Clamp(distanceVolume, 0f, 1f);

        // Calculate pan
        float pan = 0f;

        if (distance > 1f)
        {
            pan = MathHelper.Clamp(offset.X / maxDistance, -1f, 1f);
        }

        PlaySound(sound, volume * distanceVolume, 0f, pan);
    }

    /// <summary>
    /// Starts an ambient loop.
    /// </summary>
    public void StartAmbient(string ambientId, SoundEffect sound, float volume = 1f)
    {
        if (!IsInitialized || _ambientLoops.ContainsKey(ambientId))
        {
            return;
        }

        if (!_soundPools.TryGetValue(sound, out var pool))
        {
            return;
        }

        // Create a dedicated instance for this ambient loop
        try
        {
            var path = $"Audio/SFX/{sound}";
            var effect = _content.Load<Microsoft.Xna.Framework.Audio.SoundEffect>(path);
            var instance = effect.CreateInstance();
            instance.IsLooped = true;
            instance.Volume = volume * GetEffectiveVolume(AudioCategory.Ambient);
            instance.Play();

            _ambientLoops[ambientId] = instance;
        }
        catch
        {
            // Failed to create ambient loop
        }
    }

    /// <summary>
    /// Stops an ambient loop.
    /// </summary>
    public void StopAmbient(string ambientId, bool immediate = false)
    {
        if (!_ambientLoops.TryGetValue(ambientId, out var instance))
        {
            return;
        }

        if (immediate)
        {
            instance.Stop();
            instance.Dispose();
            _ambientLoops.Remove(ambientId);
        }
        else
        {
            // Fade out would need to be handled in Update
            instance.Stop();
            instance.Dispose();
            _ambientLoops.Remove(ambientId);
        }
    }

    /// <summary>
    /// Stops all ambient loops.
    /// </summary>
    public void StopAllAmbient()
    {
        foreach (var kvp in _ambientLoops)
        {
            kvp.Value.Stop();
            kvp.Value.Dispose();
        }

        _ambientLoops.Clear();
    }

    /// <summary>
    /// Sets the volume for a category.
    /// </summary>
    public void SetVolume(AudioCategory category, float volume)
    {
        _volumes[category] = MathHelper.Clamp(volume, 0f, 1f);
        UpdateVolumes();
    }

    /// <summary>
    /// Gets the volume for a category.
    /// </summary>
    public float GetVolume(AudioCategory category)
    {
        return _volumes.TryGetValue(category, out var volume) ? volume : 1f;
    }

    /// <summary>
    /// Gets the effective volume (accounting for master).
    /// </summary>
    public float GetEffectiveVolume(AudioCategory category)
    {
        if (IsMuted(category) || IsMuted(AudioCategory.Master))
        {
            return 0f;
        }

        float masterVolume = _volumes[AudioCategory.Master];
        float categoryVolume = _volumes.TryGetValue(category, out var vol) ? vol : 1f;

        return masterVolume * categoryVolume;
    }

    /// <summary>
    /// Mutes or unmutes a category.
    /// </summary>
    public void SetMuted(AudioCategory category, bool muted)
    {
        _muted[category] = muted;
        UpdateVolumes();
    }

    /// <summary>
    /// Checks if a category is muted.
    /// </summary>
    public bool IsMuted(AudioCategory category)
    {
        return _muted.TryGetValue(category, out var muted) && muted;
    }

    /// <summary>
    /// Toggles mute for a category.
    /// </summary>
    public void ToggleMute(AudioCategory category)
    {
        SetMuted(category, !IsMuted(category));
    }

    private void UpdateVolumes()
    {
        // Update music volume
        if (_currentTrack != MusicTrack.None && !_isFadingOut)
        {
            MediaPlayer.Volume = GetEffectiveVolume(AudioCategory.Music);
        }

        // Update ambient volumes
        foreach (var kvp in _ambientLoops)
        {
            kvp.Value.Volume = GetEffectiveVolume(AudioCategory.Ambient);
        }
    }

    private AudioCategory GetSoundCategory(SoundEffect sound)
    {
        return sound switch
        {
            SoundEffect.UISelect or SoundEffect.UIConfirm or SoundEffect.UICancel or
            SoundEffect.UIOpen or SoundEffect.UIClose or SoundEffect.UIError or
            SoundEffect.UINotification => AudioCategory.UI,

            SoundEffect.AmbientWind or SoundEffect.AmbientRain or SoundEffect.AmbientThunder or
            SoundEffect.AmbientMachinery or SoundEffect.AmbientNature or SoundEffect.AmbientData
                => AudioCategory.Ambient,

            _ => AudioCategory.SoundEffects
        };
    }

    /// <summary>
    /// Plays UI select sound.
    /// </summary>
    public void PlayUISelect() => PlaySound(SoundEffect.UISelect);

    /// <summary>
    /// Plays UI confirm sound.
    /// </summary>
    public void PlayUIConfirm() => PlaySound(SoundEffect.UIConfirm);

    /// <summary>
    /// Plays UI cancel sound.
    /// </summary>
    public void PlayUICancel() => PlaySound(SoundEffect.UICancel);

    /// <summary>
    /// Plays UI error sound.
    /// </summary>
    public void PlayUIError() => PlaySound(SoundEffect.UIError);

    /// <summary>
    /// Gets the appropriate biome music track.
    /// </summary>
    public static MusicTrack GetBiomeMusic(string biomeType)
    {
        return biomeType?.ToLower() switch
        {
            "fringe" => MusicTrack.BiomeFringe,
            "rust" => MusicTrack.BiomeRust,
            "green" => MusicTrack.BiomeGreen,
            "quiet" => MusicTrack.BiomeQuiet,
            "teeth" => MusicTrack.BiomeTeeth,
            "glow" => MusicTrack.BiomeGlow,
            "archivescar" or "archive" => MusicTrack.BiomeArchive,
            _ => MusicTrack.BiomeFringe
        };
    }

    /// <summary>
    /// Gets the appropriate combat music track.
    /// </summary>
    public static MusicTrack GetCombatMusic(bool isBoss = false, string? bossId = null)
    {
        if (!isBoss)
        {
            return MusicTrack.CombatNormal;
        }

        return bossId?.ToLower() switch
        {
            "diadem" or "diadem_guardian" => MusicTrack.CombatDiadem,
            "liminal" or "liminal_entity" => MusicTrack.CombatLiminal,
            _ => MusicTrack.CombatBoss
        };
    }

    /// <summary>
    /// Exports audio settings for saving.
    /// </summary>
    public Dictionary<string, float> ExportVolumeSettings()
    {
        var settings = new Dictionary<string, float>();

        foreach (var kvp in _volumes)
        {
            settings[kvp.Key.ToString()] = kvp.Value;
        }

        return settings;
    }

    /// <summary>
    /// Imports audio settings from save data.
    /// </summary>
    public void ImportVolumeSettings(Dictionary<string, float>? settings)
    {
        if (settings == null)
        {
            return;
        }

        foreach (var kvp in settings)
        {
            if (Enum.TryParse<AudioCategory>(kvp.Key, out var category))
            {
                _volumes[category] = MathHelper.Clamp(kvp.Value, 0f, 1f);
            }
        }

        UpdateVolumes();
    }

    /// <summary>
    /// Cleans up all audio resources.
    /// </summary>
    public void Dispose()
    {
        StopMusic(false);
        StopAllAmbient();

        foreach (var pool in _soundPools.Values)
        {
            foreach (var instance in pool)
            {
                instance.Dispose();
            }
        }

        _soundPools.Clear();
        _music.Clear();
    }
}

