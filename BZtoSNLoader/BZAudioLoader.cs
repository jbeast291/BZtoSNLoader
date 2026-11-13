using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DependencyDataFormat;
using Fmod5Sharp.FmodTypes;
using FModBankParser;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;

namespace BZtoSNLoader;

public static class BZAudioLoader
{
    public readonly record struct FMODAudioName(string Path);

    public static void LoadAudioAsync(FMODAudioName audioName, IOut<byte[]> result)
    {
        if (!BZGameData.BelowZeroFound) {
            throw new Exception("BZ Asset Loader not initialized! Check the console for previous path errors");
        }

        if (!BZGameData.fmodFileLocatorData.fileNameToLocation.TryGetValue(audioName.Path, 
                out FMODFileLocatorFormat.FMODAudioLocator locator)
        ) {
            throw new InvalidKeyException($"Invalid BZ FMOD Audio Name: \"{audioName}\"!");
        }

        FModReader reader = FModBankParser.FModBankParser.LoadSoundBank(
            new(@$"C:\Program Files (x86)\Steam\steamapps\common\SubnauticaZero\SubnauticaZero_Data\StreamingAssets\{locator.bankName}.bank"));

        reader.SoundBankData[0].Samples[locator.sampleIndex].RebuildAsStandardFileFormat(out byte[] data, out _);

        result.Set(data);
    }

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
}