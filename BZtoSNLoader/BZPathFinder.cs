using System;
using System.IO;
using BepInEx;
using UnityEngine;
namespace BZtoSNLoader;


internal static class BZPathFinder
{
    //TODO: improve error handling/logging. Shouldn't log error when config is invalid for example
    internal static bool TryDetermineBZAddressables(out string bundleFolderPath)
    {
        if (CheckConfigPath(out bundleFolderPath)) return true;
        if (TryDetermineAutomaticBZPath(out bundleFolderPath)) return true;

        Plugin.Logger.LogError($"Failed to determine Subnautica: Below Zero folder!");
        bundleFolderPath = string.Empty;
        return false;
    }

    private static bool TryDetermineAutomaticBZPath(out string bundleFolderPath)
    {
        string BZDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "SubnauticaZero"));
        bundleFolderPath = CreateBundleFolderPathFromRoot(BZDirectory);
        return ValidateBundleFolder(bundleFolderPath);
    }

    private static bool CheckConfigPath(out string bundleFolderPath)
    {
        if (Plugin.bzRootPath.Value == null)
        {
            bundleFolderPath = string.Empty;
            return false;
        }
        bundleFolderPath = CreateBundleFolderPathFromRoot(Plugin.bzRootPath.Value);
        return ValidateBundleFolder(bundleFolderPath);
    }
    
    private static string CreateBundleFolderPathFromRoot(string gameRootFolder)
    {
        return Application.platform switch
        {
            RuntimePlatform.WindowsPlayer => Path.Combine(gameRootFolder, "SubnauticaZero_Data", "StreamingAssets", "aa", "StandaloneWindows64"),
            RuntimePlatform.OSXPlayer => Path.Combine(gameRootFolder, "SubnauticaZero.app", "Contents", "Resources", "Data", "StreamingAssets", "aa", "StandaloneOSX"),
            _ => throw new Exception("Unsupported platform detected!")
        };
    }

    private static bool ValidateBundleFolder(string bundleFolderPath)
    {
        if (!Directory.Exists(bundleFolderPath))
        {
            Plugin.Logger.LogError($"Addressables bundle folder does not exist at: \"{bundleFolderPath}\"");
            return false;
        }
        const string ShadowLeviathanBundle = "shadowleviathan.prefab_414f7245007e8f135caaae693fbdf7d5.bundle";
        if (!File.Exists(Path.Combine(bundleFolderPath, ShadowLeviathanBundle)))
        {
            Plugin.Logger.LogError($"Addressables bundle folder: \"{bundleFolderPath}\" is missing bundles");
            return false;
        }
        return true;
    }
}
