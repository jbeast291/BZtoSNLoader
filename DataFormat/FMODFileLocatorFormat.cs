using System.Collections.Generic;
using Newtonsoft.Json;

namespace DependencyDataFormat;

internal class FMODFileLocatorFormat
{
    [JsonProperty] internal Dictionary<string, FMODAudioLocator> fileNameToLocation { get; set; } = new();

    internal record struct FMODAudioLocator(string bankName, int sampleIndex);
}