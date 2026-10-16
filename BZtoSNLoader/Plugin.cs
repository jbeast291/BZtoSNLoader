using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Nautilus.Handlers;

namespace BZtoSNLoader;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private void Awake()
    {
        Logger = base.Logger;
        BZGameData.InitDependencyData();
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(DebugCommands));
    }

    internal static string GetModFolderPath(string assetName) =>
        Path.Combine(Path.GetDirectoryName(Assembly.Location)!, assetName);
} 