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
        // 1. กรองด้วย Tag "Player" ก่อน เพื่อรองรับ CharacterController
        //    (CharacterController จะส่ง Collider ของตัวเองเข้ามาใน OnTriggerEnter)
        if (!other.CompareTag("Player")) return;

        // 2. หา SkillManager จาก GameObject ของ CharacterController ขึ้นไปหา parent
        SkillManager skillManager = other.GetComponentInParent<SkillManager>();

        // fallback: หาบน GameObject ตรงๆ (กรณี SkillManager อยู่บน root เดียวกับ CharacterController)
        if (skillManager == null)
        {
            skillManager = other.GetComponent<SkillManager>();
        }

        // 3. ถ้าเจอผู้เล่นและมี SkillManager
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
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(collectSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
                }
            }

            // 5. ทำลายไอเทมชิ้นนี้ออกจากฉาก
           // Destroy(gameObject);
        }
    }
}
