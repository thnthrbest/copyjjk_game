using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Detection")]
    public float sightRange = 10f;
    public LayerMask whatIsPlayer;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform  firePoint;          // จุดยิงกระสุน (สร้าง Empty GameObject ไว้หน้า Enemy)
    public float      projectileSpeed    = 20f;
    public float      timeBetweenAttacks = 2f;

    [Header("Debug")]
    public bool playerInSightRange = false;

    private Transform player;
    private bool      alreadyAttacked = false;

    void Awake()
    {
        // หา Player อัตโนมัติ
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning("[Enemy] ไม่พบ Player! ตั้ง Tag = 'Player' ก่อนนะครับ");
    }

    void Update()
    {
        if (player == null) return;

        // ─── เช็คระยะ ───
        float dist = Vector3.Distance(transform.position, player.position);
        playerInSightRange = dist <= sightRange;

        if (playerInSightRange)
        {
            LookAtPlayer();
            TryAttack();
        }
    }

    void LookAtPlayer()
    {
        // หมุนหน้าไปหาผู้เล่นแค่แกน Y (ไม่ก้มหัว)
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation   = Quaternion.Slerp(
                transform.rotation, targetRot, 10f * Time.deltaTime
            );
        }
    }

    void TryAttack()
    {
        if (alreadyAttacked) return;

        // ─── ยิงกระสุน ───
        Transform spawnPoint = firePoint != null ? firePoint : transform;

        GameObject bullet = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;   // กระสุนเป็นเส้นตรง ไม่ตกลงพื้น
            rb.velocity   = spawnPoint.forward * projectileSpeed;
        }

        // ─── ทำลายกระสุนหลัง 5 วิถ้าไม่โดนอะไร ───
        Destroy(bullet, 5f);

        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);

        Debug.Log("[Enemy] ยิงกระสุน!");
    }

    void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // ─── แสดง Gizmos ใน Editor ───
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // แสดงทิศยิง
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 3f);
        }
    }
}