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
        LoadAllEmbeddedDlls();
        BZGameData.InitDependencyData();
        BZLocalizationLoader.Register();
        Harmony.CreateAndPatchAll(Assembly, MyPluginInfo.PLUGIN_GUID);
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(AudioCommands));
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(AssetCommands));
        Logger.LogInfo("Finished Loading!");
    }

    internal static void LoadAllEmbeddedDlls()
    {
        string[] names = Assembly.GetManifestResourceNames();
        foreach (string resourceName in names)
        {
            if (!resourceName.EndsWith(".dll")) continue;
            using Stream stream = Assembly.GetManifestResourceStream(resourceName);
            using BinaryReader br = new(stream);
            byte[] bytes = br.ReadBytes((int)stream.Length);
            Assembly.Load(bytes);
            Logger.LogInfo("Loaded Assembly: " + resourceName);
        }
    }

    internal static string ReadTextResources(string assetName)
    {
        using Stream stream = Assembly.GetManifestResourceStream($"BZtoSNLoader.Resources.{assetName}");

        if (stream == null) throw new FileNotFoundException($"Embedded resource '{assetName}' not found!");

        using TextReader tr = new StreamReader(stream);
        return tr.ReadToEnd();
    }

    internal static string GetFilePathNextToPlugin(string assetName) 
        => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, assetName);
} 