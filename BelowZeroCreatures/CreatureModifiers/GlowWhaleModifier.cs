using BelowZeroCreatures.BzCreatureAudio;
using BelowZeroCreatures.CreatureModifiers.System;
using BelowZeroCreatures.MonoBehaviours;
using UnityEngine;

namespace BelowZeroCreatures.CreatureModifiers;

[RequestBzSounds(120, Call1, Call2, Call3, Call4, Call5)]
public class GlowWhaleModifier : BzCreatureModifier
{
    private const string Call1 = "sfx_creature_glowwhale_call_01";
    private const string Call2 = "sfx_creature_glowwhale_call_02";
    private const string Call3 = "sfx_creature_glowwhale_call_03";
    private const string Call4 = "sfx_creature_glowwhale_call_04";
    private const string Call5 = "sfx_creature_glowwhale_call_05";

    public override string[] ClassIds => new[] { "GlowWhale" };
    
    protected override void ModifyPrefab(GameObject prefab)
    {
        BzCreatureVoice.ApplyToCreature(prefab, "call", 13, 22,
            Call1, Call2, Call3, Call4, Call5)
            .WithCallAnimationMatchingSoundIndex();
    }
}