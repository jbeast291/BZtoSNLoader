using System;
using System.IO;
using BepInEx.Configuration;
using UnityEngine;

namespace BZtoSNLoader;

internal readonly record struct LocatedPaths(
    string AddressablesFolder, 
    string LocalizationFolder,
    string BankFolder
);

internal static class BZPathLocator
{
    internal static bool TryDetermineBZAddressables(out LocatedPaths locatedPaths)
    {
        if (TryCheckConfigPath(out locatedPaths, out string configErrorMsg) || 
            TryDetermineAutomaticBZPath(out locatedPaths, out string automaticErrorMsg)
        ) {
            return true;
        }
        Plugin.Logger.LogError($"Failed to determine Subnautica: Below Zero folder!" +
                               $"\nConfig failed: {configErrorMsg}" +
                               $"\nAutomatic failed: {automaticErrorMsg}");
        locatedPaths = default;
        return false;
    }

    private static ConfigEntry<string> bzRootPath;

    private static bool TryCheckConfigPath(out LocatedPaths paths, out string errorMsg) 
    {
        bzRootPath = Plugin.Config.Bind("BelowZero",
            "PathToBZFolder",
            @"C:\Program Files (x86)\Steam\steamapps\common\SubnauticaZero",
            "Path to your Subnautica: Below Zero folder. This should point to the folder containing the game's .exe on windows (Or .app on macos)"
        );
        if (bzRootPath.Value == null) 
        {
            paths = default;
            errorMsg = "Config Value Null!";
            return false;
        }
        string bundleFolderPath = GetBundleFolderPath(bzRootPath.Value);
        paths = CreateLocatedPaths(bundleFolderPath);
        return ArePathsValid(paths, out errorMsg);
    }

    private static bool TryDetermineAutomaticBZPath(out LocatedPaths paths, out string errorMsg)
    {
        string BZDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "SubnauticaZero"));
        string bundleFolderPath = GetBundleFolderPath(BZDirectory);
        paths = CreateLocatedPaths(bundleFolderPath);
        return ArePathsValid(paths, out errorMsg);
    }

    private static string GetBundleFolderPath(string gameRootFolder) 
        => Application.platform switch {
            RuntimePlatform.WindowsPlayer => Path.Combine(gameRootFolder, "SubnauticaZero_Data", "StreamingAssets", "aa", "StandaloneWindows64"),
            RuntimePlatform.OSXPlayer => Path.Combine(gameRootFolder, "SubnauticaZero.app", "Contents", "Resources", "Data", "StreamingAssets", "aa", "StandaloneOSX"),
            _ => throw new Exception("Unsupported platform detected!")
        };

    private static LocatedPaths CreateLocatedPaths(string bundleFolderPath) 
        => new(bundleFolderPath, 
            GetLocalizationFolderPath(bundleFolderPath),
            GetBankFolderPath(bundleFolderPath)
        );

    private static string GetLocalizationFolderPath(string bundleFolder)
        => Path.GetFullPath(Path.Combine(bundleFolder, "..", "..", "SNUnmanagedData", "LanguageFiles"));

    private static string GetBankFolderPath(string bundleFolder)
        => Path.GetFullPath(Path.Combine(bundleFolder, "..", ".."));

    private static bool ArePathsValid(LocatedPaths paths, out string errorMsg)
    {
        errorMsg = string.Empty;
        return ValidatePath(paths.AddressablesFolder, "shadowleviathan.prefab_414f7245007e8f135caaae693fbdf7d5.bundle", ref errorMsg) &&
               ValidatePath(paths.LocalizationFolder, "English.json", ref errorMsg) &&
               ValidatePath(paths.BankFolder, "Player.bank", ref errorMsg);

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