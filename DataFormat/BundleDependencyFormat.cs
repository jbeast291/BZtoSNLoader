using System.Collections.Generic;

namespace DependencyDataFormat;

public class BundleDependencyFormat
{
    internal const int latestVersion = 1;
    public int Version { get; private set; } = latestVersion;
    public Dictionary<string, List<string>> assetPathToBundleDeps { get; set; } = new();
}