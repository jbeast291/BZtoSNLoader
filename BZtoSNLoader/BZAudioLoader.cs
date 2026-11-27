using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DependencyDataFormat;
using Fmod5Sharp.FmodTypes;
using FModBankParser;
using FMODUnity;
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
    public readonly record struct FMODRaw(string name);

    // flow: request -> enqueued -> wait screen -> sort by bank -> fill results -> process callbacks
    private static bool allowNewWaitScreenAudioRequests = true;
    private static readonly Dictionary<FMODRaw, List<RawNameResult>> queuedRequests = new();
    
    /// <summary>
    /// The result of a FMODRaw load request. Can be used to register the audio file with nautilus. 
    /// </summary>
    /// <param name="raw">Same raw provided in the request</param>
    /// <param name="data">The resulting data file. Subnautica: Below Zero files are all .ogg files.
    /// This can be registered with FMOD directly in memory or written to disk for other use.</param>
    /// <remarks>Modification of the <paramref name="data"/> is strictly forbidden.
    /// It is left as a writeable array to make registration easier with FMOD, but the reference
    /// is shared between callbacks and modifying it will break other mods</remarks>
    public record struct RawNameResult(FMODRaw raw, byte[] data);
 
    private static readonly List<CallbackGroup> callbackGroups = new();
    private record CallbackGroup(RawNameResult[] results, FMODRawNameCallback onLoaded);

    private union FMODRawNameCallback(Action<RawNameResult>, Action<RawNameResult[]>);

    /// <summary>
    /// Enqueues a request to load a given <see cref="FMODRaw"/> file. The request is executed by
    /// the <see cref="BZAudioLoader"/> during a <see cref="WaitScreenHandler.RegisterLateAsyncLoadTask">
    /// LateLoadTask</see>. The <paramref name="onLoaded"/> callback will be executed with the result
    /// of the operation. 
    /// </summary>
    /// <exception cref="Exception">This method throws an exception if 1) the Below Zero
    /// installation was not found or 2) an InvalidKeyException if the FMODRaw file is invalid</exception>
    /// <param name="raw">The FMODRaw file to request during the wait screen.
    /// See <see cref="FMODRaw"/> for valid values.</param>
    /// <param name="onLoaded">Callback to be executed with the audio file result.</param>
    /// <remarks>A call to <c>RuntimeManager.StudioSystem.flushCommands();</c> is done after all
    /// callbacks are completed. There is no need to call this within the callback.</remarks>
    public static void RequestWaitScreenLoadAudio(FMODRaw raw, Action<RawNameResult> onLoaded) 
        => EnqueueWaitScreenLoad([raw], onLoaded);

    /// <summary>
    /// Enqueues a request to load given <see cref="FMODRaw"/> files. The request is executed by
    /// the <see cref="BZAudioLoader"/> during a <see cref="WaitScreenHandler.RegisterLateAsyncLoadTask">
    /// LateLoadTask</see>. The <paramref name="onLoaded"/> callback will be executed with the results
    /// of the operation.
    /// </summary>
    /// <exception cref="Exception">This method throws an exception if 1) the Below Zero
    /// installation was not found or 2) an InvalidKeyException if the FMODRaw file is invalid</exception>
    /// <param name="raws">The FMODRaw files to request during the wait screen.
    /// See <see cref="FMODRaw"/> for valid values.</param>
    /// <param name="onLoaded">Callback to be executed with the audio files, in the same order requested</param>
    /// <remarks>A call to <c>RuntimeManager.StudioSystem.flushCommands();</c> is done after all
    /// callbacks are completed. There is no need to call this within the callback.</remarks>
    public static void RequestWaitScreenLoadAudio(FMODRaw[] raws, Action<RawNameResult[]> onLoaded)
        => EnqueueWaitScreenLoad(raws, onLoaded);

    private static void EnqueueWaitScreenLoad(FMODRaw[] rawNames, FMODRawNameCallback onLoaded)
    {
        BZGameData.ThrowIfNotFound();

        if(!allowNewWaitScreenAudioRequests) {
            throw new Exception("Cannot register wait screen audio this late!");
        }

        AssertValidRawNames(rawNames);

        CallbackGroup group = new(new RawNameResult[rawNames.Length], onLoaded);
        for(int i = 0; i < rawNames.Length; i++) {
            FMODRaw raw = rawNames[i];
            RawNameResult result = new(raw, null);
            group.results[i] = result;

            if (!queuedRequests.TryGetValue(raw, out List<RawNameResult> results)) {
                results = new();
                queuedRequests[raw] = results;
            }
            results.Add(result);
        }
        callbackGroups.Add(group);
    }

    private static void AssertValidRawNames(FMODRaw[] rawNames) {
        foreach (FMODRaw rawName in rawNames) {
            if (!BZGameData.fmodFileLocatorData.fileNameToLocation.TryGetValue(
                    rawName.name, out _)
            ) {
                throw new InvalidKeyException($"Invalid BZ FMOD Audio Name: \"{rawNames}\"!");
            }
        }
    }

    // Note: Assumes the locator is valid
    private static FMODAudioLocator GetLocator(FMODRaw raw) 
        => BZGameData.fmodFileLocatorData.fileNameToLocation[raw.name];

    internal static void Register() 
        => WaitScreenHandler.RegisterLateAsyncLoadTask(MyPluginInfo.PLUGIN_NAME, ProcessAllAudioRequests);

    private record RawLocator(FMODRaw Raw, FMODAudioLocator locator);
    private static IEnumerator ProcessAllAudioRequests(WaitScreenHandler.WaitScreenTask task)
    {
        allowNewWaitScreenAudioRequests = false;
        RawLocator[] withLocators = queuedRequests.Keys
            .Select(rN => new RawLocator(rN, GetLocator(rN)))
            .ToArray();

        Dictionary<string, List<RawLocator>> requestsByBank = new();
        foreach (RawLocator withLocator in withLocators)
        {
            if (!requestsByBank.TryGetValue(withLocator.locator.bankName,
                    out List<RawLocator> locators)
            ) {
                locators = new();
                requestsByBank[withLocator.locator.bankName] = locators;
            }
            locators.Add(withLocator);
        }

        foreach ((string bankName, List<RawLocator> requestsWithinBank) in requestsByBank)
        {
            FModReader reader = null;
            Task bankLoad = Task.Run(() => {
                reader = FModBankParser.FModBankParser.LoadSoundBank(
                    new(BZGameData.GetBankFilePath(bankName))
                );
            });
            yield return AsyncUtils.WaitUntilTaskComplete(bankLoad);

            FmodSoundBank soundBank = reader.SoundBankData[0];
            yield return ProcessBankAudioRequests(soundBank, requestsWithinBank);
            soundBank = null; // explicit GC stuff
        }

        ProcessCallback: foreach (CallbackGroup callback in callbackGroups)
        {
            if (callback == null) continue;
            foreach (RawNameResult result in callback.results)
            {
                if (result.data != null) continue;
                Plugin.Logger.LogError($"Failed to load data for {result.raw.name}." +
                                       $" Skipping callbacks...");
                continue ProcessCallback;
            }

            try {
                switch (callback.onLoaded) {
                    case Action<RawNameResult> onLoaded:
                        onLoaded.Invoke(callback.results[0]);
                        break;
                    case Action<RawNameResult[]> onLoaded:
                        onLoaded.Invoke(callback.results);
                        break;
                }
            } catch (Exception e){
                Plugin.Logger.LogError($"Exception thrown while executing callback. {e}");
            }
        }
        callbackGroups.Clear();
        queuedRequests.Clear();
        RuntimeManager.StudioSystem.flushCommands();
    }

    private static IEnumerator ProcessBankAudioRequests(FmodSoundBank soundBank, List<RawLocator> requests)
    {
        foreach (RawLocator request in requests)
        {
            byte[] data = null;
            Task rawFileProcess = Task.Run(() => {
                soundBank
                    .Samples[request.locator.sampleIndex]
                    .RebuildAsStandardFileFormat(out data, out _);
            });
            yield return AsyncUtils.WaitUntilTaskComplete(rawFileProcess);

            queuedRequests[request.Raw].ForEach((result) => result.data = data);
        }
    }
}