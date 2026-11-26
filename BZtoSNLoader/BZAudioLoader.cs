using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DependencyDataFormat;
using Fmod5Sharp.FmodTypes;
using FModBankParser;
using Nautilus.Utility;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;
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
    /// Asynchronously loads the given raw audio file from a BZ FMOD bank file.
    /// </summary>
    /// <remarks>
    /// This method can cause GC stutters as it loads entire banks into memory.
    /// It is preferred to load sounds on the loading screen/main menu before actual gameplay.
    /// </remarks>
    /// <param name="rawName">The raw audio file from BZ. See <see cref="FMODRawName"/> for more details.</param>
    /// <param name="result">The interface used to output the resulting audio file as an array of bytes.
    /// See <seealso cref="TaskResult{T}"/>.</param>
    /// <exception cref="Exception">This method throws an exception if 1) the Below Zero installation was not found or 2)
    /// the FMODRawName does not exist within BZ</exception>
    /// <example>The following example code loads an Alan voiceline and registers it with Nautilus
    /// custom sound's handler on the <c>"bus:/master"</c>. It must be executed in a coroutine.
    /// <code>
    /// TaskResult&lt;byte[]&gt; requestResult = new();
    /// yield return BZAudioLoader.LoadRawAudioAsync(new("vo_Log_Cine_MeetAl_P5_Component_03"), requestResult);
    /// byte[] result = requestResult.Get();
    /// </code>
    /// Register with nautilus:
    /// <code>
    /// CREATESOUNDEXINFO exinfo = new() {
    ///     cbsize = Marshal.SizeOf&lt;CREATESOUNDEXINFO&gt;(),
    ///     length = (uint)result.Length,
    /// };
    /// RuntimeManager.CoreSystem.createSound(result, MODE.OPENMEMORY | MODE.CREATESAMPLE, ref exinfo, out Sound sound);
    /// CustomSoundHandler.RegisterCustomSound(eventName, sound, "bus:/master");
    /// </code>
    /// </example>
    public static IEnumerator LoadRawAudioAsync(FMODRawName rawName, IOut<byte[]> result)
    {
        if (!BZGameData.BelowZeroFound) {
            throw new Exception("BZ Asset Loader not initialized! Check the console for previous path errors");
        }

        if (!BZGameData.fmodFileLocatorData.fileNameToLocation.TryGetValue(rawName.name, 
                out FMODFileLocatorFormat.FMODAudioLocator locator)
        ) {
            throw new InvalidKeyException($"Invalid BZ FMOD Audio Name: \"{rawName}\"!");
        }

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