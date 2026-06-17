using UnityEngine;

/// <summary>
/// ระบบแสดงค่า FPS บนหน้าจอเกม (OnGUI)
/// ใช้สำหรับวัดประสิทธิภาพก่อนและหลังการ Optimize
/// แนบสคริปต์นี้กับ GameObject ใดก็ได้ในฉาก (เช่น Main Camera)
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("อัปเดตตัวเลข FPS ทุกกี่วินาที")]
    public float updateInterval = 0.5f;

    [Tooltip("ขนาดฟอนต์")]
    public int fontSize = 22;

    // Internal
    private float accumulatedTime = 0f;
    private int   frameCount      = 0;
    private float currentFPS      = 0f;
    private float minFPS          = float.MaxValue;
    private float maxFPS          = 0f;

    private GUIStyle guiStyle;

    void Start()
    {
        // ตั้ง Application.targetFrameRate สำหรับ WebGL
        // WebGL จะถูกจำกัดโดย requestAnimationFrame (~60 Hz) อยู่แล้ว
        // แต่การตั้งค่านี้ช่วยให้ Unity ไม่พยายามเรนเดอร์เกินไป
#if !UNITY_EDITOR && UNITY_WEBGL
        Application.targetFrameRate = 60;
#endif
    }

    void Update()
    {
        // ใช้ unscaledDeltaTime เพื่อไม่ให้ Time.timeScale กระทบต่อค่า FPS
        accumulatedTime += Time.unscaledDeltaTime;
        frameCount++;

        if (accumulatedTime >= updateInterval)
        {
            currentFPS = frameCount / accumulatedTime;

            if (currentFPS < minFPS) minFPS = currentFPS;
            if (currentFPS > maxFPS) maxFPS = currentFPS;

            accumulatedTime = 0f;
            frameCount = 0;
        }
    }

    void OnGUI()
    {
        if (guiStyle == null)
        {
            guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.fontSize = fontSize;
            guiStyle.fontStyle = FontStyle.Bold;
            guiStyle.normal.textColor = Color.white;
        }

        // เปลี่ยนสีตามระดับ FPS
        if (currentFPS >= 30f)
            guiStyle.normal.textColor = Color.green;
        else if (currentFPS >= 15f)
            guiStyle.normal.textColor = Color.yellow;
        else
            guiStyle.normal.textColor = Color.red;

        // วาดพื้นหลังกึ่งโปร่งใส
        Rect bgRect = new Rect(8, 8, 280, 80);
        GUI.Box(bgRect, GUIContent.none);

        // แสดงข้อมูล FPS
        string fpsText = string.Format(
            "FPS: {0:F1}\nMin: {1:F1}  Max: {2:F1}\nFrame: {3:F1} ms",
            currentFPS,
            minFPS == float.MaxValue ? 0f : minFPS,
            maxFPS,
            currentFPS > 0f ? 1000f / currentFPS : 0f
        );

        GUI.Label(new Rect(14, 12, 270, 75), fpsText, guiStyle);
    }

    /// <summary>
    /// รีเซ็ตค่า Min/Max FPS (เรียกใช้เมื่อต้องการวัดใหม่)
    /// </summary>
    public void ResetMinMax()
    {
        minFPS = float.MaxValue;
        maxFPS = 0f;
    }
}
