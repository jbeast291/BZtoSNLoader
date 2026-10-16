using System;
using System.Data;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using DependencyDataFormat;
using Nautilus.Handlers;
using Newtonsoft.Json;

namespace BZtoSNLoader;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    private const string DependencyDataFileName = "assetPathToBundleDeps.json";

    // TODO: Only initialize these 2 upon loading the game (early load task?)
    private static BelowZeroGameData _bzGameData;
    private static ManagedBundleLoader _managedBundleLoader;

    public static BelowZeroAssetLoader AssetLoader { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;
        LoadDependencyData();
        _managedBundleLoader = new ManagedBundleLoader();
        AssetLoader = new BelowZeroAssetLoader(_bzGameData, _managedBundleLoader);
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(Commands));
    }

    private static void LoadDependencyData()
    {
        if (!BZPathdeterminer.TryDetermineBZAddressables(out string bzAddressablesFolder))
        {
            throw new Exception("Manual selection fallback not implemented!");
        }

        // Load bundles dependency data
        string jsonData = File.ReadAllText(GetModFolderPath(DependencyDataFileName));
        var dependencyData = JsonConvert.DeserializeObject<BundleDependencyFormat>(jsonData);

        _bzGameData = new BelowZeroGameData(bzAddressablesFolder, dependencyData);

        if (dependencyData.Version != BundleDependencyFormat.latestVersion)
        {
            throw new VersionNotFoundException($"Invalid Dependency Version! Please ensure you replaced the \"{DependencyDataFileName}\" when updating!");
        }
    }

    private static string GetModFolderPath(string assetName) =>
        Path.Combine(Path.GetDirectoryName(Assembly.Location)!, assetName);
} 