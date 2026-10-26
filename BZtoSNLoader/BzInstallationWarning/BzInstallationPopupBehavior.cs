using System.IO;
using UnityEngine;

namespace BZtoSNLoader.BzInstallationWarning;

internal static class BzInstallationPopupBehavior
{
    internal static void OnButtonQuit() => Application.Quit();
    internal static void OnButtonEditPath()
    {
        string path = Path.Combine(BepInEx.Paths.ConfigPath, $"{PluginInfo.PLUGIN_GUID}.cfg");
        Application.OpenURL(path);
    }

    internal static void OnButtonOpenPluginFolder() 
        => Application.OpenURL(Path.GetDirectoryName(Plugin.Assembly.Location));

    internal static void OnButtonPurchaseBelowZero() 
        => Application.OpenURL("https://store.steampowered.com/app/848450/Subnautica_Below_Zero/");
}