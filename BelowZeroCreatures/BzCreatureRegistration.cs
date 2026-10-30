using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.StructureLoading;
using BZtoSNLoader;
using ComponentPortFramework;
using Nautilus.Handlers;
using Newtonsoft.Json;
using UnityEngine.U2D;

namespace BelowZeroCreatures;

public static class BzCreatureRegistration
{
    private static Dictionary<string, JsonPrefab> AssetPathsWithComponentData { get; set; }
    private static bool _waitScreenTaskExecuted;

    // TODO: Unload prefabs on quit to main menu?
    public static IEnumerator LoadMod(WaitScreenHandler.WaitScreenTask task)
    {
        if (_waitScreenTaskExecuted)
            yield break;

        _waitScreenTaskExecuted = true;

        LoadComponentExportData();
        CreatureModifiersManager.RegisterCreatureEdits();

        // Register prefabs
        HashSet<TechType> techTypes = new HashSet<TechType>();
        HashSet<string> classIds = new HashSet<string>();
        foreach (var asset in AssetPathsWithComponentData)
        {
            var classId = GetBestClassId(asset.Key);

            // Skip Snow Stalkers because they seriously break the game
            if (ShouldSkipClassId(classId))
                continue;

            var info = ModData.GetPrefabInfo(classId);
            new BzCreaturePrefab(info
                .WithFolderPath("Below Zero Creatures"), new BZAssetLoader.AssetKey(asset.Key), asset.Value).Register();

            // IGNORE schools of fish in TechTypes list
            if (!classId.EndsWith("School"))
                techTypes.Add(info.TechType);
            classIds.Add(classId);
        }

        CreatureModifiersManager.CheckForInvalidModifiers(classIds);

        // Register TechType specific data

        TechType[] techTypesArray = techTypes.ToArray();
        RegisterAllLocalization(techTypesArray);

        // Register PDA entries
        LanguageHandler.SetLanguageLine("EncyPath_Lifeforms/Fauna/BelowZero", "Sector Zero");
        foreach (var techType in techTypesArray)
        {
            string techTypeName = techType.ToString();
            string databankName = ModData.GetDatabankName(techTypeName);
            PDAHandler.AddCustomScannerEntry(techType, 3, false, databankName);
            PDAHandler.AddEncyclopediaEntry(databankName, "Lifeforms/Fauna/BelowZero");
        }

        // Register structures
        StructureRegistrationUtils.RegisterStructures(
            StructureRegistrationUtils.GetStructuresFolderPath(Plugin.Assembly));

        // Load creature icons
        TaskResult<SpriteAtlas> atlasResult = new TaskResult<SpriteAtlas>();
        yield return BZAssetLoader.LoadAssetAsync(
            new BZAssetLoader.AssetKey("Assets/AddressableResources/Atlases/Items.spriteatlas"), atlasResult);
        var atlas = atlasResult.Get();
        foreach (var techType in techTypes)
        {
            string spriteName = techType.ToString().ToLower();
            var sprite = atlas.GetSprite(spriteName);
            if (sprite != null)
                SpriteHandler.RegisterSprite(techType, sprite);
        }

        // Set equipment type
        foreach (var techType in techTypes)
        {
            if (ModData.HoldableCreatures.Contains(techType.ToString()))
                CraftDataHandler.SetEquipmentType(techType, EquipmentType.Hand);
        }
    }

    private static void LoadComponentExportData()
    {
        var resourceName = "BelowZeroCreatures.ComponentDataExport.json";

        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        string result = reader.ReadToEnd();
        AssetPathsWithComponentData = JsonConvert.DeserializeObject<Dictionary<string, JsonPrefab>>(result);
    }

    private static void RegisterAllLocalization(TechType[] techTypesArray)
    {
        List<string> techTypesToRequest = new List<string>();
        foreach (var techType in techTypesArray)
        {
            var techTypeName = techType.ToString();
            techTypesToRequest.Add(techTypeName);
            techTypesToRequest.Add("Tooltip_" + techTypeName);
            string databankName = ModData.GetDatabankName(techTypeName);
            techTypesToRequest.Add("Ency_" + databankName);
            techTypesToRequest.Add("EncyDesc_" + databankName);
        }

        techTypesToRequest.AddRange(ModData.MiscLocalizationToRequest);
        string[] allStringKeys = techTypesToRequest.ToArray();

        string[] allTranslations =  BZLocalizationLoader.GetLocalization(allStringKeys, true);

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

    private static string GetBestClassId(string assetPath)
    {
        var fileName = assetPath.Split('/').Last().Split('.').First();
        if (ModData.NameConversions.TryGetValue(fileName, out var converted))
            fileName = converted;
        return fileName;
    }
}