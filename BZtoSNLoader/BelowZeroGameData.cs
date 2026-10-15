using System.IO;
using DependencyDataFormat;

namespace BZtoSNLoader;

internal class BelowZeroGameData
{
    public BundleDependencyFormat BundleDependencyData { get; }

    private string BelowZeroAddressablesFolder { get; }

    public BelowZeroGameData(string belowZeroAddressablesFolder, BundleDependencyFormat dependencyData)
    {
        BelowZeroAddressablesFolder = belowZeroAddressablesFolder;
        BundleDependencyData = dependencyData;
    }

    public string GetPathToBelowZeroBundle(string bundleName) => Path.Combine(BelowZeroAddressablesFolder, bundleName);
}