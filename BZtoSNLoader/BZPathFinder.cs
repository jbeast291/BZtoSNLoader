using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
namespace BZtoSNLoader;


internal static class BZPathFinder
{
    internal static bool TryDetermineBZAddressables(out LocatedPaths locatedPaths)
    {
        string bundleFolderPath;
        if (CheckConfigPath(out bundleFolderPath, out string configErrorMsg))
        {
            locatedPaths = GetPathsFromBundleFolder(bundleFolderPath);
            return true;
        }

        if (TryDetermineAutomaticBZPath(out bundleFolderPath, out string automaticErrorMsg))
        {
            locatedPaths = GetPathsFromBundleFolder(bundleFolderPath);
            return true;
        }

        Plugin.Logger.LogError($"Failed to determine Subnautica: Below Zero folder!" +
                               $"\nConfig failed: {configErrorMsg}" +
                               $"\nAutomatic failed: {automaticErrorMsg}");
        locatedPaths = null;
        return false;
    }

    private static LocatedPaths GetPathsFromBundleFolder(string bundleFolderPath)
    {
        return new LocatedPaths
        {
            AddressablesFolder = bundleFolderPath,
            LocalizationFolder = GetLocalizationFolderPathFromBundleFolder(bundleFolderPath),
        };
    }

    private static ConfigEntry<string> bzRootPath;

    internal static void SetupBepInConfig()
    {
        bzRootPath = Plugin.Config.Bind("BelowZero",
            "PathToBZFolder",
            @"C:\Program Files (x86)\Steam\steamapps\common\SubnauticaZero",
            "Path to your Subnautica: Below Zero folder. This should point to the folder containing the game's .exe on windows (Or .app on macos)");
    }

    private static bool CheckConfigPath(out string bundleFolderPath, out string errorMsg)
    {
        if (bzRootPath.Value == null)
        {
            bundleFolderPath = string.Empty;
            errorMsg = "Config Value Null!";
            return false;
        }
        bundleFolderPath = GetBundleFolderPath(bzRootPath.Value);
        return ValidateBundleFolder(bundleFolderPath, out errorMsg);
    }

    private static bool TryDetermineAutomaticBZPath(out string bundleFolderPath, out string errorMsg)
    {
        string BZDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "SubnauticaZero"));
        bundleFolderPath = GetBundleFolderPath(BZDirectory);
        return ValidateBundleFolder(bundleFolderPath, out errorMsg);
    }

    private static string GetLocalizationFolderPathFromBundleFolder(string bundleFolder)
    {
        return Path.GetFullPath(Path.Combine(bundleFolder, "..", "..", "SNUnmanagedData", "LanguageFiles"));
    }
    
    private static string GetBundleFolderPath(string gameRootFolder)
    {
        var dataPath = GetDataFolderPath(gameRootFolder);
        return Application.platform switch
        {
            RuntimePlatform.WindowsPlayer => Path.Combine(dataPath, "StreamingAssets", "aa", "StandaloneWindows64"),
            RuntimePlatform.OSXPlayer => Path.Combine(dataPath, "StreamingAssets", "aa", "StandaloneOSX"),
            _ => throw new Exception("Unsupported platform detected!")
        };
    }
    
    private static string GetDataFolderPath(string gameRootFolder)
    {
        return Application.platform switch
        {
            RuntimePlatform.WindowsPlayer => Path.Combine(gameRootFolder, "SubnauticaZero_Data"),
            RuntimePlatform.OSXPlayer => Path.Combine(gameRootFolder, "SubnauticaZero.app", "Contents", "Resources", "Data"),
            _ => throw new Exception("Unsupported platform detected!")
        };
    }

    private static bool ValidateBundleFolder(string bundleFolderPath, out string errorMsg)
    {
        if (!Directory.Exists(bundleFolderPath))
        {
            errorMsg = $"Addressables bundle folder does not exist at: \"{bundleFolderPath}\"";
            return false;
        }
        const string ShadowLeviathanBundle = "shadowleviathan.prefab_414f7245007e8f135caaae693fbdf7d5.bundle";
        if (!File.Exists(Path.Combine(bundleFolderPath, ShadowLeviathanBundle)))
        {
            errorMsg = $"Addressables bundle folder: \"{bundleFolderPath}\" is missing bundles";
            return false;
        }
        errorMsg = string.Empty;
        return true;
    }

    internal class LocatedPaths
    {
        public string AddressablesFolder { get; init; }
        public string LocalizationFolder { get; init; }
    }
}
