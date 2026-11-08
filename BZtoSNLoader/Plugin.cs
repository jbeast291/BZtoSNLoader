using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;

namespace BZtoSNLoader;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus", "1.0.0.56")]
internal sealed class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; }
    internal new static ConfigFile Config { get; private set; }
    internal static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private void Awake()
    {
        Logger = base.Logger;
        Config = base.Config;
        BZGameData.InitDependencyData();
        BZLocalizationLoader.Register();
        Harmony.CreateAndPatchAll(Assembly, MyPluginInfo.PLUGIN_GUID);
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(DebugCommands));
        Logger.LogInfo("Finished Loading!");
    }

    internal static string ReadFromResources(string assetName)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"BZtoSNLoader.Resources.{assetName}");

        if (stream == null) throw new FileNotFoundException($"Embedded resource '{assetName}' not found!");

        using TextReader tr = new StreamReader(stream);
        return tr.ReadToEnd();
    }
} 