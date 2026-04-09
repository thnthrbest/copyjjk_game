    using UnityEngine;

// ─── ใส่บน Bullet หรือ Attack Object ───
public class DamageData : MonoBehaviour
{
    public enum DamageType
    {
        Normal,     // ปกติ
        Heavy,      // หนัก
        Poison,     // พิษ
        Fire,       // ไฟ
    }

    [Header("Damage Settings")]
    public DamageType damageType = DamageType.Normal;
    public float      damage     = 10f;

    // ─── ดาเมจแต่ละประเภท ───
    public static float GetDamage(DamageType type)
    {
        switch (type)
        {
            case DamageType.Normal: return 10f;
            case DamageType.Heavy:  return 30f;
            case DamageType.Poison: return 5f;
            case DamageType.Fire:   return 20f;
            default:                return 10f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp == null) return;

            float dmg = damage > 0 ? damage : GetDamage(damageType);
            hp.TakeDamage(dmg, damageType.ToString());
            
            Destroy(gameObject);
        }
    }
}