using UnityEngine;

public class BulletTime : MonoBehaviour
{
    [Header("Bullet Time Settings")]
    public float slowMotionScale = 0.2f;
    public float normalTimeScale = 1.0f;

    [Header("Smooth Transition")]
    public float enterSpeed = 5f;
    public float exitSpeed  = 8f;

    [Header("Sound Settings")]
    public AudioClip bulletTimeEnterSound;
    public AudioClip bulletTimeExitSound;

    private bool  isActive       = false;
    private float targetTimeScale = 1f;

    /// <summary>true ขณะที่ Bullet Time กำลังทำงาน (slow motion)</summary>
    public bool IsActive => isActive;

    /// <summary>true เมื่อ timeScale กลับใกล้ปกติแล้ว (threshold 0.95)</summary>
    public bool IsTimeScaleNormal() => !isActive && Time.timeScale >= 0.95f;

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

        if (bulletTimeEnterSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(bulletTimeEnterSound);
        }
    }

    public void Exit()
    {
        isActive        = false;
        targetTimeScale = normalTimeScale;
        Debug.Log("[BulletTime] ออกจาก Slow Motion");

        if (bulletTimeExitSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(bulletTimeExitSound);
        }
    }

    void OnDestroy()
    {
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}   