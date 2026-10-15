using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;

namespace BZPrefabReader;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    
    internal static Sn1TypeDatabase TypeDatabase { get; } = new();

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;
        
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(Commands));
        
        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        
        // Load SN1 types
        var resourceName = "BZPrefabReader.SN1Classes";

        using (Stream stream = Assembly.GetManifestResourceStream(resourceName))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                TypeDatabase.Initialize(result.Split('\n'));
            }
        }
        
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
}