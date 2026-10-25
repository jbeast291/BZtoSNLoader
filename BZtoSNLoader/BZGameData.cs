using System;
using System.Data;
using System.IO;
using DependencyDataFormat;
using Newtonsoft.Json;

namespace BZtoSNLoader;

internal static class BZGameData
{
    internal static bool isInitialized;
    internal static BundleDependencyFormat BundleDependencyData;
    private static string BelowZeroAddressablesFolder;
    internal static bool BelowZeroFound { get; private set; } 

    internal static void InitDependencyData()
    {
        if (!BZPathFinder.TryDetermineBZAddressables(out string bzAddressablesFolder))
        {
            isInitialized = false;
            throw new Exception("Manual selection fallback not implemented!");
        }
        // Load bundles dependency data
        BelowZeroFound = true;
        const string DependencyDataFileName = "assetPathToBundleDeps.json";
        string jsonData = Plugin.ReadFromResources(DependencyDataFileName);
        var dependencyData = JsonConvert.DeserializeObject<BundleDependencyFormat>(jsonData);

        BelowZeroAddressablesFolder = bzAddressablesFolder;
        BundleDependencyData = dependencyData;

        if (dependencyData.Version != BundleDependencyFormat.latestVersion)
        {
            isInitialized = false;
            throw new VersionNotFoundException($"Invalid Dependency Version! Please ensure you replaced the \"{DependencyDataFileName}\" when updating!");
        }
        isInitialized = true;
    }

    internal static string GetPathToBelowZeroBundle(string bundleName) => Path.Combine(BelowZeroAddressablesFolder, bundleName);
}