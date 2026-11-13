using System.IO;
using DependencyDataFormat;
using Newtonsoft.Json;

namespace BZtoSNLoader;

internal static class BZGameData
{
    internal static BundleDependencyFormat bundleDependencyData;
    internal static FMODFileLocatorFormat fmodFileLocatorData;

    private static string addressablesFolder;
    private static string localizationFolder;
    internal static bool BelowZeroFound { get; private set; } 

    internal static void InitDependencyData()
    {
        BelowZeroFound = BZPathFinder.TryDetermineBZAddressables(out LocatedPaths paths);

        // Load bundles dependency data
        string jsonData_bundleDependencyData = Plugin.ReadTextResources("assetPathToBundleDeps.json");
        bundleDependencyData = JsonConvert.DeserializeObject<BundleDependencyFormat>(jsonData_bundleDependencyData);

        string jsonData_fmodFileLocatorData = Plugin.ReadTextResources("fmodFilePaths.json");
        fmodFileLocatorData = JsonConvert.DeserializeObject<FMODFileLocatorFormat>(jsonData_fmodFileLocatorData);

        addressablesFolder = paths.AddressablesFolder;
        localizationFolder = paths.LocalizationFolder;
        Plugin.Logger.LogInfo("Loaded Dependency Data!");
    }

    internal static string GetPathToBelowZeroBundle(string bundleName) 
        => Path.Combine(addressablesFolder, bundleName);

    internal static string GetLocalizationFilePath(string fileNameWithExtension) 
        => Path.Combine(localizationFolder, fileNameWithExtension);
}