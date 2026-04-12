using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Detection")]
    public float sightRange = 10f;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 40f;
    public float timeBetweenAttacks = 2f;

    [Header("Aim")]
    public float aimHeightOffset = 26f; // ใช้กรณีไม่มี collider

    private Transform player;
    private bool alreadyAttacked = false;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Awake()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning("[Enemy] ไม่พบ Player (Tag = Player)");
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= sightRange)
        {
            LookAtPlayer(); // หมุนเฉพาะแกน Y
            TryAttack();

            if (animator != null)
                animator.SetTrigger("attack");
        }
    }

    // =========================
    // หมุนเฉพาะแกน Y
    // =========================
    void LookAtPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
        }
    }

    // =========================
    // ยิงกระสุน (เล็งแม่น)
    // =========================
    void TryAttack()
    {
        if (alreadyAttacked) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;

        GameObject bullet = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;

            // 🔥 เล็งไปที่ "กลาง collider จริง"
            Vector3 targetPos;

            Collider col = player.GetComponent<Collider>();

            if (col != null)
            {
                targetPos = col.bounds.center; // 🔥 จุดสำคัญ
            }
            else
            {
                targetPos = player.position + Vector3.up * aimHeightOffset;
            }

            Vector3 dir = (targetPos - spawnPoint.position).normalized;

            rb.velocity = dir * projectileSpeed;

            // หมุนกระสุนให้ตรงทิศ
            bullet.transform.forward = dir;

            // 🔍 debug เส้นยิง
            Debug.DrawLine(spawnPoint.position, targetPos, Color.red, 1f);
        }

        Destroy(bullet, 5f);

        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // =========================
    // DEBUG
    // =========================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 3f);
        }
    }
}