using UnityEngine;

public class dogobj : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 15f;

    [Header("Sound Settings")]
    public AudioClip biteSound;

    [Header("Target Type")]
    public bool targetGroundEnemy = true;   // โจมตีศัตรูพื้น
    public bool targetAirEnemy = false;     // โจมตีศัตรูฟ้า

    [Header("Enemy Settings")]
    public string enemyTag = "Enemy";

    [Header("Y Check")]
    public float airEnemyHeight = 3f;
    // ถ้า enemy สูงกว่าค่านี้จากพื้น → ถือว่าเป็น enemy บนฟ้า

    [Header("Movement")]
    public float moveSpeed = 12f;
    public float rotationSpeed = 10f;
    public float stopDistance = 1.2f;


    GameObject player; // เอฟเฟกต์ตอนชนศัตรู (ถ้ามี)

    private Transform target;
    private bool move = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        FindNearestTarget();
    }

    void Update()
    {
        // target หาย → หาใหม่
        if (target == null)
        {
            FindNearestTarget();
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        // ถ้าอยู่นอกระยะ → ไม่ทำอะไร
        if (dist > detectionRange)
            return;

        LookAtTarget();

       

        if (move)
        {
            MoveToTargetDirect();
        }
    }

    // =====================================
    // หา enemy ที่ใกล้ที่สุด
    // แยกพื้น / ฟ้า ด้วยการเช็คแกน Y
    // =====================================
    void FindNearestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        float closestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (GameObject enemy in enemies)
        {
            bool isAirEnemy = enemy.transform.position.y >= airEnemyHeight;
            bool isGroundEnemy = enemy.transform.position.y < airEnemyHeight;

            // กรองตามประเภทที่ต้องการ
            if (isGroundEnemy && !targetGroundEnemy)
                continue;

            if (isAirEnemy && !targetAirEnemy)
                continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearestTarget = enemy.transform;
            }
        }

        target = nearestTarget;
    }

    // =====================================
    // ถ้ายังอยากใช้ Animation Event
    // =====================================
    public void setmove()
    {
        move = true;
        //player.GetComponent<PlayerHealth>().godMode = false; // ปิดโหมดเทพ
    }

    // =====================================
    // หันหน้าไปหาเป้าหมาย
    // =====================================
    void LookAtTarget()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // =====================================
    // พุ่งตรงแบบไม่ใช้ฟิสิกส์
    // ยิงขึ้นฟ้า / ลงพื้น ได้
    // =====================================
    void MoveToTargetDirect()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPos = target.position;

        // ถ้ามี Collider → ใช้จุดกลางตัว
        Collider col = target.GetComponent<Collider>();
        if (col != null)
        {
            targetPos = col.bounds.center;
        }

        Vector3 direction = (targetPos - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // ไม่ใช้ Rigidbody / Gravity
        transform.position += direction * moveSpeed * Time.deltaTime;

        float dist = Vector3.Distance(transform.position, targetPos);

        // if (dist <= stopDistance)
        // {
        //     Destroy(gameObject);
        // }
    }

    // =====================================
    // ชนแล้วหาย
    // =====================================
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (biteSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFXAtPoint(biteSound, transform.position);
            }

            Destroy(other.gameObject); // ทำลายศัตรูที่ชน (ถ้าต้องการ)
            Destroy(gameObject);
        }
    }

    // =====================================
    // DEBUG
    // =====================================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}