using System;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Handlers;

namespace BelowZeroCreatures;

internal static class ModData
{
    public static TechType JellyfishTechType { get; } = EnumHandler.AddEntry<TechType>("Jellyfish");
    public static TechType SquidSharkTechType { get; } = EnumHandler.AddEntry<TechType>("SquidShark");

    public static readonly Dictionary<string, string> NameConversions = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Chelicerata", "Chelicerate" },
        { "Vent_garden_standing", "LargeVentGarden" },
        { "VentGardenSmall", "SmallVentGarden" },
        { "TitanHoleFish", "TitanHolefish" }
    };

    public static readonly string[] MiscLocalizationToRequest =
    {
        "TitanHolefishOxygenUse",
        "WhaleInteract"
    };

    public static readonly HashSet<string> HoldableCreatures = new()
    {
        "ArcticPeeper", "ArrowRay", "DiscusFish", "FeatherFish", "FeatherFishRed", "NootFish", "SpinnerFish",
        "Triops", "PenguinBaby", "SeaMonkeyBaby", "Symbiote", "PenguinBaby"
    };

    public static readonly HashSet<string> CookableCreatures = new()
    {
        "ArcticPeeper", "ArrowRay", "DiscusFish", "FeatherFish", "FeatherFishRed", "NootFish", "SpinnerFish", "Triops",
        "Symbiote"
    };
    
    public static string GetDatabankName(string techTypeString)
    {
        return techTypeString switch
        {
            "TrivalveBlue" => "TrivalveB",
            "TrivalveYellow" => "TrivalveA",
            "LargeVentGarden" => "VentGarden",
            _ => techTypeString
        };
    }
    
    public static PrefabInfo GetPrefabInfo(string classId)
    {
        if (classId == "RockPuncher")
        {
            return new PrefabInfo("RockPuncher", "RockPuncherPrefab", TechType.RockPuncher);
        }

        if (classId.Contains("SquidShark"))
        {
            return new PrefabInfo(classId, classId + "Prefab", ModData.SquidSharkTechType);
        }

        if (classId.Contains("JellyFish"))
        {
            return new PrefabInfo(classId, classId + "Prefab", ModData.JellyfishTechType);
        }

        return PrefabInfo.WithTechType(classId);
    }
}