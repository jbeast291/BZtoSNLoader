using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class SpinnerFishModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "SpinnerFish" };
    protected override string HoldAnimationParam => "peeper";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("Spinner_holding_anim");
        fpModel.localPosition = new Vector3(-0.025f, -0.01f, -0.02f);
        fpModel.localEulerAngles = new Vector3(34.28f, 197.1f, 197.1f);
    }
}