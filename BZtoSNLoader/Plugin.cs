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

    internal static string ReadFromResources(string assetName)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"BtwSourceGenerators.Resources.{assetName}");

        if (stream == null) throw new FileNotFoundException($"Embedded resource '{assetName}' not found!");

        using TextReader tr = new StreamReader(stream);
        return tr.ReadToEnd();
    }
} 