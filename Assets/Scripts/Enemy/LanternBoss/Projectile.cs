using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum DamageType
    {
        Normal,
        Heavy,
        Poison,
        Fire
    }

    [Header("Damage")]
    public DamageType damageType = DamageType.Normal;
    public float damage = 10f;

    [Header("Movement")]
    public float speed = 25f;
    public float lifeTime = 5f;

    [Header("Behavior")]
    public bool destroyOnPlayerHit = true;
    public bool destroyOnBulletDestroyer = true;
    public bool destroyOnMagic = true;

    private Vector3 direction;
    private bool hasDirection = false;

    private Rigidbody rb;

    // =========================
    // INIT
    // =========================
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; // 🔥 กันลืม ตั้งให้เลย
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // =========================
    // 🎯 รับ target
    // =========================
    public void SetTarget(Vector3 target)
    {
        direction = (target - transform.position).normalized;
        hasDirection = true;
    }

    void Update()
    {
        if (!hasDirection) return;

        transform.position += direction * speed * Time.deltaTime;
        transform.forward = direction;
    }

    // =========================
    // 💥 Collision
    // =========================
    void OnTriggerEnter(Collider other)
    {
        // 🔴 Player
        if (other.CompareTag("Player"))
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();

            if (hp != null)
            {
                float dmg = damage > 0 ? damage : GetDamage(damageType);
                hp.TakeDamage(dmg, damageType.ToString());
            }

            if (destroyOnPlayerHit)
                Destroy(gameObject);
        }

        // 🔵 BulletDestroyer
        else if (other.CompareTag("BulletDestroyer"))
        {
            if (destroyOnBulletDestroyer)
                Destroy(gameObject);
        }

        // 🟣 Magic
        else if (other.CompareTag("Magic"))
        {
            if (destroyOnMagic)
                Destroy(gameObject);
        }
    }

    // =========================
    // 🔢 Default Damage
    // =========================
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
}