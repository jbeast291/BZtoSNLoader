using BelowZeroCreatures.CreatureModifiers.System;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

public class ChelicerateModifier : BzCreatureModifier
{
    public override string[] ClassIds => new[] { "Chelicerate" };
    protected override void ModifyPrefab(GameObject prefab)
    {
        var attackCyclops = prefab.AddComponent<AttackCyclops>();
        attackCyclops.aggressPerSecond = 0.4f;
        attackCyclops.attackAggressionThreshold = 0.75f;
        attackCyclops.attackPause = 3;
        attackCyclops.maxDistToLeash = 160;
        attackCyclops.swimVelocity = 25;
        attackCyclops.swimInterval = 0.8f;
        attackCyclops.aggressiveToNoise = new CreatureTrait(0, 0.01f);
        attackCyclops.lastTarget = prefab.EnsureComponent<LastTarget>();
        attackCyclops.evaluatePriority = 0.91f;
        attackCyclops.priorityMultiplier = new AnimationCurve(
            new Keyframe(0, 1),
            new Keyframe(1, 1));
    }
}