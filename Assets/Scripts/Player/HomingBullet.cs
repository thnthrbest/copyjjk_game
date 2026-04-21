using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [Header("Target")]
    Transform target;

    [Header("Movement")]
    public float speed = 25f;
    public float rotateSpeed = 10f;

    [Header("Lifetime")]
    public float lifeTime = 5f;

    Rigidbody rb;
    ParticleSystem ps;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ps = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
        // ทำให้ particle ทำงานแน่นอน
        if (ps != null)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.Play();
        }

        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform enemy)
    {
        target = enemy;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // หา direction
        Vector3 dir = (target.position - transform.position).normalized;

        // หมุนแบบ smooth
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRot,
            rotateSpeed * Time.fixedDeltaTime
        );

        // เคลื่อนที่ด้วย Rigidbody
        rb.velocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // TODO: ใส่ damage ตรงนี้ได้

            Destroy(gameObject);
        }
    }
}