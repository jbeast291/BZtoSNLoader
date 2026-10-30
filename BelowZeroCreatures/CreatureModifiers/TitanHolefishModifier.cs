using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using Nautilus.Utility;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class TitanHolefishModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "TitanHolefish" };
    
    protected override void ModifyPrefab(GameObject prefab)
    {
        // add oxygen provider
        var air = prefab.transform.Find("Collisions/AirSack");
        var target = air.gameObject.AddComponent<TitanHolefishOxygenHandTarget>();
        target.liveMixin = prefab.GetComponent<LiveMixin>();
        target.sound = AudioUtils.GetFmodAsset("event:/env/bubble_pop_player");

        // I think these avoid their own BODIES, so I change this value
        var avoid = prefab.GetComponent<AvoidObstacles>();
        avoid.avoidTerrainOnly = true;
        avoid.avoidanceDistance = 12;
        avoid.scanDistance = 12;
    }
}