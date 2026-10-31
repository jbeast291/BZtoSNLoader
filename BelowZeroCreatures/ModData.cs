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

    public static EdibleData GetEdibleDataForCookedFish(string classId, bool cured)
    {
        var baseEdibleData = GetEdibleDataByClassId(classId);
        if (cured) return new EdibleData(baseEdibleData.Food, -2);
        return baseEdibleData;
    }

    private static EdibleData GetEdibleDataByClassId(string classId)
    {
        if (classId == "Triops") return new EdibleData(13, 5);
        if (classId == "ArrowRay") return new EdibleData(13, 5);
        if (classId == "ArcticPeeper") return new EdibleData(32, 5);
        if (classId == "DiscusFish") return new EdibleData(18, 10);
        return new EdibleData(23, 3);
    }

    public struct EdibleData
    {
        public float Food { get; }
        public float Water { get; }

        public EdibleData(float food, float water)
        {
            Food = food;
            Water = water;
        }
    }

    public static readonly Dictionary<string, CreatureDataEntry> CreatureData = new(StringComparer.OrdinalIgnoreCase)
    {
        // SMALL FISH
        { "ArrowRay", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 30) },
        { "FeatherFish", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 30) },
        { "FeatherFishRed", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 30) },
        { "ArcticPeeper", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 30) },
        { "SpinnerFish", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 30) },
        { "Symbiote", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 30) },
        { "Triops", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 30) },
        { "SeaMonkeyBaby", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 100) },
        { "DiscusFish", new(BehaviourType.SmallFish, LargeWorldEntity.CellLevel.Near, 20) },
        // MEDIUM FISH
        { "Penguin", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 50) },
        { "PenguinBaby", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 50) },
        { "Pinnacarid", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 100) },
        { "ArcticRay", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 100) },
        { "TrivalveBlue", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 500) },
        { "TrivalveYellow", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 500) },
        { "SeaMonkey", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 300) },
        { "NootFish", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Near, 100) },
        { "Brinewing", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Near, 100) },
        { "LilyPaddler", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 100) },
        // CRABS
        { "RockPuncher", new(BehaviourType.Crab, LargeWorldEntity.CellLevel.Medium, 200) },
        // SHARKS
        { "BruteShark", new(BehaviourType.Shark, LargeWorldEntity.CellLevel.Medium, 200) },
        { "SquidShark", new(BehaviourType.Shark, LargeWorldEntity.CellLevel.Medium, 1000) },
        { "Cryptosuchus", new(BehaviourType.Shark, LargeWorldEntity.CellLevel.Medium, 200) },
        { "SnowStalker", new(BehaviourType.Shark, LargeWorldEntity.CellLevel.Medium, 200) },
        { "SnowStalkerBaby", new(BehaviourType.Shark, LargeWorldEntity.CellLevel.Medium, 200) },
        // WHALES
        { "GlowWhale", new(BehaviourType.Whale, LargeWorldEntity.CellLevel.Far, 10000) },
        { "TitanHolefish", new(BehaviourType.Whale, LargeWorldEntity.CellLevel.Medium, 2000) },
        { "Jellyfish", new(BehaviourType.Whale, LargeWorldEntity.CellLevel.Medium, 200) },
        // TRUE LEVIATHANS
        { "Chelicerate", new(BehaviourType.Leviathan, LargeWorldEntity.CellLevel.VeryFar, 5000) },
        { "ShadowLeviathan", new(BehaviourType.Leviathan, LargeWorldEntity.CellLevel.VeryFar, 5000) },
        // SCHOOLS
        { "ArrowRay_School", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 100) },
        { "FeatherFishRed_School", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 100) },
        { "FeatherFish_School", new(BehaviourType.MediumFish, LargeWorldEntity.CellLevel.Medium, 100) },
        // FAKE LEVIATHANS (health doesn't apply)
        { "SmallVentGarden", new(BehaviourType.Leviathan, LargeWorldEntity.CellLevel.VeryFar, float.MaxValue) },
        { "LargeVentGarden", new(BehaviourType.Leviathan, LargeWorldEntity.CellLevel.VeryFar, float.MaxValue) }
    };

    public class CreatureDataEntry
    {
        public BehaviourType BehaviourType { get; }
        public LargeWorldEntity.CellLevel CellLevel { get; }
        public float MaxHealth { get; }

        public CreatureDataEntry(
            BehaviourType behaviourType,
            LargeWorldEntity.CellLevel cellLevel,
            float maxHealth)
        {
            BehaviourType = behaviourType;
            CellLevel = cellLevel;
            MaxHealth = maxHealth;
        }
    }
}