using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using Nautilus.Handlers;
using UnityEngine;

namespace BZtoSNLoader;

/// <summary>
/// The API for loading specific translations from language keys that are present in Below Zero.
/// </summary>
public static class BZLocalizationLoader
{
    private static Dictionary<string, string> LoadedLocalization { get; set; }
    private static string CurrentlyCachedLanguageName { get; set; }

    internal static void Register() => WaitScreenHandler.RegisterLateLoadTask(PluginInfo.PLUGIN_NAME, OnLoadingScreenEnd);

    /// <summary>
    /// Translates all requested keys and fills the results array with the translated strings. 
    /// </summary>
    /// <param name="requestedKeys">An array containing every localization key to translate.</param>
    /// <param name="results">The array that will be populated with the results. Its size must be the same as the size of <paramref name="requestedKeys"/>.</param>
    /// <param name="warnMissing">If true, log warnings will be displayed when any key could not be found.</param>
    /// <param name="languageOverride">If a value is not provided, the current language will be used. Example value: "English"</param>
    /// <exception cref="Exception">An exception will be thrown if this method is called while not in the Wait Screen.</exception>
    /// <exception cref="ArgumentException">An exception is thrown when the size of the arrays does not match.</exception>
    /// <exception cref="FileNotFoundException">An exception is thrown if the designated language file, AND the default fallback of English.json, could not be found.</exception>
    public static void GetLocalization(string[] requestedKeys, string[] results, bool warnMissing, string languageOverride = null)
    {
        if (!WaitScreen.IsWaiting)
            throw new Exception("Below Zero localization cannot be accessed outside of the WaitScreen.");

        if (results.Length != requestedKeys.Length)
            throw new ArgumentException($"Results array size ({results.Length}) did not match requested keys size ({requestedKeys.Length})");

        string currentLanguage = !string.IsNullOrEmpty(languageOverride)
            ? languageOverride
            : Language.main.GetCurrentLanguage();

        var languageFilePath = BZGameData.GetLocalizationFilePath($"{currentLanguage}.json");

        if (!File.Exists(languageFilePath))
        {
            Plugin.Logger.LogError($"Failed to find language file by name {currentLanguage}; falling back!");
            currentLanguage = "English";
            languageFilePath = BZGameData.GetLocalizationFilePath("English.json");
            if (!File.Exists(languageFilePath))
            {
                throw new FileNotFoundException($"Below Zero localization files not found (path: '{languageFilePath}').");
            }
        }

        EnsureLocalizationLoaded(languageFilePath, currentLanguage);

        for (int i = 0; i < requestedKeys.Length; i++)
        {
            if (LoadedLocalization.TryGetValue(requestedKeys[i], out var translation))
            {
                results[i] = translation;
            }
            else
            {
                if (warnMissing)
                    Plugin.Logger.LogWarning($"Translation not found for key '{requestedKeys[i]}'");
                results[i] = requestedKeys[i];
            }
        }
    }

    private static void EnsureLocalizationLoaded(string filePath, string languageName)
    {
        if (LoadedLocalization is { Count: > 0 })
        {
            if (languageName.Equals(CurrentlyCachedLanguageName, StringComparison.InvariantCultureIgnoreCase))
            {
                // No new load is needed because this language is already cached
                return;
            }
        }
        CurrentlyCachedLanguageName = languageName;

        JsonData jsonData;
        using StreamReader reader = new(filePath); 
        try {
            jsonData = JsonMapper.ToObject(reader);
        } catch {
            Plugin.Logger.LogError($"Exception thrown while loading localization for language '{languageName}'.");
            throw;
        }

        LoadedLocalization?.Clear();
        LoadedLocalization = new Dictionary<string, string>();

        foreach (string key in jsonData.Keys)
        {
            LoadedLocalization[key] = (string)jsonData[key];
        }

        Plugin.Logger.LogInfo($"Loaded {jsonData.Count} entries from Below Zero localization.");
    }

    private static void OnLoadingScreenEnd(WaitScreenHandler.WaitScreenTask task)
    {
        LoadedLocalization?.Clear();
        LoadedLocalization = null;
        CurrentlyCachedLanguageName = null;
    }
}