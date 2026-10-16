using System;
using System.IO;
using UnityEngine;
namespace BZtoSNLoader;


internal static class BZPathFinder
{
    internal static bool TryDetermineBZAddressables(out string bundleFolderPath)
    {
        try 
        {
            bundleFolderPath = Application.platform switch
            {
                RuntimePlatform.WindowsPlayer => Windows__GetFolder(),
                RuntimePlatform.OSXPlayer => OSX__GetFolder(),
                _ => throw new Exception("Unsupported platform detected!")
            };
            return true;
        } 
        catch (InvalidGamePathException e) 
        {
            Plugin.Logger.LogError($"Failed to determine Subnautica: Below Zero folder automatically! {e.Message}");
            bundleFolderPath = string.Empty;
            return false;
        }
    }

    private const string ShadowLeviathanBundle = "shadowleviathan.prefab_414f7245007e8f135caaae693fbdf7d5.bundle";

    private static string Windows__GetFolder()
    {
        string BZDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "SubnauticaZero"));

        if (!Directory.Exists(BZDirectory)) throw new InvalidGamePathException($"BZDirectory does not exist at: \"{BZDirectory}\"");
        if (!File.Exists(Path.Combine(BZDirectory, "SubnauticaZero.exe"))) throw new InvalidGamePathException($"BZDirectory: \"{BZDirectory}\" is invalid! Missing SubnauticaZero.exe");

        string bundleFolderPath = Path.Combine(BZDirectory, "SubnauticaZero_Data", "StreamingAssets", "aa", "StandaloneWindows64");

        if (!Directory.Exists(bundleFolderPath)) throw new InvalidGamePathException($"Addressables bundle folder does not exist at: \"{bundleFolderPath}\"");
        if (!File.Exists(Path.Combine(bundleFolderPath, ShadowLeviathanBundle))) throw new InvalidGamePathException($"Addressables bundle folder: \"{bundleFolderPath}\" is missing bundles");

        return bundleFolderPath;
    }

    private static string OSX__GetFolder()
    {
        //TODO: ensure kookoo can make this work
        // home/jbeast/.local/share/Steam/steamapps/common/SubnauticaZero/SubnauticaZero.app/Contents/Resources/Data/StreamingAssets/aa/StandaloneOSX/
        Plugin.Logger.LogError($"App Domain: {AppDomain.CurrentDomain.BaseDirectory}");
        string BZDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "SubnauticaZero"));
        Plugin.Logger.LogError($"BZDirectory: {BZDirectory}");
        if (!Directory.Exists(BZDirectory)) throw new InvalidGamePathException($"BZDirectory does not exist at: \"{BZDirectory}\"");
        if (!File.Exists(Path.Combine(BZDirectory, "SubnauticaZero.exe"))) throw new InvalidGamePathException($"BZDirectory: \"{BZDirectory}\" is invalid! Missing SubnauticaZero.exe");

        string bundleFolderPath = Path.Combine(BZDirectory, "SubnauticaZero.app", "Contents", "Resources", "Data", "StreamingAssets", "aa", "StandaloneOSX");
        Plugin.Logger.LogError(bundleFolderPath);
        if (!Directory.Exists(bundleFolderPath)) throw new InvalidGamePathException($"Addressables bundle folder does not exist at: \"{bundleFolderPath}\"");
        if (!File.Exists(Path.Combine(bundleFolderPath, ShadowLeviathanBundle))) throw new InvalidGamePathException($"Addressables bundle folder: \"{bundleFolderPath}\" is missing bundles");

        return bundleFolderPath;
    }

    private class InvalidGamePathException(string message) : Exception(message);
}
