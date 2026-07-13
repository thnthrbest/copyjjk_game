using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// ระบบจัดการสถานะ (Status Effect) ของผู้เล่น
/// รองรับ: ไฟเผา (Burn), โหมดป้องกันสถานะ (Status Resist), ล้างสถานะ (Cleanse)
/// </summary>
public class PlayerStatusEffect : MonoBehaviour
{
    // ─── Enums ───────────────────────────────────────────
    public enum StatusType { None, Burn }

    // ─── Burn Settings ───────────────────────────────────
    [Header("Burn Effect Settings")]
    [Tooltip("ดาเมจไฟต่อวินาที")]
    public float burnDamagePerSecond = 3f;
    [Tooltip("ระยะเวลาไฟเผา (วินาที)")]
    public float burnDuration = 10f;
    [Tooltip("อนุภาคไฟ (Particle) ที่จะแสดงเมื่อถูกเผา (ไม่บังคับ)")]
    public ParticleSystem burnParticle;

    // ─── Status Resist Settings ──────────────────────────
    [Header("Status Resist Mode")]
    [Tooltip("เวลาที่โหมดป้องกันสถานะทำงาน (วินาที)")]
    public float resistDuration = 5f;
    [Tooltip("UI Image แสดงว่าโหมด Resist กำลังทำงาน (ไม่บังคับ)")]
    public Image resistIcon;

    // ─── Cleanse UI (optional) ───────────────────────────
    [Header("UI (Optional)")]
    [Tooltip("UI แสดง Icon สถานะไฟ (ไม่บังคับ)")]
    public Image burnIcon;
    [Tooltip("UI Text แสดงเวลาไฟที่เหลือ (ไม่บังคับ)")]
    public Text burnTimerText;

    // ─── Private State ───────────────────────────────────
    private bool isBurning        = false;
    private bool isResisting      = false;
    private Coroutine burnCoroutine   = null;
    private Coroutine resistCoroutine = null;

    private PlayerHealth playerHealth;

    // ─── Public Read-only Properties ─────────────────────
    public bool IsBurning   => isBurning;
    public bool IsResisting => isResisting;

    // ─── Current Status (สำหรับ UI / Debug) ──────────────
    public StatusType CurrentStatus => isBurning ? StatusType.Burn : StatusType.None;

    // ─────────────────────────────────────────────────────
    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // อัปเดต UI Timer ไฟ (ถ้ามี)
        if (burnTimerText != null)
        {
            burnTimerText.text = isBurning ? $"🔥 {burnDuration:F1}s" : "";
        }
    }

    // ─────────────────────────────────────────────────────
    //  APPLY BURN — เรียกจาก FireBullet เมื่อกระสุนถูกผู้เล่น
    // ─────────────────────────────────────────────────────
    public void ApplyBurn(float damage = -1f, float duration = -1f)
    {
        if (playerHealth != null && playerHealth.IsDead()) return;

        // ถ้ากำลัง Resist อยู่ → ปัดทิ้ง
        if (isResisting)
        {
            Debug.Log("[Status] ผู้เล่นกำลัง Resist: ป้องกันสถานะ Burn สำเร็จ!");
            return;
        }

        float dmg = damage  > 0f ? damage  : burnDamagePerSecond;
        float dur  = duration > 0f ? duration : burnDuration;

        // ถ้าไฟเผาอยู่แล้ว → รีเซ็ตเวลาใหม่
        if (isBurning && burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }

        burnCoroutine = StartCoroutine(BurnRoutine(dmg, dur));
    }

    // ─────────────────────────────────────────────────────
    //  CLEANSE — ล้างสถานะทั้งหมด
    // ─────────────────────────────────────────────────────
    public void Cleanse()
    {
        if (isBurning && burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }

        isBurning = false;
        StopBurnVFX();
        Debug.Log("[Status] ล้างสถานะสำเร็จ!");
    }

    // ─────────────────────────────────────────────────────
    //  ACTIVATE RESIST — เปิดโหมดป้องกันสถานะชั่วคราว
    // ─────────────────────────────────────────────────────
    public void ActivateResist(float duration = -1f)
    {
        float dur = duration > 0f ? duration : resistDuration;

        // ถ้ากำลัง Resist อยู่แล้ว → ต่อเวลา
        if (isResisting && resistCoroutine != null)
        {
            StopCoroutine(resistCoroutine);
        }

        resistCoroutine = StartCoroutine(ResistRoutine(dur));
        Debug.Log($"[Status] เปิดโหมด Resist {dur}s");
    }

    // ─────────────────────────────────────────────────────
    //  CLEANSE + RESIST — ล้างสถานะแล้วเปิด Resist ทันที
    // ─────────────────────────────────────────────────────
    public void CleanseAndResist(float resistDurationOverride = -1f)
    {
        Cleanse();
        ActivateResist(resistDurationOverride);
    }

    // ─────────────────────────────────────────────────────
    //  COROUTINES
    // ─────────────────────────────────────────────────────
    private IEnumerator BurnRoutine(float dmgPerSec, float duration)
    {
        isBurning = true;
        PlayBurnVFX();
        UpdateBurnUI(true);

        float elapsed = 0f;
        float tickInterval = 1f; // ดาเมจทุก 1 วินาที

        Debug.Log($"[Burn] เริ่มถูกเผา: {dmgPerSec} ดาเมจ/วิ เป็นเวลา {duration} วิ");

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;

            if (playerHealth == null || playerHealth.IsDead()) break;

            // ใช้ TakeDamageOverTime เพื่อไม่ให้ Invincibility frame ขัด
            playerHealth.TakeDamageOverTime(dmgPerSec);
            Debug.Log($"[Burn] โดนไฟ {dmgPerSec} HP | เหลือ {elapsed}/{duration}s");
        }

        isBurning = false;
        StopBurnVFX();
        UpdateBurnUI(false);
        Debug.Log("[Burn] สถานะไฟดับแล้ว");
    }

    private IEnumerator ResistRoutine(float duration)
    {
        isResisting = true;
        UpdateResistUI(true);

        yield return new WaitForSeconds(duration);

        isResisting = false;
        UpdateResistUI(false);
        Debug.Log("[Status] โหมด Resist หมดเวลาแล้ว");
    }

    // ─────────────────────────────────────────────────────
    //  VFX & UI Helpers
    // ─────────────────────────────────────────────────────
    private void PlayBurnVFX()
    {
        if (burnParticle != null)
        {
            burnParticle.gameObject.SetActive(true);
            burnParticle.Play();
        }
    }

    private void StopBurnVFX()
    {
        if (burnParticle != null)
        {
            burnParticle.Stop();
            burnParticle.gameObject.SetActive(false);
        }
    }

    private void UpdateBurnUI(bool active)
    {
        if (burnIcon != null)
            burnIcon.gameObject.SetActive(active);
    }

    private void UpdateResistUI(bool active)
    {
        if (resistIcon != null)
            resistIcon.gameObject.SetActive(active);
    }

    // ─────────────────────────────────────────────────────
    //  DEBUG / Inspector Buttons (เรียกด้วย Context Menu)
    // ─────────────────────────────────────────────────────
    [ContextMenu("TEST: Apply Burn")]
    private void DebugApplyBurn() => ApplyBurn();

    [ContextMenu("TEST: Cleanse Status")]
    private void DebugCleanse() => Cleanse();

    [ContextMenu("TEST: Activate Resist")]
    private void DebugResist() => ActivateResist();

    [ContextMenu("TEST: Cleanse + Resist")]
    private void DebugCleanseResist() => CleanseAndResist();
}
