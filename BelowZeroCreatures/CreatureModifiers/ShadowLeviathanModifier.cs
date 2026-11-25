using BelowZeroCreatures.BzCreatureAudio;
using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

[RequestBzSounds(60, Chitter1, Chitter2, Chitter3, Chitter4, Chitter5, Chitter6)]
//[RequestBzSounds(20, Swim1, Swim2, Swim3, Swim4, Swim5)]
public class ShadowLeviathanModifier : BzCreatureModifier
{
    private const string Chitter1 = "sfx_creature_shadowleviathan_chitter_01";
    private const string Chitter2 = "sfx_creature_shadowleviathan_chitter_02";
    private const string Chitter3 = "sfx_creature_shadowleviathan_chitter_03";
    private const string Chitter4 = "sfx_creature_shadowleviathan_chitter_04";
    private const string Chitter5 = "sfx_creature_shadowleviathan_chitter_05";
    private const string Chitter6 = "sfx_creature_shadowleviathan_chitter_06";
    
    /*
    private const string Swim1 = "sfx_creature_shadowleviathan_swimfast_01";
    private const string Swim2 = "sfx_creature_shadowleviathan_swimfast_02";
    private const string Swim3 = "sfx_creature_shadowleviathan_swimfast_03";
    private const string Swim4 = "sfx_creature_shadowleviathan_swimfast_04";
    private const string Swim5 = "sfx_creature_shadowleviathan_swimfast_05";
*/
    
    public override string[] ClassIds => new[] { "ShadowLeviathan" };
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
        
        // NEVER call the 'roar' animator param, this bricks the shadow leviathan
        BzCreatureVoice.ApplyToCreature(prefab, "call", 20, 24,
            Chitter1, Chitter2, Chitter3, Chitter4, Chitter5, Chitter6)
            .WithCallAnimationMatchingSoundIndex();
        
        // BzSwimSound.ApplyToCreature(prefab, 3, Swim1, Swim2, Swim3, Swim4, Swim5);
    }
}