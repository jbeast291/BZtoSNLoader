using BelowZeroCreatures.CreatureModifiers.System;
using ComponentPortFramework;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class SquidsharkModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "SquidShark", "SquidShark_LilyPads" };
    
    protected override void ModifyPrefab(GameObject prefab)
    {
        // attack last target
        var component = prefab.AddComponent<AttackLastTarget>();
        component.evaluatePriority = 0.6f;
        component.swimVelocity = 8;
        component.aggressionThreshold = 0.75f;
        component.minAttackDuration = 3;
        component.maxAttackDuration = 7;
        component.pauseInterval = 7;
        component.rememberTargetTime = 7;
        component.resetAggressionOnTime = true;
        component.lastTarget = prefab.GetComponent<LastTarget>();
        component.minActionCheckInterval = 3;

        // Later, we could make this do proper electrical damage!
        foreach (var onTouchFixer in prefab.GetComponentsInChildren<OnTouchFixer>())
        {
            if (onTouchFixer.gameObject.name == "mouth")
            {
                continue;
            }
            
            // replace or remove electrical damage trigger fixer
            Object.DestroyImmediate(onTouchFixer);
        }
    }
}