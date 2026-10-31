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

    public static readonly HashSet<string> HoldableCreatures = new(StringComparer.OrdinalIgnoreCase)
    {
        "ArcticPeeper", "ArrowRay", "DiscusFish", "FeatherFish", "FeatherFishRed", "NootFish", "SpinnerFish",
        "Triops", "PenguinBaby", "SeaMonkeyBaby", "Symbiote"
    };

    public static readonly HashSet<string> CookableCreatures = new(StringComparer.OrdinalIgnoreCase)
    {
        "ArcticPeeper", "ArrowRay", "DiscusFish", "FeatherFish", "FeatherFishRed", "NootFish", "SpinnerFish", "Triops",
        "Symbiote"
    };

    public static readonly Dictionary<string, BehaviourType> BehaviourTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ArrowRay", BehaviourType.SmallFish },
        { "FeatherFish", BehaviourType.SmallFish },
        { "FeatherFishRed", BehaviourType.SmallFish },
        { "ArcticPeeper", BehaviourType.SmallFish },
        { "SpinnerFish", BehaviourType.SmallFish },
        { "Symbiote", BehaviourType.SmallFish },
        { "Triops", BehaviourType.SmallFish },
        { "SeaMonkeyBaby", BehaviourType.SmallFish },
        { "DiscusFish", BehaviourType.SmallFish },
        { "Penguin", BehaviourType.MediumFish },
        { "Pinnacarid", BehaviourType.MediumFish },
        { "ArcticRay", BehaviourType.MediumFish },
        { "TrivalveBlue", BehaviourType.MediumFish },
        { "TrivalveYellow", BehaviourType.MediumFish },
        { "SeaMonkey", BehaviourType.MediumFish },
        { "NootFish", BehaviourType.MediumFish },
        { "Brinewing", BehaviourType.MediumFish },
        { "RockPuncher", BehaviourType.Crab },
        { "BruteShark", BehaviourType.Shark },
        { "SquidShark", BehaviourType.Shark },
        { "Cryptosuchus", BehaviourType.Shark },
        { "SnowStalker", BehaviourType.Shark },
        { "SnowStalkerBaby", BehaviourType.Shark },
        { "GlowWhale", BehaviourType.Whale },
        { "TitanHolefish", BehaviourType.Whale },
        { "Jellyfish", BehaviourType.Whale },
        { "Chelicerate", BehaviourType.Leviathan },
        { "ShadowLeviathan", BehaviourType.Leviathan }
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
    
    public static string FixCookedCreatureAssetPath(string classId)
    {
        if (classId == "SpinnerFish") return "Spinnerfish";
        if (classId == "NootFish") return "Nootfish";
        return classId;
    }
    
    public static string FixCookedCreatureTechTypeName(string classId)
    {
        if (classId == "SpinnerFish") return "Spinnerfish";
        return classId;
    }
}