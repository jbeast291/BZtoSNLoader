using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using ComponentPortFramework;
using HarmonyLib;
using Nautilus.Handlers;

namespace BZPrefabReader;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    internal static ILogger FrameworkLogger { get; } = new FrameworkLogger();
    
    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;
        
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(Commands));
        
        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        
        Sn1TypeDatabase.Initialize(FrameworkLogger);
        
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
}