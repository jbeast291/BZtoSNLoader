using System;
using System.IO;

namespace BZtoSNLoader;

public static class BZAudioLoader
{
    public static void Test()
    {
        var readers = FModBankParser.FModBankParser.LoadSoundBanks(new(@"C:\Program Files (x86)\Steam\steamapps\common\SubnauticaZero\SubnauticaZero_Data\StreamingAssets"));
        var outDir = new DirectoryInfo(@"C:\Users\jbeast\Downloads\GameOutput");
        foreach (var reader in readers)
        {
            try {
                FModBankParser.FModBankParser.ResolveAudioEvents(reader);
                var exportedAudio = FModBankParser.FModBankParser.ExportAudio(reader, outDir);

                if (exportedAudio.Success) {
                    Plugin.Logger.LogInfo($"Exported {exportedAudio.FilesExported} audio files to: {outDir.FullName}");
                } else {
                    Plugin.Logger.LogError($"FAILED");
                }
            } catch (Exception e) {
                Plugin.Logger.LogFatal(e.Message);
            }
        }
    }
}