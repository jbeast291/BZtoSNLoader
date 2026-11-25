using System.Collections;
using System.Runtime.InteropServices;
using FMOD;
using FMODUnity;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Utility;
using UWE;

namespace BZtoSNLoader;

internal static class AudioCommands
{
    //audiobz vo_Log_Cine_MeetAl_P5_Component_03
    [ConsoleCommand("audiobz")]
    private static void PlayBzAudio(string bzSoundName) 
        => CoroutineHost.StartCoroutine(playAudioAsync(bzSoundName));

    private static IEnumerator playAudioAsync(string bzSoundName)
    {
        TaskResult<byte[]> requestResult = new();
        yield return BZAudioLoader.LoadAudioAsync(new(bzSoundName), requestResult);
        byte[] result = requestResult.Get();
        CreateEventMemory(result, bzSoundName);
        yield return null;
        FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset(bzSoundName), Player.main.transform.position);

    }

    private static void CreateEventMemory(byte[] audioFile, string eventName)
    {
        if (CustomSoundHandler.TryGetCustomSound(eventName, out Sound _)) return;

        CREATESOUNDEXINFO exinfo = new() {
            cbsize = Marshal.SizeOf<CREATESOUNDEXINFO>(),
            length = (uint)audioFile.Length,
        };

        RuntimeManager.CoreSystem.createSound(audioFile, MODE.OPENMEMORY, ref exinfo, out Sound sound);
        CustomSoundHandler.RegisterCustomSound(eventName, sound, "bus:/master");
    }
}