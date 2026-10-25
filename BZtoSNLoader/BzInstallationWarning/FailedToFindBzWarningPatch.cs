using HarmonyLib;

namespace BZtoSNLoader.BzInstallationWarning;

[HarmonyPatch]
internal static class FailedToFindBzWarningPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Start))]
    private static void MainMenuStartPostfix(uGUI_MainMenu __instance)
    {
        //if (!BZGameData.BelowZeroFound)
            TryShowPopup(__instance);
    }

    private static void TryShowPopup(uGUI_MainMenu mainMenu)
    {
        if (mainMenu == null)
        {
            Plugin.Logger.LogError("Main menu not found while trying to show missing BZ installation popup");
        }

        try
        {
            BzWarningPopupBuilder.CreateMissingBzWarningPopup(mainMenu);
        }
        catch
        {
            Plugin.Logger.LogError("Failed to create missing BZ warning popup! Exception thrown");
            throw;
        }
    }
}