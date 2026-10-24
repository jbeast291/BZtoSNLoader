using System.IO;
using UnityEngine;

namespace BZtoSNLoader.BzInstallationWarning;

internal class BzInstallationPopupBehavior : MonoBehaviour
{
    public void OnButtonQuit() => Application.Quit();
    public void OnButtonEditPath()
    {
        var path = Path.Combine(BepInEx.Paths.ConfigPath, $"{PluginInfo.PLUGIN_GUID}.cfg");
        if (!File.Exists(path))
        {
            ErrorMessage.AddMessage("<color=#FF0000>No config file found</color>");
            return;
        }
        Application.OpenURL(path);
    }

    public void OnButtonOpenPluginsFolder()
    {
        Application.OpenURL(BepInEx.Paths.PluginPath);
    }
}