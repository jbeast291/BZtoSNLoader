using System.IO;
using DependencyDataFormat;
using Newtonsoft.Json;

namespace BZtoSNLoader;

internal static class BZGameData
{
    internal static BundleDependencyFormat bundleDependencyData;

    private static string addressablesFolder;
    private static string localizationFolder;
    internal static bool BelowZeroFound { get; private set; } 

    internal static void InitDependencyData()
    {
        BelowZeroFound = BZPathFinder.TryDetermineBZAddressables(out LocatedPaths paths);

        // Load bundles dependency data
        string jsonData = Plugin.ReadFromResources("assetPathToBundleDeps.json");
        var dependencyData = JsonConvert.DeserializeObject<BundleDependencyFormat>(jsonData);

        addressablesFolder = paths.AddressablesFolder;
        localizationFolder = paths.LocalizationFolder;
        bundleDependencyData = dependencyData;
        Plugin.Logger.LogInfo("Loaded Dependency Data!");
    }

    internal static string GetPathToBelowZeroBundle(string bundleName) 
        => Path.Combine(addressablesFolder, bundleName);

    internal static string GetLocalizationFilePath(string fileNameWithExtension) 
        => Path.Combine(localizationFolder, fileNameWithExtension);
}