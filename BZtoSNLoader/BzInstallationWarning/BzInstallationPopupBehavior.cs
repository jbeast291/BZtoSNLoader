using System.IO;
using UnityEngine;

namespace BZtoSNLoader.BzInstallationWarning;

internal class BzInstallationPopupBehavior : MonoBehaviour
{
    internal void OnButtonQuit() => Application.Quit();
    internal void OnButtonEditPath()
    {
        var path = Path.Combine(BepInEx.Paths.ConfigPath, $"{PluginInfo.PLUGIN_GUID}.cfg");
        if (!File.Exists(path))
        {
            ErrorMessage.AddMessage("<color=#FF0000>No config file found</color>");
            return;
        }
        Application.OpenURL(path);
    }

    internal void OnButtonOpenPluginFolder()
    {
        Application.OpenURL(Path.GetDirectoryName(Plugin.Assembly.Location));
    }

    internal void OnButtonPurchaseBelowZero()
    {
        Application.OpenURL("https://store.steampowered.com/app/848450/Subnautica_Below_Zero/");
    }
}