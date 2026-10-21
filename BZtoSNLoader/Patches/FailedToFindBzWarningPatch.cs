using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BZtoSNLoader.Patches;

[HarmonyPatch]
public static class FailedToFindBzWarningPatch
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
            CreateMissingBzWarningPopup(mainMenu);
        }
        catch
        {
            Plugin.Logger.LogError("Failed to create missing BZ warning popup! Exception thrown");
            throw;
        }
    }

    private static void CreateMissingBzWarningPopup(uGUI_MainMenu mainMenu)
    {
        var optionsWindowReference = mainMenu.transform.Find("Panel/Options");
        var menuPanel =
            Object.Instantiate(optionsWindowReference.gameObject, optionsWindowReference.parent, true);
        menuPanel.name = "BzMissingWindow";
        var menuBehaviour = menuPanel.AddComponent<MenuBehavior>();
        menuPanel.SetActive(false);
        var optionsPanel = menuPanel.GetComponentInChildren<uGUI_OptionsPanel>();
        var panePrefab = optionsPanel.panePrefab;
        Object.DestroyImmediate(optionsPanel);
        var menuHeader = menuPanel.transform.Find("Header");
        menuHeader.gameObject.GetComponent<TextMeshProUGUI>().text =
            $"{PluginInfo.PLUGIN_NAME} mod - Below Zero installation not found";
        Object.DestroyImmediate(menuHeader.GetComponent<TranslationLiveUpdate>());
        menuPanel.transform.Find("Middle/TabsHolder").gameObject.SetActive(false);
        var panesHolder = menuPanel.transform.Find("Middle/PanesHolder");
        foreach (Transform child in panesHolder)
        {
            Object.Destroy(child.gameObject);
        }

        // var mainPaneTransform = Object.Instantiate(panePrefab, panesHolder).transform;
        // var mainPaneContent = mainPaneTransform.Find("Viewport/Content");

        // Add quit button
        var quitButton = menuPanel.transform.Find("Bottom/ButtonBack").gameObject;
        Object.DestroyImmediate(quitButton.GetComponentInChildren<TranslationLiveUpdate>());
        quitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit game";
        var quitButtonComponent = quitButton.GetComponent<Button>();
        quitButtonComponent.onClick.RemoveAllListeners();
        quitButtonComponent.onClick.AddListener(menuBehaviour.OnButtonQuit);

        // Add edit path button
        var editPathButton = menuPanel.transform.Find("Bottom/ButtonApply").gameObject;
        Object.DestroyImmediate(editPathButton.GetComponentInChildren<TranslationLiveUpdate>());
        editPathButton.GetComponentInChildren<TextMeshProUGUI>().text = "Edit path";
        var editPathButtonComponent = editPathButton.GetComponent<Button>();
        editPathButtonComponent.onClick.RemoveAllListeners();
        editPathButtonComponent.onClick.AddListener(menuBehaviour.OnButtonEditPath);
        
        // reactivate menu panel
        menuPanel.SetActive(true);
    }

    private class MenuBehavior : MonoBehaviour
    {
        public void OnButtonQuit() => Application.Quit();
        public void OnButtonEditPath() => throw new NotImplementedException();
    }
}