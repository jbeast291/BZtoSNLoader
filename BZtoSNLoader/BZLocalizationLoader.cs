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
    private static Dictionary<string, string> CachedLocalization { get; set; }
    private static string CurrentlyCachedLanguageName { get; set; }

    internal static void Register() => WaitScreenHandler.RegisterLateLoadTask(PluginInfo.PLUGIN_NAME, OnLoadingScreenEnd);

    /// <summary>
    /// Translates all requested keys and fills the results array with the translated strings.
    /// This method is only valid to be called during the wait screen (Nautilus: <see cref="WaitScreenHandler"/>)
    /// </summary>
    /// <param name="requestedKeys">An array containing every localization key to translate.</param>
    /// <param name="warnMissing">If true, log warnings will be displayed when any key could not be found.</param>
    /// <param name="languageOverride">If a value is not provided, the current language will be used. Example value: "English"</param>
    /// <exception cref="Exception">An exception will be thrown if this method is called while not in the Wait Screen.</exception>
    /// <exception cref="FileNotFoundException">An exception is thrown if the designated language file, AND the default fallback of English.json, could not be found.</exception>
    public static string[] GetLocalization(string[] requestedKeys, bool warnMissing, string languageOverride = null)
    {
        if (!BZGameData.BelowZeroFound) {
            throw new Exception("BZ Asset Loader not initialized! Check the console for previous path errors");
        }
        if (!WaitScreen.IsWaiting) {
            throw new Exception("Below Zero localization cannot be accessed outside of the WaitScreen.");
        }

        string currentLanguage = !string.IsNullOrEmpty(languageOverride)
            ? languageOverride
            : Language.main.GetCurrentLanguage();

        string languageFilePath = BZGameData.GetLocalizationFilePath($"{currentLanguage}.json");

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

        CacheLocalizationLanguage(languageFilePath, currentLanguage);

        string[] translations = new string[requestedKeys.Length];
        for (int i = 0; i < requestedKeys.Length; i++)
        {
            if (CachedLocalization.TryGetValue(requestedKeys[i], out var translation)) {
                translations[i] = translation;
                continue;
            }

            if (warnMissing) {
                Plugin.Logger.LogWarning($"Translation not found for key '{requestedKeys[i]}'");
            }
            translations[i] = requestedKeys[i];
        }
        return translations;
    }

    private static void CacheLocalizationLanguage(string filePath, string languageName)
    {
        if (CachedLocalization is { Count: > 0 } &&
            languageName.Equals(CurrentlyCachedLanguageName, StringComparison.InvariantCultureIgnoreCase)
        ) {
            return; // No new load is needed because this language is already cached
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

        CachedLocalization?.Clear();
        CachedLocalization = new Dictionary<string, string>();

        foreach (string key in jsonData.Keys)
        {
            CachedLocalization[key] = (string)jsonData[key];
        }

        Plugin.Logger.LogInfo($"Loaded {jsonData.Count} entries from Below Zero localization.");
    }

    private static void OnLoadingScreenEnd(WaitScreenHandler.WaitScreenTask task)
    {
        CachedLocalization?.Clear();
        CachedLocalization = null;
        CurrentlyCachedLanguageName = null;
    }
}