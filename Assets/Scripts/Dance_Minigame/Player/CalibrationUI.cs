using UnityEngine;
using TMPro;

/// <summary>
/// แสดงข้อความแนะนำผู้เล่นระหว่าง Calibrate
/// อ่านค่าจาก HandDataReceiver.CalibPhase และ CalibCountdown
/// ติดบน GameObject ที่เป็น UI แยกต่างหาก (calibrateUIRoot)
/// </summary>
public class CalibrationUI : MonoBehaviour
{
    [Header("UI Text")]
    [Tooltip("ข้อความหลัก เช่น 'กางมือออก!'")]
    public TextMeshProUGUI instructionText;

    [Tooltip("ตัวเลขถอยหลัง เช่น '1.5'")]
    public TextMeshProUGUI countdownText;

    [Header("FPS ของ Python (ใช้แปลง frames → วินาที)")]
    [Tooltip("ปกติ Python รันที่ ~30 fps")]
    public float pythonFPS = 30f;

    void Update()
    {
        UpdateUI(HandDataReceiver.CalibPhase, HandDataReceiver.CalibCountdown);
    }

    void UpdateUI(string phase, int framesLeft)
    {
        float secondsLeft = framesLeft / pythonFPS;

        switch (phase)
        {
            case "ready":
                SetText("Prepare...", $"", secondsLeft);
                break;
            case "open":
                SetText("Open your hand", "", secondsLeft);
                break;
            case "close":
                SetText("Close your hand", "", secondsLeft);
                break;
            case "done":
            default:
                SetText("Ready!", "", 0f);
                break;
        }
    }

    void SetText(string instruction, string sub, float seconds)
    {
        if (instructionText != null)
            instructionText.text = instruction;

        if (countdownText != null)
            countdownText.text = seconds > 0f ? $"{seconds:F1}" : "";
    }
}