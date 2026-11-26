using System.Collections;
using System.Runtime.InteropServices;
using BZtoSNLoader;
using FMOD;
using FMODUnity;
using Nautilus.FMod;
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
            yield return RegisterSound(requested);
        }
    }

    public static FMODAsset GetBzCreatureFmodAsset(string pathWithoutPrefix)
    {
        return AudioUtils.GetFmodAsset(PathPrefix + pathWithoutPrefix);
    }

    private static IEnumerator RegisterSound(SoundRequest request)
    {
        string eventName = PathPrefix + request.EventName;

        var soundFiles = request.BzSounds;

        if (soundFiles.Length == 1)
        {
            TaskResult<byte[]> requestResult = new();
            yield return BZAudioLoader.LoadRawAudioAsync(new BZAudioLoader.FMODRawName(soundFiles[0]), requestResult);
            byte[] result = requestResult.Get();
            CREATESOUNDEXINFO exinfo = new() {
                cbsize = Marshal.SizeOf<CREATESOUNDEXINFO>(),
                length = (uint)result.Length,
            };

            RuntimeManager.CoreSystem.createSound(result, MODE.OPENMEMORY | MODE._3D | MODE._3D_LINEARSQUAREROLLOFF | MODE.CREATESAMPLE, ref exinfo, out Sound sound);
            sound.set3DMinMaxDistance(request.MinDistance, request.MaxDistance);
        
            CustomSoundHandler.RegisterCustomSound(eventName, sound, CreaturesBus);
        }
        else
        {
            Sound[] allSounds = new Sound[soundFiles.Length];

            for (int i = 0; i < soundFiles.Length; i++)
            {
                TaskResult<byte[]> requestResult = new();
                yield return BZAudioLoader.LoadRawAudioAsync(new BZAudioLoader.FMODRawName(soundFiles[i]), requestResult);
                byte[] result = requestResult.Get();
                CREATESOUNDEXINFO exinfo = new() {
                    cbsize = Marshal.SizeOf<CREATESOUNDEXINFO>(),
                    length = (uint)result.Length,
                };

                RuntimeManager.CoreSystem.createSound(result, MODE.OPENMEMORY | MODE._3D | MODE._3D_LINEARSQUAREROLLOFF | MODE.CREATESAMPLE, ref exinfo, out Sound sound);
                sound.set3DMinMaxDistance(request.MinDistance, request.MaxDistance);
                allSounds[i] = sound;
            }

            var multiSounds = new FModMultiSounds(allSounds, CreaturesBus, randomizeSounds: true);
            CustomSoundHandler.RegisterCustomSound(eventName, multiSounds);
        }
    }
}