using System;
using System.IO;
using BepInEx.Configuration;
using UnityEngine;

namespace BZtoSNLoader;

internal readonly record struct LocatedPaths(string AddressablesFolder, string LocalizationFolder);

internal static class BZPathFinder
{
    internal static bool TryDetermineBZAddressables(out LocatedPaths locatedPaths)
    {
        if (TryCheckConfigPath(out string bundleFolderPath, out string configErrorMsg) || 
            TryDetermineAutomaticBZPath(out bundleFolderPath, out string automaticErrorMsg)
        ) {
            locatedPaths = new LocatedPaths(bundleFolderPath, GetLocalizationFolderPath(bundleFolderPath));
            return true;
        }
        Plugin.Logger.LogError($"Failed to determine Subnautica: Below Zero folder!" +
                               $"\nConfig failed: {configErrorMsg}" +
                               $"\nAutomatic failed: {automaticErrorMsg}");
        locatedPaths = default;
        return false;
    }

    private static ConfigEntry<string> bzRootPath;

    private static bool TryCheckConfigPath(out string bundleFolderPath, out string errorMsg) 
    {
        bzRootPath = Plugin.Config.Bind("BelowZero",
            "PathToBZFolder",
            @"C:\Program Files (x86)\Steam\steamapps\common\SubnauticaZero",
            "Path to your Subnautica: Below Zero folder. This should point to the folder containing the game's .exe on windows (Or .app on macos)"
        );
        if (bzRootPath.Value == null) 
        {
            bundleFolderPath = string.Empty;
            errorMsg = "Config Value Null!";
            return false;
        }
        bundleFolderPath = GetBundleFolderPath(bzRootPath.Value);
        LocatedPaths paths = CreateLocatedPaths(bundleFolderPath);
        return ArePathsValid(paths, out errorMsg);
    }

    private static bool TryDetermineAutomaticBZPath(out string bundleFolderPath, out string errorMsg)
    {
        string BZDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "SubnauticaZero"));
        bundleFolderPath = GetBundleFolderPath(BZDirectory);
        LocatedPaths paths = CreateLocatedPaths(bundleFolderPath);
        return ArePathsValid(paths, out errorMsg);
    }

    private static string GetBundleFolderPath(string gameRootFolder) 
        => Application.platform switch {
            RuntimePlatform.WindowsPlayer => Path.Combine(gameRootFolder, "SubnauticaZero_Data", "StreamingAssets", "aa", "StandaloneWindows64"),
            RuntimePlatform.OSXPlayer => Path.Combine(gameRootFolder, "SubnauticaZero.app", "Contents", "Resources", "Data", "StreamingAssets", "aa", "StandaloneOSX"),
            _ => throw new Exception("Unsupported platform detected!")
        };

    private static LocatedPaths CreateLocatedPaths(string bundleFolderPath) 
        => new(bundleFolderPath, GetLocalizationFolderPath(bundleFolderPath));

    private static string GetLocalizationFolderPath(string bundleFolder)
        => Path.GetFullPath(Path.Combine(bundleFolder, "..", "..", "SNUnmanagedData", "LanguageFiles"));

    private static bool ArePathsValid(LocatedPaths paths, out string errorMsg)
    {
        errorMsg = string.Empty;
        return ValidatePath(paths.AddressablesFolder, "shadowleviathan.prefab_414f7245007e8f135caaae693fbdf7d5.bundle", ref errorMsg) &
               ValidatePath(paths.LocalizationFolder, "English.json", ref errorMsg);

        bool ValidatePath(string path, string fileToCheck, ref string msg)
        {
            if (!Directory.Exists(path))
            {
                msg += $"\n\tFolder does not exist at: \"{paths.AddressablesFolder}\"";
                return false;
            }
            if (!File.Exists(Path.Combine(path, fileToCheck)))
            {
                msg += $"\n\tFile is missing: \"{paths.AddressablesFolder}\"";
                return false;
            }
            return true;
        }
    }
}