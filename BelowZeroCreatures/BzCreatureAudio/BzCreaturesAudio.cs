using System.Collections;
using System.Runtime.InteropServices;
using BZtoSNLoader;
using FMOD;
using FMODUnity;
using Nautilus.Handlers;
using Nautilus.Utility;

namespace BelowZeroCreatures.BzCreatureAudio;

internal static class BzCreaturesAudio
{
    public const string PathPrefix = "bz_creatures_mod_";
    
    private const string CreaturesBus = "bus:/master/SFX_for_pause/PDA_pause/all/SFX/creatures";
    
    public static IEnumerator RegisterAudio()
    {
        var requestedSounds = BzSoundRequestLocator.GetAllRequestedBelowZeroSounds();
        foreach (var requested in requestedSounds)
        {
            yield return RegisterSound(PathPrefix + requested.EventName, requested.MinDistance,
                requested.MaxDistance, new BZAudioLoader.FMODRawName(requested.EventName));
        }
    }

    public static FMODAsset GetBzCreatureFmodAsset(string pathWithoutPrefix)
    {
        return AudioUtils.GetFmodAsset(PathPrefix + pathWithoutPrefix);
    }

    private static IEnumerator RegisterSound(string eventPath, float minDistance, float maxDistance,
        BZAudioLoader.FMODRawName soundName)
    {
        TaskResult<byte[]> requestResult = new();
        yield return BZAudioLoader.LoadRawAudioAsync(soundName, requestResult);
        byte[] result = requestResult.Get();
        CREATESOUNDEXINFO exinfo = new() {
            cbsize = Marshal.SizeOf<CREATESOUNDEXINFO>(),
            length = (uint)result.Length,
        };

        RuntimeManager.CoreSystem.createSound(result, MODE.OPENMEMORY | MODE._3D | MODE._3D_LINEARSQUAREROLLOFF, ref exinfo, out Sound sound);
        sound.set3DMinMaxDistance(minDistance, maxDistance);
        CustomSoundHandler.RegisterCustomSound(eventPath, sound, CreaturesBus);
    }
}