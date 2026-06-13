#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class MainMenuSceneCreator : EditorWindow
{
    [MenuItem("Tools/Generate Main Menu Scene")]
    public static void GenerateMainMenuScene()
    {
        // 1. Create a new empty scene
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
            UnityEditor.SceneManagement.NewSceneMode.Single
        );

        // 2. Adjust default camera if found
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f); // Dark midnight blue background
            }
        }

        // 3. Create EventSystem if not exists
        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // 4. Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 5. Add MainMenuController to Canvas
        MainMenuController menuController = canvasObj.AddComponent<MainMenuController>();

        // 6. Create UI Background Image (Midnight blue glassmorphism theme)
        GameObject bgObj = new GameObject("BackgroundPanel", typeof(RectTransform));
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.08f, 1f); // Sleek dark aesthetic

        // 7. Create Center Menu Area
        GameObject menuAreaObj = new GameObject("MenuArea", typeof(RectTransform));
        menuAreaObj.transform.SetParent(canvasObj.transform, false);
        RectTransform menuAreaRect = menuAreaObj.GetComponent<RectTransform>();
        menuAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuAreaRect.pivot = new Vector2(0.5f, 0.5f);
        menuAreaRect.sizeDelta = new Vector2(500, 600);

        VerticalLayoutGroup layout = menuAreaObj.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 30f;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        // 8. Create Game Title Text
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
        titleObj.transform.SetParent(menuAreaObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(500, 100);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "COPY JJK GAME";
        titleText.fontSize = 55;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.0f, 0.8f, 1.0f, 1.0f); // Bright Cyan
        
        // Add subtle drop shadow to title text
        titleText.extraPadding = true;
        titleText.outlineWidth = 0.2f;
        titleText.outlineColor = Color.black;

        // Helper to create buttons
        Button playBtn = CreateStyledButton("PlayButton", menuAreaObj.transform, "PLAY", new Color(0.1f, 0.7f, 0.3f, 1.0f)); // Bright green
        Button tutorialBtn = CreateStyledButton("TutorialButton", menuAreaObj.transform, "TUTORIAL", new Color(0.1f, 0.5f, 0.8f, 1.0f)); // Premium blue
        Button settingsBtn = CreateStyledButton("SettingsButton", menuAreaObj.transform, "SETTINGS", new Color(0.4f, 0.4f, 0.45f, 1.0f)); // Charcoal gray

        // Hook up main menu buttons in Editor
        UnityEventTools.AddVoidPersistentListener(playBtn.onClick, menuController.PlayGame);
        UnityEventTools.AddVoidPersistentListener(tutorialBtn.onClick, menuController.OpenTutorial);
        UnityEventTools.AddVoidPersistentListener(settingsBtn.onClick, menuController.OpenSettings);

        // 9. Create Settings Popup Panel (Initially inactive)
        GameObject popupOverlayObj = new GameObject("SettingsPopup", typeof(RectTransform));
        popupOverlayObj.transform.SetParent(canvasObj.transform, false);
        RectTransform overlayRect = popupOverlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        
        Image overlayImg = popupOverlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.75f); // Dimmed screen background

        // Settings Dialog Window
        GameObject popupWinObj = new GameObject("PopupWindow", typeof(RectTransform));
        popupWinObj.transform.SetParent(popupOverlayObj.transform, false);
        RectTransform winRect = popupWinObj.GetComponent<RectTransform>();
        winRect.anchorMin = new Vector2(0.5f, 0.5f);
        winRect.anchorMax = new Vector2(0.5f, 0.5f);
        winRect.pivot = new Vector2(0.5f, 0.5f);
        winRect.sizeDelta = new Vector2(500, 450);
        
        Image winImg = popupWinObj.AddComponent<Image>();
        winImg.color = new Color(0.12f, 0.12f, 0.18f, 1.0f); // Sleek popup background
        // Add Outline to popup
        Outline winOutline = popupWinObj.AddComponent<Outline>();
        winOutline.effectColor = new Color(0.25f, 0.25f, 0.35f, 1.0f);
        winOutline.effectDistance = new Vector2(2, 2);

        // Popup Title Text
        GameObject popupTitleObj = new GameObject("PopupTitle", typeof(RectTransform));
        popupTitleObj.transform.SetParent(popupWinObj.transform, false);
        RectTransform popupTitleRect = popupTitleObj.GetComponent<RectTransform>();
        popupTitleRect.anchorMin = new Vector2(0.5f, 1f);
        popupTitleRect.anchorMax = new Vector2(0.5f, 1f);
        popupTitleRect.pivot = new Vector2(0.5f, 1f);
        popupTitleRect.anchoredPosition = new Vector2(0f, -30f);
        popupTitleRect.sizeDelta = new Vector2(400, 50);

        TextMeshProUGUI popupTitleText = popupTitleObj.AddComponent<TextMeshProUGUI>();
        popupTitleText.text = "SETTINGS";
        popupTitleText.fontSize = 32;
        popupTitleText.fontStyle = FontStyles.Bold;
        popupTitleText.alignment = TextAlignmentOptions.Center;
        popupTitleText.color = Color.white;

        // Content Area inside popup (vertical layout)
        GameObject contentArea = new GameObject("ContentArea", typeof(RectTransform));
        contentArea.transform.SetParent(popupWinObj.transform, false);
        RectTransform contentRect = contentArea.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0f, 10f);
        contentRect.sizeDelta = new Vector2(420, 250);

        VerticalLayoutGroup contentLayout = contentArea.AddComponent<VerticalLayoutGroup>();
        contentLayout.childAlignment = TextAnchor.MiddleCenter;
        contentLayout.spacing = 25f;
        contentLayout.childControlHeight = false;
        contentLayout.childControlWidth = false;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = false;

        // Volume row: Label + Slider + ValueText
        GameObject volumeRow = new GameObject("VolumeRow", typeof(RectTransform));
        volumeRow.transform.SetParent(contentArea.transform, false);
        RectTransform volRowRect = volumeRow.GetComponent<RectTransform>();
        volRowRect.sizeDelta = new Vector2(400, 50);

        GameObject volLabelObj = new GameObject("Label", typeof(RectTransform));
        volLabelObj.transform.SetParent(volumeRow.transform, false);
        RectTransform volLabelRect = volLabelObj.GetComponent<RectTransform>();
        volLabelRect.anchorMin = new Vector2(0f, 0.5f);
        volLabelRect.anchorMax = new Vector2(0f, 0.5f);
        volLabelRect.pivot = new Vector2(0f, 0.5f);
        volLabelRect.anchoredPosition = new Vector2(0f, 0f);
        volLabelRect.sizeDelta = new Vector2(100, 40);
        TextMeshProUGUI volLabel = volLabelObj.AddComponent<TextMeshProUGUI>();
        volLabel.text = "VOLUME";
        volLabel.fontSize = 18;
        volLabel.alignment = TextAlignmentOptions.MidlineLeft;

        // Create standard slider
        GameObject sliderObj = new GameObject("VolumeSlider", typeof(RectTransform));
        sliderObj.transform.SetParent(volumeRow.transform, false);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(10f, 0f);
        sliderRect.sizeDelta = new Vector2(180, 20);

        Slider slider = sliderObj.AddComponent<Slider>();
        
        // Background and Fill of slider
        GameObject sliderBg = new GameObject("Background", typeof(RectTransform));
        sliderBg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgSliderRect = sliderBg.GetComponent<RectTransform>();
        bgSliderRect.anchorMin = Vector2.zero;
        bgSliderRect.anchorMax = Vector2.one;
        bgSliderRect.sizeDelta = Vector2.zero;
        Image sliderBgImg = sliderBg.AddComponent<Image>();
        sliderBgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, 0);

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0f, 0.8f, 1.0f, 1f); // Cyan fill
        slider.fillRect = fillRect;

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        slider.handleRect = handleRect;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        // Value text
        GameObject valueTextObj = new GameObject("ValueText", typeof(RectTransform));
        valueTextObj.transform.SetParent(volumeRow.transform, false);
        RectTransform valTextRect = valueTextObj.GetComponent<RectTransform>();
        valTextRect.anchorMin = new Vector2(1f, 0.5f);
        valTextRect.anchorMax = new Vector2(1f, 0.5f);
        valTextRect.pivot = new Vector2(1f, 0.5f);
        valTextRect.anchoredPosition = new Vector2(0f, 0f);
        valTextRect.sizeDelta = new Vector2(70, 40);
        TextMeshProUGUI valText = valueTextObj.AddComponent<TextMeshProUGUI>();
        valText.text = "100%";
        valText.fontSize = 18;
        valText.alignment = TextAlignmentOptions.MidlineRight;

        // Mute Row: Label + Toggle
        GameObject muteRow = new GameObject("MuteRow", typeof(RectTransform));
        muteRow.transform.SetParent(contentArea.transform, false);
        RectTransform muteRowRect = muteRow.GetComponent<RectTransform>();
        muteRowRect.sizeDelta = new Vector2(400, 50);

        GameObject muteLabelObj = new GameObject("Label", typeof(RectTransform));
        muteLabelObj.transform.SetParent(muteRow.transform, false);
        RectTransform muteLabelRect = muteLabelObj.GetComponent<RectTransform>();
        muteLabelRect.anchorMin = new Vector2(0f, 0.5f);
        muteLabelRect.anchorMax = new Vector2(0f, 0.5f);
        muteLabelRect.pivot = new Vector2(0f, 0.5f);
        muteLabelRect.anchoredPosition = new Vector2(0f, 0f);
        muteLabelRect.sizeDelta = new Vector2(150, 40);
        TextMeshProUGUI muteLabel = muteLabelObj.AddComponent<TextMeshProUGUI>();
        muteLabel.text = "MUTE SOUND";
        muteLabel.fontSize = 18;
        muteLabel.alignment = TextAlignmentOptions.MidlineLeft;

        // Create standard Toggle
        GameObject toggleObj = new GameObject("MuteToggle", typeof(RectTransform));
        toggleObj.transform.SetParent(muteRow.transform, false);
        RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 0.5f);
        toggleRect.anchorMax = new Vector2(1f, 0.5f);
        toggleRect.pivot = new Vector2(1f, 0.5f);
        toggleRect.anchoredPosition = new Vector2(0f, 0f);
        toggleRect.sizeDelta = new Vector2(30, 30);

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        GameObject toggleBg = new GameObject("Background", typeof(RectTransform));
        toggleBg.transform.SetParent(toggleObj.transform, false);
        RectTransform toggleBgRect = toggleBg.GetComponent<RectTransform>();
        toggleBgRect.anchorMin = Vector2.zero;
        toggleBgRect.anchorMax = Vector2.one;
        toggleBgRect.sizeDelta = Vector2.zero;
        Image toggleBgImg = toggleBg.AddComponent<Image>();
        toggleBgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform));
        checkmark.transform.SetParent(toggleBg.transform, false);
        RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.1f, 0.1f);
        checkmarkRect.anchorMax = new Vector2(0.9f, 0.9f);
        checkmarkRect.sizeDelta = Vector2.zero;
        Image checkmarkImg = checkmark.AddComponent<Image>();
        checkmarkImg.color = new Color(0f, 0.8f, 1.0f, 1f);
        toggle.graphic = checkmarkImg;
        toggle.targetGraphic = toggleBgImg;

        // Close settings Button
        Button closeBtn = CreateStyledButton("CloseButton", contentArea.transform, "CLOSE", new Color(0.8f, 0.25f, 0.25f, 1.0f)); // Soft red
        
        // Hook close button click event
        UnityEventTools.AddVoidPersistentListener(closeBtn.onClick, menuController.CloseSettings);

        // 10. Hook up references to MenuController
        menuController.settingsPopup = popupOverlayObj;
        menuController.volumeSlider = slider;
        menuController.muteToggle = toggle;
        menuController.volumeText = valText;

        // 11. Save the Scene
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string scenePath = "Assets/Scenes/MainMenu.unity";
        bool saveSuccess = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
        
        if (saveSuccess)
        {
            Debug.Log($"MainMenu Scene successfully generated at {scenePath}");
            RegisterSceneInBuildSettings(scenePath);
        }
        else
        {
            Debug.LogError("Failed to save the generated MainMenu scene.");
        }
    }

    private static Button CreateStyledButton(string objectName, Transform parent, string label, Color color)
    {
        GameObject btnObj = new GameObject(objectName, typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320, 60);

        Image img = btnObj.AddComponent<Image>();
        img.color = color;
        // Rounded border effect using outline
        Outline btnOutline = btnObj.AddComponent<Outline>();
        btnOutline.effectColor = new Color(0f, 0f, 0f, 0.3f);
        btnOutline.effectDistance = new Vector2(1, 1);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        // Add visual micro-interactions
        btnObj.AddComponent<UIButtonEffects>();

        // Button label
        GameObject labelObj = new GameObject("Text", typeof(RectTransform));
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmpText = labelObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = label;
        tmpText.fontSize = 22;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        return btn;
    }

    private static void RegisterSceneInBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        // Remove existing duplicates of MainMenu scene if any
        buildScenes.RemoveAll(s => s.path == scenePath);

        // Insert at the front (index 0) so it is the default first scene
        buildScenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));

        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log($"MainMenu Scene registered in Build Settings at index 0.");
    }
}
#endif
