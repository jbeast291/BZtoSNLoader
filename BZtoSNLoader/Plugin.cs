using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;

namespace BZtoSNLoader;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; }
    internal static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private void Awake()
    {
        Logger = base.Logger;
        BZGameData.InitDependencyData();
        Harmony.CreateAndPatchAll(Assembly, PluginInfo.PLUGIN_GUID);
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(DebugCommands));
    }

    internal static string GetModFolderFile(string assetName) =>
        Path.Combine(Path.GetDirectoryName(Assembly.Location)!, assetName);
} 