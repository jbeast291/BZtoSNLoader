using BelowZeroCreatures.BzCreatureAudio;
using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

[RequestBzSounds(160, RoarSound1, RoarSound2, RoarSound3, RoarSound4, RoarSound5)]
[RequestBzSounds(23, Swim1, Swim2, Swim3, Swim4, Swim5)]
public class ChelicerateModifier : BzCreatureModifier
{
    private const string RoarSound1 = "sfx_creature_chelicerate_callout_01";
    private const string RoarSound2 = "sfx_creature_chelicerate_callout_02";
    private const string RoarSound3 = "sfx_creature_chelicerate_callout_03";
    private const string RoarSound4 = "sfx_creature_chelicerate_callout_04";
    private const string RoarSound5 = "sfx_creature_chelicerate_callout_05";
    
    private const string Swim1 = "sfx_creature_chelicerate_swim_slow_01";
    private const string Swim2 = "sfx_creature_chelicerate_swim_slow_02";
    private const string Swim3 = "sfx_creature_chelicerate_swim_slow_03";
    private const string Swim4 = "sfx_creature_chelicerate_swim_slow_04";
    private const string Swim5 = "sfx_creature_chelicerate_swim_slow_05";

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

        BzCreatureVoice.ApplyToCreature(prefab, "roar", 13, 18,
            RoarSound1, RoarSound2, RoarSound3, RoarSound4, RoarSound5);
        
        BzSwimSound.ApplyToCreature(prefab, 1.5f, Swim1, Swim2, Swim3, Swim4, Swim5);
    }
}