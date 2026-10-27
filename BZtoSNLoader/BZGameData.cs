using System.IO;
using DependencyDataFormat;
using Newtonsoft.Json;

namespace BZtoSNLoader;

internal static class BZGameData
{
    internal static BundleDependencyFormat BundleDependencyData;

    private static string BelowZeroAddressablesFolder;
    private static string BelowZeroLocalizationFolder;
    internal static bool BelowZeroFound { get; private set; } 

    internal static void InitDependencyData()
    {
        BelowZeroFound = BZPathFinder.TryDetermineBZAddressables(out BZPathFinder.LocatedPaths paths);

        // Load bundles dependency data
        string jsonData = Plugin.ReadFromResources("assetPathToBundleDeps.json");
        var dependencyData = JsonConvert.DeserializeObject<BundleDependencyFormat>(jsonData);

        BelowZeroAddressablesFolder = paths.AddressablesFolder;
        BelowZeroLocalizationFolder = paths.LocalizationFolder;
        BundleDependencyData = dependencyData;
        Plugin.Logger.LogInfo("Loaded Dependency Data!");
    }

    internal static string GetPathToBelowZeroBundle(string bundleName) => Path.Combine(BelowZeroAddressablesFolder, bundleName);
}