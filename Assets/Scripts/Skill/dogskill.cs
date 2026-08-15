using UnityEngine;

public class dogskill : MonoBehaviour
{
    [Header("Dog Settings")]
    public GameObject dogPrefab; 
    public GameObject dogPrefab2;        // Prefab ของหมา
    public Transform spawnPoint;         // จุดอ้างอิง (ถ้าไม่ใส่จะใช้ Player)
    
    public float sideDistance = 3f;      // ระยะซ้าย/ขวาจาก Player
    public float forwardDistance = 2f;   // ระยะด้านหน้าจาก Player
    public float activeDuration = 5f;    // เวลาที่หมาอยู่ในฉาก
    public AudioClip summonSound;        // Summon sound effect

    [Header("Cooldown")]
    public float cooldown = 10f;         // คูลดาวน์สกิล

    private bool isActive = false;
    private bool isOnCooldown = false;

    private GameObject leftDog;
    private GameObject rightDog;
    private GameObject centerDog;

    void Update()
    {
        // กดปุ่ม 3 เพื่อใช้สกิล
        if (Input.GetKeyDown(KeyCode.Alpha3) && !isActive && !isOnCooldown)
        {
            StartSkill();
        }
    }

    // =========================
    // เริ่มใช้สกิล
    // =========================
    public void StartSkill()
    {
        // Check if skill is unlocked
        SkillManager sm = GetComponent<SkillManager>();
        if (sm != null && !sm.IsSkillUnlocked("dog"))
        {
            Debug.LogWarning("[DogSkill] สกิลสุนัขยังไม่ได้ปลดล็อค! ไม่สามารถใช้งานได้");
            return;
        }

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.godMode = true; // เปิดโหมดเทพ
        }
        
        if (dogPrefab == null)
        {
            Debug.LogWarning("[DogSkill] ยังไม่ได้ใส่ dogPrefab");
            return;
        }

        if (summonSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFXAtPoint(summonSound, transform.position);
        }

        playerHealth.activeDuration = activeDuration;
        Transform point = spawnPoint != null ? spawnPoint : transform;

        // ตำแหน่งซ้าย
        Vector3 leftPos =
            point.position
            + (-transform.right * sideDistance)
            + (transform.forward * forwardDistance);

        // ตำแหน่งขวา
        Vector3 rightPos =
            point.position
            + (transform.right * sideDistance)
            + (transform.forward * forwardDistance);

        // ให้หมาหันไปทางเดียวกับ Player
        Quaternion dogRot = Quaternion.LookRotation(transform.forward);

        // สร้างหมา 2 ตัวพื้นฐาน
        leftDog = Instantiate(dogPrefab, leftPos, dogRot);
        rightDog = Instantiate(dogPrefab2, rightPos, dogRot);

        // เช็คโบนัสเครื่องราง (+1 ตัว)
        int extraWolf = Charms.CharmManager.Instance != null ? Charms.CharmManager.Instance.GetWolfExtraSummonCount() : 0;
        if (extraWolf > 0)
        {
            Vector3 centerPos = point.position + (transform.forward * (forwardDistance + 1.5f));
            centerDog = Instantiate(dogPrefab, centerPos, dogRot);
            Debug.Log("[DogSkill] เรียกหมาตัวที่ 3 ตรงกลางจากโบนัสเครื่องราง!");
        }

        isActive = true;
        isOnCooldown = true;

        Debug.Log("[DogSkill] เรียกหมาเรียบร้อยแล้ว");

        // หมดเวลา → ลบหมา
        Invoke(nameof(EndSkill), activeDuration);

        // คูลดาวน์หมด → ใช้ใหม่ได้
        Invoke(nameof(ResetCooldown), cooldown);
    }

    // =========================
    // จบสกิล
    // =========================
    void EndSkill()
    {
        if (leftDog != null)
            Destroy(leftDog);

        if (rightDog != null)
            Destroy(rightDog);

        if (centerDog != null)
            Destroy(centerDog);

        isActive = false;


        Debug.Log("[DogSkill] สกิลจบแล้ว");
    }

    // =========================
    // รีเซ็ตคูลดาวน์
    // =========================
    void ResetCooldown()
    {
        isOnCooldown = false;

        Debug.Log("[DogSkill] Skill พร้อมใช้แล้ว");
    }

    // =========================
    // Debug Scene View
    // =========================
    void OnDrawGizmosSelected()
    {
        Transform point = spawnPoint != null ? spawnPoint : transform;

        Vector3 leftPos =
            point.position
            + (-transform.right * sideDistance)
            + (transform.forward * forwardDistance);

        Vector3 rightPos =
            point.position
            + (transform.right * sideDistance)
            + (transform.forward * forwardDistance);

        Gizmos.color = isActive ? Color.green : Color.cyan;

        Gizmos.DrawWireSphere(leftPos, 1f);
        Gizmos.DrawWireSphere(rightPos, 1f);

        Gizmos.DrawLine(transform.position, leftPos);
        Gizmos.DrawLine(transform.position, rightPos);
    }
}