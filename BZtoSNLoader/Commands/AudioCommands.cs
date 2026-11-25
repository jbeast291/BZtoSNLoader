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
        => CoroutineHost.StartCoroutine(PlayAudioAsync(bzSoundName));

    private static IEnumerator PlayAudioAsync(string bzSoundName)
    {
        TaskResult<byte[]> requestResult = new();
        yield return BZAudioLoader.LoadRawAudioAsync(new(bzSoundName), requestResult);
        byte[] result = requestResult.Get();
        EnsureCreateEventMemory(result, bzSoundName);
        RuntimeManager.StudioSystem.flushCommands();
        FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset(bzSoundName), Player.main.transform.position);
    }

    private static void EnsureCreateEventMemory(byte[] audioFile, string eventName)
    {
        if (CustomSoundHandler.TryGetCustomSound(eventName, out _)) return;

        CREATESOUNDEXINFO exinfo = new() {
            cbsize = Marshal.SizeOf<CREATESOUNDEXINFO>(),
            length = (uint)audioFile.Length,
        };

        RuntimeManager.CoreSystem.createSound(audioFile, MODE.OPENMEMORY, ref exinfo, out Sound sound);
        CustomSoundHandler.RegisterCustomSound(eventName, sound, "bus:/master");
    }

    [ConsoleCommand("galaudiobz")]
    private static void PlayAllBzAudio()
        => CoroutineHost.StartCoroutine(PlayAllAudioAsync());

    private static IEnumerator PlayAllAudioAsync()
    {
        var audioDict = BZGameData.fmodFileLocatorData.fileNameToLocation;
        int total = audioDict.Count;
        int current = 0;
        foreach (string fileName in audioDict.Keys)
        {
            Plugin.Logger.LogInfo($"Loading... {current++}/{total} | {fileName}");
            yield return PlayAudioAsync(fileName);
        }
    }
}