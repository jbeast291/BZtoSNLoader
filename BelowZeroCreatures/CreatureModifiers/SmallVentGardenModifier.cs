using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class SmallVentGardenModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "SmallVentGarden" };
    
    protected override void ModifyPrefab(GameObject prefab)
    {
        var motion = prefab.AddComponent<SmallVentGardenMotion>();
        motion.swimBehaviour = prefab.GetComponent<SwimBehaviour>();
    }
}