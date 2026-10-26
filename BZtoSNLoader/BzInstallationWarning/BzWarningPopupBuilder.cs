using Nautilus.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BZtoSNLoader.BzInstallationWarning;

internal static class BzWarningPopupBuilder
{
    internal static void CreateMissingBzWarningPopup(uGUI_MainMenu mainMenu)
    {
        var optionsWindowReference = mainMenu.transform.Find("Panel/Options");

        CreateDarkBackground(optionsWindowReference.parent.GetComponent<RectTransform>());

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

        // Add header
        var menuHeader = menuPanel.transform.Find("Header");
        menuPanel.transform.Find("Middle/TabsHolder").gameObject.SetActive(false);
        SetUpHeaderTitle(menuHeader);

        string leftButtonName = "Bottom/ButtonBack";
        string rightButtonName = "Bottom/ButtonApply";

        // Add 'open plugins folder' button
        var pluginsButton = menuPanel.transform.Find(rightButtonName).gameObject;
        Object.DestroyImmediate(pluginsButton.GetComponentInChildren<TranslationLiveUpdate>());
        pluginsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Open Plugin Folder";
        var pluginsButtonComponent = pluginsButton.GetComponent<Button>();
        pluginsButtonComponent.onClick.RemoveAllListeners();
        pluginsButtonComponent.onClick.AddListener(menuBehaviour.OnButtonOpenPluginFolder);

        // Add 'edit path' button
        var editPathButton = menuPanel.transform.Find(leftButtonName).gameObject;
        Object.DestroyImmediate(editPathButton.GetComponentInChildren<TranslationLiveUpdate>());
        editPathButton.GetComponentInChildren<TextMeshProUGUI>().text = "Edit BZ Path";
        var editPathButtonComponent = editPathButton.GetComponent<Button>();
        editPathButtonComponent.onClick.RemoveAllListeners();
        editPathButtonComponent.onClick.AddListener(menuBehaviour.OnButtonEditPath);
 
        // Add 'open plugins folder' button
        var quitButton = Object.Instantiate(editPathButton.gameObject, editPathButton.transform.parent);
        Object.DestroyImmediate(quitButton.GetComponentInChildren<TranslationLiveUpdate>());
        quitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit Game";
        var quitButtonComponent = quitButton.GetComponent<Button>();
        quitButtonComponent.onClick.RemoveAllListeners();
        quitButtonComponent.onClick.AddListener(menuBehaviour.OnButtonQuit);

        // Create main body
        var mainPaneContent = mainPaneTransform.Find("Viewport/Content").GetComponent<RectTransform>();
        AddTextContents(mainPaneContent, editPathButton.gameObject, menuBehaviour);
        mainPaneContent.GetComponent<VerticalLayoutGroup>().spacing = 45;

        // Reactivate menu panel
        menuPanel.SetActive(true);
    }

    private static void CreateDarkBackground(RectTransform parent)
    {
        var panel = new GameObject("TransparentBackgroundForBZWarning");
        panel.layer = 5;
        panel.transform.parent = panel.transform;
        var rect = panel.AddComponent<RectTransform>();
        rect.SetParent(parent);
        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = panel.AddComponent<Image>();
        image.color = new Color(0.05f, 0.05f, 0.05f, 0.5f);
    }

    private static void AddTextContents(RectTransform parent, GameObject buttonReference,
        BzInstallationPopupBehavior behavior)
    {
        AddText("TopText", parent, 32,
            "The <b><color=#fbaa1a>BZtoSNLoader modding library</color></b> failed to load!\n\n" +
            "This error appears when you have mods that load Below Zero assets but your installation of\n" +
            "Subnautica: Below Zero <b>could not be automatically detected</b>.\n\n" +
            "This error prevents you from playing the game because the risk of serious errors.");

        var purchaseButton = Object.Instantiate(buttonReference, parent);
        Object.DestroyImmediate(purchaseButton.GetComponentInChildren<TranslationLiveUpdate>());
        var purchaseText = purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
        purchaseText.text = "Purchase Subnautica: Below Zero on Steam";
        purchaseText.fontStyle = FontStyles.Underline;
        purchaseText.fontSize = 50;
        var purchaseButtonComponent = purchaseButton.GetComponent<Button>();
        purchaseButtonComponent.onClick.RemoveAllListeners();
        purchaseButtonComponent.onClick.AddListener(behavior.OnButtonPurchaseBelowZero);
        var purchaseRect = purchaseButtonComponent.GetComponent<RectTransform>();
        purchaseRect.localPosition = Vector3.zero;
        purchaseRect.localRotation = Quaternion.identity;
        purchaseRect.localScale = Vector3.one;
        purchaseButton.AddComponent<LayoutElement>().preferredHeight = 90;

        AddText("FixText", parent, 28,
            "Make sure Subnautica: Below Zero is installed on your PC. If you don't own the game, you must purchase and\n" +
            "install it. You may need to manually edit the path to point to your installation. Do so by pressing \"Edit BZ Path\"");

        AddText("UninstallText", parent, 28,
            "If you are unable to purchase the game, it is recommended that you locate the plugin folder, " +
            "close the game,\nand remove the BZtoSNLoader mod and any other mods that depend on it.");

        AddText("ReloadRequiredText", parent, 30, "<u>A game reload is required!</u>");
    }

    private static void AddText(string name, RectTransform parent, float fontSize, string text)
    {
        var topTextRect = new GameObject(name).AddComponent<RectTransform>();
        topTextRect.gameObject.layer = 5;
        topTextRect.SetParent(parent);
        topTextRect.localPosition = Vector3.zero;
        topTextRect.localRotation = Quaternion.identity;
        topTextRect.localScale = Vector3.one;
        var topText = topTextRect.gameObject.AddComponent<TextMeshProUGUI>();
        topText.font = FontUtils.Aller_Rg;
        topText.text = text;
        topText.fontSize = fontSize;
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