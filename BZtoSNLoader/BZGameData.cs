using System;
using System.IO;
using DependencyDataFormat;
using Newtonsoft.Json;

namespace BZtoSNLoader;

internal static class BZGameData
{
    internal static BundleDependencyFormat bundleDependencyData;
    internal static FMODFileLocatorFormat fmodFileLocatorData;

    private static LocatedPaths paths;

    internal static bool BelowZeroFound { get; private set; } 

    internal static void InitDependencyData()
    {
        BelowZeroFound = BZPathLocator.TryDetermineBZAddressables(out paths);

        // Load bundles dependency data
        string jsonData_bundleDependencyData = Plugin.ReadTextResources("assetPathToBundleDeps.json");
        bundleDependencyData = JsonConvert.DeserializeObject<BundleDependencyFormat>(jsonData_bundleDependencyData);

        string jsonData_fmodFileLocatorData = Plugin.ReadTextResources("fmodFilePaths.json");
        fmodFileLocatorData = JsonConvert.DeserializeObject<FMODFileLocatorFormat>(jsonData_fmodFileLocatorData);

        Plugin.Logger.LogInfo("Loaded Dependency Data!");
    }

    internal static string GetPathToBelowZeroBundle(string bundleName) 
        => Path.Combine(paths.AddressablesFolder, bundleName);

    internal static string GetLocalizationFilePath(string localizationFileName) 
        => Path.Combine(paths.LocalizationFolder, localizationFileName);

    internal static string GetBankFilePath(string bankFileNameNoExt) 
        => Path.Combine(paths.LocalizationFolder, $"{bankFileNameNoExt}.bank");
}