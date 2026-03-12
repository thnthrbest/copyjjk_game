using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // ─── ถ้าโดน Player ───
        if (other.CompareTag("player"))
        {
            Debug.Log("[Bullet] โดน Player!");
            // other.GetComponent<PlayerHealth>()?.TakeDamage(10); ← เปิดทีหลัง
            Destroy(gameObject);
        }

        // ─── ถ้าโดนกำแพงหรืออื่นๆ ───
        if (!other.CompareTag("enemy"))
        {
            Destroy(gameObject);
        }
    }
}