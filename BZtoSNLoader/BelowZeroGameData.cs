using System.IO;
using DependencyDataFormat;

namespace BZtoSNLoader;

internal class BelowZeroGameData
{
    public string BelowZeroRootFolder { get; }
    public BundleDependencyFormat BundleDependencyData { get; }
    
    private string BelowZeroAddressablesFolder { get; }

    public BelowZeroGameData(string belowZeroRootFolder, string belowZeroAddressablesFolder, BundleDependencyFormat dependencyData)
    {
        BelowZeroRootFolder = belowZeroRootFolder;
        BelowZeroAddressablesFolder = belowZeroAddressablesFolder;
        BundleDependencyData = dependencyData;
    }
    
    public string GetPathToBelowZeroBundle(string bundleName) => Path.Combine(BelowZeroAddressablesFolder, bundleName);
}