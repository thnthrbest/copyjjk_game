using UnityEngine;

public class cowskill : MonoBehaviour
{
    [Header("Cow Settings")]
    public GameObject cowPrefab;            // Prefab of the Cow (must have cowobj script)
    public float castProtectDuration = 1.0f; // Brief invincibility to protect player while casting
    public float forwardOffset = 2.0f;       // Distance in front of player to spawn the cow
    public AudioClip summonSound;            // Summon sound effect

    [Header("Cooldown")]
    public float cooldown = 10f;             // Skill cooldown

    private bool isActive = false;
    public bool IsActive => isActive;        // Returns true if player is currently in casting protection

    private bool isOnCooldown = false;
    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Press 4 to use skill (for testing)
        if (Input.GetKeyDown(KeyCode.Alpha4) && !isActive && !isOnCooldown)
        {
            StartSkill();
        }
    }

    // =========================
    // เริ่มใช้สกิล
    // =========================
    public void StartSkill()
    {
        if (isActive || isOnCooldown) return;

        // Check if skill is unlocked
        SkillManager sm = GetComponent<SkillManager>();
        if (sm != null && !sm.IsSkillUnlocked("cow"))
        {
            Debug.LogWarning("[CowSkill] สกิลวัวยังไม่ได้ปลดล็อค! ไม่สามารถใช้งานได้");
            return;
        }

        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();

        if (cowPrefab == null)
        {
            Debug.LogWarning("[CowSkill] ยังไม่ได้ใส่ cowPrefab");
            return;
        }

        float extraDur = Charms.CharmManager.Instance != null ? Charms.CharmManager.Instance.GetBullExtraDuration() : 0f;
        float finalProtectDuration = castProtectDuration + extraDur;

        // 1. เปิดโหมดเทพป้องกันตอนร่ายสกิล (Cast Protection)
        if (playerHealth != null)
        {
            playerHealth.godMode = true;
            playerHealth.activeDuration = finalProtectDuration;
        }

        // 2. สร้างวัวพุ่งไปข้างหน้า (Cowobj will handle its own movement & destruction)
        if (summonSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFXAtPoint(summonSound, transform.position);
        }

        Vector3 spawnPos = transform.position + (transform.forward * forwardOffset);
        Quaternion spawnRot = Quaternion.LookRotation(transform.forward);
        GameObject spawnedCow = Instantiate(cowPrefab, spawnPos, spawnRot);

        cowobj co = spawnedCow.GetComponent<cowobj>();
        if (co != null)
        {
            co.Setup(transform);
        }
        else
        {
            Debug.LogWarning("[CowSkill] cowPrefab ไม่มีคอมโพเนนต์ cowobj!");
        }

        isActive = true;
        isOnCooldown = true;

        Debug.Log($"[CowSkill] เสกวัวพุ่งชนและคุ้มกันผู้เล่นชั่วคราว ({finalProtectDuration} วิ)");

        // จบคุ้มกันร่ายสกิลหลังครบเวลา
        Invoke(nameof(EndProtect), finalProtectDuration);

        // คูลดาวน์หมด → ใช้ใหม่ได้
        Invoke(nameof(ResetCooldown), cooldown);
    }

    // =========================
    // จบคุ้มกันการร่ายสกิล
    // =========================
    void EndProtect()
    {
        if (playerHealth != null)
        {
            playerHealth.godMode = false;
        }

        isActive = false;
        Debug.Log("[CowSkill] หมดเวลาคุ้มกันการร่ายสกิล");
    }

    // =========================
    // รีเซ็ตคูลดาวน์
    // =========================
    void ResetCooldown()
    {
        isOnCooldown = false;
        Debug.Log("[CowSkill] สกิลวัวพร้อมใช้แล้ว");
    }
}
