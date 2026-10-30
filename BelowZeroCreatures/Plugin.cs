using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using ComponentPortFramework;
using HarmonyLib;
using Nautilus.Handlers;
using ILogger = ComponentPortFramework.ILogger;

namespace BelowZeroCreatures;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus", "1.0.0.54")]
[BepInDependency("com.snmodding.BZtoSNLoader")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    internal static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private static ILogger ComponentSerializerLogger { get; } = new ComponentSerializerLogger();

    internal static PrefabComponentSerializer ComponentSerializer { get; } = new(
        ComponentSerializerLogger,
        new PrefabComponentSerializerSettings
        {
            EnableCreatureFixes = true,
            EnableAllComponents = true
        });

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;

        Sn1TypeDatabase.Initialize(ComponentSerializerLogger);

        WaitScreenHandler.RegisterEarlyAsyncLoadTask("Below Zero Creatures", BzCreatureRegistration.LoadMod,
            "Registering Below Zero creatures");

        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
}