using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Charms;

/// <summary>
/// Displays current run collected Charm Boxes on the gameplay UI during stage runs (e.g. stage_1).
/// </summary>
public class StageBoxUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI boxCountText;
    public Text legacyBoxCountText;

    [Header("Display Settings")]
    public string displayFormat = "x{0}";

    private void OnEnable()
    {
        if (CharmManager.Instance != null)
        {
            CharmManager.Instance.OnBoxesCountChanged += UpdateBoxCountUI;
            UpdateBoxCountUI(CharmManager.Instance.GetRunBoxesCount());
        }
        else
        {
            UpdateBoxCountUI(0);
        }
    }

    private void OnDisable()
    {
        if (CharmManager.Instance != null)
        {
            CharmManager.Instance.OnBoxesCountChanged -= UpdateBoxCountUI;
        }
    }

    private void Start()
    {
        if (boxCountText == null && legacyBoxCountText == null)
        {
            boxCountText = GetComponent<TextMeshProUGUI>();
            if (boxCountText == null)
            {
                legacyBoxCountText = GetComponent<Text>();
            }
        }

        if (CharmManager.Instance != null)
        {
            UpdateBoxCountUI(CharmManager.Instance.GetRunBoxesCount());
        }
    }

    public void UpdateBoxCountUI(int count)
    {
        string textValue = string.Format(displayFormat, count);

        if (boxCountText != null)
        {
            boxCountText.text = textValue;
        }

        if (legacyBoxCountText != null)
        {
            legacyBoxCountText.text = textValue;
        }
    }

    /// <summary>
    /// Auto-initializes Box UI HUD element on the scene Canvas if none exists in stage_1.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitStageBoxUI()
    {
        if (FindObjectOfType<StageBoxUI>() != null) return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            GameObject boxUiObj = new GameObject("StageBoxUI_HUD");
            boxUiObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = boxUiObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1); // Top Right
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-30, -30);
            rect.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI tmpText = boxUiObj.AddComponent<TextMeshProUGUI>();
            tmpText.fontSize = 28;
            tmpText.alignment = TextAlignmentOptions.Right;
            tmpText.color = new Color(1.0f, 0.85f, 0.2f); // Gold color
            tmpText.fontStyle = FontStyles.Bold;

            StageBoxUI stageBoxUI = boxUiObj.AddComponent<StageBoxUI>();
            stageBoxUI.boxCountText = tmpText;

            if (CharmManager.Instance != null)
            {
                stageBoxUI.UpdateBoxCountUI(CharmManager.Instance.GetRunBoxesCount());
            }
            else
            {
                stageBoxUI.UpdateBoxCountUI(0);
            }
        }
    }
}
