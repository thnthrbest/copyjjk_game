using UnityEngine;

/// <summary>
/// กระสุนติดไฟจากศัตรู
/// เมื่อกระสุนถูกผู้เล่น:
///   1. ดีลดาเมจปกติ (bulletDamage)
///   2. ปลุกสถานะไฟเผา → 3 ดาเมจ/วิ เป็นเวลา 10 วิ (ปรับได้ใน Inspector)
/// </summary>
public class FireBullet : MonoBehaviour
{
    // ─── Bullet Settings ─────────────────────────────────
    [Header("Bullet Settings")]
    [Tooltip("ดาเมจปกติของกระสุน (หักเลือดทันทีที่โดน)")]
    public float bulletDamage = 10f;

    [Tooltip("ความเร็วกระสุน (ถ้าใช้ script นี้ขับเคลื่อนเอง, ไม่ใช้ถ้า Rigidbody velocity ถูกกำหนดจาก Spawner)")]
    public float speed = 20f;

    [Tooltip("อายุกระสุน (วินาที) ก่อนถูกทำลายอัตโนมัติ")]
    public float lifeTime = 6f;

    // ─── Burn Settings ────────────────────────────────────
    [Header("Burn (Fire) Settings")]
    [Tooltip("ดาเมจไฟต่อวินาที")]
    public float burnDamagePerSecond = 3f;
    [Tooltip("ระยะเวลาไฟเผา (วินาที)")]
    public float burnDuration = 10f;

    // ─── VFX ──────────────────────────────────────────────
    [Header("Visual Effects")]
    [Tooltip("Particle Effect เมื่อกระสุนกระทบ (ไม่บังคับ)")]
    public GameObject impactVFX;
    [Tooltip("แยก VFX ออกจากกระสุน หลังกระทบ (ไม่บังคับ)")]
    public float vfxLifetime = 2f;

    // ─── Tag Settings ─────────────────────────────────────
    [Header("Collision Tags")]
    [Tooltip("Tag ของผู้เล่น")]
    public string playerTag = "Player";

    // ─── Private ──────────────────────────────────────────
    private bool hasHit = false;
    private Rigidbody rb;

    // ─────────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb != null)
        {
            rb.useGravity = false;
            // ถ้าไม่มีใครกำหนด velocity มาก่อน → ยิงไปข้างหน้า
            if (rb.velocity.sqrMagnitude < 0.01f)
            {
                rb.velocity = transform.forward * speed;
            }
        }

        Destroy(gameObject, lifeTime);
    }

    // ─────────────────────────────────────────────────────
    //  ชนกับ Player (Tag = Player)
    // ─────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag(playerTag))
        {
            hasHit = true;
            HandleHitPlayer(other);
            SpawnImpactVFX();
            Destroy(gameObject);
        }
        // // ชนกับสิ่งแวดล้อม (ไม่ใช่ Player)
        // else if (!other.isTrigger && !other.CompareTag("Enemy") && !other.CompareTag("Bullet"))
        // {
        //     hasHit = true;
        //     SpawnImpactVFX();
        //     Destroy(gameObject);
        // }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        if (collision.collider.CompareTag(playerTag))
        {
            hasHit = true;
            HandleHitPlayer(collision.collider);
            SpawnImpactVFX();
            Destroy(gameObject);
        }
        else if (!collision.collider.CompareTag("Enemy") && !collision.collider.CompareTag("Bullet"))
        {
            hasHit = true;
            SpawnImpactVFX();
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────────────
    //  Logic เมื่อโดน Player
    // ─────────────────────────────────────────────────────
    void HandleHitPlayer(Collider playerCollider)
    {
        // หา Root GameObject ของ Player (รองรับ child collider)
        GameObject playerRoot = playerCollider.transform.root.gameObject;

        // ── 1. หักเลือดปกติ ──
        PlayerHealth health = playerRoot.GetComponentInChildren<PlayerHealth>();
        if (health != null && !health.IsDead())
        {
            health.TakeDamage(bulletDamage, "FireBullet");
            Debug.Log($"[FireBullet] โดน Player ดาเมจ {bulletDamage}");
        }

        // ── 2. ติดสถานะไฟเผา ──
        PlayerStatusEffect statusEffect = playerRoot.GetComponentInChildren<PlayerStatusEffect>();
        if (statusEffect != null)
        {
            statusEffect.ApplyBurn(burnDamagePerSecond, burnDuration);
            Debug.Log($"[FireBullet] ปลุกไฟ: {burnDamagePerSecond}/วิ เป็นเวลา {burnDuration}วิ");
        }
        else
        {
            Debug.LogWarning("[FireBullet] ไม่พบ PlayerStatusEffect บน Player! (โปรด Add Component)");
        }
    }

    // ─────────────────────────────────────────────────────
    //  Spawn Impact VFX
    // ─────────────────────────────────────────────────────
    void SpawnImpactVFX()
    {
        if (impactVFX != null)
        {
            GameObject fx = Instantiate(impactVFX, transform.position, Quaternion.identity);
            Destroy(fx, vfxLifetime);
        }
    }

    // ─────────────────────────────────────────────────────
    //  Public: ตั้งค่าทิศทางจาก Spawner (เช่น EnemyController)
    // ─────────────────────────────────────────────────────
    public void Launch(Vector3 direction, float overrideSpeed = -1f)
    {
        float spd = overrideSpeed > 0f ? overrideSpeed : speed;
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = direction.normalized * spd;
        }
        transform.forward = direction.normalized;
    }
}
