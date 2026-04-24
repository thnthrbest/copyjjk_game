using System;     
using System.Collections.Generic;
using UnityEngine;

public class rabbitskill : MonoBehaviour
{
   [Header("Shield Settings")]
    public GameObject shieldPrefab;
    public float      shieldRadius   = 5f;
    public LayerMask  bulletLayer;

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
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.godMode = true; // เปิดโหมดเทพ
        }

        isActive    = true;
        activeTimer = 0f;
        blockedBullets.Clear();

        InvokeRepeating(nameof(TryBlockBullet), 0f, checkInterval);
        Debug.Log($"[Shield] Skill เปิดแล้ว! ทำงาน {activeDuration} วิ");
    }

    void StopSkill()
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.godMode = false; // ปิดโหมดเทพ
        }
        isActive = false;
        blockedBullets.Clear();
        CancelInvoke(nameof(TryBlockBullet));

        isOnCooldown = true;
        Invoke(nameof(ResetCooldown), cooldown);
        Debug.Log("[Shield] Skill หมดเวลา — รอ Cooldown");
    }

    void TryBlockBullet()
    {
        blockedBullets.RemoveWhere(b => b == null);

        Collider[] bullets = Physics.OverlapSphere(
            transform.position, shieldRadius, bulletLayer
        );

        if (bullets.Length == 0) return;

        foreach (Collider b in bullets)
        {
            if (b == null) continue;

            // ─── ข้าม rabbitobj ตัวเอง ───
            if (b.CompareTag("Magic")) continue;

            if (blockedBullets.Contains(b.gameObject)) continue;

            blockedBullets.Add(b.gameObject);
            SpawnShieldBetween(b.transform);
        }
    }

    void SpawnShieldBetween(Transform bullet)
    {
        if (bullet == null) return;

        Vector3    dirToBullet    = (bullet.position - transform.position).normalized;
        Vector3    shieldPosition = transform.position + dirToBullet * 40.0f;
        Quaternion shieldRotation = Quaternion.LookRotation(dirToBullet);

        GameObject shield = Instantiate(shieldPrefab, shieldPosition, shieldRotation);

        GameObject bulletRef = bullet.gameObject;

        rabbitobj so = shield.GetComponent<rabbitobj>();
        if (so != null)
        {
            so.targetBullet = bulletRef;

            // ─── ตอน shield ถูกทำลาย → เอา bullet ออกจาก Set ───
            so.onDestroyed = () =>
            {
                if (bulletRef != null)
                    blockedBullets.Remove(bulletRef);
            };
        }
        else
        {
            // ─── ถ้าไม่มี rabbitobj → เอาออกทันที ───
            blockedBullets.Remove(bulletRef);
            Debug.LogWarning("[Shield] shieldPrefab ไม่มี rabbitobj!");
        }
    }

    void ResetCooldown()
    {
        isOnCooldown = false;
        Debug.Log("[Shield] Skill พร้อมใช้แล้ว");
    }

    // void OnGUI()
    // {
    //     GUIStyle style   = new GUIStyle();
    //     style.fontSize   = 20;

    //     if (isActive)
    //     {
    //         float remaining = activeDuration - activeTimer;
    //         style.normal.textColor = Color.green;
    //         GUI.Label(new Rect(10, 50, 300, 30),
    //             $"Shield Active: {remaining:F1}s", style);
    //     }
    //     else if (isOnCooldown)
    //     {
    //         style.normal.textColor = Color.red;
    //         GUI.Label(new Rect(10, 50, 300, 30), "Shield Cooldown...", style);
    //     }
    //     else
    //     {
    //         style.normal.textColor = Color.white;
    //         GUI.Label(new Rect(10, 50, 300, 30), "Press 2 for Shield", style);
    //     }
    // }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shieldRadius);
    }
}