using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;

namespace BelowZeroCreatures;

internal static class BzFoodRegistration
{
    public static IEnumerator RegisterFoods(IEnumerable<PrefabInfo> infos, List<TechType> techTypeResults)
    {
        List<CookedCreature> cookedCreatures = new();
        foreach (var info in infos)
        {
            var techTypeName = info.ClassID;
            if (ModData.CookableCreatures.Contains(techTypeName))
            {
                cookedCreatures.Add(new CookedCreature(info, false));
                cookedCreatures.Add(new CookedCreature(info, true));
            }
        }

        foreach (var cooked in cookedCreatures)
        {
            cooked.Register();
            techTypeResults.Add(cooked.Info.TechType);
        }

        yield break;
    }
}