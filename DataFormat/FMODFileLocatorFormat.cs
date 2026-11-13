using System.Collections.Generic;
using Newtonsoft.Json;

namespace DependencyDataFormat;

public class FMODFileLocatorFormat
{
    [JsonProperty] internal Dictionary<string, FMODAudioLocator> fileNameToLocation { get; set; } = new();

    public record struct FMODAudioLocator(string bankName, int sampleIndex);
}