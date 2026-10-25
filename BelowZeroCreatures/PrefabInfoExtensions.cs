using Nautilus.Assets;

namespace BelowZeroCreatures;

public static class PrefabInfoExtensions
{
    public static PrefabInfo WithFolderPath(this PrefabInfo info, string folderPath)
    {
        return info.WithFileName($"{folderPath}/{info.ClassID}");
    }
}