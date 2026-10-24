using HarmonyLib;
using Nautilus.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BZtoSNLoader.BzInstallationWarning;

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
        var menuBehaviour = menuPanel.AddComponent<BzInstallationPopupBehavior>();
        menuPanel.SetActive(false);
        var optionsPanel = menuPanel.GetComponentInChildren<uGUI_OptionsPanel>();
        Object.DestroyImmediate(optionsPanel);
        
        // Clean up old menu
        var panesHolder = menuPanel.transform.Find("Middle/PanesHolder");
        foreach (Transform child in panesHolder)
        {
            Object.Destroy(child.gameObject);
        }
        
        var panePrefab = optionsPanel.panePrefab;
        var mainPaneTransform = Object.Instantiate(panePrefab, panesHolder).transform;
        var mainPaneContent = mainPaneTransform.Find("Viewport/Content").GetComponent<RectTransform>();
        AddTextContents(mainPaneContent);

        // Add header
        var menuHeader = menuPanel.transform.Find("Header");
        menuPanel.transform.Find("Middle/TabsHolder").gameObject.SetActive(false);
        SetUpHeaderTitle(menuHeader);

        string leftButtonName = "Bottom/ButtonBack";
        string rightButtonName = "Bottom/ButtonApply";
        
        // Add quit button
        var quitButton = menuPanel.transform.Find(rightButtonName).gameObject;
        Object.DestroyImmediate(quitButton.GetComponentInChildren<TranslationLiveUpdate>());
        quitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit game";
        var quitButtonComponent = quitButton.GetComponent<Button>();
        quitButtonComponent.onClick.RemoveAllListeners();
        quitButtonComponent.onClick.AddListener(menuBehaviour.OnButtonQuit);

        // Add 'edit path' button
        var editPathButton = menuPanel.transform.Find(leftButtonName).gameObject;
        Object.DestroyImmediate(editPathButton.GetComponentInChildren<TranslationLiveUpdate>());
        editPathButton.GetComponentInChildren<TextMeshProUGUI>().text = "Edit path";
        var editPathButtonComponent = editPathButton.GetComponent<Button>();
        editPathButtonComponent.onClick.RemoveAllListeners();
        editPathButtonComponent.onClick.AddListener(menuBehaviour.OnButtonEditPath);

        // Add 'open plugins folder' button
        var openPluginsFolderButton = Object.Instantiate(editPathButton.gameObject, editPathButton.transform.parent);
        Object.DestroyImmediate(openPluginsFolderButton.GetComponentInChildren<TranslationLiveUpdate>());
        openPluginsFolderButton.GetComponentInChildren<TextMeshProUGUI>().text = "Open plugins folder";
        var openPluginsButtonComponent = openPluginsFolderButton.GetComponent<Button>();
        openPluginsButtonComponent.onClick.RemoveAllListeners();
        openPluginsButtonComponent.onClick.AddListener(menuBehaviour.OnButtonOpenPluginsFolder);
        
        // Reactivate menu panel
        menuPanel.SetActive(true);
    }

    /*
    private static void CreateBackground()
    {
        var image = new GameObject("TransparentBackgroundForBZWarning");
        image.layer = 5;
        image.transform.parent = image.transform;
    }*/

    private static void AddTextContents(RectTransform parent)
    {
        var topTextRect = new GameObject("TopText").AddComponent<RectTransform>();
        topTextRect.gameObject.layer = 5;
        topTextRect.SetParent(parent);
        topTextRect.localPosition = Vector3.zero;
        topTextRect.localRotation = Quaternion.identity;
        topTextRect.localScale = Vector3.one;
        var topText = topTextRect.gameObject.AddComponent<TextMeshProUGUI>();
        topText.font = FontUtils.Aller_Rg;
        topText.text = "The <b><color=#fbaa1a>BZtoSNLoader modding library</color></b> failed to load!\n\n" +
                       "This error appears when you have mods that load Below Zero assets but your installation of " +
                       "Subnautica: Below Zero <b>could not be automatically detected</b>.\n\n" +
                       "This error prevents you from playing the game because the risk of serious errors.";
    }

    private static void SetUpHeaderTitle(Transform menuHeader)
    {
        Object.DestroyImmediate(menuHeader.GetComponent<TranslationLiveUpdate>());

        var rect = menuHeader.GetComponent<RectTransform>();
        var offsetMin = rect.offsetMin;
        offsetMin.y = -96;
        rect.offsetMin = offsetMin;
        var offsetMax = rect.offsetMax;
        offsetMax.y = 0;
        rect.offsetMax = offsetMax;
        var text = menuHeader.GetComponent<TextMeshProUGUI>();
        text.text = "Below Zero installation not found";
        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.enableAutoSizing = true;
        text.fontSizeMax = 70;
    }
}