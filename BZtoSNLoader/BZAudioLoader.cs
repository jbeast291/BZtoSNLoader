using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DependencyDataFormat;
using Fmod5Sharp.FmodTypes;
using FModBankParser;
using Nautilus.Handlers;
using Nautilus.Utility;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;
using UnityEngine.Bindings;
using Task = System.Threading.Tasks.Task;

namespace BZtoSNLoader;

public static class BZAudioLoader
{
    /// <summary>
    /// Holds a path to a FMOD audio file name. This is reefers to a fmod file compressed within a bank.
    /// A list of valid names is available here:
    /// https://raw.githubusercontent.com/jbeast291/BZtoSNLoader/refs/heads/master/BZtoSNLoader/Resources/fmodFilePaths.json
    /// </summary>
    /// <param name="name">The name of the file, do not include any extensions</param>
    /// <example><c>vo_Log_Cine_MeetAl_P5_Component_03</c></example>
    public readonly record struct FMODRawName(string name);
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Action{string, byte[]}"></typeparam>
    public union FMODRawNameCallback(Action<byte[]>, Action<string, byte[]>);

    private static bool allowNewWaitScreenAudioRequests = true;
    private static readonly Dictionary<string, List<RawNameLoadRequest>> waitScreenLoadRequests = new();
    private readonly record struct RawNameLoadRequest(
        FMODRawName rawName, 
        FMODFileLocatorFormat.FMODAudioLocator locator,
        FMODRawNameCallback onLoaded
    );

    /// <summary>
    /// Queue's an audio request
    /// </summary>
    /// <param name="rawName">The raw audio file from BZ. See <see cref="FMODRawName"/> for more details.</param>
    /// TODO: docs update
    /// <exception cref="Exception">This method throws an exception if 1) the Below Zero installation was not found or 2)
    /// the FMODRawName does not exist within BZ</exception>
    public static void RequestWaitScreenLoadAudio(FMODRawName rawName, FMODRawNameCallback onLoaded)
    {
        BZGameData.ThrowIfNotFound();

        GetLocator(rawName, out var locator);

        if(!allowNewWaitScreenAudioRequests) throw new Exception("Cannot register wait screen audio this late!");

        if (!waitScreenLoadRequests.TryGetValue(locator.bankName, 
                out List<RawNameLoadRequest> requests)
        ) {
            requests = new();
            waitScreenLoadRequests[locator.bankName] = requests;
        }

        requests.Add(new RawNameLoadRequest(rawName, locator, onLoaded));
    }

    private static void GetLocator(FMODRawName rawName, out FMODFileLocatorFormat.FMODAudioLocator locator) {
        if (!BZGameData.fmodFileLocatorData.fileNameToLocation.TryGetValue(rawName.name, out locator)) {
            throw new InvalidKeyException($"Invalid BZ FMOD Audio Name: \"{rawName}\"!");
        }
    }

    internal static void Register()
    {
        WaitScreenHandler.RegisterLateAsyncLoadTask(MyPluginInfo.PLUGIN_NAME, ProcessAllAudioRequests);
    }

    private static IEnumerator ProcessAllAudioRequests(WaitScreenHandler.WaitScreenTask task)
    {
        foreach ((string bankName, List<RawNameLoadRequest> requests) in waitScreenLoadRequests)
        {
            FModReader reader = null;
            Task bankLoad = Task.Run(() => {
                reader = FModBankParser.FModBankParser.LoadSoundBank(
                    new(BZGameData.GetBankFilePath(bankName))
                );
            });
            yield return AsyncUtils.WaitUntilTaskComplete(bankLoad);

            FmodSoundBank soundBank = reader.SoundBankData[0];
            yield return ProcessBankAudioRequests(soundBank, requests);
        }
        allowNewWaitScreenAudioRequests = false;
    }

    private static IEnumerator ProcessBankAudioRequests(FmodSoundBank soundBank, List<RawNameLoadRequest> requests)
    {
        foreach (RawNameLoadRequest request in requests)
        {
            byte[] data = null;
            Task fileLoad = Task.Run(() => {
                soundBank
                    .Samples[request.locator.sampleIndex]
                    .RebuildAsStandardFileFormat(out data, out _);
            });
            yield return AsyncUtils.WaitUntilTaskComplete(fileLoad);

            try {
                switch (request.onLoaded)
                {
                    case Action<byte[]> action: 
                        action.Invoke(data); 
                        break;
                    case Action<string, byte[]> actionWithName:
                        actionWithName.Invoke(request.rawName.name, data);
                        break;
                }
            } catch (Exception e) {
                Plugin.Logger.LogError("Error while executing onLoaded callback: " + e.Message);
            }
        }
    }

    internal static IEnumerator LoadRawAudioAsync(FMODRawName rawName, IOut<byte[]> result) 
    {
        BZGameData.ThrowIfNotFound();

        GetLocator(rawName, out var locator);

        Task decode = Task.Run(() => {
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
            //NOTE: soundBankIndex appears to always is SoundBankData.Count for BZ.
            //  Not sure why, but store this value in the future if that changes
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