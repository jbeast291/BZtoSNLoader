using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class SymbioteModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "Symbiote" };
    
    protected override string HoldAnimationParam => "spadefish";

    protected override void ModifyPrefab(GameObject prefab)
    {
        var fpModel = prefab.transform.Find("Symbiote_Fish_view");
        fpModel.localPosition = new Vector3(0.23f, -0.025f, 0.322f);
        fpModel.localEulerAngles = new Vector3(358.9f, 154.1f, 1.654f);
    }
}