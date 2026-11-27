using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DependencyDataFormat;
using FMOD;
using Fmod5Sharp.FmodTypes;
using FModBankParser;
using FMODUnity;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Utility;
using Newtonsoft.Json;
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
        yield return LoadRawAudioAsync(new(bzSoundName), requestResult);
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

        RuntimeManager.CoreSystem.createSound(audioFile, MODE.OPENMEMORY | MODE.CREATESAMPLE, ref exinfo, out Sound sound);
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
    
    internal static IEnumerator LoadRawAudioAsync(BZAudioLoader.FMODRaw raw, IOut<byte[]> result) 
    {
        BZGameData.ThrowIfNotFound();

        FMODAudioLocator locator = BZGameData.fmodFileLocatorData.fileNameToLocation[raw.name];

        System.Threading.Tasks.Task decode = System.Threading.Tasks.Task.Run(() => {
            FModReader reader = FModBankParser.FModBankParser.LoadSoundBank(
                new(BZGameData.GetBankFilePath(locator.bankName))
            );

            reader
                .SoundBankData[0]
                .Samples[locator.sampleIndex]
                .RebuildAsStandardFileFormat(out byte[] data, out _);
            reader = null; // for gc stuff, don't remove
            result.Set(data);
        });
        yield return AsyncUtils.WaitUntilTaskComplete(decode);
    }

#if DEBUG
    private static void DumpAudioPaths()
    {
        IEnumerable<FModReader> bankReaders = FModBankParser.FModBankParser.LoadSoundBanks(
            new(@"C:\Program Files (x86)\Steam\steamapps\common\SubnauticaZero\SubnauticaZero_Data\StreamingAssets")
        );

        FMODFileLocatorFormat locator = new();

        foreach (FModReader bankReader in bankReaders)
        {
            //NOTE: soundBankIndex appears to always have SoundBankData.Count = 0 for BZ.
            for (int soundBankIndex = 0; soundBankIndex < bankReader.SoundBankData.Count; soundBankIndex++)
            {
                FmodSoundBank soundBank = bankReader.SoundBankData[soundBankIndex];
                if (soundBank.Samples.Count == 0) continue;
                for (int sampleIndex = 0; sampleIndex < soundBank.Samples.Count; sampleIndex++)
                {
                    var sample = soundBank.Samples[sampleIndex];
                    var sampleName = sample.Name;
                    if (string.IsNullOrWhiteSpace(sampleName))
                    {
                        sampleName = bankReader.SoundTable?.TryGetKeyByIndex(sampleIndex, out var key) == true
                            ? $"Sample_{sampleIndex}_hash_{key:X16}"
                            : $"Sample_{sampleIndex}";
                    }

                    if (locator.fileNameToLocation.ContainsKey(sampleName)) {
                        Plugin.Logger.LogDebug("Skipping dump of duplicate samples between bundles...");
                        continue;
                    }

                    locator.fileNameToLocation.Add(sampleName, new(bankReader.BankName, sampleIndex));
                }
            }
        }

        string dumpPath = Plugin.GetFilePathNextToPlugin("fmodFilePaths.json");
        string jsonText = JsonConvert.SerializeObject(locator, Formatting.Indented);
        File.WriteAllText(dumpPath, jsonText);
    }
#endif
}