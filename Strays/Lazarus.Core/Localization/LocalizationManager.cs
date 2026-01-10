using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace Lazarus.Core.Localization;

/// <summary>
/// Supported languages in the game.
/// </summary>
public enum GameLanguage
{
    English,
    Spanish,
    French,
    German,
    Japanese,
    Portuguese,
    Italian,
    Russian,
    Korean,
    ChineseSimplified,
    ChineseTraditional
}

/// <summary>
/// Text direction for layout purposes.
/// </summary>
public enum TextDirection
{
    LeftToRight,
    RightToLeft
}

/// <summary>
/// Information about a supported language.
/// </summary>
public class LanguageInfo
{
    public GameLanguage Language { get; init; }
    public string CultureCode { get; init; } = "";
    public string NativeName { get; init; } = "";
    public string EnglishName { get; init; } = "";
    public TextDirection Direction { get; init; } = TextDirection.LeftToRight;
    public bool IsComplete { get; init; } = true;
    public float CompletionPercent { get; init; } = 100f;
}

/// <summary>
/// Event args for language change events.
/// </summary>
public class LanguageChangedEventArgs : EventArgs
{
    public GameLanguage OldLanguage { get; }
    public GameLanguage NewLanguage { get; }
    public CultureInfo NewCulture { get; }

    public LanguageChangedEventArgs(GameLanguage oldLang, GameLanguage newLang, CultureInfo culture)
    {
        OldLanguage = oldLang;
        NewLanguage = newLang;
        NewCulture = culture;
    }
}

/// <summary>
/// Manages localization settings for the game, including retrieving supported cultures,
/// setting the current culture, and providing localized strings with formatting support.
/// </summary>
public class LocalizationManager
{
    private static LocalizationManager? _instance;
    private static readonly object _lock = new();

    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static LocalizationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new LocalizationManager();
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// The culture code we default to.
    /// </summary>
    public const string DEFAULT_CULTURE_CODE = "en-US";

    private readonly ResourceManager _resourceManager;
    private readonly Dictionary<GameLanguage, LanguageInfo> _languages = new();
    private readonly Dictionary<string, string> _overrides = new();

    private GameLanguage _currentLanguage = GameLanguage.English;
    private CultureInfo _currentCulture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Event fired when the language changes.
    /// </summary>
    public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    /// <summary>
    /// Current language.
    /// </summary>
    public GameLanguage CurrentLanguage => _currentLanguage;

    /// <summary>
    /// Current culture info.
    /// </summary>
    public CultureInfo CurrentCulture => _currentCulture;

    /// <summary>
    /// Text direction for current language.
    /// </summary>
    public TextDirection CurrentTextDirection => GetLanguageInfo(_currentLanguage)?.Direction ?? TextDirection.LeftToRight;

    /// <summary>
    /// Whether the current language reads right-to-left.
    /// </summary>
    public bool IsRightToLeft => CurrentTextDirection == TextDirection.RightToLeft;

    private LocalizationManager()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        _resourceManager = new ResourceManager("Lazarus.Core.Localization.Resources", assembly);

        InitializeLanguages();
        SetLanguage(GameLanguage.English);
    }

    private void InitializeLanguages()
    {
        _languages[GameLanguage.English] = new LanguageInfo
        {
            Language = GameLanguage.English,
            CultureCode = "en-US",
            NativeName = "English",
            EnglishName = "English",
            Direction = TextDirection.LeftToRight,
            IsComplete = true,
            CompletionPercent = 100f
        };

        _languages[GameLanguage.Spanish] = new LanguageInfo
        {
            Language = GameLanguage.Spanish,
            CultureCode = "es-ES",
            NativeName = "Español",
            EnglishName = "Spanish",
            Direction = TextDirection.LeftToRight,
            IsComplete = true,
            CompletionPercent = 100f
        };

        _languages[GameLanguage.French] = new LanguageInfo
        {
            Language = GameLanguage.French,
            CultureCode = "fr-FR",
            NativeName = "Français",
            EnglishName = "French",
            Direction = TextDirection.LeftToRight,
            IsComplete = true,
            CompletionPercent = 100f
        };

        _languages[GameLanguage.German] = new LanguageInfo
        {
            Language = GameLanguage.German,
            CultureCode = "de-DE",
            NativeName = "Deutsch",
            EnglishName = "German",
            Direction = TextDirection.LeftToRight,
            IsComplete = true,
            CompletionPercent = 100f
        };

        _languages[GameLanguage.Japanese] = new LanguageInfo
        {
            Language = GameLanguage.Japanese,
            CultureCode = "ja-JP",
            NativeName = "日本語",
            EnglishName = "Japanese",
            Direction = TextDirection.LeftToRight,
            IsComplete = true,
            CompletionPercent = 100f
        };

        _languages[GameLanguage.Portuguese] = new LanguageInfo
        {
            Language = GameLanguage.Portuguese,
            CultureCode = "pt-BR",
            NativeName = "Português",
            EnglishName = "Portuguese (Brazil)",
            Direction = TextDirection.LeftToRight,
            IsComplete = false,
            CompletionPercent = 85f
        };

        _languages[GameLanguage.Italian] = new LanguageInfo
        {
            Language = GameLanguage.Italian,
            CultureCode = "it-IT",
            NativeName = "Italiano",
            EnglishName = "Italian",
            Direction = TextDirection.LeftToRight,
            IsComplete = false,
            CompletionPercent = 80f
        };

        _languages[GameLanguage.Russian] = new LanguageInfo
        {
            Language = GameLanguage.Russian,
            CultureCode = "ru-RU",
            NativeName = "Русский",
            EnglishName = "Russian",
            Direction = TextDirection.LeftToRight,
            IsComplete = false,
            CompletionPercent = 75f
        };

        _languages[GameLanguage.Korean] = new LanguageInfo
        {
            Language = GameLanguage.Korean,
            CultureCode = "ko-KR",
            NativeName = "한국어",
            EnglishName = "Korean",
            Direction = TextDirection.LeftToRight,
            IsComplete = false,
            CompletionPercent = 70f
        };

        _languages[GameLanguage.ChineseSimplified] = new LanguageInfo
        {
            Language = GameLanguage.ChineseSimplified,
            CultureCode = "zh-CN",
            NativeName = "简体中文",
            EnglishName = "Chinese (Simplified)",
            Direction = TextDirection.LeftToRight,
            IsComplete = false,
            CompletionPercent = 70f
        };

        _languages[GameLanguage.ChineseTraditional] = new LanguageInfo
        {
            Language = GameLanguage.ChineseTraditional,
            CultureCode = "zh-TW",
            NativeName = "繁體中文",
            EnglishName = "Chinese (Traditional)",
            Direction = TextDirection.LeftToRight,
            IsComplete = false,
            CompletionPercent = 65f
        };
    }

    /// <summary>
    /// Gets information about all supported languages.
    /// </summary>
    public IEnumerable<LanguageInfo> GetSupportedLanguages()
    {
        return _languages.Values;
    }

    /// <summary>
    /// Gets information about a specific language.
    /// </summary>
    public LanguageInfo? GetLanguageInfo(GameLanguage language)
    {
        return _languages.TryGetValue(language, out var info) ? info : null;
    }

    /// <summary>
    /// Gets a list of supported cultures based on available language resources.
    /// </summary>
    public static List<CultureInfo> GetSupportedCultures()
    {
        List<CultureInfo> supportedCultures = new();
        Assembly assembly = Assembly.GetExecutingAssembly();
        ResourceManager resourceManager = new("Lazarus.Core.Localization.Resources", assembly);

        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

        foreach (CultureInfo culture in cultures)
        {
            try
            {
                var resourceSet = resourceManager.GetResourceSet(culture, true, false);

                if (resourceSet != null)
                {
                    supportedCultures.Add(culture);
                }
            }
            catch (MissingManifestResourceException)
            {
                // No .resx for this culture
            }
        }

        supportedCultures.Add(CultureInfo.InvariantCulture);

        return supportedCultures;
    }

    /// <summary>
    /// Sets the current language.
    /// </summary>
    public void SetLanguage(GameLanguage language)
    {
        if (language == _currentLanguage && _currentCulture != CultureInfo.InvariantCulture)
        {
            return;
        }

        var oldLanguage = _currentLanguage;
        _currentLanguage = language;

        var info = GetLanguageInfo(language);
        string cultureCode = info?.CultureCode ?? DEFAULT_CULTURE_CODE;

        SetCulture(cultureCode);

        LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldLanguage, language, _currentCulture));
    }

    /// <summary>
    /// Sets the current culture based on the specified culture code.
    /// </summary>
    public void SetCulture(string cultureCode)
    {
        if (string.IsNullOrEmpty(cultureCode))
        {
            cultureCode = DEFAULT_CULTURE_CODE;
        }

        try
        {
            _currentCulture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = _currentCulture;
            Thread.CurrentThread.CurrentUICulture = _currentCulture;

            // Also update the static Resources culture for direct Resources.XXX access
            Resources.Culture = _currentCulture;
        }
        catch (CultureNotFoundException)
        {
            _currentCulture = CultureInfo.InvariantCulture;
            Resources.Culture = CultureInfo.InvariantCulture;
        }
    }

    /// <summary>
    /// Gets a localized string by key.
    /// </summary>
    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return "";
        }

        // Check overrides first
        if (_overrides.TryGetValue(key, out var overrideValue))
        {
            return overrideValue;
        }

        try
        {
            string? value = _resourceManager.GetString(key, _currentCulture);

            if (value != null)
            {
                return value;
            }

            // Fallback to English
            value = _resourceManager.GetString(key, CultureInfo.InvariantCulture);

            return value ?? $"[{key}]";
        }
        catch
        {
            return $"[{key}]";
        }
    }

    /// <summary>
    /// Gets a localized string with format parameters.
    /// </summary>
    public string Get(string key, params object[] args)
    {
        string format = Get(key);

        try
        {
            return string.Format(_currentCulture, format, args);
        }
        catch
        {
            return format;
        }
    }

    /// <summary>
    /// Gets a pluralized string based on count.
    /// </summary>
    public string GetPlural(string singularKey, string pluralKey, int count)
    {
        string key = count == 1 ? singularKey : pluralKey;

        return Get(key, count);
    }

    /// <summary>
    /// Gets a pluralized string with multiple plural forms (for languages that need it).
    /// </summary>
    public string GetPlural(string key, int count)
    {
        // Try language-specific plural key first
        string pluralKey = GetPluralForm(key, count);
        string? value = null;

        try
        {
            value = _resourceManager.GetString(pluralKey, _currentCulture);
        }
        catch
        {
            // Ignore
        }

        if (value != null)
        {
            return string.Format(_currentCulture, value, count);
        }

        // Fallback to base key
        return Get(key, count);
    }

    private string GetPluralForm(string baseKey, int count)
    {
        // Different languages have different plural rules
        // English: singular (1), plural (other)
        // Russian: singular (1), few (2-4), many (5-20, ends in 11-19), plural (other)
        // Japanese: no plural forms
        // Arabic: zero, one, two, few (3-10), many (11-99), other

        return _currentLanguage switch
        {
            GameLanguage.Japanese or GameLanguage.Korean or
            GameLanguage.ChineseSimplified or GameLanguage.ChineseTraditional
                => baseKey, // No plural forms

            GameLanguage.Russian => GetRussianPluralKey(baseKey, count),

            _ => count == 1 ? $"{baseKey}_One" : $"{baseKey}_Other"
        };
    }

    private static string GetRussianPluralKey(string baseKey, int count)
    {
        int mod10 = count % 10;
        int mod100 = count % 100;

        if (mod10 == 1 && mod100 != 11)
        {
            return $"{baseKey}_One";
        }

        if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20))
        {
            return $"{baseKey}_Few";
        }

        return $"{baseKey}_Many";
    }

    /// <summary>
    /// Checks if a string key exists.
    /// </summary>
    public bool HasKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        try
        {
            return _resourceManager.GetString(key, _currentCulture) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sets a runtime override for a string.
    /// Useful for user-generated content or dynamic text.
    /// </summary>
    public void SetOverride(string key, string value)
    {
        _overrides[key] = value;
    }

    /// <summary>
    /// Removes a runtime override.
    /// </summary>
    public void RemoveOverride(string key)
    {
        _overrides.Remove(key);
    }

    /// <summary>
    /// Clears all runtime overrides.
    /// </summary>
    public void ClearOverrides()
    {
        _overrides.Clear();
    }

    /// <summary>
    /// Formats a number according to current culture.
    /// </summary>
    public string FormatNumber(int number)
    {
        return number.ToString("N0", _currentCulture);
    }

    /// <summary>
    /// Formats a number according to current culture.
    /// </summary>
    public string FormatNumber(float number, int decimals = 1)
    {
        return number.ToString($"N{decimals}", _currentCulture);
    }

    /// <summary>
    /// Formats a percentage according to current culture.
    /// </summary>
    public string FormatPercent(float value, int decimals = 0)
    {
        return value.ToString($"P{decimals}", _currentCulture);
    }

    /// <summary>
    /// Formats a date according to current culture.
    /// </summary>
    public string FormatDate(DateTime date)
    {
        return date.ToString("d", _currentCulture);
    }

    /// <summary>
    /// Formats a time span as duration.
    /// </summary>
    public string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        return $"{duration.Minutes}:{duration.Seconds:D2}";
    }

    /// <summary>
    /// Formats a time span as play time (e.g., "12h 34m").
    /// </summary>
    public string FormatPlayTime(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return Get("Format_PlayTimeHours", (int)duration.TotalHours, duration.Minutes);
        }

        return Get("Format_PlayTimeMinutes", duration.Minutes);
    }

    /// <summary>
    /// Gets the language from a culture code.
    /// </summary>
    public GameLanguage GetLanguageFromCultureCode(string cultureCode)
    {
        foreach (var kvp in _languages)
        {
            if (kvp.Value.CultureCode.Equals(cultureCode, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }

            // Also check just the language part (e.g., "en" matches "en-US")
            if (cultureCode.StartsWith(kvp.Value.CultureCode.Split('-')[0], StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }

        return GameLanguage.English;
    }

    /// <summary>
    /// Detects the system language and sets it if supported.
    /// </summary>
    public void UseSystemLanguage()
    {
        var systemCulture = CultureInfo.CurrentUICulture;
        var language = GetLanguageFromCultureCode(systemCulture.Name);
        SetLanguage(language);
    }
}

/// <summary>
/// Extension methods for localization.
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    /// Gets a localized string using the default LocalizationManager.
    /// </summary>
    public static string Localize(this string key)
    {
        return LocalizationManager.Instance.Get(key);
    }

    /// <summary>
    /// Gets a localized string with parameters using the default LocalizationManager.
    /// </summary>
    public static string Localize(this string key, params object[] args)
    {
        return LocalizationManager.Instance.Get(key, args);
    }
}

/// <summary>
/// Static shorthand for common localization operations.
/// </summary>
public static class L
{
    /// <summary>
    /// Gets a localized string.
    /// </summary>
    public static string Get(string key) => LocalizationManager.Instance.Get(key);

    /// <summary>
    /// Gets a localized string with format parameters.
    /// </summary>
    public static string Get(string key, params object[] args) => LocalizationManager.Instance.Get(key, args);

    /// <summary>
    /// Gets a pluralized string.
    /// </summary>
    public static string Plural(string singularKey, string pluralKey, int count)
        => LocalizationManager.Instance.GetPlural(singularKey, pluralKey, count);

    /// <summary>
    /// Current language.
    /// </summary>
    public static GameLanguage Language => LocalizationManager.Instance.CurrentLanguage;

    /// <summary>
    /// Whether the current language reads right-to-left.
    /// </summary>
    public static bool IsRTL => LocalizationManager.Instance.IsRightToLeft;
}
