using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using Nautilus.Assets;
using Nautilus.Handlers;
using UnityEngine;
using UWE;

namespace BelowZeroCreatures;

internal static class BzCreatureLootDistribution
{
    public static void LoadAllFromJson(IEnumerable<PrefabInfo> infos)
    {
        var prefabInfoMapping = MapPrefabInfos(infos);
        
        var jsonFolder = Path.Combine(Path.GetDirectoryName(Plugin.Assembly.Location), "RandomSpawns");
        foreach (var file in Directory.GetFiles(jsonFolder, "*.json"))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (!prefabInfoMapping.TryGetValue(fileName, out var info))
            {
                Plugin.Logger.LogWarning($"Unknown creature TechType or ClassID found in RandomSpawns folder: '{fileName}'");
                continue;
            }
            
            var data = ParseJsonData(file);
            RegisterBiomeData(info, data);
        }
    }

    private static void RegisterBiomeData(PrefabInfo info, IEnumerable<LootDistributionData.BiomeData> data)
    {
        if (info.TechType == TechType.None)
        {
            Plugin.Logger.LogWarning("Registering creature spawns for TechType.None");
            return;
        }

        if (!ModData.CellLevels.TryGetValue(info.TechType.ToString(), out var cellLevel))
        {
            Plugin.Logger.LogWarning("Could not find CellLevel for creature TechType: " + cellLevel);
        }
        
        LootDistributionHandler.AddLootDistributionData(info, data, new WorldEntityInfo
        {
            cellLevel = cellLevel,
            classId = info.ClassID,
            techType = info.TechType,
            localScale = Vector3.one,
            prefabZUp = false,
            slotType = EntitySlot.Type.Creature
        });
    }

    private static Dictionary<string, PrefabInfo> MapPrefabInfos(IEnumerable<PrefabInfo> allInfo)
    {
        Dictionary<string, PrefabInfo> mapping = new(StringComparer.OrdinalIgnoreCase);
        foreach (var info in allInfo)
        {
            mapping.Add(info.ClassID, info);
        }
        return mapping;
    }
    
    private static List<LootDistributionData.BiomeData> ParseJsonData(string path)
    {
        List<LootDistributionData.BiomeData> jsonData;
        using StreamReader reader = new(path); 
        try {
            JsonMapper.RegisterImporter<double, float>(Convert.ToSingle);
            jsonData = JsonMapper.ToObject<List<LootDistributionData.BiomeData>>(reader);
        } catch {
            Plugin.Logger.LogError($"Exception thrown while loading JSON file at path {path}");
            throw;
        }

        return jsonData;
    }
}