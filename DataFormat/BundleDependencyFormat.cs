using System.Collections.Generic;
using Newtonsoft.Json;

namespace DependencyDataFormat;

internal class BundleDependencyFormat
{
    [JsonProperty] internal Dictionary<string, List<string>> assetPathToBundleDeps { get; set; } = new();
}