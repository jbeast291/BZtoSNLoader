using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BelowZeroCreatures.StructureLoading;
using BepInEx;
using BepInEx.Logging;
using BZtoSNLoader;
using ComponentPortFramework;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Handlers;
using Newtonsoft.Json;

namespace BelowZeroCreatures;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus", "1.0.0.54")]
[BepInDependency("com.snmodding.BZtoSNLoader")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    internal static Dictionary<string, JsonPrefab> AssetPathsWithComponentData { get; private set; }
    
    private static ILogger ComponentSerializerLogger { get; } = new ComponentSerializerLogger();

    private static TechType JellyfishTechType { get; } = EnumHandler.AddEntry<TechType>("Jellyfish");
    private static TechType SquidSharkTechType { get; } = EnumHandler.AddEntry<TechType>("SquidShark");

    private static readonly Dictionary<string, string> NameConversions = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Chelicerata", "Chelicerate" },
        { "Vent_garden_standing", "LargeVentGarden" },
        { "VentGardenSmall", "SmallVentGarden" },
        { "TitanHoleFish", "TitanHolefish" }
    };

    private static readonly string[] MiscLocalizationToRequest =
    {
        "TitanHolefishOxygenUse"
    };
    
    internal static PrefabComponentSerializer ComponentSerializer { get; } = new(ComponentSerializerLogger,
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

        WaitScreenHandler.RegisterEarlyLoadTask("Below Zero Creatures", LoadMod, "Registering Below Zero creatures");
        
        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void LoadComponentExportData()
    {
        var resourceName = "BelowZeroCreatures.ComponentDataExport.json";

        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        string result = reader.ReadToEnd();
        AssetPathsWithComponentData = JsonConvert.DeserializeObject<Dictionary<string, JsonPrefab>>(result);
    }

    // TODO: Unload prefabs on quit to main menu?
    private void LoadMod(WaitScreenHandler.WaitScreenTask task)
    {
        LoadComponentExportData();
        
        // Register prefabs
        HashSet<TechType> techTypes = new HashSet<TechType>();
        foreach (var asset in AssetPathsWithComponentData)
        {
            var classId = GetBestClassId(asset.Key);
            
            // Skip Snow Stalkers because they seriously break the game
            if (ShouldSkipClassId(classId))
                continue;

            var info = GetPrefabInfo(classId);
            new BzCreaturePrefab(info
                .WithFolderPath("Below Zero Creatures"), new BZAssetLoader.AssetKey(asset.Key), asset.Value).Register();
            
            // IGNORE schools of fish in TechTypes list
            if (!classId.EndsWith("School"))
                techTypes.Add(info.TechType);
        }
        
        // Register TechType specific data
        
        TechType[] techTypesArray = techTypes.ToArray();
        RegisterAllLocalization(techTypesArray);
        
        // TODO: set up tech data (item size), localization, icons, and tool state?
        
        // Register structures
        StructureRegistrationUtils.RegisterStructures(StructureRegistrationUtils.GetStructuresFolderPath(Assembly));
    }

    private static void RegisterAllLocalization(TechType[] techTypesArray)
    {
        List<string> techTypesToRequest = new List<string>();
        foreach (var techType in techTypesArray)
        {
            var techTypeName = techType.ToString();
            techTypesToRequest.Add(techTypeName);
            techTypesToRequest.Add("Tooltip_" + techTypeName);
        }

        techTypesToRequest.AddRange(MiscLocalizationToRequest);
        string[] allStringKeys = techTypesToRequest.ToArray();
        string[] allTranslations = new string[allStringKeys.Length];
        
        BZLocalizationLoader.GetLocalization(allStringKeys, allTranslations, true);
        
        for (int i = 0; i < allStringKeys.Length; i++)
        {
            if (!string.Equals(allStringKeys[i], allTranslations[i]))
                LanguageHandler.SetLanguageLine(allStringKeys[i], allTranslations[i]);
        }
    }

    private static bool ShouldSkipClassId(string classId)
    {
        string toLower = classId.ToLower();
        return toLower.Contains("snowstalker");
    }

    private static PrefabInfo GetPrefabInfo(string classId)
    {
        if (classId == "RockPuncher")
        {
            return new PrefabInfo("RockPuncher", "RockPuncherPrefab", TechType.RockPuncher);
        }
        
        if (classId.Contains("SquidShark"))
        {
            return new PrefabInfo(classId, classId + "Prefab", SquidSharkTechType);
        }
        
        if (classId.Contains("JellyFish"))
        {
            return new PrefabInfo(classId, classId + "Prefab", JellyfishTechType);
        }

        return PrefabInfo.WithTechType(classId);
    }

    private static string GetBestClassId(string assetPath)
    {
        var fileName = assetPath.Split('/').Last().Split('.').First();
        if (NameConversions.TryGetValue(fileName, out var converted))
            fileName = converted;
        return fileName;
    }
}