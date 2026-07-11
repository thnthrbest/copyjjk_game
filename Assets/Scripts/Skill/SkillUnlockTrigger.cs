using UnityEngine;

public class SkillUnlockTrigger : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("ชื่อสกิลที่ต้องการปลดล็อค (เช่น 'rabbit', 'dog', 'cow')")]
    public string skillName = "rabbit";

    [Header("Effects (Optional)")]
    [Tooltip("เอฟเฟกต์ที่จะเกิดขึ้นเมื่อถูกเก็บ (เช่น แสงระเบิด หรือสะเก็ดไฟ)")]
    public GameObject collectEffectPrefab;

    [Tooltip("เสียงที่จะเล่นเมื่อเก็บของได้")]
    public AudioClip collectSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    private void OnTriggerEnter(Collider other)
    {
        // 1. ตรวจสอบว่าวัตถุที่มาชนคือผู้เล่น (เช็คจาก Tag หรือมีคอมโพเนนต์ SkillManager)
        SkillManager skillManager = other.GetComponent<SkillManager>();
        
        // เผื่อโครงสร้างตัวละครมีโมเดลแยกอยู่ด้านใน ให้หาใน parent หรือ child
        if (skillManager == null)
        {
            skillManager = other.GetComponentInParent<SkillManager>();
        }

        // 2. ถ้าเจอผู้เล่นและมี SkillManager
        if (skillManager != null)
        {
            // ดำเนินการปลดล็อคสกิล
            skillManager.UnlockSkill(skillName);

            // 3. เล่นเอฟเฟกต์ (หากมีการใส่ไว้ใน Inspector)
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }

            // 4. เล่นเสียง (หากมีการใส่ไว้ใน Inspector)
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
            }

            // 5. ทำลายไอเทมชิ้นนี้ออกจากฉาก
            Destroy(gameObject);
        }
    }
}
