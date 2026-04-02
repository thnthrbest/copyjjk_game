using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    Transform target;

    public float speed = 25f;

    public void SetTarget(Transform enemy)
    {
        target = enemy;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;

        transform.LookAt(target);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}