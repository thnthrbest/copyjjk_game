using UnityEngine;

public class BulletTime : MonoBehaviour
{
    [Header("Bullet Time Settings")]
    public float slowMotionScale = 0.2f;
    public float normalTimeScale = 1.0f;

    [Header("Smooth Transition")]
    public float enterSpeed = 5f;
    public float exitSpeed  = 8f;

    private bool  isActive       = false;
    private float targetTimeScale = 1f;

    // ─── เรียกจาก HandInputReceiver ───
    public static BulletTime Instance;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        SmoothTimeScale();
    }

    void SmoothTimeScale()
    {
        float speed = isActive ? enterSpeed : exitSpeed;

        Time.timeScale = Mathf.Lerp(
            Time.timeScale,
            targetTimeScale,
            speed * Time.unscaledDeltaTime
        );

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void Enter()
    {
        isActive        = true;
        targetTimeScale = slowMotionScale;
        Debug.Log("[BulletTime] เข้า Slow Motion");
    }

    public void Exit()
    {
        isActive        = false;
        targetTimeScale = normalTimeScale;
        Debug.Log("[BulletTime] ออกจาก Slow Motion");
    }

    void OnDestroy()
    {
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void OnGUI()
    {
        GUIStyle style  = new GUIStyle();
        style.fontSize  = 18;
        style.fontStyle = FontStyle.Bold;

        if (isActive)
        {
            style.normal.textColor = Color.cyan;
            GUI.Label(new Rect(10, 70, 300, 30), "BULLET TIME ACTIVE", style);
        }
    }
}   