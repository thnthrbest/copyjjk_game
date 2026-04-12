using System;     
using System.Collections.Generic;
using UnityEngine;

public class rabbitskill : MonoBehaviour
{
   [Header("Shield Settings")]
    public GameObject shieldPrefab;
    public float      shieldRadius   = 5f;
    public LayerMask  bulletLayer;
    public float distanceToBullet = 50f;

    [Header("Auto Settings")]
    public float activeDuration  = 20f;
    public float checkInterval   = 0.3f;

    [Header("Cooldown")]
    public float cooldown        = 10f;

    private bool  isActive       = false;
    private bool  isOnCooldown   = false;
    private float activeTimer    = 0f;

    // ─── เก็บ bullet ที่มีโล่บังอยู่แล้ว ───
    private HashSet<GameObject> blockedBullets = new HashSet<GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2) && !isActive && !isOnCooldown)
            StartSkill();

        if (isActive)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= activeDuration)
                StopSkill();
        }
    }

    public void StartSkill()
    {
        // ─── เช็ค Stack ก่อนใช้ ───
        if (!PlayerEnergy.Instance.UseStack(1))
        {
            Debug.Log("[Skill] Stack ไม่พอ!");
            return;
        }

        isActive    = true;
        activeTimer = 0f;
        blockedBullets.Clear();

        InvokeRepeating(nameof(TryBlockBullet), 0f, checkInterval);
        Debug.Log($"[Shield] Skill เปิดแล้ว! ทำงาน {activeDuration} วิ");
    }

    void StopSkill()
    {
        isActive = false;
        blockedBullets.Clear();
        CancelInvoke(nameof(TryBlockBullet));

        isOnCooldown = true;
        Invoke(nameof(ResetCooldown), cooldown);
        Debug.Log("[Shield] Skill หมดเวลา — รอ Cooldown");
    }

    void TryBlockBullet()
    {
        // ─── ล้าง bullet ที่ถูกทำลายไปแล้วออกจาก Set ───
        blockedBullets.RemoveWhere(b => b == null);

        Collider[] bullets = Physics.OverlapSphere(
            transform.position, shieldRadius, bulletLayer
        );

        if (bullets.Length == 0) return;

        foreach (Collider b in bullets)
        {
            // ─── ข้ามถ้ามีโล่บังอยู่แล้ว ───
            if (blockedBullets.Contains(b.gameObject)) continue;

            // ─── สร้างโล่บัง bullet ลูกนี้ ───
            SpawnShieldBetween(b.transform);
            blockedBullets.Add(b.gameObject);
        }
    }

    void SpawnShieldBetween(Transform bullet)
{
    Vector3    dirToBullet    = (bullet.position - transform.position).normalized;
    Vector3    shieldPosition = transform.position + dirToBullet * distanceToBullet;
    Quaternion shieldRotation = Quaternion.LookRotation(dirToBullet);

    GameObject shield = Instantiate(shieldPrefab, shieldPosition, shieldRotation);

    // ✅ เก็บ reference ไว้ก่อนที่ bullet จะถูกทำลาย
    GameObject bulletRef = bullet.gameObject;

    rabbitobj so = shield.GetComponent<rabbitobj>();
    if (so != null)
    {
        so.targetBullet = bulletRef;
        so.onDestroyed  = () => blockedBullets.Remove(bulletRef);  // ← ใช้ bulletRef แทน bullet.gameObject
    }

    Debug.Log($"[Shield] สร้างโล่บัง — เหลือเวลา {activeDuration - activeTimer:F1} วิ");
}

    void ResetCooldown()
    {
        isOnCooldown = false;
        Debug.Log("[Shield] Skill พร้อมใช้แล้ว");
    }

    void OnGUI()
    {
        GUIStyle style   = new GUIStyle();
        style.fontSize   = 20;

        if (isActive)
        {
            float remaining = activeDuration - activeTimer;
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(10, 50, 300, 30),
                $"Shield Active: {remaining:F1}s", style);
        }
        else if (isOnCooldown)
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(10, 50, 300, 30), "Shield Cooldown...", style);
        }
        else
        {
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(10, 50, 300, 30), "Press 2 for Shield", style);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shieldRadius);
    }
}