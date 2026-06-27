using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 8f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 8f;

    private Transform player;
    private bool isChasing = false;
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
            Debug.LogWarning("[EnemyChase] ไม่พบ Player (Tag = Player)");
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // ตรวจจับระยะ
        if (dist <= detectionRange)
            isChasing = true;
        else
            isChasing = false;

        if (isChasing)
        {
            LookAtPlayer();
            animator.SetBool("attack", true);
            MoveToPlayer();
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
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // =========================
    // เคลื่อนที่เข้าหา Player
    // =========================
    void MoveToPlayer()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    // =========================
    // ชนแล้วหาย
    // =========================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
             Debug.Log("[EnemyChase] โดนผู้เล่นแล้ว");
            Destroy(gameObject);
        }
        if (other.CompareTag("dog"))
        {
            Debug.Log("[EnemyChase] โดนหมาแล้ว");
            Destroy(gameObject);
        }
    }

    // =========================
    // DEBUG (Gizmo)
    // =========================
    void OnDrawGizmosSelected()
    {
        // วงตรวจจับ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // เส้น forward
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}