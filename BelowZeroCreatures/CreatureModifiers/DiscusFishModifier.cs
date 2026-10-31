using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class DiscusFishModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "DiscusFish" };
    protected override string HoldAnimationParam => "bladderfish";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("model_FP");
        fpModel.localPosition = new Vector3(0.070f, 0.040f, 0.160f);
        fpModel.localEulerAngles = new Vector3(37.428f, 332.857f, 102.857f);
        fpModel.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }
}