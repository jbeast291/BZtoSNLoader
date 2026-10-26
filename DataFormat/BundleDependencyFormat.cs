using System.Collections.Generic;

namespace DependencyDataFormat;

public class BundleDependencyFormat
{
    public Dictionary<string, List<string>> assetPathToBundleDeps { get; set; } = new();
}