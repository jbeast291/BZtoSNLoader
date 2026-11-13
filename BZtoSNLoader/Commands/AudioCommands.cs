using System.Runtime.InteropServices;
using FMOD;
using FMODUnity;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace BZtoSNLoader;

public static class AudioCommands
{
    [ConsoleCommand("" +
                    "audioBZ" +
                    "")]
    private static void SpawnBzAsset(string soundName)
    {
        TaskResult<byte[]> requestResult = new();
        BZAudioLoader.LoadAudioAsync(new(soundName), requestResult);
        byte[] result = requestResult.Get();

        CREATESOUNDEXINFO exinfo;
        unsafe
        {
            exinfo = new() {
                cbsize = sizeof(CREATESOUNDEXINFO),
                length = (uint)result.Length
            };
        }

        RuntimeManager.CoreSystem.createSound(result, MODE.OPENMEMORY, ref exinfo, out Sound sound);
        CustomSoundHandler.RegisterCustomSound(soundName, sound, AudioUtils.BusPaths.PlayerSFXs);
        FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset(soundName), Player.main.transform.position);
    }
}